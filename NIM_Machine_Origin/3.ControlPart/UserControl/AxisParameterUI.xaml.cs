using System;
using System.Windows.Controls;
using System.Windows.Media;
using static EMotionSnetBase.SnetDevice;

namespace MachineControlBase
{
    /// <summary>
    /// AxisInfo.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AxisParameterUI : UserControl
    {
        /// <summary>
        /// 축 파라미터 데이터
        /// </summary>
        private AxisParam axisParam;

        /// <summary>
        /// 축 선택 시 색상 변경
        /// </summary>
        private bool bSelected = false;

        public bool Selected
        {
            get { return bSelected; }
            set
            {
                bSelected = value;
                MainFrame.Background = value ? new BrushConverter().ConvertFromString("#007ACC") as SolidColorBrush : Brushes.Transparent;
            }
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public AxisParameterUI()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="axisPraram"></param>
        public void Init(AxisParam axisParam)
        {
            this.axisParam = axisParam;
            ParamToControls();
        }

        public delegate void buttonActionHandler(int nAxis);

        public buttonActionHandler ActionButton;

        /// <summary>
        /// 파라미터 저장
        /// </summary>
        public void ControlsToParam()
        {
            axisParam.uiHomeVel = Convert.ToUInt32(HomeVel.Text);
            axisParam.uiHomeAcc = Convert.ToUInt32(HomeAcc.Text);
            axisParam.dHomeOffset = Convert.ToDouble(HomeOffset.Text);
            axisParam.uiHomeTimeout = Convert.ToUInt32(HomeTimeOut.Text);
            axisParam.uiHomeClearTime = Convert.ToUInt32(HomeClearTime.Text);

            switch (HomeUseZPhase.SelectedIndex)
            {
                case 0: axisParam.bHomeUseZPhase = true; break;
                case 1: axisParam.bHomeUseZPhase = false; break;
            }

            switch (HomeDirection.SelectedIndex)
            {
                case 0: axisParam.iHomeMoveDirection = (int)eSnetMoveDirection.Negative; break;
                case 1: axisParam.iHomeMoveDirection = (int)eSnetMoveDirection.Positive; break;
            }

            switch (HomeSenType.SelectedIndex)
            {
                case 0: axisParam.iHomeSenserType = (int)eSnetAxisSensor.LimitMinus; break;
                case 1: axisParam.iHomeSenserType = (int)eSnetAxisSensor.LimitPlus; break;
                case 2: axisParam.iHomeSenserType = (int)eSnetAxisSensor.HomeSensor; break;
                case 3: axisParam.iHomeSenserType = (int)eSnetAxisSensor.ZPhase; break;
            }
            axisParam.dScale = Convert.ToDouble(Scale.Text);
            axisParam.uiJogVel = Convert.ToUInt32(JVel.Text);
            axisParam.uiVel = Convert.ToUInt32(Vel.Text);
            axisParam.uiAccel = Convert.ToUInt32(Acc.Text);
            axisParam.uiDecel = Convert.ToUInt32(Dec.Text);
            axisParam.uiMotionTimeout = Convert.ToUInt32(MotionTimeOut.Text);
        }

        /// <summary>
        /// 파라미터 Load
        /// </summary>
        private void ParamToControls()
        {
            AxisName.Content = axisParam.eAxis;
            HomeVel.Text = axisParam.uiHomeVel.ToString();
            HomeAcc.Text = axisParam.uiHomeAcc.ToString();
            HomeOffset.Text = axisParam.dHomeOffset.ToString();
            HomeTimeOut.Text = axisParam.uiHomeTimeout.ToString();
            HomeClearTime.Text = axisParam.uiHomeClearTime.ToString();
            if (axisParam.bHomeUseZPhase == true)
            {
                HomeUseZPhase.SelectedIndex = 0;
            }
            else
            {
                HomeUseZPhase.SelectedIndex = 1;
            }
            switch (axisParam.iHomeMoveDirection)
            {
                case (int)eSnetMoveDirection.Negative: HomeDirection.SelectedIndex = 0; break;
                case (int)eSnetMoveDirection.Positive: HomeDirection.SelectedIndex = 1; break;
            }
            switch (axisParam.iHomeSenserType)
            {
                case (int)eSnetAxisSensor.LimitMinus: HomeSenType.SelectedIndex = 0; break;
                case (int)eSnetAxisSensor.LimitPlus: HomeSenType.SelectedIndex = 1; break;
                case (int)eSnetAxisSensor.HomeSensor: HomeSenType.SelectedIndex = 2; break;
                case (int)eSnetAxisSensor.ZPhase: HomeSenType.SelectedIndex = 3; break;
            }
            Scale.Text = axisParam.dScale.ToString();
            JVel.Text = axisParam.uiJogVel.ToString();
            Vel.Text = axisParam.uiVel.ToString();
            Acc.Text = axisParam.uiAccel.ToString();
            Dec.Text = axisParam.uiDecel.ToString();
            MotionTimeOut.Text = axisParam.uiMotionTimeout.ToString();
        }

        /// <summary>
        /// 이름 클릭 시 강조 표시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AxisName_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ActionButton != null) ActionButton((int)axisParam.eAxis);
        }
    }
}