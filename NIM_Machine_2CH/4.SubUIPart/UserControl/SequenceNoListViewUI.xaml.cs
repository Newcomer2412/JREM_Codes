using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// SequenceNoListViewUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SequenceNoListViewUI : Window
    {
        public SequenceNoListViewUI()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Main Lib
        /// </summary>
        private CMainLib ml;

        /// <summary>
        /// Data 갱신용 타이머
        /// </summary>
        private DispatcherTimer cDataTimer = null;

        /// <summary>
        /// 시퀀스 별 No 표시 UserControl
        /// </summary>
        private SequenceNoViewUI[] sequenceNoViewUI = null;

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

        /// <summary>
        // UI 초기화
        /// </summary>
        public void Init()
        {
            ml = CMainLib.Ins;

            bOpened = true;
            int iSeqCount = ml.Seq.Worker.Count;
            foreach (KeyValuePair<eSequence, ISequence> item in ml.Seq.Worker)
            {
                if (((ISequence)item.Value.Ins).SubWorker != null)
                {
                    iSeqCount += ((ISequence)item.Value.Ins).SubWorker.Count;
                }
            }

            sequenceNoViewUI = new SequenceNoViewUI[iSeqCount];
            int iUICount = 0;
            for (int i = 0; i < ml.Seq.Worker.Count; i++)
            {
                sequenceNoViewUI[iUICount] = new SequenceNoViewUI();
                sequenceNoViewUI[iUICount].Init((ISeqNo)ml.Seq.Worker[(eSequence)i].Ins);
                Sequence_Panel.Children.Add(sequenceNoViewUI[iUICount++]);
                if (ml.Seq.Worker[(eSequence)i].SubWorker != null)
                {
                    for (int j = 0; j < ((ISequence)ml.Seq.Worker[(eSequence)i].Ins).SubWorker.Count; j++)
                    {
                        sequenceNoViewUI[iUICount] = new SequenceNoViewUI();
                        sequenceNoViewUI[iUICount].Init((ISeqNo)((ISequence)ml.Seq.Worker[(eSequence)i].Ins).SubWorker[j].Ins);
                        Sequence_Panel.Children.Add(sequenceNoViewUI[iUICount++]);
                    }
                }
            }

            if (cDataTimer == null)
            {
                cDataTimer = new DispatcherTimer();
                cDataTimer.Interval = TimeSpan.FromMilliseconds(10);   // 시간 간격 설정
                cDataTimer.Tick += new EventHandler(DataTimer_Tick);    // 이벤트 추가
            }
            cDataTimer.Start(); // 타이머 시작
        }

        /// <summary>
        /// 데이터 갱신용 반복 타이머
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataTimer_Tick(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate ()
            {
                for (int i = 0; i < sequenceNoViewUI.Length; i++)
                {
                    sequenceNoViewUI[i].DataTimer_Tick();
                }
            });
        }

        /// <summary>
        /// 창을 Drag 하기 위한 기능 간단한 함수가 있지만 그 함수를 쓰면 프로그램 먹통 증상이
        /// 있어 기능을 풀어놓았다.
        /// </summary>
        private System.Drawing.Point _windowMoveMouseStart;

        private double _windowMoveStartTop;
        private double _windowMoveStartLeft;

        /// <summary>
        /// 창 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 창 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            bOpened = false;
            if (cDataTimer.IsEnabled == true) cDataTimer.Stop();
            Close();
        }
    }
}