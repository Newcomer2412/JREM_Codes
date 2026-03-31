using System.Windows.Controls;
using System.Windows.Media;

namespace MachineControlBase
{
    /// <summary>
    /// IOUserUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class IOUserUI : UserControl
    {
        /// <summary>
        /// 초기화
        /// </summary>
        public void Init()
        {
            string strGetData = string.Empty;
            if (_bInput == true) strGetData = CMainLib.Ins.Seq.SeqIO.GetInputName(iAddress);
            else strGetData = CMainLib.Ins.Seq.SeqIO.GetOutputName(iAddress);
            _strIOName = strGetData;
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public IOUserUI()
        {
            InitializeComponent();
        }

        #region Property 설정

        /// <summary>
        ///  I/O Type
        /// </summary>
        private bool bInput = true;

        /// <summary>
        /// Type 설정
        /// True - Input, False - Output
        /// </summary>
        public bool _bInput
        {
            get { return bInput; }
            set
            {
                bInput = value;
                if (bInput == true)
                {
                    IOType.Text = "I";
                    SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.CadetBlue);
                    IOTypeBackground.Background = solidColorBrush;
                    BTSet.IsEnabled = false;
                }
                else
                {
                    IOType.Text = "O";
                    SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.PaleVioletRed);
                    IOTypeBackground.Background = solidColorBrush;
                    BTSet.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// I/O name
        /// </summary>
        private string strIOName = string.Empty;

        /// <summary>
        /// IO Name 설정
        /// </summary>
        public string _strIOName
        {
            get { return strIOName; }
            set
            {
                strIOName = value;
                IOName.Text = value;
            }
        }

        /// <summary>
        /// Address
        /// </summary>
        private int iAddress = 0;

        /// <summary>
        /// IO Address 설정
        /// </summary>
        public int _iAddress
        {
            get
            {
                return iAddress;
            }
            set
            {
                iAddress = value;
                IOAddress.Text = iAddress.ToString();
            }
        }

        #endregion Property 설정

        /// <summary>
        /// 이전 상태
        /// </summary>
        private bool bOldStatus = false;

        /// <summary>
        /// IO 버튼을 클릭(입력이면 무시한다)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BTSet_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CMainLib.Ins.McState == eMachineState.RUN || CMainLib.Ins.McState == eMachineState.MANUALRUN) return;
            if (bInput == false)
            {
                if (bOldStatus == true) CMainLib.Ins.Seq.SeqIO.SetOutput(iAddress, false);
                else CMainLib.Ins.Seq.SeqIO.SetOutput(iAddress, true);
            }
        }

        /// <summary>
        /// 반복 갱신 함수
        /// </summary>
        public void RepeatUpdateTimer()
        {
            bool bRtn = false;

            if (bInput == true)
            {
                bRtn = CMainLib.Ins.Seq.SeqIO.GetInput(iAddress);
                if (bRtn != bOldStatus)
                {
                    SolidColorBrush solidColorBrush;
                    if (bRtn == true) solidColorBrush = new SolidColorBrush(Colors.CadetBlue);
                    else solidColorBrush = new SolidColorBrush(Colors.Black);
                    IOOnOffBackground.Background = solidColorBrush;
                }
            }
            else
            {
                bRtn = CMainLib.Ins.Seq.SeqIO.GetOutput(iAddress);
                if (bRtn != bOldStatus)
                {
                    SolidColorBrush solidColorBrush;
                    if (bRtn == true) solidColorBrush = new SolidColorBrush(Colors.PaleVioletRed);
                    else solidColorBrush = new SolidColorBrush(Colors.Black);
                    IOOnOffBackground.Background = solidColorBrush;
                }
            }
            bOldStatus = bRtn;
        }
    }
}