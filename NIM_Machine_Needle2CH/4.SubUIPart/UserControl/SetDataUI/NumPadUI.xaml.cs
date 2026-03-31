using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MachineControlBase
{
    /// <summary>
    /// NumPadUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NumPadUI : Window
    {
        /// <summary>
        /// Set 기능으로 좌표를 가져올 때 사용된다.
        /// </summary>
        private eMotor eAxis = eMotor.MOT_NONE;

        /// <summary>
        /// 데이터를 설정할 TextBox 클래스
        /// </summary>
        private TextBlock cTaxtBlock;

        public string strGetNamee = string.Empty;

        /// <summary>
        /// 창이 열렸는지 확인하는 변수
        /// </summary>
        private static bool _bOpened = false;

        public bool bOpened
        {
            get { return _bOpened; }
            set
            {
                _bOpened = value;
            }
        }

        /// <summary>
        /// 창이 캔슬 관련
        /// </summary>
        private static bool _bCancel = false;

        public bool bCancel
        {
            get { return _bCancel; }
            set
            {
                _bCancel = value;
            }
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public NumPadUI()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="cTaxtBlock"></param>
        /// <param name="iAxisNo"></param>
        public void Init(TextBlock cTaxtBlock, eMotor eAxis = eMotor.MOT_NONE)
        {
            bOpened = true;
            bCancel = false;
            this.cTaxtBlock = cTaxtBlock;
            this.eAxis = eAxis;
            try
            {
                TBInputData.Text = cTaxtBlock.Text;
                TBOldInputData.Text = cTaxtBlock.Text;
            }
            catch { }
        }

        /// <summary>
        /// 창을 닫음
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            bOpened = false;
            bCancel = true;
            Close();
        }

        /// <summary>
        /// 입력 데이터 삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            TBInputData.Text = string.Empty;
        }

        /// <summary>
        /// 문자 하나 삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (TBInputData.Text == string.Empty) return;
            TBInputData.Text = TBInputData.Text.Remove(TBInputData.Text.Length - 1);
        }

        /// <summary>
        /// Data 입력
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetData_Click(object sender, RoutedEventArgs e)
        {
            if (TBInputData.Text == string.Empty) return;
            double dVelue;
            if (double.TryParse(TBInputData.Text.ToString(), out dVelue) == false) return;
            cTaxtBlock.Text = TBInputData.Text;
            bOpened = false;
            Close();
        }

        /// <summary>
        /// 숫자키 클릭 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Num_Click(object sender, RoutedEventArgs e)
        {
            string strInputNum = ((sender as Button).Content as string).ToString();

            if (strInputNum == "±")    // 부호를 변경
            {
                if (TBInputData.Text.Contains("-") == true)
                {
                    TBInputData.Text = TBInputData.Text.Replace("-", string.Empty);
                }
                else
                {
                    TBInputData.Text = "-" + TBInputData.Text;
                }
            }
            else if (strInputNum == "PSet")  // 축의 현재 위치를 가져와 설정
            {
                if (Define.SIMULATION == true) return;
                if (eAxis != eMotor.MOT_NONE)
                {
                    double dPos = CMainLib.Ins.Axis[eAxis].GetCmdPostion();
                    TBInputData.Text = dPos.ToString();
                }
            }
            else
            {
                if (strInputNum == ".") // 소수점 입력
                {
                    if (TBInputData.Text.Contains(".") == true)
                    {
                        return;
                    }
                }
                TBInputData.Text += strInputNum;
            }
        }

        /// <summary>
        /// 창을 Drag 하기 위한 기능 간단한 함수가 있지만 그 함수를 쓰면 프로그램 먹통 증상이
        /// 있어 기능을 풀어놓았다.
        /// </summary>
        private System.Drawing.Point _windowMoveMouseStart;

        private double _windowMoveStartTop;
        private double _windowMoveStartLeft;

        private void InputData_MouseDown(object sender, MouseButtonEventArgs e)
        {
            UIElement handle = sender as UIElement;
            if (handle == null)
                return;

            _windowMoveMouseStart = System.Windows.Forms.Control.MousePosition;
            _windowMoveStartTop = this.Top;
            _windowMoveStartLeft = this.Left;

            handle.MouseMove += InputData_MouseMove;
            handle.CaptureMouse();
        }

        private void InputData_MouseMove(object sender, MouseEventArgs e)
        {
            UIElement handle = sender as UIElement;
            if (handle == null)
                return;

            if (e.LeftButton == MouseButtonState.Released)
            {
                handle.MouseMove -= InputData_MouseMove; // Detach listener on mouse up.
                handle.ReleaseMouseCapture();
            }
            else
            {
                var smp = System.Windows.Forms.Control.MousePosition;
                var distanceX = smp.X - _windowMoveMouseStart.X;
                var distanceY = smp.Y - _windowMoveMouseStart.Y;

                this.Left = _windowMoveStartLeft + distanceX;
                this.Top = _windowMoveStartTop + distanceY;
            }
        }

        /// <summary>
        /// Key Down 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 한글이면 키입력 막음
            if (e.Key != Key.ImeProcessed)
            {
                e.Handled = true;
                return;
            }
            if (((e.Key >= Key.D0 && e.Key <= Key.D9) ||
                 (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||
                  e.Key == Key.Back || e.Key == Key.Decimal ||
                  e.Key == Key.OemPeriod || e.Key == Key.Enter ||
                  e.Key == Key.Left || e.Key == Key.Right ||
                  e.Key == Key.Subtract || e.Key == Key.Add ||
                  e.Key == Key.Escape) == false)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape)
            {
                bOpened = false;
                this.Close();
            }
            else if (e.Key == Key.Back && TBInputData.IsFocused == false)
            {
                if (TBInputData.Text == string.Empty) return;
                TBInputData.Text = TBInputData.Text.Remove(TBInputData.Text.Length - 1);
            }
            else if (e.Key == Key.Subtract)    // 음수 부호 변경
            {
                if (TBInputData.Text.Contains("-") == false)
                {
                    TBInputData.Text = "-" + TBInputData.Text;
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Add)    // 양수 부호 변경
            {
                if (TBInputData.Text.Contains("-") == true)
                {
                    TBInputData.Text = TBInputData.Text.Replace("-", string.Empty);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                BtSet.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                e.Handled = true;
            }
            else if (TBInputData.IsFocused == false)
            {
                KeyConverter kc = new KeyConverter();
                var str = KeyEventUtility.GetCharFromKey(e.Key);
                TBInputData.Text += str;
            }
        }
    }

    /// <summary>
    /// 키보드 입력 e.Key를 문자로 변환
    /// </summary>
    public static class KeyEventUtility
    {
        // ReSharper disable InconsistentNaming
        public enum MapType : uint
        {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3,
        }

        // ReSharper restore InconsistentNaming

        [DllImport("user32.dll")]
        public static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs( UnmanagedType.LPWStr, SizeParamIndex = 4 )]
        StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, MapType uMapType);

        public static char GetCharFromKey(Key key)
        {
            char ch = ' ';

            int virtualKey = KeyInterop.VirtualKeyFromKey(key);
            var keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            uint scanCode = MapVirtualKey((uint)virtualKey, MapType.MAPVK_VK_TO_VSC);
            var stringBuilder = new StringBuilder(2);

            int result = ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch (result)
            {
                case -1:
                    break;

                case 0:
                    break;

                case 1:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
                default:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
            }
            return ch;
        }
    }
}