using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// IOMonitorMiniUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class IOMonitorMiniUI : UserControl
    {
        /// <summary>
        /// Data 갱신용 타이머
        /// </summary>
        private DispatcherTimer cDataTimer = null;

        /// <summary>
        /// Input Mini UI List
        /// </summary>
        private List<IOSingleMiniUI> cInputSingleUI = new List<IOSingleMiniUI>();

        /// <summary>
        /// Output Mini UI List
        /// </summary>
        private List<IOSingleMiniUI> cOutputSingleUI = new List<IOSingleMiniUI>();

        /// <summary>
        /// Input이 현재 보여주는 페이지
        /// </summary>
        private int iInputPageNo = 0;

        /// <summary>
        /// Output이 현재 보여주는 페이지
        /// </summary>
        private int iOutputPageNo = 0;

        /// <summary>
        /// IO Monitor UI 생성자
        /// </summary>
        public IOMonitorMiniUI()
        {
            InitializeComponent();

            // Input List 저장
            cInputSingleUI.Add(IOSinglePanel1);
            cInputSingleUI.Add(IOSinglePanel2);
            cInputSingleUI.Add(IOSinglePanel3);
            cInputSingleUI.Add(IOSinglePanel4);
            cInputSingleUI.Add(IOSinglePanel5);
            cInputSingleUI.Add(IOSinglePanel6);
            cInputSingleUI.Add(IOSinglePanel7);
            cInputSingleUI.Add(IOSinglePanel8);
            cInputSingleUI.Add(IOSinglePanel9);
            cInputSingleUI.Add(IOSinglePanel10);
            cInputSingleUI.Add(IOSinglePanel11);
            cInputSingleUI.Add(IOSinglePanel12);
            cInputSingleUI.Add(IOSinglePanel13);
            cInputSingleUI.Add(IOSinglePanel14);
            cInputSingleUI.Add(IOSinglePanel15);
            cInputSingleUI.Add(IOSinglePanel16);
            // Output List 저장
            cOutputSingleUI.Add(IOSinglePanel17);
            cOutputSingleUI.Add(IOSinglePanel18);
            cOutputSingleUI.Add(IOSinglePanel19);
            cOutputSingleUI.Add(IOSinglePanel20);
            cOutputSingleUI.Add(IOSinglePanel21);
            cOutputSingleUI.Add(IOSinglePanel22);
            cOutputSingleUI.Add(IOSinglePanel23);
            cOutputSingleUI.Add(IOSinglePanel24);
            cOutputSingleUI.Add(IOSinglePanel25);
            cOutputSingleUI.Add(IOSinglePanel26);
            cOutputSingleUI.Add(IOSinglePanel27);
            cOutputSingleUI.Add(IOSinglePanel28);
            cOutputSingleUI.Add(IOSinglePanel29);
            cOutputSingleUI.Add(IOSinglePanel30);
            cOutputSingleUI.Add(IOSinglePanel31);
            cOutputSingleUI.Add(IOSinglePanel32);

            // Input 초기값 세팅
            InputDataSet();
            // Output 초기값 세팅
            OutputDataSet();
        }

        /// <summary>
        /// Input Data를 갱신한다
        /// </summary>
        private void InputDataSet()
        {
            foreach (IOSingleMiniUI cIOSingleUI in cInputSingleUI)
            {
                cIOSingleUI._iAddress = cIOSingleUI._iIndex + iInputPageNo * 16;
                string strHex = string.Format("{0:X2}", cIOSingleUI._iAddress);
                cIOSingleUI._strHexAddress = strHex;
                cIOSingleUI._strIOName = CMainLib.Ins.Seq.SeqIO.GetInputName(cIOSingleUI._iAddress);
            }
        }

        /// <summary>
        /// Output Data를 갱신한다
        /// </summary>
        private void OutputDataSet()
        {
            foreach (IOSingleMiniUI cIOSingleUI in cOutputSingleUI)
            {
                cIOSingleUI._iAddress = cIOSingleUI._iIndex + iOutputPageNo * 16;
                string strHex = string.Format("{0:X2}", cIOSingleUI._iAddress);
                cIOSingleUI._strHexAddress = strHex;
                cIOSingleUI._strIOName = CMainLib.Ins.Seq.SeqIO.GetOutputName(cIOSingleUI._iAddress);
            }
        }

        /// <summary>
        /// Input Up 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtInputUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            int MaxInputIndex = Define.INPUT_TOTAL_BIT / Define.INPUT_DEFINE_BIT;

            if (iInputPageNo < MaxInputIndex - 1) iInputPageNo++;
            else return;
            InputDataSet();
        }

        /// <summary>
        /// Input Down 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtInputDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (iInputPageNo > 0) iInputPageNo--;
            else return;
            InputDataSet();
        }

        /// <summary>
        /// Output Up 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtOutputUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            int MaxOutputIndex = Define.OUTPUT_TOTAL_BIT / Define.OUTPUT_DEFINE_BIT;

            if (iOutputPageNo < MaxOutputIndex - 1) iOutputPageNo++;
            else return;
            OutputDataSet();
        }

        /// <summary>
        /// Output Down 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtOutputDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (iOutputPageNo > 0) iOutputPageNo--;
            else return;
            OutputDataSet();
        }

        /// <summary>
        /// 타이머 시작
        /// </summary>
        public void StartTimer()
        {
            if (cDataTimer == null)
            {
                cDataTimer = new DispatcherTimer();
                cDataTimer.Interval = TimeSpan.FromMilliseconds(100);   // 시간 간격 설정
                cDataTimer.Tick += new EventHandler(DataTimer_Tick);    // 이벤트 추가
            }
            cDataTimer.Start(); // 타이머 시작
        }

        /// <summary>
        /// 타이머 정지
        /// </summary>
        public void StopTimer()
        {
            if (cDataTimer.IsEnabled == true) cDataTimer.Stop();
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
                foreach (IOSingleMiniUI cIOsingUI in cInputSingleUI)
                {
                    cIOsingUI.RepeatUpdateTimer();
                }

                foreach (IOSingleMiniUI cIOsingUI in cOutputSingleUI)
                {
                    cIOsingUI.RepeatUpdateTimer();
                }
            });
        }
    }
}