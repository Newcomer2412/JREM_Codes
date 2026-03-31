using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MachineControlBase
{
    #region UI 컨트롤 관련 클래스

    /// <summary>
    /// 토글 스위치 조절
    /// </summary>
    public class ToggleSwitchOffsetConverter : IValueConverter
    {
        public bool IsReversed { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var width = (double)value;
            return width > 20D ? IsReversed ? -((width / 2) - 10) : (width / 2) - 10 : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public static class DependencyObjectExtension
    {
        public static bool TryCast<TElement>(this DependencyObject dObj, out TElement element) where TElement : UIElement
        {
            element = dObj as TElement;
            return element != null;
        }

        public static DependencyObject FindAncestorOfType(this DependencyObject o, Type ancestorType)
        {
            var parent = VisualTreeHelper.GetParent(o);
            if (parent != null)
            {
                if (parent.GetType().IsSubclassOf(ancestorType) || parent.GetType() == ancestorType)
                {
                    return parent;
                }
                return FindAncestorOfType(parent, ancestorType);
            }
            return null;
        }
    }

    #endregion UI 컨트롤 관련 클래스

    #region 기타 기능 클래스

    /// <summary>
    /// MainWindow 통신 연결 끊김 이벤트 처리
    /// </summary>
    /// <param name="bStatus"></param>
    public delegate void TCPConnectStatus_EventHandler(bool bStatus);

    /// <summary>
    /// 공용 클래스
    /// </summary>
    public static class CCommon
    {
        /// <summary>
        /// UserControl에서 자식 Control의 정보를 가져온다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static List<T> GetLogicalChildCollection<T>(this DependencyObject parent) where T : DependencyObject
        {
            List<T> logicalCollection = new List<T>();
            GetLogicalChildCollection(parent, logicalCollection);
            return logicalCollection;
        }

        /// <summary>
        /// UserControl에서 자식 Control의 정보를 가져온다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <param name="logicalCollection"></param>
        private static void GetLogicalChildCollection<T>(DependencyObject parent, List<T> logicalCollection) where T : DependencyObject
        {
            IEnumerable children = LogicalTreeHelper.GetChildren(parent);
            foreach (object child in children)
            {
                if (child is DependencyObject)
                {
                    DependencyObject depChild = child as DependencyObject;
                    if (child is T)
                    {
                        logicalCollection.Add(child as T);
                    }
                    GetLogicalChildCollection(depChild, logicalCollection);
                }
            }
        }

        /// <summary>
        /// 그냥 메세지를 보여준다.
        /// </summary>
        /// <param name="strData"></param>
        public static void ShowMessage(string strData)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                CMessageUI dlg = new CMessageUI();
                dlg.m_strInfo = strData;
                dlg.m_showOption = 0;
                dlg.m_userButtonUsed = false;
                dlg.m_userButtonString = "";
                dlg.ShowDialog();
            }));
        }

        /// <summary>
        /// 선택한 버튼 No를 리턴함.
        /// </summary>
        /// <param name="iOption"></param>
        /// <param name="strData"></param>
        /// <returns></returns>
        public static int ShowMessage(int iOption, string strData)
        {
            int returnValue = 0;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                CMessageUI dlg = new CMessageUI();
                dlg.m_strInfo = strData;
                dlg.m_showOption = iOption;
                dlg.m_userButtonUsed = false;
                dlg.m_userButtonString = "";
                dlg.ShowDialog();
                returnValue = dlg.m_selectButton;
            }));

            return returnValue;
        }

        /// <summary>
        /// 그냥 메세지를 보여준다.
        /// </summary>
        /// <param name="strData"></param>
        public static void ShowMessageMini(string strData)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                CMessageMiniUI dlg = new CMessageMiniUI();
                dlg.m_strInfo = strData;
                dlg.m_showOption = 0;
                dlg.m_userButtonUsed = false;
                dlg.m_userButtonString = "";
                dlg.ShowDialog();
            }));
        }

        /// <summary>
        /// 선택한 버튼 No를 리턴함.
        /// </summary>
        /// <param name="iOption"></param>
        /// <param name="strData"></param>
        /// <returns></returns>
        public static int ShowMessageMini(int iOption, string strData)
        {
            int returnValue = 0;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                CMessageMiniUI dlg = new CMessageMiniUI();
                dlg.m_strInfo = strData;
                dlg.m_showOption = iOption;
                dlg.m_userButtonUsed = false;
                dlg.m_userButtonString = "";
                dlg.ShowDialog();
                returnValue = dlg.m_selectButton;
            }));

            return returnValue;
        }

        /// <summary>
        /// ※ 현재 패스를 string class 형으로 리턴.
        /// iOption(1: 현 상태 그대로 , 2: Debug 빼고 리턴)
        /// </summary>
        /// <param name="iOption"></param>
        /// <returns></returns>
        public static string CurrPath(int iOption)
        {
            string retString = string.Empty;
            string filePath = AppDomain.CurrentDomain.BaseDirectory;

            if (iOption == 0)
            {
                retString = filePath;
            }
            else if (iOption == 1)
            {
                retString = filePath.Replace(@"Debug\", "");
            }
            else if (iOption == 2)
            {
                // return 값에 외부에서 문자 경로를 덧 붙힐 경우 경로문자를 인지하지 못하는 일이 발생할 수 있어서 마지막 \ 문자 삭제 하여 출력.
                retString = filePath.Replace(@"\Debug\", "");
            }
            return retString;
        }

        /// <summary>
        /// Data 자동 삭제
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="iDays">Store the number of days after which you want to delete the Data.</param>
        public static void AutoDelete(string strPath, int iDays)
        {
            DirectoryInfo dir = new DirectoryInfo(strPath);
            // 폴더가 없으면 리턴
            if (dir.Exists == false) return;

            foreach (FileInfo fi in dir.GetFiles())
            {
                if (fi.CreationTime <= DateTime.Now.AddDays(-iDays)) fi.Delete(); // Delete the file.
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                AutoDelete(di.FullName, iDays);
                String[] objSubSubDirectory = Directory.GetDirectories(di.FullName);
                if (objSubSubDirectory.Length == 0 && Directory.GetFiles(di.FullName).Length == 0) di.Delete();
            }
        }

        #region D드라이브의 사용량이 전체의 절반을 초과되면 Vision 이미지를 3일분만 남겨두고 전부 삭제한다.

        /// <summary>
        /// D드라이브의 사용량이 전체의 절반을 초과하면 bAutoDeleteStart 플래그 true
        /// </summary>
        public static void Checkdrives()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in drives)
            {
                if (drive.DriveType == DriveType.Fixed)
                {
                    if (drive.Name.Contains("D"))
                    {
                        if (drive.TotalFreeSpace > drive.AvailableFreeSpace * 2)
                        {
                            CXMLProcess.bAutoDeleteStart = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// D드라이브의 사용량이 전체의 절반을 초과되면 Vision 이미지를 3일분만 남겨두고 전부 삭제한다.
        /// 추가 확인 후 업데이트 필요
        /// </summary>
        /// <param name="strName"></param>
        public static void AutoDelete(string strName)
        {
            Checkdrives();

            if (CXMLProcess.bAutoDeleteStart == true)
            {
                DirectoryInfo dir = new DirectoryInfo(strName);
                if (dir.Exists == false) return;

                FileInfo[] fi = dir.GetFiles();
                for (int i = 0; i < fi.Length - 1; i++)
                {
                    if (fi[i].Name.Contains("Camera"))
                    {
                        foreach (DirectoryInfo di in dir.GetDirectories())
                        {
                            FileInfo[] files = di.GetFiles();

                            if (files.Length <= 3) return;

                            var filesorted = files.OrderByDescending(x => x.CreationTime).ToArray();

                            foreach (FileInfo file in filesorted.Skip(3))
                            {
                                file.Delete();
                            }
                        }
                    }
                    else
                    {
                        if (fi.Length <= 3) return;

                        var filesorted = fi.OrderByDescending(x => x.CreationTime).ToArray();

                        foreach (FileInfo file in filesorted.Skip(3))
                        {
                            file.Delete();
                        }
                    }
                }
            }
        }

        #endregion D드라이브의 사용량이 전체의 절반을 초과되면 Vision 이미지를 3일분만 남겨두고 전부 삭제한다.

        /// <summary>
        /// 장비 제어 관련 데이터를 백업
        /// </summary>
        public static void DataBackUp()
        {
            // 복사할 디렉토리
            DirectoryInfo sourceDirectory = new DirectoryInfo(System.Windows.Forms.Application.StartupPath.Substring(0,
                                                System.Windows.Forms.Application.StartupPath.LastIndexOf("\\")) + @"\Data\");

            DateTime dt = DateTime.Now;
            string DestDir = CXMLProcess.DataBackupPath + dt.ToString("yyyyMMdd");
            // 저장될 폴더
            DirectoryInfo destDirectory = new DirectoryInfo(DestDir);

            // 저장될 폴더가 없으면 생성
            if (destDirectory.Exists == false)
            {
                destDirectory.Create();
            }
            // 복사를 진행
            CopyDirectories(sourceDirectory, destDirectory);
        }

        /// <summary>
        /// sourceDirectory를 destDirectory경로에 복사
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="destDirectory"></param>
        private static void CopyDirectories(DirectoryInfo sourceDirectory, DirectoryInfo destDirectory)
        {
            FileInfo[] sourceFiles = sourceDirectory.GetFiles();

            // 폴더 내의 파일들을 복사
            foreach (FileInfo file in sourceFiles)
            {
                file.CopyTo(destDirectory.FullName + "\\" + file.Name, true);
            }

            DirectoryInfo[] sourceSubDirectories = sourceDirectory.GetDirectories();
            // 하위 폴더를 검색
            foreach (DirectoryInfo subDirectory in sourceSubDirectories)
            {
                DirectoryInfo destSubDirectory = destDirectory.CreateSubdirectory(subDirectory.Name);
                // 재귀 호출
                CopyDirectories(subDirectory, destSubDirectory);
            }
        }

        /// <summary>
        /// 장비 제어 관련 데이터를 백업
        /// </summary>
        public static void CheckDataBackUp()
        {
            COptionData cOptionData = CMainLib.Ins.cOptionData;

            if (CMainLib.Ins.McState == eMachineState.RUN || CMainLib.Ins.McState == eMachineState.MANUALRUN) return;
            // 백업 설정 사용 시
            if (cOptionData.bAutoBackupUse == true)
            {
                DateTime CurrentDate = DateTime.Now;

                // 매일 저장
                if (cOptionData.iBackupDateIndex == (int)_Day.Everyday)
                {
                    try
                    {
                        // 현재 시간이 설정한 시간을 지났으면
                        if (DateTime.Parse(cOptionData.strCompareBackupDate).CompareTo(CurrentDate) < 0)
                        {
                            // 다음 날짜로 변경. 매일이므로 + 1
                            cOptionData.strCompareBackupDate = DateTime.Parse(CurrentDate.ToString()).AddDays(1).ToString();
                            // Data 폴더 복사
                            DataBackUp();
                        }
                    }
                    catch
                    {
                        cOptionData.strCompareBackupDate = DateTime.Now.ToString();
                        NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.FATAL, "Auto Data Backup Exception");
                    }
                }
                // 매일 저장이 아니고 요일별 저장
                else
                {
                    // 설정한 요일이고
                    if (cOptionData.strBackupDate == CurrentDate.DayOfWeek.ToString())
                    {
                        try
                        {
                            // 현재시간이 설정한 시간을 지났으면
                            if (DateTime.Parse(cOptionData.strCompareBackupDate).CompareTo(CurrentDate) < 0)
                            {
                                // 다음주 날짜로 변경 주단위이므로 + 7
                                cOptionData.strCompareBackupDate = DateTime.Parse(cOptionData.strCompareBackupDate).AddDays(7).ToString();
                                // 데이터를 복사
                                DataBackUp();
                            }
                        }
                        catch
                        {
                            cOptionData.strCompareBackupDate = DateTime.Now.ToString();
                            NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.FATAL, "Auto Data Backup Exception");
                        }
                    }
                }
            }
            CXMLProcess.WriteXml(CXMLProcess.OptionDataFilePath, cOptionData);
        }

        /// <summary>
        /// Deep Clone 구현
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepClone<T>(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("Object cannot be null.");
            return (T)Process(obj, new Dictionary<object, object>() { });
        }

        /// <summary>
        /// Deep Copy 처리
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="circular"></param>
        /// <returns></returns>
        private static object Process(object obj, Dictionary<object, object> circular)
        {
            if (obj == null)
                return null;

            Type type = obj.GetType();

            if (type.IsValueType || type == typeof(string))
            {
                return obj;
            }

            if (type.IsArray)
            {
                if (circular.ContainsKey(obj))
                    return circular[obj];

                string typeNoArray = type.FullName.Replace("[]", string.Empty);
                Type elementType = Type.GetType(typeNoArray + ", " + type.Assembly.FullName);
                var array = obj as Array;
                Array arrCopied = Array.CreateInstance(elementType, array.Length);
                circular[obj] = arrCopied;

                for (int i = 0; i < array.Length; i++)
                {
                    object element = array.GetValue(i);
                    object objCopy = null;

                    if (element != null && circular.ContainsKey(element))
                        objCopy = circular[element];
                    else
                        objCopy = Process(element, circular);
                    arrCopied.SetValue(objCopy, i);
                }
                return Convert.ChangeType(arrCopied, obj.GetType());
            }

            if (type.IsClass)
            {
                if (circular.ContainsKey(obj))
                    return circular[obj];

                object objValue = Activator.CreateInstance(obj.GetType());
                circular[obj] = objValue;
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (FieldInfo field in fields)
                {
                    object fieldValue = field.GetValue(obj);
                    if (fieldValue == null)
                        continue;
                    object objCopy = circular.ContainsKey(fieldValue) ? circular[fieldValue] : Process(fieldValue, circular);
                    field.SetValue(objValue, objCopy);
                }
                return objValue;
            }
            else
                throw new ArgumentException("Unknown type");
        }

        /// <summary>
        /// Serializable 객체에 대한 Deep Clone
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T SerializableDeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var bformatter = new BinaryFormatter();
                bformatter.Serialize(ms, obj);
                ms.Position = 0;
                return (T)bformatter.Deserialize(ms);
            }
        }
    }

    /// <summary>
    /// 지금 화면 표시 중인 UI Index
    /// </summary>
    public enum _UIIndex : int
    {
        /// <summary>
        /// None UI
        /// </summary>
        NoneUI = -1,

        /// <summary>
        /// Main UI
        /// </summary>
        MainUI = 0,

        /// <summary>
        /// Data Select UI
        /// </summary>
        DataSelectUI,

        /// <summary>
        /// Data Edit UI
        /// </summary>
        DataEditUI,

        /// <summary>
        /// Option Form
        /// </summary>
        UserSetUI,

        /// <summary>
        /// IO Monitor UI
        /// </summary>
        IOMonitorUI,

        /// <summary>
        /// Alarm Screen UI
        /// </summary>
        AlarmScreenUI,

        /// <summary>
        /// Log View UI
        /// </summary>
        LogViewUI,

        /// <summary>
        /// 결과 Graph View UI
        /// </summary>
        GraphViewUI,

        /// <summary>
        /// Vision Graph View UI
        /// </summary>
        VisionGraphViewUI,

        /// <summary>
        /// 에러 결과 Graph View UI
        /// </summary>
        ErrorViewUI,
    }

    public enum eMBoxRtn : int
    {
        A_OK = 0,
        A_Cancel = 1,
        A_Ignore = 2
    };

    /// <summary>
    /// 저장될 요일
    /// </summary>
    public enum _Day : int
    {
        /// <summary>
        /// 매일
        /// </summary>
        Everyday = 0,

        /// <summary>
        /// 월요일
        /// </summary>
        Monday = 1,

        /// <summary>
        /// 화요일
        /// </summary>
        Tuesday = 2,

        /// <summary>
        /// 수요일
        /// </summary>
        Wednesday = 3,

        /// <summary>
        /// 목요일
        /// </summary>
        Thursday = 4,

        /// <summary>
        /// 금요일
        /// </summary>
        Friday = 5,

        /// <summary>
        /// 토요일
        /// </summary>
        Saturday = 6,

        /// <summary>
        /// 일요일
        /// </summary>
        Sunday = 7,
    }

    /// <summary>
    /// 지금 Main에 보이는 화면이 무엇인지 확인 class
    /// </summary>
    public static class CheckNowUseUI
    {
        private static _UIIndex currentUIIndex = _UIIndex.NoneUI;

        /// <summary>
        /// 지금 사용중인 UI Index
        /// </summary>
        public static _UIIndex CurrentUIIndex
        {
            get
            {
                return currentUIIndex;
            }
            set
            {
                BeforeCurrentUIIndex = currentUIIndex;
                currentUIIndex = value;
            }
        }

        public static _UIIndex BeforeCurrentUIIndex { get; set; } = _UIIndex.NoneUI;
    }

    #endregion 기타 기능 클래스
}