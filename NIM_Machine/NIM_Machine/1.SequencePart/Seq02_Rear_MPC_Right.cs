using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// 시퀀스 클래스
    /// </summary>
    public class Seq02_Rear_MPC_Right : ISequence, ISeqNo
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
        public Seq02_Rear_MPC_Right(eSequence sequence)
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
            eLogType = eLogType.Seq02_Rear_MPC_Right;

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
                ml.AddError(eErrorCode.SEQ02_TIME_OUT);
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
                        MapDataLib MPC1_FarRight = ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_RIGHT);
                        MapDataLib MPC2_FarRight = ml.cRunUnitData.GetIndexData(eData.MPC2_FAR_RIGHT);
                        MapDataLib MPC2_UV = ml.cRunUnitData.GetIndexData(eData.MPC2_UV);
                        MapDataLib MPC2_FarLeft = ml.cRunUnitData.GetIndexData(eData.MPC2_FAR_LEFT);

                        bool bFarRight_Y_MoveMapData = (MPC1_FarRight.GetStatus(eStatus.DISPENSING) || MPC1_FarRight.GetStatus(eStatus.EMPTY) || MPC1_FarRight.GetStatus(eStatus.NG)) &&
                                                        MPC2_FarRight.GetStatus(eStatus.NONE) &&
                                                        MPC2_UV.GetStatus(eStatus.NONE) &&
                                                        MPC2_FarLeft.GetStatus(eStatus.NONE);

                        bool bMPC2_MoveMapData = (MPC2_FarRight.GetStatus(eStatus.DISPENSING) || MPC2_FarRight.GetStatus(eStatus.EMPTY) || MPC2_FarRight.GetStatus(eStatus.NG)) &&
                                                  MPC2_UV.GetStatus(eStatus.NONE) &&
                                                  MPC2_FarLeft.GetStatus(eStatus.NONE);

                        bool bUVCure_MapData = MPC2_UV.GetStatus(eStatus.DISPENSING);

                        if (bFarRight_Y_MoveMapData == true)
                        {
                            Next(10);
                        }
                        else if (bMPC2_MoveMapData == true)
                        {
                            Next(20);
                        }
                        else if (bUVCure_MapData == true)
                        {
                            Next(30);
                        }
                    }
                    break;

                case 10:    // 맨 오른쪽 Y축 MPC2 레일로 이동 함수
                    {
                        if (FarRight_Y_MoveMPC2() == true)
                        {
                            Next(0);
                        }
                    }
                    break;

                case 20:    // 맨 오른쪽 팔렛 UV 위치로 이동 함수
                    {
                        if (FarRightPalletsMoveUV() == true)
                        {
                            Next(0);
                        }
                    }
                    break;

                case 30:    // 홀더 UV 큐어 함수
                    {
                        if (UV_Cure() == true)
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
        /// 맨 오른쪽 Y축 MPC2 레일로 이동하는 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool FarRight_Y_MoveMPC2()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib MPC1_FarRight = ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_RIGHT);
            MapDataLib MPC2_FarRight = ml.cRunUnitData.GetIndexData(eData.MPC2_FAR_RIGHT);
            MapDataLib MPC2_UV = ml.cRunUnitData.GetIndexData(eData.MPC2_UV);
            MapDataLib MPC2_FarLeft = ml.cRunUnitData.GetIndexData(eData.MPC2_FAR_LEFT);

            switch (iSubStep)
            {
                case 0:     // 맨 오른쪽 팔렛 MPC2 레일로 이동 맵데이터 조건 확인
                    {
                        bool bFarRight_Y_MoveMapData = (MPC1_FarRight.GetStatus(eStatus.DISPENSING) || MPC1_FarRight.GetStatus(eStatus.EMPTY) || MPC1_FarRight.GetStatus(eStatus.NG)) &&
                                                        MPC2_FarRight.GetStatus(eStatus.NONE) &&
                                                        MPC2_UV.GetStatus(eStatus.NONE) &&
                                                        MPC2_FarLeft.GetStatus(eStatus.NONE);

                        if (bFarRight_Y_MoveMapData == true)
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

                case 10:    // MPC1 맨 오른쪽 팔렛 감지센서 On 확인, MPC2 맨 오른쪽 팔렛 감지센서 Off 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_RIGHT) == true &&
                            cIO.GetInput((int)eIO_I.PALLETS_DETECTION_REAR_RIGHT, true) == false)
                        {
                            SubNext(11);
                        }
                        else
                        {
                            if (cIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_RIGHT) == false)
                                ml.AddError(eErrorCode.SEQ02_PALLETS_NOT_DETECTION_FRONT_RIGHT, iSubStep);
                            else if (cIO.GetInput((int)eIO_I.PALLETS_DETECTION_REAR_RIGHT) == true)
                                ml.AddError(eErrorCode.SEQ02_PALLETS_NOT_DETECTION_REAR_RIGHT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 11:    // MPC1, MPC2 두가지 벨트 Pitch 센서 On 확인 (벨트 Pitch 센서가 모두 'On'이여야 Y축 이동시 팔렛이 벨트에 맞물린다)
                    {
                        if (cIO.GetInput((int)eIO_I.BELT_PITCH_DETECTION_RIGHT) == true &&
                            cIO.GetInput((int)eIO_I.BELT_PITCH_DETECTION_LEFT) == true)
                        {
                            SubNext(20);
                        }
                        else
                        {
                            if (cIO.GetInput((int)eIO_I.BELT_PITCH_DETECTION_RIGHT) == false)
                                ml.AddError(eErrorCode.SEQ02_FRONT_MPC_BELT_PITCH_NOT_DETECTION, iSubStep);
                            else if (cIO.GetInput((int)eIO_I.BELT_PITCH_DETECTION_LEFT) == false)
                                ml.AddError(eErrorCode.SEQ02_REAR_MPC_BELT_PITCH_NOT_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 20:    // 오른쪽 LM 레일 실린더 BWD 동작
                    {
                        cIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_RIGHT, true);
                        cTimeOut.Restart();
                        SubNext(21);
                    }
                    break;

                case 21:    // 오른쪽 LM 레일 실린더 BWD 확인
                    {
                        if (cIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_RIGHT) == false ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Restart();
                            cDelay.Restart();
                            SubNext(22);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ02_LM_RAIL_GRUIDE_RIGHT_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 22:    // MPC 맨 오른쪽 Y축 MPC2 위치로 이동
                    {
                        if (cDelay.ElapsedMilliseconds < 1000) break;
                        cDelay.Stop();

                        if (ml.Axis[eMotor.MPC_RIGHT_Y].MoveAbsolute((int)eAxisRightMPC_Y.Rear) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(23);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR5_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 23:    // 맨 오른쪽 Y축 이동 확인
                    {
                        if (ml.Axis[eMotor.MPC_RIGHT_Y].IsMoveDone() == true)
                        {
                            // 맨 오른쪽 Y축 이동 완료했으면 맵데이터 미리 변경
                            MPC2_FarRight.GetUnitNo(0).eStatus = MPC1_FarRight.GetUnitNo(0).eStatus;
                            MPC1_FarRight.SetAllStatus(eStatus.NONE);
                            SubNext(30);
                        }
                    }
                    break;

                case 30:    // MPC1 맨 오른쪽 팔렛 감지센서 Off 확인, MPC2 맨 오른쪽 팔렛 감지센서 On 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_RIGHT, true) == false &&
                            cIO.GetInput((int)eIO_I.PALLETS_DETECTION_REAR_RIGHT) == true)
                        {
                            SubNext(31);
                        }
                        else
                        {
                            if (cIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_RIGHT) == false)
                                ml.AddError(eErrorCode.SEQ02_PALLETS_NOT_DETECTION_FRONT_RIGHT, iSubStep);
                            else if (cIO.GetInput((int)eIO_I.PALLETS_DETECTION_REAR_RIGHT) == true)
                                ml.AddError(eErrorCode.SEQ02_PALLETS_NOT_DETECTION_REAR_RIGHT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 31:    // 오른쪽 LM 레일 실린더 FWD 동작
                    {
                        cIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_RIGHT, false);
                        cTimeOut.Restart();
                        SubNext(32);
                    }
                    break;

                case 32:    // 오른쪽 LM 레일 실린더 FWD 확인
                    {
                        if (cIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_RIGHT) == true ||
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
                                ml.AddError(eErrorCode.SEQ02_LM_RAIL_GRUIDE_RIGHT_CYLINDER_TIMEOUT, iSubStep);
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
        /// 맨 오른쪽 팔렛 UV 위치로 이동 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool FarRightPalletsMoveUV()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib MPC2_FarRight = ml.cRunUnitData.GetIndexData(eData.MPC2_FAR_RIGHT);
            MapDataLib MPC2_UV = ml.cRunUnitData.GetIndexData(eData.MPC2_UV);
            MapDataLib MPC2_FarLeft = ml.cRunUnitData.GetIndexData(eData.MPC2_FAR_LEFT);

            switch (iSubStep)
            {
                case 0:     // UV Cure 맵데이터 조건 확인
                    {
                        bool bMPC2_MoveMapData = (MPC2_FarRight.GetStatus(eStatus.DISPENSING) || MPC2_FarRight.GetStatus(eStatus.EMPTY) || MPC2_FarRight.GetStatus(eStatus.NG)) &&
                                                  MPC2_UV.GetStatus(eStatus.NONE) &&
                                                  MPC2_FarLeft.GetStatus(eStatus.NONE);

                        if (bMPC2_MoveMapData == true)
                        {
                            ml.Axis[eMotor.MPC_REAR_X].SetZero();
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

                case 10:    // MPC2 맨 오른쪽 팔렛 감지센서 On 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PALLETS_DETECTION_REAR_RIGHT) == true)
                        {
                            SubNext(11);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ02_PALLETS_NOT_DETECTION_REAR_RIGHT, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 11:    //MPC2 벨트 Pitch 센서 On 확인
                    {
                        if (cIO.GetInput((int)eIO_I.BELT_PITCH_DETECTION_LEFT) == true)
                        {
                            SubNext(12);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ02_REAR_MPC_BELT_PITCH_NOT_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 12:    // 오른쪽 LM 레일 실린더 FWD 동작
                    {
                        cIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_RIGHT, false);
                        cTimeOut.Restart();
                        SubNext(13);
                    }
                    break;

                case 13:    // 오른쪽 LM 레일 실린더 FWD 확인
                    {
                        if (cIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_RIGHT) == true ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Stop();
                            SubNext(14);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ02_LM_RAIL_GRUIDE_RIGHT_CYLINDER_TIMEOUT, iSubStep);
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

                case 20:    // MPC2 팔렛 UV존으로 이동
                    {
                        if (ml.Axis[eMotor.MPC_REAR_X].MoveRelative((int)eAxisRearMPC.UV_Zone) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
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

                case 21:
                    {
                        if (ml.Axis[eMotor.MPC_REAR_X].GetActPostion() >= 180 ||
                            Define.SIMULATION == true)
                        {
                            SubNext(30);
                        }
                    }
                    break;

                case 30:    // 오른쪽 LM 레일 실린더 BWD 동작
                    {
                        cIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_RIGHT, true);
                        cTimeOut.Restart();
                        SubNext(31);
                    }
                    break;

                case 31:    // 오른쪽 LM 레일 실린더 FWD 확인
                    {
                        if (cIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_RIGHT) == false ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Restart();
                            SubNext(32);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ02_LM_RAIL_GRUIDE_RIGHT_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 32:    // MPC 맨 오른쪽 Y축 MPC1 위치로 복귀
                    {
                        if (ml.Axis[eMotor.MPC_RIGHT_Y].MoveAbsolute((int)eAxisRightMPC_Y.Front) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(33);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR5_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 33:    // 맨 오른쪽 Y축 이동 확인
                    {
                        if (ml.Axis[eMotor.MPC_RIGHT_Y].IsMoveDone() == true)
                        {
                            SubNext(34);
                        }
                    }
                    break;

                case 34:    // 오른쪽 LM 레일 실린더 BWD 동작
                    {
                        cIO.SetOutput((int)eIO_O.LM_RAIL_FORWARD_RIGHT, false);
                        cTimeOut.Restart();
                        SubNext(35);
                    }
                    break;

                case 35:    // 오른쪽 LM 레일 실린더 FWD 확인
                    {
                        if (cIO.GetInput((int)eIO_I.LM_RAIL_FORWARD_ON_RIGHT) == true ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Stop();
                            SubNext(40);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ02_LM_RAIL_GRUIDE_RIGHT_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 40:    // MPC2 이동 확인
                    {
                        if (ml.Axis[eMotor.MPC_REAR_X].IsMoveDone() == true)
                        {
                            // MPC2 이동 완료했으면 맵데이터 미리 변경
                            MPC2_UV.GetUnitNo(0).eStatus = MPC2_FarRight.GetUnitNo(0).eStatus;
                            MPC2_FarRight.SetAllStatus(eStatus.NONE);
                            SubNext(41);
                        }
                    }
                    break;

                case 41:    // MPC2 맨 오른쪽 팔렛 감지센서 Off 확인
                    {
                        if (cIO.GetInput((int)eIO_I.PALLETS_DETECTION_REAR_RIGHT, true) == false)
                        {
                            SubNext(100);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ02_PALLETS_DETECTION_REAR_RIGHT, iSubStep);
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

        private int iUV_Recount = 1;

        /// <summary>
        /// 홀더 UV 큐어
        /// </summary>
        /// <returns></returns>
        public bool UV_Cure()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib MPC2_UV = ml.cRunUnitData.GetIndexData(eData.MPC2_UV);

            switch (iSubStep)
            {
                case 0:     // UV Cure 맵데이터 조건 확인
                    {
                        bool bUVCure_MapData = MPC2_UV.GetStatus(eStatus.DISPENSING);

                        if (bUVCure_MapData == true)
                        {
                            iUV_Recount = 1;
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

                case 10:    // UV 홀더 고정 실린더 Down
                    {
                        cIO.SetOutput((int)eIO_O.UV_FIXTURE_DOWN, true);
                        cTimeOut.Restart();
                        SubNext(11);
                    }
                    break;

                case 11:    // UV 홀더 고정 실린더 Down 확인
                    {

                        if (cIO.GetInput((int)eIO_I.HOLDER_FIXTURE_UP_ON) == false ||
                        Define.SIMULATION == true)
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
                                ml.AddError(eErrorCode.SEQ02_UV_HOLDER_FIX_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 12:
                    {
                        if (cDelay.ElapsedMilliseconds > 2000)
                        {
                            cDelay.Stop();
                            cDelay.Restart();
                            SubNext(20);
                        }
                    }
                    break;

                case 20:    // UV On
                    {
                        if (ml.cOptionData.bDryRunUse == false) ml.cUV_Ctl.UV_On();
                        SubNext(21);
                    }
                    break;
                // UV 후 뒷면에 니들이 조금 삐져나오는 현상을 방지하기 위해
                // UV 시 가이드를 내린 상태로 유지, UV 후 이동하기 전까지 가이드 내린 상태로 유지하도록 변경
                case 21: // 큐어 시간 초과 후 UV Off
                    {
                        if (cDelay.ElapsedMilliseconds > cSysArray.dUVCureDelay * 1000)
                        {
                            cDelay.Stop();
                            if (ml.cOptionData.bDryRunUse == false) ml.cUV_Ctl.UV_Off();
                            SubNext(100);
                        }
                    }
                    break;

                //case 21:    // 큐어 시간 초과 후 UV Off
                //    {
                //        if (cDelay.ElapsedMilliseconds > cSysArray.dUVCureDelay * 1000)
                //        {
                //            cDelay.Stop();
                //            if (ml.cOptionData.bDryRunUse == false) ml.cUV_Ctl.UV_Off();
                //            SubNext(100);
                //        }
                //        else
                //        {
                //            if (cDelay.ElapsedMilliseconds > 2000 * iUV_Recount)
                //            {
                //                cDelay.Stop();
                //                cTimeOut.Restart();
                //                if (ml.cOptionData.bDryRunUse == false) ml.cUV_Ctl.UV_Off();
                //                SubNext(22);
                //            }
                //        }
                //    }
                //    break;

                //case 22:    // UV 홀더 고정 실린더 Up
                //    {
                //        cIO.SetOutput((int)eIO_O.UV_FIXTURE_DOWN, false);
                //        cTimeOut.Restart();
                //        SubNext(23);
                //    }
                //    break;

                //case 23:    // UV 홀더 고정 실린더 Down 확인
                //    {
                //        if (cIO.GetInput((int)eIO_I.HOLDER_FIXTURE_UP_ON) == true ||
                //            Define.SIMULATION == true)
                //        {
                //            cTimeOut.Stop();
                //            SubNext(24);
                //        }
                //        else
                //        {
                //            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                //            {
                //                cTimeOut.Stop();
                //                ml.AddError(eErrorCode.SEQ02_UV_HOLDER_FIX_CYLINDER_TIMEOUT, iSubStep);
                //                return true;
                //            }
                //        }
                //    }
                //    break;

                //case 24:    // UV 홀더 고정 실린더 Down
                //    {
                //        cIO.SetOutput((int)eIO_O.UV_FIXTURE_DOWN, true);
                //        cTimeOut.Restart();
                //        SubNext(25);
                //    }
                //    break;

                //case 25:    // UV 홀더 고정 실린더 Down 확인
                //    {
                //        if (cIO.GetInput((int)eIO_I.HOLDER_FIXTURE_DOWN) == true ||
                //            Define.SIMULATION == true)
                //        {
                //            cTimeOut.Restart();
                //            SubNext(26);
                //        }
                //        else
                //        {
                //            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                //            {
                //                cTimeOut.Stop();
                //                ml.AddError(eErrorCode.SEQ02_UV_HOLDER_FIX_CYLINDER_TIMEOUT, iSubStep);
                //                return true;
                //            }
                //        }
                //    }
                //    break;

                //case 26:
                //    {
                //        if (cTimeOut.ElapsedMilliseconds > 500)
                //        {
                //            cTimeOut.Stop();
                //            cDelay.Start();
                //            iUV_Recount++;
                //            SubNext(20);
                //        }
                //    }
                //    break;

                //case 30:    // UV 홀더 고정 실린더 Up
                //    {
                //        cIO.SetOutput((int)eIO_O.UV_FIXTURE_DOWN, false);
                //        cTimeOut.Restart();
                //        SubNext(31);
                //    }
                //    break;

                //case 31:    // UV 홀더 고정 실린더 Up 확인
                //    {
                //        if (cIO.GetInput((int)eIO_I.HOLDER_FIXTURE_UP_ON) == true ||
                //            Define.SIMULATION == true)
                //        {
                //            cTimeOut.Stop();
                //            SubNext(100);
                //        }
                //        else
                //        {
                //            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                //            {
                //                cTimeOut.Stop();
                //                ml.AddError(eErrorCode.SEQ02_UV_HOLDER_FIX_CYLINDER_TIMEOUT, iSubStep);
                //                return true;
                //            }
                //        }
                //    }
                //    break;

                case 100:   // UV Cure 팔렛부분 맵데이터 변경
                    {
                        MPC2_UV.SetAllStatus(eStatus.UV);
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
                        if (ml.cVar.Manual_FarRight_Y_MoveMPC2 == true)
                        {
                            Next(10);
                            ml.cVar.Manual_FarRight_Y_MoveMPC2 = false;
                        }
                        else if (ml.cVar.Manual_FarRightPalletsMoveUV == true)
                        {
                            Next(20);
                            ml.cVar.Manual_FarRightPalletsMoveUV = false;
                        }
                        else if (ml.cVar.Manual_UV_Cure == true)
                        {
                            Next(30);
                            ml.cVar.Manual_UV_Cure = false;
                        }
                    }
                    break;

                case 10:    // 맨 오른쪽 Y축 MPC2 레일로 이동 함수
                    {
                        if (FarRight_Y_MoveMPC2() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 20:    // 맨 오른쪽 팔렛 UV 위치로 이동 함수
                    {
                        if (FarRightPalletsMoveUV() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 30:    // 홀더 UV 큐어 함수
                    {
                        if (UV_Cure() == true)
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