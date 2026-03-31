using System.Text;
using System.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// AIM 피더 통신 프로토콜
    /// </summary>
    public class AIM_FlexiblePartsFeeder
    {
        /// <summary>
        /// 클라이언트 통신
        /// </summary>
        public CTCPAsyncClient cTCPClient = null;

        /// <summary>
        /// 스레드 충돌 방지를 위한 Lock
        /// </summary>
        private object Lock = new object();

        /// <summary>
        /// 통신 이벤트
        /// </summary>
        private AutoResetEvent hEvent = new AutoResetEvent(false);

        /// <summary>
        /// 통신 응답 데이터
        /// </summary>
        private string[] strCommRtn = null;

        /// <summary>
        /// 통신 응답 데이터
        /// </summary>
        private string strCommRcv = string.Empty;

        /// <summary>
        /// 초기화
        /// </summary>
        public void Init()
        {
            cTCPClient = new CTCPAsyncClient();
        }

        public void Free()
        {
            // 통신 종료
            if (cTCPClient != null)
            {
                cTCPClient.Disconnect();
                cTCPClient.CloseTCPClient();
            }
        }

        /// <summary>
        /// 레이저 마커 번호
        /// </summary>
        private int iNo = 0;

        /// <summary>
        /// 로그 타입 설정
        /// </summary>
        private eLogType eLogType;

        /// <summary>
        /// 서버 IP 주소
        /// </summary>
        private string strIP = null;

        /// <summary>
        /// 통신을 연결합니다.
        /// </summary>
        /// <param name="iNo"></param>
        /// <param name="eLogType"></param>
        /// <param name="strIP"></param>
        /// <param name="uiPort"></param>
        public void Connect(int iNo, eLogType eLogType, string strIP, uint uiPort)
        {
            // TCP Client Start
            this.iNo = iNo;
            this.eLogType = eLogType;
            this.strIP = strIP;
            cTCPClient.SetLog(NLogger.GetLogClass(eLogType));
            if (cTCPClient.Connect(strIP, uiPort,
                                   bServerConnectStatus,
                                   OnVisionReceiveClient
                                   ) == false)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, $"{strIP} AIM_FlexiblePartsFeeder Not Server Connect");
            }
        }

        public event TCPConnectStatus_EventHandler TCPConnectStatus;

        /// <summary>
        /// Server와 연결 상태
        /// </summary>
        /// <param name="bStatus"></param>
        public void bServerConnectStatus(bool bStatus)
        {
            if (bStatus == true)
            {
                TCPConnectStatus?.Invoke(true);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, $"{strIP} [Client] The client has connected to the AIM_FlexiblePartsFeeder server.");
            }
            else
            {
                if (CMainLib.Ins.McState == eMachineState.RUN) CMainLib.Ins.AddError(eErrorCode.LASER1_CLIENT_NOT_CONNECT + iNo);
                TCPConnectStatus?.Invoke(false);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, $"{strIP} [Client] The client has been disconnected from the AIM_FlexiblePartsFeeder server.");
            }
        }

        /// <summary>
        /// 데이터 전달
        /// </summary>
        /// <param name="strSendData"></param>
        /// <returns></returns>
        public bool SendData(string strSendData)
        {
            return cTCPClient.SendData(strSendData);
        }

        /// <summary>
        /// 피더 모터 Servo On
        /// </summary>
        /// <returns></returns>
        public bool ServoOn()
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData("<C1>");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "AIM Feeder Servo On : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRcv == "<C1>") return true;
                else return false;
            }
        }

        /// <summary>
        /// 피더 모터 Servo Off
        /// </summary>
        /// <returns></returns>
        public bool ServoOff()
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData("<C2>");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "AIM Feeder Servo Off : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRcv == "<C2>") return true;
                else return false;
            }
        }

        /// <summary>
        /// 피더 모터 Home 진행
        /// </summary>
        /// <returns></returns>
        public bool Home()
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData("<C3>");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "GuideLaser : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRcv == "DONE") return true;
                else return false;
            }
        }

        /// <summary>
        /// 피더 보터 Servo On 후 Home 진행
        /// </summary>
        /// <returns></returns>
        public bool ServoOnHome()
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData("<C4>");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "StopMarking : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRcv == "<C4>") return true;
                else return false;
            }
        }

        /// <summary>
        /// 피더의 동작을 실행한다.
        /// </summary>
        /// <param name="Dist"> 거리 </param>
        /// <param name="Hz"> 주기 </param>
        /// <param name="Time"> 시간 </param>
        /// <returns></returns>
        public bool FeederRun(uint Dist, uint Hz, uint Time)
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                Dist *= 10; Time *= 10;
                string strSendData = $"<P0,{Dist},{Hz},{Time}>";
                cTCPClient.SendData(strSendData);
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "Feeder Run : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRcv == "DONE") return true;
                else return false;
            }
        }

        /// <summary>
        /// 피더의 1번 동작을 저장한다.
        /// </summary>
        /// <param name="Dist"> 거리 </param>
        /// <param name="Hz"> 주기 </param>
        /// <param name="Time"> 시간 </param>
        /// <returns></returns>
        public bool Funcion1_Save(uint Dist, uint Hz, uint Time)
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                Dist *= 10; Time *= 10;
                string strSendData = $"<P1,{Dist},{Hz},{Time}>";
                cTCPClient.SendData(strSendData);
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "Feeder Funcion1 Save : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRcv == strSendData) return true;
                else return false;
            }
        }

        /// <summary>
        /// 피더의 2번 동작을 저장한다.
        /// </summary>
        /// <param name="Dist"> 거리 </param>
        /// <param name="Hz"> 주기 </param>
        /// <param name="Time"> 시간 </param>
        /// <returns></returns>
        public bool Funcion2_Save(uint Dist, uint Hz, uint Time)
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                Dist *= 10; Time *= 10;
                string strSendData = $"<P2,{Dist},{Hz},{Time}>";
                cTCPClient.SendData(strSendData);
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "Feeder Funcion2 Save : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRcv == strSendData) return true;
                else return false;
            }
        }

        /// <summary>
        /// 피더의 1번 동작을 저장한다.
        /// </summary>
        /// <returns></returns>
        public bool Funcion1_Run()
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData("<F1>");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "Feeder Funcion1 Run : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRcv == "<F1>") return true;
                else return false;
            }
        }

        /// <summary>
        /// 피더의 2번 동작을 저장한다.
        /// </summary>
        /// <returns></returns>
        public bool Funcion2_Run()
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData("<F2>");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "Feeder Funcion2 Run : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRcv == "<F2>") return true;
                else return false;
            }
        }

        /// <summary>
        /// Funcion을 Read 한다
        /// </summary>
        /// <returns></returns>
        public bool FuncionRead()
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData("<R>");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "Funcion Read : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRcv == "<R>") return true;
                else return false;
            }
        }

        /// <summary>
        /// 상태 확인
        /// </summary>
        /// <returns></returns>
        public bool State()
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData("<S>");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "State : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRcv == "<S>") return true;
                else return false;
            }
        }

        /// <summary>
        /// 백라이트 조명 On
        /// </summary>
        /// <returns></returns>
        public bool LED_On()
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData("<L1>");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "LED On : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRcv == "OK") return true;
                else return false;
            }
        }

        /// <summary>
        /// 백라이트 조명 Off
        /// </summary>
        /// <returns></returns>
        public bool LED_Off()
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData("<L2>");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "LED On : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRcv == "OK") return true;
                else return false;
            }
        }

        /// <summary>
        /// 피더 BootReset
        /// </summary>
        /// <returns></returns>
        public bool BootReset()
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData("<B>");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "Boot Reset : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRcv == "<B>") return true;
                else return false;
            }
        }

        private static object ReceiveLock = new object();

        /// <summary>
        /// 서버에서 클라이언트로 보내온 데이터
        /// </summary>
        /// <param name="strReceiveData"></param>
        private void OnVisionReceiveClient(string strReceiveData)
        {
            try
            {
                lock (ReceiveLock)
                {
                    strReceiveData = strReceiveData.Substring(0, strReceiveData.Length - 1);
                    strCommRcv = strReceiveData;
                    //string[] receiveText_Split_Main = strReceiveData.Split(',');
                    //strCommRtn = receiveText_Split_Main;
                    hEvent.Set();
                }
            }
            catch
            {
                NLogger.AddLog(eLogType, NLogger.eLogLevel.FATAL, "Client receive exception : " + strReceiveData);
            }
        }
    }
}