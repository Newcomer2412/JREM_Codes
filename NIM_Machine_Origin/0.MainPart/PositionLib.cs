using System;
using System.Collections.Generic;
using System.Linq;
using EMotionSnetBase;

namespace MachineControlBase
{
    #region Motor & Index 정의

    /// <summary>
    /// Motor Name Define
    /// </summary>
    public enum eMotor : int
    {
        /// <summary>
        /// Tray에 담긴 홀더를 MPC에 공급 및 회수하는 Y축
        /// </summary>
        HOLDER_TRAY_Y = 0,

        /// <summary>
        /// Tray에 담긴 홀더를 MPC에 공급 및 회수하는 X축
        /// </summary>
        HOLDER_LOADING_X = 1,

        /// <summary>
        /// MPC 레일 전면 X축
        /// </summary>
        MPC_FRONT_X = 2,

        /// <summary>
        /// MPC 레일 후면 X축
        /// </summary>
        MPC_REAR_X = 3,

        /// <summary>
        /// MPC 왼쪽 Y축
        /// </summary>
        MPC_LEFT_Y = 4,

        /// <summary>
        /// MPC 오른쪽 Y축
        /// </summary>
        MPC_RIGHT_Y = 5,

        /// <summary>
        /// 피더와 파이프 트렌스퍼 실린더를 오가는 P&P X축
        /// </summary>
        PIPE_PnP_X = 6,

        /// <summary>
        /// 피더와 파이프 트렌스퍼 실린더를 오가는 P&P Y축
        /// </summary>
        PIPE_PnP_Y = 7,

        /// <summary>
        /// 피더와 파이프 트렌스퍼 실린더를 오가는 P&P Y축
        /// </summary>
        PIPE_PnP_Z = 8,

        /// <summary>
        /// 피더와 니들 트렌스퍼 실린더를 오가는 P&P X축
        /// </summary>
        NEEDLE_PnP_X = 9,

        /// <summary>
        /// 피더와 니들 트렌스퍼 실린더를 오가는 P&P X축
        /// </summary>
        NEEDLE_PnP_Y = 10,

        /// <summary>
        /// 피더와 니들 트렌스퍼 실린더를 오가는 P&P X축
        /// </summary>
        NEEDLE_PnP_Z = 11,

        /// <summary>
        /// 파이프를 홀더에 MOUNT하는 X축
        /// </summary>
        PIPE_MOUNT_X = 12,

        /// <summary>
        /// 파이프를 홀더에 MOUNT하는 Y축
        /// </summary>
        PIPE_MOUNT_Y = 13,

        /// <summary>
        /// 파이프를 홀더에 MOUNT하는 Z축
        /// </summary>
        PIPE_MOUNT_Z = 14,

        /// <summary>
        /// 니들를 홀더에 MOUNT하는 X축
        /// </summary>
        NEEDLE_MOUNT_X = 15,

        /// <summary>
        /// 니들를 홀더에 MOUNT하는 Y축
        /// </summary>
        NEEDLE_MOUNT_Y = 16,

        /// <summary>
        /// 니들를 홀더에 MOUNT하는 Z축
        /// </summary>
        NEEDLE_MOUNT_Z = 17,

        /// <summary>
        /// 홀더 용액 분주 Y축
        /// </summary>
        DISPENSER_Y = 18,

        /// <summary>
        /// 피더에서 파이프 픽업시 파이프 방향대로 움직이는 T축
        /// </summary>
        PIPE_PnP_T = 19,

        /// <summary>
        /// 피더에서 니들 픽업시 니들 방향대로 움직이는 T축
        /// </summary>
        NEEDLE_PnP_T = 20,

        /// <summary>
        /// 파이프 트렌스퍼 실린더에서 파이프를 회전하는 R축
        /// </summary>
        PIPE_ROTATE = 21,

        /// <summary>
        /// 니들 트렌스퍼 실린더에서 니들을 회전하는 R축
        /// </summary>
        NEEDLE_ROTATE = 22,

        /// <summary>
        /// 홀더를 뒤집는 T축
        /// </summary>
        FLIP_T = 23,

        /// <summary>
        /// 홀더를 뒤집기 위해 UP, DOWN하는 Z축
        /// </summary>
        FLIP_Z = 24,

        /// <summary>
        /// 홀더 용액 분주 X축
        /// </summary>
        DISPENSER_X = 25,

