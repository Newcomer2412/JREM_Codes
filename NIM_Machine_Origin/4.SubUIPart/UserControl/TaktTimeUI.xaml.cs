using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// TactTimeUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TaktTimeUI : Window, IDisposable
    {
        /// <summary>
        /// Data 갱신용 타이머
        /// </summary>
        private DispatcherTimer cTactTimeTimer = null;

        private bool disposed;

        ~TaktTimeUI()
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

        public TaktTimeUI()
        {
            InitializeComponent();
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                if (cTactTimeTimer == null)
                {
                    cTactTimeTimer = new DispatcherTimer();
                    cTactTimeTimer.Interval = TimeSpan.FromMilliseconds(100);   // 시간 간격 설정
                    cTactTimeTimer.Tick += new EventHandler(DataTimer_Tick);    // 이벤트 추가
                }
                cTactTimeTimer.Start(); // 타이머 시작
            }
            else
            {
                cTactTimeTimer.Stop();
            }
        }

        /// <summary>
        /// 데이터 갱신용 반복 타이머
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataTimer_Tick(object sender, EventArgs e)
        {
            if (tbTaktTime1.Text != CMainLib.Ins.cVar.iPipeCycleTime.ToString())
                tbTaktTime1.Text = CMainLib.Ins.cVar.iPipeCycleTime.ToString();

            if (tbTaktTime2.Text != CMainLib.Ins.cVar.iNeedleCycleTime.ToString())
                tbTaktTime2.Text = CMainLib.Ins.cVar.iNeedleCycleTime.ToString();
        }

        /// <summary>
        /// 창을 Drag 하기 위한 간단한 함수가 있지만 그 함수를 쓰면 프로그램 먹통 증상이
        /// 있어 기능을 풀어놓았다. MS 자체 버그가 있는 걸로 검색된다.
        /// </summary>
        private System.Drawing.Point _windowMoveMouseStart;

        private double _windowMoveStartTop;
        private double _windowMoveStartLeft;

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UIElement handle = sender as UIElement;
            if (handle == null)
                return;

            _windowMoveMouseStart = System.Windows.Forms.Control.MousePosition;
            _windowMoveStartTop = this.Top;
            _windowMoveStartLeft = this.Left;

            handle.MouseMove += Border_MouseMove;
            handle.CaptureMouse();
        }

        private void BtClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Border_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
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