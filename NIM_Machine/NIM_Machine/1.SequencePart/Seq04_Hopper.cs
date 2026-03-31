using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// 시퀀스 클래스
    /// </summary>
    public class Seq04_Hopper : ISequence, ISeqNo
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
        public Seq04_Hopper(eSequence sequence)
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
            eLogType = eLogType.Seq04_Hopper;

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
                ml.AddError(eErrorCode.SEQ04_TIME_OUT);
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
                        MapDataLib FeederBasket = ml.cRunUnitData.GetIndexData(eData.FEEDER_BASKET);
                        MapDataLib NeedlePnP = ml.cRunUnitData.GetIndexData(eData.NEEDLE_PnP);

                        if (NeedlePnP != null &&
                            FeederBasket != null)
                        {
                            // 호퍼 동작 맵데이터 조건 확인
                            //bool bHopperRunMapData = FeederBasket.GetAllStatus(eStatus.EMPTY, false);
                            bool bHopperRunMapData = FeederBasket.GetUnitNo(1).eStatus == eStatus.EMPTY;

                            if (bHopperRunMapData == true)
                            {
                                Next(10);
                            }
                        }
                    }
                    break;

                case 10:    // 호퍼 동작 함수
                    {
                        if (HopperRun() == true)
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
        /// Hopper 모터
        /// </summary>
        private eMotor HopperMotor;

        /// <summary>
        /// Hopper 딜레이
        /// </summary>
        private uint uiHopperDelay = 0;

        /// <summary>
        /// 니들 찾는 횟수
        /// </summary>
        private int iNeedleFindCount = 0;

        /// <summary>
        /// 호퍼 파이프 & 니들 공급
        /// </summary>
        /// <returns></returns>
        public bool HopperRun()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            MapDataLib FeederBasket = ml.cRunUnitData.GetIndexData(eData.FEEDER_BASKET);

            switch (iSubStep)
            {
                case 0:     // 호퍼 동작 맵데이터 조건 확인
                    {
                        //bool bHopperRunMapData = FeederBasket.GetAllStatus(eStatus.EMPTY, false);
                        bool bHopperRunMapData = FeederBasket.GetUnitNo(1).eStatus == eStatus.EMPTY;

                        if (bHopperRunMapData == true)
                        {
                            if (ml.cOptionData.bDryRunUse == true)
                            {
                                SubNext(100);
                            }
                            else
                            {
                                cTimeOut.Restart();
                                iNeedleFindCount = 0;
                                cIO.SetOutput((int)eIO_O.HOPPER_NEEDLE_BLOW, true);
                                SubNext(10);
                            }
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

                case 10:    // Pipe PnP Y축이 호퍼존에 벗어나있는지 확인
                    {
                        double dGetActPipePnP_Y = ml.Axis[eMotor.PIPE_PnP_Y].GetActPostion();
                        if (dGetActPipePnP_Y >= 100 ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Restart();
                            SubNext(11);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.PIPE_PNP_Y_ON_THE_FEEDER_ZONE, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 11:    // Needle PnP Y축이 호퍼존에 벗어나있는지 확인
                    {
                        double dGetActNeedlePnP_Y = ml.Axis[eMotor.NEEDLE_PnP_Y].GetActPostion();
                        if (dGetActNeedlePnP_Y >= 100 ||
                            Define.SIMULATION == true)
                        {
                            cTimeOut.Stop();
                            SubNext(12);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iMotorTimeOut)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.NEEDLE_PNP_Y_ON_THE_FEEDER_ZONE, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 12:
                    {
                        ml.AIM_Feeder.LED_On();
                        cDelay.Restart();
                        SubNext(13);
                    }
                    break;

                case 13:
                    {
                        if (cDelay.ElapsedMilliseconds > 500)
                        {
                            cDelay.Stop();
                            SubNext(20);
                        }
                    }
                    break;

                case 20:    // 비전을 촬영
                    {
                        CAM0_ResultData.DataClear();
                        ml.cCVisionToolBlockLib[0].VisionShoot(0);
                        cTimeOut.Restart();
                        SubNext(21);
                    }
                    break;

                case 21:    // 파이프 또는 니들이 놓여진 개수에 따라 Hopper에서 모듈을 공급
                    {
                        // 비전 촬영을 마쳤는지 확인
                        if (CAM0_ResultData.bShootFinish == true)
                        {
                            cTimeOut.Stop();
                            // 니들 공급할 필요가 없으면 시퀀스를 종료
                            if (CAM0_ResultData.uiNeedleCount >= cSysArray.uiFeederNeedleCount ||
                                Define.SIMULATION == true)
                            {
                                ClearBlow_BackLight();
                                SubNext(100);
                            }
                            else
                            {
                                SubNext(22);
                            }
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > 3000)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ04_FEEDER_PIPE_NEEDLE_VISION_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 22:    // AIM 피더 작동
                    {
                        ml.AIM_Feeder.FeederRun(cSysArray.uiFeederRunParam[0],
                                                cSysArray.uiFeederRunParam[1],
                                                cSysArray.uiFeederRunParam[2]);
                        cDelay.Restart();
                        SubNext(23);
                    }
                    break;

                case 23:    // 피더 딜레이 (파이프와 니들의 잔 진동이 남이 있기 때문에 +1초 딜레이를 추가한다)
                    {
                        if (cDelay.ElapsedMilliseconds > (cSysArray.uiFeederRunParam[2] * 1000) + 1000)
                        {
                            cDelay.Stop();
                            // 다시 비전촬영 하러 20번으로 이동
                            SubNext(30);
                        }
                    }
                    break;

                case 30:    // 비전을 촬영
                    {
                        CAM0_ResultData.DataClear();
                        ml.cCVisionToolBlockLib[0].VisionShoot(0);
                        cTimeOut.Restart();
                        SubNext(31);
                    }
                    break;

                case 31:    // 파이프 또는 니들이 놓여진 개수에 따라 Hopper에서 모듈을 공급
                    {
                        // 비전 촬영을 마쳤는지 확인
                        if (CAM0_ResultData.bShootFinish == true)
                        {
                            cTimeOut.Stop();
                            // 니들 공급할 필요가 없으면 시퀀스를 종료
                            if (CAM0_ResultData.uiNeedleCount >= cSysArray.uiFeederNeedleCount ||
                                Define.SIMULATION == true)
                            {
                                ClearBlow_BackLight();
                                SubNext(100);
                                break;
                            }
                            // 니들을 공급해야 할때
                            else if (CAM0_ResultData.uiNeedleCount < cSysArray.uiFeederNeedleCount)
                            {
                                // 니들 찾기 실패
                                if (iNeedleFindCount >= 5)
                                {
                                    ClearBlow_BackLight();
                                    ml.AddError(eErrorCode.SEQ04_FEEDER_NOT_FIND_NEEDLE, iSubStep);
                                    return true;
                                }
                                else
                                {
                                    iNeedleFindCount++;
                                    HopperMotor = eMotor.NEEDLE_HOPPER;
                                    uiHopperDelay = cSysArray.uiHopperNeedleRunDelay;
                                    //cIO.SetOutput((int)eIO_O.HOPPER_NEEDLE_BLOW, true);
                                    SubNext(40);
                                }
                            }
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > 3000)
                            {
                                cTimeOut.Stop();
                                ml.AddError(eErrorCode.SEQ04_FEEDER_PIPE_NEEDLE_VISION_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 40:    // Hopper를 동작시킨다.
                    {
                        ml.Axis[HopperMotor].MoveVelocity(true);
                        cDelay.Restart();
                        SubNext(41);
                    }
                    break;

                case 41:    // 딜레이만큼 Hopper를 동작 후 정지한다.
                    {
                        if (cDelay.ElapsedMilliseconds > uiHopperDelay)
                        {
                            cDelay.Stop();
                            //cIO.SetOutput((int)eIO_O.HOPPER_NEEDLE_BLOW, false);
                            ml.Axis[HopperMotor].Stop();
                            SubNext(42);
                        }
                    }
                    break;

                case 42:    // 모든 공급을 마치면 다시 파이프와 니들의 개수를 파악하기 위해 비전 촬영을 한다.
                    {
                        if (ml.Axis[HopperMotor].IsMoveDone() == true)
                        {
                            SubNext(60);
                        }
                    }
                    break;

                case 60:    // AIM 피더 작동
                    {
                        ml.AIM_Feeder.FeederRun(cSysArray.uiFeederRunParam[0],
                                                cSysArray.uiFeederRunParam[1],
                                                cSysArray.uiFeederRunParam[2]);
                        cDelay.Restart();
                        SubNext(61);
                    }
                    break;

                case 61:    // 피더 딜레이 (파이프와 니들의 잔 진동이 남이 있기 때문에 +1초 딜레이를 추가한다)
                    {
                        if (cDelay.ElapsedMilliseconds > (cSysArray.uiFeederRunParam[2] * 1000) + 1000)
                        {
                            cDelay.Stop();
                            // 다시 비전촬영 하러 20번으로 이동
                            SubNext(30);
                        }
                    }
                    break;

                case 100:   // 맵데이터 변경
                    {
                        FeederBasket.SetAllStatus(eStatus.MOUNT);

                        SubNext(0);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// 호퍼 파이프 니들 블로워 Off,ㄱ 피더 백라이트 Off
        /// </summary>
        private void ClearBlow_BackLight()
        {
            cIO.SetOutput((int)eIO_O.HOPPER_NEEDLE_BLOW, false);
            ml.AIM_Feeder.LED_Off();
        }

        #region Maunal

        /// <summary>
        /// Manual Seuence 구동 스레드
        /// </summary>
        public bool ManualDo()
        {
            if (CSeqProc.bSeqStopCommand == true)
            {
                ml.Axis[eMotor.PIPE_HOPPER].Stop();
                ml.Axis[eMotor.NEEDLE_HOPPER].Stop();
                return true;
            }

            switch (iStep)
            {
                case 0:
                    {
                        if (ml.cVar.Manual_HopperRun == true)
                        {
                            Next(10);
                            ml.cVar.Manual_HopperRun = false;
                        }
                        else if (ml.cVar.Manual_PipeHopperRunTest == true)
                        {
                            Next(20);
                            ml.cVar.Manual_PipeHopperRunTest = false;
                        }
                        else if (ml.cVar.Manual_NeedleHopperRunTest == true)
                        {
                            Next(30);
                            ml.cVar.Manual_NeedleHopperRunTest = false;
                        }
                        else if (ml.cVar.Manual_BothHopperRunTest == true)
                        {
                            Next(40);
                            ml.cVar.Manual_BothHopperRunTest = false;
                        }
                        else if (ml.cVar.Manual_FeederRunTest == true)
                        {
                            Next(50);
                            ml.cVar.Manual_FeederRunTest = false;
                        }
                        else if (ml.cVar.Manual_PipeNeedlePnPMoveSafePos == true)
                        {
                            Next(80);
                            ml.cVar.Manual_PipeNeedlePnPMoveSafePos = false;
                        }
                        else if (ml.cVar.Manual_PipeNeedleRobotAllWorkClear == true)
                        {
                            Next(90);
                            ml.cVar.Manual_PipeNeedleRobotAllWorkClear = false;
                        }
                    }
                    break;

                case 10:    // 호퍼 동작 함수
                    {
                        if (HopperRun() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 20:    // 파이프 호퍼 테스트 함수
                    {
                        if (PipeHopperRunTest() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 30:    // 니들 호퍼 테스트 함수
                    {
                        if (NeedleHopperRunTest() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 40:    // 파이프 & 니들 호퍼 테스트 함수
                    {
                        if (BothHopperRunTest() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 50:    // 피더 동작 테스트 함수
                    {
                        if (FeederRunTest() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 80:    // 파이프 니들 PnP 대기위치 이동 함수
                    {
                        if (PipeNeedlePnPMoveSafePos() == true)
                        {
                            Next(100);
                        }
                    }
                    break;

                case 90:    // 파이프 니들 관련 로봇 자재 클리어 함수
                    {
                        if (PipeNeedleRobotAllWorkClear() == true)
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

        /// <summary>
        /// 파이프 호퍼 동작 테스트
        /// </summary>
        /// <returns></returns>
        public bool PipeHopperRunTest()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            switch (iManualStep)
            {
                case 0:
                    {
                        ManualNext(10);
                    }
                    break;

                case 10:    // Hopper를 동작시킨다.
                    {
                        ml.Axis[eMotor.PIPE_HOPPER].MoveVelocity(true);
                        cDelay.Restart();
                        ManualNext(11);
                    }
                    break;

                case 11:    // 딜레이만큼 Hopper를 동작 후 정지한다.
                    {
                        if (cDelay.ElapsedMilliseconds > cSysArray.uiHopperPipeRunDelay)
                        {
                            cDelay.Stop();
                            ml.Axis[eMotor.PIPE_HOPPER].Stop();
                            ManualNext(12);
                        }
                    }
                    break;

                case 12:
                    {
                        if (ml.Axis[eMotor.PIPE_HOPPER].IsMoveDone() == true)
                        {
                            ManualNext(100);
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

        /// <summary>
        /// 니들 호퍼 동작 테스트
        /// </summary>
        /// <returns></returns>
        public bool NeedleHopperRunTest()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            switch (iManualStep)
            {
                case 0:
                    {
                        ManualNext(10);
                    }
                    break;

                case 10:    // Hopper를 동작시킨다.
                    {
                        ml.Axis[eMotor.NEEDLE_HOPPER].MoveVelocity(true);
                        cDelay.Restart();
                        ManualNext(11);
                    }
                    break;

                case 11:    // 딜레이만큼 Hopper를 동작 후 정지한다.
                    {
                        if (cDelay.ElapsedMilliseconds > cSysArray.uiHopperNeedleRunDelay)
                        {
                            cDelay.Stop();
                            ml.Axis[eMotor.NEEDLE_HOPPER].Stop();
                            ManualNext(12);
                        }
                    }
                    break;

                case 12:
                    {
                        if (ml.Axis[eMotor.NEEDLE_HOPPER].IsMoveDone() == true)
                        {
                            ManualNext(100);
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

        /// <summary>
        /// 니들 호퍼 동작 테스트
        /// </summary>
        /// <returns></returns>
        public bool BothHopperRunTest()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            switch (iManualStep)
            {
                case 0:
                    {
                        ManualNext(10);
                    }
                    break;

                case 10:    // 두개 Hopper 동작
                    {
                        ml.Axis[eMotor.PIPE_HOPPER].MoveVelocity(true);
                        ml.Axis[eMotor.NEEDLE_HOPPER].MoveVelocity(true);
                        cDelay.Restart();
                        ManualNext(11);
                    }
                    break;

                case 11:    // 딜레이만큼 Hopper를 동작 후 정지한다.
                    {
                        if (cDelay.ElapsedMilliseconds > cSysArray.uiHopperPipeRunDelay)
                        {
                            ml.Axis[eMotor.PIPE_HOPPER].Stop();
                        }

                        if (cDelay.ElapsedMilliseconds > cSysArray.uiHopperNeedleRunDelay)
                        {
                            ml.Axis[eMotor.NEEDLE_HOPPER].Stop();
                        }

                        if (ml.Axis[eMotor.PIPE_HOPPER].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_HOPPER].IsMoveDone() == true)
                        {
                            cDelay.Stop();
                            ManualNext(100);
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

        /// <summary>
        /// 피더 동작 테스트
        /// </summary>
        /// <returns></returns>
        public bool FeederRunTest()
        {
            if (iSubStep != iPreSubStep)
            {
                iPreSubStep = iSubStep;
                string strFuncName = MethodBase.GetCurrentMethod().Name;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, string.Format($"{SeqName} {strFuncName} Func Step : {iSubStep} Process"), false);
            }

            switch (iManualStep)
            {
                case 0:
                    {
                        ManualNext(10);
                    }
                    break;

                case 10:    // AIM 피더 작동
                    {
                        ml.AIM_Feeder.FeederRun(cSysArray.uiFeederRunParam[0],
                                                cSysArray.uiFeederRunParam[1],
                                                cSysArray.uiFeederRunParam[2]);
                        cDelay.Restart();
                        ManualNext(11);
                    }
                    break;

                case 12:    // 피더 딜레이 (파이프와 니들의 잔 진동이 남이 있기 때문에 +1초 딜레이를 추가한다)
                    {
                        if (cDelay.ElapsedMilliseconds > (cSysArray.uiFeederRunParam[2] * 1000) + 1000)
                        {
                            cDelay.Stop();
                            ManualNext(100);
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

        /// <summary>
        /// 파이프, 니들 PnP 대기위치 이동
        /// </summary>
        /// <returns></returns>
        public bool PipeNeedlePnPMoveSafePos()
        {
            switch (iManualStep)
            {
                case 0:
                    {
                        ManualNext(10);
                    }
                    break;

                case 10:    // 파이프, 니들 Z축 안전 위치로 이동
                    {
                        ml.Axis[eMotor.PIPE_PnP_Z].MoveAbsolute((int)eAxisPipePnP_Z.Safe);
                        ml.Axis[eMotor.NEEDLE_PnP_Z].MoveAbsolute((int)eAxisNeedlePnP_Z.Safe);
                        ManualNext(11);
                    }
                    break;

                case 11:
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Z].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_PnP_Z].IsMoveDone() == true)
                        {
                            ManualNext(12);
                        }
                    }
                    break;

                case 12:    // 파이프, 니들 PnP X축 대기위치 이동
                    {
                        ml.Axis[eMotor.PIPE_PnP_X].MoveAbsolute((int)eAxisPipePnP_X.Safe);
                        ml.Axis[eMotor.NEEDLE_PnP_X].MoveAbsolute((int)eAxisNeedlePnP_X.Safe);
                        ManualNext(13);
                    }
                    break;

                case 13:
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_PnP_X].IsMoveDone() == true)
                        {
                            ManualNext(14);
                        }
                    }
                    break;

                case 14:    // 파이프, 니들 PnP Y축 대기위치 이동
                    {
                        ml.Axis[eMotor.PIPE_PnP_Y].MoveAbsolute((int)eAxisPipePnP_Y.Safe);
                        ml.Axis[eMotor.NEEDLE_PnP_Y].MoveAbsolute((int)eAxisNeedlePnP_Y.Safe);
                        ManualNext(15);
                    }
                    break;

                case 15:
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Y].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_PnP_Y].IsMoveDone() == true)
                        {
                            ManualNext(100);
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

        /// <summary>
        /// 파이프와 니들 공급에 관련된 로봇들의 자재들을 전부 Clear 시켜준다.
        /// </summary>
        /// <returns></returns>
        public bool PipeNeedleRobotAllWorkClear()
        {
            switch (iManualStep)
            {
                case 0:
                    {
                        ManualNext(10);
                    }
                    break;

                case 10:    // Z축 관련 모두 안전 위치로 이동
                    {
                        ml.Axis[eMotor.PIPE_PnP_Z].MoveAbsolute((int)eAxisPipePnP_Z.Safe);
                        ml.Axis[eMotor.NEEDLE_PnP_Z].MoveAbsolute((int)eAxisNeedlePnP_Z.Safe);
                        ml.Axis[eMotor.PIPE_MOUNT_Z].MoveAbsolute((int)eAxisPipeMount_Z.Safe);
                        ml.Axis[eMotor.NEEDLE_MOUNT_Z].MoveAbsolute((int)eAxisNeedleMount_Z.Safe);
                        ManualNext(11);
                    }
                    break;

                case 11:
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Z].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_PnP_Z].IsMoveDone() == true &&
                            ml.Axis[eMotor.PIPE_MOUNT_Z].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_MOUNT_Z].IsMoveDone() == true)
                        {
                            ManualNext(12);
                        }
                    }
                    break;

                case 12:    // 파이프, 니들 PnP Y축 픽업위치로 이동
                    {
                        ml.Axis[eMotor.PIPE_PnP_Y].MoveAbsolute((int)eAxisPipePnP_Y.PipePickUp);
                        ml.Axis[eMotor.NEEDLE_PnP_Y].MoveAbsolute((int)eAxisNeedlePnP_Y.NeedlePickUp);
                        ManualNext(13);
                    }
                    break;

                case 13:
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Y].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_PnP_Y].IsMoveDone() == true)
                        {
                            ManualNext(14);
                        }
                    }
                    break;

                case 14:    // 파이프, 니들 PnP X축 픽업 위치로 이동
                    {
                        ml.Axis[eMotor.PIPE_PnP_X].MoveAbsolute((int)eAxisPipePnP_X.PipePickUp);
                        ml.Axis[eMotor.NEEDLE_PnP_X].MoveAbsolute((int)eAxisNeedlePnP_X.NeedlePickUp);
                        ManualNext(15);
                    }
                    break;

                case 15:    // 파이프, 니들 벅큠 해제
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_PnP_X].IsMoveDone() == true)
                        {
                            cIO.SetOutput((int)eIO_O.PIPE_VACUUM, false);
                            cIO.SetOutput((int)eIO_O.NEEDLE_VACUUM, false);
                            ManualNext(20);
                        }
                    }
                    break;

                case 20:
                    {
                        // 트랜스퍼 로테이션 축 90도 위치로 이동
                        ml.Axis[eMotor.PIPE_ROTATE].MoveAbsolute((int)eAxisPipeRotate.Trash);
                        ml.Axis[eMotor.NEEDLE_ROTATE].MoveAbsolute((int)eAxisNeedleRotate.Trash);
                        cIO.SetOutput((int)eIO_O.TRANSFER_PIPE_FORWARD, true);
                        cIO.SetOutput((int)eIO_O.TRANSFER_PIPE_BACKWARD, false);
                        cIO.SetOutput((int)eIO_O.TRANSFER_NEEDLE_FORWARD, true);
                        cIO.SetOutput((int)eIO_O.TRANSFER_NEEDLE_BACKWARD, false);
                        ManualNext(21);
                    }
                    break;

                case 21:
                    {
                        if (cIO.GetInput((int)eIO_I.TRANSFER_PIPE_FORWARD) == true &&
                            cIO.GetInput((int)eIO_I.TRANSFER_NEEDLE_FORWARD) == true)
                        {
                            cTimeOut.Stop();
                            ManualNext(22);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                if (cIO.GetInput((int)eIO_I.TRANSFER_PIPE_FORWARD) == false)
                                    ml.AddError(eErrorCode.SEQ07_TRANSFER_LEFT_CYLINDER_FWD_TIMEOUT, iManualStep);
                                else if (cIO.GetInput((int)eIO_I.TRANSFER_NEEDLE_FORWARD) == false)
                                    ml.AddError(eErrorCode.SEQ07_TRANSFER_RIGHT_CYLINDER_FWD_TIMEOUT, iManualStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 22:
                    {
                        if (ml.Axis[eMotor.PIPE_ROTATE].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_ROTATE].IsMoveDone() == true)
                        {
                            ManualNext(23);
                        }
                    }
                    break;

                case 23:    // 트랜스퍼 로테이션 벅큠 해제
                    {
                        cIO.SetOutput((int)eIO_O.TRANSFER_PIPE_VACUUM, false);
                        cIO.SetOutput((int)eIO_O.TRANSFER_NEEDLE_VACUUM, false);
                        cIO.SetOutput((int)eIO_O.TRANSFER_PIPE_BLOW, true);
                        cIO.SetOutput((int)eIO_O.TRANSFER_NEEDLE_BLOW, true);
                        cTimeOut.Restart();
                        ManualNext(30);
                    }
                    break;

                case 30:
                    {
                        ml.Axis[eMotor.PIPE_MOUNT_X].MoveAbsolute((int)eAxisPipeMount_X.PipeClampUp);
                        ml.Axis[eMotor.PIPE_MOUNT_Y].MoveAbsolute((int)eAxisPipeMount_Y.PipeClampUp);
                        ml.Axis[eMotor.NEEDLE_MOUNT_X].MoveAbsolute((int)eAxisNeedleMount_X.NeedleClampUp);
                        ml.Axis[eMotor.NEEDLE_MOUNT_Y].MoveAbsolute((int)eAxisNeedleMount_Y.NeedleClampUp);
                        cTimeOut.Stop();
                        ManualNext(31);
                    }
                    break;

                case 31:
                    {
                        if (ml.Axis[eMotor.PIPE_MOUNT_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.PIPE_MOUNT_Y].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_MOUNT_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_MOUNT_Y].IsMoveDone() == true)
                        {
                            ManualNext(32);
                        }
                    }
                    break;

                case 32:
                    {
                        cIO.SetOutput((int)eIO_O.PIPE_CHUCK_OPEN, true);
                        cIO.SetOutput((int)eIO_O.NEEDLE_CHUCK_OPEN, true);
                        cIO.SetOutput((int)eIO_O.NEEDLE_CLAMP_VACUUM, false);
                        cTimeOut.Restart();
                        ManualNext(34);
                    }
                    break;

                case 34:
                    {
                        if (cIO.GetInput((int)eIO_I.PIPE_CHUCK_OPEN) == true &&
                            cIO.GetInput((int)eIO_I.NEEDLE_CHUCK_OPEN) == true)
                        {
                            cTimeOut.Stop();
                            ManualNext(40);
                        }
                        else
                        {
                            if (cTimeOut.ElapsedMilliseconds > ml.cSysOne.iSensorCheckTimeOut)
                            {
                                cTimeOut.Stop();
                                if (cIO.GetInput((int)eIO_I.PIPE_CHUCK_OPEN) == false)
                                    ml.AddError(eErrorCode.SEQ08_PIPE_MOUNT_CHUCK_CYLINDER_OPEN_TIMEOUT, iSubStep);
                                else if (cIO.GetInput((int)eIO_I.NEEDLE_CHUCK_OPEN) == false)
                                    ml.AddError(eErrorCode.SEQ09_NEEDLE_MOUNT_CHUCK_CYLINDER_OPEN_TIMEOUT, iSubStep);
                                return true;
                            }
                        }
                    }
                    break;

                case 40:    // 파이프, 니들 PnP X축 대기위치 이동
                    {
                        ml.Axis[eMotor.PIPE_PnP_X].MoveAbsolute((int)eAxisPipePnP_X.Safe);
                        ml.Axis[eMotor.NEEDLE_PnP_X].MoveAbsolute((int)eAxisNeedlePnP_X.Safe);
                        ManualNext(41);
                    }
                    break;

                case 41:
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_X].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_PnP_X].IsMoveDone() == true)
                        {
                            ManualNext(42);
                        }
                    }
                    break;

                case 42:    // 파이프, 니들 PnP Y축 대기위치 이동
                    {
                        ml.Axis[eMotor.PIPE_PnP_Y].MoveAbsolute((int)eAxisPipePnP_Y.Safe);
                        ml.Axis[eMotor.NEEDLE_PnP_Y].MoveAbsolute((int)eAxisNeedlePnP_Y.Safe);
                        ManualNext(43);
                    }
                    break;

                //니들 마운터 Blow ON
                case 43:
                    {
                        cIO.SetOutput((int)eIO_O.NEEDLE_CLAMP_BLOW, true);
                        cTimeOut.Restart();
                        ManualNext(44);
                    }
                    break;
                //니들 마운터 Blow OFF
                case 44:
                    {
                        if (cTimeOut.ElapsedMilliseconds > 1000)
                        {
                            cIO.SetOutput((int)eIO_O.NEEDLE_CLAMP_BLOW, false);
                            cTimeOut.Stop();
                            ManualNext(45);
                        }
                    }
                    break;

                case 45:
                    {
                        if (ml.Axis[eMotor.PIPE_PnP_Y].IsMoveDone() == true &&
                            ml.Axis[eMotor.NEEDLE_PnP_Y].IsMoveDone() == true)
                        {
                            cIO.SetOutput((int)eIO_O.TRANSFER_PIPE_BLOW, false);
                            cIO.SetOutput((int)eIO_O.TRANSFER_NEEDLE_BLOW, false);
                            ManualNext(100);
                        }
                    }
                    break;

                case 100:   // 관련 데이터는 모두 Empty로 변경한다.
                    {
                        ml.cRunUnitData.GetIndexData(eData.PIPE_PnP).SetAllStatus(eStatus.EMPTY);
                        ml.cRunUnitData.GetIndexData(eData.NEEDLE_PnP).SetAllStatus(eStatus.EMPTY);
                        ml.cRunUnitData.GetIndexData(eData.PIPE_TRANSFER).SetAllStatus(eStatus.EMPTY);
                        ml.cRunUnitData.GetIndexData(eData.NEEDLE_TRANSFER).SetAllStatus(eStatus.EMPTY);
                        ml.cRunUnitData.GetIndexData(eData.PIPE_MOUNTER).SetAllStatus(eStatus.EMPTY);
                        ml.cRunUnitData.GetIndexData(eData.NEEDLE_MOUNTER).SetAllStatus(eStatus.EMPTY);
                        ml.cRunUnitData.GetIndexData(eData.FEEDER_BASKET).SetAllStatus(eStatus.EMPTY);
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