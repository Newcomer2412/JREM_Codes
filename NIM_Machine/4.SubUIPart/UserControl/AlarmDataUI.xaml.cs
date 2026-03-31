using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace MachineControlBase
{
    /// <summary>
    /// Alarm Data
    /// </summary>
    public class CAlarmData
    {
        public CAlarmData()
        {
        }

        /// <summary>
        /// 안쓰는 알람 번호 자동 삭제를 위한 플래그
        /// </summary>
        [XmlIgnore]
        public bool bUse = false;

        /// <summary>
        /// Alarm Number
        /// </summary>
        public int iNo = 0;

        /// <summary>
        /// Alarm Image Path
        /// </summary>
        public string strImagePath = string.Empty;

        /// <summary>
        /// Alarm Action KOR
        /// </summary>
        public string strAction_KOR = string.Empty;

        /// <summary>
        /// Alarm Action ENG
        /// </summary>
        public string strAction_ENG = string.Empty;

        /// <summary>
        /// Alarm Action CHN
        /// </summary>
        public string strAction_CHN = string.Empty;
    }

    /// <summary>
    /// AlarmDataUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AlarmDataUI : Window, IDisposable
    {
        private bool disposed;

        ~AlarmDataUI()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed) return;
            if (disposing)
            {
                // IDisposable 인터페이스를 구현하는 멤버들을 여기서 정리
            }
            // .NET Framework에 의하여 관리되지 않는 외부 리소스들을 여기서 정리
            this.disposed = true;
        }

        /// <summary>
        /// 선택된 알람 데이터
        /// </summary>
        private CAlarmData cAlarmData = null;

        public AlarmDataUI()
        {
            InitializeComponent();
            SetListBoxData();
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
        /// ListBox Data를 갱신한다.
        /// </summary>
        private void SetListBoxData()
        {
            LbAlarmData.Items.Clear();
            string strAddItem = string.Empty;

            var vErrorCodeArrayString = Enum.GetNames(typeof(eErrorCode));
            for (int i = 0; i < Enum.GetValues(typeof(eErrorCode)).Length - 1; i++)
            {
                int iCodeNo = (int)Enum.Parse(typeof(eErrorCode), vErrorCodeArrayString[i]);
                strAddItem = string.Format("[ {0:00000} ] {1}", iCodeNo, vErrorCodeArrayString[i]);
                LbAlarmData.Items.Add(strAddItem);
                // 알람 데이터가 없으면 생성
                CAlarmData cData = CMainLib.Ins.cSysOne.ListAlarmData.Find(x => x.iNo == iCodeNo);
                if (cData == null)
                {
                    cData = new CAlarmData();
                    cData.iNo = iCodeNo;
                    cData.strImagePath = "Default.png";
                    cData.strAction_KOR = " ";
                    cData.strAction_ENG = " ";
                    cData.strAction_CHN = " ";
                    CMainLib.Ins.cSysOne.ListAlarmData.Add(cData);
                }
                cData.bUse = true;
            }
            // 알람 데이터가 사용되지 않았으면 삭제
            List<CAlarmData> ListDeleteAlarmData = CMainLib.Ins.cSysOne.ListAlarmData.FindAll(x => x.bUse == false);
            foreach (CAlarmData alarm in ListDeleteAlarmData)
            {
                CMainLib.Ins.cSysOne.ListAlarmData.Remove(alarm);
            }
            // 번호 순서대로 정렬
            CMainLib.Ins.cSysOne.ListAlarmData = CMainLib.Ins.cSysOne.ListAlarmData.OrderBy(x => x.iNo).ToList();
        }

        /// <summary>
        /// Image Delete 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtImageDelete_Click(object sender, RoutedEventArgs e)
        {
            if (cAlarmData != null)
            {
                cAlarmData.strImagePath = string.Empty;
            }
        }

        /// <summary>
        /// Image Set 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtImageSet_Click(object sender, RoutedEventArgs e)
        {
            if (cAlarmData != null)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image files (*.png;*.jpg)|*.png;*.jpg|All files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == true)
                {
                    cAlarmData.strImagePath = TbImagePath.Text = openFileDialog.FileName.Substring(openFileDialog.FileName.LastIndexOf("\\") + 1); ;
                }
            }
        }

        /// <summary>
        /// 알람 수정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtAlaramSet_Click(object sender, RoutedEventArgs e)
        {
            if (cAlarmData != null)
            {
                cAlarmData.strImagePath = TbImagePath.Text;
                cAlarmData.strAction_KOR = TbAction_Kor.Text;
                cAlarmData.strAction_ENG = TbAction_Eng.Text;
                cAlarmData.strAction_CHN = TbAction_Chn.Text;
            }
            else
            {
                CCommon.ShowMessageMini("수정할 알람 List를 선택하세요.");
                return;
            }
        }

        /// <summary>
        /// ListBox Select
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LbAlarmData_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            object selecteditem = LbAlarmData.SelectedItem;   // 반환형 object

            if (selecteditem != null)
            {
                string[] sp = selecteditem.ToString().Split(']');
                sp[0] = sp[0].Replace("[", "");
                sp[0] = sp[0].Replace(" ", "");
                int iNo = 0;
                if (int.TryParse(sp[0], out iNo))
                {
                    cAlarmData = CMainLib.Ins.cSysOne.ListAlarmData.Find(x => x.iNo == iNo);
                    TbCodeNo.Text = string.Format("{0:00000}", iNo);
                    TbImagePath.Text = cAlarmData.strImagePath;
                    TbAction_Kor.Text = cAlarmData.strAction_KOR;
                    TbAction_Eng.Text = cAlarmData.strAction_ENG;
                    TbAction_Chn.Text = cAlarmData.strAction_CHN;
                }
            }
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