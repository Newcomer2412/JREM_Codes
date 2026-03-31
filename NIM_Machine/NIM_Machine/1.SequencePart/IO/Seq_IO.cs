using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// Read IO (IO 관련 모든 제어 클래스)
    /// </summary>
    public class Seq_IO
    {
        /// <summary>
        /// Main Lib
        /// </summary>
        private CMainLib ml;

        /* 입출력 스레드 Lock */
        private static readonly object _Inputlocker = new object();
        private static readonly object _Inputlocker_Data = new object();
        private static readonly object _Outputlocker = new object();
        private static readonly object _Outputlocker_Data = new object();
        private static readonly object _Outputlocker_OldData = new object();

        /// <summary>
        /// 시퀀스 스레드 동작 관리 클래스
        /// </summary>
        public CSeqProc Proc { get; set; }

        /// <summary>
        /// IO 변수 정의
        /// </summary>
        private static uint[] _IO_Input = new uint[Define.INPUT_TOTAL_BIT / Define.INPUT_DEFINE_BIT];

        public static uint[] IO_Input
        {
            get
            {
                lock (_Inputlocker_Data)
                {
                    return _IO_Input;
                }
            }
            set
            {
                lock (_Inputlocker_Data)
                {
                    _IO_Input = value;
                }
            }
        }

        private static uint[] _IO_Output = new uint[Define.OUTPUT_TOTAL_BIT / Define.OUTPUT_DEFINE_BIT];

        public static uint[] IO_Output
        {
            get
            {
                lock (_Outputlocker_Data)
                {
                    return _IO_Output;
                }
            }
            set
            {
                lock (_Outputlocker_Data)
                {
                    _IO_Output = value;
                }
            }
        }

        private static uint[] _IO_Output_Old = new uint[Define.OUTPUT_TOTAL_BIT / Define.OUTPUT_DEFINE_BIT];

        public static uint[] IO_Output_Old
        {
            get
            {
                lock (_Outputlocker_OldData)
                {
                    return _IO_Output_Old;
                }
            }
            set
            {
                lock (_Outputlocker_OldData)
                {
                    _IO_Output_Old = value;
                }
            }
        }

        /* IO 주소 외부 재정의로 인해 변경된 주소 저장 리스트 */
        private List<CIOData> cInputList = new List<CIOData>();
        private List<CIOData> cOutputList = new List<CIOData>();

        /// <summary>
        /// Input Name
        /// </summary>
        private string[] strInputName = new string[Define.INPUT_TOTAL_BIT];

        /// <summary>
        /// Output Name
        /// </summary>
        private string[] strOutputName = new string[Define.OUTPUT_TOTAL_BIT];

        /// <summary>
        /// 생성자
        /// </summary>
        public Seq_IO()
        {
            ml = CMainLib.Ins;
            InitIOList();
            InitName();
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Init()
        {
            Proc = new CSeqProc();
            Proc.SeqName = "ReadIO";
        }

        /// <summary>
        /// 프로그램 종료
        /// </summary>
        private bool bClose = false;

        /// <summary>
        /// 시퀀스 소멸
        /// </summary>
        public void Free()
        {
            bClose = true;
            Proc.Free();
        }

        /// <summary>
        /// 스래드 시작
        /// </summary>
        public void Start()
        {
            Proc.Start(Do, ThreadPriority.Highest);
        }

        /// <summary>
        /// 스래드 정지
        /// </summary>
        public void Stop()
        {
            Proc.Stop();
        }

        /// <summary>
        /// 메인 Sequence 구동 스레드
        /// </summary>
        public bool Do()
        {
            if (bClose == true || Define.SIMULATION == true) return true;
            GetRepeatInput();
            SetRepeatOutput();
            Thread.Sleep(3);
            return false;
        }

        /// <summary>
        /// 프로그램 구동 초기 IO를 읽어 초기화
        /// </summary>
        public void InitFirstIO()
        {
            GetRepeatInput();
            ml.IO.First_Output();
        }

        /// <summary>
        /// IO 리스트를 정의한다. 이곳에서 IO 주소 외부 변경 시 대응
        /// </summary>
        private void InitIOList()
        {
            // 데이터 로드 시 문제 생기는지 확인하기 위해 try-catch 구문을 통해 로그 추가
            try
            {
                string strFilePath = string.Format("{0}ChangeIO.ini", CXMLProcess.IniDataFolderPath);

                if (!System.IO.File.Exists(strFilePath))
                {
                    throw new Exception();
                }

                CIniFile cIni = new CIniFile();
                cIni.Load(strFilePath);
                int iGetCurruntNum;

                for (int i = 0; i < Define.INPUT_TOTAL_BIT; i++)
                {
                    string strName = Enum.GetName(typeof(eIO_I), i);
                    if (strName != null)
                    {
                        int.TryParse(cIni["INPUT"][i.ToString()].ToString(), out iGetCurruntNum);
                        if (iGetCurruntNum != -1 && iGetCurruntNum != i)
                        {
                            CIOData cIOData = new CIOData();
                            cIOData.iOriginIONum = i;
                            cIOData.iCurrentIONum = iGetCurruntNum;
                            cInputList.Add(cIOData);
                        }
                    }
                }

                for (int i = 0; i < Define.OUTPUT_TOTAL_BIT; i++)
                {
                    string strName = Enum.GetName(typeof(eIO_O), i);
                    if (strName != null)
                    {
                        int.TryParse(cIni["OUTPUT"][i.ToString()].ToString(), out iGetCurruntNum);
                        if (iGetCurruntNum != -1 && iGetCurruntNum != i)
                        {
                            CIOData cIOData = new CIOData();
                            cIOData.iOriginIONum = i;
                            cIOData.iCurrentIONum = iGetCurruntNum;
                            cOutputList.Add(cIOData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.DEBUG, ex.ToString());
                CCommon.ShowMessageMini("ChangeIO.ini 파일 손상");
            }
        }

        /// <summary>
        /// IO Name을 등록한다.
        /// </summary>
        private void InitName()
        {
            for (int i = 0; i < Define.INPUT_TOTAL_BIT; i++)
            {
                string strName = Enum.GetName(typeof(eIO_I), i);
                if (strName != null)
                {
                    int iCurruntNo = GetInputCurruntNum(i);
                    if (iCurruntNo == -1) strInputName[i] = strName;
                    else strInputName[iCurruntNo] = strName;
                }
            }

            for (int i = 0; i < Define.OUTPUT_TOTAL_BIT; i++)
            {
                string strName = Enum.GetName(typeof(eIO_O), i);
                if (strName != null)
                {
                    int iCurruntNo = GetOutputCurruntNum(i);
                    if (iCurruntNo == -1) strOutputName[i] = strName;
                    else strOutputName[iCurruntNo] = strName;
                }
            }

            for (int i = 0; i < Define.INPUT_TOTAL_BIT; i++)
            {
                if (strInputName[i] == null || strInputName[i] == string.Empty) strInputName[i] = "EMPTY";
            }

            for (int i = 0; i < Define.OUTPUT_TOTAL_BIT; i++)
            {
                if (strOutputName[i] == null || strOutputName[i] == string.Empty) strOutputName[i] = "EMPTY";
            }
        }

        /// <summary>
        /// Input의 실재 사용 주소를 리턴한다.
        /// </summary>
        /// <param name="iInputNo"></param>
        /// <returns></returns>
        public int GetInputCurruntNum(int iInputNo)
        {
            CIOData cIOData = cInputList.Find(x => x.iOriginIONum == iInputNo);
            if (cIOData == null) return -1;
            else return cIOData.iCurrentIONum;
        }

        /// <summary>
        /// Output의 실재 사용 주소를 리턴한다.
        /// </summary>
        /// <param name="iOutputNo"></param>
        /// <returns></returns>
        public int GetOutputCurruntNum(int iOutputNo)
        {
            CIOData cIOData = cOutputList.Find(x => x.iOriginIONum == iOutputNo);
            if (cIOData == null) return -1;
            else return cIOData.iCurrentIONum;
        }

        /// <summary>
        /// Input의 Name을 리턴
        /// </summary>
        /// <param name="iInputNo"></param>
        /// <returns></returns>
        public string GetInputName(int iInputNo)
        {
            return strInputName[iInputNo];
        }

        /// <summary>
        /// Output의 Name을 리턴
        /// </summary>
        /// <param name="iOutputNo"></param>
        /// <returns></returns>
        public string GetOutputName(int iOutputNo)
        {
            return strOutputName[iOutputNo];
        }

        /// <summary>
        /// IO 데이터를 초기화
        /// </summary>
        public void IO_init()
        {
            //※ 여기서 IO out 경우 변수와 IO_inout을 똑같이 맞추어 준다.
            // 만약 High 로 시작할 경우는 여기서 설정하여 주어야 한다.
            int iModule = Define.INPUT_TOTAL_BIT / Define.INPUT_DEFINE_BIT;

            for (int i = 0; i < iModule; i++)
            {
                IO_Input[i] = 0;
            }

            iModule = Define.OUTPUT_TOTAL_BIT / Define.OUTPUT_DEFINE_BIT;

            for (int i = 0; i < iModule; i++)
            {
                if (i == 0)
                {
                    IO_Output[i] &= 0x0003;
                    IO_Output_Old[i] &= 0x0003;
                }
                else
                {
                    IO_Output[i] = 0;
                    IO_Output_Old[i] = 0;
                }
            }

            // 초기 IO를 읽어 데이터 설정
            InitFirstIO();
        }

        /// <summary>
        /// 반복적으로 Input을 갱신 하는 함수
        /// </summary>
        public void GetRepeatInput()
        {
            lock (_Inputlocker)
            {
                ml.IO.GetRepeatInput();
            }
        }

        /// <summary>
        /// IO Output Channel
        /// </summary>
        public void SetRepeatOutput()
        {
            lock (_Outputlocker)
            {
                ml.IO.SetRepeatOutput();
            }
        }

        /// <summary>
        /// 입력 처리
        /// </summary>
        /// <param name="iInputNo">Input 번호</param>
        /// <param name="bOffGood">시뮬레이션일 경우 상태를 정의 한다. 플래그 기본 On, true 삽입 시 Off로 리턴</param>
        /// <param name="bChangeAddress">어드레스를 강제로 변경한다</param>
        /// <returns></returns>
        public bool GetInput(int iInputNo, bool bOffGood = false, bool bChangeAddress = false)
        {
            lock (_Inputlocker)
            {
                int iNo = -1;
                if (bChangeAddress == true)
                {
                    CIOData cIOData = cInputList.Find(x => x.iOriginIONum == iInputNo);
                    if (cIOData == null) iNo = iInputNo;
                    else iNo = cIOData.iCurrentIONum;
                }
                else
                {
                    iNo = iInputNo;
                }

                int iModule = iNo / Define.INPUT_DEFINE_BIT;
                int iBitData = 0;
                if (iModule > 0) iBitData = (0x1 << (iNo % Define.INPUT_DEFINE_BIT));
                else iBitData = 0x1 << iNo;

                if (Define.SIMULATION == true)
                {
                    bool bDone = false;
                    if (SimCylinderInputCheck(iInputNo, ref bDone) == true)
                    {
                        if (bDone == true)
                        {
                            IO_Input[iModule] |= (uint)iBitData;
                        }
                        else
                        {
                            iBitData = ~iBitData;
                            IO_Input[iModule] &= (uint)iBitData;
                        }
                    }
                    else
                    {
                        if (bOffGood == true)
                        {
                            iBitData = ~iBitData;
                            IO_Input[iModule] &= (uint)iBitData;
                        }
                        else
                        {
                            IO_Input[iModule] |= (uint)iBitData;
                        }
                    }
                }

                return (IO_Input[iModule] & iBitData) == iBitData;
            }
        }

        /// <summary>
        /// 출력 처리
        /// </summary>
        /// <param name="iOutputNo"></param>
        /// <param name="bChangeAddress"></param>
        /// <returns></returns>
        public bool GetOutput(int iOutputNo, bool bChangeAddress = false)
        {
            lock (_Outputlocker)
            {
                int iNo = -1;
                if (bChangeAddress == true)
                {
                    CIOData cIOData = cOutputList.Find(x => x.iOriginIONum == iOutputNo);
                    if (cIOData == null) iNo = iOutputNo;
                    else iNo = cIOData.iCurrentIONum;
                }
                else
                {
                    iNo = iOutputNo;
                }

                int iModule = iNo / Define.OUTPUT_DEFINE_BIT;
                int iBitData = 0;
                if (iModule > 0) iBitData = (0x1 << (iNo % Define.OUTPUT_DEFINE_BIT));
                else iBitData = 0x1 << iNo;

                return (IO_Output[iModule] & iBitData) == iBitData;
            }
        }

        /// <summary>
        /// 출력 처리
        /// </summary>
        /// <param name="iOutputNo"></param>
        /// <param name="bOnOff"></param>
        /// <param name="bChangeAddress"></param>
        public void SetOutput(int iOutputNo, bool bOnOff, bool bChangeAddress = false)
        {
            lock (_Outputlocker)
            {
                int iNo = -1;
                if (bChangeAddress == true)
                {
                    CIOData cIOData = cOutputList.Find(x => x.iOriginIONum == iOutputNo);
                    if (cIOData == null) iNo = iOutputNo;
                    else iNo = cIOData.iCurrentIONum;
                }
                else
                {
                    iNo = iOutputNo;
                }
                int iModule = iNo / Define.OUTPUT_DEFINE_BIT;
                uint iBitData = 0;
                if (iModule > 0) iBitData = (uint)(0x1 << (iNo % Define.OUTPUT_DEFINE_BIT));
                else iBitData = (uint)(0x1 << iNo);

                if (bOnOff == true) IO_Output[iModule] |= iBitData;
                else IO_Output[iModule] &= ~iBitData;
            }
        }

        /// <summary>
        /// Input을 직접 제어기에서 읽음
        /// </summary>
        /// <param name="iInputNo"></param>
        /// <param name="bInput"></param>
        /// <param name="bChangeAddress"></param>
        /// <returns></returns>
        public bool GetDirectInput(int iInputNo, ref bool bInput, bool bChangeAddress = true)
        {
            lock (_Inputlocker)
            {
                int iNo = -1;
                if (bChangeAddress == true)
                {
                    CIOData cIOData = cInputList.Find(x => x.iOriginIONum == iInputNo);
                    if (cIOData == null) iNo = iInputNo;
                    else iNo = cIOData.iCurrentIONum;
                }
                else
                {
                    iNo = iInputNo;
                }

                int iInputData = 0;
                if (ml.IO.GetSingleInput(iNo, ref iInputData) == true)
                {
                    if (iInputData == 1) bInput = true;
                    else bInput = false;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Output을 직접 제어기에서 읽음
        /// </summary>
        /// <param name="iOutputNo"></param>
        /// <param name="bOutput"></param>
        /// <param name="bChangeAddress"></param>
        /// <returns></returns>
        public bool GetDirectOutput(int iOutputNo, ref bool bOutput, bool bChangeAddress = false)
        {
            lock (_Outputlocker)
            {
                int iNo = -1;
                if (bChangeAddress == true)
                {
                    CIOData cIOData = cOutputList.Find(x => x.iOriginIONum == iOutputNo);
                    if (cIOData == null) iNo = iOutputNo;
                    else iNo = cIOData.iCurrentIONum;
                }
                else
                {
                    iNo = iOutputNo;
                }

                int iOutputData = 0;
                if (ml.IO.GetSingleOutput(iNo, ref iOutputData) == true)
                {
                    if (iOutputData == 1) bOutput = true;
                    else bOutput = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Output을 직접 제어기에 명령
        /// </summary>
        /// <param name="iOutputNo"></param>
        /// <param name="bOnOff"></param>
        /// <param name="bChangeAddress"></param>
        public void SetDirectOutput(int iOutputNo, bool bOnOff, bool bChangeAddress = false)
        {
            lock (_Outputlocker)
            {
                int iNo = -1;
                if (bChangeAddress == true)
                {
                    CIOData cIOData = cOutputList.Find(x => x.iOriginIONum == iOutputNo);
                    if (cIOData == null) iNo = iOutputNo;
                    else iNo = cIOData.iCurrentIONum;
                }
                else
                {
                    iNo = iOutputNo;
                }

                int iModule = iNo / Define.OUTPUT_DEFINE_BIT;
                uint iBitData = 0;
                if (iModule > 0) iBitData = (uint)(0x1 << (iNo % Define.OUTPUT_DEFINE_BIT));
                else iBitData = (uint)(0x1 << iNo);
                if (bOnOff == true)
                {
                    if (Define.SIMULATION == false)
                    {
                        if (ml.IO.SetOutput(iNo, 1) == true) IO_Output_Old[iModule] |= iBitData;
                        IO_Output[iModule] |= iBitData;
                    }
                    else
                    {
                        IO_Output[iModule] |= iBitData;
                    }
                }
                else
                {
                    if (Define.SIMULATION == false)
                    {
                        if (ml.IO.SetOutput(iNo, 0) == true) IO_Output_Old[iModule] &= ~iBitData;
                        IO_Output[iModule] &= ~iBitData;
                    }
                    else
                    {
                        IO_Output[iModule] &= ~iBitData;
                    }
                }
            }
        }

        /// <summary>
        /// Machine State에 따른 부저, 램프 제어
        /// </summary>
        /// <param name="eFlag"></param>
        public void SetMachineState(eMachineState eFlag)
        {
            switch (eFlag)
            {
                case eMachineState.INIT:
                    ml.McState = eMachineState.INIT;
                    SetPatLight(_eLightColor.BLACK);
                    SetSwitchLamp(eMachineState.INIT);
                    SetBuzzer(false);
                    break;

                case eMachineState.READY:
                    ml.McState = eMachineState.READY;
                    SetPatLight(_eLightColor.YELLOW);
                    SetSwitchLamp(eMachineState.READY);
                    SetBuzzer(false);
                    ml.cVar.iErrorCode = -1;
                    break;

                case eMachineState.RUN:
                    ml.McState = eMachineState.RUN;
                    SetPatLight(_eLightColor.GREEN);
                    SetSwitchLamp(eMachineState.RUN);
                    SetBuzzer(false);
                    break;

                case eMachineState.MANUALRUN:
                    ml.McState = eMachineState.MANUALRUN;
                    SetPatLight(_eLightColor.GREEN);
                    SetSwitchLamp(eMachineState.MANUALRUN);
                    break;

                case eMachineState.ALARM:
                    SetPatLight(_eLightColor.RED);
                    SetBuzzer(true);
                    break;

                case eMachineState.ERROR:
                    ml.McState = eMachineState.ERROR;
                    SetPatLight(_eLightColor.RED);
                    SetSwitchLamp(eMachineState.ERROR);
                    SetBuzzer(true);
                    break;
            }
        }

        /// <summary>
        /// 색상
        /// </summary>
        private _eLightColor eLightColor = _eLightColor.RED;

        /// <summary>
        /// Light 점멸용 타이머
        /// </summary>
        private DispatcherTimer cLightBlinkTimer = null;

        /// <summary>
        /// Pat Light를 제어
        /// </summary>
        /// <param name="eFlag">색상</param>
        /// <param name="bBlink">점멸</param>
        public void SetPatLight(_eLightColor eFlag, bool bBlink = false)
        {
            if (cLightBlinkTimer == null)
            {
                cLightBlinkTimer = new DispatcherTimer();
                cLightBlinkTimer.Interval = TimeSpan.FromMilliseconds(500);         // 시간 간격 설정
                cLightBlinkTimer.Tick += new EventHandler(LightBlinkTimer_Tick);    // 이벤트 추가
            }

            if (bBlink == true)
            {
                eLightColor = eFlag;
                cLightBlinkTimer.Start(); // 타이머 시작
            }
            else
            {
                cLightBlinkTimer.Stop();
                Thread.Sleep(100);
                PatLightOnOff(eFlag);
            }
        }

        /// <summary>
        /// 라이트 점멸 온오프 플래그
        /// </summary>
        private bool bLightBlinkOnOff = false;

        /// <summary>
        /// 데이터 갱신용 반복 타이머
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LightBlinkTimer_Tick(object sender, EventArgs e)
        {
            if (bLightBlinkOnOff == false)
            {
                PatLightOnOff(eLightColor);
                bLightBlinkOnOff = true;
            }
            else
            {
                PatLightOnOff(_eLightColor.BLACK);
                bLightBlinkOnOff = false;
            }
        }

        /// <summary>
        /// 경광등을 색상에 따라 On/Off 한다.
        /// </summary>
        /// <param name="eFlag"></param>
        public void PatLightOnOff(_eLightColor eFlag)
        {
            switch (eFlag)
            {
                case _eLightColor.RED:
                    SetOutput((int)eIO_O.TOWER_LAMP_RED, true);
                    SetOutput((int)eIO_O.TOWER_LAMP_GREEN, false);
                    SetOutput((int)eIO_O.TOWER_LAMP_YELLOW, false);
                    break;

                case _eLightColor.GREEN:
                    SetOutput((int)eIO_O.TOWER_LAMP_RED, false);
                    SetOutput((int)eIO_O.TOWER_LAMP_GREEN, true);
                    SetOutput((int)eIO_O.TOWER_LAMP_YELLOW, false);
                    break;

                case _eLightColor.YELLOW:
                    SetOutput((int)eIO_O.TOWER_LAMP_RED, false);
                    SetOutput((int)eIO_O.TOWER_LAMP_GREEN, false);
                    SetOutput((int)eIO_O.TOWER_LAMP_YELLOW, true);
                    break;

                case _eLightColor.BLACK:
                    SetOutput((int)eIO_O.TOWER_LAMP_RED, false);
                    SetOutput((int)eIO_O.TOWER_LAMP_GREEN, false);
                    SetOutput((int)eIO_O.TOWER_LAMP_YELLOW, false);
                    break;
            }
        }

        /// <summary>
        /// 스위치 램프를 제어
        /// </summary>
        /// <param name="eFlag"></param>
        public void SetSwitchLamp(eMachineState eFlag)
        {
            //switch (eFlag)
            //{
            //    case eMachineState.INIT:
            //        SetOutput((int)eIO_O.START_SW_LAMP, false);
            //        SetOutput((int)eIO_O.STOP_SW_LAMP, false);
            //        SetOutput((int)eIO_O.RESET_SW_LAMP, false);
            //        break;

            //    case eMachineState.READY:
            //        SetOutput((int)eIO_O.START_SW_LAMP, false);
            //        SetOutput((int)eIO_O.STOP_SW_LAMP, true);
            //        SetOutput((int)eIO_O.RESET_SW_LAMP, false);
            //        break;

            //    case eMachineState.RUN:
            //        SetOutput((int)eIO_O.START_SW_LAMP, true);
            //        SetOutput((int)eIO_O.STOP_SW_LAMP, false);
            //        SetOutput((int)eIO_O.RESET_SW_LAMP, false);
            //        break;

            //    case eMachineState.MANUALRUN:
            //        SetOutput((int)eIO_O.START_SW_LAMP, true);
            //        SetOutput((int)eIO_O.STOP_SW_LAMP, false);
            //        SetOutput((int)eIO_O.RESET_SW_LAMP, false);
            //        break;

            //    case eMachineState.ERROR:
            //        SetOutput((int)eIO_O.START_SW_LAMP, false);
            //        SetOutput((int)eIO_O.STOP_SW_LAMP, false);
            //        SetOutput((int)eIO_O.RESET_SW_LAMP, true);
            //        break;
            //}
        }

        /// <summary>
        /// Buzzer Off 용 타이머
        /// </summary>
        private DispatcherTimer cBuzzerTimer = null;

        /// <summary>
        /// Buzzer 시간 Count
        /// </summary>
        private int iBuzzerCount = 0;

        /// <summary>
        /// buzzer on off
        /// </summary>
        /// <param name="bOnOff"></param>
        public void SetBuzzer(bool bOnOff)
        {
            if (bOnOff == true)
            {
                if (ml.cOptionData.bBuzzerUse == false) return;
                SetOutput((int)eIO_O.TOWER_LAMP_BUZZER, true);
                if (ml.cOptionData.bAlarmTimeUse == true)
                {
                    if (cBuzzerTimer == null)
                    {
                        cBuzzerTimer = new DispatcherTimer();
                        cBuzzerTimer.Interval = TimeSpan.FromMilliseconds(1000);   // 시간 간격 설정
                        cBuzzerTimer.Tick += new EventHandler(BuzzerTimer_Tick);    // 이벤트 추가
                    }
                    cBuzzerTimer.Start(); // 타이머 시작
                }
            }
            else
            {
                SetOutput((int)eIO_O.TOWER_LAMP_BUZZER, false);
            }
        }

        /// <summary>
        /// 데이터 갱신용 반복 타이머
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BuzzerTimer_Tick(object sender, EventArgs e)
        {
            iBuzzerCount++;
            if (ml.cOptionData.iAlarmTime <= iBuzzerCount)
            {
                SetOutput((int)eIO_O.TOWER_LAMP_BUZZER, false);
                iBuzzerCount = 0;
                cBuzzerTimer.Stop();
            }
        }

        /// <summary>
        /// 장비 Run 시 IO 상태
        /// </summary>
        public void SetSystemStartIO()
        {
            // Buzzer Off
            SetBuzzer(false);
        }

        /// <summary>
        /// 장비 Stop 시 IO 상태
        /// </summary>
        public void SetSystemStopIO()
        {
        }

        #region 시뮬레이션 관련

        private static readonly object readLock = new object();

        /// <summary>
        /// 실린더 딜레이 클래스 생성 (시뮬레이션)
        /// </summary>
        private CCylinderDelay cCylinderDelay = null;

        /// <summary>
        /// 실린더 도착 딜레이 리스트 (시뮬레이션)
        /// </summary>
        public List<CCylinderDelay> cCylinderDelayList = new List<CCylinderDelay>();

        /// <summary>
        /// 실린더 딜레이 리스트 Init
        /// </summary>
        public void CylinderListInit()
        {
            foreach (IOActuatorUI iOActuatorUI in ml.cDataEditUIManager.ListIOActuatorUI)
            {
                cCylinderDelay = cCylinderDelayList.Find(x => x.iInputNo == iOActuatorUI._iBWDInput);
                if (cCylinderDelay == null)
                {
                    cCylinderDelay = new CCylinderDelay();
                    cCylinderDelay.iInputNo = iOActuatorUI._iBWDInput;
                    cCylinderDelay.cCylinderDelay = new Stopwatch();
                    cCylinderDelayList.Add(cCylinderDelay);
                }

                cCylinderDelay = cCylinderDelayList.Find(x => x.iInputNo == iOActuatorUI._iFWDInput);
                if (cCylinderDelay == null)
                {
                    cCylinderDelay = new CCylinderDelay();
                    cCylinderDelay.iInputNo = iOActuatorUI._iFWDInput;
                    cCylinderDelay.cCylinderDelay = new Stopwatch();
                    cCylinderDelayList.Add(cCylinderDelay);
                }
            }
        }

        /// <summary>
        /// 시뮬레이션 모드일 때, 실린더 In Put Check
        /// 실린더 이동 Delay Check
        /// </summary>
        /// <param name="iInputNo"></param>
        /// <param name="bDone"></param>
        /// <returns></returns>
        public bool SimCylinderInputCheck(int iInputNo, ref bool bDone)
        {
            lock (readLock)
            {
                // 해당 I/O가 실린더 I/O 인지 찾는다.
                IOActuatorUI cCylinderData = ml.cDataEditUIManager.ListIOActuatorUI.Find(x => x._iBWDInput == iInputNo ||
                                                                                              x._iFWDInput == iInputNo);

                // 실린더가 I/O가 Null이면 리턴 시킨다.
                if (cCylinderData == null) return false;

                // 실린더 I/O를 찾았으면, 실린더 도착 딜레이를 사용할 클래스 리스트를 찾는다.
                cCylinderDelay = cCylinderDelayList.Find(x => x.iInputNo == iInputNo);

                // 실린더가 I/O가 Null이면 리턴 시킨다.
                if (cCylinderDelay == null) return false;

                int iCylinderIOStep = 0;
                if (cCylinderData._iBWDInput != -1 && cCylinderData._iFWDInput != -1) iCylinderIOStep = 1;
                else if (cCylinderData._iBWDInput == -1 && cCylinderData._iFWDInput != -1) iCylinderIOStep = 2;
                else if (cCylinderData._iBWDInput != -1 && cCylinderData._iFWDInput == -1) iCylinderIOStep = 3;

                switch (iCylinderIOStep)
                {
                    case 1:

                        #region Fwd BWd 전부 주소가 있을 때

                        // 해당 실린더 I/O가 BWD인지 확인
                        if (cCylinderData._iBWDInput == iInputNo)
                        {
                            if (cCylinderData.bBwdState == true)
                                bDone = true;
                            else
                            {
                                // Output 신호를 확인 후 BWD Input을 신호를 결정한다.
                                if (GetOutput(cCylinderData._iBWDOutput) == true)
                                {
                                    if (cCylinderDelay.cCylinderDelay.IsRunning == false) cCylinderDelay.cCylinderDelay.Restart();
                                    if (cCylinderDelay.cCylinderDelay.ElapsedMilliseconds > Define.iSimCylDelayTIme)
                                    {
                                        cCylinderDelay.cCylinderDelay.Stop();
                                        cCylinderData.bBwdState = true;
                                        cCylinderData.bFwdState = false;
                                    }
                                }
                                else
                                {
                                    // BWD 출력신호가 '-1'면, FWD 출력이 Off일 때 BWD 플래그를 True 시킨다.
                                    if (cCylinderData._iBWDOutput == -1)
                                    {
                                        if (GetOutput(cCylinderData._iFWDOutput) == false)
                                        {
                                            if (cCylinderDelay.cCylinderDelay.IsRunning == false) cCylinderDelay.cCylinderDelay.Restart();
                                            if (cCylinderDelay.cCylinderDelay.ElapsedMilliseconds > Define.iSimCylDelayTIme)
                                            {
                                                cCylinderDelay.cCylinderDelay.Stop();
                                                cCylinderData.bBwdState = true;
                                                cCylinderData.bFwdState = false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        cCylinderData.bBwdState = false;
                                        cCylinderData.bFwdState = true;
                                    }
                                }
                            }
                        }
                        // 해당 실린더 I/O가 FWD인지 확인
                        else if (cCylinderData._iFWDInput == iInputNo)
                        {
                            if (cCylinderData.bFwdState == true)
                                bDone = true;
                            else
                            {
                                // Output 신호를 확인 후 FWD Input을 신호를 결정한다.
                                if (GetOutput(cCylinderData._iFWDOutput) == true)
                                {
                                    if (cCylinderDelay.cCylinderDelay.IsRunning == false) cCylinderDelay.cCylinderDelay.Restart();
                                    if (cCylinderDelay.cCylinderDelay.ElapsedMilliseconds > Define.iSimCylDelayTIme)
                                    {
                                        cCylinderDelay.cCylinderDelay.Stop();
                                        cCylinderData.bBwdState = false;
                                        cCylinderData.bFwdState = true;
                                    }
                                }
                                else
                                {
                                    // FWD 출력신호가 '-1'면, BWD 출력이 Off일 때 FWD 플래그를 True 시킨다.
                                    if (cCylinderData._iFWDOutput == -1)
                                    {
                                        if (GetOutput(cCylinderData._iBWDOutput) == false)
                                        {
                                            if (cCylinderDelay.cCylinderDelay.IsRunning == false) cCylinderDelay.cCylinderDelay.Restart();
                                            if (cCylinderDelay.cCylinderDelay.ElapsedMilliseconds > Define.iSimCylDelayTIme)
                                            {
                                                cCylinderDelay.cCylinderDelay.Stop();
                                                cCylinderData.bBwdState = false;
                                                cCylinderData.bFwdState = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        cCylinderData.bBwdState = true;
                                        cCylinderData.bFwdState = false;
                                    }
                                }
                            }
                        }

                        #endregion Fwd BWd 전부 주소가 있을 때

                        break;

                    case 2:

                        #region Bwd 주소가 -1 있을 때

                        // 해당 실린더 I/O가 FWD인지 확인
                        if (cCylinderData._iFWDInput == iInputNo)
                        {
                            // Output 신호를 확인 후 FWD Input을 신호를 결정한다.
                            if (GetOutput(cCylinderData._iFWDOutput) == true)
                            {
                                if (cCylinderData.bFwdState == true)
                                    bDone = true;
                                else
                                {
                                    if (cCylinderDelay.cCylinderDelay.IsRunning == false) cCylinderDelay.cCylinderDelay.Restart();
                                    if (cCylinderDelay.cCylinderDelay.ElapsedMilliseconds > Define.iSimCylDelayTIme)
                                    {
                                        cCylinderDelay.cCylinderDelay.Stop();
                                        cCylinderData.bBwdState = false;
                                        cCylinderData.bFwdState = true;
                                    }
                                }
                            }
                            else
                            {
                                if (cCylinderData.bBwdState == true)
                                    bDone = true;
                                else
                                {
                                    if (cCylinderDelay.cCylinderDelay.IsRunning == false) cCylinderDelay.cCylinderDelay.Restart();
                                    if (cCylinderDelay.cCylinderDelay.ElapsedMilliseconds > Define.iSimCylDelayTIme)
                                    {
                                        cCylinderDelay.cCylinderDelay.Stop();
                                        cCylinderData.bBwdState = true;
                                        cCylinderData.bFwdState = false;
                                    }
                                }
                            }
                        }

                        #endregion Bwd 주소가 -1 있을 때

                        break;

                    case 3:

                        #region Fwd 주소가 -1 있을 때

                        if (cCylinderData._iBWDInput == iInputNo)
                        {
                            // Output 신호를 확인 후 BWD Input을 신호를 결정한다.
                            if (GetOutput(cCylinderData._iBWDOutput) == true)
                            {
                                if (cCylinderData.bBwdState == true)
                                    bDone = true;
                                else
                                {
                                    if (cCylinderDelay.cCylinderDelay.IsRunning == false) cCylinderDelay.cCylinderDelay.Restart();
                                    if (cCylinderDelay.cCylinderDelay.ElapsedMilliseconds > Define.iSimCylDelayTIme)
                                    {
                                        cCylinderDelay.cCylinderDelay.Stop();
                                        cCylinderData.bBwdState = true;
                                        cCylinderData.bFwdState = false;
                                    }
                                }
                            }
                            else
                            {
                                if (cCylinderData.bFwdState == true)
                                    bDone = true;
                                else
                                {
                                    if (cCylinderDelay.cCylinderDelay.IsRunning == false) cCylinderDelay.cCylinderDelay.Restart();
                                    if (cCylinderDelay.cCylinderDelay.ElapsedMilliseconds > Define.iSimCylDelayTIme)
                                    {
                                        cCylinderDelay.cCylinderDelay.Stop();
                                        cCylinderData.bBwdState = false;
                                        cCylinderData.bFwdState = true;
                                    }
                                }
                            }
                        }

                        #endregion Fwd 주소가 -1 있을 때

                        break;
                }

                return true;
            }
        }

        #endregion 시뮬레이션 관련
    }

    /// <summary>
    /// IO 외부 주소 변경을 위한 데이터 저장 클래스
    /// </summary>
    public class CIOData
    {
        public int iOriginIONum = -1;
        public int iCurrentIONum = -1;
    }

    /// <summary>
    /// 실린더 도착 딜레이 관련 클래스 (시뮬레이션)
    /// </summary>
    public class CCylinderDelay
    {
        /// <summary>
        /// 실린더 딜레이 스톱워치 (시뮬레이션)
        /// </summary>
        public Stopwatch cCylinderDelay;

        /// <summary>
        /// 실린더 Input No (시뮬레이션)
        /// </summary>
        public int iInputNo = -1;
    }
}