using System.Windows.Controls;
using System.Windows.Input;

namespace MachineControlBase
{
    /// <summary>
    /// AxisPositionUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AxisPositionUI : UserControl, IUIEditData
    {
        /// <summary>
        /// Main Lib
        /// </summary>
        private CMainLib ml;

        public AxisPositionUI()
        {
            InitializeComponent();
            ml = CMainLib.Ins;
        }

        #region Property 설정

        /// <summary>
        /// AxisNo
        /// </summary>
        private eMotor eAxis = eMotor.MOT_NONE;

        /// <summary>
        /// Axis No 설정
        /// </summary>
        public eMotor _eAxis
        {
            get { return eAxis; }
            set { eAxis = value; }
        }

        /// <summary>
        /// Move No
        /// </summary>
        private int iMoveNo = -1;

        /// <summary>
        /// Axis Move No 설정
        /// </summary>
        public int _iMoveNo
        {
            get { return iMoveNo; }
            set
            {
                iMoveNo = value;
                //TBMoveNo.Text = value.ToString();
            }
        }

        /// <summary>
        ///  Axis Move Type
        /// </summary>
        private bool bMoveType = true;

        /// <summary>
        /// Axis Move type 설정
        /// True - ABS, False - INC
        /// </summary>
        public bool _bMoveType_Abs
        {
            get { return bMoveType; }
            set
            {
                bMoveType = value;
                if (bMoveType == true)
                {
                    TBMoveType.Text = "ABS";
                }
                else
                {
                    TBMoveType.Text = "INC";
                }
            }
        }

        /// <summary>
        /// Move name
        /// </summary>
        private string strMoveName = string.Empty;

        /// <summary>
        /// Move Name 설정
        /// </summary>
        public string _strMoveName
        {
            get { return strMoveName; }
            set
            {
                strMoveName = value;
                TBName.Text = value;
            }
        }

        /// <summary>
        /// Move Pos
        /// </summary>
        private double dMovePos = 0;

        /// <summary>
        /// Move Pos 설정
        /// </summary>
        public double _dMovePos
        {
            get
            {
                return dMovePos;
            }
            set
            {
                dMovePos = value;
                TBPos.Text = string.Format("{0:0.###}", value);
            }
        }

        /// <summary>
        /// Move Vel
        /// </summary>
        private uint uiMoveVel = 100;

        /// <summary>
        /// Move Vel 설정
        /// </summary>
        public uint _uiMoveVel
        {
            get { return uiMoveVel; }
            set
            {
                if (value > 100) value = 100;
                else if (value < 0) value = 0;
                uiMoveVel = value;
                TBVel.Text = value.ToString();
            }
        }

        /// <summary>
        /// 이송 단위
        /// </summary>
        private int iPosUnit = 0;

        /// <summary>
        /// 이송 단위 설정
        /// </summary>
        public int _iPosUnit
        {
            get { return iPosUnit; }
            set
            {
                iPosUnit = value;
                if (value == 0) TBUnit.Text = "um";
                else if (value == 1) TBUnit.Text = "mm";
                else if (value == 2) TBUnit.Text = "°";
                else if (value == 3) TBUnit.Text = "ml";
            }
        }

        /// <summary>
        /// 숫자 입력 클래스
        /// </summary>
        private NumPadUI m_NumPadUI = null;

        /// <summary>
        /// 이전 Pos 데이터
        /// </summary>
        private string strOldPos = string.Empty;

        /// <summary>
        /// 이전 Vel 데이터
        /// </summary>
        private string strOldVel = string.Empty;

        /// <summary>
        /// 초기화
        /// </summary>
        /// <returns></returns>
        public void Init()
        {
        }

        /// <summary>
        /// 위치 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TBPos_MouseDown(object sender, MouseButtonEventArgs e)
        {
            m_NumPadUI = new NumPadUI();

            if (m_NumPadUI.bOpened == false)
            {
                m_NumPadUI.Init(sender as TextBlock, eAxis);
                m_NumPadUI.ShowDialog();
                if (m_NumPadUI.bCancel == true) return;

                double dGetData = 0.0;
                if (double.TryParse((sender as TextBlock).Text, out dGetData) == true)
                {
                    _dMovePos = dGetData;
                }
            }
            else m_NumPadUI.Close();
        }

        /// <summary>
        /// 속도 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TBVel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            m_NumPadUI = new NumPadUI();

            if (m_NumPadUI.bOpened == false)
            {
                m_NumPadUI.Init(sender as TextBlock);
                m_NumPadUI.ShowDialog();
                if (m_NumPadUI.bCancel == true) return;

                uint uiGetData = 0;
                if (uint.TryParse((sender as TextBlock).Text, out uiGetData) == false) return;
                _uiMoveVel = uiGetData;
            }
            else m_NumPadUI.Close();
        }