        /// <summary>
        /// 홀더 용액 분주 Z축
        /// </summary>
        DISPENSER_Z = 26,

        /// <summary>
        /// 파이프를 바스켓에 떨구기위해 진동을 이르키는 축
        /// </summary>
        PIPE_HOPPER = 27,

        /// <summary>
        /// 파이프를 바스켓에 떨구기위해 진동을 이르키는 축
        /// </summary>
        NEEDLE_HOPPER = 28,

        /// <summary>
        /// NONE
        /// </summary>
        MOT_NONE = 999,
    }

    /// <summary>
    /// Tray에 담긴 홀더를 MPC로 이동하는 Y축 위치 정의
    /// </summary>
    public enum eAxisHolderTray_Y : int
    {
        Loading = 0,
        Y_Pitch = 1,
    }

    /// <summary>
    /// Tray에 담긴 홀더를 MPC로 이동하는 X축 위치 정의
    /// </summary>
    public enum eAxisHolderPicker_X : int
    {
        Safe = 0,
        TraySupply = 1,
        TrayRecovery = 2,
        PalletPlace = 3,
        PalletPickUp = 4,
        NG_Place = 5,
        Tray_X_Pitch = 6,
    }

    /// <summary>
    /// MPC 전면 레일 축 위치 정의
    /// </summary>
    public enum eAxisFrontMPC : int
    {
        MovePitch = 0,
    }

    /// <summary>
    /// MPC 후면 레일 축 위치 정의
    /// </summary>
    public enum eAxisRearMPC : int
    {
        UV_Zone = 0,
        Unload_Pitch = 1,
    }

    /// <summary>
    /// MPC 왼쪽 레일 Y축 위치 정의
    /// </summary>
    public enum eAxisLeftMPC_Y : int
    {
        Front = 0,
        HolderSupply = 1,
        Rear = 2,
    }

    /// <summary>
    /// MPC 오른쪽 레일 Y축 위치 정의
    /// </summary>
    public enum eAxisRightMPC_Y : int
    {
        Front = 0,
        Rear = 1,
    }

    /// <summary>
    /// 파이프 이송 PnP X축 위치 정의
    /// </summary>
    public enum eAxisPipePnP_X : int
    {
        Safe = 0,
        PipePickUp = 1,
        PipePlaceDown = 2,
        PipeFeederJig = 3,
    }

    /// <summary>
    /// 파이프 이송 PnP Y축 위치 정의
    /// </summary>
    public enum eAxisPipePnP_Y : int
    {
        Safe = 0,
        PipePickUp = 1,
        PipePlaceDown = 2,
        PipeFeederJig = 3,
    }

    /// <summary>
    /// 파이프 이송 PnP Z축 위치 정의
    /// </summary>
    public enum eAxisPipePnP_Z : int
    {
        Safe = 0,
        PipePickUp = 1,
        PipePlaceDown = 2,
        CalDown = 3,
    }

    /// <summary>
    /// 파이프 이송 PnP T축 위치 정의
    /// </summary>
    public enum eAxisPipePnP_T : int
    {
        Origin = 0,
        PipePlace = 1,
    }

    /// <summary>
    /// 파이프 회전 R축 위치 정의
    /// </summary>
    public enum eAxisPipeRotate : int
    {
        Origin = 0,
        Rotate90 = 1,
        Trash = 2,
    }

    /// <summary>
    /// 니들 이송 PnP X축 위치 정의
    /// </summary>
    public enum eAxisNeedlePnP_X : int
    {
        Safe = 0,
        NeedlePickUp = 1,
        NeedlePlaceDown = 2,
        NeedleFeederJig = 3,
    }

    /// <summary>
    /// 니들 이송 PnP Y축 위치 정의
    /// </summary>
    public enum eAxisNeedlePnP_Y : int
    {
        Safe = 0,
        NeedlePickUp = 1,
        NeedlePlaceDown = 2,
        NeedleFeederJig = 3,
    }

    /// <summary>
    /// 니들 이송 PnP Z축 위치 정의
    /// </summary>
    public enum eAxisNeedlePnP_Z : int
    {
        Safe = 0,
        NeedlePickUp = 1,
        NeedlePlaceDown = 2,
        CalDown = 3,
    }

    /// <summary>
    /// 니들 이송 PnP T축 위치 정의
    /// </summary>
    public enum eAxisNeedlePnP_T : int
    {
        Origin = 0,
        NeedlePlace = 1,
    }

