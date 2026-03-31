using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;

namespace MachineControlBase
{
    /// <summary>
    /// IOActuatorUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class IOActuatorUI : UserControl
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public IOActuatorUI()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Init()
        {
            // 초기화 세팅
            if (iFWDInput == -1)
            {
                if (CMainLib.Ins.Seq.SeqIO.GetInput(iBWDInput) == false)
                {
                    ACTBorder1.Background = Brushes.BlanchedAlmond;
                    bOldInputStatus[0] = true;
                }
            }
            else if (iBWDInput == -1)
            {
                if (CMainLib.Ins.Seq.SeqIO.GetInput(iFWDInput) == false)
                {
                    ACTBorder2.Background = Brushes.BlanchedAlmond;
                }
                bOldInputStatus[1] = true;
            }
        }

        /// <summary>
        /// Actuator FWD Output
        /// </summary>
        private int iFWDOutput = -1;

        /// <summary>
        /// Actuator FWD Output
        /// </summary>
        public int _iFWDOutput
        {
            get { return iFWDOutput; }
            set { iFWDOutput = value; }
        }

        /// <summary>
        /// Actuator FWD Input
        /// </summary>
        private int iFWDInput = -1;

        /// <summary>
        /// Actuator FWD Input
        /// </summary>
        public int _iFWDInput
        {
            get { return iFWDInput; }
            set { iFWDInput = value; }
        }

        /// <summary>
        /// Actuator BWD Output
        /// </summary>
        private int iBWDOutput = -1;

        /// <summary>
        /// Actuator BWD Output
        /// </summary>
        public int _iBWDOutput
        {
            get { return iBWDOutput; }
            set { iBWDOutput = value; }
        }

        /// <summary>
        /// Actuator BWD Input
        /// </summary>
        private int iBWDInput = -1;

        /// <summary>
        /// Actuator BWD Input
        /// </summary>
        public int _iBWDInput
        {
            get { return iBWDInput; }
            set { iBWDInput = value; }
        }

        /// <summary>
        /// Actuator name
        /// </summary>
        private string strActuatorName = string.Empty;

        /// <summary>
        /// Actuator Name 설정
        /// </summary>
        public string _strActuatorName
        {
            get { return strActuatorName; }
            set
            {
                strActuatorName = value;
                ActuatorName.Text = value;
            }
        }

        /// <summary>
        /// 실린더 FWD Concept Name
        /// </summary>
        private string strFwdName = string.Empty;

        /// <summary>
        /// 실린더 FWD Concept Name 설정
        /// </summary>
        public string _strFwdName
        {
            get { return strFwdName; }
            set
            {
                strFwdName = value;
                TbFWD.Text = value;
            }
        }

        /// <summary>
        /// 실린더 BWD Concept Name
        /// </summary>
        private string strBwdName = string.Empty;

        /// <summary>
        /// 실린더 BWD Concept Name 설정
        /// </summary>
        public string _strBwdName
        {
            get { return strBwdName; }
            set
            {
                strBwdName = value;
                TbBWD.Text = value;
            }
        }

        /// <summary>
        /// 실린더 BWD 상태 여부 (시뮬레이션)
        /// </summary>
        public bool bBwdState = false;

        /// <summary>
        /// 실린더 BWD 상태 여부 (시뮬레이션)
        /// </summary>
        public bool bFwdState = false;

