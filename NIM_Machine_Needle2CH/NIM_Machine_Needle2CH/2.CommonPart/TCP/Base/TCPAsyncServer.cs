using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace MachineControlBase
{
    /// <summary>
    /// 비동기 형식 TCP/IP Server
    /// </summary>
    public class CTCPAsyncServer
    {
        #region Object Define

        /// <summary>
        /// 서버의 시작 여부
        /// </summary>
        private bool bServertStart { get; set; } = false;

        /// <summary>
        /// 서버의 시작 여부
        /// </summary>
        /// <returns></returns>
        public bool bServerStart()
        {
            return bServertStart;
        }

        /// <summary>
        /// TCP/IP Async Server
        /// </summary>
        private CTCPServerProcess cTCPServerProcess = null;

        #endregion Object Define

        #region FileDataLog Class Ref 정의

        /// <summary>
        /// TCP/IP Log Class
        /// </summary>
        private Logger cLogClass = null;

        /// <summary>
        /// TCPServerProcess에서 사용하는 로그 델리게이트 함수
        /// </summary>
        /// <param name="strLog"></param>
        private void AddLog(string strLog)
        {
            try
            {
                if (cLogClass != null)
                {
                    cLogClass.Info(strLog);
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

        #endregion FileDataLog Class Ref 정의

        #region 외부 Delegate 정의

        /// <summary>
        /// Client Connecttion Change Event
        /// </summary>
        public delegate void ClientConnectChangeEvent();

        /// <summary>
        /// Client 연결 발생시 호출되는 Call Back
        /// </summary>
        private ClientConnectChangeEvent ClientConnectedEvent = null;

        /// <summary>
        /// Client 연결 해제시 호출되는 Call Back
        /// </summary>
        private ClientConnectChangeEvent ClientDisconnectedEvent = null;

        /// <summary>
        /// Data 수신완료 시 호출되는 외부 CallBack
        /// </summary>
        /// <param name="strRecvMsg"></param>
        public delegate void EventReceiveStringDataClient(string strRecvMsg);

        public delegate void EventReceiveByteDataClient(byte[] byteRecvMsg);

        /// <summary>
        /// Data 수신완료 시 호출되는 외부 CallBack
        /// </summary>
        private EventReceiveStringDataClient delReceiveStringDataClient;

        private EventReceiveByteDataClient delReceiveByteDataClient;

        /// <summary>
        /// 리턴 값이 string으로 받을지 byte[]로 받을지 선택 true : string
        /// </summary>
        public bool bReceiveStringByteChoice = false;

        #endregion 외부 Delegate 정의

        #region 서버 Start / 종료

        /// <summary>
        /// TCP Server Start 기능
        /// 접속요청을 받아들이기 위해 Listener Thread 구동 시작
        /// </summary>
        /// <param name="strIP"></param>
        /// <param name="uiPortNo"></param>
        /// <param name="ClientConnectedEvent"></param>
        /// <param name="ClientDisconnectedEvent"></param>
        /// <param name="delReceiveStringDataClient"></param>
        /// <param name="delReceiveByteDataClient"></param>
        /// <param name="bReceiveStringByteChoice"></param>
        /// <returns></returns>
        public bool TCPServerStart(string strIP, uint uiPortNo,
                    ClientConnectChangeEvent ClientConnectedEvent,
                    ClientConnectChangeEvent ClientDisconnectedEvent,
                    EventReceiveStringDataClient delReceiveStringDataClient = null,
                    EventReceiveByteDataClient delReceiveByteDataClient = null,
                    bool bReceiveStringByteChoice = true)
        {
            try
            {
                this.ClientConnectedEvent = ClientConnectedEvent;
                this.ClientDisconnectedEvent = ClientDisconnectedEvent;
                this.delReceiveStringDataClient = delReceiveStringDataClient;
                this.delReceiveByteDataClient = delReceiveByteDataClient;
                this.bReceiveStringByteChoice = bReceiveStringByteChoice;

                // create server control and add event handler
                if (cTCPServerProcess == null)
                {
                    cTCPServerProcess = new CTCPServerProcess();
                    cTCPServerProcess.cLogClass = cLogClass;
                    cTCPServerProcess.delUpdateClientList += new EventUpdateClientList(ClientCconnected);
                    //cTCPServerProcess.delServerStarted += new EventServerStarted(ClientConnectedEvent);
                    cTCPServerProcess.delServerStopped += new EventServerStopped(ClientDisconnected);
                    cTCPServerProcess.delOnReceived += new EventOnReceived(RecvDataFromClient);
                }

                if (cTCPServerProcess == null)
                {
                    CCommon.ShowMessageMini("Server is not created");
                    return false;
                }

                if (cTCPServerProcess.ServerStart(strIP, uiPortNo))
                {
                    bServertStart = true;
                    AddLog(string.Format("TCPServerStarted IP Address [{0}], PortNo:{1}", strIP, uiPortNo));
                    return true;
                }
                else
                {
                    if (cLogClass != null)
                    {
                        cLogClass.Error(string.Format("TCPServerStarting Fail IP Address [{0}], PortNo:{1}", strIP, uiPortNo));
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (cLogClass != null)
                {
                    cLogClass.Fatal(string.Format("apTCPServerStart - Exception :{0}", ex.ToString()));
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// TCP Server 종료
        /// </summary>
        public void ServerStop()
        {
            cTCPServerProcess.StopServer();
            bServertStart = false;
            AddLog("TCPServer Stop");
        }

        #endregion 서버 Start / 종료

        #region 1.Client 접속 대기 pool / 이벤트 발생

        /// <summary>
        /// Client의 연결이 종료되었을때 호출되는 Call Back 함수
        /// </summary>
        /// <param name="_listClientEP"></param>
        private void ClientCconnected(List<string> _listClientEP)
        {
            try
            {
                if (_listClientEP.Count > 0)
                {
                    //File Log 기록
                    AddLog("Client connected");
                    if (ClientConnectedEvent != null) ClientConnectedEvent();
                }
                else
                {
                    AddLog("Client Disconnected");
                    if (ClientDisconnectedEvent != null) ClientDisconnectedEvent();
                }
            }
            catch (Exception ex)
            {
                if (cLogClass != null)
                {
                    cLogClass.Fatal(string.Format("ClientCconnected - Exception :{0}", ex.ToString()));
                }
            }
        }

        /// <summary>
        /// Client의 연결이 종료되었을때 호출되는 Call Back 함수
        /// </summary>
        private void ClientDisconnected()
        {
            try
            {
                AddLog("Client Disconnected");
                if (ClientDisconnectedEvent != null) ClientDisconnectedEvent();
            }
            catch (Exception ex)
            {
                if (cLogClass != null)
                {
                    cLogClass.Fatal(string.Format("ClientDisconnected - Exception :{0}", ex.ToString()));
                }
            }
        }

        #endregion 1.Client 접속 대기 pool / 이벤트 발생

        #region 2. Client로 부터 Data 수신처리 pool 및 처리 이벤트

        /// <summary>
        /// 수신 데이터 처리
        /// </summary>
        /// <param name="byteRecvData"></param>
        private void RecvDataFromClient(byte[] byteRecvData)
        {
            string strMsg = Encoding.Default.GetString(byteRecvData, 0, byteRecvData.Length);

            if (bReceiveStringByteChoice == true)
            {
                if (delReceiveStringDataClient != null)
                {
                    delReceiveStringDataClient(strMsg);
                }
            }
            else
            {
                if (delReceiveByteDataClient != null)
                {
                    delReceiveByteDataClient(byteRecvData);
                }
            }

            AddLog(string.Format("[ReceiveDataFormClient] : {0}", strMsg));
        }

        #endregion 2. Client로 부터 Data 수신처리 pool 및 처리 이벤트

        #region 3. Client Sending

        /// <summary>
        /// 특정 IP Address에 Data 전송
        /// </summary>
        /// <param name="strIP"></param>
        /// <param name="strSendData"></param>
        /// <returns></returns>
        public bool SendClient(string strIP, string strSendData)
        {
            if (string.IsNullOrEmpty(strSendData))
            {
                if (cLogClass != null)  // Log 기록
                {
                    cLogClass.Error(string.Format("[Error] Can not send EMPTY message to client !!\nSend Data : {0}", strSendData));
                }
                return false;
            }
            return SendData(strIP, strSendData);
        }

        /// <summary>
        /// 특정 IP로 Data 전송
        /// </summary>
        /// <param name="strIP"></param>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        private bool SendData(string strIP, string strMsg)
        {
            try
            {
                cTCPServerProcess.SendMsg(strIP, strMsg);
                AddLog(string.Format("SendData IP[{0}] : {1}", strIP, strMsg));
                return true;
            }
            catch (Exception ex)
            {
                if (cLogClass != null)  // Log 기록
                {
                    cLogClass.Fatal(string.Format("SendClient - Exception :{0}", ex.ToString()));
                }
                return false;
            }
        }

        /// <summary>
        /// 접속된 모든 Client에게 message(Data)를 보낸다
        /// </summary>
        /// <param name="strMsg"></param>
        private bool SendData(string strMsg)
        {
            try
            {
                if (string.IsNullOrEmpty(strMsg))
                {
                    if (cLogClass != null)  // Log 기록
                    {
                        cLogClass.Error(string.Format("[Error] Can not send EMPTY message to client !!\nSend Data : {0}", strMsg));
                    }
                    return false;
                }

                cTCPServerProcess.SendMsgAll(strMsg);
                AddLog(string.Format("All IP SendData : {0}", strMsg));
                return true;
            }
            catch (Exception ex)
            {
                if (cLogClass != null)  // Log 기록
                {
                    cLogClass.Fatal(string.Format("SendClient - Exception :{0}", ex.ToString()));
                }
                return false;
            }
        }

        #endregion 3. Client Sending

        #region 4. 상태 및 정보 편의 기능

        /// <summary>
        /// 접속된 IP Address List를 얻어온다.
        /// </summary>
        /// <returns></returns>
        public List<string> GetClientIPAddress()
        {
            List<string> ListStrIP = new List<string>();

            try
            {
                foreach (string strData in cTCPServerProcess.liststrClientEP)
                {
                    ListStrIP.Add(strData);
                }
                return ListStrIP;
            }
            catch { return ListStrIP; }
        }

        /// <summary>
        /// Client의 연결 상태 리턴
        /// </summary>
        /// <returns></returns>
        public bool GetClientConnectionStatus()
        {
            if (cTCPServerProcess != null)
            {
                if (cTCPServerProcess.liststrClientEP.Count > 0) return true;
                else return false;
            }
            return false;
        }

        #endregion 4. 상태 및 정보 편의 기능
    }
}