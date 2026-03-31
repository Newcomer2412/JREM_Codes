using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MachineControlBase
{
    /// <summary>
    /// HolderWorkUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HolderWorkUI : UserControl
    {
        /// <summary>
        /// Main Lib
        /// </summary>
        private CMainLib ml;

        /// <summary>
        /// 19개 홀의 UI를 가져올 Ellipse 배열
        /// </summary>
        private Ellipse[] elList = new Ellipse[19];

        public HolderWorkUI()
        {
            InitializeComponent();
            ml = CMainLib.Ins;
            NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.DEBUG, ml.cSysOne.GetHashCode().ToString());
            NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.DEBUG, ml.cSysOne.bHolderNeedlePinWorkCount.Length.ToString());

            for (int i = 0; i < 19; i++)
            {
                object obEl = this.FindName($"HolderPin{i + 1}");
                elList[i] = (Ellipse)obEl;
            }
        }

        /// <summary>
        /// 작업할 Pin No. 선택 이벤트
        /// 클릭 시 1차 삽입(파란색), 2차 삽입(초록색), 작업 안함(하얀색) 순으로 상태 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HolderPin_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Ellipse el = (Ellipse)sender;

            if (el.Fill == Brushes.Blue)
            {
                el.Fill = Brushes.Green;
            }
            else if (el.Fill == Brushes.Green)
            {
                int iHoleCount = 0;
                for (int i = 0; i < 19; i++)
                {
                    if (elList[i].Fill == Brushes.White) iHoleCount++;
                }
                // 19개의 홀 모두 미삽입 하도록 할 수 없다.
                if (iHoleCount == 18)
                {
                    CCommon.ShowMessageMini(0, "모든 홀을 미삽입할 수 없습니다.");
                    return;
                }

                el.Fill = Brushes.White;
            }
            else
            {
                el.Fill = Brushes.Blue;
            }
        }

        /// <summary>
        /// 홀더 작업 니들 핀 번호 최신화
        /// </summary>
        public void HolderNeedlePinStateUpdate()
        {
            for (int i = 0; i < ml.cSysOne.bHolderNeedlePinWorkCount.Length; i++)
            {
                if (ml.cSysOne.bHolderNeedlePinWorkCount[i] == (int)eNeedlePinStatus.Firstwork)
                {
                    Ellipse el = (Ellipse)FindName($"HolderPin{i + 1}");
                    el.Fill = Brushes.Blue;
                }
                else if (ml.cSysOne.bHolderNeedlePinWorkCount[i] == (int)eNeedlePinStatus.Secondwork)
                {
                    Ellipse el = (Ellipse)FindName($"HolderPin{i + 1}");
                    el.Fill = Brushes.Green;
                }
                else
                {
                    Ellipse el = (Ellipse)FindName($"HolderPin{i + 1}");
                    el.Fill = Brushes.White;
                }
            }
        }

        /// <summary>
        /// 홀더 작업 니들 핀 번호 저장
        /// </summary>
        public void HolderNeedlePinStateSave()
        {
            MapDataLib HolderNeedleMap = ml.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT);
            MapDataLib HolderPipeMap = ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT);
            for (int i = 0; i < ml.cSysOne.bHolderNeedlePinWorkCount.Length; i++)
            {
                Ellipse el = (Ellipse)FindName($"HolderPin{i + 1}");

                if (el.Fill == Brushes.Blue)
                {
                    ml.cSysOne.bHolderNeedlePinWorkCount[i] = (int)eNeedlePinStatus.Firstwork;
                    if (HolderNeedleMap.GetUnitNo(i).eStatus == eStatus.HOLDER || HolderNeedleMap.GetUnitNo(i).eStatus == eStatus.NONE)
                    {
                        if (HolderNeedleMap.GetStatus(eStatus.EMPTY) == true)
                            HolderNeedleMap.GetUnitNo(i).eStatus = eStatus.EMPTY;
                        else
                            HolderNeedleMap.GetUnitNo(i).eStatus = eStatus.FIRST_HOLE;
                    }
                    if (HolderPipeMap.GetUnitNo(i).eStatus == eStatus.HOLDER || HolderPipeMap.GetUnitNo(i).eStatus == eStatus.NONE)
                    {
                        if (HolderPipeMap.GetStatus(eStatus.EMPTY) == true)
                            HolderPipeMap.GetUnitNo(i).eStatus = eStatus.EMPTY;
                        else
                            HolderPipeMap.GetUnitNo(i).eStatus = eStatus.FIRST_HOLE;
                    }
                }
                else if (el.Fill == Brushes.Green)
                {
                    ml.cSysOne.bHolderNeedlePinWorkCount[i] = (int)eNeedlePinStatus.Secondwork;
                    if (HolderNeedleMap.GetUnitNo(i).eStatus == eStatus.FIRST_HOLE || HolderNeedleMap.GetUnitNo(i).eStatus == eStatus.NONE)
                    {
                        if (HolderNeedleMap.GetStatus(eStatus.EMPTY) == true)
                            HolderNeedleMap.GetUnitNo(i).eStatus = eStatus.EMPTY;
                        else
                            HolderNeedleMap.GetUnitNo(i).eStatus = eStatus.HOLDER;
                    }
                    if (HolderPipeMap.GetUnitNo(i).eStatus == eStatus.FIRST_HOLE || HolderPipeMap.GetUnitNo(i).eStatus == eStatus.NONE)
                    {
                        if (HolderPipeMap.GetStatus(eStatus.EMPTY) == true)
                            HolderPipeMap.GetUnitNo(i).eStatus = eStatus.EMPTY;
                        else
                            HolderPipeMap.GetUnitNo(i).eStatus = eStatus.HOLDER;
                    }
                }
                else
                {
                    ml.cSysOne.bHolderNeedlePinWorkCount[i] = (int)eNeedlePinStatus.Notwork;
                    HolderNeedleMap.GetUnitNo(i).eStatus = eStatus.NONE;
                    HolderPipeMap.GetUnitNo(i).eStatus = eStatus.NONE;
                }
            }
        }
    }
}