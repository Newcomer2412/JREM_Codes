using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// 시퀀스 클래스
    /// </summary>
    public class Seq08_PipeMount : ISequence, ISeqNo
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
        public Seq08_PipeMount(eSequence sequence)
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
            CAM3_ResultData = CMainLib.Ins.cVar.GetVisionResultData(3, 0);

            // 로그 설정
            eLogType = eLogType.Seq08_PipeMount;

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
        private CVisionResultData CAM3_ResultData;

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
            ml.cVar.PipeMountCycleTime.Start();
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
            ml.cVar.PipeMountCycleTime.Stop();
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
                ml.AddError(eErrorCode.SEQ08_TIME_OUT);
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
                        MapDataLib PipeTransfer = ml.cRunUnitData.GetIndexData(eData.PIPE_TRANSFER);
                        MapDataLib PipeMounter = ml.cRunUnitData.GetIndexData(eData.PIPE_MOUNTER);
                        MapDataLib NPC1_PipeMount = ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT);

                        if (PipeTransfer != null && PipeMounter != null && NPC1_PipeMount != null)
                        {
                            // 양품 파이프 픽업 맵데이터 조건
                            bool bGoodPipePickUpMapData = PipeTransfer.GetStatus(eStatus.WORK_DONE) &&
                                                          PipeMounter.GetStatus(eStatus.EMPTY);

                            // 파이프 홀더에 마운트 맵데이터 조건
                            bool bPipeMountMapData = PipeMounter.GetStatus(eStatus.MOUNT) &&
                                                     NPC1_PipeMount.GetStatus(eStatus.HOLDER);

                            bool bAllDonePipeMount = NPC1_PipeMount.GetAllStatus(eStatus.PIPE_MOUNT, eStatus.SKIPPED, false);

                            if (bGoodPipePickUpMapData == true)
                            {
                                Next(10);
                            }
                            else if (bPipeMountMapData == true)
                            {
                                Next(20);
                            }
                            else if (bAllDonePipeMount == true)
                            {
                                Next(30);
                            }
                        }
                    }
                    break;

                case 10:    // 양품 파이프 픽업 함수
                    {
                        if (PipeClampUp() == true)
                        {
                            Next(0);
                        }
                    }
                    break;

                case 20:    // 파이프 홀더에 마운트 함수
                    {
                        if (PipeMount() == true)
                        {
                            Next(0);
                        }
                    }
                    break;

                case 30:    // 파이프 홀더 마그넷 작업 함수
                    {
                        if (MagneticInjection() == true)
                        {
                            Next(0);
                        }
                    }
                    break;

                case 40:    // 안전위치 대기 함수
                    {
                        if (MoveSafePipeMounter() == true)
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
        /// 파이프 & 니들 픽업 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool PipeClampUp()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib PipeTransfer = ml.cRunUnitData.GetIndexData(eData.PIPE_TRANSFER);
            MapDataLib PipeMounter = ml.cRunUnitData.GetIndexData(eData.PIPE_MOUNTER);

            switch (iSubStep)
            {
                case 0:     // 양품 파이프 픽업 맵데이터 조건
                    {
                        bool bGoodPipePickUpMapData = PipeTransfer.GetStatus(eStatus.WORK_DONE) &&
                                                      PipeMounter.GetStatus(eStatus.EMPTY);

                        if (bGoodPipePickUpMapData == true)
                        {
                            cTimeOut.Restart();
                            SubNext(1);
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

                case 1:     // 파이프 푸셔도 같이 진행해야 되는지 확인한다.
                    {
                        if (ml.cVar.bHolderPipePushBegin == true)
                        {
                            SubNext(21);
                        }
                        else
                        {
                            SubNext(10);
                        }
                    }
                    break;

                case 10:    // 파이프 마운트 Z축 안전 위치로 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].MoveAbsolute((int)eAxisPipeMount_Z.Safe) == true)
                        {
                            cTimeOut.Restart();
                            // 파이프 척 실린더 오픈
                            cIO.SetOutput((int)eIO_O.PIPE_CHUCK_OPEN, true);
                            // Push 실린더 Up
                            cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, false);
                            // 전자석 Off
                            cIO.SetOutput((int)eIO_O.PIPE_HOLDER_ELECTROMAGNET, false);
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR14_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 파이프 마운트 Z축 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // 파이프 Chuck이 오픈되었는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_CHUCK_OPEN) == true)
                        {
                            SubNext(13);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_PIPE_MOUNT_CHUCK_CYLINDER_OPEN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 13:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_PUSHER_UP_ON) == true)
                        {
                            SubNext(14);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_HOLDER_PIPE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 14:    // 파이프 트랜스퍼가 FWD 위치인지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.TRANSFER_PIPE_FORWARD) == true)
                        {
                            SubNext(15);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                ml.AddError(eErrorCode.SEQ08_TRANSFER_LEFT_CYLINDER_FWD_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 15:    // 파이프 트랜스퍼가 파이프를 잡고있는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.TRANSFER_PIPE_VACUUM) == true ||
                            Define.SIMULATION == true ||
                            ml.cOptionData.bDryRunUse == true)
                        {
                            SubNext(16);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ08_TRANSFER_LEFT_PIPE_NOT_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 16:    // 파이프 트렌스퍼 R축이 현재 Posture 위치인지 확인
                    {
                        double dGetCmdTransfer_R = ml.Axis[eMotor.PIPE_ROTATE].GetCmdPostion();
                        double dTransfer_R_Posture = ml.cAxisPosCollData.GetAxisPosition(eMotor.PIPE_ROTATE, (int)eAxisPipeRotate.Rotate90);
                        if (dGetCmdTransfer_R == dTransfer_R_Posture)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ08_TRANSFER_LEFT_MOTOR_R_NOT_POSTURE_POS, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 20:    // X, Y축 파이프 클램프 할 위치로 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_X].MoveAbsolute((int)eAxisPipeMount_X.PipeClampUp) == true &&
                            ml.Axis[eMotor.PIPE_MOUNT_Y].MoveAbsolute((int)eAxisPipeMount_Y.PipeClampUp) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.PIPE_MOUNT_X].MoveAbsolute((int)eAxisPipeMount_X.PipeClampUp) == false)
                                    ml.AddError(eErrorCode.MOTOR12_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.PIPE_MOUNT_Y].MoveAbsolute((int)eAxisPipeMount_Y.PipeClampUp) == false)
                                    ml.AddError(eErrorCode.MOTOR13_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // X, Y축 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.PIPE_MOUNT_Y].IsMoveDone() == true)
                        {
                            // 파이프 푸쉬할 실린더가 있으면 Push Down 진행
                            if (ml.cVar.bHolderPipePushBegin == true)
                            {
                                cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, true);
                            }

                            cTimeOut.Restart();
                            SubNext(22);
                        }
                    }
                    break;

                case 22:    // Z축 클램프 할 위치로 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].MoveAbsolute((int)eAxisPipeMount_Z.PipeClampUp) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(23);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR14_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 23:    // Z축 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].IsMoveDone() == true)
                        {
                            cDelay.Restart();
                            SubNext(24);
                        }
                    }
                    break;

                case 24:
                    {
                        if (cDelay.ElapsedMilliseconds > 100)
                        {
                            cDelay.Stop();
                            SubNext(30);
                        }
                    }
                    break;

                case 30:    // 파이프 Chuck 클로즈
                    {
                        cIO.SetOutput((int)eIO_O.PIPE_CHUCK_OPEN, false);
                        cTimeOut.Restart();
                        SubNext(31);
                    }
                    break;

                case 31:    // 파이프 Chuck 클로즈 센서 화인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_CHUCK_CLOSE_ON, true) == true)
                        {
                            cTimeOut.Stop();
                            cDelay.Restart();
                            SubNext(32);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_PIPE_MOUNT_CHUCK_CYLINDER_CLOSE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 32:    // 파이프 Chuck 딜레이
                    {
                        if (cDelay.ElapsedMilliseconds > cSysArray.uiPipeClampUpDelay)
                        {
                            cDelay.Stop();
                            if (ml.cVar.bHolderPipePushBegin == true)
                            {
                                // 파이프 푸셔 실린더 Up
                                cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, false);
                            }
                            // 트랜스 진공 Off
                            cIO.SetOutput((int)eIO_O.TRANSFER_PIPE_VACUUM, false);
                            cTimeOut.Restart();
                            SubNext(33);
                        }
                    }
                    break;

                case 33:    // 클램프가 완료되면 Z축 Up
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].MoveAbsolute((int)eAxisPipeMount_Z.Safe) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(34);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR14_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 34:
                    {
                        if (ml.cVar.bHolderPipePushBegin == true)
                        {
                            if (cIO.GetInput((int)eIO_I.PIPE_PUSHER_UP_ON) == true)
                            {
                                ml.cVar.iPipeCycleTime = (int)ml.cVar.PipeMountCycleTime.ElapsedMilliseconds;
                                ml.cVar.PipeMountCycleTime.Restart();

                                cTimeOut.Stop();
                                SubNext(35);
                            }
                            else
                            {
                                if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                                {
                                    cTimeOut.Stop();
                                    ml.cVar.bHolderPipePushBegin = false;
                                    for (int i = 0; i < ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT).iMaxXindex; i++)
                                    {
                                        if (ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT).GetUnitNo(i).eStatus == eStatus.HOLDER)
                                        {
                                            ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT).GetUnitNo(i).eStatus = eStatus.PIPE_MOUNT;
                                            break;
                                        }
                                    }
                                    ml.AddError(eErrorCode.SEQ08_HOLDER_PIPE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            SubNext(35);
                        }
                    }
                    break;

                case 35:    // Z축 Up 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Stop();
                            if (ml.cVar.bHolderPipePushBegin == true)
                            {
                                ml.cVar.bHolderPipePushBegin = false;
                                for (int i = 0; i < ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT).iMaxXindex; i++)
                                {
                                    if (ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT).GetUnitNo(i).eStatus == eStatus.HOLDER)
                                    {
                                        ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT).GetUnitNo(i).eStatus = eStatus.PIPE_MOUNT;
                                        break;
                                    }
                                }
                            }

                            SubNext(100);
                        }
                    }
                    break;

                case 100:   // 파이프 마운터 맵데이터 변경
                    {
                        PipeTransfer.SetAllStatus(eStatus.EMPTY);
                        PipeMounter.SetAllStatus(eStatus.MOUNT);

                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 파이프 & 니들 트렌스퍼 플레이스 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool PipeMount()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib PipeMounter = ml.cRunUnitData.GetIndexData(eData.PIPE_MOUNTER);
            MapDataLib NPC1_PipeMount = ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT);

            switch (iSubStep)
            {
                case 0:     // 파이프 홀더에 마운트 맵데이터 조건
                    {
                        bool bPipeMountMapData = PipeMounter.GetStatus(eStatus.MOUNT) &&
                                                 NPC1_PipeMount.GetStatus(eStatus.HOLDER);

                        if (bPipeMountMapData == true)
                        {
                            ml.cVar.bPipeMountInspSkip = false;
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

                case 10:    // 파이프 마운트 Z축 안전 위치로 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].MoveAbsolute((int)eAxisPipeMount_Z.Safe) == true)
                        {
                            // Push 실린더 Up
                            cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, false);
                            // 홀더 Fix 실린더 Down
                            cIO.SetOutput((int)eIO_O.FIXTURE_DOWN, true);
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR14_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 파이프 마운트 Z축 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_PUSHER_UP_ON) == true)
                        {
                            SubNext(13);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_HOLDER_PIPE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 13:    // 홀더 Fix 실린더 다운 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_HOLDER_FIXTURE_DOWN) == true &&
                            cIO.GetInput((int)eIO_I.NEEDLE_HOLDER_FIXTURE_DOWN) == true ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_HOLDER_FIX_CYLINDER_DOWN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // Y축 홀더의 파이프 홀 촬영위치로 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Y].MoveAbsolute((int)eAxisPipeMount_Y.PipeMountVision) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            cTimeOut.Stop();
                            ml.AddError(eErrorCode.MOTOR13_MOVE_TIMEOUT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 21:    // Y축 홀더 촬영위치 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Y].IsMoveDone() == true)
                        {
                            cDelay.Restart();
                            SubNext(22);
                        }
                    }
                    break;

                case 22:    // 홀더 Vision 촬영 시작
                    {
                        if (cDelay.ElapsedMilliseconds > 100)
                        {
                            cDelay.Stop();
                            if (ml.cOptionData.bDryRunUse == true ||
                                Define.SIMULATION == true)
                            {
                                SubNext(30);
                            }
                            else
                            {
                                CAM3_ResultData.DataClear();
                                ml.cCVisionToolBlockLib[3].VisionShoot(0);
                                cTimeOut.Restart();
                                SubNext(23);
                            }
                        }
                    }
                    break;

                case 23:    // 촬영 완료
                    {
                        if (CAM3_ResultData.bShootFinish == true)
                        {
                            if (CAM3_ResultData.bHolderEmpty == true)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_PIPE_MOUNT_HOLDER_EMPTY, iSubStep);
                                return true;
                            }

                            //// 이전 파이프 이중삽입 의심
                            //if (CAM3_ResultData.bPipeDoubleInsertSuspection == true)
                            //{
                            //    cTimeOut.Stop();
                            //    int iSkipUnitNo = NPC1_PipeMount.GetUnitMin(eStatus.HOLDER).iUnitNo - 1;
                            //    NPC1_PipeMount.GetUnitNo(iSkipUnitNo).eStatus = eStatus.SKIPPED;
                            //}

                            // 파이프 삽입 실패
                            if (CAM3_ResultData.bPipeInserFail == true)
                            {
                                cTimeOut.Stop();
                                int Er = NPC1_PipeMount.GetUnitMin(eStatus.HOLDER).iUnitNo - 1;
                                ml.AddError(eErrorCode.PIPE_1_INSERTION_FAIL + Er, iSubStep);
                                return true;
                            }
                            // 파이프 홀 찾기 실패
                            else if (CAM3_ResultData.bPipeHoleSearchFail == true)
                            {
                                cTimeOut.Stop();
                                NPC1_PipeMount.GetUnitMin(eStatus.HOLDER).eStatus = eStatus.SKIPPED;

                                ml.cVar.iPipeCycleTime = (int)ml.cVar.PipeMountCycleTime.ElapsedMilliseconds;
                                ml.cVar.PipeMountCycleTime.Restart();

                                SubNext(0);
                                return true;
                            }
                            else
                            {
                                // 홀더에 파이프를 꽂을 구멍이 남아있는지 화인
                                if (CAM3_ResultData.bGoodNg == true)
                                {
                                    cTimeOut.Restart();
                                    SubNext(30);
                                }
                                else
                                {
                                    ml.AddError(eErrorCode.SEQ08_PIPE_MOUNT_NOT_FIND_HOLE, iSubStep);
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > 3000)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_PIPE_MOUNT_VISION_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 30:    // X, Y축 파이프 마운트 보정 좌표이동
                    {
                        double dPipe_X_PipeMount = ml.cAxisPosCollData.GetAxisPosition(eMotor.PIPE_MOUNT_X, (int)eAxisPipeMount_X.PipeMountCenter);
                        double dPipe_Y_PipeMount = ml.cAxisPosCollData.GetAxisPosition(eMotor.PIPE_MOUNT_Y, (int)eAxisPipeMount_Y.PipeMountCenter);
                        double dMovePipeMount_X = dPipe_X_PipeMount + CAM3_ResultData.dPipeMountX;
                        double dMovePipeMount_Y = dPipe_Y_PipeMount + CAM3_ResultData.dPipeMountY;

                        if (ml.Axis[eMotor.PIPE_MOUNT_X].MoveAbsolute(dMovePipeMount_X) == true &&  // X축은 절대이동
                            ml.Axis[eMotor.PIPE_MOUNT_Y].MoveAbsolute(dMovePipeMount_Y) == true)    // Y축은 상대이동
                        {
                            NLogger.AddLog(eLogType.Seq09_NeedleMount, NLogger.eLogLevel.INFO, $"Move Pipe Mount : X:{dMovePipeMount_X} Y:{dMovePipeMount_Y}");
                            cTimeOut.Stop();
                            SubNext(31);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.PIPE_MOUNT_X].MoveAbsolute(dMovePipeMount_X) == false)
                                    ml.AddError(eErrorCode.MOTOR12_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.PIPE_MOUNT_Y].MoveAbsolute(dMovePipeMount_Y) == true)
                                    ml.AddError(eErrorCode.MOTOR13_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 31:    // 파이프 X, Y축 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.PIPE_MOUNT_Y].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(32);
                        }
                    }
                    break;

                case 32:    // 파이프 마운트 하러 Z축 다운
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].MoveAbsolute((int)eAxisPipeMount_Z.PipeMountHolder) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(33);
                        }
                        else
                        {
                            cTimeOut.Stop();
                            ml.AddError(eErrorCode.MOTOR14_MOVE_TIMEOUT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 33:    // Z축 Down 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(34);
                        }
                    }
                    break;

                case 34:    // Z축 다운 후 파이프를 마운트하러 천천히 내려간다.
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].MoveAbsolute((int)eAxisPipeMount_Z.MountSlow) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(35);
                        }
                        else
                        {
                            cTimeOut.Stop();
                            ml.AddError(eErrorCode.MOTOR14_MOVE_TIMEOUT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 35:    // 파이프 마운트 Down 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].IsMoveDone() == true)
                        {
                            SubNext(40);
                        }
                    }
                    break;

                case 40:    // 파이프 Chuck 실린더 Open
                    {
                        cIO.SetOutput((int)eIO_O.PIPE_CHUCK_OPEN, true);
                        cTimeOut.Restart();
                        SubNext(41);
                    }
                    break;

                case 41:
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_CHUCK_OPEN) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(42);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_PIPE_MOUNT_CHUCK_CYLINDER_OPEN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 42:    // 파이프 마운트 Z축 안전 위치로 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].MoveAbsolute((int)eAxisPipeMount_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(43);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR14_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 43:    // 파이프 마운트 Z축 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(50);
                        }
                    }
                    break;

                case 50:    // X, Y축 홀더에 꽂힌 파이프를 Push하러 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_X].MoveAbsolute((int)eAxisPipeMount_X.PipeClampUp) == true &&
                            ml.Axis[eMotor.PIPE_MOUNT_Y].MoveAbsolute((int)eAxisPipeMount_Y.PipeClampUp) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(51);
                        }
                        else
                        {
                            cTimeOut.Stop();
                            if (ml.Axis[eMotor.PIPE_MOUNT_X].MoveAbsolute((int)eAxisPipeMount_X.PipeClampUp) == false)
                                ml.AddError(eErrorCode.MOTOR12_MOVE_TIMEOUT, iSubStep);
                            else if (ml.Axis[eMotor.PIPE_MOUNT_Y].MoveAbsolute((int)eAxisPipeMount_Y.PipeClampUp) == false)
                                ml.AddError(eErrorCode.MOTOR13_MOVE_TIMEOUT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 51:    // 파이프 마운트 X, Y축 홀더 Push 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.PIPE_MOUNT_Y].IsMoveDone() == true)
                        {
                            // 마지막 파이프 작업일 때, 비전을 한번더 촬영해야 하기 때문에,
                            // 파이프 푸셔를 진행하고 파이프 삽입 유무를 확인한다.

                            // 파이프 트랜스퍼에 클램프 할 파이프가 있다면, 클램프 할때 동시에 꽂힌 파이프 푸쉬하도록 진행
                            if (ml.cRunUnitData.GetIndexData(eData.PIPE_TRANSFER).GetStatus(eStatus.WORK_DONE) == true &&
                                NPC1_PipeMount.GetUnitMin(eStatus.HOLDER).iUnitNo < 18)
                            {
                                ml.cVar.bHolderPipePushBegin = true;
                                SubNext(99);
                            }
                            else
                            {
                                SubNext(52);
                            }
                        }
                    }
                    break;

                case 52:    // Push 실린더 Down
                    {
                        cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, true);
                        cTimeOut.Restart();
                        SubNext(53);
                    }
                    break;

                case 53:    // Push 실린더 Down 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_PUSHER_DOWN) == true ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Stop();
                            cDelay.Restart();
                            SubNext(54);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, false);
                                ml.AddError(eErrorCode.SEQ08_HOLDER_PIPE_PUSHER_CYLINDER_DOWN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 54:    // Push 실린더 Down 딜레이
                    {
                        if (cDelay.ElapsedMilliseconds > 200)
                        {
                            cDelay.Stop();
                            SubNext(55);
                        }
                    }
                    break;

                case 55:    // Push 실린더 Up
                    {
                        cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(56);
                    }
                    break;

                case 56:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_PUSHER_UP_ON) == true ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Stop();
                            if (NPC1_PipeMount.GetStatusCount(eStatus.HOLDER) > 1)
                            {
                                SubNext(100);
                            }
                            else
                            {
                                SubNext(60);
                            }
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_HOLDER_PIPE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 60:    // 파이프 마운터 X,Y축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Y].MoveAbsolute((int)eAxisPipeMount_Y.PipeMountVision) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(61);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR13_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 61:
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Y].IsMoveDone() == true)
                        {
                            cDelay.Restart();
                            SubNext(62);
                        }
                    }
                    break;

                case 62:    // 홀더 Vision 촬영 시작
                    {
                        if (cDelay.ElapsedMilliseconds > 100)
                        {
                            cDelay.Stop();
                            if (ml.cOptionData.bDryRunUse == true ||
                                Define.SIMULATION == true)
                            {
                                SubNext(100);
                            }
                            else
                            {
                                CAM3_ResultData.DataClear();
                                CAM3_ResultData.bLastHoleInsp = true;
                                ml.cCVisionToolBlockLib[3].VisionShoot(0);
                                cTimeOut.Restart();
                                SubNext(63);
                            }
                        }
                    }
                    break;

                case 63:    // 촬영 완료
                    {
                        if (CAM3_ResultData.bShootFinish == true)
                        {
                            if (CAM3_ResultData.bHolderEmpty == true)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_PIPE_MOUNT_HOLDER_EMPTY, iSubStep);
                                return true;
                            }
                            else if (CAM3_ResultData.bPipeInserFail == true)
                            {
                                cTimeOut.Stop();
                                int Er = NPC1_PipeMount.GetUnitMin(eStatus.HOLDER).iUnitNo;
                                ml.AddError(eErrorCode.PIPE_1_INSERTION_FAIL + 18, iSubStep);
                                return true;
                            }
                            else if (CAM3_ResultData.bPipeDoubleInsertSuspection == true)
                            {
                                cTimeOut.Stop();
                                Next(98);
                            }
                            else
                            {
                                // 19번째 파이프도 삽입이 완료되었으면 시퀀스 종료
                                if (CAM3_ResultData.bGoodNg == true)
                                {
                                    cTimeOut.Restart();
                                    SubNext(100);
                                }
                                else
                                {
                                    ml.AddError(eErrorCode.SEQ08_PIPE_MOUNT_NOT_FIND_HOLE, iSubStep);
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > 3000)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_PIPE_MOUNT_VISION_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 97:    // 파이프 이중삽입 의심, 파이프 홀더 맵데이터 스킵으로 변경
                    {
                    }
                    break;

                case 98:    // 파이프 홀 인식 실패, 파이프 홀더 맵데이터 스킵으로 변경
                    {
                    }
                    break;

                case 99:    // 파이프 푸셔를 하지 않았을 때는 파이프 마운터 맵데이터만 변경한다.
                    {
                        PipeMounter.SetAllStatus(eStatus.EMPTY);

                        ml.cVar.iPipeCycleTime = (int)ml.cVar.PipeMountCycleTime.ElapsedMilliseconds;
                        ml.cVar.PipeMountCycleTime.Restart();

                        SubNext(0);
                        return true;
                    }
                    break;

                case 100:   // 파이프 마운터 맵데이터 변경
                    {
                        NPC1_PipeMount.GetUnitMin(eStatus.HOLDER).eStatus = eStatus.PIPE_MOUNT;
                        PipeMounter.SetAllStatus(eStatus.EMPTY);

                        ml.cVar.iPipeCycleTime = (int)ml.cVar.PipeMountCycleTime.ElapsedMilliseconds;
                        ml.cVar.PipeMountCycleTime.Restart();

                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 파이프 홀더에 자기장 형성
        /// </summary>
        /// <returns></returns>
        public bool MagneticInjection()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib PipeMounter = ml.cRunUnitData.GetIndexData(eData.PIPE_MOUNTER);
            MapDataLib NPC1_PipeMount = ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT);

            switch (iSubStep)
            {
                case 0:     // 파이프 홀더 마그넷 작업 맵데이터 조건
                    {
                        bool bAllDonePipeMount = NPC1_PipeMount.GetAllStatus(eStatus.PIPE_MOUNT, eStatus.SKIPPED, false);

                        if (bAllDonePipeMount == true)
                        {
                            cTimeOut.Restart();
                            SubNext(10);
                        }
                    }
                    break;

                case 10:    // 파이프 마운터 Z축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].MoveAbsolute((int)eAxisPipeMount_Z.Safe) == true)
                        {
                            // Push 실린더 Up
                            cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, false);
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                ml.AddError(eErrorCode.MOTOR14_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 파이프 마운터 Z축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_PUSHER_UP_ON) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_HOLDER_PIPE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // 파이프 마운터 Y축 자기장 주입 위치 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Y].MoveAbsolute((int)eAxisPipeMount_Y.MagneticInjection) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR13_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Y].IsMoveDone() == true)
                        {
                            SubNext(30);
                        }
                    }
                    break;

                case 30:    // 푸쉬 실린더 Down 및 전자석 On
                    {
                        cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, true);
                        cIO.SetOutput((int)eIO_O.PIPE_HOLDER_ELECTROMAGNET, true);
                        cTimeOut.Restart();
                        SubNext(31);
                    }
                    break;

                case 31:    // 파이프 푸쉬 실린더 Down 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_PUSHER_DOWN) == true)
                        {
                            cTimeOut.Stop();
                            cDelay.Restart();
                            SubNext(32);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, false);
                                ml.AddError(eErrorCode.SEQ08_HOLDER_PIPE_PUSHER_CYLINDER_DOWN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 32:    // 전자석 딜레이 후 푸쉬 실린더 Up 진행
                    {
                        if (cDelay.ElapsedMilliseconds > ml.cSysOne.uiMagneticDelay)
                        {
                            cDelay.Stop();
                            // Push 실린더 Up
                            cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, false);
                            // 전자석 Off
                            cIO.SetOutput((int)eIO_O.PIPE_HOLDER_ELECTROMAGNET, false);
                            SubNext(33);
                        }
                    }
                    break;

                case 33:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_PUSHER_UP_ON) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(100);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_HOLDER_PIPE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 100:
                    {
                        for (int i = 0; i < 19; i++)
                        {
                            if (NPC1_PipeMount.GetUnitNo(i).eStatus == eStatus.PIPE_MOUNT)
                            {
                                NPC1_PipeMount.GetUnitNo(i).eStatus = eStatus.PIPE_MAGNETIC;
                            }
                        }

                        //NPC1_PipeMount.SetAllStatus(eStatus.PIPE_MAGNETIC);
                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 이전 파이프 검사 안하는 파이프 마운트 함수
        /// </summary>
        /// <returns></returns>
        public bool PipeMountNotInsp()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib PipeMounter = ml.cRunUnitData.GetIndexData(eData.PIPE_MOUNTER);
            MapDataLib NPC1_PipeMount = ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT);

            switch (iSubStep)
            {
                case 0:     // 파이프 홀더에 마운트 맵데이터 조건
                    {
                        bool bPipeMountMapData = PipeMounter.GetStatus(eStatus.MOUNT) &&
                                                 NPC1_PipeMount.GetStatus(eStatus.HOLDER);

                        if (bPipeMountMapData == true)
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

                case 10:    // 파이프 마운트 Z축 안전 위치로 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].MoveAbsolute((int)eAxisPipeMount_Z.Safe) == true)
                        {
                            // Push 실린더 Up
                            cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, false);
                            // 홀더 Fix 실린더 Down
                            cIO.SetOutput((int)eIO_O.FIXTURE_DOWN, true);
                            // 전자석 Off
                            cIO.SetOutput((int)eIO_O.PIPE_HOLDER_ELECTROMAGNET, false);
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR14_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 파이프 마운트 Z축 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_PUSHER_UP_ON) == true)
                        {
                            SubNext(13);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_HOLDER_PIPE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 13:    // 홀더 Fix 실린더 다운 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_HOLDER_FIXTURE_DOWN) == true &&
                            cIO.GetInput((int)eIO_I.NEEDLE_HOLDER_FIXTURE_DOWN) == true ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_HOLDER_FIX_CYLINDER_DOWN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // Y축 홀더의 파이프 홀 촬영위치로 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Y].MoveAbsolute((int)eAxisPipeMount_Y.PipeMountVision) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            cTimeOut.Stop();
                            ml.AddError(eErrorCode.MOTOR13_MOVE_TIMEOUT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 21:    // Y축 홀더 촬영위치 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Y].IsMoveDone() == true)
                        {
                            ml.cVar.bPipeMountInspSkip = true;
                            cDelay.Restart();
                            SubNext(22);
                        }
                    }
                    break;

                case 22:    // 홀더 Vision 촬영 시작
                    {
                        if (cDelay.ElapsedMilliseconds > 100)
                        {
                            cDelay.Stop();
                            if (ml.cOptionData.bDryRunUse == true ||
                                Define.SIMULATION == true)
                            {
                                SubNext(30);
                            }
                            else
                            {
                                CAM3_ResultData.DataClear();
                                ml.cCVisionToolBlockLib[3].VisionShoot(0);
                                cTimeOut.Restart();
                                SubNext(23);
                            }
                        }
                    }
                    break;

                case 23:    // 촬영 완료
                    {
                        if (CAM3_ResultData.bShootFinish == true)
                        {
                            if (CAM3_ResultData.bHolderEmpty == true)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_PIPE_MOUNT_HOLDER_EMPTY, iSubStep);
                                return true;
                            }

                            // 파이프 홀 찾기 실패
                            if (CAM3_ResultData.bPipeHoleSearchFail == true)
                            {
                                cTimeOut.Stop();
                                NPC1_PipeMount.GetUnitMin(eStatus.HOLDER).eStatus = eStatus.SKIPPED;

                                ml.cVar.iPipeCycleTime = (int)ml.cVar.PipeMountCycleTime.ElapsedMilliseconds;
                                ml.cVar.PipeMountCycleTime.Restart();

                                SubNext(0);
                                return true;
                            }
                            else
                            {
                                // 홀더에 파이프를 꽂을 구멍이 남아있는지 화인
                                if (CAM3_ResultData.bGoodNg == true)
                                {
                                    cTimeOut.Restart();
                                    SubNext(30);
                                }
                                else
                                {
                                    ml.AddError(eErrorCode.SEQ08_PIPE_MOUNT_NOT_FIND_HOLE, iSubStep);
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > 3000)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_PIPE_MOUNT_VISION_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 30:    // X, Y축 파이프 마운트 보정 좌표이동
                    {
                        ml.cVar.bPipeMountInspSkip = false;
                        double dPipe_X_PipeMount = ml.cAxisPosCollData.GetAxisPosition(eMotor.PIPE_MOUNT_X, (int)eAxisPipeMount_X.PipeMountCenter);
                        double dPipe_Y_PipeMount = ml.cAxisPosCollData.GetAxisPosition(eMotor.PIPE_MOUNT_Y, (int)eAxisPipeMount_Y.PipeMountCenter);
                        double dMovePipeMount_X = dPipe_X_PipeMount + CAM3_ResultData.dPipeMountX;
                        double dMovePipeMount_Y = dPipe_Y_PipeMount + CAM3_ResultData.dPipeMountY;

                        if (ml.Axis[eMotor.PIPE_MOUNT_X].MoveAbsolute(dMovePipeMount_X) == true &&  // X축은 절대이동
                            ml.Axis[eMotor.PIPE_MOUNT_Y].MoveAbsolute(dMovePipeMount_Y) == true)    // Y축은 상대이동
                        {
                            NLogger.AddLog(eLogType.Seq09_NeedleMount, NLogger.eLogLevel.INFO, $"Move Pipe Mount : X:{dMovePipeMount_X} Y:{dMovePipeMount_Y}");
                            cTimeOut.Stop();
                            SubNext(31);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.PIPE_MOUNT_X].MoveAbsolute(dMovePipeMount_X) == false)
                                    ml.AddError(eErrorCode.MOTOR12_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.PIPE_MOUNT_Y].MoveAbsolute(dMovePipeMount_Y) == true)
                                    ml.AddError(eErrorCode.MOTOR13_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 31:    // 파이프 X, Y축 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.PIPE_MOUNT_Y].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(32);
                        }
                    }
                    break;

                case 32:    // 파이프 마운트 하러 Z축 다운
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].MoveAbsolute((int)eAxisPipeMount_Z.PipeMountHolder) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(33);
                        }
                        else
                        {
                            cTimeOut.Stop();
                            ml.AddError(eErrorCode.MOTOR14_MOVE_TIMEOUT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 33:    // Z축 Down 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(34);
                        }
                    }
                    break;

                case 34:    // Z축 다운 후 파이프를 마운트하러 천천히 내려간다.
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].MoveAbsolute((int)eAxisPipeMount_Z.MountSlow) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(35);
                        }
                        else
                        {
                            cTimeOut.Stop();
                            ml.AddError(eErrorCode.MOTOR14_MOVE_TIMEOUT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 35:    // 파이프 마운트 Down 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].IsMoveDone() == true)
                        {
                            SubNext(40);
                        }
                    }
                    break;

                case 40:    // 파이프 Chuck 실린더 Open
                    {
                        cIO.SetOutput((int)eIO_O.PIPE_CHUCK_OPEN, true);
                        cTimeOut.Restart();
                        SubNext(41);
                    }
                    break;

                case 41:
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_CHUCK_OPEN) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(42);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_PIPE_MOUNT_CHUCK_CYLINDER_OPEN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 42:    // 파이프 마운트 Z축 안전 위치로 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].MoveAbsolute((int)eAxisPipeMount_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(43);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR14_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 43:    // 파이프 마운트 Z축 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(50);
                        }
                    }
                    break;

                case 50:    // X, Y축 홀더에 꽂힌 파이프를 Push하러 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_X].MoveAbsolute((int)eAxisPipeMount_X.PipeClampUp) == true &&
                            ml.Axis[eMotor.PIPE_MOUNT_Y].MoveAbsolute((int)eAxisPipeMount_Y.PipeClampUp) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(51);
                        }
                        else
                        {
                            cTimeOut.Stop();
                            if (ml.Axis[eMotor.PIPE_MOUNT_X].MoveAbsolute((int)eAxisPipeMount_X.PipeClampUp) == false)
                                ml.AddError(eErrorCode.MOTOR12_MOVE_TIMEOUT, iSubStep);
                            else if (ml.Axis[eMotor.PIPE_MOUNT_Y].MoveAbsolute((int)eAxisPipeMount_Y.PipeClampUp) == false)
                                ml.AddError(eErrorCode.MOTOR13_MOVE_TIMEOUT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 51:    // 파이프 마운트 X, Y축 홀더 Push 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.PIPE_MOUNT_Y].IsMoveDone() == true)
                        {
                            // 마지막 파이프 작업일 때, 비전을 한번더 촬영해야 하기 때문에,
                            // 파이프 푸셔를 진행하고 파이프 삽입 유무를 확인한다.

                            // 파이프 트랜스퍼에 클램프 할 파이프가 있다면, 클램프 할때 동시에 꽂힌 파이프 푸쉬하도록 진행
                            if (ml.cRunUnitData.GetIndexData(eData.PIPE_TRANSFER).GetStatus(eStatus.WORK_DONE) == true &&
                                NPC1_PipeMount.GetUnitMin(eStatus.HOLDER).iUnitNo < 18)
                            {
                                ml.cVar.bHolderPipePushBegin = true;
                                SubNext(99);
                            }
                            else
                            {
                                SubNext(52);
                            }
                        }
                    }
                    break;

                case 52:    // Push 실린더 Down
                    {
                        cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, true);
                        cTimeOut.Restart();
                        SubNext(53);
                    }
                    break;

                case 53:    // Push 실린더 Down 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_PUSHER_DOWN) == true ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Stop();
                            cDelay.Restart();
                            SubNext(54);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, false);
                                ml.AddError(eErrorCode.SEQ08_HOLDER_PIPE_PUSHER_CYLINDER_DOWN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 54:    // Push 실린더 Down 딜레이
                    {
                        if (cDelay.ElapsedMilliseconds > 200)
                        {
                            cDelay.Stop();
                            SubNext(55);
                        }
                    }
                    break;

                case 55:    // Push 실린더 Up
                    {
                        cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(56);
                    }
                    break;

                case 56:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_PUSHER_UP_ON) == true ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Stop();
                            if (NPC1_PipeMount.GetStatusCount(eStatus.HOLDER) > 1)
                            {
                                SubNext(100);
                            }
                            else
                            {
                                SubNext(60);
                            }
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_HOLDER_PIPE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 60:    // 파이프 마운터 X,Y축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Y].MoveAbsolute((int)eAxisPipeMount_Y.PipeMountVision) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(61);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR13_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 61:
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Y].IsMoveDone() == true)
                        {
                            cDelay.Restart();
                            SubNext(62);
                        }
                    }
                    break;

                case 62:    // 홀더 Vision 촬영 시작
                    {
                        if (cDelay.ElapsedMilliseconds > 100)
                        {
                            cDelay.Stop();
                            if (ml.cOptionData.bDryRunUse == true ||
                                Define.SIMULATION == true)
                            {
                                SubNext(100);
                            }
                            else
                            {
                                CAM3_ResultData.DataClear();
                                ml.cCVisionToolBlockLib[3].VisionShoot(0);
                                cTimeOut.Restart();
                                SubNext(63);
                            }
                        }
                    }
                    break;

                case 63:    // 촬영 완료
                    {
                        if (CAM3_ResultData.bShootFinish == true)
                        {
                            if (CAM3_ResultData.bHolderEmpty == true)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_PIPE_MOUNT_HOLDER_EMPTY, iSubStep);
                                return true;
                            }
                            else if (CAM3_ResultData.bPipeInserFail == true)
                            {
                                cTimeOut.Stop();
                                int Er = NPC1_PipeMount.GetUnitMin(eStatus.HOLDER).iUnitNo;
                                ml.AddError(eErrorCode.PIPE_1_INSERTION_FAIL + Er, iSubStep);
                                return true;
                            }
                            else
                            {
                                // 19번째 파이프도 삽입이 완료되었으면 시퀀스 종료
                                if (CAM3_ResultData.bGoodNg == true)
                                {
                                    cTimeOut.Restart();
                                    SubNext(100);
                                }
                                else
                                {
                                    ml.AddError(eErrorCode.SEQ08_PIPE_MOUNT_NOT_FIND_HOLE, iSubStep);
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > 3000)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_PIPE_MOUNT_VISION_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 99:    // 파이프 푸셔를 하지 않았을 때는 파이프 마운터 맵데이터만 변경한다.
                    {
                        PipeMounter.SetAllStatus(eStatus.EMPTY);

                        SubNext(0);
                        return true;
                    }
                    break;

                case 100:   // 파이프 마운터 맵데이터 변경
                    {
                        NPC1_PipeMount.GetUnitMin(eStatus.HOLDER).eStatus = eStatus.PIPE_MOUNT;
                        PipeMounter.SetAllStatus(eStatus.EMPTY);

                        ml.cVar.iPipeCycleTime = (int)ml.cVar.PipeMountCycleTime.ElapsedMilliseconds;
                        ml.cVar.PipeMountCycleTime.Restart();

                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 홀더 파이프 누름 실린더만 동작하는
        /// </summary>
        /// <returns></returns>
        public bool JustPipePush()
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

                case 10:    // 파이프 마운터 Z축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].MoveAbsolute((int)eAxisPipeMount_Z.Safe) == true)
                        {
                            // Push 실린더 Up
                            cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, false);
                            // 전자석 Off
                            cIO.SetOutput((int)eIO_O.PIPE_HOLDER_ELECTROMAGNET, false);
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                ml.AddError(eErrorCode.MOTOR14_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 파이프 마운터 Z축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_PUSHER_UP_ON) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_HOLDER_PIPE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // 파이프 마운터 Y축 파이프 누름 위치 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Y].MoveAbsolute((int)eAxisPipeMount_Y.PipeClampUp) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR13_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Y].IsMoveDone() == true)
                        {
                            SubNext(30);
                        }
                    }
                    break;

                case 30:    // Push 실린더 다운
                    {
                        cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, true);
                        cTimeOut.Restart();
                        SubNext(31);
                    }
                    break;

                case 31:    // Push 다운 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_PUSHER_DOWN) == true)
                        {
                            cTimeOut.Stop();
                            cDelay.Restart();
                            SubNext(32);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, false);
                                ml.AddError(eErrorCode.SEQ08_HOLDER_PIPE_PUSHER_CYLINDER_DOWN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 32:    // Push 실린더 Down 딜레이
                    {
                        if (cDelay.ElapsedMilliseconds > 200)
                        {
                            cDelay.Stop();
                            SubNext(33);
                        }
                    }
                    break;

                case 33:    // Push 실린더 업
                    {
                        cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(34);
                    }
                    break;

                case 34:    // Push 업 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_PUSHER_UP_ON) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(100);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_HOLDER_PIPE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
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
        /// 파이프 마운터 X,Y,Z 안전위치 이동 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool MoveSafePipeMounter()
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

                case 10:    // 파이프 마운터 Z축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].MoveAbsolute((int)eAxisPipeMount_Z.Safe) == true)
                        {
                            // Push 실린더 Up
                            cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, false);
                            // 전자석 Off
                            cIO.SetOutput((int)eIO_O.PIPE_HOLDER_ELECTROMAGNET, false);
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                ml.AddError(eErrorCode.MOTOR14_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 파이프 마운터 Z축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_PUSHER_UP_ON) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_HOLDER_PIPE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // 파이프 마운터 X,Y축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_X].MoveAbsolute((int)eAxisPipeMount_X.Safe) == true &&
                            ml.Axis[eMotor.PIPE_MOUNT_Y].MoveAbsolute((int)eAxisPipeMount_Y.Safe) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.PIPE_MOUNT_X].MoveAbsolute((int)eAxisPipeMount_X.Safe) == false)
                                    ml.AddError(eErrorCode.MOTOR12_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.PIPE_MOUNT_Y].MoveAbsolute((int)eAxisPipeMount_Y.Safe) == false)
                                    ml.AddError(eErrorCode.MOTOR13_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.PIPE_MOUNT_Y].IsMoveDone() == true)
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
        /// 그냥 파이프 홀더를 촬영한다.
        /// </summary>
        /// <returns></returns>
        public bool JustPipeHolderShot()
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

                case 10:    // 파이프 마운터 Z축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].MoveAbsolute((int)eAxisPipeMount_Z.Safe) == true)
                        {
                            // Push 실린더 Up
                            cIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, false);
                            // 전자석 Off
                            cIO.SetOutput((int)eIO_O.PIPE_HOLDER_ELECTROMAGNET, false);
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                ml.AddError(eErrorCode.MOTOR14_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 파이프 마운터 Z축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_PUSHER_UP_ON) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ08_HOLDER_PIPE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // 파이프 마운터 X,Y축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Y].MoveAbsolute((int)eAxisPipeMount_Y.PipeMountVision) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR13_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_Y].IsMoveDone() == true)
                        {
                            cDelay.Restart();
                            SubNext(22);
                        }
                    }
                    break;

                case 22:
                    {
                        if (cDelay.ElapsedMilliseconds > 100)
                        {
                            cDelay.Stop();
                            ml.cCVisionToolBlockLib[3].cAcqFifoTool.Run();
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
                        if (ml.cVar.Manual_PipeMounterClampUp == true)
                        {
                            Next(10);
                            ml.cVar.Manual_PipeMounterClampUp = false;
                        }
                        else if (ml.cVar.Manual_PipeMount == true)
                        {
                            Next(20);
                            ml.cVar.Manual_PipeMount = false;
                        }
                        else if (ml.cVar.Manual_MagneticInjection == true)
                        {
                            Next(30);
                            ml.cVar.Manual_MagneticInjection = false;
                        }
                        else if (ml.cVar.Manual_PipeMounterSafe == true)
                        {
                            Next(40);
                            ml.cVar.Manual_PipeMounterSafe = false;
                        }
                        else if (ml.cVar.Manual_JustPipeHolderShot == true)
                        {
                            Next(50);
                            ml.cVar.Manual_JustPipeHolderShot = false;
                        }
                        else if (ml.cVar.Manual_PipeMountNotInsp == true)
                        {
                            Next(60);
                            ml.cVar.Manual_PipeMountNotInsp = false;
                        }
                        else if (ml.cVar.Manual_JustPipePush == true)
                        {
                            Next(70);
                            ml.cVar.Manual_JustPipePush = false;
                        }
                    }
                    break;

                case 10:    // 양품 파이프 픽업 함수
                    {
                        if (PipeClampUp() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 20:    // 파이프 홀더에 마운트 함수
                    {
                        if (PipeMount() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 30:    // 파이프 홀더 마그네틱 인젝션 함수
                    {
                        if (MagneticInjection() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 40:    // 안전위치 대기 함수
                    {
                        if (MoveSafePipeMounter() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 50:    // 그냥 파이프 홀더 촬영
                    {
                        if (JustPipeHolderShot() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 60:    // 이전 파이프 검사 안하는 파이프 마운트
                    {
                        if (PipeMountNotInsp() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 70:    // 홀더 파이프 누름 실린더만 동작하는
                    {
                        if (JustPipePush() == true)
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