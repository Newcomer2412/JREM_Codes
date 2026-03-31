using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MachineControlBase
{
    /// <summary>
    /// 상수 정의
    /// </summary>
    public static class Define
    {
        /// <summary>
        /// 시스템 이름
        /// </summary>
        public static string SYSTEM_NAME = "NIM_Machine";

        /// <summary>
        /// 시뮬레이션 모드 설정
        /// </summary>
        public const bool SIMULATION = true;

        /// <summary>
        /// 시뮬레이션 실린더 동작 시간
        /// </summary>
        public const int iSimCylDelayTIme = 200;

        /// <summary>
        /// 카메라 수
        /// </summary>
        public const int MAX_CAMERA = 6;

        #region Motion & IO 관련 선언

        /* IO 보드 사용 전역변수 */
        public const int INPUT_TOTAL_BIT = 16 * 6; // Input 총 개 수
        public const int OUTPUT_TOTAL_BIT = 16 * 4; // Output 총 개 수
        public const int INPUT_DEFINE_BIT = 16;     // 프로그램 내부 나누는 정의 개 수
        public const int OUTPUT_DEFINE_BIT = 16;    // 프로그램 내부 나누는 정의 개 수

        #endregion Motion & IO 관련 선언

        #region 사용자 추가 선언

        public const long SEQ_TIME_OUT = 60000;

        #endregion 사용자 추가 선언
    }

    #region 자동운전 데이터

    /// <summary>
    /// 자동 운전 자재(UNIT) 데이터 - 장비의 각 파트의 유닛 데이터들을 저장한다.
    /// </summary>
    [Serializable]
    public class CRunUnitData
    {
        private static readonly object readLock = new object();

        /// <summary>
        /// 모든 자재 데이터 Map을 구성하는 배열 데이터
        /// </summary>
        public MapDataLib[] MapData = null;

        /// <summary>
        /// 생성자
        /// </summary>
        public CRunUnitData()
        {
        }

        /// <summary>
        /// 데이터 초기화(데이터 중복 생성을 막기 위해 확인하여 생성)
        /// </summary>
        public void init()
        {
            if (MapData == null)
            {
                MapData = new MapDataLib[Enum.GetValues(typeof(eData)).Length];
            }
            else if (MapData.Length != Enum.GetValues(typeof(eData)).Length)
            {
                MapData = null;
                MapData = new MapDataLib[Enum.GetValues(typeof(eData)).Length];
            }

            for (int i = 0; i < Enum.GetValues(typeof(eData)).Length; i++)
            {
                if (MapData[i] == null)
                {
                    MapData[i] = new MapDataLib();
                    MapData[i].strDataName = Enum.GetName(typeof(eData), i);
                }
            }
        }

        /// <summary>
        /// MapDataLib 데이터를 가져온다.
        /// </summary>
        /// <param name="eData"></param>
        public MapDataLib GetIndexData(eData eData)
        {
            lock (readLock)
            {
                return MapData[(int)eData];
            }
        }

        /// <summary>
        /// MPC1의 데이터를 한칸씩 쉬프트 한다.
        /// </summary>
        public void MPC1_MapDataShift()
        {
            lock (readLock)
            {
                MapData[(int)eData.MPC1_DISPENSING].MPC_DataCopy(MapData[(int)eData.MPC1_FAR_RIGHT]);
                MapData[(int)eData.MPC1_FLIP].MPC_DataCopy(MapData[(int)eData.MPC1_DISPENSING]);
                MapData[(int)eData.MPC1_NEEDLE_MOUNT].MPC_DataCopy(MapData[(int)eData.MPC1_FLIP]);
                MapData[(int)eData.MPC1_BUFFER_2].MPC_DataCopy(MapData[(int)eData.MPC1_NEEDLE_MOUNT]);
                MapData[(int)eData.MPC1_PIPE_MOUNT].MPC_DataCopy(MapData[(int)eData.MPC1_BUFFER_2]);
                MapData[(int)eData.MPC1_BUFFER_1].MPC_DataCopy(MapData[(int)eData.MPC1_PIPE_MOUNT]);
                MapData[(int)eData.MPC1_FAR_LEFT].MPC_DataCopy(MapData[(int)eData.MPC1_BUFFER_1]);
                MapData[(int)eData.MPC1_FAR_LEFT].SetAllStatus(eStatus.NONE);
            }
        }

        /// <summary>
        /// 모든 맵데이터를 초기화 한다.
        /// </summary>
        public void MapDataAllInit()
        {
            for (int i = 0; i < Enum.GetValues(typeof(eData)).Length - 1; i++)
            {
                MapData[i].MapDataReset();
            }
        }
    }

    #endregion 자동운전 데이터

    #region 시스템 정보

    /// <summary>
    /// 시스템 정보 - 장비 관련하여 모델별이 아닌 독립적 변수를 저장한다.
    /// </summary>
    [Serializable]
    public class CSystemParameterSingle
    {
        /// <summary>
        /// ENGINEER Password 값
        /// </summary>
        public string strEngineerPassword = string.Empty;

        /// <summary>
        /// ADMINISTRATOR Password 값
        /// </summary>
        public string strAdministratorPassword = string.Empty;

        /// <summary>
        /// Master Password 값
        /// </summary>
        public string strMasterPassword = "5961";

        /// <summary>
        /// 현재 선택되어져 있는 Model Number
        /// </summary>
        public uint uiCurrentModelNo = 0;

        /// <summary>
        /// Jam List Data
        /// </summary>
        public List<CAlarmData> ListAlarmData = new List<CAlarmData>();

        public int iAllAutoRatio = 100;
        public int iMotorTimeOut = 5000;             // 모터 동작 타임아웃 시간.
        public int iSensorCheckTimeOut = 5000;       // 실린더 동작 타임아웃 시간.
        public int iCylinderRepeatTime = 500;

        public int iUPHCount = 0;                    // UPH 카운트 값
        public DateTime DTRunning_Time;              // 가동 시간
        public DateTime DTError_Time;                // 에러 시간
        public DateTime DTStop_Time;                 // 정지 시간

        /* 비전 서클 파인드 센터 영역 계산값 */
        public double dSetCirclePipeAreaX = 0.0;
        public double dSetCirclePipeAreaY = 0.0;
        public double dSetCircleNeedleAreaX = 0.0;
        public double dSetCircleNeedleAreaY = 0.0;

        /* 비전에서 찾은 최초 19개의 홀의 센터 */
        public double[] dFindCircleCenterX = new double[19];
        public double[] dFindCircleCenterY = new double[19];

        /* 디스펜싱 작업 횟수 */
        public int iDispWorkCount = 0;
        public int iDispWorkLimitCount = 300;

        /// <summary>
        /// 디스펜서 Z축 클린 다운 딜레이 (sec)
        /// </summary>
        public uint uiCleanDelay = 1;

        /// <summary>
        /// 디스펜서 펌프 분주 딜레이 (sec)
        /// </summary>
        public double uiDispValveDelay = 10;

        /// <summary>
        /// 전자석 주입 딜레이
        /// </summary>
        public uint uiMagneticDelay = 3000;

        /// <summary>
        /// 홀더의 19개 Pin중 선택한 홀만 작업하는 기능
        /// </summary>
        public readonly int[] bHolderNeedlePinWorkCount = new int[19]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
        };

        /// <summary>
        /// 설비 Start to Stop 기능 날짜 저장 데이터
        /// </summary>
        public string strDate = string.Empty;

        /// <summary>
        /// 홀더 카운트
        /// </summary>
        public int iProductCount = 0;

        /// <summary>
        /// 생성자
        /// </summary>
        public CSystemParameterSingle()
        {
        }
    }

    #endregion 시스템 정보

    #region 모델 별 시스템 동작 관련 변수

    /// <summary>
    /// 모델별 시스템 관련 변수
    /// </summary>
    [Serializable]
    public class CSystemParameterArray : ICloneable
    {
        /// <summary>
        /// 모델 번호
        /// </summary>
        public uint uiModelNo = 0;

        /// <summary>
        /// Model Name 을 저장한다.
        /// </summary>
        public string strModelName = string.Empty;

        /* 제품 생산 정보! (전체) */
        public int iTotalCount = 0;                                 // 토탈 카운트
        public int iNgCount = 0;                                    // NG 카운트

        /// <summary>
        /// 얼라인 측정 각도 계산을 위한 튜브의 반지름 값
        /// </summary>
        public double dTube_Radius = 7.5;

        /* 카메라 Offset 관련 */
        public double dCAM0_PipePnP_X_CameraCal = 0.0;
        public double dCAM0_PipePnP_Y_CameraCal = 0.0;
        public double dCAM0_NeedlePnP_X_CameraCal = 0.0;
        public double dCAM0_NeedlePnP_Y_CameraCal = 0.0;

        /* 캘리브레이션 Limit 파라메터 */
        public double dCAM3_PipeMount_X_CameraCal = 0.1;
        public double dCAM3_PipeMount_Y_CameraCal = 0.1;
        public double dCAM4_NeedleMount_X_CameraCal = 0.1;
        public double dCAM4_NeedleMount_Y_CameraCal = 0.1;
        public double dCAM3_PipeMountCal_X_Limit = 0.1;
        public double dCAM3_PipeMountCal_Y_Limit = 0.1;
        public double dCAM4_NeedleMountCal_X_Limit = 0.1;
        public double dCAM4_NeedleMountCal_Y_Limit = 0.1;
        public double dCAM5_Disp_X_CameraCal = 1.0;
        public double dCAM5_Disp_Y_CameraCal = 1.0;

        /* 트랜스퍼 파이프 & 니들 자세 확인, 니들 뾰족한 부분 확인 */
        public double dCAM1_TransferPipeDegreeLimit = 1.0;
        public double dCAM2_TransferNeedleDegreeLimit = 1.0;
        public double dCAM2_TransferNeedleSharpGap = 2.0;

        /// <summary>
        /// 파이프 PnP 캘리브레이션 완료 여부
        /// </summary>
        public bool bPipePnPCalDone = false;

        /// <summary>
        /// 니들 PnP 캘리브레이션 완료 여부
        /// </summary>
        public bool bNeedlePnPCalDone = false;

        /// <summary>
        /// 파이프 마운터 캘리브레이션 완료 여부
        /// </summary>
        public bool bPipeMounterCalDone = false;

        /// <summary>
        /// 니들 마운터 캘리브레이션 완료 여부
        /// </summary>
        public bool bNeedleMounterCalDone = false;

        /// <summary>
        /// 디스펜서 XY축 캘리브레이션 완료 여부
        /// </summary>
        public bool bDispenserXYCalDone = false;

        /// <summary>
        /// 디스펜서 Z축 캘리브레이션 완료 여부
        /// </summary>
        public bool bDispenserZCalDone = false;

        /// <summary>
        /// 홀더 클램프 다운 딜레이
        /// </summary>
        public uint uiHolderClampDownDelay = 500;

        /// <summary>
        /// 홀더 언클램프 다운 딜레이
        /// </summary>
        public uint uiHolderUnClampDownDelay = 500;

        /// <summary>
        /// UV 큐어 딜레이 (sec)
        /// </summary>
        public double dUVCureDelay = 30;

        /// <summary>
        /// UV 파워
        /// </summary>
        public uint uiUVPower = 80;

        /// <summary>
        /// 피더에 놓여진 파이프 n개 이상이어야 한다
        /// </summary>
        public uint uiFeederPipeCount = 1;

        /// <summary>
        /// 피더에 놓여진 니들 n개 이상이어야 한다
        /// </summary>
        public uint uiFeederNeedleCount = 1;

        /// <summary>
        /// 호퍼 모터 파이프 동작 시간 (mm)
        /// </summary>
        public uint uiHopperPipeRunDelay = 5000;

        /// <summary>
        /// 호퍼 모터 니들 동작 시간 (mm)
        /// </summary>
        public uint uiHopperNeedleRunDelay = 5000;

        /// <summary>
        /// 파이프 PnP 벅큠 딜레이
        /// </summary>
        public uint uiPnPPipeVacuumDelay = 1000;

        /// <summary>
        /// 니들 PnP 벅큠 딜레이
        /// </summary>
        public uint uiPnPNeedleVacuumDelay = 1000;

        /// <summary>
        /// 파이프 & 니들 트렌스퍼 벅큠 딜레이
        /// </summary>
        public uint uiTransferVacuumDelay = 1000;

        /// <summary>
        /// 파이프 클램프 딜레이
        /// </summary>
        public uint uiPipeClampUpDelay = 1000;

        /// <summary>
        /// 니들 클램프 진공 딜레이
        /// </summary>
        public uint uiNeedleClampVacuumDelay = 1000;

        /// <summary>
        /// 플립 그리퍼 Close 딜레이
        /// </summary>
        public uint uiFlipGripperCloseDelay = 5000;

        /// <summary>
        /// AIM 피더 동작 관련 파라메터
        /// [0]:Stroke(1~7mm), [1]:주파수(1~90Hz), [2]:Time,
        /// </summary>
        public uint[] uiFeederRunParam = new uint[3] { 2, 20, 2 };

        /// <summary>
        /// Platform List Data
        /// </summary>
        public List<string> ListPlatform = new List<string>();

        /// <summary>
        /// 생성자
        /// </summary>
        public CSystemParameterArray()
        {
        }

        /// <summary>
        /// Deep Copy
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            CSystemParameterArray copy = new CSystemParameterArray();
            copy = CCommon.DeepClone(this);
            return copy;
        }
    }

    /// <summary>
    /// 모든 모델 시스템 관련 변수
    /// </summary>
    [Serializable]
    public class CSystemParameterCollectionData
    {
        /// <summary>
        /// SystemParameterArray 저장 리스트
        /// </summary>
        public List<CSystemParameterArray> cSystemParameterList = new List<CSystemParameterArray>();

        /// <summary>
        /// CSystemParameterArray Data를 Model 별로 정렬
        /// </summary>
        public void OrderByModel()
        {
            cSystemParameterList = cSystemParameterList.OrderBy(x => x.uiModelNo).ToList();
        }

        /// <summary>
        /// Model 데이터가 있는지 확인
        /// </summary>
        /// <param name="uiModel"></param>
        /// <returns></returns>
        public bool CheckSysArray(uint uiModel)
        {
            CSystemParameterArray cSystemParameterArray = cSystemParameterList.Find(x => x.uiModelNo == uiModel);
            if (cSystemParameterArray == null) return false;
            else return true;
        }

        /// <summary>
        /// 현재 사용 중인 Model SystemParameterArray를 가져옴
        /// </summary>
        /// <returns></returns>
        public CSystemParameterArray GetSysArray()
        {
            CSystemParameterArray cSystemParameterArray = cSystemParameterList.Find(x => x.uiModelNo == CMainLib.Ins.cSysOne.uiCurrentModelNo);
            if (cSystemParameterArray == null)
            {
                cSystemParameterArray = new CSystemParameterArray();
                cSystemParameterArray.uiModelNo = CMainLib.Ins.cSysOne.uiCurrentModelNo;
                cSystemParameterList.Add(cSystemParameterArray);
            }
            return cSystemParameterArray;
        }

        /// <summary>
        /// Model 번호를 이용해 SystemParameterArray를 가져옴
        /// </summary>
        /// <param name="uiModelNo"></param>
        /// <returns></returns>
        public CSystemParameterArray GetSysArray(uint uiModelNo)
        {
            CSystemParameterArray cSystemParameterArray = cSystemParameterList.Find(x => x.uiModelNo == uiModelNo);
            if (cSystemParameterArray == null)
            {
                cSystemParameterArray = new CSystemParameterArray();
                cSystemParameterArray.uiModelNo = uiModelNo;
                cSystemParameterList.Add(cSystemParameterArray);
            }
            return cSystemParameterArray;
        }

        /// <summary>
        /// SystemParameterArray 데이터 삭제
        /// </summary>
        /// <param name="uiTarget"></param>
        public void DataClear(uint uiTarget)
        {
            CSystemParameterArray cSystemParameterArray = cSystemParameterList.Find(x => x.uiModelNo == uiTarget);
            if (cSystemParameterList != null) cSystemParameterList.Remove(cSystemParameterArray);
        }

        /// <summary>
        /// System Parameter 데이터 복사
        /// </summary>
        /// <param name="uiTarget"></param>
        /// <param name="uiSource"></param>
        public void DataCopy(uint uiTarget, uint uiSource)
        {
            DataClear(uiTarget);
            CSystemParameterArray cSystemParameterArray = cSystemParameterList.Find(x => x.uiModelNo == uiSource);
            CSystemParameterArray cNewSystemParameterArray = cSystemParameterArray.Clone() as CSystemParameterArray;
            cNewSystemParameterArray.uiModelNo = uiTarget;
            cSystemParameterList.Add(cNewSystemParameterArray);
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public CSystemParameterCollectionData()
        {
        }
    }

    #endregion 모델 별 시스템 동작 관련 변수

    #region 사용자 정의 변수

    /// <summary>
    /// COptionData 정의 변수
    /// </summary>
    [Serializable]
    public class COptionData
    {
        /// <summary>
        /// 형광등을 사용
        /// </summary>
        public bool bFluorescentUse = false;

        /// <summary>
        /// 부저의 사용 유무.
        /// </summary>
        public bool bBuzzerUse = false;

        /// <summary>
        /// 프로그램 시작 시 모터 홈밍을 수행합니다.
        /// </summary>
        public bool bOriginInitialize = false;

        /// <summary>
        /// DryRun Mode로 실행
        /// </summary>
        public bool bDryRunUse = false;

        /// <summary>
        /// 언어를 선택
        /// </summary>
        public int iLanguageMode = 0;

        /// <summary>
        /// 자동 로그 및 데이터 삭제 기능 사용 유무
        /// </summary>
        public bool bAutoDeleteUse = false;

        /// <summary>
        /// 자동 로그 및 데이터 삭제 기준일
        /// </summary>
        public int iAutoDeleteDays = 30;

        /// <summary>
        /// Backup을 할 날짜
        /// </summary>
        public string strBackupDate = string.Empty;

        /// <summary>
        /// Backup date index
        /// </summary>
        public int iBackupDateIndex = 0;

        /// <summary>
        /// 비교할 날짜- 매일 백업 시 필요
        /// </summary>
        public string strCompareBackupDate = string.Empty;

        /// <summary>
        /// Auto Backup 사용 유무
        /// </summary>
        public bool bAutoBackupUse = false;

        /// <summary>
        /// Door Lock 사용 유무
        /// </summary>
        public bool bDoorLockUse = false;

        /// <summary>
        /// 알람 발생 시 부저 On 유지 시간 설정 사용 유무
        /// </summary>
        public bool bAlarmTimeUse = false;

        /// <summary>
        /// 알람 발생 시 부저 On 유지 시간
        /// </summary>
        public int iAlarmTime = 0;

        /// <summary>
        /// 카메라에서 촬영한 모든 이미지 저장 여부
        /// </summary>
        public bool bImage_SaveUse = false;

        /// <summary>
        /// 카메라에서 촬영한 NG 이미지 저장 여부
        /// </summary>
        public bool bImage_NGSaveUse = false;

        /// <summary>
        /// 조명 컨트롤러 Serial 사용 유무
        /// </summary>
        public bool bLightSerialUse = false;

        /// <summary>
        /// 조명 컨트롤러 Serial Com Port
        /// </summary>
        public string strLightSerialComPort = "COM1";

        /// <summary>
        /// 홀더 비전검사(파이프, 니들 유무) Skip 옵션
        /// </summary>
        public bool bHolderInspSkip = false;

        /// <summary>
        /// 디스펜싱 비전검사 시 파이프 유무 Skip 옵션
        /// </summary>
        public bool bDispensingPipeInspSkip = false;

        /// <summary>
        /// 니들 자세검사 한번 더 옵션 (True : 니들 자세검사 한번 더, False : 니들 자세검사 더 X)
        /// </summary>
        public bool bNeedlePosCheckTwice = false;

        /// <summary>
        /// Vision 모델 및 카메라 별 화면 분할 값 저장 리스트
        /// </summary>
        public List<CVisionView> cVisionViewList = new List<CVisionView>();

        /// <summary>
        /// Vision 조명 No 및 온오프, 파워 설정 값 저장 리스트
        /// </summary>
        public List<CVisionLight> cVisionLightList = new List<CVisionLight>();

        /// <summary>
        /// Vision Offset 값 저장 리스트
        /// </summary>
        public List<CCamOffset> cVisionOffsetList = new List<CCamOffset>();

        /// <summary>
        /// 파이프 삽입 Offset
        /// </summary>
        public List<PipeNeedleOffsetList> cVisionPipeMountOffsetList = new List<PipeNeedleOffsetList>();

        /// <summary>
        /// 니들 삽입 Offset
        /// </summary>
        public List<PipeNeedleOffsetList> cVisionNeedleMountOffsetList = new List<PipeNeedleOffsetList>();

        /// <summary>
        /// AIM 피더 I/P 주소
        /// </summary>
        public string strAIM_FeederIP = "192.168.1.10";

        /// <summary>
        /// UV 컨트롤러 컴포트
        /// </summary>
        public string strUV_ComPort = "COM2";

        private static object readLock = new object();

        /// <summary>
        /// Vision Display 표시 Index 값 리턴
        /// </summary>
        /// <param name="uiCameraNo"></param>
        /// <returns></returns>
        public CVisionView GetDisplayIndex(uint uiCameraNo)
        {
            lock (readLock)
            {
                CVisionView cVisionView = cVisionViewList.Find(x => x.uiModelNo == CMainLib.Ins.cSysOne.uiCurrentModelNo &&
                                                                    x.uiCameraNo == uiCameraNo);

                if (cVisionView == null)
                {
                    cVisionView = new CVisionView();
                    cVisionView.uiModelNo = CMainLib.Ins.cSysOne.uiCurrentModelNo;
                    cVisionView.uiCameraNo = uiCameraNo;
                    cVisionViewList.Add(cVisionView);
                }
                return cVisionView;
            }
        }

        /// <summary>
        /// Vision Display 표시 Max 값 리턴
        /// </summary>
        /// <param name="uiCameraNo"></param>
        /// <returns></returns>
        public uint GetDisplayMaxIndex(uint uiCameraNo)
        {
            lock (readLock)
            {
                CVisionView cVisionView = cVisionViewList.Find(x => x.uiModelNo == CMainLib.Ins.cSysOne.uiCurrentModelNo &&
                                                                    x.uiCameraNo == uiCameraNo);

                if (cVisionView == null)
                {
                    cVisionView = new CVisionView();
                    cVisionView.uiModelNo = CMainLib.Ins.cSysOne.uiCurrentModelNo;
                    cVisionView.uiCameraNo = uiCameraNo;
                    cVisionViewList.Add(cVisionView);
                }
                return cVisionView.uiDisplayMaxIndex;
            }
        }

        /// <summary>
        /// Vision Display X, Y 분할 Index 값 리턴
        /// </summary>
        /// <param name="uiCameraNo"></param>
        /// <param name="uiXIndex"></param>
        /// <param name="uiYIndex"></param>
        public void GetDisplayXYIndex(uint uiCameraNo, ref uint uiXIndex, ref uint uiYIndex)
        {
            lock (readLock)
            {
                CVisionView cVisionView = cVisionViewList.Find(x => x.uiModelNo == CMainLib.Ins.cSysOne.uiCurrentModelNo &&
                                                                    x.uiCameraNo == uiCameraNo);

                if (cVisionView == null)
                {
                    cVisionView = new CVisionView();
                    cVisionView.uiModelNo = CMainLib.Ins.cSysOne.uiCurrentModelNo;
                    cVisionView.uiCameraNo = uiCameraNo;
                    cVisionViewList.Add(cVisionView);
                }
                uiXIndex = cVisionView.uiDisplayXIndex;
                uiYIndex = cVisionView.uiDisplayYIndex;
            }
        }

        /// <summary>
        /// Vision Light 정보 클래스 리턴
        /// </summary>
        /// <param name="uiCameraNo"></param>
        /// <returns></returns>
        public CVisionLight GetVisionLight(uint uiCameraNo)
        {
            lock (readLock)
            {
                CVisionLight cVisionLight = cVisionLightList.Find(x => x.uiCameraNo == uiCameraNo);

                if (cVisionLight == null)
                {
                    cVisionLight = new CVisionLight();
                    cVisionLight.uiCameraNo = uiCameraNo;
                    cVisionLightList.Add(cVisionLight);
                }
                return cVisionLight;
            }
        }

        /// <summary>
        /// Vision Offset 정보 클래스 리턴
        /// </summary>
        /// <param name="uiCameraNo"></param>
        /// <param name="uiToolBlockNo"></param>
        /// <returns></returns>
        public CCamOffset GetVisionOffset(uint uiCameraNo, uint uiToolBlockNo)
        {
            lock (readLock)
            {
                CCamOffset cVisionOffset = cVisionOffsetList.Find(x => x.uiCameraNo == uiCameraNo &&
                                                                       x.uiToolBlockNo == uiToolBlockNo);
                if (cVisionOffset == null)
                {
                    cVisionOffset = new CCamOffset();
                    cVisionOffset.uiCameraNo = uiCameraNo;
                    cVisionOffset.uiToolBlockNo = uiToolBlockNo;
                    cVisionOffsetList.Add(cVisionOffset);
                }

                return cVisionOffset;
            }
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public COptionData()
        {
        }
    }

    /// <summary>
    /// Vision 화면 분할 설정 값 클래스
    /// </summary>
    [Serializable]
    public class CVisionView
    {
        /// <summary>
        /// Model No
        /// </summary>
        public uint uiModelNo = 0;

        /// <summary>
        /// Visoin Camera No
        /// </summary>
        public uint uiCameraNo = 0;

        /// <summary>
        /// Vision 각 카메라의 Display에서 몇 개의 Display에 표시할지 최대 개 수
        /// </summary>
        public uint uiDisplayMaxIndex = 1;

        /// <summary>
        /// Vision 각 카메라의 Display에서 X축 방향 몇 개의 Display에 표시 할지
        /// </summary>
        public uint uiDisplayXIndex = 1;

        /// <summary>
        /// Vision 각 카메라의 Display에서 Y축 방향 몇 개의 Display에 표시 할지
        /// </summary>
        public uint uiDisplayYIndex = 1;
    }

    /// <summary>
    /// Vision 조명 No 및 파워 설정
    /// </summary>
    [Serializable]
    public class CVisionLight
    {
        /// <summary>
        /// Visoin Camera No
        /// </summary>
        public uint uiCameraNo = 0;

        /// <summary>
        /// Light 시리얼 통신 번호
        /// </summary>
        public uint uiLightNo = 0;

        /// <summary>
        /// 조명의 켜고 꺼짐을 저장
        /// </summary>
        public bool bLightOnOff = true;

        /// <summary>
        /// 조명 밝기 저장 값
        /// </summary>
        public uint uiLightValue = 100;
    }

    /// <summary>
    /// Camera 기능별 Offset
    /// </summary>
    [Serializable]
    public class CCamOffset
    {
        /// <summary>
        /// ToolBlock No
        /// </summary>
        public uint uiToolBlockNo = 0;

        /// <summary>
        /// Visoin Camera No
        /// </summary>
        public uint uiCameraNo = 0;

        /// <summary>
        /// 홀더 넘버
        /// </summary>
        public uint uiHolderNo = 0;

        /// <summary>
        /// X Offset
        /// </summary>
        public double dXOffset = 0;

        /// <summary>
        /// Y Offset
        /// </summary>
        public double dYOffset = 0;

        /// <summary>
        /// T Offset
        /// </summary>
        public double dTOffset = 0;
    }

    /// <summary>
    /// 파이프, 니들 마운트 클래스
    /// </summary>
    [Serializable]
    public class PipeNeedleOffsetList
    {
        /// <summary>
        /// 홀더 넘버
        /// </summary>
        public uint uiHolderNo = 0;

        /// <summary>
        /// X Offset
        /// </summary>
        public double X_Offset = 0.0;

        /// <summary>
        /// Y Offset
        /// </summary>
        public double Y_Offset = 0.0;
    }

    #endregion 사용자 정의 변수

    #region 시스템 Run 시 사용하는 전역변수 (No Save Variable)

    /// <summary>
    /// 시스템 Run 시 사용하는 전역변수 (No Save Variable)
    /// </summary>
    public class CVar
    {
        /// <summary>
        /// 프로그램 사용자 레벨
        /// (0:OPERATOR, 1:ENGINEER, 2:ADMINISTRATOR)
        /// </summary>
        public int iUserLevel = 0;

        public bool bSystemUtilityIOCheck = false;                    // System Utility IO Check
        public bool bInitializeMotor = false;                         // 각 축 홈동작 초기화가 완료 되었다.
        public bool bInitializeComplete = false;                      // 초기화가 완료 되었다.
        public bool bMCPowerFlag = false;                             // MC Power 상태 Flag
        public bool bMainAirFlag = false;                             // Main Air 상태 Flag
        public bool bUsePipe = false;                              // 파이프 사용 플래그 (True : 파이프 사용 O, False : 파이프 사용 X)
        public bool bUniAxisErrorCheckOnce = false;                       // Uni로 제어하는 축 에러코드 한번만 받아오는 플래그
        public bool bSnetAxisErrorCheckOnce = false;                       // Snet으로 제어하는 축 에러코드 한번만 받아오는 플래그
        public bool bUniErrorCheckOnce = false;                       // Uni로 제어하는 제어기 에러코드 한번만 받아오는 플래그
        public bool bSnetErrorCheckOnce = false;                       // Snet으로 제어하는 제어기 에러코드 한번만 받아오는 플래그

        /// <summary>
        /// 매뉴얼 모션 UI에서 콤보박스 축 번호를 저장
        /// </summary>
        public int iManualMotionAxisNo = 0;

        /// <summary>
        /// 매뉴얼 IAI UI에서 콤보박스 축 번호를 저장
        /// </summary>
        public int iManualIAINo = 0;

        /// <summary>
        /// error code
        /// </summary>
        public int iErrorCode = -1;

        /// <summary>
        /// Error Name
        /// </summary>
        public string strErrorName = null;

        /// <summary>
        /// Good 배출 수량
        /// </summary>
        public uint uiGoodOutUnitCount = 0;

        /// <summary>
        /// NG 배출 수량
        /// </summary>
        public uint uiNgOutUnitCount = 0;

        /// <summary>
        /// U.P.H 스톱 워치
        /// </summary>
        public Stopwatch swUPH = new Stopwatch();

        /// <summary>
        /// Vision 카메라 결과 값 저장 리스트
        /// </summary>
        public List<CVisionResultData> cVisionResultDataList = new List<CVisionResultData>();

        /// <summary>
        /// Vision Result Data 정보 클래스 리턴
        /// </summary>
        /// <param name="uiCameraNo"></param>
        /// <param name="uiToolBlockNo"></param>
        /// <returns></returns>
        public CVisionResultData GetVisionResultData(uint uiCameraNo, uint uiToolBlockNo)
        {
            return cVisionResultDataList.Find(x => x.uiCamNo == uiCameraNo &&
                                                   x.uiBlockNo == uiToolBlockNo);
        }

        #region 매뉴얼 플래그 정의

        /// <summary>
        /// 매뉴얼 MPC1 Pitch 이동
        /// </summary>
        public bool Manual_MPC1_MovePitch = false;

        /// <summary>
        /// 매뉴얼 Holder Flip
        /// </summary>
        public bool Manual_HolderFlip = false;

        /// <summary>
        /// 매뉴얼 플립 모터 및 실린더 위치 초기화
        /// </summary>
        public bool Manual_FlipPosClear = false;

        /// <summary>
        /// 매뉴얼 MPC2 팔렛 맨 왼쪽으로 이동
        /// </summary>
        public bool Manual_MPC2_MoveFarLeft = false;

        /// <summary>
        /// 매뉴얼 맨 왼쪽 Y축 홀더 공급 위치로 이동
        /// </summary>
        public bool Manual_FarLeft_Y_MoveHolser = false;

        /// <summary>
        /// 매뉴얼 맨 왼쪽 Y축 MPC1 위치로 이동
        /// </summary>
        public bool Manual_FarLeft_Y_MoveMPC1 = false;

        /// <summary>
        /// 매뉴얼 맨 오른쪽 Y축 MPC2 위치로 이동
        /// </summary>
        public bool Manual_FarRight_Y_MoveMPC2 = false;

        /// <summary>
        /// 매뉴얼 맨 오른쪽 팔렛 UV존으로 이동
        /// </summary>
        public bool Manual_FarRightPalletsMoveUV = false;

        /// <summary>
        /// 매뉴얼 UV 큐어
        /// </summary>
        public bool Manual_UV_Cure = false;

        /// <summary>
        /// 매뉴얼 MPC 자동 간격이동 테스트
        /// </summary>
        public bool Manual_MPCAutoPitchMoveTest = false;

        /// <summary>
        /// 매뉴얼 빈 홀더 픽업
        /// </summary>
        public bool Manual_EmptyHolderPickUp = false;

        /// <summary>
        /// 매뉴얼 빈 홀더 공급
        /// </summary>
        public bool Manual_EmptyHolderSupply = false;

        /// <summary>
        /// 매뉴얼 작업 완료된 홀더 픽업
        /// </summary>
        public bool Manual_DoneHolderPickUp = false;

        /// <summary>
        /// 매뉴얼 작업 완료되 홀더 회수
        /// </summary>
        public bool Manual_DoneHolderRecovery = false;

        /// <summary>
        /// 매뉴얼 홀더 트레이 교환 위치로 이동
        /// </summary>
        public bool Manual_NgHolderRecovery = false;

        /// <summary>
        /// 매뉴얼 홀더 트레이 교환 위치로 이동
        /// </summary>
        public bool Manual_HolderTrayChangeMove = false;

        /// <summary>
        /// 매뉴얼 매뉴얼 피더 동작
        /// </summary>
        public bool Manual_HopperRun = false;

        /// <summary>
        /// 매뉴얼 파이프 호퍼 테스트 동작
        /// </summary>
        public bool Manual_PipeHopperRunTest = false;

        /// <summary>
        /// 매뉴얼 니들 호퍼 테스트 동작
        /// </summary>
        public bool Manual_NeedleHopperRunTest = false;

        /// <summary>
        /// 매뉴얼 양쪽 호퍼 테스트 동작
        /// </summary>
        public bool Manual_BothHopperRunTest = false;

        /// <summary>
        /// 매뉴얼 피더 테스트 동작
        /// </summary>
        public bool Manual_FeederRunTest = false;

        /// <summary>
        /// 매뉴얼 파이프 PnP 픽업
        /// </summary>
        public bool Manual_PipePnPPickUp = false;

        /// <summary>
        /// 매뉴얼 파이프 PnP 플레이스
        /// </summary>
        public bool Manual_PipePnPPlace = false;

        /// <summary>
        /// 매뉴얼 파이프 PnP 안전위치 이동
        /// </summary>
        public bool Manual_PipePnPSafe = false;

        /// <summary>
        /// 매뉴얼 니들 PnP 픽업
        /// </summary>
        public bool Manual_NeedlePnPPickUp = false;

        /// <summary>
        /// 매뉴얼 니들 PnP 플레이스
        /// </summary>
        public bool Manual_NeedlePnPPlace = false;

        /// <summary>
        /// 매뉴얼 니들 PnP 안전위치 이동
        /// </summary>
        public bool Manual_NeedlePnPSafe = false;

        /// <summary>
        /// 매뉴얼 트랜스퍼 파이프 & 니들 받기
        /// </summary>
        public bool Manual_TransferRecv = false;

        /// <summary>
        /// 매뉴얼 트랜스퍼 파이프 & 니들 보내기
        /// </summary>
        public bool Manual_TransferSend = false;

        /// <summary>
        /// 매뉴얼 파이프 트랜스퍼 NG 교환 위치로
        /// </summary>
        public bool Manual_TransferNG_Change = false;

        /// <summary>
        /// 매뉴얼 파이프 마운터 픽업
        /// </summary>
        public bool Manual_PipeMounterClampUp = false;

        /// <summary>
        /// 매뉴얼 파이프 마운트
        /// </summary>
        public bool Manual_PipeMount = false;

        /// <summary>
        /// 매뉴얼 검사가 없는 파이프 마운트
        /// </summary>
        public bool Manual_PipeMountNotInsp = false;

        /// <summary>
        /// 매뉴얼 홀더의 파이프를 푸쉬만 동작
        /// </summary>
        public bool Manual_JustPipePush = false;

        /// <summary>
        /// 매뉴얼 파이프 마운터 안전위치
        /// </summary>
        public bool Manual_PipeMounterSafe = false;

        /// <summary>
        /// 매뉴얼 그냥 파이프 홀더를 촬영
        /// </summary>
        public bool Manual_JustPipeHolderShot = false;

        /// <summary>
        /// 매뉴얼 니들 마운터 픽업
        /// </summary>
        public bool Manual_NeedleMounterClampUp = false;

        /// <summary>
        /// 매뉴얼 니들 마운트
        /// </summary>
        public bool Manual_NeedleMount = false;

        /// <summary>
        /// 매뉴얼 검사가 없는 니들 마운트
        /// </summary>
        public bool Manual_NeedeleMountNotInsp = false;

        /// <summary>
        /// 매뉴얼 홀더의 니들을 푸쉬만 동작
        /// </summary>
        public bool Manual_JustNeedlePush = false;

        /// <summary>
        /// 매뉴얼 니들 마운터 안전위치
        /// </summary>
        public bool Manual_NeedleMounterSafe = false;

        /// <summary>
        /// 매뉴얼 검사가 없는 디스펜싱
        /// </summary>
        public bool Manual_DispensingNotInsp = false;

        /// <summary>
        /// 매뉴얼 그냥 니들 홀더를 촬영
        /// </summary>
        public bool Manual_JustNeedleHolderShot = false;

        /// <summary>
        /// 매뉴얼 강제 니들 마운트
        /// </summary>
        public bool Manual_ForcedNeedleMount = false;

        /// <summary>
        /// 매뉴얼 푸쉬 안하는 니들 마운트
        /// </summary>
        public bool Manual_NeedleMountNotPush = false;

        /// <summary>
        /// 매뉴얼 홀더 디스펜싱
        /// </summary>
        public bool Manual_HolderDispensing = false;

        /// <summary>
        /// 매뉴얼 디스펜서 노즐 Clean
        /// </summary>
        public bool Manual_DispenserClean = false;

        /// <summary>
        /// 매뉴얼 디스펜서 안전위치
        /// </summary>
        public bool Manual_DispenserSafe = false;

        /// <summary>
        /// 매뉴얼 디스펜서 Z 캘리브레이션
        /// </summary>
        public bool Manual_DispenserZCal = false;

        /// <summary>
        /// 매뉴얼 그냥 디스펜서 홀더 촬영
        /// </summary>
        public bool Manual_JustDispHolderShot = false;

        /// <summary>
        /// 매뉴얼 파이프, 니들 PnP 대기위치 이동
        /// </summary>
        public bool Manual_PipeNeedlePnPMoveSafePos = false;

        /// <summary>
        /// 매뉴얼 파이프, 니들 로봇 관련 자재 올 클리어 동작
        /// </summary>
        public bool Manual_PipeNeedleRobotAllWorkClear = false;

        #endregion 매뉴얼 플래그 정의

        /// <summary>
        /// 설비에 생산중인 모든 홀더 자재를 비우기 위해 홀더 공급을 중단시키는 기능
        /// </summary>
        public bool bHolder_SupplyStop = false;

        /// <summary>
        /// 파이프 호퍼
        /// </summary>
        public bool bHopper_SupplyPipe = false;

        /// <summary>
        /// 니들 호퍼
        /// </summary>
        public bool bHopper_SupplyNeedle = false;

        /// <summary>
        /// 이전 파이프 검사 안하는 파이프 마운트 플래그
        /// </summary>
        public bool bPipeMountInspSkip = false;

        /// <summary>
        /// 전 니들 검사 안하는 파이프 마운트 플래그
        /// </summary>
        public bool bNeedleMountInspSkip = false;

        /// <summary>
        /// 홀더 파이프 푸쉬 실린더 작업 진행 여부
        /// </summary>
        public bool bHolderPipePushBegin = false;

        /// <summary>
        /// 홀더 니들 푸쉬 실린더 작업 진행 여부
        /// </summary>
        public bool bHolderNeedlePushBegin = false;

        /// <summary>
        /// 홀더 Flip 작업 진행 여부
        /// </summary>
        public bool bFlipDone = false;

        /// <summary>
        /// 강제 니들 마운트 플래그
        /// </summary>
        public bool bForcedNeedleMount = false;

        /// <summary>
        /// 파이프 마운트 Cycle Time
        /// </summary>
        public Stopwatch PipeMountCycleTime = new Stopwatch();

        /// <summary>
        /// 니들 마운트 Cycle Time
        /// </summary>
        public Stopwatch NeedleMountCycleTime = new Stopwatch();

        /// <summary>
        /// 파이프 택타임 담는 변수
        /// </summary>
        public int iPipeCycleTime = 0;

        /// <summary>
        /// 니들 택타임 담는 변수
        /// </summary>
        public int iNeedleCycleTime = 0;

        /// <summary>
        /// 생성자
        /// </summary>
        public CVar()
        {
            for (uint i = 0; i < Define.MAX_CAMERA; i++)
            {
                cVisionResultDataList.Add(new CVisionResultData(i, 0));
                if (i == 1 || i == 2)
                {
                    cVisionResultDataList.Add(new CVisionResultData(i, 1));
                }
            }
        }
    }

    #endregion 시스템 Run 시 사용하는 전역변수 (No Save Variable)

    #region 카메라 기본설정값 (No Save Variable)

    /// <summary>
    /// 카메라 기본 설정 값 (No Save Variable)
    /// </summary>
    public class CVisionData
    {
        /// <summary>
        /// 촬영 Image 저장 경로
        /// </summary>
        public string[] strVisionImagePath = new string[Define.MAX_CAMERA];

        /// <summary>
        /// Vision 각 카메라 화면 사이즈 정의
        /// </summary>
        public uint[] iScreenWidth = new uint[Define.MAX_CAMERA] { 4024, 4024, 4024, 4024, 4024, 4024 };

        public uint[] iScreenHeight = new uint[Define.MAX_CAMERA] { 3036, 3036, 3036, 3036, 3036, 3036 };

        /// <summary>
        /// Vision 각 화면 사이즈 반절
        /// </summary>
        public uint[] iScreenHalfWidth = new uint[Define.MAX_CAMERA];

        public uint[] iScreenHalfHeight = new uint[Define.MAX_CAMERA];

        /// <summary>
        /// 카메라 Resolution
        /// </summary>
        public double[] dResolution = new double[Define.MAX_CAMERA] { 0.0285714285714286, 0.0012057758848752, 0.0012057758848752,
                                                                      0.0060881540351676, 0.0059880239520958, 0.0055333349049314};

        /// <summary>
        /// Vision 각 카메라 ToolBlock 이름 정의
        /// </summary>
        public string[][] strToolBlockName = new string[Define.MAX_CAMERA][];

        /// <summary>
        /// Trigger 사용 유무 (UI 관련 Mode 자동 변경용)
        /// </summary>
        public bool[] bTriggerMode = new bool[Define.MAX_CAMERA] { false, false, false, false, false, false };

        /// <summary>
        /// Image Rotate 사용 유무
        /// </summary>
        public bool[] bRotateMode = new bool[Define.MAX_CAMERA] { false, false, false, false, false, false };

        /// <summary>
        /// Image Rotate 방향 결정
        /// </summary>
        public eRotate[] eRotates = new eRotate[Define.MAX_CAMERA]
                                             { eRotate._90Deg, eRotate._90Deg, eRotate._90Deg,
                                               eRotate._90Deg, eRotate._90Deg, eRotate._90Deg};

        // 파이프 홀을 1개씩 서칭하기위해 검색 영역의 Center값을 여기서 설정한다.
        public double[] dPipeMatchingPosX = new double[19]
            { 1813, 2019, 2231, 2019, 1604, 1392, 1598, 2013, 2371, 2596, 2605, 2399, 2041, 1623, 1262, 1056, 1047, 1258, 1614 };

        public double[] dPipeMatchingPosY = new double[19]
            { 1482, 1145, 1491, 1853, 1856, 1500, 1145, 734, 921, 1288, 1700, 2061, 2283, 2286, 2071, 1709, 1295, 936, 730 };

        public double[] dPipeMatchingPosD = new double[12]
            { -18, 10, 40, 74, 101, 134, 162, -167, -138, -106, -77, -46 };

        // 니들 홀을 1개씩 서칭하기위해 검색 영역의 Center값을 여기서 설정한다.
        public double[] dNeedleMatchingPosX = new double[19]
            { 2099, 2312, 2508, 2314, 1870, 1684, 1877, 2297, 2659, 2863, 2865, 2680, 2301, 1892, 1519, 1316, 1307, 1503, 1880 };

        public double[] dNeedleMatchingPosY = new double[19]
            { 1506, 1168, 1516, 1878, 1882, 1520, 1161, 737, 945, 1298, 1728, 2078, 2290, 2292, 2089, 1732, 1318, 961, 731 };

        public CVisionData()
        {
            for (int i = 0; i < Define.MAX_CAMERA; i++)
            {
                strVisionImagePath[i] = string.Format(CXMLProcess.BackupBasePath + @"VisionImage\Camera{0}", i);
                iScreenHalfWidth[i] = iScreenWidth[i] / 2;
                iScreenHalfHeight[i] = iScreenHeight[i] / 2;
            }

            // Vision UI Setup 화면의 콤보 박스를 정의함 (카메라 당 ToolBlock 개 수)
            strToolBlockName[0] = new string[1] { "PipeNeedlePickUp" };
            strToolBlockName[1] = new string[1] { "Pipe" };
            strToolBlockName[2] = new string[1] { "Needle" };
            strToolBlockName[3] = new string[1] { "PipeMount" };
            strToolBlockName[4] = new string[1] { "NeedleMount" };
            strToolBlockName[5] = new string[1] { "Dispenser" };
        }
    }

    /// <summary>
    /// Vision 결과 및 촬영 상태 값
    /// </summary>
    public class CVisionResultData
    {
        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="uiCamNo"></param>
        /// <param name="uiBlockNo"></param>
        public CVisionResultData(uint uiCamNo, uint uiBlockNo)
        {
            this.uiCamNo = uiCamNo;
            this.uiBlockNo = uiBlockNo;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void DataClear()
        {
            bShootFinish = false;
            strBarcord = string.Empty;
            bGoodNg = true;
            dPipeMountX = 0;
            dPipeMountY = 0;
            dNeedleMountX = 0;
            dNeedleMountY = 0;
            dDegree = 0;
            dPipeX = 0; dPipeY = 0; dPipeDegree = 0;
            dNeedleX = 0; dNeedleY = 0; dNeedleDegree = 0;
            uiPipeCount = 0;
            uiNeedleCount = 0;

            bHolderEmpty = false;

            bPipeInserFail = false;
            bNeedleInserFail = false;

            bHolderPositionFail = false;

            bPipeHoleSearchFail = false;
            bNeedleHoleSearchFail = false;

            bPipeDoubleInsertSuspection = false;
            bNeedleDoubleInsertSuspection = false;

            bPipeDoubleCatch = false;
            bNeedleDoubleCatch = false;
        }

        private readonly object VisionDataLock = new object();

        /// <summary>
        /// 카메라 번호
        /// </summary>
        public uint uiCamNo = 0;

        /// <summary>
        /// 카메라 기능 번호
        /// </summary>
        public uint uiBlockNo = 0;

        /// <summary>
        /// 카메라 촬영 상태
        /// </summary>
        private bool _bShootFinish = false;

        public bool bShootFinish
        {
            get
            {
                lock (VisionDataLock)
                {
                    return _bShootFinish;
                }
            }
            set
            {
                lock (VisionDataLock)
                {
                    _bShootFinish = value;
                }
            }
        }

        /// <summary>
        /// 바코드 정보
        /// </summary>
        public string strBarcord = string.Empty;

        /// <summary>
        /// Vision 결과
        /// </summary>
        public bool bGoodNg = true;

        /// <summary>
        /// CAM3 파이프 마운트 X 위치
        /// </summary>
        public double dPipeMountX = 0;

        /// <summary>
        /// CAM3 파이프 마운트 Y 위치
        /// </summary>
        public double dPipeMountY = 0;

        /// <summary>
        /// CAM4 니들 마운트 X 위치
        /// </summary>
        public double dNeedleMountX = 0;

        /// <summary>
        /// CAM4 니들 마운트 Y 위치
        /// </summary>
        public double dNeedleMountY = 0;

        /// <summary>
        /// CAM1 파이프 틀어짐 각도
        /// </summary>
        public double dDegree = 0.0;

        /// <summary>
        /// 파이프 클램프 Z축 보정
        /// </summary>
        public double dPipeClampZ = 0.0;

        /// <summary>
        /// 니들 클램프 Z축 보정
        /// </summary>
        public double dNeedleClampZ = 0.0;

        /// <summary>
        /// 피더에서 확인된 파이프의 개수
        /// </summary>
        public uint uiPipeCount = 0;

        /// <summary>
        /// 피더에서 확인된 니들의 개수
        /// </summary>
        public uint uiNeedleCount = 0;

        /// <summary>
        /// CAM0 파이프 좌표 X
        /// </summary>
        public double dPipeX = 0.0;

        /// <summary>
        /// CAM0 파이프 좌표 Y
        /// </summary>
        public double dPipeY = 0.0;

        /// <summary>
        /// CAM0 파이프 각도
        /// </summary>
        public double dPipeDegree = 0.0;

        /// <summary>
        /// CAM0 니들 좌표 X
        /// </summary>
        public double dNeedleX = 0.0;

        /// <summary>
        /// CAM0 니들 좌표 Y
        /// </summary>
        public double dNeedleY = 0.0;

        /// <summary>
        /// CAM0 파이프 각도
        /// </summary>
        public double dNeedleDegree = 0.0;

        /// <summary>
        /// 분주 Point
        /// </summary>
        public bool bHolderEmpty = false;

        /// <summary>
        /// CAM5 홀더 위치 틀어짐
        /// </summary>
        public bool bHolderPositionFail = false;

        /// <summary>
        /// 파이프 삽입 실패 확인
        /// </summary>
        public bool bPipeInserFail = false;

        /// <summary>
        /// 니들 삽입 실패 확인
        /// </summary>
        public bool bNeedleInserFail = false;

        /// <summary>
        /// 파이프 or 니들 마지막 삽입검사
        /// </summary>
        public bool bLastHoleInsp = false;

        /// <summary>
        /// 파이프 홀 인식 실패 확인
        /// </summary>
        public bool bPipeHoleSearchFail = false;

        /// <summary>
        /// 니들 홀 인식 실패 확인
        /// </summary>
        public bool bNeedleHoleSearchFail = false;

        /// <summary>
        /// 파이프 이중삽입 의심 확인
        /// </summary>
        public bool bPipeDoubleInsertSuspection = false;

        /// <summary>
        /// 니들 이중삽입 의심 확인
        /// </summary>
        public bool bNeedleDoubleInsertSuspection = false;

        /// <summary>
        /// 니들 여러개 집었는지 확인 플래그(여러개 집었으면 True)
        /// </summary>
        public bool bNeedleDoubleCatch = false;

        /// <summary>
        /// 파이프 여러개 집었는지 확인 플래그(여러개 집었으면 True)
        /// </summary>
        public bool bPipeDoubleCatch = false;
    }

    #endregion 카메라 기본설정값 (No Save Variable)
}