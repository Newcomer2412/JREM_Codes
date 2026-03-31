using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// MapData.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MapDataUI : UserControl, IUIMapData
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public MapDataUI()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 그릴 수 있는지 Flag
        /// </summary>
        private bool bCanUpdateView = false;

        /// <summary>
        /// 화면 UI Object 관리 리스트
        /// </summary>
        private List<CObjectUnit> ObjectUIList = new List<CObjectUnit>();

        private eData eData { get; set; }

        /// <summary>
        /// Map의 종류 정의
        /// </summary>
        public eData _eData
        {
            get
            {
                return eData;
            }
            set
            {
                eData = value;
            }
        }

        private int[] iArrayStatus = null;

        private string strPopupStatus { get; set; }

        /// <summary>
        /// 팝업에 띄울 enum 정의
        /// </summary>
        public string _strPopupStatus
        {
            get
            {
                return strPopupStatus;
            }

            set
            {
                strPopupStatus = value;
                iArrayStatus = (int[])(typeof(MapDataFunction).GetField(strPopupStatus).GetValue(null));
            }
        }

        private int iMethod { get; set; }

        /// <summary>
        /// Map의 Count 순서 방법 설정
        /// </summary>
        public int _iMethod
        {
            get
            {
                return iMethod;
            }
            set
            {
                iMethod = value;
            }
        }

        private int iMaxXindex { get; set; } = 0;

        /// <summary>
        /// 최대 X Index 개 수
        /// </summary>
        public int _iMaxXindex
        {
            get
            {
                return iMaxXindex;
            }
            set
            {
                iMaxXindex = value;
            }
        }

        private int iMaxYindex { get; set; } = 0;

        /// <summary>
        /// 최대 Y Index 개 수
        /// </summary>
        public int _iMaxYindex
        {
            get
            {
                return iMaxYindex;
            }
            set
            {
                iMaxYindex = value;
            }
        }

        private double dStartPosX { get; set; } = 0;

        /// <summary>
        /// X 시작 위치
        /// </summary>
        public double _dStartPosX
        {
            get
            {
                return dStartPosX;
            }
            set
            {
                dStartPosX = value;
            }
        }

        private double dStartPosY { get; set; } = 0;

        /// <summary>
        /// Y 시작 위치
        /// </summary>
        public double _dStartPosY
        {
            get
            {
                return dStartPosY;
            }
            set
            {
                dStartPosY = value;
            }
        }

        private double dOffsetX { get; set; } = 0;

        /// <summary>
        /// X Offset
        /// </summary>
        public double _dOffsetX
        {
            get
            {
                return dOffsetX;
            }
            set
            {
                dOffsetX = value;
            }
        }

        private double dOffsetY { get; set; } = 0;

        /// <summary>
        /// Y Offset
        /// </summary>
        public double _dOffsetY
        {
            get
            {
                return dOffsetY;
            }
            set
            {
                dOffsetY = value;
            }
        }

        private bool bShowTextNo { get; set; }

        /// <summary>
        /// Map에 순서 번호를 표시할지 설정
        /// </summary>
        public bool _bShowTextNo
        {
            get
            {
                return bShowTextNo;
            }
            set
            {
                bShowTextNo = value;
            }
        }

        private bool bShowTextInfo { get; set; }

        /// <summary>
        /// Map에 상태 정보를 표시할지 설정
        /// </summary>
        public bool _bShowTextInfo
        {
            get
            {
                return bShowTextInfo;
            }
            set
            {
                bShowTextInfo = value;
            }
        }

        private int iTextInfoWidth { get; set; } = 0;

        /// <summary>
        /// 맵 상태 정보 가로 크기(길이가 길면 줄바꿈을 위해 설정)
        /// </summary>
        public int _iTextInfoWidth
        {
            get
            {
                return iTextInfoWidth;
            }
            set
            {
                iTextInfoWidth = value;
            }
        }

        private Thickness TkTextInfoMargin { get; set; }

        /// <summary>
        /// 맵 상태 정보의 Margin 설정
        /// </summary>
        public Thickness _TkTextInfoMargin
        {
            get
            {
                return TkTextInfoMargin;
            }
            set
            {
                TkTextInfoMargin = value;
            }
        }

        private string strName { get; set; }

        /// <summary>
        /// Map의 Name
        /// </summary>
        public string _strName
        {
            get
            {
                return strName;
            }
            set
            {
                strName = value;
                TbMapName.Text = value;
            }
        }

        private bool bShowName { get; set; } = true;

        /// <summary>
        /// Map 명칭을 숨긴다
        /// </summary>
        public bool _bShowName
        {
            get
            {
                return bShowName;
            }
            set
            {
                bShowName = value;
                if (value == false) BdNameView.Visibility = Visibility.Hidden;
                else BdNameView.Visibility = Visibility.Visible;
            }
        }

        private string strShape { get; set; } = "Ellipse";

        /// <summary>
        /// Unit 모양
        /// </summary>
        public string _strShape
        {
            get
            {
                return strShape;
            }
            set
            {
                strShape = value;
            }
        }

        private double dEllipseWidthHeight { get; set; } = 0;

        /// <summary>
        /// Ellipse 경우 폭, 넓이 설정값
        /// </summary>
        public double _dEllipseWidthHeight
        {
            get
            {
                return dEllipseWidthHeight;
            }
            set
            {
                dEllipseWidthHeight = value;
            }
        }

        private bool bPopupUse { get; set; } = true;

        /// <summary>
        /// 팝업 사용 유무
        /// </summary>
        public bool _bPopupUse
        {
            get
            {
                return bPopupUse;
            }
            set
            {
                bPopupUse = value;
            }
        }

        private int iPopupWidth { get; set; }

        /// <summary>
        /// 팝업 가로 크기
        /// </summary>
        public int _iPopupWidth
        {
            get
            {
                return iPopupWidth;
            }
            set
            {
                iPopupWidth = value;
                Popup_UnitStatus.Width = value;
            }
        }

        private int iPopupHeight { get; set; }

        /// <summary>
        /// 팝업 세로 크기
        /// </summary>
        public int _iPopupHeight
        {
            get
            {
                return iPopupHeight;
            }
            set
            {
                iPopupHeight = value;
                Popup_UnitStatus.Height = value;
            }
        }

        /// <summary>
        /// 팝업 표시되는 위치 설정
        /// </summary>
        private PlacementMode ePlacementMode { get; set; }

        public PlacementMode _ePlacementMode
        {
            get
            {
                return ePlacementMode;
            }
            set
            {
                ePlacementMode = value;
                Popup_UnitStatus.Placement = value;
            }
        }

        private int iUnitMapMargin { get; set; } = 10;

        /// <summary>
        /// Unit Map Margin
        /// </summary>
        public int _iUnitMapMargin
        {
            get
            {
                return iUnitMapMargin;
            }
            set
            {
                iUnitMapMargin = value;
            }
        }

        private int iBorderCornerRadius { get; set; } = 10;

        /// <summary>
        /// Border Unit의 CornerRadius
        /// </summary>
        public int _iBorderCornerRadius
        {
            get
            {
                return iBorderCornerRadius;
            }
            set
            {
                iBorderCornerRadius = value;
            }
        }

        private Brush brushDataViewBorder { get; set; }

        /// <summary>
        /// Data Unit Background 바탕 색상 설정
        /// </summary>
        public Brush _brushDataViewBorder
        {
            get
            {
                return brushDataViewBorder;
            }
            set
            {
                brushDataViewBorder = value;
                BdDataView.Background = value;
            }
        }

        private bool bNoSetData { get; set; } = false;

        /// <summary>
        /// Map Data 생성 및 변경을 하지 않고 에디트만 되도록 하는 Flag
        /// </summary>
        public bool _bNoSetData
        {
            get
            {
                return bNoSetData;
            }
            set
            {
                bNoSetData = value;
            }
        }

        /// <summary>
        /// 초기화
        /// </summary>
        /// <returns></returns>
        public void Init()
        {
            MapDataLib mapDataLib = CMainLib.Ins.cRunUnitData.GetIndexData(eData);
            if (iMaxXindex == 0 || iMaxYindex == 0)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, "Data UI iMaxXindex, iMaxYindex Error " + eData.ToString());
                return;
            }
            else if (iArrayStatus == null)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, "Data UI iArrayStatus Error " + eData.ToString());
                return;
            }

            if (_bPopupUse == true)
            {
                // Unit이 하나면 ALL 기능은 제외 한다.
                if (iMaxXindex == 1 && iMaxYindex == 1)
                {
                    GridDivide(Popup_Grid, iArrayStatus.Length, 2);
                    GridDivide(Popup_AllGrid, Enum.GetValues(typeof(eStatus)).Length - 1, 2);
                    PopupButtonAdd(Popup_Grid, false);
                    PopupButtonAllAdd(Popup_AllGrid, false);
                }
                else
                {
                    int iSetGridRow = 3;
                    if (eData == eData.SUPPLY_HOLDER_TRAY ||
                        eData == eData.WORK_DONE_HOLDER_TRAY) iSetGridRow++;

                    // 팝업창 화면 분할
                    GridDivide(Popup_Grid, iArrayStatus.Length, iSetGridRow);
                    GridDivide(Popup_AllGrid, Enum.GetValues(typeof(eStatus)).Length - 1, 3);
                    // 팝업창 생성 및 배치
                    PopupButtonAdd(Popup_Grid);
                    PopupButtonAllAdd(Popup_AllGrid);
                }
            }

            // UI 개 수에 맞게 맵 데이터 생성
            if (bNoSetData == false)
            {
                mapDataLib.Init(iMaxXindex, iMaxYindex);
            }

            // 파이프 및 니들 마운트 맵은 여기서 생성한다.
            if (mapDataLib.strDataName == Enum.GetName(typeof(eData), eData.MPC1_PIPE_MOUNT) ||
                mapDataLib.strDataName == Enum.GetName(typeof(eData), eData.MPC1_NEEDLE_MOUNT) ||
                mapDataLib.strDataName == Enum.GetName(typeof(eData), eData.MPC1_BUFFER_2))
            {
                CanvasView.Visibility = Visibility.Visible;
                CanvasUnitMapAdd(CanvasView, iMaxXindex, iMaxYindex);
            }
            else
            {
                // Grid 화면 분할
                GridDivide(GdDataView, iMaxXindex, iMaxYindex);
                //분할 된 화면에 맵데이터 분배 하기
                GridUnitMapAdd(GdDataView, iMaxXindex, iMaxYindex);
            }

            // Map을 Display 할 수 있도록 초기화 완료
            bCanUpdateView = true;
        }

        /// <summary>
        /// 팝업창 초기화
        /// </summary>
        public void Free()
        {
            if (MapDataFunction.bPopupOpen == true) MapDataFunction.bPopupOpen = false;
            Popup_UnitStatus.IsOpen = false;
            Popup_AllUnitStatus.IsOpen = false;
        }

        /// <summary>
        /// 그리드 나눔
        /// </summary>
        /// <param name="cGrid"></param>
        /// <param name="iMaxX"></param>
        /// <param name="iMaxY"></param>
        private void GridDivide(Grid cGrid, int iMaxX, int iMaxY)
        {
            ColumnDefinition[] columnDefinition = new ColumnDefinition[iMaxX];
            RowDefinition[] rowDefinition = new RowDefinition[iMaxY];

            for (int i = 0; i < iMaxX; i++)
            {
                columnDefinition[i] = new ColumnDefinition();
                columnDefinition[i].Width = new GridLength(1, GridUnitType.Star);
                cGrid.ColumnDefinitions.Add(columnDefinition[i]);
            }

            for (int i = 0; i < iMaxY; i++)
            {
                rowDefinition[i] = new RowDefinition();
                rowDefinition[i].Height = new GridLength(1, GridUnitType.Star);
                cGrid.RowDefinitions.Add(rowDefinition[i]);
            }
        }

        /// <summary>
        /// 팝업 버튼 추가(설정한 상태값만 보여주는 버튼)
        /// </summary>
        /// <param name="cGrid"></param>
        /// <param name="bAllButtonUes"></param>
        private void PopupButtonAdd(Grid cGrid, bool bAllButtonUes = true)
        {
            int iRow = 0;
            for (int i = 0; i < iArrayStatus.Length; i++)
            {
                iRow = 0;
                // 버튼 생성 및 설정
                Button button = new Button();
                Grid.SetColumn(button, i);
                Grid.SetRow(button, iRow++);
                button.Click += Popup_Button_Click;
                button.Content = Enum.GetName(typeof(eStatus), iArrayStatus[i]);
                button.FontSize = 11;
                button.Foreground = Brushes.White;
                button.Background = MapDataFunction.StatusColor((eStatus)iArrayStatus[i]);
                button.Margin = new Thickness(1);
                cGrid.Children.Add(button);

                if (eData == eData.SUPPLY_HOLDER_TRAY ||
                    eData == eData.WORK_DONE_HOLDER_TRAY)
                {
                    // 선택한 Unit No의 데이터 까지만 변경하는 버튼 생성 및 설정
                    Button UntilButton = new Button();
                    Grid.SetColumn(UntilButton, i);
                    Grid.SetRow(UntilButton, iRow++);
                    UntilButton.Click += Popup_Button_Click;
                    UntilButton.Content = "UT_" + Enum.GetName(typeof(eStatus), iArrayStatus[i]);
                    UntilButton.FontSize = 11;
                    UntilButton.Foreground = Brushes.White;
                    UntilButton.Background = MapDataFunction.StatusColor((eStatus)iArrayStatus[i]);
                    UntilButton.Margin = new Thickness(1);
                    cGrid.Children.Add(UntilButton);
                }

                if (bAllButtonUes == true)
                {
                    // 데이터 모두 변경 버튼 생성 및 설정
                    Button AllButton = new Button();
                    Grid.SetColumn(AllButton, i);
                    Grid.SetRow(AllButton, iRow++);
                    AllButton.Click += Popup_Button_Click;
                    AllButton.Content = "ALL_" + Enum.GetName(typeof(eStatus), iArrayStatus[i]);
                    AllButton.FontSize = 11;
                    AllButton.Foreground = Brushes.White;
                    AllButton.Background = MapDataFunction.StatusColor((eStatus)iArrayStatus[i]);
                    AllButton.Margin = new Thickness(1);
                    cGrid.Children.Add(AllButton);
                }
            }

            // Close 버튼 생성 및 설정
            Button CloseButton = new Button();
            Grid.SetColumn(CloseButton, 0);
            Grid.SetColumnSpan(CloseButton, iArrayStatus.Length);
            if (bAllButtonUes == true) Grid.SetRow(CloseButton, iRow++);
            else Grid.SetRow(CloseButton, iRow++);
            CloseButton.Click += Popup_Button_Click;
            CloseButton.PreviewMouseLeftButtonDown += Close_Button_Down;
            CloseButton.Content = "CLOSE";
            CloseButton.Foreground = Brushes.White;
            CloseButton.Background = Brushes.YellowGreen;
            CloseButton.Margin = new Thickness(1);
            cGrid.Children.Add(CloseButton);
        }

        /// <summary>
        /// 팝업 버튼 추가(모든 상태 변경 값을 보여주는 버튼)
        /// </summary>
        /// <param name="cGrid"></param>
        /// <param name="bAllButtonUes"></param>
        private void PopupButtonAllAdd(Grid cGrid, bool bAllButtonUes = true)
        {
            int i = 0;
            foreach (var Number in Enum.GetValues(typeof(eStatus)))
            {
                if ((int)Number == -1) continue;
                // 버튼 생성 및 설정
                Button button = new Button();
                Grid.SetColumn(button, i);
                Grid.SetRow(button, 0);
                button.Click += Popup_Button_Click;
                button.Content = Enum.GetName(typeof(eStatus), (int)Number);
                button.FontSize = 8;
                button.Foreground = Brushes.White;
                button.Background = MapDataFunction.StatusColor((eStatus)(int)Number);
                button.Margin = new Thickness(1);
                cGrid.Children.Add(button);

                if (bAllButtonUes == true)
                {
                    // 데이터 모두 변경 버튼 생성 및 설정
                    Button AllButton = new Button();
                    Grid.SetColumn(AllButton, i);
                    Grid.SetRow(AllButton, 1);
                    AllButton.Click += Popup_Button_Click;
                    AllButton.Content = "ALL_" + Enum.GetName(typeof(eStatus), (int)Number);
                    AllButton.FontSize = 8;
                    AllButton.Foreground = Brushes.White;
                    AllButton.Background = MapDataFunction.StatusColor((eStatus)(int)Number);
                    AllButton.Margin = new Thickness(1);
                    cGrid.Children.Add(AllButton);
                }
                i++;
            }

            // Close 버튼 생성 및 설정
            Button CloseButton = new Button();
            Grid.SetColumn(CloseButton, 0);
            Grid.SetColumnSpan(CloseButton, Enum.GetValues(typeof(eStatus)).Length - 1);
            if (bAllButtonUes == true) Grid.SetRow(CloseButton, 2);
            else Grid.SetRow(CloseButton, 1);
            CloseButton.Click += Popup_Button_Click;
            CloseButton.Content = "CLOSE";
            CloseButton.Foreground = Brushes.White;
            CloseButton.Background = Brushes.YellowGreen;
            CloseButton.Margin = new Thickness(1);
            cGrid.Children.Add(CloseButton);
        }

        /// <summary>
        /// 그리드에 원형 데이터 UI 채움(이곳에서 iMethod에 맞도록 배열을 정렬한다) →←↓↑
        /// </summary>
        /// <param name="cGrid"></param>
        /// <param name="iMaxX"></param>
        /// <param name="iMaxY"></param>
        private void GridUnitMapAdd(Grid cGrid, int iMaxX, int iMaxY)
        {
            MapDataLib mapDataLib = CMainLib.Ins.cRunUnitData.GetIndexData(eData);

            if (iMethod == 0)   // 기본 X→ 증가 Y↓ 증가
            {
                for (int j = 0; j < iMaxY; j++)
                {
                    for (int i = 0; i < iMaxX; i++)
                    {
                        SetUnitMap(cGrid, mapDataLib, i, j);
                    }
                }
            }
            else if (iMethod == 1)   // X← 증가 Y↓ 증가
            {
                for (int j = 0; j < iMaxY; j++)
                {
                    for (int i = iMaxX - 1; i >= 0; i--)
                    {
                        SetUnitMap(cGrid, mapDataLib, i, j);
                    }
                }
            }
            else if (iMethod == 2)   // 기본 X→ 증가 Y↑ 증가
            {
                for (int j = iMaxY - 1; j >= 0; j--)
                {
                    for (int i = 0; i < iMaxX; i++)
                    {
                        SetUnitMap(cGrid, mapDataLib, i, j);
                    }
                }
            }
            else if (iMethod == 3)   // X← 증가 Y↑ 증가
            {
                for (int j = iMaxY - 1; j >= 0; j--)
                {
                    for (int i = iMaxX - 1; i >= 0; i--)
                    {
                        SetUnitMap(cGrid, mapDataLib, i, j);
                    }
                }
            }
            else if (iMethod == 4)   // 기본 X↓ 증가 Y→ 증가
            {
                for (int i = 0; i < iMaxX; i++)
                {
                    for (int j = 0; j < iMaxY; j++)
                    {
                        SetUnitMap(cGrid, mapDataLib, i, j);
                    }
                }
            }
            else if (iMethod == 5)   // X↑ 증가 Y→ 증가
            {
                for (int i = 0; i < iMaxX; i++)
                {
                    for (int j = iMaxY - 1; j >= 0; j--)
                    {
                        SetUnitMap(cGrid, mapDataLib, i, j);
                    }
                }
            }
            else if (iMethod == 6)   // 기본 X↓ 증가 Y← 증가
            {
                for (int i = iMaxX - 1; i >= 0; i--)
                {
                    for (int j = 0; j < iMaxY; j++)
                    {
                        SetUnitMap(cGrid, mapDataLib, i, j);
                    }
                }
            }
            else if (iMethod == 7)   // 기본 X↑ 증가 Y← 증가
            {
                for (int i = iMaxX - 1; i >= 0; i--)
                {
                    for (int j = iMaxY - 1; j >= 0; j--)
                    {
                        SetUnitMap(cGrid, mapDataLib, i, j);
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cGrid"></param>
        /// <param name="iMaxX"></param>
        /// <param name="iMaxY"></param>
        private void CanvasUnitMapAdd(Canvas cCanvas, int iMaxX, int iMaxY)
        {
            MapDataLib mapDataLib = CMainLib.Ins.cRunUnitData.GetIndexData(eData);

            for (int j = 0; j < iMaxY; j++)
            {
                for (int i = 0; i < iMaxX; i++)
                {
                    SetUnitMapCanvas(cCanvas, mapDataLib, i, j);
                }
            }
        }

        /// <summary>
        /// Unit Map 번호 설정
        /// </summary>
        private int UnitNoCount = 0;

        /// <summary>
        /// Unit Map을 생성하여 Grid에 추가
        /// </summary>
        /// <param name="cGrid"></param>
        /// <param name="mapDataLib"></param>
        /// <param name="iX"></param>
        /// <param name="iY"></param>
        private void SetUnitMap(Grid cGrid, MapDataLib mapDataLib, int iX, int iY)
        {
            // object 관리 클래스 생성 및 설정
            CObjectUnit cObjectUnit = new CObjectUnit();
            BaseMapData cBaseMapData = mapDataLib.GetIndex(iX, iY);
            cObjectUnit.iX = iX;
            cObjectUnit.iY = iY;
            cObjectUnit.iNo = UnitNoCount++;
            cBaseMapData.iUnitNo = cObjectUnit.iNo;

            // Position 설정(Index 기준으로 하기때문에 좌측 상단이 무조건 (0,0) 원점 기준으로 설정된다.
            // 원점 기준을 변경하기 위해서는 Start Pos를 다른 위치로 하고 Offset 값에서 Inverse를 한다.
            if (dOffsetX != 0 ||
                dOffsetY != 0)
            {
                cBaseMapData.dPosX = dStartPosX + dOffsetX * iX;
                cBaseMapData.dPosY = dStartPosY + dOffsetY * iY;
            }

            Grid grid = new Grid();
            Grid.SetColumn(grid, iX);
            Grid.SetRow(grid, iY);
            grid.Margin = new Thickness(iUnitMapMargin);
            cGrid.Margin = new Thickness(iUnitMapMargin / 2);

            if (strShape == "Ellipse")
            {
                // 원 UI 생성 및 설정
                cObjectUnit.ellipse = new Ellipse();
                if (dEllipseWidthHeight != 0) cObjectUnit.ellipse.Width = cObjectUnit.ellipse.Height = dEllipseWidthHeight;
                cObjectUnit.ellipse.MouseLeftButtonDown += elUnit_MouseLeftButtonDown;
                cObjectUnit.ellipse.Name = string.Format("elUnit{0}", cObjectUnit.iNo); // 데이터 Object 이름 정의
                cGrid.RegisterName(cObjectUnit.ellipse.Name, cObjectUnit.ellipse);      // 등록하지 않으면 FineName에서 검색되지 않음
                cObjectUnit.ellipse.Tag = string.Format("{0}", cObjectUnit.iNo);
                grid.Children.Add(cObjectUnit.ellipse);
            }
            else if (strShape == "Border")
            {
                // 사각형 UI 생성 및 설정
                cObjectUnit.border = new Border();
                cObjectUnit.border.MouseLeftButtonDown += elUnit_MouseLeftButtonDown;
                cObjectUnit.border.Name = string.Format("elUnit{0}", cObjectUnit.iNo); // 데이터 Object 이름 정의
                cGrid.RegisterName(cObjectUnit.border.Name, cObjectUnit.border);      // 등록하지 않으면 FineName에서 검색되지 않음
                cObjectUnit.border.Tag = string.Format("{0}", cObjectUnit.iNo);
                cObjectUnit.border.CornerRadius = new CornerRadius(iBorderCornerRadius);
                grid.Children.Add(cObjectUnit.border);
            }

            if (bShowTextInfo == true)
            {
                // 맵 내부 텍스트 생성 및 설정
                cObjectUnit.textBlock = new TextBlock();
                cObjectUnit.textBlock.Foreground = Brushes.White;
                if (iTextInfoWidth != 0) cObjectUnit.textBlock.Width = iTextInfoWidth;
                cObjectUnit.textBlock.Margin = TkTextInfoMargin;
                cObjectUnit.textBlock.TextWrapping = TextWrapping.Wrap;
                cObjectUnit.textBlock.TextAlignment = TextAlignment.Center;
                // UI의 각도가 회전되어 있을 경우 폰트에 역 각도를 설정하여 보정
                RotateTransform rotation = RenderTransform as RotateTransform;
                if (rotation != null)
                {
                    cObjectUnit.textBlock.RenderTransformOrigin = new Point(0.5, 0.5);
                    cObjectUnit.textBlock.RenderTransform = new RotateTransform(rotation.Angle * -1);
                }

                // 맵 내부 텍스트 크기 자동 조절을 위한 ViewBox 설정
                cObjectUnit.viewbox = new Viewbox();
                cObjectUnit.viewbox.Child = cObjectUnit.textBlock;
                cObjectUnit.viewbox.Stretch = Stretch.Uniform;
                cObjectUnit.viewbox.MouseLeftButtonDown += Viewbox_MouseLeftButtonDown;
                cObjectUnit.viewbox.Tag = string.Format("{0}", cObjectUnit.iNo);    // View Box가 클릭되었을 경우 데이터 Objec로 이벤트 토스를 위한 번호 정의
                grid.Children.Add(cObjectUnit.viewbox);
            }

            if (bShowTextNo == true)
            {
                // 맵 내부 텍스트 생성 및 설정
                TextBlock textBlock = new TextBlock();
                textBlock.Foreground = Brushes.White;
                textBlock.TextAlignment = TextAlignment.Center;
                textBlock.HorizontalAlignment = HorizontalAlignment.Left;
                textBlock.VerticalAlignment = VerticalAlignment.Top;
                textBlock.Margin = new Thickness(2);
                if (mapDataLib.strDataName == Enum.GetName(typeof(eData), eData.SUPPLY_HOLDER_TRAY) ||
                    mapDataLib.strDataName == Enum.GetName(typeof(eData), eData.WORK_DONE_HOLDER_TRAY) ||
                    mapDataLib.strDataName == Enum.GetName(typeof(eData), eData.NG_HOLDER_TRAY))
                {
                    textBlock.Text = (cBaseMapData.iUnitNo + 1).ToString();
                }
                else
                {
                    textBlock.Text = cBaseMapData.iUnitNo.ToString();
                }
                textBlock.FontSize = 11;
                RotateTransform rotation = RenderTransform as RotateTransform;
                if (rotation != null)
                {
                    textBlock.RenderTransformOrigin = new Point(0.5, 0.5);
                    textBlock.RenderTransform = new RotateTransform(rotation.Angle * -1);
                }
                grid.Children.Add(textBlock);
            }
            cGrid.Children.Add(grid);
            ObjectUIList.Add(cObjectUnit);
        }

        /// <summary>
        /// Unit Map을 생성하여 Canvas에 추가
        /// </summary>
        /// <param name="cGrid"></param>
        /// <param name="mapDataLib"></param>
        /// <param name="iX"></param>
        /// <param name="iY"></param>
        private void SetUnitMapCanvas(Canvas cCanvas, MapDataLib mapDataLib, int iX, int iY)
        {
            // object 관리 클래스 생성 및 설정
            CObjectUnit cObjectUnit = new CObjectUnit();
            BaseMapData cBaseMapData = mapDataLib.GetIndex(iX, iY);
            cObjectUnit.iX = iX;
            cObjectUnit.iY = iY;
            cObjectUnit.iNo = UnitNoCount++;
            cBaseMapData.iUnitNo = cObjectUnit.iNo;

            // Position 설정(Index 기준으로 하기때문에 좌측 상단이 무조건 (0,0) 원점 기준으로 설정된다.
            // 원점 기준을 변경하기 위해서는 Start Pos를 다른 위치로 하고 Offset 값에서 Inverse를 한다.
            if (dOffsetX != 0 ||
                dOffsetY != 0)
            {
                cBaseMapData.dPosX = dStartPosX + dOffsetX * iX;
                cBaseMapData.dPosY = dStartPosY + dOffsetY * iY;
            }

            Canvas canvas = new Canvas();
            if (iX == 0)
            {
                Canvas.SetLeft(canvas, 150);
                Canvas.SetTop(canvas, 150);
            }
            else if (iX == 1)
            {
                Canvas.SetLeft(canvas, 185);
                Canvas.SetTop(canvas, 90);
            }
            else if (iX == 2)
            {
                Canvas.SetLeft(canvas, 220);
                Canvas.SetTop(canvas, 150);
            }
            else if (iX == 3)
            {
                Canvas.SetLeft(canvas, 185);
                Canvas.SetTop(canvas, 210);
            }
            else if (iX == 4)
            {
                Canvas.SetLeft(canvas, 115);
                Canvas.SetTop(canvas, 210);
            }
            else if (iX == 5)
            {
                Canvas.SetLeft(canvas, 80);
                Canvas.SetTop(canvas, 150);
            }
            else if (iX == 6)
            {
                Canvas.SetLeft(canvas, 115);
                Canvas.SetTop(canvas, 90);
            }
            else if (iX == 7)
            {
                Canvas.SetLeft(canvas, 185);
                Canvas.SetTop(canvas, 20);
            }
            else if (iX == 8)
            {
                Canvas.SetLeft(canvas, 250);
                Canvas.SetTop(canvas, 55);
            }
            else if (iX == 9)
            {
                Canvas.SetLeft(canvas, 285);
                Canvas.SetTop(canvas, 115);
            }
            else if (iX == 10)
            {
                Canvas.SetLeft(canvas, 285);
                Canvas.SetTop(canvas, 185);
            }
            else if (iX == 11)
            {
                Canvas.SetLeft(canvas, 250);
                Canvas.SetTop(canvas, 245);
            }
            else if (iX == 12)
            {
                Canvas.SetLeft(canvas, 185);
                Canvas.SetTop(canvas, 280);
            }
            else if (iX == 13)
            {
                Canvas.SetLeft(canvas, 115);
                Canvas.SetTop(canvas, 280);
            }
            else if (iX == 14)
            {
                Canvas.SetLeft(canvas, 55);
                Canvas.SetTop(canvas, 245);
            }
            else if (iX == 15)
            {
                Canvas.SetLeft(canvas, 20);
                Canvas.SetTop(canvas, 185);
            }
            else if (iX == 16)
            {
                Canvas.SetLeft(canvas, 20);
                Canvas.SetTop(canvas, 115);
            }
            else if (iX == 17)
            {
                Canvas.SetLeft(canvas, 50);
                Canvas.SetTop(canvas, 55);
            }
            else if (iX == 18)
            {
                Canvas.SetLeft(canvas, 115);
                Canvas.SetTop(canvas, 20);
            }

            if (strShape == "Ellipse")
            {
                // 원 UI 생성 및 설정
                cObjectUnit.ellipse = new Ellipse();
                if (dEllipseWidthHeight != 0) cObjectUnit.ellipse.Width = cObjectUnit.ellipse.Height = dEllipseWidthHeight;
                cObjectUnit.ellipse.MouseLeftButtonDown += elUnit_MouseLeftButtonDown;
                cObjectUnit.ellipse.Name = string.Format("elUnit{0}", cObjectUnit.iNo); // 데이터 Object 이름 정의
                cCanvas.RegisterName(cObjectUnit.ellipse.Name, cObjectUnit.ellipse);      // 등록하지 않으면 FineName에서 검색되지 않음
                cObjectUnit.ellipse.Tag = string.Format("{0}", cObjectUnit.iNo);
                canvas.Children.Add(cObjectUnit.ellipse);
            }
            else if (strShape == "Border")
            {
                // 사각형 UI 생성 및 설정
                cObjectUnit.border = new Border();
                cObjectUnit.border.MouseLeftButtonDown += elUnit_MouseLeftButtonDown;
                cObjectUnit.border.Name = string.Format("elUnit{0}", cObjectUnit.iNo); // 데이터 Object 이름 정의
                cCanvas.RegisterName(cObjectUnit.border.Name, cObjectUnit.border);      // 등록하지 않으면 FineName에서 검색되지 않음
                cObjectUnit.border.Tag = string.Format("{0}", cObjectUnit.iNo);
                cObjectUnit.border.CornerRadius = new CornerRadius(iBorderCornerRadius);
                canvas.Children.Add(cObjectUnit.border);
            }

            if (bShowTextInfo == true)
            {
                // 맵 내부 텍스트 생성 및 설정
                cObjectUnit.textBlock = new TextBlock();
                cObjectUnit.textBlock.Foreground = Brushes.White;
                if (iTextInfoWidth != 0) cObjectUnit.textBlock.Width = iTextInfoWidth;
                cObjectUnit.textBlock.Margin = TkTextInfoMargin;
                cObjectUnit.textBlock.TextWrapping = TextWrapping.Wrap;
                cObjectUnit.textBlock.TextAlignment = TextAlignment.Center;
                cObjectUnit.textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                cObjectUnit.textBlock.VerticalAlignment = VerticalAlignment.Center;
                // UI의 각도가 회전되어 있을 경우 폰트에 역 각도를 설정하여 보정
                RotateTransform rotation = RenderTransform as RotateTransform;
                if (rotation != null)
                {
                    cObjectUnit.textBlock.RenderTransformOrigin = new Point(0.5, 0.5);
                    cObjectUnit.textBlock.RenderTransform = new RotateTransform(rotation.Angle * -1);
                }

                // 맵 내부 텍스트 크기 자동 조절을 위한 ViewBox 설정
                cObjectUnit.viewbox = new Viewbox();
                cObjectUnit.viewbox.Child = cObjectUnit.textBlock;
                cObjectUnit.viewbox.Stretch = Stretch.Uniform;
                cObjectUnit.viewbox.MouseLeftButtonDown += Viewbox_MouseLeftButtonDown;
                cObjectUnit.viewbox.Tag = string.Format("{0}", cObjectUnit.iNo);    // View Box가 클릭되었을 경우 데이터 Objec로 이벤트 토스를 위한 번호 정의
                canvas.Children.Add(cObjectUnit.viewbox);
            }

            if (bShowTextNo == true)
            {
                // 맵 내부 텍스트 생성 및 설정
                TextBlock textBlock = new TextBlock();
                textBlock.Foreground = Brushes.White;
                textBlock.TextAlignment = TextAlignment.Center;
                textBlock.HorizontalAlignment = HorizontalAlignment.Left;
                textBlock.VerticalAlignment = VerticalAlignment.Top;
                textBlock.Margin = new Thickness(2);
                if (mapDataLib.strDataName == Enum.GetName(typeof(eData), eData.MPC1_PIPE_MOUNT) ||
                    mapDataLib.strDataName == Enum.GetName(typeof(eData), eData.MPC1_NEEDLE_MOUNT) ||
                    mapDataLib.strDataName == Enum.GetName(typeof(eData), eData.MPC1_BUFFER_2))
                {
                    textBlock.Text = (cBaseMapData.iUnitNo + 1).ToString();
                }
                else
                {
                    textBlock.Text = cBaseMapData.iUnitNo.ToString();
                }
                textBlock.FontSize = 11;
                RotateTransform rotation = RenderTransform as RotateTransform;
                if (rotation != null)
                {
                    textBlock.RenderTransformOrigin = new Point(0.5, 0.5);
                    textBlock.RenderTransform = new RotateTransform(rotation.Angle * -1);
                }
                canvas.Children.Add(textBlock);
            }
            cCanvas.Children.Add(canvas);
            ObjectUIList.Add(cObjectUnit);
        }

        /// <summary>
        /// 모든 상태 표시용 스톱 워치
        /// </summary>
        private Stopwatch stopwatch = new Stopwatch();

        /// <summary>
        /// 클로즈 버튼 오래 누르고 있으면 모든 상태변경 창을 연다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Button_Down(object sender, System.Windows.RoutedEventArgs e)
        {
            stopwatch.Restart();
        }

        /// <summary>
        /// Popup 창의 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Popup_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CMainLib.Ins.McState == eMachineState.RUN ||
                CMainLib.Ins.McState == eMachineState.MANUALRUN ||
                _bPopupUse == false) return;

            Button button = sender as Button;
            eStatus eStatus = eStatus.INIT;
            Popup_UnitStatus.IsOpen = false;
            Popup_AllUnitStatus.IsOpen = false;
            MapDataFunction.bPopupOpen = false;

            if ((string)button.Content == "CLOSE")                              // 닫기 팝업 버튼 클릭
            {
                if (stopwatch.ElapsedMilliseconds >= 1000)
                {
                    stopwatch.Reset();
                    Popup_AllUnitStatus.IsOpen = true;
                    MapDataFunction.bPopupOpen = true;
                }
                stopwatch.Stop();
                return;
            }

            // 팝업 버튼의 데이터 정보 값 취득
            bool bAllFlag = false, bUntilFlag = false;
            foreach (var Number in Enum.GetValues(typeof(eStatus)))
            {
                string strName = Enum.GetName(typeof(eStatus), (int)Number);
                if ((string)button.Content == strName)                              // 데이터 하나 변경 클릭
                {
                    eStatus = (eStatus)Number;
                    break;
                }
                else if (((string)button.Content).Contains("UT_") == true &&       // UNTIL 데이터 변경 클릭
                         ((string)button.Content).Replace("UT_", "") == strName)
                {
                    bUntilFlag = true;
                    eStatus = (eStatus)Number;
                    break;
                }
                else if (((string)button.Content).Contains("ALL_") == true &&       // ALL 데이터 변경 클릭
                         ((string)button.Content).Replace("ALL_", "") == strName)
                {
                    bAllFlag = true;
                    eStatus = (eStatus)Number;
                    break;
                }
            }

            // 상태 정보 데이터 설정
            string strLog = string.Empty;
            MapDataLib mapDataLib = CMainLib.Ins.cRunUnitData.GetIndexData(eData);
            if (bUntilFlag == true)
            {
                int iUnitNo = 0;
                string strTag = frameworkElement.Tag.ToString();
                if (int.TryParse(strTag, out iUnitNo) == false) return;
                mapDataLib.SetUntilStatus(iUnitNo, eStatus);
            }
            else if (bAllFlag == true)
            {
                // 전, 후면 팔레트 맨 왼쪾 및 맨 오른쪽 데이터를 변경시 팔레트 존재 유무 확인
                // *********************************************************************************************************************************
                if (eData == eData.MPC1_FAR_LEFT) return;
                // *********************************************************************************************************************************

                mapDataLib.SetAllStatus(eStatus, true);
                strLog = string.Format($"MapData Change [Map = {eData}] [All Data = {eStatus}]");
            }
            else
            {
                // 전, 후면 팔레트 맨 왼쪾 및 맨 오른쪽 데이터를 변경시 팔레트 존재 유무 확인
                // *********************************************************************************************************************************
                if (eData == eData.MPC1_FAR_LEFT)
                {
                    if (eStatus == eStatus.NONE)
                    {
                        if (CMainLib.Ins.Seq.SeqIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_LEFT) == true)
                        {
                            return;
                        }
                        else
                        {
                            double GetActLeftRail_Y = CMainLib.Ins.Axis[eMotor.MPC_LEFT_Y].GetActPostion();
                            double LeftRail_Y_FrontPos = CMainLib.Ins.cAxisPosCollData.GetAxisPosition(eMotor.MPC_LEFT_Y, (int)eAxisLeftMPC_Y.Front);
                            double LeftRail_Y_SupplyPos = CMainLib.Ins.cAxisPosCollData.GetAxisPosition(eMotor.MPC_LEFT_Y, (int)eAxisLeftMPC_Y.HolderSupply);
                            if ((GetActLeftRail_Y >= LeftRail_Y_FrontPos - 0.05 &&
                                 GetActLeftRail_Y <= LeftRail_Y_FrontPos + 0.05) ||
                                (GetActLeftRail_Y >= LeftRail_Y_SupplyPos - 0.05 &&
                                 GetActLeftRail_Y <= LeftRail_Y_SupplyPos + 0.05))
                            {
                                return;
                            }
                        }
                    }
                    else if (eStatus == eStatus.MOUNT)
                    {
                        double GetActLeftRail_Y = CMainLib.Ins.Axis[eMotor.MPC_LEFT_Y].GetActPostion();
                        double LeftRail_Y_SupplyPos = CMainLib.Ins.cAxisPosCollData.GetAxisPosition(eMotor.MPC_LEFT_Y, (int)eAxisLeftMPC_Y.HolderSupply);
                        if (GetActLeftRail_Y >= LeftRail_Y_SupplyPos - 0.05 &&
                            GetActLeftRail_Y <= LeftRail_Y_SupplyPos + 0.05)
                        {
                            // 맵 변경 가능 위치
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if (eStatus == eStatus.EMPTY || eStatus == eStatus.UV)
                    {
                        double GetActLeftRail_Y = CMainLib.Ins.Axis[eMotor.MPC_LEFT_Y].GetActPostion();
                        double LeftRail_Y_FrontPos = CMainLib.Ins.cAxisPosCollData.GetAxisPosition(eMotor.MPC_LEFT_Y, (int)eAxisLeftMPC_Y.HolderSupply);
                        if (GetActLeftRail_Y >= LeftRail_Y_FrontPos - 0.05 &&
                            GetActLeftRail_Y <= LeftRail_Y_FrontPos + 0.05)
                        {
                            // 맵 변경 가능 위치
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if (eStatus == eStatus.HOLDER)
                    {
                        if (CMainLib.Ins.Seq.SeqIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_LEFT) != true) return;
                    }
                }
                else if (eData == eData.MPC1_FAR_RIGHT)
                {
                    if (eStatus == eStatus.NONE)
                    {
                        if (CMainLib.Ins.Seq.SeqIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_RIGHT) == true) return;
                    }
                    else
                    {
                        if (CMainLib.Ins.Seq.SeqIO.GetInput((int)eIO_I.PALLETS_DETECTION_FRONT_RIGHT) != true) return;
                    }
                }
                else if (eData == eData.MPC2_FAR_LEFT)
                {
                    if (eStatus == eStatus.NONE)
                    {
                        if (CMainLib.Ins.Seq.SeqIO.GetInput((int)eIO_I.PALLETS_DETECTION_REAR_LEFT) == true) return;
                    }
                    else
                    {
                        if (CMainLib.Ins.Seq.SeqIO.GetInput((int)eIO_I.PALLETS_DETECTION_REAR_LEFT) != true) return;
                    }
                }
                else if (eData == eData.MPC2_FAR_RIGHT)
                {
                    if (eStatus == eStatus.NONE)
                    {
                        if (CMainLib.Ins.Seq.SeqIO.GetInput((int)eIO_I.PALLETS_DETECTION_REAR_RIGHT) == true) return;
                    }
                    else
                    {
                        if (CMainLib.Ins.Seq.SeqIO.GetInput((int)eIO_I.PALLETS_DETECTION_REAR_RIGHT) != true) return;
                    }
                }
                // *********************************************************************************************************************************

                int iUnitNo = 0;
                string strTag = frameworkElement.Tag.ToString();
                if (int.TryParse(strTag, out iUnitNo) == false) return;
                BaseMapData baseMapData = mapDataLib.GetUnitNo(iUnitNo);

                // 니들 마운트 작업 맵 데이터면 해당 홀의 작업 여부에 따라 변경 확인한다.
                // **********************************************************************************
                if (eData == eData.MPC1_NEEDLE_MOUNT)
                {
                    if (CMainLib.Ins.cSysOne.bHolderNeedlePinWorkCount[iUnitNo] == false) return;
                }
                // **********************************************************************************

                eStatus Oldstatus = baseMapData.eStatus;
                baseMapData.eStatus = eStatus;
                strLog = string.Format($"MapData Change [Map = {eData}] [Index = {baseMapData.iIndexX},{baseMapData.iIndexY}] [Unit No : {iUnitNo}] [Data = {Oldstatus} > {eStatus}]");
            }
            NLogger.AddLog(eLogType.SEQ_MAIN, NLogger.eLogLevel.INFO, strLog, false);
        }

        /// <summary>
        /// 여러 Object를 따로 만들기 번거로워 UI Tag를 얻을 수 있는 클래스로 대체
        /// </summary>
        private FrameworkElement frameworkElement = null;

        /// <summary>
        /// Unit UI를 클릭했을 경우 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void elUnit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CMainLib.Ins.McState == eMachineState.RUN ||
                CMainLib.Ins.McState == eMachineState.MANUALRUN ||
                _bPopupUse == false) return;

            if (Popup_UnitStatus.IsOpen == true ||
                MapDataFunction.bPopupOpen == true) return;

            frameworkElement = (FrameworkElement)sender;
            Popup_UnitStatus.PlacementTarget = (UIElement)sender;
            Popup_AllUnitStatus.PlacementTarget = (UIElement)sender;
            // 팝업창을 띄울 경우 UI에 각도가 있으면 기울여져서 표시되므로 보정하여 표시
            RotateTransform rotation = RenderTransform as RotateTransform;
            if (rotation != null)
            {
                Popup_UnitStatus.RenderTransformOrigin = new Point(0.5, 0.5);
                Popup_AllUnitStatus.RenderTransformOrigin = new Point(0.5, 0.5);
                Popup_UnitStatus.RenderTransform = new RotateTransform(rotation.Angle * -1);
                Popup_AllUnitStatus.RenderTransform = new RotateTransform(rotation.Angle * -1);
            }

            Popup_UnitStatus.IsOpen = true;
            MapDataFunction.bPopupOpen = true;
        }

        /// <summary>
        /// Unit UI의 TextBlock 측을 누를경우 텍스트에 이벤트가 발생하면 다시 Unit Map에 이벤트를 전달한다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Viewbox_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (CMainLib.Ins.McState == eMachineState.RUN ||
                CMainLib.Ins.McState == eMachineState.MANUALRUN) return;

            Viewbox viewbox = sender as Viewbox;
            int iUINo = 0;
            if (int.TryParse(viewbox.Tag.ToString(), out iUINo) == false) return;
            object Object = FindName("elUnit" + iUINo.ToString());
            if (Object != null)
            {
                MouseDevice mouseDevice = Mouse.PrimaryDevice;
                MouseButtonEventArgs mouseButtonEventArgs = new MouseButtonEventArgs(mouseDevice, 0, MouseButton.Left);
                mouseButtonEventArgs.RoutedEvent = Mouse.MouseDownEvent;
                mouseButtonEventArgs.Source = Object;
                ((UIElement)Object).RaiseEvent(mouseButtonEventArgs);
            }
        }

        /// <summary>
        /// Old 맵 데이터 리셋(DataEdit와 MainUI의 데이터 화면 표시를 위해서)
        /// </summary>
        public void ResetOldMapData()
        {
            MapDataLib mapDataLib = CMainLib.Ins.cRunUnitData.GetIndexData(eData);
            mapDataLib.OldMapDataReset();
        }

        /// <summary>
        /// 반복 갱신 함수
        /// </summary>
        public void RepeatUpdateTimer()
        {
            if (bCanUpdateView == true)
            {
                MapDataLib mapDataLib = CMainLib.Ins.cRunUnitData.GetIndexData(eData);
                foreach (BaseMapData baseMapData in mapDataLib.BaseMapDataList)
                {
                    if (baseMapData.eStatus != baseMapData.eOldStatus)
                    {
                        baseMapData.eOldStatus = baseMapData.eStatus;
                        CObjectUnit cObjectUnit = ObjectUIList.Find(x => x.iNo == baseMapData.iUnitNo);
                        if (cObjectUnit.ellipse != null) cObjectUnit.ellipse.Fill = MapDataFunction.StatusColor(baseMapData.eStatus);
                        else if (cObjectUnit.border != null) cObjectUnit.border.Background = MapDataFunction.StatusColor(baseMapData.eStatus);
                        if (bShowTextInfo == true) cObjectUnit.textBlock.Text = baseMapData.eStatus.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Map Data Update
        /// </summary>
        public void UpdateMapData()
        {
            if (bCanUpdateView == true)
            {
                MapDataLib mapDataLib = CMainLib.Ins.cRunUnitData.GetIndexData(eData);
                foreach (BaseMapData baseMapData in mapDataLib.BaseMapDataList)
                {
                    CObjectUnit cObjectUnit = ObjectUIList.Find(x => x.iNo == baseMapData.iUnitNo);
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate ()
                    {
                        if (cObjectUnit.ellipse != null) cObjectUnit.ellipse.Fill = MapDataFunction.StatusColor(baseMapData.eStatus);
                        else if (cObjectUnit.border != null) cObjectUnit.border.Background = MapDataFunction.StatusColor(baseMapData.eStatus);
                        if (bShowTextInfo == true) cObjectUnit.textBlock.Text = baseMapData.eStatus.ToString();
                    });
                }
            }
        }
    }

    /// <summary>
    /// 각 UI를 접근하기 위한 Object 저장 클래스
    /// </summary>
    internal class CObjectUnit
    {
        /// <summary>
        /// Index X
        /// </summary>
        public int iX = 0;

        /// <summary>
        /// Index Y
        /// </summary>
        public int iY = 0;

        /// <summary>
        /// Unit No
        /// </summary>
        public int iNo = 0;

        /// <summary>
        /// 원 Unit Map
        /// </summary>
        public Ellipse ellipse = null;

        /// <summary>
        /// 사각형 Unit Map
        /// </summary>
        public Border border = null;

        /// <summary>
        /// Text Font 사이즈 자동 조절 View
        /// </summary>
        public Viewbox viewbox = null;

        /// <summary>
        /// 정보 제공 Text
        /// </summary>
        public TextBlock textBlock = null;
    }
}