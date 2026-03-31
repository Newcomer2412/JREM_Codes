using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// 시퀀스 클래스
    /// </summary>
    public class Seq01_Rear_MPC_Left : ISequence, ISeqNo
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
        public Seq01_Rear_MPC_Left(eSequence sequence)
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
            eLogType = eLogType.Seq01_Rear_MPC_Left;

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
                ml.AddError(eErrorCode.SEQ01_TIME_OUT);
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
                        MapDataLib HolderPicker = ml.cRunUnitData.GetIndexData(eData.HOLDER_PICKER);
                        MapDataLib SupplyHolderTray = ml.cRunUnitData.GetIndexData(eData.SUPPLY_HOLDER_TRAY);

                        MapDataLib MPC2_FarLeft = ml.cRunUnitData.GetIndexData(eData.MPC2_FAR_LEFT);
                        MapDataLib MPC2_UV = ml.cRunUnitData.GetIndexData(eData.MPC2_UV);

                        MapDataLib MPC1_FarLeft = ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_LEFT);
                        MapDataLib MPC1_Buffer1 = ml.cRunUnitData.GetIndexData(eData.MPC1_BUFFER_1);
                        MapDataLib MPC1_PipeMount = ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT);
                        MapDataLib MPC1_Buffer2 = ml.cRunUnitData.GetIndexData(eData.MPC1_BUFFER_2);
                        MapDataLib MPC1_NeedleMount = ml.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT);
                        MapDataLib MPC1_Flip = ml.cRunUnitData.GetIndexData(eData.MPC1_FLIP);
                        MapDataLib MPC1_Dispensing = ml.cRunUnitData.GetIndexData(eData.MPC1_DISPENSING);
                        MapDataLib MPC1_FarRight = ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_RIGHT);
                        MapDataLib MPC2_FarRight = ml.cRunUnitData.GetIndexData(eData.MPC2_FAR_RIGHT);

                        bool bUVPalletsMoveMapData = (MPC2_UV.GetStatus(eStatus.EMPTY) || MPC2_UV.GetStatus(eStatus.UV) || MPC2_UV.GetStatus(eStatus.NG)) &&
                                                      MPC2_FarLeft.GetStatus(eStatus.NONE) &&
                                                      MPC1_FarLeft.GetAllStatus(eStatus.NONE);

                        bool bFarLeft_Y_MoveMapData = (MPC2_FarLeft.GetStatus(eStatus.EMPTY) || MPC2_FarLeft.GetStatus(eStatus.UV) || MPC2_FarLeft.GetStatus(eStatus.NG)) &&
                                                       MPC1_FarLeft.GetAllStatus(eStatus.NONE);

                        bool bFarLeft_Y_MoveMPC1MapData = false;

                        if (ml.cVar.bHolder_SupplyStop == false)
                        {
                            if (SupplyHolderTray.GetStatus(eStatus.MOUNT) == true ||
                                HolderPicker.GetStatus(eStatus.MOUNT) == true)
                            {
                                bFarLeft_Y_MoveMPC1MapData = MPC1_FarLeft.GetUnitNo(0).eStatus == eStatus.MOUNT &&
                                                             MPC1_FarLeft.GetUnitNo(1).eStatus == eStatus.EMPTY &&
                                                             cIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_LEFT, true) == false;
                            }
                            else
                            {
                                bFarLeft_Y_MoveMPC1MapData = ((MPC1_FarLeft.GetUnitNo(0).eStatus == eStatus.MOUNT && MPC1_FarLeft.GetUnitNo(1).eStatus == eStatus.EMPTY) ||
                                                              MPC1_FarLeft.GetAllStatus(eStatus.EMPTY, false)) &&
                                                             (MPC2_FarLeft.GetStatus(eStatus.UV) || MPC2_FarLeft.GetStatus(eStatus.NG) ||
                                                              MPC2_UV.GetStatus(eStatus.UV) || MPC2_UV.GetStatus(eStatus.NG) ||
                                                              MPC1_FarRight.GetStatus(eStatus.DISPENSING) || MPC1_FarRight.GetStatus(eStatus.NG) ||
                                                              MPC2_FarRight.GetStatus(eStatus.DISPENSING) || MPC2_FarRight.GetStatus(eStatus.NG) ||
                                                              MPC1_Buffer1.GetStatus(eStatus.EMPTY) == false || MPC1_PipeMount.GetAllStatus(eStatus.EMPTY, false) == false ||
                                                              MPC1_Buffer2.GetStatus(eStatus.EMPTY) == false || MPC1_NeedleMount.GetAllStatus(eStatus.EMPTY, true) == false ||
                                                              MPC1_Flip.GetStatus(eStatus.EMPTY) == false || MPC1_Dispensing.GetStatus(eStatus.EMPTY) == false) &&
                                                              cIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_LEFT, true) == false;
                            }
                        }
                        else
                        {
                            if (HolderPicker.GetStatus(eStatus.MOUNT) == true)
                            {
                                bFarLeft_Y_MoveMPC1MapData = MPC1_FarLeft.GetUnitNo(0).eStatus == eStatus.MOUNT &&
                                                             MPC1_FarLeft.GetUnitNo(1).eStatus == eStatus.EMPTY &&
                                                             cIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_LEFT, true) == false;
                            }
                            else
                            {
                                bFarLeft_Y_MoveMPC1MapData = ((MPC1_FarLeft.GetUnitNo(0).eStatus == eStatus.MOUNT && MPC1_FarLeft.GetUnitNo(1).eStatus == eStatus.EMPTY) ||
                                                              MPC1_FarLeft.GetAllStatus(eStatus.EMPTY, false)) &&
                                                             (MPC2_FarLeft.GetStatus(eStatus.UV) || MPC2_FarLeft.GetStatus(eStatus.NG) ||
                                                              MPC2_UV.GetStatus(eStatus.UV) || MPC2_UV.GetStatus(eStatus.NG) ||
                                                              MPC1_FarRight.GetStatus(eStatus.DISPENSING) || MPC1_FarRight.GetStatus(eStatus.NG) ||
                                                              MPC2_FarRight.GetStatus(eStatus.DISPENSING) || MPC2_FarRight.GetStatus(eStatus.NG) ||
                                                              MPC1_Buffer1.GetStatus(eStatus.EMPTY) == false || MPC1_PipeMount.GetAllStatus(eStatus.EMPTY, false) == false ||
                                                              MPC1_Buffer2.GetStatus(eStatus.EMPTY) == false || MPC1_NeedleMount.GetAllStatus(eStatus.EMPTY, true) == false ||
                                                              MPC1_Flip.GetStatus(eStatus.EMPTY) == false || MPC1_Dispensing.GetStatus(eStatus.EMPTY) == false) &&
                                                              cIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_LEFT, true) == false;
                            }
                        }

                        if (bUVPalletsMoveMapData == true)
                        {
                            Next(10);
                        }
                        else if (bFarLeft_Y_MoveMapData == true)
                        {
                            Next(20);
                        }
                        else if (bFarLeft_Y_MoveMPC1MapData == true)
                        {
                            double GetActLeftRail_Y = ml.Axis[eMotor.MPC_LEFT_Y].GetCmdPostion();
                            double LeftRail_Y_FrontPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.MPC_LEFT_Y, (int)eAxisLeftMPC_Y.Front);
                            bool bMPC_Left_Y_FrontPos = GetActLeftRail_Y == LeftRail_Y_FrontPos;
                            if (bMPC_Left_Y_FrontPos == false)
                            {
                                Next(30);
                            }
                        }
                    }
                    break;

                case 10:    // UV 팔렛 맨 왼쪽 위치로 이동 함수
                    {
                        if (UVPalletsMoveFarLeft() == true)
                        {
                            Next(0);
                        }
                    }
                    break;

                case 20:    // 맨 왼쪽 팔렛 홀더 공급 위치로 이동 함수
                    {
                        if (FarLeftPalletsMoveHolderSupply() == true)
                        {
                            Next(0);
                        }
                    }
                    break;

                case 30:    // 맨 왼쪽 팔렛 MPC1 레일로 이동 함수
                    {
                        if (FarLeftPalletsMoveMPC1() == true)
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
        /// UV 팔렛 맨 왼쪽으로 이동하는 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool UVPalletsMoveFarLeft()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib MPC2_FarLeft = ml.cRunUnitData.GetIndexData(eData.MPC2_FAR_LEFT);
            MapDataLib MPC2_UV = ml.cRunUnitData.GetIndexData(eData.MPC2_UV);
            MapDataLib MPC1_FarLeft = ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_LEFT);

            switch (iSubStep)
            {
                case 0:     // UV 팔렛 -> 맨 왼쪽 위치이동 맵데이터 조건 확인
                    {
                        bool bUVPalletsMoveMapData = (MPC2_UV.GetStatus(eStatus.EMPTY) || MPC2_UV.GetStatus(eStatus.UV) || MPC2_UV.GetStatus(eStatus.NG)) &&
                                                      MPC2_FarLeft.GetStatus(eStatus.NONE) &&
                                                      MPC1_FarLeft.GetAllStatus(eStatus.NONE);

                        if (bUVPalletsMoveMapData == true)
                        {
                            ml.Axis[eMotor.MPC_REAR_X].SetZero();
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

                case 1:    // UV 홀더 고정 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_FIXTURE_UP_ON) == true ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Stop();
                            SubNext(10);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ02_UV_HOLDER_FIX_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                            else
                            {
                                cIO.SetOutput((int)eIO_O.UV_FIXTURE_DOWN, false);
                            }
                        }
                    }
                    break;

                case 10:    // MPC1, MPC2 맨 왼쪽 팔렛 감지센서 모두 Off 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_LEFT, true) == false &&
                            cIO.GetInput((int)eIO_I.PALLETS_DETECTION_REAR_LEFT, true) == false)
                        {
                            SubNext(11);
                        }
                        else
                        {
                            if (cIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_LEFT) == true)
                                ml.AddError(eErrorCode.SEQ01_PALLETS_DETECTION_FRONT_LEFT, iSubStep);
                            else if (cIO.GetInput((int)eIO_I.PALLETS_DETECTION_REAR_LEFT) == true)
                                ml.AddError(eErrorCode.SEQ01_PALLETS_DETECTION_REAR_LEFT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 11:    // MPC2 벨트 피치 센서 On 확인
                    {
                        if (cIO.GetInput((int)eIO_I.BELT_PITCH_DETECTION_LEFT) == true)
                        {
                            SubNext(12);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ01_REAR_MPC_BELT_PITCH_NOT_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 12:    // Left Pallets Y 이동 가이드 실린더 BWD 이동
                    {
                        cIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_LEFT, true);
                        cTimeOut.Restart();
                        SubNext(13);
                    }
                    break;

                case 13:    // Left Pallets Y 이동 가이드 실린더 BWD 확인
                    {
                        if (cIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_LEFT) == false ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Restart();
                            SubNext(14);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ01_LM_RAIL_GUIDE_LEFT_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 14:    // 홀더 고정 실린더 Up
                    {
                        cIO.SetOutput((int)eIO_O.UV_FIXTURE_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(15);
                    }
                    break;

                case 15:    // 홀더 고정 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_FIXTURE_UP_ON) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ02_UV_HOLDER_FIX_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // MPC 맨 왼쪽 Y축 Rear 위치 이동
                    {
                        if (ml.Axis[eMotor.MPC_LEFT_Y].MoveAbsolute((int)eAxisLeftMPC_Y.Rear) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR4_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // MPC 맨 왼쪽 Y축 도착 확인
                    {
                        if (ml.Axis[eMotor.MPC_LEFT_Y].IsMoveDone() == true)
                        {
                            SubNext(22);
                        }
                    }
                    break;

                case 22:    // Left Pallets Y 이동 가이드 실린더 FWD 이동
                    {
                        cIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_LEFT, false);
                        cTimeOut.Restart();
                        SubNext(23);
                    }
                    break;

                case 23:    // Left Pallets Y 이동 가이드 실린더 FWD 확인
                    {
                        if (cIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_LEFT) == true ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Restart();
                            SubNext(24);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ01_LM_RAIL_GUIDE_LEFT_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 24:    // MPC2 540mm 전진
                    {
                        if (ml.Axis[eMotor.MPC_REAR_X].MoveRelative((int)eAxisRearMPC.Unload_Pitch) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(25);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR3_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 25:    // MPC2 도착 확인
                    {
                        if (ml.Axis[eMotor.MPC_REAR_X].IsMoveDone() == true)
                        {
                            // MPC2 도착 했으면 맵데이터 미리 변경 변경
                            MPC2_FarLeft.GetUnitNo(0).eStatus = MPC2_UV.GetUnitNo(0).eStatus;
                            MPC2_UV.SetAllStatus(eStatus.NONE);
                            SubNext(30);
                        }
                    }
                    break;

                case 30:    // 후면 맨 왼쪽에 팔렛 감지 센서 On 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PALLETS_DETECTION_REAR_LEFT) == true)
                        {
                            SubNext(31);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ01_PALLETS_NOT_DETECTION_REAR_LEFT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 31:    // MPC2 벨트 피치 센서 On 확인 (정확한 Pitch 이동이 되었으면 벨트 안쪽면의 이빨을 감지해야 한다)
                    {
                        if (cIO.GetInput((int)eIO_I.BELT_PITCH_DETECTION_LEFT) == true)
                        {
                            SubNext(32);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ01_REAR_MPC_BELT_PITCH_NOT_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 32:
                    {
                        SubNext(100);
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
        /// 맨 왼쪽 팔렛 홀더 공급위치 이동하는 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool FarLeftPalletsMoveHolderSupply()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib MPC2_FarLeft = ml.cRunUnitData.GetIndexData(eData.MPC2_FAR_LEFT);
            MapDataLib MPC1_FarLeft = ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_LEFT);

            switch (iSubStep)
            {
                case 0:     // 맨 왼쪽 팔렛 MPC2 -> 홀더공급 위치이동 맵데이터 조건 확인
                    {
                        bool bFarLeft_Y_MoveMapData = (MPC2_FarLeft.GetStatus(eStatus.EMPTY) || MPC2_FarLeft.GetStatus(eStatus.UV) || MPC2_FarLeft.GetStatus(eStatus.NG)) &&
                                                       MPC1_FarLeft.GetAllStatus(eStatus.NONE);

                        if (bFarLeft_Y_MoveMapData == true)
                        {
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

                case 10:    // 전면 맨 왼쪽에 팔렛이 감지 안되는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_LEFT, true) == false)
                        {
                            SubNext(11);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ01_PALLETS_DETECTION_REAR_LEFT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 11:    // 후면 맨 왼쪽 팔렛 감지 센서 On 화인
                    {
                        if (cIO.GetInput((int)eIO_I.PALLETS_DETECTION_REAR_LEFT) == true)
                        {
                            SubNext(20);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ01_PALLETS_NOT_DETECTION_REAR_LEFT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 20:    // 왼쪽 LM 레일 실린더 BWD 이동
                    {
                        cIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_LEFT, true);
                        cTimeOut.Restart();
                        SubNext(21);
                    }
                    break;

                case 21:    // 왼쪽 LM 레일 실린더 BWD 확인
                    {
                        if (cIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_LEFT) == false ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Restart();
                            SubNext(22);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ01_LM_RAIL_GUIDE_LEFT_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 22:    // Left Pallets Y축 홀더 공급 위치로 이동
                    {
                        if (ml.Axis[eMotor.MPC_LEFT_Y].MoveAbsolute((int)eAxisLeftMPC_Y.HolderSupply) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(23);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR4_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 23:    // Left Pallets Y축 이동 완료 확인
                    {
                        if (ml.Axis[eMotor.MPC_LEFT_Y].IsMoveDone() == true)
                        {
                            // Left Pallets 이동 완료 했으면 맵데이터 미리 변경
                            MPC1_FarLeft.GetUnitNo(1).eStatus = MPC2_FarLeft.GetUnitNo(0).eStatus;
                            MPC1_FarLeft.GetUnitNo(0).eStatus = eStatus.EMPTY;
                            MPC2_FarLeft.SetAllStatus(eStatus.NONE);
                            SubNext(24);
                        }
                    }
                    break;

                case 24:    // 후면 맨 왼쪽 팔렛 감지 센서 Off 되었는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PALLETS_DETECTION_REAR_LEFT, true) == false)
                        {
                            SubNext(100);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ01_PALLETS_DETECTION_REAR_LEFT, iSubStep);
                            return true;
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
        /// 맨 왼쪽 팔렛 MPC1로 이동하는 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool FarLeftPalletsMoveMPC1()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib HolderPicker = ml.cRunUnitData.GetIndexData(eData.HOLDER_PICKER);
            MapDataLib EmptyHolderTray = ml.cRunUnitData.GetIndexData(eData.SUPPLY_HOLDER_TRAY);

            MapDataLib MPC2_FarLeft = ml.cRunUnitData.GetIndexData(eData.MPC2_FAR_LEFT);
            MapDataLib MPC2_UV = ml.cRunUnitData.GetIndexData(eData.MPC2_UV);

            MapDataLib MPC1_FarLeft = ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_LEFT);
            MapDataLib MPC1_Buffer1 = ml.cRunUnitData.GetIndexData(eData.MPC1_BUFFER_1);
            MapDataLib MPC1_PipeMount = ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT);
            MapDataLib MPC1_Buffer2 = ml.cRunUnitData.GetIndexData(eData.MPC1_BUFFER_2);
            MapDataLib MPC1_NeedleMount = ml.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT);
            MapDataLib MPC1_Flip = ml.cRunUnitData.GetIndexData(eData.MPC1_FLIP);
            MapDataLib MPC1_Dispensing = ml.cRunUnitData.GetIndexData(eData.MPC1_DISPENSING);
            MapDataLib MPC1_FarRight = ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_RIGHT);
            MapDataLib MPC2_FarRight = ml.cRunUnitData.GetIndexData(eData.MPC2_FAR_RIGHT);

            switch (iSubStep)
            {
                case 0:     // 맨 왼쪽 팔렛 MPC1 레일 위치이동 맵데이터 조건 확인
                    {
                        bool bFarLeft_Y_MoveMPC1MapData = false;

                        if (ml.cVar.bHolder_SupplyStop == false)
                        {
                            if (EmptyHolderTray.GetStatus(eStatus.MOUNT) == true ||
                                HolderPicker.GetStatus(eStatus.MOUNT) == true)
                            {
                                bFarLeft_Y_MoveMPC1MapData = MPC1_FarLeft.GetUnitNo(0).eStatus == eStatus.MOUNT &&
                                                             MPC1_FarLeft.GetUnitNo(1).eStatus == eStatus.EMPTY &&
                                                             cIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_LEFT, true) == false;
                            }
                            else
                            {
                                bFarLeft_Y_MoveMPC1MapData = ((MPC1_FarLeft.GetUnitNo(0).eStatus == eStatus.MOUNT && MPC1_FarLeft.GetUnitNo(1).eStatus == eStatus.EMPTY) ||
                                                              MPC1_FarLeft.GetAllStatus(eStatus.EMPTY, false)) &&
                                                             (MPC2_FarLeft.GetStatus(eStatus.UV) || MPC2_FarLeft.GetStatus(eStatus.NG) ||
                                                              MPC2_UV.GetStatus(eStatus.UV) || MPC2_UV.GetStatus(eStatus.NG) ||
                                                              MPC1_FarRight.GetStatus(eStatus.DISPENSING) || MPC1_FarRight.GetStatus(eStatus.NG) ||
                                                              MPC2_FarRight.GetStatus(eStatus.DISPENSING) || MPC2_FarRight.GetStatus(eStatus.NG) ||
                                                              MPC1_Buffer1.GetStatus(eStatus.EMPTY) == false || MPC1_PipeMount.GetAllStatus(eStatus.EMPTY, false) == false ||
                                                              MPC1_Buffer2.GetStatus(eStatus.EMPTY) == false || MPC1_NeedleMount.GetAllStatus(eStatus.EMPTY, true) == false ||
                                                              MPC1_Flip.GetStatus(eStatus.EMPTY) == false || MPC1_Dispensing.GetStatus(eStatus.EMPTY) == false) &&
                                                              cIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_LEFT, true) == false;
                            }
                        }
                        else
                        {
                            if (HolderPicker.GetStatus(eStatus.MOUNT) == true)
                            {
                                bFarLeft_Y_MoveMPC1MapData = MPC1_FarLeft.GetUnitNo(0).eStatus == eStatus.MOUNT &&
                                                             MPC1_FarLeft.GetUnitNo(1).eStatus == eStatus.EMPTY &&
                                                             cIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_LEFT, true) == false;
                            }
                            else
                            {
                                bFarLeft_Y_MoveMPC1MapData = ((MPC1_FarLeft.GetUnitNo(0).eStatus == eStatus.MOUNT && MPC1_FarLeft.GetUnitNo(1).eStatus == eStatus.EMPTY) ||
                                                              MPC1_FarLeft.GetAllStatus(eStatus.EMPTY, false)) &&
                                                             (MPC2_FarLeft.GetStatus(eStatus.UV) || MPC2_FarLeft.GetStatus(eStatus.NG) ||
                                                              MPC2_UV.GetStatus(eStatus.UV) || MPC2_UV.GetStatus(eStatus.NG) ||
                                                              MPC1_FarRight.GetStatus(eStatus.DISPENSING) || MPC1_FarRight.GetStatus(eStatus.NG) ||
                                                              MPC2_FarRight.GetStatus(eStatus.DISPENSING) || MPC2_FarRight.GetStatus(eStatus.NG) ||
                                                              MPC1_Buffer1.GetStatus(eStatus.EMPTY) == false || MPC1_PipeMount.GetAllStatus(eStatus.EMPTY, false) == false ||
                                                              MPC1_Buffer2.GetStatus(eStatus.EMPTY) == false || MPC1_NeedleMount.GetAllStatus(eStatus.EMPTY, true) == false ||
                                                              MPC1_Flip.GetStatus(eStatus.EMPTY) == false || MPC1_Dispensing.GetStatus(eStatus.EMPTY) == false) &&
                                                              cIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_LEFT, true) == false;
                            }
                        }

                        if (bFarLeft_Y_MoveMPC1MapData == true)
                        {
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

                case 10:    // 전면 맨 왼쪽에 팔렛이 감지 안되는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_LEFT, true) == false)
                        {
                            SubNext(11);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ01_PALLETS_DETECTION_REAR_LEFT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 11:    // MPC1 벨트 피치 센서 On 확인
                    {
                        if (cIO.GetInput((int)eIO_I.BELT_PITCH_DETECTION_RIGHT) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(15);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ01_FRONT_MPC_BELT_PITCH_NOT_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                // Left MPC 레일 실린더 BWD
                case 15:
                    {
                        cIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_LEFT, true);
                        cTimeOut.Restart();
                        SubNext(16);
                    }
                    break;

                case 16:    // Left MPC 레일 실린더 BWD 상태인지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_LEFT) == false)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ01_LM_RAIL_GUIDE_LEFT_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // Left Pallets Y축 MPC1 위치로 이동
                    {
                        if (ml.Axis[eMotor.MPC_LEFT_Y].MoveAbsolute((int)eAxisLeftMPC_Y.Front) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR4_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // Left Pallets Y축 이동 완료 확인
                    {
                        if (ml.Axis[eMotor.MPC_LEFT_Y].IsMoveDone() == true)
                        {
                            // Left Pallets Y축 이동 완료하면 맵데이터 미리 변경
                            if (MPC1_FarLeft.GetUnitNo(0).eStatus == eStatus.MOUNT)
                            {
                                MPC1_FarLeft.GetUnitNo(0).eStatus = eStatus.HOLDER;
                            }
                            SubNext(22);
                        }
                    }
                    break;

                case 22:    // 후면 맨 왼쪽에 팔렛이 감지 안되는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PALLETS_DETECTION_REAR_LEFT, true) == false)
                        {
                            SubNext(23);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ01_PALLETS_DETECTION_REAR_LEFT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 23:    // 전면 맨 왼쪽에 팔렛 감지 센서 On 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_LEFT) == true)
                        {
                            SubNext(30);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ01_PALLETS_NOT_DETECTION_FRONT_LEFT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 30:    // 왼쪽 LM 레일 실린더 FWD 이동
                    {
                        cIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_LEFT, false);
                        cTimeOut.Restart();
                        SubNext(31);
                    }
                    break;

                case 31:    // 왼쪽 LM 레일 실린더 FWD 확인
                    {
                        if (cIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_LEFT, true) == true ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Stop();
                            SubNext(100);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ01_LM_RAIL_GUIDE_LEFT_CYLINDER_TIMEOUT, iSubStep);
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
                        if (ml.cVar.Manual_MPC2_MoveFarLeft == true)
                        {
                            Next(10);
                            ml.cVar.Manual_MPC2_MoveFarLeft = false;
                        }
                        else if (ml.cVar.Manual_FarLeft_Y_MoveHolser == true)
                        {
                            Next(20);
                            ml.cVar.Manual_FarLeft_Y_MoveHolser = false;
                        }
                        else if (ml.cVar.Manual_FarLeft_Y_MoveMPC1 == true)
                        {
                            Next(30);
                            ml.cVar.Manual_FarLeft_Y_MoveMPC1 = false;
                        }
                    }
                    break;

                case 10:    // UV 팔렛 맨 왼쪽 위치로 이동 함수
                    {
                        if (UVPalletsMoveFarLeft() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 20:    // 맨 왼쪽 팔렛 홀더 공급 위치로 이동 함수
                    {
                        if (FarLeftPalletsMoveHolderSupply() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 30:    // 맨 왼쪽 팔렛 MPC1 레일로 이동 함수
                    {
                        if (FarLeftPalletsMoveMPC1() == true)
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