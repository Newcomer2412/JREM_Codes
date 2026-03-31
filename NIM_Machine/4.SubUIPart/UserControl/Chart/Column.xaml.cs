using System;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Controls;
using System.Windows.Media;

namespace MachineControlBase
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Column : UserControl
    {
        public SeriesCollection SeriesCollection { get; set; }
        public AxesCollection AxisYCollection { get; set; }

        public string[] Labels { get; set; }
        public Func<int, string> Formatter { get; set; }

        public ColumnSeries MyCs;
        public ChartValues<int> MyCv;

        public Column()
        {
            InitializeComponent();
            Read_Data();
        }

        public void Read_Data()
        {
            SeriesCollection = new SeriesCollection();

            MyCs = new ColumnSeries();
            MyCs.Title = "Number of Error : ";

            MyCv = new ChartValues<int>();
            MyCs.Values = MyCv;

            SeriesCollection.Add(MyCs);

            Formatter = value => value.ToString("N");

            DataContext = this;
        }
    }
}