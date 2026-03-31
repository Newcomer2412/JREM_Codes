using System;
using System.Threading;
using static EMotionSnetBase.SnetDevice;

namespace MachineControlBase
{
    /// <summary>
    /// 축 클래스
    /// </summary>
    public class eMotionTekRtexAxis : MotionProcMgr
    {
        /// <summary>
        /// Main Lib
        /// </summary>
        private CMainLib ml = null;

        /// <summary>
        /// Axis Param
        /// </summary>
        public AxisParam axisParam = null;

        /// <summary>
        /// Axis Position
        /// </summary>
        public AxisPositionData _axisPositionData;

        public AxisPositionData axisPositionData
        {
            get { return _axisPositionData; }
            set { _axisPositionData = value; }
        }

        /// <summary>
        /// eMotionTekRTEX RTEX 프로토콜
        /// </summary>
        private eMotionTekRTEX rtex;

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
        /// Moving 전 체크사항 델리게이트
        /// </summary>
        public delegate bool AxisSafeCheckHandler();

        private AxisSafeCheckHandler SafeHandler;

        /// <summary>
        /// Simulation 관련 클래스 생성 (시뮬레이션)
        /// </summary>
        public SimMotionControl simMotionControl = null;

        /// <summary>
        /// RTEX 모듈과 축 주소 받아서 저장
        /// </summary>
        /// <param name="rtex"></param>
        /// <param name="axisParam"></param>
        /// <param name="axisPositionData"></param>
        public eMotionTekRtexAxis(eMotionTekRTEX rtex, AxisParam axisParam, AxisPositionData axisPositionData)
        {
            ml = CMainLib.Ins;
            this.axisParam = axisParam;
            this.axisPositionData = axisPositionData;
            this.rtex = rtex;
            simMotionControl = new SimMotionControl(axisParam);
        }

        public void Init()
        {
        }

        public void Free()
        {
        }

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
                if (rtex.bConnected() == false) return false;

                bool bRtn = true;
                int iSensor, iStep, iDirection, iEdge, iDwellTime, uiVel, uiAccel;

                // 홈 동작 시 Z상 두번 잡는 증상으로 추가
                int iStatus = eSnetSetAbsRelMode(rtex.iNet, (int)uiControllerAxis, 0);

                if (iStatus != (int)eSnetApiReturnCode.Success)
                {
                    bRtn = false;
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("eSnetSetAxisAbsRelMode Fail : Result {0}", iStatus));
                    ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                }

                if (iHomeSenserType == (int)eSnetAxisSensor.ZPhase)
                {
                    // DT축은 Z상으로 원점을 잡아야 하므로 따로 정의함.
                    // [Step 2 설정]
                    iStep = 0;
                    iDirection = iHomeMoveDirection;
                    iEdge = (int)eSnetOriginEdge.Rising;
                    iSensor = iHomeSenserType;
                    iDwellTime = (int)uiHomeClearTime;
                    uiVel = (int)uiHomeVel * 60;
                    uiAccel = (int)uiHomeAcc;
                    eSnetAddHomingStep(rtex.iNet, (int)uiControllerAxis, iStep, uiVel, uiAccel, iDirection, iSensor, iEdge, iDwellTime);
                }
                else
                {
                    // [Step 0 설정]
                    iStep = 0; // homing 구동 Step (0 번 부터 시작)
                    iDirection = iHomeMoveDirection; // -1-->(-)방향, 1-->(+)방향
                    iEdge = (int)eSnetOriginEdge.Rising; // 1-->rising edge, 0-->falling edge (active_low, active_high 에 무관함)
                    iSensor = iHomeSenserType; // 0-->(-)Limit, 1-->(+)Limit, 2-->Home_Sensor, 3-->Z(C)Phase
                    iDwellTime = (int)uiHomeClearTime; // Default-->500msec
                    uiVel = (int)uiHomeVel * 60;
                    uiAccel = (int)uiHomeAcc;
                    eSnetAddHomingStep(rtex.iNet, (int)uiControllerAxis, iStep, uiVel, uiAccel, iDirection, iSensor, iEdge, iDwellTime);

                    // [Step 1 설정]
                    iStep = 1;
                    iDirection = iDirection * -1;
                    iEdge = (int)eSnetOriginEdge.Falling; // 0-->falling edge
                    uiVel = uiVel / 2;
                    uiAccel = uiAccel * 2;
                    eSnetAddHomingStep(rtex.iNet, (int)uiControllerAxis, iStep, uiVel, uiAccel, iDirection, iSensor, iEdge, iDwellTime);

                    if (bHomeUseZPhase == true)
                    {
                        //// [Step 2 설정] ////
                        iStep = 2;
                        iEdge = (int)eSnetOriginEdge.Rising; // 1-->rising edge
                        iSensor = (int)eSnetAxisSensor.ZPhase; // 3-->Z(C)Phase
                        uiVel = uiVel / 2;
                        uiAccel = uiAccel * 2;
                        eSnetAddHomingStep(rtex.iNet, (int)uiControllerAxis, iStep, uiVel, uiAccel, iDirection, iSensor, iEdge, iDwellTime);
                    }
                }
                // [Offset 설정]
                if (dHomeOffset != 0)
                {
                    int nZeroOffset = (int)(dHomeOffset * dScale);
                    int nSetTime = 1000; // Offset 구동 후 1000msec 뒤에 "0"점을 잡음
                    eSnetSetHomingShiftEx(rtex.iNet, (int)uiControllerAxis, (int)uiHomeVel * 180, uiAccel, (int)uiHomeDec, 66, 66, nZeroOffset, nSetTime);
                }

