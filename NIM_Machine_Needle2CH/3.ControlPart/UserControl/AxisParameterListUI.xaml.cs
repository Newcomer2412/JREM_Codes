using System.Windows;

namespace MachineControlBase
{
    /// <summary>
    /// AxisParameterListUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AxisParameterListUI : Window
    {
        /// <summary>
        /// 축 파라미터 설정 UI
        /// </summary>
        AxisParameterUI[] cAxisParameterUI;

        /// <summary>
        /// 생성자
        /// </summary>
        public AxisParameterListUI()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Init()
        {
            cAxisParameterUI = new AxisParameterUI[CMainLib.Ins.cAxisParamCollData.cAxisPramList.Count];
            for (int idx = 0; idx < cAxisParameterUI.Length; idx++)
            {
                AxisParam axisParam = CMainLib.Ins.cAxisParamCollData.GetAxisParam((eMotor)idx);
                cAxisParameterUI[idx] = new AxisParameterUI();
                cAxisParameterUI[idx].Init(axisParam);
                cAxisParameterUI[idx].ActionButton = AxisControlEvent;
                Panel_AxisParam.Children.Add(cAxisParameterUI[idx]);
            }
        }

        /// <summary>
        /// 각 축에서 이름 클릭 이벤트
        /// </summary>
        /// <param name="nAxis"></param>
        void AxisControlEvent(int nAxis)
        {
            for (int i = 0; i < cAxisParameterUI.Length; i++)
                cAxisParameterUI[i].Selected = false;
            cAxisParameterUI[nAxis].Selected = true;
        }

        private void btnSaveParam_Click(object sender, RoutedEventArgs e)
        {
            for (int idx = 0; idx < cAxisParameterUI.Length; idx++)
            {
                cAxisParameterUI[idx].ControlsToParam();
            }
            CXMLProcess.WriteXml(CXMLProcess.AxisParameterCollectionDataFilePath, CMainLib.Ins.cAxisParamCollData);
            CCommon.ShowMessageMini("SAVE");
        }

        /// <summary>
        /// Close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 창 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
