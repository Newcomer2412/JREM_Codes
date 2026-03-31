using System.Windows.Controls;
using System.Windows.Media;

namespace MachineControlBase
{
    /// <summary>
    /// IOSingleUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class IOSingleMiniUI : UserControl
    {
        public IOSingleMiniUI()
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
                if (bInput)
                {
                    SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.CadetBlue);
                    IOTypeBackground.Background = solidColorBrush;
                    SetIO.IsEnabled = false;
                }
                else
                {
                    SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.PaleVioletRed);
                    IOTypeBackground.Background = solidColorBrush;
                    SetIO.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// IO Index
        /// </summary>
        private int iIndex = 0;

        /// <summary>
        /// IO Index 설정
        /// </summary>
        public int _iIndex
        {
            get
            {
                return iIndex;
            }
            set
            {
                iIndex = value;
            }
        }

        /// <summary>
        /// IO Address
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
                IOAddress.Text = value.ToString();
            }
        }

        /// <summary>
        /// IO Hex Address
        /// </summary>
        private string strHexAddress = string.Empty;

        /// <summary>
        /// IO Offset Address 설정
        /// </summary>
        public string _strHexAddress
        {
            get
            {
                return strHexAddress;
            }
            set
            {
                strHexAddress = value;
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
        /// I/O On/Off
        /// </summary>
        private bool bIOOnOff = false;

        /// <summary>
        /// I/O On/Off 설정
        /// </summary>
        public bool _bIOOnOff
        {
            get { return bIOOnOff; }
            set
            {
                bIOOnOff = value;
                if (bIOOnOff == true)
                {
                    if (bInput == true)
                    {
                        SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.CadetBlue);
                        IOSignalBackground.Background = solidColorBrush;
                    }
                    else
                    {
                        SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.PaleVioletRed);
                        IOSignalBackground.Background = solidColorBrush;
                    }
                }
                else
                {
                    if (bInput == true)
                    {
                        SolidColorBrush solidColorBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF303030"));
                        IOSignalBackground.Background = solidColorBrush;
                    }
                    else
                    {
                        SolidColorBrush solidColorBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF303030"));
                        IOSignalBackground.Background = solidColorBrush;
                    }
                }
            }
        }

        #endregion Property 설정

        /// <summary>
        /// 출력 버튼을 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetIO_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_bIOOnOff == true) CMainLib.Ins.Seq.SeqIO.SetOutput(_iAddress, false, false);
            else CMainLib.Ins.Seq.SeqIO.SetOutput(_iAddress, true, false);
        }

        /// <summary>
        /// 이전 상태
        /// </summary>
        private bool bOldStatus = false;

        /// <summary>
        /// 반복 갱신 함수
        /// </summary>
        public void RepeatUpdateTimer()
        {
            if (bInput == true)
            {
                bool bGetInput = CMainLib.Ins.Seq.SeqIO.GetInput(_iAddress, false);
                if (bOldStatus != bGetInput)
                {
                    _bIOOnOff = bGetInput;
                    bOldStatus = bGetInput;
                }
            }
            else
            {
                bool bGetOutput = CMainLib.Ins.Seq.SeqIO.GetOutput(_iAddress, false);
                if (bOldStatus != bGetOutput)
                {
                    _bIOOnOff = bGetOutput;
                    bOldStatus = bGetOutput;
                }
            }
        }
    }
}