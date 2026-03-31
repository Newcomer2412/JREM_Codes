using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace MachineControlBase
{
    /// <summary>
    /// Program Version 저장
    /// </summary>
    public static class ProgramVersion
    {
        public static string Version = string.Empty;
        public static string VersionInformation = string.Empty;

        public static void LoadVersionDocument()
        {
            var uri = new Uri("pack://application:,,,/VersionInformation.txt");
            var resourceStream = Application.GetResourceStream(uri);

            using (var reader = new StreamReader(resourceStream.Stream))
            {
                // Version 정보를 읽고 마지막 Version No를 추출하여 저장
                VersionInformation = reader.ReadToEnd();
                int ifirstPos = VersionInformation.IndexOf("Ver");
                int iEndPos = VersionInformation.IndexOf("\r\n");
                Version = (VersionInformation.Substring(ifirstPos, iEndPos)).Trim();
            }
        }
    }

    /// <summary>
    /// CVersionUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CVersionUI : Window
    {
        public CVersionUI()
        {
            InitializeComponent();
            RTbVersionInfo.AppendText(ProgramVersion.VersionInformation);
        }

        /// <summary>
        /// Close 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 창을 Drag 하기 위한 간단한 함수가 있지만 그 함수를 쓰면 프로그램 먹통 증상이
        /// 있어 기능을 풀어놓았다. MS 자체 버그가 있는 걸로 검색된다.
        /// </summary>
        private System.Drawing.Point _windowMoveMouseStart;

        private double _windowMoveStartTop;
        private double _windowMoveStartLeft;

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UIElement handle = sender as UIElement;
            if (handle == null)
                return;

            _windowMoveMouseStart = System.Windows.Forms.Control.MousePosition;
            _windowMoveStartTop = this.Top;
            _windowMoveStartLeft = this.Left;

            handle.MouseMove += Border_MouseMove;
            handle.CaptureMouse();
        }

        private void Border_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            UIElement handle = sender as UIElement;
            if (handle == null)
                return;

            if (e.LeftButton == MouseButtonState.Released)
            {
                handle.MouseMove -= Border_MouseMove; // Detach listener on mouse up.
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
    }
}