    /// <summary>
    /// 니들 회전 R축 위치 정의
    /// </summary>
    public enum eAxisNeedleRotate : int
    {
        Origin = 0,
        Rotate90 = 1,
        Rotate180 = 2,
        Trash = 3,
    }

    /// <summary>
    /// 홀더에 파이프 마운트 X축 위치 정의
    /// </summary>
    public enum eAxisPipeMount_X : int
    {
        Safe = 0,
        PipeClampUp = 1,
        PipeMountCenter = 2,
    }

    /// <summary>
    /// 홀더에 파이프 마운트 Y축 위치 정의
    /// </summary>
    public enum eAxisPipeMount_Y : int
    {
        Safe = 0,
        PipeClampUp = 1,
        PipeMountVision = 2,
        PipeMountCenter = 3,
        MagneticInjection = 4,
    }

    /// <summary>
    /// 홀더에 파이프 마운트 Z축 위치 정의
    /// </summary>
    public enum eAxisPipeMount_Z : int
    {
        Safe = 0,
        PipeClampUp = 1,
        PipeMountHolder = 2,
        MountSlow = 3,
    }

    /// <summary>
    /// 홀더에 니들 마운트 X축 위치 정의
    /// </summary>
    public enum eAxisNeedleMount_X : int
    {
        Safe = 0,
        NeedleClampUp = 1,
        NeedleClampUp180 = 2,
        NeedleMountCenter = 3,
    }

    /// <summary>
    /// 홀더에 니들 마운트 Y축 위치 정의
    /// </summary>
    public enum eAxisNeedleMount_Y : int
    {
        Safe = 0,
        NeedleClampUp = 1,
        NeedleClampUp180 = 2,
        NeedleMountVision = 3,
        NeedleMountCenter = 4,
        ClampVacuumPitch = 5,
    }

    /// <summary>
    /// 홀더에 니들 마운트 Z축 위치 정의
    /// </summary>
    public enum eAxisNeedleMount_Z : int
    {
        Safe = 0,
        NeedleClampUp = 1,
        NeedleMountHolder = 2,
        MountSlow = 3,
    }

    /// <summary>
    /// 홀더 분주 X축 위치 정의
    /// </summary>
    public enum eAxisDispenser_X : int
    {
        Safe = 0,
        Clean = 1,
        NeedleFlat = 2,
        HolderVision = 3,
        Dispensing = 4,
        NozzleHeigh = 5,
    }

    /// <summary>
    /// 홀더 분주 Y축 위치 정의
    /// </summary>
    public enum eAxisDispenser_Y : int
    {
        Safe = 0,
        Clean = 1,
        NeedleFlat = 2,
        HolderVision = 3,
        Dispensing = 4,
        NozzleHeigh = 5,
    }

    /// <summary>
    /// 홀더 분주 Z축 위치 정의
    /// </summary>
    public enum eAxisDispenser_Z : int
    {
        Safe = 0,
        Clean = 1,
        HolderVision = 2,
        Dispensing = 3,
        NozzleHeigh = 4,
        NozzleHeighSlow = 5,
    }

    /// <summary>
    /// 홀더 Flip T축 위치 정의
    /// </summary>
    public enum eAxisFlip_T : int
    {
        Origin = 0,
        Flip = 1,
    }

    /// <summary>
    /// 홀더 Flip Z축 위치 정의
    /// </summary>
    public enum eAxisFlip_Z : int
    {
        FlipUp = 0,
        FlipDown = 1,
    }

    #endregion Motor & Index 정의

    #region 각 Axis Parameter

    [Serializable]
    public class AxisParam
    {
        public AxisParam()
        { }

        /// <summary>
        /// Axis No
        /// </summary>
        public eMotor eAxis = eMotor.MOT_NONE;

        /// <summary>
        /// 제어기상 축의 번호
        /// </summary>
        public uint uiControllerAxis;

        /// <summary>
        /// Axis 이송 속도
        /// </summary>
        public uint uiVel = 100;

        /// <summary>
        /// 조그 속도
        /// </summary>
        public uint uiJogVel = 10;

        /// <summary>
        /// Axis Accel 값
        /// </summary>
        public uint uiAccel = 100;

        /// <summary>
        /// Axis Decel 값
        /// </summary>
        public uint uiDecel = 100;

