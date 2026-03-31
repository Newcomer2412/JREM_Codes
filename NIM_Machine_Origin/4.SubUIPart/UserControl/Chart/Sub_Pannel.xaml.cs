using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MachineControlBase
{
    /// <summary>
    /// Sub_Pannel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Sub_Pannel : UserControl
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public Sub_Pannel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="uiCameraNo"></param>
        /// <param name="iBlockCount"></param>
        public void Init(uint uiCameraNo, int iBlockCount, bool bVisionOrBarcord = true)
        {
            // Vision 화면 이름 정의
            TbTitle.Text = ((eCAM)uiCameraNo).ToString();

            if (iBlockCount == 0)
            {
                Pie.Visibility = Visibility.Hidden;
                return;
            }
            else
            {
                Grid_div(uiCameraNo, bVisionOrBarcord);
            }
        }

        /// <summary>
        /// 그리드 분할
        /// </summary>
        /// <param name="uiCameraNo"></param>
        /// <param name="bVisionOrBarcord"></param>
        public void Grid_div(uint uiCameraNo, bool bVisionOrBarcord)
        {
            int iXIndex = 1, iYIndex = 1;
            int iFuncLength = 1;
            if (bVisionOrBarcord == true) iFuncLength = CMainLib.Ins.cVisionData.strToolBlockName[uiCameraNo].Length;

            if (iFuncLength == 1)
            {
                ViewInit(iXIndex, iYIndex, uiCameraNo);
            }
            else if (iFuncLength == 2)
            {
                iXIndex = 2;
                iYIndex = 1;
                ViewInit(iXIndex, iYIndex, uiCameraNo);
            }
            else if (iFuncLength == 3 || iFuncLength == 4)
            {
                iXIndex = iYIndex = 2;
                ViewInit(iXIndex, iYIndex, uiCameraNo);
            }
        }

        /// <summary>
        /// Grid 생성 후 그래프 UI 생성하여 추가
        /// </summary>
        /// <param name="iXIndex"></param>
        /// <param name="iYIndex"></param>
        /// <param name="uiCameraNo"></param>
        public void ViewInit(int iXIndex, int iYIndex, uint uiCameraNo)
        {
            ColumnDefinition[] columnDefinition = new ColumnDefinition[iXIndex];
            RowDefinition[] rowDefinition = new RowDefinition[iYIndex];

            // Grid 화면 분할
            for (int i = 0; i < iXIndex; i++)
            {
                columnDefinition[i] = new ColumnDefinition();
                columnDefinition[i].Width = new GridLength(1, GridUnitType.Star);
                GdMainView.ColumnDefinitions.Add(columnDefinition[i]);
            }

            for (int i = 0; i < iYIndex; i++)
            {
                rowDefinition[i] = new RowDefinition();
                rowDefinition[i].Height = new GridLength(1, GridUnitType.Star);
                GdMainView.RowDefinitions.Add(rowDefinition[i]);
            }

            //분할 된 화면에 파이 붙여 넣기
            int iIndex = 0;
            Pie[] Pies = new Pie[iXIndex * iYIndex];

            for (int j = 0; j < iYIndex; j++)
            {
                for (int i = 0; i < iXIndex; i++)
                {
                    Pies[iIndex] = new Pie();
                    Grid.SetColumn(Pies[iIndex], i);
                    Grid.SetRow(Pies[iIndex], j);

                    Pies[iIndex].MyPv1.Add(Convert.ToDouble(VisionGraphViewUI.F_Table[uiCameraNo][iIndex][2]));
                    Pies[iIndex].MyPv2.Add(Convert.ToDouble(VisionGraphViewUI.F_Table[uiCameraNo][iIndex][3]));
                    TextBlock TbText = new TextBlock();
                    TbText.Text = VisionGraphViewUI.F_Table[uiCameraNo][iIndex][4];
                    TbText.FontSize = 20;
                    TbText.Foreground = Brushes.White;
                    Grid.SetColumn(TbText, i);
                    Grid.SetRow(TbText, j);
                    GdMainView.Children.Add(TbText);
                    GdMainView.Children.Add(Pies[iIndex]);
                    iIndex++;
                }
            }
        }
    }
}