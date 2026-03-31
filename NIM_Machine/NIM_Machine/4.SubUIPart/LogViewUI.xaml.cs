using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// LogViewUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LogViewUI : UserControl
    {
        /// <summary>
        /// 로그 폴더 주소
        /// </summary>
        private string strFolderPath = string.Empty;

        /// <summary>
        /// 로그 파일 경로 주소
        /// </summary>
        private string strCurrentFilePath = string.Empty;

        /// <summary>
        /// 로그 종류
        /// </summary>
        private eLogType eLogType = 0;

        /// <summary>
        /// 생성자
        /// </summary>
        public LogViewUI()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Task UI 접근을 위한 파라미터 정의
        /// </summary>
        private TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

        /// <summary>
        /// 초기화 함수
        /// </summary>
        /// <param name="eLogType"></param>
        public void Init(eLogType eLogType)
        {
            this.eLogType = eLogType;
            StartTitleText(eLogType);

            strFolderPath = CXMLProcess.LogPath + eLogType.ToString();
            strCurrentFilePath = CXMLProcess.LogPath + eLogType.ToString() + @"\" + "Log.log";

            if (CXMLProcess.FileFind(strCurrentFilePath) == true)
            {
                Task.Factory.StartNew(() =>
                {
                    // 버튼 컨트롤들 비활성화
                    EnableControl(false);
                    // Log File을 불러와 String 변수에 저장
                    string strData = GetFileLogDataToString();
                    RTbLogView.Document.Blocks.Clear();
                    RTbLogView.Document.Blocks.Add(new Paragraph(new Run(strData)));
                    RTbLogView.Dispatcher.Invoke((Action)(() => { RTbLogView.ScrollToEnd(); }), DispatcherPriority.ApplicationIdle);
                    EndTitleText(eLogType);
                    EnableControl(true);
                }, CancellationToken.None, TaskCreationOptions.None, uiScheduler);
            }
            else
            {
                TBLogTitle.Text = "File does not exist.";
            }
        }

        /// <summary>
        /// Log File을 불러와 String에 저장.
        /// </summary>
        /// <returns></returns>
        public string GetFileLogDataToString()
        {
            string strReturnString = "";

            try
            {
                // 문서 파일을 string 변수에 읽어옴
                StringBuilder sb = new StringBuilder();
                foreach (int progress in LoadFileWithProgress(strCurrentFilePath, sb))
                {
                    // Progress를 추가할 경우 여기에 추가
                }
                strReturnString = sb.ToString();
            }
            catch
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.FATAL, "GetFileLogDataToString() Exception 에러 \n{0}");
                return null;
            }

            return strReturnString;
        }

        /// <summary>
        /// 텍스트 파일을 빨리 읽기 위한 버퍼 기반 바이너리 Read 함수
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="stringData"></param>
        /// <returns></returns>
        public static IEnumerable<int> LoadFileWithProgress(string filename, StringBuilder stringData)
        {
            const int charBufferSize = 1024;
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (BinaryReader br = new BinaryReader(fs, Encoding.UTF8))
                {
                    long length = fs.Length;
                    int numberOfChunks = Convert.ToInt32((length / charBufferSize)) + 1;
                    double iter = 100 / Convert.ToDouble(numberOfChunks);
                    double currentIter = 0;
                    yield return Convert.ToInt32(currentIter);
                    while (true)
                    {
                        char[] buffer = br.ReadChars(charBufferSize);
                        if (buffer.Length == 0) break;
                        stringData.Append(buffer);
                        currentIter += iter;
                        yield return Convert.ToInt32(currentIter);
                    }
                }
            }
        }

        /// <summary>
        /// 컨트롤 활성화 제어
        /// </summary>
        /// <param name="bEnable"></param>
        private void EnableControl(bool bEnable)
        {
            if (bEnable == true)
            {
                BtFileOpen.IsEnabled = true;
                BtFileSave.IsEnabled = true;
                BtReload.IsEnabled = true;
            }
            else
            {
                BtFileOpen.IsEnabled = false;
                BtFileSave.IsEnabled = false;
                BtReload.IsEnabled = false;
            }
        }

        /// <summary>
        /// Log 파일로 Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileOpen_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Filter = "로그파일|*.log|모든파일|*.*";

            // 로그 파일을 열 경우 최종 로그가 저장 된 곳으로 경로를 설정
            fileDialog.InitialDirectory = strFolderPath;

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StartTitleText(eLogType);
                // 현재 Read 할 Log 경로 설정
                strCurrentFilePath = fileDialog.FileName;

                Task.Factory.StartNew(() =>
                {
                    // 버튼 컨트롤들 비활성화
                    EnableControl(false);
                    // Log File을 불러와 String 변수에 저장
                    string strData = GetFileLogDataToString();
                    RTbLogView.Document.Blocks.Clear();
                    RTbLogView.Document.Blocks.Add(new Paragraph(new Run(strData)));
                    RTbLogView.Dispatcher.Invoke((Action)(() => { RTbLogView.ScrollToEnd(); }), DispatcherPriority.ApplicationIdle);
                    EndTitleText(eLogType);
                    EnableControl(true);
                }, CancellationToken.None, TaskCreationOptions.None, uiScheduler);
            }
            EndTitleText(eLogType);
            EnableControl(true);
        }

        /// <summary>
        /// Log 파일 저장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileSave_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            FlowDocument flowDocument = RTbLogView.Document;
            TextRange textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);

            //RichTextBox의 Log를 설정한 txt 파일로 저장
            System.Windows.Forms.SaveFileDialog fileDialog = new System.Windows.Forms.SaveFileDialog();
            fileDialog.Filter = "로그파일|*.txt";
            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Stream stream = null;
                try
                {
                    stream = new FileStream(fileDialog.FileName, FileMode.Create);
                    textRange.Save(stream, DataFormats.Text);
                }
                catch
                {
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Log 다시 Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Reload_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StartTitleText(eLogType);

            if (CXMLProcess.FileFind(strCurrentFilePath) == true)
            {
                Task.Factory.StartNew(() =>
                {
                    // 버튼 컨트롤들 비활성화
                    EnableControl(false);
                    // Log File을 불러와 String 변수에 저장
                    string strData = GetFileLogDataToString();
                    RTbLogView.Document.Blocks.Clear();
                    RTbLogView.Document.Blocks.Add(new Paragraph(new Run(strData)));
                    RTbLogView.Dispatcher.Invoke((Action)(() => { RTbLogView.ScrollToEnd(); }), DispatcherPriority.ApplicationIdle);
                    EndTitleText(eLogType);
                    EnableControl(true);
                }, CancellationToken.None, TaskCreationOptions.None, uiScheduler);
            }
            else
            {
                TBLogTitle.Text = "File does not exist.";
            }
            EndTitleText(eLogType);
            EnableControl(true);
        }

        /// <summary>
        /// 시작 타이틀 문자
        /// </summary>
        /// <param name="eLogType"></param>
        private void StartTitleText(eLogType eLogType)
        {
            TBLogTitle.Text = eLogType.ToString() + " Log View (Log Loading...)";
        }

        /// <summary>
        /// 끝 타이틀 문자
        /// </summary>
        /// <param name="eLogType"></param>
        private void EndTitleText(eLogType eLogType)
        {
            TBLogTitle.Text = eLogType.ToString() + " Log View";
        }
    }
}