                // [7. Homing 시작]
                eSnetStartHoming(rtex.iNet, (int)uiControllerAxis);

                return bRtn;
            }
        }

        /// <summary>
        /// Homing 완료 확인
        /// </summary>
        /// <returns></returns>
        public bool HomeDone()
        {
            if (Define.SIMULATION == true) return true;
            int iGetStep = 0;
            int iGetEventId = 0;
            eSnetGetHomingResult(rtex.iNet, (int)uiControllerAxis, out iGetStep, out iGetEventId);

            if (iGetEventId == (int)eSnetOriginState.OriginDoneOk)
            {
                HomeComplete = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Homing 완료까지 대기
        /// </summary>
        /// <param name="strFailMsg"></param>
        /// <returns></returns>
        public bool WaitHome(ref string strFailMsg)
        {
            if (Define.SIMULATION == true) return true;
            int iGetStep = 0;
            int iGetEventId = 0;
            eSnetGetHomingResult(rtex.iNet, (int)uiControllerAxis, out iGetStep, out iGetEventId);

            BreakStop = false;
            int nCts = 0;
            while (iGetEventId != (int)eSnetOriginState.OriginDoneOk)
            {
                Thread.Sleep(10);
                eSnetGetHomingResult(rtex.iNet, (int)uiControllerAxis, out iGetStep, out iGetEventId);
                if (BreakStop == true)
                {
                    strFailMsg = string.Format("eSnetGetOriginResult Fail : Axis {0} Motor Stop Command", eAxis);
                    return false;
                }
                if (nCts >= (uiHomeTimeout * 100))   // ms로 변환 while문 한번도는데 대략 10ms
                {
                    strFailMsg = string.Format("eSnetGetOriginResult Fail : Axis {0} Time Out Result {1}", eAxis, iGetEventId);
                    Thread.Sleep(500);
                    Reset();
                    return false;
                }
                nCts++;
            }
            HomeComplete = true;
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
                PosIndex = -1;
                return simMotionControl.SimMotorMove(SimMoveType.ABS_Move, dPos, 100);
            }
            else
            {
                if (rtex.bConnected() == false) return false;
                PosIndex = -1;
                if (bSafe == true)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                }
                uint vel = (uint)(uiVelocity * ((double)ml.cSysOne.iAllAutoRatio / 100));
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
                PosIndex = iPosIndex;
                return simMotionControl.SimMotorMove(SimMoveType.ABS_Move, positionData.dPos, positionData.uiSpeed);
            }
            else
            {
                if (rtex.bConnected() == false) return false;
                PosIndex = iPosIndex;
                if (bSafe == true)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                }
                uint vel = (uint)(uiVelocity * ((double)ml.cSysOne.iAllAutoRatio / 100) * ((double)positionData.uiSpeed / 100));
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
                PosIndex = iPosIndex;
                return simMotionControl.SimMotorMove(SimMoveType.ABS_Move, positionData.dPos, positionData.uiSpeed);
            }
            else
            {
                if (rtex.bConnected() == false) return false;
                PosIndex = iPosIndex;
                if (bSafe == true)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                }
                uint vel = (uint)(uiVelocity * ((double)uiPercent / 100) * ((double)ml.cSysOne.iAllAutoRatio / 100));
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
                if (rtex.bConnected() == false) return false;
                PosIndex = -1;
                if (bSafe == true)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                }
                uint vel = (uint)(uiVel * ((double)ml.cSysOne.iAllAutoRatio / 100));
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
                if (rtex.bConnected() == false) return false;
                PosIndex = -1;
                if (bSafe == true)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                }
                uint vel = (uint)(uiVelocity * ((double)uiPercent / 100) * ((double)ml.cSysOne.iAllAutoRatio / 100));
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
                if (rtex.bConnected() == false) return false;
                PosIndex = -1;
                if (bSafe == true && Math.Abs(dPos) > 1.0)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                };
                uint vel = (uint)(uiVelocity * ((double)ml.cSysOne.iAllAutoRatio / 100));
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
                if (rtex.bConnected() == false) return false;
                PosIndex = iPosIndex;
                if (bSafe == true && Math.Abs(positionData.dPos) > 1.0)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                };
                uint vel = (uint)(uiVelocity * ((double)ml.cSysOne.iAllAutoRatio / 100) * ((double)positionData.uiSpeed / 100));
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
                if (rtex.bConnected() == false) return false;
                PosIndex = iPosIndex;
                if (bSafe == true && Math.Abs(positionData.dPos) > 1.0)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                };
                uint vel = (uint)(uiVelocity * ((double)uiPercent / 100) * ((double)ml.cSysOne.iAllAutoRatio / 100));
                return MoveRelative(positionData.dPos, uiAccel, uiDecel, vel);
            }
        }

        /// <summary>
        /// 상대이동(속도 변환 없음)
        /// </summary>
        /// <param name="iPosIndex"></param>
        /// <param name="bSafe"></param>
        /// <returns></returns>
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
                if (rtex.bConnected() == false) return false;
                PosIndex = iPosIndex;
                if (bSafe == true && Math.Abs(positionData.dPos) > 1.0)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                };
                return MoveRelativeNoVelConv(positionData.dPos, uiAccel, uiDecel, uiVelocity);
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
                return simMotionControl.SimMotorMove(SimMoveType.ABS_Move, dPos, uiPercent);
            }
            else
            {
                if (rtex.bConnected() == false) return false;
                PosIndex = -1;
                if (bSafe == true && Math.Abs(dPos) > 1.0)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                };
                uint vel = (uint)(uiVelocity * ((double)uiPercent / 100) * ((double)ml.cSysOne.iAllAutoRatio / 100));
                return MoveRelative(dPos, uiAccel, uiDecel, vel);
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
                if (rtex.bConnected() == false) return false;
                PosIndex = -1;
                if (bSafe == true && Math.Abs(dPos) > 1.0)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                };
                uint vel = (uint)(uiVel * ((double)ml.cSysOne.iAllAutoRatio / 100));
                return MoveRelative(dPos, uiAcc, uiDec, vel);
            }
        }

        /// <summary>
        /// 조그이동
        /// </summary>
        /// <param name="bCW"></param>
        /// <param name="bSafe"></param>
        public bool MoveVelocity(bool bCW, bool bSafe = true)
        {
            if (rtex.bConnected() == false) return false;
            PosIndex = -1;
            if (bSafe == true)
            {
                if (CInterLock.GetAxisSafety(eAxis) == false) return false;
            }
            return MoveVelocity(bCW, uiAccel, uiDecel, uiJogVel);
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
            if (rtex.bConnected() == false) return false;
            PosIndex = -1;
            if (bSafe == true)
            {
                if (CInterLock.GetAxisSafety(eAxis) == false) return false;
            }
            return MoveVelocity(bCW, uiAccel, uiDecel, uiVel);
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
            if (Define.SIMULATION == true) return true;

            int iStatus = eSnetSetAbsRelMode(rtex.iNet, (int)uiControllerAxis, 0);
            if (iStatus == (int)eSnetApiReturnCode.Success)
            {
                int iMoveType = (int)eSnetMoveType.Scurve;
                int iPos = (int)(dPos * dScale);
                int iVelHap = (int)uiVel * 60; // mm/s => mm/min;
                iStatus = eSnetMoveSingleEx(rtex.iNet, (int)uiControllerAxis, iMoveType, iVelHap, (int)uiAcc, (int)uiDec, 66, iPos);
                if (iStatus != (int)eSnetApiReturnCode.Success &&
                    iStatus != (int)eSnetApiReturnCode.MoveInvalidPosition)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("MoveAbsolute Fail : Result {0}", iStatus));
                    ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                    return false;
                }
            }
            else
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("MoveAbsolute eSnetSetAbsRelMode Fail : Result {0}", iStatus));
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
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
            if (Define.SIMULATION == true) return true;

            int iStatus = eSnetSetAbsRelMode(rtex.iNet, (int)uiControllerAxis, 1);
            if (iStatus == (int)eSnetApiReturnCode.Success)
            {
                int iMoveType = (int)eSnetMoveType.Scurve;
                int iPos = (int)(dPos * dScale);
                int iVelHap = (int)uiVel * 60; // mm/s => mm/min;
                iStatus = eSnetMoveSingleEx(rtex.iNet, (int)uiControllerAxis, iMoveType, iVelHap, (int)uiAcc, (int)uiDec, 66, iPos);
                if (iStatus != (int)eSnetApiReturnCode.Success &&
                    iStatus != (int)eSnetApiReturnCode.MoveInvalidPosition)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("MoveRelative Fail : Result {0}", iStatus));
                    ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                    return false;
                }
            }
            else
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("MoveRelative eSnetSetAbsRelMode Fail : Result {0}", iStatus));
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 상대이동(속도 변환 없음)
        /// </summary>
        /// <param name="dPos"></param>
        /// <param name="uiAcc"></param>
        /// <param name="uiDec"></param>
        /// <param name="uiVel"></param>
        /// <returns></returns>
        public bool MoveRelativeNoVelConv(double dPos, uint uiAcc, uint uiDec, uint uiVel)
        {
            if (Define.SIMULATION == true) return true;

            int iStatus = eSnetSetAbsRelMode(rtex.iNet, (int)uiControllerAxis, 1);
            if (iStatus == (int)eSnetApiReturnCode.Success)
            {
                int iMoveType = (int)eSnetMoveType.Scurve;
                int iPos = (int)(dPos * dScale);
                iStatus = eSnetMoveSingleEx(rtex.iNet, (int)uiControllerAxis, iMoveType, (int)uiVel, (int)uiAcc, (int)uiDec, 66, iPos);
                if (iStatus != (int)eSnetApiReturnCode.Success &&
                    iStatus != (int)eSnetApiReturnCode.MoveInvalidPosition)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("MoveRelative Fail : Result {0}", iStatus));
                    ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                    return false;
                }
            }
            else
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("MoveRelative eSnetSetAbsRelMode Fail : Result {0}", iStatus));
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
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
            if (Define.SIMULATION == true) return true;

            int iJerk = 66; // 단위 [%]
            int iVelHap = (int)uiVel * 60; // mm/s => mm/min;
            int iCw = bCW == true ? 1 : -1;
            int iStatus = eSnetMoveSingleExJog(rtex.iNet, (int)uiControllerAxis, iVelHap, (int)uiAcc, (int)uiDec, iJerk, iCw);
            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("MoveVelocity Fail : Result {0}", iStatus));
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
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
            if (rtex.bConnected() == false) return;

            BreakStop = false;
            int nCts = 0;
            while (IsMoveDone() == false)
            {
                Thread.Sleep(10);
                if (BreakStop)
                    break;

                if (nCts >= (uiMotionTimeout * 100))//ms로 변환 while문 한번도는데 대략 10ms
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("Motion WaitDone Time Out : Axis {0}", eAxis));
                    ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
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
            if (rtex.bConnected() == false) return false;
            int iTime = 300; // Slow Time

            int iStatus = eSnetSlowStop(rtex.iNet, (int)uiControllerAxis, iTime);
            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("Stop Fail : Result {0}, Axis {1}", iStatus, eAxis));
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
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
            if (rtex.bConnected() == false) return;

            {
                CMainLib.Ins.cVar.bInitializeMotor = false;
            }

            int iStatus = eSnetEmergencyStop(rtex.iNet, (int)uiControllerAxis);
            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("EmergencyStop Fail : Result {0}, Axis {1}", iStatus, eAxis));
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
            }
        }

        /// <summary>
        /// Set zero
        /// </summary>
        public void SetZero()
        {
            if (Define.SIMULATION == true)
            {
                dSimCmdPos = 0;
                return;
            }

            if (rtex.bConnected() == false) return;
            int iPosition = 0;
            int iStatus = eSnetSetHomePosition(rtex.iNet, (int)uiControllerAxis, iPosition);
            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("SetZero Fail : Result {0}, Axis {1}", iStatus, eAxis));
                ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
            }
        }

        /// <summary>
        /// Reset alarm clear
        /// </summary>
        public void Reset()
        {
            if (Define.SIMULATION == true) return;
            if (rtex.bConnected() == false) return;
            int iStatus = eSnetSetServoAlarmClear(rtex.iNet, (int)uiControllerAxis, 1, 1200);
            iStatus = eSnetReset(rtex.iNet, (int)uiControllerAxis);

            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("Reset Fail : Result {0}, Axis {1}", iStatus, eAxis));
            }
        }

        #region 토크 제한

        /// <summary>
        /// RTEX 드라이버의 "Pr00.13(제 1 토크한계)"파라미터 값이 변경됩니다
        /// 변경된 설정 값은 드라이버 내부 "Eeprom(비 휘발성 메모리)"에 저장되지 않습니다.
        /// (주의)"토크 한계"가 너무 낮게 설정되면 모터에 걸리는 부하 또는 "지령 속도"에 따라 정상 동작 시 모터가
        /// 회전을 못하는 경우가 발생할 수 있습니다.이 상태서 부하가 낮아 "지면 지령 위치를 추종하기 위해 급 가속
        /// 현상"이 생길 수 있습니다. 주의를 요합니다.
        /// </summary>
        /// <param name="iTrq">단위 [%]</param>
        /// <returns></returns>
        public bool SetTrqLimit(int iTrq)
        {
            if (Define.SIMULATION == true) return true;
            if (rtex.bConnected() == false) return false;

            int iStatus = eSnetRtexSetTrqLimit(rtex.iNet, (int)uiControllerAxis, iTrq);
            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("Axis {0}, SetTrqLimit Error : RESULT NUM : {1}", eAxis, iStatus));
                return false;
            }

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
            if (rtex.bConnected() == false) return false;

            int iStatus = eSnetRtexGetTrqLimit(rtex.iNet, (int)uiControllerAxis, out iTrq);
            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("Axis {0}, GetTrqLimit Error : RESULT NUM : {1}", eAxis, iStatus));
                return false;
            }

            return true;
        }

        /// <summary>
        /// "종점 위치"까지 단축 이송을 실행합니다. 이송 중 RTEX 드라이버의 "토크 제한 신호가" 설정 시간
        /// 이상 유지되면 축 이송을 중지하고 제어기의 "Command Position"이 엔코더 입력 값으로 계산된 "Actual
        /// Position"으로 변경됩니다.
        /// </summary>
        /// <param name="dPos">종점 위치[um]</param>
        /// <param name="uiVel">축 속도</param>
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
                if (rtex.bConnected() == false) return false;
                PosIndex = -1;
                if (bSafe == true)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                };

                int iPos = (int)(dPos / 0.001);
                int iStatus = eSnetRtexMoveAxisUntilTrqLimit(rtex.iNet, (int)uiControllerAxis, iPos, (int)uiVel, (int)uiAccel, (int)uiDecel, 66, (int)uiTime);
                if (iStatus != (int)eSnetApiReturnCode.Success)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("Axis {0}, MoveAxisUntilTrqLimit Error : RESULT NUM : {1}", eAxis, iStatus));
                    return false;
                }

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
                if (rtex.bConnected() == false) return false;
                PosIndex = -1;
                if (bSafe == true)
                {
                    if (CInterLock.GetAxisSafety(eAxis) == false) return false;
                };

                int iStatus = eSnetSetAbsRelMode(rtex.iNet, (int)uiControllerAxis, 0);
                if (iStatus == (int)eSnetApiReturnCode.Success)
                {
                    int iPos1 = (int)(dPos1 / 0.001);
                    int iPos2 = (int)(dPos2 / 0.001);
                    iStatus = eSnetRtexSetTrqLimitAndMove(rtex.iNet, (int)uiControllerAxis, iSettrq_1, iSettrq_2, (int)uiTime, iPos1, iPos2, (int)uiVel1, (int)uiVel2, (int)uiAccel, (int)uiDecel, 66);
                    if (iStatus != (int)eSnetApiReturnCode.Success)
                    {
                        NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("Axis {0}, SetTrqLimitAndMove Error : RESULT NUM : {1}", eAxis, iStatus));
                        return false;
                    }
                }
                else
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("SetTrqLimitAndMove eSnetSetAbsRelMode Fail : Result {0}", iStatus));
                    ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// SetTrqLimitAndMove" 지령 후 동작 상태를 확인 합니다.
        /// </summary>
        /// status : 현재 실행 상태
        /// 0x1 bit 0 : "1"-> "eSnetRtexSetTrqLimitAndMove" 동작 중
        /// 0x2 bit 1 : "1"-> "eSnetRtexSetTrqLimitAndMove" 동작 완료(종료 상태)
        /// 0x4 bit 2 : "1"-> 시퀀스 실행중 축 이송 전 정지 상태가 아님
        /// 0x8 bit 3 : "1"-> 시퀀스 실행중 축 이송 전 축 알람 발생 상태
        /// 0x10 bit 4 : "1"-> 시퀀스 실행중 축 이송 전 지정 축이 연속 보간에서 사용 중
        /// 0x20 bit 5 : "1"-> 시퀀스 실행중 축 이송 전 지정 축이 동기 운전의 슬레이브 축으로 사용중
        /// 0x40 bit 6 : "1"-> 시퀀스 실행중 MPG 모드로 전환됨 	</param>
        /// <returns></returns>
        public bool GetStatusTrqLimitAndMove()
        {
            if (Define.SIMULATION == true) return true;
            if (rtex.bConnected() == false) return false;

            int iStep = 0, iStatus = 0;
            int iRtn = eSnetRtexGetStatusTrqLimitAndMove(rtex.iNet, (int)uiControllerAxis, out iStep, out iStatus);
            if (iRtn != (int)eSnetApiReturnCode.Success)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                               string.Format("Axis {0}, GetStatusTrqLimitAndMove Error : RESULT NUM : {1}, Step {2}, Status {3}", eAxis, iRtn, iStep, iStatus));
                return false;
            }
            else
            {
                if ((iStatus & 0x1) != 0x1 &&
                    (iStatus & 0x2) == 0x2)
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
        /// ResetTrqLimitAndMove
        /// </summary>
        /// <returns></returns>
        public bool ResetTrqLimitAndMove()
        {
            if (Define.SIMULATION == true) return true;
            if (rtex.bConnected() == false) return false;

            int iStatus = eSnetRtexResetTrqLimitAndMove(rtex.iNet, (int)uiControllerAxis);
            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("Axis {0}, ResetTrqLimitAndMove Error : RESULT NUM : {1}", eAxis, iStatus));
                return false;
            }

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
            if (rtex.bConnected() == false) return;

            int iOnOff = bOn == true ? 1 : 0;
            int iStatus = eSnetSetServoOn(rtex.iNet, (int)uiControllerAxis, iOnOff);
            HomeComplete = false;
            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("Servo {0} Fail, Axis {1}", bOn == true ? "On" : "Off", eAxis));
            }
        }

        /// <summary>
        /// Get servo state
        /// </summary>
        /// <returns></returns>
        public bool GetServoState()
        {
            if (Define.SIMULATION == true) return true;
            if (rtex.bConnected() == false) return false;

            int iGetEnable = -1;
            int iStatus = eSnetGetServoOn(rtex.iNet, (int)uiControllerAxis, out iGetEnable);
            if (iStatus == (int)eSnetApiReturnCode.Success)
            {
                return iGetEnable == 1 ? true : false;
            }
            else
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("GetServoState Fail : Result {0}, Axis {1}", iStatus, eAxis));
                return false;
            }
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
                if (rtex.bConnected() == false) return true;
                int iGetValue;
                int iStatus = eSnetGetAxisStatus(rtex.iNet, (int)uiControllerAxis, out iGetValue);
                if (iStatus == (int)eSnetApiReturnCode.Success)
                {
                    if ((iGetValue & 0x1) == 0x1)
                    {
                        return false;
                    }
                    else
                    {
                        if ((iGetValue & 0x200) == 0x200)
                        {
                            NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("Inposition check time over : Result {0}", iGetValue));
                            ml.AddError(eErrorCode.MOTOR0_ERROR + (int)eAxis);
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("IsMoveDone Fail : Result {0}, Axis {1}", iStatus, eAxis));
                }
            }
            return false;
        }

        /// <summary>
        /// 제어기 에러 확인
        /// </summary>
        /// <returns></returns>
        public bool GetControllerFault()
        {
            if (Define.SIMULATION == true) return false;
            if (rtex.bConnected() == false) return true;

            int iGetValue;
            int iStatus = eSnetGetAxisSignalStatus(rtex.iNet, (int)uiControllerAxis, out iGetValue);

            if (iStatus == (int)eSnetApiReturnCode.Success)
            {
                if ((iGetValue & 0x80) == 0x80)
                    return true;
                else
                    return false;
            }
            else
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("GetControllerFault Fail : Result {0}, Axis {1}", iStatus, eAxis));
            }
            return false;
        }

        /// <summary>
        /// Servo 에러 확인
        /// </summary>
        /// <returns></returns>
        public bool GetServoFault()
        {
            if (Define.SIMULATION == true) return false;
            if (rtex.bConnected() == false) return true;

            int iGetValue = 0;
            int iStatus = eSnetGetAxisSignalStatus(rtex.iNet, (int)uiControllerAxis, out iGetValue);
            if (iStatus == (int)eSnetApiReturnCode.Success)
            {
                if ((iGetValue & 0x8) == 0x8)
                    return true;
                else
                    return false;
            }
            else
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("GetServoFault Fail : Result {0}, Axis {1}", iStatus, eAxis));
            }
            return false;
        }

        /// <summary>
        /// InPosition 확인
        /// </summary>
        /// <returns></returns>
        public bool GetInPosition()
        {
            if (Define.SIMULATION == true) return false;
            if (rtex.bConnected() == false) return true;

            int iGetValue = 0;
            int iStatus = eSnetGetAxisSignalStatus(rtex.iNet, (int)uiControllerAxis, out iGetValue);
            if (iStatus == (int)eSnetApiReturnCode.Success)
            {
                if ((iGetValue & 0x7) == 0x7)
                    return true;
                else
                    return false;
            }
            else
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("GetInPosition Fail : Result {0}, Axis {1}", iStatus, eAxis));
            }
            return false;
        }

        /// <summary>
        /// Check minus limit check
        /// </summary>
        /// <returns></returns>
        public bool GetMinusLimit()
        {
            if (Define.SIMULATION == true) return false;
            if (rtex.bConnected() == false) return true;

            int iLimit = 0;
            int iStatus = eSnetGetLimitStatus(rtex.iNet, (int)uiControllerAxis, out iLimit);
            if (iStatus == (int)eSnetApiReturnCode.Success)
            {
                if ((iLimit & 0x5) > 0)//0101
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("GetMinusLimit Fail : Result {0}, Axis {1}", iStatus, eAxis));
            }
            return false;
        }

        /// <summary>
        /// Check plus limit check
        /// </summary>
        /// <returns></returns>
        public bool GetPlusLimit()
        {
            if (Define.SIMULATION == true) return false;
            if (rtex.bConnected() == false) return true;

            int iLimit = 0;
            int iStatus = eSnetGetLimitStatus(rtex.iNet, (int)uiControllerAxis, out iLimit);
            if (iStatus == (int)eSnetApiReturnCode.Success)
            {
                if (((iLimit & (0xA)) > 0))//1010
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("GetPlusLimit Fail : Result {0}, Axis {1}", iStatus, eAxis));
            }
            return false;
        }

        /// <summary>
        /// Home senser check
        /// </summary>
        /// <returns></returns>
        public bool GetHomeSensor()
        {
            if (Define.SIMULATION == true) return false;
            if (rtex.bConnected() == false) return false;

            int iLimitN = 0; // (-)Limit
            int iLimitP = 0; // (+)Limit
            int iOrigin = 0;
            int iSvAlarm = 0; // Servo Alarm
            int iSvReady = 0; // Servo Ready
            int iSvOn = 0; // Servo On
            int iStatus = eSnetGetAxisIo(rtex.iNet, (int)uiControllerAxis, out iLimitN, out iLimitP, out iOrigin, out iSvAlarm, out iSvReady, out iSvOn);
            if (iStatus == (int)eSnetApiReturnCode.Success)
            {
                return iOrigin == 1 ? true : false;
            }
            else
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("GetHomeSensor Fail : Result {0}, Axis {1}", iStatus, eAxis));
            }
            return false;
        }

        /// <summary>
        /// Get alaram
        /// </summary>
        /// <returns></returns>
        public bool GetAlarm()
        {
            if (Define.SIMULATION == true) return false;
            if (rtex.bConnected() == false) return true;

            int iLimitN = 0; // (-)Limit
            int iLimitP = 0; // (+)Limit
            int iOrigin = 0;
            int iSvAlarm = 0; // Servo Alarm
            int iSvReady = 0; // Servo Ready
            int iSvOn = 0; // Servo On
            int iStatus = eSnetGetAxisIo(rtex.iNet, (int)uiControllerAxis, out iLimitN, out iLimitP, out iOrigin, out iSvAlarm, out iSvReady, out iSvOn);
            if (iStatus == (int)eSnetApiReturnCode.Success)
            {
                return iSvAlarm == 1 ? true : false;
            }
            else
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("GetAlarm Fail : Result {0}, Axis {1}", iStatus, eAxis));
            }
            return false;
        }

        /// <summary>
        /// Get cmd position
        /// </summary>
        /// <returns></returns>
        public double GetCmdPostion()
        {
            if (Define.SIMULATION == true)
            {
                return dSimCmdPos;
            }
            else
            {
                if (rtex.bConnected() == false) return 0;

                int iPos = 0;
                int iStatus = eSnetGetCommandPosition(rtex.iNet, (int)uiControllerAxis, out iPos);
                if (iStatus == (int)eSnetApiReturnCode.Success)
                {
                    return Math.Round(iPos / dScale, 3);
                }
                else
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("GetCmdPostion Fail : Result {0}, Axis {1}", iStatus, eAxis));
                }
                return 0;
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
                return dSimCmdPos;
            }
            if (rtex.bConnected() == false) return 0;

            int iPos = 0;
            int iStatus = eSnetGetActualPosition(rtex.iNet, (int)uiControllerAxis, out iPos);
            if (iStatus == (int)eSnetApiReturnCode.Success)
            {
                return iPos / dScale;
            }
            else
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("GetActPostion Fail : Result {0}, Axis {1}", iStatus, eAxis));
            }
            return 0;
        }

        /// <summary>
        /// Get servo ready state check
        /// </summary>
        /// <returns></returns>
        public bool GetServoReady()
        {
            if (Define.SIMULATION == true) return true;
            if (rtex.bConnected() == false) return false;

            int iLimitN = 0; // (-)Limit
            int iLimitP = 0; // (+)Limit
            int iOrigin = 0;
            int iSvAlarm = 0; // Servo Alarm
            int iSvReady = 0; // Servo Ready
            int iSvOn = 0; // Servo On
            int iStatus = eSnetGetAxisIo(rtex.iNet, (int)uiControllerAxis, out iLimitN, out iLimitP, out iOrigin, out iSvAlarm, out iSvReady, out iSvOn);
            if (iStatus == (int)eSnetApiReturnCode.Success)
            {
                return iSvReady == 1 ? true : false;
            }
            else
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("GetServoReady Fail : Result {0}, Axis {1}", iStatus, eAxis));
            }
            return false;
        }
    }
}