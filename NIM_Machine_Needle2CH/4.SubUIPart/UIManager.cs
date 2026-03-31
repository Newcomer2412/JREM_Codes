using System.Collections.Generic;
using System.Windows;

namespace MachineControlBase
{
    /// <summary>
    /// 데이터 에디트 유저 컨트롤 인터페이스
    /// </summary>
    public interface IUIEditData
    {
        /// <summary>
        /// 초기화
        /// </summary>
        void Init();

        /// <summary>
        /// 저장 데이터 -> UI 데이터
        /// </summary>
        void LoadData();

        /// <summary>
        /// UI 데이터 -> 저장 데이터
        /// </summary>
        void SaveData();
    }

    /// <summary>
    /// 맵 데이터 유저 컨트롤 인터페이스
    /// </summary>
    public interface IUIMapData
    {
        /// <summary>
        /// 초기화
        /// </summary>
        void Init();

        /// <summary>
        /// 정리
        /// </summary>
        void Free();

        /// <summary>
        /// Old 데이터 초기화
        /// </summary>
        void ResetOldMapData();

        /// <summary>
        /// 데이터 갱신 함수
        /// </summary>
        void RepeatUpdateTimer();

        /// <summary>
        /// 맵 데이터 다시 그림
        /// </summary>
        void UpdateMapData();
    }

    /// <summary>
    /// UserControl 관리 Class
    /// 1. UserControl의 Control 수집
    /// 2. UserControl의 Control 데이터 초기화
    /// 3. UserControl의 Control 데이터 반복 갱신
    /// </summary>
    public class CUIManager
    {
        public CUIManager()
        { }

        /// <summary>
        /// Axis Position UI List
        /// </summary>
        public List<IUIEditData> ListEditUI = new List<IUIEditData>();

        /// <summary>
        /// Actuator UI List
        /// </summary>
        public List<IOActuatorUI> ListIOActuatorUI = new List<IOActuatorUI>();

        /// <summary>
        /// User IO UI List
        /// </summary>
        public List<IOUserUI> ListIOUserUI = new List<IOUserUI>();

        /// <summary>
        /// Actuator UI Monitor List
        /// </summary>
        public List<IOActuatorUI> ListMonitorIOActuatorUI = new List<IOActuatorUI>();

        /// <summary>
        /// User IO UI Monitor List
        /// </summary>
        public List<IOUserUI> ListMonitorIOUserUI = new List<IOUserUI>();

