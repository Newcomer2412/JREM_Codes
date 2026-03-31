using System.Windows.Controls;

namespace MachineControlBase
{
    /// <summary>
    /// UserNumInputUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UserNumInputUI : UserControl
    {
        public UserNumInputUI()
        {
            InitializeComponent();
            if (strToolTipName == string.Empty) ToolTipService.SetIsEnabled(TbInputName, false);
        }

        /// <summary>
        /// ToolTip Name
        /// </summary>
        private string strToolTipName = string.Empty;

        /// <summary>
        /// ToolTip Name 설정
        /// </summary>
        public string _strToolTipName
        {
            get { return strToolTipName; }
            set
            {
                strToolTipName = value;
                ToolTipService.SetIsEnabled(TbInputName, true);
                ToolTipName.Text = value;
            }
        }

        /// <summary>
        /// ToolTip Data
        /// </summary>
        private string strToolTipData = string.Empty;

        /// <summary>
        /// ToolTip Data 설정
        /// </summary>
        public string _strToolTipData
        {
            get { return strToolTipData; }
            set
            {
                strToolTipData = value;
                ToolTipData.Text = value;
            }
        }

        /// <summary>
        /// name
        /// </summary>
        private string strName = string.Empty;

        /// <summary>
        /// Name 설정
        /// </summary>
        public string _strName
        {
            get { return strName; }
            set
            {
                strName = value;
                TbInputName.Text = value;
            }
        }

        /// <summary>
        /// Data
        /// </summary>
        private string strData = string.Empty;

        /// <summary>
        /// Data 설정
        /// </summary>
        public string _strData
        {
            get { return TbData.Text; }
            set
            {
                strData = value;
                TbData.Text = value;
            }
        }

        /// <summary>
        /// 숫자 입력 클래스
        /// </summary>
        private NumPadUI m_NumPadUI = null;

        /// <summary>
        /// 데이터 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Data_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            m_NumPadUI = new NumPadUI();

            if (m_NumPadUI.bOpened == false)
            {
                m_NumPadUI.strGetNamee = TbInputName.Text;
                m_NumPadUI.Init(TbData);
                m_NumPadUI.ShowDialog();
            }
            else m_NumPadUI.Close();
        }

        /// <summary>
        /// int Data 설정
        /// </summary>
        /// <param name="iData"></param>
        public void SetData(int iData)
        {
            _strData = iData.ToString();
        }

        /// <summary>
        /// uint Data 설정
        /// </summary>
        /// <param name="uiData"></param>
        public void SetData(uint uiData)
        {
            _strData = uiData.ToString();
        }

        /// <summary>
        /// short Data 설정
        /// </summary>
        /// <param name="sData"></param>
        public void SetData(short sData)
        {
            _strData = sData.ToString();
        }

        /// <summary>
        /// ushort Data 설정
        /// </summary>
        /// <param name="usData"></param>
        public void SetData(ushort usData)
        {
            _strData = usData.ToString();
        }

        /// <summary>
        /// double Data 설정
        /// </summary>
        /// <param name="dData"></param>
        public void SetData(double dData)
        {
            _strData = dData.ToString();
        }

        /// <summary>
        /// string Data 설정
        /// </summary>
        /// <param name="strData"></param>
        public void SetData(string strData)
        {
            _strData = strData;
        }

        /// <summary>
        /// int Data 설정
        /// </summary>
        /// <param name="iData"></param>
        public void GetData(ref int iData)
        {
            int iUIData = 0;
            string strMessage = string.Empty;

            if (int.TryParse(_strData, out iUIData) == false)
            {
                strMessage = string.Format("Data Error >> {0} : {1}", _strName, _strData);
                CCommon.ShowMessageMini(strMessage);
                return;
            }

            // 변경 Log 기록
            if (iUIData != iData)
            {
                strMessage = string.Format("[Data Save] {0} : {1} -> {2}", _strName, iData, _strData);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, strMessage);
            }

            iData = iUIData;
        }

        /// <summary>
        /// uint Data 설정
        /// </summary>
        /// <param name="uiData"></param>
        public void GetData(ref uint uiData)
        {
            uint uiUIData = 0;
            string strMessage = string.Empty;

            if (uint.TryParse(_strData, out uiUIData) == false)
            {
                strMessage = string.Format("Data Error >> {0} : {1}", _strName, _strData);
                CCommon.ShowMessageMini(strMessage);
                return;
            }

            // 변경 Log 기록
            if (uiUIData != uiData)
            {
                strMessage = string.Format("[Data Save] {0} : {1} -> {2}", _strName, uiData, _strData);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, strMessage);
            }

            uiData = uiUIData;
        }

        /// <summary>
        /// short Data 설정
        /// </summary>
        /// <param name="sData"></param>
        public void GetData(ref short sData)
        {
            short sUIData = 0;
            string strMessage = string.Empty;

            if (short.TryParse(_strData, out sUIData) == false)
            {
                strMessage = string.Format("Data Error >> {0} : {1}", _strName, _strData);
                CCommon.ShowMessageMini(strMessage);
                return;
            }

            // 변경 Log 기록
            if (sUIData != sData)
            {
                strMessage = string.Format("[Data Save] {0} : {1} -> {2}", _strName, sData, _strData);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, strMessage);
            }

            sData = sUIData;
        }

        /// <summary>
        /// ushort Data 설정
        /// </summary>
        /// <param name="usData"></param>
        public void GetData(ref ushort usData)
        {
            ushort usUIData = 0;
            string strMessage = string.Empty;

            if (ushort.TryParse(_strData, out usUIData) == false)
            {
                strMessage = string.Format("Data Error >> {0} : {1}", _strName, _strData);
                CCommon.ShowMessageMini(strMessage);
                return;
            }

            // 변경 Log 기록
            if (usUIData != usData)
            {
                strMessage = string.Format("[Data Save] {0} : {1} -> {2}", _strName, usData, _strData);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, strMessage);
            }

            usData = usUIData;
        }

        /// <summary>
        /// double Data 설정
        /// </summary>
        /// <param name="dData"></param>
        /// <param name="bNotLog"></param>
        public void GetData(ref double dData, bool bNotLog = false)
        {
            double dUIData = 0;
            string strMessage = string.Empty;

            if (double.TryParse(_strData, out dUIData) == false)
            {
                strMessage = string.Format("Data Error >> {0} : {1}", _strName, _strData);
                CCommon.ShowMessageMini(strMessage);
                return;
            }

            // 변경 Log 기록
            if (bNotLog == false && dUIData != dData)
            {
                strMessage = string.Format("[Data Save] {0} : {1} -> {2}", _strName, dData, _strData);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, strMessage);
            }

            dData = dUIData;
        }

        /// <summary>
        /// string Data 설정
        /// </summary>
        /// <param name="strData"></param>
        public void GetData(ref string strData)
        {
            string strMessage = string.Empty;

            // 변경 Log 기록
            if (_strData != strData)
            {
                strMessage = string.Format("[Data Save] {0} : {1} -> {2}", _strName, strData, _strData);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, strMessage);
            }

            strData = _strData;
        }
    }
}