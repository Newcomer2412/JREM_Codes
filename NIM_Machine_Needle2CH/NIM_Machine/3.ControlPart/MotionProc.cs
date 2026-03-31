using static EMotionSnetBase.SnetDevice;
using static EMotionUniBase.UniDevice;

namespace MachineControlBase
{
    /// <summary>
    /// 다른 제어기를 공통으로 명령 내리고 관리하기 위한 인터페이스
    /// </summary>
    public interface MotionProcMgr
    {
        eMotor eAxis { get; set; }
        uint uiControllerAxis { get; set; }
        uint uiVelocity { get; set; }
        uint uiJogVel { get; set; }
        uint uiAccel { get; set; }
        uint uiDecel { get; set; }
        double dScale { get; set; }
        uint uiHomeVel { get; set; }
        uint uiHomeAcc { get; set; }
        uint uiHomeDec { get; set; }
        double dHomeOffset { get; set; }
        uint uiHomeClearTime { get; set; }
        bool bHomeUseZPhase { get; set; }
        int iHomeSenserType { get; set; }
        int iHomeMoveDirection { get; set; }
        uint uiHomeTimeout { get; set; }
        uint uiMotionTimeout { get; set; }
        bool bSimMoveDone { get; set; }
        double dSimCmdPos { get; set; }
        bool BreakStop { get; set; }
        bool HomeComplete { get; set; }
        int PosIndex { get; set; }

        AxisPositionData axisPositionData { get; set; }

        void Init();

        void Free();

        bool Home();

        bool HomeDone();

        bool WaitHome(ref string strFailMsg);

        bool MoveAbsolute(double dPos, bool bSafe = true);

        bool MoveAbsolute(int iPosIndex, bool bSafe = true);

        bool MoveAbsolute(int iPosIndex, uint uiPercent, bool bSafe = true);

        bool MoveAbsolute(double dPos, uint uiPercent, bool bSafe = true);

        bool MoveAbsolute(double dPos, uint uiVel, uint uiAcc, uint uiDec, bool bSafe = true);

        bool MoveRelative(double dPos, bool bSafe = true);

        bool MoveRelative(int iPosIndex, bool bSafe = true);

        bool MoveRelativeNoVelConv(int iPosIndex, bool bSafe = true);

        bool MoveRelative(int iPosIndex, uint uiPercent, bool bSafe = true);

        bool MoveRelative(double dPos, uint uiPercent, bool bSafe = true);

        bool MoveRelative(double dPos, uint uiVel, uint uiAcc, uint uiDec, bool bSafe = true);

        bool MoveVelocity(bool bCW, bool bSafe = true);

        bool MoveVelocity(bool bCW, uint uiVel, bool bSafe = true);

        void WaitDone();

        bool IsMoveDone();

        bool Stop();

        void EmergencyStop();

        void SetZero();

        void Reset();

        bool SetTrqLimit(int iTrq);

        bool GetTrqLimit(ref int iTrq);

        bool MoveAxisUntilTrqLimit(double dPos, uint uiVel, uint uiTime, bool bSafe = true);

        bool SetTrqLimitAndMove(double dPos1, double dPos2, int iSettrq_1, int iSettrq_2, uint uiTime, uint uiVel1, uint uiVel2, bool bSafe = true);

        bool GetStatusTrqLimitAndMove();

        bool ResetTrqLimitAndMove();

        void SetServoState(bool bOn);

        bool GetServoState();

        bool GetControllerFault();

        bool GetServoFault();

        bool GetMinusLimit();

        bool GetPlusLimit();

        bool GetHomeSensor();

        bool GetAlarm();

        double GetCmdPostion();

        double GetActPostion();

        bool GetInPosition();

        bool GetServoReady();
    }

    /// <summary>
    /// SNET RTEX 연결 클래스
    /// IO는 이 클래스에서 제어하고 축 제어는 축 관리 클래스에서 제어
    /// </summary>
    public class eMotionTekRTEX
    {
        /// <summary>
        /// Main Lib
        /// </summary>
        private CMainLib ml;

        /// <summary>
        /// 제어기 Net 번호 IP 끝자리
        /// </summary>
        public int iNet = 1;

