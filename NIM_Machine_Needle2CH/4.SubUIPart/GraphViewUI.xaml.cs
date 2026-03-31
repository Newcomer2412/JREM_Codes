using LiveCharts;
using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MachineControlBase
{
    /// <summary>
    /// GraphViewUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GraphViewUI : UserControl
    {
        /// <summary>
        /// save the Table into array
        /// </summary>
        public static string[][][] F_Table;

        public static string[] sCapaG = new string[2];
        public static string strName = "MassProduction";

        /// <summary>
        /// 생성자
        /// </summary>
        public GraphViewUI()
        {
            InitializeComponent();
            //DBFileCheck();
            // 실행 시 설정된 시간으로 Set
            DateTime ini = default(DateTime);
            St_date.txtHours.Text = ini.ToString("HH");
            St_date.txtMinutes.Text = ini.ToString("mm");
            St_date.txtSeconds.Text = ini.ToString("ss");
            Pie.myPieChart.LegendLocation = LegendLocation.Bottom;
            LoadDBData();
        }

        /// <summary>
        /// 포커스를 얻거나 잃을 때 타이머 제어
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                Ed_date.txtHours.Text = DateTime.Now.ToString("HH");
                Ed_date.txtMinutes.Text = DateTime.Now.ToString("mm");
                Ed_date.txtSeconds.Text = DateTime.Now.ToString("ss");

                BtSearch.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
        }

        private DateTime DtStartDate;
        private static string strStartDate;
        private static string strEndDate;

        /// <summary>
        /// 시작 날짜 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DpStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DtStartDate = (DateTime)((DatePicker)sender).SelectedDate;
            strStartDate = DtStartDate.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 종료 날짜 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DpEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime DtEndDate = (DateTime)((DatePicker)sender).SelectedDate;
            if (DtStartDate <= DtEndDate) strEndDate = DtEndDate.ToString("yyyy-MM-dd");
            else if (DtStartDate > DtEndDate) CCommon.ShowMessageMini("Start date is not able to be earlier than End date");
        }

        private string strTotalStartTime;
        private string strTotalEndTime;

        /// <summary>
        /// 날짜 값과 시간값을 입력
        /// </summary>
        public void SetDateAndTime()
        {
            strTotalStartTime = strStartDate + St_date.DateTimeValue.Value.ToString(" HH:mm:ss");
            strTotalEndTime = strEndDate + Ed_date.DateTimeValue.Value.ToString(" HH:mm:ss");
        }

        /// <summary>
        /// 검색 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void YSrch_btn_Click(object sender, RoutedEventArgs e)
        {
            LoadDBData();
        }

        /// <summary>
        /// DB 데이터에서 설정한 날짜 범위 안의 데이터를 배열에 저장한다.
        /// </summary>
        private void LoadDBData()
        {
            SetDateAndTime();
            try
            {
                int Total_T = 0, Total_G = 0, Total_NG = 0;

                // ---------------------- Table 전체 값 카운팅 -----------------------------//
                // Performance 테이블 내 값의 수 카운팅
                string LCnt = "SELECT COUNT(*) FROM " + strName + " WHERE Start BETWEEN ( '" + strTotalStartTime + "') And ( '" + strTotalEndTime + "')";
                SQLiteCommand LCnt_comm = new SQLiteCommand(LCnt, CMainLib.Ins.SQLProduction);

                int Row_Cnt = Convert.ToInt32(LCnt_comm.ExecuteScalar());

                if (Row_Cnt == 0)
                {
                    return;
                }

                // 테이블을 Arrr 어레이에 저장
                string[,] Arrr = GetTable(strName, CMainLib.Ins.SQLProduction);

                // 총 갯수, Good, Not Good 값 더하기
                for (int k = 0; k < Row_Cnt; k++)
                {
                    int iTotal = Convert.ToInt32(Arrr[k, 2]);
                    int iG = Convert.ToInt32(Arrr[k, 3]);
                    int iNG = Convert.ToInt32(Arrr[k, 4]);

                    Total_T += iTotal;
                    Total_G += iG;
                    Total_NG += iNG;
                }

                sCapaG[0] = Total_G.ToString();
                sCapaG[1] = Total_NG.ToString();

                if (Total_T != 0)
                {
                    Pie.Visibility = Visibility.Visible;
                    Graph_Showing();
                }

                if (Total_T == 0)
                {
                    Pie.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.FATAL, ex.Message.ToString());
                MessageBox.Show(ex.Message);
                return;
            }
        }

        /// <summary>
        /// 테이블 값을 배열 형태로 가져오는 함수
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        private string[,] GetTable(string tablename, SQLiteConnection conn)
        {
            int cols = 0, rows = 0;
            string sqlstr = "SELECT * FROM " + tablename + " WHERE Start BETWEEN ( '" + strTotalStartTime + "') And ( '" + strTotalEndTime + "')";

            SQLiteCommand cmd = new SQLiteCommand(sqlstr, conn);
            SQLiteDataReader reader = cmd.ExecuteReader();
            reader.Read();

            /*get number of columns in table*/
            /*only need to read one row*/
            while (true)
            {
                try
                {
                    reader.GetValue(cols);
                    cols++;
                }
                catch (Exception)
                {
                    break;
                }
            }

            sqlstr = "SELECT COUNT(*) FROM " + tablename + " WHERE Start BETWEEN ( '" + strTotalStartTime + "') And ( '" + strTotalEndTime + "')";
            cmd = new SQLiteCommand(sqlstr, conn);
            object v = cmd.ExecuteScalar();
            rows = Convert.ToInt32(v);

            sqlstr = "SELECT * FROM " + tablename + " WHERE Start BETWEEN ( '" + strTotalStartTime + "') And ( '" + strTotalEndTime + "')";
            cmd = new SQLiteCommand(sqlstr, conn);
            reader = cmd.ExecuteReader();

            string[,] array = new string[rows, cols];
            for (int y = 0; y < rows; y++)
            {
                reader.Read();
                for (int x = 0; x < cols; x++) array[y, x] = reader.GetValue(x).ToString();
            }

            // clean up
            cmd.Dispose();
            reader.Close();
            return array;
        }

        public void Graph_Showing()
        {
            Pie.Visibility = Visibility.Visible;

            Pie.MyPv1.Clear();
            Pie.MyPv2.Clear();

            Pie.MyPv1.Add(Convert.ToDouble(sCapaG[0]));
            Pie.MyPv2.Add(Convert.ToDouble(sCapaG[1]));

            Pie.MyPs1.FontSize = 33;
            Pie.MyPs2.FontSize = 33;
        }
    }
}