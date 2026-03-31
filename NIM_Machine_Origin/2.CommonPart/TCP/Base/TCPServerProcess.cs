using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NLog;

namespace MachineControlBase
{
    #region delegate

    public delegate void EventUpdateClientList(List<string> listClientEP);  // 접속한 클라이언트 리스트가 업데이트 되었을 때

    public delegate void EventServerStarted();                              // 서버 시작 되었을 때

    public delegate void EventServerStopped();                              // 서버 정지 되었을 때

    #endregion delegate

    internal class CTCPServerProcess
    {
        public Logger cLogClass = null;
        public EventUpdateClientList delUpdateClientList = null;
        public EventServerStarted delServerStarted = null;
        public EventServerStopped delServerStopped = null;
        public EventOnReceived delOnReceived = null;
        private TcpListener cTcpListener = null;
        private string strServerIP = "192.168.0.1";
        private uint uiServerPort = 4000;
        private bool bServerStarted = false;
        private List<NetworkStream> listNetworkStream = new List<NetworkStream>();
        private List<TcpClient> listTcpClient = new List<TcpClient>();
        public List<string> liststrClientEP = new List<string>();

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
        /// 서버 정지
        /// </summary>
        public void StopServer()
        {
            if (bServerStarted)
            {
                bServerStarted = false;
                // 연결된 모든 클라이언트를 제거
                foreach (var stream in listNetworkStream)
                {
                    stream.Close();
                }
                foreach (var client in listTcpClient)
                {
                    client.Close();
                }
                listNetworkStream.Clear();
                listTcpClient.Clear();
                // 클라이언트 리스트 UI에서 제거
                liststrClientEP.Clear();
                delUpdateClientList?.Invoke(liststrClientEP);
                // 서버 정지
                cTcpListener.Stop();
            }
        }

        /// <summary>
        /// 서버 시작
        /// </summary>
        /// <param name="strServerIP"></param>
        /// <param name="uiServerPort"></param>
        /// <returns></returns>
        public bool ServerStart(string strServerIP, uint uiServerPort)
        {
            try
            {
                if (bServerStarted) return true;
                this.strServerIP = strServerIP;
                this.uiServerPort = uiServerPort;

                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(strServerIP), (int)uiServerPort);
                cTcpListener = new TcpListener(iPEndPoint);
                cTcpListener.Start();
                AddLog(string.Format("서버:{0}:{1} 서비스 시작", strServerIP, uiServerPort));
                delServerStarted();

                // run wait new client connection thread
                Thread thWaitClient = new Thread(new ThreadStart(this.WaitClientLoop));
                thWaitClient.Start();

                return true;
            }
            catch (Exception ex)
            {
                if (cLogClass != null)
                {
                    cLogClass.Fatal(string.Format("Server Start Fail !\n{0}", ex.ToString()));
                }
                return false;
            }
        }

        /// <summary>
        /// 서버 접속 대기 루프
        /// </summary>
        private void WaitClientLoop()
        {
            bServerStarted = true;

            try
            {
                while (true)
                {
                    // wait new client connection
                    AddLog("새 클라이언트 접속 대기중... ");
                    TcpClient tcpClient = cTcpListener.AcceptTcpClient();
                    tcpClient.NoDelay = true;  //꼭 True로 할 것!
                    AddLog(string.Format("새 클라이언트 {0} 접속", ((IPEndPoint)tcpClient.Client.RemoteEndPoint).ToString()));
                    // receive thread start
                    Thread thReceive = new Thread(new ParameterizedThreadStart(this.ReceiveLoop));
                    thReceive.Start(tcpClient);
                }
            }
            catch (Exception ex)
            {
                if (cLogClass != null)
                {
                    cLogClass.Fatal(string.Format("Wait Client Loop Fail !\n{0}", ex.ToString()));
                }
            }
            finally
            {
                // 유저가 서비스를 정지한 경우
                AddLog(string.Format("서버:{0}:{1} 서비스 정지", strServerIP, uiServerPort));
                delServerStopped();
            }
        }

