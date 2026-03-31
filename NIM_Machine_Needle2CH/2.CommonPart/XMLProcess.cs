using System;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

/* Serializable 방법
 * 클래스 위에 [Serializable] 삽입하면 모두 Serializable 된다. 제외하고 싶을 때는 원하는 값 위에 [NonSerialized]를 추가한다.
 * XML 저장에 제외하고 싶을 때에는 변수나 클래스 위에 [XmlIgnore] 를 선언한다.
 */

namespace MachineControlBase
{
    /// <summary>
    /// Lib Data를 XML로 저장하고 불러오는 함수
    /// </summary>
    public static class CXMLProcess
    {
        #region 파일 경로

        public static string BasePath = Application.StartupPath.Substring(0, Application.StartupPath.LastIndexOf("\\"));
        public static string AlarmImgData = Application.StartupPath + @"\AlarmImgData\";

        public static string DataFolderPath = BasePath + @"\Data\SetData";
        public static string SystemParameterSingleFilePath = DataFolderPath + @"\SystemParameterSingle.xml";
        public static string RunUnitDataFilePath = DataFolderPath + @"\RunUnitData.xml";
        public static string SystemParameterCollectionDataFilePath = DataFolderPath + @"\SystemParameterCollectionData.xml";
        public static string AxisParameterCollectionDataFilePath = DataFolderPath + @"\AxisParameterCollectionData.xml";
        public static string AxisPositionCollectionDataFilePath = DataFolderPath + @"\AxisPositionCollectionData.xml";
        public static string OptionDataFilePath = DataFolderPath + @"\OptionData.xml";
        public static string IniDataFolderPath = BasePath + @"\Data\IniData\";
        public static string VisionData = BasePath + @"\Data\VisionData\";

        public static string BackupBasePath = @"D:\Jrem\";
        public static string DataBackupPath = BackupBasePath + @"Backup\";
        public static string LogPath = BackupBasePath + @"Logs\";

        public static bool bAutoDeleteStart = false;        //AutoDelete 개선된 함수를 작동하게 하는 플래그 현재 사용안하고 있음 추후 수정 후 적용 필요

        /// <summary>
        /// DB 관련 위치 확인
        /// </summary>
        public static string DBPath = BackupBasePath + @"DB\";

        public static string DBFile_MassProduction = DBPath + @"\MassProduction.db";
        public static string DBFile_Error = DBPath + @"\Error.db";
        public static string DBFile_Vision = DBPath + @"\Vision.db";

        #endregion 파일 경로

        /// <summary>
        /// DB File 및 Folder 확인하여 없으면 생성
        /// </summary>
        public static void DBFileCheck()
        {
            // To Create saving folder for DB file, after checking folder existing.
            Directory.CreateDirectory(DBPath);

            if (File.Exists(DBFile_Error) == false)
            {
                // To Create DB file after check is it exisitng or not.
                SQLiteConnection.CreateFile(DBFile_Error);
            }

            if (File.Exists(DBFile_MassProduction) == false)
            {
                SQLiteConnection.CreateFile(DBFile_MassProduction);
            }
        }

