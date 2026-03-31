using System.Windows.Controls;

namespace MachineControlBase
{
    /// <summary>
    /// ModelSelectUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ModelSelectUI : UserControl
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public ModelSelectUI()
        {
            InitializeComponent();
        }

        public const int DEFAULT_MODEL = 0; // 기본 모델 번호
        private CMainLib ml = null;
        private CSystemParameterSingle cSysOne = null;
        private COptionData cOptionData = null;

        /// <summary>
        /// 창 UI가 포커스를 얻었거나 잃을 경우
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                ml = CMainLib.Ins;
                cSysOne = CMainLib.Ins.cSysOne;
                cOptionData = CMainLib.Ins.cOptionData;

                ListBoxReflash();
                CSystemParameterArray cSysArray = ml.cSysParamCollData.GetSysArray();
                TbModelName.Text = string.Format("[ {0:00} ] {1}", cSysArray.uiModelNo, cSysArray.strModelName);
                TbModelEditName.Text = cSysArray.strModelName;
            }
            else
            {
                CXMLProcess.XmlFileSave();
            }
        }

        /// <summary>
        /// ListBox 데이터 갱신
        /// </summary>
        private void ListBoxReflash()
        {
            LbModelData.Items.Clear();
            string strModelData;

            foreach (CSystemParameterArray cSystemParameterArray in ml.cSysParamCollData.cSystemParameterList)
            {
                strModelData = string.Format("[ {0:00} ] {1}", cSystemParameterArray.uiModelNo, cSystemParameterArray.strModelName);
                LbModelData.Items.Add(strModelData);
            }
        }

        /// <summary>
        /// ListBox List 선택 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LbModelData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChoiceModelData();
        }

        /// <summary>
        /// 선택한 모델 번호
        /// </summary>
        private int iSelectModelNo = -1;

        /// <summary>
        /// ListBox의 Model을 선택하였을 때 데이터 갱신 처리
        /// </summary>
        private void ChoiceModelData()
        {
            if (LbModelData.SelectedItem != null)
            {
                string strSelectItem = LbModelData.SelectedItem.ToString();
                string strNumber = strSelectItem.Substring(3, 2);
                int.TryParse(strNumber, out iSelectModelNo);
                CSystemParameterArray cSysArray = ml.cSysParamCollData.GetSysArray((uint)iSelectModelNo);
                TbModelName.Text = string.Format("[ {0:00} ] {1}", cSysArray.uiModelNo, cSysArray.strModelName);
                TbModelEditName.Text = cSysArray.strModelName;
            }
        }

        /// <summary>
        /// Model Delete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (iSelectModelNo == -1) return;
            string strMessage = string.Empty;
            CSystemParameterArray cSysArray = ml.cSysParamCollData.GetSysArray((uint)iSelectModelNo);

            if (cSysArray.uiModelNo == DEFAULT_MODEL)  // 0번 삭제 금지
            {
                if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = "기본 모델은 삭제할 수 없습니다";
                else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = "Default Model can't delete";
                CCommon.ShowMessageMini(strMessage);
                return;
            }

            if (cSysArray.uiModelNo == cSysOne.uiCurrentModelNo)  // 사용중인 Model 삭제 금지
            {
                if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = "현재 사용중인 모델은 삭제할 수 없습니다";
                else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = "Can't delete on the current use model";
                CCommon.ShowMessageMini(strMessage);
                return;
            }

            if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN)
                strMessage = string.Format("선택한 모델을 삭제하겠습니까? [{0}]", cSysArray.strModelName);
            else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH)
                strMessage = string.Format("Do you want selected model delete? [{0}]", cSysArray.strModelName);

            if (CCommon.ShowMessageMini(2, strMessage) == (int)eMBoxRtn.A_OK)
            {
                ml.cAxisPosCollData.DataClear(cSysArray.uiModelNo);
                ml.cSysParamCollData.DataClear(cSysArray.uiModelNo);
                ListBoxReflash();
            }
            else return;

            iSelectModelNo = -1;
            if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = "삭제되었습니다";
            else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = "Delete Complete";
            CCommon.ShowMessageMini(strMessage);
        }

        /// <summary>
        /// Name Change 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Change_Name_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (iSelectModelNo == -1) return;
            string strMessage = string.Empty;
            CSystemParameterArray cSysArray = ml.cSysParamCollData.GetSysArray((uint)iSelectModelNo);

            string strData = TbModelEditName.Text;
            strData.Trim();

            if (strData == string.Empty && iSelectModelNo != 0)
            {
                if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = "모델명을 입력하세요.";
                else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = "Please enter the model name.";
                CCommon.ShowMessageMini(strMessage);
                return;
            }
            if (cSysArray.strModelName == strData)
            {
                if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = "기존 모델명과 동일합니다";
                else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = "Same as previous model name.";
                CCommon.ShowMessageMini(strMessage);
                return;
            }
            else
            {
                cSysArray.strModelName = strData;
                ListBoxReflash();
                if (iSelectModelNo == cSysOne.uiCurrentModelNo)
                {
                    // 타이틀 바 현재 설정되어 있는 모델 표시
                    var mw = (MainWindow)System.Windows.Application.Current.MainWindow;
                    if (mw != null) mw.ModelNameChange();
                }
            }
        }

        /// <summary>
        /// Apply 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Apply_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (iSelectModelNo == -1) return;
            string strMessage = string.Empty;
            object selecteditem = LbModelData.SelectedItem;   // 반환형 object

            if (selecteditem.ToString().Length <= 7)
            {
                CCommon.ShowMessageMini("Selected Model is empty");
                return;
            }

            if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = "선택한 Model을 적용 하시겠습니까? 적용 후 프로그램 재시작.";
            else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = "Do you want the choice apply Model? Program restart after application.";
            if (CCommon.ShowMessageMini(2, strMessage) == (int)eMBoxRtn.A_Cancel) return;

            cSysOne.uiCurrentModelNo = (uint)iSelectModelNo;
            // 타이틀 바 현재 설정되어 있는 모델 표시
            var mw = (MainWindow)System.Windows.Application.Current.MainWindow;
            if (mw != null) mw.ModelNameChange();
        }

        /// <summary>
        /// Model Data Copy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Copy_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string strMessage = string.Empty;

            if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = "데이터를 복사 하시겠습니까?";
            else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = "Do you want to data copy?";
            if (CCommon.ShowMessageMini(2, strMessage) == (int)eMBoxRtn.A_Cancel) return;

            uint uiSource, uiTarget;
            uint.TryParse(TbSourceNo.Text, out uiSource);
            uint.TryParse(TbTargetNo.Text, out uiTarget);

            if (uiSource < 0 || uiSource > 99 || uiTarget < 0 || uiTarget > 99)
            {
                if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = "모델 번호 범위는 0 ~ 99입니다.";
                else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = "Model numbers range from 0 to 99.";
                CCommon.ShowMessageMini(strMessage);
                return;
            }

            if (uiSource == uiTarget)  // 같은 번호
            {
                if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = "데이터를 복사할 수 없습니다! 타겟과 소스의 번호가 똑같습니다";
                else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = "Data not copy ! Same number of source to target";
                CCommon.ShowMessageMini(strMessage);
                return;
            }

            if (ml.cSysParamCollData.CheckSysArray(uiTarget) == true)
            {
                if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = "복사하려는 위치에 Model 데이타가 있습니다. 덮어쓰시겠습니까?";
                else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = "This Model Number Already data. Do you want Overwrite data?";
                if (CCommon.ShowMessageMini(2, strMessage) == (int)eMBoxRtn.A_Cancel) return;
            }

            // Data copy
            ModelDataCopy(uiTarget, uiSource);
            CSystemParameterArray cSysArray_Target = ml.cSysParamCollData.GetSysArray(uiTarget);
            CSystemParameterArray cSysArray_Source = ml.cSysParamCollData.GetSysArray(uiSource);
            cSysArray_Target.uiModelNo = uiTarget;
            cSysArray_Target.strModelName = cSysArray_Source.strModelName + "_Copy";
            ml.cSysParamCollData.OrderByModel();
            ListBoxReflash();

            if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = "복사되었습니다";
            else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = "Copied Complete";
            CCommon.ShowMessageMini(strMessage);
        }

        /// <summary>
        /// Model Data Copy
        /// </summary>
        /// <param name="uiTarget"></param>
        /// <param name="uiSource"></param>
        private void ModelDataCopy(uint uiTarget, uint uiSource)
        {
            ml.cAxisPosCollData.DataCopy(uiTarget, uiSource);
            ml.cSysParamCollData.DataCopy(uiTarget, uiSource);
        }

        /// <summary>
        /// 숫자 입력 클래스
        /// </summary>
        private NumPadUI m_NumPadUI = null;

        /// <summary>
        /// Copy TextBlock 숫자키 입력 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBlock_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            m_NumPadUI = new NumPadUI();

            if (m_NumPadUI.bOpened == false)
            {
                m_NumPadUI.Init(sender as TextBlock);
                m_NumPadUI.ShowDialog();
            }
            else m_NumPadUI.Close();
        }

        /// <summary>
        /// Copy TextBlock 클릭 자동 삽입 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            object selecteditem = LbModelData.SelectedItem;   // 반환형 object
            if (selecteditem != null)
            {
                string[] sp = selecteditem.ToString().Split(']');
                sp[0] = sp[0].Replace("[", "");
                sp[0] = sp[0].Replace(" ", "");

                if ((sender as TextBlock).Text == "Source") TbSourceNo.Text = sp[0];
                else TbTargetNo.Text = sp[0];
            }
        }

        /// <summary>
        /// 윈도우 가상 키보드 열기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Key_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Diagnostics.Process ps = new System.Diagnostics.Process();
            ps.StartInfo.FileName = "osk.exe";
            ps.Start();
        }
    }
}