        /// <summary>
        /// 축 스케일 (pulse/mm 1펄스에 몇 mm or pulse/degree 1펄스에 몇 도
        /// </summary>
        public double dScale = 1000;

        /// <summary>
        /// 홈 동작 속도
        /// </summary>
        public uint uiHomeVel = 10;

        /// <summary>
        /// 홈 동작 가속
        /// </summary>
        public uint uiHomeAcc = 500;

        /// <summary>
        /// 홈 동작 감속
        /// </summary>
        public uint uiHomeDec = 500;

        /// <summary>
        /// 홈 동작 완료 후 오프셋(이동 거리)
        /// </summary>
        public double dHomeOffset = 0.0;

        /// <summary>
        /// 홈 동작 초기화 시간
        /// </summary>
        public uint uiHomeClearTime = 500;

        /// <summary>
        /// 홈 동작 Z상 사용 유무
        /// </summary>
        public bool bHomeUseZPhase = true;

        /// <summary>
        /// 홈 동작 검색 센서
        /// </summary>
        public int iHomeSenserType = (int)SnetDevice.eSnetAxisSensor.HomeSensor; // Z상으로만 잡을 경우 홈 잡는 방식 다르게 하기위해

        /// <summary>
        /// 홈 동작 방향
        /// </summary>
        public int iHomeMoveDirection = (int)SnetDevice.eSnetMoveDirection.Negative;

        /// <summary>
        /// 홈 동작 타임 아웃 (sec)
        /// </summary>
        public uint uiHomeTimeout = 180;

        /// <summary>
        /// 모터 동작 타임아웃(sec)
        /// </summary>
        public uint uiMotionTimeout = 10;

        /// <summary>
        /// 시뮬레이션일 때, 모터 동작 완료 여부
        /// </summary>
        public bool bSimMoveDone = true;

        /// <summary>
        /// 시뮬레이션일 때, 현재 모터 값
        /// </summary>
        public double dSimCmdPos = 0.0;
    }

    #endregion 각 Axis Parameter

    #region 각 축 위치 값 정의

    /// <summary>
    /// Motion Position 정보 Class
    /// </summary>
    [Serializable]
    public class PositionData
    {
        public PositionData()
        { }

        /// <summary>
        /// Axis No
        /// </summary>
        public eMotor eAxis = eMotor.MOT_NONE;

        /// <summary>
        /// Position No
        /// </summary>
        public int iNo = 0;

        /// <summary>
        /// Position 의 이름
        /// </summary>
        public string strName = string.Empty;

        /// <summary>
        /// Position Data
        /// </summary>
        public double dPos = 0;

        /// <summary>
        /// Position Speed(%)
        /// </summary>
        public uint uiSpeed = 100;

        /// <summary>
        /// 절대 위치(true) or 상대 위치(false) 여부
        /// </summary>
        public bool bAbsOrRel = true;
    }

    /// <summary>
    /// Axis Position Data List
    /// </summary>
    [Serializable]
    public class AxisPositionData
    {
        /// <summary>
        /// Axis 축 번호
        /// </summary>
        public eMotor eAxis = eMotor.MOT_NONE;

        /// <summary>
        /// Position List
        /// </summary>
        public List<PositionData> cPositionDataList = new List<PositionData>();

        /// <summary>
        /// Position Data를 No 별로 정렬
        /// </summary>
        public void OrderByNo()
        {
            cPositionDataList = cPositionDataList.OrderBy(x => x.iNo).ToList();
        }

        /// <summary>
        /// 생성자 - Serialize를 진행하기 위해 기본 생성자를 정의한다.
        /// </summary>
        public AxisPositionData()
        { }

        /// <summary>
        /// 생성자 - 축번호를 설정
        /// </summary>
        /// <param name="eAxis"></param>
        public AxisPositionData(eMotor eAxis)
        {
            this.eAxis = eAxis;
        }

