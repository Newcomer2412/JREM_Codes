using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Class 정의

        /// <summary>
        /// 모든 데이터 정의
        /// </summary>
        private CMainLib ml = null;

        /// <summary>
        /// System Array Data
        /// </summary>
        private CSystemParameterArray cSysArray = null;

        /// <summary>
        /// JREM Main 화면
        /// </summary>
        public MainUI cMainUI = null;

        /// <summary>
        /// Model을 선택하는 UI
        /// </summary>
        private ModelSelectUI cModelSelectUI = null;

        /// <summary>
        /// Data Edit UI
        /// </summary>
        private DataEditUI cDataEditUI = null;

        /// <summary>
        /// Option Edit UI
        /// </summary>
        private OptionUI cOptionUI = null;

        /// <summary>
        /// IO 설정 및 모니터링 UI
        /// </summary>
        private IOMonitorUI cIOMonitorUI = null;

        /// <summary>
        /// Alarm 표시 UI
        /// </summary>
        private AlarmScreenUI cAlarmScreenUI = null;

        /// <summary>
        /// Log를 보여주는 UI
        /// </summary>
        private LogViewUI cLogViewUI = null;

        /// <summary>
        /// 생산량 그래프 UI
        /// </summary>
        private GraphViewUI cGraphViewUI = null;

        /// <summary>
        /// Vision 그래프 UI
        /// </summary>
        private VisionGraphViewUI visionGraphViewUI = null;

        /// <summary>
        /// 에러 그래프 UI
        /// </summary>
        private ErrorViewUI cErrorViewUI = null;

        /// <summary>
        /// Axis Parameter List UI
        /// </summary>
        private AxisParameterListUI cAxisInfoListUI = null;

        /// <summary>
        /// Axis 매뉴얼 동작
        /// </summary>
        private ManualMotionUI cManualMotionUI = null;

        /// <summary>
        /// 시퀀스 No 모니터 UI
        /// </summary>
        private SequenceNoListViewUI sequenceNoListViewUI = null;

        /// <summary>
        /// 초기화용 타이머
        /// </summary>
        private DispatcherTimer cInitTimer = new DispatcherTimer();

        /// <summary>
        /// Data 갱신용 타이머
        /// </summary>
        private DispatcherTimer cDataRefreshTimer = new DispatcherTimer();

        #endregion Class 정의

        public MainWindow()
        {
            InitializeComponent();

            // Debug를 위한 메인 스레드 이름 정의
            Thread.CurrentThread.Name = "JremMachinThread";
            // Exception 발생 시 가능하면 프로그램을 죽이지 않는다.
            App app = App.Current as App;
            app.DoHandle = true;

            // 각 UI 클래스 생성 및 초기화
            UICreateAndInit();
            // 통합 데이터 클래스 생성
            ml = CMainLib.Ins;
            // MainUI 초기화
            MainUI.Ins.Init();
            // Vision 초기화
            if (ml.VisionInit() == false) Application.Current.Shutdown();

            if (Define.SIMULATION == false)
            {
                // AIM 피더 이니셜
                ml.AIM_Feeder.Init();
                // AIM 피더 Connect
                ml.AIM_Feeder.Connect(0, eLogType.AIM_Feeder, CMainLib.Ins.cOptionData.strAIM_FeederIP, 1470);
                // LED 백라이트 On
                ml.AIM_Feeder.LED_On();
            }

            CheckNowUseUI.CurrentUIIndex = _UIIndex.MainUI;
        }

        #region 윈도우 이벤트 정의

        /// <summary>
        /// Main 윈도우 로딩 완료 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TopWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 윈도우 로딩 완료 이전에 파라미터 수정하면 에러나서 파라미터만 로딩 완료 후 따로 수정
            for (int i = 0; i < Define.MAX_CAMERA; i++)
            {
                ml.cVisionToolBlockUI[i].CogDisplayParamSetting();
            }

            // Program Version 정보를 읽음
            ProgramVersion.LoadVersionDocument();
            TbVersion.Text = ProgramVersion.Version;

            if (Define.SIMULATION == true)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "디버그 모드로 시작 되었습니다.");
                ml.cVar.iUserLevel = (int)eUserLevel.ADMINISTRATOR;
            }
            else
            {
                ml.cVar.iUserLevel = (int)eUserLevel.OPERATOR;
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Machine System is Start.");
            }

            ml.cVar.iUserLevel = (int)eUserLevel.ADMINISTRATOR;
            // 모델명 설정
            ModelNameChange();
            // System 모델 데이터 갱신
            cSysArray = ml.cSysParamCollData.GetSysArray();
            // Main의 UI가 보이도록 갱신
            MainPanel.Content = MainUI.Ins;

            // 레벨 표시 갱신
            if (ml.cVar.iUserLevel == (int)eUserLevel.OPERATOR) TbUserLevel.Text = "Operator";
            else if (ml.cVar.iUserLevel == (int)eUserLevel.ENGINEER) TbUserLevel.Text = "Engineer";
            else if (ml.cVar.iUserLevel == (int)eUserLevel.ADMINISTRATOR) TbUserLevel.Text = "Administrator";
            else TbUserLevel.Text = "None";

            // 장비 초기화 타이머
            cInitTimer.Interval = TimeSpan.FromMilliseconds(100);     // 시간 간격 설정
            cInitTimer.Tick += new EventHandler(InitTimer_Tick);      // 이벤트 추가
            cInitTimer.Start();                                       // 타이머 시작
        }

        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
        private static extern uint TimeEndPeriod(uint uMilliseconds);

        /// <summary>
        /// Main 윈도우 소멸 이벤트
        /// </summary>
        private void TopWindow_Closed(object sender, EventArgs e)
        {
            if (cDataRefreshTimer.IsEnabled == true) cDataRefreshTimer.Stop();
            TimeEndPeriod(1);
            ml.Free();
        }

        /// <summary>
        /// 초기화용 타이머
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InitTimer_Tick(object sender, EventArgs e)
        {
            cInitTimer.Stop();
            Dispatcher.Invoke(DispatcherPriority.Background, (Action)delegate ()
            {
                //Program 시작시 초기화 다이얼로그로 홈 동작
                if (ml.cOptionData.bOriginInitialize == true)
                {
                    // Initial 시작!
                    using (InitializeBoxUI cInitializeBoxUI = new InitializeBoxUI())
                    {
                        cInitializeBoxUI.ShowDialog();
                    }
                }

                // Data 갱신 타이머
                cDataRefreshTimer.Interval = TimeSpan.FromMilliseconds(100);            // 시간 간격 설정
                cDataRefreshTimer.Tick += new EventHandler(DataRefreshTimer_Tick);      // 이벤트 추가
                cDataRefreshTimer.Start();                                              // 타이머 시작
            });
        }

        /// <summary>
        /// 데이터 갱신용 반복 타이머
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataRefreshTimer_Tick(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate ()
            {
                // 장비 상태 Title 변경, Model 제목 표시
                if (TbTitleStatus.Text != ml.McState.ToString())
                {
                    if (ml.McState == eMachineState.INIT || ml.McState == eMachineState.READY)
                    {
                        TbTitleStatus.Text = ml.McState.ToString();
                        BdTitle.Background = new SolidColorBrush(Color.FromArgb(255, 40, 40, 40));
                    }
                    else if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN)
                    {
                        TbTitleStatus.Text = ml.McState.ToString();
                        BdTitle.Background = Brushes.Blue;
                    }
                    else if (ml.McState == eMachineState.ERROR)
                    {
                        if (ml.Seq.GetAllStop() == true)
                        {
                            TbTitleStatus.Text = ml.McState.ToString();
                            BdTitle.Background = Brushes.Red;
                        }
                    }
                }

                //에러 상태 체크. 에러 상태이면 에러 UI출력
                if (ml.McState == eMachineState.ERROR &&
                    ml.Seq.GetAllStop() == true)
                {
                    if (MainPanel.Content != cAlarmScreenUI)
                    {
                        CheckNowUseUI.CurrentUIIndex = _UIIndex.AlarmScreenUI;
                        MainPanel.Content = cAlarmScreenUI;
                    }
                }

                // MainUI 데이터 반복 갱신
                MainUI.Ins.DataTimer_Tick();
                // 화면 우측 상단 상태 표시 갱신
                UIDataUpdate();
                // 화면 좌측 상단 시간 정보 갱신
                UpdateTime();
            });
        }

        /// <summary>
        /// 기존 데이터를 저장 비교
        /// </summary>
        private bool[] bOldUIData = new bool[6] { false, false, false, false, false, false };

        /// <summary>
        /// UI 데이터 변경이 있을 경우 반영
        /// </summary>
        private void UIDataUpdate()
        {
            // Main MC Power 갱신
            if (ml.cVar.bMCPowerFlag != bOldUIData[0])
            {
                if (ml.cVar.bMCPowerFlag == true) Border_MainPowerFlag.Background = Brushes.DodgerBlue;
                else Border_MainPowerFlag.Background = Brushes.Gray;
                bOldUIData[0] = ml.cVar.bMCPowerFlag;
            }

            // Main Air 갱신
            if (ml.cVar.bMainAirFlag != bOldUIData[1])
            {
                if (ml.cVar.bMainAirFlag == true) Border_MainAirFlag.Background = Brushes.DodgerBlue;
                else Border_MainAirFlag.Background = Brushes.Gray;
                bOldUIData[1] = ml.cVar.bMainAirFlag;
            }

            // 도어 체크 사용 유무
            if (ml.cOptionData.bDoorLockUse != bOldUIData[2])
            {
                if (ml.cOptionData.bDoorLockUse == true) Border_DoorCheckFlag.Background = Brushes.DodgerBlue;
                else Border_DoorCheckFlag.Background = Brushes.Gray;
                bOldUIData[2] = ml.cOptionData.bDoorLockUse;
            }
        }

        #region Lot 시간 정보 표시

        private TimeSpan TSGapTime;
        private DateTime DTpreTime = DateTime.Now;

        private string strOldRunningTime = string.Empty;
        private string strOldStopTime = string.Empty;
        private string strOldErrorTime = string.Empty;

        /// <summary>
        /// 시간 정보 Update
        /// </summary>
        public void UpdateTime()
        {
            DateTime DTCurrentTime = DateTime.Now;
            TSGapTime = DTCurrentTime - DTpreTime;

            if (TSGapTime.Hours < 0)
            {
                DTpreTime = DTCurrentTime;
                return;
            }

            if (ml.McState == eMachineState.RUN)
            {
                ml.cSysOne.DTRunning_Time += TSGapTime;
            }
            else
            {
                if (ml.McState == eMachineState.ERROR)
                {
                    ml.cSysOne.DTError_Time += TSGapTime;
                }
                else
                {
                    ml.cSysOne.DTStop_Time += TSGapTime;
                }
            }

            DTpreTime = DTCurrentTime;

            string strTime = String.Format("{0}:{1:00}:{2:00}", (ml.cSysOne.DTRunning_Time.Day - 1) * 24 + ml.cSysOne.DTRunning_Time.Hour,
                                                                 ml.cSysOne.DTRunning_Time.Minute,
                                                                 ml.cSysOne.DTRunning_Time.Second);
            if (strOldRunningTime != strTime)
            {
                TbRunnigTime.Text = strTime;
                strOldRunningTime = strTime;
            }

            strTime = String.Format("{0}:{1:00}:{2:00}", (ml.cSysOne.DTStop_Time.Day - 1) * 24 + ml.cSysOne.DTStop_Time.Hour,
                                                          ml.cSysOne.DTStop_Time.Minute,
                                                          ml.cSysOne.DTStop_Time.Second);
            if (strOldStopTime != strTime)
            {
                TbStopTime.Text = strTime;
                strOldStopTime = strTime;
            }

            strTime = String.Format("{0}:{1:00}:{2:00}", (ml.cSysOne.DTError_Time.Day - 1) * 24 + ml.cSysOne.DTError_Time.Hour,
                                                          ml.cSysOne.DTError_Time.Minute,
                                                          ml.cSysOne.DTError_Time.Second);
            if (strOldErrorTime != strTime)
            {
                TbErrorTime.Text = strTime;
                strOldErrorTime = strTime;
            }
        }

        #endregion Lot 시간 정보 표시

        /// <summary>
        /// Data Select 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataSelect_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN) return;
            if (ml.GetUserLevelCheck((int)eUserLevel.ENGINEER) == false) return;
            if (CheckNowUseUI.CurrentUIIndex != _UIIndex.DataSelectUI)
            {
                CheckNowUseUI.CurrentUIIndex = _UIIndex.DataSelectUI;
                MainPanel.Content = cModelSelectUI;
            }
            PopupManuCtrl(3);
        }

        /// <summary>
        /// Data Edit 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataEdit_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN) return;
            if (ml.GetUserLevelCheck((int)eUserLevel.ENGINEER) == false) return;
            if (CheckNowUseUI.CurrentUIIndex != _UIIndex.DataEditUI)
            {
                CheckNowUseUI.CurrentUIIndex = _UIIndex.DataEditUI;
                MainPanel.Content = cDataEditUI;
            }
            PopupManuCtrl(3);
        }

        /// <summary>
        /// Option 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Option_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN) return;
            if (ml.GetUserLevelCheck((int)eUserLevel.ENGINEER) == false) return;
            if (CheckNowUseUI.CurrentUIIndex != _UIIndex.UserSetUI)
            {
                CheckNowUseUI.CurrentUIIndex = _UIIndex.UserSetUI;
                MainPanel.Content = cOptionUI;
            }
            PopupManuCtrl(3);
        }

        /// <summary>
        /// I/O Monitor 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IOMonitor_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN) return;
            if (ml.GetUserLevelCheck((int)eUserLevel.ENGINEER) == false) return;
            if (CheckNowUseUI.CurrentUIIndex != _UIIndex.IOMonitorUI)
            {
                CheckNowUseUI.CurrentUIIndex = _UIIndex.IOMonitorUI;
                MainPanel.Content = cIOMonitorUI;
            }
            PopupManuCtrl(3);
        }

        /// <summary>
        /// Error List UI 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErrorListUI_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN) return;
            if (ml.GetUserLevelCheck((int)eUserLevel.ENGINEER) == false) return;
            PopupManuCtrl(3);
            using (AlarmDataUI jamInputDataUI = new AlarmDataUI())
            {
                jamInputDataUI.ShowDialog();
            }
        }

        /// <summary>
        /// Axis Parameter 설정창
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btViewMotionParam_Click(object sender, RoutedEventArgs e)
        {
            if (ml.GetUserLevelCheck((int)eUserLevel.ADMINISTRATOR) == false) return;

            if (cAxisInfoListUI != null)
            {
                cAxisInfoListUI.Close();
                cAxisInfoListUI = null;
            }
            if (cAxisInfoListUI == null)
            {
                cAxisInfoListUI = new AxisParameterListUI();
                cAxisInfoListUI.Init();
            }
            cAxisInfoListUI.Show();
            PopupManuCtrl(3);
        }

        /// <summary>
        /// 축 수동 조작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btManualAxisCtl_Click(object sender, RoutedEventArgs e)
        {
            if (ml.GetUserLevelCheck((int)eUserLevel.ADMINISTRATOR) == false) return;

            if (cManualMotionUI != null)
            {
                if (cManualMotionUI.bOpened == true) return;
                cManualMotionUI.Close();
                cManualMotionUI = null;
            }
            if (cManualMotionUI == null)
            {
                cManualMotionUI = new ManualMotionUI();
                cManualMotionUI.Init();
            }
            cManualMotionUI.Show();
            PopupManuCtrl(3);
        }

        /// <summary>
        /// All Homming 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void All_Homming_Button_Click(object sender, RoutedEventArgs e)
        {
            PopupManuCtrl(3);
            if (ml.cVar.bInitializeComplete == true)
            {
                if (ml.cOptionData.iLanguageMode == (int)eLanguage.KOREAN)
                {
                    if (CCommon.ShowMessage(1, "장비 초기화를 다시 하시겠습니까 ?") != (int)eMBoxRtn.A_OK) return;
                }
                else if (ml.cOptionData.iLanguageMode == (int)eLanguage.ENGLISH)
                {
                    if (CCommon.ShowMessage(1, "Do you want machine Initialize again ?") != (int)eMBoxRtn.A_OK) return;
                }
            }
            // MainScreen으로 변경
            btMainUI.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            // Log 기록!
            NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Machine ReInitialize..!");
            // System Utility IO Check Stop~!
            ml.cVar.bSystemUtilityIOCheck = false;
            // Initial 시작!
            using (InitializeBoxUI cInitializeBoxUI = new InitializeBoxUI())
            {
                cInitializeBoxUI.ShowDialog();
            }
        }

        /// <summary>
        /// 시퀀스 No를 실시간으로 볼 수 있는 창을 연다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ml.GetUserLevelCheck((int)eUserLevel.ADMINISTRATOR) == false) return;
            if (sequenceNoListViewUI != null)
            {
                if (sequenceNoListViewUI.bOpened == true) return;
                sequenceNoListViewUI.Close();
                sequenceNoListViewUI = null;
            }
            if (sequenceNoListViewUI == null)
            {
                sequenceNoListViewUI = new SequenceNoListViewUI();
                sequenceNoListViewUI.Init();
            }
            sequenceNoListViewUI.Show();
        }

        #region Info View UI 버튼 관련

        /// <summary>
        /// Program Log View를 연다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgramLogView_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CheckNowUseUI.CurrentUIIndex == _UIIndex.AlarmScreenUI) return;

            cLogViewUI.Init(eLogType.PROGRAM);
            CheckNowUseUI.CurrentUIIndex = _UIIndex.LogViewUI;
            MainPanel.Content = cLogViewUI;
            PopupManuCtrl(3);
        }

        /// <summary>
        /// Seq Main Log View를 연다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SeqMainLogView_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CheckNowUseUI.CurrentUIIndex == _UIIndex.AlarmScreenUI) return;

            cLogViewUI.Init(eLogType.SEQ_MAIN);
            CheckNowUseUI.CurrentUIIndex = _UIIndex.LogViewUI;
            MainPanel.Content = cLogViewUI;
            PopupManuCtrl(3);
        }

        /// <summary>
        /// Front MPC Log View를 연다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Front_MPC_View_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CheckNowUseUI.CurrentUIIndex == _UIIndex.AlarmScreenUI) return;

            cLogViewUI.Init(eLogType.Seq00_Front_MPC);
            CheckNowUseUI.CurrentUIIndex = _UIIndex.LogViewUI;
            MainPanel.Content = cLogViewUI;
            PopupManuCtrl(3);
        }

        /// <summary>
        /// Rear MPC Left Log View를 연다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RearMPC_Left_View_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CheckNowUseUI.CurrentUIIndex == _UIIndex.AlarmScreenUI) return;

            cLogViewUI.Init(eLogType.Seq01_Rear_MPC_Left);
            CheckNowUseUI.CurrentUIIndex = _UIIndex.LogViewUI;
            MainPanel.Content = cLogViewUI;
            PopupManuCtrl(3);
        }

        /// <summary>
        /// Rear MPC Right Log View를 연다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RearMPC_Right_View_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CheckNowUseUI.CurrentUIIndex == _UIIndex.AlarmScreenUI) return;

            cLogViewUI.Init(eLogType.Seq02_Rear_MPC_Right);
            CheckNowUseUI.CurrentUIIndex = _UIIndex.LogViewUI;
            MainPanel.Content = cLogViewUI;
            PopupManuCtrl(3);
        }

        /// <summary>
        /// Holder Supply & Recovery Log View를 연다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HolderSnR_View_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CheckNowUseUI.CurrentUIIndex == _UIIndex.AlarmScreenUI) return;

            cLogViewUI.Init(eLogType.Seq03_HolderS_R);
            CheckNowUseUI.CurrentUIIndex = _UIIndex.LogViewUI;
            MainPanel.Content = cLogViewUI;
            PopupManuCtrl(3);
        }

        /// <summary>
        /// Hopper Log View를 연다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hopper_View_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CheckNowUseUI.CurrentUIIndex == _UIIndex.AlarmScreenUI) return;

            cLogViewUI.Init(eLogType.Seq04_Hopper);
            CheckNowUseUI.CurrentUIIndex = _UIIndex.LogViewUI;
            MainPanel.Content = cLogViewUI;
            PopupManuCtrl(3);
        }

        /// <summary>
        /// Pipe PnP Log View를 연다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PipePnP_View_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CheckNowUseUI.CurrentUIIndex == _UIIndex.AlarmScreenUI) return;

            cLogViewUI.Init(eLogType.Seq05_PipePnP);
            CheckNowUseUI.CurrentUIIndex = _UIIndex.LogViewUI;
            MainPanel.Content = cLogViewUI;
            PopupManuCtrl(3);
        }

        /// <summary>
        /// Needle PnP Log View를 연다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NeedlePnP_View_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CheckNowUseUI.CurrentUIIndex == _UIIndex.AlarmScreenUI) return;

            cLogViewUI.Init(eLogType.Seq06_NeedlePnP);
            CheckNowUseUI.CurrentUIIndex = _UIIndex.LogViewUI;
            MainPanel.Content = cLogViewUI;
            PopupManuCtrl(3);
        }

        /// <summary>
        /// Transfer Log View를 연다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Transfer_View_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CheckNowUseUI.CurrentUIIndex == _UIIndex.AlarmScreenUI) return;

            cLogViewUI.Init(eLogType.Seq07_Transfer);
            CheckNowUseUI.CurrentUIIndex = _UIIndex.LogViewUI;
            MainPanel.Content = cLogViewUI;
            PopupManuCtrl(3);
        }

        /// <summary>
        /// Pipe Mount Log View를 연다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PipeMount_View_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CheckNowUseUI.CurrentUIIndex == _UIIndex.AlarmScreenUI) return;

            cLogViewUI.Init(eLogType.Seq08_PipeMount);
            CheckNowUseUI.CurrentUIIndex = _UIIndex.LogViewUI;
            MainPanel.Content = cLogViewUI;
            PopupManuCtrl(3);
        }

        /// <summary>
        /// Needle Mount Log View를 연다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NeedleMount_View_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CheckNowUseUI.CurrentUIIndex == _UIIndex.AlarmScreenUI) return;

            cLogViewUI.Init(eLogType.Seq09_NeedleMount);
            CheckNowUseUI.CurrentUIIndex = _UIIndex.LogViewUI;
            MainPanel.Content = cLogViewUI;
            PopupManuCtrl(3);
        }

        /// <summary>
        /// Dispensing Log View를 연다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispMount_View_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CheckNowUseUI.CurrentUIIndex == _UIIndex.AlarmScreenUI) return;

            cLogViewUI.Init(eLogType.Seq10_Dispensing);
            CheckNowUseUI.CurrentUIIndex = _UIIndex.LogViewUI;
            MainPanel.Content = cLogViewUI;
            PopupManuCtrl(3);
        }

        /// <summary>
        /// 양산 결과 그래프 View를 연다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Graph_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN) return;
            if (CheckNowUseUI.CurrentUIIndex == _UIIndex.AlarmScreenUI) return;

            CheckNowUseUI.CurrentUIIndex = _UIIndex.GraphViewUI;
            MainPanel.Content = cGraphViewUI;
            PopupManuCtrl(3);
        }

        /// <summary>
        /// 양산 결과 그래프 View를 연다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VisionGraph_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN) return;
            if (CheckNowUseUI.CurrentUIIndex == _UIIndex.AlarmScreenUI) return;

            CheckNowUseUI.CurrentUIIndex = _UIIndex.VisionGraphViewUI;
            MainPanel.Content = visionGraphViewUI;
            PopupManuCtrl(3);
        }

        /// <summary>
        /// 에러 결과 그래프 View를 연다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErrorView_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN) return;
            if (CheckNowUseUI.CurrentUIIndex == _UIIndex.AlarmScreenUI) return;

            CheckNowUseUI.CurrentUIIndex = _UIIndex.ErrorViewUI;
            MainPanel.Content = cErrorViewUI;
            PopupManuCtrl(3);
        }

        /// <summary>
        /// 택 타임 창
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaktTime_Button_Click(object sender, RoutedEventArgs e)
        {
            using (TaktTimeUI taktTimeUI = new TaktTimeUI())
            {
                taktTimeUI.Show();
            }
            PopupManuCtrl(3);
        }

        #endregion Info View UI 버튼 관련

        /// <summary>
        /// Main 화면이 보이도록 Main UI 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainUI_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN) return;
            if (CheckNowUseUI.CurrentUIIndex == _UIIndex.AlarmScreenUI) return;

            if (CheckNowUseUI.CurrentUIIndex != _UIIndex.MainUI)
            {
                CheckNowUseUI.CurrentUIIndex = _UIIndex.MainUI;
                MainPanel.Content = MainUI.Ins;
            }
            SlideVisionUI.Ins.VisionMainView();
        }

        /// <summary>
        /// Start 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            // System 모델 데이터 갱신
            cSysArray = ml.cSysParamCollData.GetSysArray();
            // 팝업창 닫기
            PopupManuCtrl(3);
            ml.McStart();
        }

        /// <summary>
        /// Stop 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Stop_Button_Click(object sender, RoutedEventArgs e)
        {
            ml.McStop();
        }

        /// <summary>
        /// Reset 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            if (CheckNowUseUI.CurrentUIIndex == _UIIndex.AlarmScreenUI)
            {
                CheckNowUseUI.CurrentUIIndex = _UIIndex.MainUI;
                MainPanel.Content = MainUI.Ins;
            }
            ml.McReset();
        }

        #region UserLevel Password 관련

        /// <summary>
        /// UserLevel Popup Flag
        /// </summary>
        private bool bUserLevelShowPopup = false;

        /// <summary>
        /// 선택 된 User Level
        /// </summary>
        private int iChoiceUserLevel = 0;

        /// <summary>
        /// UserLeve 변경 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtUserLevel_Click(object sender, RoutedEventArgs e)
        {
            if (bUserLevelShowPopup == false)
            {
                PopupManuCtrl(0);
                SetUserLevelBackgroundBrush(ml.cVar.iUserLevel);
            }
            else
            {
                Popup_UserLevel.IsOpen = false;
            }
            bUserLevelShowPopup = !bUserLevelShowPopup;
        }

        /// <summary>
        /// UserLevel Toggle Button 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton cToggleButton = sender as ToggleButton;
            string strContent = cToggleButton.Content.ToString();

            if (strContent == "OPERATOR") SetUserLevelBackgroundBrush((int)eUserLevel.OPERATOR);
            else if (strContent == "ENGINEER") SetUserLevelBackgroundBrush((int)eUserLevel.ENGINEER);
            else if (strContent == "ADMINISTRATOR") SetUserLevelBackgroundBrush((int)eUserLevel.ADMINISTRATOR);
            PbPassword.Focus();
        }

        /// <summary>
        /// User Level 바탕색 변경
        /// </summary>
        /// <param name="iUserLevel"></param>
        private void SetUserLevelBackgroundBrush(int iUserLevel)
        {
            if (iUserLevel == (int)eUserLevel.OPERATOR)
            {
                SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.OrangeRed);
                TbOperator.Background = solidColorBrush;
                solidColorBrush = new SolidColorBrush(Colors.CornflowerBlue);
                TbEngineer.Background = solidColorBrush;
                TbAdministrator.Background = solidColorBrush;
                iChoiceUserLevel = (int)eUserLevel.OPERATOR;
            }
            else if (iUserLevel == (int)eUserLevel.ENGINEER)
            {
                SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.OrangeRed);
                TbEngineer.Background = solidColorBrush;
                solidColorBrush = new SolidColorBrush(Colors.CornflowerBlue);
                TbOperator.Background = solidColorBrush;
                TbAdministrator.Background = solidColorBrush;
                iChoiceUserLevel = (int)eUserLevel.ENGINEER;
            }
            else if (iUserLevel == (int)eUserLevel.ADMINISTRATOR)
            {
                SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.OrangeRed);
                TbAdministrator.Background = solidColorBrush;
                solidColorBrush = new SolidColorBrush(Colors.CornflowerBlue);
                TbOperator.Background = solidColorBrush;
                TbEngineer.Background = solidColorBrush;
                iChoiceUserLevel = (int)eUserLevel.ADMINISTRATOR;
            }
            iChoiceUserLevel = iUserLevel;
        }

        /// <summary>
        /// Password 변경 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Password_Change_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.cVar.iUserLevel >= iChoiceUserLevel)
            {
                if (PbPassword.Password != string.Empty)
                {
                    if (iChoiceUserLevel == (int)eUserLevel.ENGINEER)
                    {
                        ml.cSysOne.strEngineerPassword = PbPassword.Password;
                    }
                    else if (iChoiceUserLevel == (int)eUserLevel.ADMINISTRATOR)
                    {
                        ml.cSysOne.strAdministratorPassword = PbPassword.Password;
                    }
                    else
                    {
                        return;
                    }
                    CXMLProcess.WriteXml(CXMLProcess.SystemParameterSingleFilePath, ml.cSysOne);
                    if (ml.cOptionData.iLanguageMode == (int)eLanguage.KOREAN) CCommon.ShowMessageMini("변경이 완료되었습니다.");
                    else if (ml.cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) CCommon.ShowMessageMini("This change is complete.");
                }
                else
                {
                    if (ml.cOptionData.iLanguageMode == (int)eLanguage.KOREAN) CCommon.ShowMessageMini("변경할 패스워드를 입력하세요.");
                    else if (ml.cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) CCommon.ShowMessageMini("Please enter a password to change.");
                }
            }
            else
            {
                if (ml.cOptionData.iLanguageMode == (int)eLanguage.KOREAN) CCommon.ShowMessageMini("권한이 없습니다.");
                else if (ml.cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) CCommon.ShowMessageMini("You do not have permission.");
            }
        }

        /// <summary>
        /// Login 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            if (iChoiceUserLevel == (int)eUserLevel.OPERATOR)
            {
                if (PbPassword.Password == ml.cSysOne.strMasterPassword)
                {
                    ml.cVar.iUserLevel = (int)eUserLevel.ADMINISTRATOR;
                    TbUserLevel.Text = "Administrator";
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Level Change : Engineer");
                    Popup_UserLevel.IsOpen = false;
                    bUserLevelShowPopup = false;
                    return;
                }
                ml.cVar.iUserLevel = (int)eUserLevel.OPERATOR;
                TbUserLevel.Text = "Operator";
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Level Change : Operator");
                Popup_UserLevel.IsOpen = false;
                bUserLevelShowPopup = false;
                return;
            }
            if (PbPassword.Password != string.Empty)
            {
                if (iChoiceUserLevel == (int)eUserLevel.ENGINEER)
                {
                    if (PbPassword.Password == ml.cSysOne.strEngineerPassword)
                    {
                        ml.cVar.iUserLevel = (int)eUserLevel.ENGINEER;
                        TbUserLevel.Text = "Engineer";
                        NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Level Change : Engineer");
                        Popup_UserLevel.IsOpen = false;
                        bUserLevelShowPopup = false;
                        PbPassword.Password = string.Empty;
                        return;
                    }
                }
                else if (iChoiceUserLevel == (int)eUserLevel.ADMINISTRATOR)
                {
                    if (PbPassword.Password == ml.cSysOne.strAdministratorPassword ||
                        PbPassword.Password == "5961")
                    {
                        ml.cVar.iUserLevel = (int)eUserLevel.ADMINISTRATOR;
                        TbUserLevel.Text = "Administrator";
                        NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Level Change : Administrator");
                        Popup_UserLevel.IsOpen = false;
                        bUserLevelShowPopup = false;
                        PbPassword.Password = string.Empty;
                        return;
                    }
                }

                if (ml.cOptionData.iLanguageMode == (int)eLanguage.KOREAN) CCommon.ShowMessageMini("패스워드가 다릅니다.");
                else if (ml.cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) CCommon.ShowMessageMini("Password is different.");
            }
            else
            {
                if (ml.cOptionData.iLanguageMode == (int)eLanguage.KOREAN) CCommon.ShowMessageMini("패스워드를 입력하세요.");
                else if (ml.cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) CCommon.ShowMessageMini("Please enter a password.");
            }
            PbPassword.Password = string.Empty;
        }

        /// <summary>
        /// PasswordBox 키 입력 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PbPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtLogin.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
        }

        #endregion UserLevel Password 관련

        /// <summary>
        /// Version 정보 창을 오픈
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TbVersion_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CVersionUI cVersionUI = new CVersionUI();
            cVersionUI.ShowDialog();
        }

        /// <summary>
        /// Info Popup Flag
        /// </summary>
        private bool bInfoShowPopup = false;

        /// <summary>
        /// Info 매뉴 이벤트 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN) return;
            if (ml.GetUserLevelCheck((int)eUserLevel.ENGINEER) == false) return;

            if (bInfoShowPopup == false)
            {
                PopupManuCtrl(1);
            }
            else
            {
                Popup_Info.IsOpen = false;
            }
            bInfoShowPopup = !bInfoShowPopup;
        }

        /// <summary>
        /// Setup Popup Flag
        /// </summary>
        private bool bSetupShowPopup = false;

        /// <summary>
        /// Setup 매뉴 이벤트 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetupButton_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN) return;
            if (ml.GetUserLevelCheck((int)eUserLevel.ENGINEER) == false) return;

            if (bSetupShowPopup == false)
            {
                PopupManuCtrl(2);
            }
            else
            {
                Popup_Setup.IsOpen = false;
            }
            bSetupShowPopup = !bSetupShowPopup;
        }

        /// <summary>
        /// 프로그램 종료
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Quit_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN ||
                ml.McState == eMachineState.MANUALRUN)
            {
                CCommon.ShowMessageMini(0, "구동중에는 종료할 수 없습니다.");
                return;
            }

            if (Define.SIMULATION == false &&
                ml.cOptionData.bDryRunUse == false)
            {
                if (ml.cRunUnitData.GetIndexData(eData.PIPE_PnP).GetStatus(eStatus.EMPTY) == false ||
                    ml.cRunUnitData.GetIndexData(eData.NEEDLE_PnP).GetStatus(eStatus.EMPTY) == false ||
                    ml.cRunUnitData.GetIndexData(eData.PIPE_TRANSFER).GetStatus(eStatus.EMPTY) == false ||
                    ml.cRunUnitData.GetIndexData(eData.NEEDLE_TRANSFER).GetStatus(eStatus.EMPTY) == false ||
                    ml.cRunUnitData.GetIndexData(eData.FEEDER_BASKET).GetStatus(eStatus.EMPTY) == false ||
                    ml.Seq.SeqIO.GetInput((int)eIO_I.PNP_PIPE_VACUUM) == true ||
                    ml.Seq.SeqIO.GetInput((int)eIO_I.PNP_NEEDLE_VACUUM) == true ||
                    ml.Seq.SeqIO.GetInput((int)eIO_I.TRANSFER_PIPE_VACUUM) == true ||
                    ml.Seq.SeqIO.GetInput((int)eIO_I.TRANSFER_NEEDLE_VACUUM) == true)
                {
                    CCommon.ShowMessageMini(0, "잡고 있는 파이프와 니들을 비워주세요.");
                    return;
                }
            }

            // Data Save
            CXMLProcess.WriteXml(CXMLProcess.RunUnitDataFilePath, CMainLib.Ins.cRunUnitData);
            CXMLProcess.WriteXml(CXMLProcess.SystemParameterSingleFilePath, CMainLib.Ins.cSysOne);
            CXMLProcess.WriteXml(CXMLProcess.SystemParameterCollectionDataFilePath, CMainLib.Ins.cSysParamCollData);

            // 자동 중요 데이터 백업
            CCommon.CheckDataBackUp();

            if (ml.cOptionData.iLanguageMode == (int)eLanguage.KOREAN) if (CCommon.ShowMessage(1, "종료 하시겠습니까 ?") != (int)eMBoxRtn.A_OK) return;
                else if (ml.cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) if (CCommon.ShowMessage(1, "Close program ?") != (int)eMBoxRtn.A_OK) return;

            // Log 기록!
            NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "System Exit...", false);
            // 프로그램 종료 시 IO 세팅!
            ml.Seq.SeqMonitoring.Set_IO_System_Close();
            Close();
        }

        /// <summary>
        /// 팝업 창을 열면 다른 창은 닫히도록 변경
        /// </summary>
        /// <param name="iCtrl">0 : 로그인, 1 : Info, 2 : Setup, 3 : 모두 닫기</param>
        private void PopupManuCtrl(int iCtrl)
        {
            if (iCtrl == 0) // 로그인
            {
                Popup_UserLevel.IsOpen = true;
                bInfoShowPopup = false;
                Popup_Info.IsOpen = false;
                bSetupShowPopup = false;
                Popup_Setup.IsOpen = false;
            }
            else if (iCtrl == 1) // Info
            {
                bUserLevelShowPopup = false;
                Popup_UserLevel.IsOpen = false;
                Popup_Info.IsOpen = true;
                bSetupShowPopup = false;
                Popup_Setup.IsOpen = false;
            }
            else if (iCtrl == 2) // Setup
            {
                bUserLevelShowPopup = false;
                Popup_UserLevel.IsOpen = false;
                bInfoShowPopup = false;
                Popup_Info.IsOpen = false;
                Popup_Setup.IsOpen = true;
            }
            else if (iCtrl == 3) // 모두 닫기
            {
                bUserLevelShowPopup = false;
                Popup_UserLevel.IsOpen = false;
                bInfoShowPopup = false;
                Popup_Info.IsOpen = false;
                bSetupShowPopup = false;
                Popup_Setup.IsOpen = false;
            }
        }

        #endregion 윈도우 이벤트 정의

        #region 통신 연결 상태

        /// <summary>
        /// 통신 재 연결
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VisionReConnect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// TCPConnectCheck Popup Flag
        /// </summary>
        private bool bTCPConnectCheckShowPopup = false;

        /// <summary>
        /// TCP 연결 상태 표시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_ConnectCheckFlag_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ml.McState == eMachineState.RUN ||
                ml.McState == eMachineState.MANUALRUN)
            {
                return;
            }

            if (bTCPConnectCheckShowPopup == false)
            {
                Popup_ConnectCheck.IsOpen = true;
                TCPConnectCheck();
            }
            else
            {
                Popup_ConnectCheck.IsOpen = false;
            }
            bTCPConnectCheckShowPopup = !bTCPConnectCheckShowPopup;
        }

        /// <summary>
        /// TCP 연결 체크 위치 활성화
        /// </summary>
        private void TCPConnectCheck()
        {
            //string[] strArrError = null;
            //bool bErrorRtn = false;
            //int iRtn = 0;

            if (ml.cLightCtl != null &&
                ml.cLightCtl._bPortOpend == true)
            {
                ImgVisionLightConnect.Opacity = 1.0;
            }
            else
            {
                ImgVisionLightConnect.Opacity = 0.3;
            }

            if (ml.cUV_Ctl != null &&
                ml.cUV_Ctl._bPortOpend == true)
            {
                ImgAltisUV.Opacity = 1.0;
            }
            else
            {
                ImgAltisUV.Opacity = 0.3;
            }

            if (ml.AIM_Feeder != null &&
                ml.AIM_Feeder.cTCPClient != null &&
                ml.AIM_Feeder.cTCPClient.bConncect == true)
            {
                ImgAIMFeeder.Opacity = 1.0;
            }
            else
            {
                ImgAIMFeeder.Opacity = 0.3;
            }
        }

        #endregion 통신 연결 상태

        #region Common 함수 정의

        /// <summary>
        /// 초기 각 UI를 생성하고 초기화
        /// </summary>
        private void UICreateAndInit()
        {
            // Main UI 생성
            if (cMainUI == null)
            {
                cMainUI = MainUI.Ins;
            }

            // Model Data Select UI 생성
            if (cModelSelectUI == null)
            {
                cModelSelectUI = new ModelSelectUI();
            }

            // Model Data Edit 생성
            if (cDataEditUI == null)
            {
                cDataEditUI = new DataEditUI();
            }

            // DataEdit UI를 다중으로 생성 할 경우로 초기화를 해야한다.
            CMainLib.Ins.cDataEditUIManager.Init();
            // MapData UI 초기화
            CMainLib.Ins.cDataEditUIManager.MapDataInit();

            // User Set UI 생성
            if (cOptionUI == null)
            {
                cOptionUI = new OptionUI();
            }

            // IO Monitor UI 생성
            if (cIOMonitorUI == null)
            {
                cIOMonitorUI = new IOMonitorUI();
            }

            // Alarm Scereen UI 생성
            if (cAlarmScreenUI == null)
            {
                cAlarmScreenUI = new AlarmScreenUI();
            }

            // Log View UI 생성
            if (cLogViewUI == null)
            {
                cLogViewUI = new LogViewUI();
            }

            // 생산량 View UI 생성
            if (cGraphViewUI == null)
            {
                cGraphViewUI = new GraphViewUI();
            }

            // Vision 결과 View UI 생성
            if (visionGraphViewUI == null)
            {
                visionGraphViewUI = new VisionGraphViewUI();
            }

            // 에러 View UI 생성
            if (cErrorViewUI == null)
            {
                cErrorViewUI = new ErrorViewUI();
            }
        }

        /// <summary>
        /// 모델명 변경
        /// </summary>
        public void ModelNameChange()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate ()
            {
                // 현재 설정되어 있는 모델 표시
                if (ml.cSysParamCollData.CheckSysArray(ml.cSysOne.uiCurrentModelNo) == false) ml.cSysOne.uiCurrentModelNo = 0;
                CSystemParameterArray cSysArray = ml.cSysParamCollData.GetSysArray();
                string strModelName = string.Format("『{0}』 {1}", ml.cSysOne.uiCurrentModelNo,
                                                                 cSysArray.strModelName);
                TbModelName.Text = strModelName;
            });
        }

        /// <summary>
        /// 보이는 Main UI를 변경한다.
        /// </summary>
        /// <param name="_uiIndex"></param>
        public void Change_Form(_UIIndex _uiIndex)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate ()
            {
                if (_uiIndex == _UIIndex.MainUI &&
                    CheckNowUseUI.CurrentUIIndex != _UIIndex.MainUI)
                {
                    CheckNowUseUI.CurrentUIIndex = _UIIndex.MainUI;
                    MainPanel.Content = MainUI.Ins;
                }
            });
        }

        /// <summary>
        /// DoorCheck Popup Flag
        /// </summary>
        private bool bDoorCheckShowPopup = false;

        /// <summary>
        /// 어느 도어가 열려있는지 표시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_DoorCheckFlag_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ml.McState == eMachineState.RUN ||
                ml.McState == eMachineState.MANUALRUN)
            {
                return;
            }

            if (bDoorCheckShowPopup == false)
            {
                Popup_DoorCheck.IsOpen = true;
                DoorCheckEnable(true);
            }
            else
            {
                Popup_DoorCheck.IsOpen = false;
                DoorCheckEnable(false);
            }
            bDoorCheckShowPopup = !bDoorCheckShowPopup;
        }

        /// <summary>
        /// 도어 체크 위치 활성화
        /// </summary>
        /// <param name="bEnable"></param>
        private void DoorCheckEnable(bool bEnable)
        {
            if (bEnable == true)
            {
                if (ml.Seq.SeqIO.GetInput((int)eIO_I.DOOR_SWITCH_1_CLOSE_CHECK_DOOR_CLOSE_ON_FRONT_RIGHT) == false) Rt_14_CLOSE_CHECK_DOOR_FRONT_RIGHT.Visibility = Visibility.Visible;
                if (ml.Seq.SeqIO.GetInput((int)eIO_I.DOOR_SWITCH_2_CLOSE_CHECK_DOOR_LOCK_ON_FRONT_CENTER_RIGHT) == false) Rt_16_CLOSE_CHECK_DOOR_FRONT_CENTER_RIGHT.Visibility = Visibility.Visible;
                if (ml.Seq.SeqIO.GetInput((int)eIO_I.DOOR_SWITCH_3_CLOSE_CHECK_DOOR_CLOSE_ON_FRONT_CENTER_LEFT) == false) Rt_18_CLOSE_CHECK_DOOR_FRONT_CENTER_LEFT.Visibility = Visibility.Visible;
                if (ml.Seq.SeqIO.GetInput((int)eIO_I.DOOR_SWITCH_4_CLOSE_CHECK_DOOR_CLOSE_ON_REAR_LEFT) == false) Rt_20_CLOSE_CHECK_DOOR_REAR_LEFT.Visibility = Visibility.Visible;
                if (ml.Seq.SeqIO.GetInput((int)eIO_I.DOOR_SWITCH_5_CLOSE_CHECK_DOOR_CLOSE_ON_REAR_RIGHT) == false) Rt_22_CLOSE_CHECK_DOOR_REAR_RIGHT.Visibility = Visibility.Visible;
                if (ml.Seq.SeqIO.GetInput((int)eIO_I.DOOR_SWITCH_6_CLOSE_CHECK_DOOR_CLOSE_ON_FRONT_LEFT) == false) Rt_88_CLOSE_CHECK_DOOR_FRONT_LEFT.Visibility = Visibility.Visible;
            }
            else
            {
                Rt_14_CLOSE_CHECK_DOOR_FRONT_RIGHT.Visibility = Visibility.Hidden;
                Rt_16_CLOSE_CHECK_DOOR_FRONT_CENTER_RIGHT.Visibility = Visibility.Hidden;
                Rt_18_CLOSE_CHECK_DOOR_FRONT_CENTER_LEFT.Visibility = Visibility.Hidden;
                Rt_20_CLOSE_CHECK_DOOR_REAR_LEFT.Visibility = Visibility.Hidden;
                Rt_22_CLOSE_CHECK_DOOR_REAR_RIGHT.Visibility = Visibility.Hidden;
                Rt_88_CLOSE_CHECK_DOOR_FRONT_LEFT.Visibility = Visibility.Hidden;
            }
        }

        #endregion Common 함수 정의
    }
}