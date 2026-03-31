using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// 각 시퀀스에 명령을 내리기 위한 정의
    /// </summary>
    public enum InputFlag
    {
        NONE,
        RUN,
        STOP,
    }

    /// <summary>
    /// 각 시퀀스에 상태를 보기 위한 정의
    /// </summary>
    public enum OutputFlag
    {
        NONE,
        READY,
        BUSY,
        ERROR,
    }

    /// <summary>
    /// 시퀀스 번호 및 Comm 시퀀스를 관리하기 위한 인터페이스
    /// </summary>
    public interface ISeqNo
    {
        object Ins { get; set; }
        int iStep { get; set; }
        int iPreStep { get; set; }
        int iSubStep { get; set; }
        int iPreSubStep { get; set; }
        string SeqName { get; set; }

        void Init();

        void Free();

        void MainTimerStop();

        bool Do();
    }

    /// <summary>
    /// Dictionary 구조를 사용하기 위한 인터페이스
    /// </summary>
    public interface ISequence
    {
        object Ins { get; set; }
        CSeqProc Proc { get; set; }
        Stopwatch cSwMainTimeOut { get; set; }
        Dictionary<int, ISeqNo> SubWorker { get; set; }

        void Init();

        void Free();

        void Stop();

        void EMGStop();

        void Start();

        void ManualStart();

        void Pause();

        void ReStart();

        bool Do();

        bool ManualDo();

        void Next(int nStep);

        void ClearSequence();

        void MainTimerStop();
    }

    /// <summary>
    /// 시퀀스 스레드 제어를 위한 클래스
    /// </summary>
    public class CSeqProc
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public CSeqProc()
        {
        }

        /// <summary>
        /// 클래스 소멸
        /// </summary>
        public void Free()
        {
            Stop();
            cThread = null;
            procHandler = null;
        }

        /// <summary>
        /// 시퀀스 스레드
        /// </summary>
        private Thread cThread = null;

        public delegate bool RunCallBackMethod();

        /// <summary>
        /// 메인 시퀀스 스레드 Do 함수 연결자
        /// </summary>
        private RunCallBackMethod procHandler = null;

        /// <summary>
        /// Manual 시퀀스 스레드 Manual Do 함수 연결자
        /// </summary>
        private RunCallBackMethod procManualHandler = null;

        /// <summary>
        /// 시퀀스 이름
        /// </summary>
        private string seqName;

        public string SeqName
        {
            get { return seqName; }
            set { seqName = value; }
        }

        /// <summary>
        /// 시퀀스 스레드 실행 Main 플래그
        /// </summary>
        private bool bRunAlive = false;

        public bool RunAlive
        {
            get { return bRunAlive; }
            set { bRunAlive = value; }
        }

        /// <summary>
        /// 시퀀스 스레드 실행,정지 플래그
        /// </summary>
        private bool bRun = false;

        public bool Run
        {
            get { return bRun; }
            set { bRun = value; }
        }

        /// <summary>
        /// 일시 정지
        /// </summary>
        public void Pause()
        {
            if (RunAlive == true)
            {
                if (bRun == true)
                    bRun = false;
            }
        }

        /// <summary>
        /// 재시작
        /// </summary>
        public void Restart()
        {
            if (RunAlive == true)
            {
                if (bRun == false)
                    bRun = true;
            }
        }

        /// <summary>
        /// 자동 운전 : 각 시퀀스 스레드 생성, 시작 및 스래드 호출 함수 연결
        /// </summary>
        /// <param name="_procHandler"></param>
        /// <param name="threadPriority"></param>
        public void Start(RunCallBackMethod _procHandler, ThreadPriority threadPriority)
        {
            if (procHandler != null) procHandler = null;
            procHandler = _procHandler;

            if (cThread != null)
            {
                if (cThread.ThreadState == System.Threading.ThreadState.Running) cThread.Join();
                cThread = null;
            }

            if (CMainLib.Ins.McState == eMachineState.ERROR) return;

            bSeqStopCommand = false;
            RunAlive = true;
            Run = true;
            cThread = new Thread(new ThreadStart(Do))
            {
                Name = SeqName,
                Priority = threadPriority,
                IsBackground = true
            };
            cThread.Start();
        }

        /// <summary>
        /// 매뉴얼 운전 : 각 시퀀스 스레드 생성, 시작 및 스래드 호출 함수 연결
        /// </summary>
        /// <param name="_manualHandler"></param>
        /// <param name="threadPriority"></param>
        public void ManualStart(RunCallBackMethod _manualHandler, ThreadPriority threadPriority)
        {
            if (procManualHandler != null) procManualHandler = null;
            procManualHandler = _manualHandler;

            if (cThread != null)
            {
                if (cThread.ThreadState == System.Threading.ThreadState.Running) cThread.Join();
                cThread = null;
            }

            if (CMainLib.Ins.McState == eMachineState.ERROR) return;

            bSeqStopCommand = false;
            RunAlive = true;
            Run = true;
            cThread = new Thread(new ThreadStart(ManualDo))
            {
                Name = SeqName,
                Priority = threadPriority,
                IsBackground = true
            };
            cThread.Start();
        }

        /// <summary>
        /// 스래드 정지
        /// </summary>
        public void Stop()
        {
            if (bSeqStopCommand == false) bSeqStopCommand = true;
        }

        /// <summary>
        /// 시퀀스가 모두 완료되고 정지해야 하므로 플래그를 추가
        /// </summary>
        public static bool bSeqStopCommand = false;

        /// <summary>
        /// 스래드 긴급 정지
        /// </summary>
        public void EMGStop()
        {
            RunAlive = false;
            Run = false;
        }

        /// <summary>
        /// Main 구동 스레드
        /// </summary>
        private void Do()
        {
            while (RunAlive == true)
            {
                Thread.Sleep(1);
                if (Run == true)
                {
                    try
                    {
                        if (procHandler != null)
                        {
                            if (procHandler() == true &&
                                bSeqStopCommand == true)
                            {
                                RunAlive = false;
                                Run = false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        NLogger.AddLog(eLogType.SEQ_MAIN, NLogger.eLogLevel.FATAL, ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Manual 구동 스레드
        /// </summary>
        private void ManualDo()
        {
            while (RunAlive == true)
            {
                Thread.Sleep(1);
                if (Run == true)
                {
                    try
                    {
                        if (bSeqStopCommand == true)
                        {
                            RunAlive = false;
                            Run = false;
                        }
                        if (procManualHandler != null)
                        {
                            procManualHandler();
                        }
                    }
                    catch (Exception ex)
                    {
                        NLogger.AddLog(eLogType.SEQ_MAIN, NLogger.eLogLevel.FATAL, ex.ToString());
                    }
                }
            }
        }
    }
}