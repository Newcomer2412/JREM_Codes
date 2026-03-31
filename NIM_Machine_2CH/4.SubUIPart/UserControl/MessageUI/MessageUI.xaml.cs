using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// CMessageUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMessageUI : Window
    {
        public string m_strInfo { get; set; } = string.Empty;
        public int m_showOption { get; set; } = 0;
        public bool m_userButtonUsed { get; set; } = false;
        public string m_userButtonString { get; set; } = string.Empty;
        public int m_selectButton { get; set; } = 0;
        public bool m_bAutoCloseWndFlag { get; set; } = false;
        public bool m_bAutoCloseWndAction { get; set; } = false;

        private string[] m_buttonMessage = new string[3];

        /// <summary>
        /// 타이머
        /// </summary>
        private DispatcherTimer ctimer = new DispatcherTimer();

        /// <summary>
        /// 생성자
        /// </summary>
        public CMessageUI()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 창 열림
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TBText.Text = m_strInfo;

            if (m_userButtonUsed == false)
            {
                if (m_showOption == 0)
                {
                    m_buttonMessage[0] = "";
                    m_buttonMessage[1] = "OK";
                    m_buttonMessage[2] = "";
                }
                else if (m_showOption == 1)
                {
                    m_buttonMessage[0] = "Yes";
                    m_buttonMessage[1] = "";
                    m_buttonMessage[2] = "No";
                }
                else if (m_showOption == 2)
                {
                    m_buttonMessage[0] = "OK";
                    m_buttonMessage[1] = "";
                    m_buttonMessage[2] = "Cancel";
                }
                else if (m_showOption == 3)
                {
                    m_buttonMessage[0] = "OK";
                    m_buttonMessage[1] = "Cencel";
                    m_buttonMessage[2] = "Ignore";
                }
                else if (m_showOption == 4)
                {
                    m_buttonMessage[0] = "Recheck";
                    m_buttonMessage[1] = "";
                    m_buttonMessage[2] = "Ignore";
                }
            }
            else
            {
                // 사용자 입력 일때는 무조건 3개로
                m_showOption = 3;
                // 여기서 입력 갯수가 잘못 될 경우 Error 가 날 수 있다.
                string[] strData = m_userButtonString.Split(',');

                if (strData.Length == 3)
                {
                    m_buttonMessage[0] = strData[0];
                    m_buttonMessage[1] = strData[1];
                    m_buttonMessage[2] = strData[2];
                }
                else
                    CCommon.ShowMessageMini("MessageUI 사용자 입력이 잘못 되었습니다.");
            }

            BT1.Content = m_buttonMessage[0];
            BT2.Content = m_buttonMessage[1];
            BT3.Content = m_buttonMessage[2];

            if (m_showOption == 0)
            {
                TBCaption.Text = "Information";
                BT1.Visibility = Visibility.Hidden;
                BT2.Visibility = Visibility.Visible;
                BT3.Visibility = Visibility.Hidden;
                BitmapImage bitmapImage = new BitmapImage(new Uri("/ImagePart/info.png", UriKind.RelativeOrAbsolute));
                ImageBox.Source = bitmapImage;
            }
            else if (m_showOption == 1 || m_showOption == 2 || m_showOption == 4)
            {
                TBCaption.Text = "Question";
                BT1.Visibility = Visibility.Visible;
                BT2.Visibility = Visibility.Hidden;
                BT3.Visibility = Visibility.Visible;
                BitmapImage bitmapImage = new BitmapImage(new Uri("/ImagePart/Question.png", UriKind.RelativeOrAbsolute));
                ImageBox.Source = bitmapImage;
            }
            else if (m_showOption == 3)
            {
                TBCaption.Text = "Question";
                BT1.Visibility = Visibility.Visible;
                BT2.Visibility = Visibility.Visible;
                BT3.Visibility = Visibility.Visible;
                BitmapImage bitmapImage = new BitmapImage(new Uri("/ImagePart/Question.png", UriKind.RelativeOrAbsolute));
                ImageBox.Source = bitmapImage;
            }

            if (m_bAutoCloseWndFlag == true)
            {
                m_bAutoCloseWndAction = false;
                ctimer.Interval = TimeSpan.FromMilliseconds(100);     // 시간 간격 설정
                ctimer.Tick += new EventHandler(Timer_Tick);          // 이벤트 추가
                ctimer.Start();                                       // 타이머 시작
            }
        }

        /// <summary>
        /// 데이터 갱신용 반복 타이머
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            //Dispatcher.Invoke(DispatcherPriority.Background, (Action)delegate ()
            //{
            if (m_bAutoCloseWndAction == true)
            {
                m_bAutoCloseWndAction = false;
                Close();
            }
            //});
        }

        /// <summary>
        /// 1번 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BT1_Click(object sender, RoutedEventArgs e)
        {
            if (m_showOption == 1 || m_showOption == 2 || m_showOption == 3)
            {
                m_selectButton = 0;
            }
            Close();
        }

        /// <summary>
        /// 2번 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BT2_Click(object sender, RoutedEventArgs e)
        {
            if (m_showOption == 3) m_selectButton = 1;
            else m_selectButton = 0;   // 0 이면 ok
            Close();
        }

        /// <summary>
        /// 3번 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BT3_Click(object sender, RoutedEventArgs e)
        {
            if (m_showOption == 1 || m_showOption == 2 || m_showOption == 4) m_selectButton = 1;
            else if (m_showOption == 3) m_selectButton = 2;
            Close();
        }

        /// <summary>
        /// 창 닫힘
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (ctimer.IsEnabled)
            {
                ctimer.Stop();
            }
        }
    }
}