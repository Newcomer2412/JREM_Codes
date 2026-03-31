using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// 시퀀스 클래스
    /// </summary>
    public class Seq10_Dispensing : ISequence, ISeqNo
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
        public Seq10_Dispensing(eSequence sequence)
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
            CAM5_ResultData = CMainLib.Ins.cVar.GetVisionResultData(5, 0);

            // 로그 설정
            eLogType = eLogType.Seq10_Dispensing;

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
        /// CAM3 Vision Result Data
        /// </summary>
        private CVisionResultData CAM5_ResultData;

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
                ml.AddError(eErrorCode.SEQ10_TIME_OUT);
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
                        MapDataLib MPC1_Dispensing = ml.cRunUnitData.GetIndexData(eData.MPC1_DISPENSING);
                        MapDataLib Dispensing = ml.cRunUnitData.GetIndexData(eData.DISPENSER);

                        if (Dispensing != null && MPC1_Dispensing != null)
                        {
                            // 디스펜싱 맵데이터 조건
                            bool bHolderDispensingMapData = MPC1_Dispensing.GetStatus(eStatus.FLIP_DONE) &&
                                                            Dispensing.GetStatus(eStatus.STANBY);
                            // 디스펜서 클린 맵데이터 조건
                            bool bDispenserClean = Dispensing.GetStatus(eStatus.DISPENSER_CLEAN);

                            if (bDispenserClean == true)
                            {
                                Next(20);
                            }
                            else if (bHolderDispensingMapData == true)
                            {
                                Next(10);
                            }
                        }
                    }
                    break;

                case 10:    // 홀더 디스펜싱 함수
                    {
                        if (HolderDispensing() == true)
                        {
                            Next(0);
                        }
                    }
                    break;

                case 20:    // 디스펜서 노즐 Clean 함수
                    {
                        if (DispenserClean() == true)
                        {
                            Next(0);
                        }
                    }
                    break;

                case 30:    // 디스펜서 안전위치 이동 함수
                    {
                        if (MoveSafeDispenser() == true)
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

        /// <summary>
        /// 홀더 불량
        /// </summary>
        private bool bHolderFail = false;

        /// <summary>
        /// 홀더 디스펜싱 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool HolderDispensing()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib MPC1_Dispensing = ml.cRunUnitData.GetIndexData(eData.MPC1_DISPENSING);
            MapDataLib Dispensing = ml.cRunUnitData.GetIndexData(eData.DISPENSER);

            switch (iSubStep)
            {
                case 0:
                    {
                        // 디스펜싱 맵데이터 조건
                        bool bHolderDispensingMapData = MPC1_Dispensing.GetStatus(eStatus.FLIP_DONE) &&
                                                        Dispensing.GetStatus(eStatus.STANBY);

                        if (bHolderDispensingMapData == true)
                        {
                            cTimeOut.Restart();
                            bHolderFail = false;
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

                case 10:    // 디스펜서 Z축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            // 펌프 밸브 Close
                            cIO.SetOutput((int)eIO_O.DISPENSER_VALVE_OPEN, false);
                            // 클린 블로우 Off
                            cIO.SetOutput((int)eIO_O.DISPENSER_CLEAN_BLOW, false);
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR26_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 디스펜서 Z축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // 디스펜서 니들 평탄 측정 실린더 Up
                    {
                        cIO.SetOutput((int)eIO_O.DISP_NEEDLE_FLAT_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(13);
                    }
                    break;

                case 13:    // 디스펜서 니들 평탄 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.DISP_NEEDLE_FLAT_CHECK_UP_ON) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(14);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ10_DISP_HEIGH_CHECK_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 14:
                    {
                        if (ml.cSysOne.iDispWorkCount < ml.cSysOne.iDispWorkLimitCount)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SDQ10_DISP_LIQUIDE_RELOAD, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 20:    // 디스펜서 X,Y축 니들 평탄 작업 위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.NeedleFlat) == true &&
                            ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.NeedleFlat) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.NeedleFlat) == false)
                                    ml.AddError(eErrorCode.MOTOR25_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.NeedleFlat) == false)
                                    ml.AddError(eErrorCode.MOTOR18_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // 디스펜서 X,Y축 니들 평탄 작업 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.DISPENSER_Y].IsMoveDone() == true)
                        {
                            SubNext(22);
                        }
                    }
                    break;

                case 22:    // 디스펜서 니들 평탄 측정 실린더 Down
                    {
                        cIO.SetOutput((int)eIO_O.DISP_NEEDLE_FLAT_DOWN, true);
                        cTimeOut.Restart();
                        SubNext(23);
                    }
                    break;

                case 23:    // 디스펜서 니들 평탄 실린더 Down 확인
                    {
                        if (cIO.GetInput((int)eIO_I.DISP_NEEDLE_FLAT_CHECK_DOWN, true) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(24);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ10_DISP_HEIGH_CHECK_CYLINDER_DOWN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 24:    // 디스펜서 니들 평탄 측정 실린더 Up
                    {
                        cIO.SetOutput((int)eIO_O.DISP_NEEDLE_FLAT_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(25);
                    }
                    break;

                case 25:    // 디스펜서 니들 평탄 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.DISP_NEEDLE_FLAT_CHECK_UP_ON, true) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(30);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ10_DISP_HEIGH_CHECK_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 30:    // 디스펜서 X,Y축 홀더 비전 촬영 위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.HolderVision) == true &&
                            ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.HolderVision) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(31);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.HolderVision) == false)
                                    ml.AddError(eErrorCode.MOTOR25_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.HolderVision) == false)
                                    ml.AddError(eErrorCode.MOTOR18_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 31:    // 디스펜서 X,Y축 홀더 비전 촬영 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.DISPENSER_Y].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(32);
                        }
                    }
                    break;

                case 32:    // 디스펜서 Z축 홀더 비전 촬영 위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.HolderVision) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(33);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR26_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 33:    // 디스펜서 Z축 홀더 비전 촬영 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].IsMoveDone() == true)
                        {
                            cDelay.Restart();
                            SubNext(40);
                        }
                    }
                    break;

                case 40:
                    {
                        if (cDelay.ElapsedMilliseconds > 100)
                        {
                            cDelay.Stop();
                            if (ml.cOptionData.bDryRunUse == true)
                            {
                                SubNext(50);
                            }
                            else
                            {
                                CAM5_ResultData.DataClear();
                                ml.cCVisionToolBlockLib[5].VisionShoot(0);
                                SubNext(41);
                            }
                        }
                    }
                    break;

                case 41:    // 촬영 결과 여부
                    {
                        if (CAM5_ResultData.bShootFinish == true)
                        {
                            if (CAM5_ResultData.bHolderEmpty == true)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ10_DISP_HOLDER_EMPTY, iSubStep);
                                return true;
                            }
                            else if (CAM5_ResultData.bHolderPositionFail == true)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ10_DISP_HOLDER_POSITION_FAIL, iSubStep);
                                return true;
                            }
                            else
                            {
                                // 홀더 비전검사(파이프, 니들 유무) 스킵 유무
                                if (CAM5_ResultData.bGoodNg == true)
                                {
                                    // 촬영 성공
                                    cTimeOut.Restart();
                                    SubNext(50);
                                }
                                else
                                {
                                    cTimeOut.Stop();
                                    ml.AddError(eErrorCode.SEQ10_DISP_PIPEorNEEDLE_EMPTY, iSubStep);
                                    ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.Safe);
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > 3000)
                            {
                                // Vision 촬영 Time out 에러
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ10_DISP_VISION_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 50:    // 디스펜서 X,Y축 홀더 분주 위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.Dispensing, false) == true &&
                            ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.Dispensing, false) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(51);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.Dispensing) == false)
                                    ml.AddError(eErrorCode.MOTOR25_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.Dispensing) == false)
                                    ml.AddError(eErrorCode.MOTOR18_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 51:    // 디스펜서 X,Y축 홀더 분주 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.DISPENSER_Y].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(52);
                        }
                    }
                    break;

                case 52:    // 디스펜서 Z축 홀더 분주 위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.Dispensing) == true)
                        {
                            cTimeOut.Stop();
                            if (ml.cOptionData.bDryRunUse == false)
                            {
                                ml.Uz_IO_Module.ClearOutputTrigger((int)eIO_O.DISPENSER_VALVE_OPEN, 1);
                            }
                            SubNext(53);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR26_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 53:    // 디스펜서 Z축 홀더 분주 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].IsMoveDone() == true)
                        {
                            SubNext(60);
                        }
                    }
                    break;

                case 60:    // 펌프 밸브 Open
                    {
                        if (ml.cOptionData.bDryRunUse == false)
                        {
                            ml.Uz_IO_Module.StartOutputTrigger((int)eIO_O.DISPENSER_VALVE_OPEN, 1);
                        }
                        cDelay.Restart();
                        SubNext(61);
                    }
                    break;

                case 61:    // 펌프 밸브 딜레이
                    {
                        if (cDelay.ElapsedMilliseconds > ml.cSysOne.uiDispValveDelay * 1000)
                        {
                            cDelay.Stop();
                            SubNext(62);
                        }
                    }
                    break;

                case 62:    // 펌프 밸브 Close
                    {
                        if (ml.cOptionData.bDryRunUse == false)
                        {
                            ml.cSysOne.iDispWorkCount++; // 토출 횟수 증가
                        }
                        cTimeOut.Restart();
                        SubNext(63);
                    }
                    break;

                case 63:    // 디스펜서 Z축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(64);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR26_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 64:    // 디스펜서 Z축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(70);
                        }
                    }
                    break;

                case 70:    // 디스펜서 X,Y축 니들 평탄 작업 위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.NeedleFlat) == true &&
                            ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.NeedleFlat) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(71);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.NeedleFlat) == false)
                                    ml.AddError(eErrorCode.MOTOR25_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.NeedleFlat) == false)
                                    ml.AddError(eErrorCode.MOTOR18_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 71:    // 디스펜서 X,Y축 니들 평탄 작업 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.DISPENSER_Y].IsMoveDone() == true)
                        {
                            SubNext(72);
                        }
                    }
                    break;

                case 72:    // 디스펜서 니들 평탄 측정 실린더 Down
                    {
                        cIO.SetOutput((int)eIO_O.DISP_NEEDLE_FLAT_DOWN, true);
                        cTimeOut.Restart();
                        SubNext(73);
                    }
                    break;

                case 73:    // 디스펜서 니들 평탄 실린더 Down 확인
                    {
                        if (cIO.GetInput((int)eIO_I.DISP_NEEDLE_FLAT_CHECK_DOWN, true) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(74);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ10_DISP_HEIGH_CHECK_CYLINDER_DOWN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 74:    // 디스펜서 니들 평탄 측정 실린더 Up
                    {
                        cIO.SetOutput((int)eIO_O.DISP_NEEDLE_FLAT_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(75);
                    }
                    break;

                case 75:    // 디스펜서 니들 평탄 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.DISP_NEEDLE_FLAT_CHECK_UP_ON, true) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(100);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ10_DISP_HEIGH_CHECK_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 100:   // 홀더 디스펜싱 맵데이터 변경
                    {
                        if (bHolderFail == true)
                        {
                            MPC1_Dispensing.SetAllStatus(eStatus.NG);
                        }
                        else
                        {
                            MPC1_Dispensing.SetAllStatus(eStatus.DISPENSING);
                            Dispensing.SetAllStatus(eStatus.DISPENSER_CLEAN);
                        }

                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 비전검사무시 홀더 디스펜싱 동작
        /// </summary>
        /// <returns></returns>
        public bool HolderDispensingSkipVision()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib MPC1_Dispensing = ml.cRunUnitData.GetIndexData(eData.MPC1_DISPENSING);
            MapDataLib Dispensing = ml.cRunUnitData.GetIndexData(eData.DISPENSER);

            switch (iSubStep)
            {
                case 0:     // 양품 니들 픽업 맵데이터 조건
                    {
                        // 디스펜싱 맵데이터 조건
                        bool bHolderDispensingMapData = MPC1_Dispensing.GetStatus(eStatus.FLIP_DONE) &&
                                                        Dispensing.GetStatus(eStatus.STANBY);

                        if (bHolderDispensingMapData == true)
                        {
                            cTimeOut.Restart();
                            bHolderFail = false;
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

                case 10:    // 디스펜서 Z축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            // 펌프 밸브 Close
                            cIO.SetOutput((int)eIO_O.DISPENSER_VALVE_OPEN, false);
                            // 클린 블로우 Off
                            cIO.SetOutput((int)eIO_O.DISPENSER_CLEAN_BLOW, false);
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR26_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 디스펜서 Z축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // 디스펜서 니들 평탄 측정 실린더 Up
                    {
                        cIO.SetOutput((int)eIO_O.DISP_NEEDLE_FLAT_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(13);
                    }
                    break;

                case 13:    // 디스펜서 니들 평탄 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.DISP_NEEDLE_FLAT_CHECK_UP_ON) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(14);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ10_DISP_HEIGH_CHECK_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 14:
                    {
                        if (ml.cSysOne.iDispWorkCount < ml.cSysOne.iDispWorkLimitCount)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SDQ10_DISP_LIQUIDE_RELOAD, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 20:    // 디스펜서 X,Y축 니들 평탄 작업 위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.NeedleFlat) == true &&
                            ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.NeedleFlat) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.NeedleFlat) == false)
                                    ml.AddError(eErrorCode.MOTOR25_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.NeedleFlat) == false)
                                    ml.AddError(eErrorCode.MOTOR18_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // 디스펜서 X,Y축 니들 평탄 작업 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.DISPENSER_Y].IsMoveDone() == true)
                        {
                            SubNext(22);
                        }
                    }
                    break;

                case 22:    // 디스펜서 니들 평탄 측정 실린더 Down
                    {
                        cIO.SetOutput((int)eIO_O.DISP_NEEDLE_FLAT_DOWN, true);
                        cTimeOut.Restart();
                        SubNext(23);
                    }
                    break;

                case 23:    // 디스펜서 니들 평탄 실린더 Down 확인
                    {
                        if (cIO.GetInput((int)eIO_I.DISP_NEEDLE_FLAT_CHECK_DOWN, true) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(24);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ10_DISP_HEIGH_CHECK_CYLINDER_DOWN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 24:    // 디스펜서 니들 평탄 측정 실린더 Up
                    {
                        cIO.SetOutput((int)eIO_O.DISP_NEEDLE_FLAT_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(25);
                    }
                    break;

                case 25:    // 디스펜서 니들 평탄 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.DISP_NEEDLE_FLAT_CHECK_UP_ON, true) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(50);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ10_DISP_HEIGH_CHECK_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 50:    // 디스펜서 X,Y축 홀더 분주 위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.Dispensing, false) == true &&
                            ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.Dispensing, false) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(51);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.Dispensing) == false)
                                    ml.AddError(eErrorCode.MOTOR25_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.Dispensing) == false)
                                    ml.AddError(eErrorCode.MOTOR18_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 51:    // 디스펜서 X,Y축 홀더 분주 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.DISPENSER_Y].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(52);
                        }
                    }
                    break;

                case 52:    // 디스펜서 Z축 홀더 분주 위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.Dispensing) == true)
                        {
                            cTimeOut.Stop();
                            if (ml.cOptionData.bDryRunUse == false)
                            {
                                ml.Uz_IO_Module.ClearOutputTrigger((int)eIO_O.DISPENSER_VALVE_OPEN, 1);
                            }
                            SubNext(53);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR26_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 53:    // 디스펜서 Z축 홀더 분주 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].IsMoveDone() == true)
                        {
                            SubNext(60);
                        }
                    }
                    break;

                case 60:    // 펌프 밸브 Open
                    {
                        if (ml.cOptionData.bDryRunUse == false)
                        {
                            ml.Uz_IO_Module.StartOutputTrigger((int)eIO_O.DISPENSER_VALVE_OPEN, 1);
                        }
                        cDelay.Restart();
                        SubNext(61);
                    }
                    break;

                case 61:    // 펌프 밸브 딜레이
                    {
                        if (cDelay.ElapsedMilliseconds > ml.cSysOne.uiDispValveDelay * 1000)
                        {
                            cDelay.Stop();
                            SubNext(62);
                        }
                    }
                    break;

                case 62:    // 펌프 밸브 Close
                    {
                        if (ml.cOptionData.bDryRunUse == false)
                        {
                            ml.cSysOne.iDispWorkCount++; // 토출 횟수 증가
                        }
                        cTimeOut.Restart();
                        SubNext(63);
                    }
                    break;

                case 63:    // 디스펜서 Z축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(64);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR26_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 64:    // 디스펜서 Z축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(70);
                        }
                    }
                    break;

                case 70:    // 디스펜서 X,Y축 니들 평탄 작업 위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.NeedleFlat) == true &&
                            ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.NeedleFlat) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(71);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.NeedleFlat) == false)
                                    ml.AddError(eErrorCode.MOTOR25_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.NeedleFlat) == false)
                                    ml.AddError(eErrorCode.MOTOR18_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 71:    // 디스펜서 X,Y축 니들 평탄 작업 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.DISPENSER_Y].IsMoveDone() == true)
                        {
                            SubNext(72);
                        }
                    }
                    break;

                case 72:    // 디스펜서 니들 평탄 측정 실린더 Down
                    {
                        cIO.SetOutput((int)eIO_O.DISP_NEEDLE_FLAT_DOWN, true);
                        cTimeOut.Restart();
                        SubNext(73);
                    }
                    break;

                case 73:    // 디스펜서 니들 평탄 실린더 Down 확인
                    {
                        if (cIO.GetInput((int)eIO_I.DISP_NEEDLE_FLAT_CHECK_DOWN, true) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(74);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ10_DISP_HEIGH_CHECK_CYLINDER_DOWN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 74:    // 디스펜서 니들 평탄 측정 실린더 Up
                    {
                        cIO.SetOutput((int)eIO_O.DISP_NEEDLE_FLAT_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(75);
                    }
                    break;

                case 75:    // 디스펜서 니들 평탄 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.DISP_NEEDLE_FLAT_CHECK_UP_ON, true) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(100);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ10_DISP_HEIGH_CHECK_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 100:   // 홀더 디스펜싱 맵데이터 변경
                    {
                        if (bHolderFail == true)
                        {
                            MPC1_Dispensing.SetAllStatus(eStatus.NG);
                        }
                        else
                        {
                            MPC1_Dispensing.SetAllStatus(eStatus.DISPENSING);
                            Dispensing.SetAllStatus(eStatus.DISPENSER_CLEAN);
                        }

                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 디스펜서 안전위치 이동
        /// </summary>
        /// <returns></returns>
        public bool MoveSafeDispenser()
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
                        // 클린 블로우 Off
                        cIO.SetOutput((int)eIO_O.DISPENSER_CLEAN_BLOW, false);
                        SubNext(10);
                    }
                    break;

                case 10:    // 디스펜서 Z축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.Safe) == true)
                        {
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR26_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 디스펜서 Z축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // 디스펜서 높이 측정 실린더 Up
                    {
                        cIO.SetOutput((int)eIO_O.DISP_NEEDLE_FLAT_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(13);
                    }
                    break;

                case 13:    // 디스펜서 높이 측정 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.DISP_NEEDLE_FLAT_CHECK_UP_ON) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ10_DISP_HEIGH_CHECK_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // 디스펜서 X,Y축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.Safe) == true &&
                            ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.Safe) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.Safe) == false)
                                    ml.AddError(eErrorCode.MOTOR25_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.Safe) == false)
                                    ml.AddError(eErrorCode.MOTOR18_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // 디스펜서 X,Y축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.DISPENSER_Y].IsMoveDone() == true)
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

        /// <summary>
        /// 디스펜서 클린 작업
        /// </summary>
        /// <returns></returns>
        public bool DispenserClean()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib Dispensing = ml.cRunUnitData.GetIndexData(eData.DISPENSER);

            switch (iSubStep)
            {
                case 0:     // 디스펜서 클린 맵데이터 조건
                    {
                        bool bDispenserClean = Dispensing.GetStatus(eStatus.DISPENSER_CLEAN);

                        if (bDispenserClean == true)
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

                case 10:    // 디스펜서 Z축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            // 펌프 밸브 Close
                            cIO.SetOutput((int)eIO_O.DISPENSER_VALVE_OPEN, false);
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR26_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 디스펜서 Z축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // 디스펜서 높이 측정 실린더 Up
                    {
                        cIO.SetOutput((int)eIO_O.DISP_NEEDLE_FLAT_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(13);
                    }
                    break;

                case 13:    // 디스펜서 높이 측정 실린더 다운 Up
                    {
                        if (cIO.GetInput((int)eIO_I.DISP_NEEDLE_FLAT_CHECK_UP_ON) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ10_DISP_HEIGH_CHECK_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // 디스펜서 X,Y축 클린 위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.Clean) == true &&
                            ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.Clean) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.Clean) == false)
                                    ml.AddError(eErrorCode.MOTOR25_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.Clean) == false)
                                    ml.AddError(eErrorCode.MOTOR18_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // 디스펜서 X,Y축 클린 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.DISPENSER_Y].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(22);
                        }
                    }
                    break;

                case 22:    // 디스펜서 Z축 클린 위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.Clean) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(23);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR26_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 23:    // 디스펜서 Z축 클린 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].IsMoveDone() == true)
                        {
                            cIO.SetOutput((int)eIO_O.DISPENSER_CLEAN_BLOW, true);
                            cDelay.Restart();
                            SubNext(24);
                        }
                    }
                    break;

                case 24:    // 딜레이 후 Z축 Up
                    {
                        if (cDelay.ElapsedMilliseconds > ml.cSysOne.uiCleanDelay * 1000)
                        {
                            cDelay.Stop();
                            cIO.SetOutput((int)eIO_O.DISPENSER_CLEAN_BLOW, false);
                            cTimeOut.Restart();
                            SubNext(25);
                        }
                    }
                    break;

                case 25:    // 디스펜서 Z축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(26);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR26_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 26:    // 디스펜서 Z축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].IsMoveDone() == true)
                        {
                            SubNext(100);
                        }
                    }
                    break;

                case 100:   // 홀더 디스펜싱 맵데이터 변경
                    {
                        Dispensing.SetAllStatus(eStatus.STANBY);
                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 디스펜서 Z축 노즐 높이 측정
        /// </summary>
        /// <returns></returns>
        public bool DispenserZCal()
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
                        cSysArray.bDispenserZCalDone = false;
                        cTimeOut.Restart();
                        SubNext(10);
                    }
                    break;

                case 10:    // 디스펜서 Z축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            // 펌프 밸브 Close
                            cIO.SetOutput((int)eIO_O.DISPENSER_VALVE_OPEN, false);
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR26_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 디스펜서 Z축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // 홀더 니들 정렬 실린더 측정 실린더 Up
                    {
                        cIO.SetOutput((int)eIO_O.DISP_NEEDLE_FLAT_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(13);
                    }
                    break;

                case 13:    // 홀더 니들 정렬 실린더 측정 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.DISP_NEEDLE_FLAT_CHECK_UP_ON) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ10_DISP_HEIGH_CHECK_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // 디스펜서 X,Y축 Z축 높이측정 위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.NozzleHeigh) == true &&
                            ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.NozzleHeigh) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.NozzleHeigh) == false)
                                    ml.AddError(eErrorCode.MOTOR25_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.NozzleHeigh) == false)
                                    ml.AddError(eErrorCode.MOTOR18_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // 디스펜서 X,Y축 Z축 높이측정 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.DISPENSER_Y].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(22);
                        }
                    }
                    break;

                case 22:    // 디스펜서 Z축 높이측정 센서 위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.NozzleHeigh) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(23);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR26_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 23:    // 디스펜서 Z축 높이측정 센서 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(24);
                        }
                    }
                    break;

                case 24:    // 디스펜서 Z축 높이 측정 센서로 천천히 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.NozzleHeighSlow) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(30);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR26_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 30: // 높이 측정 센서가 감지되면 Z축을 멈춘다.
                    {
                        if (cIO.GetInput((int)eIO_I.DISP_NOZZLE_HEIGHT_CHECK_SENSOR) == true)
                        {
                            ml.Axis[eMotor.DISPENSER_Z].Stop();
                            SubNext(31);
                        }
                        else
                        {
                            // 센서가 감지되지 않으면 에러
                            if (ml.Axis[eMotor.DISPENSER_Z].IsMoveDone() == true)
                            {
                                ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.Safe);
                                ml.AddError(eErrorCode.SEQ10_DISP_NOZZLE_HEIGH_SENSOR_NOT_DETECTION, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 31:    // 높이 측정 센서에 감지된 Z축 포지션 저장
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].IsMoveDone() == true)
                        {
                            PositionData Dispensing_Z = ml.cAxisPosCollData.GetPositionData(eMotor.DISPENSER_Z, (int)eAxisDispenser_Z.Dispensing);
                            Dispensing_Z.dPos = ml.Axis[eMotor.DISPENSER_Z].GetCmdPostion();
                            CXMLProcess.WriteXml(CXMLProcess.AxisPositionCollectionDataFilePath, CMainLib.Ins.cAxisPosCollData);

                            cTimeOut.Restart();
                            SubNext(40);
                        }
                    }
                    break;

                case 40:    // 디스펜서 Z축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(41);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR26_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 41:    // 디스펜서 Z축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].IsMoveDone() == true)
                        {
                            SubNext(100);
                        }
                    }
                    break;

                case 100:   // Z축 높이 캘리브레이션 완료
                    {
                        cSysArray.bDispenserZCalDone = true;
                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 그냥 디스펜싱 홀더 촬영
        /// </summary>
        /// <returns></returns>
        public bool JustDispHolderShot()
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

                case 10:    // 디스펜서 Z축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.Safe) == true)
                        {
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR26_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 디스펜서 Z축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // 디스펜서 니들 평탄 측정 실린더 Up
                    {
                        cIO.SetOutput((int)eIO_O.DISP_NEEDLE_FLAT_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(13);
                    }
                    break;

                case 13:    // 디스펜서 니들 평탄 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.DISP_NEEDLE_FLAT_CHECK_UP_ON) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ10_DISP_HEIGH_CHECK_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // 디스펜서 X,Y축 홀더 비전 위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.HolderVision) == true &&
                            ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.HolderVision) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.HolderVision) == false)
                                    ml.AddError(eErrorCode.MOTOR25_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.HolderVision) == false)
                                    ml.AddError(eErrorCode.MOTOR18_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // 디스펜서 X,Y축 홀더 비전 촬영 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.DISPENSER_Y].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(22);
                        }
                    }
                    break;

                case 22:    // 디스펜서 Z축 홀더 비전 촬영 위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.HolderVision) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(23);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR26_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 23:    // 디스펜서 Z축 홀더 비전 촬영 위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].IsMoveDone() == true)
                        {
                            cDelay.Restart();
                            SubNext(30);
                        }
                    }
                    break;

                case 30:    // 디스펜서 홀더 카메라 촬영 시작
                    {
                        if (cDelay.ElapsedMilliseconds > 100)
                        {
                            cDelay.Stop();
                            if (ml.cOptionData.bDryRunUse == false)
                                ml.cCVisionToolBlockLib[5].cAcqFifoTool.Run();
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
                        if (ml.cVar.Manual_HolderDispensing == true)
                        {
                            Next(10);
                            ml.cVar.Manual_HolderDispensing = false;
                        }
                        else if (ml.cVar.Manual_DispenserClean == true)
                        {
                            Next(20);
                            ml.cVar.Manual_DispenserClean = false;
                        }
                        else if (ml.cVar.Manual_DispenserSafe == true)
                        {
                            Next(30);
                            ml.cVar.Manual_DispenserSafe = false;
                        }
                        else if (ml.cVar.Manual_DispenserZCal == true)
                        {
                            Next(40);
                            ml.cVar.Manual_DispenserZCal = false;
                        }
                        else if (ml.cVar.Manual_JustDispHolderShot == true)
                        {
                            Next(50);
                            ml.cVar.Manual_JustDispHolderShot = false;
                        }
                        else if (ml.cVar.Manual_DispensingNotInsp == true)
                        {
                            Next(60);
                            ml.cVar.Manual_DispensingNotInsp = false;
                        }
                    }
                    break;

                case 10:    // 홀디 디스펜싱 함수
                    {
                        if (HolderDispensing() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 20:    // 디스펜서 노즐 Clean 함수
                    {
                        if (DispenserClean() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 30:    // 디스펜서 안전위치 이동 함수
                    {
                        if (MoveSafeDispenser() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 40:    // 디스펜서 Z 캘리브레이션 함수
                    {
                        if (DispenserZCal() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 50:    // 그냥 디스펜서 홀더 촬영
                    {
                        if (JustDispHolderShot() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 60:    // 매뉴얼 검사 없는 디스펜싱
                    {
                        if (HolderDispensingSkipVision() == true)
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