        /// <summary>
        /// Axis축의 Position Data를 List 객체에 저장
        /// </summary>
        /// <param name="iNo">Position No </param>
        /// <param name="strName">Position 의 이름 </param>
        /// <param name="dPos">Position Data - 실제 이동할 자료 </param>
        /// <param name="uiSpeed">Position Speed </param>
        /// <param name="bAbsOrRel">절대위치 or 상대위치 여부</param>
        public void AddPositionData(int iNo, string strName, double dPos, uint uiSpeed, bool bAbsOrRel)
        {
            PositionData cPositionData = cPositionDataList.Find(x => x.iNo == iNo);
            if (cPositionData == null)
            {
                cPositionData = new PositionData();
                cPositionData.iNo = iNo;
                cPositionData.strName = strName;
                cPositionData.dPos = dPos;
                cPositionData.uiSpeed = uiSpeed;
                cPositionData.bAbsOrRel = bAbsOrRel;
                cPositionDataList.Add(cPositionData);
            }
            else
            {
                cPositionData.iNo = iNo;
                cPositionData.strName = strName;
                cPositionData.dPos = dPos;
                cPositionData.uiSpeed = uiSpeed;
                cPositionData.bAbsOrRel = bAbsOrRel;
            }
        }

        /// <summary>
        /// Axis축의 Position Data를 List 객체에 저장
        /// </summary>
        /// <param name="positionData"></param>
        public void AddPositionData(PositionData positionData)
        {
            PositionData cPositionData = cPositionDataList.Find(x => x.iNo == positionData.iNo);
            if (cPositionData == null)
            {
                cPositionDataList.Add(positionData);
            }
            else
            {
                cPositionData = positionData;
            }
        }

        /// <summary>
        /// 해당축의 Position No를 이용해서 Position Data를 가져옴
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public PositionData GetPositionData(int index)
        {
            PositionData GetData = cPositionDataList.Find(x => x.iNo == index);
            if (GetData == null)
            {
                GetData = new PositionData();
                GetData.iNo = index;
                AddPositionData(GetData);
            }
            return GetData;
        }

        /// <summary>
        /// 해당축의 Position No를 이용해서 Position을 가져옴
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double GetPosition(int index)
        {
            PositionData GetData = cPositionDataList.Find(x => x.iNo == index);
            if (GetData == null)
            {
                GetData = new PositionData();
                GetData.iNo = index;
                AddPositionData(GetData);
            }
            return GetData.dPos;
        }
    }

    /// <summary>
    /// Model Position Data List(가장 큰 단위)
    /// </summary>
    [Serializable]
    public class ModelAxisPositionData : ICloneable
    {
        /// <summary>
        /// 모델 번호
        /// </summary>
        public uint uiModelNo = 0;

        /// <summary>
        /// Position 저장 리스트
        /// </summary>
        public List<AxisPositionData> cAxisPositonDataList = new List<AxisPositionData>();

        /// <summary>
        /// 생성자
        /// </summary>
        public ModelAxisPositionData()
        { }

        /// <summary>
        /// 생성자 - Model 데이터 번호를 설정
        /// </summary>
        /// <param name="uiModelNo"></param>
        public ModelAxisPositionData(uint uiModelNo)
        {
            this.uiModelNo = uiModelNo;
        }

        /// <summary>
        /// Deep Copy
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            ModelAxisPositionData copy = new ModelAxisPositionData();
            copy.cAxisPositonDataList = this.cAxisPositonDataList;
            return copy;
        }

        /// <summary>
        /// Position Data를 No 별로 정렬
        /// </summary>
        public void OrderByNo()
        {
            foreach (AxisPositionData cAxisData in cAxisPositonDataList)
            {
                cAxisData.OrderByNo();
            }
        }

        /// <summary>
        /// Axis Data를 List 객체에 저장
        /// </summary>
        /// <param name="axisPostionData"></param>
        public void AddAxisPositionData(AxisPositionData axisPostionData)
        {
            AxisPositionData cAxisPositionData = cAxisPositonDataList.Find(x => x.eAxis == axisPostionData.eAxis);
            if (cAxisPositionData == null)
            {
                cAxisPositonDataList.Add(axisPostionData);
            }
            else
            {
                cAxisPositionData = axisPostionData;
            }
        }

