using System;
using System.Windows;
using System.Windows.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// InitializeBoxUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InitializeBoxUI : Window, IDisposable
    {
        /// <summary>
        /// 타이머
        /// </summary>
        private DispatcherTimer ctimer = new DispatcherTimer();

        private bool disposed;

        private static InitializeBoxUI instance = null;

        public static InitializeBoxUI Instance
        {
            get
            {
                if (instance == null) instance = new InitializeBoxUI();
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public InitializeBoxUI()
        {
            InitializeComponent();
            CMainLib.Ins.Seq.SeqInitilize.InitializingEnded += InitFinish;

            ctimer.Interval = TimeSpan.FromMilliseconds(100);     // 시간 간격 설정
            ctimer.Tick += new EventHandler(Timer_Tick);          // 이벤트 추가
            ctimer.Start();                                       // 타이머 시작
        }

        ~InitializeBoxUI()
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
        /// 윈도우 로딩 완료
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressBar.StartupDelay = 0;
            COptionData cOptionData = CMainLib.Ins.cOptionData;
            if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) TBProgressName.Text = "전체 초기화를 시작합니다. 안전에 주의해 주세요.";
            else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) TBProgressName.Text = "Machine Initilize Start. Please, Check Safety.";
        }

        /// <summary>
        /// 데이터 갱신용 반복 타이머
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (bStartOrStop == true)
            {
                TBProgesssText.Text = CMainLib.Ins.Seq.SeqInitilize.iStep.ToString() + "%";
                TBProgressName.Text = CMainLib.Ins.Seq.SeqInitilize.strMsgState;
            }
        }

        /// <summary>
        /// 종료 이벤트
        /// </summary>
        public void InitFinish()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                ctimer.Stop();
                Close();
            }));
        }

        /// <summary>
        /// Start, Stop 상태 플래그
        /// </summary>
        private bool bStartOrStop = false;

        /// <summary>
        /// Home Start Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtHomeStartStop_Click(object sender, RoutedEventArgs e)
        {
            if (bStartOrStop == false) // Start
            {
                ProgressBar.Visibility = Visibility.Visible;
                BTHomeStartStop.Content = "STOP";
                CMainLib.Ins.Seq.SeqInitilize.Start();
            }
            else // Stop
            {
                CMainLib.Ins.Seq.SeqInitilize.InitStop();
                CMainLib.Ins.Seq.SeqIO.SetMachineState(eMachineState.READY);
                Close();
            }
            bStartOrStop = !bStartOrStop;
        }
    }
}