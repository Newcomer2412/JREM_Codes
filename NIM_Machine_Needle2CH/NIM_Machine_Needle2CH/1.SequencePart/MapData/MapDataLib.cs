using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;

namespace MachineControlBase
{
    /// <summary>
    /// 맵 데이터 관련 정의 클래스
    /// </summary>
    public static class MapDataFunction
    {
        /// <summary>
        /// 팝업이 열려 있는 상태인지 확인 플래그
        /// </summary>
        public static bool bPopupOpen = false;

        /// <summary>
        /// 데이터 Edit에서 갱신되도록 메인 UnitMap 타이머 갱신 정지 Flag
        /// </summary>
        public static bool bDataEditOpen = false;

        /// <summary>
        /// Unit 상태에 대한 색상을 리턴
        /// </summary>
        /// <param name="eStatus"></param>
        /// <returns></returns>
        public static SolidColorBrush StatusColor(eStatus eStatus)
        {
            SolidColorBrush brush = Brushes.Black;
            switch ((int)eStatus)
            {
                case 0: brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00)); break; // Black
                case 1: brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x80, 0x80, 0x80)); break; // Gray
                case 2: brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x1E, 0x90, 0xFF)); break; // DodgerBlue
                case 3: brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x5F, 0x9E, 0xA0)); break; // CadetBlue
                case 4: brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x80, 0x00, 0x80)); break; // Purple
                case 5: brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x6B, 0x99, 0x00)); break; // Olive
                case 6: brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0xFF)); break; // Blue
                case 7: brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x1D, 0xDB, 0x16)); break; // Green
                case 8: brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x22, 0x74, 0x1C)); break; // Deep Green
                case 9: brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x00, 0xDD)); break; // Hot Pink
                case 10: brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x98, 0x00, 0x00)); break; // Brown
                case 11: brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x8A, 0x00)); break; // Cream
                case 12: brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x7F, 0x50)); break; // Flesh
                case 13: brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00)); break; // Black
                case 14: brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x00, 0x00)); break; // Red
                case 99: brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x00, 0x00)); break; // Red
            }
            return brush;
        }

        public static readonly int[] iHolderPicker = new int[3] { 1, 3, 12 };
        public static readonly int[] iMPC_FrontFarLeft = new int[5] { 0, 1, 3, 5, 12 };
        public static readonly int[] iMPC_BUFFER1 = new int[2] { 1, 5 };
        public static readonly int[] iMPC_PipeMount = new int[4] { 1, 6, 7, 13 };
        public static readonly int[] iMPC_BUFFER2 = new int[4] { 1, 6, 7, 13 };
        public static readonly int[] iMPC_NeedleMount = new int[4] { 1, 5, 8, 13 };
        public static readonly int[] iMPC_Flip = new int[5] { 1, 8, 9, 13, 14 };
        public static readonly int[] iMPC_Disp = new int[4] { 1, 9, 10, 14 };
        public static readonly int[] iMPC_FarRight = new int[3] { 0, 1, 10 };
        public static readonly int[] iMPC_UV = new int[4] { 0, 1, 10, 12 };
        public static readonly int[] iMPC_RearFarLeft = new int[3] { 0, 1, 12 };
        public static readonly int[] iTransfer = new int[4] { 1, 2, 3, 4 };
        public static readonly int[] iWork = new int[3] { 1, 3, 4 };
        public static readonly int[] iDispenser = new int[2] { 2, 11 };
        public static readonly int[] iHolderSupply = new int[2] { 1, 3 };
        public static readonly int[] iHolderDone = new int[2] { 1, 4 };
        public static readonly int[] iNG = new int[2] { 1, 99 };
    }

    /// <summary>
    /// Unit 상태
    /// </summary>
    public enum eStatus : int
    {
        /// <summary>
        /// 정의 되지 않음
        /// </summary>
        INIT = -1,

        /// <summary>
        /// 사용하지 않음
        /// </summary>
        NONE = 0,

        /// <summary>
        /// Unit 없음
        /// </summary>
        EMPTY = 1,

        /// <summary>
        /// 작업 대기
        /// </summary>
        STANBY = 2,

        /// <summary>
        /// Unit 있음
        /// </summary>
        MOUNT = 3,

        /// <summary>
        /// Vision 및 바코드, 높이 측정, 디캡핑, 캡핑 등 체크 완료된 TUBE
        /// </summary>
        WORK_DONE = 4,

        /// <summary>
        /// MPC 홀더 공급
        /// </summary>
        HOLDER = 5,

        /// <summary>
        /// 홀더에 첫번째 삽입 이전 상태
        /// </summary>
        FIRST_HOLE = 6,

        /// <summary>
        /// 홀더에 첫번째 니들 결합 완료 상태
        /// </summary>
        FIRST_MOUNT = 7,

        /// <summary>
        /// 홀더에 두번째 니들 결합 완료 상태 (플립 직전 상태)
        /// </summary>
        SECOND_MOUNT = 8,

        /// <summary>
        /// 홀더 Flip 완료 상태
        /// </summary>
        FLIP_DONE = 9,

        /// <summary>
        /// 디스펜싱 완료 상태
        /// </summary>
        DISPENSING = 10,

        /// <summary>
        /// 디스펜서 클린
        /// </summary>
        DISPENSER_CLEAN = 11,

        /// <summary>
        /// 홀더 UV 큐어 완료 상태
        /// </summary>
        UV = 12,

        /// <summary>
        /// 파이프, 니들 삽입을 스킵한 상태(비전 홀 인식 실패 OR 파이프 이중삽입 방지)
        /// </summary>
        SKIPPED = 13,

        /// <summary>
        /// 플립 수행한 SKIP 상태(SKIP일 때 플립을 수행했는지 확인하기 위한 상태)
        /// </summary>
        SKIP_AFTER_FLIP = 14,

        /// <summary>
        /// NG
        /// </summary>
        NG = 99,
    }

    /// <summary>
    /// Unit Data
    /// </summary>
    public enum eData : int
    {
        SUPPLY_HOLDER_TRAY = 0,             // 홀더 공급 TRAY
        WORK_DONE_HOLDER_TRAY,              // 완성된 홀더 회수 TRAY

        MPC1_FAR_LEFT,                      // MPC1 맨 왼쪽
        MPC1_BUFFER_1,                      // MPC1 BUFFER 1
        MPC1_PIPE_MOUNT,                    // MPC1 홀더에 파이프 삽입 구간
        MPC1_BUFFER_2,                      // MPC1 BUFFER 2
        MPC1_NEEDLE_MOUNT,                  // MPC1 홀더에 니들 삽입 구간
        MPC1_FLIP,                          // MPC1 홀더 뒤집는 구간
        MPC1_DISPENSING,                    // MPC1 홀더 분주 구간
        MPC1_FAR_RIGHT,                     // MPC1 맨 오른쪽

        MPC2_FAR_LEFT,                      // MPC2 맨 왼쪽
        MPC2_UV,                            // MPC2 UV 구간
        MPC2_FAR_RIGHT,                     // MPC2 맨 오른쪽

        HOLDER_PICKER,                      // 홀더 공급 및 회수 피커

        FEEDER_BASKET,                      // 피더 바스켓의 파이프 니들 공급부
        PIPE_PnP,                           // 피더와 트랜스퍼를 오가는 파이프 PnP
        NEEDLE_PnP,                         // 피더와 트랜스퍼를 오가는 니들 PnP
        PIPE_TRANSFER,                      // 트랜스퍼와 파이프 마운트 피커를 오가는 TRANSFER
        NEEDLE_TRANSFER,                    // 트랜스퍼와 니들 마운트 피커르 오가는 TRANSFER
        PIPE_MOUNTER,                       // 파이프를 홀더에 마운트하는 피커
        NEEDLE_MOUNTER,                     // 니들을 홀더에 마운트하는 피커

        DISPENSER,                          // 홀더에 용액 분주

        NG_HOLDER_TRAY,                     // NG 홀더 회수 TRAY
    }

    /// <summary>
    /// Base Map Data
    /// </summary>
    [Serializable]
    public class BaseMapData
    {
        /// <summary>
        /// 데이터 에러 방지를 위한 Read Lock
        /// </summary>
        private readonly object oDataReadLock = new object();

        private int _iIndexX = 0;

        /// <summary>
        /// Unit X Index
        /// </summary>
        public int iIndexX
        {
            get
            {
                lock (oDataReadLock)
                {
                    return _iIndexX;
                }
            }
            set
            {
                lock (oDataReadLock)
                {
                    _iIndexX = value;
                }
            }
        }

        private int _iIndexY = 0;

        /// <summary>
        /// Unit Y index
        /// </summary>
        public int iIndexY
        {
            get
            {
                lock (oDataReadLock)
                {
                    return _iIndexY;
                }
            }
            set
            {
                lock (oDataReadLock)
                {
                    _iIndexY = value;
                }
            }
        }

        private double _dPosX = -1;

        /// <summary>
        /// Unit X Pos 위치
        /// </summary>
        public double dPosX
        {
            get
            {
                lock (oDataReadLock)
                {
                    return _dPosX;
                }
            }
            set
            {
                lock (oDataReadLock)
                {
                    _dPosX = value;
                }
            }
        }

        private double _dPosY = 0;

        /// <summary>
        /// Unit Y Pos 위치
        /// </summary>
        public double dPosY
        {
            get
            {
                lock (oDataReadLock)
                {
                    return _dPosY;
                }
            }
            set
            {
                lock (oDataReadLock)
                {
                    _dPosY = value;
                }
            }
        }

        private int _iUnitNo = 0;

        /// <summary>
        /// Unit 배열 넘버
        /// </summary>
        public int iUnitNo
        {
            get
            {
                lock (oDataReadLock)
                {
                    return _iUnitNo;
                }
            }
            set
            {
                lock (oDataReadLock)
                {
                    _iUnitNo = value;
                }
            }
        }

        private eStatus _eStatus = eStatus.EMPTY;

        /// <summary>
        /// Unit 상태 값
        /// </summary>
        public eStatus eStatus
        {
            get
            {
                lock (oDataReadLock)
                {
                    return _eStatus;
                }
            }
            set
            {
                lock (oDataReadLock)
                {
                    _eStatus = value;
                }
            }
        }

        private eStatus _eOldStatus = eStatus.INIT;

        /// <summary>
        /// Unit Old 상태 값
        /// </summary>
        [XmlIgnore]
        public eStatus eOldStatus
        {
            get
            {
                lock (oDataReadLock)
                {
                    return _eOldStatus;
                }
            }
            set
            {
                lock (oDataReadLock)
                {
                    _eOldStatus = value;
                }
            }
        }

        private bool _bLiquidHigh = false;

        /// <summary>
        /// 튜브 라벨을 프린트 했는지 여부
        /// </summary>
        public bool bLiquidHigh
        {
            get
            {
                lock (oDataReadLock)
                {
                    return _bLiquidHigh;
                }
            }
            set
            {
                lock (oDataReadLock)
                {
                    _bLiquidHigh = value;
                }
            }
        }

        private bool _bNgTubeDecappingIn = false;

        /// <summary>
        /// NG 튜브가 디캡핑으로 들어갔다 나왔는지 여부(튜브 순서대로 들어가야해서 대기 시킨다)
        /// </summary>
        public bool bNgTubeDecappingIn
        {
            get
            {
                lock (oDataReadLock)
                {
                    return _bNgTubeDecappingIn;
                }
            }
            set
            {
                lock (oDataReadLock)
                {
                    _bNgTubeDecappingIn = value;
                }
            }
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public BaseMapData()
        { }

        /// <summary>
        /// Map Data 생성자
        /// </summary>
        /// <param name="iX"></param>
        /// <param name="iY"></param>
        public BaseMapData(int iX, int iY)
        {
            iIndexX = iX;
            iIndexY = iY;
        }
    }

    /// <summary>
    /// Map Data 관리 클래스
    /// </summary>
    [Serializable]
    public class MapDataLib : ICloneable
    {
        /// <summary>
        /// 생성자
        /// </summary>
        public MapDataLib()
        {
        }

        /// <summary>
        /// 최대 X Index 개 수
        /// </summary>
        public int iMaxXindex = 0;

        /// <summary>
        /// 최대 Y Index 개 수
        /// </summary>
        public int iMaxYindex = 0;

        /// <summary>
        /// 데이터 이름
        /// </summary>
        public string strDataName = string.Empty;

        /// <summary>
        /// 데이터 에러를 위한 Read Lock
        /// </summary>
        private readonly object oDataReadLock = new object();

        /// <summary>
        /// Map을 구성하는 Data List
        /// </summary>
        private List<BaseMapData> _BaseMapDataList = new List<BaseMapData>();

        public List<BaseMapData> BaseMapDataList
        {
            get
            {
                lock (oDataReadLock)
                {
                    return _BaseMapDataList;
                }
            }
            set
            {
                lock (oDataReadLock)
                {
                    _BaseMapDataList = value;
                }
            }
        }

        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="iMaxXindex"></param>
        /// <param name="iMaxYindex"></param>
        public void Init(int iMaxXindex, int iMaxYindex)
        {
            if (this.iMaxXindex != iMaxXindex ||
                this.iMaxYindex != iMaxYindex ||
                BaseMapDataList.Count != (iMaxXindex * iMaxYindex))
            {
                this.iMaxXindex = iMaxXindex;
                this.iMaxYindex = iMaxYindex;
                BaseMapDataList.Clear();
                for (int j = 0; j < iMaxYindex; j++)
                {
                    for (int i = 0; i < iMaxXindex; i++)
                    {
                        BaseMapData baseMapData = new BaseMapData(i, j);
                        BaseMapDataList.Add(baseMapData);
                    }
                }
            }
        }

        /// <summary>
        /// 모든 데이터를 리셋
        /// </summary>
        public void MapDataReset()
        {
            foreach (BaseMapData baseMapData in BaseMapDataList)
            {
                baseMapData.eStatus = eStatus.EMPTY;
            }
        }

        /// <summary>
        /// 모든 데이터를 리셋
        /// </summary>
        public void OldMapDataReset()
        {
            foreach (BaseMapData baseMapData in BaseMapDataList)
            {
                baseMapData.eOldStatus = eStatus.INIT;
            }
        }

        /// <summary>
        /// Index 자리의 데이터를 리턴
        /// </summary>
        /// <param name="iIndexX"></param>
        /// <param name="iIndexY"></param>
        /// <returns></returns>
        public BaseMapData GetIndex(int iIndexX, int iIndexY)
        {
            BaseMapData baseMapData = BaseMapDataList.Find(x => x.iIndexX == iIndexX && x.iIndexY == iIndexY);
            if (baseMapData == null) return null;
            return baseMapData;
        }

        /// <summary>
        /// Index X 자리의 데이터를 리턴(Y가 한줄일 경우 사용)
        /// </summary>
        /// <param name="iIndexX"></param>
        /// <returns></returns>
        public BaseMapData GetIndexX(int iIndexX)
        {
            BaseMapData baseMapData = BaseMapDataList.Find(x => x.iIndexX == iIndexX && x.iIndexY == 0);
            if (baseMapData == null) return null;
            return baseMapData;
        }

        /// <summary>
        /// Index Y 자리의 데이터를 리턴(X가 한줄일 경우 사용)
        /// </summary>
        /// <param name="iIndexY"></param>
        /// <returns></returns>
        public BaseMapData GetIndexY(int iIndexY)
        {
            BaseMapData baseMapData = BaseMapDataList.Find(x => x.iIndexX == 0 && x.iIndexY == iIndexY);
            if (baseMapData == null) return null;
            return baseMapData;
        }

        /// <summary>
        /// Unit No를 기준으로 데이터를 리턴
        /// </summary>
        /// <param name="iNo"></param>
        /// <returns></returns>
        public BaseMapData GetUnitNo(int iNo)
        {
            BaseMapData baseMapData = BaseMapDataList.Find(x => x.iUnitNo == iNo);
            if (baseMapData == null) return null;
            return baseMapData;
        }

        /// <summary>
        /// 선택한 상태값 기준으로 Unit No가 가장 작은 데이터를 리턴
        /// </summary>
        /// <param name="eStatus"></param>
        /// <returns></returns>
        public BaseMapData GetUnitMin(eStatus eStatus)
        {
            BaseMapData baseMapData = BaseMapDataList.Find(x => x.iUnitNo == (from BaseMapData in BaseMapDataList
                                                                              where BaseMapData.eStatus == eStatus
                                                                              select BaseMapData).Min(BaseMapData => BaseMapData.iUnitNo));
            if (baseMapData == null) return null;
            return baseMapData;
        }

        /// <summary>
        /// 선택한 상태값 기준으로 Unit No가 가장 큰 데이터를 리턴
        /// </summary>
        /// <param name="eStatus"></param>
        /// <returns></returns>
        public BaseMapData GetUnitMax(eStatus eStatus)
        {
            BaseMapData baseMapData = BaseMapDataList.Find(x => x.iUnitNo == (from BaseMapData in BaseMapDataList
                                                                              where BaseMapData.eStatus == eStatus
                                                                              select BaseMapData).Max(BaseMapData => BaseMapData.iUnitNo));
            if (baseMapData == null) return null;
            return baseMapData;
        }

        /// <summary>
        /// 선택한 Unit No의 데이터 상태를 변경
        /// </summary>
        /// <param name="eStatus"></param>
        public void SetStatus(int iNo, eStatus eStatus)
        {
            foreach (BaseMapData baseMapData in BaseMapDataList)
            {
                if (baseMapData.iUnitNo == iNo)
                {
                    baseMapData.eStatus = eStatus;
                }
            }
        }

        /// <summary>
        /// 선택한 Unit No까지 데이터 상태를 변경
        /// </summary>
        /// <param name="eStatus"></param>
        public void SetUntilStatus(int iNo, eStatus eStatus)
        {
            foreach (BaseMapData baseMapData in BaseMapDataList)
            {
                if (baseMapData.iUnitNo <= iNo)
                {
                    baseMapData.eStatus = eStatus;
                }
            }
        }

        /// <summary>
        /// 전체 데이터 상태를 변경
        /// </summary>
        /// <param name="eStatus"></param>
        /// <param name="bNoneOK"></param>
        public void SetAllStatus(eStatus eStatus, bool bNoneOK = false)
        {
            // 맵데이터가 Needle Holder 일때
            if (strDataName == Enum.GetName(typeof(eData), eData.MPC1_PIPE_MOUNT) ||
                strDataName == Enum.GetName(typeof(eData), eData.MPC1_NEEDLE_MOUNT))
            {
                int m = 0;
                foreach (BaseMapData baseMapData in BaseMapDataList)
                {
                    if (CMainLib.Ins.cSysOne.bHolderNeedlePinWorkCount[m] == (int)eNeedlePinStatus.Notwork)
                    {
                        baseMapData.eStatus = eStatus.NONE;
                    }
                    else
                    {
                        baseMapData.eStatus = eStatus;
                    }
                    m++;
                }
                return;
            }

            foreach (BaseMapData baseMapData in BaseMapDataList)
            {
                if (baseMapData.eStatus != eStatus.NONE || bNoneOK == true)
                {
                    baseMapData.eStatus = eStatus;
                }
            }
        }

        /// <summary>
        /// 상태 값이 있는지 확인
        /// </summary>
        /// <param name="eStatus"></param>
        /// <returns></returns>
        public bool GetStatus(eStatus eStatus)
        {
            int iCount = BaseMapDataList.FindAll(x => x.eStatus == eStatus).Count;
            if (iCount != 0) return true;
            return false;
        }

        /// <summary>
        /// 전체 상태 값이 맞는지 확인
        /// </summary>
        /// <param name="eStatus"></param>
        /// <param name="bAddNone"></param>
        /// <returns></returns>
        public bool GetAllStatus(eStatus eStatus, bool bAddNone = true)
        {
            int iCount = BaseMapDataList.FindAll(x => x.eStatus == eStatus).Count;
            int iNoneCount = BaseMapDataList.FindAll(x => x.eStatus == eStatus.NONE).Count;
            if (eStatus == eStatus.NONE)
            {
                if (iNoneCount == BaseMapDataList.Count) return true;
                else return false;
            }
            else
            {
                if (bAddNone == true)
                {
                    if (iCount + iNoneCount == BaseMapDataList.Count) return true;
                    return false;
                }
                else
                {
                    if (iCount == BaseMapDataList.Count) return true;
                    return false;
                }
            }
        }

        /// <summary>
        /// 전체 상태 값이 맞는지 확인(인수 2개짜리)
        /// </summary>
        /// <param name="eStatus1"></param>
        /// <param name="eStatus2"></param>
        /// <param name="bAddNone"></param>
        /// <returns></returns>
        public bool GetAllStatus(eStatus eStatus1, eStatus eStatus2, bool bAddNone = true)
        {
            int iCount1 = BaseMapDataList.FindAll(x => x.eStatus == eStatus1).Count;
            int iCount2 = BaseMapDataList.FindAll(x => x.eStatus == eStatus2).Count;
            int iNoneCount = BaseMapDataList.FindAll(x => x.eStatus == eStatus.NONE).Count;
            if (eStatus1 == eStatus.NONE)
            {
                if (iNoneCount == BaseMapDataList.Count) return true;
                else return false;
            }
            else
            {
                if (bAddNone == true)
                {
                    if (iCount1 + iCount2 + iNoneCount == BaseMapDataList.Count) return true;
                    return false;
                }
                else
                {
                    if (iCount1 + iCount2 == BaseMapDataList.Count) return true;
                    return false;
                }
            }
        }

        /// <summary>
        /// 해당 상태 값이 몇개인지 확인
        /// </summary>
        /// <param name="eStatus"></param>
        /// <returns></returns>
        public int GetStatusCount(eStatus eStatus)
        {
            int iCount = BaseMapDataList.FindAll(x => x.eStatus == eStatus).Count;
            return iCount;
        }

        /// <summary>
        /// 데이터 복사
        /// </summary>
        /// <param name="mapDataLib"></param>
        public void DataCopy(MapDataLib mapDataLib)
        {
            for (int i = 0; i < BaseMapDataList.Count; i++)
            {
                BaseMapData baseMapData = BaseMapDataList.Find(x => x.iIndexX == i);
                BaseMapData cTargetPoint = mapDataLib.BaseMapDataList.Find(x => x.iIndexX == i);
                cTargetPoint.eStatus = baseMapData.eStatus;
            }
            mapDataLib.OldMapDataReset();
        }

        /// <summary>
        /// MPC 데이터 복사
        /// </summary>
        /// <param name="mapDataLib"></param>
        public void MPC_DataCopy(MapDataLib mapDataLib)
        {
            if (strDataName == Enum.GetName(typeof(eData), eData.MPC1_NEEDLE_MOUNT))
            {
                if (GetStatus(eStatus.SKIPPED) == true)
                {
                    mapDataLib.SetAllStatus(eStatus.SKIPPED);
                }
                else if ((GetStatus(eStatus.SECOND_MOUNT) == true || GetStatus(eStatus.FIRST_MOUNT) == true) &&
                    GetStatus(eStatus.EMPTY) == false)
                {
                    mapDataLib.SetAllStatus(eStatus.SECOND_MOUNT);
                }
                else
                {
                    mapDataLib.SetAllStatus(eStatus.EMPTY);
                }
            }
            //else if (strDataName == Enum.GetName(typeof(eData), eData.MPC1_BUFFER_2))
            //{
            //    if (GetStatus(eStatus.PIPE_MAGNETIC) == true)
            //    {
            //        mapDataLib.SetAllStatus(eStatus.PIPE_MAGNETIC);
            //    }
            //    else if (GetStatus(eStatus.HOLDER) == true)
            //    {
            //        mapDataLib.SetAllStatus(eStatus.HOLDER);
            //    }
            //    else
            //    {
            //        mapDataLib.SetAllStatus(eStatus.EMPTY);
            //    }
            //}

            //else if (strDataName == Enum.GetName(typeof(eData), eData.MPC1_PIPE_MOUNT))
            //{
            //    if (GetStatus(eStatus.SKIPPED) == true)
            //    {
            //        mapDataLib.SetAllStatus(eStatus.SKIPPED);
            //    }
            //    else if (GetAllStatus(eStatus.PIPE_MAGNETIC, false) == true)
            //    {
            //        mapDataLib.SetAllStatus(eStatus.PIPE_MAGNETIC);
            //    }
            //    else
            //    {
            //        mapDataLib.SetAllStatus(eStatus.EMPTY);
            //    }
            //}
            else if (strDataName == Enum.GetName(typeof(eData), eData.MPC1_BUFFER_1))
            {
                if (GetStatus(eStatus.HOLDER) == true)
                {
                    for (int i = 0; i < CMainLib.Ins.cSysOne.bHolderNeedlePinWorkCount.Length; i++)
                    {
                        if (CMainLib.Ins.cSysOne.bHolderNeedlePinWorkCount[i] == (int)eNeedlePinStatus.Firstwork)
                        {
                            mapDataLib.SetStatus(i, eStatus.FIRST_HOLE);
                        }
                        else if (CMainLib.Ins.cSysOne.bHolderNeedlePinWorkCount[i] == (int)eNeedlePinStatus.Secondwork)
                        {
                            mapDataLib.SetStatus(i, eStatus.HOLDER);
                        }
                        else
                        {
                            mapDataLib.SetStatus(i, eStatus.NONE);
                        }
                    }
                }
                else
                {
                    mapDataLib.SetAllStatus(eStatus.EMPTY);
                }
            }
            else if (strDataName == Enum.GetName(typeof(eData), eData.MPC1_FAR_LEFT))
            {
                if (GetStatus(eStatus.HOLDER) == true)
                {
                    mapDataLib.SetAllStatus(eStatus.HOLDER);
                }
                else
                {
                    mapDataLib.SetAllStatus(eStatus.EMPTY);
                }
            }
            else
            {
                for (int i = 0; i < BaseMapDataList.Count; i++)
                {
                    BaseMapData baseMapData = BaseMapDataList.Find(x => x.iIndexX == i);
                    BaseMapData cTargetPoint = mapDataLib.BaseMapDataList.Find(x => x.iIndexX == i);
                    cTargetPoint.eStatus = baseMapData.eStatus;
                }
            }
            mapDataLib.OldMapDataReset();
        }

        /// <summary>
        /// Deep Copy
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            MapDataLib newCopy = new MapDataLib();
            newCopy.BaseMapDataList = this.BaseMapDataList;
            newCopy.OldMapDataReset();
            return newCopy;
        }
    }
}