using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ToolBlock;
using System.Windows.Controls;

namespace MachineControlBase
{
    /// <summary>
    /// SlideVisionUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SlideVisionUI : UserControl
    {
        private static readonly object VisionUIreadLock = new object();
        private static SlideVisionUI instance = null;

        public static SlideVisionUI Ins
        {
            get
            {
                lock (VisionUIreadLock)
                {
                    if (instance == null) instance = new SlideVisionUI();
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

        public SlideVisionUI()
        {
            InitializeComponent();
            instance = this;
            ml = CMainLib.Ins;
        }

        /// <summary>
        /// Cognex Vision 초기화 함수
        /// </summary>
        public void CogInit(VisionToolBlockUI[] cVisionToolBlockUI)
        {
            for (int i = 0; i < Define.MAX_CAMERA; i++)
            {
                string name_i = "ccVision" + i.ToString();
                var vContentControl = FindName(name_i) as ContentControl;
                if (vContentControl != null) vContentControl.Content = cVisionToolBlockUI[i];
            }
        }

        /// <summary>
        /// Tool Block Setup 탭으로 전환하여 설정을 보여줌
        /// </summary>
        /// <param name="cogToolBlockEditV2"></param>
        public void ToolBlockSetupView(CogToolBlockEditV2 cogToolBlockEditV2)
        {
            cogToolBlockEdit.Subject = cogToolBlockEditV2.Subject;
            TabToolBlock.IsSelected = true;
        }

        /// <summary>
        /// ImageFile Setup 탭으로 전환하여 설정을 보여줌
        /// </summary>
        /// <param name="cCogImageFileTool"></param>
        public void ImageFileSetupView(CogImageFileTool cCogImageFileTool)
        {
            cogImageFileEdit.Subject = cCogImageFileTool;
            TabImage.IsSelected = true;
        }

        /// <summary>
        /// AcqFifo Setup 탭으로 전환하여 설정을 보여줌
        /// </summary>
        /// <param name="cCogAcqFifoTool"></param>
        public void AcqFifoSetupView(CogAcqFifoTool cCogAcqFifoTool)
        {
            cogAcqFifoEdit.Subject = cCogAcqFifoTool;
            TabACQFIFO.IsSelected = true;
        }

        /// <summary>
        /// Vision Main View 화면 표시
        /// </summary>
        public void VisionMainView()
        {
            TabVisionMain.IsSelected = true;
        }
    }
}