        /// <summary>
        /// 클라이언트에서 보내온 데이터 응답 루프
        /// </summary>
        /// <param name="objClient"></param>
        private void ReceiveLoop(object objClient)
        {
            TcpClient tcpClient = (TcpClient)objClient;
            NetworkStream networkStream = tcpClient.GetStream();
            string strClientEP = ((IPEndPoint)(tcpClient.Client.RemoteEndPoint)).ToString();
            UpdateClientList(tcpClient, networkStream, true);

            byte[] byteTemp = new byte[256];
            try
            {
                // 전달받은 데이터 처리
                while (true)
                {
                    int iLength = networkStream.Read(byteTemp, 0, byteTemp.Length);
                    if (iLength > 0)
                    {
                        byte[] byteRead = new byte[iLength];
                        Array.Copy(byteTemp, 0, byteRead, 0, iLength);
                        delOnReceived?.Invoke(byteRead);
                        string strReadString = Encoding.Default.GetString(byteRead, 0, iLength);
                        AddLog(string.Format("수신 : {0} : {1}", strClientEP, strReadString));
                    }
                    if (iLength == 0) break;
                }
            }
            catch (Exception ex)
            {
                if (cLogClass != null)
                {
                    cLogClass.Fatal(string.Format("Server Start Fail !\n{0}", ex.ToString()));
                }
            }
            finally
            {
                // 클라이언트 측이 끊었을 경우
                if (bServerStarted)
                {
                    AddLog(string.Format("{0} 접속 종료.", strClientEP));
                    UpdateClientList(tcpClient, networkStream, false);
                    networkStream.Close();
                    tcpClient.Close();
                }
            }
        }

        /// <summary>
        /// 클라이언트에 메세지 전송
        /// </summary>
        /// <param name="strIP"></param>
        /// <param name="strMsg"></param>
        public void SendMsg(string strIP, string strMsg)
        {
            for (int i = 0; i < liststrClientEP.Count; i++)
            {
                if (liststrClientEP[i] == strIP)
                {
                    byte[] byteWrite = Encoding.Default.GetBytes(strMsg);
                    listNetworkStream[i].Write(byteWrite, 0, byteWrite.Length);
                    AddLog(string.Format("송신 : {0} : {1}", liststrClientEP[i], strMsg));
                }
            }
        }

        /// <summary>
        /// 클라이언트에 메세지 전송
        /// </summary>
        /// <param name="strIP"></param>
        /// <param name="byteData"></param>
        public void SendMsg(string strIP, byte[] byteData)
        {
            for (int i = 0; i < liststrClientEP.Count; i++)
            {
                if (liststrClientEP[i] == strIP)
                {
                    string strLog = Encoding.Default.GetString(byteData, 0, byteData.Length);
                    listNetworkStream[i].Write(byteData, 0, byteData.Length);
                    AddLog(string.Format("송신 : {0} : {1}", liststrClientEP[i], strLog));
                }
            }
        }

        /// <summary>
        /// 전체 연결된 클라이언트에 메세지를 전송
        /// </summary>
        /// <param name="strMsg"></param>
        public void SendMsgAll(string strMsg)
        {
            for (int i = 0; i < liststrClientEP.Count; i++)
            {
                byte[] byteWrite = Encoding.Default.GetBytes(strMsg);
                listNetworkStream[i].Write(byteWrite, 0, byteWrite.Length);
                AddLog(string.Format("송신 : {0} : {1}", liststrClientEP[i], strMsg));
            }
        }

        /// <summary>
        /// 전체 연결된 클라이언트에 메세지를 전송
        /// </summary>
        /// <param name="byteData"></param>
        public void SendMsgAll(byte[] byteData)
        {
            string strLog = Encoding.Default.GetString(byteData, 0, byteData.Length);
            for (int i = 0; i < liststrClientEP.Count; i++)
            {
                listNetworkStream[i].Write(byteData, 0, byteData.Length);
                AddLog(string.Format("송신 : {0} : {1}", liststrClientEP[i], strLog));
            }
        }

        /// <summary>
        /// 클라이언트 연결 정보를 갱신
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="networkStream"></param>
        /// <param name="bAdd"></param>
        private void UpdateClientList(TcpClient tcpClient, NetworkStream networkStream, bool bAdd)
        {
            try
            {
                string strEP = ((IPEndPoint)(tcpClient.Client.RemoteEndPoint)).ToString();
                if (bAdd)
                {
                    listNetworkStream.Add(networkStream);
                    listTcpClient.Add(tcpClient);
                    liststrClientEP.Add(strEP);
                }
                else
                {
                    listNetworkStream.Remove(networkStream);
                    listTcpClient.Remove(tcpClient);
                    liststrClientEP.Remove(strEP);
                }

                // for update form client list
                delUpdateClientList?.Invoke(liststrClientEP);
            }
            catch (Exception ex)
            {
                if (cLogClass != null)
                {
                    cLogClass.Fatal(string.Format("Update Client List Fail !\n{0}", ex.ToString()));
                }
                liststrClientEP.Clear();
                delUpdateClientList?.Invoke(liststrClientEP);
            }
        }
    }
}