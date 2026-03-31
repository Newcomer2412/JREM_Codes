using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MachineControlBase
{
    /// <summary>
    /// JamScreenUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AlarmScreenUI : UserControl
    {
        /// <summary>
        /// Main Library
        /// </summary>
        private CMainLib ml = null;

        /// <summary>
        /// JAM Scrren UI 생성자
        /// </summary>
        public AlarmScreenUI()
        {
            InitializeComponent();
        }

        /// <summary>
        /// UI의 활성화 여부에 따라 타이머를 시작하고 정지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ml = CMainLib.Ins;

            if ((bool)e.NewValue == true)
            {
                try
                {
                    // Date 표시
                    DateTime today = DateTime.Now;
                    TbTime.Text = string.Format("{0}", today);
                    TbCodeNo.Text = ml.cVar.iErrorCode.ToString() + "\n" + ml.cVar.strErrorName;
                    // jam No 저장 및 화면 표시
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("Alarm Code : {0}", ml.cVar.iErrorCode));

                    CAlarmData cAlarmData = ml.cSysOne.ListAlarmData.Find(x => x.iNo == ml.cVar.iErrorCode);
                    if (cAlarmData != null)
                    {
                        if (ml.cOptionData.iLanguageMode == (int)eLanguage.KOREAN)
                        {
                            TbAction.Text = cAlarmData.strAction_KOR;
                        }
                        else if (ml.cOptionData.iLanguageMode == (int)eLanguage.ENGLISH)
                        {
                            TbAction.Text = cAlarmData.strAction_ENG;
                        }
                        else if (ml.cOptionData.iLanguageMode == (int)eLanguage.CHINESE)
                        {
                            TbAction.Text = cAlarmData.strAction_CHN;
                        }

                        if (cAlarmData.strImagePath != string.Empty)
                        {
                            BitmapImage bitmapImage = new BitmapImage(new Uri(CXMLProcess.AlarmImgData + cAlarmData.strImagePath,
                                                                              UriKind.RelativeOrAbsolute));
                            ImageBox.Source = bitmapImage;
                        }
                    }
                    else
                    {
                        TbAction.Text = "Empty";
                        BitmapImage bitmapImage = new BitmapImage(new Uri(CXMLProcess.AlarmImgData + "Default.png",
                                                      UriKind.RelativeOrAbsolute));
                        ImageBox.Source = bitmapImage;
                    }
                }
                catch (Exception ex)
                {
                    NLogger.AddLog(eLogType.SEQ_MAIN, NLogger.eLogLevel.FATAL, ex.ToString());
                }
            }
            else
            {
            }
        }
    }
}