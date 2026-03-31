using System.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// 미쓰비시 Melfa 통신 프로토콜
    /// 통신 프로토콜 전송 정의 : <Robot No.>;<Slot No>;<Command><Argument>
    /// <Robot No.> : 조작할 로봇 번호(0 ~ 3) 생략 시 1번 로봇 자동 지정 0번 지정 시 모든 로봇에 지령
    /// <Slot No.> : 조작할 슬롯 번호(0 ~ 33), TASKMAX + 1, 편집 슬롯(9) 생략 시 1번 슬롯 자동 지정 0번 지정 시 모든 슬롯에 지령
    /// <Command><Argument> : 각각의 명령어 참조
    /// 통신 프로토콜 응답 정의 : QoK<Answer> or QeR <Error No.>
    /// <Answer> : 명령 실행에 대한 결과 응답(각각의 명령어 참조)
    /// <Error No.> : 명령을 실행할 수 없을 때 에러 코드 응답 에러 코드는 컨트롤러 취급설명서(트러블 슈팅) 참조
    /// </summary>
    public class MitsubishiMelfa
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
        /// 기기 번호
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
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, $"{strIP} MitsubishiMelfa Not Server Connect");
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
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, $"{strIP} [Client] The client has connected to the MitsubishiMelfa server.");
            }
            else
            {
                if (CMainLib.Ins.McState == eMachineState.RUN) CMainLib.Ins.AddError((eErrorCode)iNo); // 에러 코드 추가 필요.
                TCPConnectStatus?.Invoke(false);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, $"{strIP} [Client] The client has been disconnected from the MitsubishiMelfa server.");
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
        /// XYZ 타입 모든 축 위치 읽기
        /// </summary>
        /// <param name="dPos"></param>
        /// <returns></returns>
        public bool GetXYZPos(ref double[] dPos)
        {
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData($"1;1;PPOSF");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(200) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "GetXYZPos : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[0].Contains("QoK"))
                {
                    double.TryParse(strCommRtn[1], out dPos[0]);
                    double.TryParse(strCommRtn[3], out dPos[1]);
                    double.TryParse(strCommRtn[5], out dPos[2]);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Joint 타입 모든 축 위치 읽기
        /// </summary>
        /// <param name="iAxis"></param>
        /// <param name="dPos"></param>
        /// <returns></returns>
        public bool GetJointPos(int iAxis, ref double dPos)
        {
            if (cTCPClient.bConncect == false) return false;
            if (iAxis < 1 || iAxis > 7) return false;
            lock (Lock)
            {
                cTCPClient.SendData($"1;1;JPOS{iAxis}");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(200) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "GetJointPos : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[0].Contains("QoK"))
                {
                    double.TryParse(strCommRtn[1], out dPos);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// OP 속도 읽기
        /// </summary>
        /// <param name="iVel"></param>
        /// <returns></returns>
        public bool GetOverride(ref int iVel)
        {
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData($"1;1;OVRD");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(200) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "GetOverride : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[0].Contains("QoK"))
                {
                    string strData = strCommRtn[0].Replace("QoK", "");
                    int.TryParse(strData, out iVel);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// OP 속도 설정
        /// </summary>
        /// <returns></returns>
        public bool SetOverride(int iVel)
        {
            if (cTCPClient.bConncect == false) return false;
            if (iVel <= 0 || iVel > 100) return false;
            lock (Lock)
            {
                cTCPClient.SendData($"1;1;OVRD={iVel}");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(200) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "SetOverride : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[0].Contains("QoK"))
                {
                    string strData = strCommRtn[0].Replace("QoK", "");
                    int iGetVel;
                    int.TryParse(strData, out iGetVel);
                    if (iVel == iGetVel) return true;
                    else return false;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// I/O 읽기
        /// </summary>
        /// <param name="iInputNo"></param>
        /// <param name="iOutputNo"></param>
        /// <param name="uiInputData"></param>
        /// <param name="uiOutputData"></param>
        /// <returns></returns>
        public bool GetIO(int iInputNo, int iOutputNo, ref uint uiInputData, ref uint uiOutputData)
        {
            if (cTCPClient.bConncect == false) return false;
            if (iInputNo < 0 || iInputNo > 10) return false;
            if (iOutputNo < 0 || iOutputNo > 10) return false;
            lock (Lock)
            {
                cTCPClient.SendData($"1;1;IOSIGNAL{iInputNo};{iOutputNo}");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(200) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "GetIO : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[0].Contains("QoK"))
                {
                    string strData = strCommRtn[0].Replace("QoK", "");
                    uiInputData = uint.Parse(strData.Substring(0, 4), System.Globalization.NumberStyles.HexNumber);
                    uiOutputData = uint.Parse(strData.Substring(4, 4), System.Globalization.NumberStyles.HexNumber);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Output 읽기
        /// </summary>
        /// <param name="iOutputNo"></param>
        /// <param name="uiOutputData"></param>
        /// <returns></returns>
        public bool GetOutput(int iOutputNo, ref uint uiOutputData)
        {
            if (cTCPClient.bConncect == false) return false;
            if (iOutputNo < 0 || iOutputNo > 10) return false;
            lock (Lock)
            {
                cTCPClient.SendData($"1;1;OUT{iOutputNo}");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(200) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "GetOutput : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[0].Contains("QoK"))
                {
                    string strData = strCommRtn[0].Replace("QoK", "");
                    uiOutputData = uint.Parse(strData, System.Globalization.NumberStyles.HexNumber);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 조작권 온/오프 설정
        /// </summary>
        /// <param name="bOnOff"></param>
        /// <returns></returns>
        public bool SetControl(bool bOnOff)
        {
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                string strOnOff = bOnOff == true ? "ON" : "OFF";
                cTCPClient.SendData($"1;1;CNTL{strOnOff}");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(200) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "SetControl : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[0].Contains("QoK"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// SERVO 온/오프 설정
        /// </summary>
        /// <param name="bOnOff"></param>
        /// <returns></returns>
        public bool SetServo(bool bOnOff)
        {
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                string strOnOff = bOnOff == true ? "ON" : "OFF";
                cTCPClient.SendData($"1;1;SRV{strOnOff}");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(200) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "SetServo : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[0].Contains("QoK"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Break 온/오프 설정
        /// </summary>
        /// <param name="bOnOff"></param>
        /// <returns></returns>
        public bool SetBreak(bool bOnOff)
        {
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                string strOnOff = bOnOff == true ? "00" : "FF";
                cTCPClient.SendData($"1;1;BREAKON{strOnOff}");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(200) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "SetBreak : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[0].Contains("QoK"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Program Start
        /// </summary>
        /// <param name="strProgramName"></param>
        /// <returns></returns>
        public bool ProgramStart(string strProgramName = "MAIN")
        {
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData($"1;1;RUN{strProgramName};1");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(200) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "ProgramStart : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[0].Contains("QoK"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Program Stop
        /// </summary>
        /// <returns></returns>
        public bool ProgramStop()
        {
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData($"1;1;STOP");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(200) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "ProgramStop : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[0].Contains("QoK"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Program Reset
        /// </summary>
        /// <returns></returns>
        public bool ProgramReset()
        {
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData($"1;1;SLOTINIT");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(200) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "ProgramReset : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[0].Contains("QoK"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 핸드 1 온/오프 설정
        /// </summary>
        /// <param name="iHandNo"></param>
        /// <param name="bOnOff"></param>
        /// <returns></returns>
        public bool SetHand(int iHandNo, bool bOnOff)
        {
            if (cTCPClient.bConncect == false) return false;
            if (iHandNo < 1 || iHandNo > 8) return false;
            lock (Lock)
            {
                string strOnOff = bOnOff == true ? "ON" : "OFF";
                cTCPClient.SendData($"1;1;HND{strOnOff}{iHandNo}");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(200) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "SetHand : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[0].Contains("QoK"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 스타트 상태 확인
        /// </summary>
        /// <param name="bStatus"></param>
        /// <returns></returns>
        public bool GetStart(ref bool bStatus)
        {
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData($"1;1;STATE");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(200) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "GetStart : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[0].Contains("QoK"))
                {
                    string strData = strCommRtn[4];
                    if (strData == string.Empty) bStatus = false;
                    int iGetE = 0;
                    if (strData.Length == 5) iGetE = System.Int32.Parse(strData.Substring(0, 1), System.Globalization.NumberStyles.HexNumber);
                    else if (strData.Length == 6) iGetE = System.Int32.Parse(strData.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    if ((iGetE & 0xE) == 0xE) bStatus = true;
                    else bStatus = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Error Reset
        /// </summary>
        /// <returns></returns>
        public bool ErrorReset()
        {
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData($"1;1;RSTALRM");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(200) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "ErrorReset : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[0].Contains("QoK"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Error No 읽기
        /// </summary>
        /// <param name="iErrorNo"></param>
        /// <returns></returns>
        public bool GetErrorNo(ref int iErrorNo)
        {
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData($"1;1;ERROR");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(200) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "GetErrorNo : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[0].Contains("QoK"))
                {
                    string strData = strCommRtn[0].Replace("QoK", "");
                    if (strData == string.Empty) iErrorNo = -1;
                    else int.TryParse(strData, out iErrorNo);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Error 메세지 읽기
        /// </summary>
        /// <param name="iError"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public bool GetErrorString(int iError, ref string strError)
        {
            if (cTCPClient.bConncect == false) return false;
            lock (Lock)
            {
                cTCPClient.SendData($"1;1;ERRORMES{iError}");
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(200) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, "GetErrorNo : No received");
                    hEvent.Reset();
                    return false;
                }

                // 통신 받은 데이터 처리
                hEvent.Reset();
                if (strCommRtn[0].Contains("QoK"))
                {
                    strError = strCommRtn[0].Replace("QoK", "");
                    return true;
                }
                else
                {
                    return false;
                }
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
                    strReceiveData = strReceiveData.Substring(0, strReceiveData.Length);
                    string[] receiveText_Split_Main = strReceiveData.Split(';');
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