        /// <summary>
        /// DB 내 테이블이 있는지 확인 후 없으면 생성
        /// </summary>
        public static void Create_Tables()
        {
            try
            {
                string strDBData_Error = "CREATE TABLE IF NOT EXISTS " + "Error" + " (DateTime DATETIME NOT NULL," +
                                         "ErrorCode INTEGER NOT NULL," + "Count INTEGER NOT NULL)";

                string strDBData_MassProduction = "CREATE TABLE IF NOT EXISTS " + "MassProduction" +
                                                  "(Start DATETIME NOT NULL, End DATETIME NOT NULL," +
                                                  "Total INTEGER NOT NULL, Good INTEGER NOT NULL, Not_Good INTEGER NOT NULL)";

                SQLiteCommand cSQLiteCommand_Error = new SQLiteCommand(strDBData_Error, CMainLib.Ins.SQLError);
                cSQLiteCommand_Error.ExecuteNonQuery();

                SQLiteCommand cSQLiteCommand_MassProduction = new SQLiteCommand(strDBData_MassProduction, CMainLib.Ins.SQLProduction);
                cSQLiteCommand_MassProduction.ExecuteNonQuery();

                for (int i = 0; i < Define.MAX_CAMERA; i++)
                {
                    for (int j = 0; j < CMainLib.Ins.cVisionData.strToolBlockName[i].Length; j++)
                    {
                        string strCamName = ((eCAM)i).ToString();
                        string strFuncName = strCamName + "_" + CMainLib.Ins.cVisionData.strToolBlockName[i][j];

                        string strDBData = "CREATE TABLE IF NOT EXISTS " + strFuncName + " (N_Cam varchar(20) NOT NULL," +
                                           "Start DATETIME NOT NULL ," +
                                           "End DATETIME NOT NULL ," +
                                           "Total INTEGER NOT NULL," +
                                           "Good INTEGER NOT NULL," +
                                           "Not_Good INTEGER NOT NULL," +
                                           "Function varchar(20) NOT NULL)";
                        SQLiteCommand cSQLiteCommand = new SQLiteCommand(strDBData, CMainLib.Ins.SQLVision);
                        cSQLiteCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.FATAL, ex.Message.ToString());
                MessageBox.Show(ex.Message);
                return;
            }
        }

        /// <summary>
        /// Xml File 데이터 로드 (리스트 데이터 사용 시 데이터 중복 생성 주의. 생성자에 리스트 추가 넣으면 안됨)
        /// </summary>
        public static void XmlFileLoad()
        {
            Directory.CreateDirectory(DataFolderPath);

            if (File.Exists(AxisParameterCollectionDataFilePath) == true)
            {
                CMainLib.Ins.cAxisParamCollData = (CAxisParameterCollectionData)ReadXml(AxisParameterCollectionDataFilePath, typeof(CAxisParameterCollectionData));
                if (CMainLib.Ins.cAxisParamCollData == null)
                {
                    // XML 파일이 깨진 경우 old 백업으로 데이터 로딩 시도
                    CMainLib.Ins.cAxisParamCollData = (CAxisParameterCollectionData)ReadXml(AxisParameterCollectionDataFilePath + ".old", typeof(CAxisParameterCollectionData));
                    if (CMainLib.Ins.cAxisParamCollData == null)
                    {
                        CCommon.ShowMessageMini("AxisParameterCollectionData.xml 파일 손상! Jrem에 문의하세요.");
                    }
                    else
                    {
                        CCommon.ShowMessageMini("AxisParameterCollectionData.xml 파일 자동 복구! 각 축 파라메터 값이 최신값과 같은지 확인하세요.");
                    }
                }
            }

            if (File.Exists(SystemParameterSingleFilePath) == true)
            {
                CMainLib.Ins.cSysOne = (CSystemParameterSingle)ReadXml(SystemParameterSingleFilePath, typeof(CSystemParameterSingle));
                if (CMainLib.Ins.cSysOne == null)
                {
                    CMainLib.Ins.cSysOne = (CSystemParameterSingle)ReadXml(SystemParameterSingleFilePath + ".old", typeof(CSystemParameterSingle));
                    if (CMainLib.Ins.cSysOne == null)
                    {
                        CCommon.ShowMessageMini("SystemParameterSingle.xml 파일 손상! Jrem에 문의하세요.");
                    }
                    else
                    {
                        WriteXml(SystemParameterSingleFilePath, CMainLib.Ins.cSysOne);
                        CCommon.ShowMessageMini("SystemParameterSingle.xml 파일 자동 복구! 각 파라메터 값이 최신값과 같은지 확인하세요.");
                    }
                }
            }
            else
            {
                WriteXml(SystemParameterSingleFilePath, CMainLib.Ins.cSysOne);
            }

            if (File.Exists(RunUnitDataFilePath) == true)
            {
                CMainLib.Ins.cRunUnitData = (CRunUnitData)ReadXml(RunUnitDataFilePath, typeof(CRunUnitData));
                if (CMainLib.Ins.cRunUnitData == null)
                {
                    CMainLib.Ins.cRunUnitData = (CRunUnitData)ReadXml(RunUnitDataFilePath + ".old", typeof(CRunUnitData));
                    if (CMainLib.Ins.cRunUnitData == null)
                    {
                        CCommon.ShowMessageMini("RunUnitData.xml 파일 손상! Jrem에 문의하세요.");
                    }
                    else
                    {
                        WriteXml(RunUnitDataFilePath, CMainLib.Ins.cRunUnitData);
                        CCommon.ShowMessageMini("RunUnitData.xml 파일 자동 복구! 각 자재 데이터가 최신값과 같은지 확인하세요.");
                    }
                }
                if (CMainLib.Ins.cRunUnitData != null) CMainLib.Ins.cRunUnitData.init();
            }
            else
            {
                CMainLib.Ins.cRunUnitData.init();
                WriteXml(RunUnitDataFilePath, CMainLib.Ins.cRunUnitData);
            }

            if (File.Exists(AxisPositionCollectionDataFilePath) == true)
            {
                CMainLib.Ins.cAxisPosCollData = (CAxisPositionCollectionData)ReadXml(AxisPositionCollectionDataFilePath, typeof(CAxisPositionCollectionData));
                if (CMainLib.Ins.cAxisPosCollData == null)
                {
                    CMainLib.Ins.cAxisPosCollData = (CAxisPositionCollectionData)ReadXml(AxisPositionCollectionDataFilePath + ".old", typeof(CAxisPositionCollectionData));
                    if (CMainLib.Ins.cAxisPosCollData == null)
                    {
                        CCommon.ShowMessageMini("AxisPositionCollectionData.xml 파일 손상! Jrem에 문의하세요.");
                    }
                    else
                    {
                        WriteXml(AxisPositionCollectionDataFilePath, CMainLib.Ins.cAxisPosCollData);
                        CCommon.ShowMessageMini("AxisPositionCollectionData.xml 파일 자동 복구! 각 축 위치 데이터가 최신값과 같은지 확인하세요.");
                    }
                }
            }
            else
            {
                WriteXml(AxisPositionCollectionDataFilePath, CMainLib.Ins.cAxisPosCollData);
            }

            if (File.Exists(SystemParameterCollectionDataFilePath) == true)
            {
                CMainLib.Ins.cSysParamCollData = (CSystemParameterCollectionData)ReadXml(SystemParameterCollectionDataFilePath, typeof(CSystemParameterCollectionData));
                if (CMainLib.Ins.cSysParamCollData == null)
                {
                    CMainLib.Ins.cSysParamCollData = (CSystemParameterCollectionData)ReadXml(SystemParameterCollectionDataFilePath + ".old", typeof(CSystemParameterCollectionData));
                    if (CMainLib.Ins.cSysParamCollData == null)
                    {
                        CCommon.ShowMessageMini("SystemParameterCollectionData.xml 파일 손상! Jrem에 문의하세요.");
                    }
                    else
                    {
                        WriteXml(SystemParameterCollectionDataFilePath, CMainLib.Ins.cSysParamCollData);
                        CCommon.ShowMessageMini("SystemParameterCollectionData.xml 파일 자동 복구! 각 파라메터 데이터가 최신값과 같은지 확인하세요.");
                    }
                }
            }
            else
            {
                WriteXml(SystemParameterCollectionDataFilePath, CMainLib.Ins.cSysParamCollData);
            }

            if (File.Exists(OptionDataFilePath) == true)
            {
                CMainLib.Ins.cOptionData = (COptionData)ReadXml(OptionDataFilePath, typeof(COptionData));
                if (CMainLib.Ins.cOptionData == null)
                {
                    CMainLib.Ins.cOptionData = (COptionData)ReadXml(OptionDataFilePath + ".old", typeof(COptionData));
                    if (CMainLib.Ins.cOptionData == null)
                    {
                        CCommon.ShowMessageMini("OptionData.xml 파일 손상! Jrem에 문의하세요.");
                    }
                    else
                    {
                        WriteXml(OptionDataFilePath, CMainLib.Ins.cOptionData);
                        CCommon.ShowMessageMini("OptionData.xml 파일 자동 복구! 설정 값이 최신값과 같은지 확인하세요.");
                    }
                }
            }
            else
            {
                WriteXml(OptionDataFilePath, CMainLib.Ins.cOptionData);
            }
        }

        /// <summary>
        /// Xml File 데이터 저장
        /// </summary>
        public static void XmlFileSave()
        {
            WriteXml(AxisParameterCollectionDataFilePath, CMainLib.Ins.cAxisParamCollData);
            WriteXml(RunUnitDataFilePath, CMainLib.Ins.cRunUnitData);
            foreach (ModelAxisPositionData posData in CMainLib.Ins.cAxisPosCollData.cModelAxisPositionDataList)
            {
                posData.OrderByNo(); // Position No 순차적 정렬
            }
            WriteXml(AxisPositionCollectionDataFilePath, CMainLib.Ins.cAxisPosCollData);
            WriteXml(SystemParameterCollectionDataFilePath, CMainLib.Ins.cSysParamCollData);
            WriteXml(SystemParameterSingleFilePath, CMainLib.Ins.cSysOne);
            WriteXml(OptionDataFilePath, CMainLib.Ins.cOptionData);
        }

        /// <summary>
        /// 폴더 유무 확인
        /// </summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public static bool FindFolder(string strPath)
        {
            return Directory.Exists(strPath);
        }

        /// <summary>
        /// 파일이 있는지 없는지를 확인
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool FileFind(string filePath)
        {
            FileInfo fi = new FileInfo(filePath);
            return fi.Exists;
        }

        /// <summary>
        /// 폴더를 찿아 없으면 생성
        /// </summary>
        /// <param name="strFilePath"></param>
        public static void CreateFolder(string strFilePath)
        {
            if (Directory.Exists(strFilePath) == false)
            {
                Directory.CreateDirectory(strFilePath);
            }
        }

        /// <summary>
        /// 입력받은 클래스에 파일 로드
        /// </summary>
        /// <param name="sFileName"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static object ReadXml(string sFileName, Type t)
        {
            object obj = null;
            if (File.Exists(sFileName))
            {
                FileStream stream = File.OpenRead(sFileName);
                try
                {
                    XmlSerializer xml = new XmlSerializer(t);
                    obj = xml.Deserialize(stream);
                }
                catch (Exception ex)
                {
                    stream.Close();
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, ex.ToString());
                }
                stream.Close();
            }
            return obj;
        }

        /// <summary>
        /// 입력받은 클래스로 파일 생성
        /// </summary>
        /// <param name="sFileName"></param>
        /// <param name="obj"></param>
        public static void WriteXml(string sFileName, object obj)
        {
            // XML파일을 쓰다가 강제 종료 혹은 파워가 다운되면 XML 데이터가 초기화되어 문제가 됨으로 old 복사본 생성
            if (FileFind(sFileName) == true)
            {
                FileInfo file = new FileInfo(sFileName);
                file.CopyTo(sFileName + ".old", true);
            }

            FileStream stream = File.Create(sFileName);
            try
            {
                XmlSerializer xml = new XmlSerializer(obj.GetType());
                xml.Serialize(stream, obj);
            }
            catch (Exception ex)
            {
                stream.Close();
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, ex.ToString());
            }
            stream.Close();
        }

        /// <summary>
        /// Model Vision 폴더 경로
        /// </summary>
        /// <param name="uiModelNo"></param>
        /// <returns></returns>
        public static string GetVisionModelPath(uint uiModelNo)
        {
            return string.Format(@"{0}Model{1}\", VisionData, uiModelNo);
        }
    }
}