using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// 시퀀스 클래스
    /// </summary>
    public class Seq00_Front_MPC : ISequence, ISeqNo
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
        public Seq00_Front_MPC(eSequence sequence)
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

            // 로그 설정
            eLogType = eLogType.Seq00_Front_MPC;

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
                ml.AddError(eErrorCode.SEQ00_TIME_OUT);
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
                        MapDataLib EmptyHolderTray = ml.cRunUnitData.GetIndexData(eData.SUPPLY_HOLDER_TRAY);

                        MapDataLib MPC1_FarLeft = ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_LEFT);
                        MapDataLib MPC1_Buffer1 = ml.cRunUnitData.GetIndexData(eData.MPC1_BUFFER_1);
                        MapDataLib MPC1_PipeMount = ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT);
                        MapDataLib MPC1_Buffer2 = ml.cRunUnitData.GetIndexData(eData.MPC1_BUFFER_2);
                        MapDataLib MPC1_NeedleMount = ml.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT);
                        MapDataLib MPC1_Flip = ml.cRunUnitData.GetIndexData(eData.MPC1_FLIP);
                        MapDataLib MPC1_Dispensing = ml.cRunUnitData.GetIndexData(eData.MPC1_DISPENSING);
                        MapDataLib MPC1_FarRight = ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_RIGHT);
                        MapDataLib MPC2_FarRight = ml.cRunUnitData.GetIndexData(eData.MPC2_FAR_RIGHT);
                        MapDataLib MPC2_UV = ml.cRunUnitData.GetIndexData(eData.MPC2_UV);

                        if (MPC1_FarLeft != null && MPC1_Buffer1 != null && MPC1_PipeMount != null && MPC1_Buffer2 != null &&
                            MPC1_NeedleMount != null && MPC1_Flip != null && MPC1_Dispensing != null && MPC1_FarRight != null)
                        {
                            // 전면 MPC가 전부 Empty 상태이면 리턴 시킨다.
                            if (MPC1_FarLeft.GetAllStatus(eStatus.EMPTY, false) == true &&
                                MPC1_Buffer1.GetStatus(eStatus.EMPTY) == true &&
                                MPC1_PipeMount.GetAllStatus(eStatus.EMPTY, false) == true &&
                                MPC1_Buffer2.GetStatus(eStatus.EMPTY) == true &&
                                MPC1_NeedleMount.GetAllStatus(eStatus.EMPTY, true) == true &&
                                MPC1_Flip.GetStatus(eStatus.EMPTY) == true &&
                                MPC1_Dispensing.GetStatus(eStatus.EMPTY) == true &&
                                MPC2_UV.GetStatus(eStatus.EMPTY) == true)
                            {
                                return false;
                            }

                            bool bFirstPalletMapData = (MPC1_FarLeft.GetUnitNo(0).eStatus == eStatus.HOLDER && MPC1_FarLeft.GetUnitNo(1).eStatus == eStatus.EMPTY) ||
                                                        MPC1_FarLeft.GetAllStatus(eStatus.EMPTY, false);

                            bool bPitchMoveMapData = false;

                            // 파이프 사용 안하면 플래그 변경하여 적용
                            if (ml.cVar.bUsePipe == false)
                            {
                                bPitchMoveMapData = (MPC1_Buffer1.GetStatus(eStatus.HOLDER) || MPC1_Buffer1.GetStatus(eStatus.EMPTY)) &&
                                                               (MPC1_PipeMount.GetAllStatus(eStatus.HOLDER, eStatus.SKIPPED, true) || MPC1_PipeMount.GetAllStatus(eStatus.EMPTY, true)) &&
                                                               (MPC1_Buffer2.GetStatus(eStatus.HOLDER) || MPC1_Buffer2.GetStatus(eStatus.EMPTY)) &&
                                                               (MPC1_NeedleMount.GetAllStatus(eStatus.NEEDLE_MOUNT, eStatus.SKIPPED, true) || MPC1_NeedleMount.GetAllStatus(eStatus.EMPTY, true)) &&
                                                               (MPC1_Flip.GetStatus(eStatus.FLIP_DONE) || MPC1_Flip.GetStatus(eStatus.SKIP_AFTER_FLIP) || MPC1_Flip.GetStatus(eStatus.EMPTY)) &&
                                                               (MPC1_Dispensing.GetStatus(eStatus.DISPENSING) || MPC1_Dispensing.GetStatus(eStatus.EMPTY) || MPC1_Dispensing.GetStatus(eStatus.NG)) &&
                                                                MPC1_FarRight.GetStatus(eStatus.NONE) &&
                                                                MPC2_FarRight.GetStatus(eStatus.NONE);
                            }
                            else
                            {
                                bPitchMoveMapData = (MPC1_Buffer1.GetStatus(eStatus.HOLDER) || MPC1_Buffer1.GetStatus(eStatus.EMPTY)) &&
                                                             (MPC1_PipeMount.GetAllStatus(eStatus.PIPE_MAGNETIC, eStatus.SKIPPED, true) || MPC1_PipeMount.GetAllStatus(eStatus.EMPTY, true)) &&
                                                             (MPC1_Buffer2.GetStatus(eStatus.PIPE_MAGNETIC) || MPC1_Buffer2.GetStatus(eStatus.EMPTY)) &&
                                                             (MPC1_NeedleMount.GetAllStatus(eStatus.NEEDLE_MOUNT, eStatus.SKIPPED, true) || MPC1_NeedleMount.GetAllStatus(eStatus.EMPTY, true)) &&
                                                             (MPC1_Flip.GetStatus(eStatus.FLIP_DONE) || MPC1_Flip.GetStatus(eStatus.SKIP_AFTER_FLIP) || MPC1_Flip.GetStatus(eStatus.EMPTY)) &&
                                                             (MPC1_Dispensing.GetStatus(eStatus.DISPENSING) || MPC1_Dispensing.GetStatus(eStatus.EMPTY) || MPC1_Dispensing.GetStatus(eStatus.NG)) &&
                                                              MPC1_FarRight.GetStatus(eStatus.NONE) &&
                                                              MPC2_FarRight.GetStatus(eStatus.NONE);
                            }

                            bool bFlipMapData = MPC1_Flip.GetStatus(eStatus.NEEDLE_MOUNT) || MPC1_Flip.GetStatus(eStatus.SKIPPED);

                            if (bFirstPalletMapData == true &&
                                bPitchMoveMapData == true)
                            {
                                cIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_LEFT, false);

                                double GetActLeftRail_Y = ml.Axis[eMotor.MPC_LEFT_Y].GetActPostion();
                                double LeftRail_Y_FrontPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.MPC_LEFT_Y, (int)eAxisLeftMPC_Y.Front);
                                if (GetActLeftRail_Y >= LeftRail_Y_FrontPos - 0.05 &&
                                    GetActLeftRail_Y <= LeftRail_Y_FrontPos + 0.05 &&
                                    cIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_LEFT) == true)
                                {
                                    Next(10);
                                }
                            }
                            else if (bFlipMapData == true)
                            {
                                Next(20);
                            }
                        }
                    }
                    break;

                case 10:    // MPC 한칸 Pitch 이동 함수
                    {
                        if (MPCMovePitch() == true)
                        {
                            Next(0);
                        }
                    }
                    break;

                case 20:    // 홀더 Flip 함수
                    {
                        if (FlipHolder() == true)
                        {
                            Next(0);
                        }
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// MPC Y축 Front 위치 및 LM 실린더 FWD 확인
        /// </summary>
        /// <returns></returns>
        private bool CheckMPC_Y_Axis()
        {
            double GetActLeftRail_Y = ml.Axis[eMotor.MPC_LEFT_Y].GetActPostion();
            double LeftRail_Y_FrontPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.MPC_LEFT_Y, (int)eAxisLeftMPC_Y.Front);
            double GetActRightRail_Y = ml.Axis[eMotor.MPC_RIGHT_Y].GetActPostion();
            double RightRail_Y_RearPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.MPC_RIGHT_Y, (int)eAxisRightMPC_Y.Front);

            return GetActLeftRail_Y >= LeftRail_Y_FrontPos - 0.05 &&
                   GetActRightRail_Y <= RightRail_Y_RearPos + 0.05 &&
                   cIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_LEFT) == true &&
                   cIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_RIGHT) == true;
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
        /// MPC1 Pitch 이동 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool MPCMovePitch()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib MPC1_FarLeft = ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_LEFT);
            MapDataLib MPC1_Buffer1 = ml.cRunUnitData.GetIndexData(eData.MPC1_BUFFER_1);
            MapDataLib MPC1_PipeMount = ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT);
            MapDataLib MPC1_Buffer2 = ml.cRunUnitData.GetIndexData(eData.MPC1_BUFFER_2);
            MapDataLib MPC1_NeedleMount = ml.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT);
            MapDataLib MPC1_Flip = ml.cRunUnitData.GetIndexData(eData.MPC1_FLIP);
            MapDataLib MPC1_Dispensing = ml.cRunUnitData.GetIndexData(eData.MPC1_DISPENSING);
            MapDataLib MPC1_FarRight = ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_RIGHT);
            MapDataLib MPC2_FarRight = ml.cRunUnitData.GetIndexData(eData.MPC2_FAR_RIGHT);
            MapDataLib MPC2_UV = ml.cRunUnitData.GetIndexData(eData.MPC2_UV);

            switch (iSubStep)
            {
                case 0:     // MPC1 Pitch 이동 맵데이터 조건 확인
                    {
                        if (ml.McState == eMachineState.MANUALRUN &&
                            MPC1_FarLeft.GetAllStatus(eStatus.EMPTY, false) == true &&
                            MPC1_Buffer1.GetStatus(eStatus.EMPTY) == true &&
                            MPC1_PipeMount.GetAllStatus(eStatus.EMPTY, false) == true &&
                            MPC1_Buffer2.GetStatus(eStatus.EMPTY) == true &&
                            MPC1_NeedleMount.GetAllStatus(eStatus.EMPTY, true) == true &&
                            MPC1_Flip.GetStatus(eStatus.EMPTY) == true &&
                            MPC1_Dispensing.GetStatus(eStatus.EMPTY) == true &&
                            MPC2_UV.GetStatus(eStatus.EMPTY) == true)
                        {
                            return true;
                        }

                        bool bFirstPalletMapData = (MPC1_FarLeft.GetUnitNo(0).eStatus == eStatus.HOLDER && MPC1_FarLeft.GetUnitNo(1).eStatus == eStatus.EMPTY) ||
                                                    MPC1_FarLeft.GetAllStatus(eStatus.EMPTY, false);

                        bool bPitchMoveMapData = false;

                        // 파이프 사용 안하면 플래그 변경하여 적용
                        if (ml.cVar.bUsePipe == false)
                        {
                            bPitchMoveMapData = (MPC1_Buffer1.GetStatus(eStatus.HOLDER) || MPC1_Buffer1.GetStatus(eStatus.EMPTY)) &&
                                                           (MPC1_PipeMount.GetAllStatus(eStatus.HOLDER, eStatus.SKIPPED, true) || MPC1_PipeMount.GetAllStatus(eStatus.EMPTY, true)) &&
                                                           (MPC1_Buffer2.GetStatus(eStatus.HOLDER) || MPC1_Buffer2.GetStatus(eStatus.EMPTY)) &&
                                                           (MPC1_NeedleMount.GetAllStatus(eStatus.NEEDLE_MOUNT, eStatus.SKIPPED, true) || MPC1_NeedleMount.GetAllStatus(eStatus.EMPTY, true)) &&
                                                           (MPC1_Flip.GetStatus(eStatus.FLIP_DONE) || MPC1_Flip.GetStatus(eStatus.SKIP_AFTER_FLIP) || MPC1_Flip.GetStatus(eStatus.EMPTY)) &&
                                                           (MPC1_Dispensing.GetStatus(eStatus.DISPENSING) || MPC1_Dispensing.GetStatus(eStatus.EMPTY) || MPC1_Dispensing.GetStatus(eStatus.NG)) &&
                                                            MPC1_FarRight.GetStatus(eStatus.NONE) &&
                                                            MPC2_FarRight.GetStatus(eStatus.NONE);
                        }
                        else
                        {
                            bPitchMoveMapData = (MPC1_Buffer1.GetStatus(eStatus.HOLDER) || MPC1_Buffer1.GetStatus(eStatus.EMPTY)) &&
                                                         (MPC1_PipeMount.GetAllStatus(eStatus.PIPE_MAGNETIC, eStatus.SKIPPED, true) || MPC1_PipeMount.GetAllStatus(eStatus.EMPTY, true)) &&
                                                         (MPC1_Buffer2.GetStatus(eStatus.PIPE_MAGNETIC) || MPC1_Buffer2.GetStatus(eStatus.EMPTY)) &&
                                                         (MPC1_NeedleMount.GetAllStatus(eStatus.NEEDLE_MOUNT, eStatus.SKIPPED, true) || MPC1_NeedleMount.GetAllStatus(eStatus.EMPTY, true)) &&
                                                         (MPC1_Flip.GetStatus(eStatus.FLIP_DONE) || MPC1_Flip.GetStatus(eStatus.SKIP_AFTER_FLIP) || MPC1_Flip.GetStatus(eStatus.EMPTY)) &&
                                                         (MPC1_Dispensing.GetStatus(eStatus.DISPENSING) || MPC1_Dispensing.GetStatus(eStatus.EMPTY) || MPC1_Dispensing.GetStatus(eStatus.NG)) &&
                                                          MPC1_FarRight.GetStatus(eStatus.NONE) &&
                                                          MPC2_FarRight.GetStatus(eStatus.NONE);
                        }
                        if (bFirstPalletMapData == true &&
                            bPitchMoveMapData == true)
                        {
                            cTimeOut.Restart();
                            ml.Axis[eMotor.MPC_FRONT_X].SetZero();
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

                case 10:    // MPC 왼쪽레일 모터가 전진 위치에 있는지 확인
                    {
                        double GetActLeftRail_Y = ml.Axis[eMotor.MPC_LEFT_Y].GetActPostion();
                        double LeftRail_Y_FrontPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.MPC_LEFT_Y, (int)eAxisLeftMPC_Y.Front);
                        if (GetActLeftRail_Y >= LeftRail_Y_FrontPos - 0.05 &&
                            GetActLeftRail_Y <= LeftRail_Y_FrontPos + 0.05)
                        {
                            cTimeOut.Restart();
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MPC_LEFT_RAIL_Y_NOT_FRONT_POSITION, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // MPC 오른쪽레인 모터가 전진 위치에 있는지 확인
                    {
                        double GetActRightRail_Y = ml.Axis[eMotor.MPC_RIGHT_Y].GetActPostion();
                        double RightRail_Y_FrontPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.MPC_RIGHT_Y, (int)eAxisRightMPC_Y.Front);
                        if (GetActRightRail_Y >= RightRail_Y_FrontPos - 0.05 &&
                            GetActRightRail_Y <= RightRail_Y_FrontPos + 0.05)
                        {
                            cTimeOut.Stop();
                            SubNext(12);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MPC_RIGHT_RAIL_Y_NOT_FRONT_POSITION, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 12:    // 파이프 마운트 Z축 안전위치 확인
                    {
                        double GetActPipeMount_Z = ml.Axis[eMotor.PIPE_MOUNT_Z].GetActPostion();
                        double PipeMount_Z_SafePos = ml.cAxisPosCollData.GetAxisPosition(eMotor.PIPE_MOUNT_Z, (int)eAxisPipeMount_Z.Safe);
                        if (GetActPipeMount_Z >= PipeMount_Z_SafePos - 1)
                        {
                            cTimeOut.Restart();
                            SubNext(13);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.PIPE_MOUNT_Z_NOT_SAFE_POSITION, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 13:    // 니들 마운트 Z축 안전위치 확인
                    {
                        double GetActNeedleMount_Z = ml.Axis[eMotor.NEEDLE_MOUNT_Z].GetActPostion();
                        double NeedleMount_Z_SafePos = ml.cAxisPosCollData.GetAxisPosition(eMotor.NEEDLE_MOUNT_Z, (int)eAxisNeedleMount_Z.Safe);
                        if (GetActNeedleMount_Z >= NeedleMount_Z_SafePos - 1)
                        {
                            cTimeOut.Restart();
                            SubNext(14);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.NEEDLE_MOUNT_Z_NOT_SAFE_POSITION, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 14:    // Flip Z축 Up 위치 확인
                    {
                        double GetActFlip_Z = ml.Axis[eMotor.FLIP_Z].GetActPostion();
                        double Flip_Z_UpPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.FLIP_Z, (int)eAxisFlip_Z.FlipUp);
                        if (GetActFlip_Z >= Flip_Z_UpPos - 1)
                        {
                            cTimeOut.Restart();
                            SubNext(15);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.FLIP_Z_NOT_UP_POSITION, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 15:    // Dispenser Z축 안전위치 확인
                    {
                        double GetActDispenser_Z = ml.Axis[eMotor.DISPENSER_Z].GetActPostion();
                        double Dispenser_Z_SafePos = ml.cAxisPosCollData.GetAxisPosition(eMotor.DISPENSER_Z, (int)eAxisDispenser_Z.Safe);
                        if (GetActDispenser_Z >= Dispenser_Z_SafePos - 1)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.DISPENSER_Z_NOT_SAFE_POSITION, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // Pipe, Needle 홀더 푸셔 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_PUSHER_UP_ON) == true &&
                            cIO.GetInput((int)eIO_I.NEEDLE_PUSHER_UP_ON) == true)
                        {
                            cTimeOut.Restart();
                            // 홀더 고정 실린더 Up
                            cIO.SetOutput((int)eIO_O.FIXTURE_DOWN, false);
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                if (cIO.GetInput((int)eIO_I.PIPE_PUSHER_UP_ON) == false)
                                    ml.AddError(eErrorCode.SEQ00_PIPE_HOLDER_PUSHER_CYLINDER_NOT_UP, iSubStep);
                                else if (cIO.GetInput((int)eIO_I.NEEDLE_PUSHER_UP_ON) == false)
                                    ml.AddError(eErrorCode.SEQ00_NEEDLE_HOLDER_PUSHER_CYLINDER_NOT_UP, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // 파이프 니들 홀더 고정 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_HOLDER_FIXTURE_UP_ON) == true &&
                            cIO.GetInput((int)eIO_I.NEEDLE_HOLDER_FIXTURE_UP_ON) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(22);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                if (cIO.GetInput((int)eIO_I.PIPE_HOLDER_FIXTURE_UP_ON) == false)
                                    ml.AddError(eErrorCode.SEQ00_PIPE_FIX_CYLINDER_NOT_UP, iSubStep);
                                else if (cIO.GetInput((int)eIO_I.NEEDLE_HOLDER_FIXTURE_UP_ON) == false)
                                    ml.AddError(eErrorCode.SEQ00_NEEDLE_FIX_CYLINDER_NOT_UP, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 22:    // Left Pallets Y 이동 가이드 실린더 FWD 확인
                    {
                        if (cIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_LEFT) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(23);
                        }
                        else
                        {
                            cIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_LEFT, false);

                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ01_LM_RAIL_GUIDE_LEFT_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 23:    // Right Pallets Y 이동 가이드 실린더 FWD 확인
                    {
                        if (cIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_RIGHT) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(30);
                        }
                        else
                        {
                            cIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_RIGHT, false);

                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ01_LM_RAIL_GUIDE_LEFT_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 30:    // 전면 맨 왼쪽 팔레트가 감지 센서 On 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_LEFT) == true)
                        {
                            SubNext(31);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ00_PALLETS_NOT_DETECTION_FRONT_LEFT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 31:    // 전면 맨 오른쪽 팔레트가 감지 센서 Off 확인 (맨 오른쪽 팔렛이 없어야 팔렛 Pitch이동이 가능)
                    {
                        if (cIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_RIGHT, true) == false)
                        {
                            cDelay.Restart();
                            SubNext(32);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ00_PALLETS_DETECTION_FRONT_RIGHT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 32:    // MPC 이동 완료 후 벨트 Pitch 확인 (정확한 Pitch 이동이 되었으면 벨트 안쪽면의 이빨을 감지해야 한다)
                    {
                        if (cIO.GetInput((int)eIO_I.BELT_PITCH_DETECTION_RIGHT) == true)
                        {
                            cDelay.Stop();
                            SubNext(33);
                        }

                        if (cDelay.ElapsedMilliseconds > 1000)
                        {
                            cDelay.Stop();
                            ml.AddError(eErrorCode.SEQ00_FRONT_MPC_BELT_PITCH_NOT_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 33:    // 전면 팔렛 Pitch 센서 On 확인 (센서가 On이면 팔렛이 정위치에 있다는 의미)
                    {
                        if (cIO.GetInput((int)eIO_I.PALLETS_PITCH_DETECTION_FRONT) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(40);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ00_PALLETS_PITCH_NOT_DETECTION_FRONT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 40:    // MPC 전면 Pitch 이동
                    {
                        double dMovePitch = ml.cAxisPosCollData.GetAxisPosition(eMotor.MPC_FRONT_X, (int)eAxisFrontMPC.MovePitch);
                        if (ml.Axis[eMotor.MPC_FRONT_X].MoveRelative(dMovePitch) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(41);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR2_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 41:    // MPC 이동 확인
                    {
                        if (ml.Axis[eMotor.MPC_FRONT_X].IsMoveDone() == true)
                        {
                            cDelay.Restart();
                            SubNext(42);
                        }
                    }
                    break;

                case 42:    // 전면 팔렛 Pitch 센서 On 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PALLETS_PITCH_DETECTION_FRONT) == true)
                        {
                            SubNext(43);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ00_PALLETS_PITCH_NOT_DETECTION_FRONT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 43:    // MPC 이동 완료 후 벨트 Pitch 확인 (정확한 Pitch 이동이 되었으면 벨트 안쪽면의 이빨을 감지해야 한다)
                    {
                        if (cIO.GetInput((int)eIO_I.BELT_PITCH_DETECTION_RIGHT) == true)
                        {
                            cDelay.Stop();
                            SubNext(44);
                        }

                        if (cDelay.ElapsedMilliseconds > 1000)
                        {
                            cDelay.Stop();
                            ml.AddError(eErrorCode.SEQ00_FRONT_MPC_BELT_PITCH_NOT_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 44:
                    {
                        SubNext(50);
                    }
                    break;

                case 50:    // 파이프 니들 홀더 고정 실린더 Down
                    {
                        cIO.SetOutput((int)eIO_O.FIXTURE_DOWN, true);
                        cTimeOut.Restart();
                        SubNext(51);
                    }
                    break;

                case 51:    // 파이프 니들 홀더 고정 실린더 Down 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_HOLDER_FIXTURE_DOWN) == true &&
                            cIO.GetInput((int)eIO_I.NEEDLE_HOLDER_FIXTURE_DOWN) == true ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Stop();
                            SubNext(60);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                if (cIO.GetInput((int)eIO_I.PIPE_HOLDER_FIXTURE_DOWN) == false)
                                    ml.AddError(eErrorCode.SEQ00_PIPE_FIX_CYLINDER_NOT_DOWN, iSubStep);
                                else if (cIO.GetInput((int)eIO_I.NEEDLE_HOLDER_FIXTURE_DOWN) == false)
                                    ml.AddError(eErrorCode.SEQ00_NEEDLE_FIX_CYLINDER_NOT_DOWN, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                // MPC 전면 이동 완료 했으면 맵데이터 변경
                // 에러 타이밍을 맞추기 위해 맵데이터 변경을 가능하면 나중에 하도록 고민중
                case 60:
                    {
                        if (MPC1_Flip.GetStatus(eStatus.SKIP_AFTER_FLIP) == true)
                        {
                            SubNext(70);
                        }
                        else if (MPC1_PipeMount.GetStatus(eStatus.SKIPPED) == true)
                        {
                            SubNext(90);
                        }
                        else
                        {
                            ml.cRunUnitData.MPC1_MapDataShift();
                            SubNext(100);
                        }
                    }
                    break;

                case 70:    // 디스펜서 Z축 안전위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.Safe) == true)
                        {
                            SubNext(71);
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

                case 71:    // 디스펜서 Z축 안전위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].IsMoveDone() == true)
                        {
                            SubNext(72);
                        }
                    }
                    break;

                case 72:    // 디스펜서 니들 평탄 측정 실린더 Up
                    {
                        cIO.SetOutput((int)eIO_O.DISP_NEEDLE_FLAT_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(73);
                    }
                    break;

                case 73:    // 디스펜서 니들 평탄 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.DISP_NEEDLE_FLAT_CHECK_UP_ON) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(80);
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

                case 80:    // 디스펜서 X,Y축 홀더 비전 위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].MoveAbsolute((int)eAxisDispenser_X.HolderVision) == true &&
                            ml.Axis[eMotor.DISPENSER_Y].MoveAbsolute((int)eAxisDispenser_Y.HolderVision) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(81);
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

                case 81:    // 디스펜서 X,Y축 홀더 비전 촬영 위치 이동 확인
                    {
                        if (ml.Axis[eMotor.DISPENSER_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.DISPENSER_Y].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(82);
                        }
                    }
                    break;

                case 82:    // 디스펜서 Z축 홀더 비전 촬영 위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].MoveAbsolute((int)eAxisDispenser_Z.HolderVision) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(83);
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

                case 83:    // 디스펜서 Z축 홀더 비전 촬영 위치 이동
                    {
                        if (ml.Axis[eMotor.DISPENSER_Z].IsMoveDone() == true)
                        {
                            cDelay.Restart();
                            SubNext(90);
                        }
                    }
                    break;

                case 90:
                    {
                        if (MPC1_Flip.GetStatus(eStatus.SKIP_AFTER_FLIP) &&
                            MPC1_PipeMount.GetStatus(eStatus.SKIPPED) == true)
                        {
                            ml.AddError(eErrorCode.SEQ00_PIPE_NEEDLE_SKIPPED_HOLDER_DETECTED, iSubStep);
                            ml.cRunUnitData.MPC1_MapDataShift();
                            return true;
                        }

                        if (MPC1_PipeMount.GetStatus(eStatus.SKIPPED) == true)
                        {
                            ml.AddError(eErrorCode.SEQ00_PIPE_SKIPPED_HOLDER_DETECTED, iSubStep);
                            ml.cRunUnitData.MPC1_MapDataShift();
                            return true;
                        }

                        if (MPC1_Flip.GetStatus(eStatus.SKIP_AFTER_FLIP) == true)
                        {
                            ml.AddError(eErrorCode.SEQ00_NEEDLE_SKIPPED_HOLDER_DETECTED, iSubStep);
                            ml.cRunUnitData.MPC1_MapDataShift();
                            return true;
                        }
                    }
                    break;

                //플립 플래그 초기화
                case 100:
                    {
                        ml.cVar.bFlipDone = false;
                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 홀더를 뒤집는 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool FlipHolder()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib MPC1_Flip = ml.cRunUnitData.GetIndexData(eData.MPC1_FLIP);

            switch (iSubStep)
            {
                case 0:     // Flip 동작 맵데이터 조건 확인
                    {
                        bool bFlipMapData = MPC1_Flip.GetAllStatus(eStatus.NEEDLE_MOUNT, false) || MPC1_Flip.GetAllStatus(eStatus.SKIPPED, false);

                        if (bFlipMapData == true)
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

                case 10:    // 척 실린더 Open, 테이블 BWD, 테이블 Left 이동
                    {
                        // 플립 척 오픈
                        cIO.SetOutput((int)eIO_O.FLIP_CHUCK_CLOSE, false);
                        cIO.SetOutput((int)eIO_O.FLIP_CHUCK_OPEN, true);
                        cTimeOut.Restart();
                        SubNext(11);
                    }
                    break;

                case 11:    // 척 실린더 Open 확인
                    {
                        if (cIO.GetInput((int)eIO_I.FLIP_CHUCK_OPEN_ON, true) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(20);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ00_FLIP_CHUCK_OPEN_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // Flip Z축 Up 이동
                    {
                        if (ml.Axis[eMotor.FLIP_Z].MoveAbsolute((int)eAxisFlip_Z.FlipUp) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR24_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // Flip Z축 Up 이동 확인
                    {
                        if (ml.Axis[eMotor.FLIP_Z].IsMoveDone() == true)
                        {
                            // Flip T축이 포지션 값이 '0' 또는 '180'이 아니면 원점위치로 돌려놓는다.
                            if (ml.Axis[eMotor.FLIP_T].GetCmdPostion() == 0 ||
                                ml.Axis[eMotor.FLIP_T].GetCmdPostion() == 180)
                            {
                                SubNext(23);
                            }
                            else
                            {
                                cTimeOut.Restart();
                                SubNext(22);
                            }
                        }
                    }
                    break;

                case 22:    // Flip T 축 원점으로 이동
                    {
                        if (ml.Axis[eMotor.FLIP_T].MoveAbsolute((int)eAxisFlip_T.Origin) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(23);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR23_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 23:    // Flip T축 원점위치 이동 확인
                    {
                        if (ml.Axis[eMotor.FLIP_T].IsMoveDone() == true)
                        {
                            SubNext(24);
                        }
                    }
                    break;

                case 24:    // Flip 테이블 왼쪽 이동
                    {
                        cIO.SetOutput((int)eIO_O.FLIP_TABLE_LEFT, true);
                        cIO.SetOutput((int)eIO_O.FLIP_TABLE_RIGHT, false);
                        cTimeOut.Restart();
                        SubNext(25);
                    }
                    break;

                case 25:    // Flip 테이블 실린더 왼쪽 확인
                    {
                        if (cIO.GetInput((int)eIO_I.FLIP_TABLE_LEFT, true) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(30);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ00_FLIP_TABLE_RIGHT_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 30:    // Flip Z축 Down
                    {
                        if (ml.Axis[eMotor.FLIP_Z].MoveAbsolute((int)eAxisFlip_Z.FlipDown) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(31);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR24_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 31:    // Flip Z축 Down 확인
                    {
                        if (ml.Axis[eMotor.FLIP_Z].IsMoveDone() == true)
                        {
                            cDelay.Restart();
                            SubNext(32);
                        }
                    }
                    break;

                case 32:    // 딜레이 후 척 Close
                    {
                        if (cDelay.ElapsedMilliseconds > 500)
                        {
                            cDelay.Stop();
                            SubNext(40);
                        }
                    }
                    break;

                case 40:    // 척 실린더 Close
                    {
                        cIO.SetOutput((int)eIO_O.FLIP_CHUCK_OPEN, false);
                        cIO.SetOutput((int)eIO_O.FLIP_CHUCK_CLOSE, true);
                        cTimeOut.Restart();
                        SubNext(41);
                    }
                    break;

                case 41:    // 척 실린더 Close 확인
                    {
                        if (cIO.GetInput((int)eIO_I.FLIP_CHUCK_CLOSE, true) == true)
                        {
                            cDelay.Restart();
                            SubNext(42);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ00_FLIP_CHUCK_CLOSE_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 42:    // Close 후 딜레이
                    {
                        if (cDelay.ElapsedMilliseconds > cSysArray.uiFlipGripperCloseDelay)
                        {
                            cDelay.Stop();
                            cTimeOut.Restart();
                            SubNext(43);
                        }
                    }
                    break;

                case 43:    // Flip Z축 Up
                    {
                        if (ml.Axis[eMotor.FLIP_Z].MoveAbsolute((int)eAxisFlip_Z.FlipUp, 10) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(44);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR24_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 44:    // Flip Z축 Up 확인
                    {
                        if (ml.Axis[eMotor.FLIP_Z].IsMoveDone() == true)
                        {
                            cTimeOut.Stop();
                            SubNext(50);
                        }
                    }
                    break;

                case 50:    // 테이블 실린더 오른쪽 이동
                    {
                        cIO.SetOutput((int)eIO_O.FLIP_TABLE_LEFT, false);
                        cIO.SetOutput((int)eIO_O.FLIP_TABLE_RIGHT, true);
                        cTimeOut.Restart();
                        SubNext(51);
                    }
                    break;

                case 51:    // 테이블 실린더 오른쪽 확인
                    {
                        if (cIO.GetInput((int)eIO_I.FLIP_TABLE_RIGHT_ON, false) == true ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Restart();
                            SubNext(52);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ00_FLIP_TABLE_RIGHT_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 52:    // Flip T축 180도 회전
                    {
                        if (ml.Axis[eMotor.FLIP_T].MoveAbsolute((int)eAxisFlip_T.Flip) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(53);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR23_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 53:    // Flip T축 180도 확인
                    {
                        if (ml.Axis[eMotor.FLIP_T].IsMoveDone() == true)
                        {
                            //ml.Axis[eMotor.FLIP_T].SetZero();
                            cTimeOut.Restart();
                            SubNext(60);
                        }
                    }
                    break;

                case 60:    // Flip Z축 Down
                    {
                        if (ml.Axis[eMotor.FLIP_Z].MoveAbsolute((int)eAxisFlip_Z.FlipDown) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(61);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR24_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 61:    // Flip Z축 Down 확인
                    {
                        if (ml.Axis[eMotor.FLIP_Z].IsMoveDone() == true)
                        {
                            cDelay.Restart();
                            SubNext(62);
                        }
                    }
                    break;

                case 62:    // 1초 딜레이
                    {
                        if (cDelay.ElapsedMilliseconds > 1000)
                        {
                            cDelay.Stop();
                            SubNext(63);
                        }
                    }
                    break;

                case 63:    // 실린더 척 Open
                    {
                        cIO.SetOutput((int)eIO_O.FLIP_CHUCK_CLOSE, false);
                        cIO.SetOutput((int)eIO_O.FLIP_CHUCK_OPEN, true);
                        cTimeOut.Restart();
                        SubNext(64);
                    }
                    break;

                case 64:    // 실린더 척 Open 확인
                    {
                        if (cIO.GetInput((int)eIO_I.FLIP_CHUCK_OPEN_ON, true) == true)
                        {
                            cDelay.Restart();
                            cTimeOut.Stop();
                            SubNext(65);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ00_FLIP_CHUCK_OPEN_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 65:    // 척 오픈 딜레이
                    {
                        if (cDelay.ElapsedMilliseconds > 1000)
                        {
                            cDelay.Stop();
                            cTimeOut.Restart();
                            SubNext(70);
                        }
                    }
                    break;

                case 70:    // Flip Z축 Up
                    {
                        if (ml.Axis[eMotor.FLIP_Z].MoveAbsolute((int)eAxisFlip_Z.FlipUp) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(71);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR24_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 71:    // Flip Z축 Up 확인
                    {
                        if (ml.Axis[eMotor.FLIP_Z].IsMoveDone() == true)
                        {
                            SubNext(72);
                        }
                    }
                    break;

                case 72:    // Flip 테이블 왼쪽 이동
                    {
                        cIO.SetOutput((int)eIO_O.FLIP_TABLE_LEFT, true);
                        cIO.SetOutput((int)eIO_O.FLIP_TABLE_RIGHT, false);
                        cTimeOut.Restart();
                        SubNext(73);
                    }
                    break;

                case 73:    // Flip 테이블 왼쪽 확인
                    {
                        if (cIO.GetInput((int)eIO_I.FLIP_TABLE_LEFT) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(74);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ00_FLIP_TABLE_LEFT_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 74:    // Flip T축 원점위치 이동
                    {
                        if (ml.Axis[eMotor.FLIP_T].MoveAbsolute((int)eAxisFlip_T.Origin) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(75);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR23_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 75:    // T축 이동 확인
                    {
                        if (ml.Axis[eMotor.FLIP_T].IsMoveDone() == true)
                        {
                            SubNext(100);
                        }
                    }
                    break;

                case 100:   // Flip 팔렛부분 맵데이터 변경
                    {
                        if (MPC1_Flip.GetStatus(eStatus.SKIPPED) == true)
                        {
                            MPC1_Flip.SetAllStatus(eStatus.SKIP_AFTER_FLIP);
                        }
                        else
                        //else if (MPC1_Flip.GetStatus(eStatus.NEEDLE_MOUNT) == true)
                        {
                            MPC1_Flip.SetAllStatus(eStatus.FLIP_DONE);
                        }
                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 플립 모터 및 실린더 초기화 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool FlipPosClear()
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
                        SubNext(10);
                    }
                    break;

                case 10:    // 플립 척 Close
                    {
                        cIO.SetOutput((int)eIO_O.FLIP_CHUCK_CLOSE, true);
                        cIO.SetOutput((int)eIO_O.FLIP_CHUCK_OPEN, false);
                        cTimeOut.Restart();
                        SubNext(11);
                    }
                    break;

                case 11:    // 척 실린더 Close 확인
                    {
                        if (cIO.GetInput((int)eIO_I.FLIP_CHUCK_CLOSE, true) == true)
                        {
                            cTimeOut.Stop();
                            cDelay.Restart();
                            SubNext(12);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ00_FLIP_CHUCK_CLOSE_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 12:    // 500ms 딜레이
                    {
                        if (cDelay.ElapsedMilliseconds > 500)
                        {
                            cDelay.Stop();
                            cTimeOut.Restart();
                            SubNext(13);
                        }
                    }
                    break;

                case 13:    // Flip Z축 Up 이동
                    {
                        if (ml.Axis[eMotor.FLIP_Z].MoveAbsolute((int)eAxisFlip_Z.FlipUp) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(14);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR24_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 14:    // Flip Z축 Up 이동 확인
                    {
                        if (ml.Axis[eMotor.FLIP_Z].IsMoveDone() == true)
                        {
                            SubNext(20);
                        }
                    }
                    break;

                case 20:    // 척 Open 후 플립 후진 및 T축 회전
                    {
                        // 플립 척 오픈
                        cIO.SetOutput((int)eIO_O.FLIP_CHUCK_CLOSE, false);
                        cIO.SetOutput((int)eIO_O.FLIP_CHUCK_OPEN, true);
                        cTimeOut.Restart();
                        SubNext(21);
                    }
                    break;

                case 21:    // 척 실린더 Open 확인
                    {
                        if (cIO.GetInput((int)eIO_I.FLIP_CHUCK_OPEN_ON, true) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(30);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ00_FLIP_CHUCK_OPEN_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 30:    // Flip T축이 포지션 값이 '0' 또는 '180'이 아니면 원점위치로 돌려놓는다.
                    {
                        if (ml.Axis[eMotor.FLIP_T].GetCmdPostion() == 0 ||
                            ml.Axis[eMotor.FLIP_T].GetCmdPostion() == 180)
                        {
                            SubNext(32);
                        }
                        else
                        {
                            cTimeOut.Restart();
                            SubNext(31);
                        }

                        // T축 움직임과 동시에 Flip 테이블 왼쪽 이동
                        cIO.SetOutput((int)eIO_O.FLIP_TABLE_LEFT, true);
                        cIO.SetOutput((int)eIO_O.FLIP_TABLE_RIGHT, false);
                    }
                    break;

                case 31:    // Flip T 축 원점으로 이동
                    {
                        if (ml.Axis[eMotor.FLIP_T].MoveAbsolute((int)eAxisFlip_T.Origin) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(32);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR23_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 32:    // Flip T축 원점위치 이동 확인
                    {
                        if (ml.Axis[eMotor.FLIP_T].IsMoveDone() == true)
                        {
                            cTimeOut.Restart();
                            SubNext(33);
                        }
                    }
                    break;

                case 33:    // Flip 테이블 실린더 왼쪽 확인
                    {
                        if (cIO.GetInput((int)eIO_I.FLIP_TABLE_LEFT, true) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(100);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ00_FLIP_TABLE_RIGHT_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 100:   // 플립 포지션 클리어 종료
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
                        if (ml.cVar.Manual_MPC1_MovePitch == true)
                        {
                            Next(10);
                            ml.cVar.Manual_MPC1_MovePitch = false;
                        }
                        else if (ml.cVar.Manual_HolderFlip == true)
                        {
                            Next(20);
                            ml.cVar.Manual_HolderFlip = false;
                        }
                        else if (ml.cVar.Manual_MPCAutoPitchMoveTest == true)
                        {
                            Next(30);
                            ml.cVar.Manual_MPCAutoPitchMoveTest = false;
                        }
                        else if (ml.cVar.Manual_FlipPosClear == true)
                        {
                            Next(40);
                            ml.cVar.Manual_FlipPosClear = false;
                        }
                    }
                    break;

                case 10:
                    {
                        if (MPCMovePitch() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 20:
                    {
                        if (FlipHolder() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 30:
                    {
                        if (MPCAutoPitchMoveTest() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 40:
                    {
                        if (FlipPosClear() == true)
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

        private Seq01_Rear_MPC_Left MPC2_Left_Rail = null;
        private Seq02_Rear_MPC_Right MPC2_Right_Rail = null;

        /// <summary>
        /// MPC 자동 간격이동 테스트 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool MPCAutoPitchMoveTest()
        {
            if (iManualStep != iPreManualStep)
            {
                iPreManualStep = iManualStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iManualStep} Process"), false);
            }

            switch (iManualStep)
            {
                case 0:     // MPC1 Pitch 이동 맵데이터 조건 확인
                    {
                        MPC2_Left_Rail = ml.Seq.Worker[eSequence.Seq01_MPC_Rail2_1] as Seq01_Rear_MPC_Left;
                        MPC2_Right_Rail = ml.Seq.Worker[eSequence.Seq02_MPC_Rail2_2] as Seq02_Rear_MPC_Right;
                        MPC2_Left_Rail.ClearSequence();
                        MPC2_Right_Rail.ClearSequence();
                        ManualNext(1);
                    }
                    break;

                case 1:
                    {
                        MapDataLib MPC1_FarLeft = ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_LEFT);
                        MapDataLib MPC1_Buffer1 = ml.cRunUnitData.GetIndexData(eData.MPC1_BUFFER_1);
                        MapDataLib MPC1_PipeMount = ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT);
                        MapDataLib MPC1_Buffer2 = ml.cRunUnitData.GetIndexData(eData.MPC1_BUFFER_2);
                        MapDataLib MPC1_NeedleMount = ml.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT);
                        MapDataLib MPC1_Flip = ml.cRunUnitData.GetIndexData(eData.MPC1_FLIP);
                        MapDataLib MPC1_Dispensing = ml.cRunUnitData.GetIndexData(eData.MPC1_DISPENSING);
                        MapDataLib MPC1_FarRight = ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_RIGHT);
                        MapDataLib MPC2_FarRight = ml.cRunUnitData.GetIndexData(eData.MPC2_FAR_RIGHT);
                        MapDataLib MPC2_UV = ml.cRunUnitData.GetIndexData(eData.MPC2_UV);
                        MapDataLib MPC2_FarLeft = ml.cRunUnitData.GetIndexData(eData.MPC2_FAR_LEFT);

                        bool bPitchMoveMapData = MPC1_FarLeft.GetAllStatus(eStatus.EMPTY, false) &&
                                                 MPC1_Buffer1.GetStatus(eStatus.EMPTY) &&
                                                 MPC1_PipeMount.GetAllStatus(eStatus.EMPTY, false) &&
                                                 MPC1_Buffer2.GetStatus(eStatus.EMPTY) &&
                                                 MPC1_NeedleMount.GetAllStatus(eStatus.EMPTY, true) &&
                                                 MPC1_Flip.GetStatus(eStatus.EMPTY) &&
                                                 MPC1_Dispensing.GetStatus(eStatus.EMPTY) &&
                                                 MPC1_FarRight.GetStatus(eStatus.NONE);

                        bool bFarRight_Y_MoveMapData = MPC1_FarRight.GetStatus(eStatus.EMPTY) &&
                                                       MPC2_FarRight.GetStatus(eStatus.NONE) &&
                                                       MPC2_UV.GetStatus(eStatus.NONE) &&
                                                       MPC2_FarLeft.GetStatus(eStatus.NONE);

                        bool bMPC2_MoveMapData = MPC2_FarRight.GetStatus(eStatus.EMPTY) &&
                                                 MPC2_UV.GetStatus(eStatus.NONE) &&
                                                 MPC2_FarLeft.GetStatus(eStatus.NONE);

                        bool bUVCure_MapData = MPC2_UV.GetStatus(eStatus.DISPENSING);

                        bool bUVPalletsMoveMapData = MPC2_UV.GetStatus(eStatus.EMPTY) &&
                                                     MPC2_FarLeft.GetStatus(eStatus.NONE) &&
                                                     MPC1_FarLeft.GetAllStatus(eStatus.NONE);

                        bool bFarLeft_Y_MoveMapData = MPC2_FarLeft.GetStatus(eStatus.EMPTY) &&
                                                      MPC1_FarLeft.GetAllStatus(eStatus.NONE);

                        bool bFarLeft_Y_MoveMPC1MapData = MPC1_FarLeft.GetAllStatus(eStatus.EMPTY, false);

                        if (bPitchMoveMapData == true)
                        {
                            ManualNext(10);
                        }
                        else if (bFarRight_Y_MoveMapData == true)
                        {
                            ManualNext(30);
                        }
                        else if (bMPC2_MoveMapData == true)
                        {
                            ManualNext(31);
                        }
                        else if (bUVPalletsMoveMapData == true)
                        {
                            ManualNext(20);
                        }
                        else if (bFarLeft_Y_MoveMapData == true)
                        {
                            ManualNext(21);
                        }
                        else if (bFarLeft_Y_MoveMPC1MapData == true)
                        {
                            ManualNext(22);
                        }
                        else if (bUVCure_MapData == true)
                        {
                            ManualNext(32);
                        }
                    }
                    break;

                case 10:    // MPC1 Pitch 이동 함수
                    {
                        if (MPCMovePitch() == true)
                        {
                            ManualNext(1);
                        }
                    }
                    break;

                case 20:    // UV 팔렛 맨 왼쪽 위치로 이동 함수
                    {
                        if (MPC2_Left_Rail.UVPalletsMoveFarLeft() == true)
                        {
                            ManualNext(1);
                        }
                    }
                    break;

                case 21:    // 맨 왼쪽 팔렛 홀더 공급 위치로 이동 함수
                    {
                        if (MPC2_Left_Rail.FarLeftPalletsMoveHolderSupply() == true)
                        {
                            ManualNext(1);
                        }
                    }
                    break;

                case 22:    // 맨 왼쪽 팔렛 MPC1 레일로 이동 함수
                    {
                        if (MPC2_Left_Rail.FarLeftPalletsMoveMPC1() == true)
                        {
                            ManualNext(100);
                        }
                    }
                    break;

                case 30:    // 맨 오른쪽 Y축 MPC2 레일로 이동 함수
                    {
                        if (MPC2_Right_Rail.FarRight_Y_MoveMPC2() == true)
                        {
                            ManualNext(1);
                        }
                    }
                    break;

                case 31:    // 맨 오른쪽 팔렛 UV 위치로 이동 함수
                    {
                        if (MPC2_Right_Rail.FarRightPalletsMoveUV() == true)
                        {
                            ManualNext(1);
                        }
                    }
                    break;

                case 32:    // 홀더 UV 큐어 함수
                    {
                        if (MPC2_Right_Rail.UV_Cure() == true)
                        {
                            ManualNext(1);
                        }
                    }
                    break;

                case 100:
                    {
                        ManualNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        #endregion Maunal
    }
}