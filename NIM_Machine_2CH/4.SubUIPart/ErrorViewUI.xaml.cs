using System;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MachineControlBase
{
    /// <summary>
    /// GraphViewUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ErrorViewUI : UserControl
    {
        /// <summary>
        /// DB 값을 불러와 배열에 저장하기 위한 값 지정
        /// </summary>
        public static int Row_Cnt = 0; // 검색 범위 내 데이터 베이스의 총 수

        public static string[,] Save; // 검색 범위의 모든 데이터를 저장
        public static int[] Total_F;  // 에러가 발생한 총 횟수 값을 입력
        public static string[] Cde;  // 에러 코드 저장
        public static string[] Cde_Name;  // 에러명 저장
        public static string strName = "Error"; //error.db 불러오기 위함

        /// <summary>
        /// ListView 바인딩.
        /// </summary>
        public class ListViewBind
        {
            public string ErrorCode { get; set; }
            public string ErrorName { get; set; }
            public DateTime Time { get; set; }
            public int Count { get; set; }
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public ErrorViewUI()
        {
            InitializeComponent();

            //DBFileCheck();
            // 실행 시 설정된 시간으로 Set
            DateTime ini = default(DateTime);

            St_date.txtHours.Text = ini.ToString("HH");
            St_date.txtMinutes.Text = ini.ToString("mm");
            St_date.txtSeconds.Text = ini.ToString("ss");
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
            // Listview 클리어
            LvResult.Items.Clear();
            lvAllerror.Items.Clear();

            //Read ListView and Graph
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
                // ---------------------- Table 전체 값 카운팅 -----------------------------//
                string LCnt = "SELECT COUNT(*) FROM " + strName + " WHERE DateTime BETWEEN ( '" + strTotalStartTime + "') And ( '" + strTotalEndTime + "')";

                SQLiteCommand LCnt_comm = new SQLiteCommand(LCnt, CMainLib.Ins.SQLError);
                Row_Cnt = Convert.ToInt32(LCnt_comm.ExecuteScalar());  // 검색 범위 내 값을 카운트

                Save = GetTable(strName, CMainLib.Ins.SQLError); // 검색 된 값을 모두 저장.

                if (Row_Cnt == 0)
                {
                    Column.Visibility = Visibility.Hidden;
                    return;
                }
                else if (Row_Cnt != 0)
                {
                    int icnt = 0; // 이상한 에러코드를 거를 숫자를 세기 위함

                    // 값을 지우기 위한
                    int Amount = 0;
                    int iToRemove = 0;
                    string sToRemove = null;

                    Cde = new string[Row_Cnt];  // 에러 코드를 담을 배열
                    Total_F = new int[Row_Cnt]; // 합을 담을 배열

                    for (int i = 0; i < Row_Cnt; i++)
                    {
                        string strEName = Enum.GetName(typeof(eErrorCode), Convert.ToInt32(Save[i, 1]));

                        lvAllerror.Items.Add(new ListViewBind
                        {
                            Time = Convert.ToDateTime(Save[i, 0]),
                            ErrorCode = Save[i, 1],
                            ErrorName = strEName,
                        });
                    }

                    // 총 갯수, Freq 값 더하기 및 에러코드 저장
                    for (int k = 0; k < Row_Cnt; k++)
                    {
                        if (Convert.ToInt32(Save[k, 1]) == -1) // 에러코드가 -1 이면 그냥 넘김
                        {
                            icnt++;
                            continue;
                        }
                        Cde[k - icnt] = Save[k, 1]; // 그냥 넘긴 만큼을 제외.

                        for (int i = k + 1; i < Row_Cnt; i++)
                        {
                            if (Save[k, 1] == Save[i, 1])
                            {
                                Save[i, 1] = null;
                                Amount += Convert.ToInt32(Save[i, 2]);
                                Save[i, 2] = "0";
                            }
                            if (Save[k, 1] != Save[i, 1]) continue;
                        }
                        Total_F[(k - icnt)] = (Convert.ToInt32(Save[k, 2]) + Amount);
                        Amount = 0;
                    }
                    Total_F = Total_F.Where((source, index) => source != iToRemove).ToArray();  // 0은 다 삭제
                    Cde = Cde.Where((source, index) => source != sToRemove).ToArray();          // null은 다 삭제

                    // 리스트에 보여주기
                    for (int i = 0; i < Total_F.Length; i++)
                    {
                        string strEName = Enum.GetName(typeof(eErrorCode), Convert.ToInt32(Cde[i]));

                        LvResult.Items.Add(new ListViewBind
                        {
                            ErrorCode = Cde[i],
                            ErrorName = strEName,
                            Count = Total_F[i],
                        });
                    }
                    Column.Visibility = Visibility.Visible;
                    Graph_Showing();
                }
            }
            catch (Exception ex)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.FATAL, ex.Message.ToString());
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void FindTable()
        {
            String strName = "Error";  //error.db 불러오기 위함

            // ---------------------- Table 전체 값 카운팅 -----------------------------//
            string LCnt = "SELECT COUNT(*) FROM " + strName + " WHERE DateTime BETWEEN ( '" + strTotalStartTime + "') And ( '" + strTotalEndTime + "')";
            SQLiteCommand LCnt_comm = new SQLiteCommand(LCnt, CMainLib.Ins.SQLError);
            Row_Cnt = Convert.ToInt32(LCnt_comm.ExecuteScalar());
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
            /*get number of columns in table*/
            /*only need to read one row*/
            string sqlstr = "SELECT * FROM " + tablename + " WHERE DateTime BETWEEN ( '" + strTotalStartTime + "') And ( '" + strTotalEndTime + "')";
            SQLiteCommand cmd = new SQLiteCommand(sqlstr, conn);
            SQLiteDataReader reader = cmd.ExecuteReader();
            reader.Read();

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

            sqlstr = "SELECT COUNT(*) FROM " + tablename + " WHERE DateTime BETWEEN ( '" + strTotalStartTime + "') And ( '" + strTotalEndTime + "')";
            cmd = new SQLiteCommand(sqlstr, conn);
            object v = cmd.ExecuteScalar();
            rows = Convert.ToInt32(v);

            sqlstr = "SELECT * FROM " + tablename + " WHERE DateTime BETWEEN ( '" + strTotalStartTime + "') And ( '" + strTotalEndTime + "')";
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
            Column.MyCv.Clear();
            Column.Labels = new string[Total_F.Length];

            for (int i = 0; i < Total_F.Length; i++)
            {
                string strEName = Enum.GetName(typeof(eErrorCode), Convert.ToInt32(Cde[i]));  //이름으로 보여주기 위한 코드 입니다.

                Column.Labels[i] = Cde[i];  // 현재는 코드 번호로 나타내는 중
                Column.MyCv.Add(Total_F[i]);
            }
        }
    }
}