        /// <summary>
        /// UI의 각 컨트롤을 검색하여 리스트에 저장함(하부 Object까지 모두 검색 등록)
        /// </summary>
        /// <param name="Controls"></param>
        public void UserContorListAddControl(DependencyObject Controls)
        {
            // 컨트롤 검색하여 저장
            ListEditUI.AddRange(CCommon.GetLogicalChildCollection<AxisPositionUI>(Controls));
            ListIOActuatorUI.AddRange(CCommon.GetLogicalChildCollection<IOActuatorUI>(Controls));
            ListIOUserUI.AddRange(CCommon.GetLogicalChildCollection<IOUserUI>(Controls));
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Init()
        {
            foreach (IOUserUI IOUserUI in ListIOUserUI)
            {
                IOUserUI.Init();
            }

            foreach (IOActuatorUI IOActuatorUI in ListIOActuatorUI)
            {
                IOActuatorUI.Init();
            }

            // 실린더 딜레이 Init
            CMainLib.Ins.Seq.SeqIO.CylinderListInit();
        }

        /// <summary>
        /// UI의 각 컨트롤을 검색하여 Monitor 리스트에 저장 후 반복 데이터 갱신
        /// </summary>
        /// <param name="Controls"></param>
        public void UserContorMonitorListAddControl(DependencyObject Controls)
        {
            // 컨트롤 Clear
            ListMonitorIOActuatorUI.Clear();
            ListMonitorIOUserUI.Clear();

            // 컨트롤 검색하여 저장
            ListMonitorIOActuatorUI = CCommon.GetLogicalChildCollection<IOActuatorUI>(Controls);
            ListMonitorIOUserUI = CCommon.GetLogicalChildCollection<IOUserUI>(Controls);
        }

        /// <summary>
        /// 반복하여 컨트롤 데이터 갱신용 함수
        /// </summary>
        public void RepeatTimer()
        {
            foreach (IOActuatorUI IOActuatorUI in ListMonitorIOActuatorUI)
            {
                IOActuatorUI.RepeatUpdateTimer();
            }
            foreach (IOUserUI IOUserUI in ListMonitorIOUserUI)
            {
                IOUserUI.RepeatUpdateTimer();
            }
            foreach (IUIMapData MonitorMapData in ListMonitorMapData)
            {
                MonitorMapData.RepeatUpdateTimer();
            }
        }

        /// <summary>
        /// 데이터를 로드하여 UI 데이터를 갱신한다.
        /// </summary>
        public void LoadData()
        {
            foreach (IUIEditData EditUI in ListEditUI)
            {
                EditUI.LoadData();
            }
        }

        /// <summary>
        /// UI의 데이터를 저장한다.
        /// </summary>
        public void SaveData()
        {
            foreach (IUIEditData EditUI in ListEditUI)
            {
                EditUI.SaveData();
            }
        }

        ///////////////////////// Map Data UI ////////////////////////////

        /// <summary>
        /// Content Map Data UI List
        /// </summary>
        public List<IUIMapData> ListMapData = new List<IUIMapData>();

        /// <summary>
        /// Monitor Content Map Data UI List
        /// </summary>
        public List<IUIMapData> ListMonitorMapData = new List<IUIMapData>();

        /// <summary>
        /// Map Data UI의 각 컨트롤을 검색하여 리스트에 저장함(하부 Object까지 모두 검색 등록)
        /// </summary>
        /// <param name="Controls"></param>
        public void MapDataUIListAddControl(DependencyObject Controls)
        {
            // 컨트롤 검색하여 저장
            ListMapData.AddRange(CCommon.GetLogicalChildCollection<MapDataUI>(Controls));
        }

        /// <summary>
        /// Map Data UI의 각 컨트롤을 검색하여 DataEdit 모니터링용 리스트에 저장함(하부 Object까지 모두 검색 등록)
        /// </summary>
        /// <param name="Controls"></param>
        public void DataEditMonitorListAddControl(DependencyObject Controls)
        {
            // 컨트롤 Clear
            ListMonitorMapData.Clear();
            // 컨트롤 검색하여 저장
            ListMonitorMapData.AddRange(CCommon.GetLogicalChildCollection<MapDataUI>(Controls));
        }

        /// <summary>
        /// 맵 데이터 다시 그리기
        /// </summary>
        public void UpdateMapData()
        {
            foreach (IUIMapData MapData in ListMapData)
            {
                MapData.UpdateMapData();
            }
        }

        /// <summary>
        /// 모니터 맵 데이터 다시 그리기
        /// </summary>
        public void UpdateMonitorMapData()
        {
            foreach (IUIMapData MapData in ListMonitorMapData)
            {
                MapData.UpdateMapData();
            }
        }

        /// <summary>
        /// Map Data 초기화
        /// </summary>
        public void MapDataInit()
        {
            foreach (IUIMapData MapData in ListMapData)
            {
                MapData.Init();
            }
            foreach (IUIMapData MapData in ListMonitorMapData)
            {
                MapData.Init();
            }
        }

        /// <summary>
        /// 팝업 창이 열려 있으면 닫고 초기화
        /// </summary>
        public void PopupClose()
        {
            foreach (IUIMapData MapData in ListMapData)
            {
                MapData.Free();
            }
            foreach (IUIMapData MapData in ListMonitorMapData)
            {
                MapData.Free();
            }
        }

        /// <summary>
        /// 반복하여 컨트롤 데이터 갱신용 함수
        /// </summary>
        public void MapDataRepeatTimer()
        {
            foreach (IUIMapData MapData in ListMapData)
            {
                MapData.RepeatUpdateTimer();
            }
        }
    }
}