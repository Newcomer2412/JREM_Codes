using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// MainUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainUI : UserControl
    {
        private static readonly object readLock = new object();
        private static MainUI instance = null;

        public static MainUI Ins
        {
            get
            {
                lock (readLock)
                {
                    if (instance == null) instance = new MainUI();
                    return instance;
                }
            }
            set
            {
                instance = value;
            }
        }

        /// <summary>
        /// Main Library
        /// </summary>
        private CMainLib ml = null;

        /// <summary>
        /// Slider Main UI
        /// </summary>
        private SlideMainUI slideMainUI = new SlideMainUI();

        /// <summary>
        /// Slider Vision UI
        /// </summary>
        private SlideVisionUI slideVisionUI = new SlideVisionUI();

        /// <summary>
        /// 생성자
        /// </summary>
        public MainUI()
        {
            InitializeComponent();
            instance = this;
            ml = CMainLib.Ins;
            NLogger.LogUpdateEvent += NLogger_LogUpdateEvent;

            // 각각 Map을 표시하는 User Control을 Slide Panel에 대입한다.
            SlidePanel1.Content = slideMainUI;
            SlidePanel2.Content = slideVisionUI;
        }

        /// <summary>
        /// 초기화 함수
        /// </summary>
        public void Init()
        {
        }

        /// <summary>
        /// 데이터 갱신용 반복 타이머
        /// </summary>
        public void DataTimer_Tick()
        {
            CSystemParameterArray cSysArray = ml.cSysParamCollData.GetSysArray();

            slideMainUI.DataTimer_Tick();
            // U.P.H
            if (TbUPHCount.Text != ml.cSysOne.iUPHCount.ToString())
                TbUPHCount.Text = ml.cSysOne.iUPHCount.ToString();
            // 작업 개수
            if (TbWorkCount.Text != cSysArray.iTotalCount.ToString())
                TbWorkCount.Text = cSysArray.iTotalCount.ToString();
            // 불량 개수
            if (TbNgCount.Text != cSysArray.iNgCount.ToString())
                TbNgCount.Text = cSysArray.iNgCount.ToString();
        }

        /// <summary>
        /// Main UI 및 Vision에 로그 표시
        /// </summary>
        /// <param name="eType"></param>
        /// <param name="eLevel"></param>
        /// <param name="strMsg"></param>
        private void NLogger_LogUpdateEvent(eLogType eType, NLogger.eLogLevel eLevel, string strMsg)
        {
            DateTime DtToday = DateTime.Now;
            string strMsgLog = string.Format("[{0:T}][{1}] {2}", DtToday, eLevel, strMsg);

            if (eType == eLogType.PROGRAM)
            {
                ListViewProgramLog.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                          new ListViewDelegate(UpdateListView), strMsgLog);
            }
            else if (eType >= eLogType.CAM0_PipeNeedlePickUp && eType <= eLogType.CAM5_Dispenser)
            {
                eType -= eLogType.VisionShoot;
                ml.cVisionToolBlockUI[(int)eType - 1].AddVisionLog(strMsg);
            }
        }

        /// <summary>
        /// 쓰레드 등 충돌을 방지하기 위한 델리게이트
        /// </summary>
        /// <param name="strLog"></param>
        private delegate void ListViewDelegate(string strLog);

        /// <summary>
        /// Program Log의 ListView 데이터를 갱신하고 많을 경우 초기화 한다.
        /// </summary>
        /// <param name="strLog"></param>
        private void UpdateListView(string strLog)
        {
            if (ListViewProgramLog.Items.Count > 50)
            {
                ListViewProgramLog.Items.Clear();
            }
            ListMessage listMessage = new ListMessage() { strMessage = strLog };
            ListViewProgramLog.Items.Insert(0, listMessage);
        }

        #region Manual

        /// <summary>
        /// 작업 수량 초기화
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Work_Count_Reset_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CSystemParameterArray cSysArray = ml.cSysParamCollData.GetSysArray();
            cSysArray.iTotalCount = 0;
        }

        /// <summary>
        /// NG 수량 초기화
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NG_Count_Reset_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CSystemParameterArray cSysArray = ml.cSysParamCollData.GetSysArray();
            cSysArray.iNgCount = 0;
        }

        /// <summary>
        /// 화면에 보여지고 있는 화면 번호
        /// </summary>
        private int iMainResultViewNum = 0;

        /// <summary>
        /// 슬라이더 1번 화면 전환
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReSlide1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ml.VisionColorDisable();
            SlideVisionUI.Ins.VisionMainView();
            DoubleAnimation doubleAnimation = Resources["MoveAnimationKey"] as DoubleAnimation;
            iMainResultViewNum = 0;
            doubleAnimation.To = -iMainResultViewNum * SlideGrid.ActualHeight;
            SetEllipseColor(iMainResultViewNum);
            SlideStackPanel.BeginAnimation(Canvas.TopProperty, doubleAnimation, HandoffBehavior.Compose);
        }

        /// <summary>
        /// 슬라이더 2번 화면 전환
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReSlide2_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ml.cDataEditUIManager.PopupClose();
            SlideMainUI.Ins.PopupClose();
            DoubleAnimation doubleAnimation = Resources["MoveAnimationKey"] as DoubleAnimation;
            iMainResultViewNum = 1;
            doubleAnimation.To = -iMainResultViewNum * SlideGrid.ActualHeight;
            SetEllipseColor(iMainResultViewNum);
            SlideStackPanel.BeginAnimation(Canvas.TopProperty, doubleAnimation, HandoffBehavior.Compose);
        }

        /// <summary>
        /// 슬라이드 버튼 컬러 변경
        /// </summary>
        /// <param name="iNum"></param>
        private void SetEllipseColor(int iNum)
        {
            switch (iNum)
            {
                case 0:
                    {
                        ReSlide1.Stroke = Brushes.Yellow;
                        ReSlide2.Stroke = Brushes.Transparent;
                    }
                    break;

                case 1:
                    {
                        ReSlide1.Stroke = Brushes.Transparent;
                        ReSlide2.Stroke = Brushes.Yellow;
                    }
                    break;

                default: break;
            }
        }

        #endregion Manual
    }

    /// <summary>
    /// Log 메세지 바인딩 클래스
    /// </summary>
    public class ListMessage
    {
        public string strMessage { get; set; }
    }
}