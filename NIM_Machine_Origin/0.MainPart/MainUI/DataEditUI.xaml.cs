using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// DataEditUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DataEditUI : UserControl
    {
        /// <summary>
        /// Main Library
        /// </summary>
        private CMainLib ml = null;

        /// <summary>
        /// SystemParameterArray
        /// </summary>
        private CSystemParameterArray cSysArray;

        /// <summary>
        /// Data 갱신용 타이머
        /// </summary>
        private DispatcherTimer cDataTimer = null;

        /// <summary>
        /// 파이프, 니들 홀더 상태에 따른 색상
        /// </summary>
        private SolidColorBrush Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));

        /// <summary>
        /// 생성자
        /// </summary>
        public DataEditUI()
        {
            InitializeComponent();
            ml = CMainLib.Ins;
            ml.cDataEditUIManager.UserContorListAddControl(MainTab);
            ml.cDataEditUIManager.DataEditMonitorListAddControl(MainTab);
            InitUserControlInformation();
        }

        /// <summary>
        /// 포커스를 얻거나 잃을 때 타이머 제어
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                cSysArray = ml.cSysParamCollData.GetSysArray();
                DataLoad();

                if (cDataTimer == null)
                {
                    cDataTimer = new DispatcherTimer();
                    cDataTimer.Interval = TimeSpan.FromMilliseconds(100);   // 시간 간격 설정
                    cDataTimer.Tick += new EventHandler(DataTimer_Tick);    // 이벤트 추가
                }

                // 데이터 에디트 창을 여러개로 분리할 경우 다른 데이터 에디트 창을 가면 탭 리스트 갱신이 안되므로 추가함
                ml.cDataEditUIManager.UserContorMonitorListAddControl(MainTab);
                ml.cDataEditUIManager.DataEditMonitorListAddControl(MainTab);
                MapDataFunction.bDataEditOpen = true;
                ml.cDataEditUIManager.UpdateMonitorMapData();
                cDataTimer.Start(); // 타이머 시작
                UISelectHolderWorkPin.HolderNeedlePinStateUpdate();
            }
            else
            {
                cDataTimer.Stop();
                ml.cDataEditUIManager.UpdateMapData();
                MapDataFunction.bDataEditOpen = false;
            }
            // 데이터 맵의 팝업창이 열려있으면 닫는다.
            ml.cDataEditUIManager.PopupClose();
        }

        /// <summary>
        /// 선택된 탭 클래스
        /// </summary>
        private TabItem selectedTab = null;

        /// <summary>
        /// Tab Item을 선택하였을 경우 발생 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cDataTimer.Stop();
            selectedTab = e.AddedItems[0] as TabItem;  // Gets selected tab
            if (selectedTab == null) return;
            ml.cDataEditUIManager.UserContorMonitorListAddControl(selectedTab);
            ml.cDataEditUIManager.DataEditMonitorListAddControl(selectedTab);
            UISelectHolderWorkPin.HolderNeedlePinStateUpdate();
            cDataTimer.Start();
        }

        /// <summary>
        /// 데이터 갱신용 반복 타이머
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataTimer_Tick(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate ()
            {
                // 각 탭에서 데이터를 반복 갱신 시 여기에 탭명과 반복할 코드를 삽입
                if (selectedTab != null)
                {
                    if (selectedTab.Name == "TiMPC")
                    {
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
                        else if (ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT).GetStatus(eStatus.HOLDER) == true)
                        {
                            if (BdHolderPipeDataView.Background != Brush)
                            {
                                //Olive
                                Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x6B, 0x99, 0x00));
                                BdHolderPipeDataView.Background = Brush;
                            }
                        }
                        else if (ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT).GetAllStatus(eStatus.PIPE_MOUNT, false) == true)
                        {
                            if (BdHolderPipeDataView.Background != Brush)
                            {
                                //Blue
                                Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0xFF));
                                BdHolderPipeDataView.Background = Brush;
                            }
                        }
                        else if (ml.cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT).GetAllStatus(eStatus.NEEDLE_MOUNT, true) == true)
                        {
                            if (BdHolderNeedleDataView.Background != Brush)
                            {
                                //Green
                                Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x1D, 0xDB, 0x16));
                                BdHolderNeedleDataView.Background = Brush;
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
                        else if (ml.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT).GetAllStatus(eStatus.HOLDER, true) == true)
                        {
                            if (BdHolderNeedleDataView.Background != Brush)
                            {
                                //Olive
                                Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x6B, 0x99, 0x00));
                                BdHolderPipeDataView.Background = Brush;
                            }
                        }
                        else if (ml.cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT).GetAllStatus(eStatus.NEEDLE_MOUNT, true) == true)
                        {
                            if (BdHolderNeedleDataView.Background != Brush)
                            {
                                //Green
                                Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x1D, 0xDB, 0x16));
                                BdHolderNeedleDataView.Background = Brush;
                            }
                        }
                    }
                    else if (selectedTab.Name == "TiDispenser")
                    {
                        if (tbDispWorkCount.Text != ml.cSysOne.iDispWorkCount.ToString())
                            tbDispWorkCount.Text = ml.cSysOne.iDispWorkCount.ToString();
                    }
                }
                ml.cDataEditUIManager.RepeatTimer();
            });
        }

        /// <summary>
        /// Data를 모두 Load 한다.
        /// </summary>
        private void DataLoad()
        {
            // UserControl의 데이터를 모두 갱신
            ml.cDataEditUIManager.LoadData();

            // 홀더 클램프, 언클램프 다운 딜레이
            UIHolderClampDelay.SetData(cSysArray.uiHolderClampDownDelay);
            UIHolderUnClampDelay.SetData(cSysArray.uiHolderUnClampDownDelay);
            // UV 동작 딜레이
            UIUVDelay.SetData(cSysArray.dUVCureDelay);
            // UV 파워
            UIUVPower.SetData(cSysArray.uiUVPower);
            // 호퍼 동작 딜레이
            UIHopperPipeRunDelay.SetData(cSysArray.uiHopperPipeRunDelay);
            UIHopperNeedleRunDelay.SetData(cSysArray.uiHopperNeedleRunDelay);
            // AIM 피더 Stroke
            UIAIM_Stroke.SetData(cSysArray.uiFeederRunParam[0]);
            // AIM 피더 진동 주파수
            UIAIM_Hz.SetData(cSysArray.uiFeederRunParam[1]);
            // AIM 피더 동작 시간
            UIAIM_RunTime.SetData(cSysArray.uiFeederRunParam[2]);
            // 파이프 & 니들 PnP 픽업 진공 딜레이
            UIPnPPipeVacuumDelay.SetData(cSysArray.uiPnPPipeVacuumDelay);
            UIPnPNeedleVacuumDelay.SetData(cSysArray.uiPnPNeedleVacuumDelay);
            // 파이프 & 니들 트렌스퍼 진공 딜레이
            UITransferVacuumDelay.SetData(cSysArray.uiTransferVacuumDelay);
            // 파이프 & 니들 클램프 진공 딜레이
            UIPipeMountVacuumDelay.SetData(cSysArray.uiPipeClampUpDelay);
            UINeedleMountVacuumDelay.SetData(cSysArray.uiNeedleClampVacuumDelay);
            // 파이프 마그넷틱 딜레이
            UIMagneticInjectionDelay.SetData(ml.cSysOne.uiMagneticDelay);
            // Disp 밸브 딜레이
            UIDispValveDelay.SetData(ml.cSysOne.uiDispValveDelay);
            // Disp 토출 Limit Count
            UIDispWorkLimitCount.SetData(ml.cSysOne.iDispWorkLimitCount);
            // 클린 패드에 다운 후 딜레이
            UIDispCleanDelay.SetData(ml.cSysOne.uiCleanDelay);
            // 플립 그리퍼 Close 딜레이
            UIFlipGripperCloseDelay.SetData(cSysArray.uiFlipGripperCloseDelay);
        }

        /// <summary>
        /// Save 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BTSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Position 관련 UserControl의 데이터를 모두 저장
            ml.cDataEditUIManager.SaveData();

            // 홀더 작업 니들 핀 번호
            UISelectHolderWorkPin.HolderNeedlePinStateSave();
            // 홀더 클램프, 언클램프 다운 딜레이
            UIHolderClampDelay.GetData(ref cSysArray.uiHolderClampDownDelay);
            UIHolderUnClampDelay.GetData(ref cSysArray.uiHolderUnClampDownDelay);
            // UV 동작 딜레이
            UIUVDelay.GetData(ref cSysArray.dUVCureDelay);
            // UV 파워
            UIUVPower.GetData(ref cSysArray.uiUVPower);
            if (Define.SIMULATION == false)
            {
                ml.cUV_Ctl.UV_OnTime((uint)cSysArray.dUVCureDelay * 10);
                ml.cUV_Ctl.UV_PowerSet(cSysArray.uiUVPower);
            }
            // 호퍼 동작 딜레이
            UIHopperPipeRunDelay.GetData(ref cSysArray.uiHopperPipeRunDelay);
            UIHopperNeedleRunDelay.GetData(ref cSysArray.uiHopperNeedleRunDelay);
            // AIM 피더 Stroke
            UIAIM_Stroke.GetData(ref cSysArray.uiFeederRunParam[0]);
            // AIM 피더 진동 주파수
            UIAIM_Hz.GetData(ref cSysArray.uiFeederRunParam[1]);
            // AIM 피더 동작 시간
            UIAIM_RunTime.GetData(ref cSysArray.uiFeederRunParam[2]);
            // 파이프 & 니들 PnP 픽업 진공 딜레이
            UIPnPPipeVacuumDelay.GetData(ref cSysArray.uiPnPPipeVacuumDelay);
            UIPnPNeedleVacuumDelay.GetData(ref cSysArray.uiPnPNeedleVacuumDelay);
            // 파이프 & 니들 트렌스퍼 진공 딜레이
            UITransferVacuumDelay.GetData(ref cSysArray.uiTransferVacuumDelay);
            // 파이프 & 니들 클램프 진공 딜레이
            UIPipeMountVacuumDelay.GetData(ref cSysArray.uiPipeClampUpDelay);
            UINeedleMountVacuumDelay.GetData(ref cSysArray.uiNeedleClampVacuumDelay);
            // 파이프 마그넷틱 딜레이
            UIMagneticInjectionDelay.GetData(ref ml.cSysOne.uiMagneticDelay);
            // Disp 밸브 딜레이
            UIDispValveDelay.GetData(ref ml.cSysOne.uiDispValveDelay);
            // 디스펜서 밸브 Output 트리거 셋
            int[] iTriggerTime = new int[4] { (int)ml.cSysOne.uiDispValveDelay * 1000, 0, 0, 0 };
            ml.Uz_IO_Module.SetOutputTriggerPeriod((int)eIO_O.DISPENSER_VALVE_OPEN, 1, iTriggerTime);
            ml.Uz_IO_Module.SetOutputTriggerForTime((int)eIO_O.DISPENSER_VALVE_OPEN, 1, iTriggerTime);
            ml.Uz_IO_Module.SetTriggerCount((int)eIO_O.DISPENSER_VALVE_OPEN, 1);
            // Disp 토출 Limit Count
            UIDispWorkLimitCount.GetData(ref ml.cSysOne.iDispWorkLimitCount);
            // 클린 패드에 다운 후 딜레이
            UIDispCleanDelay.GetData(ref ml.cSysOne.uiCleanDelay);
            // 플립 그리퍼 Close 딜레이
            UIFlipGripperCloseDelay.GetData(ref cSysArray.uiFlipGripperCloseDelay);

            CXMLProcess.XmlFileSave();
            CCommon.ShowMessageMini("Save Complete.");
        }

        /// <summary>
        /// 데이터 변경이 있으면 로그로 남기고 저장
        /// </summary>
        /// <param name="cToggleButton"></param>
        /// <param name="bData"></param>
        private void DataChangeCheck(ToggleButton cToggleButton, ref bool bData)
        {
            // 변경 Log 기록
            if (cToggleButton.IsChecked != bData)
            {
                string strMessage = string.Format("[Data Save] {0} : {1} -> {2}", cToggleButton.Content, bData, cToggleButton.IsChecked);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, strMessage);
                bData = (bool)cToggleButton.IsChecked;
            }
        }

        /// <summary>
        /// User Control에 마우스 오버 시 설명이 나오도록 내용 등록
        /// </summary>
        private void InitUserControlInformation()
        {
            //UIAllAutoRatio._strToolTipName = "전체 모터 속도 설정";
            //UIAllAutoRatio._strToolTipData = "전체 모터 속도를 퍼센트로 제어합니다.\n속도 입력 시 높은 값이 들어가지 않도록 유의하시기 바랍니다.";
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

        #region Manual 버튼 기능 정의

        /// <summary>
        /// Tray에 담긴 빈 홀더 픽업
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_EmptyHolderPickUp_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual Tray에 담긴 빈 홀더를 픽업하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_EmptyHolderPickUp = true;
            ml.ManualStart(eSequence.Seq03_Holder_SnR);
        }

        /// <summary>
        /// 빈 홀더 MPC 팔렛에 공급
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_EmptyHolderSupply_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 빈 홀더를 MPC 팔렛에 공급하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_EmptyHolderSupply = true;
            ml.ManualStart(eSequence.Seq03_Holder_SnR);
        }

        /// <summary>
        /// 작업 완료 홀더 픽업
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_DoneHolderPickUp_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 작업 완료된 홀더를 픽업하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_DoneHolderPickUp = true;
            ml.ManualStart(eSequence.Seq03_Holder_SnR);
        }

        /// <summary>
        /// 작업 완료 홀더 회수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_DoneHolderRecovery_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 작업 완료된 홀더를 회수하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_DoneHolderRecovery = true;
            ml.ManualStart(eSequence.Seq03_Holder_SnR);
        }

        /// <summary>
        /// 불량 홀더 회수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_DoneNgHolderRecovery_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 불량 홀더를 회수하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_NgHolderRecovery = true;
            ml.ManualStart(eSequence.Seq03_Holder_SnR);
        }

        /// <summary>
        /// Front MPC 간격 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_FrontMPCPitchMove_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual Front MPC 간격 이동을 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_MPC1_MovePitch = true;
            ml.ManualStart(eSequence.Seq00_MPC_Rail1);
        }

        /// <summary>
        /// 플립 홀더 동작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_FlipHolder_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 플립 홀더를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_HolderFlip = true;
            ml.ManualStart(eSequence.Seq00_MPC_Rail1);
        }

        /// <summary>
        /// UV존에 있는 팔렛 맨 왼쪽으로 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_UVPalletsMoveFarLeft_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual UV 팔렛 맨 왼쪽으로 이동하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_MPC2_MoveFarLeft = true;
            ml.ManualStart(eSequence.Seq01_MPC_Rail2_1);
        }

        /// <summary>
        /// 팔렛 Rear 위치에서 홀더 공급존으로 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_FarLeftPalletsMoveHolderSupply_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual Rear에서 홀더 공급으로 이동하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_FarLeft_Y_MoveHolser = true;
            ml.ManualStart(eSequence.Seq01_MPC_Rail2_1);
        }

        /// <summary>
        /// 홀더 공급에서 Front로 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_FarLeftPalletsMoveMPC1_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 홀더 공급에서 Front로 이동하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_FarLeft_Y_MoveMPC1 = true;
            ml.ManualStart(eSequence.Seq01_MPC_Rail2_1);
        }

        /// <summary>
        /// 팔렛 Front위치에서 Rear 위치로 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_FarRight_Y_MoveMPC2_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual Front에서 Rear로 이동하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_FarRight_Y_MoveMPC2 = true;
            ml.ManualStart(eSequence.Seq02_MPC_Rail2_2);
        }

        /// <summary>
        /// 맨 오른쪽 팔렛 UV 존으로 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_FarRightPalletsMoveUV_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 팔렛을 UV존으로 이동하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_FarRightPalletsMoveUV = true;
            ml.ManualStart(eSequence.Seq02_MPC_Rail2_2);
        }

        /// <summary>
        /// UV 큐어
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_UV_Cure_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual UV 큐어 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_UV_Cure = true;
            ml.ManualStart(eSequence.Seq02_MPC_Rail2_2);
        }

        /// <summary>
        /// MPC 자동 간격이동 테스트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_AutoPitchMoveTest_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual MPC 자동 간격이동 테스트를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_MPCAutoPitchMoveTest = true;
            ml.ManualStart(eSequence.Seq00_MPC_Rail1);
        }

        /// <summary>
        /// 호퍼 파이프, 니들 공급 동작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_HopperRun_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 파이프, 니들을 공급하기위해 호퍼를 동작하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_HopperRun = true;
            ml.ManualStart(eSequence.Seq04_Hopper);
        }

        /// <summary>
        /// 파이프 PnP 픽업 동작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_PipePnPPickUp_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 파이프 PnP 픽업을 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_PipePnPPickUp = true;
            ml.ManualStart(eSequence.Seq05_Pipe_PnP);
        }

        /// <summary>
        /// 파이프 PnP 플레이스 동작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_PipePnPPlace_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 파이프 PnP 플레이스를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_PipePnPPlace = true;
            ml.ManualStart(eSequence.Seq05_Pipe_PnP);
        }

        /// <summary>
        /// 파이프 PnP 안전위치 이동 동작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_PipePnPSafe_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 파이프 PnP 안전위치를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_PipePnPSafe = true;
            ml.ManualStart(eSequence.Seq05_Pipe_PnP);
        }

        /// <summary>
        /// 니들 PnP 픽업 동작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_NeedlePnPPickUp_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 니들 PnP 픽업을 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_NeedlePnPPickUp = true;
            ml.ManualStart(eSequence.Seq06_Needle_PnP);
        }

        /// <summary>
        /// 니들 PnP 플레이스 동작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_NeedlePnPPlace_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 니들 PnP 플레이스를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_NeedlePnPPlace = true;
            ml.ManualStart(eSequence.Seq06_Needle_PnP);
        }

        /// <summary>
        /// 니들 PnP 안전위치 이동 동작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_NeedlePnPSafe_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 니들 PnP 안전위치를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_NeedlePnPSafe = true;
            ml.ManualStart(eSequence.Seq06_Needle_PnP);
        }

        /// <summary>
        /// 트렌스퍼 파이프 받기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_PipeTransferRecv_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 트렌스퍼 파이프 받기를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_TransferRecv = true;
            ml.ManualStart(eSequence.Seq07_Transfer1);
        }

        /// <summary>
        /// 트랜스퍼 파이프 보내기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_PipeTransferSend_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 트렌스퍼 파이프 보내기를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_TransferSend = true;
            ml.ManualStart(eSequence.Seq07_Transfer1);
        }

        /// <summary>
        /// 트렌스퍼 니들 받기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_NeedleTransferRecv_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 트렌스퍼 니들 받기를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_TransferRecv = true;
            ml.ManualStart(eSequence.Seq07_Transfer2);
        }

        /// <summary>
        /// 트랜스퍼 니들 보내기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_NeedleTransferSend_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 트렌스퍼 니들 보내기를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_TransferSend = true;
            ml.ManualStart(eSequence.Seq07_Transfer2);
        }

        /// <summary>
        /// 파이프 마운터 클램프 업
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_PipeMounterClampUp_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 파이프 클램프 업을 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_PipeMounterClampUp = true;
            ml.ManualStart(eSequence.Seq08_Pipe_Mount);
        }

        /// <summary>
        /// 홀더에 파이프 마운트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_PipeMount_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 홀더에 파이프 마운트를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_PipeMount = true;
            ml.ManualStart(eSequence.Seq08_Pipe_Mount);
        }

        /// <summary>
        /// 파이프 마운터 안전위치 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_PipeMounterSafe_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 파이프 마운터 안전위치 이동하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_PipeMounterSafe = true;
            ml.ManualStart(eSequence.Seq08_Pipe_Mount);
        }

        /// <summary>
        /// 강제 파이프 마운트 동작 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_PipeMountNotInsp_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "강제 파이프 마운트를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_PipeMountNotInsp = true;
            ml.ManualStart(eSequence.Seq08_Pipe_Mount);
        }

        /// <summary>
        /// 홀더 파이프 누름 동작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_JustPipePush_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "파이프 누름 동작을 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_JustPipePush = true;
            ml.ManualStart(eSequence.Seq08_Pipe_Mount);
        }

        /// <summary>
        /// 파이프 홀더 마그네틱 인젝션 동작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_MagneticInjection_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "마그네틱 인젝션을 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_MagneticInjection = true;
            ml.ManualStart(eSequence.Seq08_Pipe_Mount);
        }

        /// <summary>
        /// 니들 마운터 클램프 업
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_NeedleMounterClampUp_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 니들 클램프 업을 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_NeedleMounterClampUp = true;
            ml.ManualStart(eSequence.Seq09_Needle_Mount);
        }

        /// <summary>
        /// 니들 마운트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_NeedleMount_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 니들 마운트를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_NeedleMount = true;
            ml.ManualStart(eSequence.Seq09_Needle_Mount);
        }

        /// <summary>
        /// 니들 마운터 안전위치 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_NeedleMounterSafe_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 니들 마운터 안전위치 이동하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_NeedleMounterSafe = true;
            ml.ManualStart(eSequence.Seq09_Needle_Mount);
        }

        /// <summary>
        /// 강제 니들 마운트 동작 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_NeedleMountNotInsp_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "강제 니들 마운트를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_NeedeleMountNotInsp = true;
            ml.ManualStart(eSequence.Seq09_Needle_Mount);
        }

        /// <summary>
        /// 홀더 니들 누름 동작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_JustNeedlePush_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "니들 누름 동작을 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_JustNeedlePush = true;
            ml.ManualStart(eSequence.Seq09_Needle_Mount);
        }

        /// <summary>
        /// 푸쉬 없는 니들 마운트 동작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_NeedleMountNotPush_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "푸쉬 없는 니들 마운트 동작을 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_NeedleMountNotPush = true;
            ml.ManualStart(eSequence.Seq09_Needle_Mount);
        }

        /// <summary>
        /// 홀더 디스펜싱
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_HolderDispensing_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 홀더 디스펜싱을 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_HolderDispensing = true;
            ml.ManualStart(eSequence.Seq10_Dispensing);
        }

        /// <summary>
        /// 디스펜서 클린
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_DispenserClean_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 디스펜서 클린을 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_DispenserClean = true;
            ml.ManualStart(eSequence.Seq10_Dispensing);
        }

        /// <summary>
        /// 디스펜서 안전위치 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_DispenserSafe_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 디스펜서 안전위치 이동하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_DispenserSafe = true;
            ml.ManualStart(eSequence.Seq10_Dispensing);
        }

        /// <summary>
        /// 디스펜서 Z 캘리브레이션
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manual_DispenserZCal_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "Manual 디스펜서 Z 캘리브레이션을 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_DispenserZCal = true;
            ml.ManualStart(eSequence.Seq10_Dispensing);
        }

        /// <summary>
        /// 디스펜서 토출 횟수 초기화
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispWorkCountClear_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessageMini(1, "디스펜스 토출 횟수를 초기화하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cSysOne.iDispWorkCount = 0;
            CXMLProcess.WriteXml(CXMLProcess.SystemParameterSingleFilePath, ml.cSysOne);
        }

        private void Manual_PipeHopperRunTest_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "파이프 호퍼 테스트를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_PipeHopperRunTest = true;
            ml.ManualStart(eSequence.Seq04_Hopper);
        }

        private void Manual_NeedleHopperRunTest_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "니들 호퍼 테스트를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_NeedleHopperRunTest = true;
            ml.ManualStart(eSequence.Seq04_Hopper);
        }

        private void Manual_BothHopperRunTest_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "두개의 호퍼 테스트를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cVar.Manual_BothHopperRunTest = true;
            ml.ManualStart(eSequence.Seq04_Hopper);
        }

        #endregion Manual 버튼 기능 정의

        private void Manual_PipeHopperCW_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.Axis[eMotor.PIPE_HOPPER].MoveVelocity(true);
        }

        private void Manual_PipeHopperStop_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.Axis[eMotor.PIPE_HOPPER].Stop();
        }

        private void Manual_NeedleHopperCW_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.Axis[eMotor.NEEDLE_HOPPER].MoveVelocity(true);
        }

        private void Manual_NeedleHopperStop_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.Axis[eMotor.NEEDLE_HOPPER].Stop();
        }

        private void Manual_FeederHome_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "피더 초기화를 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            ml.AIM_Feeder.Home();
        }

        private void Manual_FeederRunTest_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            if (CCommon.ShowMessage(1, "피더 바이브레이터 진행하시겠습니까?") != (int)eMBoxRtn.A_OK) return;
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.AIM_Feeder.FeederRun(cSysArray.uiFeederRunParam[0],
                                    cSysArray.uiFeederRunParam[1],
                                    cSysArray.uiFeederRunParam[2]);
        }

        private void UVon_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cUV_Ctl.UV_On();
        }

        private void UVoff_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.cUV_Ctl.UV_Off();
        }

        private void FeederBackLightOn_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.AIM_Feeder.LED_On();
        }

        private void FeederBackLightOff_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;
            ml.AIM_Feeder.LED_Off();
        }

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
    }
}