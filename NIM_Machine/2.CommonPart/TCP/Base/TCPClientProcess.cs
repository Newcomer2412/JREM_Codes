using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using NLog;

namespace MachineControlBase
{
    #region delegate

    public delegate void EventServerConnected();            // 서버에 접속 되었을 때

    public delegate void EventServerDisconnected();         // 서버와 접속이 끊겼을 때

    public delegate void EventOnReceived(byte[] byteMsg);   // message를 수신했을 때

    #endregion delegate

    public class CTCPClientProcess
    {
        public Logger cLogClass = null;
        public EventServerConnected delServerConnected = null;
        public EventServerDisconnected delServerDisconnected = null;
        public EventOnReceived delOnReceived = null;
        private AutoResetEvent areConnectToServerThreadEnd = new AutoResetEvent(false);
        private TcpClient tcpClient = null;
        private NetworkStream nsStream = null;
        private string strServerIP = "192.168.0.1";
        private uint uiServerPort = 4000;
        public bool bConnected = false;
        private string strServerEP;
        private bool bAutoReconnect = false;
        private bool bConnecting = false;

        /// <summary>
        /// 연결 끊음
        /// </summary>
        public void Disconnect()
        {
            // connecting 시도 중일 경우 중지
            if (bConnecting == true)
            {
                bAutoReconnect = false;                    // 쓰레드 소멸
                areConnectToServerThreadEnd.WaitOne(1000);  // wait thread(while loop) exit
            }

            if (bConnected == true)
            {
                bConnected = false;
                if (nsStream != null) nsStream.Close();
                if (tcpClient != null) tcpClient.Close();
                nsStream = null;
                tcpClient = null;
                delServerDisconnected();
            }
        }

        /// <summary>
        /// 오토 Reconnect 모드 제어
        /// </summary>
        /// <param name="bOn"></param>
        public void SetAutoReconnectMode(bool bOn)
        {
            bAutoReconnect = bOn;
        }

        /// <summary>
        /// 로그 함수
        /// </summary>
        /// <param name="strLog"></param>
        private void AddLog(string strLog)
        {
            if (cLogClass != null)
            {
                cLogClass.Info(strLog);
            }
        }

        /// <summary>
        /// 통신 연결
        /// </summary>
        /// <param name="strServerIP"></param>
        /// <param name="uiServerPort"></param>
        /// <param name="bAutoReconnect"></param>
        /// <returns></returns>
        public bool Connect(string strServerIP, uint uiServerPort, bool bAutoReconnect = false)
        {
            try
            {
                if (bConnected == true) return false;
                bConnecting = true;
                areConnectToServerThreadEnd.Reset();
                this.strServerIP = strServerIP;
                this.uiServerPort = uiServerPort;
                this.bAutoReconnect = bAutoReconnect;
                AddLog(string.Format("서버:{0}:{1} 연결중", strServerIP, uiServerPort));

                if (bAutoReconnect == true)
                {
                    int iTimeOut = 30; // 30 sec
                    int iWaitSec = 1;  // 1초
                    int iMaxTry = iTimeOut / iWaitSec;
                    int iTry = 1;
                    while (true)
                    {
                        tcpClient = new TcpClient();
                        tcpClient.NoDelay = true;

                        AddLog(string.Format("서버:{0}:{1} 접속시도 {2}/{3}", strServerIP, uiServerPort, iTry, iMaxTry));

                        var result = tcpClient.BeginConnect(strServerIP, (int)uiServerPort, null, null);
                        bool bSuccess = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(iWaitSec));

                        if (bSuccess == true)
                        {
                            if (tcpClient.Connected) break;  // 접속 성공 또는 exception , exit ile loop
                            else tcpClient.Close();
                        }
                        else
                        {
                            AddLog(string.Format("서버:{0}:{1} 접속실패", strServerIP, uiServerPort));
                            tcpClient.Close();
                        }

                        // auto reconnect 옵션이 변경될 경우 체크
                        if (bAutoReconnect == false)
                        {
                            AddLog(string.Format("서버:{0}:{1} 접속시도 사용자 정지", strServerIP, uiServerPort));
                            return false;
                        }

                        // try timeout check
                        iTry++;
                        if (iTry > iTimeOut)
                        {
                            AddLog(string.Format("서버:{0}:{1} 접속시도 회수 초과", strServerIP, uiServerPort));
                            return false;
                        }

                        Application.DoEvents();
                    }
                }
                else
                {
                    tcpClient = new TcpClient();
                    tcpClient.NoDelay = true;
                    var result = tcpClient.BeginConnect(strServerIP, (int)uiServerPort, null, null);
                    int iWaitSec = 1;  // 1초
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(iWaitSec));
                    if (!success)
                    {
                        throw new Exception(string.Format("서버:{0}:{1} 접속실패", strServerIP, uiServerPort));
                    }
                }

