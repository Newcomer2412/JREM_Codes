using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 프로그램 중복 실행 방지
        /// </summary>
        private Mutex _mutex = null;

        private CMainLib ml = CMainLib.Ins;

        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
        private static extern uint TimeBeginPeriod(uint uMilliseconds);

        protected override void OnStartup(StartupEventArgs e)
        {
            string mutexName = "MachineControlBase";
            bool isCreatedNew = false;
            try
            {
                _mutex = new Mutex(true, mutexName, out isCreatedNew);

                if (isCreatedNew)
                {
                    base.OnStartup(e);

                    // 시간 측정 시 정확한 시간 측정을 위한 1ms 설정
                    TimeBeginPeriod(1);

                    // 초기 로딩 화면을 생성
                    var splashScreen = new StartLoadingUI();
                    MainWindow = splashScreen;
                    splashScreen.Show();

                    // UI가 반응 유지를 위한 다른 스레드 작업
                    Task.Factory.StartNew(() =>
                    {
                        // 완료되면 UI스레드에 있지 않으므로 Dispatcher를 사용하여 기본 창을 작성하고 표시
                        Dispatcher.Invoke(() =>
                        {
                            // 기본 창을 초기화하고 응용 프로그램 기본 창으로 설정, 스플래시 화면 종료
                            ml.Init();
                            var mainWindow = new MainWindow();
                            MainWindow = mainWindow;
                            mainWindow.Show();
                            splashScreen.Close();
                        });
                    });
                }
                else
                {
                    MessageBox.Show("Application already started.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace + "\n\n" + "Application Existing...", "Exception thrown");
                Current.Shutdown();
            }
        }

        /// <summary>
        /// 윈도우 종료 및 로그오프 시 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            // Data Save
            CXMLProcess.WriteXml(CXMLProcess.RunUnitDataFilePath, CMainLib.Ins.cRunUnitData);
            CXMLProcess.WriteXml(CXMLProcess.SystemParameterSingleFilePath, CMainLib.Ins.cSysOne);
            CXMLProcess.WriteXml(CXMLProcess.SystemParameterCollectionDataFilePath, CMainLib.Ins.cSysParamCollData);
            Thread.Sleep(5000);
        }

        public bool DoHandle { get; set; }

        /// <summary>
        /// 프로그램이 Exception에 걸렸을 경우
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            CXMLProcess.WriteXml(CXMLProcess.RunUnitDataFilePath, CMainLib.Ins.cRunUnitData);
            NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.FATAL, e.Exception.Message);

            if (this.DoHandle)  // UnhandledExcpeiton 핸들러 내에서 예외 처리
            {
                e.Handled = true;
            }
            else // e.Handled를 true로 설정하지 않으면 충돌로 인해 응용 프로그램이 닫힙니다.
            {
                e.Handled = false;
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        /// <summary>
        /// Thread 에서 Exception이 발생 했을 경우
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            CXMLProcess.WriteXml(CXMLProcess.RunUnitDataFilePath, CMainLib.Ins.cRunUnitData);
            Exception ex = e.ExceptionObject as Exception;
            NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.FATAL, ex.Message);
        }
    }
}