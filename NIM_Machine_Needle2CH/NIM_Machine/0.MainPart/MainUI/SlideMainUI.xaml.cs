using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MachineControlBase
{
    /// <summary>
    /// SlideMainUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SlideMainUI : UserControl
    {
        private static readonly object MainUIreadLock = new object();
        private static SlideMainUI instance = null;

        public static SlideMainUI Ins
        {
            get
            {
                lock (MainUIreadLock)
                {
                    if (instance == null) instance = new SlideMainUI();
                    return instance;
                }
            }
            set
            {
                instance = value;
            }
        }

        /// <summary>
        /// Main Library
        /// </summary>
        private CMainLib ml = null;

        /// <summary>
        /// 더블 애니메이션 효과 클래스
        /// </summary>
        private DoubleAnimation da = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(500)));

        /// <summary>
        /// 파이프, 니들 홀더 상태에 따른 색상
        /// </summary>
        private SolidColorBrush Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));

        public SlideMainUI()
        {
            InitializeComponent();
            instance = this;
            ml = CMainLib.Ins;
            ml.cDataEditUIManager.MapDataUIListAddControl(gridMain);
            da.RepeatBehavior = RepeatBehavior.Forever;
        }

        /// <summary>
        /// 데이터 갱신용 반복 타이머
        /// </summary>
        public void DataTimer_Tick()
        {
            //CSystemParameterArray cSysArray = ml.cSysParamCollData.GetSysArray();
            // Map Data UI 데이터 갱신
            if (MapDataFunction.bDataEditOpen == false)
            {
                ml.cDataEditUIManager.MapDataRepeatTimer();
            }

            // 파이프 홀더 상태에 따른 색상 변경
            if (ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT).GetStatus(eStatus.EMPTY) == true)
            {
                if (BdHolderPipeDataView.Background != Brush)
                {
                    //Gray
                    Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x80, 0x80, 0x80));
                    BdHolderPipeDataView.Background = Brush;
                }
            }
            else if (ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT).GetStatus(eStatus.FIRST_HOLE) == true)
            {
                if (BdHolderPipeDataView.Background != Brush)
                {
                    //Blue
                    Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0xFF));
                    BdHolderPipeDataView.Background = Brush;
                }
            }
            else if (ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT).GetStatus(eStatus.FIRST_HOLE) == false)
            {
                if (BdHolderPipeDataView.Background != Brush)
                {
                    //Green
                    Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x1D, 0xDB, 0x16));
                    BdHolderPipeDataView.Background = Brush;
                }
            }
            // 니들 홀더 상태에 따른 색상 변경
            if (ml.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT).GetStatus(eStatus.EMPTY) == true)
            {
                if (BdHolderNeedleDataView.Background != Brush)
                {
                    //Gray
                    Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x80, 0x80, 0x80));
                    BdHolderNeedleDataView.Background = Brush;
                }
            }
            else if (ml.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT).GetStatus(eStatus.HOLDER) == true)
            {
                if (BdHolderNeedleDataView.Background != Brush)
                {
                    //Olive
                    Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x6B, 0x99, 0x00));
                    BdHolderNeedleDataView.Background = Brush;
                }
            }
            else if (ml.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT).GetStatus(eStatus.HOLDER) == false)
            {
                if (BdHolderNeedleDataView.Background != Brush)
                {
                    //Deep Green
                    Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x22, 0x74, 0x1C));
                    BdHolderNeedleDataView.Background = Brush;
                }
            }

            // 디스펜서 토출 카운트 최신화
            if (tbDispWorkCount.Text != ml.cSysOne.iDispWorkCount.ToString())
                tbDispWorkCount.Text = ml.cSysOne.iDispWorkCount.ToString();
            // 디스펜서 용액을 교체해야 된다면 해당 Text Block 반짝 반짝 효과를 넣는다.
            if (ml.cSysOne.iDispWorkCount >= ml.cSysOne.iDispWorkLimitCount)
            {
                if (tbDispWorkCount.Foreground != Brushes.Red)
                {
                    da.AutoReverse = true;
                    tbDispWorkCount.Foreground = Brushes.Red;
                    tbDispWorkCount.BeginAnimation(OpacityProperty, da);
                }
            }

            int iMountCount = ml.cRunUnitData.GetIndexData(eData.SUPPLY_HOLDER_TRAY).BaseMapDataList.FindAll(x => x.eStatus == eStatus.MOUNT).Count;
            if (int.Parse(tbSupplyHolderCount.Text) != iMountCount)
                tbSupplyHolderCount.Text = iMountCount.ToString();
        }

        /// <summary>
        /// Recovery Tray 데이터 팝업창 오픈
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BdRecoveryTrayDataView_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ml.cDataEditUIManager.PopupClose();
            if (Popup_RecoveryTrayUnitStatus.IsOpen == true)
            {
                Popup_RecoveryTrayUnitStatus.IsOpen = false;
            }
            else
            {
                PopupClose();
                Popup_RecoveryTrayUnitStatus.IsOpen = true;
            }
        }

        /// <summary>
        /// Supply Tray 데이터 팝업창 오픈
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BdSupplyTrayDataView_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ml.cDataEditUIManager.PopupClose();
            if (Popup_SupplyTrayUnitStatus.IsOpen == true)
            {
                Popup_SupplyTrayUnitStatus.IsOpen = false;
            }
            else
            {
                PopupClose();
                Popup_SupplyTrayUnitStatus.IsOpen = true;
            }
        }

        /// <summary>
        /// 팝업창 닫기
        /// </summary>
        public void PopupClose()
        {
            Popup_RecoveryTrayUnitStatus.IsOpen = false;
            Popup_SupplyTrayUnitStatus.IsOpen = false;
            Popup_HolderPipeUnitStatus.IsOpen = false;
            Popup_HolderNeedleUnitStatus.IsOpen = false;
            Popup_NG_TrayUnitStatus.IsOpen = false;
        }

        /// <summary>
        /// 디스펜서 토출 초기화
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispWorkCountClear_Button_Click(object sender, RoutedEventArgs e)
        {
            if (CCommon.ShowMessageMini(1, "토출 횟수를 초기화하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            da.AutoReverse = false;
            tbDispWorkCount.BeginAnimation(OpacityProperty, null);
            tbDispWorkCount.Foreground = Brushes.White;
            ml.cSysOne.iDispWorkCount = 0;
            CXMLProcess.WriteXml(CXMLProcess.SystemParameterSingleFilePath, ml.cSysOne);
        }

        /// <summary>
        /// 홀더 공급작업 일시정지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HolderSupplyStop_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.cVar.bHolder_SupplyStop == true)
            {
                if (CCommon.ShowMessageMini(1, "홀더 공급을 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
                ml.cVar.bHolder_SupplyStop = false;
                HolderSupplyStop.Content = "홀더 공급 일시정지";
                HolderSupplyStop.Background = Brushes.LightGreen;
            }
            else
            {
                if (CCommon.ShowMessageMini(1, "홀더 공급을 일시정지 하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
                ml.cVar.bHolder_SupplyStop = true;
                HolderSupplyStop.Content = "홀더 공급 재게";
                HolderSupplyStop.Background = Brushes.Red;
            }
        }

        /// <summary>
        /// 홀더 Tray 교환 위치로 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btHolderTrayChangeMove_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "홀더 Tray 교체 위치로 이동하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_HolderTrayChangeMove = true;
            ml.ManualStart(eSequence.Seq03_Holder_SnR);
        }

        /// <summary>
        /// 파이프 트랜스퍼 NG Tray 교체 위치로 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btPipeTransferNG_Move_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "파이프 트랜스퍼 NG 교체 위치로 이동하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_TransferNG_Change = true;
            ml.ManualStart(eSequence.Seq07_Transfer1);
        }

        /// <summary>
        /// 니들 트랜스퍼 NG Tray 교체 위치로 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btNeedleTransferNG_Move_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "니들 트랜스퍼 NG 교체 위치로 이동하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_TransferNG_Change = true;
            ml.ManualStart(eSequence.Seq07_Transfer2);
        }

        /// <summary>
        /// 파이프 니들 PnP 대기위치 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btPipeNeedlePnPMoveSafe_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "파이프 니들 PnP 대기위치 이동하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_PipeNeedlePnPMoveSafePos = true;
            ml.ManualStart(eSequence.Seq04_Hopper);
        }

        /// <summary>
        /// 강제 파이프 마운트 동작 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btPipeMountNotInsp_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "강제 파이프 마운트를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_PipeMountNotInsp = true;
            ml.ManualStart(eSequence.Seq08_Pipe_Mount);
        }

        /// <summary>
        /// 강제 니들 마운트 동작 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btNeedleMountNotInsp_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "강제 니들 마운트를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_NeedeleMountNotInsp = true;
            ml.ManualStart(eSequence.Seq09_Needle_Mount);
        }

        /// <summary>
        /// 매뉴얼 검사 없는 디스펜싱 동작 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btDispensingNotInsp_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "강제 디스펜싱을 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_DispensingNotInsp = true;
            ml.ManualStart(eSequence.Seq10_Dispensing);
        }

        /// <summary>
        /// 파이프 니들을 잡고 있는 축들의 자재 Clear 동작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btPipeNeedleAllWorkClear_Move_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "파이프와 니들을 잡고 있는 모든 것을 Clear 하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_PipeNeedleRobotAllWorkClear = true;
            ml.ManualStart(eSequence.Seq04_Hopper);
        }

        /// <summary>
        /// 홀더 파이프 누름 Down 동작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btJustPipePush_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "파이프 누름 동작을 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_JustPipePush = true;
            ml.ManualStart(eSequence.Seq08_Pipe_Mount);
        }

        /// <summary>
        /// 홀더 파이프 누름 Up 동작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btJustPipePushUp_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "파이프 누름 UP 동작을 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.Seq.SeqIO.SetOutput((int)eIO_O.PIPE_PUSHER_DOWN, false);
        }

        /// <summary>
        /// 플립 모터 및 실린더 위치 초기화
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btFlipPosClear_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "플립 위치 초기화를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_FlipPosClear = true;
            ml.ManualStart(eSequence.Seq00_MPC_Rail1);
        }

        /// <summary>
        /// 홀더 니들 누름 동작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btJustNeedlePush_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "니들 누름 동작을 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_JustNeedlePush = true;
            ml.ManualStart(eSequence.Seq09_Needle_Mount);
        }

        /// <summary>
        /// 홀더 니들 누름 Up 동작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btJustNeedlePushUp_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "니들 누름 Up 동작을 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.Seq.SeqIO.SetOutput((int)eIO_O.NEEDLE_PUSHER_DOWN, false);
        }

        /// <summary>
        /// Just 파이프 홀더 촬영
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btPipeHolderShot_Move_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "파이프 홀더 사진만 촬영하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_JustPipeHolderShot = true;
            ml.ManualStart(eSequence.Seq08_Pipe_Mount);
        }

        /// <summary>
        /// Just 니들 홀더 촬영
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btNeedleHolderShot_Move_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "니들 홀더 사진만 촬영하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_JustNeedleHolderShot = true;
            ml.ManualStart(eSequence.Seq09_Needle_Mount);
        }

        /// <summary>
        /// Just 디스펜서 홀더 촬영
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btDispHolderShot_Move_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "Disp 홀더 사진만 촬영하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_JustDispHolderShot = true;
            ml.ManualStart(eSequence.Seq10_Dispensing);
        }

        /// <summary>
        /// 홀더 파이프 마운트 맵 데이터
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BdHolderPipeDataView_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ml.cDataEditUIManager.PopupClose();
            if (Popup_HolderPipeUnitStatus.IsOpen == true)
            {
                Popup_HolderPipeUnitStatus.IsOpen = false;
            }
            else
            {
                PopupClose();
                Popup_HolderPipeUnitStatus.IsOpen = true;
            }
        }

        /// <summary>
        /// 홀더 버퍼 맵 데이터
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BdHolderBuffer2DataView_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ml.cDataEditUIManager.PopupClose();
            if (Popup_HolderBuffer2UnitStatus.IsOpen == true)
            {
                Popup_HolderBuffer2UnitStatus.IsOpen = false;
            }
            else
            {
                PopupClose();
                Popup_HolderBuffer2UnitStatus.IsOpen = true;
            }
        }

        /// <summary>
        /// 홀더 니들 마운트 맵 데이터
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BdHolderNeedleDataView_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ml.cDataEditUIManager.PopupClose();
            if (Popup_HolderNeedleUnitStatus.IsOpen == true)
            {
                Popup_HolderNeedleUnitStatus.IsOpen = false;
            }
            else
            {
                PopupClose();
                Popup_HolderNeedleUnitStatus.IsOpen = true;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BdNG_TrayDataView_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ml.cDataEditUIManager.PopupClose();
            if (Popup_NG_TrayUnitStatus.IsOpen == true)
            {
                Popup_NG_TrayUnitStatus.IsOpen = false;
            }
            else
            {
                PopupClose();
                Popup_NG_TrayUnitStatus.IsOpen = true;
            }
        }

        /// <summary>
        /// 공급 트레이 마운트 횟수 설정 (-)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SupplyHolderCountMinus_Button_Click(object sender, RoutedEventArgs e)
        {
            MapDataLib HolderSupplyTray = ml.cRunUnitData.GetIndexData(eData.SUPPLY_HOLDER_TRAY);
            if (HolderSupplyTray.GetStatus(eStatus.MOUNT) == false) return;
            int iMaxCount = HolderSupplyTray.iMaxXindex * HolderSupplyTray.iMaxYindex;
            for (int i = iMaxCount - 1; i >= 0; i--)
            {
                if (HolderSupplyTray.GetUnitNo(i).eStatus == eStatus.MOUNT)
                {
                    HolderSupplyTray.GetUnitNo(i).eStatus = eStatus.EMPTY;
                    break;
                }
            }
        }

        /// <summary>
        /// 공급 트레이 마운트 횟수 설정 (+)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SupplyHolderCountPlus_Button_Click(object sender, RoutedEventArgs e)
        {
            MapDataLib HolderSupplyTray = ml.cRunUnitData.GetIndexData(eData.SUPPLY_HOLDER_TRAY);
            if (HolderSupplyTray.GetStatus(eStatus.EMPTY) == false) return;
            int iMaxCount = HolderSupplyTray.iMaxXindex * HolderSupplyTray.iMaxYindex;
            for (int i = 0; i < iMaxCount; i++)
            {
                if (HolderSupplyTray.GetUnitNo(i).eStatus == eStatus.EMPTY)
                {
                    HolderSupplyTray.GetUnitNo(i).eStatus = eStatus.MOUNT;
                    break;
                }
            }
        }

        /// <summary>
        /// 대기존2, 토출존 맵데이터 정상으로 변경
        /// 대기존2 : Pipe_Magnetic, 토출존 : Filp_Done
        /// 스킵 추가로 인해 작업자가 새로운 맵데이터에 익숙하지 않을 수 있기 때문에 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSkipMapDataChange_Click(object sender, RoutedEventArgs e)
        {
            MapDataLib MPC1_Dispensing = ml.cRunUnitData.GetIndexData(eData.MPC1_DISPENSING);

            if (MPC1_Dispensing.GetAllStatus(eStatus.SKIP_AFTER_FLIP, false) == true)
            {
                MPC1_Dispensing.SetAllStatus(eStatus.FLIP_DONE);
            }
        }

        /// <summary>
        /// 강제 니들 마운트 동작
        /// RedLineTool -> FindCircle 로 좌표를 찾지 않고
        /// RedLineTool의 Center값을 좌표로 하여 삽입한다.
        /// FindCircle이 제대로 안되는 홀이 있기 때문에 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btForcedNeedleMount_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "강제 니들 마운트 동작을 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_ForcedNeedleMount = true;
            ml.ManualStart(eSequence.Seq09_Needle_Mount);
        }

        /// <summary>
        /// 공급 트레이 전체 마운트 버튼
        /// 동작중에 홀더 공급 편하게 하기 위해 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SupplyHolderFull_Button_Click(object sender, RoutedEventArgs e)
        {
            MapDataLib HolderSupplyTray = ml.cRunUnitData.GetIndexData(eData.SUPPLY_HOLDER_TRAY);
            if (HolderSupplyTray.GetAllStatus(eStatus.EMPTY) == false)
            {
                CCommon.ShowMessageMini(0, "공급 트레이가 비어있지 않습니다.");
                return;
            }
            HolderSupplyTray.SetAllStatus(eStatus.MOUNT);
        }
    }
}