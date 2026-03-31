using static EMotionSnetBase.SnetDevice;
using static EMotionUniBase.UniDevice;
using System.Linq;
using System.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// 축 클래스
    /// </summary>
    public class eMotionTekUniAxis : MotionProcMgr
    {
        /// <summary>
        /// Main Lib
        /// </summary>
        private CMainLib ml;

        /// <summary>
        /// Axis Param
        /// </summary>
        public AxisParam axisParam = null;

        /// <summary>
        /// Axis Position
        /// </summary>
        public AxisPositionData _axisPositionData = null;

        public AxisPositionData axisPositionData
        {
            get { return _axisPositionData; }
            set { _axisPositionData = value; }
        }

        /// <summary>
        /// Uni 통신 프로토콜
        /// </summary>
        private eMotionTekUNI uni;

        /// <summary>
        /// 축 번호 및 이름
        /// </summary>
        public eMotor eAxis
        {
            get { return axisParam.eAxis; }
            set { axisParam.eAxis = value; }
        }

        /// <summary>
        /// 컨트롤러 상의 축 번호
        /// </summary>
        public uint uiControllerAxis
        {
            get { return axisParam.uiControllerAxis; }
            set { axisParam.uiControllerAxis = value; }
        }

        /// <summary>
        /// 축 이송 속도
        /// </summary>
        public uint uiVelocity
        {
            get { return axisParam.uiVel; }
            set { axisParam.uiVel = value; }
        }

        /// <summary>
        /// 축 조그 이송 속도
        /// </summary>
        public uint uiJogVel
        {
            get { return axisParam.uiJogVel; }
            set { axisParam.uiJogVel = value; }
        }

        /// <summary>
        /// 축 가속도
        /// </summary>
        public uint uiAccel
        {
            get { return axisParam.uiAccel; }
            set { axisParam.uiAccel = value; }
        }

        /// <summary>
        /// 축 감속도
        /// </summary>
        public uint uiDecel
        {
            get { return axisParam.uiDecel; }
            set { axisParam.uiDecel = value; }
        }

        /// <summary>
        /// 축 Scale
        /// </summary>
        public double dScale
        {
            get { return axisParam.dScale; }
            set { axisParam.dScale = value; }
        }

        /// <summary>
        /// 홈 속도
        /// </summary>
        public uint uiHomeVel
        {
            get { return axisParam.uiHomeVel; }
            set { axisParam.uiHomeVel = value; }
        }

        /// <summary>
        /// 홈 가속도
        /// </summary>
        public uint uiHomeAcc
        {
            get { return axisParam.uiHomeAcc; }
            set { axisParam.uiHomeAcc = value; }
        }

        /// <summary>
        /// 홈 감속도
        /// </summary>
        public uint uiHomeDec
        {
            get { return axisParam.uiHomeDec; }
            set { axisParam.uiHomeDec = value; }
        }

        /// <summary>
        /// 홈 Offset(홈 완료 후 이송 거리)
        /// </summary>
        public double dHomeOffset
        {
            get { return axisParam.dHomeOffset; }
            set { axisParam.dHomeOffset = value; }
        }

        /// <summary>
        /// 홈 완료 후 Clear 시간
        /// </summary>
        public uint uiHomeClearTime
        {
            get { return axisParam.uiHomeClearTime; }
            set { axisParam.uiHomeClearTime = value; }
        }

        /// <summary>
        /// 홈 동작 Z상 사용 유무
        /// </summary>
        public bool bHomeUseZPhase
        {
            get { return axisParam.bHomeUseZPhase; }
            set { axisParam.bHomeUseZPhase = value; }
        }

        /// <summary>
        /// 홈 센서 타입(Home, +-Limit)
        /// </summary>
        public int iHomeSenserType
        {
            get { return axisParam.iHomeSenserType; }
            set { axisParam.iHomeSenserType = value; }
        }

        /// <summary>
        /// 홈 동작 방향
        /// </summary>
        public int iHomeMoveDirection
        {
            get { return axisParam.iHomeMoveDirection; }
            set { axisParam.iHomeMoveDirection = value; }
        }

        /// <summary>
        /// 홈 동작 타임 아웃 시간
        /// </summary>
        public uint uiHomeTimeout
        {
            get { return axisParam.uiHomeTimeout; }
            set { axisParam.uiHomeTimeout = value; }
        }

        /// <summary>
        /// 모션 완료 타임 아웃 시간
        /// </summary>
        public uint uiMotionTimeout
        {
            get { return axisParam.uiMotionTimeout; }
            set { axisParam.uiMotionTimeout = value; }
        }

        /// <summary>
        /// 시뮬레이션 Move Done
        /// </summary>
        public bool bSimMoveDone
        {
            get { return axisParam.bSimMoveDone; }
            set { axisParam.bSimMoveDone = value; }
        }

        /// <summary>
        /// 시뮬레이션 Command Position
        /// </summary>
        public double dSimCmdPos
        {
            get { return axisParam.dSimCmdPos; }
            set { axisParam.dSimCmdPos = value; }
        }

        /// <summary>
        /// 홈 동작 중 정지 명령
        /// </summary>
        private bool breakStop = false;

        public bool BreakStop
        {
            get { return breakStop; }
            set { breakStop = value; }
        }

        /// <summary>
        /// 홈 동작 완료 상태
        /// </summary>
        private bool homeComplete = false;

        public bool HomeComplete
        {
            get { return homeComplete; }
            set { homeComplete = value; }
        }

        /// <summary>
        /// Index 이송 시 Index 번호
        /// </summary>
        private int posIndex = -1;

        public int PosIndex
        {
            get { return posIndex; }
            set { posIndex = value; }
        }

        /// <summary>
        /// Simulation 관련 클래스 생성 (시뮬레이션)
        /// </summary>
        public SimMotionControl simMotionControl = null;

        /// <summary>
        /// uni 모듈과 축 주소 받아서 저장
        /// </summary>
        /// <param name="uni"></param>
        /// <param name="axisParam"></param>
        /// <param name="axisPositionData"></param>
        public eMotionTekUniAxis(eMotionTekUNI uni, AxisParam axisParam, AxisPositionData axisPositionData)
        {
            ml = CMainLib.Ins;
            this.axisParam = axisParam;
            this.axisPositionData = axisPositionData;
            this.uni = uni;
            simMotionControl = new SimMotionControl(axisParam);
        }

        public void Init()
        {
        }

        public void Free()
        {
        }

        /// <summary>
        /// UNI에 추가된 자동 Homming 기능 사용 유무
        /// </summary>
        private bool bAutoHomming = true;

        /// <summary>
        /// Homing
        /// </summary>
        public bool Home()
        {
            if (Define.SIMULATION == true)
            {
                return simMotionControl.SimHome();
            }
            else
            {
                if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                if (uni.bConnected() == false) return false;

                int iStatus;
                if (uni.iNet == 6 || (uni.iNet == 5 && (int)uiControllerAxis == 0)) bAutoHomming = false;

                if (bAutoHomming == true)
                {
                    int methodNo = 19;  // 원점 이송 방향
                    if (iHomeMoveDirection == (int)eSnetMoveDirection.Negative) methodNo = 21;

                    int nZeroOffset = (int)(dHomeOffset * dScale);
                    eUniStartHomingMethod(uni.iNet, (int)uiControllerAxis, methodNo, (int)uiHomeVel * 60, (int)uiHomeAcc, (int)uiHomeDec,
                                          (int)uiHomeVel * 60, (int)uiHomeAcc, (int)uiHomeDec, nZeroOffset, 1);
                }
                else
                {
                    iStatus = eUniRemoveAllHomingStep(uni.iNet, (int)uiControllerAxis);
                    if (iStatus != (int)eUniApiReturnCode.Success)
                    {
                        ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                        NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                         string.Format("Axis {0} Remove All Hoiming Step Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
                        return false;
                    }

                    int iPoint = (int)eUniAxisInputNumber.Input0; // 0은 LimitMinus 원점으로 어떤 센서를 사용할지 결정 (SNET으로 디파인돼있어 재정의)
                    if (iHomeSenserType == (int)eSnetAxisSensor.HomeSensor) iPoint = (int)eUniAxisInputNumber.Input1;
                    else if (iHomeSenserType == (int)eSnetAxisSensor.LimitPlus) iPoint = (int)eUniAxisInputNumber.Input2;

                    int iDirection = 0; // 원점 이송 방향
                    if (iHomeMoveDirection == (int)eSnetMoveDirection.Positive) iDirection = 1;

                    if (iHomeSenserType == (int)eSnetAxisSensor.ZPhase)
                    {
                        // DT축은 Z상으로 원점을 잡아야 하므로 따로 정의함.
                        iStatus = eUniAddHomingStep(uni.iNet, (int)uiControllerAxis, 0, (int)uiHomeVel * 60, (int)uiHomeAcc, (int)uiHomeDec, 66, 66, iDirection, 4, 1, 0, 50);
                        if (iStatus != (int)eUniApiReturnCode.Success)
                        {
                            ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                            NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                             string.Format("Axis {0} Add Homing Step Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
                            return false;
                        }
                    }
                    else
                    {
                        iStatus = eUniAddHomingStep(uni.iNet, (int)uiControllerAxis, 0, (int)uiHomeVel * 60, (int)uiHomeAcc, (int)uiHomeDec, 66, 66, iDirection, iPoint, 1, 1, 50);
                        if (iStatus != (int)eUniApiReturnCode.Success)
                        {
                            ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                            NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                             string.Format("Axis {0} Add Homing Step Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
                            return false;
                        }

                        iDirection = iDirection == 0 ? 1 : 0;  // 원점 방향 반대 설정 (SNET으로 디파인돼있어 재정의)

                        iStatus = eUniAddHomingStep(uni.iNet, (int)uiControllerAxis, 1, (int)uiHomeVel * 60, (int)uiHomeAcc, (int)uiHomeDec, 66, 66, iDirection, iPoint, 0, 0, 50);
                        if (iStatus != (int)eUniApiReturnCode.Success)
                        {
                            ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                            NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                             string.Format("Axis {0} Add Homing Step Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
                            return false;
                        }

                        if (bHomeUseZPhase == true)
                        {
                            iStatus = eUniAddHomingStep(uni.iNet, (int)uiControllerAxis, 2, (int)uiHomeVel * 180, (int)uiHomeAcc, (int)uiHomeDec, 66, 66, iDirection, 4, 1, 0, 50);
                            if (iStatus != (int)eUniApiReturnCode.Success)
                            {
                                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                                 string.Format("Axis {0} Add Homing Step Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
                                return false;
                            }
                        }
                    }

                    // [Offset 설정]
                    if (dHomeOffset != 0)
                    {
                        int nZeroOffset = (int)(dHomeOffset * dScale);
                        eUniSetHomingShift(uni.iNet, (int)uiControllerAxis, 1, (int)uiHomeVel * 60, (int)uiHomeAcc, (int)uiHomeDec, 66, 66, nZeroOffset);
                    }

                    iStatus = eUniStartHoming(uni.iNet, (int)uiControllerAxis, 0);
                    if (iStatus != (int)eUniApiReturnCode.Success)
                    {
                        ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                        NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                         string.Format("Axis {0} Homing Start Step Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Homing 완료 확인
        /// </summary>
        /// <returns></returns>
        public bool HomeDone()
        {
            if (Define.SIMULATION == true) return true;
            int iStep = 0;
            int iRate = 0;
            int statecode = 0;
            int iHoming = 0;
            int iOffset = 0;

            int iStatus = eUniIsHoming(uni.iNet, (int)uiControllerAxis, out iHoming, out iStep, out iOffset);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} Homing IsHoming Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
                return false;
            }

            iStatus = eUniGetHomingResult(uni.iNet, (int)uiControllerAxis, out iStep, out iRate, out statecode);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} Get is homing Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
                return false;
            }

            if (iRate == 100)
            {
                HomeComplete = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Homing 완료까지 대기
        /// </summary>
        /// <returns></returns>
        public bool WaitHome(ref string strFailMsg)
        {
            if (Define.SIMULATION == true) return true;
            int iStep = 0;
            int iRate = 0;
            int statecode = 0;
            int iHoming = 0;
            int iOffset = 0;
            bool bHommingDone = false;
            int nCts = 0;
            BreakStop = false;

            while (bHommingDone == false)
            {
                Thread.Sleep(10);
                int iStatus = eUniIsHoming(uni.iNet, (int)uiControllerAxis, out iHoming, out iStep, out iOffset);
                if (iStatus != (int)eUniApiReturnCode.Success)
                {
                    ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                     string.Format("Axis {0} Homing IsHoming Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
                    return false;
                }

                iStatus = eUniGetHomingResult(uni.iNet, (int)uiControllerAxis, out iStep, out iRate, out statecode);
                if (iStatus != (int)eUniApiReturnCode.Success)
                {
                    ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                     string.Format("Axis {0} Get is homing Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
                    return false;
                }

                if (iRate == 100)
                {
                    HomeComplete = true;
                    bHommingDone = true;
                }

                if (BreakStop == true)
                {
                    strFailMsg = string.Format("eUniGetHomingResult Fail : Axis {0} Motor Stop Command", eAxis);
                    return false;
                }
                if (nCts >= (uiHomeTimeout * 100))   // ms로 변환 while문 한번도는데 대략 10ms
                {
                    strFailMsg = string.Format("eUniGetHomingResult Fail : Axis {0} Time Out Result {1}", eAxis, statecode);
                    Thread.Sleep(500);
                    Reset();
                    return false;
                }
                nCts++;
            }
            return true;
        }

        /// <summary>
        /// 절대이동
        /// </summary>
        /// <param name="dPos"></param>
        /// <param name="bSafe"></param>
        public bool MoveAbsolute(double dPos, bool bSafe = true)
        {
            if (Define.SIMULATION == true)
            {
                return simMotionControl.SimMotorMove(SimMoveType.ABS_Move, dPos, 100);
            }
            else
            {
                if (uni.bConnected() == false) return false;
                PosIndex = -1;
                if (bSafe == true)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                }
                uint vel = (uint)((uiVelocity * 60) * ((double)ml.cSysOne.iAllAutoRatio / 100));
                return MoveAbsolute(dPos, uiAccel, uiDecel, vel);
            }
        }

        /// <summary>
        /// 절대이동
        /// </summary>
        /// <param name="iPosIndex"></param>
        /// <param name="bSafe"></param>
        public bool MoveAbsolute(int iPosIndex, bool bSafe = true)
        {
            PositionData positionData = axisPositionData.GetPositionData(iPosIndex);
            if (Define.SIMULATION == true)
            {
                return simMotionControl.SimMotorMove(SimMoveType.ABS_Move, positionData.dPos, positionData.uiSpeed);
            }
            else
            {
                if (uni.bConnected() == false) return false;
                PosIndex = iPosIndex;
                if (bSafe == true)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                }
                uint vel = (uint)((uiVelocity * 60) * ((double)ml.cSysOne.iAllAutoRatio / 100) * ((double)positionData.uiSpeed / 100));
                return MoveAbsolute(positionData.dPos, uiAccel, uiDecel, vel);
            }
        }

        /// <summary>
        /// 절대이동
        /// </summary>
        /// <param name="iPosIndex"></param>
        /// <param name="uiPercent"></param>
        /// <param name="bSafe"></param>
        public bool MoveAbsolute(int iPosIndex, uint uiPercent, bool bSafe = true)
        {
            PositionData positionData = axisPositionData.GetPositionData(iPosIndex);
            if (Define.SIMULATION == true)
            {
                return simMotionControl.SimMotorMove(SimMoveType.ABS_Move, positionData.dPos, positionData.uiSpeed);
            }
            else
            {
                if (uni.bConnected() == false) return false;
                PosIndex = iPosIndex;
                if (bSafe == true)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                }
                uint vel = (uint)((uiVelocity * 60) * ((double)uiPercent / 100) * ((double)ml.cSysOne.iAllAutoRatio / 100));
                return MoveAbsolute(positionData.dPos, uiAccel, uiDecel, vel);
            }
        }

        /// <summary>
        /// 절대이동
        /// </summary>
        /// <param name="dPos"></param>
        /// <param name="uiVel"></param>
        /// <param name="uiAcc"></param>
        /// <param name="uiDec"></param>
        /// <param name="bSafe"></param>
        /// <returns></returns>
        public bool MoveAbsolute(double dPos, uint uiVel, uint uiAcc, uint uiDec, bool bSafe = true)
        {
            if (Define.SIMULATION == true)
            {
                PosIndex = -1;
                return simMotionControl.SimMotorMove(SimMoveType.ABS_Move, dPos, 100);
            }
            else
            {
                if (uni.bConnected() == false) return false;
                PosIndex = -1;
                if (bSafe == true)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                }
                uint vel = (uint)((uiVel * 60) * ((double)ml.cSysOne.iAllAutoRatio / 100));
                return MoveAbsolute(dPos, uiAcc, uiDec, vel);
            }
        }

        /// <summary>
        /// 절대이동
        /// </summary>
        /// <param name="dPos"></param>
        /// <param name="uiPercent"></param>
        /// <param name="bSafe"></param>
        public bool MoveAbsolute(double dPos, uint uiPercent, bool bSafe = true)
        {
            if (Define.SIMULATION == true)
            {
                PosIndex = -1;
                return simMotionControl.SimMotorMove(SimMoveType.ABS_Move, dPos, uiPercent);
            }
            else
            {
                if (uni.bConnected() == false) return false;
                PosIndex = -1;
                if (bSafe == true)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                }
                uint vel = (uint)((uiVelocity * 60) * ((double)uiPercent / 100) * ((double)ml.cSysOne.iAllAutoRatio / 100));
                return MoveAbsolute(dPos, uiAccel, uiDecel, vel);
            }
        }

        /// <summary>
        /// 상대이동
        /// </summary>
        /// <param name="dPos"></param>
        /// <param name="bSafe"></param>
        public bool MoveRelative(double dPos, bool bSafe = true)
        {
            if (Define.SIMULATION == true)
            {
                PosIndex = -1;
                return simMotionControl.SimMotorMove(SimMoveType.REL_Move, dPos, 100);
            }
            else
            {
                if (uni.bConnected() == false) return false;
                PosIndex = -1;
                if (bSafe == true)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                }
                uint vel = (uint)((uiVelocity * 60) * ((double)ml.cSysOne.iAllAutoRatio / 100));
                return MoveRelative(dPos, uiAccel, uiDecel, vel);
            }
        }

        /// <summary>
        /// 상대이동
        /// </summary>
        /// <param name="iPosIndex"></param>
        /// <param name="bSafe"></param>
        public bool MoveRelative(int iPosIndex, bool bSafe = true)
        {
            PositionData positionData = axisPositionData.GetPositionData(iPosIndex);
            if (Define.SIMULATION == true)
            {
                PosIndex = iPosIndex;
                return simMotionControl.SimMotorMove(SimMoveType.REL_Move, positionData.dPos, positionData.uiSpeed);
            }
            else
            {
                if (uni.bConnected() == false) return false;
                PosIndex = iPosIndex;
                if (bSafe == true)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                }
                uint vel = (uint)((uiVelocity * 60) * ((double)ml.cSysOne.iAllAutoRatio / 100) * ((double)positionData.uiSpeed / 100));
                return MoveRelative(positionData.dPos, uiAccel, uiDecel, vel);
            }
        }

        /// <summary>
        /// 상대이동
        /// </summary>
        /// <param name="iPosIndex"></param>
        /// <param name="uiPercent"></param>
        /// <param name="bSafe"></param>
        public bool MoveRelative(int iPosIndex, uint uiPercent, bool bSafe = true)
        {
            PositionData positionData = axisPositionData.GetPositionData(iPosIndex);
            if (Define.SIMULATION == true)
            {
                PosIndex = iPosIndex;
                return simMotionControl.SimMotorMove(SimMoveType.REL_Move, positionData.dPos, positionData.uiSpeed);
            }
            else
            {
                if (uni.bConnected() == false) return false;
                PosIndex = iPosIndex;
                if (bSafe == true)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                }
                uint vel = (uint)((uiVelocity * 60) * ((double)uiPercent / 100) * ((double)ml.cSysOne.iAllAutoRatio / 100));
                return MoveRelative(positionData.dPos, uiAccel, uiDecel, vel);
            }
        }

        /// <summary>
        /// 상대이동
        /// </summary>
        /// <param name="iPosIndex"></param>
        /// <param name="bSafe"></param>
        public bool MoveRelativeNoVelConv(int iPosIndex, bool bSafe = true)
        {
            PositionData positionData = axisPositionData.GetPositionData(iPosIndex);
            if (Define.SIMULATION == true)
            {
                PosIndex = iPosIndex;
                return simMotionControl.SimMotorMove(SimMoveType.REL_Move, positionData.dPos, positionData.uiSpeed);
            }
            else
            {
                if (uni.bConnected() == false) return false;
                PosIndex = iPosIndex;
                if (bSafe == true)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                }
                return MoveRelative(positionData.dPos, uiAccel, uiDecel, uiVelocity);
            }
        }

        /// <summary>
        /// 상대이동
        /// </summary>
        /// <param name="dPos"></param>
        /// <param name="uiVel"></param>
        /// <param name="uiAcc"></param>
        /// <param name="uiDec"></param>
        /// <param name="bSafe"></param>
        /// <returns></returns>
        public bool MoveRelative(double dPos, uint uiVel, uint uiAcc, uint uiDec, bool bSafe = true)
        {
            if (Define.SIMULATION == true)
            {
                PosIndex = -1;
                return simMotionControl.SimMotorMove(SimMoveType.REL_Move, dPos, 100);
            }
            else
            {
                if (uni.bConnected() == false) return false;
                PosIndex = -1;
                if (bSafe == true)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                }
                uint vel = (uint)((uiVel * 60) * ((double)ml.cSysOne.iAllAutoRatio / 100));
                return MoveRelative(dPos, uiAcc, uiDec, vel);
            }
        }

        /// <summary>
        /// 상대이동
        /// </summary>
        /// <param name="dPos"></param>
        /// <param name="uiPercent"></param>
        /// <param name="bSafe"></param>
        public bool MoveRelative(double dPos, uint uiPercent, bool bSafe = true)
        {
            if (Define.SIMULATION == true)
            {
                PosIndex = -1;
                return simMotionControl.SimMotorMove(SimMoveType.REL_Move, dPos, uiPercent);
            }
            else
            {
                if (uni.bConnected() == false) return false;
                PosIndex = -1;
                if (bSafe == true)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                }
                uint vel = (uint)((uiVelocity * 60) * ((double)uiPercent / 100) * ((double)ml.cSysOne.iAllAutoRatio / 100));
                return MoveRelative(dPos, uiAccel, uiDecel, vel);
            }
        }

        /// <summary>
        /// 조그이동
        /// </summary>
        /// <param name="bCW"></param>
        /// <param name="bSafe"></param>
        public bool MoveVelocity(bool bCW, bool bSafe = true)
        {
            if (Define.SIMULATION == true) return true;
            if (uni.bConnected() == false) return false;
            PosIndex = -1;
            if (bSafe == true)
            {
                if (CInterLock.GetAxisSafety(eAxis) == false) return false;
            }
            return MoveVelocity(bCW, uiAccel, uiDecel, uiJogVel * 60);
        }

        /// <summary>
        /// 조그이동
        /// </summary>
        /// <param name="bCW"></param>
        /// <param name="uiVel"></param>
        /// <param name="bSafe"></param>
        /// <returns></returns>
        public bool MoveVelocity(bool bCW, uint uiVel, bool bSafe = true)
        {
            if (Define.SIMULATION == true) return true;
            if (uni.bConnected() == false) return false;
            PosIndex = -1;
            if (bSafe == true)
            {
                if (CInterLock.GetAxisSafety(eAxis) == false) return false;
            }
            return MoveVelocity(bCW, uiAccel, uiDecel, uiVel * 60);
        }

        /// <summary>
        /// 절대이동
        /// </summary>
        /// <param name="dPos"></param>
        /// <param name="uiAcc"></param>
        /// <param name="uiDec"></param>
        /// <param name="uiVel"></param>
        /// <returns></returns>
        public bool MoveAbsolute(double dPos, uint uiAcc, uint uiDec, uint uiVel)
        {
            int iPos = (int)(dPos * dScale);
            int iStatus = eUniMoveSCurve(uni.iNet, (int)uiControllerAxis, (int)uiVel, (int)uiAcc, (int)uiDec, 66, 66, iPos);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} Move Absolute Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// 상대이동
        /// </summary>
        /// <param name="dPos"></param>
        /// <param name="uiAcc"></param>
        /// <param name="uiDec"></param>
        /// <param name="uiVel"></param>
        /// <returns></returns>
        public bool MoveRelative(double dPos, uint uiAcc, uint uiDec, uint uiVel)
        {
            int iPos = (int)(dPos * dScale);
            int iStatus = eUniMoveSCurve(uni.iNet, (int)uiControllerAxis, (int)uiVel, (int)uiAcc, (int)uiDec, 66, 66, iPos, (int)eUniMotionMode.Incremental);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} Move Relative Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// 조그이동
        /// </summary>
        /// <param name="bCW"></param>
        /// <param name="uiAcc"></param>
        /// <param name="uiDec"></param>
        /// <param name="uiVel"></param>
        /// <returns></returns>
        public bool MoveVelocity(bool bCW, uint uiAcc, uint uiDec, uint uiVel)
        {
            int iCw = bCW == true ? 1 : 0;
            int iStatus = eUniMoveVelocity(uni.iNet, (int)uiControllerAxis, (int)uiVel, (int)uiAcc, (int)uiDec, 66, 66, iCw);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} Move JOG Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// 이동 완료 대기
        /// </summary>
        public void WaitDone()
        {
            if (Define.SIMULATION == true) return;
            if (uni.bConnected() == false) return;

            BreakStop = false;
            int nCts = 0;
            while (IsMoveDone() == false)
            {
                Thread.Sleep(10);
                if (BreakStop) break;

                if (nCts >= (uiMotionTimeout * 100))//ms로 변환 while문 한번도는데 대략 10ms
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("Motion WaitDone Time Out : Axis {0}", eAxis));
                    ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                    return;
                }
                nCts++;
            }
        }

        /// <summary>
        /// 정지
        /// </summary>
        public bool Stop()
        {
            if (Define.SIMULATION == true) return true;
            if (uni.bConnected() == false) return false;

            int iTime = 300; // Slow Time
            int iStatus = eUniSlowStop(uni.iNet, (int)uiControllerAxis, iTime);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} Stop Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
                return false;
            }
            BreakStop = true;
            return true;
        }

        /// <summary>
        /// Emg stop
        /// </summary>
        public void EmergencyStop()
        {
            if (Define.SIMULATION == true) return;
            if (uni.bConnected() == false) return;
            CMainLib.Ins.cVar.bInitializeMotor = false;

            int iStatus = eUniEmergencyStop(uni.iNet, (int)uiControllerAxis);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} Emergency Stop Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
            }
        }

        /// <summary>
        /// Set zero
        /// </summary>
        public void SetZero()
        {
            if (Define.SIMULATION == true)
            {
                axisParam.dSimCmdPos = 0;
                return;
            }

            if (uni.bConnected() == false) return;

            int iStatus = eUniSetHomePosition(uni.iNet, (int)uiControllerAxis, 0);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} Set Home Position Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
            }
        }

        /// <summary>
        /// Reset alarm clear
        /// </summary>
        public void Reset()
        {
            if (Define.SIMULATION == true) return;
            if (uni.bConnected() == false) return;

            int iStatus = eUniReset(uni.iNet, (int)uiControllerAxis);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} Reset Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
            }
        }

        #region 토크 제한

        /// <summary>
        ///
        /// </summary>
        /// <param name="iTrq">단위 [%]</param>
        /// <returns></returns>
        public bool SetTrqLimit(int iTrq)
        {
            if (Define.SIMULATION == true) return true;
            if (uni.bConnected() == false) return false;

            return true;
        }

        /// <summary>
        /// 설정된 토크 값을 읽어옴
        /// </summary>
        /// <param name="iTrq"></param>
        /// <returns></returns>
        public bool GetTrqLimit(ref int iTrq)
        {
            if (Define.SIMULATION == true) return true;
            if (uni.bConnected() == false) return false;

            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="dPos">종점 위치[um]</param>
        /// <param name="uiVel">축 설정 속도의[%]</param>
        /// <param name="uiTime">"토크 제한 신호" 유지 시간 [msec]</param>
        /// <returns></returns>
        public bool MoveAxisUntilTrqLimit(double dPos, uint uiVel, uint uiTime, bool bSafe = true)
        {
            if (Define.SIMULATION == true)
            {
                PosIndex = -1;
                return simMotionControl.SimMotorMove(SimMoveType.REL_Move, dPos, 100);
            }
            else
            {
                if (uni.bConnected() == false) return false;
                PosIndex = -1;
                if (bSafe == true)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                };

                return true;
            }
        }

        /// <summary>
        ///  첫번째 토크 제한 설정값을 적용 하고 첫번째 설정 위치로 이송 후 설정 시간 동안 대기 하고
        ///  두번째 토크 제한 설정값을 적용 하고 두번째 설정 위치로 이송 합니다.
        /// </summary>
        /// <param name="iSettrq_1">    : 첫번째 토오크 제한 설정값 				</param>
        /// <param name="iSettrq_2">    : 두번째 토오크 제한 설정값 				</param>
        /// <param name="uiTime"> 	    : 첫번째 이송 완료 후 대기 시간[msec]     </param>
        /// <param name="dPos1"> 	    : 첫번째 이송 위치 [um]					</param>
        /// <param name="dPos2"> 	    : 두번째 이송 위치 [um]  					</param>
        /// <param name="uiVel1"> 	    : 첫번째 이송 속도 [mm/min]				</param>
        /// <param name="uiVel2"> 	    : 두번째 이송 속도 [mm/min]				</param>
        /// <param name="bSafe"></param>
        /// <returns></returns>
        public bool SetTrqLimitAndMove(double dPos1, double dPos2, int iSettrq_1, int iSettrq_2, uint uiTime, uint uiVel1, uint uiVel2, bool bSafe = true)
        {
            if (Define.SIMULATION == true)
            {
                PosIndex = -1;
                simMotionControl.SimMotorMove(SimMoveType.ABS_Move, dPos1, 100);
                return simMotionControl.SimMotorMove(SimMoveType.ABS_Move, dPos2, 100);
            }
            else
            {
                if (uni.bConnected() == false) return false;
                PosIndex = -1;
                if (bSafe == true)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                };

                return true;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool GetStatusTrqLimitAndMove()
        {
            if (Define.SIMULATION == true) return true;
            if (uni.bConnected() == false) return false;

            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool ResetTrqLimitAndMove()
        {
            if (Define.SIMULATION == true) return true;
            if (uni.bConnected() == false) return false;

            return true;
        }

        #endregion 토크 제한

        /// <summary>
        /// Set servo state
        /// </summary>
        /// <param name="bOn"></param>
        public void SetServoState(bool bOn)
        {
            if (Define.SIMULATION == true) return;
            if (uni.bConnected() == false) return;

            int iOnOff = bOn == false ? 0 : 1;
            int iStatus = eUniSetServoOn(uni.iNet, (int)uiControllerAxis, iOnOff);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} Set Servo State Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
            }
            HomeComplete = false;
        }

        /// <summary>
        /// Get servo state
        /// </summary>
        /// <returns></returns>
        public bool GetServoState()
        {
            if (Define.SIMULATION == true) return true;
            if (uni.bConnected() == false) return false;

            int iOnOff;
            int iStatus = eUniGetServoOn(uni.iNet, (int)uiControllerAxis, out iOnOff);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} Get Servo State Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
            }
            return iOnOff == 1 ? true : false;
        }

        /// <summary>
        /// Motion moving finish check
        /// </summary>
        /// <returns></returns>
        public bool IsMoveDone()
        {
            if (Define.SIMULATION == true)
            {
                return simMotionControl.SimMoveDone();
            }
            else
            {
                if (uni.bConnected() == false) return false;

                int[] iGetdata = Enumerable.Repeat(0, 13).ToArray();
                int iAlarm;
                int iStatus = eUniGetMotionStatus(uni.iNet, (int)uiControllerAxis, out iGetdata[0]);
                int iStatus2 = eUniGetMotionAlarm(uni.iNet, (int)uiControllerAxis, out iAlarm);
                if (iStatus != (int)eUniApiReturnCode.Success && iStatus2 != (int)eUniApiReturnCode.Success)
                {
                    ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                     string.Format("Axis {0} Move Done Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
                }
                if (iGetdata[0] == 0)
                {
                    if ((iAlarm & 0x20) == 0x20)
                    {
                        NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("Inposition check time over : Result {0}", iAlarm));
                        ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 제어기 에러 확인
        /// </summary>
        /// <returns></returns>
        public bool GetControllerFault()
        {
            if (Define.SIMULATION == true) return false;
            if (uni.bConnected() == false) return false;

            int iGetData = 0;

            int iStatus = eUniGetErrorCode(uni.iNet, (int)uiControllerAxis, out iGetData);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} GetControllerFault, Error Code {1}", eAxis, (eUniDeviceErrorCode)iGetData).ToString());
            }
            return iGetData == 0 ? false : true;
        }

        /// <summary>
        /// 서보 에러 확인(서보가 아니라서 드라이버 에러 상태를 가지고 옴)
        /// </summary>
        /// <returns></returns>
        public bool GetServoFault()
        {
            if (Define.SIMULATION == true) return false;
            if (uni.bConnected() == false) return false;

            int iGetData = 0;

            int iStatus = eUniGetErrorCode(uni.iNet, (int)uiControllerAxis, out iGetData);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} GetServoFault, Error Code {1}", eAxis, (eUniDeviceErrorCode)iGetData).ToString());
            }
            return iGetData == 0 ? false : true;
        }

        /// <summary>
        /// Check minus limit check
        /// </summary>
        /// <returns></returns>
        public bool GetHardwareMinusLimit()
        {
            if (Define.SIMULATION == true) return false;
            if (uni.bConnected() == false) return true;

            int iLimitN, iLimitP, iHomeSensor, iServoAlarm, iServoReady, iServoOn;

            int iStatus = eUniGetSignalStatus(uni.iNet, (int)uiControllerAxis, out iLimitN, out iLimitP, out iHomeSensor,
                                                        out iServoAlarm, out iServoReady, out iServoOn);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} Get Hardware Minus Limit Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
            }
            return iLimitN == 0 ? false : true;
        }

        /// <summary>
        /// Check Hardware plus limit check
        /// </summary>
        /// <returns></returns>
        public bool GetHardwarePlusLimit()
        {
            if (Define.SIMULATION == true) return false;
            if (uni.bConnected() == false) return false;

            int iLimitN, iLimitP, iHomeSensor, iServoAlarm, iServoReady, iServoOn;

            int iStatus = eUniGetSignalStatus(uni.iNet, (int)uiControllerAxis, out iLimitN, out iLimitP, out iHomeSensor,
                                                        out iServoAlarm, out iServoReady, out iServoOn);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} Get Hardware Plus Limit Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
            }
            return iLimitP == 0 ? false : true;
        }

        /// <summary>
        /// Check Hardware minus limit check
        /// </summary>
        /// <returns></returns>
        public bool GetMinusLimit()
        {
            if (Define.SIMULATION == true) return false;
            if (uni.bConnected() == false) return true;

            int iGetData = 0;

            int iStatus = eUniGetMotionAlarm(uni.iNet, (int)uiControllerAxis, out iGetData);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} Get Minus Limit Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
            }
            // 0x2 Hardware -Limit, 0x8 Software -Limit
            return (iGetData & 0x2) == 0x2 ||
                   (iGetData & 0x8) == 0x8 ? true : false;
        }

        /// <summary>
        /// Check plus limit check
        /// </summary>
        /// <returns></returns>
        public bool GetPlusLimit()
        {
            if (Define.SIMULATION == true) return false;
            if (uni.bConnected() == false) return false;

            int iGetData = 0;

            int iStatus = eUniGetMotionAlarm(uni.iNet, (int)uiControllerAxis, out iGetData);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} Get Plus Limit Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
            }
            // 0x1 Hardware +Limit, 0x4 Software +Limit
            return (iGetData & 0x1) == 0x1 ||
                   (iGetData & 0x4) == 0x4 ? true : false;
        }

        /// <summary>
        /// Home senser check
        /// </summary>
        /// <returns></returns>
        public bool GetHomeSensor()
        {
            if (Define.SIMULATION == true) return false;
            if (uni.bConnected() == false) return false;

            int iLimitN, iLimitP, iHomeSensor, iServoAlarm, iServoReady, iServoOn;

            int iStatus = eUniGetSignalStatus(uni.iNet, (int)uiControllerAxis, out iLimitN, out iLimitP, out iHomeSensor,
                                                        out iServoAlarm, out iServoReady, out iServoOn);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} Get Home Sensor Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
            }
            return iHomeSensor == 0 ? false : true;
        }

        /// <summary>
        /// Get alaram
        /// </summary>
        /// <returns></returns>
        public bool GetAlarm()
        {
            if (Define.SIMULATION == true) return false;
            if (uni.bConnected() == false) return false;

            int iLimitN, iLimitP, iHomeSensor, iServoAlarm, iServoReady, iServoOn;

            int iStatus = eUniGetSignalStatus(uni.iNet, (int)uiControllerAxis, out iLimitN, out iLimitP, out iHomeSensor,
                                                        out iServoAlarm, out iServoReady, out iServoOn);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} Get Fault Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
            }
            return iServoAlarm == 0 ? false : true;
        }

        /// <summary>
        /// Get servo ready state check
        /// </summary>
        /// <returns></returns>
        public bool GetServoReady()
        {
            if (Define.SIMULATION == true) return true;
            if (uni.bConnected() == false) return false;

            int iLimitN, iLimitP, iHomeSensor, iServoAlarm, iServoReady, iServoOn;

            int iStatus = eUniGetSignalStatus(uni.iNet, (int)uiControllerAxis, out iLimitN, out iLimitP, out iHomeSensor,
                                                        out iServoAlarm, out iServoReady, out iServoOn);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} Get Servo Ready Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
            }
            return iServoReady == 0 ? false : true;
        }

        /// <summary>
        /// Get cmd position
        /// </summary>
        /// <returns></returns>
        public double GetCmdPostion()
        {
            if (Define.SIMULATION == true)
            {
                return axisParam.dSimCmdPos;
            }
            else
            {
                if (uni.bConnected() == false) return 0;

                int iPos = 0;
                int iStatus = eUniGetCommandPosition(uni.iNet, (int)uiControllerAxis, out iPos);
                if (iStatus != (int)eUniApiReturnCode.Success)
                {
                    ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                     string.Format("Axis {0} Get Cmd Postion Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
                    return 0;
                }
                else
                {
                    return System.Math.Round(iPos / dScale, 3);
                }
            }
        }

        /// <summary>
        /// Get actual position
        /// </summary>
        /// <returns></returns>
        public double GetActPostion()
        {
            if (Define.SIMULATION == true)
            {
                return axisParam.dSimCmdPos;
            }
            if (uni.bConnected() == false) return 0;

            int iPos = 0;
            int iStatus = eUniGetActualPosition(uni.iNet, (int)uiControllerAxis, out iPos);
            if (iStatus != (int)eUniApiReturnCode.Success)
            {
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                 string.Format("Axis {0} Get Act Postion Fail, Error Code {1}", eAxis, (eUniApiReturnCode)iStatus).ToString());
                return 0;
            }
            else
            {
                return iPos / dScale;
            }
        }

        /// <summary>
        /// Inposition 상태 확인(UNI에 없는 기능이라 False 리턴)
        /// </summary>
        /// <returns></returns>
        public bool GetInPosition()
        {
            return false;
        }
    }
}