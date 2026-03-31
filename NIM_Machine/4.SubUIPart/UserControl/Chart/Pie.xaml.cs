using System;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Controls;
using System.Windows.Media;

namespace MachineControlBase
{
    /// <summary>
    /// Pie.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Pie : UserControl
    {
        public SeriesCollection SeriesCollection { get; set; }

        public string[] Labels { get; set; }
        public Func<int, string> Formatter { get; set; }

        public PieSeries MyPs1;
        public PieSeries MyPs2;

        public ChartValues<double> MyPv1;
        public ChartValues<double> MyPv2;

        private BrushConverter ColorChange = new BrushConverter();

        public Pie()
        {
            InitializeComponent();
            PointLabel = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);
            DataContext = this;
            PieAdding();
        }

        public void PieAdding()
        {
            PieChart pieChart = new PieChart();
            ((DefaultLegend)pieChart.ChartLegend).BulletSize = 1000;
            ((DefaultTooltip)pieChart.DataTooltip).BulletSize = 100;

            pieChart.LegendLocation = LegendLocation.Bottom;

            SeriesCollection = new SeriesCollection();

            MyPs1 = new PieSeries();
            MyPs2 = new PieSeries();

            MyPs1.Title = "Good";
            MyPs2.Title = "Fail";

            MyPv1 = new ChartValues<double>();
            MyPv2 = new ChartValues<double>();

            MyPs1.Values = MyPv1;
            MyPs1.Fill = (SolidColorBrush)(ColorChange.ConvertFrom("#00CC66"));

            MyPs2.Values = MyPv2;
            MyPs2.Fill = (SolidColorBrush)(ColorChange.ConvertFrom("#FF7272"));

            SeriesCollection.Add(MyPs1);
            SeriesCollection.Add(MyPs2);

            Labels = new string[1];
            Formatter = value => value.ToString("N");

            DataContext = this;
        }

        public Func<ChartPoint, string> PointLabel { get; set; }

        private void Chart_OnDataClick(object sender, ChartPoint chartpoint)
        {
            var chart = (LiveCharts.Wpf.PieChart)chartpoint.ChartView;

            //clear selected slice.
            foreach (PieSeries series in chart.Series)
                series.PushOut = 0;

            var selectedSeries = (PieSeries)chartpoint.SeriesView;
            selectedSeries.PushOut = 7;
        }
    }
}