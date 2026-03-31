using Cognex.VisionPro;
using Cognex.VisionPro.Exceptions;
using Cognex.VisionPro.Implementation;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// VisionToolBlockUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class VisionToolBlockUI : UserControl
    {
        /// <summary>
        /// Main Lib
        /// </summary>
        private CMainLib ml = null;

        /// <summary>
        /// Camera No
        /// </summary>
        private uint uiCameraNo = 0;

        /// <summary>
        /// Vision ToolBlock Lib
        /// </summary>
        private CVisionToolBlockLib cCVisionToolBlockLib = null;

        /// <summary>
        /// 카메라 Trigger Mode 사용 유무
        /// </summary>
        private bool bTriggerMode = false;

        /// <summary>
        /// 생성자
        /// </summary>
        public VisionToolBlockUI()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 초기화 함수
        /// </summary>
        public void Init(CVisionToolBlockLib cCVisionToolBlockLib, uint uiCameraNo)
        {
            ml = CMainLib.Ins;
            bTriggerMode = ml.cVisionData.bTriggerMode[uiCameraNo];
            this.cCVisionToolBlockLib = cCVisionToolBlockLib;
            this.uiCameraNo = uiCameraNo;

            // Tool Block 필요한 개 수만큼 콤보박스 데이터 추가
            for (int i = 0; i < ml.cVisionData.strToolBlockName[uiCameraNo].Length; i++)
                CbToolBlockList.Items.Add(ml.cVisionData.strToolBlockName[uiCameraNo][i]);
            CbToolBlockList.SelectedIndex = 0;

            // 라디오 버튼 그룹명이 중복되지 않도록 정의
            RbCameraLiveCam.GroupName = ((eCAM)uiCameraNo).ToString();
            RbCameraLiveFile.GroupName = ((eCAM)uiCameraNo).ToString();
            RbCameraLiveCam.IsChecked = true;

            // Vision 화면 이름 정의
            TbTitle.Text = ((eCAM)uiCameraNo).ToString();

            // 화면 분할 값 표시
            DisplayIndex_DataLoad();

            // Vision 표시 화면 개수 정의
            uint uiXIndex = 0, uiYIndex = 0;
            ml.cOptionData.GetDisplayXYIndex(uiCameraNo, ref uiXIndex, ref uiYIndex);
            ViewInit(uiXIndex, uiYIndex);
            // 실행 초기 셋업 모드로 화면 표시(Vision 화면 개 수에 상관없이 Main 화면 하나만 보여줌)
            SetDisplayMode(false);
        }

        private CogRecordDisplay[] cogRecordDisplays = null;
        private WindowsFormsHost[] windowsFormsHosts = null;
        private ColumnDefinition[] columnDefinition = null;
        private RowDefinition[] rowDefinition = null;

        /// <summary>
        /// Vision 메인 화면을 개 수 만큼 분할하여 View를 생성
        /// </summary>
        /// <param name="uiXIndex"></param>
        /// <param name="uiYIndex"></param>
        public void ViewInit(uint uiXIndex, uint uiYIndex)
        {
            columnDefinition = new ColumnDefinition[uiXIndex];
            rowDefinition = new RowDefinition[uiYIndex];

            // Grid 화면 분할
            for (int i = 0; i < uiXIndex; i++)
            {
                columnDefinition[i] = new ColumnDefinition();
                columnDefinition[i].Width = new GridLength(1, GridUnitType.Star);
                GdMainView.ColumnDefinitions.Add(columnDefinition[i]);
            }
            for (int i = 0; i < uiYIndex; i++)
            {
                rowDefinition[i] = new RowDefinition();
                rowDefinition[i].Height = new GridLength(1, GridUnitType.Star);
                GdMainView.RowDefinitions.Add(rowDefinition[i]);
            }

            // CogDisplay 생성
            cogRecordDisplays = new CogRecordDisplay[uiXIndex * uiYIndex];
            windowsFormsHosts = new WindowsFormsHost[uiXIndex * uiYIndex];

            int iIndex = 0;
            // CogDisPlay Grid 배치
            for (int i = 0; i < uiYIndex; i++)
            {
                // Vision 화면 추가
                for (int j = 0; j < uiXIndex; j++)
                {
                    windowsFormsHosts[iIndex] = new WindowsFormsHost();
                    cogRecordDisplays[iIndex] = new CogRecordDisplay();
                    windowsFormsHosts[iIndex].Child = cogRecordDisplays[iIndex];
                    Grid.SetRow(windowsFormsHosts[iIndex], i);
                    Grid.SetColumn(windowsFormsHosts[iIndex], j);
                    GdMainView.Children.Add(windowsFormsHosts[iIndex]);
                    iIndex++;
                }
            }

            //// CogDisplay 파라미터 설정 (BeginInvoke 쓰지 않으면 GdMainView에 추가 전이기 때문에 에러 발생)
            //foreach (CogRecordDisplay cogRecordDisplay in cogRecordDisplays)
            //{
            //    Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate ()
            //    {
            //        cogRecordDisplay.HorizontalScrollBar = false;
            //        cogRecordDisplay.VerticalScrollBar = false;
            //        cogRecordDisplay.BackColor = System.Drawing.Color.Black;
            //    });
            //}

            // 조명 값 불러옴
            CVisionLight cVisionLight = ml.cOptionData.GetVisionLight(uiCameraNo);
            TbLightValue.Text = cVisionLight.uiLightValue.ToString();
        }

        /// <summary>
        /// CogDisplay 파라미터 설정
        /// </summary>
        public void CogDisplayParamSetting()
        {
            foreach (CogRecordDisplay cogRecordDisplay in cogRecordDisplays)
            {
                // BeginInvoke 쓰지 않으면 GdMainView에 추가 전이기 때문에 에러 발생
                Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate ()
                {
                    cogRecordDisplay.HorizontalScrollBar = false;
                    cogRecordDisplay.VerticalScrollBar = false;
                    cogRecordDisplay.BackColor = System.Drawing.Color.Black;
                });
            }
        }

        /// <summary>
        /// 어떤 Display에 데이터를 표시할지 정하는 Index
        /// </summary>
        public int iCogDisplayIndex = 0;

        /// <summary>
        /// CogDisplay를 리턴한다.
        /// </summary>
        /// <returns></returns>
        public CogRecordDisplay GetCogDisplay()
        {
            //if (CMainLib.Ins.McState == Define.eMachineState.READY) return cogRecordDisplays[0];
            return cogRecordDisplays[iCogDisplayIndex];
        }

        /// <summary>
        /// CogDisplay를 리턴한다.
        /// </summary>
        /// <param name="iDisplayNo"></param>
        /// <returns></returns>
        public CogRecordDisplay GetCogDisplay(int iDisplayNo)
        {
            //if (CMainLib.Ins.McState == Define.eMachineState.READY) return cogRecordDisplays[0];
            return cogRecordDisplays[iDisplayNo];
        }

        /// <summary>
        /// 코그넥스 디스플레이 클리어 및 화면 오토 설정
        /// </summary>
        public void DisplayClear()
        {
            cogRecordDisplays[iCogDisplayIndex].InteractiveGraphics.Clear();
            cogRecordDisplays[iCogDisplayIndex].StaticGraphics.Clear();
            cogRecordDisplays[iCogDisplayIndex].AutoFit = true;
            cogRecordDisplays[iCogDisplayIndex].Fit();
        }

        /// <summary>
        /// 코그넥스 디스플레이 클리어 및 화면 오토 설정
        /// </summary>
        /// <param name="iDisplayNo"></param>
        public void DisplayClear(int iDisplayNo)
        {
            cogRecordDisplays[iDisplayNo].InteractiveGraphics.Clear();
            cogRecordDisplays[iDisplayNo].StaticGraphics.Clear();
            cogRecordDisplays[iDisplayNo].AutoFit = true;
            cogRecordDisplays[iDisplayNo].Fit();
        }

        /// <summary>
        /// InteractiveGraphics Add CogRecordDisplay
        /// </summary>
        /// <param name="graphic"></param>
        /// <param name="groupName"></param>
        /// <param name="checkForDuplicates"></param>
        public void AddInteractiveGraphics(ICogGraphicInteractive graphic, string groupName, bool checkForDuplicates)
        {
            int iNo = iCogDisplayIndex;
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate ()
            {
                GetCogDisplay(iNo).InteractiveGraphics.Add(graphic, groupName, checkForDuplicates);
            });
        }

        /// <summary>
        /// InteractiveGraphics Add CogRecordDisplay
        /// </summary>
        /// <param name="iDisplayIndex"></param>
        /// <param name="graphic"></param>
        /// <param name="groupName"></param>
        /// <param name="checkForDuplicates"></param>
        public void AddInteractiveGraphics(int iDisplayIndex, ICogGraphicInteractive graphic, string groupName, bool checkForDuplicates)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate ()
            {
                GetCogDisplay(iDisplayIndex).InteractiveGraphics.Add(graphic, groupName, checkForDuplicates);
            });
        }

        /// <summary>
        /// Display에 CogRecord 설정
        /// </summary>
        /// <param name="cogRecord"></param>
        public void CogRecord(CogRecord cogRecord)
        {
            int iNo = iCogDisplayIndex;
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate ()
            {
                CogRecordDisplay cogRecordDisplay = GetCogDisplay(iNo);
                cogRecordDisplay.Record = cogRecord;
                cogRecordDisplay.BackColor = System.Drawing.Color.Black;
            });
        }

        /// <summary>
        /// Display에 CogRecord 설정
        /// </summary>
        /// <param name="iDisplayIndex"></param>
        /// <param name="cogRecord"></param>
        public void CogRecord(int iDisplayIndex, CogRecord cogRecord)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate ()
            {
                CogRecordDisplay cogRecordDisplay = GetCogDisplay(iDisplayIndex);
                cogRecordDisplay.Record = cogRecord;
                cogRecordDisplay.BackColor = System.Drawing.Color.Black;
            });
        }

        /// <summary>
        /// 다음 화면에 표시하도록 Index를 증가 시킨다.
        /// </summary>
        public void CogDisplayNextIndex()
        {
            iCogDisplayIndex++;
            CVisionView cVisionView = ml.cOptionData.GetDisplayIndex(uiCameraNo);
            if (iCogDisplayIndex >= cVisionView.uiDisplayMaxIndex ||
                iCogDisplayIndex >= (cVisionView.uiDisplayXIndex * cVisionView.uiDisplayYIndex))
                iCogDisplayIndex = 0;
        }

        /// <summary>
        /// 화면에 표시하는 index를 초기화한다.
        /// </summary>
        public void ResetIndexCogDisplay()
        {
            iCogDisplayIndex = 0;
            foreach (CogRecordDisplay display in cogRecordDisplays)
            {
                display.InteractiveGraphics.Clear();
            }
        }

        /// <summary>
        /// Start 시 각 분할 화면 표시, 정지 시 Main 화면만 표시하도록 그리드 정의
        /// </summary>
        /// <param name="bSetupMode"></param>
        public void SetDisplayMode(bool bSetupMode)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate ()
            {
                uint uiXIndex = 0, uiYIndex = 0;
                ml.cOptionData.GetDisplayXYIndex(uiCameraNo, ref uiXIndex, ref uiYIndex);

                if (bSetupMode == true)
                {
                    for (int i = 1; i < uiXIndex; i++)
                    {
                        if (columnDefinition.Length <= i) return;
                        columnDefinition[i].Width = new GridLength(0, GridUnitType.Star);
                    }

                    for (int i = 1; i < uiYIndex; i++)
                    {
                        if (rowDefinition.Length <= i) return;
                        rowDefinition[i].Height = new GridLength(0, GridUnitType.Star);
                    }
                }
                else
                {
                    for (int i = 1; i < uiXIndex; i++)
                    {
                        if (columnDefinition.Length <= i) return;
                        columnDefinition[i].Width = new GridLength(1, GridUnitType.Star);
                    }
                    for (int i = 1; i < uiYIndex; i++)
                    {
                        if (rowDefinition.Length <= i) return;
                        rowDefinition[i].Height = new GridLength(1, GridUnitType.Star);
                    }
                }
            });
        }

        /// <summary>
        /// Cross Line 사용 Flag
        /// </summary>
        private bool bCrossLineUseFlag = false;

        /// <summary>
        /// Camera1 Cross Line 설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtCameraCrossLine_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (bCrossLineUseFlag == false)
            {
                bCrossLineUseFlag = true;
                SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.OrangeRed); // Cross Line 정지
                BtCrossLine.Background = solidColorBrush;
                Set_CrossLine();
            }
            else
            {
                bCrossLineUseFlag = false;
                SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Gray); // Cross Line 정지
                BtCrossLine.Background = solidColorBrush;
                Set_CrossLineDelete();
            }
        }

        /// <summary>
        /// Camera Live 설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtCameraLive_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN) return;
            if (ml.GetUserLevelCheck((int)eUserLevel.ENGINEER) == false) return;

            DisplayClear();
            // 라이브 설정
            if (cCVisionToolBlockLib.iAcqConnnection == true && RbCameraLiveCam.IsChecked == true)
            {
                if (GetCogDisplay().LiveDisplayRunning == true)
                {
                    if (bTriggerMode == true) cCVisionToolBlockLib.TriggerEnable(true);
                    GetCogDisplay().StopLiveDisplay();
                    RdCameraLiveEnable(true);
                    SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Gray); // Live 정지
                    BtCameraLive.Background = solidColorBrush;
                }
                else if (cCVisionToolBlockLib.cAcqFifoTool.Operator != null)
                {
                    if (bTriggerMode == true) cCVisionToolBlockLib.TriggerEnable(false);
                    GetCogDisplay().StartLiveDisplay(cCVisionToolBlockLib.cAcqFifoTool.Operator, false);
                    RdCameraLiveEnable(false);
                    SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.OrangeRed); // Live 시작
                    BtCameraLive.Background = solidColorBrush;
                }
            }
            else
            {
                CCommon.ShowMessageMini("카메라로 설정되어 있지 않습니다.");
            }
        }

        /// <summary>
        /// 카메라가 라이브일 경우 Off 한다.
        /// </summary>
        public void CameraLiveOff()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate ()
            {
                DisplayClear();
                if (cCVisionToolBlockLib.iAcqConnnection == true && RbCameraLiveCam.IsChecked == true)
                {
                    if (GetCogDisplay().LiveDisplayRunning == true)
                    {
                        if (bTriggerMode == true) cCVisionToolBlockLib.TriggerEnable(true);
                        GetCogDisplay().StopLiveDisplay();
                        RdCameraLiveEnable(true);
                        SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Gray); // Live 정지
                        BtCameraLive.Background = solidColorBrush;
                    }
                }
            });
        }

        /// <summary>
        /// Camera Vision 이미지 취득 진행
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtCameraNext_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN) return;
            if (ml.GetUserLevelCheck((int)eUserLevel.ENGINEER) == false) return;

            CogRecordDisplay cogRecordDisplay = GetCogDisplay();
            DisplayClear();

            // cCVisionToolBlockLib 에서 cAcqFifoTool.Run() 을 하기위해서는 ToolBlock No를 정의 해줘야 한다.
            if (cCVisionToolBlockLib != null)
            {
                cCVisionToolBlockLib.uiToolBlockNo = (uint)CbToolBlockList.SelectedIndex;
                if (cCVisionToolBlockLib.cAcqFifoTool != null)
                {
                    if (RbCameraLiveCam.IsChecked == true) // 카메라
                    {
                        // Camera 촬영
                        Parallel.Invoke(() => { cCVisionToolBlockLib.cAcqFifoTool.Run(); });
                    }
                    else // 파일
                    {
                        CogToolBlockClass cCogToolBlockClass = cCVisionToolBlockLib.cCogToolBlockList.Find(x => x.uiToolBlockNo == cCVisionToolBlockLib.uiToolBlockNo);
                        if (cCogToolBlockClass != null) cCogToolBlockClass.cImageFileTool.Run();
                    }
                }
            }
        }

        /// <summary>
        /// Camera Vision 촬영 진행
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtCameraRun_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN) return;
            if (ml.GetUserLevelCheck((int)eUserLevel.ENGINEER) == false) return;

            // CVisionToolBlockLib 에서 cCogToolBlockEditV2.Subject.Run() 을 하기위해서는 ToolBlock No를 정의 해줘야 한다.
            if (cCVisionToolBlockLib != null)
            {
                cCVisionToolBlockLib.uiToolBlockNo = (uint)CbToolBlockList.SelectedIndex;
                CogToolBlockClass cCogToolBlockClass = cCVisionToolBlockLib.cCogToolBlockList.Find(x => x.uiToolBlockNo == cCVisionToolBlockLib.uiToolBlockNo);
                try
                {
                    CogRecordDisplay cogRecordDisplay = GetCogDisplay();
                    DisplayClear();
                    Parallel.Invoke(() =>
                    {
                        cCogToolBlockClass.cCogToolBlockEditV2.Subject.Run();
                        if (cCogToolBlockClass.cCogToolBlockEditV2.Subject.RunStatus.Exception != null &&
                            cCogToolBlockClass.cCogToolBlockEditV2.Subject.RunStatus.Exception.ToString().Contains("CogFixtureTool") == false &&
                            cCogToolBlockClass.cCogToolBlockEditV2.Subject.RunStatus.Exception.ToString().Contains("CogFindLineTool") == false &&
                            cCogToolBlockClass.cCogToolBlockEditV2.Subject.RunStatus.Exception.ToString().Contains("CogIntersectLineLineTool") == false)
                        {
                            // 실패 결과 처리
                            CCommon.ShowMessageMini(cCogToolBlockClass.cCogToolBlockEditV2.Subject.RunStatus.Exception.Message);
                        }
                    });
                }
                catch (CogException cogex)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, cogex.ToString());
                }
            }
        }

        /// <summary>
        /// Cross Line 설정
        /// </summary>
        private void Set_CrossLine()
        {
            CogLine m_cogLine_1 = new CogLine();
            m_cogLine_1.LineStyle = CogGraphicLineStyleConstants.Solid;
            m_cogLine_1.Color = CogColorConstants.Red;

            CogLine m_cogLine_2 = new CogLine();
            m_cogLine_2.LineStyle = CogGraphicLineStyleConstants.Solid;
            m_cogLine_2.Color = CogColorConstants.Red;

            m_cogLine_1.SetFromStartXYEndXY(0,
                                            ml.cVisionData.iScreenHalfHeight[uiCameraNo],
                                            ml.cVisionData.iScreenWidth[uiCameraNo],
                                            ml.cVisionData.iScreenHalfHeight[uiCameraNo]);
            m_cogLine_2.SetFromStartXYEndXY(ml.cVisionData.iScreenHalfWidth[uiCameraNo],
                                            0,
                                            ml.cVisionData.iScreenHalfWidth[uiCameraNo],
                                            ml.cVisionData.iScreenHeight[uiCameraNo]);

            GetCogDisplay().StaticGraphics.Add(m_cogLine_1, "W");
            GetCogDisplay().StaticGraphics.Add(m_cogLine_2, "H");
        }

        /// <summary>
        /// Cross Line 삭제
        /// </summary>
        private void Set_CrossLineDelete()
        {
            GetCogDisplay().StaticGraphics.Clear();
        }

        /// <summary>
        /// Camera Live 의 라디오 버튼 활성화 제어
        /// </summary>
        private void RdCameraLiveEnable(bool bEnable)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate ()
            {
                RbCameraLiveFile.IsEnabled = bEnable;
                RbCameraLiveCam.IsEnabled = bEnable;
            });
        }

        /// <summary>
        /// Button 활성 비활성화
        /// </summary>
        /// <param name="bEnable"></param>
        public void ButtonEnable(bool bEnable)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate ()
            {
                BtCameraLive.IsEnabled = bEnable;
                BtVisionSetup.IsEnabled = bEnable;
            });
        }

        /// <summary>
        /// Vision Log의 쓰레드 등 충돌을 방지하기 위한 델리게이트
        /// </summary>
        /// <param name="strLog"></param>
        private delegate void ListViewDelegate(string strLog);

        /// <summary>
        /// Vision Log Log에 값을 추가하고 많을 경우 초기화 한다.
        /// </summary>
        /// <param name="strLog"></param>
        public void AddVisionLog(string strLog)
        {
            ListViewVisionLog.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                   new ListViewDelegate(UpdateListView), strLog);
        }

        /// <summary>
        /// Vision Log ListView의 데이터를 갱신한다.
        /// </summary>
        /// <param name="strLog"></param>
        private void UpdateListView(string strLog)
        {
            if (ListViewVisionLog.Items.Count > 50)
            {
                ListViewVisionLog.Items.Clear();
            }
            DateTime today = DateTime.Now;
            ListViewVisionLog.Items.Insert(0, string.Format("[{0:T}] ", today) + strLog);
        }

        /// <summary>
        /// Log Popup Flag
        /// </summary>
        private bool bLogShowPopup = false;

        /// <summary>
        /// Log 버튼 클릭 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtLog_Click(object sender, RoutedEventArgs e)
        {
            if (bLogShowPopup == false)
            {
                SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.OrangeRed);
                BtLog.Background = solidColorBrush;
                Popup_VisionLog.IsOpen = true;
            }
            else
            {
                SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Gray);
                BtLog.Background = solidColorBrush;
                Popup_VisionLog.IsOpen = false;
            }
            bLogShowPopup = !bLogShowPopup;
        }

        /// <summary>
        /// Vision Log를 클릭한 경우 View 초기화 한다.
        /// </summary>
        public void LogViewHide()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate ()
            {
                if (bLogShowPopup == true)
                {
                    SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Gray);
                    BtLog.Background = solidColorBrush;
                    Popup_VisionLog.IsOpen = false;
                    bLogShowPopup = false;
                }
            });
        }

        /// <summary>
        /// Setup Popup Flag
        /// </summary>
        private bool bSetupShowPopup = false;

        /// <summary>
        /// Setup 버튼 클릭 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtVisionSetup_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN) return;
            if (ml.GetUserLevelCheck((int)eUserLevel.ENGINEER) == false) return;

            if (bSetupShowPopup == false)
            {
                SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.OrangeRed);
                BtVisionSetup.Background = solidColorBrush;
                Popup_VisionSetup.IsOpen = true;
            }
            else
            {
                SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Gray);
                BtVisionSetup.Background = solidColorBrush;
                Popup_VisionSetup.IsOpen = false;
            }
            bSetupShowPopup = !bSetupShowPopup;
        }

        /// <summary>
        /// Vision Setup 을 클릭한 경우 View를 초기화 한다.
        /// </summary>
        public void SetupViewHide()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate ()
            {
                if (bSetupShowPopup == true)
                {
                    SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Gray);
                    BtVisionSetup.Background = solidColorBrush;
                    Popup_VisionSetup.IsOpen = false;
                    bSetupShowPopup = false;
                }
            });
        }

        /// <summary>
        /// 카메라 설정 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Vision_CameraSet_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN) return;
            if (ml.GetUserLevelCheck((int)eUserLevel.ENGINEER) == false) return;

            var mw = (MainWindow)Application.Current.MainWindow;
            if (CheckNowUseUI.CurrentUIIndex != _UIIndex.MainUI)
            {
                CheckNowUseUI.CurrentUIIndex = _UIIndex.MainUI;
                if (mw != null) mw.MainPanel.Content = MainUI.Ins;
            }
            if (cCVisionToolBlockLib != null) SlideVisionUI.Ins.AcqFifoSetupView(cCVisionToolBlockLib.cAcqFifoTool);
            // 버튼 초기화
            CMainLib.Ins.VisionColorDisable();
        }

        /// <summary>
        /// 이미지 설정 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Vision_ImageLoad_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN) return;
            if (ml.GetUserLevelCheck((int)eUserLevel.ENGINEER) == false) return;

            var mw = (MainWindow)Application.Current.MainWindow;
            if (CheckNowUseUI.CurrentUIIndex != _UIIndex.MainUI)
            {
                CheckNowUseUI.CurrentUIIndex = _UIIndex.MainUI;
                if (mw != null) mw.MainPanel.Content = MainUI.Ins;
            }

            if (cCVisionToolBlockLib != null)
            {
                cCVisionToolBlockLib.uiToolBlockNo = (uint)CbToolBlockList.SelectedIndex;
                CogToolBlockClass cCogToolBlockClass = cCVisionToolBlockLib.cCogToolBlockList.Find(x => x.uiToolBlockNo == cCVisionToolBlockLib.uiToolBlockNo);
                SlideVisionUI.Ins.ImageFileSetupView(cCogToolBlockClass.cImageFileTool);
            }
            // 버튼 초기화
            CMainLib.Ins.VisionColorDisable();
        }

        /// <summary>
        /// ToolBlock 설정 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Vision_ToolBlock_Setup_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ml.McState == eMachineState.RUN) return;
            if (ml.GetUserLevelCheck((int)eUserLevel.ENGINEER) == false) return;

            var mw = (MainWindow)Application.Current.MainWindow;
            if (CheckNowUseUI.CurrentUIIndex != _UIIndex.MainUI)
            {
                CheckNowUseUI.CurrentUIIndex = _UIIndex.MainUI;
                if (mw != null) mw.MainPanel.Content = MainUI.Ins;
            }

            if (cCVisionToolBlockLib != null)
            {
                cCVisionToolBlockLib.uiToolBlockNo = (uint)CbToolBlockList.SelectedIndex;
                CogToolBlockClass cCogToolBlockClass = cCVisionToolBlockLib.cCogToolBlockList.Find(x => x.uiToolBlockNo == cCVisionToolBlockLib.uiToolBlockNo);
                SlideVisionUI.Ins.ToolBlockSetupView(cCogToolBlockClass.cCogToolBlockEditV2);
            }
            // 버튼 초기화
            CMainLib.Ins.VisionColorDisable();
        }

        /// <summary>
        /// 화면 분할 값 표시
        /// </summary>
        public void DisplayIndex_DataLoad()
        {
            uint uiXIndex = 0, uiYIndex = 0;
            uint uiMaxIndex = ml.cOptionData.GetDisplayMaxIndex(uiCameraNo);
            ml.cOptionData.GetDisplayXYIndex(uiCameraNo, ref uiXIndex, ref uiYIndex);

            TbDisplayMaxIndex.Text = uiMaxIndex.ToString();
            TbDisplayXIndex.Text = uiXIndex.ToString();
            TbDisplayYIndex.Text = uiYIndex.ToString();
        }

        /// <summary>
        /// Display Index 값 변경 시 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayIndex_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            uint uiGetTextBoxData;

            CVisionView cVisionView = ml.cOptionData.GetDisplayIndex(uiCameraNo);

            if (uint.TryParse(textBox.Text, out uiGetTextBoxData) == true)
            {
                if (textBox.Name == "TbDisplayMaxIndex")
                {
                    if (cVisionView.uiDisplayMaxIndex != uiGetTextBoxData) cVisionView.uiDisplayMaxIndex = uiGetTextBoxData;
                }
                else if (textBox.Name == "TbDisplayXIndex")
                {
                    if (cVisionView.uiDisplayXIndex != uiGetTextBoxData) cVisionView.uiDisplayXIndex = uiGetTextBoxData;
                }
                else if (textBox.Name == "TbDisplayYIndex")
                {
                    if (cVisionView.uiDisplayYIndex != uiGetTextBoxData) cVisionView.uiDisplayYIndex = uiGetTextBoxData;
                }

                CXMLProcess.WriteXml(CXMLProcess.OptionDataFilePath, ml.cOptionData);
            }
        }

        /// <summary>
        /// 숫자만 입력되도록 제한한다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayIndex_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^1-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        /// <summary>
        /// Light 밝기 설정 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Light_Set_Click(object sender, RoutedEventArgs e)
        {
            //uint uiLightValue;
            //if (uint.TryParse(TbLightValue.Text, out uiLightValue) == true)
            //{
            //    if (uiLightValue < 0) uiLightValue = 0;
            //    else if (uiLightValue > 255) uiLightValue = 255;
            //    if (CMainLib.Ins.cLightCtl != null && CMainLib.Ins.cLightCtl._bPortOpend)
            //    {
            //        CVisionLight cVisionLight = ml.cOptionData.GetVisionLight(uiCameraNo);
            //        CMainLib.Ins.cLightCtl.SetLightValue(cVisionLight.uiLightNo, uiLightValue);
            //        TbLightValue.Text = uiLightValue.ToString();
            //        cVisionLight.uiLightValue = uiLightValue;
            //    }
            //}
        }

        /// <summary>
        /// 조명을 킨다
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Light_On_Click(object sender, RoutedEventArgs e)
        {
            if (uiCameraNo == 0) return; // 니들 삽입기에서 0번 카메라 조명은 다른 디바이스에서 사용
            uint uiLightValue;
            if (uint.TryParse(TbLightValue.Text, out uiLightValue) == true)
            {
                if (uiLightValue < 0) uiLightValue = 0;
                else if (uiLightValue > 255) uiLightValue = 255;
                if (CMainLib.Ins.cLightCtl != null && CMainLib.Ins.cLightCtl._bPortOpend)
                {
                    CVisionLight cVisionLight = ml.cOptionData.GetVisionLight(uiCameraNo);
                    CMainLib.Ins.cLightCtl.SetLightOn(cVisionLight.uiLightNo, uiLightValue);
                    TbLightValue.Text = uiLightValue.ToString();
                    cVisionLight.uiLightValue = uiLightValue;
                }
            }
        }

        /// <summary>
        /// 조명을 끈다
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Light_Off_Click(object sender, RoutedEventArgs e)
        {
            if (CMainLib.Ins.cLightCtl != null && CMainLib.Ins.cLightCtl._bPortOpend)
            {
                CVisionLight cVisionLight = ml.cOptionData.GetVisionLight(uiCameraNo);
                CMainLib.Ins.cLightCtl.SetLightOff(cVisionLight.uiLightNo);
            }
        }
    }
}