using System.Text;
using System.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// 키엔스 레이저 마커 통신 프로토콜
    /// </summary>
    public class KeyenceLaserMarker
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
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, $"{strIP} KeyenceLaserMarker Not Server Connect");
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
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, $"{strIP} [Client] The client has connected to the KeyenceLaserMarker server.");
            }
            else
            {
                if (CMainLib.Ins.McState == eMachineState.RUN) CMainLib.Ins.AddError(eErrorCode.LASER1_CLIENT_NOT_CONNECT + iNo);
                TCPConnectStatus?.Invoke(false);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, $"{strIP} [Client] The client has been disconnected from the KeyenceLaserMarker server.");
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
        /// Ready 상태 확인
        /// </summary>
        /// <param name="iRtn">1:READYOFF(에러 발생 중), 2:READYOFF(인쇄 중 또는 전개중)</param>
        /// <returns></returns>
        public bool GetReady(ref int iRtn)
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData($"RX,Ready\r");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "GetReady : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[1] == "OK")
                {
                    if (strCommRtn[2] == "0") return true;
                    else if (strCommRtn[2] == "1") iRtn = 1;
                    else if (strCommRtn[2] == "2") iRtn = 2;
                    return false;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 인쇄를 시작합니다.
        /// </summary>
        /// <returns></returns>
        public bool StartMarking()
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData($"WX,StartMarking\r");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "StartMarking : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[1] == "OK") return true;
                else return false;
            }
        }

        /// <summary>
        /// 파라미터 A 에서 지정한 가이드 레이저의 종류로, 가이드 레이저 인쇄를 시작합니다. 가이드 레이저 1 회를 제외하고는 30 초간 연속
        /// 조사하여, 가이드 레이저 1 회는 인쇄시간과 동일한 시간동안 조사합니다.
        /// </summary>
        /// 가이드 레이저 종류
        ///1:1 회
        ///2:연속
        ///3:영역 테두리
        ///4:워크 이미지
        ///5:블록 테두리
        /// <returns></returns>
        public bool GuideLaser()
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData($"WX,GuideLaser=4\r");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "GuideLaser : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[1] == "OK") return true;
                else return false;
            }
        }

        /// <summary>
        /// 가이드 레이저 인쇄를 중단합니다.
        /// </summary>
        /// <returns></returns>
        public bool StopMarking()
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData($"WX,StopMarking\r");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "StopMarking : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[1] == "OK") return true;
                else return false;
            }
        }

        /// <summary>
        /// 디스턴스 포인터를 점등/소등합니다.
        /// </summary>
        /// <param name="iOnOff">0:소등, 1:점등</param>
        /// <returns></returns>
        public bool DistancePointer(int iOnOff)
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData($"WX,DistancePointer={iOnOff}\r");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "DistancePointer : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[1] == "OK") return true;
                else return false;
            }
        }

        /// <summary>
        /// 레이저 마킹기에서 발생한 에러를 요구합니다.
        /// </summary>
        /// <param name="strArrError"></param>
        /// <returns></returns>
        public bool GetErrorInfo(ref bool bError, ref string[] strArrError)
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData($"RX,Error\r");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "GetErrorInfo : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[1] == "OK")
                {
                    if (strCommRtn[2] == "1")
                    {
                        bError = true;
                        int iLength = strCommRtn.Length - 3;
                        strArrError = new string[iLength];
                        for (int i = 0; i < iLength; i++)
                        {
                            strArrError[i] = strCommRtn[i + 3];
                        }
                    }
                    else
                    {
                        bError = false;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 레이저 마킹기에서 발생한 에러를 해제합니다.
        /// </summary>
        /// <returns></returns>
        public bool ErrorClear()
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData($"WX,ErrorClear\r");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "ErrorClear : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[1] == "OK") return true;
                else return false;
            }
        }

        /// <summary>
        /// 프로그램 번호를 설정합니다.
        /// </summary>
        /// <param name="iPrgNo"></param>
        /// <returns></returns>
        public bool SetProgramNo(int iPrgNo)
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                string strSendData = string.Format($"WX,ProgramNo={iPrgNo.ToString("D4")}\r");
                cTCPClient.SendData(strSendData);
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "SetProgramNo : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[1] == "OK") return true;
                else return false;
            }
        }

        /// <summary>
        /// 문자열 또는 로고 및 사진 파일을 변경/요구합니다
        /// </summary>
        /// <param name="iPrgNo"></param>
        /// <param name="iBlkNo"></param>
        /// <param name="strData"></param>
        /// <returns></returns>
        public bool SetCharacterString(int iPrgNo, int iBlkNo, string strData)
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                string strSendData = string.Format($"WX,PRG={iPrgNo},BLK={iBlkNo},CharacterString={strData}\r");
                cTCPClient.SendData(strSendData);
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "SetCharacterString : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[1] == "OK") return true;
                else return false;
            }
        }

        /// <summary>
        /// 문자열 또는 로고 및 사진 파일을 변경/요구합니다
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public bool SetCharacterString(string[] strData)
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return true;
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                StringBuilder sbSendData = new StringBuilder();
                sbSendData.Append("WX,");
                for (int i = 0; i < strData.Length; i++)
                {
                    if (i != strData.Length - 1) sbSendData.Append($"BLK={i},CharacterString={strData[i]},");
                    else sbSendData.Append($"BLK={i},CharacterString={strData[i]}");
                }
                sbSendData.Append("\r");
                string strSendData = sbSendData.ToString();
                cTCPClient.SendData(strSendData);
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(5000) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "SetCharacterString : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[1] == "OK") return true;
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
                    string[] receiveText_Split_Main = strReceiveData.Split(',');
                    strCommRtn = receiveText_Split_Main;
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