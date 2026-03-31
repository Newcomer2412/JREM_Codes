using System.Windows.Controls;

namespace MachineControlBase
{
    /// <summary>
    /// SequenceNoViewUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SequenceNoViewUI : UserControl
    {
        public SequenceNoViewUI()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 시퀀스 클래스 Step 번호를 받기 위한 Interface
        /// </summary>
        private ISeqNo iSeqNo = null;

        /// <summary>
        /// Step No 화면 갱신을 위한 비교 값
        /// </summary>
        private int iOldStepNo = 0;

        private int iOldSubStepNo = 0;

        /// <summary>
        /// 초기화
        /// </summary>
        public void Init(ISeqNo iSeqNo)
        {
            this.iSeqNo = (ISeqNo)iSeqNo.Ins;
            TbName.Text = ((ISeqNo)iSeqNo.Ins).SeqName;
            TbStepNo.Text = ((ISeqNo)iSeqNo.Ins).iStep.ToString();
            TbSubStepNo.Text = ((ISeqNo)iSeqNo.Ins).iSubStep.ToString();
        }

        /// <summary>
        /// 데이터 갱신 타이머
        /// </summary>
        public void DataTimer_Tick()
        {
            if (iOldStepNo != ((ISeqNo)iSeqNo.Ins).iStep) TbStepNo.Text = ((ISeqNo)iSeqNo.Ins).iStep.ToString();
            if (iOldSubStepNo != ((ISeqNo)iSeqNo.Ins).iSubStep) TbSubStepNo.Text = ((ISeqNo)iSeqNo.Ins).iSubStep.ToString();
        }
    }
}