        /// <summary>
        /// 이모션텍 모션제어기 연결 끊김 Flag
        /// </summary>
        public bool bConnect = false;

        /// <summary>
        /// 생성자
        /// </summary>
        public eMotionTekRTEX()
        {
            ml = CMainLib.Ins;
        }

        /// <summary>
        /// Module Init
        /// </summary>
        /// <param name="iNet"></param>
        public bool Init(int iNet)
        {
            if (Define.SIMULATION == true) return true;
            this.iNet = iNet;
            int iStatus = eSnetConnectEx(192, 168, 241, iNet);
            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (CCommon.ShowMessage(0, $"이모션텍 Servo 제어기 '{iNet}' 연결실패...\n프로그램을 종료합니다.") == (int)eMBoxRtn.A_OK)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("eMotionTek Rtex{0} Connect Fail : RESULT NUM : {1}", iNet, iStatus));
                    return false;
                }
            }
            bConnect = true;
            return true;
        }

        /// <summary>
        /// disconnect
        /// </summary>
        public void Free()
        {
            if (Define.SIMULATION == true) return;

            if (bConnected() == true)
            {
                int iStatus = eSnetDisconnect(iNet);
                if (iStatus != (int)eSnetApiReturnCode.Success)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("eMotionTek Rtex{0} Disconnect Fail : RESULT NUM : {1}", iNet, iStatus));
                }
            }
        }

        /// <summary>
        /// 통신 제대로 되는지 체크
        /// </summary>
        /// <returns></returns>
        public bool bConnected()
        {
            return bConnect;
        }

        #region IO 관련 함수

        /// <summary>
        /// Set Output For Time
        /// </summary>
        /// <param name="iCh"></param>
        /// <param name="iPort"></param>
        /// <param name="iData"></param>
        /// <param name="iMsTime"></param>
        /// <returns></returns>
        public bool SetOutputForTime(int iCh, int iPort, int iData, int iMsTime)
        {
            if (Define.SIMULATION == true) return true;
            if (bConnect == false) return false;

            int iStatus = eSnetSetOutputForTime(iNet, iCh, 1, iPort, iMsTime, iData);

            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == (int)eSnetApiReturnCode.Disconnected ||
                    iStatus == (int)eSnetApiReturnCode.TimeOut) bConnect = false;
                string strRtn = string.Format("SetOutputForTime Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Set Output For Time
        /// </summary>
        /// <param name="iCh"></param>
        /// <returns></returns>
        public bool GetOutputForTimeRun(int iCh)
        {
            if (Define.SIMULATION == true) return true;
            if (bConnect == false) return false;
            int iRunStatus = -1;

            int iStatus = eSnetGetOutputForTimeRun(iNet, iCh, out iRunStatus);

            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == (int)eSnetApiReturnCode.Disconnected ||
                    iStatus == (int)eSnetApiReturnCode.TimeOut) bConnect = false;
                string strRtn = string.Format("SetOutputForTime Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                return false;
            }

            // 출력이 완료됨. 1이면 출력 중.
            if (iRunStatus == 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 정한 시간 출력 On 기능 초기화
        /// </summary>
        /// <param name="iCh"></param>
        /// <returns></returns>
        public bool ResetOutputForTime(int iCh)
        {
            if (Define.SIMULATION == true) return true;
            if (bConnect == false) return false;

            int iStatus = eSnetResetOutputForTime(iNet, iCh);

            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == (int)eSnetApiReturnCode.Disconnected ||
                    iStatus == (int)eSnetApiReturnCode.TimeOut) bConnect = false;
                string strRtn = string.Format("ResetOutputForTime Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                return false;
            }

            return true;
        }

        /// <summary>
        /// "Latch 입력" 접점 번호를 설정하고 기능을 활성화합니다.
        /// 사용자가 "eSnetGetLatchInput" 함수를 사용하여 상태를 읽기 전까지 입력 상태를 유지합니다.
        /// </summary>
        /// <param name="iCh">입력 채널 번호 선택: 0 ~ 3 최대 4 개의 Latch 입력을 지정할 수 있습니다.</param>
        /// <param name="iBitNum">입력 번호</param>
        /// <param name="iEdge">"0": B 접점(Normal Close), ON->OFF 상태 입력, "1": A 접점(Normal Open), OFF->ON 상태 입력</param>
        /// <returns></returns>
        public bool SetLatchInput(int iCh, int iBitNum, int iEdge)
        {
            // In type : "0"->Rtex Driver Input(X4) / "1"-> Remote Input / "2"->Rtex Input(INP1~INP6)
            if (Define.SIMULATION == true) return true;
            if (bConnect == false) return false;

            int iPort = iBitNum / 32;
            int iPoint = iBitNum % 32;
            int iStatus = eSnetSetLatchInput(iNet, iCh, 1, iPort, iPoint, iEdge);

            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == 1) bConnect = false;
                string strRtn = string.Format("SetLatchInput Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 래치 입력 상태를 읽습니다
        /// </summary>
        /// <param name="iCh"></param>
        /// <param name="bInput"></param>
        /// <returns></returns>
        public bool GetLatchInput(int iCh, ref bool bInput)
        {
            if (Define.SIMULATION == true) return true;

            if (bConnect == false) return false;
            int iInput = 0;

            int iStatus = eSnetGetLatchInput(iNet, iCh, out iInput);

            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == 1) bConnect = false;
                string strRtn = string.Format("GetLatchInput Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                return false;
            }

            if (iInput == 1) bInput = true;
            else bInput = false;

            return true;
        }

        /// <summary>
        /// 래치 입력 상태를 초기화 합니다.
        /// </summary>
        /// <param name="iCh"></param>
        /// <returns></returns>
        public bool ResetLatchInput(int iCh)
        {
            if (Define.SIMULATION == true) return true;

            if (bConnect == false) return false;

            int iStatus = eSnetResetLatchInput(iNet, iCh);

            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == 1) bConnect = false;
                string strRtn = string.Format("ResetLatchInput Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 각 축들의 Input 센서 상태를 가져옴
        /// </summary>
        /// <param name="iAxis"></param>
        /// <returns></returns>
        public bool GetMotorInput(ref int[] iAxis)
        {
            if (bConnect == false) return false;

            int iStatus = eSnetGetMcbUserInput(iNet, out iAxis[0]);
            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == (int)eSnetApiReturnCode.Disconnected ||
                    iStatus == (int)eSnetApiReturnCode.TimeOut) bConnect = false;
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("eMotionTek Rtex{0} GetContinueMotorStop Error : RESULT NUM : {1}", iNet, iStatus));
                return false;
            }
            return true;
        }

        #endregion IO 관련 함수

        #region Trigger 함수

        /// <summary>
        /// Trigger 에 관련된 Parameter 설정
        /// </summary>
        /// <param name="iChNo">Trigger 출력을 내보낼 채널 번호</param>
        /// <param name="iAxisNo">구동 축</param>
        /// <param name="iPulseOnTime">Trigger Signal 이 On 을 유지하는 시간 (msec)</param>
        /// <param name="iSetLevel">Trigger output 극성 : Normal Open (A 접점)</param>
        /// <param name="iSetMode">Trigger 출력 위치 소스 : Command Position</param>
        /// <returns></returns>
        public bool SetTriggerTimeLevel(int iChNo, int iAxisNo, int iPulseOnTime, int iSetLevel, int iSetMode)
        {
            if (bConnect == false) return false;

            int iStatus = eSnetSetTriggerTimeLevel(iNet, iChNo, iAxisNo, iPulseOnTime, iSetLevel, iSetMode);

            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == (int)eSnetApiReturnCode.Disconnected ||
                    iStatus == (int)eSnetApiReturnCode.TimeOut) bConnect = false;
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("eMotionTek Rtex{0} SetTriggerTimeLevel Error : RESULT NUM : {1}", iNet, iStatus));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 트리거 기능을 활성화하여 사용자가 정한 위치에서 Trigger 를 출력
        /// </summary>
        /// <param name="iChNo">Trigger 채널 번호</param>
        /// <param name="iPositionCount">Trigger 를 출력할 position 의 개수</param>
        /// <param name="iPositions">Trigger 를 출력할 position 값들의 배열</param>
        /// <returns></returns>
        public bool SetTriggerOnlyAbs(int iChNo, int iPositionCount, int[] iPositions)
        {
            if (bConnect == false) return false;

            int iStatus = eSnetSetTriggerOnlyAbs(iNet, iChNo, iPositionCount, out iPositions[0]); //트리거 기능 활성화

            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == (int)eSnetApiReturnCode.Disconnected ||
                    iStatus == (int)eSnetApiReturnCode.TimeOut) bConnect = false;
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("eMotionTek Rtex{0} SetTriggerOnlyAbs Error : RESULT NUM : {1}", iNet, iStatus));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 트리거 출력 기능을 해제
        /// </summary>
        /// <param name="iChNo"></param>
        /// <returns></returns>
        public bool TriggerReset(int iChNo)
        {
            if (bConnect == false) return false;

            int iStatus = eSnetResetTrigger(iNet, iChNo); // Motion 완료 후 Trigger 기능 정지 시킴

            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == (int)eSnetApiReturnCode.Disconnected ||
                    iStatus == (int)eSnetApiReturnCode.TimeOut) bConnect = false;
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("eMotionTek Rtex{0} TriggerReset Error : RESULT NUM : {1}", iNet, iStatus));
                return false;
            }

            return true;
        }

        #endregion Trigger 함수

        #region 연속 보간 함수

        /// <summary>
        /// 연속 이송 중 출력 설정 (연속 이송 설정 명령 보다 앞에 와야함 eMotionTek)
        /// </summary>
        /// <param name="iOutCount"></param>
        /// <param name="iDistanceOrTime">[0]:거리, [1]:시간</param>
        /// <param name="aiOutType"></param>
        /// <param name="aiOutPort"></param>
        /// <param name="aiOutPoint"></param>
        /// <param name="aiOnOff"></param>
        /// <param name="iDistanceOrTimeBeforeStepEnd"></param>
        /// <returns></returns>
        public bool SetContinueOutputConfig(int iOutCount, int iDistanceOrTime, int[] aiOutType, int[] aiOutPort,
                                            int[] aiOutPoint, int[] aiOnOff, int[] iDistanceOrTimeBeforeStepEnd)
        {
            if (bConnect == false) return false;

            int iStatus = eSnetSetContiOutputConfig(iNet, iOutCount, iDistanceOrTime, out aiOutType[0], out aiOutPort[0],
                                                               out aiOutPoint[0], out aiOnOff[0], out iDistanceOrTimeBeforeStepEnd[0]);

            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == (int)eSnetApiReturnCode.Disconnected ||
                    iStatus == (int)eSnetApiReturnCode.TimeOut) bConnect = false;
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("eMotionTek Rtex{0} SetContinueOutputConfig Error : RESULT NUM : {1}", iNet, iStatus));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 연속 이송 중 출력 확인
        /// </summary>
        /// <param name="iOutPutIndex"></param>
        /// <param name="iOutputCount"></param>
        /// <param name="iDistanceOrTime">[0]:거리, [1]:시간</param>
        /// <param name="aiOutType"></param>
        /// <param name="aiOutPort"></param>
        /// <param name="aiOutPoint"></param>
        /// <param name="aiOnOff"></param>
        /// <param name="iDistanceOrTimeBeforeStepEnd"></param>
        /// <returns></returns>
        public bool GetContinueOutputConfig(int iOutPutIndex, ref int iOutputCount, ref int iDistanceOrTime, ref int[] aiOutType, int[] aiOutPort,
                                        int[] aiOutPoint, int[] aiOnOff, int[] iDistanceOrTimeBeforeStepEnd)
        {
            if (bConnect == false) return false;

            int iStatus = eSnetGetContiOutputConfig(iNet, iOutPutIndex, out iOutputCount, out iDistanceOrTime,
                                                               out aiOutType[0], out aiOutPort[0], out aiOutPoint[0], out aiOnOff[0],
                                                               out iDistanceOrTimeBeforeStepEnd[0]);

            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == (int)eSnetApiReturnCode.Disconnected ||
                    iStatus == (int)eSnetApiReturnCode.TimeOut) bConnect = false;
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("eMotionTek Rtex{0} GetContinueOutputConfig Error : RESULT NUM : {1}", iNet, iStatus));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 연속 이송 명령 저장 시작
        /// </summary>
        /// <returns></returns>
        public bool SetContinueMakeJobBegin()
        {
            if (bConnect == false) return false;

            int iStatus = eSnetBeginContiMakeJob(iNet);
            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == (int)eSnetApiReturnCode.Disconnected ||
                    iStatus == (int)eSnetApiReturnCode.TimeOut) bConnect = false;
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("eMotionTek Rtex{0} eSnetSetContiMakeJobBegin Error : RESULT NUM : {1}", iNet, iStatus));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 연속 이송 명령 저장 종료
        /// </summary>
        /// <returns></returns>
        public bool SetContinueMakeJobEnd()
        {
            if (bConnect == false) return false;

            int iStatus = eSnetEndContiMakeJob(iNet);
            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == (int)eSnetApiReturnCode.Disconnected ||
                    iStatus == (int)eSnetApiReturnCode.TimeOut) bConnect = false;
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("eMotionTek Rtex{0} eSnetSetContiMakeJobEnd Error : RESULT NUM : {1}", iNet, iStatus));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 연속 이송 시작
        /// </summary>
        /// <returns></returns>
        public bool SetContinueStart(int iCh)
        {
            if (bConnect == false) return false;

            int iStatus = eSnetStartConti(iNet, iCh);
            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == (int)eSnetApiReturnCode.Disconnected ||
                    iStatus == (int)eSnetApiReturnCode.TimeOut) bConnect = false;
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("eMotionTek Rtex{0} SetContinueStart Error : RESULT NUM : {1}", iNet, iStatus));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 연속 이송 Job Index 총 개수 진행 중인 Step 취득
        /// </summary>
        /// <param name="iAll_index_count"></param>
        /// <param name="iMoving_Step_Count"></param>
        /// <returns></returns>
        public bool GetContinueJobIndexCount(ref int iAll_index_count, ref int iMoving_Step_Count)
        {
            if (bConnect == false) return false;

            int iStatus = eSnetGetContiJobIndexCount(iNet, out iAll_index_count, out iMoving_Step_Count);
            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == (int)eSnetApiReturnCode.Disconnected ||
                    iStatus == (int)eSnetApiReturnCode.TimeOut) bConnect = false;
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("eMotionTek Rtex{0} GetContinueJobIndexCount Error : RESULT NUM : {1}", iNet, iStatus));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 연속 이송 Job Index 총 개수 진행 중인 Step 취득
        /// </summary>
        /// <param name="bStop">[0] : idle or normal state, [1] : moving state</param>
        /// <returns></returns>
        public bool GetContinueMotorStop(ref bool bStop, ref int iCurrentMotionStep)
        {
            if (bConnect == false) return false;

            int ibMoving = 0;

            int iStatus = eSnetGetContiIsMotion(iNet, out ibMoving, out iCurrentMotionStep);
            if (iStatus == (int)eSnetApiReturnCode.Success)
            {
                if (ibMoving == 0) bStop = true;
                else bStop = false;
            }
            else
            {
                if (iStatus == (int)eSnetApiReturnCode.Disconnected ||
                    iStatus == (int)eSnetApiReturnCode.TimeOut) bConnect = false;
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("eMotionTek Rtex{0} GetContinueMotorStop Error : RESULT NUM : {1}", iNet, iStatus));
                return false;
            }
            return true;
        }

        #endregion 연속 보간 함수
    }

    /// <summary>
    /// SNET UNI Step 컨트롤러 연결 클래스
    /// IO는 이 클래스에서 제어하고 축 제어는 축 관리 클래스에서 제어
    /// </summary>
    public class eMotionTekUNI
    {
        /// <summary>
        /// Main Lib
        /// </summary>
        private CMainLib ml;

        /// <summary>
        /// 제어기 Net 번호 IP 끝자리
        /// </summary>
        public int iNet = 1;

        /// <summary>
        /// 생성자
        /// </summary>
        public eMotionTekUNI()
        {
            ml = CMainLib.Ins;
        }

        /// <summary>
        /// Module Init
        /// </summary>
        /// <param name="iNet"></param>
        public bool Init(int iNet)
        {
            if (Define.SIMULATION == true) return true;
            this.iNet = iNet;
            int iStatus = eUniConnectEx(192, 168, 241, iNet, 10025);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                if (CCommon.ShowMessage(0, $"이모션텍 Step 제어기 '{iNet}' 연결실패...\n프로그램을 종료합니다.") == (int)eMBoxRtn.A_OK)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                    string.Format("eMotionTek UNI{0} Connect Fail : RESULT NUM : {1}", iNet, ((eUniApiReturnCode)iStatus).ToString()));
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// disconnect
        /// </summary>
        public void Free()
        {
            if (Define.SIMULATION == true) return;
            if (bConnected() == true)
            {
                int iStatus = eUniDisconnect(iNet);
                if (iStatus != (int)eUniApiReturnCode.Success)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                        string.Format("eMotionTek UNI{0} Disconnect Fail : RESULT NUM : {0}", iNet, ((eUniApiReturnCode)iStatus).ToString()));
                }
            }
        }

        /// <summary>
        /// 통신 제대로 되는지 체크
        /// </summary>
        /// <returns></returns>
        public bool bConnected()
        {
            if (Define.SIMULATION == true) return true;
            bool bRtn = true;
            if (eUniIsConnected(iNet, out bRtn) != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.UNI_CONTROLLER_NOTCONNECT);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("UNI Net {0} Not Connection", iNet));
            }
            return bRtn;
        }

        /// <summary>
        /// 2 축 보간 이송
        /// </summary>
        /// <param name="dPosA"></param>
        /// <param name="dPosB"></param>
        /// <param name="iPercent"></param>
        /// <returns></returns>
        public bool MoveLine(double dPosA, double dPosB, int iPercent)
        {
            if (Define.SIMULATION == true) return true;
            int iPosA = (int)(dPosA / 0.001);
            int iPosB = (int)(dPosB / 0.001);
            int vel = (int)(300 * ((double)iPercent / 100) * ((double)ml.cSysOne.iAllAutoRatio / 100));
            int iStatus = eUniMoveLine(iNet, vel, 100, 100, 66, 66, iPosA, iPosB);

            if (iStatus != (int)eUniApiReturnCode.Success) return false;
            else return true;
        }

        /// <summary>
        /// Input을 이용한 정지 이송
        /// </summary>
        /// <param name="iAxis"></param>
        /// <param name="iPos"> um </param>
        /// <param name="iVel"> mm/min </param>
        /// <param name="iPoint"> 0 : Input0(-Limit), 1 : Input1(Home), 2 : Input2(+Limit), 3 : Input3(User) </param>
        /// <param name="iEdge"> 0 : Falling, 1 : Rising </param>
        /// <returns></returns>
        public bool MoveSCurveIo(int iAxis, int iPos, int iVel, int iPoint, int iEdge)
        {
            if (Define.SIMULATION == true) return true;
            int iStatus = eUniMoveSCurveIo(iNet, iAxis, iVel, 100, 100, 66, 66, iPos, iPoint, iEdge, 0, 1);

            if (iStatus != (int)eUniApiReturnCode.Success) return false;
            else return true;
        }

        /// <summary>
        /// UNI의 출력을 제어
        /// </summary>
        /// <param name="iAxisNo"></param>
        /// <param name="iOutputNo"></param>
        /// <param name="iOnOff"></param>
        /// <returns></returns>
        public bool SetOutput(int iAxisNo, int iOutputNo, int iOnOff)
        {
            if (Define.SIMULATION == true) return true;
            int iStatus = eUniSetOutput(iNet, iAxisNo, iOutputNo, iOnOff);

            if (iStatus != (int)eUniApiReturnCode.Success) return false;
            else return true;
        }

        /// <summary>
        /// UNI의 동기축 설정
        /// </summary>
        /// <param name="iAxisNo">설정할 축 번호</param>
        /// <param name="ienable">동기 축 기능 활성화 유무. 1: ON, 0: OFF</param>
        /// <param name="isyncAxisNo"> 해당 축과 동기 할 축 번호</param>
        /// <returns></returns>
        public bool SetSyncAxis(int iAxisNo, int ienable, int isyncAxisNo)
        {
            if (Define.SIMULATION == true) return true;
            int iStatus = eUniSetSyncAxis(iNet, iAxisNo, ienable, isyncAxisNo);

            if (iStatus != (int)eUniApiReturnCode.Success) return false;
            else return true;
        }
    }
}