        /// <summary>
        /// 축의 번호를 이용해 Position List를 가져옴
        /// </summary>
        /// <param name="eAxis"></param>
        /// <returns></returns>
        public AxisPositionData GetAxisPositionData(eMotor eAxis)
        {
            AxisPositionData cAxisPositionData = cAxisPositonDataList.Find(x => x.eAxis == eAxis);
            if (cAxisPositionData == null)
            {
                cAxisPositionData = new AxisPositionData();
                cAxisPositionData.eAxis = eAxis;
                AddAxisPositionData(cAxisPositionData);
            }
            return cAxisPositionData;
        }
    }

    #endregion 각 축 위치 값 정의

    #region Axis Param

    /// <summary>
    /// 각 축의 속도, 홈 동작 등 파라미터 저장
    /// </summary>
    [Serializable]
    public class CAxisParameterCollectionData
    {
        /// <summary>
        /// Axis 파라미터 저장 리스트
        /// </summary>
        public List<AxisParam> cAxisPramList = new List<AxisParam>();

        /// <summary>
        /// Axis 파라미터 정보 클래스를 검색하여 반환한다.
        /// </summary>
        /// <param name="eAxis"></param>
        /// <returns></returns>
        public AxisParam GetAxisParam(eMotor eAxis)
        {
            AxisParam cAxisPram = cAxisPramList.Find(x => x.eAxis == eAxis);

            if (cAxisPram == null)
            {
                cAxisPram = new AxisParam();
                cAxisPram.eAxis = eAxis;
                cAxisPramList.Add(cAxisPram);
            }
            return cAxisPram;
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public CAxisParameterCollectionData()
        {
        }
    }

    #endregion Axis Param

    #region Teaching Point

    /// <summary>
    /// 모델별 각 축의 티칭 포인트 저장
    /// </summary>
    [Serializable]
    public class CAxisPositionCollectionData
    {
        /// <summary>
        /// Position 저장 리스트
        /// </summary>
        public List<ModelAxisPositionData> cModelAxisPositionDataList = new List<ModelAxisPositionData>();

        /// <summary>
        /// Axis 축의 Position Data를 얻어온다.
        /// 만약 자료가 없으면 해당 Axis No에 해당하는 "AxisData"를 생성한다.
        /// </summary>
        /// <param name="eAxis"></param>
        /// <returns></returns>
        public AxisPositionData GetAxisPositionData(eMotor eAxis)
        {
            uint uiCurrentModelNo = CMainLib.Ins.cSysOne.uiCurrentModelNo;
            ModelAxisPositionData cModelAxisPositionData = cModelAxisPositionDataList.Find(x => x.uiModelNo == uiCurrentModelNo);

            if (cModelAxisPositionData == null)
            {
                cModelAxisPositionData = new ModelAxisPositionData(uiCurrentModelNo);
                cModelAxisPositionDataList.Add(cModelAxisPositionData);
            }
            return cModelAxisPositionData.GetAxisPositionData(eAxis);
        }

        /// <summary>
        /// 좌표 값을 얻어온다.
        /// </summary>
        /// <param name="eAxis"></param>
        /// <param name="iIndexNo"></param>
        /// <returns></returns>
        public double GetAxisPosition(eMotor eAxis, int iIndexNo)
        {
            AxisPositionData axisDataLib = GetAxisPositionData(eAxis);
            return axisDataLib.GetPositionData(iIndexNo).dPos;
        }

        /// <summary>
        /// 좌표 클래스를 얻어온다.
        /// </summary>
        /// <param name="eAxis"></param>
        /// <param name="iIndexNo"></param>
        /// <returns></returns>
        public PositionData GetPositionData(eMotor eAxis, int iIndexNo)
        {
            AxisPositionData axisDataLib = GetAxisPositionData(eAxis);
            return axisDataLib.GetPositionData(iIndexNo);
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public CAxisPositionCollectionData()
        {
            cModelAxisPositionDataList.Clear();
        }

        /// <summary>
        /// 위치 데이터 삭제
        /// </summary>
        /// <param name="uiTarget"></param>
        public void DataClear(uint uiTarget)
        {
            ModelAxisPositionData cModelAxisData = cModelAxisPositionDataList.Find(x => x.uiModelNo == uiTarget);
            if (cModelAxisPositionDataList != null) cModelAxisPositionDataList.Remove(cModelAxisData);
        }

        /// <summary>
        /// 위치 데이터 복사
        /// </summary>
        /// <param name="uiTarget"></param>
        /// <param name="uiSource"></param>
        public void DataCopy(uint uiTarget, uint uiSource)
        {
            DataClear(uiTarget);
            ModelAxisPositionData cModelAxisData = cModelAxisPositionDataList.Find(x => x.uiModelNo == uiSource);
            ModelAxisPositionData cNewModelAxisData = cModelAxisData.Clone() as ModelAxisPositionData;
            cNewModelAxisData.uiModelNo = uiTarget;
            cModelAxisPositionDataList.Add(cNewModelAxisData);
        }
    }

    #endregion Teaching Point
}