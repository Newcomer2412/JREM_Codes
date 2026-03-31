using System.Windows.Controls;

namespace MachineControlBase
{
    /// <summary>
    /// UserStrInputUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UserStrInputUI : UserControl
    {
        public UserStrInputUI()
        {
            InitializeComponent();
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
        /// Data 설정
        /// </summary>
        public string _strData
        {
            get { return TbData.Text; }
            set
            {
                TbData.Text = value;
            }
        }

        /// <summary>
        /// 숫자 입력 클래스
        /// </summary>
        //NumPadUI m_NumPadUI = null;

        /// <summary>
        /// 데이터 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Data_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //m_NumPadUI = new NumPadUI();

            //if (m_NumPadUI.bOpened == false)
            //{
            //    m_NumPadUI.Init(sender as TextBlock);
            //    m_NumPadUI.ShowDialog();
            //}
            //else m_NumPadUI.Close();
        }

        /// <summary>
        /// Int Data 설정
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
        /// Double Data 설정
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
        /// Int Data 설정
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
        /// Int Data 설정
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
        /// Double Data 설정
        /// </summary>
        /// <param name="dData"></param>
        public void GetData(ref double dData)
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
            if (dUIData != dData)
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