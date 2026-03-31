using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MachineControlBase
{
    /// <summary>
    /// UserSetUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OptionUI : UserControl
    {
        /// <summary>
        /// Main Library
        /// </summary>
        private CMainLib ml = null;

        /// <summary>
        /// 옵션 값 클래스
        /// </summary>
        private COptionData cOptionData = null;

        /// <summary>
        /// SystemParameterArray
        /// </summary>
        private CSystemParameterArray cSysArray;

        /// <summary>
        /// 숫자 입력 클래스
        /// </summary>
        private NumPadUI m_NumPadUI = null;

        public OptionUI()
        {
            InitializeComponent();
            ml = CMainLib.Ins;
            cOptionData = CMainLib.Ins.cOptionData;

            if (cOptionData.cVisionPipeMountOffsetList.Count == 0)
            {
                for (uint p = 0; p < 19; p++)
                {
                    PipeNeedleOffsetList cPipeOffset = new PipeNeedleOffsetList();
                    cPipeOffset.uiHolderNo = p + 1;
                    cOptionData.cVisionPipeMountOffsetList.Add(cPipeOffset);
                }
            }

            if (cOptionData.cVisionNeedleMountOffsetList.Count == 0)
            {
                for (uint n = 0; n < 19; n++)
                {
                    PipeNeedleOffsetList cNeedleOffset = new PipeNeedleOffsetList();
                    cNeedleOffset.uiHolderNo = n + 1;
                    cOptionData.cVisionNeedleMountOffsetList.Add(cNeedleOffset);
                }
            }

            for (int i = 0; i < cOptionData.cVisionPipeMountOffsetList.Count; i++)
            {
                cbPipeMountOffset.Items.Add($"Holder Offset {i + 1}");
            }
            cbPipeMountOffset.SelectedIndex = 0;

            for (int i = 0; i < cOptionData.cVisionNeedleMountOffsetList.Count; i++)
            {
                cbNeedleMountOffset.Items.Add($"Needle Offset {i + 1}");
            }
            cbNeedleMountOffset.SelectedIndex = 0;
        }

        /// <summary>
        /// 포커스를 얻거나 잃을 때
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                cSysArray = ml.cSysParamCollData.GetSysArray();
                DataLoad();
            }
            else { }
        }

        /// <summary>
        /// User Set Data를 모두 Load 한다.
        /// </summary>
        private void DataLoad()
        {
            #region Common Data Load

            UIAllAutoRatio.SetData(ml.cSysOne.iAllAutoRatio);
            UIMotorTimeOut.SetData(ml.cSysOne.iMotorTimeOut);
            UISensorCheckTimeOut.SetData(ml.cSysOne.iSensorCheckTimeOut);
            UICylinderRepeatTime.SetData(ml.cSysOne.iCylinderRepeatTime);

            #endregion Common Data Load

            //CbLanguage.SelectedIndex = cOptionData.iLanguageMode;

            TbHolderInspSkip.IsChecked = cOptionData.bHolderInspSkip;
            TbNeedlePosCheckTwice.IsChecked = cOptionData.bNeedlePosCheckTwice;

            TbImage_SaveUse.IsChecked = cOptionData.bImage_SaveUse;
            TbImage_NGSaveUse.IsChecked = cOptionData.bImage_NGSaveUse;

            TbFluorescentUse.IsChecked = cOptionData.bFluorescentUse;
            TbBuzzerUse.IsChecked = cOptionData.bBuzzerUse;
            TbStartInitUse.IsChecked = cOptionData.bOriginInitialize;
            TbDryRunUse.IsChecked = cOptionData.bDryRunUse;

            TbAutoDeleteUse.IsChecked = cOptionData.bAutoDeleteUse;
            UIAutoDeleteDay.SetData(cOptionData.iAutoDeleteDays);

            TbDataAutoBackupUse.IsChecked = cOptionData.bAutoBackupUse;
            CbDataAutoBackupIndex.SelectedIndex = cOptionData.iBackupDateIndex;

            TbDoorLockUse.IsChecked = cOptionData.bDoorLockUse;

            TbAlarmTimeUse.IsChecked = cOptionData.bAlarmTimeUse;
            UIAlarmTime.SetData(cOptionData.iAlarmTime);

            // 외부기기 통신 Address
            TbLightSerialUse.IsChecked = cOptionData.bLightSerialUse;
            UILightSerialComPort.SetData(cOptionData.strLightSerialComPort);
            UIUV_CtlComPort.SetData(cOptionData.strUV_ComPort);
            UIAIM_FeederIP.SetData(cOptionData.strAIM_FeederIP);

            // 카메라0 Calibration 값
            UICAM0_PipePnP_X_CameraCal.SetData(cSysArray.dCAM0_PipePnP_X_CameraCal);
            UICAM0_PipePnP_Y_CameraCal.SetData(cSysArray.dCAM0_PipePnP_Y_CameraCal);
            UICAM0_NeedlePnP_X_CameraCal.SetData(cSysArray.dCAM0_NeedlePnP_X_CameraCal);
            UICAM0_NeedlePnP_Y_CameraCal.SetData(cSysArray.dCAM0_NeedlePnP_Y_CameraCal);
            UICAM0_PipeMount_X_CameraCal.SetData(cSysArray.dCAM3_PipeMount_X_CameraCal);
            UICAM0_PipeMount_Y_CameraCal.SetData(cSysArray.dCAM3_PipeMount_Y_CameraCal);
            UICAM0_NeedleMount_X_CameraCal.SetData(cSysArray.dCAM4_NeedleMount_X_CameraCal);
            UICAM0_NeedleMount_Y_CameraCal.SetData(cSysArray.dCAM4_NeedleMount_Y_CameraCal);

            // 카메라1, 파이프 각도
            UICAM1_PipeDegreeLimit.SetData(cSysArray.dCAM1_TransferPipeDegreeLimit);

            // 카메라2, 니들 각도와 뾰족한 부분 Gap
            UICAM2_NeedleDegreeLimit.SetData(cSysArray.dCAM2_TransferNeedleDegreeLimit);
            UICAM2_NeedleSharpGap.SetData(cSysArray.dCAM2_TransferNeedleSharpGap);

            // 카메라3, 파이프 마운트 캘리브레이션 Limit
            UICAM3_PipeMountCal_X_Limit.SetData(cSysArray.dCAM3_PipeMountCal_X_Limit);
            UICAM3_PipeMountCal_Y_Limit.SetData(cSysArray.dCAM3_PipeMountCal_Y_Limit);

            // 카메라4, 니들 마운트 캘리브레이션 Limit
            UICAM4_NeedleMountCal_X_Limit.SetData(cSysArray.dCAM4_NeedleMountCal_X_Limit);
            UICAM4_NeedleMountCal_Y_Limit.SetData(cSysArray.dCAM4_NeedleMountCal_Y_Limit);

            // 카메라5, 디스펜서 캘리브레이션 Limit
            UICAM5_Disp_X_CameraCal.SetData(cSysArray.dCAM5_Disp_X_CameraCal);
            UICAM5_Disp_Y_CameraCal.SetData(cSysArray.dCAM5_Disp_Y_CameraCal);
        }

        /// <summary>
        /// Save 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BTSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            #region Common Data Save

            UIAllAutoRatio.GetData(ref ml.cSysOne.iAllAutoRatio);
            UIMotorTimeOut.GetData(ref ml.cSysOne.iMotorTimeOut);
            UISensorCheckTimeOut.GetData(ref ml.cSysOne.iSensorCheckTimeOut);
            UICylinderRepeatTime.GetData(ref ml.cSysOne.iCylinderRepeatTime);

            #endregion Common Data Save

            DataChangeCheck(TbHolderInspSkip, ref cOptionData.bHolderInspSkip);
            DataChangeCheck(TbNeedlePosCheckTwice, ref cOptionData.bNeedlePosCheckTwice);

            DataChangeCheck(TbImage_SaveUse, ref cOptionData.bImage_SaveUse);
            if (cOptionData.bImage_SaveUse == true &&
                CMainLib.Ins.cimageSaveProcess == null) CMainLib.Ins.cimageSaveProcess = new ImageSaveProcess();
            DataChangeCheck(TbImage_NGSaveUse, ref cOptionData.bImage_NGSaveUse);

            //cOptionData.iLanguageMode = CbLanguage.SelectedIndex;

            DataChangeCheck(TbFluorescentUse, ref cOptionData.bFluorescentUse);
            CMainLib.Ins.Seq.SeqIO.SetOutput((int)eIO_O.FLUORESCENT_LAMP_ONOFF, cOptionData.bFluorescentUse);

            DataChangeCheck(TbBuzzerUse, ref cOptionData.bBuzzerUse);
            DataChangeCheck(TbStartInitUse, ref cOptionData.bOriginInitialize);
            DataChangeCheck(TbDryRunUse, ref cOptionData.bDryRunUse);

            DataChangeCheck(TbAutoDeleteUse, ref cOptionData.bAutoDeleteUse);
            UIAutoDeleteDay.GetData(ref cOptionData.iAutoDeleteDays);

            DataChangeCheck(TbDataAutoBackupUse, ref cOptionData.bAutoBackupUse);
            cOptionData.iBackupDateIndex = CbDataAutoBackupIndex.SelectedIndex;
            cOptionData.strBackupDate = CbDataAutoBackupIndex.Text;

            DataChangeCheck(TbDoorLockUse, ref cOptionData.bDoorLockUse);

            // 도어락 상태 바로 적용
            //if (cOptionData.bDoorLockUse == true)
            //{
            //    CMainLib.Ins.Seq.SeqIO.SetOutput((int)eIO_O.SOFTREWE_BY_PASS_ON_OFF, false);
            //}
            //else
            //{
            //    CMainLib.Ins.Seq.SeqIO.SetOutput((int)eIO_O.SOFTREWE_BY_PASS_ON_OFF, true);
            //    CMainLib.Ins.Seq.SeqIO.SetOutput((int)eIO_O.DOOR_SOLENOID_LOCK_ONOFF, false);
            //}

            DataChangeCheck(TbAlarmTimeUse, ref cOptionData.bAlarmTimeUse);
            UIAlarmTime.GetData(ref cOptionData.iAlarmTime);

            DataChangeCheck(TbLightSerialUse, ref cOptionData.bLightSerialUse);

            // 외부기기 통신 Address
            UILightSerialComPort.GetData(ref cOptionData.strLightSerialComPort);
            UIUV_CtlComPort.GetData(ref cOptionData.strUV_ComPort);
            UIAIM_FeederIP.GetData(ref cOptionData.strAIM_FeederIP);

            // 카메라0, Offset 캘리브레이션
            UICAM0_PipePnP_X_CameraCal.GetData(ref cSysArray.dCAM0_PipePnP_X_CameraCal);
            UICAM0_PipePnP_Y_CameraCal.GetData(ref cSysArray.dCAM0_PipePnP_Y_CameraCal);
            UICAM0_NeedlePnP_X_CameraCal.GetData(ref cSysArray.dCAM0_NeedlePnP_X_CameraCal);
            UICAM0_NeedlePnP_Y_CameraCal.GetData(ref cSysArray.dCAM0_NeedlePnP_Y_CameraCal);
            UICAM0_PipeMount_X_CameraCal.GetData(ref cSysArray.dCAM3_PipeMount_X_CameraCal);
            UICAM0_PipeMount_Y_CameraCal.GetData(ref cSysArray.dCAM3_PipeMount_Y_CameraCal);
            UICAM0_NeedleMount_X_CameraCal.GetData(ref cSysArray.dCAM4_NeedleMount_X_CameraCal);
            UICAM0_NeedleMount_Y_CameraCal.GetData(ref cSysArray.dCAM4_NeedleMount_Y_CameraCal);

            // 카메라1, 파이프 각도
            UICAM1_PipeDegreeLimit.GetData(ref cSysArray.dCAM1_TransferPipeDegreeLimit);

            // 카메라2, 니들 각도와 뾰족한 부분 Gap
            UICAM2_NeedleDegreeLimit.GetData(ref cSysArray.dCAM2_TransferNeedleDegreeLimit);
            UICAM2_NeedleSharpGap.GetData(ref cSysArray.dCAM2_TransferNeedleSharpGap);

            // 카메라3, 파이프 마운트 캘리브레이션 Limit
            UICAM3_PipeMountCal_X_Limit.GetData(ref cSysArray.dCAM3_PipeMountCal_X_Limit);
            UICAM3_PipeMountCal_Y_Limit.GetData(ref cSysArray.dCAM3_PipeMountCal_Y_Limit);

            PipeNeedleOffsetList Pipeoffset = cOptionData.cVisionPipeMountOffsetList[cbPipeMountOffset.SelectedIndex];
            Pipeoffset.X_Offset = double.Parse(tbHolderPipeOffset_X.Text);
            Pipeoffset.Y_Offset = double.Parse(tbHolderPipeOffset_Y.Text);

            // 카메라4, 니들 마운트 캘리브레이션 Limit
            UICAM4_NeedleMountCal_X_Limit.GetData(ref cSysArray.dCAM4_NeedleMountCal_X_Limit);
            UICAM4_NeedleMountCal_Y_Limit.GetData(ref cSysArray.dCAM4_NeedleMountCal_Y_Limit);

            PipeNeedleOffsetList Needeoffset = cOptionData.cVisionNeedleMountOffsetList[cbNeedleMountOffset.SelectedIndex];
            Needeoffset.X_Offset = double.Parse(tbHolderNeedleOffset_X.Text);
            Needeoffset.Y_Offset = double.Parse(tbHolderNeedleOffset_Y.Text);

            // 카메라5, 디스펜서 카메라 캘리브레이션
            UICAM5_Disp_X_CameraCal.GetData(ref cSysArray.dCAM5_Disp_X_CameraCal);
            UICAM5_Disp_Y_CameraCal.GetData(ref cSysArray.dCAM5_Disp_Y_CameraCal);

            CXMLProcess.WriteXml(CXMLProcess.SystemParameterCollectionDataFilePath, CMainLib.Ins.cSysParamCollData);
            CXMLProcess.WriteXml(CXMLProcess.SystemParameterSingleFilePath, CMainLib.Ins.cSysOne);
            CXMLProcess.WriteXml(CXMLProcess.OptionDataFilePath, CMainLib.Ins.cOptionData);
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
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbPipeMountOffset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PipeNeedleOffsetList Pipeoffset = cOptionData.cVisionPipeMountOffsetList[cbPipeMountOffset.SelectedIndex];
            tbHolderPipeOffset_X.Text = Pipeoffset.X_Offset.ToString();
            tbHolderPipeOffset_Y.Text = Pipeoffset.Y_Offset.ToString();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbNeedleMountOffset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PipeNeedleOffsetList Needeoffset = cOptionData.cVisionNeedleMountOffsetList[cbNeedleMountOffset.SelectedIndex];
            tbHolderNeedleOffset_X.Text = Needeoffset.X_Offset.ToString();
            tbHolderNeedleOffset_Y.Text = Needeoffset.Y_Offset.ToString();
        }

        /// <summary>
        /// Key Pad Open
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Value_Changer(object sender, MouseButtonEventArgs e)
        {
            if (ml.McState == eMachineState.RUN || ml.McState == eMachineState.MANUALRUN) return;

            TextBlock tb = sender as TextBlock;

            m_NumPadUI = new NumPadUI();

            if (m_NumPadUI.bOpened == false)
            {
                m_NumPadUI.Init(tb);
                m_NumPadUI.ShowDialog();
            }
            else m_NumPadUI.Close();
        }

        /// <summary>
        /// 피더 재연결 버튼
        /// 피더쪽 케이블 접촉불량으로 인해 피더 꺼졌을 때 다시 재연결 하기 위해 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtFeederReconnect_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // AIM 피더 연결 해제
            ml.AIM_Feeder.Free();
            // AIM 피더 이니셜
            ml.AIM_Feeder.Init();
            // AIM 피더 Connect
            ml.AIM_Feeder.Connect(0, eLogType.AIM_Feeder, CMainLib.Ins.cOptionData.strAIM_FeederIP, 1470);
            // LED 백라이트 On
            ml.AIM_Feeder.LED_On();
            CCommon.ShowMessageMini(0, "피더와의 통신이 재연결되었습니다.");
        }
    }
}