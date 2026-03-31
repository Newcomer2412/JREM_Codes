using System;
using System.Collections.Generic;
using NLog;

namespace MachineControlBase
{
    /// <summary>
    /// Log 관리 클래스
    /// </summary>
    public static class NLogger
    {
        public enum eLogLevel
        {
            INFO,
            ERROR,
            FATAL,
            DEBUG,
            TRACE,
            WARN,
        }

        public delegate void LogUpdateEventHandler(eLogType eType, eLogLevel eLevel, string strMsg);

        public static event LogUpdateEventHandler LogUpdateEvent;

        private static Dictionary<eLogType, Logger> log = new Dictionary<eLogType, Logger>();
        private static bool isInitailize;

        public static bool IsInitailize
        {
            get { return isInitailize; }
        }

        /// <summary>
        /// 로그를 초기화한다.
        /// </summary>
        public static void Init()
        {
            var Config = new NLog.Config.LoggingConfiguration();
            for (int i = 0; i < Enum.GetValues(typeof(eLogType)).Length; i++)
            {
                string strLogName = ((eLogType)i).ToString();
                var logFile = new NLog.Targets.FileTarget(strLogName)
                {
                    Name = strLogName,
                    Layout = "${longdate} [${level:uppercase=true}] ${message}",
                    FileNameKind = NLog.Targets.FilePathKind.Relative,
                    FileName = CXMLProcess.LogPath + strLogName + "\\Log.log",
                    KeepFileOpen = false,
                    CreateDirs = true,

                    // 아카이브에 사용할 파일의 이름입니다.
                    ArchiveFileName = CXMLProcess.LogPath + strLogName + "\\${shortdate}\\Log_{##}.log",//Log_{##}.log",
                    // 시작시 이전 로그 파일을 보관합니다. 기본값 : False
                    ArchiveOldFileOnStartup = false,
                    // 아카이브 파일을 zip 파일로 압축할지 여부를 나타냅니다. 부울 기본값 : False
                    EnableArchiveFileCompression = false,
                    // 파일이 아카이브 된 경우에만 바닥 글을 작성해야하는지 여부를 나타냅니다.
                    // False면 다른 파일에 쓰기를 시작할 때와 대상이 닫힐 때 바닥 글도 작성됩니다 . 부울 기본값 : False
                    WriteFooterOnArchivingOnly = false,
                    // 시작시 이전 로그 파일을 아카이브하기위한 파일 크기 임계 값입니다.
                    // 기본값은 0이며 이는 archiveOldFileOnStartup 이 활성화 되는 즉시 파일이 아카이브됨을 의미합니다.
                    //ArchiveOldFileOnStartupAboveSize = (long)Math.Pow(1024, 2) * 5,   // 5MB
                    // 파일 아카이브에 번호가 매겨집니다. 아카이브 번호 매기기 예제를 참조하십시오
                    ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.Sequence,
                    // 지정된 시간이 지날 때마다 자동으로 로그 파일을 보관할지 여부를 나타냅니다.
                    ArchiveEvery = NLog.Targets.FileArchivePeriod.Day,
                    // 로그 파일이 자동으로 아카이브 될 크기 (바이트)입니다.
                    ArchiveAboveSize = (long)Math.Pow(1024, 2) * 5,   // 5MB
                    // 보관 해야하는 최대 아카이브 파일 수입니다. 경우 maxArchiveFiles가 작거나 0으로 동일, 이전 파일은 삭제되지 않습니다 정수 기본값 : 0
                    MaxArchiveFiles = 100,
                    // 보관해야하는 아카이브 파일의 최대 수명. archiveNumbering 이 Rolling 경우 효과가 없습니다.
                    // maxArchiveDays가 작거나 0으로 동일, 이전 파일은 삭제되지 않습니다 정수 기본값 : 0
                    MaxArchiveDays = 30,
                };
                Config.AddRule(LogLevel.Trace, LogLevel.Fatal, logFile, strLogName);
            }

            LogManager.Configuration = Config;

            for (int i = 0; i < Enum.GetValues(typeof(eLogType)).Length; i++)
            {
                Logger logger = LogManager.GetLogger(((eLogType)i).ToString());
                log.Add((eLogType)i, logger);
            }

            isInitailize = true;
        }

        /// <summary>
        /// Log를 저장한다.
        /// </summary>
        /// <param name="eType"></param>
        /// <param name="eLevel"></param>
        /// <param name="strMsg"></param>
        /// <param name="strMsg"></param>
        public static void AddLog(eLogType eType, eLogLevel eLevel, string strMsg, bool bShowUI = true)
        {
            if (isInitailize == false)
                Init();

            switch (eLevel)
            {
                case eLogLevel.TRACE:
                    log[eType].Trace(strMsg);
                    if (eType > eLogType.SEQ_MAIN) log[eLogType.SEQ_MAIN].Trace(strMsg);
                    break;

                case eLogLevel.DEBUG:
                    log[eType].Debug(strMsg);
                    if (eType > eLogType.SEQ_MAIN) log[eLogType.SEQ_MAIN].Debug(strMsg);
                    break;

                case eLogLevel.INFO:
                    log[eType].Info(strMsg);
                    if (eType > eLogType.SEQ_MAIN) log[eLogType.SEQ_MAIN].Info(strMsg);
                    break;

                case eLogLevel.WARN:
                    log[eType].Warn(strMsg);
                    if (eType > eLogType.SEQ_MAIN) log[eLogType.SEQ_MAIN].Warn(strMsg);
                    break;

                case eLogLevel.ERROR:
                    log[eType].Error(strMsg);
                    if (eType > eLogType.SEQ_MAIN) log[eLogType.SEQ_MAIN].Error(strMsg);
                    break;

                case eLogLevel.FATAL:
                    log[eType].Fatal(strMsg);
                    if (eType > eLogType.SEQ_MAIN) log[eLogType.SEQ_MAIN].Fatal(strMsg);
                    break;
            }

            if (LogUpdateEvent != null && bShowUI == true)
                LogUpdateEvent(eType, eLevel, strMsg);
        }

        /// <summary>
        /// Log 클래스를 가져온다.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Logger GetLogClass(eLogType type)
        {
            return log[type];
        }

        /*
        ArchiveNumbering
        롤링-롤링 스타일 번호 지정 (가장 최근 항목은 항상 # 0 다음에 # 1, ..., #N).
        시퀀스-시퀀스 스타일 번호 지정. 가장 최근 아카이브의 수가 가장 높습니다.
        날짜-날짜 스타일 번호 매기기. 날짜는의 값에 따라 형식이 지정됩니다 archiveDateFormat.
        경고 : NLog ver. 이전 4.5.7 그러면 이것은 archiveAboveSize. 최신 버전은 아카이브의 기존 파일에 올바르게 병합됩니다.
        DateAndSequence- Date 와 Sequence의 조합. 아카이브에는 이전 기간 (년, 월, 일) datetime이 표시됩니다.
        가장 최근의 아카이브가 가장 높은 숫자 (날짜와 함께)를 갖습니다. 날짜는 archiveDateFormat 값에 따라 형식이 지정됩니다.
         */
    }
}