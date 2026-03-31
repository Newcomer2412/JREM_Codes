using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// MotionUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ManualMotionUI : Window, IDisposable
    {
        /// <summary>
        /// Main Lib
        /// </summary>
        private CMainLib ml = null;

        /// <summary>
        /// 설정 값
        /// </summary>
        private COptionData cOptionData = null;

        /// <summary>
        /// IO Monitor Mini UI
        /// </summary>
        private IOMonitorMiniUI cIOMonitorMiniUI = null;

        /// <summary>
        /// Data 갱신용 타이머
        /// </summary>
        private DispatcherTimer cDataTimer = null;

        /// <summary>
        /// 창이 열렸는지 확인하는 변수
        /// </summary>
        private static bool _bOpened = false;

        public bool bOpened
        {
            get { return _bOpened; }
            set
            {
                _bOpened = value;
            }
        }

        private bool disposed;

        ~ManualMotionUI()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed) return;
            if (disposing)
            {
                // IDisposable 인터페이스를 구현하는 멤버들을 여기서 정리
            }
            // .NET Framework에 의하여 관리되지 않는 외부 리소스들을 여기서 정리
            this.disposed = true;
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public ManualMotionUI()
        {
            InitializeComponent();
            BtShowIo.Content = ">";
            GridShowIO.Width = 0;
            this.Width = 350;
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Init()
        {
            ml = CMainLib.Ins;
            cOptionData = ml.cOptionData;

            bOpened = true;
            cIOMonitorMiniUI = new IOMonitorMiniUI();
            IOPanel.Content = cIOMonitorMiniUI;
            RbFirst.IsChecked = true;

            thHome = new Thread[ml.Axis.Count];

            for (int i = 0; i < ml.Axis.Count; i++)
            {
                string strAxisName = string.Format("『{0:00}』 {1}", i, Enum.GetName(typeof(eMotor), i));
                CbAxisList.Items.Add(strAxisName);
            }
            CbAxisList.SelectedIndex = ml.cVar.iManualMotionAxisNo;

            if (cDataTimer == null)
            {
                cDataTimer = new DispatcherTimer();
                cDataTimer.Interval = TimeSpan.FromMilliseconds(100);   // 시간 간격 설정
                cDataTimer.Tick += new EventHandler(DataTimer_Tick);    // 이벤트 추가
            }
            cDataTimer.Start(); // 타이머 시작
        }

        private bool bShowIO = false;

        /// <summary>
        /// IO를 보여주거나 감춘다
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowIO_Button_Click(object sender, RoutedEventArgs e)
        {
            bShowIO = !bShowIO;
            if (bShowIO == true)
            {
                GridShowIO.Width = 600;
                this.Width = 950;
                BtShowIo.Content = "<";
                cIOMonitorMiniUI.StartTimer();
            }
            else
            {
                GridShowIO.Width = 0;
                this.Width = 350;
                BtShowIo.Content = ">";
                cIOMonitorMiniUI.StopTimer();
            }
        }

        /// <summary>
        /// Close 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            bOpened = false;
            if (cDataTimer.IsEnabled == true) cDataTimer.Stop();
            Close();
        }

        private bool[] bOldStatus = new bool[7] { false, false, false, false, false, false, false };
        private double dOldCommandPos = 0, dOldFeedBackPos = 0;

        /// <summary>
        /// 데이터 갱신용 반복 타이머
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataTimer_Tick(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate ()
            {
                int iSelectAxis = CbAxisList.SelectedIndex;
                double dPos = 0;
                dPos = ml.Axis[(eMotor)iSelectAxis].GetCmdPostion();
                if (dPos != dOldCommandPos)
                {
                    TbCommandPos.Text = dPos.ToString();
                    dOldCommandPos = dPos;
                }
                dPos = ml.Axis[(eMotor)iSelectAxis].GetActPostion();
                if (dPos != dOldFeedBackPos)
                {
                    TbFeedBackPos.Text = dPos.ToString();
                    dOldFeedBackPos = dPos;
                }

                bool bRtn = false;

                // SAlarm
                bRtn = ml.Axis[(eMotor)iSelectAxis].GetServoFault();
                if (bRtn != bOldStatus[0])
                {
                    if (bRtn == true) AxisSAlarm.Foreground = Brushes.Red;
                    else AxisSAlarm.Foreground = Brushes.Gray;
                    bOldStatus[0] = bRtn;
                }

                // - Limit
                bRtn = ml.Axis[(eMotor)iSelectAxis].GetMinusLimit();
                if (bRtn != bOldStatus[1])
                {
                    if (bRtn == true) AxisMLimit.Foreground = Brushes.Red;
                    else AxisMLimit.Foreground = Brushes.Gray;
                    bOldStatus[1] = bRtn;
                }

                // + Limit
                bRtn = ml.Axis[(eMotor)iSelectAxis].GetPlusLimit();
                if (bRtn != bOldStatus[2])
                {
                    if (bRtn == true) AxisPLimit.Foreground = Brushes.Red;
                    else AxisPLimit.Foreground = Brushes.Gray;
                    bOldStatus[2] = bRtn;
                }

                // Origin
                bRtn = ml.Axis[(eMotor)iSelectAxis].GetHomeSensor();
                if (bRtn != bOldStatus[3])
                {
                    if (bRtn == true) AxisOrigin.Foreground = Brushes.Red;
                    else AxisOrigin.Foreground = Brushes.Gray;
                    bOldStatus[3] = bRtn;
                }

                // InPos
                bRtn = ml.Axis[(eMotor)iSelectAxis].GetInPosition();
                if (bRtn != bOldStatus[4])
                {
                    if (bRtn == true) AxisInpos.Foreground = Brushes.Red;
                    else AxisInpos.Foreground = Brushes.Gray;
                    bOldStatus[4] = bRtn;
                }

                // S-ON
                bRtn = ml.Axis[(eMotor)iSelectAxis].GetServoState();
                if (bRtn != bOldStatus[5])
                {
                    if (bRtn == true) AxisSOn.Foreground = Brushes.Red;
                    else AxisSOn.Foreground = Brushes.Gray;
                    bOldStatus[5] = bRtn;
                }

                // Controller Alarm
                bRtn = ml.Axis[(eMotor)iSelectAxis].GetControllerFault();
                if (bRtn != bOldStatus[6])
                {
                    if (bRtn == true) AxisAlarm.Foreground = Brushes.Red;
                    else AxisAlarm.Foreground = Brushes.Gray;
                    bOldStatus[6] = bRtn;
                }

                if (bRepeatMoveStart == true) RepeatMove();
            });
        }

        /// <summary>
        /// Repeat Move 시작 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RepeatMove_Button_Click(object sender, RoutedEventArgs e)
        {
            // 간섭 및 충돌 확인! (인터락, 엔지니어 레벨은 미확인)
            if (ml.GetUserLevelCheck((int)eUserLevel.ADMINISTRATOR) == false || bRepeatMoveStart == true) return;
            int iSelectAxis = CbAxisList.SelectedIndex;
            if (ml.Axis[(eMotor)iSelectAxis].IsMoveDone() == false) return;

            if (uint.TryParse(TbSpeed.Text, out uiRepeatSpeed) == false)
            {
                string strMessage = string.Empty;
                if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = string.Format("속도를 입력하세요");
                else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = string.Format("Please Input Speed");
                CCommon.ShowMessageMini(strMessage);
                return;
            }

            // 입력 제한!
            if (uiRepeatSpeed > 500) uiRepeatSpeed = 500;
            if (uiRepeatSpeed < 0) uiRepeatSpeed = 0;

            if (double.TryParse(TbStartPos.Text, out dStartPos) == false)
            {
                string strMessage = string.Empty;
                if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = string.Format("시작 위치를 입력하세요");
                else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = string.Format("Please Input Start Pos");
                CCommon.ShowMessageMini(strMessage);
                return;
            }

            if (double.TryParse(TbEndPos.Text, out dEndPos) == false)
            {
                string strMessage = string.Empty;
                if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = string.Format("도착 위치를 입력하세요");
                else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = string.Format("Please Input End Pos");
                CCommon.ShowMessageMini(strMessage);
                return;
            }

            if (int.TryParse(TbDelayTime.Text, out iDelayTime) == false)
            {
                string strMessage = string.Empty;
                if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = string.Format("Delay Time을 입력하세요");
                else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = string.Format("Please Input Delay Time");
                CCommon.ShowMessageMini(strMessage);
                return;
            }

            // 반복 이송 시작
            iRepeatMoveStep = 10;
            bRepeatMoveStart = true;
        }

        /// <summary>
        /// Start 위치
        /// </summary>
        private double dStartPos = 0;

        /// <summary>
        /// End 위치
        /// </summary>
        private double dEndPos = 0;

        /// <summary>
        /// Delay Time
        /// </summary>
        private int iDelayTime = 0;

        /// <summary>
        /// 반복 이송 Flag
        /// </summary>
        private bool bRepeatMoveStart = false;

        /// <summary>
        /// 반복 이송 Step
        /// </summary>
        private int iRepeatMoveStep = 0;

        /// <summary>
        /// 반복 이송 속도
        /// </summary>
        private uint uiRepeatSpeed = 0;

        /// <summary>
        /// 시간 체크 타이머
        /// </summary>
        private Stopwatch cDelayTimer = new Stopwatch();

        /// <summary>
        /// 반복 이송
        /// </summary>
        private void RepeatMove()
        {
            int iSelectAxis = CbAxisList.SelectedIndex;

            if (bOldStatus[0] == true || bOldStatus[1] == true || bOldStatus[2] == true || bOldStatus[5] == false)
            {
                bRepeatMoveStart = false;               // Repeat Move Flag는 false로 초기화
                iRepeatMoveStep = 0;                    // Step 초기화
                ml.Axis[(eMotor)iSelectAxis].EmergencyStop();
                string strMessage = string.Empty;
                if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = string.Format("모터 축 알람 혹은 홈 완료 필요로 정지하였습니다.");
                else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = string.Format("Motor axis alarm or Not Home stopped.");
                CCommon.ShowMessageMini(strMessage);
                return;
            }

            switch (iRepeatMoveStep)
            {
                case 10:
                    if (ml.Axis[(eMotor)iSelectAxis].IsMoveDone() == true)
                    {
                        ml.Axis[(eMotor)iSelectAxis].MoveAbsolute(dStartPos, uiRepeatSpeed, 100, 100);
                        cDelayTimer.Restart();
                        iRepeatMoveStep = 20;
                    }
                    return;

                case 20:
                    if (cDelayTimer.ElapsedMilliseconds >= iDelayTime)
                    {
                        cDelayTimer.Restart();
                        ml.Axis[(eMotor)iSelectAxis].MoveAbsolute(dEndPos, uiRepeatSpeed, 100, 100);
                        iRepeatMoveStep = 30;
                    }
                    return;

                case 30:
                    if (cDelayTimer.ElapsedMilliseconds >= iDelayTime)
                    {
                        iRepeatMoveStep = 10;
                    }
                    return;
            }

            return;
        }

        /// <summary>
        /// Servo On 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SON_Button_Click(object sender, RoutedEventArgs e)
        {
            int iSelectAxis = CbAxisList.SelectedIndex;
            ml.Axis[(eMotor)iSelectAxis].SetServoState(true);
        }

        /// <summary>
        /// Servo Off
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SOFF_Button_Click(object sender, RoutedEventArgs e)
        {
            string strMessage = string.Empty;
            if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = "해당 축을 서보 Off 하시겠습니까?";
            else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = "Do you want Axis Servo Off?";
            if (CCommon.ShowMessageMini(2, strMessage) == (int)eMBoxRtn.A_Cancel) return;

            int iSelectAxis = CbAxisList.SelectedIndex;
            ml.cVar.bInitializeMotor = false;
            ml.Axis[(eMotor)iSelectAxis].SetServoState(false);
        }

        /// <summary>
        /// 이송 방향 정의
        /// </summary>
        private enum eDirection : int
        {
            DIR_CCW = 0,
            DIR_CW = 1,
        }

        /// <summary>
        /// CW 방향 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CCW_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 간섭 및 충돌 확인! (인터락, 엔지니어 레벨은 미확인)
            if (ml.GetUserLevelCheck((int)eUserLevel.ADMINISTRATOR) == false || bRepeatMoveStart == true) return;
            int iSelectAxis = CbAxisList.SelectedIndex;
            if (ml.Axis[(eMotor)iSelectAxis].IsMoveDone() == false) return;

            uint uiSpeed = 0;
            if (uint.TryParse(TbSpeed.Text, out uiSpeed) == false)
            {
                string strMessage = string.Empty;
                if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = string.Format("속도를 입력하세요");
                else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = string.Format("Please Input Speed");
                CCommon.ShowMessageMini(strMessage);
                return;
            }

            // 입력 제한!
            //if (uiSpeed > 500) uiSpeed = 500;
            if (uiSpeed < 0) uiSpeed = 0;

            switch (iCurruntRadioCheckNo)
            {
                case (int)eRadioCheck._1umCheck:
                    ml.Axis[(eMotor)iSelectAxis].MoveRelative(-0.001, uiSpeed, 100, 100, false);
                    break;

                case (int)eRadioCheck._10umCheck:
                    ml.Axis[(eMotor)iSelectAxis].MoveRelative(-0.01, uiSpeed, 100, 100, false);
                    break;

                case (int)eRadioCheck._100umCheck:
                    ml.Axis[(eMotor)iSelectAxis].MoveRelative(-0.1, uiSpeed, 100, 100, false);
                    break;

                case (int)eRadioCheck._1mmCheck:
                    ml.Axis[(eMotor)iSelectAxis].MoveRelative(-1, uiSpeed, 100, 100, false);
                    break;

                case (int)eRadioCheck._10mmCheck:
                    ml.Axis[(eMotor)iSelectAxis].MoveRelative(-10, uiSpeed, 100, 100);
                    break;

                case (int)eRadioCheck._JogCheck:
                    ml.Axis[(eMotor)iSelectAxis].MoveVelocity(false, uiSpeed);
                    break;

                case (int)eRadioCheck._DefineCheck:
                    double dDefinePos = 0;
                    if (double.TryParse(TbDefinePos.Text, out dDefinePos) == false)
                    {
                        string strMessage = string.Empty;
                        if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = string.Format("Define에 거리를 입력하세요");
                        else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = string.Format("Please Input Define Pos");
                        CCommon.ShowMessageMini(strMessage);
                        return;
                    }
                    // 입력 제한!
                    //if (dDefinePos > 999) dDefinePos = 999;
                    //if (dDefinePos < -999) dDefinePos = -999;
                    ml.Axis[(eMotor)iSelectAxis].MoveRelative(-dDefinePos, uiSpeed, 100, 100);
                    break;

                case (int)eRadioCheck._AbsCheck:
                    double dAbsPos = 0;
                    if (double.TryParse(TbAbsPos.Text, out dAbsPos) == false)
                    {
                        string strMessage = string.Empty;
                        if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = string.Format("ABS에 거리를 입력하세요");
                        else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = string.Format("Please Input Abs Pos");
                        CCommon.ShowMessageMini(strMessage);
                        return;
                    }
                    // 입력 제한!
                    //if (dDefinePos > 999) dDefinePos = 999;
                    //if (dDefinePos < -999) dDefinePos = -999;
                    ml.Axis[(eMotor)iSelectAxis].MoveAbsolute(-dAbsPos, uiSpeed, 100, 100);
                    break;
            }
        }

        /// <summary>
        /// CCW 방향 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CCW_Button_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (iCurruntRadioCheckNo == (int)eRadioCheck._JogCheck)
            {
                int iSelectAxis = CbAxisList.SelectedIndex;
                ml.Axis[(eMotor)iSelectAxis].Stop();
            }
        }

        /// <summary>
        /// CW 방향 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CW_Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 간섭 및 충돌 확인! (인터락, 엔지니어 레벨은 미확인)
            if (ml.GetUserLevelCheck((int)eUserLevel.ADMINISTRATOR) == false || bRepeatMoveStart == true) return;
            int iSelectAxis = CbAxisList.SelectedIndex;
            if (ml.Axis[(eMotor)iSelectAxis].IsMoveDone() == false) return;

            uint uiSpeed = 0;
            if (uint.TryParse(TbSpeed.Text, out uiSpeed) == false)
            {
                string strMessage = string.Empty;
                if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = string.Format("속도를 입력하세요");
                else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = string.Format("Please Input Speed");
                CCommon.ShowMessageMini(strMessage);
                return;
            }

            // 입력 제한!
            //if (uiSpeed > 500) uiSpeed = 500;
            if (uiSpeed < 0) uiSpeed = 0;

            switch (iCurruntRadioCheckNo)
            {
                case (int)eRadioCheck._1umCheck:
                    ml.Axis[(eMotor)iSelectAxis].MoveRelative(0.001, uiSpeed, 100, 100, false);
                    break;

                case (int)eRadioCheck._10umCheck:
                    ml.Axis[(eMotor)iSelectAxis].MoveRelative(0.01, uiSpeed, 100, 100, false);
                    break;

                case (int)eRadioCheck._100umCheck:
                    ml.Axis[(eMotor)iSelectAxis].MoveRelative(0.1, uiSpeed, 100, 100, false);
                    break;

                case (int)eRadioCheck._1mmCheck:
                    ml.Axis[(eMotor)iSelectAxis].MoveRelative(1, uiSpeed, 100, 100, false);
                    break;

                case (int)eRadioCheck._10mmCheck:
                    ml.Axis[(eMotor)iSelectAxis].MoveRelative(10, uiSpeed, 100, 100);
                    break;

                case (int)eRadioCheck._JogCheck:
                    ml.Axis[(eMotor)iSelectAxis].MoveVelocity(true, uiSpeed);
                    break;

                case (int)eRadioCheck._DefineCheck:
                    double dDefinePos = 0;
                    if (double.TryParse(TbDefinePos.Text, out dDefinePos) == false)
                    {
                        string strMessage = string.Empty;
                        if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = string.Format("Define에 거리를 입력하세요");
                        else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = string.Format("Please Input Define Pos");
                        CCommon.ShowMessageMini(strMessage);
                        return;
                    }
                    // 입력 제한!
                    //if (dDefinePos > 999) dDefinePos = 999;
                    //if (dDefinePos < -999) dDefinePos = -999;
                    ml.Axis[(eMotor)iSelectAxis].MoveRelative(dDefinePos, uiSpeed, 100, 100);
                    break;

                case (int)eRadioCheck._AbsCheck:
                    double dAbsPos = 0;
                    if (double.TryParse(TbAbsPos.Text, out dAbsPos) == false)
                    {
                        string strMessage = string.Empty;
                        if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = string.Format("ABS에 거리를 입력하세요");
                        else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = string.Format("Please Input Abs Pos");
                        CCommon.ShowMessageMini(strMessage);
                        return;
                    }
                    // 입력 제한!
                    //if (dDefinePos > 999) dDefinePos = 999;
                    //if (dDefinePos < -999) dDefinePos = -999;
                    ml.Axis[(eMotor)iSelectAxis].MoveAbsolute(dAbsPos, uiSpeed, 100, 100);
                    break;
            }
        }

        /// <summary>
        /// CW 방향 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CW_Button_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (iCurruntRadioCheckNo == (int)eRadioCheck._JogCheck)
            {
                int iSelectAxis = CbAxisList.SelectedIndex;
                ml.Axis[(eMotor)iSelectAxis].Stop();
            }
        }

        /// <summary>
        /// Stop 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Stop_Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < ml.Axis.Count; i++)
            {
                ml.Axis[(eMotor)i].Stop();
            }

            bRepeatMoveStart = false;
            iRepeatMoveStep = 0;
        }

        /// <summary>
        /// Home 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Home_Button_Click(object sender, RoutedEventArgs e)
        {
            int iSelectAxis = CbAxisList.SelectedIndex;
            // 간섭 및 충돌 확인! (인터락, 엔지니어 레벨은 미확인)
            if (ml.GetUserLevelCheck((int)eUserLevel.ADMINISTRATOR) == false) return;
            if (ml.Axis[(eMotor)iSelectAxis].IsMoveDone() == false) return;

            string strMessage = string.Empty;
            if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = "해당 축을 단독 원점 검색 하시겠습니까?";
            else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = "Do you want Single Axis Homming Motion?";
            if (CCommon.ShowMessageMini(2, strMessage) == (int)eMBoxRtn.A_Cancel) return;

            if (ml.Axis[(eMotor)iSelectAxis].GetServoState() == false)
            {
                if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = "모터 전원이 꺼져있습니다. 모터 전원을 켜주세요!";
                else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = "Motor power off, Plesase Motor power on!";
                if (CCommon.ShowMessageMini(2, strMessage) == (int)eMBoxRtn.A_Cancel) return;
            }

            if (thHome[iSelectAxis] != null)
            {
                ml.Axis[(eMotor)iSelectAxis].BreakStop = true;
                Thread.Sleep(50);
                thHome[iSelectAxis].Abort();
            }
            thHome[iSelectAxis] = new Thread(new ParameterizedThreadStart(HomeThread));
            thHome[iSelectAxis].IsBackground = true;
            thHome[iSelectAxis].Start(iSelectAxis);
        }

        /// <summary>
        /// 홈 동작 관리 스레드
        /// </summary>
        private Thread[] thHome;

        /// <summary>
        /// 각 축 홈 동작 스레드
        /// </summary>
        /// <param name="nAxis"></param>
        private void HomeThread(object oAxis)
        {
            try
            {
                int iAxis = (int)oAxis;
                ml.Axis[(eMotor)iAxis].Home();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 현재 선택된 라디오 버튼
        /// </summary>
        private int iCurruntRadioCheckNo = 0;

        /// <summary>
        /// Radio Button 정의
        /// </summary>
        private enum eRadioCheck : int
        {
            _1umCheck = 0,
            _10umCheck = 1,
            _100umCheck = 2,
            _1mmCheck = 3,
            _10mmCheck = 4,
            _JogCheck = 5,
            _DefineCheck = 6,
            _AbsCheck = 7,
        }

        /// <summary>
        /// 라디오 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // Get RadioButton reference.
            var Rdbutton = sender as RadioButton;
            // Display button content as title.
            string strRdContent = Rdbutton.Content.ToString();

            switch (strRdContent)
            {
                case "1 um": iCurruntRadioCheckNo = (int)eRadioCheck._1umCheck; break;
                case "10 um": iCurruntRadioCheckNo = (int)eRadioCheck._10umCheck; break;
                case "100 um": iCurruntRadioCheckNo = (int)eRadioCheck._100umCheck; break;
                case "1 mm": iCurruntRadioCheckNo = (int)eRadioCheck._1mmCheck; break;
                case "10 mm": iCurruntRadioCheckNo = (int)eRadioCheck._10mmCheck; break;
                case "JOG": iCurruntRadioCheckNo = (int)eRadioCheck._JogCheck; break;
                case "Define(mm)": iCurruntRadioCheckNo = (int)eRadioCheck._DefineCheck; break;
                case "ABS(mm)": iCurruntRadioCheckNo = (int)eRadioCheck._AbsCheck; break;
            }
        }

        /// <summary>
        /// 숫자 입력 클래스
        /// </summary>
        private NumPadUI m_NumPadUI = null;

        /// <summary>
        /// 입력 가능한 창에 마우스 클릭 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            m_NumPadUI = new NumPadUI();

            if (m_NumPadUI.bOpened == false)
            {
                m_NumPadUI.Init(sender as TextBlock);
                m_NumPadUI.ShowDialog();
            }
            else m_NumPadUI.Close();
        }

        /// <summary>
        /// 창을 Drag 하기 위한 기능 간단한 함수가 있지만 그 함수를 쓰면 프로그램 먹통 증상이
        /// 있어 기능을 풀어놓았다.
        /// </summary>
        private System.Drawing.Point _windowMoveMouseStart;

        private double _windowMoveStartTop;
        private double _windowMoveStartLeft;

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UIElement handle = sender as UIElement;
            if (handle == null)
                return;

            _windowMoveMouseStart = System.Windows.Forms.Control.MousePosition;
            _windowMoveStartLeft = this.Left;
            _windowMoveStartTop = this.Top;

            handle.MouseMove += Border_MouseMove;
            handle.CaptureMouse();

            ToggleButton_RepeatMove.IsChecked = false;
        }

        /// <summary>
        /// Pos 값 0으로 초기화
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PosZero_Button_Click(object sender, RoutedEventArgs e)
        {
            int iSelectAxis = CbAxisList.SelectedIndex;
            ml.Axis[(eMotor)iSelectAxis].SetZero();
        }

        /// <summary>
        /// 알람 리셋
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            int iSelectAxis = CbAxisList.SelectedIndex;
            ml.Axis[(eMotor)iSelectAxis].Reset();
        }

        /// <summary>
        /// 선택한 축 정보를 저장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbAxisList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ml.cVar.iManualMotionAxisNo = CbAxisList.SelectedIndex;
        }

        /// <summary>
        /// 창 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            UIElement handle = sender as UIElement;
            if (handle == null)
                return;

            if (e.LeftButton == MouseButtonState.Released)
            {
                handle.MouseMove -= Border_MouseMove; // Detach listener on mouse up.
                handle.ReleaseMouseCapture();
            }
            else
            {
                var smp = System.Windows.Forms.Control.MousePosition;
                var distanceX = smp.X - _windowMoveMouseStart.X;
                var distanceY = smp.Y - _windowMoveMouseStart.Y;

                this.Left = _windowMoveStartLeft + distanceX;
                this.Top = _windowMoveStartTop + distanceY;
            }
        }
    }
}