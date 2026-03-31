using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// 시퀀스 클래스
    /// </summary>
    public class Seq07_Transfer : ISequence, ISeqNo
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
        public Seq07_Transfer(eSequence sequence, eLine Line)
        {
            instance = this;
            Proc = new CSeqProc();
            Proc.SeqName = SeqName = sequence.ToString();
            cSwMainTimeOut = new Stopwatch();
            this.Line = Line;

            if (this.Line == eLine.Left)
            {
                // 파이프 R축 정의
                Transfer_R = eMotor.PIPE_ROTATE;

                // Input 정의
                I_VACUUM_SENSOR = (int)eIO_I.TRANSFER_PIPE_VACUUM;
                I_TRANSFER_FWD = (int)eIO_I.TRANSFER_PIPE_FORWARD;
                I_TRANSFER_BWD = (int)eIO_I.TRANSFER_PIPE_BACKWARD_ON;
                I_TRANSFER_TRIGGER = (int)eIO_I.TRANSFER_PIPE_TRIGGER;

                // Output 정의
                O_VACUUM = (int)eIO_O.TRANSFER_PIPE_VACUUM;
                O_BLOW = (int)eIO_O.TRANSFER_PIPE_BLOW;
                O_TRANSFER_FWD_SOL = (int)eIO_O.TRANSFER_PIPE_FORWARD;
                O_TRANSFER_BWD_SOL = (int)eIO_O.TRANSFER_PIPE_BACKWARD;
            }
            else
            {
                // 니들 트렌스퍼 R축 정의
                Transfer_R = eMotor.NEEDLE_ROTATE;

                // Input 정의
                I_VACUUM_SENSOR = (int)eIO_I.TRANSFER_NEEDLE_VACUUM;
                I_TRANSFER_FWD = (int)eIO_I.TRANSFER_NEEDLE_FORWARD;
                I_TRANSFER_BWD = (int)eIO_I.TRANSFER_NEEDLE_BACKWARD_ON;
                I_TRANSFER_TRIGGER = (int)eIO_I.TRANSFER_NEEDLE_TRIGGER;

                // Output 정의
                O_VACUUM = (int)eIO_O.TRANSFER_NEEDLE_VACUUM;
                O_BLOW = (int)eIO_O.TRANSFER_NEEDLE_BLOW;
                O_TRANSFER_FWD_SOL = (int)eIO_O.TRANSFER_NEEDLE_FORWARD;
                O_TRANSFER_BWD_SOL = (int)eIO_O.TRANSFER_NEEDLE_BACKWARD;
            }
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

            if (this.Line == eLine.Left)
            {
                //CAM1CAM2_ResultData = CMainLib.Ins.cVar.GetVisionResultData(1, 0);

                CAM1_ResultData = CMainLib.Ins.cVar.GetVisionResultData(1, 0);
            }
            else
            {
                //CAM1CAM2_ResultData = CMainLib.Ins.cVar.GetVisionResultData(2, 0);

                CAM2_ResultData = CMainLib.Ins.cVar.GetVisionResultData(2, 0);
            }

            // 로그 설정
            eLogType = eLogType.Seq07_Transfer;
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
        /// CAM2 Vision Result Data
        /// </summary>
        private CVisionResultData CAM1CAM2_ResultData;

        /// <summary>
        /// CAM1 Vision Result Data
        /// </summary>
        private CVisionResultData CAM1_ResultData;

        /// <summary>
        /// CAM2 Vision Result Data
        /// </summary>
        private CVisionResultData CAM2_ResultData;

        /// <summary>
        /// 라인 정의
        /// </summary>
        private eLine Line;

        /* 공정별 맵 데이터 */
        private MapDataLib TransferMap = null;

        /* PnP 모터 정의 */
        private eMotor Transfer_R;

        /* Input 정의 */
        private int I_VACUUM_SENSOR;
        private int I_TRANSFER_FWD;
        private int I_TRANSFER_BWD;
        private int I_TRANSFER_TRIGGER;

        /* Output 정의 */
        private int O_VACUUM;
        private int O_BLOW;
        private int O_TRANSFER_FWD_SOL;
        private int O_TRANSFER_BWD_SOL;

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
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} Line{Line} Do Step : {iStep} Process"), false);
            }

            // 동작 타임 아웃 측정
            if (cSwMainTimeOut.ElapsedMilliseconds > Define.SEQ_TIME_OUT &&
                iStep != 0)
            {
                // 타임아웃 에러
                ml.AddError(eErrorCode.SEQ07_TIME_OUT);
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
                        if (Line == eLine.Left)
                        {
                            TransferMap = ml.cRunUnitData.GetIndexData(eData.PIPE_TRANSFER);    // 파이프 맵데이터 정의
                        }
                        else
                        {
                            TransferMap = ml.cRunUnitData.GetIndexData(eData.NEEDLE_TRANSFER);  // 니들 맵데이터 정의
                        }

                        if (TransferMap != null)
                        {
                            // 파이프 or 니들 받는 맵데이터 조건
                            bool bRecvDelivery = TransferMap.GetStatus(eStatus.EMPTY);

                            // 파이프 or 니들 Vision촬영 후 전달 맵데이터 조건
                            bool bSendDelivery = TransferMap.GetStatus(eStatus.MOUNT);

                            if (bRecvDelivery == true)
                            {
                                Next(10);
                            }
                            else if (bSendDelivery == true)
                            {
                                Next(20);
                            }
                        }
                    }
                    break;

                case 10:    // 파이프 or 니들 전달받는 함수
                    {
                        if (TransferRecvDelivery() == true)
                        {
                            Next(0);
                        }
                    }
                    break;

                case 20:    // 파이프 or 니들 Vision촬영 후 전달하는 함수
                    {
                        if (TransferSendDelivery() == true)
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
        /// 파이프 or 니들 트렌스퍼로 전달 받는 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool TransferRecvDelivery()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} Line{Line} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            if (Line == eLine.Left)
            {
                TransferMap = ml.cRunUnitData.GetIndexData(eData.PIPE_TRANSFER);    // 파이프 맵데이터 정의
            }
            else
            {
                TransferMap = ml.cRunUnitData.GetIndexData(eData.NEEDLE_TRANSFER);  // 니들 맵데이터 정의
            }

            switch (iSubStep)
            {
                case 0:     // 파이프 or 니들 받는 맵데이터 조건
                    {
                        bool bRecvDelivery = TransferMap.GetStatus(eStatus.EMPTY);

                        if (bRecvDelivery == true)
                        {
                            cTimeOut.Restart();
                            // 벅큠 ON, 블로우 Off
                            cIO.SetOutput(O_VACUUM, true);
                            cIO.SetOutput(O_BLOW, false);
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

                case 10:    // 트랜스퍼 실린더 및 R축 받는 위치로 이동
                    {
                        if (ml.Axis[Transfer_R].MoveAbsolute((int)eAxisPipeRotate.Origin) == true)
                        {
                            cTimeOut.Restart();
                            cIO.SetOutput(O_TRANSFER_FWD_SOL, false);
                            cIO.SetOutput(O_TRANSFER_BWD_SOL, true);
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (Line == eLine.Left)
                                    ml.AddError(eErrorCode.MOTOR21_MOVE_TIMEOUT, iSubStep);
                                else
                                    ml.AddError(eErrorCode.MOTOR22_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:
                    {
                        if (ml.Axis[Transfer_R].IsMoveDone() == true)
                        {
                            SubNext(12);
                        }
                    }
                    break;

                case 12:    // 트렌스퍼 실린더 BWD 확인
                    {
                        if (cIO.GetInput(I_TRANSFER_BWD) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(100);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                if (Line == eLine.Left)
                                    ml.AddError(eErrorCode.SEQ07_TRANSFER_LEFT_CYLINDER_BWD_TIMEOUT, iSubStep);
                                else
                                    ml.AddError(eErrorCode.SEQ07_TRANSFER_RIGHT_CYLINDER_BWD_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 100:   // 맵데이터 변경
                    {
                        TransferMap.SetAllStatus(eStatus.STANBY);

                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 니들 비전 재촬영 카운트
        /// </summary>
        private uint uiShootCount = 0;

        /// <summary>
        /// 니들 촬영 실패 리트라이 카운트
        /// </summary>
        private uint uiNeedleRetryCount = 0;

        /// <summary>
        /// 파이프, 니들 자세 확인 프랠그
        /// </summary>
        private bool bGood = false;

        /// <summary>
        /// 파이프 or 니들 Vision 촬영 후 전달하는 시퀀스
        /// </summary>
        /// <returns></returns>
        public bool TransferSendDelivery()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} Line{Line} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            if (Line == eLine.Left)
            {
                TransferMap = ml.cRunUnitData.GetIndexData(eData.PIPE_TRANSFER);    // 파이프 맵데이터 정의
            }
            else
            {
                TransferMap = ml.cRunUnitData.GetIndexData(eData.NEEDLE_TRANSFER);  // 니들 맵데이터 정의
            }

            switch (iSubStep)
            {
                case 0:     //
                    {
                        bool bFeederRunMapData = TransferMap.GetStatus(eStatus.MOUNT);

                        if (bFeederRunMapData == true)
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

                case 10:    // 파이프, 니들 진공 감지되는지 확인
                    {
                        if (cIO.GetInput(I_VACUUM_SENSOR, true) == true ||
                            Define.SIMULATION == true ||
                            ml.cOptionData.bDryRunUse == true)
                        {
                            SubNext(11);
                        }
                        else
                        {
                            if (Line == eLine.Left)
                                ml.AddError(eErrorCode.SEQ07_TRANSFER_LEFT_PIPE_NOT_DETECTION, iSubStep);
                            else
                                ml.AddError(eErrorCode.SEQ07_TRANSFER_RIGHT_NEEDLE_NOT_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 11:    // 트랜스퍼 실린더 및 R축 받는 위치로 이동
                    {
                        PositionData PosData = null;

                        if (Line == eLine.Left)
                            PosData = ml.cAxisPosCollData.GetPositionData(Transfer_R, (int)eAxisPipeRotate.Rotate90);
                        else
                            PosData = ml.cAxisPosCollData.GetPositionData(Transfer_R, (int)eAxisNeedleRotate.Rotate90);

                        cIO.SetOutput(O_TRANSFER_FWD_SOL, true);
                        cIO.SetOutput(O_TRANSFER_BWD_SOL, false);
                        if (ml.Axis[Transfer_R].MoveAbsolute(PosData.iNo) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(12);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (Line == eLine.Left)
                                    ml.AddError(eErrorCode.MOTOR21_MOVE_TIMEOUT, iSubStep);
                                else
                                    ml.AddError(eErrorCode.MOTOR22_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 12:    // 트렌스퍼 실린더 FWD 확인
                    {
                        if (cIO.GetInput(I_TRANSFER_FWD) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(13);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                if (Line == eLine.Left)
                                    ml.AddError(eErrorCode.SEQ07_TRANSFER_LEFT_CYLINDER_FWD_TIMEOUT, iSubStep);
                                else
                                    ml.AddError(eErrorCode.SEQ07_TRANSFER_RIGHT_CYLINDER_FWD_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 13:
                    {
                        if (ml.Axis[Transfer_R].IsMoveDone() == true)
                        {
                            SubNext(14);
                        }
                    }
                    break;

                case 14:    // 트렌스퍼 전진 Trigger 센서 확인
                    {
                        if (cIO.GetInput(I_TRANSFER_TRIGGER) == true)
                        {
                            if (Line == eLine.Right)
                            {
                                uiShootCount = 0;
                                uiNeedleRetryCount = 0;
                            }
                            cDelay.Restart();
                            SubNext(20);
                        }
                        else
                        {
                            ml.AddError(eErrorCode.SEQ07_TRANSFER_LEFT_TRIGGER_FWD_SENSOR_NOT_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 20:    // 파이프, 니들 자세 Vision 촬영
                    {
                        if (cDelay.ElapsedMilliseconds > 100)
                        {
                            cDelay.Stop();
                            if (ml.cOptionData.bDryRunUse == true)
                            {
                                bGood = true;
                                SubNext(100);
                            }
                            else
                            {
                                if (Line == eLine.Left)
                                {
                                    CAM1_ResultData.DataClear();
                                    CAM2_ResultData.bGoodNg = false;
                                }
                                else
                                {
                                    CAM2_ResultData.DataClear();
                                    CAM2_ResultData.bGoodNg = false;
                                }

                                ml.cCVisionToolBlockLib[(int)Line + 1].VisionShoot(0);
                                cTimeOut.Restart();
                                if (ml.cOptionData.bNeedlePosCheckTwice == true)
                                {
                                    SubNext(21);
                                }
                                else
                                {
                                    SubNext(22);
                                }
                            }
                        }
                    }
                    break;

                case 21:    // 파이프, 니들 자세 Good, Ng 판정 확인 (자세검사 2번)
                    {
                        // 파이프 자세 판정
                        if (Line == eLine.Left)
                        {
                            if (CAM1_ResultData.bShootFinish == true)
                            {
                                if (CAM1_ResultData.bGoodNg == true)
                                {
                                    bGood = true;
                                    SubNext(100);
                                }
                                else
                                {
                                    bGood = false;
                                    cTimeOut.Restart();
                                    SubNext(30); // 파이프 Ng면 파이프 버림
                                }
                            }
                            else
                            {
                                if (cTimeOut.ElapsedMilliseconds > 3000)
                                {
                                    cTimeOut.Stop();
                                    ml.AddError(eErrorCode.SEQ07_TRANSFER_LEFT_VISION_TIMEOUT, iSubStep);
                                    return true;
                                }
                            }
                        }
                        // 니들 자세 판정
                        else
                        {
                            if (CAM2_ResultData.bShootFinish == true)
                            {
                                if (uiShootCount >= 1 && CAM2_ResultData.bGoodNg == true)
                                {
                                    bGood = true;
                                    SubNext(100);
                                }
                                else
                                {
                                    // 니들이 여러개 잡혀있으면 NG, 니들 버림
                                    if (CAM2_ResultData.bNeedleDoubleCatch == true)
                                    {
                                        bGood = false;
                                        cTimeOut.Restart();
                                        SubNext(30);
                                    }
                                    // 180도 회전해서 반대쪽도 니들 한개만 잡혀있는지 확인
                                    else if (uiShootCount == 0)
                                    {
                                        cTimeOut.Restart();
                                        uiShootCount++;
                                        uiNeedleRetryCount = 0;
                                        SubNext(50);
                                    }
                                    // 돌렸을 때 뾰족한 부분이 아니라면 다시 원래 위치로 돌려서 재촬영
                                    else if (uiShootCount == 1)
                                    {
                                        cTimeOut.Restart();
                                        uiShootCount++; // 니들 Ng면 180도 회전 후 다시 재촬영
                                        uiNeedleRetryCount = 0;
                                        SubNext(60);
                                    }
                                    else
                                    {
                                        bGood = false;
                                        cTimeOut.Restart();
                                        SubNext(30); // 다시 재촬영해도 Ng이면 니들 버림
                                    }
                                }
                            }
                            else
                            {
                                // 비전 오류 시 다시 한번 리트라이
                                if (uiNeedleRetryCount == 0)
                                {
                                    cTimeOut.Restart();
                                    uiNeedleRetryCount++;
                                    SubNext(20);
                                }
                                // 리트라이 해도 안되면 에러 띄움
                                else if (cTimeOut.ElapsedMilliseconds > 3000)
                                {
                                    cTimeOut.Stop();
                                    ml.AddError(eErrorCode.SEQ07_TRANSFER_RIGHT_VISION_TIMEOUT, iSubStep);
                                    return true;
                                }
                            }
                        }
                    }
                    break;

                case 22:    // 파이프, 니들 자세 Good, Ng 판정 확인 (자세검사 1번)
                    {
                        // 파이프 자세 판정
                        if (Line == eLine.Left)
                        {
                            if (CAM1_ResultData.bShootFinish == true)
                            {
                                if (CAM1_ResultData.bGoodNg == true)
                                {
                                    bGood = true;
                                    SubNext(100);
                                }
                                else
                                {
                                    bGood = false;
                                    cTimeOut.Restart();
                                    SubNext(30); // 파이프 Ng면 파이프 버림
                                }
                            }
                            else
                            {
                                if (cTimeOut.ElapsedMilliseconds > 3000)
                                {
                                    cTimeOut.Stop();
                                    ml.AddError(eErrorCode.SEQ07_TRANSFER_LEFT_VISION_TIMEOUT, iSubStep);
                                    return true;
                                }
                            }
                        }
                        // 니들 자세 판정
                        else
                        {
                            if (CAM2_ResultData.bShootFinish == true)
                            {
                                if (CAM2_ResultData.bGoodNg == true)
                                {
                                    bGood = true;
                                    SubNext(100);
                                }
                                else
                                {
                                    // 니들이 여러개 잡혀있으면 NG, 니들 버림
                                    if (CAM2_ResultData.bNeedleDoubleCatch == true)
                                    {
                                        bGood = false;
                                        cTimeOut.Restart();
                                        SubNext(30);
                                    }
                                    // 뾰족한 부분이 아니라면 180도 돌려서 재촬영
                                    else if (uiShootCount == 0)
                                    {
                                        cTimeOut.Restart();
                                        uiShootCount++; // 니들 Ng면 180도 회전 후 다시 재촬영
                                        uiNeedleRetryCount = 0;
                                        SubNext(50);
                                    }
                                    else
                                    {
                                        bGood = false;
                                        cTimeOut.Restart();
                                        SubNext(30); // 그래도 이상하면 니들 버림
                                    }
                                }
                            }
                            else
                            {
                                // 비전 오류 시 다시 한번 리트라이
                                if (uiNeedleRetryCount == 0)
                                {
                                    cTimeOut.Restart();
                                    uiNeedleRetryCount++;
                                    SubNext(20);
                                }
                                // 리트라이 해도 안되면 에러 띄움
                                else if (cTimeOut.ElapsedMilliseconds > 3000)
                                {
                                    cTimeOut.Stop();
                                    ml.AddError(eErrorCode.SEQ07_TRANSFER_RIGHT_VISION_TIMEOUT, iSubStep);
                                    return true;
                                }
                            }
                        }
                    }
                    break;

                case 30:
                    {
                        PositionData PosData = null;
                        if (Line == eLine.Left)
                            PosData = ml.cAxisPosCollData.GetPositionData(Transfer_R, (int)eAxisPipeRotate.Trash);
                        else
                            PosData = ml.cAxisPosCollData.GetPositionData(Transfer_R, (int)eAxisNeedleRotate.Trash);

                        if (ml.Axis[Transfer_R].MoveAbsolute(PosData.iNo) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(31);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (Line == eLine.Left)
                                    ml.AddError(eErrorCode.MOTOR21_MOVE_TIMEOUT, iSubStep);
                                else
                                    ml.AddError(eErrorCode.MOTOR22_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 31:
                    {
                        if (ml.Axis[Transfer_R].IsMoveDone() == true)
                        {
                            // 벅큠 Off, 블로우 On
                            cIO.SetOutput(O_VACUUM, false);
                            cIO.SetOutput(O_BLOW, true);
                            // 트랜스퍼 BWD
                            cIO.SetOutput(O_TRANSFER_FWD_SOL, false);
                            cIO.SetOutput(O_TRANSFER_BWD_SOL, true);
                            cTimeOut.Restart();
                            SubNext(32);
                        }
                    }
                    break;

                case 32:    // 트렌스퍼 실린더 BWD 확인
                    {
                        if (cIO.GetInput(I_TRANSFER_BWD) == true)
                        {
                            // 블로우 Off
                            cIO.SetOutput(O_BLOW, false);
                            cTimeOut.Restart();
                            SubNext(33);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                if (Line == eLine.Left)
                                    ml.AddError(eErrorCode.SEQ07_TRANSFER_LEFT_CYLINDER_BWD_TIMEOUT, iSubStep);
                                else
                                    ml.AddError(eErrorCode.SEQ07_TRANSFER_RIGHT_CYLINDER_BWD_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 33:    // 트랜스퍼 실린더 및 R축 받는 위치로 이동
                    {
                        if (ml.Axis[Transfer_R].MoveAbsolute((int)eAxisPipeRotate.Origin) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(34);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (Line == eLine.Left)
                                    ml.AddError(eErrorCode.MOTOR21_MOVE_TIMEOUT, iSubStep);
                                else
                                    ml.AddError(eErrorCode.MOTOR22_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 34:
                    {
                        if (ml.Axis[Transfer_R].IsMoveDone() == true)
                        {
                            // 벅큠 On
                            cIO.SetOutput(O_VACUUM, true);
                            SubNext(35);
                        }
                    }
                    break;

                case 35:    // 파이프, 니들이 없는지 확인
                    {
                        if (cIO.GetInput(I_VACUUM_SENSOR, true) == false)
                        {
                            SubNext(100);
                        }
                        else
                        {
                            if (Line == eLine.Left)
                                ml.AddError(eErrorCode.SEQ07_TRANSFER_LEFT_PIPE_DETECTION, iSubStep);
                            else
                                ml.AddError(eErrorCode.SEQ07_TRANSFER_RIGHT_NEEDLE_DETECTION, iSubStep);
                            return true;
                        }
                    }
                    break;

                case 50:    // 니들 Roate 모터 180도 회전
                    {
                        if (ml.Axis[Transfer_R].MoveRelative((int)eAxisNeedleRotate.Rotate180) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(51);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR22_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 51:
                    {
                        if (ml.Axis[Transfer_R].IsMoveDone() == true)
                        {
                            cDelay.Restart();
                            SubNext(20);
                        }
                    }
                    break;

                case 60:    // 니들 Roate 모터 90도 회전
                    {
                        PositionData PosData = null;
                        PosData = ml.cAxisPosCollData.GetPositionData(Transfer_R, (int)eAxisNeedleRotate.Rotate90);

                        if (ml.Axis[Transfer_R].MoveAbsolute(PosData.iNo) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(61);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.MOTOR22_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 61:
                    {
                        if (ml.Axis[Transfer_R].IsMoveDone() == true)
                        {
                            cDelay.Restart();
                            SubNext(20);
                        }
                    }
                    break;

                case 100:   // 맵데이터 변경
                    {
                        if (bGood == true)
                        {
                            TransferMap.SetAllStatus(eStatus.WORK_DONE);
                        }
                        else
                        {
                            TransferMap.SetAllStatus(eStatus.STANBY);
                        }

                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 트랜스퍼 NG Tray 교체 위치로 이동
        /// </summary>
        /// <returns></returns>
        public bool TransferNG_TrayChange()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} Line{Line} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            if (Line == eLine.Left)
            {
                TransferMap = ml.cRunUnitData.GetIndexData(eData.PIPE_TRANSFER);    // 파이프 맵데이터 정의
            }
            else
            {
                TransferMap = ml.cRunUnitData.GetIndexData(eData.NEEDLE_TRANSFER);  // 니들 맵데이터 정의
            }

            switch (iSubStep)
            {
                case 0:
                    {
                        SubNext(10);
                    }
                    break;

                case 10:    // 트랜스퍼 실린더 및 R축 90도 위치로 이동
                    {
                        PositionData PosData = null;

                        if (Line == eLine.Left)
                            PosData = ml.cAxisPosCollData.GetPositionData(Transfer_R, (int)eAxisPipeRotate.Rotate90);
                        else
                            PosData = ml.cAxisPosCollData.GetPositionData(Transfer_R, (int)eAxisNeedleRotate.Rotate90);

                        cIO.SetOutput(O_TRANSFER_FWD_SOL, false);
                        cIO.SetOutput(O_TRANSFER_BWD_SOL, true);
                        if (ml.Axis[Transfer_R].MoveAbsolute(PosData.iNo) == true)
                        {
                            cTimeOut.Restart();
                            SubNext(12);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                if (Line == eLine.Left)
                                    ml.AddError(eErrorCode.MOTOR21_MOVE_TIMEOUT, iSubStep);
                                else
                                    ml.AddError(eErrorCode.MOTOR22_MOVE_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 12:    // 트렌스퍼 실린더 BWD 확인
                    {
                        if (cIO.GetInput(I_TRANSFER_BWD) == true)
                        {
                            cTimeOut.Stop();
                            SubNext(13);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                if (Line == eLine.Left)
                                    ml.AddError(eErrorCode.SEQ07_TRANSFER_LEFT_CYLINDER_BWD_TIMEOUT, iSubStep);
                                else
                                    ml.AddError(eErrorCode.SEQ07_TRANSFER_RIGHT_CYLINDER_BWD_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 13:    // R축 모터 90도 방향 이동 확인
                    {
                        if (ml.Axis[Transfer_R].IsMoveDone() == true)
                        {
                            // 진공 해제
                            cIO.SetOutput(O_VACUUM, false);
                            SubNext(100);
                        }
                    }
                    break;

                case 100:   // 맵데이터 Empty로 변경
                    {
                        TransferMap.SetAllStatus(eStatus.EMPTY);

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
                        if (ml.cVar.Manual_TransferRecv == true)
                        {
                            Next(10);
                            ml.cVar.Manual_TransferRecv = false;
                        }
                        else if (ml.cVar.Manual_TransferSend == true)
                        {
                            Next(20);
                            ml.cVar.Manual_TransferSend = false;
                        }
                        else if (ml.cVar.Manual_TransferNG_Change == true)
                        {
                            Next(30);
                            ml.cVar.Manual_TransferNG_Change = false;
                        }
                    }
                    break;

                case 10:    // 파이프 or 니들 전달받는 함수
                    {
                        if (TransferRecvDelivery() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 20:    // 파이프 or 니들 Vision촬영 후 전달하는 함수
                    {
                        if (TransferSendDelivery() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 30:    // 파이프 or 니들 트랜스퍼 NG 교체 위치로 이동
                    {
                        if (TransferNG_TrayChange() == true)
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