using NLog;
using System;
using System.Text;
using System.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// 비동기 형식 TCP/IP Client
    /// </summary>
    public class CTCPAsyncClient
    {
        /// <summary>
        /// TCP Client Class
        /// </summary>
        public CTCPClientProcess cTCPClientProcess = null;

        #region LogClass Class 정의

        /// <summary>
        /// TCP/IP Log Class
        /// </summary>
        private Logger cLogClass = null;

        #endregion LogClass Class 정의

        #region TCP/IP Call Back Delegate 정의

        /// <summary>
        /// Server로부터 Data 수신시 Call Back
        /// </summary>
        /// <param name="strRecvMsg"></param>
        public delegate void EventReceiveStringDataServer(string strRecvMsg);

        public delegate void EventReceiveByteDataServer(byte[] byteRecvMsg);

        /// <summary>
        /// Server로부터 Data 수신시 Call Back
        /// </summary>
        private EventReceiveStringDataServer delReceiveStringDataServer = null;

        private EventReceiveByteDataServer delReceiveByteDataServer = null;

        /// <summary>
        /// Server에 접속 후 상태 변경시 호출되는 Call back
        /// </summary>
        /// <param name="bStatus"></param>
        public delegate void ConnectionStatusChanged(bool bStatus);

        /// <summary>
        /// Osram Sever에 접속후 상태 변경시 호출되는 Call back
        /// </summary>
        private ConnectionStatusChanged connectionStatusChanged = null;

        /// <summary>
        /// 로그 함수
        /// </summary>
        /// <param name="msg"></param>
        private void AddLog(string msg)
        {
            try
            {
                if (cLogClass != null)
                {
                    cLogClass.Info(msg);
                }
            }
            catch { }
        }

        /// <summary>
        /// Log 연결
        /// </summary>
        /// <param name="cLogClass"></param>
        public void SetLog(Logger cLogClass)
        {
            this.cLogClass = cLogClass;
        }

        /// <summary>
        /// IP주소
        /// </summary>
        private string strServerIP = string.Empty;

        /// <summary>
        /// 포트 번호
        /// </summary>
        private uint uiPortNo = 0;

        /// <summary>
        /// 연결 끊김 시 자동 접속 여부
        /// </summary>
        public bool bAutoReconnect = false;

        /// <summary>
        /// 접속 여부
        /// </summary>
        public bool bConncect = false;

        /// <summary>
        /// 리턴 값이 string으로 받을지 byte[]로 받을지 선택 true : string
        /// </summary>
        public bool bReceiveStringByteChoice = false;

        #endregion TCP/IP Call Back Delegate 정의

        #region 1. 서버 연결 및 접속 종료

        /// <summary>
        /// 서버 연결
        /// </summary>
        /// <param name="strServerIP">Server IP Address</param>
        /// <param name="uiServerPortNo">Server Port No</param>
        /// <param name="connectionStatusChanged">Server의 연결상태 변경시 호출되는 Delegate</param>
        /// <param name="delReceiveStringDataServer">Server로 부터 Data 수신시 받아서 처리할 Delegate(string)</param>
        /// <param name="delReceiveByteDataServer">Server로 부터 Data 수신시 받아서 처리할 Delegate(byte)</param>
        /// <param name="bReceiveStringByteChoice">Server로 부터 Data 수신시 string으로 처리할지 Byte로 처리할지 Flag</param>
        public bool Connect(string strServerIP, uint uiServerPortNo,
                            ConnectionStatusChanged connectionStatusChanged,
                            EventReceiveStringDataServer delReceiveStringDataServer = null,
                            EventReceiveByteDataServer delReceiveByteDataServer = null,
                            bool bReceiveStringByteChoice = true)
        {
            this.delReceiveStringDataServer = delReceiveStringDataServer;
            this.delReceiveByteDataServer = delReceiveByteDataServer;
            this.connectionStatusChanged = connectionStatusChanged;
            this.bReceiveStringByteChoice = bReceiveStringByteChoice;

            try
            {
                if (cTCPClientProcess == null)
                {
                    cTCPClientProcess = new CTCPClientProcess();
                    cTCPClientProcess.cLogClass = cLogClass;
                    cTCPClientProcess.delServerConnected += new EventServerConnected(ServerConnection);
                    cTCPClientProcess.delServerDisconnected += new EventServerDisconnected(AutoReconnect);
                    cTCPClientProcess.delOnReceived += new EventOnReceived(CallBackDataReceived);
                }

                this.strServerIP = strServerIP;
                uiPortNo = uiServerPortNo;
                bAutoReconnect = true;
                bConnecting = true;
                Thread th = new Thread(ConnectServerForThreading);
                th.Start();
                AddLog(string.Format("Server Connected OK. Serevr IP : {0}, Port : {1}", strServerIP, uiPortNo));
                return true;
            }
            catch (Exception ex)
            {
                if (cLogClass != null)
                {
                    cLogClass.Fatal(string.Format("Server Connected Fail !\n{0}", ex.ToString()));
                }
                return false;
            }
        }

        /// <summary>
        /// Thread 서버측에 연결
        /// </summary>
        /// <param name="obj"></param>
        private void ConnectServerForThreading(object obj)
        {
            cTCPClientProcess.Connect(strServerIP, uiPortNo, bAutoReconnect);
            bConnecting = false;
        }

        /// <summary>
        /// 서버 Disconnect
        /// </summary>
        public void Disconnect()
        {
            bAutoReconnect = false;

            if (cTCPClientProcess != null)
            {
                cTCPClientProcess.Disconnect();
                if (cLogClass != null)
                {
                    cLogClass.Warn("Client Socket Disconnected !");
                }
            }
            if (connectionStatusChanged != null)
            {
                connectionStatusChanged(false);
                AddLog("Client Socket Connection Status Disconnect");
            }
        }

        /// <summary>
        /// Disconnect 이벤트가 발생할 경우 재접속 진행
        /// </summary>
        public void AutoReconnect()
        {
            bConncect = false;
            if (connectionStatusChanged != null)
            {
                connectionStatusChanged(false);
                AddLog("Client Socket Connection Status Disconnect");
            }

            if (bAutoReconnect)
            {
                bConnecting = true;
                Thread th = new Thread(ConnectServerForThreading);
                th.Start();
                if (cLogClass != null)
                {
                    cLogClass.Warn("Client Socket Reconnected!");
                }
            }
        }

        /// <summary>
        /// 연결 시도 중인지 확인 플래그
        /// </summary>
        private bool bConnecting = false;

        /// <summary>
        /// 재접속 진행
        /// </summary>
        public void Reconnect()
        {
            if (bConnecting == true) return;
            bConnecting = true;
            Thread th = new Thread(ConnectServerForThreading);
            th.Start();
            if (cLogClass != null)
            {
                cLogClass.Warn("Client Socket Reconnected!");
            }
        }

        /// <summary>
        /// 서버 연결 종료
        /// </summary>
        public void CloseTCPClient()
        {
            if (cTCPClientProcess != null)
            {
                cTCPClientProcess.Disconnect();
                AddLog("Client Socket Closed!");
            }
        }

        #endregion 1. 서버 연결 및 접속 종료

        #region 2. 서버 접속 상태가 변경된 경우 Call Back

        /// <summary>
        /// Client 접속이 된 경우
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="status"></param>
        public void ServerConnection()
        {
            bConncect = true;
            if (connectionStatusChanged != null)
            {
                connectionStatusChanged(true);
                AddLog("Client Socket Connection Status Connected");
            }
        }

        /// <summary>
        /// Server 연결 상태를 전달
        /// </summary>
        /// <returns></returns>
        public bool GetServerConnectionStatus()
        {
            if (cTCPClientProcess != null)
            {
                return cTCPClientProcess.bConnected;
            }
            return false;
        }

        #endregion 2. 서버 접속 상태가 변경된 경우 Call Back

        #region 3. 서버로 부터 Data가 수신된 경우 Call Back

        /// <summary>
        /// 서버로 부터 Data 수신시 호출
        /// </summary>
        /// <param name="strByteMsg"></param>
        private void CallBackDataReceived(byte[] strByteMsg)
        {
            string strMsg = Encoding.Default.GetString(strByteMsg, 0, strByteMsg.Length);

            if (bReceiveStringByteChoice == true)
            {
                if (delReceiveStringDataServer != null)
                {
                    delReceiveStringDataServer(strMsg);
                }
            }
            else
            {
                if (delReceiveByteDataServer != null)
                {
                    delReceiveByteDataServer(strByteMsg);
                }
            }

            AddLog(string.Format("[ReceiveDataFormServer] : {0}", strMsg));
        }

        #endregion 3. 서버로 부터 Data가 수신된 경우 Call Back

        #region 4. 서버로 Data Send

        /// <summary>
        /// Data를 Server로 전송
        /// </summary>
        /// <param name="strSendMsg"></param>
        public bool SendData(string strSendMsg)
        {
            if (cTCPClientProcess == null ||
                cTCPClientProcess.bConnected != true)
            {
                if (cLogClass != null)
                {
                    cLogClass.Error(string.Format("[Error] Can Not Send To Server !!\nSend Data : {0}", strSendMsg));
                }
                Thread.Sleep(10);
                return false;
            }

            if (string.IsNullOrEmpty(strSendMsg))
            {
                if (cLogClass != null)
                {
                    cLogClass.Error("Data Send NULL Data.");
                }
                return false;
            }

            try
            {
                cTCPClientProcess.SendMsg(strSendMsg);
                AddLog("Send Data : " + strSendMsg);
                return true;
            }
            catch (Exception ex)
            {
                if (cLogClass != null)
                {
                    cLogClass.Fatal(string.Format("[Excpetion Error] Can Not Send To Server !!\n{0}", ex.ToString()));
                }
                return false;
            }
        }

        /// <summary>
        /// Data를 Server로 전송
        /// </summary>
        /// <param name="byteSendMsg"></param>
        public bool SendData(byte[] byteSendMsg)
        {
            string strSendMsg = Encoding.Default.GetString(byteSendMsg, 0, byteSendMsg.Length);

            if (cTCPClientProcess == null ||
                cTCPClientProcess.bConnected != true)
            {
                if (cLogClass != null)
                {
                    cLogClass.Error(string.Format("[Error] Can Not Send To Server !!\nSend Data : {0}", strSendMsg));
                }
                Thread.Sleep(10);
                return false;
            }

            if (string.IsNullOrEmpty(strSendMsg))
            {
                if (cLogClass != null)
                {
                    cLogClass.Error("Data Send NULL Data.");
                }
                return false;
            }

            try
            {
                cTCPClientProcess.SendMsg(byteSendMsg);
                AddLog("Send Data : " + strSendMsg);
                return true;
            }
            catch (Exception ex)
            {
                if (cLogClass != null)
                {
                    cLogClass.Fatal(string.Format("[Excpetion Error] Can Not Send To Server !!\n{0}", ex.ToString()));
                }
                return false;
            }
        }

        #endregion 4. 서버로 Data Send
    }
}