        /// <summary>
        /// FWD 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btFWD_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CMainLib.Ins.McState == eMachineState.RUN || CMainLib.Ins.McState == eMachineState.MANUALRUN) return;
            if (bRepeatStart == true) return;
            FWDPrcess();
        }

        /// <summary>
        /// FWD 처리
        /// </summary>
        private void FWDPrcess()
        {
            //실린더 안전 이동 확인
            if (SafeCheckCylinder(iFWDOutput))
            {
                if (iFWDOutput != -1)
                {
                    if (iBWDOutput != -1) CMainLib.Ins.Seq.SeqIO.SetOutput(iBWDOutput, false);
                    CMainLib.Ins.Seq.SeqIO.SetOutput(iFWDOutput, true);
                }
                else
                {
                    CMainLib.Ins.Seq.SeqIO.SetOutput(iBWDOutput, false);
                }

                // 시뮬레이션이 아니면 여기서 실린더 이동시간 딜레이를 시작한다.
                if (Define.SIMULATION == false)
                {
                    CCylinderDelay cCylinder = CMainLib.Ins.Seq.SeqIO.cCylinderDelayList.Find(x => x.iInputNo == iFWDInput);
                    if (cCylinder.cCylinderDelay.IsRunning == false) cCylinder.cCylinderDelay.Restart();
                }

                if (iFWDInput == -1)
                {
                    ACTBorder1.Background = Brushes.BlanchedAlmond;
                }
            }
            else
            {
                // Repeat이면 취소시킨다.
                if (bRepeatStart == true)
                {
                    bRepeatStart = false;
                    ACTBorder3.Background = Brushes.CadetBlue;
                    cTimeCheck.Stop();
                }
                CCommon.ShowMessageMini("충돌위험, 이동할 수 없습니다.");
            }
        }

        /// <summary>
        /// FWD 처리 완료 확인
        /// </summary>
        /// <returns></returns>
        private bool bFWDCheck()
        {
            if (CMainLib.Ins.Seq.SeqIO.GetInput((int)iFWDInput) == true)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// BWD 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btBWD_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CMainLib.Ins.McState == eMachineState.RUN || CMainLib.Ins.McState == eMachineState.MANUALRUN) return;
            if (bRepeatStart == true) return;
            BWDPrcess();
        }

        /// <summary>
        /// BWD 처리
        /// </summary>
        private void BWDPrcess()
        {
            //실린더 안전 이동 확인
            if (SafeCheckCylinder(iBWDOutput))
            {
                if (iBWDOutput != -1)
                {
                    if (iFWDOutput != -1) CMainLib.Ins.Seq.SeqIO.SetOutput(iFWDOutput, false);
                    CMainLib.Ins.Seq.SeqIO.SetOutput(iBWDOutput, true);
                }
                else
                {
                    CMainLib.Ins.Seq.SeqIO.SetOutput(iFWDOutput, false);
                }

                // 시뮬레이션이 아니면 여기서 실린더 이동시간 딜레이를 시작한다.
                if (Define.SIMULATION == false)
                {
                    CCylinderDelay cCylinder = CMainLib.Ins.Seq.SeqIO.cCylinderDelayList.Find(x => x.iInputNo == iBWDInput);
                    if (cCylinder.cCylinderDelay.IsRunning == false) cCylinder.cCylinderDelay.Restart();
                }

                if (iBWDInput == -1)
                {
                    ACTBorder2.Background = Brushes.BlanchedAlmond;
                }
            }
            else
            {
                // Repeat이면 취소시킨다.
                if (bRepeatStart == true)
                {
                    bRepeatStart = false;
                    ACTBorder3.Background = Brushes.CadetBlue;
                    cTimeCheck.Stop();
                }
                CCommon.ShowMessageMini("충돌위험, 이동할 수 없습니다");
            }
        }

        /// <summary>
        /// BWD 처리 완료 확인
        /// </summary>
        /// <returns></returns>
        private bool bBWDCheck()
        {
            if (CMainLib.Ins.Seq.SeqIO.GetInput((int)iBWDInput) == true)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Repeat 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btRepeat_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CMainLib.Ins.McState == eMachineState.RUN || CMainLib.Ins.McState == eMachineState.MANUALRUN) return;
            bRepeatStart = !bRepeatStart;
            if (bRepeatStart == true)
            {
                ACTBorder3.Background = Brushes.DarkRed;
                cTimeCheck.Restart();
            }
            else
            {
                ACTBorder3.Background = Brushes.CadetBlue;
                cTimeCheck.Stop();
            }
        }

        /// <summary>
        /// 실린더 반복 동작 Flag
        /// </summary>
        private bool bRepeatStart = false;

        /// <summary>
        /// 실린더 반복 동작 On/Off Flag
        /// </summary>
        private bool bRepeatOnOff = false;

        private bool[] bOldInputStatus = new bool[2] { false, false };

        /// <summary>
        /// 시간 체크
        /// </summary>
        private Stopwatch cTimeCheck = new Stopwatch();

        /// <summary>
        /// 반복 갱신 함수
        /// </summary>
        public void RepeatUpdateTimer()
        {
            if (bRepeatStart == true)
            {
                if (bRepeatOnOff == false)
                {
                    FWDPrcess();
                    if (bFWDCheck())
                    {
                        if (cTimeCheck.ElapsedMilliseconds >= CMainLib.Ins.cSysOne.iCylinderRepeatTime)
                        {
                            bRepeatOnOff = !bRepeatOnOff;
                            cTimeCheck.Restart();
                        }
                    }
                }
                else
                {
                    BWDPrcess();
                    if (bBWDCheck())
                    {
                        if (cTimeCheck.ElapsedMilliseconds >= CMainLib.Ins.cSysOne.iCylinderRepeatTime)
                        {
                            bRepeatOnOff = !bRepeatOnOff;
                            cTimeCheck.Restart();
                        }
                    }
                }
            }

            bool bRtn = false;
            if (iFWDInput != -1)
            {
                bRtn = CMainLib.Ins.Seq.SeqIO.GetInput(iFWDInput);
                if (bRtn != bOldInputStatus[0])
                {
                    if (bRtn == true)
                    {
                        CCylinderDelay cCylinder = CMainLib.Ins.Seq.SeqIO.cCylinderDelayList.Find(x => x.iInputNo == iFWDInput);
                        cCylinder.cCylinderDelay.Stop();
                        ActuatorTime.Text = cCylinder.cCylinderDelay.ElapsedMilliseconds.ToString();

                        ACTBorder2.Background = Brushes.Gray;
                        ACTBorder1.Background = Brushes.BlanchedAlmond;
                    }
                    else ACTBorder1.Background = Brushes.Gray;
                    bOldInputStatus[0] = bRtn;
                }
            }
            if (iBWDInput != -1)
            {
                bRtn = CMainLib.Ins.Seq.SeqIO.GetInput(iBWDInput);
                if (bRtn != bOldInputStatus[1])
                {
                    if (bRtn == true)
                    {
                        CCylinderDelay cCylinder = CMainLib.Ins.Seq.SeqIO.cCylinderDelayList.Find(x => x.iInputNo == iBWDInput);
                        cCylinder.cCylinderDelay.Stop();
                        ActuatorTime.Text = cCylinder.cCylinderDelay.ElapsedMilliseconds.ToString();

                        ACTBorder1.Background = Brushes.Gray;
                        ACTBorder2.Background = Brushes.BlanchedAlmond;
                    }
                    else ACTBorder2.Background = Brushes.Gray;
                    bOldInputStatus[1] = bRtn;
                }
            }
        }

        /// <summary>
        /// 실린더 이동시 안전 확인
        /// </summary>
        public bool SafeCheckCylinder(int i_OutAddress)
        {
            //switch (i_OutAddress)
            //{
            //    case (int)eIO_O.WASTE_CONTAINER_IN:
            //        if (CMainLib.Ins.Axis[eMotor.MOT_DISPENSER_Z].GetActPostion() > -9)
            //        {
            //            return true;
            //        }
            //        return false;
            //}
            return true;
        }
    }
}