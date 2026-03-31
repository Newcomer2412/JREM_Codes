using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// 시퀀스 클래스
    /// </summary>
    public class Seq09_NeedleMount : ISequence, ISeqNo
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
        public Seq09_NeedleMount(eSequence sequence)
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
            CAM4_ResultData = CMainLib.Ins.cVar.GetVisionResultData(4, 0);

            // 로그 설정
            eLogType = eLogType.Seq09_NeedleMount;

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
        private CVisionResultData CAM4_ResultData;

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
            ml.cVar.NeedleMountCycleTime.Start();
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
            ml.cVar.NeedleMountCycleTime.Stop();
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
                ml.AddError(eErrorCode.SEQ09_TIME_OUT);
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
                        MapDataLib NeedleTransfer = ml.cRunUnitData.GetIndexData(eData.NEEDLE_TRANSFER);
                        MapDataLib NeedleMounter = ml.cRunUnitData.GetIndexData(eData.NEEDLE_MOUNTER);
                        MapDataLib NPC1_NeedleMount = ml.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT);

                        if (NeedleTransfer != null && NeedleMounter != null && NPC1_NeedleMount != null)
                        {
                            // 양품 니들 픽업 맵데이터 조건
                            bool bGoodNeedlePickUpMapData = NeedleTransfer.GetStatus(eStatus.WORK_DONE) &&
                                                            NeedleMounter.GetStatus(eStatus.EMPTY);
                            // 니들 홀더에 마운트 맵데이터 조건
                            bool bNeedleMountMapData = NeedleMounter.GetStatus(eStatus.MOUNT) &&
                                                       NPC1_NeedleMount.GetStatus(eStatus.HOLDER);

                            if (bGoodNeedlePickUpMapData == true)
                            {
                                Next(10);
                            }
                            else if (bNeedleMountMapData == true)
                            {
                                Next(20);
                            }
                        }
                    }
                    break;

                case 10:    // 양품 니들 픽업 함수
                    {
                        if (GoodNeedlePickUp() == true)
                        {
                            Next(0);
                        }
                    }
                    break;

                case 20:    // 니들 홀더에 마운트 함수
                    {
                        if (NeedleMount() == true)
                        {
                            Next(0);
                        }
                    }
                    break;

                case 30:    // 안전위치 대기 함수
                    {
                        if (MoveSafeNeedleMounter() == true)
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
        /// 니들 & 니들 픽업 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool GoodNeedlePickUp()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib NeedleTransfer = ml.cRunUnitData.GetIndexData(eData.NEEDLE_TRANSFER);
            MapDataLib NeedleMounter = ml.cRunUnitData.GetIndexData(eData.NEEDLE_MOUNTER);

            switch (iSubStep)
            {
                case 0:     // 양품 니들 픽업 맵데이터 조건
                    {
                        bool bGoodNeedlePickUpMapData = NeedleTransfer.GetStatus(eStatus.WORK_DONE) &&
                                                        NeedleMounter.GetStatus(eStatus.EMPTY);

                        if (bGoodNeedlePickUpMapData == true)
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

                case 10:    // 니들 마운트 Z축 안전 위치로 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.Safe) == true)
                        {
                            // 니들 척 실린더 오픈
                            cIO.SetOutput((int)eIO_O.NEEDLE_CHUCK_OPEN, true);
                            // Push 실린더 Up
                            cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, false);
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 니들 마운트 Z축 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // 니들 Chuck이 오픈되었는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_CHUCK_OPEN) == true)
                        {
                            SubNext(13);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_CHUCK_CYLINDER_OPEN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 13:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_PUSHER_UP_ON) == true)
                        {
                            SubNext(14);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_HOLDER_NEEDLE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 14:    // 니들 트랜스퍼가 FWD 위치인지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.TRANSFER_NEEDLE_FORWARD) == true)
                        {
                            SubNext(15);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                ml.AddError(eErrorCode.SEQ09_TRANSFER_RIGHT_CYLINDER_FWD_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 15:    // 니들 트랜스퍼가 니들을 잡고있는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.TRANSFER_NEEDLE_VACUUM) == true ||
                            Define.SIMULATION == true ||
                            ml.cOptionData.bDryRunUse == true)
                        {
                            SubNext(16);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ09_TRANSFER_RIGHT_NEEDLE_NOT_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 16:    // 니들 트렌스퍼 R축이 현재 Posture 위치인지 확인
                    {
                        double dGetCmdTransfer_R = ml.Axis[eMotor.NEEDLE_ROTATE].GetCmdPostion();
                        double d_R = ml.cAxisPosCollData.GetAxisPosition(eMotor.NEEDLE_ROTATE, (int)eAxisNeedleRotate.Rotate90);
                        double d_R_180 = ml.cAxisPosCollData.GetAxisPosition(eMotor.NEEDLE_ROTATE, (int)eAxisNeedleRotate.Rotate180);
                        if (dGetCmdTransfer_R == d_R ||
                            dGetCmdTransfer_R >= d_R + d_R_180)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ09_TRANSFER_RIGHT_MOTOR_R_NOT_POSTURE_POS, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 20:    // X, Y축 니들 클램프 할 위치로 이동
                    {
                        double dNeedle_X_ClampUp = 0.0; double dNeedle_Y_ClampUp = 0.0;
                        dNeedle_X_ClampUp = ml.cAxisPosCollData.GetAxisPosition(eMotor.NEEDLE_MOUNT_X, (int)eAxisNeedleMount_X.NeedleClampUp);
                        dNeedle_Y_ClampUp = ml.cAxisPosCollData.GetAxisPosition(eMotor.NEEDLE_MOUNT_Y, (int)eAxisNeedleMount_Y.NeedleClampUp);

                        // Needle Rotation 모터가 180도 회전한 상태의 위치값이면 X, Y 픽업 포지션을 변경한다.
                        double dTransfer_R = ml.cAxisPosCollData.GetAxisPosition(eMotor.NEEDLE_ROTATE, (int)eAxisNeedleRotate.Rotate90);
                        double dNeedleRotation = ml.cAxisPosCollData.GetAxisPosition(eMotor.NEEDLE_ROTATE, (int)eAxisNeedleRotate.Rotate180);
                        double dGetCmdNeedleRotate = ml.Axis[eMotor.NEEDLE_ROTATE].GetCmdPostion();
                        if (dGetCmdNeedleRotate >= dTransfer_R + dNeedleRotation)
                        {
                            dNeedle_X_ClampUp = ml.cAxisPosCollData.GetAxisPosition(eMotor.NEEDLE_MOUNT_X, (int)eAxisNeedleMount_X.NeedleClampUp180);
                            dNeedle_Y_ClampUp = ml.cAxisPosCollData.GetAxisPosition(eMotor.NEEDLE_MOUNT_Y, (int)eAxisNeedleMount_Y.NeedleClampUp180);
                        }

                        if (ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute(dNeedle_X_ClampUp) == true &&
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute(dNeedle_Y_ClampUp) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute(dNeedle_X_ClampUp) == false)
                                    ml.AddError(eErrorCode.MOTOR15_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute(dNeedle_Y_ClampUp) == false)
                                    ml.AddError(eErrorCode.MOTOR16_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // X, Y축 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(22);
                        }
                    }
                    break;

                case 22:    // Z축 클램프 할 위치로 이동
                    {
                        // 니들 푸쉬할 실린더가 있으면 Push Down 진행
                        if (ml.cVar.bHolderNeedlePushBegin == true)
                        {
                            cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, true);
                        }

                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.NeedleClampUp) == true)
                        {
                            // 니들 벅큠 On
                            cIO.SetOutput((int)eIO_O.NEEDLE_CLAMP_VACUUM, true);
                            cTimeOut.Restart();
                            SubNext(23);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 23:
                    {
                        if (ml.cVar.bHolderNeedlePushBegin == true)
                        {
                            if (cIO.GetInput((int)eIO_I.NEEDLE_PUSHER_DOWN) == true)
                            {
                                cTimeOut.Stop();
                                SubNext(24);
                            }
                            else
                            {
                                if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                                {
                                    cTimeOut.Stop();
                                    cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, false);
                                    ml.AddError(eErrorCode.SEQ08_HOLDER_PIPE_PUSHER_CYLINDER_DOWN_TIMEOUT, iSubStep);
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            cTimeOut.Stop();
                            SubNext(24);
                        }
                    }
                    break;

                case 24:    // Z축 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            cDelay.Restart();
                            SubNext(25);
                        }
                    }
                    break;

                case 25:
                    {
                        // 니들 푸쉬할 실린더 Up 진행
                        if (ml.cVar.bHolderNeedlePushBegin == true)
                        {
                            if (cDelay.ElapsedMilliseconds > 200)
                            {
                                cDelay.Restart();
                                // 니들 푸쉬할 실린더가 있으면 Push Up 진행
                                cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, false);
                                SubNext(26);
                            }
                        }
                        else
                        {
                            // 니들 푸쉬할 실린더가 없으면 바로 Y축 작업 진행
                            cDelay.Stop();
                            cTimeOut.Restart();
                            SubNext(30);
                        }
                    }
                    break;

                case 26:    // 니들 푸쉬할 실린더 Up 진행 후 200ms 대기 후 진행한다.
                    {
                        if (cDelay.ElapsedMilliseconds > 200)
                        {
                            cDelay.Stop();
                            SubNext(30);
                        }
                    }
                    break;

                case 30:    // Y축을 니들이 진공에 가까워 지도록 이동시킨다.
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveRelative((int)eAxisNeedleMount_Y.ClampVacuumPitch, false) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(31);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR16_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 31:    // Y축 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
                        {
                            cIO.SetOutput((int)eIO_O.TRANSFER_NEEDLE_VACUUM, false);
                            cDelay.Restart();
                            SubNext(32);
                        }
                    }
                    break;

                case 32:
                    {
                        if (cDelay.ElapsedMilliseconds > cSysArray.uiNeedleClampVacuumDelay)
                        {
                            cDelay.Stop();
                            cTimeOut.Restart();
                            SubNext(40);
                        }
                    }
                    break;

                case 40:    // 클램프가 완료되면 Z축 Up
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(41);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 41:    // Z축이 올라가는 중에 니들 Chuck 클로즈
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].GetActPostion() >= -20)
                        {
                            cIO.SetOutput((int)eIO_O.NEEDLE_CHUCK_OPEN, false);
                            cTimeOut.Restart();
                            SubNext(42);
                        }
                    }
                    break;

                case 42:    // 니들 Chuck 클로즈 센서 확인
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_CHUCK_CLOSE_ON, true) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(43);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_CHUCK_CYLINDER_CLOSE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 43:    // Z축 Up 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            if (ml.cVar.bHolderNeedlePushBegin == true)
                            {
                                if (cIO.GetInput((int)eIO_I.NEEDLE_PUSHER_UP_ON) == true)
                                {
                                    ml.cVar.bHolderNeedlePushBegin = false;

                                    ml.cVar.iNeedleCycleTime = (int)ml.cVar.NeedleMountCycleTime.ElapsedMilliseconds;
                                    ml.cVar.NeedleMountCycleTime.Restart();

                                    cTimeOut.Stop();
                                    for (int i = 0; i < ml.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT).iMaxXindex; i++)
                                    {
                                        if (ml.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT).GetUnitNo(i).eStatus == eStatus.HOLDER)
                                        {
                                            ml.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT).GetUnitNo(i).eStatus = eStatus.SECOND_MOUNT;
                                            break;
                                        }
                                    }
                                    SubNext(100);
                                }
                                else
                                {
                                    if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                                    {
                                        cTimeOut.Stop();
                                        ml.AddError(eErrorCode.SEQ08_HOLDER_PIPE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
                                        return true;
                                    }
                                }
                            }
                            else
                            {
                                cTimeOut.Stop();
                                SubNext(100);
                            }
                        }
                    }
                    break;

                case 100:   // 니들 마운터 맵데이터 변경
                    {
                        NeedleTransfer.SetAllStatus(eStatus.EMPTY);
                        NeedleMounter.SetAllStatus(eStatus.MOUNT);

                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 니들 & 니들 트렌스퍼 플레이스 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool NeedleMount()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib NeedleMounter = ml.cRunUnitData.GetIndexData(eData.NEEDLE_MOUNTER);
            MapDataLib NPC1_NeedleMount = ml.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT);

            switch (iSubStep)
            {
                case 0:     // 니들 홀더에 마운트 맵데이터 조건
                    {
                        bool bNeedleMountMapData = NeedleMounter.GetStatus(eStatus.MOUNT) &&
                                                   NPC1_NeedleMount.GetStatus(eStatus.HOLDER);

                        if (bNeedleMountMapData == true)
                        {
                            ml.cVar.bNeedleMountInspSkip = false;
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

                case 10:    // 니들 마운트 Z축 안전 위치로 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.Safe) == true)
                        {
                            // Push 실린더 Up
                            cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, false);
                            // 홀더 Fix 실린더 Down
                            cIO.SetOutput((int)eIO_O.FIXTURE_DOWN, true);
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 니들 마운트 Z축 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_PUSHER_UP_ON) == true)
                        {
                            SubNext(13);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_HOLDER_NEEDLE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
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
                                ml.AddError(eErrorCode.SEQ09_HOLDER_FIX_CYLINDER_DOWN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // Y축 홀더의 니들 홀 촬영위치로 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute((int)eAxisNeedleMount_Y.NeedleMountVision) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR16_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // Y축 홀더 촬영위치 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
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
                                CAM4_ResultData.DataClear();
                                ml.cCVisionToolBlockLib[4].VisionShoot(0);
                                cTimeOut.Restart();
                                SubNext(23);
                            }
                        }
                    }
                    break;

                case 23:    // 촬영 완료
                    {
                        if (CAM4_ResultData.bShootFinish == true)
                        {
                            if (CAM4_ResultData.bHolderEmpty == true)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_HOLDER_EMPTY, iSubStep);
                                return true;
                            }

                            //니들 홀 찾기 실패
                            if (CAM4_ResultData.bNeedleHoleSearchFail == true)
                            {
                                cTimeOut.Stop();

                                NPC1_NeedleMount.GetUnitMin(eStatus.HOLDER).eStatus = eStatus.SKIPPED;

                                ml.cVar.iNeedleCycleTime = (int)ml.cVar.NeedleMountCycleTime.ElapsedMilliseconds;
                                ml.cVar.NeedleMountCycleTime.Restart();

                                SubNext(0);
                                return true;
                            }
                            else if (CAM4_ResultData.bNeedleInserFail == true)
                            {
                                cTimeOut.Stop();
                                int Er = NPC1_NeedleMount.GetUnitMin(eStatus.HOLDER).iUnitNo - 1;
                                ml.AddError(eErrorCode.NEEDLE_1_INSERTION_FAIL + Er, iSubStep);
                                return true;
                            }
                            else
                            {
                                // 홀더에 니들를 꽂을 구멍이 남아있는지 화인
                                if (CAM4_ResultData.bGoodNg == true)
                                {
                                    cTimeOut.Restart();
                                    SubNext(30);
                                }
                                else
                                {
                                    ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_NOT_FIND_HOLE, iSubStep);
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > 3000)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_VISION_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 30:    // X, Y축 니들 마운트 보정 좌표이동
                    {
                        double dNeedle_X_NeedleMount = ml.cAxisPosCollData.GetAxisPosition(eMotor.NEEDLE_MOUNT_X, (int)eAxisNeedleMount_X.NeedleMountCenter);
                        double dNeedle_Y_NeedleMount = ml.cAxisPosCollData.GetAxisPosition(eMotor.NEEDLE_MOUNT_Y, (int)eAxisNeedleMount_Y.NeedleMountCenter);
                        double dMoveNeedleMount_X = dNeedle_X_NeedleMount + CAM4_ResultData.dNeedleMountX;
                        double dMoveNeedleMount_Y = dNeedle_Y_NeedleMount + CAM4_ResultData.dNeedleMountY;

                        if (ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute(dMoveNeedleMount_X) == true &&  // X축은 절대이동
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute(dMoveNeedleMount_Y) == true)    // Y축은 상대이동
                        {
                            NLogger.AddLog(eLogType.Seq09_NeedleMount, NLogger.eLogLevel.INFO, $"Move Needle Mount : X:{dMoveNeedleMount_X} Y:{dMoveNeedleMount_Y}");
                            cTimeOut.Stop();
                            SubNext(31);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute(dMoveNeedleMount_X) == false)
                                    ml.AddError(eErrorCode.MOTOR15_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute(dMoveNeedleMount_Y) == true)
                                    ml.AddError(eErrorCode.MOTOR16_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 31:    // 니들 X, Y축 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(32);
                        }
                    }
                    break;

                case 32:    // 니들 마운트 하러 Z축 다운
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.NeedleMountHolder) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(33);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 33:    // Z축 Down 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(34);
                        }
                    }
                    break;

                case 34:    // Z축 다운 후 니들를 마운트하러 천천히 내려간다.
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.MountSlow) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(35);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 35:    // 니들 마운트 Down 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(40);
                        }
                    }
                    break;

                case 40:    // 니들 진공 Off, Chuck 실린더 Open
                    {
                        cIO.SetOutput((int)eIO_O.NEEDLE_CLAMP_VACUUM, false);
                        cIO.SetOutput((int)eIO_O.NEEDLE_CHUCK_OPEN, true);
                        ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveRelative(0.5, false);
                        cTimeOut.Restart();
                        SubNext(41);
                    }
                    break;

                case 41:
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_CHUCK_OPEN) == true &&
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Restart();
                            SubNext(42);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_CHUCK_CYLINDER_OPEN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 42:    // 니들 마운트 Z축 안전 위치로 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(43);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 43:    // 니들 마운트 Z축 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(50);
                        }
                    }
                    break;

                case 50:    // Y축 홀더에 꽂힌 니들를 Push하러 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute((int)eAxisNeedleMount_X.NeedleClampUp) == true &&
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute((int)eAxisNeedleMount_Y.NeedleClampUp) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(51);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute((int)eAxisNeedleMount_X.NeedleClampUp) == false)
                                    ml.AddError(eErrorCode.MOTOR15_MOVE_TIMEOUT, iSubStep);
                                if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute((int)eAxisNeedleMount_Y.NeedleClampUp) == false)
                                    ml.AddError(eErrorCode.MOTOR16_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 51:    // 니들 마운트 Y축 홀더 Push 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
                        {
                            // 마지막 니들 작업일 때, 비전을 한번더 촬영해야 하기 때문에,
                            // 니들 푸셔를 진행하고 니들 삽입 유무를 확인한다.

                            // 니들 트랜스퍼에 클램프 할 니들이 있다면, 클램프 할때 동시에 꽂힌 니들 푸쉬하도록 진행
                            if (ml.cRunUnitData.GetIndexData(eData.NEEDLE_TRANSFER).GetStatus(eStatus.WORK_DONE) == true &&
                                NPC1_NeedleMount.GetUnitMin(eStatus.HOLDER).iUnitNo < 18)
                            {
                                ml.cVar.bHolderNeedlePushBegin = true;
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
                        cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, true);
                        cTimeOut.Restart();
                        SubNext(53);
                    }
                    break;

                case 53:    // Push 실린더 Down 확인
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_PUSHER_DOWN) == true)
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
                                cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, false);
                                ml.AddError(eErrorCode.SEQ09_HOLDER_NEEDLE_PUSHER_CYLINDER_DOWN_TIMEOUT, iSubStep);
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
                        cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(56);
                    }
                    break;

                case 56:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_PUSHER_UP_ON) == true)
                        {
                            cTimeOut.Stop();
                            if (NPC1_NeedleMount.GetStatusCount(eStatus.HOLDER) > 1)
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
                                ml.AddError(eErrorCode.SEQ09_HOLDER_NEEDLE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 60:    // 니들 마운터 X,Y축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute((int)eAxisNeedleMount_Y.NeedleMountVision) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(61);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR16_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 61:
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
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
                                CAM4_ResultData.DataClear();
                                CAM4_ResultData.bLastHoleInsp = true;
                                ml.cCVisionToolBlockLib[4].VisionShoot(0);
                                cTimeOut.Restart();
                                SubNext(63);
                            }
                        }
                    }
                    break;

                case 63:    // 촬영 완료
                    {
                        if (CAM4_ResultData.bShootFinish == true)
                        {
                            if (CAM4_ResultData.bHolderEmpty == true)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_HOLDER_EMPTY, iSubStep);
                                return true;
                            }
                            else if (CAM4_ResultData.bNeedleInserFail == true)
                            {
                                cTimeOut.Stop();
                                int Er = NPC1_NeedleMount.GetUnitMin(eStatus.HOLDER).iUnitNo;
                                ml.AddError(eErrorCode.NEEDLE_1_INSERTION_FAIL + Er, iSubStep);
                                return true;
                            }
                            else
                            {
                                // 19번째 니들 삽입이 완료되었으면 시퀀스 종료
                                if (CAM4_ResultData.bGoodNg == true)
                                {
                                    cTimeOut.Restart();
                                    SubNext(100);
                                }
                                else
                                {
                                    ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_NOT_FIND_HOLE, iSubStep);
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > 3000)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_VISION_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 99:    // 니들 푸셔를 하지 않았을 때는 니들 마운터 맵데이터만 변경한다.
                    {
                        NeedleMounter.SetAllStatus(eStatus.EMPTY);

                        SubNext(0);
                        return true;
                    }
                    break;

                case 100:   // 니들 Mounter 맵데이터 변경
                    {
                        NPC1_NeedleMount.GetUnitMin(eStatus.HOLDER).eStatus = eStatus.SECOND_MOUNT;
                        NeedleMounter.SetAllStatus(eStatus.EMPTY);

                        ml.cVar.iNeedleCycleTime = (int)ml.cVar.NeedleMountCycleTime.ElapsedMilliseconds;
                        ml.cVar.NeedleMountCycleTime.Restart();

                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 이전 니들 검사 안하는 니들 마운트 함수
        /// </summary>
        /// <returns></returns>
        public bool NeedleMountNotInsp()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib NeedleMounter = ml.cRunUnitData.GetIndexData(eData.NEEDLE_MOUNTER);
            MapDataLib NPC1_NeedleMount = ml.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT);

            switch (iSubStep)
            {
                case 0:     // 니들 홀더에 마운트 맵데이터 조건
                    {
                        bool bNeedleMountMapData = NeedleMounter.GetStatus(eStatus.MOUNT) &&
                                                   NPC1_NeedleMount.GetStatus(eStatus.HOLDER);

                        if (bNeedleMountMapData == true)
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

                case 10:    // 니들 마운트 Z축 안전 위치로 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.Safe) == true)
                        {
                            // Push 실린더 Up
                            cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, false);
                            // 홀더 Fix 실린더 Down
                            cIO.SetOutput((int)eIO_O.FIXTURE_DOWN, true);
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 니들 마운트 Z축 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_PUSHER_UP_ON) == true)
                        {
                            SubNext(13);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_HOLDER_NEEDLE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
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
                                ml.AddError(eErrorCode.SEQ09_HOLDER_FIX_CYLINDER_DOWN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // Y축 홀더의 니들 홀 촬영위치로 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute((int)eAxisNeedleMount_Y.NeedleMountVision) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR16_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // Y축 홀더 촬영위치 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
                        {
                            ml.cVar.bNeedleMountInspSkip = true;
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
                                CAM4_ResultData.DataClear();
                                ml.cCVisionToolBlockLib[4].VisionShoot(0);
                                cTimeOut.Restart();
                                SubNext(23);
                            }
                        }
                    }
                    break;

                //case 23:    // 촬영 완료
                //    {
                //        if (CAM4_ResultData.bShootFinish == true)
                //        {
                //            if (CAM4_ResultData.bHolderEmpty == true)
                //            {
                //                cTimeOut.Stop();
                //                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_HOLDER_EMPTY, iSubStep);
                //                return true;
                //            }

                //            else
                //            {
                //                // 홀더에 니들를 꽂을 구멍이 남아있는지 화인
                //                if (CAM4_ResultData.bGoodNg == true)
                //                {
                //                    cTimeOut.Restart();
                //                    SubNext(30);
                //                }
                //                else
                //                {
                //                    ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_NOT_FIND_HOLE, iSubStep);
                //                    return true;
                //                }
                //            }
                //        }
                //        else
                //        {
                //            if (cTimeOut.ElapsedMilliseconds > 3000)
                //            {
                //                cTimeOut.Stop();
                //                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_VISION_TIMEOUT, iSubStep);
                //                return true;
                //            }
                //        }
                //    }
                //    break;

                case 23:    // 촬영 완료
                    {
                        if (CAM4_ResultData.bShootFinish == true)
                        {
                            if (CAM4_ResultData.bHolderEmpty == true)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_HOLDER_EMPTY, iSubStep);
                                return true;
                            }

                            //니들 홀 찾기 실패
                            if (CAM4_ResultData.bNeedleHoleSearchFail == true)
                            {
                                cTimeOut.Stop();
                                NPC1_NeedleMount.GetUnitMin(eStatus.HOLDER).eStatus = eStatus.SKIPPED;

                                ml.cVar.iNeedleCycleTime = (int)ml.cVar.NeedleMountCycleTime.ElapsedMilliseconds;
                                ml.cVar.NeedleMountCycleTime.Restart();

                                SubNext(0);
                                return true;
                            }
                            else
                            {
                                // 홀더에 니들를 꽂을 구멍이 남아있는지 화인
                                if (CAM4_ResultData.bGoodNg == true)
                                {
                                    cTimeOut.Restart();
                                    SubNext(30);
                                }
                                else
                                {
                                    ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_NOT_FIND_HOLE, iSubStep);
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > 3000)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_VISION_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 30:    // X, Y축 니들 마운트 보정 좌표이동
                    {
                        ml.cVar.bNeedleMountInspSkip = false;
                        double dNeedle_X_NeedleMount = ml.cAxisPosCollData.GetAxisPosition(eMotor.NEEDLE_MOUNT_X, (int)eAxisNeedleMount_X.NeedleMountCenter);
                        double dNeedle_Y_NeedleMount = ml.cAxisPosCollData.GetAxisPosition(eMotor.NEEDLE_MOUNT_Y, (int)eAxisNeedleMount_Y.NeedleMountCenter);
                        double dMoveNeedleMount_X = dNeedle_X_NeedleMount + CAM4_ResultData.dNeedleMountX;
                        double dMoveNeedleMount_Y = dNeedle_Y_NeedleMount + CAM4_ResultData.dNeedleMountY;

                        if (ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute(dMoveNeedleMount_X) == true &&  // X축은 절대이동
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute(dMoveNeedleMount_Y) == true)    // Y축은 상대이동
                        {
                            NLogger.AddLog(eLogType.Seq09_NeedleMount, NLogger.eLogLevel.INFO, $"Move Needle Mount : X:{dMoveNeedleMount_X} Y:{dMoveNeedleMount_Y}");
                            cTimeOut.Stop();
                            SubNext(31);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute(dMoveNeedleMount_X) == false)
                                    ml.AddError(eErrorCode.MOTOR15_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute(dMoveNeedleMount_Y) == true)
                                    ml.AddError(eErrorCode.MOTOR16_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 31:    // 니들 X, Y축 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(32);
                        }
                    }
                    break;

                case 32:    // 니들 마운트 하러 Z축 다운
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.NeedleMountHolder) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(33);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 33:    // Z축 Down 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(34);
                        }
                    }
                    break;

                case 34:    // Z축 다운 후 니들를 마운트하러 천천히 내려간다.
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.MountSlow) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(35);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 35:    // 니들 마운트 Down 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(40);
                        }
                    }
                    break;

                case 40:    // 니들 진공 Off, Chuck 실린더 Open
                    {
                        cIO.SetOutput((int)eIO_O.NEEDLE_CLAMP_VACUUM, false);
                        cIO.SetOutput((int)eIO_O.NEEDLE_CHUCK_OPEN, true);
                        cTimeOut.Restart();
                        SubNext(41);
                    }
                    break;

                case 41:
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_CHUCK_OPEN) == true ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Restart();
                            SubNext(42);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_CHUCK_CYLINDER_OPEN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 42:    // 니들 마운트 Z축 안전 위치로 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(43);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 43:    // 니들 마운트 Z축 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(50);
                        }
                    }
                    break;

                case 50:    // Y축 홀더에 꽂힌 니들를 Push하러 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute((int)eAxisNeedleMount_X.NeedleClampUp) == true &&
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute((int)eAxisNeedleMount_Y.NeedleClampUp) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(51);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute((int)eAxisNeedleMount_X.NeedleClampUp) == false)
                                    ml.AddError(eErrorCode.MOTOR15_MOVE_TIMEOUT, iSubStep);
                                if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute((int)eAxisNeedleMount_Y.NeedleClampUp) == false)
                                    ml.AddError(eErrorCode.MOTOR16_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 51:    // 니들 마운트 Y축 홀더 Push 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
                        {
                            // 마지막 니들 작업일 때, 비전을 한번더 촬영해야 하기 때문에,
                            // 니들 푸셔를 진행하고 니들 삽입 유무를 확인한다.

                            // 니들 트랜스퍼에 클램프 할 니들이 있다면, 클램프 할때 동시에 꽂힌 니들 푸쉬하도록 진행
                            if (ml.cRunUnitData.GetIndexData(eData.NEEDLE_TRANSFER).GetStatus(eStatus.WORK_DONE) == true &&
                                NPC1_NeedleMount.GetUnitMin(eStatus.HOLDER).iUnitNo < 18)
                            {
                                ml.cVar.bHolderNeedlePushBegin = true;
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
                        cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, true);
                        cTimeOut.Restart();
                        SubNext(53);
                    }
                    break;

                case 53:    // Push 실린더 Down 확인
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_PUSHER_DOWN) == true)
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
                                cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, false);
                                ml.AddError(eErrorCode.SEQ09_HOLDER_NEEDLE_PUSHER_CYLINDER_DOWN_TIMEOUT, iSubStep);
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
                        cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(56);
                    }
                    break;

                case 56:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_PUSHER_UP_ON) == true)
                        {
                            cTimeOut.Stop();
                            if (NPC1_NeedleMount.GetStatusCount(eStatus.HOLDER) > 1)
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
                                ml.AddError(eErrorCode.SEQ09_HOLDER_NEEDLE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 60:    // 니들 마운터 X,Y축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute((int)eAxisNeedleMount_Y.NeedleMountVision) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(61);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR16_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 61:
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
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
                                CAM4_ResultData.DataClear();
                                ml.cCVisionToolBlockLib[4].VisionShoot(0);
                                cTimeOut.Restart();
                                SubNext(63);
                            }
                        }
                    }
                    break;

                case 63:    // 촬영 완료
                    {
                        if (CAM4_ResultData.bShootFinish == true)
                        {
                            if (CAM4_ResultData.bHolderEmpty == true)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_HOLDER_EMPTY, iSubStep);
                                return true;
                            }
                            else if (CAM4_ResultData.bNeedleInserFail == true)
                            {
                                cTimeOut.Stop();
                                int Er = NPC1_NeedleMount.GetUnitMin(eStatus.HOLDER).iUnitNo;
                                ml.AddError(eErrorCode.NEEDLE_1_INSERTION_FAIL + Er, iSubStep);
                                return true;
                            }
                            else
                            {
                                // 19번째 니들 삽입이 완료되었으면 시퀀스 종료
                                if (CAM4_ResultData.bGoodNg == true)
                                {
                                    cTimeOut.Restart();
                                    SubNext(100);
                                }
                                else
                                {
                                    ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_NOT_FIND_HOLE, iSubStep);
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > 3000)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_VISION_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 99:    // 니들 푸셔를 하지 않았을 때는 니들 마운터 맵데이터만 변경한다.
                    {
                        NeedleMounter.SetAllStatus(eStatus.EMPTY);

                        SubNext(0);
                        return true;
                    }
                    break;

                case 100:   // 니들 Mounter 맵데이터 변경
                    {
                        NPC1_NeedleMount.GetUnitMin(eStatus.HOLDER).eStatus = eStatus.SECOND_MOUNT;
                        NeedleMounter.SetAllStatus(eStatus.EMPTY);

                        ml.cVar.iNeedleCycleTime = (int)ml.cVar.NeedleMountCycleTime.ElapsedMilliseconds;
                        ml.cVar.NeedleMountCycleTime.Restart();

                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 홀더 니들 누름 실린더만 동작하는 함수
        /// </summary>
        /// <returns></returns>
        public bool JustNeedlePush()
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

                case 10:    // 니들 마운터 Z축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            // Push 실린더 Up
                            cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, false);
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 니들 마운터 Z축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_PUSHER_UP_ON) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_HOLDER_NEEDLE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // 니들 마운터 Y축 니들 누름 위치 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute((int)eAxisNeedleMount_Y.NeedleClampUp) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR16_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
                        {
                            SubNext(30);
                        }
                    }
                    break;

                case 30:    // Push 실린더 Down
                    {
                        cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, true);
                        cTimeOut.Restart();
                        SubNext(31);
                    }
                    break;

                case 31:    // Push 실린더 Down 확인
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_PUSHER_DOWN) == true)
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
                                cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, false);
                                ml.AddError(eErrorCode.SEQ09_HOLDER_NEEDLE_PUSHER_CYLINDER_DOWN_TIMEOUT, iSubStep);
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

                case 33:    // Push 실린더 Up
                    {
                        cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(34);
                    }
                    break;

                case 34:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_PUSHER_UP_ON) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(100);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_HOLDER_NEEDLE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
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
        /// 니들 마운터 X,Y,Z 안전위치 이동 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool MoveSafeNeedleMounter()
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

                case 10:    // 니들 마운터 Z축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            // Push 실린더 Up
                            cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, false);
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 니들 마운터 Z축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_PUSHER_UP_ON) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_HOLDER_NEEDLE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // 니들 마운터 X,Y축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute((int)eAxisNeedleMount_X.Safe) == true &&
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute((int)eAxisNeedleMount_Y.Safe) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute((int)eAxisNeedleMount_X.Safe) == false)
                                    ml.AddError(eErrorCode.MOTOR15_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute((int)eAxisNeedleMount_Y.Safe) == false)
                                    ml.AddError(eErrorCode.MOTOR16_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
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
        /// 그냥 니들 홀더를 촬영한다.
        /// </summary>
        /// <returns></returns>
        public bool JustNeedleHolderShot()
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

                case 10:    // 니들 마운터 Z축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            // Push 실린더 Up
                            cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, false);
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 니들 마운터 Z축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_PUSHER_UP_ON) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_HOLDER_NEEDLE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // 니들 마운터 X,Y축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute((int)eAxisNeedleMount_Y.NeedleMountVision) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR16_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
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
                            ml.cCVisionToolBlockLib[4].cAcqFifoTool.Run();
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
        /// 강제 니들 마운트 함수
        /// </summary>
        /// <returns></returns>
        public bool ForcedNeedleMount()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib NeedleMounter = ml.cRunUnitData.GetIndexData(eData.NEEDLE_MOUNTER);
            MapDataLib NPC1_NeedleMount = ml.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT);

            switch (iSubStep)
            {
                case 0:     // 니들 홀더에 마운트 맵데이터 조건
                    {
                        bool bNeedleMountMapData = NeedleMounter.GetStatus(eStatus.MOUNT) &&
                                                   NPC1_NeedleMount.GetStatus(eStatus.HOLDER);

                        if (bNeedleMountMapData == true)
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

                case 10:    // 니들 마운트 Z축 안전 위치로 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.Safe) == true)
                        {
                            // Push 실린더 Up
                            cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, false);
                            // 홀더 Fix 실린더 Down
                            cIO.SetOutput((int)eIO_O.FIXTURE_DOWN, true);
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 니들 마운트 Z축 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_PUSHER_UP_ON) == true)
                        {
                            SubNext(13);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_HOLDER_NEEDLE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
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
                                ml.AddError(eErrorCode.SEQ09_HOLDER_FIX_CYLINDER_DOWN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // Y축 홀더의 니들 홀 촬영위치로 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute((int)eAxisNeedleMount_Y.NeedleMountVision) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR16_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // Y축 홀더 촬영위치 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
                        {
                            ml.cVar.bForcedNeedleMount = true;
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
                                CAM4_ResultData.DataClear();
                                ml.cCVisionToolBlockLib[4].VisionShoot(0);
                                cTimeOut.Restart();
                                SubNext(23);
                            }
                        }
                    }
                    break;

                case 23:    // 촬영 완료
                    {
                        if (CAM4_ResultData.bShootFinish == true)
                        {
                            if (CAM4_ResultData.bHolderEmpty == true)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_HOLDER_EMPTY, iSubStep);
                                return true;
                            }

                            //니들 홀 찾기 실패
                            if (CAM4_ResultData.bNeedleHoleSearchFail == true)
                            {
                                cTimeOut.Stop();
                                NPC1_NeedleMount.GetUnitMin(eStatus.HOLDER).eStatus = eStatus.SKIPPED;

                                ml.cVar.iNeedleCycleTime = (int)ml.cVar.NeedleMountCycleTime.ElapsedMilliseconds;
                                ml.cVar.NeedleMountCycleTime.Restart();

                                SubNext(0);
                                return true;
                            }
                            else
                            {
                                cTimeOut.Restart();
                                SubNext(30);
                            }
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > 3000)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_VISION_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 30:    // X, Y축 니들 마운트 보정 좌표이동
                    {
                        ml.cVar.bNeedleMountInspSkip = false;
                        double dNeedle_X_NeedleMount = ml.cAxisPosCollData.GetAxisPosition(eMotor.NEEDLE_MOUNT_X, (int)eAxisNeedleMount_X.NeedleMountCenter);
                        double dNeedle_Y_NeedleMount = ml.cAxisPosCollData.GetAxisPosition(eMotor.NEEDLE_MOUNT_Y, (int)eAxisNeedleMount_Y.NeedleMountCenter);
                        double dMoveNeedleMount_X = dNeedle_X_NeedleMount + CAM4_ResultData.dNeedleMountX;
                        double dMoveNeedleMount_Y = dNeedle_Y_NeedleMount + CAM4_ResultData.dNeedleMountY;

                        if (ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute(dMoveNeedleMount_X) == true &&  // X축은 절대이동
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute(dMoveNeedleMount_Y) == true)    // Y축은 상대이동
                        {
                            NLogger.AddLog(eLogType.Seq09_NeedleMount, NLogger.eLogLevel.INFO, $"Move Needle Mount : X:{dMoveNeedleMount_X} Y:{dMoveNeedleMount_Y}");
                            cTimeOut.Stop();
                            SubNext(31);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute(dMoveNeedleMount_X) == false)
                                    ml.AddError(eErrorCode.MOTOR15_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute(dMoveNeedleMount_Y) == true)
                                    ml.AddError(eErrorCode.MOTOR16_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 31:    // 니들 X, Y축 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(32);
                        }
                    }
                    break;

                case 32:    // 니들 마운트 하러 Z축 다운
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.NeedleMountHolder) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(33);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 33:    // Z축 Down 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(34);
                        }
                    }
                    break;

                case 34:    // Z축 다운 후 니들을 마운트하러 천천히 내려간다.
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.MountSlow) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(35);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 35:    // 니들 마운트 Down 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(40);
                        }
                    }
                    break;

                case 40:    // 니들 진공 Off, Chuck 실린더 Open
                    {
                        cIO.SetOutput((int)eIO_O.NEEDLE_CLAMP_VACUUM, false);
                        cIO.SetOutput((int)eIO_O.NEEDLE_CHUCK_OPEN, true);
                        cTimeOut.Restart();
                        SubNext(41);
                    }
                    break;

                case 41:
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_CHUCK_OPEN) == true ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Restart();
                            SubNext(42);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_CHUCK_CYLINDER_OPEN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 42:    // 니들 마운트 Z축 안전 위치로 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(43);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 43:    // 니들 마운트 Z축 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(50);
                        }
                    }
                    break;

                case 50:    // Y축 홀더에 꽂힌 니들를 Push하러 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute((int)eAxisNeedleMount_X.NeedleClampUp) == true &&
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute((int)eAxisNeedleMount_Y.NeedleClampUp) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(51);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute((int)eAxisNeedleMount_X.NeedleClampUp) == false)
                                    ml.AddError(eErrorCode.MOTOR15_MOVE_TIMEOUT, iSubStep);
                                if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute((int)eAxisNeedleMount_Y.NeedleClampUp) == false)
                                    ml.AddError(eErrorCode.MOTOR16_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 51:    // 니들 마운트 Y축 홀더 Push 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
                        {
                            // 마지막 니들 작업일 때, 비전을 한번더 촬영해야 하기 때문에,
                            // 니들 푸셔를 진행하고 니들 삽입 유무를 확인한다.

                            // 니들 트랜스퍼에 클램프 할 니들이 있다면, 클램프 할때 동시에 꽂힌 니들 푸쉬하도록 진행
                            if (ml.cRunUnitData.GetIndexData(eData.NEEDLE_TRANSFER).GetStatus(eStatus.WORK_DONE) == true &&
                                NPC1_NeedleMount.GetUnitMin(eStatus.HOLDER).iUnitNo < 18)
                            {
                                ml.cVar.bHolderNeedlePushBegin = true;
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
                        cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, true);
                        cTimeOut.Restart();
                        SubNext(53);
                    }
                    break;

                case 53:    // Push 실린더 Down 확인
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_PUSHER_DOWN) == true)
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
                                cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, false);
                                ml.AddError(eErrorCode.SEQ09_HOLDER_NEEDLE_PUSHER_CYLINDER_DOWN_TIMEOUT, iSubStep);
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
                        cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(56);
                    }
                    break;

                case 56:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_PUSHER_UP_ON) == true)
                        {
                            cTimeOut.Stop();
                            if (NPC1_NeedleMount.GetStatusCount(eStatus.HOLDER) > 1)
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
                                ml.AddError(eErrorCode.SEQ09_HOLDER_NEEDLE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 60:    // 니들 마운터 X,Y축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute((int)eAxisNeedleMount_Y.NeedleMountVision) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(61);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR16_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 61:
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
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
                                CAM4_ResultData.DataClear();
                                ml.cCVisionToolBlockLib[4].VisionShoot(0);
                                cTimeOut.Restart();
                                SubNext(63);
                            }
                        }
                    }
                    break;

                case 63:    // 촬영 완료
                    {
                        if (CAM4_ResultData.bShootFinish == true)
                        {
                            if (CAM4_ResultData.bHolderEmpty == true)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_HOLDER_EMPTY, iSubStep);
                                return true;
                            }
                            else if (CAM4_ResultData.bNeedleInserFail == true)
                            {
                                cTimeOut.Stop();
                                int Er = NPC1_NeedleMount.GetUnitMin(eStatus.HOLDER).iUnitNo;
                                ml.AddError(eErrorCode.NEEDLE_1_INSERTION_FAIL + Er, iSubStep);
                                return true;
                            }
                            else
                            {
                                // 19번째 니들 삽입이 완료되었으면 시퀀스 종료
                                if (CAM4_ResultData.bGoodNg == true)
                                {
                                    cTimeOut.Restart();
                                    SubNext(100);
                                }
                                else
                                {
                                    ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_NOT_FIND_HOLE, iSubStep);
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > 3000)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_VISION_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 99:    // 니들 푸셔를 하지 않았을 때는 니들 마운터 맵데이터만 변경한다.
                    {
                        NeedleMounter.SetAllStatus(eStatus.EMPTY);

                        SubNext(0);
                        return true;
                    }
                    break;

                case 100:   // 니들 Mounter 맵데이터 변경
                    {
                        NPC1_NeedleMount.GetUnitMin(eStatus.HOLDER).eStatus = eStatus.SECOND_MOUNT;
                        NeedleMounter.SetAllStatus(eStatus.EMPTY);

                        ml.cVar.iNeedleCycleTime = (int)ml.cVar.NeedleMountCycleTime.ElapsedMilliseconds;
                        ml.cVar.NeedleMountCycleTime.Restart();
                        ml.cVar.bForcedNeedleMount = false;

                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 푸쉬 없는 니들 마운트 함수
        /// </summary>
        /// <returns></returns>
        public bool NeedleMountNotPush()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib NeedleMounter = ml.cRunUnitData.GetIndexData(eData.NEEDLE_MOUNTER);
            MapDataLib NPC1_NeedleMount = ml.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT);

            switch (iSubStep)
            {
                case 0:     // 니들 홀더에 마운트 맵데이터 조건
                    {
                        bool bNeedleMountMapData = NeedleMounter.GetStatus(eStatus.MOUNT) &&
                                                   NPC1_NeedleMount.GetStatus(eStatus.HOLDER);

                        if (bNeedleMountMapData == true)
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

                case 10:    // 니들 마운트 Z축 안전 위치로 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.Safe) == true)
                        {
                            // Push 실린더 Up
                            cIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, false);
                            // 홀더 Fix 실린더 Down
                            cIO.SetOutput((int)eIO_O.FIXTURE_DOWN, true);
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // 니들 마운트 Z축 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // Push 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_PUSHER_UP_ON) == true)
                        {
                            SubNext(13);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_HOLDER_NEEDLE_PUSHER_CYLINDER_UP_TIMEOUT, iSubStep);
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
                                ml.AddError(eErrorCode.SEQ09_HOLDER_FIX_CYLINDER_DOWN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // Y축 홀더의 니들 홀 촬영위치로 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute((int)eAxisNeedleMount_Y.NeedleMountVision) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR16_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // Y축 홀더 촬영위치 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
                        {
                            ml.cVar.bForcedNeedleMount = true;
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
                                CAM4_ResultData.DataClear();
                                ml.cCVisionToolBlockLib[4].VisionShoot(0);
                                cTimeOut.Restart();
                                SubNext(23);
                            }
                        }
                    }
                    break;

                case 23:    // 촬영 완료
                    {
                        if (CAM4_ResultData.bShootFinish == true)
                        {
                            if (CAM4_ResultData.bHolderEmpty == true)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_HOLDER_EMPTY, iSubStep);
                                return true;
                            }

                            //니들 홀 찾기 실패
                            if (CAM4_ResultData.bNeedleHoleSearchFail == true)
                            {
                                cTimeOut.Stop();
                                NPC1_NeedleMount.GetUnitMin(eStatus.HOLDER).eStatus = eStatus.SKIPPED;

                                ml.cVar.iNeedleCycleTime = (int)ml.cVar.NeedleMountCycleTime.ElapsedMilliseconds;
                                ml.cVar.NeedleMountCycleTime.Restart();

                                SubNext(0);
                                return true;
                            }
                            else
                            {
                                cTimeOut.Restart();
                                SubNext(30);
                            }
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > 3000)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_VISION_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 30:    // X, Y축 니들 마운트 보정 좌표이동
                    {
                        ml.cVar.bNeedleMountInspSkip = false;
                        double dNeedle_X_NeedleMount = ml.cAxisPosCollData.GetAxisPosition(eMotor.NEEDLE_MOUNT_X, (int)eAxisNeedleMount_X.NeedleMountCenter);
                        double dNeedle_Y_NeedleMount = ml.cAxisPosCollData.GetAxisPosition(eMotor.NEEDLE_MOUNT_Y, (int)eAxisNeedleMount_Y.NeedleMountCenter);
                        double dMoveNeedleMount_X = dNeedle_X_NeedleMount + CAM4_ResultData.dNeedleMountX;
                        double dMoveNeedleMount_Y = dNeedle_Y_NeedleMount + CAM4_ResultData.dNeedleMountY;

                        if (ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute(dMoveNeedleMount_X) == true &&  // X축은 절대이동
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute(dMoveNeedleMount_Y) == true)    // Y축은 상대이동
                        {
                            NLogger.AddLog(eLogType.Seq09_NeedleMount, NLogger.eLogLevel.INFO, $"Move Needle Mount : X:{dMoveNeedleMount_X} Y:{dMoveNeedleMount_Y}");
                            cTimeOut.Stop();
                            SubNext(31);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute(dMoveNeedleMount_X) == false)
                                    ml.AddError(eErrorCode.MOTOR15_MOVE_TIMEOUT, iSubStep);
                                else if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute(dMoveNeedleMount_Y) == true)
                                    ml.AddError(eErrorCode.MOTOR16_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 31:    // 니들 X, Y축 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(32);
                        }
                    }
                    break;

                case 32:    // 니들 마운트 하러 Z축 다운
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.NeedleMountHolder) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(33);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 33:    // Z축 Down 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(34);
                        }
                    }
                    break;

                case 34:    // Z축 다운 후 니들을 마운트하러 천천히 내려간다.
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.MountSlow) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(35);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 35:    // 니들 마운트 Down 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(40);
                        }
                    }
                    break;

                case 40:    // 니들 진공 Off, Chuck 실린더 Open
                    {
                        cIO.SetOutput((int)eIO_O.NEEDLE_CLAMP_VACUUM, false);
                        cIO.SetOutput((int)eIO_O.NEEDLE_CHUCK_OPEN, true);
                        cTimeOut.Restart();
                        SubNext(41);
                    }
                    break;

                case 41:
                    {
                        if (cIO.GetInput((int)eIO_I.NEEDLE_CHUCK_OPEN) == true ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Restart();
                            SubNext(42);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_CHUCK_CYLINDER_OPEN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 42:    // 니들 마운트 Z축 안전 위치로 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.Safe) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(43);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR17_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 43:    // 니들 마운트 Z축 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(50);
                        }
                    }
                    break;

                case 50:    // Y축 홀더에 꽂힌 니들를 Push하러 이동
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute((int)eAxisNeedleMount_X.NeedleClampUp) == true &&
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute((int)eAxisNeedleMount_Y.NeedleClampUp) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(51);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute((int)eAxisNeedleMount_X.NeedleClampUp) == false)
                                    ml.AddError(eErrorCode.MOTOR15_MOVE_TIMEOUT, iSubStep);
                                if (ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute((int)eAxisNeedleMount_Y.NeedleClampUp) == false)
                                    ml.AddError(eErrorCode.MOTOR16_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 51:    // 니들 마운트 Y축 홀더 Push 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.NEEDLE_MOUNT_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
                        {
                            SubNext(100);
                        }
                    }
                    break;

                case 100:    // 니들 마운터 맵데이터만 변경한다.
                    {
                        NeedleMounter.SetAllStatus(eStatus.EMPTY);

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
                        if (ml.cVar.Manual_NeedleMounterClampUp == true)
                        {
                            Next(10);
                            ml.cVar.Manual_NeedleMounterClampUp = false;
                        }
                        else if (ml.cVar.Manual_NeedleMount == true)
                        {
                            Next(20);
                            ml.cVar.Manual_NeedleMount = false;
                        }
                        else if (ml.cVar.Manual_NeedleMounterSafe == true)
                        {
                            Next(30);
                            ml.cVar.Manual_NeedleMounterSafe = false;
                        }
                        else if (ml.cVar.Manual_JustNeedleHolderShot == true)
                        {
                            Next(40);
                            ml.cVar.Manual_JustNeedleHolderShot = false;
                        }
                        else if (ml.cVar.Manual_NeedeleMountNotInsp == true)
                        {
                            Next(50);
                            ml.cVar.Manual_NeedeleMountNotInsp = false;
                        }
                        else if (ml.cVar.Manual_JustNeedlePush == true)
                        {
                            Next(60);
                            ml.cVar.Manual_JustNeedlePush = false;
                        }
                        else if (ml.cVar.Manual_ForcedNeedleMount == true)
                        {
                            Next(70);
                            ml.cVar.Manual_ForcedNeedleMount = false;
                        }
                        else if (ml.cVar.Manual_NeedleMountNotPush == true)
                        {
                            Next(80);
                            ml.cVar.Manual_NeedleMountNotPush = false;
                        }
                    }
                    break;

                case 10:    // 양품 니들 픽업 함수
                    {
                        if (GoodNeedlePickUp() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 20:    // 니들 홀더에 마운트 함수
                    {
                        if (NeedleMount() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 30:    // 안전위치 대기 함수
                    {
                        if (MoveSafeNeedleMounter() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 40:    // 그냥 니들 홀더 촬영
                    {
                        if (JustNeedleHolderShot() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 50:    // 이전 니들 검사 안하는 니들 마운트
                    {
                        if (NeedleMountNotInsp() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 60:    // 홀더 니들 누름 실린더만 동작하는
                    {
                        if (JustNeedlePush() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 70:    // 강제 니들 마운트
                    {
                        if (ForcedNeedleMount() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 80:    // 푸쉬 없는 니들 마운트
                    {
                        if (NeedleMountNotPush() == true)
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