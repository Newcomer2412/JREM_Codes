using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// 시퀀스 클래스
    /// </summary>
    public class Seq03_HolderSnR : ISequence, ISeqNo
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
        public Seq03_HolderSnR(eSequence sequence)
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
            eLogType = eLogType.Seq03_HolderS_R;

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
            ml.cVar.swUPH.Start();

            // Start to Stop 생산 카운트 초기화
            ml.cSysOne.iProductCount = 0;
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
            ml.cVar.swUPH.Stop();
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
                ml.AddError(eErrorCode.SEQ03_TIME_OUT);
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
                        MapDataLib EmptyHolderTray = ml.cRunUnitData.GetIndexData(eData.SUPPLY_HOLDER_TRAY);
                        MapDataLib WorkDoneHolderTray = ml.cRunUnitData.GetIndexData(eData.WORK_DONE_HOLDER_TRAY);
                        MapDataLib MPC1_FarLeft = ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_LEFT);
                        MapDataLib NG_HolderTray = ml.cRunUnitData.GetIndexData(eData.NG_HOLDER_TRAY);

                        if (HolderPicker != null && EmptyHolderTray != null && WorkDoneHolderTray != null && MPC1_FarLeft != null && NG_HolderTray != null)
                        {
                            // 빈 홀더 픽업 맵데이터 조건 확인
                            bool bEmptyHolderPickUpMapData = HolderPicker.GetStatus(eStatus.EMPTY) &&
                                                             EmptyHolderTray.GetStatus(eStatus.MOUNT) &&
                                                             MPC1_FarLeft.GetStatus(eStatus.UV) == false &&
                                                             MPC1_FarLeft.GetStatus(eStatus.NG) == false &&
                                                             ml.cVar.bHolder_SupplyStop == false;

                            // 빈 홀더 공급 맵데이터 조건 확인
                            bool bHolderSupplyMapData = HolderPicker.GetStatus(eStatus.MOUNT) &&
                                                        MPC1_FarLeft.GetUnitNo(0).eStatus == eStatus.EMPTY;

                            double GetActLeftRail_Y = ml.Axis[eMotor.MPC_LEFT_Y].GetActPostion();
                            double LeftRail_Y_FrontPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.MPC_LEFT_Y, (int)eAxisLeftMPC_Y.HolderSupply);
                            bool Left_Y_MPC_PosCheck = GetActLeftRail_Y >= LeftRail_Y_FrontPos - 0.05 && GetActLeftRail_Y <= LeftRail_Y_FrontPos + 0.05;

                            // 작업 완료된 홀더, Tray에 회수 작업 맵데이터 조건 확인
                            bool bWorkDoneHolderPickUpMapData = HolderPicker.GetStatus(eStatus.EMPTY) &&
                                                           (MPC1_FarLeft.GetUnitNo(1).eStatus == eStatus.UV || MPC1_FarLeft.GetUnitNo(1).eStatus == eStatus.NG);

                            // 작업 완료된 홀더, Tray에 회수 작업 맵데이터 조건 확인
                            bool bWorkDoneHolderPlaceMapData = HolderPicker.GetStatus(eStatus.UV) &&
                                                               WorkDoneHolderTray.GetStatus(eStatus.EMPTY);

                            // NG 홀더, Tray에 회수 작업 맵데이터 조건 확인
                            bool bNG_HolderPlaceMapData = HolderPicker.GetStatus(eStatus.NG) &&
                                                          NG_HolderTray.GetStatus(eStatus.EMPTY);

                            if (bEmptyHolderPickUpMapData == true)
                            {
                                Next(10);
                            }
                            else if (bHolderSupplyMapData == true &&
                                     Left_Y_MPC_PosCheck == true)
                            {
                                Next(20);
                            }
                            else if (bWorkDoneHolderPickUpMapData == true &&
                                     Left_Y_MPC_PosCheck == true)
                            {
                                Next(30);
                            }
                            else if (bWorkDoneHolderPlaceMapData == true)
                            {
                                Next(40);
                            }
                            else if (bNG_HolderPlaceMapData == true &&
                                     Left_Y_MPC_PosCheck == true)
                            {
                                Next(50);
                            }
                        }
                    }
                    break;

                case 10:    // 빈 홀더 픽업 함수
                    {
                        if (EmptyHolderPickUp() == true)
                        {
                            Next(0);
                        }
                    }
                    break;

                case 20:    // 빈 홀더 공급 함수
                    {
                        if (HolderSupply() == true)
                        {
                            Next(0);
                        }
                    }
                    break;

                case 30:    // 완료된 홀더 픽업 함수
                    {
                        if (DoneHolderPickUp() == true)
                        {
                            Next(0);
                        }
                    }
                    break;

                case 40:    // 완료된 홀더 회수 함수
                    {
                        if (HolderRecovery() == true)
                        {
                            Next(0);
                        }
                    }
                    break;

                case 50:
                    {
                        if (NG_HolderRecovery() == true)
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
        /// 빈 홀더 픽업 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool EmptyHolderPickUp()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib HolderPicker = ml.cRunUnitData.GetIndexData(eData.HOLDER_PICKER);
            MapDataLib EmptyHolderTray = ml.cRunUnitData.GetIndexData(eData.SUPPLY_HOLDER_TRAY);
            MapDataLib MPC1_FarLeft = ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_LEFT);

            switch (iSubStep)
            {
                case 0:     // 빈 홀더 픽업 맵데이터 조건 확인
                    {
                        bool bEmptyHolderPickUpMapData = HolderPicker.GetStatus(eStatus.EMPTY) &&
                                                         EmptyHolderTray.GetStatus(eStatus.MOUNT) &&
                                                         MPC1_FarLeft.GetStatus(eStatus.UV) == false &&
                                                         ml.cVar.bHolder_SupplyStop == false;

                        if (bEmptyHolderPickUpMapData == true)
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

                case 10:    // Holder Picker에 홀더가 없는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_DETECTION, true) == false)
                        {
                            SubNext(11);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_HOLDER_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 11:    // Holder Picker 척 실린더 Up 및 Open 진행
                    {
                        cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_DOWN, false);
                        cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_OPEN, true);
                        cTimeOut.Restart();
                        SubNext(12);
                    }
                    break;

                case 12:    // Holder Picker 척 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_UP_ON) == true)
                        {
                            SubNext(13);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_UP_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 13:    // Holder Picker 척 실린더 Open 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_OPEN, true) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(14);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_OPEN_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 14:    // Tray 들뜸감지 센서 확인
                    {
                        // 회수 Tray에 홀더가 존재하면 들뜸감지 센서가 On이 되므로 현재 이 센서는 무용지물.....
                        if (true ||
                            cIO.GetInput((int)eIO_I.HOLDER_FLOATING_DETECTION, true) == false ||
                            ml.cOptionData.bDryRunUse == true)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ03_HOLDER_TRAY_FLOATING_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 20:    // Holder Picker X축, 빈 홀더 Tray Y축 작업위치 이동
                    {
                        // 작업할 맵데이터의 정보를 가저온다.
                        BaseMapData HolderTrayXY_Index = EmptyHolderTray.GetUnitMin(eStatus.MOUNT);

                        // Holder Picker X축의 움직일 포지션 값을 계산한다.
                        double dHolderX_SupplyPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.HOLDER_LOADING_X, (int)eAxisHolderPicker_X.TraySupply);
                        double dHolderX_Pitch = ml.cAxisPosCollData.GetAxisPosition(eMotor.HOLDER_LOADING_X, (int)eAxisHolderPicker_X.Tray_X_Pitch);
                        double dMoveX_SupplyPos = dHolderX_SupplyPos + (dHolderX_Pitch * HolderTrayXY_Index.iIndexX);

                        // Tray Y축의 움직일 포지션 값을 계산한다.
                        double dTrayY_SupplyPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.HOLDER_TRAY_Y, (int)eAxisHolderTray_Y.Loading);
                        double dTrayY_Pitch = ml.cAxisPosCollData.GetAxisPosition(eMotor.HOLDER_TRAY_Y, (int)eAxisHolderTray_Y.Y_Pitch);
                        double dMoveY_SupplyPos = dTrayY_SupplyPos + (dTrayY_Pitch * HolderTrayXY_Index.iIndexY);

                        // X, Y 이동
                        bool bMove1 = ml.Axis[eMotor.HOLDER_LOADING_X].MoveAbsolute(dMoveX_SupplyPos);
                        bool bMove2 = ml.Axis[eMotor.HOLDER_TRAY_Y].MoveAbsolute(dMoveY_SupplyPos);

                        if (bMove1 == true && bMove2 == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (bMove1 == false)
                                    ml.AddError(eErrorCode.MOTOR1_MOVE_TIMEOUT, iSubStep);
                                else if (bMove2 == false)
                                    ml.AddError(eErrorCode.MOTOR0_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // Holder Picker X축, Tray Y축 이동 확인
                    {
                        if (ml.Axis[eMotor.HOLDER_LOADING_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.HOLDER_TRAY_Y].IsMoveDone() == true)
                        {
                            SubNext(30);
                        }
                    }
                    break;

                case 30:    // Holder Picker 척 실린더 Down 진행
                    {
                        cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_DOWN, true);
                        cTimeOut.Restart();
                        SubNext(31);
                    }
                    break;

                case 31:    // Holder Picker 척 실린더 Down 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_DOWN) == true)
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
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_DOWN_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 32:    // Holder Picker 척 실린더 Close 진행
                    {
                        if (cDelay.ElapsedMilliseconds > cSysArray.uiHolderClampDownDelay)
                        {
                            cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_OPEN, false);
                            cDelay.Restart();
                            cTimeOut.Restart();
                            SubNext(33);
                        }
                    }
                    break;

                case 33:
                    {
                        if (cDelay.ElapsedMilliseconds > 500)
                        {
                            cDelay.Stop();
                            SubNext(34);
                        }
                    }
                    break;

                case 34:    // Holder Picker 척 실린더 Close 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_CLOSE_ON) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(40);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_CLOSE_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 40:    // Holder Picker 척 실린더 Up 진행
                    {
                        cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(41);
                    }
                    break;

                case 41:    // Holder Picker 척 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_UP_ON) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(42);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_UP_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 42:    // Holder Picker가 홀더를 잡고 있는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_DETECTION) == true ||
                            ml.cOptionData.bDryRunUse == true)
                        {
                            SubNext(100);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_HOLDER_NOT_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 100:   // Holder Picker, Tray 맵데이터 변경
                    {
                        HolderPicker.SetAllStatus(eStatus.MOUNT);
                        EmptyHolderTray.GetUnitMin(eStatus.MOUNT).eStatus = eStatus.EMPTY;

                        // 홀더 공급 트레이가 비었으면 알람
                        if (EmptyHolderTray.GetStatus(eStatus.MOUNT) == false)
                        {
                            // 드라이런이면 모두 마운트 시킨다.
                            if (ml.cOptionData.bDryRunUse == true)
                            {
                                EmptyHolderTray.SetAllStatus(eStatus.MOUNT);
                            }
                            else
                            {
                                //ml.AddError(eErrorCode.SEQ03_HOLDER_SUPPLY_TRAY_HOLDER_EMPTY);
                            }
                        }

                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 빈 홀더 팔렛에 공급하는 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool HolderSupply()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib HolderPicker = ml.cRunUnitData.GetIndexData(eData.HOLDER_PICKER);
            MapDataLib MPC1_FarLeft = ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_LEFT);

            switch (iSubStep)
            {
                case 0:     // 빈 홀더 공급 맵데이터 조건 확인
                    {
                        bool bHolderSupplyMapData = HolderPicker.GetStatus(eStatus.MOUNT) &&
                                                    MPC1_FarLeft.GetUnitNo(0).eStatus == eStatus.EMPTY;

                        if (bHolderSupplyMapData == true)
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

                case 10:    // Holder Picker가 홀더를 잡고 있는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_DETECTION) == true ||
                            ml.cOptionData.bDryRunUse == true)
                        {
                            SubNext(11);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_HOLDER_NOT_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 11:    // Holder Picker 척 실린더 Up 진행
                    {
                        cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(12);
                    }
                    break;

                case 12:    // Holder Picker 척 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_UP_ON) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(13);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_UP_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 13:    // MPC Left Rail Y축 공급 위치에 있는지 확인
                    {
                        double GetActLeftRail_Y = ml.Axis[eMotor.MPC_LEFT_Y].GetActPostion();
                        double LeftRail_Y_FrontPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.MPC_LEFT_Y, (int)eAxisLeftMPC_Y.HolderSupply);
                        if (GetActLeftRail_Y >= LeftRail_Y_FrontPos - 0.05 &&
                            GetActLeftRail_Y <= LeftRail_Y_FrontPos + 0.05)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ03_MPC_LEFT_RAIL_Y_NOT_SUPPLY_POSITION, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // Holder Picker X축 팔렛의 공급 위치로 이동
                    {
                        if (ml.Axis[eMotor.HOLDER_LOADING_X].MoveAbsolute((int)eAxisHolderPicker_X.PalletPlace) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR1_ERROR, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // Pipe, Needle Fix 실린더 Up 확인
                    {
                        if (ml.Axis[eMotor.HOLDER_LOADING_X].IsMoveDone() == true)
                        {
                            SubNext(30);
                        }
                    }
                    break;

                case 30:    // Holder Picker 척 실린더 Down 진행
                    {
                        cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_DOWN, true);
                        cTimeOut.Restart();
                        SubNext(31);
                    }
                    break;

                case 31:    // Holder Picker 척 실린더 Down 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_DOWN) == true)
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
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_DOWN_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 32:    // Holder Picker 척 실린더 Open 진행
                    {
                        if (cDelay.ElapsedMilliseconds > cSysArray.uiHolderUnClampDownDelay)
                        {
                            cDelay.Stop();
                            cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_OPEN, true);
                            cTimeOut.Restart();
                            SubNext(33);
                        }
                    }
                    break;

                case 33:    // Holder Picker 척 실린더 Open 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_OPEN) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(34);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_OPEN_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 34:    // Holder Picker 척 실린더 Up 진행
                    {
                        cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(35);
                    }
                    break;

                case 35:    // Holder Picker 척 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_UP_ON) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(36);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_UP_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 36:    // Holder Picker 홀더가 없는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_DETECTION, true) == false)
                        {
                            SubNext(100);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_HOLDER_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 100:   // Holder Picker 및 MPC1 Far Left 맵데이터 변경
                    {
                        HolderPicker.SetAllStatus(eStatus.EMPTY);
                        MPC1_FarLeft.GetUnitNo(0).eStatus = eStatus.MOUNT;

                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 작업 완료한 홀더 픽업 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool DoneHolderPickUp()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib HolderPicker = ml.cRunUnitData.GetIndexData(eData.HOLDER_PICKER);
            MapDataLib MPC1_FarLeft = ml.cRunUnitData.GetIndexData(eData.MPC1_FAR_LEFT);

            switch (iSubStep)
            {
                case 0:     // 작업 완료된 홀더 픽업 맵데이터 조건 확인
                    {
                        bool bWorkDoneHolderPickUpMapData = HolderPicker.GetStatus(eStatus.EMPTY) &&
                                                           (MPC1_FarLeft.GetUnitNo(1).eStatus == eStatus.UV || MPC1_FarLeft.GetUnitNo(1).eStatus == eStatus.NG);

                        if (bWorkDoneHolderPickUpMapData == true)
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

                case 10:    // Holder Picker에 홀더가 없는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_DETECTION, true) == false)
                        {
                            SubNext(11);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_HOLDER_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 11:    // Holder Picker 척 실린더 Up 및 Open 진행
                    {
                        cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_DOWN, false);
                        cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_OPEN, true);
                        cTimeOut.Restart();
                        SubNext(12);
                    }
                    break;

                case 12:    // Holder Picker 척 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_UP_ON) == true)
                        {
                            SubNext(13);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_UP_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 13:    // Holder Picker 척 실린더 Open 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_OPEN, true) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(14);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_OPEN_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 14:    // MPC Left Rail Y축 공급 위치에 있는지 확인
                    {
                        double GetActLeftRail_Y = ml.Axis[eMotor.MPC_LEFT_Y].GetActPostion();
                        double LeftRail_Y_FrontPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.MPC_LEFT_Y, (int)eAxisLeftMPC_Y.HolderSupply);
                        if (GetActLeftRail_Y >= LeftRail_Y_FrontPos - 0.05 &&
                            GetActLeftRail_Y <= LeftRail_Y_FrontPos + 0.05)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ03_MPC_LEFT_RAIL_Y_NOT_SUPPLY_POSITION, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 20:    // Holder Picker X축 팔렛의 회수 위치로 이동
                    {
                        if (ml.Axis[eMotor.HOLDER_LOADING_X].MoveAbsolute((int)eAxisHolderPicker_X.PalletPickUp) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR1_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // Holder Picker X축 이동 확인
                    {
                        if (ml.Axis[eMotor.HOLDER_LOADING_X].IsMoveDone() == true)
                        {
                            SubNext(30);
                        }
                    }
                    break;

                case 30:    // Holder Picker 척 실린더 Down 진행
                    {
                        cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_DOWN, true);
                        cTimeOut.Restart();
                        SubNext(31);
                    }
                    break;

                case 31:    // Holder Picker 척 실린더 Down 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_DOWN) == true)
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
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_DOWN_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 32:    // Holder Picker 척 실린더 Close 진행
                    {
                        if (cDelay.ElapsedMilliseconds > cSysArray.uiHolderClampDownDelay)
                        {
                            cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_OPEN, false);
                            cTimeOut.Restart();
                            cDelay.Restart();
                            SubNext(33);
                        }
                    }
                    break;

                case 33:
                    {
                        if (cDelay.ElapsedMilliseconds > 500)
                        {
                            cDelay.Stop();
                            SubNext(34);
                        }
                    }
                    break;

                case 34:    // Holder Picker 척 실린더 Close 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_CLOSE_ON) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(40);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_CLOSE_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 40:    // Holder Picker 척 실린더 Up 진행
                    {
                        cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(41);
                    }
                    break;

                case 41:    // Holder Picker 척 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_UP_ON) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(42);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_UP_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 42:    // Holder Picker가 홀더를 잡고 있는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_DETECTION) == true ||
                            ml.cOptionData.bDryRunUse == true)
                        {
                            SubNext(100);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_HOLDER_NOT_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 100:   // Holder Picker 및 MPC1 Far Left 맵데이터 변경
                    {
                        HolderPicker.GetUnitNo(0).eStatus = MPC1_FarLeft.GetUnitNo(1).eStatus;
                        MPC1_FarLeft.GetUnitNo(1).eStatus = eStatus.EMPTY;

                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 작업 완료한 홀더 Tray에 회수하는 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool HolderRecovery()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib HolderPicker = ml.cRunUnitData.GetIndexData(eData.HOLDER_PICKER);
            MapDataLib WorkDoneHolderTray = ml.cRunUnitData.GetIndexData(eData.WORK_DONE_HOLDER_TRAY);

            switch (iSubStep)
            {
                case 0:     // 작업 완료된 홀더, Tray에 회수 작업 맵데이터 조건 확인
                    {
                        bool bWorkDoneHolderPlaceMapData = HolderPicker.GetStatus(eStatus.UV) &&
                                                           WorkDoneHolderTray.GetStatus(eStatus.EMPTY);

                        if (bWorkDoneHolderPlaceMapData == true)
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

                case 10:    // Holder Picker에 홀더를 잡고 있는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_DETECTION) == true ||
                            ml.cOptionData.bDryRunUse == true)
                        {
                            SubNext(11);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_HOLDER_NOT_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 11:    // Holder Picker 척 실린더 Up 및 Open 진행
                    {
                        cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_DOWN, false);
                        SubNext(12);
                    }
                    break;

                case 12:    // Holder Picker 척 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_UP_ON) == true)
                        {
                            SubNext(13);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_UP_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 13:    // Tray 들뜸감지 센서 확인
                    {
                        // 회수 Tray에 홀더가 존재하면 들뜸감지 센서가 On이 되므로 현재 이 센서는 무용지물.....
                        if (true ||
                            cIO.GetInput((int)eIO_I.HOLDER_FLOATING_DETECTION, true) == false ||
                            ml.cOptionData.bDryRunUse == true)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ03_HOLDER_TRAY_FLOATING_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 20:    // Holder Picker X축, 작업 완료 Tray Y축 작업위치 이동
                    {
                        // 작업할 맵데이터의 정보를 가저온다.
                        BaseMapData HolderTrayXY_Index = WorkDoneHolderTray.GetUnitMin(eStatus.EMPTY);

                        // Holder Picker X축의 움직일 포지션 값을 계산한다.
                        double dHolderX_RecoveryPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.HOLDER_LOADING_X, (int)eAxisHolderPicker_X.TrayRecovery);
                        double dHolderX_Pitch = ml.cAxisPosCollData.GetAxisPosition(eMotor.HOLDER_LOADING_X, (int)eAxisHolderPicker_X.Tray_X_Pitch);
                        double dMoveX_RecoveryPos = dHolderX_RecoveryPos + (dHolderX_Pitch * HolderTrayXY_Index.iIndexX);

                        // Tray Y축의 움직일 포지션 값을 계산한다.
                        double dTrayY_RecoveryPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.HOLDER_TRAY_Y, (int)eAxisHolderTray_Y.Loading);
                        double dTrayY_Pitch = ml.cAxisPosCollData.GetAxisPosition(eMotor.HOLDER_TRAY_Y, (int)eAxisHolderTray_Y.Y_Pitch);
                        double dMoveY_RecoveryPos = dTrayY_RecoveryPos + (dTrayY_Pitch * HolderTrayXY_Index.iIndexY);

                        // X, Y 이동
                        bool bMove1 = ml.Axis[eMotor.HOLDER_LOADING_X].MoveAbsolute(dMoveX_RecoveryPos);
                        bool bMove2 = ml.Axis[eMotor.HOLDER_TRAY_Y].MoveAbsolute(dMoveY_RecoveryPos);

                        if (bMove1 == true && bMove2 == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (bMove1 == false)
                                    ml.AddError(eErrorCode.MOTOR1_ERROR, iSubStep);
                                else if (bMove2 == false)
                                    ml.AddError(eErrorCode.MOTOR0_ERROR, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // Holder Picker X축, Tray Y축 이동 확인
                    {
                        if (ml.Axis[eMotor.HOLDER_LOADING_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.HOLDER_TRAY_Y].IsMoveDone() == true)
                        {
                            cTimeOut.Stop();
                            SubNext(30);
                        }
                    }
                    break;

                case 30:    // Holder Picker 척 실린더 Down 진행
                    {
                        cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_DOWN, true);
                        cTimeOut.Restart();
                        SubNext(31);
                    }
                    break;

                case 31:    // Holder Picker 척 실린더 Down 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_DOWN) == true)
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
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_DOWN_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 32:    // Holder Picker 척 실린더 Open 진행
                    {
                        if (cDelay.ElapsedMilliseconds > cSysArray.uiHolderUnClampDownDelay)
                        {
                            cDelay.Stop();
                            cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_OPEN, true);
                            cTimeOut.Restart();
                            SubNext(33);
                        }
                    }
                    break;

                case 33:    // Holder Picker 척 실린더 OPEN 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_OPEN) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(34);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_OPEN_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 34:    // Holder Picker 척 실린더 Up 진행
                    {
                        cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(35);
                    }
                    break;

                case 35:    // Holder Picker 척 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_UP_ON) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(100);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_UP_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 36:    // Holder Picker 홀더가 없는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_DETECTION) == false)
                        {
                            SubNext(100);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_HOLDER_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 100:   // 작업 완료 홀더 회수 맵데이터 변경
                    {
                        HolderPicker.SetAllStatus(eStatus.EMPTY);
                        WorkDoneHolderTray.GetUnitMin(eStatus.EMPTY).eStatus = eStatus.WORK_DONE;

                        ml.cSysOne.iProductCount++;

                        if (ml.McState == eMachineState.RUN)
                        {
                            // 작업수량 Counting
                            if (ml.cOptionData.bDryRunUse == false)
                            {
                                cSysArray.iTotalCount++;
                                ml.SetProductionDB(1, 0); // 작업 수량 DB 저장
                            }

                            // UPH 측정
                            if (ml.cVar.swUPH.ElapsedMilliseconds != 0)
                                ml.cSysOne.iUPHCount = (int)(double)(3600 / (ml.cVar.swUPH.ElapsedMilliseconds / 1000));
                            ml.cVar.swUPH.Restart();
                        }

                        // 모든 홀더의 작업을 끝마치고 설비 정지할 조건인지 확인
                        if (CheckStopMachineState() == true)
                        {
                            ml.McStop();
                        }
                        else
                        {
                            if (WorkDoneHolderTray.GetStatus(eStatus.EMPTY) == false)
                            {
                                // 드라이런이면 모두 Empty 시킨다.
                                if (ml.cOptionData.bDryRunUse == true)
                                {
                                    WorkDoneHolderTray.SetAllStatus(eStatus.EMPTY);
                                }
                                else
                                {
                                    // 홀더 회수 Tray가 가득 찼으면 알람
                                    ml.AddError(eErrorCode.SEQ03_HOLDER_RECOVERY_TRAY_HOLDER_FULL);
                                }
                            }
                        }

                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 불량 홀더 Tray에 회수하는 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool NG_HolderRecovery()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib HolderPicker = ml.cRunUnitData.GetIndexData(eData.HOLDER_PICKER);
            MapDataLib NG_HolderTray = ml.cRunUnitData.GetIndexData(eData.NG_HOLDER_TRAY);

            switch (iSubStep)
            {
                case 0:     // 작업 완료된 홀더, Tray에 회수 작업 맵데이터 조건 확인
                    {
                        bool bNG_HolderPlaceMapData = HolderPicker.GetStatus(eStatus.NG) &&
                                                      NG_HolderTray.GetStatus(eStatus.EMPTY);

                        if (bNG_HolderPlaceMapData == true)
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

                case 10:    // Holder Picker에 홀더를 잡고 있는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_DETECTION) == true ||
                            ml.cOptionData.bDryRunUse == true)
                        {
                            SubNext(11);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_HOLDER_NOT_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 11:    // Holder Picker 척 실린더 Up 및 Open 진행
                    {
                        cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_DOWN, false);
                        SubNext(12);
                    }
                    break;

                case 12:    // Holder Picker 척 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_UP_ON) == true)
                        {
                            SubNext(13);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_UP_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 13:    // Tray 들뜸감지 센서 확인
                    {
                        // 회수 Tray에 홀더가 존재하면 들뜸감지 센서가 On이 되므로 현재 이 센서는 무용지물.....
                        if (true ||
                            cIO.GetInput((int)eIO_I.HOLDER_FLOATING_DETECTION, true) == false ||
                            ml.cOptionData.bDryRunUse == true)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ03_HOLDER_TRAY_FLOATING_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 20:    // Holder Picker X축, 작업 완료 Tray Y축 작업위치 이동
                    {
                        // 작업할 맵데이터의 정보를 가저온다.
                        BaseMapData HolderTrayXY_Index = NG_HolderTray.GetUnitMin(eStatus.EMPTY);

                        // Tray Y축의 움직일 포지션 값을 계산한다.
                        double dTrayY_RecoveryPos = ml.cAxisPosCollData.GetAxisPosition(eMotor.HOLDER_TRAY_Y, (int)eAxisHolderTray_Y.Loading);
                        double dTrayY_Pitch = ml.cAxisPosCollData.GetAxisPosition(eMotor.HOLDER_TRAY_Y, (int)eAxisHolderTray_Y.Y_Pitch);
                        double dMoveY_RecoveryPos = dTrayY_RecoveryPos + (dTrayY_Pitch * HolderTrayXY_Index.iIndexY);

                        // X, Y 이동
                        bool bMove1 = ml.Axis[eMotor.HOLDER_LOADING_X].MoveAbsolute((int)eAxisHolderPicker_X.NG_Place);
                        bool bMove2 = ml.Axis[eMotor.HOLDER_TRAY_Y].MoveAbsolute(dMoveY_RecoveryPos);

                        if (bMove1 == true && bMove2 == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (bMove1 == false)
                                    ml.AddError(eErrorCode.MOTOR1_ERROR, iSubStep);
                                else if (bMove2 == false)
                                    ml.AddError(eErrorCode.MOTOR0_ERROR, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // Holder Picker X축, Tray Y축 이동 확인
                    {
                        if (ml.Axis[eMotor.HOLDER_LOADING_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.HOLDER_TRAY_Y].IsMoveDone() == true)
                        {
                            cTimeOut.Stop();
                            SubNext(30);
                        }
                    }
                    break;

                case 30:    // Holder Picker 척 실린더 Down 진행
                    {
                        cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_DOWN, true);
                        cTimeOut.Restart();
                        SubNext(31);
                    }
                    break;

                case 31:    // Holder Picker 척 실린더 Down 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_DOWN) == true)
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
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_DOWN_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 32:    // Holder Picker 척 실린더 Open 진행
                    {
                        if (cDelay.ElapsedMilliseconds > cSysArray.uiHolderUnClampDownDelay)
                        {
                            cDelay.Stop();
                            cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_OPEN, true);
                            cTimeOut.Restart();
                            SubNext(33);
                        }
                    }
                    break;

                case 33:    // Holder Picker 척 실린더 OPEN 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_OPEN) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(34);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_OPEN_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 34:    // Holder Picker 척 실린더 Up 진행
                    {
                        cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_DOWN, false);
                        cTimeOut.Restart();
                        SubNext(35);
                    }
                    break;

                case 35:    // Holder Picker 척 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_UP_ON) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(100);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_UP_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 36:    // Holder Picker 홀더가 없는지 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_DETECTION) == false)
                        {
                            SubNext(100);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_HOLDER_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 100:   // 불량 홀더 회수 맵데이터 변경
                    {
                        HolderPicker.SetAllStatus(eStatus.EMPTY);
                        NG_HolderTray.GetUnitMin(eStatus.EMPTY).eStatus = eStatus.NG;

                        // 작업수량 Counting
                        if (ml.cOptionData.bDryRunUse == false &&
                            ml.McState == eMachineState.RUN)
                        {
                            cSysArray.iNgCount++;
                            ml.SetProductionDB(0, 1); // 작업 수량 DB 저장
                        }

                        // 모든 홀더의 작업을 끝마치고 설비 정지할 조건인지 확인
                        if (CheckStopMachineState() == true)
                        {
                            ml.McStop();
                        }
                        else
                        {
                            // 홀더 회수 Tray가 가득 찼으면 알람
                            if (NG_HolderTray.GetStatus(eStatus.EMPTY) == false)
                            {
                                ml.AddError(eErrorCode.SEQ03_HOLDER_NG_TRAY_HOLDER_FULL);
                            }
                        }

                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 트레이 교체 위치로 이동
        /// </summary>
        /// <returns></returns>
        public bool TrayChangePosMove()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            switch (iSubStep)
            {
                case 0:     // 작업 완료된 홀더, Tray에 회수 작업 맵데이터 조건 확인
                    {
                        SubNext(10);
                    }
                    break;

                case 10:    // Holder Picker 척 실린더 Up 진행
                    {
                        cIO.SetOutput((int)eIO_O.HOLDER_CHUCK_DOWN, false);
                        SubNext(11);
                    }
                    break;

                case 11:    // Holder Picker 척 실린더 Up 확인
                    {
                        if (cIO.GetInput((int)eIO_I.HOLDER_CHUCK_UP_ON) == true)
                        {
                            SubNext(12);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ03_HOLDER_PICKER_CHUCK_UP_CYLINDER_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 12:    // Tray 들뜸감지 센서 확인
                    {
                        // 회수 Tray에 홀더가 존재하면 들뜸감지 센서가 On이 되므로 현재 이 센서는 무용지물.....
                        if (true ||
                            cIO.GetInput((int)eIO_I.HOLDER_FLOATING_DETECTION, true) == false ||
                            ml.cOptionData.bDryRunUse == true)
                        {
                            cTimeOut.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ03_HOLDER_TRAY_FLOATING_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 20:    // Tray Y축 교체 위치로 이동
                    {
                        if (ml.Axis[eMotor.HOLDER_TRAY_Y].MoveAbsolute(0.0) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(21);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR0_ERROR, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 21:    // Tray Y축 이동 확인
                    {
                        if (ml.Axis[eMotor.HOLDER_TRAY_Y].IsMoveDone() == true)
                        {
                            cTimeOut.Stop();
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
        /// 공급할 홀더가 없고 MPC에 모든 홀더가 비었을 때
        /// 설비를 정지시켜야 할지 확인하는 함수
        /// </summary>
        private bool CheckStopMachineState()
        {
            MapDataLib SupplyTrayMap = ml.cRunUnitData.GetIndexData(eData.SUPPLY_HOLDER_TRAY);

            if (SupplyTrayMap.GetStatus(eStatus.MOUNT) == true) return false;

            for (int i = (int)eData.MPC1_FAR_LEFT; i <= (int)eData.MPC2_FAR_RIGHT; i++)
            {
                MapDataLib MPC_Map = ml.cRunUnitData.GetIndexData((eData)i);

                if (i == (int)eData.MPC2_FAR_LEFT)
                {
                    if (MPC_Map.GetStatus(eStatus.UV) == true || MPC_Map.GetStatus(eStatus.NG) == true) return false;
                }
                else if (i == (int)eData.MPC2_FAR_RIGHT)
                {
                    if (MPC_Map.GetStatus(eStatus.DISPENSING) == true || MPC_Map.GetStatus(eStatus.NG) == true) return false;
                }
                else if (i == (int)eData.MPC2_UV)
                {
                    if (MPC_Map.GetStatus(eStatus.DISPENSING) == true || MPC_Map.GetStatus(eStatus.UV) == true || MPC_Map.GetStatus(eStatus.NG) == true) return false;
                }
                else if (i == (int)eData.MPC1_FAR_RIGHT)
                {
                    if (MPC_Map.GetStatus(eStatus.DISPENSING) == true || MPC_Map.GetStatus(eStatus.NG) == true) return false;
                }
                else
                {
                    if (MPC_Map.GetStatus(eStatus.EMPTY) == false) return false;
                }
            }

            return true;
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
                        if (ml.cVar.Manual_EmptyHolderPickUp == true)
                        {
                            Next(10);
                            ml.cVar.Manual_EmptyHolderPickUp = false;
                        }
                        else if (ml.cVar.Manual_EmptyHolderSupply == true)
                        {
                            Next(20);
                            ml.cVar.Manual_EmptyHolderSupply = false;
                        }
                        else if (ml.cVar.Manual_DoneHolderPickUp == true)
                        {
                            Next(30);
                            ml.cVar.Manual_DoneHolderPickUp = false;
                        }
                        else if (ml.cVar.Manual_DoneHolderRecovery == true)
                        {
                            Next(40);
                            ml.cVar.Manual_DoneHolderRecovery = false;
                        }
                        else if (ml.cVar.Manual_NgHolderRecovery == true)
                        {
                            Next(50);
                            ml.cVar.Manual_NgHolderRecovery = false;
                        }
                        else if (ml.cVar.Manual_HolderTrayChangeMove == true)
                        {
                            Next(60);
                            ml.cVar.Manual_HolderTrayChangeMove = false;
                        }
                    }
                    break;

                case 10:    // 빈 홀더 픽업 함수
                    {
                        if (EmptyHolderPickUp() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 20:    // 빈 홀더 공급 함수
                    {
                        if (HolderSupply() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 30:    // 완료된 홀더 픽업 함수
                    {
                        if (DoneHolderPickUp() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 40:    // 완료된 홀더 회수 함수
                    {
                        if (HolderRecovery() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 50:    // NG 홀더 회수 함수
                    {
                        if (NG_HolderRecovery() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 60:    // 홀더 Tray 교환 위치로 이동
                    {
                        if (TrayChangePosMove() == true)
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