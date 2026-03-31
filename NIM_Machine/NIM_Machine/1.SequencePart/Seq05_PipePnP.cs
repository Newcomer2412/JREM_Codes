using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// 시퀀스 클래스
    /// </summary>
    public class Seq05_PipePnP : ISequence, ISeqNo
    {
        private readonly object readLock = new object();
        private object instance = null;

        public object Ins
        {
            get
            {
                lock (readLock)
                {
                    return instance;
                }
            }
            set
            {
                instance = value;
            }
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public Seq05_PipePnP(eSequence sequence)
        {
            instance = this;
            Proc = new CSeqProc();
            Proc.SeqName = SeqName = sequence.ToString();
            cSwMainTimeOut = new Stopwatch();
        }

        /// <summary>
        /// 시퀀스 소멸
        /// </summary>
        public void Free()
        {
            Proc.Free();
        }

        /// <summary>
        /// 공통 클래스 제어 시퀀스 관리 Dictionary
        /// </summary>
        private Dictionary<int, ISeqNo> subWorker = null;

        public Dictionary<int, ISeqNo> SubWorker
        {
            get { return subWorker; }
            set { subWorker = value; }
        }

        /// <summary>
        /// 시퀀스 초기화
        /// </summary>
        public void Init()
        {
            ml = CMainLib.Ins;
            cIO = ml.Seq.SeqIO;
            CAM0_ResultData = CMainLib.Ins.cVar.GetVisionResultData(0, 0);

            // 로그 설정
            eLogType = eLogType.Seq05_PipePnP;

            ClearSequence();
        }

        /// <summary>
        /// 시퀀스 이름
        /// </summary>
        public string SeqName { get; set; }

        /// <summary>
        /// 시퀀스 로그
        /// </summary>
        private eLogType eLogType { get; set; }

        /// <summary>
        /// Main Lib
        /// </summary>
        private CMainLib ml;

        /// <summary>
        /// IO 컨트롤
        /// </summary>
        private Seq_IO cIO;

        /// <summary>
        /// SystemParameterArray
        /// </summary>
        private CSystemParameterArray cSysArray;

        /// <summary>
        /// CAM0 Vision Result Data
        /// </summary>
        private CVisionResultData CAM0_ResultData;

        /// <summary>
        /// 시퀀스 스레드 동작 관리 클래스
        /// </summary>
        public CSeqProc Proc { get; set; }

        /// <summary>
        /// Main 시퀀스 타임아웃 체크
        /// </summary>
        public Stopwatch cSwMainTimeOut { get; set; }

        /// <summary>
        /// 메인 시퀀스 스텝
        /// </summary>
        public int iStep { get; set; }

        public int iPreStep { get; set; }

        /// <summary>
        /// 서브 시퀀스 스텝
        /// </summary>
        public int iSubStep { get; set; }

        public int iPreSubStep { get; set; }

        /// <summary>
        /// 시퀀스 스텝, 플래그 초기화
        /// </summary>
        public void ClearSequence()
        {
            cSysArray = ml.cSysParamCollData.GetSysArray();
            iStep = 0;
            iPreStep = -1;
            iSubStep = 0;
            iPreSubStep = -1;
            iManualStep = 0;
            iPreManualStep = -1;
        }

        /// <summary>
        /// 다음 이동 시퀀스 설정
        /// </summary>
        /// <param name="iStep"></param>
        public void Next(int iStep)
        {
            this.iStep = iStep;
        }

        /// <summary>
        /// 기능 함수 다음 이동 시퀀스 설정
        /// </summary>
        /// <param name="iStep"></param>
        public void SubNext(int iSubStep)
        {
            this.iSubStep = iSubStep;
        }

        /// <summary>
        /// 스레드 일시 정지
        /// </summary>
        public void Pause()
        {
            Proc.Pause();
        }

        /// <summary>
        /// 스레드 재시작
        /// </summary>
        public void ReStart()
        {
            Proc.Restart();
        }

        /// <summary>
        /// Main Timer Stop
        /// </summary>
        public void MainTimerStop()
        {
            cSwMainTimeOut.Stop();
            if (SubWorker != null)
            {
                foreach (KeyValuePair<int, ISeqNo> item in SubWorker)
                {
                    ((ISeqNo)item.Value.Ins).MainTimerStop();
                }
            }
        }

        /// <summary>
        /// 스래드 시작
        /// </summary>
        public void Start()
        {
            if (SubWorker != null)
            {
                foreach (KeyValuePair<int, ISeqNo> item in SubWorker)
                {
                    ((ISeqNo)item.Value.Ins).Init();
                }
            }

            Proc.Start(Do, ThreadPriority.Normal);
            cSwMainTimeOut.Restart();
        }

        /// <summary>
        /// 매뉴얼 스래드 시작
        /// </summary>
        public void ManualStart()
        {
            if (SubWorker != null)
            {
                foreach (KeyValuePair<int, ISeqNo> item in SubWorker)
                {
                    ((ISeqNo)item.Value.Ins).Init();
                }
            }

            Proc.ManualStart(ManualDo, ThreadPriority.Normal);
        }

        /// <summary>
        /// 스래드 정지
        /// </summary>
        public void Stop()
        {
            if (SubWorker != null)
            {
                foreach (KeyValuePair<int, ISeqNo> item in SubWorker)
                {
                    ((ISeqNo)item.Value.Ins).Free();
                }
            }

            Proc.Stop();
        }

        /// <summary>
        /// 스래드 긴급 정지
        /// </summary>
        public void EMGStop()
        {
            Proc.EMGStop();
        }

        /// <summary>
        /// 메인 Sequence 구동 스레드
        /// </summary>
        public bool Do()
        {
            // 파이프 삽입 안함
            if (ml.cVar.bUsePipe == false) return true;

            // 정지 명령로 상태가 변경되어도 부분 동작을 완료한 후 멈춤
            if (CSeqProc.bSeqStopCommand == true &&
                iStep == 0)
            {
                if (SubWorker != null)
                {
                    bool bCommSeqDone = true;
                    foreach (KeyValuePair<int, ISeqNo> item in SubWorker)
                    {
                        if (((ISeqNo)item.Value.Ins).iStep != 0) bCommSeqDone = false;
                    }
                    if (bCommSeqDone == true) return true;
                    else
                    {
                        if (SubWorker != null)
                        {
                            foreach (KeyValuePair<int, ISeqNo> item in SubWorker)
                            {
                                ((ISeqNo)item.Value.Ins).Do();
                            }
                        }
                        return false;
                    }
                }
                else return true;
            }

            if (iStep != iPreStep)
            {
                iPreStep = iStep;
                cSwMainTimeOut.Restart();
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} Do Step : {iStep} Process"), false);
            }

            // 동작 타임 아웃 측정
            if (cSwMainTimeOut.ElapsedMilliseconds > Define.SEQ_TIME_OUT &&
                iStep != 0)
            {
                // 타임아웃 에러
                ml.AddError(eErrorCode.SEQ05_TIME_OUT);
                Next(0);
                return true;
            }

            if (SubWorker != null)
            {
                foreach (KeyValuePair<int, ISeqNo> item in SubWorker)
                {
                    ((ISeqNo)item.Value.Ins).Do();
                }
            }

            switch (iStep)
            {
                case 0:
                    {
                        // 파이프 공급관련 맵데이터 정의
                        MapDataLib FeederBasket = ml.cRunUnitData.GetIndexData(eData.FEEDER_BASKET);
                        MapDataLib PipePnP = ml.cRunUnitData.GetIndexData(eData.PIPE_PnP);
                        MapDataLib PipeTransfer = ml.cRunUnitData.GetIndexData(eData.PIPE_TRANSFER);

                        if (PipePnP != null && PipeTransfer != null)
                        {
                            // 파이프 피더에서 픽업 작업 맵데이터 조건 확인
                            bool bPipePickUpFeederMapData = FeederBasket.GetUnitNo(0).eStatus == eStatus.MOUNT &&
                                                            PipePnP.GetStatus(eStatus.EMPTY);

                            // 파이프 트렌스퍼로 플레이스 작업 맵데이터 조건 확인
                            bool bPipePlaceTransferMapData = PipePnP.GetStatus(eStatus.MOUNT) &&
                                                             PipeTransfer.GetStatus(eStatus.STANBY);

                            if (bPipePickUpFeederMapData == true)
                            {
                                Next(10);
                            }
                            else if (bPipePlaceTransferMapData == true)
                            {
                                Next(20);
                            }
                            else
                            {
                                double dGetCmd_PipePnP_X = ml.Axis[eMotor.PIPE_PnP_X].GetCmdPostion();
                                double dGetCmd_PipePnP_Y = ml.Axis[eMotor.PIPE_PnP_Y].GetCmdPostion();
                                double dPipePnP_X_SafePos = ml.cAxisPosCollData.GetAxisPosition(eMotor.PIPE_PnP_X, (int)eAxisPipePnP_X.Safe);
                                double dPipePnP_Y_SafePos = ml.cAxisPosCollData.GetAxisPosition(eMotor.PIPE_PnP_Y, (int)eAxisPipePnP_Y.Safe);

                                if (dGetCmd_PipePnP_X != dPipePnP_X_SafePos ||
                                    dGetCmd_PipePnP_Y != dPipePnP_Y_SafePos)
                                {
                                    Next(30);
                                }
                            }
                        }
                    }
                    break;

                case 10:    // 파이프 픽업 함수 (바이브레이터에 있는 파이프)
                    {
                        if (PipePickUpFeeder() == true)
                        {
                            Next(0);
                        }
                    }
                    break;

                case 20:    // 파이프 트렌스퍼 플레이스 함수
                    {
                        if (PipePlaceTransfer() == true)
                        {
                            Next(0);
                        }
                    }
                    break;

                case 30:    // 안전위치 대기 함수
                    {
                        if (MoveSafePnP() == true)
                        {
                            Next(0);
                        }
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 딜레이 스톱워치
        /// </summary>
        private Stopwatch cDelay = new Stopwatch();

        /// <summary>
        /// 타임아웃 스톱워치
        /// </summary>
        private Stopwatch cTimeOut = new Stopwatch();

        // 파이프 X, Y축의 계산된 포지션 변수
        private double dMovePipePnP_X = 0.0; private double dMovePipePnP_Y = 0.0;

        /// <summary>
        /// 니들 픽업 카운트
        /// </summary>
        private int iPickRetryCount = 0;

        /// <summary>
        /// 파이프 픽업 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool PipePickUpFeeder()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib FeederBasket = ml.cRunUnitData.GetIndexData(eData.FEEDER_BASKET);
            MapDataLib PipePnP = ml.cRunUnitData.GetIndexData(eData.PIPE_PnP);
            MapDataLib NeedlePnP = ml.cRunUnitData.GetIndexData(eData.NEEDLE_PnP);
            MapDataLib MPC1_NeedleMount = ml.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT);

            switch (iSubStep)
            {
                case 0:     // 파이프 피더에서 픽업 작업 맵데이터 조건 확인
                    {
                        bool bPipePickUpFeederMapData = FeederBasket.GetUnitNo(0).eStatus == eStatus.MOUNT &&
                                                        PipePnP.GetStatus(eStatus.EMPTY);

                        if (bPipePickUpFeederMapData == true)
                        {
                            cTimeOut.Restart();
                            SubNext(10);
                        }
                        else
                        {
                            if (ml.McState == eMachineState.MANUALRUN)
                            {
                                CCommon.ShowMessageMini("Map Data 확인");
                                return true;
                            }
                        }
                    }
                    break;

                case 10:    // 파이프 Z축 안전 위치로 이동
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Z].MoveAbsolute((int)eAxisPipePnP_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR8_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 파이프 Z축 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // 파이프 Z축 흡착하고 있는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PNP_PIPE_VACUUM, true) == false)
                        {
                            SubNext(13);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ05_PIPE_PnP_VACUUM_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 13:    // 파이프 Z축 벅큠, 블로우 Off
                    {
                        cIO.SetOutput((int)eIO_O.PIPE_VACUUM, false);
                        cIO.SetOutput((int)eIO_O.PIPE_BLOW, false);
                        SubNext(20);
                    }
                    break;

                case 20:    // 파이프 위치의 좌표를 가져와 해당 위치로 X, Y축을 이동
                    {
                        // 파이프 PnP X, Y축의 픽업 포지션을 가져온다.
                        double dPipePnP_X_PickUpPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.PIPE_PnP_X, (int)eAxisPipePnP_X.PipePickUp);
                        double dPipePnP_Y_PickUpPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.PIPE_PnP_Y, (int)eAxisPipePnP_Y.PipePickUp);

                        // 픽업 할 파이프 X 위치값과 파이프 PnP X축의 계산
                        dMovePipePnP_X = dPipePnP_X_PickUpPos + CAM0_ResultData.dPipeX;
                        // 픽업 할 파이프 Y 위치값과 파이프 PnP Y축의 계산
                        dMovePipePnP_Y = dPipePnP_Y_PickUpPos + CAM0_ResultData.dPipeY;

                        cTimeOut.Restart();
                        SubNext(21);
                    }
                    break;

                case 21:    // 파이프 PnP Y, Z축 픽업위치 이동
                    {
                        // 파이프 Y축 먼저 이동하고 X축을 나중에 이동시킨다. (Transfer와 충돌방지)
                        ml.Axis[eMotor.PIPE_PnP_T].MoveAbsolute(CAM0_ResultData.dPipeDegree);
                        if (ml.Axis[eMotor.PIPE_PnP_Y].MoveAbsolute(dMovePipePnP_Y) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(22);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR7_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 22:    // 파이프 PnP Y, Z축 픽업위치 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Y].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(23);
                        }
                    }
                    break;

                case 23:    // 파이프 PnP X축 픽업위치 이동
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_X].MoveAbsolute(dMovePipePnP_X) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(24);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR6_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 24:    // 파이프 PnP X축 픽업위치 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.PIPE_PnP_T].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            // 벅큠 On
                            cIO.SetOutput((int)eIO_O.PIPE_VACUUM, true);
                            SubNext(30);
                        }
                    }
                    break;

                case 30:    // 파이프 PnP Z축 픽업 위치로 하강
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Z].MoveAbsolute((int)eAxisPipePnP_Z.PipePickUp) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(31);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR8_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 31:    // 파이프 PnP Z축 픽업 위치로 하강 확인
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Z].IsMoveDone() == true)
                        {
                            cDelay.Restart();
                            SubNext(32);
                        }
                    }
                    break;

                case 32:    // 벅큠 딜레이
                    {
                        if (cDelay.ElapsedMilliseconds > cSysArray.uiPnPPipeVacuumDelay)
                        {
                            cDelay.Stop();
                            cTimeOut.Restart();
                            SubNext(33);
                        }
                    }
                    break;

                case 33:    // 벅큠 딜레이 후 Z축 Up
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Z].MoveAbsolute((int)eAxisPipePnP_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(34);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR8_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 34:    // 파이프 Z축 안전위치 상승 확인
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(40);
                        }
                    }
                    break;

                case 40:    // 파이프 PnP X, T축 플레이스 위치 이동 (트렌스퍼와 충돌 방지)
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_X].MoveAbsolute((int)eAxisPipePnP_X.PipePlaceDown) == true &&
                            ml.Axis[eMotor.PIPE_PnP_T].MoveAbsolute((int)eAxisPipePnP_T.PipePlace) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(41);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR6_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 41:    // 파이프 PnP X축 플레이스 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_X].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(42);
                        }
                    }
                    break;

                case 42:    // 파이프 PnP Y축 플레이스 위치 이동
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Y].MoveAbsolute((int)eAxisPipePnP_Y.PipePlaceDown) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(43);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR7_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 43:    // 파이프 PnP Y, T축 플레이스 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Y].IsMoveDone() == true &&
                            ml.Axis[eMotor.PIPE_PnP_T].IsMoveDone() == true)
                        {
                            SubNext(44);
                        }
                    }
                    break;

                case 44:    // 파이프을 흡착했는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PNP_PIPE_VACUUM) == true ||
                            ml.cOptionData.bDryRunUse == true)
                        {
                            SubNext(100);
                        }
                        else
                        {
                            // 파이프 흡착하지 못했으면 피더 데이터를 변경 후 시퀀스 종료한다.
                            if (iPickRetryCount < 3)
                            {
                                if (NeedlePnP.GetStatus(eStatus.MOUNT) == true)
                                {
                                    FeederBasket.SetAllStatus(eStatus.EMPTY);
                                }
                                else
                                {
                                    FeederBasket.GetUnitNo(0).eStatus = eStatus.EMPTY;
                                }
                                SubNext(0);
                                return true;
                            }
                            else
                            {
                                // 파이프 흡착 재시도 3번 초과되면 알람을 띄운다.
                                iPickRetryCount = 0;
                                ml.AddError(eErrorCode.SEQ05_PIPE_PnP_VACUUM_NOT_DETECTION, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 100:   // 파이프 PnP 맵데이터 변경
                    {
                        PipePnP.SetAllStatus(eStatus.MOUNT);
                        if (NeedlePnP.GetStatus(eStatus.MOUNT) == true)
                        {
                            FeederBasket.SetAllStatus(eStatus.EMPTY);
                        }
                        else
                        {
                            FeederBasket.GetUnitNo(0).eStatus = eStatus.EMPTY;
                        }
                        iPickRetryCount = 0;

                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 파이프 트렌스퍼 플레이스 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool PipePlaceTransfer()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib PipePnP = ml.cRunUnitData.GetIndexData(eData.PIPE_PnP);
            MapDataLib PipeTransfer = ml.cRunUnitData.GetIndexData(eData.PIPE_TRANSFER);

            switch (iSubStep)
            {
                case 0:     // 파이프 트렌스퍼로 플레이스 작업 맵데이터 조건 확인
                    {
                        if (PipePnP.GetStatus(eStatus.MOUNT) == true &&
                            PipeTransfer.GetStatus(eStatus.STANBY) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(10);
                        }
                        else
                        {
                            if (ml.McState == eMachineState.MANUALRUN)
                            {
                                CCommon.ShowMessageMini("Map Data 확인");
                                return true;
                            }
                        }
                    }
                    break;

                case 10:    // 파이프 PnP Z축 안전위치로 이동
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Z].MoveAbsolute((int)eAxisPipePnP_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR8_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 파이프 PnP Z축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // 파이프 트랜스퍼가 BWD 위치인지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.TRANSFER_PIPE_BACKWARD_ON, true) == true ||
                            Define.SIMULATION == true)
                        {
                            SubNext(13);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                ml.AddError(eErrorCode.SEQ05_TRANSFER_LEFT_CYLINDER_BWD_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 13:    // 파이프 트랜스퍼에 파이프이 없는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.TRANSFER_PIPE_VACUUM, true) == false)
                        {
                            // 파이프 트렌스퍼 벅큠, 블로우 Off
                            cIO.SetOutput((int)eIO_O.TRANSFER_PIPE_VACUUM, false);
                            cIO.SetOutput((int)eIO_O.TRANSFER_PIPE_BLOW, false);
                            SubNext(14);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ05_TRANSFER_LEFT_PIPE_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 14:    // 파이프 트렌스퍼 R축이 현재 Origin 위치인지 확인
                    {
                        double dGetCmdTransfer_R = ml.Axis[eMotor.PIPE_ROTATE].GetCmdPostion();
                        double dTransfer_R_Origin = ml.cAxisPosCollData.GetAxisPosition(eMotor.PIPE_ROTATE, (int)eAxisPipeRotate.Origin);
                        if (dGetCmdTransfer_R == dTransfer_R_Origin)
                        {
                            cTimeOut.Restart();
                            cIO.SetOutput((int)eIO_O.TRANSFER_PIPE_VACUUM, true);
                            SubNext(20);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ05_TRANSFER_LEFT_MOTOR_R_NOT_ORIGIN_POS, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 20:    // 파이프 PnP X, T축 먼저 플레이스 위치 이동 (트렌스퍼와 충돌 방지)
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.PIPE_PnP_T].IsMoveDone() == true)
                        {
                            if (ml.Axis[eMotor.PIPE_PnP_X].MoveAbsolute((int)eAxisPipePnP_X.PipePlaceDown) == true &&
                                ml.Axis[eMotor.PIPE_PnP_T].MoveAbsolute((int)eAxisPipePnP_T.PipePlace) == true)
                            {
                                cTimeOut.Stop();
                                SubNext(21);
                            }
                            else
                            {
                                if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                                {
                                    cTimeOut.Stop();
                                    ml.AddError(eErrorCode.MOTOR6_MOVE_TIMEOUT, iSubStep);
                                    return true;
                                }
                            }
                        }
                    }
                    break;

                case 21:    // 파이프 PnP X축 플레이스 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_X].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(22);
                        }
                    }
                    break;

                case 22:    // 파이프 PnP Y축 플레이스 위치 이동
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Y].MoveAbsolute((int)eAxisPipePnP_Y.PipePlaceDown) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(23);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR7_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 23:    // 파이프 PnP Y, T축 플레이스 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Y].IsMoveDone() == true &&
                            ml.Axis[eMotor.PIPE_PnP_T].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(24);
                        }
                    }
                    break;

                case 24:    // 파이프이 흡착되어 있는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PNP_PIPE_VACUUM) == true ||
                            ml.cOptionData.bDryRunUse == true)
                        {
                            cTimeOut.Restart();
                            SubNext(30);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ05_PIPE_PnP_VACUUM_NOT_DETECTION, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 30:    // 파이프 PnP Z축 플레이스 위치 이동 / 파이프 트렌스퍼 벅큠 On
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Z].MoveAbsolute((int)eAxisPipePnP_Z.PipePlaceDown) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(31);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR8_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 31:    // 파이프 PnP Z축 플레이스 위치 이동
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Z].IsMoveDone() == true)
                        {
                            SubNext(32);
                        }
                    }
                    break;

                case 32: // 파이프 PnP 벅큠 Off, 블로우 On
                    {
                        cIO.SetOutput((int)eIO_O.PIPE_VACUUM, false);
                        cDelay.Restart();
                        SubNext(33);
                    }
                    break;

                case 33:    // 딜레이 후 파이프 PnP 블로우 Off
                    {
                        if (cDelay.ElapsedMilliseconds > cSysArray.uiTransferVacuumDelay)
                        {
                            cDelay.Stop();
                            cTimeOut.Restart();
                            SubNext(40);
                        }
                    }
                    break;

                case 40:    // 파이프 PnP Z축 안전위치 위치 이동
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Z].MoveAbsolute((int)eAxisPipePnP_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(41);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR8_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 41:    // 파이프 PnP Z축 플레이스 위치 이동
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Z].IsMoveDone() == true)
                        {
                            cDelay.Restart();
                            SubNext(42);
                        }
                    }
                    break;

                case 42:    // 파이프 트랜스퍼에 파이프이 흡착되어있는지 화인
                    {
                        if (cIO.GetInput((int)eIO_I.TRANSFER_PIPE_VACUUM) == true ||
                            ml.cOptionData.bDryRunUse == true)
                        {
                            SubNext(100);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ05_PIPE_TRANSFER_VACUUM_NOT_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 100:   // Pipe PnP 및 Transfer 맵데이터 변경
                    {
                        PipePnP.SetAllStatus(eStatus.EMPTY);
                        PipeTransfer.SetAllStatus(eStatus.MOUNT);

                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// PnP X,Y,Z 안전위치 이동 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool MoveSafePnP()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            switch (iSubStep)
            {
                case 0:
                    {
                        cTimeOut.Restart();
                        SubNext(10);
                    }
                    break;

                case 10:    // 파이프 PnP Z축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Z].MoveAbsolute((int)eAxisPipePnP_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR8_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // 파이프 PnP X,Y축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_X].MoveAbsolute((int)eAxisPipePnP_X.Safe) == true &&
                            ml.Axis[eMotor.PIPE_PnP_Y].MoveAbsolute((int)eAxisPipePnP_Y.Safe) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(13);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.PIPE_PnP_X].MoveAbsolute((int)eAxisPipePnP_X.Safe) == false)
                                    ml.AddError(eErrorCode.MOTOR6_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.PIPE_PnP_Y].MoveAbsolute((int)eAxisPipePnP_Y.Safe) == false)
                                    ml.AddError(eErrorCode.MOTOR7_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 13:    // 파이프 PnP X,Y축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.PIPE_PnP_Y].IsMoveDone() == true)
                        {
                            SubNext(100);
                        }
                    }
                    break;

                case 100:
                    {
                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        #region Maunal

        /// <summary>
        /// Manual Sequence 구동 스레드
        /// </summary>
        public bool ManualDo()
        {
            if (CSeqProc.bSeqStopCommand == true) return true;

            switch (iStep)
            {
                case 0:
                    {
                        if (ml.cVar.Manual_PipePnPPickUp == true)
                        {
                            Next(10);
                            ml.cVar.Manual_PipePnPPickUp = false;
                        }
                        else if (ml.cVar.Manual_PipePnPPlace == true)
                        {
                            Next(20);
                            ml.cVar.Manual_PipePnPPlace = false;
                        }
                        else if (ml.cVar.Manual_PipePnPSafe == true)
                        {
                            Next(30);
                            ml.cVar.Manual_PipePnPSafe = false;
                        }
                    }
                    break;

                case 10:    // 파이프 픽업 함수 (바이브레이터에 있는 파이프)
                    {
                        if (PipePickUpFeeder() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 20:    // 트렌스퍼 플레이스 함수
                    {
                        if (PipePlaceTransfer() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 30:    // 파이프 PnP 안전위치 이동
                    {
                        if (MoveSafePnP() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 100:
                    {
                        Next(0);
                        ml.McStop();
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 매뉴얼 시퀀스 스텝
        /// </summary>
        public int iManualStep { get; set; }

        public int iPreManualStep { get; set; }

        /// <summary>
        /// 매뉴얼 기능 함수 다음 이동 시퀀스 설정
        /// </summary>
        /// <param name="iManualStep"></param>
        public void ManualNext(int iManualStep)
        {
            this.iManualStep = iManualStep;
        }

        #endregion Maunal
    }
}