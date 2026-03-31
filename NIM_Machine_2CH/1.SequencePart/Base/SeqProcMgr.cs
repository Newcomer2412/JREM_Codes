using System.Collections.Generic;
using System.Diagnostics;

namespace MachineControlBase
{
    /// <summary>
    /// 시퀀스 종류 정의
    /// </summary>
    public enum eSequence
    {
        Seq00_MPC_Rail1,
        Seq01_MPC_Rail2_1,
        Seq02_MPC_Rail2_2,
        Seq03_Holder_SnR,
        Seq04_Hopper,
        Seq05_Pipe_PnP,
        Seq06_Needle_PnP,
        Seq07_Transfer1,
        Seq07_Transfer2,
        Seq08_Pipe_Mount,
        Seq09_Needle_Mount,
        Seq10_Dispensing,
    }

    /// <summary>
    /// 모든 시퀀스 관리 클래스
    /// </summary>
    public class SeqProcMgr
    {
        /// <summary>
        /// 장비 초기화 및 홈 동작
        /// </summary>
        public Seq_Initilize SeqInitilize;

        public Seq_IO SeqIO;
        public Seq_Monitoring SeqMonitoring;

        /// <summary>
        /// 제어 시퀀스 관리 Dictionary
        /// </summary>
        private Dictionary<eSequence, ISequence> worker;

        public Dictionary<eSequence, ISequence> Worker
        {
            get { return worker; }
            set { worker = value; }
        }

        /// <summary>
        /// 생성자 (이곳에서 각 구동 시퀀스를 정의 한다)
        /// </summary>
        public SeqProcMgr()
        {
            // For Real Time
            Process.GetCurrentProcess().PriorityBoostEnabled = true;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

            SeqInitilize = new Seq_Initilize();

            SeqIO = new Seq_IO();
            SeqIO.Init();
            SeqIO.Start();

            SeqMonitoring = new Seq_Monitoring();
            SeqMonitoring.Init();
            SeqMonitoring.Start();

            Worker = new Dictionary<eSequence, ISequence>();

            Worker.Add(eSequence.Seq00_MPC_Rail1, new Seq00_Front_MPC(eSequence.Seq00_MPC_Rail1));
            Worker.Add(eSequence.Seq01_MPC_Rail2_1, new Seq01_Rear_MPC_Left(eSequence.Seq01_MPC_Rail2_1));
            Worker.Add(eSequence.Seq02_MPC_Rail2_2, new Seq02_Rear_MPC_Right(eSequence.Seq02_MPC_Rail2_2));
            Worker.Add(eSequence.Seq03_Holder_SnR, new Seq03_HolderSnR(eSequence.Seq03_Holder_SnR));
            Worker.Add(eSequence.Seq04_Hopper, new Seq04_Hopper(eSequence.Seq04_Hopper));
            Worker.Add(eSequence.Seq05_Pipe_PnP, new Seq05_PipePnP(eSequence.Seq05_Pipe_PnP));
            Worker.Add(eSequence.Seq06_Needle_PnP, new Seq06_NeedlePnP(eSequence.Seq06_Needle_PnP));
            ISequence Seq07_Transfer1 = new Seq07_Transfer(eSequence.Seq07_Transfer1, eLine.Left);
            ISequence Seq07_Transfer2 = new Seq07_Transfer(eSequence.Seq07_Transfer2, eLine.Right);
            Worker.Add(eSequence.Seq07_Transfer1, Seq07_Transfer1);
            Worker.Add(eSequence.Seq07_Transfer2, Seq07_Transfer2);
            Worker.Add(eSequence.Seq08_Pipe_Mount, new Seq08_PipeMount(eSequence.Seq08_Pipe_Mount));
            Worker.Add(eSequence.Seq09_Needle_Mount, new Seq09_NeedleMount(eSequence.Seq09_Needle_Mount));
            Worker.Add(eSequence.Seq10_Dispensing, new Seq10_Dispensing(eSequence.Seq10_Dispensing));
        }

        /// <summary>
        /// 클래스 소멸
        /// </summary>
        public void Free()
        {
            SeqIO.Stop();
            SeqIO.Free();

            SeqMonitoring.Stop();
            SeqMonitoring.Free();

            if (Worker != null)
            {
                foreach (KeyValuePair<eSequence, ISequence> item in Worker)
                {
                    ((ISequence)item.Value.Ins).Stop();
                    ((ISequence)item.Value.Ins).Free();
                }
                Worker.Clear();
                Worker = null;
            }
        }

        /// <summary>
        /// 각 시퀀스 초기화
        /// </summary>
        public void Init()
        {
            foreach (KeyValuePair<eSequence, ISequence> item in Worker)
            {
                ((ISequence)item.Value.Ins).Init();
            }
        }

        /// <summary>
        /// 각 시퀀스 시작
        /// </summary>
        public void Start()
        {
            foreach (KeyValuePair<eSequence, ISequence> item in Worker)
            {
                ((ISequence)item.Value.Ins).ClearSequence();
                ((ISequence)item.Value.Ins).cSwMainTimeOut.Restart();
                ((ISequence)item.Value.Ins).Start();
            }
        }

        /// <summary>
        /// 각 시퀀스 매뉴얼 시작
        /// </summary>
        public void ManualStart(eSequence SeqType)
        {
            foreach (KeyValuePair<eSequence, ISequence> item in Worker)
            {
                if (item.Key == SeqType)
                {
                    ((ISequence)item.Value.Ins).ClearSequence();
                    ((ISequence)item.Value.Ins).ManualStart();
                }
            }
        }

        /// <summary>
        /// 각 시퀀스 정지
        /// </summary>
        public void Stop()
        {
            foreach (KeyValuePair<eSequence, ISequence> item in Worker)
            {
                ((ISequence)item.Value.Ins).Stop();
            }
        }

        /// <summary>
        /// 각 시퀀스 긴급 정지
        /// </summary>
        public void EMGStop()
        {
            foreach (KeyValuePair<eSequence, ISequence> item in Worker)
            {
                ((ISequence)item.Value.Ins).EMGStop();
            }
        }

        /// <summary>
        /// 각 시퀀스 일시 정지
        /// </summary>
        public void Pause()
        {
            foreach (KeyValuePair<eSequence, ISequence> item in Worker)
            {
                ((ISequence)item.Value.Ins).Pause();
            }
        }

        /// <summary>
        /// 각 시퀀스 재시작
        /// </summary>
        public void Restart()
        {
            foreach (KeyValuePair<eSequence, ISequence> item in Worker)
            {
                item.Value.ReStart();
            }
        }

        /// <summary>
        /// MainTimerStop
        /// </summary>
        public void MainTimerStop()
        {
            foreach (KeyValuePair<eSequence, ISequence> item in Worker)
            {
                item.Value.MainTimerStop();
            }
        }

        /// <summary>
        /// 각 시퀀스 스텝 No 및 상태 초기화
        /// </summary>
        public void ClearSequence()
        {
            foreach (KeyValuePair<eSequence, ISequence> item in Worker)
            {
                ((ISequence)item.Value.Ins).ClearSequence();
            }
        }

        /// <summary>
        /// 시퀀스가 모두 정지했는지 확인
        /// </summary>
        /// <returns></returns>
        public bool GetAllStop()
        {
            foreach (KeyValuePair<eSequence, ISequence> item in Worker)
            {
                if (((ISequence)item.Value.Ins).Proc.Run == true) return false;
            }
            return true;
        }
    }
}