        #endregion Property 설정

        /// <summary>
        /// 설정 된 위치로 이동하는 기능
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TBName_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (eAxis == eMotor.MOT_NONE) return;

            string strCaption = strMoveName + " 이송합니다.";

            if (CCommon.ShowMessageMini(1, strCaption) == (int)eMBoxRtn.A_OK)
            {
                double targetPos = 0;
                uint velPer = 0;
                if (double.TryParse(TBPos.Text, out targetPos) == false)
                {
                    CCommon.ShowMessageMini("위치값이 올바르지 않습니다.");
                    return;
                }
                if (uint.TryParse(TBVel.Text, out velPer) == false)
                {
                    CCommon.ShowMessageMini("속도 비율이 올바르지 않습니다..");
                    return;
                }
                if (velPer <= 0 || velPer > 100)
                {
                    CCommon.ShowMessageMini("속도 비율이 올바르지 않습니다..");
                    return;
                }

                if (bMoveType == true)
                {
                    ml.Axis[eAxis].MoveAbsolute(targetPos, velPer);
                }
                else
                {
                    ml.Axis[eAxis].MoveRelative(targetPos, velPer);
                }
            }
        }

        /// <summary>
        /// 데이터를 불러온다.
        /// </summary>
        public void LoadData()
        {
            PositionData cPositionData = ml.cAxisPosCollData.GetPositionData(eAxis, _iMoveNo);
            _dMovePos = cPositionData.dPos;
            _uiMoveVel = cPositionData.uiSpeed;
            strOldPos = cPositionData.dPos.ToString();
            strOldVel = cPositionData.uiSpeed.ToString();
        }

        /// <summary>
        /// 데이터를 저장한다.
        /// </summary>
        public void SaveData()
        {
            string strMessage = string.Empty;
            if (double.TryParse(TBPos.Text, out dMovePos) == false)
            {
                strMessage = string.Format("{0} Axis {1} Index Pos Data Error.", eAxis, _iMoveNo);
                CCommon.ShowMessageMini(strMessage);
            }
            if (uint.TryParse(TBVel.Text, out uiMoveVel) == false)
            {
                strMessage = string.Format("{0} Axis {1} Index Axis Dec Data Error.", eAxis, _iMoveNo);
                CCommon.ShowMessageMini(strMessage);
            }

            // 변경 Log 기록
            if (strOldPos != TBPos.Text)
            {
                strMessage = string.Format("[Data Save] Axis : {0}, MoveNo : {1}, Pos Change {2} -> {3}", eAxis, _iMoveNo, strOldPos, TBPos.Text);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, strMessage);
            }
            if (strOldVel != TBVel.Text)
            {
                strMessage = string.Format("[Data Save] Axis : {0}, MoveNo : {1}, Velocity Change {2} -> {3}", eAxis, _iMoveNo, strOldVel, TBVel.Text);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, strMessage);
            }

            PositionData cPositionData = ml.cAxisPosCollData.GetPositionData(eAxis, _iMoveNo);
            cPositionData.eAxis = eAxis;
            cPositionData.iNo = _iMoveNo;
            cPositionData.dPos = _dMovePos;
            cPositionData.uiSpeed = _uiMoveVel;
            cPositionData.strName = _strMoveName;

            strOldPos = cPositionData.dPos.ToString();
            strOldVel = cPositionData.uiSpeed.ToString();
        }
    }
}