                strServerEP = ((IPEndPoint)(tcpClient.Client.RemoteEndPoint)).ToString();
                AddLog(string.Format("서버:{0} 접속완료", strServerEP));
                delServerConnected();

                // receive thread start
                Thread thReceive = new Thread(ReceiveLoop);
                thReceive.Start(tcpClient);
                areConnectToServerThreadEnd.Set();
                bConnecting = false;
                return true;
            }
            catch (Exception ex)
            {
                AddLog(string.Format("서버:{0}:{1} 접속실패 = {2}", strServerIP, uiServerPort, ex.Message));
                tcpClient.Close();
                bConnecting = false;
                areConnectToServerThreadEnd.Set();
                return false;
            }
        }

        /// <summary>
        /// 서버 통신 데이터 대기 루프
        /// </summary>
        /// <param name="objClient"></param>
        private void ReceiveLoop(object objClient)
        {
            TcpClient rTcpClient = (TcpClient)objClient;
            NetworkStream nsStream = rTcpClient.GetStream();
            string strServerEP = ((IPEndPoint)(rTcpClient.Client.RemoteEndPoint)).ToString();

            // update server stream for send
            this.nsStream = nsStream;
            this.strServerEP = strServerEP;

            byte[] byteTemp = new byte[256];
            try
            {
                bConnected = true;
                // 전달받은 데이터 처리
                while (true)
                {
                    int iLength = nsStream.Read(byteTemp, 0, byteTemp.Length);
                    if (iLength > 0)
                    {
                        byte[] byteRead = new byte[iLength];
                        Array.Copy(byteTemp, 0, byteRead, 0, iLength);
                        delOnReceived?.Invoke(byteRead);
                        string readString = Encoding.Default.GetString(byteRead, 0, iLength);
                        AddLog(string.Format("수신 : {0} : {1}", strServerEP, readString));
                    }
                    if (iLength == 0) break;
                }
            }
            catch (Exception ex)
            {
                AddLog(string.Format("서버:{0}:{1} Receive Loop = {2}", strServerIP, uiServerPort, ex.Message));
            }
            finally
            {
                // 서버 측이 끊었을 경우
                if (bConnected == true) AddLog(string.Format("{0} 서버 종료.", strServerEP));
                else AddLog(string.Format("{0} 접속 종료.", strServerEP)); // 내가(클라이언트) 끊었을 경우)
                // 접속 종료
                Disconnect();
            }
        }

        /// <summary>
        /// 서버로 데이터 전송
        /// </summary>
        /// <param name="strMsg"></param>
        public void SendMsg(string strMsg)
        {
            try
            {
                if (nsStream != null)
                {
                    byte[] byteWrite = Encoding.Default.GetBytes(strMsg);
                    nsStream.Write(byteWrite, 0, byteWrite.Length);
                    AddLog(string.Format("송신 : {0} : {1}", strServerEP, strMsg));
                }
            }
            catch (Exception ex)
            {
                AddLog(string.Format("서버:{0}:{1} SendMsg = {2}", strServerIP, uiServerPort, ex.Message));
            }
        }

        /// <summary>
        /// 서버로 데이터 전송
        /// </summary>
        /// <param name="byteMsg"></param>
        public void SendMsg(byte[] byteMsg)
        {
            try
            {
                if (nsStream != null)
                {
                    string strSendMsg = Encoding.Default.GetString(byteMsg, 0, byteMsg.Length);
                    nsStream.Write(byteMsg, 0, byteMsg.Length);
                    AddLog(string.Format("송신 : {0} : {1}", strServerEP, strSendMsg));
                }
            }
            catch (Exception ex)
            {
                AddLog(string.Format("서버:{0}:{1} SendMsg = {2}", strServerIP, uiServerPort, ex.Message));
            }
        }
    }
}