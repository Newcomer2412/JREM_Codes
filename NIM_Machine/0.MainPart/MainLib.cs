using Cognex.VisionPro;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// 장비의 모든 데이터를 관리하는 Class
    /// </summary>
    public class CMainLib
    {
        private static readonly object readLock = new object();
        private static CMainLib instance = null;

        public static CMainLib Ins
        {
            get
            {
                lock (readLock)
                {
                    if (instance == null) instance = new CMainLib();
                    return instance;
                }
            }
            set
            {
                instance = value;
            }
        }

        #region Data

        public CAxisPositionCollectionData cAxisPosCollData = null;
        public CAxisParameterCollectionData cAxisParamCollData = null;
        public CSystemParameterCollectionData cSysParamCollData = null;
        public CSystemParameterSingle cSysOne = null;
        public CRunUnitData cRunUnitData = null;
        public COptionData cOptionData = null;
        public CVar cVar = null;

        public SQLiteConnection SQLProduction = null;
        public SQLiteConnection SQLError = null;
        public SQLiteConnection SQLVision = null;

        public CUIManager cDataEditUIManager = null;

        public CVisionData cVisionData = null;

        private CStart_to_Stop cStart_to_Stop = null;
        public List<CStart_to_Stop> cList_StS = null;

        /// <summary>
        /// Image File Save 관리 클래스
        /// </summary>
        public ImageSaveProcess cimageSaveProcess = null;

        #endregion Data

        #region 모션 제어

        /// <summary>
        /// IO 제어
        /// </summary>
        public UZ_IO_Module Uz_IO_Module = null;

        /// <summary>
        /// RTEX 이모션텍 제어기
        /// </summary>
        public eMotionTekRTEX[] RtexModule = new eMotionTekRTEX[2];

        /// <summary>
        /// 이더넷 스텝 컨트롤러 UNI 이모션텍 제어기
        /// </summary>
        public eMotionTekUNI[] UniModule = new eMotionTekUNI[5];

        /// <summary>
        /// 축 통합 제어 Dictionary
        /// </summary>
        private Dictionary<eMotor, MotionProcMgr> axis = null;

        public Dictionary<eMotor, MotionProcMgr> Axis
        {
            get { return axis; }
            set { axis = value; }
        }

        #endregion 모션 제어

        #region 통신 관련

        /// <summary>
        /// 카메라 조명(비전코웍) 컨트롤러
        /// </summary>
        public VisionCoworkLight cLightCtl = null;

        /// <summary>
        /// UV(Altis) 컨트롤러
        /// </summary>
        public UV_Altis cUV_Ctl = null;

        /// <summary>
        /// AIM 피더
        /// </summary>
        public AIM_FlexiblePartsFeeder AIM_Feeder = new AIM_FlexiblePartsFeeder();

        #endregion 통신 관련

        #region Vision

        /// <summary>
        /// Vision 화면 UI
        /// </summary>
        public VisionToolBlockUI[] cVisionToolBlockUI = new VisionToolBlockUI[Define.MAX_CAMERA];

        /// <summary>
        /// Vision ToolBlock 데이터 처리
        /// </summary>
        public CVisionToolBlockLib[] cCVisionToolBlockLib = new CVisionToolBlockLib[Define.MAX_CAMERA];

        #endregion Vision

        #region Sequence, IO, 구동 데이터 등 제어

        public IOProcMgr IO = null;

        private static readonly object SeqReadLock = new object();

        public SeqProcMgr seq = null;

        public SeqProcMgr Seq
        {
            get
            {
                lock (SeqReadLock)
                {
                    return seq;
                }
            }
            set
            {
                seq = value;
            }
        }

        #endregion Sequence, IO, 구동 데이터 등 제어

        /// <summary>
        /// 장비 운영 상태
        /// </summary>
        public eMachineState McState;

        /// <summary>
        /// 사용자 권한
        /// </summary>
        public eUserLevel UserLevel;

        /// <summary>
        /// 생성자
        /// </summary>
        public CMainLib()
        {
            cAxisParamCollData = new CAxisParameterCollectionData();
            cSysOne = new CSystemParameterSingle();
            cRunUnitData = new CRunUnitData();
            cAxisPosCollData = new CAxisPositionCollectionData();
            cSysParamCollData = new CSystemParameterCollectionData();
            cOptionData = new COptionData();
            cVar = new CVar();

            cVisionData = new CVisionData();

            cList_StS = new List<CStart_to_Stop>();

            cDataEditUIManager = new CUIManager();

            CXMLProcess.DBFileCheck(); // DB File & Folder check. If Folder and Files are not existed, it will be created.
            string strDBPath = string.Format($"Data Source= {CXMLProcess.DBFile_MassProduction}; Version=3;");
            SQLProduction = new SQLiteConnection(strDBPath);
            strDBPath = string.Format($"Data Source= {CXMLProcess.DBFile_Error}; Version=3;");
            SQLError = new SQLiteConnection(strDBPath);
            strDBPath = string.Format($"Data Source= {CXMLProcess.DBFile_Vision}; Version=3;");
            SQLVision = new SQLiteConnection(strDBPath);
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Init()
        {
            instance = this;
            // 클래스 생성
            ClassInit();
            // 데이터 로드
            CXMLProcess.XmlFileLoad();
            // connect to .db file
            SQLProduction.Open();
            SQLError.Open();
            SQLVision.Open();
            // Create Table if it does not exist.
            CXMLProcess.Create_Tables();
            // Axis Parameter
            AxisParameterInit();

            if (cOptionData.bImage_SaveUse == true) cimageSaveProcess = new ImageSaveProcess();

            // 프로그램 시작 시 Vision 이미지 백업을 삭제한다. (프로그램 종료 없이 그냥 삭제하는 경우 때문에 시작할 때 삭제 기능 추가)
            if (cOptionData.bAutoDeleteUse == true)
            {
                // 추가 확인이 필요한 기능 (D드라이브 용량 확인 후 파일 삭제하는 기능)
                //Parallel.Invoke(() => CCommon.AutoDelete(CXMLProcess.BackupBasePath + @"VisionImage\"));
                Parallel.Invoke(() => { CCommon.AutoDelete(CXMLProcess.BackupBasePath + @"VisionImage\", cOptionData.iAutoDeleteDays); });
            }

            #region Init Controller

            if (Define.SIMULATION == false)
            {
                if (IO.Init() == false)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "System Exit...");
                    Environment.Exit(0);
                    return;
                }

                Seq = new SeqProcMgr();
                Seq.SeqIO.IO_init();
                Seq.Init();

                // 서보 드라이버 제어 전원 인가
                Seq.SeqIO.SetDirectOutput((int)eIO_O.CONTROL_POWER_ON, true);
                Thread.Sleep(500);

                // 장비 MC power check.
                string strMessage = string.Empty;

                if (Seq.SeqMonitoring.Get_MCPower() == false)
                {
                    if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = "장비 MC 파워가 OFF 되어 있습니다. 파워를 켜고 OK 버튼을 눌러주세요.";
                    else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = "Power Off is Machine. Please.. Machine power ON and Ok button Click.";

                    if (CCommon.ShowMessage(1, strMessage) == (int)eMBoxRtn.A_OK)
                    {
                        if (Seq.SeqMonitoring.Get_MCPower() == false)
                        {
                            // Log 기록!
                            NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "System Exit...");
                            // 프로그램 종료 시 IO 세팅!
                            Seq.SeqMonitoring.Set_IO_System_Close();
                            Environment.Exit(0);
                            return;
                        }
                    }
                    else
                    {
                        // Log 기록!
                        NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "System Exit...");
                        // 프로그램 종료 시 IO 세팅!
                        Seq.SeqMonitoring.Set_IO_System_Close();
                        Environment.Exit(0);
                        return;
                    }
                }

                if (RtexModule[0].Init(1) == false)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "System Exit...");
                    Environment.Exit(0);
                    return;
                }
                if (RtexModule[1].Init(2) == false)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "System Exit...");
                    Environment.Exit(0);
                    return;
                }

                // 이모션텍 UNI 등록
                for (int i = 0; i < UniModule.Length; i++)
                {
                    if (UniModule[i].Init(i + 3) == false)
                    {
                        NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "System Exit...");
                        Environment.Exit(0);
                        return;
                    }
                }
                for (int i = 0; i < Enum.GetValues(typeof(eMotor)).Length - 1; i++)
                {
                    Axis[(eMotor)i].SetServoState(true);
                }

                // Serial Connect
                if (cOptionData.bLightSerialUse == true)
                {
                    if (cLightCtl == null) cLightCtl = new VisionCoworkLight();
                    if (cLightCtl.Open(cOptionData.strLightSerialComPort, null) == false)
                    {
                        NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Fail, Light Controller Port Open");
                    }
                }

                if (cUV_Ctl == null) cUV_Ctl = new UV_Altis();
                if (cUV_Ctl.Open(cOptionData.strUV_ComPort, null) == false)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Fail, UV Controller Port Open");
                }
                else
                {
                    cUV_Ctl.UV_OnTime((uint)cSysParamCollData.GetSysArray().dUVCureDelay * 10);
                    cUV_Ctl.UV_PowerSet(cSysParamCollData.GetSysArray().uiUVPower);
                }
            }
            else
            {
                Seq = new SeqProcMgr();
                Seq.SeqIO.IO_init();
                Seq.Init();
            }

            #endregion Init Controller

            // 형광등 사용 유무
            if (cOptionData.bFluorescentUse == true)
            {
                Seq.SeqIO.SetOutput((int)eIO_O.FLUORESCENT_LAMP_ONOFF, true);
            }
            else
            {
                Seq.SeqIO.SetOutput((int)eIO_O.FLUORESCENT_LAMP_ONOFF, false);
            }

            // 도어락 사용 유무
            //if (cOptionData.bDoorLockUse == true)
            //{
            //  Seq.SeqIO.SetOutput((int)eIO_O.SOFTREWE_BY_PASS_ON_OFF, false);
            //}
            //else
            //{
            Seq.SeqIO.SetOutput((int)eIO_O.DOOR_SOLENOID_LOCK_ONOFF, false);
            Seq.SeqIO.SetOutput((int)eIO_O.SOFTREWE_BY_PASS_ON_OFF, true);
            //}

            // 이온 아이저 On
            Seq.SeqIO.SetOutput((int)eIO_O.IONIZER_ONOFF, true);

            // 디스펜서 밸브 Output 트리거 셋
            int[] iTriggerTime = new int[4] { (int)CMainLib.Ins.cSysOne.uiDispValveDelay * 1000, 0, 0, 0 };
            Uz_IO_Module.SetOutputTriggerPeriod((int)eIO_O.DISPENSER_VALVE_OPEN, 1, iTriggerTime);
            Uz_IO_Module.SetOutputTriggerForTime((int)eIO_O.DISPENSER_VALVE_OPEN, 1, iTriggerTime);
            Uz_IO_Module.SetTriggerCount((int)eIO_O.DISPENSER_VALVE_OPEN, 1);

            // Initialize 완료
            cVar.bInitializeComplete = true;

            string strStSAdr = CXMLProcess.DataFolderPath + @"\Start_to_Stop.xml";
            cList_StS = (List<CStart_to_Stop>)CXMLProcess.ReadXml(strStSAdr, typeof(List<CStart_to_Stop>));
            if (cList_StS == null)
            {
                cList_StS = (List<CStart_to_Stop>)CXMLProcess.ReadXml(strStSAdr + ".old", typeof(List<CStart_to_Stop>));
                if (cList_StS == null)
                {
                    cList_StS = new List<CStart_to_Stop>();
                }
            }

            // Start to Stop 데이터 리스트 날짜 초기화
            string strToday = DateTime.Now.ToString("yyMMdd");
            if (cSysOne.strDate != strToday)
            {
                cSysOne.strDate = strToday;
                if (cList_StS != null)
                {
                    if (cList_StS.Count > 0)
                    {
                        cList_StS.RemoveRange(0, cList_StS.Count);
                        CXMLProcess.WriteXml(strStSAdr, cList_StS);
                    }
                }
            }

            // 조명 등 상태 초기화
            Seq.SeqIO.SetMachineState(eMachineState.READY);
        }

        /// <summary>
        /// MainLib Free
        /// </summary>
        public void Free()
        {
            if (Seq != null) Seq.Free();

            // DB Close
            SQLProduction.Close();
            SQLError.Close();
            SQLVision.Close();

            // Vision 종료
            for (int i = 0; i < Define.MAX_CAMERA; i++)
            {
                if (cCVisionToolBlockLib[i] != null) cCVisionToolBlockLib[i].VisionClose();
            }

            // 일정 날짜 지나면 로그 및 백업 데이터 자동 삭제 기능
            if (cOptionData.bAutoDeleteUse == true)
            {
                // 추가 확인이 필요한 기능 (D드라이브 용량 확인 후 파일 삭제하는 기능)
                //Parallel.Invoke(
                //    () => { CCommon.AutoDelete(CXMLProcess.LogPath); },
                //    () => { CCommon.AutoDelete(CXMLProcess.DataBackupPath); });

                Parallel.Invoke(
                        () => { CCommon.AutoDelete(CXMLProcess.LogPath, cOptionData.iAutoDeleteDays); },
                        () => { CCommon.AutoDelete(CXMLProcess.DataBackupPath, cOptionData.iAutoDeleteDays); });
            }

            // 이온 아이저 Off
            Seq.SeqIO.SetOutput((int)eIO_O.IONIZER_ONOFF, false);

            if (Define.SIMULATION == false)
            {
                // MotionControl Free
                if (RtexModule[0] != null) RtexModule[0].Free();
                if (RtexModule[1] != null) RtexModule[1].Free();
                for (int i = 0; i < UniModule.Length; i++)
                {
                    if (UniModule[i] != null) UniModule[i].Free();
                }

                if (cLightCtl != null) cLightCtl.Close();
                if (AIM_Feeder != null) AIM_Feeder.Free();
            }

            // I/O 해제
            IO.Free();

            CogFrameGrabbers FG = new CogFrameGrabbers();
            foreach (ICogFrameGrabber IFG in FG)
            {
                IFG.Disconnect(false);
            }
        }

        /// <summary>
        /// 클래스 생성 및 초기화
        /// </summary>
        private void ClassInit()
        {
            // Log 생성
            NLogger.Init();

            Axis = new Dictionary<eMotor, MotionProcMgr>();
            RtexModule[0] = new eMotionTekRTEX();
            RtexModule[1] = new eMotionTekRTEX();
            for (int i = 0; i < UniModule.Length; i++)
            {
                UniModule[i] = new eMotionTekUNI();
            }

            // IO 인터페이스 클래스 생성
            // (IO기기에 맞게 클래스 생성 후 IO에 매칭한다. IOProc.cs에 정의)
            Uz_IO_Module = new UZ_IO_Module();
            IO = Uz_IO_Module;

            // 정지 타이머 초기화
            cStopCheckTimer.Interval = TimeSpan.FromMilliseconds(200);            // 시간 간격 설정
            cStopCheckTimer.Tick += new EventHandler(StopTimer_Tick);             // 이벤트 추가

            cErrorStopCheckTimer.Interval = TimeSpan.FromMilliseconds(200);       // 시간 간격 설정
            cErrorStopCheckTimer.Tick += new EventHandler(ErrorStopTimer_Tick);   // 이벤트 추가
        }

        /// <summary>
        /// Axis Parameter 초기화
        /// </summary>
        private void AxisParameterInit()
        {
            bool bAxisParamCollFileExist = File.Exists(CXMLProcess.AxisParameterCollectionDataFilePath);

            for (int i = 0; i < Enum.GetValues(typeof(eMotor)).Length - 1; i++)
            {
                AxisParam axisParam = cAxisParamCollData.GetAxisParam((eMotor)i);
                AxisPositionData axisPositionData = cAxisPosCollData.GetAxisPositionData((eMotor)i);
                if (i <= (int)eMotor.MPC_FRONT_X)
                {
                    if (bAxisParamCollFileExist == false) axisParam.uiControllerAxis = (uint)i;
                    eMotionTekRtexAxis mt = new eMotionTekRtexAxis(RtexModule[0], axisParam, axisPositionData);
                    Axis.Add((eMotor)i, mt);
                }
                else if (i >= (int)eMotor.MPC_REAR_X &&
                         i <= (int)eMotor.DISPENSER_Y)
                {
                    if (bAxisParamCollFileExist == false) axisParam.uiControllerAxis = (uint)i - (uint)eMotor.MPC_REAR_X;
                    eMotionTekRtexAxis mt = new eMotionTekRtexAxis(RtexModule[1], axisParam, axisPositionData);
                    Axis.Add((eMotor)i, mt);
                }
                else if (i == (int)eMotor.PIPE_PnP_T ||
                         i == (int)eMotor.NEEDLE_PnP_T)
                {
                    if (bAxisParamCollFileExist == false) axisParam.uiControllerAxis = (uint)i - (uint)eMotor.PIPE_PnP_T;
                    eMotionTekUniAxis mt = new eMotionTekUniAxis(UniModule[0], axisParam, axisPositionData);
                    Axis.Add((eMotor)i, mt);
                }
                else if (i == (int)eMotor.PIPE_ROTATE ||
                         i == (int)eMotor.NEEDLE_ROTATE)
                {
                    if (bAxisParamCollFileExist == false) axisParam.uiControllerAxis = (uint)i - (uint)eMotor.PIPE_ROTATE;
                    eMotionTekUniAxis mt = new eMotionTekUniAxis(UniModule[1], axisParam, axisPositionData);
                    Axis.Add((eMotor)i, mt);
                }
                else if (i == (int)eMotor.FLIP_T ||
                         i == (int)eMotor.FLIP_Z)
                {
                    if (bAxisParamCollFileExist == false) axisParam.uiControllerAxis = (uint)i - (uint)eMotor.FLIP_T;
                    eMotionTekUniAxis mt = new eMotionTekUniAxis(UniModule[2], axisParam, axisPositionData);
                    Axis.Add((eMotor)i, mt);
                }
                else if (i == (int)eMotor.DISPENSER_X ||
                         i == (int)eMotor.DISPENSER_Z)
                {
                    if (bAxisParamCollFileExist == false) axisParam.uiControllerAxis = (uint)i - (uint)eMotor.DISPENSER_X;
                    eMotionTekUniAxis mt = new eMotionTekUniAxis(UniModule[3], axisParam, axisPositionData);
                    Axis.Add((eMotor)i, mt);
                }
                else if (i == (int)eMotor.PIPE_HOPPER ||
                         i == (int)eMotor.NEEDLE_HOPPER)
                {
                    if (bAxisParamCollFileExist == false) axisParam.uiControllerAxis = (uint)i - (uint)eMotor.PIPE_HOPPER;
                    eMotionTekUniAxis mt = new eMotionTekUniAxis(UniModule[4], axisParam, axisPositionData);
                    Axis.Add((eMotor)i, mt);
                }
            }
            if (bAxisParamCollFileExist == false) CXMLProcess.WriteXml(CXMLProcess.AxisParameterCollectionDataFilePath, cAxisParamCollData);
        }

        /// <summary>
        /// 에러 발생 표시
        /// </summary>
        /// <param name="eErrorCode"></param>
        public void AddError(eErrorCode eErrorCode, int iStepNo = -1)
        {
            if (cVar.iErrorCode != -1) return;
            cVar.iErrorCode = (int)eErrorCode;
            cVar.strErrorName = eErrorCode.ToString();

            // 모든 시퀀스 동작 정지 명령
            if (Seq != null && Seq.Worker != null) Seq.Stop();
            cErrorStopCheckTimer.Start();

            string strError = string.Format($"Error Code : {eErrorCode.ToString()}");
            NLogger.AddLog(eLogType.SEQ_MAIN, NLogger.eLogLevel.ERROR, strError, false);
            if (iStepNo != -1)
            {
                // 이전 Class명, 함수명
                string prevClassName = new StackTrace().GetFrame(1).GetMethod().ReflectedType.Name;
                string prevFuncName = new StackFrame(1, true).GetMethod().Name;
                strError = string.Format($"Error Func : {prevClassName + " " + prevFuncName}, Step No : {iStepNo}");
                NLogger.AddLog(eLogType.SEQ_MAIN, NLogger.eLogLevel.ERROR, strError, false);
            }
        }

        /// <summary>
        /// Error 정지 확인 타이머
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErrorStopTimer_Tick(object sender, EventArgs e)
        {
            if (Seq.GetAllStop() == true)
            {
                Seq.SeqIO.SetOutput((int)eIO_O.DOOR_SOLENOID_LOCK_ONOFF, false);
                Seq.SeqIO.SetOutput((int)eIO_O.SOFTREWE_BY_PASS_ON_OFF, true);

                Seq.MainTimerStop();
                // 장비 정지 시 IO 설정
                Seq.SeqIO.SetSystemStopIO();
                Parallel.Invoke(
                () =>
                {
                    CXMLProcess.WriteXml(CXMLProcess.RunUnitDataFilePath, cRunUnitData);
                },
                () =>
                {
                    CXMLProcess.WriteXml(CXMLProcess.SystemParameterSingleFilePath, cSysOne);
                },
                () =>
                {
                    CXMLProcess.WriteXml(CXMLProcess.SystemParameterCollectionDataFilePath, cSysParamCollData);
                },
                () =>
                {
                    // DB 알람 번호 저장
                    SetErrorLog(cVar.iErrorCode);
                    SetProductionDB(cVar.uiGoodOutUnitCount, cVar.uiNgOutUnitCount);
                    cVar.uiGoodOutUnitCount = 0;
                    cVar.uiNgOutUnitCount = 0;
                },
                () =>
                {
                    if (cOptionData.bAutoDeleteUse == true)
                    {
                        // 추가 확인이 필요한 기능 (D드라이브 용량 확인 후 파일 삭제하는 기능)
                        //CCommon.AutoDelete(CXMLProcess.BackupBasePath + @"VisionImage\");

                        CCommon.AutoDelete(CXMLProcess.BackupBasePath + @"VisionImage\", cOptionData.iAutoDeleteDays);
                    }
                });
                Seq.SeqIO.SetMachineState(eMachineState.ERROR);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Machine Error Run Stop.");
                cErrorStopCheckTimer.Stop();
            }
        }

        /// <summary>
        /// 알람 발생 표시
        /// </summary>
        /// <param name="eAlarmCode"></param>
        public void AddAlarm(eAlarmCode eAlarmCode)
        {
            if (McState == eMachineState.RUN)
            {
                // 알람 부저, 등 표시
                Seq.SeqIO.SetMachineState(eMachineState.ALARM);
                // 알람 처리 내용 추가
                //if (eAlarmCode == eAlarmCode.None)
                {
                    if (CCommon.ShowMessage(0, "None.") == (int)eMBoxRtn.A_OK)
                    {
                        Seq.SeqIO.SetBuzzer(false);
                    }
                }
            }
            string strError = string.Format("Alarm Code : {0}", (int)eAlarmCode);
            NLogger.AddLog(eLogType.SEQ_MAIN, NLogger.eLogLevel.WARN, strError, false);
        }

        /// <summary>
        /// 장비 Start
        /// </summary>
        public void McStart()
        {
            if (cVar.bInitializeComplete == false ||
                McState != eMachineState.READY) return;

            // 데이터 맵의 팝업창이 열려있으면 닫는다.
            cDataEditUIManager.PopupClose();
            SlideMainUI.Ins.PopupClose();
            VisionPopupHide();

            MapDataLib MPC1_Buffer2 = cRunUnitData.GetIndexData(eData.MPC1_BUFFER_2);
            MapDataLib MPC1_Dispensing = cRunUnitData.GetIndexData(eData.MPC1_DISPENSING);

            // Skip 후 맵데이터 변경 안되어 있으면 실행 안하고 메시지 띄운다.
            if (MPC1_Buffer2.GetStatus(eStatus.SKIPPED) || MPC1_Dispensing.GetStatus(eStatus.SKIP_AFTER_FLIP) == true)
            {
                CCommon.ShowMessageMini(0, "대기존2와 토출존의 Skip 맵데이터를 변경해주세요! ");
                return;
            }

            string strMessage = string.Empty;
            // 축 원점 검색 상태 확인
            if (Define.SIMULATION == false &&
                cVar.bInitializeMotor == false)
            {
                Seq.SeqIO.SetBuzzer(true);
                if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN)
                    strMessage = string.Format("모터 원점 검색을 다시 실행해주세요! ");
                else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH)
                    strMessage = string.Format("Please, Motor Origin Search Again !");

                CCommon.ShowMessageMini(strMessage);
                Seq.SeqIO.SetBuzzer(false);
                return;
            }

            // 도어락 체크
            if (cOptionData.bDoorLockUse == true)
            {
                if (Seq.SeqIO.GetInput((int)eIO_I.DOOR_SWITCH_1_CLOSE_CHECK_DOOR_CLOSE_ON_FRONT_RIGHT) == false ||
                    Seq.SeqIO.GetInput((int)eIO_I.DOOR_SWITCH_2_CLOSE_CHECK_DOOR_LOCK_ON_FRONT_CENTER_RIGHT) == false ||
                    Seq.SeqIO.GetInput((int)eIO_I.DOOR_SWITCH_3_CLOSE_CHECK_DOOR_CLOSE_ON_FRONT_CENTER_LEFT) == false ||
                    Seq.SeqIO.GetInput((int)eIO_I.DOOR_SWITCH_4_CLOSE_CHECK_DOOR_CLOSE_ON_REAR_LEFT) == false ||
                    Seq.SeqIO.GetInput((int)eIO_I.DOOR_SWITCH_5_CLOSE_CHECK_DOOR_CLOSE_ON_REAR_RIGHT) == false ||
                    Seq.SeqIO.GetInput((int)eIO_I.DOOR_SWITCH_6_CLOSE_CHECK_DOOR_CLOSE_ON_FRONT_LEFT) == false)
                {
                    CCommon.ShowMessageMini(0, "Door가 열려있는 곳이 있습니다. 확인해주세요.");
                    return;
                }

                Seq.SeqIO.SetOutput((int)eIO_O.DOOR_SOLENOID_LOCK_ONOFF, true);
                Seq.SeqIO.SetOutput((int)eIO_O.SOFTREWE_BY_PASS_ON_OFF, false);
                Thread.Sleep(500);
                if (Seq.SeqIO.GetInput((int)eIO_I.DOOR_SOLENOID_1_LOCK_CHECK_DOOR_LOCK_ON_FRONT_RIGHT) == false ||
                    Seq.SeqIO.GetInput((int)eIO_I.DOOR_SOLENOID_2_LOCK_CHECK_DOOR_LOCK_ON_FRONT_CENTER_RIGHT) == false ||
                    Seq.SeqIO.GetInput((int)eIO_I.DOOR_SOLENOID_3_LOCK_CHECK_DOOR_LOCK_ON_FRONT_CENTER_LEFT) == false ||
                    Seq.SeqIO.GetInput((int)eIO_I.DOOR_SOLENOID_4_LOCK_CHECK_DOOR_LOCK_ON_REAR_LEFT) == false ||
                    Seq.SeqIO.GetInput((int)eIO_I.DOOR_SOLENOID_5_LOCK_CHECK_DOOR_LOCK_ON_REAR_RIGHT) == false ||
                    Seq.SeqIO.GetInput((int)eIO_I.DOOR_SOLENOID_6_LOCK_CHECK_DOOR_LOCK_ON_FRONT_LEFT) == false ||
                    Seq.SeqIO.GetInput((int)eIO_I.INTERLOCK_BY_PASS_ON_SOFTWERE) == true ||
                    Seq.SeqIO.GetInput((int)eIO_I.INTERLOCK_BY_PASS_OFF_SOFTWERE) == false)
                {
                    Seq.SeqIO.SetOutput((int)eIO_O.DOOR_SOLENOID_LOCK_ONOFF, false);
                    Seq.SeqIO.SetOutput((int)eIO_O.SOFTREWE_BY_PASS_ON_OFF, true);
                    CCommon.ShowMessageMini(0, "Door Lock이 되지 않았습니다. 확인해주세요.");
                    return;
                }
            }
            else
            {
                if (CCommon.ShowMessageMini(2, "Door Look이 해제 중입니다. 안전에 유의하세요.") != (int)eMBoxRtn.A_OK) return;
                Seq.SeqIO.SetOutput((int)eIO_O.DOOR_SOLENOID_LOCK_ONOFF, false);
                Seq.SeqIO.SetOutput((int)eIO_O.SOFTREWE_BY_PASS_ON_OFF, true);
            }

            // 설비 시작 전 홀더 존재 여부 확인
            if ((cRunUnitData.GetIndexData(eData.MPC1_FAR_LEFT).GetAllStatus(eStatus.EMPTY, false) == true || cRunUnitData.GetIndexData(eData.MPC1_FAR_LEFT).GetAllStatus(eStatus.NONE) == true) &&
                 cRunUnitData.GetIndexData(eData.MPC1_BUFFER_1).GetStatus(eStatus.EMPTY) == true &&
                 cRunUnitData.GetIndexData(eData.MPC1_PIPE_MOUNT).GetAllStatus(eStatus.EMPTY, false) == true &&
                 cRunUnitData.GetIndexData(eData.MPC1_BUFFER_2).GetStatus(eStatus.EMPTY) == true &&
                 cRunUnitData.GetIndexData(eData.MPC1_NEEDLE_MOUNT).GetAllStatus(eStatus.EMPTY, true) == true &&
                 cRunUnitData.GetIndexData(eData.MPC1_FLIP).GetStatus(eStatus.EMPTY) == true &&
                 cRunUnitData.GetIndexData(eData.MPC1_DISPENSING).GetStatus(eStatus.EMPTY) == true &&
                (cRunUnitData.GetIndexData(eData.MPC1_FAR_RIGHT).GetStatus(eStatus.EMPTY) == true || cRunUnitData.GetIndexData(eData.MPC1_FAR_RIGHT).GetStatus(eStatus.NONE) == true) &&
                (cRunUnitData.GetIndexData(eData.MPC2_FAR_LEFT).GetStatus(eStatus.EMPTY) == true || cRunUnitData.GetIndexData(eData.MPC2_FAR_LEFT).GetStatus(eStatus.NONE) == true) &&
                (cRunUnitData.GetIndexData(eData.MPC2_UV).GetStatus(eStatus.EMPTY) == true || cRunUnitData.GetIndexData(eData.MPC2_UV).GetStatus(eStatus.NONE) == true) &&
                (cRunUnitData.GetIndexData(eData.MPC2_FAR_RIGHT).GetStatus(eStatus.EMPTY) == true || cRunUnitData.GetIndexData(eData.MPC2_FAR_RIGHT).GetStatus(eStatus.NONE) == true))
            {
                if (cRunUnitData.GetIndexData(eData.SUPPLY_HOLDER_TRAY).GetStatus(eStatus.MOUNT) == false)
                {
                    CCommon.ShowMessageMini(0, "공급할 홀더가 없습니다.");
                    return;
                }
            }

            // Buzzer On 무조건
            // 2024.8.12일 고객사 요청으로 시작시 알람 On 안함
            //Seq.SeqIO.SetBuzzer(true);

            if (cOptionData.bDryRunUse == false)
            {
                if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = string.Format("자동운전을 시작합니다. 안전에 유의해주세요.");
                else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = string.Format("Machine Auto Run Start. Please, Your Safety.");
            }
            else
            {
                if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = string.Format("시운전을 시작합니다. 안전에 유의해주세요.");
                else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = string.Format("Machine Dry Run Start. Please, Your Safety.");
            }

            if (CCommon.ShowMessageMini(2, strMessage) == (int)eMBoxRtn.A_OK)
            {
                // 메인 화면이 아닐 때 메인 화면 변경
                if (CheckNowUseUI.CurrentUIIndex != _UIIndex.MainUI)
                {
                    CheckNowUseUI.CurrentUIIndex = _UIIndex.MainUI;
                    MainWindow main = (MainWindow)System.Windows.Application.Current.MainWindow;
                    main.MainPanel.Content = MainUI.Ins;
                }

                // 시작 시간 Init
                ResetTime();
                // 장비 시작 시 IO 설정
                Seq.SeqIO.SetSystemStartIO();
                Seq.SeqIO.SetMachineState(eMachineState.RUN);

                // CStart_to_Stop 클래스가 null 이면 여기서 스타트 한다.
                string strPath = @"D:\Start_to_Stop Data\";
                DirectoryInfo di = new DirectoryInfo(strPath);
                if (di.Exists == false) di.Create();
                if (cStart_to_Stop == null)
                {
                    cStart_to_Stop = new CStart_to_Stop();
                    cStart_to_Stop.Start_Time = DateTime.Now.ToString("HH:mm:ss");
                }

                // Log 기록
                if (cOptionData.bDryRunUse == false) NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Machine Auto Run Start.");
                else NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Machine Dry Run Start.");

                // 자동운전 시작
                Seq.Start();
            }
            else
            {
                Seq.SeqIO.SetOutput((int)eIO_O.DOOR_SOLENOID_LOCK_ONOFF, false);
                Seq.SeqIO.SetOutput((int)eIO_O.SOFTREWE_BY_PASS_ON_OFF, true);
            }
        }

        /// <summary>
        /// 장비 Maunal Start
        /// </summary>
        public void ManualStart(eSequence SeqType)
        {
            if (cVar.bInitializeComplete == false ||
                McState != eMachineState.READY) return;

            string strMessage = string.Empty;

            if (Define.SIMULATION == false &&
                cVar.bInitializeMotor == false)
            {
                Seq.SeqIO.SetBuzzer(true);
                if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN)
                    strMessage = string.Format("원점 검색을 다시 실행해주세요! ");
                else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH)
                    strMessage = string.Format("Please, Motor Origin Search Again !");

                CCommon.ShowMessageMini(strMessage);
                Seq.SeqIO.SetBuzzer(false);
                return;
            }
            // 장비 시작 시 IO 설정
            Seq.SeqIO.SetSystemStartIO();
            Seq.SeqIO.SetMachineState(eMachineState.MANUALRUN);
            NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Machine Manual Start");
            // 데이터 맵의 팝업창이 열려있으면 닫는다.
            cDataEditUIManager.PopupClose();
            VisionPopupHide();
            Seq.ManualStart(SeqType);
        }

        /* Stop 확인용 타이머 */
        private DispatcherTimer cStopCheckTimer = new DispatcherTimer();
        private DispatcherTimer cErrorStopCheckTimer = new DispatcherTimer();

        /// <summary>
        /// 장비 Stop
        /// </summary>
        public void McStop()
        {
            if (McState == eMachineState.INIT ||
                McState == eMachineState.READY ||
                McState == eMachineState.ERROR)
            {
                return;
            }
            // 모든 시퀀스 동작 정지 명령
            Seq.Stop();
            // 타이머로 스레드 모두 정지 확인하여 정지 상태 변경
            cStopCheckTimer.Start();  // 타이머 시작
        }

        /// <summary>
        /// 정지 확인 타이머
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopTimer_Tick(object sender, EventArgs e)
        {
            if (Seq.GetAllStop() == true)
            {
                Seq.SeqIO.SetOutput((int)eIO_O.DOOR_SOLENOID_LOCK_ONOFF, false);
                Seq.SeqIO.SetOutput((int)eIO_O.SOFTREWE_BY_PASS_ON_OFF, true);

                for (int i = 0; i < Define.MAX_CAMERA; i++)
                {
                    if (cVisionToolBlockUI[i] != null)
                    {
                        cVisionToolBlockUI[i].ButtonEnable(true);
                        cVisionToolBlockUI[i].SetDisplayMode(false); // 기존 true
                    }
                }

                Parallel.Invoke(
                () =>
                {
                    CXMLProcess.WriteXml(CXMLProcess.RunUnitDataFilePath, cRunUnitData);
                },
                () =>
                {
                    CXMLProcess.WriteXml(CXMLProcess.SystemParameterSingleFilePath, cSysOne);
                },
                () =>
                {
                    CXMLProcess.WriteXml(CXMLProcess.SystemParameterCollectionDataFilePath, cSysParamCollData);
                },
                () =>
                {
                    // DB 저장
                    //SetProductionDB(cVar.iGoodOutUnitCount, cVar.iNgOutUnitCount);
                    //cVar.iGoodOutUnitCount = 0;
                    //cVar.iNgOutUnitCount = 0;
                });

                // 추가 확인이 필요한 기능 (D드라이브 용량 확인 후 파일 삭제하는 기능)
                //Parallel.Invoke(() => { CCommon.CheckDataBackUp(); },
                //            () => { if (cOptionData.bAutoDeleteUse == true) CCommon.AutoDelete(CXMLProcess.LogPath); },
                //            () => { if (cOptionData.bAutoDeleteUse == true) CCommon.AutoDelete(CXMLProcess.DataBackupPath); },
                //            () => { if (cOptionData.bAutoDeleteUse == true) CCommon.AutoDelete(CXMLProcess.BackupBasePath + @"VisionImage\"); });

                Parallel.Invoke(() => { CCommon.CheckDataBackUp(); },
                                () => { if (cOptionData.bAutoDeleteUse == true) CCommon.AutoDelete(CXMLProcess.LogPath, cOptionData.iAutoDeleteDays); },
                                () => { if (cOptionData.bAutoDeleteUse == true) CCommon.AutoDelete(CXMLProcess.DataBackupPath, cOptionData.iAutoDeleteDays); },
                                () => { if (cOptionData.bAutoDeleteUse == true) CCommon.AutoDelete(CXMLProcess.BackupBasePath + @"VisionImage\", cOptionData.iAutoDeleteDays); });

                Seq.MainTimerStop();
                // 장비 정지 시 IO 설정
                Seq.SeqIO.SetSystemStopIO();
                if (McState == eMachineState.MANUALRUN)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Machine Manual Run Stop.");
                    Seq.SeqIO.SetMachineState(eMachineState.READY);
                }
                else if (McState == eMachineState.ERROR)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Machine Error Run Stop.");
                }
                else
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Machine Run Stop.");
                    Seq.SeqIO.SetMachineState(eMachineState.READY);
                }

                // 설비 정지 완료시 Stop시간, Total시간, Error시간, 샘플 카운트를 입력 후 csv파일 확장자로 저장한다.
                if (cStart_to_Stop != null)
                {
                    CreatStarttoStopData();
                }

                cStopCheckTimer.Stop();
            }
        }

        /// <summary>
        /// 장비 Reset
        /// </summary>
        public void McReset()
        {
            Seq.SeqIO.SetBuzzer(false);

            if (cVar.bInitializeComplete == false ||
                McState == eMachineState.RUN ||
                McState == eMachineState.MANUALRUN ||
                Seq.GetAllStop() == false)
            {
                return;
            }

            // 제어기 초기화
            for (int i = 0; i < Axis.Count; i++)
            {
                Axis[(eMotor)i].Reset();
            }

            // 설비 정지 완료시 Stop시간, Total시간, Error시간, 샘플 카운트를 입력 후 csv파일 확장자로 저장한다.
            if (cStart_to_Stop != null)
            {
                CreatStarttoStopData();
            }

            // 장비 정지 시 IO 설정
            Seq.SeqIO.SetSystemStopIO();
            Seq.SeqIO.SetMachineState(eMachineState.READY);
        }

        /// <summary>
        /// User Level Check
        /// </summary>
        /// <param name="iCutLevel"></param>
        /// <returns></returns>
        public bool GetUserLevelCheck(int iCutLevel)
        {
            bool bReturn = true;
            string strCurrentLevel, strMessage = string.Empty;

            // 현재 레벨
            if (cVar.iUserLevel == (int)eUserLevel.OPERATOR)
                strCurrentLevel = "OPERATOR";
            else if (cVar.iUserLevel == (int)eUserLevel.ADMINISTRATOR)
                strCurrentLevel = "ADMINISTRATOR";
            else if (cVar.iUserLevel == (int)eUserLevel.ENGINEER)
                strCurrentLevel = "ENGINEER";
            else
                strCurrentLevel = "OPERATOR";

            // 사용 가능 사용자 레벨은 확인!
            if (cVar.iUserLevel < iCutLevel)
            {
                if (cOptionData.iLanguageMode == (int)eLanguage.KOREAN) strMessage = string.Format("{0} 레벨 사용자는 사용할 수 없습니다.", strCurrentLevel);
                else if (cOptionData.iLanguageMode == (int)eLanguage.ENGLISH) strMessage = string.Format("{0} level did not Use function.", strCurrentLevel);
                CCommon.ShowMessage(strMessage);
                bReturn = false;
            }
            return bReturn;
        }

        /// <summary>
        /// 시간 정보를 초기화 한다.
        /// </summary>
        public void ResetTime()
        {
            cSysOne.DTRunning_Time = DateTime.MinValue;
            cSysOne.DTStop_Time = DateTime.MinValue;
            cSysOne.DTError_Time = DateTime.MinValue;
        }

        #region Vision Part 함수 정의

        /// <summary>
        /// Vision 초기화
        /// </summary>
        public bool VisionInit()
        {
            // 코그넥스 라이센스 확인
            bool bCognexLicense = true;
            try
            {
                CogLicense.CheckForExpiredVisionProLicenses();
            }
            catch
            {
                CCommon.ShowMessageMini("Cognex License Fail.");
                bCognexLicense = false;
            }

            // ToolBlock UI 및 Lib 생성
            for (int i = 0; i < Define.MAX_CAMERA; i++)
            {
                cVisionToolBlockUI[i] = new VisionToolBlockUI();
                cCVisionToolBlockLib[i] = new CVisionToolBlockLib();
                cCVisionToolBlockLib[i].Init(cVisionToolBlockUI[i], (uint)i, bCognexLicense);

                CVisionLight cVisionLight = cOptionData.GetVisionLight((uint)i);
                // 카메라 No별로 조명 채널 셋팅
                if (i == 0) cVisionLight.uiLightNo = 99;         // 1번 카메라 조명은 다른 디바이스에서 사용
                else if (i == 1) cVisionLight.uiLightNo = 1;     // 2번 카메라는 CH1
                else if (i == 2) cVisionLight.uiLightNo = 2;     // 3번 카메라는 CH2
                else if (i == 3) cVisionLight.uiLightNo = 3;     // 4번 카메라는 CH3
                else if (i == 4) cVisionLight.uiLightNo = 4;     // 5번 카메라는 CH4
                else if (i == 5) cVisionLight.uiLightNo = 5;     // 6번 카메라는 CH5
                                                                 // 조명 초기화
                if (cLightCtl != null)
                {
                    cLightCtl.SetLightOn(cVisionLight.uiLightNo, cVisionLight.uiLightValue);
                }
            }
            SlideVisionUI.Ins.CogInit(cVisionToolBlockUI);
            return true;
        }

        /// <summary>
        /// 사이드 바 등 화면 전환 클릭 시 Vision UI에 Popup이 생성되어 있을 경우 닫는다.
        /// </summary>
        public void VisionPopupHide()
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return;
            for (int i = 0; i < Define.MAX_CAMERA; i++)
            {
                //cVisionToolBlockUI[i].ButtonEnable(false);
                cVisionToolBlockUI[i].SetDisplayMode(false);
                // Vision Live Off
                cVisionToolBlockUI[i].CameraLiveOff();
                cVisionToolBlockUI[i].LogViewHide();
                cVisionToolBlockUI[i].SetupViewHide();
            }
        }

        /// <summary>
        /// 사이드 바 등 화면 전환 클릭 시 Vision UI에 Popup이 생성되어 있을 경우 닫는다.
        /// </summary>
        public void VisionColorDisable()
        {
            for (int i = 0; i < Define.MAX_CAMERA; i++)
            {
                if (cVisionToolBlockUI[i] != null)
                {
                    cVisionToolBlockUI[i].LogViewHide();
                    cVisionToolBlockUI[i].SetupViewHide();
                }
            }
        }

        /// <summary>
        /// Vision 화면 분할 Index 데이터를 갱신한다. (모델 변경 시 재 호출 필요)
        /// </summary>
        public void VisionDisplayIndexDataLoad()
        {
            for (int i = 0; i < Define.MAX_CAMERA; i++) cVisionToolBlockUI[i].DisplayIndex_DataLoad();
        }

        /// <summary>
        /// 스레드로 결과를 날릴 경우 통신 중복을 막기 위한 Lock
        /// </summary>
        private static object ResultLock = new object();

        /// <summary>
        /// Vision 데이터 처리
        /// </summary>
        /// <param name="uiCam"></param>
        /// <param name="uiToolBlockNo"></param>
        /// <param name="strSendMessage"></param>
        /// <param name="bResult"></param>
        public void VisionResultMessage(uint uiCam, uint uiToolBlockNo, string strSendMessage, bool bResult = false)
        {
            lock (ResultLock)
            {
                string strSendData = string.Empty;
                // CAM No, ToolBlcok No, Result, X Data, Y Data, T Data, ..., END @
                strSendData = String.Format("{0},{1},{2}@",
                                            uiCam.ToString(),
                                            uiToolBlockNo.ToString(),
                                            strSendMessage);
                // Status 리스트 박스에 메세지를 보낸다.
                NLogger.AddLog((eLogType)uiCam + 1, NLogger.eLogLevel.INFO, strSendData);
            }
        }

        /// <summary>
        /// 각 Vision 화면 표시 카운트 초기화
        /// </summary>
        /// <param name="eCAM"></param>
        public void ResetIndexDisplay(eCAM eCAM)
        {
            cVisionToolBlockUI[(int)eCAM].ResetIndexCogDisplay();
        }

        #endregion Vision Part 함수 정의

        /// <summary>
        /// 양산 그래프의 데이터 값 증가.
        /// bool True, int Amount
        /// </summary>
        /// <param name="uiGood"></param>
        /// <param name="uiNg"></param>
        public void SetProductionDB(uint uiGood, uint uiNg)
        {
            if (uiGood == 0 && uiNg == 0) return;
            DateTime dt = DateTime.Now;

            string Total_STrt = String.Format(dt.ToString("yyyy-MM-dd HH:mm"));
            string Total_END = String.Format(dt.AddMinutes(1).ToString("yyyy-MM-dd HH:mm"));

            uint uiTotal = uiGood + uiNg;

            string strTableNam = "MassProduction";
            string Val_Check = "SELECT COUNT(*) FROM " + strTableNam + " WHERE Start BETWEEN ( '" + Total_STrt + "') And ( '" + Total_END + "')";
            string Val_Add = "INSERT OR REPLACE INTO " + strTableNam + " VALUES ('" + Total_STrt + "', '"
                                                                  + Total_END + "','"
                                                                  + uiTotal.ToString() + "','"
                                                                  + uiGood.ToString() + "','"
                                                                  + uiNg.ToString() + "' )";

            string Get_Val = "SELECT * FROM " + strTableNam + " WHERE Start BETWEEN ( '" + Total_STrt + "') And ( '" + Total_END + "')";

            try
            {
                SQLiteCommand Number = new SQLiteCommand(Val_Check, SQLProduction);
                SQLiteCommand Get_Val_comm = new SQLiteCommand(Get_Val, SQLProduction);

                int iNumber = Convert.ToInt32(Number.ExecuteScalar());

                if (iNumber == 0)
                {
                    SQLiteCommand Val_Create_comm = new SQLiteCommand(Val_Add, SQLProduction);
                    Val_Create_comm.ExecuteNonQuery();
                }
                else
                {
                    SQLiteDataReader read = Get_Val_comm.ExecuteReader();

                    while (read.Read()) // 값이 있으면 읽고 없으면 못읽음 따라서 없으면 그냥 바로 저장되서 업데이트 있으면 추가해서 업데이트
                    {
                        // 저장된 값
                        uint oT = Convert.ToUInt32(read["Total"]);
                        uint oG = Convert.ToUInt32(read["Good"]);
                        uint oNG = Convert.ToUInt32(read["Not_Good"]);

                        // 저장 값
                        uiGood = uiGood + oG;
                        uiNg = uiNg + oNG;
                        uiTotal = uiTotal + oT;

                        string Up_Tot = "UPDATE " + strTableNam + " SET Total = " + uiTotal.ToString() + " WHERE Start BETWEEN ( '" + Total_STrt + "') And ( '" + Total_END + "')";
                        string Up_G = "UPDATE " + strTableNam + " SET Good = " + uiGood.ToString() + " WHERE Start BETWEEN ( '" + Total_STrt + "') And ( '" + Total_END + "')";
                        string Up_NG = "UPDATE " + strTableNam + " SET Not_Good = " + uiNg.ToString() + " WHERE Start BETWEEN ( '" + Total_STrt + "') And ( '" + Total_END + "')";

                        SQLiteCommand Up_Tot_comm = new SQLiteCommand(Up_Tot, SQLProduction);
                        SQLiteCommand Up_G_comm = new SQLiteCommand(Up_G, SQLProduction);
                        SQLiteCommand Up_NG_comm = new SQLiteCommand(Up_NG, SQLProduction);

                        Up_Tot_comm.ExecuteNonQuery();
                        Up_G_comm.ExecuteNonQuery();
                        Up_NG_comm.ExecuteNonQuery();
                    }
                    read.Close();
                }
            }
            catch (Exception ex)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.FATAL, "양산그래프 대기시간" + ex.Message.ToString());
            }
            finally
            {
            }
        }

        /// <summary>
        /// 키엔스 바코드 리더 응답 데이터 1
        /// </summary>
        /// <param name="strReceiveData"></param>
        private void KeyenceBarcode1ReceivedData(string strReceiveData)
        {
            //DataText.Text = ("[" + m_reader.IpAddress + "][" + DateTime.Now + "]" + receivedData);
        }

        /// <summary>
        /// 키엔스 바코드 리더 응답 데이터 2
        /// </summary>
        /// <param name="strReceiveData"></param>
        private void KeyenceBarcode2ReceivedData(string strReceiveData)
        {
            //DataText.Text = ("[" + m_reader.IpAddress + "][" + DateTime.Now + "]" + receivedData);
        }

        /// <summary>
        /// 에러 DB 저장 로그
        /// </summary>
        /// <param name="iErrorCode"></param>
        public void SetErrorLog(int iErrorCode)
        {
            int iErrorCnt = 0;
            if (iErrorCode == -1) return;
            if (iErrorCode != 0) iErrorCnt = 1;

            DateTime dt = DateTime.Now;
            string Total_STrt = String.Format(dt.ToString("yyyy-MM-dd HH:mm:ss"));
            string Total_END = String.Format(dt.AddMinutes(1).ToString("yyyy-MM-dd HH:mm:ss"));

            string strTableNam = "Error";
            string Val_Check = "SELECT COUNT(*) FROM " + strTableNam + " WHERE DateTime BETWEEN ( '" + Total_STrt + "') And ( '" + Total_END + "')";
            string Val_Add = "INSERT OR REPLACE INTO " + strTableNam + " VALUES ('" + Total_STrt + "', '" + iErrorCode.ToString() + "','" + iErrorCnt.ToString() + "' )";
            string Get_Val = "SELECT * FROM " + strTableNam + " WHERE DateTime BETWEEN ( '" + Total_STrt + "') And ( '" + Total_END + "')";

            try
            {
                // 저장된 값이 있는지 없는지를 확인
                SQLiteCommand Val_Check_comm = new SQLiteCommand(Val_Check, SQLError);
                SQLiteCommand Val_Create_comm = new SQLiteCommand(Val_Add, SQLError);
                SQLiteCommand Get_Val_comm = new SQLiteCommand(Get_Val, SQLError);

                int iNumber = Convert.ToInt32(Val_Check_comm.ExecuteScalar());

                if (iNumber == 0)
                {
                    Val_Create_comm.ExecuteNonQuery();
                }
                else
                {
                    SQLiteDataReader read = Get_Val_comm.ExecuteReader();

                    while (read.Read())
                    {
                        // 저장된 값
                        int oCode = Convert.ToInt32(read["ErrorCode"]);
                        int oCodeCnt = Convert.ToInt32(read["Count"]);

                        iErrorCode = oCode;
                        oCodeCnt++;

                        string Up_Error = "UPDATE " + strTableNam + " SET ErrorCode = " + iErrorCode.ToString() + " WHERE DateTime BETWEEN ( '" + Total_STrt + "') And ( '" + Total_END + "')";
                        string Up_ErrorCnt = "UPDATE " + strTableNam + " SET Count = " + oCodeCnt.ToString() + " WHERE DateTime BETWEEN ( '" + Total_STrt + "') And ( '" + Total_END + "')";

                        SQLiteCommand Up_Error_comm = new SQLiteCommand(Up_Error, SQLError);
                        SQLiteCommand Up_Cnt_comm = new SQLiteCommand(Up_ErrorCnt, SQLError);

                        Up_Error_comm.ExecuteNonQuery();
                        Up_Cnt_comm.ExecuteNonQuery();
                    }
                    read.Close();
                }
            }
            catch (Exception ex)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.FATAL, ex.Message.ToString());
            }
            finally
            {
            }
        }

        /// <summary>
        /// 각 카메라의 기능 별 Good, NG DB 저장
        /// </summary>
        /// <param name="uiCamNo"></param>
        /// <param name="uiToolBlockNo"></param>
        /// <param name="bGoodOrNg"></param>
        /// <param name="bVisionOrBarcord"></param>
        public void SetDBData(uint uiCamNo, uint uiToolBlockNo, bool bGoodOrNg, bool bVisionOrBarcord = true)
        {
            string strCamName = string.Empty;
            string strFuncName = string.Empty;
            string Cam_Func = string.Empty;

            strCamName = ((eCAM)uiCamNo).ToString();
            strFuncName = strCamName + "_" + cVisionData.strToolBlockName[uiCamNo][uiToolBlockNo];
            Cam_Func = cVisionData.strToolBlockName[uiCamNo][uiToolBlockNo];

            DateTime dt = DateTime.Now;
            // Start_y + " " + Start_m;  시작 시점
            string Total_STrt = String.Format(dt.ToString("yyyy-MM-dd HH:mm"));
            // End_y + " " + End_m;  끝나는 시점
            string Total_END = String.Format(dt.AddMinutes(1).ToString("yyyy-MM-dd HH:mm"));

            int Good = 0, NGood = 0;
            if (bGoodOrNg == true) Good = 1;
            else NGood = 1;

            // Performance 테이블 내 값의 수 카운팅
            string Val_Check = "SELECT COUNT(*) FROM " + strFuncName + " WHERE Start BETWEEN ( '" + Total_STrt + "') And ( '" + Total_END + "')";
            string Val_Create = "INSERT INTO " + strFuncName + " VALUES ('" + strCamName + "', '"
                                                                            + Total_STrt + "', '"
                                                                            + Total_END + "','"
                                                                            + "1" + "','"
                                                                            + Good.ToString() + "','"
                                                                            + NGood.ToString() + "','"
                                                                            + Cam_Func + "' )";
            string Get_Val = "SELECT * FROM " + strFuncName + " WHERE Start BETWEEN ( '" + Total_STrt + "') And ( '" + Total_END + "')";

            try
            {
                // 저장된 값이 있는지 없는지를 확인
                SQLiteCommand Val_Check_comm = new SQLiteCommand(Val_Check, SQLVision);
                int Row_Cnt = Convert.ToInt32(Val_Check_comm.ExecuteScalar());

                if (Row_Cnt == 0)
                {
                    SQLiteCommand Val_Create_comm = new SQLiteCommand(Val_Create, SQLVision);
                    Val_Create_comm.ExecuteNonQuery();
                }
                else if (Row_Cnt != 0)
                {
                    SQLiteCommand Get_Val_comm = new SQLiteCommand(Get_Val, SQLVision);
                    SQLiteDataReader read = Get_Val_comm.ExecuteReader();
                    while (read.Read() == true)
                    {
                        // 저장된 값
                        int oT = Convert.ToInt32(read["Total"]);
                        int oG = Convert.ToInt32(read["Good"]);
                        int oNG = Convert.ToInt32(read["Not_Good"]);

                        // 저장 값
                        string strTotal = (oT + 1).ToString();
                        Good = Good + oG;
                        NGood = NGood + oNG;

                        string Up_Tot = "UPDATE " + strFuncName + " SET Total = " + strTotal + " WHERE Start BETWEEN ( '" + Total_STrt + "') And ( '" + Total_END + "')";
                        string Up_G = "UPDATE " + strFuncName + " SET Good = " + Good.ToString() + " WHERE Start BETWEEN ( '" + Total_STrt + "') And ( '" + Total_END + "')";
                        string Up_NG = "UPDATE " + strFuncName + " SET Not_Good = " + NGood.ToString() + " WHERE Start BETWEEN ( '" + Total_STrt + "') And ( '" + Total_END + "')";

                        SQLiteCommand Up_Tot_comm = new SQLiteCommand(Up_Tot, SQLVision);
                        SQLiteCommand Up_G_comm = new SQLiteCommand(Up_G, SQLVision);
                        SQLiteCommand Up_NG_comm = new SQLiteCommand(Up_NG, SQLVision);

                        Up_Tot_comm.ExecuteNonQuery();
                        Up_G_comm.ExecuteNonQuery();
                        Up_NG_comm.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.FATAL, ex.Message.ToString());
            }
            finally
            {
            }
        }

        /// <summary>
        /// Start to Stop 데이터를 csv 파일로 저장
        /// </summary>
        private void CreatStarttoStopData()
        {
            cStart_to_Stop.Stop_Time = DateTime.Now.ToString("HH:mm:ss");
            TimeSpan TotalSpan = DateTime.Parse(cStart_to_Stop.Stop_Time) - DateTime.Parse(cStart_to_Stop.Start_Time);
            cStart_to_Stop.Total_Time = TotalSpan.ToString();
            cStart_to_Stop.Run_Time = cSysOne.DTRunning_Time.ToString("HH:mm:ss");
            cStart_to_Stop.Error_Time = cSysOne.DTError_Time.ToString("HH: mm:ss");
            cStart_to_Stop.Product_Count = cSysOne.iProductCount.ToString();
            cList_StS.Add(cStart_to_Stop);
            cStart_to_Stop = null;

            string strPath = @"D:\Start_to_Stop Data\";
            DirectoryInfo di = new DirectoryInfo(strPath);

            if (di.Exists == true)
            {
                string strDate = DateTime.Now.ToString("yyyy-MM-dd");
                string NewfileCSV = strPath + strDate + ".csv";

                using (var sw = new StreamWriter(NewfileCSV))
                using (var cw = new CsvWriter(sw, CultureInfo.InvariantCulture))
                {
                    if (cList_StS.Count > 0)
                    {
                        cw.WriteRecords(cList_StS);
                    }
                }
            }

            string strStSAdr = CXMLProcess.DataFolderPath + @"\Start_to_Stop.xml";
            CXMLProcess.WriteXml(strStSAdr, cList_StS);
        }
    }
}