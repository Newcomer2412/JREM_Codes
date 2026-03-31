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
    public partial class VisionGraphViewUI : UserControl
    {
        /// <summary>
        /// ListView 바인딩.
        /// </summary>
        public class ListViewBind
        {
            public String N_Cam { get; set; }
            public int Total { get; set; }
            public int Good { get; set; }
            public int Not_Good { get; set; }
            public String Function { get; set; }
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public VisionGraphViewUI()
        {
            InitializeComponent();
            // 실행 시 설정된 시간으로 Set
            DateTime ini = default(DateTime);
            St_date.txtHours.Text = ini.ToString("HH");
            St_date.txtMinutes.Text = ini.ToString("mm");
            St_date.txtSeconds.Text = ini.ToString("ss");

            for (int i = 0; i < Define.MAX_CAMERA; i++)
            {
                F_Table[i] = new string[CMainLib.Ins.cVisionData.strToolBlockName[i].Length][];
            }
            for (int i = 0; i < Define.MAX_CAMERA; i++)
            {
                for (int j = 0; j < CMainLib.Ins.cVisionData.strToolBlockName[i].Length; j++)
                {
                    F_Table[i][j] = new string[5];
                }
            }
            F_Table.Initialize();
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
            LoadDBData();
            Init_Pannel();
        }

        /// <summary>
        /// save the Table into array
        /// </summary>
        public static string[][][] F_Table = new string[Define.MAX_CAMERA + 4][][];

        /// <summary>
        /// DB 데이터에서 설정한 날짜 범위 안의 데이터를 배열에 저장한다.
        /// </summary>
        private void LoadDBData()
        {
            SetDateAndTime();
            try
            {
                for (int i = 0; i < Define.MAX_CAMERA; i++)
                {
                    for (int j = 0; j < CMainLib.Ins.cVisionData.strToolBlockName[i].Length; j++)
                    {
                        string strCamName = ((eCAM)i).ToString();
                        string strFuncName = strCamName + "_" + CMainLib.Ins.cVisionData.strToolBlockName[i][j];
                        int Total_T = 0, Total_G = 0, Total_NG = 0;

                        // ---------------------- Table 전체 값 카운팅 -----------------------------//
                        // Performance 테이블 내 값의 수 카운팅
                        string P_Cnt = "SELECT COUNT(*) FROM " + strFuncName + " WHERE Start BETWEEN ( '" + strTotalStartTime + "') And ( '" + strTotalEndTime + "')";
                        SQLiteCommand P_cnt_comm = new SQLiteCommand(P_Cnt, CMainLib.Ins.SQLVision);
                        int Row_Cnt = Convert.ToInt32(P_cnt_comm.ExecuteScalar());

                        if (Row_Cnt == 0) continue;
                        else if (Row_Cnt != 0)
                        {
                            // 테이블을 Arr 어레이에 저장
                            string[,] Arrr = GetTable(strFuncName, CMainLib.Ins.SQLVision);

                            // 총 갯수, Good, Not Good 값 더하기
                            for (int k = 0; k < Row_Cnt; k++)
                            {
                                int Total = Convert.ToInt32(Arrr[k, 3]);
                                int G = Convert.ToInt32(Arrr[k, 4]);
                                int NG = Convert.ToInt32(Arrr[k, 5]);

                                Total_T += Total;
                                Total_G += G;
                                Total_NG += NG;
                            }

                            F_Table[i][j][0] = Arrr[0, 0];
                            F_Table[i][j][1] = Total_T.ToString();
                            F_Table[i][j][2] = Total_G.ToString();
                            F_Table[i][j][3] = Total_NG.ToString();
                            F_Table[i][j][4] = Arrr[0, 6];

                            // 리스트에 보여주기
                            LvResult.Items.Add(new ListViewBind
                            {
                                N_Cam = Arrr[0, 0],
                                Total = Total_T,
                                Good = Total_G,
                                Not_Good = Total_NG,
                                Function = Arrr[0, 6]
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
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
            /*get number of columns in table*/
            /*only need to read one row*/
            string sqlstr = "SELECT * FROM " + tablename + " WHERE Start BETWEEN ( '" + strTotalStartTime + "') And ( '" + strTotalEndTime + "')";
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

        /// <summary>
        /// 카메라 수 만큼 패널을 만들기 위함
        /// </summary>
        public void Init_Pannel()
        {
            Sub_Pannel[] cSub_Pannel = new Sub_Pannel[Define.MAX_CAMERA];
            int iBlockCount = LvResult.Items.Count;

            for (int i = 0; i < Define.MAX_CAMERA; i++)
            {
                cSub_Pannel[i] = new Sub_Pannel();
                if (LvResult != null)
                {
                    cSub_Pannel[i].Init((uint)i, iBlockCount);
                }
                // 각 패널의 이름을 지정해주고, 지정된 패널에 붙혀넣기
                string name_i = "Cam_" + i.ToString();
                var vContentControl = FindName(name_i) as ContentControl;
                if (vContentControl != null) vContentControl.Content = cSub_Pannel[i];
            }
        }
    }
}