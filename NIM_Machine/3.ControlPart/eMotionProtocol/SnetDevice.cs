using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net;

namespace EMotionSnetBase
{
	/// <summary>
	/// SNET device API wrapper class
	/// </summary>
	public class SnetDevice
	{
		#region API Define

		/// <summary>
		/// Return Code for EMOTION SNET API
		/// </summary>
		public enum eSnetApiReturnCode
		{
			/// .Net Exception Error
			Exception			= -1,

			/// Communication Return
			Success				= 0,
			NotConnected		= Success + 1,
			FailedConnect		= Success + 2,
			AlreadyConnected	= Success + 3,
			InvalidArgument		= Success + 8,
			FailedCommunication = Success + 13,
			TimeOut				= Success + 14,
			InvalidCommand		= Success + 15,
			NotReceive			= Success + 20,
			Disconnected		= Success + 50,
			Disconnecting		= Success + 51,
			NotInitialize		= Success + 100,
			FailedInitialize	= Success + 101,
			OutOfMemory			= Success + 110,
			DataOverflow		= Success + 111,
			DataUnderflow		= Success + 112,
			NullPtr				= Success + 113,
			SizeOverflow		= Success + 114,
			SizeUnderflow		= Success + 115,
			NotSupported		= Success + 200,

			/// Origin Return
			OriginErrorBase						= 300,
			OriginErrorCommand					= OriginErrorBase + 10,		// Homing 관련 지령이 잘못 되었을 때

			OriginPreLimitMinusOn				= OriginErrorBase + 20,		// Homing 구동 전 (-)Limit Sensor가 감지되어 있을 때
			OriginPreLimitPlusOn				= OriginErrorBase + 21,		// Homing 구동 전 (+)Limit Sensor가 감지되어 있을 때
			OriginPreSvOff						= OriginErrorBase + 22,		// Homing 구동 전 Servo On이 실행되지 않을 때
			OriginPreIsNotStepContents			= OriginErrorBase + 23,		// Homing 구동 전 homing Job이 제대로 만들어지지 않았을 때

			OriginSvOff							= OriginErrorBase + 30,		// homing 구동 중 Servo Off가 되었을 때

			OriginHomeSensorOnAtFalling			= OriginErrorBase + 40,		// Home Sensor가 On-->Off로 구동 하였는데, 계속 On 상태로 인식 될 때
			OriginHomeSensorOffAtRising			= OriginErrorBase + 41,		// Home Sensor가 Off-->On으로 구동 하였는데, 계속 Off 상태로 인식 될 때
			OriginLimitMinusOnAtHomeSensor		= OriginErrorBase + 42,		// Home Sensor의 On/Off 구동 중 (-)Limit Sensor가 감지 될 때
			OriginLimitPlusOnAtHomeSensor		= OriginErrorBase + 43,		// Home Sensor의 On/Off 구동 중 (+)Limit Sensor가 감지 될 때

			OriginLimitMinusOffAtRising			= OriginErrorBase + 50,		// (-)Limit Sensor를 Off-->On으로 구동 하였는데, 계속 Off 상태로 인식 될 때
			OriginLimitMinusOnAtFalling			= OriginErrorBase + 51,		// (-)Limit Sensor를 On-->Off로 구동 하였는데, 계속 On 상태로 인식 될 때
			OriginLimitPlusOnAtLimitMinus		= OriginErrorBase + 52,		// (-)Limit Sensor의 On/Off 구동 중 (+)Limit Sensor가 감지 될 때

			OriginLimitPlusOffAtRising			= OriginErrorBase + 60,		// (+)Limit Sensor를 Off-->On으로 구동 하였는데, 계속 Off 상태로 인식 될 때
			OriginLimitPlusOnAtFalling			= OriginErrorBase + 61,		// (+)Limit Sensor를 On-->Off로 구동 하였는데, 계속 On 상태로 인식 될 때
			OriginLimitMinusOnAtLimitPlus		= OriginErrorBase + 62,		// (+)Limit Sensor의 On/Off 구동 중 (-)Limit Sensor가 감지 될 때

			OriginHomeSensorOnAtZPhase			= OriginErrorBase + 70,		// Z(C)상 Capture 구동 시 Home Sensor 감지 될 때
			OriginLimitMinusOnAtZPhase			= OriginErrorBase + 71,		// Z(C)상 Capture 구동 시 (-)Limit Sensor 감지 될 때
			OriginLimitPlusOnAtZPhase			= OriginErrorBase + 72,		// Z(C)상 Capture 구동 시 (+)Limit Sensor 감지 될 때

			OriginLimitMinusOnAtOffsetMove		= OriginErrorBase + 80,		// zero offset 구동 시 (-)Limit Sensor 감지 될 때
			OriginLimitPlusOnAtOffsetMove		= OriginErrorBase + 81,		// zero Offset 구동 시 (+)Limit Sensor 감지 될 때
			OriginPositionDismatchAtOffsetMove	= OriginErrorBase + 82,		// zero offset 구동 시 지령 위치와 다른 위치에서 정지했을 때

			OriginDismatchNetId					= OriginErrorBase + 90,		// 지령시에 Net ID가 불일치 할 때
			OriginCommandReturnTimeOut			= OriginErrorBase + 91,		// 지령 후 통신 리턴 time out 발생 되었을 때
			OriginCommandWrongArgument			= OriginErrorBase + 92,		// 지령 인자 값이 잘못 되었을 때
			OriginCommandWrongReturn			= OriginErrorBase + 93,		// 지령 값과 통신 리턴 Token ID가 불일치 할 때
			OriginCommandDisconnect				= OriginErrorBase + 94,     // 통신 연결이 끊어졌을 때

			/// Axis Move Command Return
			MoveBase							= 1000,
			MoveInvalidAxis						= MoveBase + 1,		// 해당 축 지령 시, 축 번호 오류
			MoveAxisAlarm						= MoveBase + 2,     // 해당 축이 알람(에러) 발생 중
			MoveAxisMoving						= MoveBase + 3,     // 해당 축이 이미 이송 진행 중
			MoveInvalidVelocity					= MoveBase + 4,     // 해당 축 지령 시, 목표 속도 오류
			MoveMpgMode							= MoveBase + 5,     // 해당 축이 MPG 모드로 사용중(SNET-RTEX, EMD-L2K)
			MoveInterpolationAxis				= MoveBase + 6,     // 해당 축이 연속보간 이송으로 사용 중
			MoveSyncSlaveAxis					= MoveBase + 7,     // 해당 축이 동기 구동을 위한 Slave 축으로 설정되어 있음(Master 축만 지령 가능)
			MoveInvalidPosition					= MoveBase + 8,     // 해당 축 지령 시, 목표(이송) 위치 오류
			MoveSoftwarePositionLimitPositive	= MoveBase + 9,     // Software Position Limit (+) 감지 상태
			MoveSoftwarePositionLimitNegative	= MoveBase + 10,	// Software Position Limit (-) 감지 상태
			MoveSyncErrorSlave					= MoveBase + 11,    // 해당 축이 동기 구동을 위한 Master 축으로 설정되어 있는 상태에서, Slave 축이 알람(에러) 발생 중일 때

            /// Interpolation Move Command Return
            InterpolationBase						= 1100,
            InterpolationTrajFull					= InterpolationBase + 1,	// 동시에 실행 될 수 있는 보간 이송 명령 개수 초과 	
            InterpolationAxisAlarm					= InterpolationBase + 2,	// 지령 축이 현재 알람 상태 임 
            InterpolationAxisBusy					= InterpolationBase + 3,	// 지령 축이 현재 이송 중 
            InterpolationVelocityZero				= InterpolationBase + 4,	// 설정 속도 "0" 오류 
            InterpolationAxisCount					= InterpolationBase + 6,	// 보간 이송 지령 축 개수 오류  
            InterpolationAxisNumber					= InterpolationBase + 7,    // 지령 축 번호 오류  
            InterpolationAngle						= InterpolationBase + 8,	// 원호 보간 지령 각도 오류  
            InterpolationParameter					= InterpolationBase + 9,	// 설정값 오류 
            InterpolationRadiusTooSmall				= InterpolationBase + 10,   // 계산된 원호 반경이 너무 작음 	
            InterpolationCenterPositionCalculation	= InterpolationBase + 11,   // 원호 중심점을 만들지 못함 
            InterpolationMovingPositionZero			= InterpolationBase + 12,   // 보간 이송 거리 "0"
            InterpolationAxisSyncSlave				= InterpolationBase + 13,   // 지령 축이 Slave 축으로 동기되어 사용 중 

            /// Continuous Move Command Return 
            ContinuousBase = 1200,
            ContinuousChNumberOver					= ContinuousBase + 1, // 인수로 전달된 연속 보간 채널 번호 오류
            ContinuousOverIndex						= ContinuousBase + 2, // 등록 가능 지령 명령 개수 초과
            ContinuousNotBeginCommand				= ContinuousBase + 3, // 연속 보간 이송 전용 출력 함수가 연속 보간 등록없이 지령됨  
            ContinuousOutputNumberOver				= ContinuousBase + 4, // 출력 접점 수 초과
            ContinuousAxisUseAnother				= ContinuousBase + 5, // 연속 보간 이송 등록 중이고 인수로 전달된 축이 현재 실행 중인 다른 채널의 연속 보간 이송에서 사용중
            ContinuousOutputOverIndex				= ContinuousBase + 6, // 등록 가능 접점 출력 명령 개수 초과 

            /// Override Move Command Return 
            OverrideBase							= 1300,
            OverrideAxisNumber						= OverrideBase + 1, // 축 번호 오류 
            OverrideAxisNotBusy						= OverrideBase + 2, // 지령 축이 구동 상태가 아님 
            OverrideInterpolationUse				= OverrideBase + 3, // 지령 축이 보간 이송에 사용중 
            OverrideVelocity						= OverrideBase + 4, // 속도 오버라이드 값이 0 보다 작거나 같음 
            OverrideInterpolationNotUse				= OverrideBase + 5, // 지령 축이 포함된 보간 이송이 없음  
            OverrideAxisBusy						= OverrideBase + 6, // 지령 축이 현재 구동 중 
            OverrideParameter						= OverrideBase + 7, // 인수 설정값 오류 
            OverridePositionBase					= OverrideBase + 50,
            OverridePositionInvalidAxis				= OverridePositionBase + 1, // 지령 축 번호가 최대 설정 가능 축 번호보다 클 때
            OverridePositionNotRunning				= OverridePositionBase + 2, // 지령 축이 현재 정지 상태 또는 Velocity(Jog) 이송 중
            OverridePositionInterpolation			= OverridePositionBase + 3, // 지령 축이 보간 이송에 사용됨 
            OverridePositionInvalidPoisition		= OverridePositionBase + 4, // 목표 위치가 현재 모션의 목표 위치와 동일
            OverridePositionInvalidRelative			= OverridePositionBase + 5, // 상대이송 유무 값이 유효하지 않음

            /// MPG Move Command Return 
            MpgBase = 1400,
			MpgIsNotSteadyState = MpgBase + 1, // 모든 축이 정지 상태가 아닐 때, 특정 축을 MPG 모드로 만들기 위해서는 해당축외 다른 모든축이 정지 상태일때만 가능

            /// Set Output For Time Command Return
            SetOutputForBase = 1600,
			SetOutputForTime = SetOutputForBase + 1, // 출력 시간 설정 오류
			SetOutputForType = SetOutputForBase + 2, // "out_type"설정 오류 -> "0 ~ 2"
			SetOutputForPort = SetOutputForBase + 3, // "out_port"설정 오류
			SetOutputForRun	 = SetOutputForBase + 4, // "output during time"기능 실행 중, 설정 시간 경과전 지령 중복

            /// Output At Position Command Return
            SetOutputAtPositionBase = 1650,
            SetOutputAtPositionAxis = SetOutputAtPositionBase + 1, // 축 번호 설정 오류 
            SetOutputAtPositionMode = SetOutputAtPositionBase + 2, // 모드 설정 오류 -> 0 ~ 2
            SetOutputAtPositionOutcount = SetOutputAtPositionBase + 3, // 접점 갯수 설정 오류 -> 1 ~ 8 

            /// DAC Command Return Event
            DacBase = 1700,
			DacNumber			= DacBase + 1,  // DAC 채널 번호 오류 
            DacRange			= DacBase + 2,  // DAC 출력 Range 설정 오류

			/// ADC Command Return 
			AdcBase		= 1800,
			AdcNmber	= AdcBase + 1,	// ADC 채널 번호 오류 
            AdcRange	= AdcBase + 2,  // ADC 입력 Range 설정 오류 

            /// Move Velocity Ad Return
            MoveVelocityAdBase		= 1900,
			MoveVelocityAdAxisBusy	= MoveVelocityAdBase + 1,  // 지령 축이 현재 사용 중 
			MoveVelocityAdNumber	= MoveVelocityAdBase + 2,  // AD 번호 설정 오류
			MoveVelocityAdDirection = MoveVelocityAdBase + 3,  // 축 이송 방향 설정 오류
            MoveVelocityAdVelocity	= MoveVelocityAdBase + 4,  // 축 이송 속도 설정 오류 
			MoveVelocityAdOthers	= MoveVelocityAdBase + 99, // 기타 오류 

			/// Trigger Command Return 
			TriggerBase					= 2100,
			TriggerChannelOverCount		= TriggerBase + 1,  // "Trigger(비 주기 출력)"채널 번호 설정 오류 
            TriggerOutPositionOverCount = TriggerBase + 2,  // "Trigger(비 주기 출력)"트리거 신호 출력 위치 설정 갯수 초과 

            /// PTP Move Command Return 
            PtpMoveBase						= 2200,
            PtpMoveSetOverCount				= PtpMoveBase + 1,  // PTP 번호 설정 초과 (ptp_index) 
            PtpMoveMovingIndexOccupied		= PtpMoveBase + 2,  // 지령 PTP번호에 해당하는 PTP 이송 실행 중
            PtpMoveIndexFault				= PtpMoveBase + 3,  // "single_axis" 번호 설정 오류
            PtpMovePlaneAxisNotDefined		= PtpMoveBase + 4,  // 평면 축 번호 미 지정 (plane_axis1 ~ plane_axis3) 
            PtpMoveMovingVelocitySetFault	= PtpMoveBase + 5,  // "signle_velocity" 설정 오류 
            PtpMoveSinglePositionSetFault	= PtpMoveBase + 6,  // "single_position" 설정 오류 
            PtpMovePlaneAxis1Occupied		= PtpMoveBase + 7,  // "plane_axis1"축 현재 사용 중 
            PtpMovePlaneAxis2Occupied		= PtpMoveBase + 8,  // "plane_axis2"축 현재 사용 중 
            PtpMovePlaneAxis3Occupied		= PtpMoveBase + 9,  // "plane_axis3"축 현재 사용 중 
            PtpMoveSingleAxisOccupied		= PtpMoveBase + 10, // "single_axis"축 현재 사용 중
            PtpMovePlaneVelocitySetFault	= PtpMoveBase + 11, // "plane_velocity" 설정 오류 
            PtpMovePlanePositionSetFault	= PtpMoveBase + 12, // "plane_position" 설정 값 음수
            PtpMovePlaneAxis3IndexOverlap	= PtpMoveBase + 13, // "plane_axis1 ~ plane_axis3" 번호 중복 
            PtpMovePlaneAndSigleAxisOverlap = PtpMoveBase + 14, // "single_axis"번호가 "plane_axis1 ~ plane_axis3"번호와 중복

            /// Command Position Auto Reset 
            PositionAutoResetBase = 2300,
			PositionAutoResetOverAxisIndex = PositionAutoResetBase + 1, // 축 번호 오류 (축 번호 초과)

			/// Latch Input 
			LatchInputChannelBase		= 2400,
			LatchInputChannelIndexFault = LatchInputChannelBase + 1, // 채널 번호 설정 오류
			LatchInputChannelTypeFault	= LatchInputChannelBase + 2, // 입력 type 설정 오류
			LatchInputChannelPortFault	= LatchInputChannelBase + 3, // port 번호 설정 오류
			LatchInputChannelPointFault = LatchInputChannelBase + 4, // point 번호 설정 오류

			/// Move Spline
			SplineBase							= 2500,
			SplineSamplingCountOver				= SplineBase + 1, // sampling 갯수가 200개를 초과 
			SplinePointMinimumCount				= SplineBase + 2, // (x, y) 지령 위치가 최소 3개 이상이 안 됨
			SplinePointMaximumCount				= SplineBase + 3, // (x, y) 지령 위치가 최대 40개를 초과 
			SplineStartPointSlopeOver			= SplineBase + 4, // start point에서의 slope가 너무 작거나 너무 큼
			SplineEndPointSlopeOver				= SplineBase + 5, // end point에서의 slope가 너무 작거나 너무 큼
			SplineInnerBufferOverflow			= SplineBase + 6, // 내부 bufffer index 초과
			SplineAlgorithmClass				= SplineBase + 7, // "spline algorithm class" 호출 실패  
			SplineMovingCommandPositionError	= SplineBase + 8, // spline 구동시 좌표 지령값 오류 

			/// 보간 이종 중 트리거 신호 출력 
			InterpolTriggerBase			= 2600,
			InterpolTriggerNoSetAxis	= InterpolTriggerBase + 1, /// 축번호 설정 오류 
			InterpolTriggerAxisNumber	= InterpolTriggerBase + 2, /// 첫번째 축과 두번째 축 번호가 같음 
			InterpolTriggerDistance		= InterpolTriggerBase + 3, /// 거리 설정 오류 
			InterpolTriggerTime			= InterpolTriggerBase + 4, /// 출력 시간 설정 오류 

			// Torque Limit 
			TorqueLimitBase				= 2700,
			TorqueLimitAxisNoError		= TorqueLimitBase + 1, // 지령 축이 RTEX 축이 아님 
            TorqueLimitAxisNoOver		= TorqueLimitBase + 2, // 지령 축 번호 오류
            TorqueLimitSetTrq			= TorqueLimitBase + 3, // 토오크 설정값 오류
            TorqueLimitRtexComBusy		= TorqueLimitBase + 4, // RTEX 비주기 통신 Busy, 설정 불가 
            TorqueLimitSetTimeOver		= TorqueLimitBase + 5, // RTEX 드라이버 설정 실패 
            TorqueLimitMoveAxisRun		= TorqueLimitBase + 6, // 지령 축 이송 중
            TorqueLimitMoveAxisContiUse = TorqueLimitBase + 7, // 지령 축이 연속 보간 이송에 사용됨 

            /// Torque Limit and Move 
            TorqueLimitAndMoveBase			= 2720,
            TorqueLimitAndMoveAxisNumber	= TorqueLimitAndMoveBase + 1, // 지령 축이 RTEX 축이 아님 
            TorqueLimitAndMoveNoOver		= TorqueLimitAndMoveBase + 2, // 지령 축 번호 오류
            TorqueLimitAndMoveAxisMoving	= TorqueLimitAndMoveBase + 3, // 지령 축이 현재 이송 중 
            TorqueLimitAndMoveAxisAlarm		= TorqueLimitAndMoveBase + 4, // 지령 축 알람 발생 중 
            TorqueLimitAndMoveInterpolation = TorqueLimitAndMoveBase + 5, // 지령 축이 연속 보간 이송으로 사용 중 
            TorqueLimitAndMoveSyncSlaveAxis = TorqueLimitAndMoveBase + 6, // 지령 축이 동기 구동을 위한 Slave 축으로 설정되어 있음 
            TorqueLimitAndMoveMpgMode		= TorqueLimitAndMoveBase + 7, // MPG 모드 동작 중 
            TorqueLimitAndMoveRunning		= TorqueLimitAndMoveBase + 8, // 시퀀스 동작 중 

            // RTEX Alarm & Warning Code Read 
            RtexAlarmWarningBase = 2800,
			RtexAlarmWarningAxisNoError = RtexAlarmWarningBase + 1, // 지령 축이 RTEX 축이 아님 
            RtexAlarmWarningAxisNoOver	= RtexAlarmWarningBase + 2, // 지령 축 번호 오류 
            RtexAlarmWarningRtexComBusy = RtexAlarmWarningBase + 3, // RTEX 비주기 통신 Busy 상태 (Read 불가)
            RtexAlarmWarningTimeOver	= RtexAlarmWarningBase + 4, // RTEX 드라이버 정보 읽기 실패 

            // RTEX Driver Information Read  
            RtexMonitorCmdBase			= 2810,
			RtexMonitorCmdAxisNoError	= RtexMonitorCmdBase + 1, // 지령 축이 RTEX 축이 아님 
            RtexMonitorCmdAxisNoOver	= RtexMonitorCmdBase + 2, // 지령 축 번호 오류 
            RtexMonitorCmdComBysy		= RtexMonitorCmdBase + 3, // RTEX 비주기 통신 Busy 상태, 읽기 불가 
            RtexMonitorCmdTimeOver		= RtexMonitorCmdBase + 4, // RTEX 드라이버 응답 Fail   

            // RTEX Driver System Id Read  
            RtexDrvSystemIdBase			= 2820,
			RtexDrvSystemIdAxisNoError	= RtexDrvSystemIdBase + 1, // 지령 축이 RTEX 축이 아님 
            RtexDrvSystemIdAxisNoOver	= RtexDrvSystemIdBase + 2, // 지령 축 번호 오류 
            RtexDrvSystemIdComBysy		= RtexDrvSystemIdBase + 3, // RTEX 비주기 통신 Busy 상태, 읽기 불가 
            RtexDrvSystemIdTimeOver		= RtexDrvSystemIdBase + 4, // RTEX 드라이버 응답 Fail 

            /// Position Command Filter		
            CommandFilter				= 2900,
            CommandFilterNotSteadyState = CommandFilter + 1, // 모든 축 정지 상태가 아님 
            CommandFilterEnableError	= CommandFilter + 2, // "enable"설정값 오류 
            CommandFilterParaError		= CommandFilter + 3, // "para"설정값 오류 

            // Linear Compensation 
            LinearCompensationBase = 3000,
			LinearCompensationAxisNoError	= LinearCompensationBase + 1,// 축번호 오류
			LinearCompensationCountError	= LinearCompensationBase + 2,// "count"설정 오류

			// Interrupt Event
			InterruptEventBase					= 4000,
			InterruptEventInvalidEnable			= InterruptEventBase + 1,   // Interrupt Event 활성화/비활성화 오류
			InterruptEventInvalidIndex			= InterruptEventBase + 2,   // 지령 축 번호 혹은 입력 채널 번호 오류		
			InterruptEventInvalidInputType		= InterruptEventBase + 3,   // 입력 Type(enum 'InterruptEventInputType') 오류
			InterruptEventInvalidInputPort		= InterruptEventBase + 4,   // 입력 Port(index) 오류
			InterruptEventInvalidInputPoint		= InterruptEventBase + 5,   // 입력 Point(bit) 오류
			InterruptEventInvalidInputActive	= InterruptEventBase + 6,	// 입력 Active(level) 오류
			InterruptEventFailInitialization	= InterruptEventBase + 20,	// 초기화 오류
			InterruptEventNotEnable				= InterruptEventBase + 21,  // 현재 Interrupt Event 비활성화
			InterruptEventInvalidTableIndex		= InterruptEventBase + 22,  // Interrupt Event Table 번호 오류
			InterruptEventInvalidAxisIndex		= InterruptEventBase + 30,  // 축 번호 오류
			InterruptEventInvalidAxisType		= InterruptEventBase + 31,  // 축 Interrupt Type(enum 'InterruptEventAxisType') 오류
			InterruptEventInvalidInputChannel	= InterruptEventBase + 40,  // 입력 채널 번호 오류
			InterruptEventWaiting				= InterruptEventBase + 50,  // 이미 대기 상태 중
			InterruptEventFailedWaiting			= InterruptEventBase + 51,  // 대기 오류
			InterruptEventFailedStartServer		= InterruptEventBase + 60,  // Interrupt Event Server 활성화 오류
			InterruptEventAlreadyStartServer	= InterruptEventBase + 61,	// Interrupt Event Server가 이미 활성화 상태
			InterruptEventNotStartServer		= InterruptEventBase + 62,	// Interrupt Event Server가 비활성화 상태
			InterruptEventFailedStopServer		= InterruptEventBase + 65,  // Interrupt Event Server 비활성화 오류
			InterruptEventFailedGetServer		= InterruptEventBase + 70,  // Interrupt Event Server 정보 읽기 오류
			InterruptEventFailedSetServer		= InterruptEventBase + 71,  // Interrupt Event Server 정보 쓰기 오류
			InterruptEventFailedCreateThread	= InterruptEventBase + 80,	// Interrupt Event Server Thread 활성화 오류
			InterruptEventFailedDeleteThread	= InterruptEventBase + 81,	// Interrupt Event Server Thread 비활성화 오류
			InterruptEventInvalidToken			= InterruptEventBase + 100, // Interrupt Event Message Format 오류
			InterruptEventInvalidCommand		= InterruptEventBase + 101, // Interrupt Event Command 오류
			InterruptEventInvalidData			= InterruptEventBase + 102, // Interrupt Event Data 오류
			InterruptEventInvalidArgument		= InterruptEventBase + 110, // 잘못된 인자 값

			// GantrySync
			GantrySyncEventBase				= 5000,
			GantrySyncAxisNoOver			= GantrySyncEventBase + 1, // 지령 축 번호 오류 
			GantrySyncInvalidEnable			= GantrySyncEventBase + 2, // "enable"설정 값 오류 
			GantrySyncInvalidAxis			= GantrySyncEventBase + 3, // 마스터 축과 슬레이브 축 번호가 같음
			GantrySyncMovingSlave			= GantrySyncEventBase + 4, // 슬레이브 축이 현재 구동 중

			// OverrideEx
			OverrideExBase					= 6000,
			OverrideExInvalidArgument		= OverrideExBase + 1,  // 유효하지 않은 인자
			OverrideExInvalidAxis			= OverrideExBase + 3,  // 지령 축 번호 오류 
			OverrideExParameter				= OverrideExBase + 10, // 설정 값 오류
			OverrideExInvalidProfile		= OverrideExBase + 11, // "profile type" 설정 오류 
            OverrideExInterpolation			= OverrideExBase + 20, // 지령 축이 보간 이송 중
			OverrideExSlaveAxis				= OverrideExBase + 21, // 지령 축이 동기 Slave 축으로 동작 중임 
			OverrideExNoVelocity			= OverrideExBase + 50, // 속도 설정 오류 
			OverrideExNoAccelTime			= OverrideExBase + 51, // 가속 시간 설정 오류 
			OverrideExNoJerk				= OverrideExBase + 52, // jerk 설정 오류 
			OverrideExInvalidPosition		= OverrideExBase + 60, // 목표 이동 거리가 너무 짧음 
			OverrideExNotRunning			= OverrideExBase + 70, // 지령 축이 정지 상태임 
			OverrideExAlarm					= OverrideExBase + 72, // 지령 축 알람 발생 상태 
			OverrideExOtherCommand			= OverrideExBase + 73, // 지령 축이 다른 명령 수행 중(좌표 변경 등)
			OverrideExAxisBusy				= OverrideExBase + 75, // 지령 축 이송 중
			OverrideExSensorLimitPositive	= OverrideExBase + 78, // (+)Limit Sensor 감지 
            OverrideExSensorLimitNegative	= OverrideExBase + 79, // (-)Limit Sensor 감지 

            // I/O MAP
            IoMapBase						= 7000,
			IoMapInvalidAddress				= IoMapBase + 1, // 지원 하지 않는 I/O Map주소 
			IoMapInvalidCount				= IoMapBase + 2, // "count"설정 오류 
			IoMapBitError					= IoMapBase + 3, // "bit"설정 오류 
			IoMapInvalidOnOff				= IoMapBase + 4, // "on"설정 오류 

            // Input interlock stop
            IoStopBase = 8000,
			IoStopInvalidAxis				= IoStopBase + 1, // 지령 축 번호 오류 
			IoStopMovingAsSlave				= IoStopBase + 2, // 지령 축이 동기 Slave 축으로 동작 중임  
            IoStopInvalidEnable				= IoStopBase + 3, // "enable"설정 오류
            IoStopInvalidAddress			= IoStopBase + 4, // "address"설정 오류 
			IoStopInvalidBit				= IoStopBase + 5, // "bit"설정 오류 
			IoStopInvalidType				= IoStopBase + 6, // "level"설정 오류 

			// MoveEx
			MoveExBase						 = 9000,
			MoveExInvalidArgument			 = MoveExBase + 1, // 유효 하지 않은 인자 
			MoveExInvalidAxis				 = MoveExBase + 3, // 지령 축 번호 오류 
			MoveExParameter					 = MoveExBase + 10, // 설정 값 오류
			MoveExInvalidProfile			 = MoveExBase + 11, // "profile type" 오류 
            MoveExInterpolation				 = MoveExBase + 20, // 지령 축 보간 이송 중 
			MoveExSlaveAxis					 = MoveExBase + 21, // 지령 축이 동기 Slave 축으로 동작 중임  
            MoveExNoVelocity				 = MoveExBase + 50, // 속도 설정 오류 
			MoveExNoAccelTime				 = MoveExBase + 51, // 가속 시간 설정 오류 
			MoveExNoJerk					 = MoveExBase + 52, // jerk 설정 오류 
			MoveExInvalidPosition			 = MoveExBase + 60, // 목표 이동 거리가 너무 짧음 
            MoveExNotRunning				 = MoveExBase + 70, // 지령 축이 정지 상태임 
			MoveExAlarm						 = MoveExBase + 72, // 지령 축 알람 발생 상태 
			MoveExOtherCommand				 = MoveExBase + 73, // 지령 축이 다른 명령 수행 중(좌표 변경 등)
            MoveExAxisBusy					 = MoveExBase + 75, // 지령 축 이송 중
            MoveExSensorLimitPositive		 = MoveExBase + 78, // (+)Limit Sensor 감지 
            MoveExSensorLimitNegative		 = MoveExBase + 79, // (-)Limit Sensor 감지 

            // Velocity
            VelocityBase					= 11000,
            VelocityInvalidAxis				= VelocityBase + 1, // 유효하지 않은 축 번호
            VelocityInvalidUnit				= VelocityBase + 2, // 유효하지 않은 단위

            // Torque
            TorqueBase						= 12000,
            TorqueInvalidAxis				= TorqueBase + 1, // 유효하지 않은 축 번호
            TorqueInvalidAxisCount			= TorqueBase + 2, // 유효하지 않은 축 개수

			// Opration
			OperationBase					= 13000,
			OperationInvalidAxis			= OperationBase + 1, // 유효하지 않은 축 번호

            // Common Parameter
            CommonParameterBase				= 20000,
            CommonParameterTypeError		= CommonParameterBase + 1, // 공통 파라미터 읽기 or 쓰기 에러
            CommonParameterIndexError		= CommonParameterBase + 2, // 공통 파라미터 범위 초과 에러

            // EtherCAT
            EcatBase						= 100000,
			EcatInvalidNodeIndex			= EcatBase + 2,		// slave의 시작 주소가 유효하지 않음
			EcatSlaveNumOver				= EcatBase + 14,	// slave의 시작 주소와 읽을 개수가 slave 총 개수 초과
            EcatFailOperation				= EcatBase + 100,	// 동작 실패

            // EtherCAT - CoE
            EcatCoeBase						= 110000,
            EcatCoeNoObject					= EcatCoeBase + 1,  // 존재하지 않는 Object
            EcatCoeNoSlave					= EcatCoeBase + 10, // 존재하지 않는 Slave
            EcatCoeNoFmmu					= EcatCoeBase + 11, // FMMU 내 Object가 없음
            EcatCoeInvalidObjectLength		= EcatCoeBase + 20, // 유효하지 않은 Object 크기

            // EtherCAT - IO Node(Slave)
            EcatIoNode						= 111000,
            EcatIoNodeOverNodeIndex			= EcatIoNode + 1,   // 요청한 Node가 연결된 EtherCAT IO Node Index 보다 많음
            EcatIoNodeNotIndex				= EcatIoNode + 10,  // 요청한 Node가 IO Node가 아님
            EcatIoNodeOverIoIndex			= EcatIoNode + 20,  // 요청한 Node의 IO Index가 Node의 IO 접점 수보다 많음
            EcatIoNodeNoNodeData			= EcatIoNode + 30,	// 요청한 Node의 IO 데이터 정보를 읽을 수 없음

            // CiA402
            CiA402Base = 200000,

            // System Error
            SystemErrorBase					= 1000000,
			SystemErrorGetFile				= SystemErrorBase + 1,

		}


		/// <summary>
		/// Move Type for EMOTION SNET API
		/// </summary>
		public enum eSnetMoveType
		{
			Scurve			= 1000, // s_curve profile moving
			Trapezoidal		= 1001, // trapezoidal profile moving
			Velocity		= 1002, // velocity(jog) profile moving
			ScurveIo		= 1003, // s_curve profile moving until setting input signal
			TrapezoidalIo	= 1004, // trapezoidal profile moving until setting input signal
			VelocityIo		= 1005, // velocity(jog) profile moving until setting input signal
		}

		/// <summary>
		/// Axis Status for EMOTION SNET API
		/// </summary>
		public enum _eSnetAxisStatus
		{
			MotionDone			= 0x0001,	// bit_number_0 : "0"-->motion done,					"1"-->motion moving
			ServoReady			= 0x0002,	// bit_number_1 : "1"-->servo ready
			ServoOn				= 0x0004,	// bit_number_2 : "1"-->servo on
			MotionFault			= 0x0008,	// bit_number_3 : "1"-->motion fault
			ProfileDone			= 0x0010,	// bit_number_4 : "0"-->profile(moving command) done,	"1"-->motion command moving
			Reserved			= 0x0020,	// bit_number_5 : reserved
			MoveDirection		= 0x0040,	// bit_number_6 : "1"-->(+)direction,					"0"-->(-)direction
			InpositionDone		= 0x0080,	// bit_number_7 : "1"-->inposition done
			HardwareLimitPlus	= 0x0100,	// bit_number_8 : "1"-->hardware (+)limit
			HardwareLimitMinus	= 0x0200,	// bit_number_9 : "1"-->hardware (-)limit
			InpositionTimeOut	= 0x0400,	// bit_number_10: "1"-->inpostion done with time_out
			FollowingError		= 0x0800,	// bit_number_11: "1"-->following error
			SoftwareLimitPlus	= 0x1000,	// bit_number_12: "1"-->software (+)limit
			SoftwareLimitMinus	= 0x2000,	// bit_number_13: "1"-->software (-)limit
		}

		/// <summary>
		/// Axis Sensor Type for EMOTION SNET API 
		/// </summary>
		public enum eSnetAxisSensor
		{
			LimitMinus	= 0,
			LimitPlus	= 1,
			HomeSensor	= 2,
			ZPhase		= 3,
		}

		/// <summary>
		/// Move Direction for EMOTION SNET API
		/// </summary>
		public enum eSnetMoveDirection
		{
			Negative = -1,
			Positive = 1,
		}

		/// <summary>
		/// Origin State for EMOTION SNET API
		/// </summary>
		public enum eSnetOriginState
		{
			OriginEventNone = 0, // Homing Sequence가 진행 중에 에러가 없는 상황 일 때
			OriginDoneOk	= 1, // Homing Sequence가 에러 없이 완료 되었을 때
		}

		/// <summary>
		/// Origin Edge for EMOTION SNET API
		/// </summary>
		public enum eSnetOriginEdge
		{
			Rising	= 1,
			Falling = 0,
		}

		/// <summary>
		/// Origin Z Use for EMOTION SNET API
		/// </summary>
		public enum eSnetOriginUseZ
		{
			DirectionMinus	= -1,
			Unused			= 0,
			DirectionPlus	= 1,
		}

		/// <summary>
		/// Origin ETC for EMOTION SNET API
		/// </summary>
		public enum eSnetOriginEtc
		{
			IoArea			= 0x1,
			ZPhaseArea		= 0xf,

			ExitOperation	= -1,
			LimitCheckUseHomeSensAtFirstStep	= -1,
			LimitCheckUseHomeSensAtSecondStep	= -2,
		}

		/// <summary>
		/// User Log Section
		/// </summary>
		public enum eSnetUserLogSection
		{
			NotUsed			= 0x0000,
			Origin			= 0x0001,
			MoveContinuous	= 0x0002,
			Trigger			= 0x0004,
			Capture			= 0x0008,
			MoveSingle		= 0x0010,
			GantrySync		= 0x0012,
		}

		/// <summary>
		/// Continuous Moving Info 
		/// </summary>
		public enum eSnetContiMovingInfo
		{
			ContinuousMovingDoneOk				= 0, // 연속이송보간 구동이 정상적으로 종료됨
			FuncMoveAxisInterpolationLine		= 3300,
			FuncMoveAxisInterpolationArcRadius	= 3310,
			FuncMoveAxisInterpolationArcAngle	= 3311,
			FuncMoveAxisInterpolationArcPoint	= 3312,
			FuncMoveAxisInterpolationLineEx		= 3315,
			FuncSetContiDwell					= 9379,
		}

		/// <summary>
		/// Axis Status Error Code
		/// </summary>
		public enum eSnetAxisStatusErrorCode
		{
			SvReadyError				= 100,
			SvAlarmError				= 101,
			SoftwarePlusLimitError		= 102,
			SoftwareMinusLimitError		= 103,
			HardwarePlusLimitError		= 104,
			HardwareMinusLimitError		= 105,
			FollowingError				= 106,
			EncoderLineError			= 107,
			AnalogOutputSignalError		= 108,  // not currently used
			WatchDogError				= 109,
			RtexDriverLinkError			= 200,
		}

		/// <summary>
		/// Spline Moving Type
		/// </summary>
		public enum eSnetSplineType
		{
			CubicBasic = 0,
			CubicHermite = 1,
		}

		/// <summary>
		/// Move Past Properties for EMOTION SNET API.
		/// </summary>
		/// <remarks>
		///	# velocity	: 속도(velocity)				: 단위 [mm/min]
		///	# accel		: 가속도(acceleration)			: 단위 [msec]	(normal value = 100 ~ 300)
		///	# decel		: 감속도(deceleration)			: 단위 [msec]	(normal value = 100 ~ 300)
		///	# jerk_acc	: 가속저크(acceleration_jerk)	: 단위 [%]		(normal value = 66)
		///	# jerk_dec	: 감속저크(deceleration_jerk)	: 단위 [%]		(normal value = 66)	
		///	# jerk		: 저크(가속/감속저크 같이 반영) : 단위 [%]		(normal value = 66) 	
		///	# direction : moving방향: "-1"-- > (-)방향, "1"-- > (+)방향
		///	# port		: I/O의 몇번째 Port(Address)
		///	# point		: 각 port당 32개의 I/O 접점이 있음 : (0 ~ 31)
		///	# area		: "0x1"--> IO 감지 영역, "0xf"--> Z(C)상 감지 영역
		///	# edge		: input signal 상태, "1"--> (uncheck--> check), "0"--> (check-->uncheck)
		/// # position	: 목표 위치
		/// </remarks>
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct MOVE_COMPONENT_EX
		{
			public int id;
			public int jerk_acc;
			public int jerk_dec;
			public int accel;
			public int decel;
			public int velocity;
			public int position;
			public int stop_condition;
		}

		/// <summary>
		/// Move Properties for EMOTION SNET API
		/// </summary>
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct MOVE_COMPONENT
		{
			public int id;
			public int jerk;
			public int accel;
			public int decel;
			public int velocity;
			public int position;
		}

		/// <summary>
		/// The interrupt event type of axis
		/// </summary>
		public enum InterruptEventAxisType
		{
			MotionDone = 1,
			Alarm = 2,
			SlowStop = 3,
			EmergencyStop = 4,
		}

		/// <summary>
		/// The interrupt event type of input
		/// </summary>
		public enum InterruptEventInputType
		{
			User = 0,
			RemoteModule = 1,
			OptionModule = 2,
		}

        /// <summary>
        /// the operation mode(control mode) of specific axis
        /// </summary>
        public enum eSnetOperationMode
        {
            CyclicPositionMode = 1,
            CyclicVelocityMode = 2,
            CyclicTorqueMode = 3,
        }

        /// <summary>
        /// EtherCAT CoE object type
        /// </summary>
        public enum eSnetEcatCoeObjectType
        {
            RxPDO = 1,
			TxPDO = 2,
		}

        /// <summary>
        /// Interrupt Event Table Information
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct InterruptEventTableInfo
		{
			//
			// 0: continous event
			// 1: only first event. after first event, not occurred event 
			//
			public int oneshot;
			//
			// from 0 : the index of axis for using interrupt event
			// under 0 : no use interrupt event of axis 
			//
			public int axis_index;
			//
			// the event type of axis. see enum 'InterruptEventAxisType'
			//
			public int axis_type;
			//
			// from 0 : the index of channel for using input.
			// under 0 : no use interrupt event of input
			//
			public int input_channel;
			//
			// the event type of input. see enum 'InterruptEventInputType'
			public int input_type;
			//
			// from 0 : the port(index) of input
			// under 0 : no use interrupt event of input
			//
			public int input_port;
			//
			// from 0 : the point(bit) of input
			// under 0 : no use interrupt event of input
			//
			public int input_point;
			//
			// 0 : active low(high to low)
			// 1 : active high(low to high)
			//
			public int input_active;
		}

		/// <summary>
		/// Delegate of Interrupt event table 
		/// </summary>
		/// <param name="table_index">The index of interrupt event table when to occur event</param>
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void InterruptEventHandler(int table_index);

		#endregion

		#region API Import

		#region Connection

		/*** Conneciton ***/

		/// <summary>
		/// 제어기 연결 하기
		/// : old version name -->"함수 이름은 같지만, 인자 속성이 바뀌었음"
		/// </summary>
		/// <param name="net">			: controller id														</param>
		/// <param name="reconnect">	: "0" : 기존 Snet Device가 수행 되고 있으면 Clear시키고 재실행 함,	
		///								  "1" : User가 반드시 "eSnetDisconnect"를 해야 함,					</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")									</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetConnect(int net, int reconnect = 0);

		/// <summary>
		/// 제어기 연결 하기
		/// : old version name -->"함수 이름은 같지만, 인자 속성이 바뀌었음"
		/// </summary>
        /// <param name="ip1">			: IPv4 Address 1														</param>
        /// <param name="ip2">			: IPv4 Address 2														</param>
        /// <param name="ip3">			: IPv4 Address 3														</param>
        /// <param name="ip4">			: IPv4 Address 4														</param>
		/// <param name="reconnect">	: "0" : 기존 Snet Device가 수행 되고 있으면 Clear시키고 재실행 함,	
		///								  "1" : User가 반드시 "eSnetDisconnect"를 해야 함,					</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")									</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetConnectEx(int ip1, int ip2, int ip3, int ip4, int reconnect = 0);

		/// <summary>
		/// 제어기 연결 끊기
		/// </summary>
		/// <param name="net">	: controller id						</param>
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetDisconnect(int net);

		#endregion

		#region Version Info

		/*** Version ***/

		/// <summary>
		/// 제어기의 API 버젼 읽기
		/// : old version name -->"eSnetDllVersion"
		/// </summary>
		/// <param name="net">		: controller id						</param>
		/// <param name="version">	: api version						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetApiVersion(int net, out int version);

		/// <summary>
		/// 제어기의 OS 버젼 읽기
		/// </summary>
		/// <param name="net">			: controller id						</param>
		/// <param name="version">		: os version						</param>
		/// <param name="device_id">	: "10" : snet_p, "20" : snet_rtex	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetOsVersion(int net, out int version, out int device_id);

		/// <summary>
		/// 제어기의 FPGA 버젼 읽기
		/// </summary>
		/// <param name="net">			: controller id						</param>
		/// <param name="version">		: os version						</param>		
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetFpgaVersion(int net, out int version);

		#endregion

		#region User Log

		/*** User Log ***/

		/// <summary>
		/// Log 남기기 설정
		/// (tip 1)log 파일 위치 : 실행파일이 있는 디렉토리 -> Log디렉토리 -> 시스템날짜 파일이름
		/// : old version name -->"eSnetSetUseLog"
		/// </summary>
		/// <param name="net"> 		: controller id							</param>
		/// <param name="use">		: "1"-->use, "0"-->unUsed				</param>
		/// <param name="section">	: (see enum eSnetUserLogSection)
		///							  (예) homing sequence와 단축구동시 둘다 내용을 log에 남기고 싶다면 int형 bit or 연산을 해서 인자로 넣으면 된다.
		///							  ((int)eSnetUserLogSection::Origin | (int)eSnetUserLogSection::MoveSingle) == (0x0001 | 0x0010) == 0x0011(decimal: 17)	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetUserLogSection(int net, int use, int section = (int)eSnetUserLogSection.NotUsed);

		/// <summary>
		/// Log 남기기 설정값 읽기
		/// (tip 1)log 파일 위치 : 실행파일이 있는 디렉토리 -> Log디렉토리 -> 시스템날짜 파일이름
		/// : old version name -->"eSnetGetUseLog"
		/// </summary>
		/// <param name="net"> 		: controller id							</param>						
		/// <param name="use">		: "1"-->use, "0"-->unUsed				</param>
		/// <param name="section">	: (see enum _eSnetUserLogSection)
		///							  (예) homing sequence와 단축구동시 둘다 내용을 log에 남기고 싶다면 int형 bit or 연산을 해서 인자로 넣으면 된다.
		///							  ((int)eSnetLogSection::Origin | (int)eSnetLogSection::MoveSingle) == (0x0001 | 0x0010) == 0x0011(decimal: 17)	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetUserLogSection(int net, out int use, out int section);

		#endregion

		#region Communication Condition Setting

		/*** Communication Config ***/

		/// <summary>
		/// API 통신 Return time out시간과 retry 횟수 설정
		/// </summary>
		/// <param name="net"> 			: controller id														</param>
		/// <param name="time_out">		: API 지령 후 제어기 최대 응답 대기 시간 설정: default "1000"[msec]	</param>
		/// <param name="retry_count">	: time out이 발생시,명령 지령 재 전송 횟수							</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")									</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetCommunicationConfig(int net, int time_out, int retry_count);

		/// <summary>
		/// API 통신 Return time out시간과 retry 설정값 읽기
		/// </summary>
		/// <param name="net"> 			: controller id														</param>
		/// <param name="time_out">		: API 지령 후 제어기 최대 응답 대기 시간 설정: default "1000"[msec]	</param>
		/// <param name="retry_count">	: time out이 발생시,명령 지령 재 전송 횟수							</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")									</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetCommunicationConfig(int net, out int time_out, out int retry_count);

		/// <summary>
		/// Ehternet 통신 "PC측 UDP Port 번호 자동 변경" 기능 사용 유무 설정 (Default "0")
		/// </summary>
		/// <param name="net">	: controller id																</param>						
		/// <param name="use">	: "1"->사용, "0"->사용하지 않음:connection시에 지정된 하나의 port로만 통신	</param>						
		/// <returns>			: (see enum "eSnetApiEventCode")											</returns>										
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetCommunicationAutoPortChange(int net, int use);

		/// <summary>
		/// Ehternet 통신 "PC측 UDP Port 번호 자동 변경" 기능 설정 상태 읽기  
		/// </summary>
		/// <param name="net">	: controller id																</param>
		/// <param name="use">	: "1"->사용, "0"->사용하지 않음:connection시에 지정된 하나의 port로만 통신	</param>
		/// <returns>			: (see enum "eSnetApiEventCode")											</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetCommunicationAutoPortChange(int net, out int use);

		/// <summary>
		/// API 지령 후 함수 Return Time
		/// </summary>
		/// <param name="net"> 					: controller id																</param>
		/// <param name="communication_time">	: [usec] : DLL 함수 지령 후 제어기로 부터 return 되어 오는데까지 걸린 시간	</param>
		/// <returns>							: (see enum "eSnetApiReturnCode")											</returns>										
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetCheckCommunicationTime(int net, out int communication_time);

		#endregion

		#region Reset / Stop / Alarm Clear

		/*** Reset & Stop & Alarm Clear ***/

		/// <summary>
		/// "Fault상태"를 해제(Reset) 하거나 단축 이송시 사용자에 의한 "수동 정지(Stop)"실행 
		/// (tip 1) 이송 정지시 "기본 파라미터 P20(Max. velocity) / P21(Reset Dec.Time)"으로 계산된 감속비가 
		///			적용 됩니다.  
		/// : old versin name ->"eSnetResetAxis"
		/// </summary>
		/// <param name="net"> 	: controller id						</param>						
		/// <param name="axis">	: axis index						</param>	
		/// <returns>			: (see enum "eSnetApiReturnCode")>	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetReset(int net, int axis);

		/// <summary>
		/// 모든 축의 "Fault상태"를 해제(Reset) 하거나 사용자에 의한 "수동 정지(Stop)"실행
		/// (tip 1) 이송 정지시 "기본 파라미터 P20(Max. velocity) / P21(Reset Dec.Time)"으로 계산된 감속비가 
		///			적용 됩니다.  
		/// : old version name ->"eSnetResetAllAxis"
		/// </summary>
		/// <param name="net"> 	: controller id						</param>						
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetResetAll(int net);

		/// <summary>
		/// "Fault상태"를 해제(Reset) 하거나 단축 이송시 사용자에 의한 "수동 정지(Stop)"실행 
		/// (tip 1) 이송 정지시 "기본 파라미터 P20(Max. velocity) / P21(Reset Dec.Time)"으로 계산된 감속비가 
		///			적용 됩니다. 
		/// : old version name ->"eSnetEmgStopAxis" 
		/// </summary>
		/// <param name="net"> 	: controller id						</param>						
		/// <param name="axis">	: axis index						</param>	
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetEmergencyStop(int net, int axis);

		/// <summary>
		/// 모든 축의 "Fault상태"를 해제(Reset) 하거나 사용자에 의한 "수동 정지(Stop)"실행 
		/// (tip 1) 이송 정지시 "기본 파라미터 P20(Max. velocity) / P21(Reset Dec.Time)"으로 계산된 감속비가 
		///			적용 됩니다.
		/// : old version name ->"eSnetEmgStopAllAxis"  
		/// </summary>
		/// <param name="net"> 	: controller id						</param>						
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetEmergencyStopAll(int net);

		/// <summary>
		/// 단축 이송 중 수동"감속 정지"실행  
		/// : old version name -->"eSnetSStopAxis"
		/// </summary>
		/// <param name="net"> 		: controller id						</param>						
		/// <param name="axis">		: axis index						</param>						
		/// <param name="decel">	: 감속 시간 (msec)					</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSlowStop(int net, int axis, int decel);

		/// <summary>
		/// 모든 축의 단축 이송 중 수동"감속 정지"실행  
		/// : old version name -->"eSnetSStopAxis"
		/// </summary>
		/// <param name="net"> 		: controller id						</param>						
		/// <param name="decel">	: 감속 시간 (msec)					</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSlowStopAll(int net, int decel);

		/// <summary>
		/// 정지 조건 설정 및 활성화
		/// </summary>
		/// <param name="net">		 : Network id(IPv4 4th address)		   </param>
		/// <param name="axis">		 : 축 번호							   </param>
		/// <param name="enable">	 : 정지 조건 활성화 여부				   </param>
		///							   1 -> enable
		///							   0 -> disable (나머지 파라미터 미적용) </param>
		/// <param name="address">	 : I/O Map Index 번호				   </param>
		/// <param name="bit">		 : 0 ~ 31 bit						   </param>
		/// <param name="level">	 : 접점 active level
		///							   0 -> A 접점(Normal Open)		 
		///							   1 -> B 접점(Normal Close)			   </param>
		/// <param name="decelTime"> : 감속 시간(1msec)					   </param>
		/// <returns>				 : (see enum "eSnetApiReturnCode")     </returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetIoStop(int net, int axis, int enable, int address, int bit, int level, int decelTime);

		/// <summary>
		/// 정지 조건 설정 확인
		/// </summary>
		/// <param name="net">		 : Network id(IPv4 4th address)		   </param>
		/// <param name="axis">		 : 축 번호							   </param>
		/// <param name="enable">	 : 정지 조건 활성화 여부				   </param>
		///							   1 -> enable
		///							   0 -> disable (나머지 파라미터 미적용) </param>
		/// <param name="address">	 : I/O Map Index 번호				   </param>
		/// <param name="bit">		 : 0 ~ 31 bit						   </param>
		/// <param name="level">	 : 접점
		///							   0 -> A 접점(Normal Open)		 
		///							   1 -> B 접점(Normal Close)			   </param>
		/// <param name="decelTime"> : 감속 시간(1msec)					   </param>
		/// <returns>				 : (see enum "eSnetApiReturnCode")     </returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetIoStop(int net, int axis, out int enable, out int address, out int bit, out int level, out int decelTime);

        /// <summary>
        /// bit 단위로 정지 조건 설정 및 활성화
        /// </summary>
        /// <param name="net">		 : Network id(IPv4 4th address)		   </param>
        /// <param name="axis">		 : 축 번호							   </param>
        /// <param name="enable">	 : 정지 조건 활성화 여부				   </param>
        ///							   1 -> enable
        ///							   0 -> disable (나머지 파라미터 미적용) </param>
        /// <param name="bit">		 : I/O Map 인덱스 번호(address) = bit/32, 
        ///							   접점 번호(bit) = bit % 32				</param>
        /// <param name="level">	 : 접점 active level
        ///							   0 -> A 접점(Normal Open)		 
        ///							   1 -> B 접점(Normal Close)			   </param>
        /// <param name="decelTime"> : 감속 시간(1msec)					   </param>
        /// <returns>				 : (see enum "eSnetApiReturnCode")     </returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetIoStopBit(int net, int axis, int enable, int bit, int level, int decelTime);

        /// <summary>
        /// bit 단위로 정지 조건 설정 확인
        /// </summary>
        /// <param name="net">		 : Network id(IPv4 4th address)		   </param>
        /// <param name="axis">		 : 축 번호							   </param>
        /// <param name="enable">	 : 정지 조건 활성화 여부				   </param>
        ///							   1 -> enable
        ///							   0 -> disable (나머지 파라미터 미적용) </param>
        /// <param name="bit">		 : I/O Map 인덱스 번호(address) = bit/32, 
        ///							   접점 번호(bit) = bit % 32				</param>
        /// <param name="level">	 : 접점
        ///							   0 -> A 접점(Normal Open)		 
        ///							   1 -> B 접점(Normal Close)			   </param>
        /// <param name="decelTime"> : 감속 시간(1msec)					   </param>
        /// <returns>				 : (see enum "eSnetApiReturnCode")     </returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetIoStopBit(int net, int axis, out int enable, out int bit, out int level, out int decelTime);

		/// <summary>
		/// 축별 "Servo driver alarm clear" On/Off신호 출력 (예) panasonic amp의 경우 "1200msec"
		/// (tip 1) "Servo Driver" specification을 참고하여 "on_time" 조정  
		/// : old version name ->"eSnetSetAxisServoAlarmClear"
		/// </summary>
		/// <param name="net"> 		: controller id									</param>						
		/// <param name="axis">		: axis index									</param>						
		/// <param name="on_off">	: "1"->alarm clear 신호를 "onTime"시간동안 출력	</param>						
		/// <param name="on_time">	: alarm clear on 신호 출력 유지시간(msec) 
		///								(예)panasonic amp의 경우 "1200") 			</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")				</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetServoAlarmClear(int net, int axis, int on_off, int on_time);

		/// <summary>
		/// 모든 축 "Servo driver alarm clear" On/Off신호 출력 
		/// (tip 1) "Servo Driver" specification을 참고하여 "on_time" 조정  (예) panasonic amp의 경우 "1200msec"
		/// : old version name ->"eSnetSetAllAxisServoAlarmClear"
		/// </summary>
		/// <param name="net"> 		: controller id										</param>						
		/// <param name="on_off">	: "1"->alarm clear 신호를 "on_time"시간동안 출력	</param>						
		/// <param name="on_time">	: alarm clear on 신호 출력 유지시간(msec)			</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")					</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetServoAlarmClearAll(int net, int on_off, int on_time);

		#endregion

		#region Axis Status

		/*** Axis Status ***/

		/// <summary>
		/// 축별 상태 정보 읽기 
		/// : old version name ->"eSnetGetAxisAllStatus"
		/// </summary>
		/// <remarks>
		/// (tip 1) fault가 발생 하였을 경우, fault상태가 해제되어도 제어기는 fault 상태가 계속 유지 됩니다.
		///			사용자가 Reset을 해야만 fault상태가 해제 됩니다. 
		///			(예) limit sensor가 순간적으로 감지되었다가 해제 되었을 경우, 실시간 sensor값은 on->off로 변경 되었지만,
		///		    eSnetGetAxisStatus()함수에서는 "limit Error Fault Status"는 남아 있게 된다
		/// </remarks>
		/// <param name="net"> 		: controller id 															</param>						
		/// <param name="axis">		: axis index																</param>						
		/// <param name="value">	: 축별 제어기 상태 (see. "enum _eSnetAxisStatus"actual position)	
		///								bit 0: 축 이송 상태 "1"-> moving, "0"-> stop 
		///								bit 1: 서보 드라이버 Ready 신호 입력 상태 
		///										(단 파라미터에서 서보 ready 입력을 enable 상태로 설정 했을 경우 적용) 
		///										"1" ->servo ready / "0" ->servo not ready 	
		///								bit 2: 서보 드라이버 "Servo On" 상태  "1"-> sv on, "0"-> sv off  
		///								bit 3: Fault status(limit error, amp fault, alarm, etc)
		///								bit 4: Profile(command) moving 지령 상태 "1"-> moving, "0"->profile move done
		///								bit 5: Servo alarm 상태 "1"-> servo alarm on 
		///								bit 6: "1"->positive direction move, 
		///									   "0"->negative direction move(Snet-rtex 제어기 미사용)
		///								bit 7: Inposition 상태 "1"->inposition on 
		///								bit 8: Positive hw limit fault  "1"-> +Limit fault  
		///								bit 9: Negative hw limit fault  "1"-> -Limit fault  
		///								bit 10: Inposition check time over "1"-> time over 
		///								bit 11: Following error status "1"-> following error 
		///								bit 12: Positive software limit fault "1"-> +Sw limit fault 
		///								bit 13: Negative software limit fault "1"-> -Sw limit fault				</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")											</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetAxisStatus(int net, int axis, out int value);

		/// <summary>
		/// 축별 "축 I/O"값과 "축 상태 정보" 읽기
		///	(tip 1) : 이 함수의 sensor값은 단순히 sensor의 감지 여부만을 표시한다.
		///			  sensor가 감지 되었다가 감지 되지 않았을 경우, 이 함수에서는 "입력 Off 상태"지만
		///			  eSnetGetAxisStatus(...)함수에서는 limit Error Fault Status가 남아 있게 된다
		///			  --> 실시간 IO 상태를 체크하는 것과, 제어기의 Limit Error Status를 구분해야 한다		
		/// : old version name ->"eSnetGetAxisIoAndStatus"
		/// </summary>
		/// <param name="net"> 				: controller id																</param>						
		/// <param name="axis">				: axis index																</param>						
		/// <param name="io_and_status">	: "1"-> On, "0"-> Off  
		///										I/O 정보	-----------------------------------------------------------		
		///											bit 0:"-Limit Sensor" 입력 정보 
		///											bit 1:"+Limit Sensor" 입력 정보 
		///											bit 2:"원점 센서" 입력 정보  
		///											bit 3:"Servo Alarm" 입력 정보
		///											bit 4:"Servo Ready" 입력 정보
		///											bit 5:"Servo On" 출력 상태 정보 
		///										Status 정보	-------------------------------------------------------
		///											bit 6:"Inposition" 상태 정보 
		///											bit 7:"Fault" 상태 정보 (limit error/alarm/following error status etc...)		
		///													fault 상태가 되었을 때, eSnetGetAxisStatus(..) 또는 
		///													eSnetGetErrorCode(..)를 사용하여 추가 정보 확인				</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")											</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetAxisSignalStatus(int net, int axis, out int io_and_status);

		/// <summary>
		/// 축별 Motion Done 상태 읽기
		/// : old version name ->"eSnetGetAxisMotionDone"
		/// </summary>
		/// <param name="net"> 			: controller id 					</param>						
		/// <param name="axis">			: axis index						</param>						
		/// <param name="motion_done">	: "1"->motion done, "0"-> moving 중 </param>						
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetMotionDone(int net, int axis, out int motion_done);

		/// <summary>
		/// 제어기의 에러 번호 읽기
		/// : old version name ->"eSnetGetErrorNumber"
		/// </summary>
		/// <param name="net"> 			: controller id 										</param>						
		/// <param name="axis">			: axis index											</param>						
		/// <param name="error_code">	: error code (see. "enum _eSnetStatusErrorCode") 	
		/// 								"100" : sv ready error, 
		///									"101" : sv alarm error, 
		///									"102" : sw limit(+), 
		///									"103" : sw limit(-), 
		///									"104" : hw limit(+),
		///									"105" : hw limit(-),
		///									"106" : following error, 
		///									"109" : watch dog error,							
		///									"200" : rtex_driver_link_error / ecat slave link error,
		///									"210" : ethercat sdo write fail,
		///									"211" : ethercat sdo read fail,						</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")						</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetErrorCode(int net, out int axis, out int error_code);

		/// <summary>
		/// 축별 이송 지령값과 드라이버의 현재 이송값을 비교하여 최대/평균 이송 오차를 표시하고 마스터 축과 동기 구동
		/// 중일때 마스터 축과 슬레이브축의 드라이버 이송 지령값의 차를 표시 한다. 
		///
		/// </summary>
		/// <param name="net"> 			: controller id 									</param>						
		/// <param name="axis">			: axis index										</param>
		/// <param name="error">		: [array] 배열크기 ->"3"
		///								  error[0] -> 최대 이송오차								
		///								  error[1] -> 평균 이송오차								
		///								  error[2] -> 동기 이송오차 							</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")					</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetPositionStatus(int net, int axis, out int error);

		#endregion

		#region Driver Connection Status (SNET-RTEX)

		/*** RTEX Driver Status ***/

		/*** RTEX Driver Status ***/

		/// <summary>
		/// RTEX Driver 상태 읽기
		/// : old version name -->"eSnetGetRtexDriverStatus"
		/// </summary>
		/// <param name="net"> 				: controller id 											</param>
		/// <param name="driver_status">	: [array] 배열크기 ->"4"
		///										[0] : driver connected count (1~ )
		///										[1] : "1"->driver connection ok, "0"->driver connection fail
		///										[2] : spare
		///										[3] : spare												</param>						
		/// <returns>						: (see enum "eSnetApiReturnCode")							</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetDriverStatus(int net, out int driver_status);

		#endregion

		#region Servo On/Off

		/*** Servo Enable ***/

		/// <summary>
		/// Servo On 상태 설정
		/// : old version name ->"eSnetSetAxisServoEnable"
		/// </summary>
		/// <param name="net"> 		: controller id 					</param>						
		/// <param name="axis">		: axis index						</param>						
		/// <param name="enable">	: "1"->servo on, "0"->servo off		</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetServoOn(int net, int axis, int enable);

		/// <summary>
		/// Servo On 상태 읽기
		/// : old version name ->"eSnetGetAxisServoEnable"
		/// </summary>
		/// <param name="net"> 		: controller id 					</param>						
		/// <param name="axis">		: axis index						</param>						
		/// <param name="enable">	: "1"->servo on, "0"->servo off		</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetServoOn(int net, int axis, out int enable);

        #endregion

        #region Homing (Step)

        /*** Homing (Origin) ***/

        /// <summary>
        /// homing sequence motion moving step 설정 하기
        /// : old version name ->"eSnetSetOriginStep"
        /// </summary>
        /// <param name="net"> 			: controller id																</param>						
        /// <param name="axis">			: axis index																</param>						
        /// <param name="move_step">	: "0"~ 부터 시작																</param>						
        /// <param name="velocity">		: [mm/min]	: 속도															</param>
        /// <param name="accel">		: [msec]	: 가속도															</param>
        /// <param name="direction">	: "1"->(+)방향구동, "-1"->(-)방향 구동										</param>
        /// <param name="sensor">		: "0"->(-)limit_sensor,"1"->(+)limit_sensor, 
        ///								  "2"->origin_sensor, "3"->Z상   											</param>
        /// <param name="edge">			: "1"-->sensor 감지를 원할 때, "0"-->감지된 sensor가 감지 안 될 때를 원할 때	</param>
        /// <param name="dwell">		: [usec] : motion 구동 후 대기 시간(default : 500msec)						</param>		
        /// <returns>					: (see enum "eSnetApiReturnCode")											</returns>	
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetAddHomingStep(int net, int axis, int move_step, int velocity, int accel, int direction, int sensor, int edge, int dwell = 500);

		/// <summary>
		/// homing sequence motion moving step 설정값 읽기
		/// : old version name ->"eSnetGetOriginStep"
		/// </summary>
		/// <param name="net"> 			: controller id																</param>						
		/// <param name="axis">			: axis index																</param>						
		/// <param name="move_step">	: "0"~ 부터 시작, 첫번째 Step ==> "0"											</param>						
		/// <param name="velocity">		: [mm/min]	: 속도															</param>
		/// <param name="accel">		: [msec]	: 가속도															</param>
		/// <param name="direction">	: "1"->(+)방향구동, "-1"->(-)방향 구동										</param>
		/// <param name="sensor">		: "0"->(-)limit_sensor, "1"->(+)limit_sensor, "2"->origin_sensor			</param>
		/// <param name="edge">			: "1"-->sensor 감지를 원할 때, "0"-->감지된 sensor가 감지 안 될 때를 원할 때	</param>
		/// <param name="dwell">		: [usec] : motion 구동 후 대기 시간(default : 500msec)						</param>		
		/// <returns>					: (see enum "eSnetApiReturnCode")											</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetHomingStep(int net, int axis, int move_step, out int velocity, out int accel, out int direction, out int sensor, out int port, out int point, out int edge, out int dwell);

		/// <summary>
		/// homing sequence zero offset 설정 하기
		/// : old version name ->"eSnetSetOriginZeroOffset"
		/// </summary>
		/// <param name="net"> 			: controller id																			</param>						
		/// <param name="axis">			: axis number																			</param>						
		/// <param name="zero_offset">	: Z상부터 "0"으로 설정할 위치까지의 offset 거리			
		///									homing sequence가 끝나고, 설정한 offset만큼 이동 후 "0"점으로 Set 한다				</param>						
		/// <param name="set_time">		: [usec] : offset만큼 이동 후, set_time[msec] 후에,"0"점으로 Set 한다(default: 500msec)	</param>		
		/// <returns>					: (see enum "eSnetApiReturnCode")														</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetHomingShift(int net, int axis, int zero_offset, int set_time);

        /// <summary>
        /// homing sequence zero offset 설정 하기
        /// : old version name ->"eSnetSetOriginZeroOffset"
        /// </summary>
        /// <param name="net"> 			: controller id																			</param>						
        /// <param name="axis">			: axis number																			</param>						
        ///	<param name="velocity">		: 속도(velocity)				: 단위 [mm/min]											</param>
        ///	<param name="accel">		: 가속도(acceleration)			: 단위 [msec]	(normal value = 100 ~ 300)				</param>
        ///	<param name="decel">		: 감속도(deceleration)			: 단위 [msec]	(normal value = 100 ~ 300)				</param>
        ///	<param name="jerk_acc">		: 가속저크(acceleration_jerk)	: 단위 [%]		(normal value = 66)						</param>
        ///	<param name="jerk_dec">		: 감속저크(deceleration_jerk)	: 단위 [%]		(normal value = 66)						</param>
        /// <param name="zero_offset">	: Z상부터 "0"으로 설정할 위치까지의 offset 거리			
        ///									homing sequence가 끝나고, 설정한 offset만큼 이동 후 "0"점으로 Set 한다				</param>						
        /// <param name="set_time">		: [usec] : offset만큼 이동 후, set_time[msec] 후에,"0"점으로 Set 한다(default: 500msec)	</param>		
        /// <returns>					: (see enum "eSnetApiReturnCode")														</returns>	
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetSetHomingShiftEx(int net, int axis, int velocity, int accel, int decel, int jerk_acc, int jerk_dec, int zero_offset, int set_time);

        /// <summary>
        /// homing sequence zero offset 설정값 읽기
        /// : old version name ->"eSnetGetOriginZeroOffset"
        /// </summary>
        /// <param name="net"> 			: controller id																			</param>						
        /// <param name="axis">			: axis number																			</param>						
        /// <param name="zero_offset">	: Z상부터 "0"으로 설정할 위치까지의 offset 거리			
        ///									homing sequence가 끝나고, 설정한 offset만큼 이동 후 "0"점으로 Set 한다				</param>						
        /// <param name="set_time">		: [usec] : offset만큼 이동 후, set_time[msec] 후에,"0"점으로 Set 한다(default: 500msec)	</param>		
        /// <returns>					: (see enum "eSnetApiReturnCode")														</returns>	
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetHomingShift(int net, int axis, out int zero_offset, out int set_time);

        /// <summary>
        /// homing sequence zero offset 설정값 읽기
        /// : old version name ->"eSnetGetOriginZeroOffset"
        /// </summary>
        /// <param name="net"> 			: controller id																			</param>						
        /// <param name="axis">			: axis number																			</param>						
        ///	<param name="velocity">		: 속도(velocity)				: 단위 [mm/min]											</param>
        ///	<param name="accel">		: 가속도(acceleration)			: 단위 [msec]	(normal value = 100 ~ 300)				</param>
        ///	<param name="decel">		: 감속도(deceleration)			: 단위 [msec]	(normal value = 100 ~ 300)				</param>
        ///	<param name="jerk_acc">		: 가속저크(acceleration_jerk)	: 단위 [%]		(normal value = 66)						</param>
        ///	<param name="jerk_dec">		: 감속저크(deceleration_jerk)	: 단위 [%]		(normal value = 66)						</param>
        /// <param name="zero_offset">	: Z상부터 "0"으로 설정할 위치까지의 offset 거리			
        ///									homing sequence가 끝나고, 설정한 offset만큼 이동 후 "0"점으로 Set 한다				</param>						
        /// <param name="set_time">		: [usec] : offset만큼 이동 후, set_time[msec] 후에,"0"점으로 Set 한다(default: 500msec)	</param>		
        /// <returns>					: (see enum "eSnetApiReturnCode")														</returns>	
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetGetHomingShiftEx(int net, int axis, out int velocity, out int accel, out int decel, out int jerk_acc, out int jerk_dec, out int zero_offset, out int set_time);

        /// <summary>
        /// homing sequence 시작 하기
        /// : old version name ->"eSnetGoOrigin"
        /// </summary>
        /// <param name="net">	: controller id						</param>						
        /// <param name="axis">	: axis index						</param>						
        /// <returns>			: (see enum "eSnetApiReturnCode")	</returns>	
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetStartHoming(int net, int axis);

		/// <summary>
		/// homing sequence 중지 
		/// </summary>
		/// <param name="net">	: controller id						</param>						
		/// <param name="axis">	: axis index						</param>						
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetStopHoming(int net, int axis);

		/// <summary>
		/// homing sequence 진행율
		/// (tip 1) Homing 도중 Event가 발생해서 Homing Sequence를 빠져 나와도 100(%)가 됩니다. 
		///         따라서, 반드시 "eSnetGetOriginResult"를 통해서 Homing결과를 확인해야 합니다.
		/// : old version name ->"eSnetGetOriginRate"
		/// </summary>
		/// <param name="net"> 	: controller id						</param>						
		/// <param name="axis">	: axis index						</param>						
		/// <param name="rate">	: [%] : homing 진행율				</param>						
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetHomingRate(int net, int axis, out int rate);

		/// <summary>
		/// homing sequence 진행 상태와 결과 읽기
		/// (tip 1) "eSnetGetOriginRate"를 통해서 Homing Sequence가 100(%)가 되고 나서, 
		///         이 함수를 통해 잘 끝났는지 다른 Event가 발생했는지 반드시 확인해야 합니다. 
		/// : old version name ->"eSnetGetOriginResult"
		/// </summary>
		/// <param name="net"> 			: controller id													</param>						
		/// <param name="axis">			: axis index													</param>						
		/// <param name="move_step">	: 현재 진행중인 Moving Step("0"~:"0"번 부터 시작)				</param>						
		/// <param name="event_id">		: (see "enum _eSnetOriginEvent")
		///									"0" : homing process 진행 중
		///									"1" : homing process가 error없이 완료
		///									"그 외의 값" : ("300"~) --> (see enum "eSnetApiCodeEvent")	</param>		
		/// <returns>					: (see enum "eSnetApiReturnCode")								</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetHomingResult(int net, int axis, out int move_step, out int event_id);

		/// <summary>
		/// 각 축별로 homing 이미 완료 되어 있는지의 상태
		/// : old version name ->"eSnetGetIsOriginDone"
		/// </summary>
		/// <param name="net"> 	: controller id							</param>						
		/// <param name="axis">	: axis index							</param>						
		/// <param name="done">	: homing완료 여부 : 
		///							"1" : homing이 완료 되어 있음							
		///							"0" : homing이 완료 되지 않았음		</param>						
		/// <returns>				(see enum "eSnetApiReturnCode")		</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetIsHomingDone(int net, int axis, out int done);

		#endregion

		#region Homing (Method)

		/// <summary>
		/// homing method : homing 설정 하기
		/// : old version name ->"eSnetSetOriginMethod"
		/// : 사용을 권하지 않음
		/// </summary>
		/// <param name="net">			: controller id																					</param>		
		/// <param name="axis">			: axis index																					</param>						
		/// <param name="direction">	: 첫번째 구동 Step이 "1"->(+)방향구동, "-1"->(-)방향 구동										</param>
		/// <param name="sensor">		: "0"->(-)limit_sensor, "1"->(+)limit_sensor, "2"->origin_sensor								</param>
		/// <param name="use_z">		: "0"->Z_Phase 사용 안함, "1"->Z_Phase 사용														</param>
		/// <param name="set_time">		: [usec] : offset만큼 이동 후, set_time[msec] 후에,"0"점으로 Set 한다(default: 500msec)			</param>	
		/// <param name="zero_shift">	: user가 Z상 또는 Sensor Off상태에서 얼마큼 더 이동 후에 Origion "0"점을 잡을지에 대한 위치값	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")																</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetHomingMethod(int net, int axis, int direction, int sensor, int use_z, int set_time, int zero_shift);

		/// <summary>
		/// homing method : homing 설정값 읽기
		/// : old version name ->"eSnetGetOriginMethod"
		/// : 사용을 권하지 않음
		/// </summary>
		/// <param name="net">			: controller id																					</param>		
		/// <param name="axis">			: axis index																					</param>						
		/// <param name="direction">	: 첫번째 구동 Step이 "1"->(+)방향구동, "-1"->(-)방향 구동										</param>
		/// <param name="sensor">		: "0"->(-)limit_sensor, "1"->(+)limit_sensor, "2"->origin_sensor								</param>
		/// <param name="use_z">		: "0"->Z_Phase 사용 안함, "1"->Z_Phase 사용														</param>
		/// <param name="set_time">		: [usec] : offset만큼 이동 후, set_time[msec] 후에,"0"점으로 Set 한다(default: 500msec)			</param>	
		/// <param name="zero_shift">	: user가 Z상 또는 Sensor Off상태에서 얼마큼 더 이동 후에 Origion "0"점을 잡을지에 대한 위치값	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")																</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetHomingMethod(int net, int axis, out int direction, out int sensor, out int use_z, out int set_time, out int zero_shift);

		/// <summary>
		/// homing method : homing 속도 설정 하기
		/// : old version name ->"eSnetSetOriginMethodSpeed"
		/// : 사용을 권하지 않음
		/// </summary>
		/// <param name="net">			: controller id							</param>		
		/// <param name="axis">			: axis index							</param>						
		/// <param name="velocity_0">	: [mm/min] 첫번째 구동 Step 속도		</param>
		/// <param name="velocity_1">	: [mm/min] 두번째 구동 Step 속도		</param>
		/// <param name="velocity_2">	: [mm/min] 세번재 구동 Step 속도		</param>
		/// <param name="velocity_3">	: [mm/min] 네번째 구동 Step 속도		</param>
		/// <param name="accel_0">		: [mm/min] 첫번째/두번째 구동 가속도	</param>
		/// <param name="accel_1">		: [mm/min] 세번째/네번째 구동 가속도	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")		</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetHomingMethodSpeed(int net, int axis, int velocity_0, int velocity_1, int velocity_2, int velocity_3, int accel_0, int accel_1);

		/// <summary>
		/// homing method : homing 속도 설정값 읽기
		/// : old version name ->"eSnetGetOriginMethodSpeed"
		/// : 사용을 권하지 않음
		/// </summary>
		/// <param name="net">			: controller id							</param>		
		/// <param name="axis">			: axis index							</param>						
		/// <param name="velocity_0">	: [mm/min] 첫번째 구동 Step 속도		</param>
		/// <param name="velocity_1">	: [mm/min] 두번째 구동 Step 속도		</param>
		/// <param name="velocity_2">	: [mm/min] 세번재 구동 Step 속도		</param>
		/// <param name="velocity_3">	: [mm/min] 네번째 구동 Step 속도		</param>
		/// <param name="accel_0">		: [mm/min] 첫번째/두번째 구동 가속도	</param>
		/// <param name="accel_1">		: [mm/min] 세번째/네번째 구동 가속도	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")		</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetHomingMethodSpeed(int net, int axis, out int velocity_0, out int velocity_1, out int velocity_2, out int velocity_3, out int accel_0, out int accel_1);

		/// <summary>
		/// homing method : homing 시작 하기
		/// : old version name ->"eSnetSetOriginMethodStart"
		/// : 사용을 권하지 않음
		/// </summary>
		/// <param name="net">	: controller id						</param>		
		/// <param name="axis">	: axis index						</param>						
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetStartHomingMethod(int net, int axis);

		#endregion

		#region Moving :: Single & Jog

		/*** Move : Normal Single ***/
		/// # move_type : moving profile type
		///		(int)eSnetMoveType::Scurve			: "1000"--> s_curve profile moving
		///		(int)eSnetMoveType::Trapezoidal		: "1001"--> trapezoidal profile moving
		///		(int)eSnetMoveType::Velocity		: "1002"--> velocity(jog) profile moving
		///		(int)eSnetMoveType::ScurveIo		: "1003"--> s_curve profile moving until setting input signal
		///		(int)eSnetMoveType::TrapezoidalIo	: "1004"--> trapezoidal profile moving until setting input signal
		///		(int)eSnetMoveType::VelocityIo		: "1005"--> velocity(jog) profile moving until setting input signal
		///				( see. "enum eSnetMoveType")		
		///	# velocity	: 속도(velocity)				: 단위 [mm/min]
		///	# accel		: 가속도(acceleration)			: 단위 [msec]	(normal value = 100 ~ 300)
		///	# decel		: 감속도(deceleration)			: 단위 [msec]	(normal value = 100 ~ 300)
		///	# jerk_acc	: 가속저크(acceleration_jerk)	: 단위 [%]		(normal value = 66)
		///	# jerk_dec	: 감속저크(deceleration_jerk)	: 단위 [%]		(normal value = 66)	
		///	# jerk		: 저크(가속/감속저크 같이 반영) : 단위 [%]		(normal value = 66) 	
		///	# direction : moving방향: "-1"-- > (-)방향, "1"-- > (+)방향
		///	# port		: "I/O 커넥터" 구분 번호 또는 "Remote IO Module"id 번호  
		///	# point		: 접점 번호 
		///	# area		: "0x1"--> IO 감지 영역, "0xf"--> Z(C)상 감지 영역
		///	# edge		: input signal 상태, "1"--> (uncheck--> check), "0"--> (check-->uncheck)
		/// # position	: 목표 위치

		/// <summary>
		/// 단축 구동(구조체 이용)
		/// : old version name ->"eSnetMoveAxis"
		/// </summary>
		/// <param name="net">			: controller id						</param>
		/// <param name="axis">			: axis index						</param>
		/// <param name="move_type">	: (see enum "eSnetMoveType")		</param>
		/// <param name="data">			: (see struct "MOVE_COMPONENT_EX")	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetMove(int net, int axis, int move_type, out MOVE_COMPONENT_EX data);

		/// <summary>
		/// 단축 이송 강제 실행
		/// [정지 상태]: 구동 가능
		///				기존 이송 명령 실행
		/// [이송 상태]: 현재 위치, 속도부터 새 위치, 속도로 이송 명령 실행
		///				가속, 정속, 감속 구간에서도 구동
		/// </summary>
		/// <param name="net">:				controller id				</param>
		/// <param name="axis">:			axis index ("0"~ )			</param>
		/// <param name="relative">:		상대 이송 유무 				</param>
		/// @ "0" : 절대 이송
		/// @ "1" : 상대 이송
		/// <param name="moving_option">:	이송 중 명령 적용 시점		</param>
		/// @ "0" : 즉시 적용
		/// @ "1" : 이송 중일 경우, 정속 구간 도달 시 적용
		/// <param name="profile_type">:	속도 프로파일					</param>
		/// @ "1000" : S-Curve
		/// @ "1001" : Trapezoidal
		/// <param name="velocity">:		[mm/min] override 속도		</param>
		/// <param name="accel">:			[msec] override 가속 시간	</param>
		/// <param name="decel">:			[msec] override 감속 시간	</param>
		/// <param name="acc_jerk">:		[%] 가속 시 jerk				</param>
		/// <param name="dec_jerk">:		[%] 감속 시 jerk				</param>
		/// <param name="position">:		[um] 변경할 위치				</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")					
		///										MoveExBase					(9000)
		///										MoveExInvalidArgument		(9001)
		///										MoveExInvalidAxis			(9003)
		///										MoveExParameter				(9010)
		///										MoveExInvalidProfile		(9011)
		///										MoveExInterpolation			(9020)
		///										MoveExExSetSlave			(9021)
		///										MoveExNoVelocity			(9050)
		///										MoveExNoAccelTime			(9051)
		///										MoveExNoJerk				(9052)
		///										MoveExInvalidPosition		(9060)
		///										MoveExAlarm					(9072)
		///										MoveExOtherCommand			(9073)
		///										MoveExAxisBusy				(9075)
		///										MoveExSensorLimitPositive	(9078)
		///										MoveExSensorLimitNegative	(9079)	 </returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetMoveEx(int net, int axis, int relative, int moving_option, int profile_type, int velocity, int accel, int decel, int acc_jerk, int dec_jerk, int position);

		/// <summary>
		/// 단축 구동(구조체 이용)
		/// : old version name -->"eSnetMoveAxisPast"
		/// </summary>
		/// <param name="net">			: controller id						</param>
		/// <param name="axis">			: axis index						</param>
		/// <param name="move_type">	: (see enum "eSnetMoveType")		</param>
		/// <param name="data">			: (see struct "MOVE_COMPONENT")		</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetMoveSingle(int net, int axis, int move_type, out MOVE_COMPONENT data);

		/// <summary>
		/// 단축 구동
		/// : old version name -->"eSnetMoveAxisPast2"
		/// </summary>
		/// <param name="net">			: controller id						</param>
		/// <param name="axis">			: axis index						</param>
		/// <param name="move_type">	: (see enum "eSnetMoveType")		</param>
		/// <param name="velocity">		: [mm/min]	velocity				</param>
		/// <param name="accel">		: [msec]	acceleration			</param>
		/// <param name="decel">		: [msec]	deceleration			</param>
		/// <param name="jerk">			: [%]		jerk					</param>
		/// <param name="position">		: [um]		position				</param>		
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetMoveSingleEx(int net, int axis, int move_type, int velocity, int accel, int decel, int jerk, int pos);

		/// <summary>
		/// 단축 구동(설정된 Input Signal이 입력 될 때까지 구동)
		/// # move_type = 1003 --> "direction"인자로 전달된 "위치"로 S-CURVE 가감속 이송,입력 조건을 만족 하면 정지  
		/// # move_type = 1004 --> "direction"인자로 전달된 "위치"로 사다리꼴 가감속 이송,입력 조건을 만족 하면 정지  
		/// # move_type = 1005 --> "direction"인자로 전달된 "방향(1 or -1)" 으로 입력 조건을 만족 할 때 까지 "무한 이송" 
		/// # 입력 조건을 만족 하여 이송을 정지 할때 감속 시간은 제어기 "21번 파라미터"설정값이 적용 됨. 
		/// : old version name -->"eSnetMoveAxisPast2_IO"
		/// </summary>
		/// <param name="net">			: controller id										</param>
		/// <param name="axis">			: axis index										</param>
		/// <param name="move_type">	: (see enum "eSnetMoveType"), 
		///									ScurveIo		(1003),
		///									TrapezoidalIo	(1004),
		///									VelocityIo		(1005),							</param>
		/// <param name="velocity">		: [mm/min]	velocity								</param>
		/// <param name="accel">		: [msec]	acceleration[msec]						</param>
		/// <param name="decel">		: [msec]	deceleration							</param>
		/// <param name="jerk">			: [%]		jerk									</param>
		/// <param name="direction">	: [um]		position								</param>		
		/// <param name="port">			: input port										</param>		
		/// <param name="point">		: input point										</param>		
		/// <param name="area">			: "0x1" or "0xf"(see enum "eSnetOriginEtc")			</param>		
		/// <param name="edge">			: input point edge, low->high:"1", high->low:"0"	</param>				
		/// <returns>					: (see enum "eSnetApiReturnCode")					</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetMoveSingleExIo(int net, int axis, int move_type, int velocity, int accel, int decel, int jerk, int direction, int port, int point, int area, int edge);

		/// <summary>
		/// 단축 구동 (jog mode)
		/// : old version name ->"eSnetMoveAxisJog"
		/// </summary>
		/// <param name="net">			: controller id						</param>
		/// <param name="axis">			: axis index						</param>		
		/// <param name="velocity">		: [mm/min]	velocity				</param>
		/// <param name="accel">		: [msec]		acceleration		</param>
		/// <param name="decel">		: [msec]		deceleration		</param>
		/// <param name="jerk">			: [%]			jerk				</param>
		/// <param name="direction">	: "-1" : negative direction				
		///								: " 1" : positive direvtion 
		///								(see enum "eSnetMoveDirection")		</param>					
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetMoveSingleExJog(int net, int axis, int velocity, int accel, int decel, int jerk, int direction);

		/// <summary>
		/// 다축 동시 구동 지령 
		/// : old version name ->"eSnetMoveAxisMulti"
		/// </summary>
		/// <param name="net">			: controller id												</param>
		/// <param name="axis_count">	: 구동할 축 수												</param>
		/// <param name="axis">			: [array] 배열크기->ax_count,구동할 축 배열					</param>
		/// <param name="move_type">	: [array] 배열크기->ax_count,구동할 축의 Profile Type			
		///									(see "enum eSnetMoveType"),"1003"/"1004"/"1005"만 사용	</param>
		/// <param name="velocity">		: [array] 배열크기->ax_count, [mm/min] velocity				</param>
		/// <param name="accel">		: [array] 배열크기->ax_count, [msec] acceleration			</param>
		/// <param name="decel">		: [array] 배열크기->ax_count, [msec] deceleration			</param>
		/// <param name="jerk_acc">		: [array] 배열크기->ax_count, [%] accel_jerk				</param>
		/// <param name="jerk_dec">		: [array] 배열크기->ax_count, [%] decel_jerk				</param>
		/// <param name="position">		: [array],배열크기->ax_count, [um] target_position			</param>		
		/// <returns>					: (see enum "eSnetApiReturnCode")							</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetMoveMultiAxis(int net, int axis_count, out int axis, out int move_type,
			out int velocity, out int accel, out int decel, out int jerk_acc, out int jerk_dec, out int position);

		#endregion

		#region Moving :: Single with AD

		/*** MoveAxis According Extension Board A/D Value ***/

		/// <summary>
		/// 단축 구동 ( 무한 이송 / Ad 입력값 비교 후 정지 )
		/// (tip 1) "SNET-RTEX 옵션 보드 실장 제품" 및 "SNET-P AD 옵션 보드 실장 제품"에 적용 됩니다.  
		/// : old version name -->"eSnetMoveAxisVel_Ad"
		/// </summary>
		/// <param name="net">			: controller id																	</param>
		/// <param name="axis">			: axis index ("0" ~ )															</param>		
		/// <param name="velocity">		: [mm/min]	velocity															</param>
		/// <param name="accel">		: [msec]	acceleration time													</param>
		/// <param name="decel">		: [msec]	deceleration time													</param>
		/// <param name="direction">	: direction : "1" or "-1"														</param>							
		/// <param name="ad_index">		: AD0(0) ~ AD3(3)																</param>							
		/// <param name="ad_value">		: [mVolt] Ad voltage value 														</param>							
		/// <returns>					: (see enum "eSnetApiReturnCode")	
		///									CommunicationEventCode,
		///									MoveVelocityAdAxisBusy	= 1901,	// 인수로 전달된 축이 현재 사용 중 
		///									MoveVelocityAdNumber	= 1902,	// AD 번호 설정 오류 ( 0 ~ 3 ) 
		///									MoveVelocityAdDirection	= 1903,	// 인수로 전달된 축 이송 방향 오류 ( 1 or -1 ) 
		///									MoveVelocityAdVelocity	= 1904,	// 인수로 전달된 축 이송 속도 오류 
		///									MoveVelocityAdOthers	= 1999, // 기타 오류 								</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetMoveVelocityAd(int net, int axis, int velocity, int accel, int decel, int direction, int ad_index, int ad_value);

		/// <summary>
		/// 단축 구동 ( 위치 이송 / Ad 입력값 비교 후 정지 )
		/// : old version name -->"eSnetMoveAxisPos_Ad"
		/// </summary>
		/// <param name="net">			: controller id																	</param>
		/// <param name="axis">			: axis index ("0" ~ )															</param>		
		/// <param name="velocity">		: [mm/min]	velocity															</param>
		/// <param name="accel">		: [msec]	acceleration time													</param>
		/// <param name="decel">		: [msec]	deceleration time													</param>
		/// <param name="jerk">			: jerk																			</param>							
		/// <param name="position">		: target position																</param>							
		/// <param name="ad_index">		: AD0(0) ~ AD3(3)																</param>							
		/// <param name="ad_value">		: [mVolt] Ad voltage value														</param>							
		/// <returns>					: (see enum "eSnetApiReturnCode")	
		///									CommunicationEventCode,
		///									MoveVelocityAdAxisBusy	(1901),	// 인수로 전달된 축이 현재 사용 중 
		///									MoveVelocityAdNumber	(1902),	// AD 번호 설정 오류 ( 0 ~ 3 ) 
		///									MoveVelocityAdDirection	(1903),	// 인수로 전달된 축 이송 방향 오류 ( 1 or -1 ) 
		///									MoveVelocityAdVelocity	(1904),	// 인수로 전달된 축 이송 속도 오류 
		///									MoveVelocityAdOthers	(1999), // 기타 오류 								</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetMovePositionAd(int net, int axis, int velocity, int accel, int decel, int jerk, int position, int ad_index, int ad_value);

		#endregion

		#region Moving :: Interpolation

		/*** Move : Interpolation ***/

		/// <summary>
		/// 직선 보간 이송 지령
		/// : 최대 4축 보간 이송
		/// : old version name ->"eSnetMoveAxisInterpolationLine"
		/// </summary>
		/// <param name="net">				: 제어기 ip address4						</param>
		/// <param name="axis0">			: 첫 번째 축 번호, 사용하지 않을 때 "0xff"	</param>
		/// <param name="axis1">			: 두 번째 축 번호, 사용하지 않을 때 "0xff"	</param>
		/// <param name="axis2">			: 세 번째 축 번호, 사용하지 않을 때 "0xff"	</param>
		/// <param name="axis3">			: 네 번째 축 번호, 사용하지 않을 때 "0xff"	</param>
		/// <param name="axis0_position">	: 첫 번째 축(axis_1) 이송 좌표				</param>
		/// <param name="axis1_position">	: 두 번째 축(axis_2) 이송 좌표				</param>
		/// <param name="axis2_position">	: 세 번째 축(axis_3) 이송 좌표				</param>
		/// <param name="axis3_position">	: 네 번째 축(axis_4) 이송 좌표				</param>
		/// <param name="velocity">			: [mm/min]	보간 이송 속도					</param>
		/// <param name="accel">			: [msec]	가속시간						</param>
		/// <param name="decel">			: [msec]	감속시간						</param>
		/// <param name="jerk_acc">			: [%]		가속시간 내 jerk 비율			</param>
		/// <param name="jerk_dec">			: [%]       감속시간 내 jerk 비율			</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")
		///										
		///										InterpolationTrajFull (1100) ~
		///										InterpolationMovingPostionZero (1112)	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetMoveLine(int net, int axis0, int axis1, int axis2, int axis3,
			int axis0_position, int axis1_position, int axis2_position, int axis3_position,
			int velocity, int accel, int decel, int jerk_acc, int jerk_dec);

		/// <summary>
		/// 직선 보간 이송 지령 
		/// : ( "SNET-P8" -> 최대 8축, "SNET-RTEX" -> 최대 32축)
		/// : old version name -->"eSnetMoveAxisInterpolationLine2"
		/// </summary>
		/// <param name="net">			: 제어기 ip address4							</param>
		/// <param name="axis_count">	: 보간 이송을 할 축 갯수						</param>
		/// <param name="axis">			: [array]	배열크기->axCount,축 번호			</param>
		/// <param name="position">		: [array]	배열크기->axCount,[um] 이송 위치	</param>
		/// <param name="velocity">		: [mm/min]  보간 이송 속도						</param>
		/// <param name="accel">		: [msec]	가속시간							</param>
		/// <param name="decel">		: [msec]	감속시간							</param>
		/// <param name="jerk_acc">		: [%]		가속시간 내 jerk 비율				</param>
		/// <param name="jerk_dec">		: [%]       감속시간 내 jerk 비율				</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")				
		///									
		///									InterpolationTrajFull (1100) ~
		///									InterpolationMovingPositionZero (1112)		</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetMoveLineMultiAxis(int net, int axis_count, out int axis,
			out int position, int velocity, int accel, int decel, int jerk_acc, int jerk_dec);

		/// <summary>
		/// 반지름과 종점을 이용한 원호 보간
		/// : old version name ->"eSnetMoveAxisInterpolationArcRadius"
		/// </summary>
		/// <param name="net">				: 제어기 ip address4												</param>
		/// <param name="axis0">			: XY평면좌표 첫번째 축 번호											</param>
		/// <param name="axis1">			: XY평면좌표 두번째 축 번호											</param>
		/// <param name="axis2">			: XY평면 제외 직선 보간 축, 사용하지 않을 경우 "0xff"입력			</param>
		/// <param name="axis0_position">	: XY평면좌표 첫번째 축 종점 위치									</param>
		/// <param name="axis1_position">	: XY평면좌표 두번째 축 종점 위치									</param>
		/// <param name="axis2_position">	: XY평면 제외 직선 보간 축 종점 위치, 사용하지 않을 경우 "0"입력	</param>
		/// <param name="cw_ccw">			: "1"-->XY평명좌표   시계방향(cw),  (1-->4-->3-->2 사분면 방향), 
		/// 								 "-1"-->XY평면좌표 반시계방향(ccw), (1-->2-->3-->4 사분면 방향)		</param>
		/// <param name="radius">			: [um]		원호 반지름												</param>		 
		/// <param name="velocity">			: [mm/min]	보간 이송 속도											</param>
		/// <param name="accel">			: [msec]	가속시간												</param>
		/// <param name="decel">			: [msec]	감속시간												</param>
		/// <param name="jerk_acc">			: [%]		가속시간 내 jerk 비율									</param>
		/// <param name="jerk_dec">			: [%]       감속시간 내 jerk 비율									</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")					
		///										CommunicationEventCode, 
		///										InterpolationTrajFull			(1100) ~ 
		///										InterpolationMovingPositionZero	(1112)							</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetMoveArcRadius(int net, int axis0, int axis1, int axis2,
			int axis0_position, int axis1_position, int axis2_position, int cw_ccw, int radius,
			int velocity, int accel, int decel, int jerk_acc, int jerk_dec);

        /// <summary>
        /// 중심점과 각도를 이용한 원호보간
        /// : old version name ->"eSnetMoveAxisInterpolationArcAngle"
        /// </summary>
        /// <param name="net"> 				: 제어기 ip address4										</param>
        /// <param name="axis0">			: XY평면좌표 첫번째 축 번호									</param>
        /// <param name="axis1">			: XY평면좌표 두번째 축 번호									</param>
        /// <param name="axis2">			: XY평면 제외 직선 보간 축, 사용하지 않을 경우 "0xff"입력	</param>
        /// <param name="center_x">			: XY평면좌표 중심점 X 위치									</param>
        /// <param name="center_y">			: XY평면좌표 중심점 Y 위치									</param>
        /// <param name="angle">			: 각도, (+)각->XY평면에서 ccw방향, (-)각->XY평면에서 cw방향	</param>
        /// <param name="axis2_position">	: 직선 보간 축 위치, 사용하지 않을 경우 "0"입력				</param>
		/// <param name="velocity">			: [mm/min]	보간 이송 속도									</param>
        /// <param name="accel">			: [msec]	가속시간										</param>
        /// <param name="decel">			: [msec]	감속시간										</param>
        /// <param name="jerk_acc">			: [%]		가속시간 내 jerk 비율							</param>
        /// <param name="jerk_dec">			: [%]       감속시간 내 jerk 비율							</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")					
        ///										CommunicationEventCode, 	 
        ///										InterpolationTrajFull			(1100) ~ 
        ///										InterpolationMovingPositionZero	(1112)					</returns>	
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetMoveArcAngle(int net, int axis0, int axis1, int axis2,
			int center_x, int center_y, int angle, int axis2_position, int velocity, int accel, int decel, int jerk_acc, int jerk_dec);

		/// <summary>
		/// 중간 이동점을 이용한 원호 보간
		/// : old version name ->"eSnetMoveAxisInterpolationArcPoint"
		/// </summary>
		/// <param name="net"> 				: 제어기 ip address4										</param>
		/// <param name="axis0">			: XY평면좌표 첫번째 축 번호									</param>
		/// <param name="axis1">			: XY평면좌표 두번째 축 번호r								</param>
		/// <param name="axis2">			: XY평면 제외 직선 보간 축, 사용하지 않을 경우 "0xff"입력	</param>
		/// <param name="axis0_midpos">		: XY평면좌표 중간점 X 위치									</param>
		/// <param name="axis1_midpos">		: XY평면좌표 중간점 Y 위치									</param>
		/// <param name="axis0_endpos">		: XY평면좌표 종점 X 위치									</param>
		/// <param name="axis1_endpos">		: XY평면좌표 종점 Y 위치									</param>
		/// <param name="axis2_endpos">		: 직선 보간 축 종점 좌표, 사용하지 않을 경우 "0"입력		</param>
		/// <param name="velocity">			: [mm/min]	보간 이송 속도									</param>
		/// <param name="accel">			: [msec]	가속시간										</param>
		/// <param name="decel">			: [msec]	감속시간										</param>
		/// <param name="jerk_acc">			: [%]		가속시간 내 jerk 비율							</param>
		/// <param name="jerk_dec">			: [%]       감속시간 내 jerk 비율							</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")					
		///										CommunicationEventCode,  
		///										InterpolationTrajFull			(1100) ~ 
		///										InterpolationMovingPositionZero	(1112)					</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetMoveArcPoint(int net, int axis0, int axis1, int axis2,
			int axis0_midpos, int axis1_midpos, int axis0_endpos, int axis1_endpos, int axis2_endpos,
			int velocity, int accel, int decel, int jerk_acc, int jerk_dec);

		#endregion

		#region Moving :: Helical

		/*** Move : Helical ***/

		/// <summary>
		/// 헬리컬 구동 하기
		/// : old version name ->"eSnetMoveAxisHelical"
		/// </summary>
		/// <param name="net"> 				: controller id 								</param>						
		/// <param name="axis0">			: 원호 보간을 수행할 X axis index					</param>						
		/// <param name="axis1">			: 원호 보간을 수행할 Y axis index					</param>						
		/// <param name="axis2">			: 원호 보간과 함께 직선 보간을 수행할 Z axis index	</param>		
		/// <param name="center_x">			: 중심점 X 								</param>						
		/// <param name="center_y">			: 중심점 Y 								</param>					
		/// <param name="rotation_count">	: 회전수									</param>						
		/// <param name="angle">			: "rotation_count" 회전후 추가 이송 각도, (+)각 ccw방향, (-)각 cw방향</param>					
		/// <param name="axis2_position">	: 한 회전 마다 움직일 Z 측거리						</param>						
		/// <param name="cw_ccw">			: 회전 방향 : "-1"->반시계방향, "1"->시계방향		</param>				
		/// <param name="velocity">			: [mm/min]	보간 이송 속도							</param>
		/// <param name="accel">			: [msec]	가속시간								</param>
		/// <param name="decel">			: [msec]	감속시간								</param>
		/// <param name="jerk_acc">			: [%]		가속시간 내 jerk 비율					</param>
		/// <param name="jerk_dec">			: [%]       감속시간 내 jerk 비율					</param>	
		/// <returns>						: (see enum "eSnetApiReturnCode")					
		///										 
		///										InterpolationTrajFull			(1100) ~ 
		///										InterpolationMovingPositionZero	(1112)			</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetMoveHelical(int net, int axis0, int axis1, int axis2,
			int center_x, int center_y, int rotation_count, int angle, int axis2_position, int cw_ccw,
			int velocity, int accel, int decel, int jerk_acc, int jerk_dec);

        #endregion

        #region Moving :: Spline
        /// <summary>
        /// 스플라인 구동 하기
        /// </summary>
        /// <param name="net">				: controller id 																</param>						
        /// <param name="axis_x">			: (X, Y)평면좌표에서 스플라인 구동할 X방향 축										</param>
        /// <param name="axis_y">			: (X, Y)평면좌표에서 스플라인 구동할 Y방향 축										</param>						
        /// <param name="point_count">		: spline을 수행할 (x, y) 지령 points, 최소 3점 이상, 40점 이하						</param>
        /// <param name="x_points">			: array 배열, x point, 배열[0]이 첫번째 X 시작점									</param>
        /// <param name="y_points">			: array 배열, y point, 배열[0]dl 첫번째 Y 시작점									</param>
        /// <param name="sampling_count">	: Spline 곡선 계산식으로 산출될 미소 구간 Point 개수, 최소 4개, 최대 200개			</param>
        /// <param name="xs">				: Spline 곡선 계산식으로 산출된 미소 구간 Point의 "axis_x 축" 좌표 저장 배열 
        ///									  배열 크기 -> "sampling_count"													</param>
        /// <param name="ys">				: Spline 곡선 계산식으로 산출된 미소 구간 Point의 "axis_y 축" 좌표 저장 배열  
        ///									  배열 크기 -> "sampling_count"													</param> 	
        /// <param name="type">				: Spline 곡선 종류, "0" 고정(Cubic Spline)										</param> 	
        ///	<param name="axis3">			: Spline 곡선 평면 추과 동기 되어 움직이는, 추가 직선 보간 축 번호					</param> 	
        ///	<param name="axis3_positions">	: "axis3"축의 시작 좌표와 종료 좌표, 배열  
        ///									  배열 크기 ->2, axis3_position[0] ->시작 좌표, axis3_position[1] ->종료 좌표		</param> 	
        /// <param name="velocity">			: [mm/min]	보간 이송 속도														</param>
        /// <param name="accel">			: [msec]	가속시간																</param>
        /// <param name="decel">			: [msec]	감속시간																</param>
        /// <param name="jerk_acc">			: [%]		가속시간 내 jerk 비율													</param>
        /// <param name="jerk_dec">			: [%]       감속시간 내 jerk 비율													</param>	
        /// <returns>						: (see enum "eSnetApiReturnCode")					
        ///										SplineSamplingCountOver				(2501) ~						
        ///										SplineMovingCommandPositionError	(2508)																</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetMoveSpline(int net, int axis_x, int axis_y, int point_count, out int x_points, out int y_points,
			int sampling_count, out float xs, out float ys, int type,
			int axis3, out int axis3_positions,
			int velocity, int accel, int decel, int jerk_acc, int jerk_dec);

		#endregion

		#region Moving :: Continuous

		/*** Move : Continuous ***/
		///  # "eSnetBeginContiMakeJob"과 "eSnetEndContiMakeJob" 사이에 설정할 수 있는 함수 
		///		-보간 이송 -------------------------------	
		///		- eSnetMoveLine()
		///		- eSnetMoveLineMultiAxis()
		///		- eSnetMoveAcrRadius()
		///		- eSnetMoveArcAngle()
		///		- eSnetMoveArcPoint()
		///		- eSnetMoveHelical()
		///		- eSnetMoveSpline()
		///		-I/O--------------------------------------
		///		- eSnetSetContiOutputConfig()
		///		- eSnetRtexSetIoOutputPort
		///		- eSnetRtexSetIoOutputPoint()
		///		- eSnetPulseExSetIoOutputPoint()
		///		- eSnetPulseExSetIoOutputPort()
		///     - eSnetPulseSetUserOutputPoint()
		///		- eSnetSetOutputForTime()
		///		-기타--------------------------------------
		///		- eSnetSetAxisAbsRelMode()
		///		- eSnetSetContiDwell()

		/// <summary>
		/// 연속 보간 이송 채널 번호 변경 
		/// </summary>
		/// <param name="net">		: controller id								</param>						
		/// <param name="channel">	: channel index								</param>
		///							: SNET-P->"0 ~ 1",SNET-RTEX->"0 ~ 3"	
		/// <returns>				: (see enum "eSnetApiReturnCode")
		///								CommunicationEventCode, 
		///								ContinuousOverCount			(1201) ~ 
		///								ContinuousNotBeginCommand	(1203)		</returns>					
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetContiCh(int net, int channel);

		/// <summary>
		/// 연속 보간 이송 현재 채널 번호 확인
		/// </summary>
		/// <param name="net">		: controller id						</param>						
		/// <param name="channel">	: channel index						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetContiCh(int net, out int channel);

		/// <summary>
		/// 연속 보간 이송 Job 만들기 시작
		/// : old version name -->"eSnetSetContiMakeJobBegin"
		/// </summary>
		/// <param name="net">	: controller id						</param>						
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetBeginContiMakeJob(int net);

		/// <summary>
		/// 연속 보간 이송 Job 만들기 종료 
		/// : old version name -->"eSnetSetContiMakeJobEnd"
		/// </summary>
		/// <param name="net">	: controller id						</param>						
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetEndContiMakeJob(int net);

		/// <summary>
		/// 연속 보간 이송을 위해 설정한 API 전체 갯수와 Moving API 갯수
		/// </summary>
		/// <param name="net"> 					: controller id 																</param>						
		/// <param name="all_index_count">		: 연속 이송 보간을 위해 설정한 전체 API 갯수									</param>						
		/// <param name="moving_step_count">	: 연속 이송 보간을 위해 설정한 전체 API중에서 Motion Moving에 관련된 API 갯수	</param>						
		/// <returns>							: (see enum "eSnetApiReturnCode")												</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetContiJobIndexCount(int net, out int all_index_count, out int moving_step_count);

		/// <summary>
		/// 선택된 채널("eSnetSetContiCh")의 연속 보간 이송 Job 시작 하기
		/// </summary>
		/// <param name="net">		: controller id						</param>	
		/// <param name="channel">	: channel index to start job		</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetStartConti(int net, int channel);

		/// <summary>
		/// 연속 보간 이송 정지
		/// </summary>
		/// <param name="net">		: controller id							</param>
		/// <param name="channel">	: 연속 보간 이송 채널 번호				
		//								SNET-P->"0 ~ 1",SNET-RTEX->"0 ~ 3"	</param>
		/// <param name="decel">	: 감속 시간								</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetStopConti(int net, int channel, int decel);

		/// <summary>
		/// "연속 보간 이송"이 진행 중인지, 진행 중이라면 몇번째 Moving Step 진행 중인지 읽기
		/// </summary>
		/// <param name="net"> 					: controller id 											</param>						
		/// <param name="is_moving">			: 연속 보간 이송 동작 중인가? "1"->동작중, "0"->동작중 아님	</param>						
		/// <param name="current_motion_step">	: 현재 몇번째 Motion Moving Step 중인가?					</param>						
		/// <returns>							: (see enum "eSnetApiReturnCode")							</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetContiIsMotion(int net, out int is_moving, out int current_motion_step);

		/// <summary>
		/// "연속 보간 이송"이  진행 중인지, 진행 중이라면 몇번째 Moving Step 진행 중인지, 어떤 Moving Command로 구동 중인지, 연속이송보간의 결과가 어떤지 읽기
		/// </summary>
		/// <param name="net"> 						: controller id 															</param>						
		/// <param name="channel"> 					: channel index 															</param>						
		/// <param name="is_moving">					: 연속 보간 이송 동작 중인가? "1"->동작중, "0"->동작중 아님					</param>						
		/// <param name="current_motion_step">		: 현재 몇번째 Motion Moving Step 중인가?									</param>						
		/// <param name="current_motion_command">	: 현재 구동 중인 Motion이 어떤 Moving Command Id 인가?		
		///												(see enum "eSnetContiMovingInfo")										</param>
		///												(int)eSnetContiMovingInfo::MovingDoneOk(0),	// 연속 보간 이송 구동이 정상적으로 종료됨
		///												(int)eSnetContiMovingInfo::FuncMoveAxisInterpolationLine		(3300),
		///												(int)eSnetContiMovingInfo::FuncMoveAxisInterpolationArcRadius	(3310),
		///												(int)eSnetContiMovingInfo::FuncMoveAxisInterpolationArcAngle	(3311),
		///												(int)eSnetContiMovingInfo::FuncMoveAxisInterpolationArcPoint	(3312),
		///												(int)eSnetContiMovingInfo::FuncMoveAxisInterpolationLineEx		(3315),
		///												(int)eSnetContiMovingInfo::FuncSetContiDwell					(9379),	
		/// <returns>								: (see enum "eSnetApiReturnCode")	 
		///												CommunicationEventCode, 
		///												InterpolationTrajFull			(1101) ~ 
		///												InterpolationMovingPositionZero	(1112)									</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetContiInfoResult(int net, int channel, out int is_moving, out int current_motion_step, out int current_motion_command);

		/// <summary>
		/// 연속 보간 이송중 남은 거리가 설정 거리 보다 작으면 출력 접점 ON 또는 OFF 
		/// (tip 1)"eSnetSetContiOutputConfig"다음 지령 명령이 이송 명령이 아니면 실행 되지 않습니다.  
		/// (tip 2)"eSnetSetContiOutputConfig" 다음 보간 이송 명령 지령시 목표 위치가 현재 위치와 동일 하면 보간 이송명령이 
		///			실행되지 않고 종료 됩니다. 이때 만약 "distance_before_moving_step_end = 0" 이면 
		///			"eSnetSetContiOutputConfig"가 실행 되지 않습니다. 
		///			
		/// </summary>
		/// <param name="net"> 								: controller id 													</param>						
		/// <param name="output_count">						: 출력 접점수 (1 step당), 최대 8 접점 동시 출력 가능 				</param>
		/// <param name="select_distance_or_time">			: "0"->거리, "1"->시간	(미사용)									</param>		
		/// <param name="output_type">						: [array] 배열 크기->outputCount, 
		///														"0"->axis_io, "1"->remote_io, 
		///														"2"->rtex_user_io or Ad option board io							</param>
		/// <param name="port">								: [array] 배열 크기->output_count, port								</param>					
		/// <param name="point">							: [array] 배열크기->output_count, point								</param>						
		/// <param name="on_off">							: [array] 배열크기->output_count, "1"->on, "0"->off					</param>					
		/// <param name="distance_before_moving_step_end">	: [array] 배열크기->output_count, 출력 시작 기준 남은 이송 거리(um) </param>						
		/// <returns>										: (see enum "eSnetApiEventCode")	 
		///														CommunicationEventCode, 
		///														ContinuousOverCount			(1201), 
		///														ContinuousOverIndex			(1202),
		///														ContinuousNotBeginCommand	(1203),								</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetContiOutputConfig(int net, int output_count, int select_distance_or_time,
			out int output_type, out int port, out int point, out int on_off, out int distance_before_moving_step_end);

		/// <summary>
		/// "eSnetSetContiOutputConfig" 설정 값 확인  
		/// </summary>
		/// <param name="net">						: controller id </param>						
		/// <param name="output_index">				: 설정 정보를 읽을 index													</param>						
		/// <param name="output_count">				: 설정 출력 접점수 (1 step당)												</param>						
		/// <param name="select_distance_or_time">	: "0"->거리, "1"->시간(미사용)												</param>		
		/// <param name="output_type">				: [array] 배열크기->"8", "0"->axis_io, "1"->remote_io, "2"->rtex_user_io	</param>						
		/// <param name="port">						: [array] 배열크기->"8", port												</param>					
		/// <param name="point">					: [array] 배열크기->"8", point												</param>						
		/// <param name="on_off">					: [array] 배열크기->"8", "1"->on, "0"->off									</param>					
		/// <param name="distance_before_step_end">	: [array] 배열크기->"8", 출력 시작 기준 남은 이송 거리(um) 					</param>						
		/// <returns>								: (see enum "eSnetApiEventCode")											</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetContiOutputConfig(int net, int output_index, out int output_count, out int select_distance_or_time,
			out int output_type, out int port, out int point, out int on_off, out int distance_before_step_end);

		/// <summary>
		/// 연속 이송보간 중 대기시간 설정 
		/// </summary>
		/// <param name="net">		: controller id								</param>
		/// <param name="dwell">	: wait time									</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	 
		///								CommunicationEventCode, 
		///								ContinuousOverIndex			(1202),
		///								ContinuousNotBeginCommand	(1203),		</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetContiDwell(int net, int dwell);

		/// <summary>
		/// 연속 이송 보간 이송시 남은 이송 거리가 설정 거리 보다 작으면 설정 시간 동안 출력 접점 ON  
		/// # (tip 1)"eSnetSetContiOutputConfig"다음 지령 명령이 이송 명령이 아니면 실행 되지 않습니다.  
		///
		/// </summary>
		/// <param name="net"> 						: controller id 																	</param>						
		/// <param name="output_count">				: 연속이송보간시 사용할 output갯수(한 step당), 최대 8개 output까지 가능				</param>						
		/// <param name="output_type">				: [array] 배열 크기->output_count, "0"->axis_io, "1"->remote_io, "2"->rtex_user_io	</param>						
		/// <param name="port">						: [array] 배열 크기->output_count, port												</param>					
		/// <param name="point">					: [array] 배열 크기->output_count, point											</param>						
		/// <param name="distance_before_step_end">	: [array] 배열 크기->output_count, 출력 시작 기준 이송 남은 거리 					</param>						
		/// <param name="on_time">					: [array] 배열 크기->output_count, 출력 ON 유지 시간(msec)							</param>						
		/// <returns>								: (see enum "eSnetApiReturnCode")	 
		///												 
		///												ContinuousOverCount			(1201), 
		///												ContinuousOverIndex			(1202),
		///												ContinuousNotBeginCommand	(1203),											</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetContiOutputTrigger(int net, int output_count, out int output_type, out int port, out int point,
			out int distance_before_step_end, out int on_time);

		/// <summary>
		/// "eSnetSetContiOutputTrigger" 설정값 확인 
		/// </summary>
		/// <param name="net">						: controller id </param>						
		/// <param name="output_index">				: 설정 정보를 읽을 index													</param>						
		/// <param name="output_count">				: 연속이송보간시 사용할 output갯수(한 step당)								</param>						
		/// <param name="output_type">				: [array] 배열크기->"8", "0"->axis_io, "1"->remote_io, "2"->rtex_user_io	</param>						
		/// <param name="port">						: [array] 배열크기->"8", port												</param>					
		/// <param name="point">					: [array] 배열크기->"8", point												</param>						
		/// <param name="distance_before_step_end">	: [array] 배열크기->"8", 출력 시작 기준 이송 남은 거리 						</param>						
		/// <param name="on_time">					: [array] 배열크기->"8", 출력 ON 유지시간(msec)								</param>						
		/// <returns>								: (see enum "eSnetApiReturnCode")											</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetContiOutputTrigger(int net, int output_index, out int output_count, out int output_type, out int port, out int point,
			out int distance_before_step_end, out int on_time);

		#endregion

		#region Moving :: PTP

		/*** Move : PTP & Interpolation ***/
		// current reserved function

		/// <summary>
		/// Moving Profile은 "PTP 및 보간 이송" 메뉴얼 참조
		/// /// "Point1"에서 "Point2"로 이동할 때, 상하 축 상승 중 
		/// "Point1.1"지점에서 X축 이송을 시작하고,
		/// "Point1.2"지점에서 하강하여 포인트간 이송 시간을 줄일 수 있다
		/// </summary>
		/// <param name="net">							: controller id											</param>
		/// <param name="ptp_index">					: PTP이송구분번호:("0"~"7") : 동시 실행 가능 갯수 8개
		///													단, 동시실행가능 보간이동 갯수(15개) 범위내에서 실행 가능
		///													즉, 현재 10개의 보간 이송이 실행 중이면, 
		///														최대 5개의 PTP 이송을 실행 할 수 있다			</param>
		/// <param name="single_axis">					: single 이송 축 번호 (메뉴얼에서는 Z축(상하이동축))	</param>
		/// <param name="plane_axis1">					: 평면 구성 축 1번 (메뉴얼에서는 X축					</param>
		/// <param name="plane_axis2">					: 평면 구성 축 2번 (사용 하지 않을 경우: 0xff)			</param>
		/// <param name="plane_axis3">					: 평면 구성 축 3번 (사용 하지 않을 경우: 0xff)			</param>
		/// <param name="single_velocity">				: [mm/min]	signle축 이송 속도							</param>
		/// <param name="single_accel">					: [msec]	single축 가속도								</param>
		/// <param name="single_decel">					: [msec]	signle축 감속도								</param>
		/// <param name="single_jerk">					: [%]		single축 저크								</param>
		/// <param name="single_position">				: [um]		signle축 "남은거리", 남은 이송거리가 
		///															  설정거리 보다 작으면 평명 축 이송 시작	</param>
		/// <param name="single_destination_position1">	: single축 첫번째 이송 위치(Point1.1)					</param>
		/// <param name="single_destination_position2">	: single축 두번재 이송 위치(Point1.2)					</param>
		/// <param name="plane_velocity">				: [mm/min]	평면 축 보간이송 속도						</param>
		/// <param name="plane_accel">					: [msec]		평면 축 보간이송 가속시간				</param>
		/// <param name="plane_decel">					: [msec]		평면 축 보간이송 감속시간				</param>
		/// <param name="plane_jerk">					: [%]			평명 축 보간이송 저크					</param>
		/// <param name="plane_position">					: 평면 축 보간이송 남은거리(축 이송거리 벡터합)
		///													남은거리가 설정거리 보다 작으면 single축 이송 시작	</param>
		/// <param name="plane_destination_position1">	: 평면 구성 첫번째 이송 위치 (사용하지 않으면 "0")		</param>
		/// <param name="plane_destination_position2">	: 평면 구성 두번째 이송 위치 (사용하지 않으면 "0")		</param>
		/// <param name="plane_destination_position3">	: 평면 구성 세번째 이송 위치 (사용하지 않으면 "0")		</param>
		/// <returns>									: (see enum "eSnetApiReturnCode")					
		///													 
		///													PtpMoveSetOverCount				(2201) ~ 
		///													PtpMovePlaneAndSigleAxisOvrlap	(2214)				</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetMovePtp(int net, int ptp_index, int single_axis,
			int plane_axis1, int plane_axis2, int plane_axis3,
			int single_velocity, int single_accel, int single_decel, int single_jerk,
			int single_position, int single_destination_position1, int single_destination_position2,
			int plane_velocity, int plane_accel, int plane_decel, int plane_jerk,
			int plane_position, int plane_destination_position1, int plane_destination_position2, int plane_destination_position3);

		/// <summary>
		/// PTP 및 보간구동 상태 읽기
		/// </summary>
		/// <param name="net">			: controller id													</param>
		/// <param name="ptp_index">	: PTP 이송 구분 번호:("0 ~"7"): 동시 실행 가능 갯수 : 8개
		///									단, "동시 실행 가능 보간 이송 갯수(15개)" 범위 내에서 실행 가능 
		///									즉, 현재 10개의 보간 이송이 실행 중이면 
		///									최대 5개의 PTP이송을 실행할 수 있다							</param>
		/// <param name="status">		: 지령 인자에 대한 return 값,
		///									"0": ready,
		///									"1": PTP이송 중,
		///									"2": PTP이송 완료,
		///									"3": 이송 실패,												</param>
		/// <returns>					(see enum "eSnetApiReturnCode")									</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetPtpStatus(int net, int ptp_index, out int status);

		#endregion

		#region Moving :: Override

		/*** Move : Override ***/

		/// <summary>
		/// 단축 이송시 속도 override 실행 
		/// : old version name ->"eSnetSetOverrideVel"
		/// </summary>
		/// <param name="net"> 		: controller id						</param>						
		/// <param name="axis">		: axis index ("0"~ )				</param>						
		/// <param name="velocity">	: [mm/min]: override 속도			</param>						
		/// <param name="accel">	: [msec]	: override 가속 시간	</param>
		/// <param name="decel">	: [msec]	: override 감속 시간	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")					
		///								CommunicationEventCode, 
		///								OverrideAxisNumber	(1301) ~ 
		///								OverrideVelocity	(1304)		</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetOverrideVelocity(int net, int axis, int velocity, int accel, int decel);

		/// <summary>
		/// 위치 이송 중 속도 오버라이드 지령
		/// </summary>
		/// <param name="axis">				: axis index </param>
		/// <param name="profile_type">		: "1000"--> S-Curve, "1001"--> Trapezoidal </param>
		/// <param name="velocity">			: [um] override 속도 </param>
		/// <param name="accel">			: [msec] override 가속 시간 </param>
		/// <param name="decel">			: [msec] override 감속 시간 </param>
		/// <param name="acc_jerk">			: [%] 가속 시 jerk </param>
		/// <param name="dec_jerk">			: [%] 감속 시 jerk </param>
		/// <returns>						: (see enum "eSnetApiReturnCode")					
		///										CommunicationEventCode, 
		///										OverrideExInvalidAxis			(6003), 
		///										OverrideExNoVelocity			(6050), 
		///										OverrideExNoJerk				(6052),
		///										OverrideExNotRunning			(6070),	
		/// </returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetOverrideVelocityEx(int net, int axis, int profile_type, int velocity, int accel, int decel, int acc_jerk, int dec_jerk);

		/// <summary>
		/// 보간 이송시 속도 override 실행 
		/// : old version name ->"eSnetSetOverrideLineVel"
		/// </summary>
		/// <param name="net"> 		: controller id											</param>						
		/// <param name="axis">		: axis index(보간 이송에 사용중인 축 중 아무거나 선택)	</param>						
		/// <param name="velocity">	: [mm/min]: override 속도								</param>						
		/// <param name="accel">	: [msec]	: override 가속 시간		 				</param>
		/// <param name="decel">	: [msec]	: override 감속 시간						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")					
		///								CommunicationEventCode, 
		///								OverrideAxisNumber			(1301),  
		///								OverrideVelocity			(1304),
		///								OverrideInterpolationNotUse (1305),					</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetOverrideInterpolationVelocity(int net, int axis, int velocity, int accel, int decel);

		/// <summary>
		/// 목표 위치까지 이동하면서 복수의 지정 위치에서 속도를 변경하는 단축 이송 실행 
		/// : old version name ->"eSnetMoveOverrideVelAtMultiPos"
		/// </summary>
		/// <remarks>
		/// (tip 1) 모터 정지 상태서 사용해야 됩니다.  
		/// (tip 2) 좌표 증가 방향으로 이송할 때는 "overide_position" 값이 "현재 좌표"보다 커야 됩니다. 
		///        좌표 감소 방향으로 이송할 때는 "overide_position" 값이 "현재 좌표"보다 작아야 됩니다. 
		///
		///  (예1) 현재 좌표 100mm이고 목표 좌표 150mm 위치로 이송 중(좌표 증가) 110mm, 120mm, 130mm 지점에서 속도를 변경 하는 경우  
		///	     -> array_count=3	
		///      -> overide_position[0]=110, 
		///		    overide_position[1]=120,
		///	  	    overide_position[2]=130 
		///
		///  (예2) 현재 좌표 150mm이고 목표 좌표 100mm 위치로 이송 중(좌표 감소) 140mm, 130mm, 120mm 지점에서 속도를 변경 하는 경우  
		///      -> array_count=3	
		///      -> overide_position[0]=140, 
		///		    overide_position[1]=130,
		///		    overide_position[2]=120 
		/// </remarks>
		/// <param name="net"> 				: controller id											</param>						
		/// <param name="axis">				: axis index											</param>						
		/// <param name="velocity">			: [mm/min]:이송 속도									</param>						
		/// <param name="accel">		    : [msec]	: 가속 시간				 					</param>
		/// <param name="decel">			: [msec]	: 감속 시간									</param>
		/// <param name="jerk">				: [%]		: jerk										</param>
		/// <param name="position">			: [um]	: 이송 좌표 									</param>
		/// <param name="mode">				: 속도 override 적용 위치 source 										
		///									: "0"--> actual posion, "1"-->command position			</param>
		/// <param name="array_count">		: 위치 배열 갯수 -> 최대 10개  							</param>
		/// <param name="override_position">: [array] 배열 크기->array_count,속도 변경 적용 좌표	</param>
		/// <param name="override_velocity">: [array] 배열 크기->array_count,Override 속도			</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")					
		///											CommunicationEventCode, 
		///											OverrideAxisNumber			(1301),  
		///											OverrideAxisBusy			(1306),
		///											OverrideParameter			(1307),				</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetOverrideVelocityAtMultiPosition(int net, int axis, int velocity, int accel, int decel, int jerk, int position, int mode, int array_count, out int override_position, out int override_velocity);

		/// <summary>
		/// 목표 위치까지 이동하면서 특정 1개의 위치에서 속도및 가/감속 시간이 변경 되는 단축 이송 실행 
		/// : old version name ->"eSnetMoveOverrideAccelVelDecelAtPos"
		/// </summary>
		/// <remarks>
		/// (tip 1) 모터 정지 상태서 사용해야 됩니다.  
		/// </remarks>
		/// <param name="net"> 					: controller id									</param>						
		/// <param name="axis">					: axis index									</param>						
		/// <param name="velocity">				: [mm/min] override 속도						</param>						
		/// <param name="accel">				: [msec]	 override 가속 시간					</param>
		/// <param name="decel">				: [msec]	 override 감속 시간					</param>
		/// <param name="jerk">					: [%]		 jerk								</param>
		/// <param name="position">				: [um]	 이송 좌표 								</param>
		/// <param name="mode">					: 속도 override 적용 위치 source 										
		///										: "0"--> actual posion, "1"-->command position	</param>
		/// <param name="override_position">	: [um] : 속도를 변경할 위치		   				</param>
		/// <param name="override_velocity">	: [mm/min] 변경 속도   	      					</param>
		/// <param name="override_accel">		: [msec]	 속도 override 적용 가속 시간		</param>
		/// <param name="override_decel">		: [msec]	 속도 override 적용 감속 시간		</param>
		/// <returns>							: (see enum "eSnetApiReturnCode")					
		///											CommunicationEventCode, 
		///											OverrideAxisNumber			(1301), 
		///											OverrideVelocity			(1304), 
		///											OverrideAxisBusy			(1306),
		///											OverrideParameter			(1307),			</returns>

		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetOverrideAccelVelocityDecelAtPosition(int net, int axis, int velocity, int accel, int decel, int jerk,
			int position, int mode, int override_position, int override_velocity, int override_accel, int override_decel);

		/// <summary>
		/// 모션 구동 중 새로운 목표 위치로 변경
		/// </summary>
		/// <param name="net">			: controller network id</param>
		/// <param name="axis">			: axis index</param>
		/// <param name="position">		: 변경할 위치(um)</param>
		/// <param name="relative">		: 'position' 인자 상대 이송 유무. 1(true): 상대 이송, 0(false): 절대 이송</param>
		/// <returns>							: (see enum "eSnetApiReturnCode")					
		///											CommunicationEventCode, 
		///											OverridePositionInvalidAxis			(1351), 
		///											OverridePositionNotRunning			(1352), 
		///											OverridePositionInterpolation		(1353),
		///											OverridePositionInvalidPoisition	(1354),
		///											OverridePositionInvalidRelative		(1355),
		/// </returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetOverridePosition(int net, int axis, int position, int relative);

		#endregion

		#region Moving :: MPG (SNET-RTEX)

		/*** Move : MPG Mode ***/

		/// <summary>
		/// "MPG로 Motion 구동" 환경 설정
		/// : old version name -->"eSnetSetMpgConfig"
		/// </summary>
		/// <param name="net">			: controller id													</param>
		/// <param name="use_mode">		: "1"->MPG Mode 사용, "0"->MPG Mode 사용 안함					</param>
		/// <param name="magnification">: MPG 구동축 번호 지정,bit 단위 "0x01 ~ 0xffffffff"				</param>
		/// <param name="use_axes">		: bit번호 == 축 번호 1:1 매칭					
		///										(예) 0번 축, 2번 축을 MPG로 구동 하고 싶을 때 : 0x05
		///										(예) 3번 축, 7번 축을 MPG로 구동 하고 싶을 때 : 0x0088	</param>	
		/// <returns>					: (see enum "eSnetApiReturnCode")								
		///									CommunicationEventCode,
		///									IsNotSteadyState (1401)										</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexSetMpgConfig(int net, int use_mode, int magnification, int use_axes);

		/// <summary>
		/// "eSnetRtexSetMpgConfig" 사용자 설정값 읽기 
		/// : old version name -->"eSnetGetMpgConfig"
		/// </summary>
		/// <param name="net"> 				: controller id												</param>
		/// <param name="use_mode">			: "1"->MPG Mode 사용, "0"->MPG Mode 사용 안함				</param>
		/// <param name="magnification">	: MPG로 구동할 때 속도 배율, default->"1", max->"100배"		</param>
		/// <param name="use_axes">			: MPG 구동축 번호 지정,bit 단위 "0x01 ~ 0xffffffff"			</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")							</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetMpgConfig(int net, out int use_mode, out int magnification, out int use_axes);

		#endregion

		#region Absolute / Relative Mode

		/*** Position Absolute/Relative Mode ***/

		/// <summary>
		/// 축별 상대값 구동/절대값 구동 설정 하기
		/// : old version name ->"eSnetSetAxisAbsRelMode"
		/// </summary>
		/// <param name="net"> 		: controller id 						</param>						
		/// <param name="axis">		: axis index							</param>						
		/// <param name="relative">	: "0"->절대값 구동, "1"->상대값 구동	</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetAbsRelMode(int net, int axis, int relative);

		/// <summary>
		/// 축별 상대값 구동/절대값 구동 설정값 읽기
		/// : old version name ->"eSnetGetAxisAbsRelMode"
		/// </summary>
		/// <param name="net"> 		: controller id 						</param>						
		/// <param name="axis">		: axis index							</param>						
		/// <param name="relative">	: "0"->절대값 구동, "1"->상대값 구동	</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetAbsRelMode(int net, int axis, out int relative);

		#endregion

		#region Position Loopback

		/*** Position Loopback ***/

		/// <summary>
		/// Loopback 기능 설정 하기 
		/// : old version name ->"eSnetSetAxisLoopback"
		/// </summary>
		/// <param name="net"> 		: controller id 					</param>						
		/// <param name="axis">		: axis index						</param>						
		/// <param name="loopback">	: "1"->사용, "0"->사용하지 않음		</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetLoopback(int net, int axis, int loopback);

		/// <summary>
		/// Loopback 기능 설정값 읽기 
		/// : old version name ->"eSnetGetAxisLoopback"
		/// </summary>
		/// <param name="net"> 		: controller id 					</param>						
		/// <param name="axis">		: axis index						</param>						
		/// <param name="loopback">	: "1"->사용, "0"->사용하지 않음		</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetLoopback(int net, int axis, out int loopback);

        #endregion

        #region Operation ( SNET-ECAT )

        /// <summary>
        /// 축의 동작 모드 읽기 ( SNET-ECAT )
        /// </summary>
        /// <param name="net">		: controller id</param>
        /// <param name="axis">		: 읽을 축 번호</param>
        /// <param name="mode">		: 읽은 mode 정보가 저장될 버퍼 (see enum "eSnetOperationMode")
        /// @ "1"					: Cyclic Position Mode
        /// @ "2"					: Cyclic Velocity Mode
        /// @ "3"					: Cyclic Torque Mode
        /// </param>
        /// <returns>				: (see enum "eSnetApiReturnCode")</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetGetOperationMode(int net, int axis, out int mode);

        /// <summary>
        /// 축의 동작 모드 쓰기 ( SNET-ECAT )
        /// </summary>
		/// <param name="net">		: controller id</param>
        /// <param name="axis">		: 저장할 축 번호</param>
        /// <param name="mode">		: 저장한 mode 정보 (see enum "eSnetOperationMode")
        /// @ "1"					: Cyclic Position Mode
        /// @ "2"					: Cyclic Velocity Mode
        /// @ "3"					: Cyclic Torque Mode
		/// </param>
        /// <returns>				: (see enum "eSnetApiReturnCode")</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetSetOperationMode(int net, int axis, int mode);

        #endregion

        #region Command Position

        /*** Command Position ***/
        /// 축별 현재 "Command Position" 및 "Actual Position " 확인 및 좌표 재설정
        ///		-> "Command Position(절대 좌표)":위치 지령 기준 현재 좌표  
        ///		-> "Actual Position(기계 좌표)":"인코더 입력값"으로 계산된 현재 좌표     
        ///

        /// <summary>
        /// 축 "Command Position(절대 좌표)" 변경  
        /// (tip 1)"Command Position"변경 후, 변경 전후 좌표 편차를 이용하여 "Actual Position" 자동 변경 
        /// (tip 2)"Command Position 과 Actual Position"이 일치하면 "eSnetSetActualPosition" 사용과 
        ///			동일한 결과를 보여 줍니다.
        /// : old version name ->"eSnetSetAxisCommandPosition" 	
        /// </summary>
        /// <param name="net"> 		: controller id 					</param>						
        /// <param name="axis">		: ax number							</param>						
        /// <param name="position">	: command position, [um]			</param>						
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetCommandPosition(int net, int axis, int position);

		/// <summary>
		/// 축 "Command Position(절대 좌표)" 현재값 확인 
		/// : old version name ->"eSnetGetAxisCommandPosition" 
		/// </summary>
		/// <param name="net"> 		: controller id 					</param>						
		/// <param name="axis">		: ax number							</param>						
		/// <param name="position">	: command position, [um]			</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetCommandPosition(int net, int axis, out int position);

		/// <summary>
		/// 축별 "이송 지령 명령 전달 좌표" 확인 
		/// : old version name ->"eSnetGetAxisTargetCommandPosition"
		/// </summary>
		/// <param name="net"> 		controller id 					</param>						
		/// <param name="axis">		axis index						</param>						
		/// <param name="position">	command position, [um]			</param>
		/// <returns>				(see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetTargetPosition(int net, int axis, out int position);

		#endregion

		#region Actual Position

		/*** Position ***/

		/// <summary>
		/// 축 "Actual Position(기계 좌표)" 변경  
		/// (tip 1) "Actual Position" 변경 후,변경 전후 좌표 편차를 이용하여 "Command Position"자동 변경 
		/// (tip 2)"Command Position 과 Actual Position"이 일치하면 "eSnetSetCommandPosition" 사용과 
		///			동일한 결과를 보여 줍니다.
		/// : old version name ->"eSnetSetAxisActualPosition" 
		/// </summary>	
		/// <param name="net"> 		: controller id					</param>						
		/// <param name="axis">		: axis index					</param>						
		/// <param name="position">	: actual position				</param>						
		/// <returns>				(see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetActualPosition(int net, int axis, int position);

		/// <summary>
		/// 축 "Actual Position(기계 좌표)" 확인 
		/// : old version name ->"eSnetGetAxisActualPosition"
		/// </summary>
		/// <param name="net"> 		: controller id 					</param>						
		/// <param name="axis">		: axis index						</param>						
		/// <param name="position">	: actual position,[um]				</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		/// <returns></returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetActualPosition(int net, int axis, out int position);

		/// <summary>
		/// 원하는 축들의 현재 지령 위치와 실제 위치를 한번에 읽음
		/// </summary>
		/// <param name="net">				: controller id														</param>
		/// <param name="axisCount">		: 첫번째 축(0번) 부터 위치를 읽고 싶은 축 갯수						</param>
		/// <param name="positionCommand">	: 배열, 현재 지령 위치,	[um],												
		///										(예) positionCommand[0] : "0"번 축(첫번째 축) 현재 지령 위치,
		///										     positionCommand[1] : "1"번 축(두번째 축) 현재 지령 위치,	</param>
		/// <param name="positionActual">	: 배열, 현재 실제 위치, [um],
		///										(예) positionActual[0] : "0"번 축(첫번째 축) 현재 실제 위치,
		///											 positionActual[1] : "1"번 축(두번째 축) 현재 실제 위치,	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")									</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetPosition(int net, int axisCount, out int positionCommand, out int positionActual);

		/// <summary>
		/// 축 "Command Position"과 "Actual Position"을 동일 한 좌표로 변경 하기 
		/// (tip 1)"원점 이송" 완료시 사용 됩니다. 
		/// : old versin name ->"eSnetSetAxisHomePosition"
		/// </summary>
		/// <param name="net"> 		: controller id 					</param>						
		/// <param name="axis">		: axis index						</param>						
		/// <param name="position">	: 변경 좌표, [um]					</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetHomePosition(int net, int axis, int position);

		/// <summary>
		/// Z("C")상을 Catpure한 위치값 읽기
		/// (tip 1)Z상 이용 원점 이송 동작 시 사용 됩니다. 
		/// : old version name ->"eSnetGetAxisZCapturePosition"
		/// </summary>
		/// <param name="net"> 		: controller id 					</param>						
		/// <param name="axis">		: axis index						</param>						
		/// <param name="position">	: Z상 캡처 좌표 					</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetLatchPositionZPhase(int net, int axis, out int position);

        #endregion

        #region Command Velocity

        /// <summary>
        /// 지령(프로파일) 속도 읽기
        /// </summary>
        /// <param name="net">			: controller id</param>
        /// <param name="axis">			: 축 번호</param>
        /// <param name="unit">			: 읽을 속도 단위, 열거자(enum) 'SnetVelocityUnit' 참조</param>
        /// <param name="velocity">		: 읽어서 저장할 지령(프로파일) 속도</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetGetCommandVelocity(int net, int axis, int unit, out int velocity);

        #endregion

        #region Torque ( SNET-ECAT)

        /// <summary>
        /// 축의 지령 Torque 값 읽기 ( SNET-ECAT)
        /// </summary>
        /// <param name="net">		: controller id</param>
        /// <param name="axis">		: 읽을 시작 축 번호</param>
        /// <param name="count">	: 읽을 축 개수</param>
        /// <param name="data">		: 읽은 Torque 데이터가 축 수만큼 저장될 버퍼</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetGetCommandTorque(int net, int axis, int count, out int data);

        /// <summary>
        /// 축의 실제 Torque 값 읽기 ( SNET-ECAT)
        /// </summary>
        /// <param name="net">		: controller id</param>
        /// <param name="axis">		: 읽을 시작 축 번호</param>
        /// <param name="count">	: 읽을 축 개수</param>
        /// <param name="data">		: 읽은 Torque 데이터가 축 수만큼 저장될 버퍼</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetGetActualTorque(int net, int axis, int count, out int data);

        /// <summary>
		/// 축의 최대 지령 Torque 값 읽기 ( SNET-ECAT)
		/// * Reset 함수를 호출해야 최대 Torque 데이터가 Clear 됨
		/// </summary>
		/// <param name="net">				: controller id</param>
        /// <param name="axis">				: 읽을 시작 축 번호</param>
        /// <param name="count">			: 읽을 축 개수</param>
		/// <param name="negative_data">	: 읽은 음방향 최대 Torque 데이터가 축 수만큼 저장될 버퍼</param>
		/// <param name="positive_data">	: 읽은 양방향 최대 Torque 데이터가 축 수만큼 저장될 버퍼</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetGetCommandMaxTorque(int net, int axis, int count, out int negative_data, out int positive_data);

        /// <summary>
        /// 축의 평균 지령 Torque 값 읽기 ( SNET-ECAT)
		/// * Reset 함수를 호출해야 최대 Torque 데이터가 Clear 됨
        /// </summary>
        /// <param name="net">				: controller id</param>
        /// <param name="axis">				: 읽을 시작 축 번호</param>
        /// <param name="count">			: 읽을 축 개수</param>
        /// <param name="data">				: 읽은 평균 Torque 데이터가 축 수만큼 저장될 버퍼</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetGetCommandAverageTorque(int net, int axis, int count, out int data);

        /// <summary>
        /// 축의 최대 Torque 차이값 읽기 ( SNET-ECAT)
		/// * Reset 함수를 호출해야 최대 Torque 데이터가 Clear 됨
        /// </summary>
        /// <param name="net">				: controller id</param>
        /// <param name="axis">				: 읽을 시작 축 번호</param>
        /// <param name="count">			: 읽을 축 개수</param>
        /// <param name="negative_data">	: 읽은 음방향 최대 Torque 차이값이 축 수만큼 저장될 버퍼</param>
        /// <param name="positive_data">	: 읽은 양방향 최대 Torque 차이값이 축 수만큼 저장될 버퍼</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetGetDeltaMaxTorque(int net, int axis, int count, out int negative_data, out int positive_data);

        /// <summary>
        /// 축의 평균 Torque 차이값 읽기 ( SNET-ECAT)
		/// * Reset 함수를 호출해야 최대 Torque 데이터가 Clear 됨
        /// </summary>
        /// <param name="net">				: controller id</param>
        /// <param name="axis">				: 읽을 시작 축 번호</param>
        /// <param name="count">			: 읽을 축 개수</param>
        /// <param name="data">				: 읽은 평균 Torque 차이값이 축 수만큼 저장될 버퍼</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetGetDeltaAverageTorque(int net, int axis, int count, out int data);

        /// <summary>
        /// 축의 지령 Torque 제한치 읽기 ( SNET-ECAT)
        /// </summary>
        /// <param name="net">				: controller id</param>
        /// <param name="axis">				: 읽을 축 번호</param>
        /// <param name="negative_data">	: 읽은 음방향 지령 Torque 제한치가 저장될 버퍼</param>
        /// <param name="positive_data">	: 읽은 양방향 지령 Torque 제한치가 저장될 버퍼</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetGetCommandTorqueLimit(int net, int axis, out int negative_data, out int positive_data);

        /// <summary>
        /// 축의 지령 Torque 제한치 쓰기 ( SNET-ECAT)
		/// * 설정된 제한치를 비활성화 하려면 제한치를 0으로 설정할 것
        /// </summary>
        /// <param name="net">				: controller id</param>
        /// <param name="axis">				: 저장할 축 번호</param>
        /// <param name="negative_data">	: 저장할 음방향 지령 Torque 제한치</param>
        /// <param name="positive_data">	: 저장할 양방향 지령 Torque 제한치</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetSetCommandTorqueLimit(int net, int axis, int negative_data, int positive_data);

        /// <summary>
        /// 축의 Torque 차이값 제한치 읽기 ( SNET-ECAT)
        /// </summary>
        /// <param name="net">				: controller id</param>
        /// <param name="axis">				: 읽을 축 번호</param>
        /// <param name="negative_data">	: 읽은 음방향 Torque 차이값 제한치가 저장될 버퍼</param>
        /// <param name="positive_data">	: 읽은 양방향 Torque 차이값 제한치가 저장될 버퍼</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetGetDeltaTorqueLimit(int net, int axis, out int negative_data, out int positive_data);

        /// <summary>
        /// 축의 Torque 차이값 제한치 쓰기 ( SNET-ECAT)
		/// * 설정된 제한치를 비활성화 하려면 제한치를 0으로 설정할 것
        /// </summary>
        /// <param name="net">				: controller id</param>
        /// <param name="axis">				: 저장할 축 번호</param>
        /// <param name="negative_data">	: 저장할 음방향 Torque 차이값 제한치</param>
        /// <param name="positive_data">	: 저장할 양방향 Torque 차이값 제한치</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetSetDeltaTorqueLimit(int net, int axis, int negative_data, int positive_data);

        /// <summary>
		/// 축의 지령 Torque Gain(%) 읽기 ( SNET-ECAT)
		/// </summary>
		/// <param name="net">		: controller id</param>
		/// <param name="axis">		: 읽을 축 번호</param>
		/// <param name="data">		: 읽은 Gain(%)이 저장될 버퍼
		/// * ex:) [1] -> 0.1% / [500] -> 50%
		/// </param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetGetCommandTorqueGain(int net, int axis, out int data);

        /// <summary>
        /// 축의 지령 Torque Gain(%) 쓰기 ( SNET-ECAT)
		/// * 설정된 Gain을 비활성화 하려면 Gain을 0으로 설정할 것
        /// </summary>
        /// <param name="net">		: controller id</param>
        /// <param name="axis">		: 저장할 축 번호</param>
        /// <param name="data">		: 저장할 Gain(%)
		/// * ex:) [1] -> 0.1% / [500] -> 50%
		/// </param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetSetCommandTorqueGain(int net, int axis, int data);

        /// <summary>
        /// 축의 실제 torque를 지령 torque로 동기 활성화 유무 읽기 ( SNET-ECAT)
        /// </summary>
        /// <param name="net">		: controller id</param>
        /// <param name="axis">		: 읽을 축 번호</param>
        /// <param name="enable">	: 읽은 동기 활성화 유무
		/// @ "1"					: 활성화
		/// @ "2"					: 비활성화
		/// </param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetGetActualTorqueSync(int net, int axis, out int enable);

        /// <summary>
        /// 축의 실제 torque를 지령 torque로 동기 활성화 유무 쓰기 ( SNET-ECAT)
        /// </summary>
        /// <param name="net">		: controller id</param>
        /// <param name="axis">		: 저장할 축 번호</param>
        /// <param name="enable">	: 동기 활성화 유무
        /// @ "1"					: 활성화
        /// @ "2"					: 비활성화
        /// </param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetSetActualTorqueSync(int net, int axis, int enable);

        #endregion

        #region Roll Over

        /*** Roll over(좌표 재설정) ***/

        /// <summary>
        /// 축 이송 중 현재 지령 좌표가 설정 좌표 보다 크거나 작으면 "0"으로 변경하고 다시 시작
        /// (tip 1) 무한 회전 운동"이 요구되는 응용 분야에 적용
        /// : old version name -->"eSnetSetAxisRollover"
        /// </summary>
        /// <param name="net">		: controller id																</param>
        /// <param name="axis">		: axis index																</param>
        /// <param name="position">	: 좌표 재설정 기준 좌표
        ///								-. 양수
        ///								  -> 현재 좌표가 설정값 보다 크거나 같으면 현재 좌표를 "0"으로 초기화 
        ///								-. 음수
        ///								  -> 현재 좌표가 설정값 보다 작거나 같으면 현재 좌표를 "0"으로 초기화	</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")
        ///								CommunicationEventCode,
        ///								PositionAutoResetOverAxisIndex (2301)									</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetRollover(int net, int axis, int position);

		/// <summary>
		/// "eSnetSetAxisRollover" 설정값 확인 하기 
		/// : old version name -->"eSnetGetAxisRollover"
		/// </summary>
		/// <param name="net">		: controller id								</param>
		/// <param name="axis">		: axis index								</param>
		/// <param name="position">	: 좌표 재설정 기준 좌표						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")
		///								CommunicationEventCode,
		///								PositionAutoResetOverAxisIndex (2301)	</returns>						
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetRollover(int net, int axis, out int position);

		#endregion

		#region Axis I/O

		/*** Axis I/O ***/

		/// <summary>
		/// 실시간 축 입출력의 감지 여부를 확인
		/// (tip 1) 전기적인 "active high/active low" 상태가 아닌, 파라미터 설정값이 적용된 최종 입출력 ON/OFF 상태를 나타냅니다.  
		/// (tip 2) -limit_sensor / +limit_sensor / origin_sensor의 전기적인 active high / active low설정은 제어기 basic parameter에서 설정 합니다. 
		/// : old version name -->"eSnetGetAxisIO2"
		/// </summary>
		/// <param name="net"> 				: controller id 													</param>						
		/// <param name="axis">				: axis index														</param>						
		/// <param name="limit_minus">		: (-)limit sensor의 실시간 입력 상태 		: "1"-> On, "0"-> Off, "3"-> 설정되지 않았음	</param>						
		/// <param name="limit_plus">		: (+)limit sensor의 실시간 입력 상태 		: "1"-> On, "0"-> Off, "3"-> 설정되지 않았음	</param>						
		/// <param name="sensor_origin">	: home sensor의 실시간 입력 상태 			: "1"-> On, "0"-> Off, "3"-> 설정되지 않았음	</param>						
		/// <param name="sv_alarm">			: servo alarm signal의 실시간 입력 상태 	: "1"-> On, "0"-> Off, "3"-> 설정되지 않았음	</param>						
		/// <param name="sv_ready">			: servo ready signal의 실시간 입력 상태 	: "1"-> On, "0"-> Off, "3"-> 설정되지 않았음	</param>						
		/// <param name="sv_on">			: servo on 출력 signal의 실시간 출력 상태 	: "1"-> On, "0"-> Off, "3"-> 설정되지 않았음	</param>								
		/// <returns>						: (see enum "eSnetApiReturnCode")															</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetAxisIo(int net, int axis, out int limit_minus, out int limit_plus, out int sensor_origin, out int sv_alarm, out int sv_ready, out int sv_on);

		/// <summary>
		/// 실시간 hardware 및 software limit 상태 읽기
		/// : old version name ->"eSnetGetAxisLimitStatus"
		/// </summary>
		/// <param name="net"> 		: controller id							</param>						
		/// <param name="axis">		: axis index							</param>						
		/// <param name="value">	: 실시간 hw/sw limit 상태, 
		///								(see "enum _eSnetLimitStatus")	
		///								"1"-> Input On / "0" -> Input Off
		///								bit 0: hardware (-)limit 입력 상태
		///								bit 1: hardware (+)limit 입력 상태
		///								bit 2: software (-)limit 입력 상태
		///								bit 3: software (-)limit 입력 상태	</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetLimitStatus(int net, int axis, out int value);

		/// <summary>
		/// hardware limit sensor 감지시에 servo off를 할 것인지 설정 하기
		/// (tip 1) 전원 인가시 Default 상태는 "servo on 상태 유지" 입니다.  
		/// : old version name ->"eSnetSetAxisLimitAction"
		/// </summary>
		/// <param name="net"> 		: controller id																				</param>						
		/// <param name="axis">		: axis index																				</param>						
		/// <param name="enable">	: "1"->limit Sensor 감지시 "servo off" 수행, sensor 감지 상태를 해제하고, 
		///							     eSnetReset()를 한 다음, servo on을 수행할 수 있습니다. 
		///							  "0"->Limit Sesnor 감지시 servo off를 하지 않고 멈춤, 같은 방향으로는 더 이상 진행 하지 못하고  
		///								 반대 방향으로는 움직일 수 있습니다. 
		///							  (예) (-)limit Sensor감지시 멈춤, (-)방향으로 더 이상 구동 못함, (+)방향으로 구동 가능		</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")															</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetLimitAction(int net, int axis, int enable);

		/// <summary>
		/// "eSnetSetLimitAction" 설정값 확인 
		/// : old version name ->"eSnetGetAxisLimitAction"
		/// </summary>
		/// <param name="net"> 		: controller id						</param>						
		/// <param name="axis">		: axis index						</param>						
		/// <param name="enable">	: 설정 값							</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetLimitAction(int net, int axis, out int enable);

		#endregion

		#region Basic Parameter

		/*** Set & Get Basic Parameter ***/
		/// 기본 파라메타는 제어기 전원이 꺼져도 Flash Memory에 남아 있는 Device 설정 값.

		/// <summary>
		/// 한 축당 파라메타 Number에 해당하는 값을 설정
		/// : old version name ->"eSnetWriteSystemParameter"
		/// </summary>
		/// <param name="net">		: controller id						</param>
		/// <param name="par_no">	: parameter number ("0"~"72")		</param>
		/// <param name="axis">		: parameter를 적용할 축 번호		</param>
		/// <param name="data">		: parameter 값						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetWriteParameter(int net, int par_no, int axis, int data);

		/// <summary>
		/// 파라메타 번호에 해당하는 모든 축의 값을 배열로 일괄 설정 하기
		/// /// old version name ->"eSnetWriteSystemParameters"
		/// </summary>
		/// <param name="net">		: controll id														</param>
		/// <param name="par_no">	: parameter number ("0"~"72")										</param>
		/// <param name="data">		: 축 array 배열, "0"번 축 부터 파라메타를 적용할 축 순으로 
		///								data[0] : "0"번 축 data, 
		///								data[1] : "1"번 축 data, 등등...								</param>
		/// <param name="count">	: data의 array 배열 숫자											</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")									</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetWriteParameters(int net, int par_no, out int data, int count);

        /// <summary>
        /// 한 축에 해당하는 파라메타 모두를 배열로 일괄 설정 하기
        /// : old version name ->"eSnetWriteSystemParameterByAxis"
        /// </summary>
        /// <param name="net">	: controller id													</param>
        /// <param name="axis"> : axis idnex													</param>
        /// <param name="data"> : parameter array 배열, "1"번 parameter부터 "72"번 parameter까지
        ///						  배열 크기: 73 이상 	
        ///						  (예) data[20] : "20"번 Max Speed Parameter					</param>
        /// <returns>			: (see enum "eSnetApiReturnCode")								</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetWriteParameterByAxis(int net, int axis, out int data);

        /// <summary>
        /// 파라메타 number에 해당하는 모든 축의 설정값을 배열로 읽기
        /// : old version name ->"eSnetReadSystemParameter"
        /// </summary>
        /// <param name="net">		: controller id												</param>
        /// <param name="par_no">	: parameter number ("0"~"72")								</param>
        /// <param name="data">		: 축 array 배열, 배열 크기: 32  	  
        ///							  제품에서 지원 하는 축 수에 상관없이 배열 크기를 32로 할당 
        ///							  data[0] : "0"번 축 data, 
        ///							  data[1] : "1"번 축 data, 등등...						</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")							</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetReadParameter(int net, int par_no, out int data);

		/// <summary>
		/// 한 축에 해당하는 모든 설정된 파라메타를 배열로 읽기
		/// : old version name ->"eSnetReadSystemParameterByAxis"
		/// </summary>
		/// <param name="net">	: controller id													</param>
		/// <param name="axis"> : parameter number ("0"~"72")									</param>
		/// <param name="data">	: parameter array 배열, "1"번 parameter부터 "72"번 parameter까지
		///						  배열 크기: 73 이상 
		///						  (예) data[20] : "20"번 Max Speed Parameter					</param>
		/// <returns>			: (see enum "eSnetApiReturnCode")								</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetReadParameterByAxis(int net, int axis, out int data);

        /*** Common Parameters ***/

		/// <summary>
        /// 공통 파라미터 번호에 해당하는 값 설정
		/// </summary>
		/// <param name="net">		: controller id														</param>
		/// <param name="par_no">	: common parameter number									</param>
		/// <param name="data">		: common parameter data								</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")									</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetWriteCommonParameter(int net, int par_no, int data);

		/// <summary>
        /// 공통 파라미터 값을 주어진 파라미터 넘버부터 count만큼 배열로 읽기
		/// </summary>
		/// <param name="net">		: controller id												</param>
		/// <param name="par_no">	: common parameter number									</param>
		/// <param name="data">		: common parameter data										</param>
		/// <param name="count">	: count :  파라미터 개수, 최소 1								</param>
		///							  (예) parameter_no = 0, count = 2 일 경우 
		///						  	  0번 파라미터와 1번 파라미터 반환						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")							</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetReadCommonParameter(int net, int par_no, out int data, int count);

		/*** Basic Parameters ***/

		/// <summary>
		/// 파라메타 1번 : 제어 주기 설정
		/// </summary>
		/// <param name="net">			: controller id													</param>
		/// <param name="axis">			: axis index													</param>
		/// <param name="control_hz">	: "1", SNET-P 제품은 1 KHz로 고정, 
		///									최대 1 KHz 이내에 모든 축의 지령과 Status 갱신이 이루어짐	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")								</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam001ControlHz(int net, int axis, int control_hz);

		/// <summary>
		/// 파라메타 1번 : 제어 주기 설정값 읽기
		/// </summary>
		/// <param name="net">			: controller id													</param>
		/// <param name="axis">			: axis index													</param>
		/// <param name="control_hz">	: "1", SNET 제품은 1 KHz로 고정, 
		///									최대 1 KHz 이내에 모든 축의 지령과 Status 갱신이 이루어짐	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")								</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam001ControlHz(int net, int axis, out int control_hz);

        /// <summary>
        /// 파라메타 2번 : 각 축에 연결된 모터 드라이버 타입(Pulse or RTEX)을 지정 합니다. 
		/// (tip) 적용 제품:SNET-RTEX
        /// </summary>
        /// <param name="net">				: controller id								</param>
        /// <param name="axis">				: axis index								</param>
        /// <param name="control_signal">	: "0"->rtex 드라이버, "1"->Pulse 드라이버		</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam002ControlSignal(int net, int axis, int control_signal);

        /// <summary>
        /// 파라메타 2번 : 각축 모터 드라이버 타입(Pulse or RTEX)설정값을 읽습니다. 
        /// (tip) 적용 제품:SNET-RTEX
        /// </summary>
        /// <param name="net">				: controller id								</param>
        /// <param name="axis">				: axis index								</param>
        /// <param name="control_signal">	: "0"->rtex 드라이버, "1"->Pulse 드라이버		</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam002ControlSignal(int net, int axis, out int control_signal);

		/// <summary>
		/// 파라메타 3번 : 지령 Pulse Format 설정 하기
		/// </summary>
		/// <param name="net">			: controller id						</param>
		/// <param name="axis">			: axis index						</param>
		/// <param name="pulse_format"> : "0"-> CW/CCW 방식,
		///								  "1"-> Pulse/Direction 방식, 
		///								  "2"-> AB Phase 방식,				</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam003PulseFormat(int net, int axis, int pulse_format);

		/// <summary>
		/// 파라메타 3번 : 지령 Pulse Format 설정
		/// </summary>
		/// <param name="net">			: controller id						</param>
		/// <param name="axis">			: axis index						</param>
		/// <param name="pulse_format"> : "0"-> CW/CCW 방식,
		///								  "1"-> Pulse/Direction 방식, 
		///								  "2"-> AB Phase 방식,				</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam003PulseFormat(int net, int axis, out int pulse_format);

        /// <summary>
        /// 파라메타 10번 : 모터 샤프트를 1회전 하기 위해 필요한 펄스 수 를 설정 합니다. 
        /// </summary>
        /// <param name="net">				: controller id															</param>
        /// <param name="axis">				: axis index															</param>
        /// <param name="command_rotation"> : command pulse count per motor rotation 
        ///										(tip) 일반적으로 Driver에 설정되어 있는 한 회전당 Pulse 수를 입력	</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")										</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam010CommandRotation(int net, int axis, int command_rotation);

        /// <summary>
        /// 파라메타 10번 : "모터 샤프트를 1회전 하기 위해 필요한 펄스 수(회전당 펄스 수)" 확인 
        /// </summary>
        /// <param name="net">				: controller id															</param>
        /// <param name="axis">				: axis index															</param>
        /// <param name="command_rotation"> : command pulse count per motor rotation 
        ///										(tip) 일반적으로 Driver에 설정되어 있는 한 회전당 Pulse 수를 입력	</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")										</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam010CommandRotation(int net, int axis, out int command_rotation);

		/// <summary>
		/// 파라메타 11번 : 모터 한 회전당 실제 구동 거리(um) 설정
		/// </summary>
		/// <param name="net">					: controller id						</param>
		/// <param name="axis">					: axis index						</param>
		/// <param name="distance_rotation">	: real distance per motor rotation	</param>
		/// <returns>							: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam011DistanceRotation(int net, int axis, int distance_rotation);

		/// <summary>
		/// 파라메타 11번 : 모터 한 회전당 실제 구동 거리(um) 설정값 읽기
		/// </summary>
		/// <param name="net">					: controller id						</param>
		/// <param name="axis">					: axis index						</param>
		/// <param name="distance_rotation">	: real distance per motor rotation	</param>
		/// <returns>							: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam011DistanceRotation(int net, int axis, out int distance_rotation);

		/// <summary>
		/// 파라메타 12번 : 지령 Pulse 출력 Count에 대해 Driver로 부터 제어기로 오는 Actual Pulse(Encoder) Count 비율 설정
		/// </summary>
		/// <param name="net">				: controller id																		</param>
		/// <param name="axis">				: axis index																		</param>
		/// <param name="feedback_command"> : "command pulse" per "return actual(encoder)count" 
		///										(예) command pulse가 1000일 경우, actual pulse가 250으로 return되어 온다면,
		///											Driver에서 4체배하여 Motor을 구동하는 것이므로, 이 값을 1000/250 = "4"가 됨	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")														</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam012FeedbackCommand(int net, int axis, int feedback_command);

		/// <summary>
		/// 파라메타 12번 : 지령 Pulse 출력 Count에 대해 Driver로 부터 제어기로 오는 Actual Pulse(Encoder) Count 비율 설정값 읽기
		/// </summary>
		/// <param name="net">				: controller id																		</param>
		/// <param name="axis">				: axis index																		</param>
		/// <param name="feedback_command">	: "command pulse" per "return actual(encoder)count" 
		///										(예) command pulse가 1000일 경우, actual pulse가 250으로 return되어 온다면,
		///											Driver에서 4체배하여 Motor을 구동하는 것이므로, 이 값을 1000/250 = "4"가 됨	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")													</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam012FeedbackCommand(int net, int axis, out int feedback_command);

		/// <summary>
		/// 파라메타 13번 : Motor 구동 방향 설정 하기
		/// </summary>
		/// <param name="net">				: controller id						</param>
		/// <param name="axis">				: axis index						</param>
		/// <param name="motor_direction">	: "0"-> CW방향, "1"-> CCW방향		</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam013MotorDirection(int net, int axis, int motor_direction);

		/// <summary>
		/// 파라메타 13번 : Motor 구동 방향 설정값 읽기
		/// </summary>
		/// <param name="net">				: controller id						</param>
		/// <param name="axis">				: axis index						</param>
		/// <param name="motor_direction">	: "0"-> CW방향, "1"-> CCW방향		</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam013MotorDirection(int net, int axis, out int motor_direction);

		/// <summary>
		/// 파라메타 14번 : Encoder Phase 설정 하기
		/// </summary>
		/// <param name="net">				: controller id						</param>
		/// <param name="axis">				: axis index						</param>
		/// <param name="encoder_phase">	: "0"->A_Phase, "1"->B_Phase		</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam014EncoderPhase(int net, int axis, int encoder_phase);

		/// <summary>
		/// 파라메타 14번 : Encoder Phase 설정값 읽기
		/// </summary>
		/// <param name="net">				: controller id						</param>
		/// <param name="axis">				: axis index						</param>
		/// <param name="encoder_phase">	: "0"->A_Phase, "1"->B_Phase		</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam014EncoderPhase(int net, int axis, out int encoder_phase);

		/// <summary>
		/// 파마메타 15번 : Encoder Type 설정 하기
		/// </summary>
		/// <param name="net">			: controller id							</param>
		/// <param name="axis">			: axis index							</param>
		/// <param name="encoder_type"> : "0"->Incremental Encdoer,
		///								  "1"->Absolute Encoder,				</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")		</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam015EncoderType(int net, int axis, int encoder_type);

		/// <summary>
		/// 파마메타 15번 : Encoder Type 설정값 읽기
		/// </summary>
		/// <param name="net">			: controller id							</param>
		/// <param name="axis">			: axis index							</param>
		/// <param name="encoder_type"> : "0"->Incremental Encdoer,
		///								  "1"->Absolute Encoder,				</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")		</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam015EncoderType(int net, int axis, out int encoder_type);

		/// <summary>
		/// 파라메타 20번 : 최대 구동 속도 설정 하기
		/// </summary>
		/// <param name="net">			: controller id						</param>
		/// <param name="axis">			: axis index						</param>
		/// <param name="max_speed">	: 단위[mm/min]						</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam020MaxSpeed(int net, int axis, int max_speed);

		/// <summary>
		/// 파라메타 20번 : 최대 구동 속도 설정값 읽기
		/// </summary>
		/// <param name="net">			: controller id						</param>
		/// <param name="axis">			: axis index						</param>
		/// <param name="max_speed">	: 단위[mm/min]						</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam020MaxSpeed(int net, int axis, out int max_speed);

		/// <summary>
		/// 파라메타 21번 : 구동 중 Emergence Stop이나 Reset시에 정지시 감속 시간 설정 하기
		/// </summary>
		/// <param name="net">				: controller id						</param>
		/// <param name="axis">				: axis index						</param>
		/// <param name="reset_dectime">	: 단위 [msec]						</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam021ResetDecTime(int net, int axis, int reset_dectime);

		/// <summary>
		/// 파라메타 21번 : 구동 중 Emergence Stop이나 Reset시에 정지시 감속 시간 설정 하기
		/// </summary>
		/// <param name="net">				: controller id						</param>
		/// <param name="axis">				: axis index						</param>
		/// <param name="reset_dectime">	: 단위 [msec]						</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam021ResetDecTime(int net, int axis, out int reset_dectime);

		/// <summary>
		/// 파라메타 30번 : (+)방향 Software Limit Position 설정 하기
		/// </summary>
		/// <param name="net">			: controller id						</param>
		/// <param name="axis">			: axis id							</param>
		/// <param name="plus_swlimit"> : (+)방향 Software Limit 값			</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam030PlusSwLimit(int net, int axis, int plus_swlimit);

		/// <summary>
		/// 파라메타 30번 : (+)방향 Software Limit Position 설정값 읽기
		/// </summary>
		/// <param name="net">			: controller id						</param>
		/// <param name="axis">			: axis id							</param>
		/// <param name="plus_swlimit"> : (+)방향 Software Limit 값			</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam030PlusSwLimit(int net, int axis, out int plus_swlimit);

		/// <summary>
		/// 파라메타 31번 : (-)방향 Software Limit Position 설정 하기
		/// </summary>
		/// <param name="net">				: controller id						</param>
		/// <param name="axis">				: axis id							</param>
		/// <param name="minus_swlimit">	: (-)방향 Software Limit 값			</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam031MinusSwLimit(int net, int axis, int minus_swlimit);

		/// <summary>
		/// 파라메타 31번 : (-)방향 Software Limit Position 설정 하기
		/// </summary>
		/// <param name="net">				: controller id						</param>
		/// <param name="axis">				: axis id							</param>
		/// <param name="minus_swlimit">	: (-)방향 Software Limit 값			</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam031MinusSwLimit(int net, int axis, out int minus_swlimit);

        /// <summary>
        /// 파라메타 32번 : 모터 구동시 지령 좌표와 인코더 입력으로 계산된 현재 좌표의 편차 한계치 설정 하기
        /// </summary>
        /// <param name="net">			: controller id						</param>
        /// <param name="axis">			: axis id							</param>
        /// <param name="run_error">	: 단위 [um]							</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam032RunFollowingError(int net, int axis, int run_error);

        /// <summary>
        /// 파라메타 32번 : 모터 구동시 지령 좌표와 인코더 입력으로 계산된 현재 좌표의 편차 한계치 설정 상태 읽기 
        /// </summary>
        /// <param name="net">			: controller id						</param>
        /// <param name="axis">			: axis id							</param>
        /// <param name="run_error">	: 단위 [um]							</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam032RunFollowingError(int net, int axis, out int run_error);

        /// <summary>
        /// 파라메타 33번 : 모터 정지시 지령 좌표와 인코더 입력으로 계산된 현재 좌표의 편차 한계치 설정 하기
        /// </summary>
        /// <param name="net">			: controller id						</param>
        /// <param name="axis">			: axis id							</param>
        /// <param name="idle_error">	: 단위 [um]							</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam033IdleFollowingError(int net, int axis, int idle_error);

        /// <summary>
        /// 파라메타 33번 : 모터 정지시 지령 좌표와 인코더 입력으로 계산된 현재 좌표의 편차 한계치 설정 상태 읽기 
        /// </summary>
        /// <param name="net">			: controller id						</param>
        /// <param name="axis">			: axis id							</param>
        /// <param name="idle_error">	: 단위 [um]							</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam033IdleFollowingError(int net, int axis, out int idle_error);

		/// <summary>
		/// 파라메타 40번 : Servo Ready Singal 사용 여부 설정 하기
		/// </summary>
		/// <param name="net">		: controller id						</param>
		/// <param name="axis">		: axis id							</param>
		/// <param name="sv_ready">	: "0"->사용하지 않음,
		///							  "1"->사용함,						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam040SvReady(int net, int axis, int sv_ready);

		/// <summary>
		/// 파라메타 40번 : Servo Ready Singal 사용 여부 설정값 읽기
		/// </summary>
		/// <param name="net">		: controller id						</param>
		/// <param name="axis">		: axis id							</param>
		/// <param name="sv_ready">	: "0"->사용하지 않음,	
		///							  "1"->사용함,						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam040SvReady(int net, int axis, out int sv_ready);

		/// <summary>
		/// 파라메타 41번 : ServoAlarm Singal 사용 여부 설정 하기
		/// </summary>
		/// <param name="net">		: controller id						</param>
		/// <param name="axis">		: axis id							</param>
		/// <param name="sv_alarm">	: "0"->사용하지 않음,	
		///							  "1"->사용함,						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam041SvAlarm(int net, int axis, int sv_alarm);

		/// <summary>
		/// 파라메타 41번 : ServoAlarm Singal 사용 여부 설정 하기
		/// </summary>
		/// <param name="net">		: controller id						</param>
		/// <param name="axis">		: axis id							</param>
		/// <param name="sv_alarm">	: "0"->사용하지 않음,	
		///							  "1"->사용함,						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam041SvAlarm(int net, int axis, out int sv_alarm);

		/// <summary>
		/// 파라메타 43번 : Servo_Enable Active_Level 설정 하기
		/// </summary>
		/// <param name="net">	: controller id									</param>
		/// <param name="axis"> : axis id </param>
		/// <param name="sv_on"> : "0"->사용하지 않음, 
		///						  "1"->Normal_Open (Active_High) Servo On 방식, 
		///                       "2"->Normal_Close (Active_Low) Servo On 방식,	</param>
		/// <returns>			: (see enum "eSnetApiReturnCode")				</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam043SvOn(int net, int axis, int sv_on);

		/// <summary>
		/// 파라메타 43번 : Servo_Enable Active_Level 설정값 읽기
		/// </summary>
		/// <param name="net">	: controller id									</param>
		/// <param name="axis"> : axis id </param>
		/// <param name="sv_on"> : "0"->사용하지 않음 
		///						  "1"->Normal_Open (Active_High) Servo On 방식, 
		///                       "2"->Normal_Close (Active_Low) Servo On 방식,	</param>
		/// <returns>			: (see enum "eSnetApiReturnCode")				</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam043SvOn(int net, int axis, out int sv_on);

		/// <summary>
		/// 파라메타 45번 : (-)Limit Sensor 사용 여부와 Active_Level 설정 하기
		/// </summary>
		/// <param name="net">				: controller id									</param>
		/// <param name="axis">				: axis index									</param>
		/// <param name="use_and_level">	: "0"->사용하지 않음,
		///									  "1"->Normal_Open (Active_High) Sensor Level, 
		///									  "2"->Normal_Close (Active_Low) Sensor Level,	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")				</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam045SensorLimitNegative(int net, int axis, int use_and_level);

		/// <summary>
		/// 파라메타 45번 : (-)Limit Sensor 사용 여부와 Active_Level 설정값 읽기
		/// </summary>
		/// <param name="net">				: controller id									</param>
		/// <param name="axis">				: axis index									</param>
		/// <param name="use_and_level">	: "0"->사용하지 않음,
		///									  "1"->Normal_Open (Active_High) Sensor Level, 
		///									  "2"->Normal_Close (Active_Low) Sensor Level,	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")				</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam045SensorLimitNegative(int net, int axis, out int use_and_level);

		/// <summary>
		/// 파라메타 46번 : (+)Limit Sensor 사용 여부와 Active_Level 설정 하기
		/// </summary>
		/// <param name="net">				: controller id									</param>
		/// <param name="axis">				: axis index									</param>
		/// <param name="use_and_level">	: "0"->사용하지 않음,
		///									  "1"->Normal_Open (Active_High) Sensor Level, 
		///									  "2"->Normal_Close (Active_Low) Sensor Level,	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")				</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam046SensorLimitPositive(int net, int axis, int use_and_level);

		/// <summary>
		/// 파라메타 46번 : (+)Limit Sensor 사용 여부와 Active_Level 설정값 읽기
		/// </summary>
		/// <param name="net">				: controller id									</param>
		/// <param name="axis">				: axis index									</param>
		/// <param name="use_and_level">	: "0"->사용하지 않음,
		///									  "1"->Normal_Open (Active_High) Sensor Level, 
		///									  "2"->Normal_Close (Active_Low) Sensor Level,	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")				</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam046SensorLimitPositive(int net, int axis, out int use_and_level);

		/// <summary>
		/// 파라메타 47번 : Origin(=Home) Sensor 사용 여부와 Active_Level 설정 하기
		/// </summary>
		/// <param name="net">				: controller id									</param>
		/// <param name="axis">				: axis index									</param>
		/// <param name="use_and_level">	: "0"->사용하지 않음,
		///									  "1"->Normal_Open (Active_High) Sensor Level, 
		///									  "2"->Normal_Close (Active_Low) Sensor Level,	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")				</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam047SensorOrigin(int net, int axis, int use_and_level);

		/// <summary>
		/// 파라메타 47번 : Origin(=Home) Sensor 사용 여부와 Active_Level 설정값 읽기
		/// </summary>
		/// <param name="net">				: controller id									</param>
		/// <param name="axis">				: axis index									</param>
		/// <param name="use_and_level">	: "0"->사용하지 않음,
		///									  "1"->Normal_Open (Active_High) Sensor Level, 
		///									  "2"->Normal_Close (Active_Low) Sensor Level,	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")				</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam047SensorOrigin(int net, int axis, out int use_and_level);

		/// <summary>
		/// 파라메타 70번 : Inposition Pulse Margin(command & actual point error) 설정 하기
		/// </summary>
		/// <param name="net">				: controller id											</param>
		/// <param name="axis">				: axis index											</param>
		/// <param name="inposition_pulse">	: 단위 [pulse], 							
		/// <returns>						: (see enum "eSnetApiReturnCode")						</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam070InPositionPulse(int net, int axis, int inposition_pulse);

		/// <summary>
		/// 파라메타 70번 : Inposition Pulse Margin(command & actual point error) 설정값 읽기
		/// </summary>
		/// <param name="net">				: controller id											</param>
		/// <param name="axis">				: axis index											</param>
		/// <param name="inposition_pulse">	: 단위 [pulse], 							
		/// <returns>						: (see enum "eSnetApiReturnCode")						</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam070InPositionPulse(int net, int axis, out int inposition_pulse);

		/// <summary>
		/// 파라메타 71번 : Inposition Monitoring 시간 폭 설정 하기
		/// </summary>
		/// <param name="net">				: controller id										</param>
		/// <param name="axis">				: axis id											</param>
		/// <param name="inposition_time">	: inposition을 monitoring 할 시간 범위, 단위 [msec]	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")					</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam071InPositionTime(int net, int axis, int inposition_time);

		/// <summary>
		/// 파라메타 71번 : Inposition Monitoring 시간 폭 설정값 읽기
		/// </summary>
		/// <param name="net">				: controller id										</param>
		/// <param name="axis">				: axis id											</param>
		/// <param name="inposition_time">	: inposition을 monitoring 할 시간 범위, 단위 [msec]	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")					</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam071InPositionTime(int net, int axis, out int inposition_time);

		/// <summary>
		/// 파라메타 72번 : Inposition 최대 Monitoring time out 시간 설정 하기
		/// : timeout 시간이 지나면, Inposition이 되지 않아도, inposition monitoring을 끝내고, motion done으로 함
		/// </summary>
		/// <param name="net">					: controller id						</param>
		/// <param name="axis">					: axis id							</param>
		/// <param name="inposition_timeout">	: 단위 [msec]						</param>
		/// <returns>							: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetParam072InPositionTimeOut(int net, int axis, int inposition_timeout);

		/// <summary>
		/// 파라메타 72번 : Inposition 최대 Monitoring time out 시간 설정값 읽기
		/// : timeout 시간이 지나면, Inposition이 되지 않아도, inposition monitoring을 끝내고, motion done으로 함
		/// </summary>
		/// <param name="net">					: controller id						</param>
		/// <param name="axis">					: axis id							</param>
		/// <param name="inposition_timeout">	: 단위 [msec]						</param>
		/// <returns>							: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetParam072InPositionTimeOut(int net, int axis, out int inposition_timeout);

		/// <summary>
		/// 파라메타 쓰기를 했던 내용을 물리적으로 Flash에 Writing 시키는 함수
		/// </summary>
		/// <param name="net">	: controller id</param>
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetFlushFlashMemory(int net);

		/// <summary>
		/// Flash에 갱신 되었던 내용을 Controller를 Reset시키고 RAM에 반영시키는 함수
		/// </summary>
		/// <param name="net">	: controller id						</param>
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRestartController(int net);

		#endregion

		#region Software Limit

		/*** Virtual Limit Software ***/

		/// <summary>
		/// software (-)limit 설정 하기
		/// (tip 1) 제어기 전원이 꺼지면, 이 함수로 설정된 설정값은 사라진다(Flash 메모리에 저장 되지 않고, RAM 변수 변경)
		/// : old version name -->"eSnetSetAxisNegativeSoftwareLimit","eSnetSetAxisPositiveSoftwareLimit"
		/// </summary>
		/// <param name="net"> 				: controller id						</param>						
		/// <param name="axis">				: axis index						</param>						
		/// <param name="negative_limit">	: negative limit position			</param>
		/// <param name="positive_limit">	: positive limit position			</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>		
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetSoftwarePositionLimit(int net, int axis, int negative_limit, int positive_limit);

		/// <summary>
		/// software (-)limit 설정값 읽기
		/// : old version name -->"eSnetGetAxisNegativeSoftwareLimit","eSnetGetAxisPositiveSoftwareLimit"
		/// </summary>
		/// <param name="net"> 				: controller id						</param>						
		/// <param name="axis">				: axis index						</param>						
		/// <param name="negative_limit">	: negative limit position			</param>
		/// <param name="positive_limit">	: positive limit position			</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>		
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetSoftwarePositionLimit(int net, int axis, out int negative_limit, out int positive_limit);

		#endregion

		#region Motor Direction

		/*** Motor Direction ***/

		/// <summary>
		/// 축별 Motor 회전 방향 변경 
		/// (tip 1) 본 함수의 설정값은 제어기 전원 재 인가시 사라집니다(휘발성),항시 적용 하려면 "HMI"를 이용하여    
		///		  "basic parameter의 P13"을 변경하고 제어기에 저장하여(비휘발성) 사용하십시요 
		/// : old version name ->"eSnetSetAxisMotorDirection"
		/// </summary>
		/// <param name="net"> 			: controller id						</param>						
		/// <param name="axis">			: axis index						</param>						
		/// <param name="direction">	: "1"->CW, "-1"->CCW  				</param>	
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>		
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetMotorDirection(int net, int axis, int direction);

		/// <summary>
		/// 축별 Motor 회전 방향 설정값 확인 
		/// : old version name ->"eSnetGetAxisMotorDirection"
		/// </summary>
		/// <param name="net"> 			: controller id						</param>						
		/// <param name="axis">			: axis index						</param>						
		/// <param name="direction">	: "1"->CW, "-1"->CCW  				</param>	
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>		
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetMotorDirection(int net, int axis, out int direction);

		#endregion

		#region Stop Setting

		/*** Stop Setting ***/

		/// <summary>
		/// "기본 파라미터 P21(Reset Dec.Time)" 변경
		/// (tip 1) "eSnetReset 또는 eSnetEmergencyStop"함수 실행시 "감속비" 계산에 사용 됩니다.  
		/// (tip 2) 본 함수의 설정값은 제어기 전원 재 인가시 사라집니다(휘발성),항시 적용 하려면 "HMI"를 
		///			이용하여 "basic parameter의 P13"을 변경하고 제어기에 저장하여(비휘발성) 사용하십시요    
		/// </summary>
		/// <param name="net"> 	: controller id						</param>						
		/// <param name="axis">	: axis index						</param>	
		/// <param name="data">	: 감속 시간 (msec)					</param>	
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetStopDcc(int net, int axis, int data);

		/// <summary>
		/// "기본 파라미터 P21(Reset Dec.Time)"설정값 확인 
		/// </summary>
		/// <param name="net"> 	: controller id						</param>						
		/// <param name="axis">	: axis index						</param>	
		/// <param name="data">	: 감속 시간 (msec)					</param>	
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetStopDcc(int net, int axis, out int data);

		#endregion

		#region Following Error

		/*** Following Error ***/
		/// # Run Following Error   : During drive, the difference between the command position and 
		///							 the actual position is outside the set value 
		/// # Sleep Following Error : During steady state, the difference between the command position and 
		///							 the actual position is outside the set value 

		/// <summary>
		/// "Run following error"기준 거리 설정 
		/// (tip 1)"Basic parameter P32(1th Following Error)"설정값을 변경 합니다. 
		/// (tip 2)설정값이 "0"이면 "Run Following Error"검출 기능이 비활성화 됩니다. 
		/// (tip 3)본 설정값은 제어기 전원 재 인가시 사라집니다(휘발성),영구적으로 적용하기 위해서는 "HMI"을 사용하여
		///        "Basic parameter P32(1th Following Error)"를 변경하고 제어기에 저장하여 사용 하십시요 
		/// : old version name ->"eSnetSetAxisRunFollowingError"
		/// </summary>
		/// <param name="net"> 				: controller id						</param>						
		/// <param name="axis">				: axis index						</param>						
		/// <param name="position_error">	: [um],지령값과 실제 위치값의 차이	</param>	
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>		
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetRunFollowingError(int net, int axis, int position_error);

		/// <summary>
		/// "Run following error" 설정값 읽기
		/// : old version name ->"eSnetGetAxisRunFollowingError"
		/// </summary>
		/// <param name="net"> 				: controller id						</param>						
		/// <param name="axis">				: axis index						</param>						
		/// <param name="position_error">	: [um] 지령값과 실제 위치값의 차이	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>		
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetRunFollowingError(int net, int axis, out int position_error);

		/// <summary>
		/// "Sleep(Stop) following error"기준 거리 설정
		/// (tip 1)"Basic parameter P33(2nd Following Error)"설정값을 변경 합니다. 
		/// (tip 2)설정값이 "0"이면 "Sleep(Stop) Following Error"검출 기능이 비활성화 됩니다. 
		/// (tip 3)본 설정값은 제어기 전원 재 인가시 사라집니다(휘발성),영구적으로 적용하기 위해서는 "HMI"을 사용하여
		///        "Basic parameter P33(2nd Following Error)"을 변경하고 제어기에 저장하여 사용 하십시요 
		/// : old version name ->"eSnetSetAxisSleepFollowingError"
		/// </summary>
		/// <param name="net"> 				: controller id						</param>						
		/// <param name="axis">				: axis index						</param>						
		/// <param name="position_error">	: [um] 지령값과 위치값의 차이		</param>	
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>		
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetSleepFollowingError(int net, int axis, int position_error);

		/// <summary>
		/// "Sleep(Stop) following error" 설정값 읽기
		/// : old version name ->"eSnetGetAxisSleepFollowingError"
		/// </summary>
		/// <param name="net"> 				: controller id						</param>						
		/// <param name="axis">				: axis index						</param>						
		/// <param name="position_error">	: [um] 지령값과 위치값의 차이		</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>		
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetSleepFollowingError(int net, int axis, out int position_error);

		#endregion

		#region Inposition

		/*** Inpostion값 설정 ***/

		/// <summary>
		/// "Inposiotn 완료"기준 펄스수 설정 
		/// (tip 1)"Basic parameter P70(In-posiotn Pulse)"설정값을 변경 합니다. 
		/// (tip 2)설정값이 "0"이면 "Inposition"신호 출력 기능이 비활성화(항시 ON) 됩니다.  
		/// (tip 3)본 설정값은 제어기 전원 재 인가시 사라집니다(휘발성),영구적으로 적용하기 위해서는 "HMI"을 사용하여
		///        "Basic parameter P70(In-position Pulse)"을 변경하고 제어기에 저장하여 사용 하십시요 
		/// : old version name ->"eSnetSetAxisInpositionPulseRange"
		/// </summary>
		/// <param name="net"> 	: controller id										</param>						
		/// <param name="axis">	: axis index										</param>						
		/// <param name="data">	: [Pulse],"Inposition 완료"기준 엔코더 입력 펄스값 	</param>	
		/// <returns>			: (see enum "eSnetApiReturnCode")					</returns>		
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetInpositionPulseRange(int net, int axis, int data);

		/// <summary>
		/// "Inposiotn 완료"기준 펄스수 설정값 확인  
		/// : old version name ->"eSnetGetAxisInpositionPulseRange"
		/// </summary>
		/// <param name="net"> 	: controller id										</param>						
		/// <param name="axis">	: axis index										</param>						
		/// <param name="data">	: [Pulse],"Inposition 완료"기준 엔코더 입력 펄스값 	</param>	
		/// <returns>			: (see enum "eSnetApiReturnCode")					</returns>		
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetInpositionPulseRange(int net, int axis, out int data);

		/// <summary>
		/// "Inposition Pulse"유지 시간 설정 
		/// (tip 1)"Basic parameter P71(In-posiotn Time)"설정값을 변경 합니다. 
		/// (tip 2)본 설정값은 제어기 전원 재 인가시 사라집니다(휘발성),영구적으로 적용하기 위해서는 "HMI"을 사용하여
		///        "Basic parameter P71(In-position Time)"을 변경하고 제어기에 저장하여 사용 하십시요 
		/// : old version name ->"eSnetSetAxisInpositionDurationTime"
		/// </summary>
		/// <param name="net"> 	: controller id																		</param>						
		/// <param name="axis">	: axis index																		</param>						
		/// <param name="data">	: [msec],"Inposition Pulse" 유지 시간,축 이송 완료 후 엔코더를 통해 입력되는 펄스
		///							갯수가 해당 시간 동안 "Inposition Pulse" 이하로 유지되면 Inposition 신호 ON		</param>	
		/// <returns>			: (see enum "eSnetApiReturnCode")													</returns>		
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetInpositionDuration(int net, int axis, int data);

		/// <summary>
		/// "Inposition Pulse"유지 시간 확인 
		/// : old version name ->"eSnetGetAxisInpositionDurationTime"
		/// </summary>
		/// <param name="net"> 	: controller id							</param>						
		/// <param name="axis">	: axis index							</param>						
		/// <param name="data">	: [msec],"Inposition Pulse" 유지 시간	</param>	
		/// <returns>			: (see enum "eSnetApiReturnCode")		</returns>		
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetInpositionDuration(int net, int axis, out int data);

		/// <summary>
		/// "Inposition Check Time out" 설정 
		/// (tip 1)"Basic parameter P72(In-posiotn Escape Time)"설정값을 변경 합니다. 
		/// (tip 2)본 설정값은 제어기 전원 재 인가시 사라집니다(휘발성),영구적으로 적용하기 위해서는 "HMI"을 사용하여
		///        "Basic parameter P72(In-position Escape Time)"를 변경하고 제어기에 저장하여 사용 하십시요 
		/// : old version name ->"eSnetSetAxisInpositionTimeOut"
		/// </summary>
		/// <param name="net"> 	: controller id																			</param>						
		/// <param name="axis">	: axis index																			</param>						
		/// <param name="data">	: [msec],"Inposition Check Time out"설정 값,모션 완료 후 해당 시간 동안 "Inposition 완료"
		///							가 않되면 "Inposition 완료"신호를 출력 하고 "out_inpos"신호가 on 됩니다.			</param>	
		/// <returns>			: (see enum "eSnetApiReturnCode")														</returns>		
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetInpositionTimeout(int net, int axis, int data);

		/// <summary>
		/// "Inposition Check Time out" 설정값 확인 
		/// : old version name ->"eSnetGetAxisInpositionTimeOut"
		/// </summary>
		/// <param name="net"> 	: controller id									</param>						
		/// <param name="axis">	: axis index									</param>						
		/// <param name="data">	: [msec],"Inposition Check Time out"설정 값"	</param>	
		/// <returns>			: (see enum "eSnetApiReturnCode")				</returns>		
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetInpositionTimeout(int net, int axis, out int data);

		#endregion

		#region I/O MAP

		/// <summary>
		/// I/O MAP 32bit data 읽기
		/// </summary>
		/// <param name="net">		: Network id(IPv4 4th address)	</param>
		/// <param name="address">	: 읽을 data 주소</param>
		/// <param name="count">	: 읽을 data 수</param>
		/// <param name="data">		: 읽을 data</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetIoMap(int net, int address, int count, out int data);

		/// <summary>
		/// I/O MAP 32bit data 쓰기
		/// </summary>
		/// <param name="net">		: Network id(IPv4 4th address)	</param>
		/// <param name="address">	: data를 쓸 주소</param>
		/// <param name="count">	: 쓸 data 수</param>
		/// <param name="data">		: 쓸 data</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetIoMap(int net, int address, int count, out int data);

		/// <summary>
		/// I/O MAP 32bit 주소 내 bit 값 읽기
		/// </summary>
		/// <param name="net">		: Network id(IPv4 4th address)	</param>
		/// <param name="address">	: bit 값을 읽을 주소				</param>
		/// <param name="bit">		: bit 값을 읽을 bit 번호			</param>
		/// <param name="on">		: bit 값. 1: set, 0: clear		</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetIoMapOn(int net, int address, int bit, out int on);

		/// <summary>
		/// I/O MAP 32bit 주소 내 bit 값 쓰기
		/// </summary>
		/// <param name="net">		: Network id(IPv4 4th address)	</param>
		/// <param name="address">	: bit 값을 쓸 주소				</param>
		/// <param name="bit">		: bit 값을 쓸 bit 번호			</param>
		/// <param name="on">		: bit 값. 1: set, 0: clear		</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetIoMapOn(int net, int address, int bit, int on);


		/// <summary>
		/// I/O Map 의 특정 bit 번호부터 복수의 bit 값 읽기
		/// </summary>
		/// <param name="net">			: Network id(IPv4 4th address)	</param>
		/// <param name="start_bit">	: 복수의 bit 값을 읽을 시작 bit 번호</param>
		/// <param name="count">		: 읽을 bit 개수</param>
		/// <param name="data">			: 읽어온 복수의 bit 값이 저장될 16bit 버퍼</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetIoMapBit(int net, int start_bit, int count, out ushort data);

		/// <summary>
		/// I/O Map 의 특정 bit 번호부터 복수의 bitbit 값 쓰기
		/// </summary>
		/// <param name="net">			: Network id(IPv4 4th address)	</param>
		/// <param name="start_bit">	: 복수의 bit 값을 쓸 시작 bit 번호</param>
		/// <param name="count">		: 쓸 bit 개수</param>
		/// <param name="data">			: 쓸 복수의 bit 값이 저장된 16bit 버퍼</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetIoMapBit(int net, int start_bit, int count, out ushort data);

		/// <summary>
		/// I/O Map 의 특정 bit 번호의 값 읽기
		/// </summary>
		/// <param name="net">			: Network id(IPv4 4th address)	</param>
		/// <param name="bit">			: 읽을 bit 번호</param>
		/// <param name="on">			: 읽어온 bit 값. 1: set, 0: cleared</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetIoMapBitOn(int net, int bit, out int on);

		/// <summary>
		/// I/O Map 의 특정 bit 번호의 값 쓰기
		/// </summary>
		/// <param name="net">			: Network id(IPv4 4th address)	</param>
		/// <param name="bit">			: 쓸 bit 번호</param>
		/// <param name="on">			: 쓸 bit 값. 1: set, 0: clear</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetIoMapBitOn(int net, int bit, int on);

		#endregion

		#region I/O (H/W Address Mapping)

		/// <summary>
		/// 하드웨어적으로 Mapping된 I/O Address에 따른 모든 Input Port 읽기
		/// </summary>
		/// <param name="net"> 		: controller id														</param>						
		/// <param name="ports">	: [array], "배열 크기 -> 4", "1" ->입력 on, "0" -> 입력 off 
		///
		///							[  SNET-P  ]
		///							ports[0] : Axis3 ~ Axis0 / ports[1] : Axis7 ~ Axis4
		///
		///							bit31 : User					bit15 : NC
		///							bit30 : Home	(Axis3/7)		bit14 : NC
		///							bit29 : +Limit	(Axis3/7)		bit13 : NC 
		///							bit28 : -Limit	(Axis3/7)		bit12 : NC 
		///							bit27 : User					bit11 : User  	
		///							bit26 : Home	(Axis2/6)		bit10 : Alarm	(Axis3/7)
		///							bit25 : +Limit	(Axis2/6)	    bit9  : Ready	(Axis3/7) 	
		///							bit24 : -Limit	(Axis2/6)		bit8  : User 
		///							bit23 : User					bit7  : Alarm	(Axis2/6)	
		///							bit22 : Home	(Axis1/5)	 	bit6  : Ready	(Axis2/6)	
		///							bit21 : +Limit	(Axis1/5)		bit5  : User 	
		///							bit20 : -Limit	(Axis1/5)		bit4  : Alarm	(Axis1/5)	
		///							bit19 : User					bit3  : Ready	(Axis1/5)	
		///							bit18 : Home	(Axis0/4)	 	bit2  : User 0
		///							bit17 : +Limit	(Axis0/4)	    bit1  : Alarm	(Axis0/4)	
		///							bit16 : -Limit	(Axis0/4)		bit0  : Ready	(Axis0/4) 	
		/// 
		///							[  SNET-RTEX  ]
		///							ports[0] : Axis7 ~ Axis0	/ ports[1] : Axis15 ~ Axis8 
		///							ports[2] : Axis23 ~ Axis16	/ ports[3] : Axis31 ~ Axis24 	
		/// 
		///							bit31 : User	(Axis7/15/23/31)	bit15 : User	(Axis3/11/19/27)
		///							bit30 : Home	(Axis7/15/23/31)	bit14 : Home	(Axis3/11/19/27)
		///							bit29 : +Limit	(Axis7/15/23/31)	bit13 : +Limit	(Axis3/11/19/27)
		///							bit28 : -Limit	(Axis7/15/23/31)	bit12 : -Limit	(Axis3/11/19/27)
		///							bit27 : User	(Axis6/14/22/30)	bit11 : User  	(Axis2/10/18/26)
		///							bit26 : Home	(Axis6/14/22/30)	bit10 : Home	(Axis2/10/18/26)
		///							bit25 : +Limit	(Axis6/14/22/30)    bit9  : +Limit	(Axis2/10/18/26)
		///							bit24 : -Limit	(Axis6/14/22/30)	bit8  : -Limit	(Axis2/10/18/26)
		///							bit23 : User	(Axis5/13/21/29)	bit7  : User    (Axis1/9/17/25) 
		///							bit22 : Home	(Axis5/13/21/29)	bit6  : Home	(Axis1/9/17/25) 
		///							bit21 : +Limit	(Axis5/13/21/29)	bit5  : +Limit	(Axis1/9/17/25) 
		///							bit20 : -Limit	(Axis5/13/21/29)	bit4  : -Limit	(Axis1/9/17/25) 
		///							bit19 : User	(Axis4/12/20/28)	bit3  : User	(Axis0/8/16/24) 
		///							bit18 : Home	(Axis4/12/20/28)	bit2  : Home	(Axis0/8/16/24) 
		///							bit17 : +Limit	(Axis4/12/20/28)    bit1  : +Limit	(Axis0/8/16/24) 
		///							bit16 : -Limit	(Axis4/12/20/28)	bit0  : -Limit	(Axis0/8/16/24) 
		/// 
		///							[ SNET-EMD-L2K ]
		///                         ports[0] : 상위 워드 -> X1.F ~ X1.0 /하위 워드 -> X0.F ~ X0.0 
		///
		///							bit31 : NC						bit15 : User	(X0.F)
		///							bit30 : NC 						bit14 : User	(X0.E)
		///							bit29 : NC						bit13 : User	(X0.D)
		///							bit28 : NC 						bit12 : User	(X0.C)
		///							bit27 : User		   (X1.B)	bit11 : User	(X0.B)
		///							bit26 : Alarm3	(Axis3)(X1.A)	bit10 : User	(X0.A)
		///							bit25 : Ready	(Axis3)(X1.9)	bit9  : User	(X0.9)	
		///							bit24 : User		   (X1.8)	bit8  : User	(X0.8)	
		///							bit23 : Alarm	(Axis2)(X1.7)	bit7  : User	(X0.7)	
		///							bit22 : Ready	(Axis2)(X1.6)	bit6  : User	(X0.6)	
		///							bit21 : User		   (X1.5)	bit5  : User	(X0.5)	
		///							bit20 : Alarm	(Axis1)(X1.4)	bit4  : User	(X0.4)	
		///							bit19 : Ready	(Axis1)(X1.3)	bit3  : User	(X0.3)	
		///							bit18 : User		   (X1.2)	bit2  : User	(X0.2)	
		///							bit17 : Alarm	(Axis0)(X1.1)	bit1  : User	(X0.1)	
		///							bit16 : Ready	(Axis0)(X1.0)	bit0  : User	(X0.0)	
		///							 
		///							ports[1] : 상위 워드 -> X3.F ~ X3.0 /하위 워드 -> X2.F ~ X2.0 
		///
		///							bit31 : NC					bit15 : NC
		///							bit30 : NC					bit14 : NC
		///							bit29 : NC					bit13 :	NC
		///							bit28 : NC					bit12 : NC
		///							bit27 : NC					bit11 : Home	(Axis3)(X2.B)
		///							bit26 : NC					bit10 : -Limit	(Axis3)(X2.A)
		///							bit25 : NC					bit9  : +Limit	(Axis3)(X2.9)
		///							bit24 : NC					bit8  : Home	(Axis2)(X2.8)
		///							bit23 : User	(X3.7)		bit7  : -Limit	(Axis2)(X2.7)
		///							bit22 : User	(X3.6)		bit6  : +Limit	(Axis2)(X2.6)
		///							bit21 : User	(X3.5)		bit5  : Home	(Axis1)(X2.5)
		///							bit20 : User	(X3.4)		bit4  : -Limit	(Axis1)(X2.4)
		///							bit19 : User	(X3.3)		bit3  :	+Limit	(Axis1)(X2.3)
		///							bit18 : User	(X3.2)		bit2  : Home	(Axis0)(X2.2)
		///							bit17 : User	(X3.1)		bit1  : -Limit	(Axis0)(X2.1)
		///							bit16 : User	(X3.0)		bit0  : +Limit	(Axis0)(X2.0)
		///							   
		///							ports[2] : 상위 워드 -> X5.F ~ X5.0 /하위 워드 -> X4.F ~ X4.0 
		///
		///							bit31 : NC						bit15 : NC
		///							bit30 : NC						bit14 : NC
		///							bit29 : NC						bit13 :	NC
		///							bit28 : NC						bit12 : NC
		///							bit27 : Home	(Axis7)(X5.B)	bit11 : User		   (X4.B)
		///							bit26 : -Limit  (Axis7)(X5.A)	bit10 : Alarm	(Axis7)(X4.A)
		///							bit25 : +Limit	(Axis7)(X5.9)	bit9  : Ready	(Axis7)(X4.9)
		///							bit24 : Home	(Axis6)(X5.8)	bit8  : User		   (X4.8)
		///							bit23 : -Limit	(Axis6)(X5.7)	bit7  : Alarm	(Axis6)(X4.7)
		///							bit22 : +Limit	(Axis6)(X5.6)	bit6  : Ready	(Axis6)(X4.6)
		///							bit21 : Home	(Axis5)(X5.5)	bit5  : User		   (X4.5)
		///							bit20 : -Limit	(Axis5)(X5.4)	bit4  : Alarm	(Axis5)(X4.4)
		///							bit19 : +Limit	(Axis5)(X5.3)	bit3  :	Ready	(Axis5)(X4.3)
		///							bit18 : Home	(Axis4)(X5.2)	bit2  : User		   (X4.2)
		///							bit17 : -Limit	(Axis4)(X5.1)	bit1  : Alarm	(Axis4)(X4.1)
		///							bit16 : +Limit	(Axis4)(X5.0)	bit0  : Ready	(Axis4)(X4.0)	</param>
		///
		/// <returns>				: (see enum "eSnetApiReturnCode")					</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetMcbUserInput(int net, out int ports);

		/// <summary>
		/// 하드웨어적으로 Mapping된 I/O Address에 따른 모든 Output Port 읽기
		/// </summary>
		/// <param name="net"> 		: controller id													</param>						
		/// <param name="ports">	: hardware i/o address port
		///							 [array], "배열 크기 -> 2", "1" ->출력 on, "0" -> 출력 off 
		///							
		///							[  SNET-P  ]
		///							ports[0]    
		///
		///							bit31 : NC			bit15 : Alarm Clear	(Axis7)
		///							bit30 : NC			bit14 : Servo On	(Axis7)
		///							bit29 : NC			bit13 : Alarm Clear	(Axis6)
		///							bit28 : NC			bit12 : Servo On	(Axis6)
		///							bit27 : NC			bit11 : Alarm Clear	(Axis5)
		///							bit26 : NC			bit10 : Servo On	(Axis5)
		///							bit25 : NC			bit9  : Alarm Clear	(Axis4)
		///							bit24 : NC			bit8  : Servo On	(Axis4)
		///							bit23 : NC			bit7  : Alarm Clear	(Axis3)
		///							bit22 : NC			bit6  : Servo On	(Axis3)
		///							bit21 : NC			bit5  : Alarm Clear	(Axis2)
		///							bit20 : NC			bit4  : Servo On	(Axis2)
		///							bit19 : User		bit3  : Alarm Clear	(Axis1) 
		///							bit18 : User		bit2  : Servo On	(Axis1)
		///							bit17 : User		bit1  : Alarm Clear	(Axis0) 
		///							bit16 : User		bit0  : Servo On	(Axis0) 
		///							
		///							[  SNET-RTEX  ]
		///							ports[0] : Axis15 ~ Axis0 /	ports[1] : Axis31 ~ Axis16
		///
		///							bit31 : EX-OUT2	(Axis15/31)		bit15 : EX-OUT2	(Axis7/23)
		///							bit30 : EX-OUT1	(Axis15/31)		bit14 : EX-OUT1	(Axis7/23)
		///							bit29 : EX-OUT2	(Axis14/30)		bit13 : EX-OUT2	(Axis6/22)
		///							bit28 : EX-OUT1	(Axis14/30)		bit12 : EX-OUT1	(Axis6/22)
		///							bit27 : EX-OUT2	(Axis13/29)		bit11 : EX-OUT2	(Axis5/21)
		///							bit26 : EX-OUT1	(Axis13/29)		bit10 : EX-OUT1	(Axis5/21)
		///							bit25 : EX-OUT2	(Axis12/28)		bit9  : EX-OUT2	(Axis4/20)
		///							bit24 : EX-OUT1	(Axis12/28)		bit8  : EX-OUT1	(Axis4/20)
		///							bit23 : EX-OUT2	(Axis11/27)		bit7  : EX-OUT2	(Axis3/19)
		///							bit22 : EX-OUT1	(Axis11/27)		bit6  : EX-OUT1	(Axis3/19)
		///							bit21 : EX-OUT2	(Axis10/26)		bit5  : EX-OUT2	(Axis2/18)
		///							bit20 : EX-OUT1	(Axis10/26)		bit4  : EX-OUT1	(Axis2/18)
		///							bit19 : EX-OUT2	(Axis9/25)		bit3  :	EX-OUT2	(Axis1/17)
		///							bit18 : EX-OUT1	(Axis9/25)		bit2  : EX-OUT1	(Axis1/17)
		///							bit17 : EX-OUT2	(Axis8/24)		bit1  : EX-OUT2	(Axis0/16)
		///							bit16 : EX-OUT1	(Axis8/24)		bit0  : EX-OUT1	(Axis0/16)
		/// 
		///							[ SNET-EMD-L2K ]
		///							ports[0] : 상위 워드-> Y1.F ~ Y1.0, /하위 워드-> Y0.F ~ Y0.0 
		///
		///							bit31 : NC							bit15 : User	(Y0.F)
		///							bit30 : NC							bit14 : User	(Y0.E)
		///							bit29 : NC							bit13 : User	(Y0.D)
		///							bit28 : NC							bit12 : User	(Y0.C)
		///							bit27 : User			   (Y1.B)	bit11 : User	(Y0.B)
		///							bit26 : Alarm Clear (Axis3)(Y1.A)	bit10 : User	(Y0.A)
		///							bit25 : Servo On	(Axis3)(Y1.9)	bit9  : User	(Y0.9)
		///							bit24 : User			   (Y1.8)	bit8  : User	(Y0.8)
		///							bit23 : Alarm Clear (Axis2)(Y1.7)	bit7  : User	(Y0.7)
		///							bit22 : Servo On	(Axis2)(Y1.6)   bit6  : User	(Y0.6)
		///							bit21 : User			   (Y1.5)   bit5  : User	(Y0.5)
		///							bit20 : Alarm Clear	(Axis1)(Y1.4)   bit4  : User	(Y0.4)
		///							bit19 : Servo On	(Axis1)(Y1.3)	bit3  : User	(Y0.3)
		///							bit18 : User			   (Y1.2) 	bit2  : User	(Y0.2)
		///							bit17 : Alarm Clear	(Axis0)(Y1.1)	bit1  : User	(Y0.1)
		///							bit16 : Servo On	(Axis0)(Y1.0)	bit0  : User	(Y0.0)
		/// 
		///							ports[1] : 상위 워드-> Y3.F ~ Y3.0, /하위 워드-> Y2.F ~ Y2.0 
		///
		///							bit31 : NC							bit15 : User	(Y2.F)
		///							bit30 : NC							bit14 : User	(Y2.E)
		///							bit29 : NC							bit13 : User	(Y2.D)
		///							bit28 : NC							bit12 : User	(Y2.C)
		///							bit27 : User			   (Y3.B)	bit11 : NC		(Y2.B)
		///							bit26 : Alarm Clear (Axis7)(Y3.A)	bit10 : User	(Y2.A)
		///							bit25 : Servo On	(Axis7)(Y3.9)	bit9  : User	(Y2.9)
		///							bit24 : User			   (Y3.8)	bit8  : User	(Y2.8)
		///							bit23 : Alarm Clear (Axis6)(Y3.7)	bit7  : NC		(Y2.7)
		///							bit22 : Servo On	(Axis6)(Y3.6)   bit6  : NC		(Y2.6)
		///							bit21 : User			   (Y3.5)   bit5  : NC		(Y2.5)
		///							bit20 : Alarm Clear	(Axis5)(Y3.4)   bit4  : User	(Y2.4)
		///							bit19 : Servo On	(Axis5)(Y3.3)	bit3  : User	(Y2.3)
		///							bit18 : User			   (Y3.2) 	bit2  : User	(Y2.2)
		///							bit17 : Alarm Clear	(Axis4)(Y3.1)	bit1  : User	(Y2.1)
		///							bit16 : Servo On	(Axis4)(Y3.0)	bit0  : User	(Y2.0)	</param>
		///																						
		/// <returns>				: (see enum "eSnetApiReturnCode")							</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetMcbUserOutput(int net, out int ports);

		/// <summary>
		/// 하드웨어적으로 Mapping된 I/O Address에 다른 임의의 한 Point 읽기
		/// </summary>
		/// <param name="net"> 		: controller id												</param>						
		/// <param name="port">		: hardware i/o address port									</param>
		///							 "0" or "1"
		///
		///							[  SNET-P  ]
		///							"port = 0" 
		///
		///							bit31 : NC			bit15 : Alarm Clear	(Axis7)
		///							bit30 : NC			bit14 : Servo On	(Axis7)
		///							bit29 : NC			bit13 : Alarm Clear	(Axis6)
		///							bit28 : NC			bit12 : Servo On	(Axis6)
		///							bit27 : NC			bit11 : Alarm Clear	(Axis5)
		///							bit26 : NC			bit10 : Servo On	(Axis5)
		///							bit25 : NC			bit9  : Alarm Clear	(Axis4)
		///							bit24 : NC			bit8  : Servo On	(Axis4)
		///							bit23 : NC			bit7  : Alarm Clear	(Axis3)
		///							bit22 : NC			bit6  : Servo On	(Axis3)
		///							bit21 : NC			bit5  : Alarm Clear	(Axis2)
		///							bit20 : NC			bit4  : Servo On	(Axis2)
		///							bit19 : User		bit3  : Alarm Clear	(Axis1) 
		///							bit18 : User		bit2  : Servo On	(Axis1)
		///							bit17 : User		bit1  : Alarm Clear	(Axis0) 
		///							bit16 : User		bit0  : Servo On	(Axis0) 
		///							
		///							[  SNET-RTEX  ]
		///							"port = 0" : Axis15 ~ Axis0 /	"port = 1" : Axis31 ~ Axis16
		///
		///							bit31 : EX-OUT2	(Axis15/31)		bit15 : EX-OUT2	(Axis7/23)
		///							bit30 : EX-OUT1	(Axis15/31)		bit14 : EX-OUT1	(Axis7/23)
		///							bit29 : EX-OUT2	(Axis14/30)		bit13 : EX-OUT2	(Axis6/22)
		///							bit28 : EX-OUT1	(Axis14/30)		bit12 : EX-OUT1	(Axis6/22)
		///							bit27 : EX-OUT2	(Axis13/29)		bit11 : EX-OUT2	(Axis5/21)
		///							bit26 : EX-OUT1	(Axis13/29)		bit10 : EX-OUT1	(Axis5/21)
		///							bit25 : EX-OUT2	(Axis12/28)		bit9  : EX-OUT2	(Axis4/20)
		///							bit24 : EX-OUT1	(Axis12/28)		bit8  : EX-OUT1	(Axis4/20)
		///							bit23 : EX-OUT2	(Axis11/27)		bit7  : EX-OUT2	(Axis3/19)
		///							bit22 : EX-OUT1	(Axis11/27)		bit6  : EX-OUT1	(Axis3/19)
		///							bit21 : EX-OUT2	(Axis10/26)		bit5  : EX-OUT2	(Axis2/18)
		///							bit20 : EX-OUT1	(Axis10/26)		bit4  : EX-OUT1	(Axis2/18)
		///							bit19 : EX-OUT2	(Axis9/25)		bit3  :	EX-OUT2	(Axis1/17)
		///							bit18 : EX-OUT1	(Axis9/25)		bit2  : EX-OUT1	(Axis1/17)
		///							bit17 : EX-OUT2	(Axis8/24)		bit1  : EX-OUT2	(Axis0/16)
		///							bit16 : EX-OUT1	(Axis8/24)		bit0  : EX-OUT1	(Axis0/16)
		/// 
		///							[ SNET-EMD-L2K ]
		///							"port = 0"  
		///
		///							bit31 : NC							bit15 : User	(Y0.F)
		///							bit30 : NC							bit14 : User	(Y0.E)
		///							bit29 : NC							bit13 : User	(Y0.D)
		///							bit28 : NC							bit12 : User	(Y0.C)
		///							bit27 : User			   (Y1.B)	bit11 : User	(Y0.B)
		///							bit26 : Alarm Clear (Axis3)(Y1.A)	bit10 : User	(Y0.A)
		///							bit25 : Servo On	(Axis3)(Y1.9)	bit9  : User	(Y0.9)
		///							bit24 : User			   (Y1.8)	bit8  : User	(Y0.8)
		///							bit23 : Alarm Clear (Axis2)(Y1.7)	bit7  : User	(Y0.7)
		///							bit22 : Servo On	(Axis2)(Y1.6)   bit6  : User	(Y0.6)
		///							bit21 : User			   (Y1.5)   bit5  : User	(Y0.5)
		///							bit20 : Alarm Clear	(Axis1)(Y1.4)   bit4  : User	(Y0.4)
		///							bit19 : Servo On	(Axis1)(Y1.3)	bit3  : User	(Y0.3)
		///							bit18 : User			   (Y1.2) 	bit2  : User	(Y0.2)
		///							bit17 : Alarm Clear	(Axis0)(Y1.1)	bit1  : User	(Y0.1)
		///							bit16 : Servo On	(Axis0)(Y1.0)	bit0  : User	(Y0.0)
		/// 
		///							"port = 1"  
		///
		///							bit31 : NC							bit15 : User	(Y2.F)
		///							bit30 : NC							bit14 : User	(Y2.E)
		///							bit29 : NC							bit13 : User	(Y2.D)
		///							bit28 : NC							bit12 : User	(Y2.C)
		///							bit27 : User			   (Y3.B)	bit11 : NC		(Y2.B)
		///							bit26 : Alarm Clear (Axis7)(Y3.A)	bit10 : User	(Y2.A)
		///							bit25 : Servo On	(Axis7)(Y3.9)	bit9  : User	(Y2.9)
		///							bit24 : User			   (Y3.8)	bit8  : User	(Y2.8)
		///							bit23 : Alarm Clear (Axis6)(Y3.7)	bit7  : NC		(Y2.7)
		///							bit22 : Servo On	(Axis6)(Y3.6)   bit6  : NC		(Y2.6)
		///							bit21 : User			   (Y3.5)   bit5  : NC		(Y2.5)
		///							bit20 : Alarm Clear	(Axis5)(Y3.4)   bit4  : User	(Y2.4)
		///							bit19 : Servo On	(Axis5)(Y3.3)	bit3  : User	(Y2.3)
		///							bit18 : User			   (Y3.2) 	bit2  : User	(Y2.2)
		///							bit17 : Alarm Clear	(Axis4)(Y3.1)	bit1  : User	(Y2.1)
		///							bit16 : Servo On	(Axis4)(Y3.0)	bit0  : User	(Y2.0)		</param>
		///
		/// <param name="point">	: point : 0 ~ 31 (bit number)									</param>
		/// <param name="onoff">	: "true"->on, "false"->off										</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")								</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetMcbUserOutputPoint(int net, int port, int point, out int onoff);

		/// <summary>
		/// 하드웨어적으로 Mapping된 I/O Address에 다른 임의의 한 Port의 모든 point 쓰기
		/// </summary>
		/// <param name="port">	:	hardware i/o address port, "0 ~ 1"
		///							[  SNET-P  ]
		///							"port = 0" 
		///
		///							bit31 : NC			bit15 : Alarm Clear	(Axis7)
		///							bit30 : NC			bit14 : Servo On	(Axis7)
		///							bit29 : NC			bit13 : Alarm Clear	(Axis6)
		///							bit28 : NC			bit12 : Servo On	(Axis6)
		///							bit27 : NC			bit11 : Alarm Clear	(Axis5)
		///							bit26 : NC			bit10 : Servo On	(Axis5)
		///							bit25 : NC			bit9  : Alarm Clear	(Axis4)
		///							bit24 : NC			bit8  : Servo On	(Axis4)
		///							bit23 : NC			bit7  : Alarm Clear	(Axis3)
		///							bit22 : NC			bit6  : Servo On	(Axis3)
		///							bit21 : NC			bit5  : Alarm Clear	(Axis2)
		///							bit20 : NC			bit4  : Servo On	(Axis2)
		///							bit19 : User		bit3  : Alarm Clear	(Axis1) 
		///							bit18 : User		bit2  : Servo On	(Axis1)
		///							bit17 : User		bit1  : Alarm Clear	(Axis0) 
		///							bit16 : User		bit0  : Servo On	(Axis0) 
		///							
		///							[  SNET-RTEX  ]
		///							"port = 0" : Axis15 ~ Axis0 /	"port = 1" : Axis31 ~ Axis16
		///
		///							bit31 : EX-OUT2	(Axis15/31)		bit15 : EX-OUT2	(Axis7/23)
		///							bit30 : EX-OUT1	(Axis15/31)		bit14 : EX-OUT1	(Axis7/23)
		///							bit29 : EX-OUT2	(Axis14/30)		bit13 : EX-OUT2	(Axis6/22)
		///							bit28 : EX-OUT1	(Axis14/30)		bit12 : EX-OUT1	(Axis6/22)
		///							bit27 : EX-OUT2	(Axis13/29)		bit11 : EX-OUT2	(Axis5/21)
		///							bit26 : EX-OUT1	(Axis13/29)		bit10 : EX-OUT1	(Axis5/21)
		///							bit25 : EX-OUT2	(Axis12/28)		bit9  : EX-OUT2	(Axis4/20)
		///							bit24 : EX-OUT1	(Axis12/28)		bit8  : EX-OUT1	(Axis4/20)
		///							bit23 : EX-OUT2	(Axis11/27)		bit7  : EX-OUT2	(Axis3/19)
		///							bit22 : EX-OUT1	(Axis11/27)		bit6  : EX-OUT1	(Axis3/19)
		///							bit21 : EX-OUT2	(Axis10/26)		bit5  : EX-OUT2	(Axis2/18)
		///							bit20 : EX-OUT1	(Axis10/26)		bit4  : EX-OUT1	(Axis2/18)
		///							bit19 : EX-OUT2	(Axis9/25)		bit3  :	EX-OUT2	(Axis1/17)
		///							bit18 : EX-OUT1	(Axis9/25)		bit2  : EX-OUT1	(Axis1/17)
		///							bit17 : EX-OUT2	(Axis8/24)		bit1  : EX-OUT2	(Axis0/16)
		///							bit16 : EX-OUT1	(Axis8/24)		bit0  : EX-OUT1	(Axis0/16)
		/// 
		///							[ SNET-EMD-L2K ]
		///							"port = 0"  
		///
		///							bit31 : NC							bit15 : User	(Y0.F)
		///							bit30 : NC							bit14 : User	(Y0.E)
		///							bit29 : NC							bit13 : User	(Y0.D)
		///							bit28 : NC							bit12 : User	(Y0.C)
		///							bit27 : User			   (Y1.B)	bit11 : User	(Y0.B)
		///							bit26 : Alarm Clear (Axis3)(Y1.A)	bit10 : User	(Y0.A)
		///							bit25 : Servo On	(Axis3)(Y1.9)	bit9  : User	(Y0.9)
		///							bit24 : User			   (Y1.8)	bit8  : User	(Y0.8)
		///							bit23 : Alarm Clear (Axis2)(Y1.7)	bit7  : User	(Y0.7)
		///							bit22 : Servo On	(Axis2)(Y1.6)   bit6  : User	(Y0.6)
		///							bit21 : User			   (Y1.5)   bit5  : User	(Y0.5)
		///							bit20 : Alarm Clear	(Axis1)(Y1.4)   bit4  : User	(Y0.4)
		///							bit19 : Servo On	(Axis1)(Y1.3)	bit3  : User	(Y0.3)
		///							bit18 : User			   (Y1.2) 	bit2  : User	(Y0.2)
		///							bit17 : Alarm Clear	(Axis0)(Y1.1)	bit1  : User	(Y0.1)
		///							bit16 : Servo On	(Axis0)(Y1.0)	bit0  : User	(Y0.0)
		/// 
		///							"port = 1"  
		///
		///							bit31 : NC							bit15 : User	(Y2.F)
		///							bit30 : NC							bit14 : User	(Y2.E)
		///							bit29 : NC							bit13 : User	(Y2.D)
		///							bit28 : NC							bit12 : User	(Y2.C)
		///							bit27 : User			   (Y3.B)	bit11 : NC		(Y2.B)
		///							bit26 : Alarm Clear (Axis7)(Y3.A)	bit10 : User	(Y2.A)
		///							bit25 : Servo On	(Axis7)(Y3.9)	bit9  : User	(Y2.9)
		///							bit24 : User			   (Y3.8)	bit8  : User	(Y2.8)
		///							bit23 : Alarm Clear (Axis6)(Y3.7)	bit7  : NC		(Y2.7)
		///							bit22 : Servo On	(Axis6)(Y3.6)   bit6  : NC		(Y2.6)
		///							bit21 : User			   (Y3.5)   bit5  : NC		(Y2.5)
		///							bit20 : Alarm Clear	(Axis5)(Y3.4)   bit4  : User	(Y2.4)
		///							bit19 : Servo On	(Axis5)(Y3.3)	bit3  : User	(Y2.3)
		///							bit18 : User			   (Y3.2) 	bit2  : User	(Y2.2)
		///							bit17 : Alarm Clear	(Axis4)(Y3.1)	bit1  : User	(Y2.1)
		///							bit16 : Servo On	(Axis4)(Y3.0)	bit0  : User	(Y2.0)		</param>
		///
		/// <param name="points">	: bit data, "1" -> On, "0" -> Off								</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")								</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetMcbUserOutputPort(int net, int port, int points);

		/// <summary>
		/// 하드웨어적으로 Mapping된 I/O Address에 다른 임의의 한 Point 쓰기
		/// </summary>
		/// <param name="net"> 	: controller id														</param>						
		/// <param name="port">		: hardware i/o address port														
		///							[  SNET-P  ]
		///							"port = 0" 
		///
		///							bit31 : NC			bit15 : Alarm Clear	(Axis7)
		///							bit30 : NC			bit14 : Servo On	(Axis7)
		///							bit29 : NC			bit13 : Alarm Clear	(Axis6)
		///							bit28 : NC			bit12 : Servo On	(Axis6)
		///							bit27 : NC			bit11 : Alarm Clear	(Axis5)
		///							bit26 : NC			bit10 : Servo On	(Axis5)
		///							bit25 : NC			bit9  : Alarm Clear	(Axis4)
		///							bit24 : NC			bit8  : Servo On	(Axis4)
		///							bit23 : NC			bit7  : Alarm Clear	(Axis3)
		///							bit22 : NC			bit6  : Servo On	(Axis3)
		///							bit21 : NC			bit5  : Alarm Clear	(Axis2)
		///							bit20 : NC			bit4  : Servo On	(Axis2)
		///							bit19 : User		bit3  : Alarm Clear	(Axis1) 
		///							bit18 : User		bit2  : Servo On	(Axis1)
		///							bit17 : User		bit1  : Alarm Clear	(Axis0) 
		///							bit16 : User		bit0  : Servo On	(Axis0) 
		///							
		///							[  SNET-RTEX  ]
		///							"port = 0" : Axis15 ~ Axis0 /	"port = 1" : Axis31 ~ Axis16
		///
		///							bit31 : EX-OUT2	(Axis15/31)		bit15 : EX-OUT2	(Axis7/23)
		///							bit30 : EX-OUT1	(Axis15/31)		bit14 : EX-OUT1	(Axis7/23)
		///							bit29 : EX-OUT2	(Axis14/30)		bit13 : EX-OUT2	(Axis6/22)
		///							bit28 : EX-OUT1	(Axis14/30)		bit12 : EX-OUT1	(Axis6/22)
		///							bit27 : EX-OUT2	(Axis13/29)		bit11 : EX-OUT2	(Axis5/21)
		///							bit26 : EX-OUT1	(Axis13/29)		bit10 : EX-OUT1	(Axis5/21)
		///							bit25 : EX-OUT2	(Axis12/28)		bit9  : EX-OUT2	(Axis4/20)
		///							bit24 : EX-OUT1	(Axis12/28)		bit8  : EX-OUT1	(Axis4/20)
		///							bit23 : EX-OUT2	(Axis11/27)		bit7  : EX-OUT2	(Axis3/19)
		///							bit22 : EX-OUT1	(Axis11/27)		bit6  : EX-OUT1	(Axis3/19)
		///							bit21 : EX-OUT2	(Axis10/26)		bit5  : EX-OUT2	(Axis2/18)
		///							bit20 : EX-OUT1	(Axis10/26)		bit4  : EX-OUT1	(Axis2/18)
		///							bit19 : EX-OUT2	(Axis9/25)		bit3  :	EX-OUT2	(Axis1/17)
		///							bit18 : EX-OUT1	(Axis9/25)		bit2  : EX-OUT1	(Axis1/17)
		///							bit17 : EX-OUT2	(Axis8/24)		bit1  : EX-OUT2	(Axis0/16)
		///							bit16 : EX-OUT1	(Axis8/24)		bit0  : EX-OUT1	(Axis0/16)
		/// 
		///							[ SNET-EMD-L2K ]
		///							"port = 0"  
		///
		///							bit31 : NC							bit15 : User	(Y0.F)
		///							bit30 : NC							bit14 : User	(Y0.E)
		///							bit29 : NC							bit13 : User	(Y0.D)
		///							bit28 : NC							bit12 : User	(Y0.C)
		///							bit27 : User			   (Y1.B)	bit11 : User	(Y0.B)
		///							bit26 : Alarm Clear (Axis3)(Y1.A)	bit10 : User	(Y0.A)
		///							bit25 : Servo On	(Axis3)(Y1.9)	bit9  : User	(Y0.9)
		///							bit24 : User			   (Y1.8)	bit8  : User	(Y0.8)
		///							bit23 : Alarm Clear (Axis2)(Y1.7)	bit7  : User	(Y0.7)
		///							bit22 : Servo On	(Axis2)(Y1.6)   bit6  : User	(Y0.6)
		///							bit21 : User			   (Y1.5)   bit5  : User	(Y0.5)
		///							bit20 : Alarm Clear	(Axis1)(Y1.4)   bit4  : User	(Y0.4)
		///							bit19 : Servo On	(Axis1)(Y1.3)	bit3  : User	(Y0.3)
		///							bit18 : User			   (Y1.2) 	bit2  : User	(Y0.2)
		///							bit17 : Alarm Clear	(Axis0)(Y1.1)	bit1  : User	(Y0.1)
		///							bit16 : Servo On	(Axis0)(Y1.0)	bit0  : User	(Y0.0)
		/// 
		///							"port = 1"  
		///
		///							bit31 : NC							bit15 : User	(Y2.F)
		///							bit30 : NC							bit14 : User	(Y2.E)
		///							bit29 : NC							bit13 : User	(Y2.D)
		///							bit28 : NC							bit12 : User	(Y2.C)
		///							bit27 : User			   (Y3.B)	bit11 : NC		(Y2.B)
		///							bit26 : Alarm Clear (Axis7)(Y3.A)	bit10 : User	(Y2.A)
		///							bit25 : Servo On	(Axis7)(Y3.9)	bit9  : User	(Y2.9)
		///							bit24 : User			   (Y3.8)	bit8  : User	(Y2.8)
		///							bit23 : Alarm Clear (Axis6)(Y3.7)	bit7  : NC		(Y2.7)
		///							bit22 : Servo On	(Axis6)(Y3.6)   bit6  : NC		(Y2.6)
		///							bit21 : User			   (Y3.5)   bit5  : NC		(Y2.5)
		///							bit20 : Alarm Clear	(Axis5)(Y3.4)   bit4  : User	(Y2.4)
		///							bit19 : Servo On	(Axis5)(Y3.3)	bit3  : User	(Y2.3)
		///							bit18 : User			   (Y3.2) 	bit2  : User	(Y2.2)
		///							bit17 : Alarm Clear	(Axis4)(Y3.1)	bit1  : User	(Y2.1)
		///							bit16 : Servo On	(Axis4)(Y3.0)	bit0  : User	(Y2.0)			</param>
		///
		/// <param name="point">	: point : 0 ~ 31 (bit number)										</param>
		/// <param name="onoff">	: "1"->on, "0"->off													</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")									</returns>		
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetMcbUserOutputPoint(int net, int port, int point, int onoff);

		#endregion

		#region User I/O (SNET-P)

		/*** Snet Pulse User Input / Output ***/

		/// <summary>
		/// SNET-P4/P8 모든 축 Axis io,User io 읽기  
		/// (tip 1) 파라미터가(사용 유무,입력 극성..)적용 되지 않은 물리적 입력 상태를 보여 줍니다. 
		/// : old version name -->"eSnetGetPUserInput"
		/// </summary>
		/// <param name="net"> 		: controller id 													</param>
		/// <param name="ports"> 	: [array],배열크기 ->"2"											
		///								=> "1"입력 On, "0"입력 Off
		///								=> ports[1]:Axis7 ~ Axis4 / ports[0]:Axis3 ~ Axis0
		///
		///							    bit 15	    14		 13		  12   |       11		 10		  9		   8
		///							 		   	Axis3(Axis7) 			   |           Axis2(Axis6)    
		///							  User Input3   Home   +Limit   -Limit |  User Input2    Home   +Limit   -Limit 
		///							       7		 6		 5		  4    |        3		 2		  1		   0      
		///									   	Axis1(Axis5) 			   |           Axis0(Axis4)    
		///							  User Input1   Home   +Limit   -Limit |  User Input0    Home   +Limit   -Limit   </param>
		/// <returns>				: (see enum "eSnetApiReturnCode")									</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetPulseGetUserInput(int net, out int ports);

		/// <summary>
		/// SNET-P4/P8 모든 축 Axis io,User io 읽기  
		/// (tip 1) 파라미터가(사용 유무,입력 극성..)적용 되지 않은 물리적 입력 상태를 보여 줍니다. 
		/// : old version name -->"eSnetGetPUserInputPort2"
		/// </summary>
		/// <param name="net"> 		: controller id 													</param>						
		/// <param name="port0">	: Input value(Axis3 ~ Axis0)										</param>
		/// <param name="port1"> 	: Input value(Axis7 ~ Axis4)										
		///								=> "1"입력 On, "0"입력 Off
		///
		///							    bit 15	    14		 13		  12   |       11		 10		  9		   8
		///							 		   	Axis3(Axis7) 			   |           Axis2(Axis6)    
		///							  User Input3   Home   +Limit   -Limit |  User Input2    Home   +Limit   -Limit 
		///							       7		 6		 5		  4    |        3		 2		  1		   0      
		///									   	Axis1(Axis5) 			   |           Axis0(Axis4)    
		///							  User Input1   Home   +Limit   -Limit |  User Input0    Home   +Limit   -Limit   </param>
		///
		/// <returns>				: (see enum "eSnetApiReturnCode")									</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetPulseGetUserInputPortAll(int net, out int port0, out int port1);

		/// <summary>
		/// SNET-P4/P8 모든 축(4축 단위) Axis io,User io 읽기  
		/// : old version name -->"eSnetGetPUserInputPort"
		/// </summary>
		/// <param name="net"> 		: controller id 													</param>						
		/// <param name="port"> 	: "1"(Axis7 ~ Axis4), "0"(Axis3 ~ Axis0)							</param>						
		/// <param name="points">	: Input Value								
		///
		///							    bit 15	    14		 13		  12   |       11		 10		  9		   8
		///							 		   	Axis3(Axis7) 			   |           Axis2(Axis6)    
		///							  User Input3   Home   +Limit   -Limit |  User Input2    Home   +Limit   -Limit 
		///							       7		 6		 5		  4    |        3		 2		  1		   0      
		///									   	Axis1(Axis5) 			   |           Axis0(Axis4)    
		///							  User Input1   Home   +Limit   -Limit |  User Input0    Home   +Limit   -Limit   </param>
		///
		/// <returns>				: (see enum "eSnetApiReturnCode")									</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetPulseGetUserInputPort(int net, int port, out int points);

		/// <summary>
		/// SNET-P4/P8 축별 "Axis io 입력 /User io" 입력 상태를 bit 단위로 읽기 
		/// : old version name -->"eSnetGetPUserInputPoint"
		/// </summary>
		/// <param name="port"> 	: "1"(Axis7 ~ Axis4), "0"(Axis3 ~ Axis0)							</param>
		/// <param name="point">	: bit 번호 									
		///
		///							    bit 15	    14		 13		  12   |       11		 10		  9		   8
		///							 		   	Axis3(Axis7) 			   |           Axis2(Axis6)    
		///							  User Input3   Home   +Limit   -Limit |  User Input2    Home   +Limit   -Limit 
		///							       7		 6		 5		  4    |        3		 2		  1		   0      
		///									   	Axis1(Axis5) 			   |           Axis0(Axis4)    
		///							  User Input1   Home   +Limit   -Limit |  User Input0    Home   +Limit   -Limit   </param>
		///
		/// <param name="on_off">	: "1"-->on, "0"-->off												</param>
		/// <returns>				(see enum "eSnetApiReturnCode")										</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetPulseGetUserInputPoint(int net, int port, int point, out int on_off);

		/// <summary>
		/// SNET-P4/P8 "User Output" 출력 상태값 읽기 
		/// : old version name -->"eSnetGetPUserOutput"
		/// </summary>
		/// <param name="net"> 		: controller id 																</param>						
		/// <param name="ports"> 	: [array] 배열크기 ->"2" 						
		///								=> ports[0]: User Output_1(bit1), User Output_0(bit0) (SNET-P4 User IO 커넥터)
		///								=> ports[1]: User Output_3(bit1), User Output_2(bit0) (SNET-P8 User IO 커넥터) 
		///										|	  1		|     0	   |
		///										| Output 1	| Output 0 |											</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")												</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetPulseGetUserOutput(int net, out int ports);

		/// <summary>
		/// SNET-P4/P8 "User Output" 출력 상태값 읽기 
		/// : old version name -->"eSnetGetPUserOutputPort2"
		/// </summary>
		/// <param name="net"> 		: controller id 												</param>						
		/// <param name="port0"> 	: Output_1, User Output_0 출력 상태 (SNET-P4 User IO 커넥터) 	</param>
		/// <param name="port1"> 	: Output_3, User Output_2 출력 상태 (SNET-P8 User IO 커넥터) 	
		///								|	  1		|     0	   |
		///								| Output 1	| Output 0 |									</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")								</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetPulseGetUserOutputPortAll(int net, out int port0, out int port1);

		/// <summary>
		/// SNET-P4/P8 "User Output" 출력 상태값 읽기 
		/// : old version name -->"eSnetGetPUserOutputPort"
		/// </summary>
		/// <param name="net"> 		: controller id 											</param>
		/// <param name="port"> 	: "0" or "1"
		///							  "0"->"SNET-P4 User Output1,User Output0" 출력 상태 읽기
		///							  "1"->"SNET-P8 User Output3,User Output2" 출력 상태 읽기	</param>
		/// <param name="points">	: 출력 상태 데이터 (접점1,접점0 ->2bit)						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")							</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetPulseGetUserOutputPort(int net, int port, out int points);

		/// <summary>
		/// SNET-P4/P8 "User Output" 출력 상태를 "bit 단위(point)"로 읽기 
		/// : old version name -->"eSnetGetPUserOutputPoint"
		/// </summary>
		/// <param name="net"> 		: controller id 											</param>
		/// <param name="port"> 	: "0" or "1"
		///							  "0"->"SNET-P4 User Output1,User Output0" 출력 상태 읽기
		///							  "1"->"SNET-P8 User Output3,User Output2" 출력 상태 읽기	</param>
		/// <param name="point">	: 출력 접점 번호 선택 	
		///							  "0->출력 접점 0, "1"->출력 접점 1							</param>			
		/// <param name="on_off">	: "1"-->on, "0"-->off					
		///								=>	port =0  point= 0 : "Output_0" 읽기 
		///								=>	port =0  point= 1 : "Output_1" 읽기
		///								=>	port =1  point= 0 : "Output_2" 읽기 
		///								=>	port =1  point= 1 : "Output_3" 읽기					</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")							</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetPulseGetUserOutputPoint(int net, int port, int point, out int on_off);

		/// <summary>
		/// SNET-P4/P8 "User Output" 출력 접점 쓰기 
		/// : old version name -->"eSnetSetPUserOutputPoint"
		/// </summary>
		/// <param name="net"> 		: controller id 							</param>
		/// <param name="port"> 	: "0" or "1"		
		///							  "0"->"SNET-P4 User Output1,User Output0"
		///							  "1"->"SNET-P8 User Output3,User Output2"	</param>
		/// <param name="point">	: "0" or "1"
		///							  "0"-> 출력 접점 0, "1"->출력 접점 1		</param>
		/// <param name="on_off">	: "1"->on, "0"->off													
		///								=>	port =0  point= 0 : "Output_0" 쓰기 
		///								=>	port =0  point= 1 : "Output_1" 쓰기 
		///								=>	port =1  point= 0 : "Output_2" 쓰기 
		///								=>	port =1  point= 1 : "Output_3" 쓰기 </param>
		/// <returns>				: (see enum "eSnetApiReturnCode")			</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetPulseSetUserOutputPoint(int net, int port, int point, int on_off);

		#endregion

		#region User I/O (SNET-P AD Option Board)

		/*** Extension Board I/O ***/

		/// <summary>
		/// SNET-P Extension B'D(AD Option Board)의 모든 입력 접점 읽기(INP1 ~ INP4) 
		/// (tip 1)"INP1 ~ INP4"는 제품에 표시된 커넥터 label 이름 입니다. 
		/// (tip 2)각 port(INP1~INP4)당 입력 접점은 8개 입니다. 
		/// </summary>
		/// <param name="net"> 					: controller id 						</param>
		/// <param name="ports"> : [array] 배열크기 ->"4"
		///											[0]:INP1 입력값, [1]:INP2 입력값 
		///											[2]:INP2 입력값, [3]:INP4 입력값	</param>
		/// <returns>							: (see enum "eSnetApiReturnCode")		</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetPulseExGetIoInput(int net, out int ports);

		/// <summary>
		/// SNET-P Extension B'D(AD Option Board)의 모든 입력 접점 읽기(INP1 ~ INP4) 
		/// (tip 1)"INP1 ~ INP4"는 제품에 표시된 커넥터 label 이름 입니다. 
		/// (tip 2)각 "port(INP1~INP4)"당 입력 접점은 8개 입니다. 
		/// : old version name -->"eSnetGetPIoInputPort2"
		/// </summary>
		/// <param name="net">		: controller id						</param>
		/// <param name="port0"> 	: "INP1" 입력 데이터 				</param>
		/// <param name="port1"> 	: "INP2" 입력 데이터 				</param>
		/// <param name="port2"> 	: "INP3" 입력 데이터 				</param>	
		/// <param name="port3"> 	: "INP4" 입력 데이터 				</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetPulseExGetIoInputPortAll(int net, out int port0, out int port1, out int port2, out int port3);

		/// <summary>
		/// "SNET-P Extension B'D"의 "INP1~INP4"중 지정한 port에서 8접점(8bit) 데이터 읽기 
		/// (tip 1) "INP1 ~ INP4"는 제품에 표시된 커넥터 label 이름 입니다. 
		/// : old version name -->"eSnetGetPIoInputPort"
		/// </summary>
		/// <param name="net"> 		: controller id 						</param>						
		/// <param name="port"> 	: 입력 포트 번호,"0(INP1)"~"3(INP4)"	</param>						
		/// <param name="points">	: 8bit 입력 접점 데이터					</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetPulseExGetIoInputPort(int net, int port, out int points);

		/// <summary>
		/// SNET-P Extension B'D "INP1~INP4"중 지정한 port에서 1접점(point)입력값 읽기 
		/// : old version name -->"eSnetGetPIoInputPoint"
		/// </summary>
		/// <remarks>
		/// (tip 1) "INP1 ~ INP4"는 제품에 표시된 커넥터 label 이름 입니다. 
		/// </remarks>
		/// <param name="net"> 		: controller id 						</param>						
		/// <param name="port"> 	: 입력 포트 번호,"0(INP1)" ~ "3(INP4)"	</param>
		/// <param name="point">	: 접점 번호,"0" ~ "7" 					</param>
		/// <param name="on_off">	: 접점 입력값,"1"->on, "0"->off			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetPulseExGetIoInputPoint(int net, int port, int point, out int on_off);

		/// <summary>
		/// SNET-P Extension B'D의 "OUTP1 ~ OUTP3"출력 상태 데이터 읽기 
		/// (tip 1) "OUTP1 ~ OUTP3"은 제품에 표시된 커넥터 label 입니다. 
		/// (tip 2) "OUTP1 ~ OUTP2"의 출력 접점 개수는 port당 8접점이고 "OUTP3"은 4접점 입니다. 
		/// (tip 3) "접점 번호"는 "bit번호"와 1:1 대응 됩니다. 
		/// : old version name -->"eSnetGetPIoOutputPort"  
		/// </summary>
		/// <param name="net">		: controller id 						</param>
		/// <param name="ports">	: [array] 배열크기 ->"3" 				
		///								[0]:OUTP1 출력 상태 데이터(8bit), 
		///								[1]:OUTP2 출력 상태 데이터(8bit), 
		///								[2]:OUTP3 출력 상태 데이터(4bit),	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetPulseExGetIoOutput(int net, out int ports);

		/// <summary>
		/// SNET-P Extension B'D의 "OUTP1 ~ OUTP3" 출력 데이터 모두 읽기  
		/// (tip 1) "OUTP1 ~ OUTP3"은 제품에 표시된 커넥터 label 입니다. 
		/// (tip 2) "접점 번호"는 "bit번호"와 1:1 대응 됩니다.   
		/// : old version name -->"eSnetGetPIoOutputPort2"
		/// </summary>
		/// <param name="net"> 		: controller id 					</param>
		/// <param name="port0"> 	: "OUTP1" 출력 상태 데이터 			</param>
		/// <param name="port1">	: "OUTP2" 출력 상태 데이터 			</param>
		/// <param name="port2">	: "OUTP3" 출력 상태 데이터 			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetPulseExGetIoOutputPortAll(int net, out int port0, out int port1, out int port2);

		/// <summary>
		/// SNET-P Extension B'D의 "OUTP1 ~ OUTP3"중 지정한 port의 출력 데이터 읽기 
		/// (tip 1) "OUTP1 ~ OUTP3"은 제품에 표시된 커넥터 label 입니다. 
		/// (tip 2) "접점 번호"는 "bit번호"와 1:1 대응 됩니다.   
		/// : old version name -->"eSnetGetPIoOutputPort"
		/// </summary>
		/// <param name="net"> 		: controller id 							</param>						
		/// <param name="port"> 	: 출력 포트 번호,"0(OUTP1)" ~ "2(OUTP3)"	</param>
		/// <param name="points">	: 현재 출력 상태값							</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")			</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetPulseExGetIoOutputPort(int net, int port, out int points);

		/// <summary>
		/// SNET-P Extension B'D의 "OUTP1 ~ OUTP3"출력 접점 중 특정 port의 1접점 출력 상태 읽기 
		/// (tip 1) "OUTP1 ~ OUTP3"은 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetGetPIoOutputPoint"
		/// </summary>
		/// <param name="net"> 		: controller id 							</param>						
		/// <param name="port"> 	: 출력 포트 번호,"0(OUTP1)" ~ "2(OUTP3)"	</param>
		/// <param name="point">	: 출력 접점 번호 							
		///								port=0,port=1 -> "0(bit0)~7(bit7)"
		///								port=2		  -> "0(bit0)~3(bit3)"		</param>
		/// <param name="on_off">	: 현재 출력 상태값, "1"->on, "0"->off		</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")			</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetPulseExGetIoOutputPoint(int net, int port, int point, out int on_off);

		/// <summary>
		/// SNET-P Extension B'D "OUTP1 ~ OUTP3"출력 접점 중 "1 port 단위(8접점 or 4접점)"출력 하기 
		/// (tip 1) "OUTP1 ~ OUTP3"은 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetSetPIoOutputPort"
		/// </summary>
		/// <param name="net"> 		: controller id 							</param>						
		/// <param name="port"> 	: 출력 포트 번호,"0(OUTP1)" ~ "2(OUTP3)"	</param>
		/// <param name="points">	: 출력 값 									
		///								port=0,port=1-> 0x0 ~ 0xff(8접점:8bit) 
		///								port=2		 -> 0x0 ~ 0x04(4접점:4bit)
		///								"1"->출력 on, "0"->출력 off				</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")			</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetPulseExSetIoOutputPort(int net, int port, int points);

		/// <summary>
		/// SNET-P Extension B'D의 "OUTP1 ~ OUTP3"출력 접점 중 지정 port에서 1접점 단위 출력 하기 
		/// (tip 1) "OUTP1 ~ OUTP3"은 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetSetPIoOutputPoint"
		/// </summary>
		/// <param name="net"> 		: controller id 							</param>						
		/// <param name="port"> 	: 출력 포트 번호,"0(OUTP1)" ~ "2(OUTP3)"	</param>
		/// <param name="point">	: 출력 접점 번호 							</param>						
		///								port=0,port=1 -> "0(bit0) ~ 7(bit7)"
		///								port=2		  -> "0(bit0) ~ 3(bit3)"		
		/// <param name="on_off">	: 출력값, "1"->on, "0"->off					</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")			</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetPulseExSetIoOutputPoint(int net, int port, int point, int on_off);

		#endregion

		#region User I/O (SNET-RTEX)

		/*** RTEX I/O ***/

		/// <summary>
		/// SNET-RTEX "INP1~INP6 입력 port데이터" 한번에 읽기 
		/// (tip 1) "INP1 ~ INP6"은 제품에 표시된 커넥터 label 정보 입니다. 
		/// : old version name -->"eSnetGetRtexIoInput"
		/// </summary>
		/// <param name="net"> 		: controller id 					</param>
		/// <param name="ports">	: [array] 배열크기 ->"6"				
		///								[0]:"INP1" 8접점 입력 data  
		///								[1]:"INP2" 8접점 입력 data  
		///								[2]:"INP3" 8접점 입력 data  
		///								[3]:"INP4" 8접점 입력 data  
		///								[4]:"INP5" 8접점 입력 data  
		///								[5]:"INP6" 8접점 입력 data		</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetIoInput(int net, out int port_data);

		/// <summary>
		/// SNET-RTEX "INP1~INP6 입력 데이터" 한번에 읽기 
		/// (tip 1) "INP1 ~ INP6"은 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetGetRtexIoInput2"
		/// </summary>
		/// <param name="net"> 		: controller id 						</param>
		/// <param name="port0">	: "INP1" 8접점 입력 data				</param>
		/// <param name="port1">	: "INP2" 8접점 입력 data				</param>
		/// <param name="port2">	: "INP3" 8접점 입력 data				</param>
		/// <param name="port3">	: "INP4" 8접점 입력 data				</param>
		/// <param name="port4">	: "INP5" 8접점 입력 data				</param>
		/// <param name="port5">	: "INP6" 8접점 입력 data				</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetIoInputPortAll(int net, out int port0, out int port1, out int port2, out int port3, out int port4, out int port5);

		/// <summary>
		/// SNET-RTEX "INP1~INP6" 입력 중 1개의 port에서 데이터 읽기 
		/// (tip 1)"INP1 ~ INP6"은 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetGetRtexIoInputPort"
		/// </summary>
		/// <param name="net"> 		: controller id 							</param>
		/// <param name="port">		: 입력 port 번호, "0"->INP1 ~ "5" ->INP6	</param>
		/// <param name="points">	: 입력 data(8 접점)							</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")			</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetIoInputPort(int net, int port, out int points);

		/// <summary>
		/// SNET-RTEX "INP1~INP6" 입력 중 1개의 port에서 1접점(point)읽기 
		/// (tip 1)"INP1 ~ INP6"은 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetGetRtexIoInputPoint"
		/// </summary>
		/// <param name="net"> 		: controller id 							</param>
		/// <param name="port">		: 입력 port 번호, "0"->INP1 ~ "5" ->INP6	</param>
		/// <param name="point">	: 입력 접점 번호, "0 ~ 7"					</param> 
		/// <param name="on_off">	: 입력 상태, "1"->on, "0"->off 				</param> 
		/// <returns>				: (see enum "eSnetApiReturnCode")			</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetIoInputPoint(int net, int port, int point, out int on_off);

		/// <summary>
		/// SNET-RTEX "OUTP1 ~ OUTP5" 출력 data 한꺼번에 읽기 
		/// (tip 1)"OUTP1 ~ OUTP5"는 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetGetRtexIoOutput"
		/// </summary>
		/// <param name="net"> 		: controller id 						</param>
		/// <param name="ports">	: [array] 배열크기 ->"5"
		///								[0]:"OUTP1" 8접점 출력 상태 data
		///								[1]:"OUTP2" 8접점 출력 상태 data
		///								[2]:"OUTP3" 8접점 출력 상태 data
		///								[3]:"OUTP4" 8접점 출력 상태 data
		///								[4]:"OUTP5" 4접점 출력 상태 data	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetIoOutput(int net, out int ports);

		/// <summary>
		/// SNET-RTEX "OUTP1 ~ OUTP5" 출력 data 한꺼번에 읽기 
		/// (tip 1)"OUTP1 ~ OUTP5"는 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetGetRtexIoOutput2"
		/// </summary>
		/// <param name="net"> 		: controller id 					</param>
		/// <param name="port0">	: "OUTP1" 8접점 출력 상태 data		</param>
		/// <param name="port1">	: "OUTP2" 8접점 출력 상태 data		</param>
		/// <param name="port2">	: "OUTP3" 8접점 출력 상태 data		</param>
		/// <param name="port3">	: "OUTP4" 8접점 출력 상태 data		</param>
		/// <param name="port4">	: "OUTP5" 4접점 출력 상태 data		</param>
		/// <returns>				(see enum "eSnetApiReturnCode")		</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetIoOutputPortAll(int net, out int port0, out int port1, out int port2, out int port3, out int port4);

		/// <summary>
		/// SNET-RTEX "OUTP1 ~ OUTP5"출력 중 1개 port의 출력 data 읽기 
		/// (tip 1)"OUTP1 ~ OUTP5"는 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetGetRtexIoOutputPort"
		/// </summary>
		/// <param name="net"> 	: controller id 							</param>
		/// <param name="port">	: 출력 port 번호, "0"->OUTP1 ~ "4" ->OUTP5	</param>
		/// <param name="points">	: 출력 상태 data							</param> 
		/// <returns>			: (see enum "eSnetApiReturnCode")			</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetIoOutputPort(int net, int port, out int points);

		/// <summary>
		/// SNET-RTEX "OUTP1 ~ OUTP5"출력 중 지정 port에서 1접점 출력 상태 데이터 읽기  
		/// (tip 1)"OUTP1 ~ OUTP5"는 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetGetRtexIoOutputPoint"
		/// </summary>
		/// <param name="net"> 		: controller id 							</param>
		/// <param name="port">		: 출력 port 번호, "0"->OUTP1 ~ "4" ->OUTP5	</param>
		/// <param name="point">	: 접점 번호 									
		///								"port=0 ~ port=3" : " 0 ~ 7"(8접점)
		///								"port=4	: " 0 ~ 3"(4접점)				</param> 
		///<param name="on_off">	: 출력 상태,"1"->on, "0"->off 				</param> 
		/// <returns>				: (see enum "eSnetApiReturnCode")			</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetIoOutputPoint(int net, int port, int point, out int on_off);

		/// <summary>
		/// SNET-RTEX "OUTP1 ~ OUTP5"출력 중 지정된 port의 모든 접점 출력 하기 
		/// (tip 1)"OUTP1 ~ OUTP5"는 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetSetRtexIoOutputPort"
		/// </summary>
		/// <param name="net"> 		: controller id 										</param>
		/// <param name="port">		: 출력 port 번호, "0"->OUTP1 ~ "4" ->OUTP5				</param>
		/// <param name="points">	: 출력 data,"1"->접점 출력 on, "0"-> 접점 출력 off 
		///								접점 번호는 bit번호와 대응 됩니다.		
		///								OUTP1 ~ OUTP4 : 0x00 ~ 0xff / OUTP5: 0X00 ~ 0X0F	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")						</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexSetIoOutputPort(int net, int port, int points);

		/// <summary>
		/// SNET-RTEX "OUTP1 ~ OUTP5"출력 중 지정된 port의 1접점 출력 하기 
		/// (tip 1)"OUTP1 ~ OUTP5"는 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetSetRtexIoOutputPoint"
		/// </summary>
		/// <param name="net"> 		: controller id 							</param>
		/// <param name="port">		: 출력 port 번호, "0"->OUTP1 ~ "4" ->OUTP5	</param>
		/// <param name="point">	: 출력 접점 번호									
		///								"port=0 ~ port=3": "0 ~ 7"(8접점)
		///								"port=4 : "0 ~ 3"(4접점)				</param>
		/// <param name="on_off">	: "1"->on, "0"->off							</param>									
		/// <returns>				: (see enum "eSnetApiReturnCode")			</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexSetIoOutputPoint(int net, int port, int point, int on_off);

		#endregion

		#region Remote Module I/O

		/*** Remote IO ***/
		/// SNET 제어기와 485통신으로 연결된 Remote IO Module APIs

		/// <summary>
		/// Remote IO Module 사용 유무 설정
		/// </summary>
		/// <remarks>
		/// (tip 1)본 설정값은 제어기 전원 재 인가시 사라집니다(휘발성),영구적으로 적용하기 위해서는 
		///        "HMI -> i/o -> config_remote"창에서 사용 여부를 변경 하고 제어기에 저장하여 사용 하십시요 
		/// </remarks>
		/// <param name="net"> 		: controller id										</param>						
		/// <param name="module">	: remote io module 번호 ("0"~ "15")					</param>						
		/// <param name="type">		: "16"->remote io module과 연결, "0"->사용하지 않음	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")					</returns>			
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetRemoteIoConfig(int net, int module, int type);

		/// <summary>
		/// Remote IO Module 사용 유무 설정값 읽기
		/// </summary>
		/// <param name="net"> 			: controller id										</param>						
		/// <param name="module">		: remote io module 번호 ("0"~ "15")					</param>						
		/// <param name="type">			: "16"->remote io module, "0"->연결되어 있지 않음 	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")					</returns>			
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetRemoteIoConfig(int net, int module, out int type);

		/// <summary>
		/// Remote IO Module 통신 상태 읽기
		/// </summary>
		/// <param name="net"> 			: controller id								</param>						
		/// <param name="module">		: remote io module 번호 ("0"~ "15")			</param>						
		/// <param name="status">		: [array] module status 정보, 
		///									[0] : io type : "16"->사용, "0"->사용하지 않음, 
		///									[1] : status : "0"-->ok,
		///									[2] : communication ok count,  
		///									[3] : communication fail count,  
		///									[4] : spare,							</param>	
		/// <returns>					: (see enum "eSnetApiReturnCode")			</returns>								
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetRemoteIoStatus(int net, int module, out int status);

		/// <summary>
		/// Remote IO 모듈별 Input/Output 32bit 접점 데이터 읽기  
		/// : old version name -->"eSnetGetRemoteIoPort2"
		/// </summary>
		/// <remarks>
		/// (tip 1)입/출력 접점은 bit 번호와 1:1로 매칭 됩니다.  (예) 첫번째 접점: bit0 ~ 서른두번째 접점(bit31) 
		/// </remarks>
		/// <param name="net"> 		: controller id						</param>						
		/// <param name="module">	: remote io module 번호 ("0"~ "15")	</param>						
		/// <param name="port_in">	: 32입력 접점 데이터				</param>
		/// <param name="port_out">	: 32출력 접점 데이터				</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetRemoteIoPortInOut(int net, int module, out int port_in, out int port_out);

		/// <summary>
		/// Remote IO 모듈별 입력 접점 읽기 
		/// </summary>
		/// <param name="net"> 		: controller id						</param>						
		/// <param name="module">	: remote io module 번호 ("0"~ "15")	</param>
		/// <param name="point">	: 입력 접점 bit번호 ("0"~ "31")		</param>
		/// <param name="on_off">	: "1"->on, "0"->off					</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetRemoteIoInputPoint(int net, int module, int point, out int on_off);

		/// <summary>
		/// Remote IO 모듈별 출력(1접점 단위) 접점 상태 읽기 
		/// </summary>
		/// <param name="net"> 		: controller id						</param>						
		/// <param name="module">	: remote io module 번호 ("0"~"15")	</param>						
		/// <param name="point">	: bit number ("0"~"31") 			</param>
		/// <param name="on_off">	: "1"->on, "0"->off					</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetRemoteIoOutputPoint(int net, int module, int point, out int on_off);

		/// <summary>
		/// Remote IO 모듈 출력 접점(32접점 단위)쓰기 
		/// </summary>
		/// <param name="net"> 		: controller id														</param>						
		/// <param name="module">	: remote io module 번호 ("0"~"15")									</param>						
		/// <param name="points">	: 32bit 출력 data												
		///								(예) "data = 0x03" (첫번째,두번째 출력 접점 On 나머지 접점 Off)	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")									</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetRemoteIoOutputPort(int net, int module, int points);

		/// <summary>
		/// Remote IO 모듈 출력 접점(1접점 단위)쓰기 
		/// </summary>
		/// <param name="net"> 		: controller id						</param>						
		/// <param name="module">	: remote io module 번호 ("0"~ "15")	</param>						
		/// <param name="point">	: bit number ("0"~"31") 			</param>
		/// <param name="on_off">	: "1"->on, "0"->off					</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetRemoteIoOutputPoint(int net, int module, int point, int on_off);

		#endregion

		#region Output For Time

		/*** 설정 시간 동안 출력 ON ***/

		/// <summary>
		/// 설정 시간 동안 출력 On
		/// </summary>
		/// <param name="net"> 		: controller id																</param>						
		/// <param name="channel">	: 채널(함수) 번호("0"~"4"),동시 실행 가능 함수 개수 5개(채널0 ~ 채널4)		</param>
		/// <param name="out_type">	: 출력 접점 종류 구분					
		///								# SNET-P
		///									"0"->Axis IO, "1"->Remote IO, "2"-> AD/DA Option Board IO 
		///								# SNET-RTEX
		///									"0"->Rtex Driver IO, "1"->Remote IO, "2"-> Rtex IO(제어기 User IO)  </param>
		/// <param name="out_port">	: # SNET-P
		///									"out_type = 0"일 경우: "Only 0" 
		///									"out_type = 1"일 경우: "0"~"15",Remote IO 모듈 번호  
		///									"out_type = 2"일 경우: "0(OUTP1)"~"2(OUTP3)",출력 컨넥터 번호  
		///							  # SNET-RTEX
		///									"out_type = 0"일 경우: "0"~"31",Rtex 서보 드라이버 id 
		///									"out_type = 1"일 경우: "0"~"15",Remote IO 모듈 번호  
		///									"out_type = 2"일 경우: "0(OUTP1)"~"4(OUTP5)",출력 컨넥터 번호		</param>
		/// <param name="out_time">	: [msec],출력 On 유지 시간 													</param>
		/// <param name="out_data">	: 출력 접점 번호,접점 설정값이"1"이면 사용,"0"이면 don't care
		///								(예)"out_type=1,out_port=0" 이고 "out_data= 0x05" 일경우 
		///									-> "remote 모듈 0"의 "첫번째 출력 접점(bit0)과 세번째 출력 접점(bit2)"을 
		///							  		"eSnetSetOutputForTime(...,채널,..)"함수에 사용						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")					
		///								CommunicationEventCode,
		///									SetOutputForTime (1601) ~
		///									SetOutputForRun  (1604)												</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetOutputForTime(int net, int channel, int out_type, int out_port, int out_time, int out_data);

		/// <summary>
		/// "eSnetSetOutputForTime" 채널별 실행 상태 확인 
		/// </summary>
		/// <param name="net"> 			: controller id											</param>
		/// <param name="channel">		: 채널(함수) 번호("0"~"4")								</param>
		/// <param name="run_status">	: "1"->출력 on상태,"0"->출력 off상태(출력 종료 상태)	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")						</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetOutputForTimeRun(int net, int channel, out int run_status);

		/// <summary>
		/// "eSnetSetOutputForTime" 채널별 실행을 초기화(종료) 합니다.
		/// </summary>
		/// <param name="net"> 		: controller id						</param>
		/// <param name="channel">	: 채널(함수) 번호("0"~"4")			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetResetOutputForTime(int net, int channel);

        #endregion

        #region Output At Position
        /// <summary>
        /// 설정 좌표-Actual Position-에서 접점 On/Off 
		/// 최대 10개의 내부 Ring 버퍼를 운영 합니다. 
        /// </summary>
        /// <param name="net"> 			: controller id 							</param>
        /// <param name="axis">			: Actual 좌표를 읽을 축 번호 					</param>
        /// <param name="on_posi">		: 출력 On 좌표, "mode=2"인 경우 미 사용 		</param>
        /// <param name="off_posi">		: 출력 Off 좌표,"mode=1"인 경우 미 사용 		</param>
        /// <param name="mode">			: "0"-> 접점 출력 ON, 출력 OFF 실행  	
        ///								  "1"-> 접점 출력 ON 실행 
        ///								  "2"-> 접점 출력 OFF 실행 					</param>
        /// <param name="out_count">	: 출력 접점수 (1 step당), 최대 4 접점 동시 출력 가능 	</param>
        /// <param name="output_type">	: [array] 배열 크기->out_count, 
        ///								  "0"->axis_io, "1"->remote_io, 
        ///								  "2"->rtex_user_io or Ad option board io	</param>
        /// <param name="port">			: [array] 배열 크기->output_count, port		</param>					
        /// <param name="point">		: [array] 배열크기->output_count, point		</param>						
        /// <returns>					: (see enum "eSnetApiReturnCode")				</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetSetOutAtActualPosition(int net, int axis, int on_posi, int off_posi,
				int mode, int out_count, out int output_type, out int port, out int point);

        /// <summary>
        /// "eSnetSetOutAtActualPosition" 설정 값 확인 
        ///	(tip 1) Ring 버퍼 저장 내용 중 "tail index"번호에 저장된 설정값 입니다. 	
        /// </summary>
        /// <param name="net"> 			: controller id 							</param>
        /// <param name="*enable">		: "1"-> 기능 실행 상태   	
        ///								  "0"-> 기능 정지 상태 						</param>
        /// <param name="*head">		: Ring 버퍼 "선두 index 번호"					</param>   		
        /// <param name="*tail">		: Ring 버퍼 "실행 index 번호"					</param>   		
        /// <param name="*axis">		: Actual 좌표를 읽을 축 번호 					</param>
        /// <param name="*on_posi">		: 출력 On 좌표, "mode=2"인 경우 미 사용 		</param>
        /// <param name="*off_posi">	: 출력 Off 좌표,"mode=1"인 경우 미 사용 		</param>
        /// <param name="*mode">		: "0"-> 접점 출력 ON, 출력 OFF 실행  	
        ///								  "1"-> 접점 출력 ON 실행 
        ///								  "2"-> 접점 출력 OFF 실행 					</param>
        /// <param name="*out_count">	: 출력 접점수 (1 step당), 최대 4 접점 동시 출력 가능 	</param>
        /// <param name="*output_type">	: [array] 배열 크기->"4"		</param>
        /// <param name="*port">		: [array] 배열 크기->"4"		</param>
        /// <param name="*point">		: [array] 배열 크기->"4"		</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")				</returns>	
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetGetOutAtActualPosition(int net, out int enable, out int head, out int tail, out int axis, out int on_posi, out int off_posi, out int mode,
															 out int out_count, out int output_type, out int port, out int point);
        /// <summary>
        /// 설정 좌표에서 접점 출력 기능 초기화 
        /// </summary>
        /// <param name="net"> 			: controller id 							</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")			</returns>	
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetResetOutAtActualPosition(int net);

        #endregion

        #region Latch Input 

        /*** Latch Input ***/

        /// <summary>
        /// Latch 입력 설정 ( "eSnetGetLatchInput"을 실행 할때 까지 입력 상태 유지)
        /// </summary>
        /// <param name="net"> 		: controller id															</param>
        /// <param name="channel">	: 채널(함수) 번호("0"~"4"),동시 실행 가능 함수 개수 5개(채널0~채널4)	</param>
        /// <param name="type">		: 입력 포트의 종류: 
        ///								0->axis_io, 1->remote io, 
        ///								2->Rtex io(SNET-RTEX),SNET-P (User Input)							</param>
        /// <param name="port">		: 입력 포트 번호("in_type" 따라 설정 범위 변경)							</param>
        /// <param name="point">	: 입력 접점 bit 번호 ("port" 따라 설정 범위 변경)						</param>
        /// <param name="edge">		: 입력 극성 : 1 ->A접점 입력, 0-> B접점 입력							</param>
        ///								(예) "channel=0","in_type=1","port=0","point=0x05","edge=1" 일경우 
        ///										-> Remote io 모듈 0의 첫번째 입력 접점과 세번째 입력 접점을 Latch 입력으로 사용 
        /// <returns>				: (see enum "eSnetApiReturnCode")									
        ///								CommunicationEventCode,
        ///								LatchInputChannelIndexFault (2401) ~
        ///								LatchInputChannelPointFault (2404),									</returns>		
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetLatchInput(int net, int channel, int type, int port, int point, int edge);

		/// <summary>
		/// Latch 입력 설정 초기화(Latch 입력 설정 해제)
		/// </summary>
		/// <param name="net"> 		: controller id								</param>
		/// <param name="channel">	: 채널(함수) 번호("0"~"4")					</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")						
		///								CommunicationEventCode,
		///								LatchInputChannelIndexFault (2401) ~
		///								LatchInputChannelPointFault (2404),		</returns>		
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetResetLatchInput(int net, int channel);

		/// <summary>
		/// Latch 입력 접점 Data 읽기 
		/// (tip 1) 본 함수를 실행 하면 "Latch 입력 데이터가 초기화"되고 새로운 실시간 입력값으로 업데이트 됩니다. 
		/// </summary>
		/// <param name="net"> 		: controller id											</param>
		/// <param name="channel">	: 채널(함수) 번호("0"~"4")								</param>
		/// <param name="data">		: Latch 접점 별 입력 data,"1"->Input On,"0"->Input Off	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")									
		///								CommunicationEventCode,
		///								LatchInputChannelIndexFault (2401) ~
		///								LatchInputChannelPointFault (2404),					</returns>		
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetLatchInput(int net, int channel, out int data);

		/// <summary>
		/// Latch 입력 설정 상태(eSnetSetLatchInput) 확인 
		/// </summary>
		/// <param name="net"> 		: controller id												</param>
		/// <param name="channel">	: 채널(함수) 번호("0"~"4")									</param>
		/// <param name="type">		: 입력 포트의 종류: 0->axis_io, 1->remote io, 2-> Rtex io		</param>
		/// <param name="port">		: 입력 포트 번호("in_type" 따라 설정 범위 변경)				</param>
		/// <param name="point">	: 입력 접점 bit 번호 ("port" 따라 설정 범위 변경)			</param>
		/// <param name="edge">		: 입력 극성 : 1 ->A접점 입력, 0-> B접점 입력				</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")									
		///								CommunicationEventCode,
		///								LatchInputChannelIndexFault (2401) ~
		///								LatchInputChannelPointFault (2404),						</returns>		
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetLatchInConfig(int net, int channel, out int type, out int port, out int point, out int edge);

		#endregion

		#region Flag I/O

		/*** Flag IO ***/

		/// <summary>
		/// Flag IO 값 읽기
		/// : old version name ->"eSnetGetFlagIoPort"
		/// </summary>
		/// <param name="net">		: controller id						</param>
		/// <param name="ports">	: array 배열 : 모든 Port 값			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetFlagIo(int net, out int ports);

		/// <summary>
		/// Flag IO 모든 Port값 읽기
		/// : old version name -->"eSnetGetFlagIoPort2"
		/// </summary>
		/// <param name="net">		: controller id						</param>
		/// <param name="port0">	: 첫번째	Port 값					</param>
		/// <param name="port1">	: 두번째	Port 값					</param>
		/// <param name="port2">	: 세번째	Port 값					</param>
		/// <param name="port3">	: 네번째	Port 값					</param>
		/// <param name="port4">	: 다섯번째	Port 값					</param>
		/// <param name="port5">	: 여섯번째	Port 값					</param>
		/// <param name="port6">	: 일곱번째	Port 값					</param>
		/// <param name="port7">	: 여덟번째	Port 값					</param>
		/// <param name="port8">	: 아홉번째	Port 값					</param>
		/// <param name="port9">	: 열번째	Port 값					</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetFlagIoPortAll(int net, out int port0, out int port1, out int port2, out int port3, out int port4, out int port5, out int port6, out int port7, out int port8, out int port9);

		/// <summary>
		/// Flag IO의 한 Point(bit) 값 쓰기
		/// </summary>
		/// <param name="net">		: controller id						</param>
		/// <param name="port">		: 원하는 point가 있는 port			</param>
		/// <param name="point">	: 원하는 point				
		/// <param name="on_off">	: point(bit) 값						</param>
		///								"0"->off, "1"->on				</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetFlagIoPoint(int net, int port, int point, int on_off);

		/// <summary>
		/// point(bit) 들의 논리 연산을 통해 Flag Io의 마지막 Port의 Point(bit) 들을 활성화 시키는 부가 기능 설정 하기
		/// </summary>
		/// <param name="net">		: controller id </param>
		/// <param name="point_f9">	: 논리 연산 수행 후 결과를 표시할 Flag IO 의 point(bit) ("0"~"31")	</param>
		/// <param name="op_code">	: 논리 연산,
		///								"0"->unused, "1"->and, "2"->or, "3"->not,
		///								"4"->nand, "5"->nor, "6"->xor									</param>
		/// <param name="port_a">	: 첫번째 항 Port, 
		///								axis input	-> 0  ~ 3  (4  port),
		///								axis output	-> 4  ~ 5  (2  port),
		///								flag io		-> 6  ~ 15 (10 port),
		///								remote io	-> 16 ~ 31 (16 port), 
		///								rtex in		-> 32 ~ 33 (2  port),
		///								rtex out	-> 34 ~ 35 (2  port),								</param>
		/// <param name="point_a">	: 첫번째 항 Point(bit) ("0"~"31")									</param>
		/// <param name="port_b">	: 두번째 항 Port,
		/// 							axis input	-> 0  ~ 3  (4  port),
		///								axis output	-> 4  ~ 5  (2  port),
		///								flag io		-> 6  ~ 15 (10 port),
		///								remote io	-> 16 ~ 31 (16 port), 
		///								rtex in		-> 32 ~ 33 (2  port),
		///								rtex out	-> 34 ~ 35 (2  port),								</param>
		/// <param name="point_b">	: 두번째 항의 Point(bit) ("0"~"31")									</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")									</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetFlagIoLogicOperation(int net, int point_f9, int op_code, int port_a, int point_a, int port_b, int point_b);

		/// <summary>
		/// point(bit) 들의 논리 연산을 통해 Flag Io의 마지막 Port의 Point(bit) 들을 활성화 시키는 부가 기능 설정값 읽기
		/// </summary>
		/// <param name="net">		: controller id </param>
		/// <param name="point_f9">	: 논리 연산 수행 후 결과를 표시할 Flag IO 의 point(bit) ("0"~"31")	</param>
		/// <param name="op_code">	: 논리 연산,
		///								"0"->unused, "1"->and, "2"->or, "3"->not,
		///								"4"->nand, "5"->nor, "6"->xor									</param>
		/// <param name="port_a">	: 첫번째 항 Port, 
		///								axis input	-> 0  ~ 3  (4  port),
		///								axis output	-> 4  ~ 5  (2  port),
		///								flag io		-> 6  ~ 15 (10 port),
		///								remote io	-> 16 ~ 31 (16 port), 
		///								rtex in		-> 32 ~ 33 (2  port),
		///								rtex out	-> 34 ~ 35 (2  port),								</param>
		/// <param name="point_a">	: 첫번째 항 Point(bit) ("0"~"31")									</param>
		/// <param name="port_b">	: 두번째 항 Port,
		/// 							axis input	-> 0  ~ 3  (4  port),
		///								axis output	-> 4  ~ 5  (2  port),
		///								flag io		-> 6  ~ 15 (10 port),
		///								remote io	-> 16 ~ 31 (16 port), 
		///								rtex in		-> 32 ~ 33 (2  port),
		///								rtex out	-> 34 ~ 35 (2  port),								</param>
		/// <param name="point_b">	: 두번째 항의 Point(bit) ("0"~"31")									</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")									</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetFlagIoLogicOperation(int net, int point_f9, out int op_code, out int port_a, out int point_a, out int port_b, out int point_b);

		/// <summary>
		/// 설정된 Flag IO 논리 연산 기능을 사용하지 않기
		/// </summary>
		/// <param name="net">	: controller id						</param>
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetClearAllFlagIoLogicOperation(int net);

		#endregion

		#region ADC / DAC 

		/*** ADC/DAC ***/
		/// ADC Channel 갯수 : 4개
		/// DAC Channel 갯수 : 2개

		/// <summary>
		/// ADC Channel 설정 ("0"~"3")
		/// </summary>
		/// <param name="net"> 		: controller id					</param>						
		/// <param name="channel">	: ad channel number("0"~"3")	</param>						
		/// <param name="range">	: "0"--> -10  V  ~  +10  V,
		///							  "1"-->  -5  V  ~   +5  V,
		///							  "2"-->  -2.5V  ~   +2.5V,
		///							  "3"-->   0  V  ~  +10  V,
		///							  "4"-->   0  V  ~   +5  V,		</param>	
		/// <param name="enable">	: "1"->사용, "0"->사용하지 않음	</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")		
		///								CommunicationEventCode, 
		///								AdcNumber	(1801),
		///								AdcRange	(1802),			</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetAdcConfig(int net, int channel, int range, int enable);

		/// <summary>
		/// ADC Channel 설정값 읽기
		/// </summary>
		/// <param name="net"> 		: controller id					</param>						
		/// <param name="channel">	: ad channel number				</param>						
		/// <param name="range">	: "0"--> -10  V  ~  +10  V,
		///							: "1"-->  -5  V  ~   +5  V,
		///							: "2"-->  -2.5V  ~   +2.5V,
		///							: "3"-->   0  V  ~  +10  V,
		///							: "4"-->   0  V  ~   +5  V,		</param>	
		/// <param name="enable">	: "1"->사용, "0"->사용하지 않음	</param>			
		/// <returns>				(see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetAdcConfig(int net, int channel, out int range, out int enable);

		/// <summary>
		/// ADC Channel 현재 값 읽기
		/// </summary>
		/// <param name="net"> 		: controller id						</param>						
		/// <param name="channel">	: ad channel number					</param>						
		/// <param name="volt">		: [mV]	읽은 전압값					</param>
		/// <param name="digit">	: [16bit] 읽은 디지탈값				</param>			
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetAdcData(int net, int channel, out int volt, out int digit);

		/// <summary>
		/// DAC Channel 설정("0"~"1")
		/// </summary>
		/// <param name="net"> 		: controller id								</param>
		/// <param name="channel">	: da channel number("0"~"1")				</param>
		/// <param name="range">	: "0"-->    0  V  ~   +5  V,
		///							: "1"-->    0  V  ~  +10  V,
		///							: "2"-->    0  V  ~ +10.8 V,
		///							: "3"-->   -5  V  ~   +5  V,
		///							: "4"-->  -10  V  ~  +10  V,
		///							: "5"-->  -10.8V  ~  +10.8V,				</param>	
		/// <param name="enable">	: "1"->사용, "0"->사용하지 않음				</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")		
		///								CommunicationEventCode, 
		///								DacNumber(1701) ~ DacDigialNumber(1721)	</returns>									
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetDacConfig(int net, int channel, int range, int enable);

		/// <summary>
		/// DAC Channel 설정값 읽기
		/// </summary>
		/// <param name="net"> 		: controller id						</param>
		/// <param name="channel">	: da channel number					</param>
		/// <param name="range">	: "0"-->    0  V  ~   +5  V,
		///							  "1"-->    0  V  ~  +10  V,
		///							  "2"-->    0  V  ~  +10.8V,
		///							  "3"-->   -5  V  ~   +5  V,
		///							  "4"-->  -10  V  ~  +10  V,
		///							  "5"-->  -10.8V  ~  +10.8V,		</param>	
		/// <param name="enable">	: "1"->사용, "0"->사용하지 않음		</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetDacConfig(int net, int channel, out int range, out int enable);

		/// <summary>
		/// DAC Channel volt값으로 출력
		/// </summary>
		/// <param name="net"> 		: controller id									</param>
		/// <param name="channel">	: da channel number								</param>
		/// <param name="volt">		: [mV] volt값으로 출력							</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")		
		///								CommunicationEventCode, 
		///								DacNumber(1701) ~ DacDigialNumber (1721),	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetDacVolt(int net, int channel, int volt);

		/// <summary>
		/// DAC Channel digital값으로 출력
		/// </summary>
		/// <param name="net"> 		: controller id									</param>
		/// <param name="channel">	: da channel number								</param>
		/// <param name="digit">	: digitial 값으로 출력							</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")		
		///								CommunicationEventCode, 
		///								DacNumber(1701) ~ DacDigialNumber (1721),	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetDacDigit(int net, int channel, int digit);

		#endregion

		#region Trigger Output - High Resolution  ( SNET-P / SNET-RTEX / SNET-EMD-L2K )

		/// <summary>
		/// "Trigger (고속 주기 출력)" Encoder를 읽을 축과 output을 내 보낼 축을 설정 하기 
		/// </summary>
		/// <param name="net"> 			: controller id								</param>
		/// <param name="encoder_axis">	: # SNET-P / SNET-EMD-L2K		
		///										-> encoder값을 읽을 축 번호 (구동할 축)
		///								  # SNET-RTEX
		///										-> 인코더 입력 커넥터 선택 
		///										-> "0(ENC1)", "1(ENC2)"				</param>
		/// <param name="output_axis">	: # SNET-P									
		///										-> Trigger 신호 출력 "축 번호"				
		///								  # SNET-RTEX
		///										-> 펄스 출력 커넥터 번호 선택 
		///										-> "0(P1) ~ 5(P6)"					
		///                               # SNET-EMD-L2K 
		///										-> 미사용     						</param>	
		/// <returns>					: (see enum "eSnetApiReturnCode")			</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetTriggerPort(int net, int encoder_axis, int output_axis);

		/// <summary>
		/// "Trigger (고속 주기 출력)" Encoder를 읽을 축과 output을 내 보낼 축을 설정값 읽기 ( SNET-P / SNET-RTEX )
		/// </summary>
		/// <param name="net"> 			: controller id								</param>
		/// <param name="encoder_axis">	: # SNET-P		
		///										-> encoder값을 읽을 축 번호 (구동할 축)
		///								  # SNET-RTEX
		///										-> 인코더 입력 커넥터 선택 
		///										-> "0(ENC1)", "1(ENC2)"				</param>
		/// <param name="output_axis">	: # SNET-P									
		///										-> Trigger 신호 출력 "축 번호"				
		///								  # SNET-RTEX
		///										-> 펄스 출력 커넥터 번호 선택 
		///										-> "0(P1) ~ 5(P6)"					</param>	
		/// <returns>					: (see enum "eSnetApiReturnCode")			</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetTriggerPort(int net, out int encoder_axis, out int output_axis);

		/// <summary>
		/// "Trigger (고속 주기 출력)" 관련 파라미터 설정 ( SNET-P / SNET-RTEX )
		/// (tip 1) "SNET-RTEX" 제품의 경우 "eSnetRtexSetTriggerParameter"를 참조 하십시요  
		/// </summary>
		/// <param name="net"> 				: controller id												</param>						
		/// <param name="pulse_count">		: 출력 갯수 												</param>						
		/// <param name="first_position">	: [um] 첫번째 Trigger 신호를 출력할 좌표  					</param>
		/// <param name="pulse_interval">	: [um] Trigger 신호 출력 "거리 Offset"				
		///										"firstPosition"설정 좌표를 기준으로 "pulseInterval" 설정 거리 
		///										마다 Trigger 신호 출력									</param>
		/// <param name="pulse_on_time">	: [msec] 	: 출력 on 유지 시간 							</param>	
		/// <returns>						: (see enum "eSnetApiReturnCode")							</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetTriggerParameter(int net, int pulse_count, int first_position, int pulse_interval, int pulse_on_time);

		/// <summary>
		/// "Trigger (고속 주기 출력)" 관련 파라미터 설정값 읽기 ( SNET-P / SNET-RTEX )
		/// (tip 1) "SNET-RTEX" 제품의 경우 "eSnetRtexGetTriggerParameter"를 참조 하십시요  
		/// </summary>
		/// <param name="net"> 				: controller id												</param>						
		/// <param name="pulse_count">		: 출력 갯수 												</param>						
		/// <param name="first_position">	: [um] 첫번째 Trigger 신호를 출력할 좌표  					</param>
		/// <param name="pulse_interval">	: [um] Trigger 신호 출력 "거리 Offset"				
		///										"firstPosition"설정 좌표를 기준으로 "pulseInterval" 설정 거리 
		///										마다 Trigger 신호 출력									</param>
		/// <param name="pulse_on_time">	: [msec] 	: 출력 on 유지 시간 							</param>	
		/// <returns>						: (see enum "eSnetApiReturnCode")							</returns>			
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetTriggerParameter(int net, out int pulse_count, out int first_position, out int pulse_interval, out int pulse_on_time);

		/// <summary>
		/// "Trigger (고속 주기 출력)" 연동 축 번호 설정 ( SNET-RTEX )
		/// (tip 1)지정된 "Rtex 드라이버"의 엔코더 출력 신호를 
		///		   "eSnetSetTriggerPort"로 지정된 제어기의 엔코더 입력 단자(ENC1 or ENC2)로 연결 합니다. 
		/// : old version name -->"eSnetSetTriggerSource"
		/// </summary>
		/// <param name="net"> 		: controller id 						</param>
		/// <param name="source">	: Rtex축 번호,"0(축0)" ~ "31(축31)"		</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexSetTriggerSource(int net, int source);

		/// <summary>
		/// "Trigger (고속 주기 출력)" "eSnetRtexSetTriggerSource" 사용자 설정값 확인 ( SNET-RTEX )
		/// : old version name -->"eSnetGetTriggerSource"
		/// </summary>
		/// <param name="net"> 		: controller id 						</param>
		/// <param name="source">	: Rtex축 번호,"0(축0)" ~ "31(축31)"		</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetTriggerSource(int net, out int source);

		/// <summary>
		/// "Trigger (고속 주기 출력)" Trigger 신호 출력 상태 정보 확인 ( SNET-P / SNET-RTEX )
		/// </summary>
		/// <param name="net"> 				: controller id											</param>						
		/// <param name="run_flag">			: "1"->trigger기능 사용 중, "0"->trigger기능 사용 안함	</param>						
		/// <param name="trigger_count">	: 현재까지 출력된 trigger 신호 갯수						</param>						
		/// <returns>						: (see enum "eSnetApiReturnCode")						</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetTriggerStatus(int net, out int run_flag, out int trigger_count);

		/// <summary>
		/// "Trigger (고속 주기 출력)" Trigger 신호 출력 기능 시작 ( SNET-P / SNET-RTEX )
		/// (tip 1) eSnetStartTrigger() 지령 후 "축 이송 명령"을 사용 하십시요 
		/// : old version name ->"eSnetSetTriggerStart"
		/// </summary>
		/// <param name="net"> 		: controller id										</param>
		/// <param name="enable">	: "1"->trigger기능 사용, "0"->trigger기능 사용 안함	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")					</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetStartTrigger(int net, int enable);

		#endregion

		#region Trigger Output - Interpolation ( SNET-P )

		/*** Trigger 출력( 보간 이송 거리 기준 ) ( SNET-P / SNET-RTEX ) ***/

		/// <summary>
		/// 지정한 2축의 벡터 합성 거리를(절대 이동 거리)누적 하여 "설정 거리" 마다 Trigger 신호를 출력 
		/// </summary>
		/// <param name="net"> 		: controller id																</param>
		/// <param name="set_array">: [array] 배열크기 ->"4",		
		///								[0]:첫번째 축 번호, 
		///								[1]:두번째 축 번호,
		///								[2]:Trigger 신호 출력 "거리 Offset" (um),
		///								[3]:"출력 ON" 유지 시간(msec),											</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		
		///								CommunicationEventCode, 
		///								InterpolTriggerNoSetAxis (2601)(축번호 설정 오류),
		///								InterpolTriggerDistance	 (2602)(첫번째 축번호와 두번째 축번호가 같을 경우,
		///								InterpolTriggerTime		 (2603)(거리 설정 오류),
		///								InterpolTriggerTime		 (2604)(출력 ON 유지 시간 설정 오류,			</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetInterpolTriggerConfig(int net, out int set_array);

		/// <summary>
		/// <summary>
		/// "eSnetSetBoganTrigger" 설정값 확인 
		/// </summary>
		/// <param name="net"> 		: controller id						</param>
		/// <param name="getarray">	: [array] 배열크기 ->"4",			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetInterpolTriggerConfig(int net, out int get_array);

		/// <summary>
		/// "보간 이송 거리 기준 Trigger 출력" 기능 시작 하기 
		/// (tip 1) 사용전 "eSnetSetInterpolTriggerConfig()"를 이용하여 적절한 파라미터 설정 후 사용 하십시요 
		/// (tip 2) "보간 이송 벡터 합성 누적 거리가" 초기화 됩니다. 
		/// : old version name ->"eSnetSetInterpolTriggerStart"
		/// </summary>
		/// <param name="net"> 		: controller id						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetStartInterpolTrigger(int net);

		/// <summary>
		/// "보간 이송 거리 기준 Trigger 출력" 기능 중지  
		/// (tip 1) "eSnetSetInterpolTriggerConfig()"를 사용하여 설정한 값은 유지 됩니다. 
		/// (tip 2) "보간 이송 벡터 합성 누적 거리가" 초기화 됩니다. 
		/// : old versin name ->"eSnetSetInterpolStop"
		/// </summary>
		/// <param name="net"> 		: controller id						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetStopInterpolTrigger(int net);

		/// <summary>
		/// "보간 이송 거리 기준 Trigger 출력" 동작 상태 정보 읽기 
		/// (tip 1) 이송 속도가 빠르거나 트리거 신호 출력 거리 간격이 짧고, 트리거 신호 출력 시간이 긴 경우 
		///			트리거 출력 신호가 OFF 되지 못하고 계속 ON 상태가 될 수 있습니다. 
		///			이 상태가 되면 "getarray[1]" 과 "getarray[2]"의 값이 달라집니다. 
		/// </summary>
		/// <param name="net"> 		: controller id									</param>
		/// <param name="get_array" : [array] 배열크기 ->"5",						
		///								[0]:동작 상태,"1"->동작 중, "0"->중지 상태		 
		///								[1]:Trigger 신호 출력 횟수, 
		///								[2]:Trigger 신호 실제 출력 횟수, 
		///								[3]:보간 이송 누적 거리 [um], 
		///								[4]:Trigger 신호 출력 거리,					</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")				</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetInterpolTriggerStatus(int net, out int get_array);

		#endregion

		#region Trigger Output - Acyclic 
		/// <summary>
		/// "eSnetTriggerOnlyAbs()" 함수 사용 전 트리거 출력 채널 번호, 출력 시간, 축 번호, 출력 극성, 기준 위치 소스 의 종류를 설정하기
		/// : old version name -->"eSnetTriggerSetTimeLevel"
		///
		/// (tip 1) "SNET-ECAT" 제품 사용자는 "eSnetSetTriggerTimeLevelEx()"를 사용 하십시요  
		/// (tip 2) 채널별 트리거 신호 출력 접점은 다음과 같습니다. (사용자 메뉴얼 참조)
		///         SNET-P4 : "User IO 커넥터" 17번핀(채널0), 18번핀(채널1)
		///			SNET-P8 : "User IO 커넥터(0 ~ 3축 모듈)" 17번핀(채널0), 18번핀(채널1)  
		///					  "User IO 커넥터(4 ~ 7축 모듈)" 17번핀(채널2), 18번핀(채널3)	
		///			SNET-RTEX: RTEX 드라이버(axis 인자로 지정된 축) "X4 커넥터 SO2" 
		///			SNET-EMD-L2K: Y0.0(채널0) ~ Y0.3(채널3)			
		/// </summary>
		/// <param name="net">		: 제어기 ip address									</param>
		/// <param name="channel">	: 트리거 출력 채널 번호	
		///							  SNET-P4: 0 ~ 1, 그외 다른 제품: 0 ~ 3				</param>
		///								
		/// <param name="axis">		: Trigger 신호 출력 연동 축 번호 						</param>
		/// <param name="time">		: [msec] "출력 ON" 유지 시간							</param>
		/// <param name="level">	: 출력 신호 극성,"0"-> Normal Open(A접점), "1" -> Normal Close(B 접점)	</param>
		/// <param name="mode">		: 좌표 소스 선택,"0"-> Actual position, "1"-> Command Position)			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")							
		///								CommunicationCodeEvent, 
		///								TriggerChannelOverCount		(2101),
		///								TriggerOutPositionOverCount (2102),									</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetTriggerTimeLevel(int net, int channel, int axis, int time, int level, int mode);

		/// <summary>
		/// "eSnetSetTriggerTimeLevel()" 설정값 읽기 
		/// : old version name -->"eSnetTriggerGetTimeLevel"
		/// </summary>
		/// <param name="net">		: 제어기 ip address									</param>
		/// <param name="channel">	: 트리거 출력 채널 번호								</param>
		/// <param name="axis">		: Trigger 신호 출력 연동 축 번호 						</param>
		/// <param name="time">		: [msec] "출력 ON" 유지 시간							</param>
		/// <param name="level">	: 출력 신호 극성,"0"-> Normal Open(A접점), "1" -> Normal Close(B 접점)	</param>
		/// <param name="mode">		: 좌표 소스 선택,"0"-> Actual position, "1"-> Command Position)			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")							
		///								CommunicationCodeEvent, 
		///								TriggerChannelOverCount		(2101),
		///								TriggerOutPositionOverCount (2102),									</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetTriggerTimeLevel(int net, int channel, out int axis, out int time, out int level, out int mode);

		/// <summary>
		/// 비주기 Trigger 설정 값 쓰기
		/// (tip 1) "remote i/o 모듈(SNET-EMD-L2K제품외 기타 제품)" 또는 "MCU-EL/E 확장 i/o 모듈(SNET-EMD-L2K)" 의 출력 접점 
		///         에서 트리거 신호가 출력 됩니다. 
		/// </summary>
		/// <param name="net">		: 제어기 ip address											</param>
		/// <param name="channel">	: 트리거 출력 채널 번호										</param>
		/// <param name="axis">		: Trigger 신호 출력 연동 축 번호								</param>
		/// <param name="port">		: Trigger 신호를 출력할 "remote io 모듈 id "					
		///							  SNET-EMD-L2K -> "0 or 1"
		///							  SNET-P/SNET-RTEX/SNET-ECAT : "0 ~ 15" 					</param>
		/// <param name="point">	: Trigger 신호 출력 접점 번호(bit)							</param>
		/// <param name="time">		: [msec] "출력 ON" 유지 시간</param>
		/// <param name="level">	: 출력 신호 극성,"0"-> Normal Open(A접점), "1" -> Normal Close(B 접점)</param>
		/// <param name="mode">		: 좌표 소스 선택,"0"-> Actual position, "1"-> Command Position)</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")							
		///								CommunicationCodeEvent, 
		///								TriggerChannelOverCount		(2101),
		///								TriggerOutPositionOverCount (2102),	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetTriggerTimeLevelEx(int net, int channel, int axis, int port, int point, int time, int level, int mode);

		/// <summary>
		/// 비주기 Trigger 설정 값 읽기 
		/// </summary>
		/// <param name="net">		: 제어기 ip address											</param>
		/// <param name="channel">	: 트리거 출력 채널 번호										</param>
		/// <param name="axis">		: Trigger 신호 출력 연동 축 번호								</param>
		/// <param name="port">		: Trigger 신호를 출력할 "remote io 모듈 id "					</param>
		/// <param name="point">	: Trigger 신호 출력 접점 번호(bit)							</param>
		/// <param name="time">		: [msec] "출력 ON" 유지 시간</param>
		/// <param name="level">	: 출력 신호 극성,"0"-> Normal Open(A접점), "1" -> Normal Close(B 접점)</param>
		/// <param name="mode">		: 좌표 소스 선택,"0"-> Actual position, "1"-> Command Position)</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")							
		///								CommunicationCodeEvent, 
		///								TriggerChannelOverCount		(2101),
		///								TriggerOutPositionOverCount (2102),	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetTriggerTimeLevelEx(int net, int channel, out int axis, out int port, out int point, out int time, out int level, out int mode);

		/// <summary>
		/// 사용자가 지정한 위치에서 트리거 신호를 출력합니다.
		/// : old version name -->"eSnetTriggerOnlyAbs"
		/// </summary>
		/// <param name="net">				: 제어기 IP Address											</param>
		/// <param name="channel">			: 트리거 출력 채널 번호										</param>
		/// <param name="set_count">		: Trigger 신호 출력 위치 개수 (최대 50 점)					</param>
		/// <param name="set_position">		: [array] 배열 크기->setTrigCount,트리거 출력 위치(배열)	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")				
		///										CommunicationCodeEvent, 
		///										TriggerChannelOverCount		(2101),
		///										TriggerOutPositionOverCount (2102),						</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetTriggerOnlyAbs(int net, int channel, int set_count, out int set_position);

		/// <summary>
		/// 사용자가 설정한 트리거 출력 기능("eSnetSetTriggerOnlyAbs")을 해제합니다.
		/// : old version name -->"eSnetTriggerSetReset"
		/// </summary>
		/// <param name="net">		: 제어기 IP address					</param>
		/// <param name="channel">	: 트리거 출력 채널 번호				</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetResetTrigger(int net, int channel);

		#endregion

		#region Trigger Output - Cyclic ( SNET-RTEX / SNET-ECAT)

		///
		/// Trigger : SNET-RTEX / SNET-ECAT
		///

		/// <summary>
		/// "Trigger (주기 출력)" 축 번호, 접점 번호 설정 ( SNET-RTEX / SNET-ECAT )
		/// </summary>
		/// <param name="net">			: controller id 								</param>
		/// <param name="axis">			: Encoder 위치 확인 및 신호를 출력할 축 번호		</param>
		/// <param name="point">		: trigger 신호 출력 접점 번호(bit)				</param>			
		/// <returns>					: (see enum "eSnetApiReturnCode")				</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetCyclicTriggerPort(int net, int axis, int point);

		/// <summary>
		/// "Trigger (주기 출력)" 축 번호, 접점 번호 사용자 설정값 확인 ( SNET-RTEX / SNET-ECAT )
		/// </summary>
		/// <param name="net">			: controller id 								</param>
		/// <param name="axis">			: Encoder 위치 확인 및 신호를 출력할 축 번호		</param>
		/// <param name="point">		: trigger 신호 출력 접점 번호	(bit)				</param>				
		/// <returns>					: (see enum "eSnetApiReturnCode")				</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetCyclicTriggerPort(int net, out int axis, out int point);

		/// <summary>
		/// "Trigger (주기 출력)" 축 번호, I/O Module 번호, 접점 번호 설정 ( SNET-ECAT )
		/// </summary>
		/// <param name="net">			: controller id 								</param>
		/// <param name="axis">			: Encoder 위치를 확인할 축 번호						</param>
		/// <param name="port">			: trigger 신호를 출력할 I/O module 번호('axis' 인자로 지정한 축으로 출력 시 '-1' 대입)</param>
		/// <param name="point">		: trigger 신호 출력 접점 번호(bit)				</param>		
		/// <returns>					: (see enum "eSnetApiReturnCode")				</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetCyclicTriggerPortEx(int net, int axis, int port, int point);

		/// <summary>
		/// "Trigger (주기 출력)" 축 번호, I/O Module 번호, 접점 번호 사용자 설정값 확인 ( SNET-ECAT )
		/// </summary>
		/// <param name="net">			: controller id 								</param>
		/// <param name="axis">			: Encoder 위치를 확인할 축 번호						</param>
		/// <param name="port">			: trigger 신호를 출력할 I/O module 번호('axis' 인자로 지정한 축으로 출력 시 '-1')</param>
		/// <param name="point">		: trigger 신호 출력 접점 번호(bit)				</param>	
		/// <returns>					: (see enum "eSnetApiReturnCode")				</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetCyclicTriggerPortEx(int net, out int axis, out int port, out int point);

		/// <summary>
		/// "Trigger (주기 출력)" 관련 세부 파라미터 설정 ( SNET-RTEX / SNET-ECAT )
		/// </summary>
		/// <param name="net"> 				: controller id 							</param>
		/// <param name="pulse_count">		: trigger 신호 출력 개수 					</param>
		/// <param name="first_position">	: [um],trigger 신호 출력 "시작 좌표"		</param>
		/// <param name="pulse_interval">	: [um],trigger 신호 출력 "거리 간격" 		</param>
		/// <param name="pulse_time">		: [usec],trigger 신호 "출력 ON" 유지시간	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")			</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetCyclicTriggerParameter(int net, int pulse_count, int first_position, int pulse_interval, int pulse_time);

		/// <summary>
		/// "Trigger (주기 출력)" 관련 세부 파라미터 설정값 확인 ( SNET-RTEX / SNET-ECAT )
		/// </summary>
		/// <param name="net"> 				: controller id 							</param>
		/// <param name="pulse_count">		: trigger 신호 출력 개수 					</param>
		/// <param name="first_position">	: [um],trigger 신호 출력 "시작 좌표"		</param>
		/// <param name="pulse_interval">	: [um],trigger 신호 출력 "거리 간격" 		</param>
		/// <param name="pulse_time">		: [usec],trigger 신호 "출력 ON" 유지시간	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")			</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetCyclicTriggerParameter(int net, out int pulse_count, out int first_position, out int pulse_interval, out int pulse_time);

		/// <summary>
		/// "Trigger (주기 출력)" 동작 상태 확인 ( SNET-RTEX / SNET-ECAT )
		/// </summary>
		/// <param name="net"> 				: controller id 						</param>
		/// <param name="run_flag">			: "1"->동작 중,"0"->정지 상태 			</param>
		/// <param name="trigger_count">	: 현재 까지 출력한 트리거 신호 개수 	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")		</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetCyclicTriggerStatus(int net, out int run_flag, out int trigger_count);

		/// <summary>
		/// "Trigger (주기 출력)" 시작 ( SNET-RTEX / SNET-ECAT )
		/// </summary>
		/// <param name="net"> 		: controller id 					</param>
		/// <param name="enable">	: "1"->기능 시작,"0"->기능 정지		</param>
		/// <returns>				(see enum "eSnetApiReturnCode")		</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetStartCyclicTrigger(int net, int enable);

		///
		/// Trigger : SNET-RTEX
		///

		/// <summary>
		/// "Trigger (주기 출력)" 축 번호 선택 ( SNET-RTEX )
		/// (tip 1)"ENC1","ENC2","P1 ~ P6"는 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetSetTriggerPortRtex"
		/// </summary>
		/// <param name="net">			: controller id 								</param>
		/// <param name="encoder_port">	: rtex 축 번호									</param>
		/// <param name="output_port">	: Rtex 드라이버 trigger 신호 출력 접점 설정
		///									"0(SO2)" or "1(SO3)"						</param>			
		/// <returns>					: (see enum "eSnetApiReturnCode")				</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexSetTriggerPort(int net, int encoder_port, int output_port);

		/// <summary>
		/// "Trigger (주기 출력)" "eSnetRtexSetTriggerPort" 사용자 설정값 확인 ( SNET-RTEX )
		/// : old version name -->"eSnetGetTriggerPortRtex"
		/// </summary>
		/// <param name="net">			: controller id 								</param>
		/// <param name="encoder_port">	: rtex 축 번호									</param>
		/// <param name="output_port">	: Rtex 드라이버 trigger 신호 출력 접점 설정
		///									"0(SO2)" or "1(SO3)"						</param>				
		/// <returns>					: (see enum "eSnetApiReturnCode")				</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetTriggerPort(int net, out int encoder_port, out int output_port);

		/// <summary>
		/// "Trigger (주기 출력)" 관련 세부 파라미터 설정 ( SNET-RTEX )
		/// : old version name -->"eSnetSetTriggerParameterRtex"
		/// </summary>
		/// <param name="net"> 				: controller id 							</param>
		/// <param name="pulse_count">		: trigger 신호 출력 개수 					</param>
		/// <param name="first_position">	: [um],trigger 신호 출력 "시작 좌표"		</param>
		/// <param name="pulse_interval">	: [um],trigger 신호 출력 "거리 간격" 		</param>
		/// <param name="pulse_time">		: [usec],trigger 신호 "출력 ON" 유지시간	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")			</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexSetTriggerParameter(int net, int pulse_count, int first_position, int pulse_interval, int pulse_time);

		/// <summary>
		/// "Trigger (주기 출력)" "eSnetRtexSetTriggerParameter" 사용자 설정값 확인 ( SNET-RTEX )
		/// : old version name -->"eSnetGetTriggerParameterRtex"
		/// </summary>
		/// <param name="net"> 				: controller id 							</param>
		/// <param name="pulse_count">		: trigger 신호 출력 개수 					</param>
		/// <param name="first_position">	: [um],trigger 신호 출력 "시작 좌표"		</param>
		/// <param name="pulse_interval">	: [um],trigger 신호 출력 "거리 간격" 		</param>
		/// <param name="pulse_time">		: [usec],trigger 신호 "출력 ON" 유지시간	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")			</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetTriggerParameter(int net, out int pulse_count, out int first_position, out int pulse_interval, out int pulse_time);

		/// <summary>
		/// "Trigger (주기 출력)" 동작 상태 확인 ( SNET-RTEX )
		/// : old version name -->"eSnetGetTriggerStatusRtex"
		/// </summary>
		/// <param name="net"> 				: controller id 						</param>
		/// <param name="run_flag">			: "1"->동작 중,"0"->정지 상태 			</param>
		/// <param name="trigger_count">	: 현재 까지 출력한 트리거 신호 개수 	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")		</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetTriggerStatus(int net, out int run_flag, out int trigger_count);

		/// <summary>
		/// "Trigger (주기 출력)" 시작 ( SNET-RTEX )
		/// : old version name -->"eSnetSetTriggerStartRtex"
		/// </summary>
		/// <param name="net"> 		: controller id 					</param>
		/// <param name="enable">	: "1"->기능 시작,"0"->기능 정지		</param>
		/// <returns>				(see enum "eSnetApiReturnCode")		</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexStartTrigger(int net, int enable);

		#endregion

		#region Torque Limit (SNET-RTEX)

		/// <summary>
		/// RTEX Driver "토오크 제한값" 변경 하기 
		/// (tip 1) RTEX 드라이버 "파라미터 Pr0.13"를 변경 합니다. -> 단 Eeprom에는 저장 되지 않습니다. 
		/// </summary>
		/// <param name="net"> 		: controller index 					</param>
		/// <param name="axis"> 	: 축 번호   						</param>
		/// <param name="settrq">	: 토오크 제한값,[%]					</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexSetTrqLimit(int net, int axis, int settrq);

		/// <summary>
		/// RTEX Driver "토오크 제한값"설정값 확인 
		/// (tip 1) RTEX 드라이버 "파라미터 Pr0.13"설정값을 확인 합니다. 
		/// </summary>
		/// <param name="net">		: controller index 					</param>
		/// <param name="axis"> 	: 축 번호   						</param>
		/// <param name="gettrq">	: 토오크 제한 설정값,[%]			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetTrqLimit(int net, int axis, out int gettrq);

		/// <summary>
		/// "RTEX Driver" 현재 토오크값 확인  
		/// (tip 1) 축별 실시간 토오크 값을 확인 합니다. 단위 0.1[%]( 123 -> 12.3 % )
		/// </summary>
		/// <param name="net"> 		: controller index 					</param>
		/// <param name="axis"> 	: 축 번호   						</param>
		/// <param name="curtrq">	: 실시간 토오크값,[0.1%]  			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetCurTrq(int net, int axis, out int curtrq);

		/// <summary>
		/// 지정된 목표위치로 단축 이송 단 이송 중 토오크 리밋이 설정 시간 이상 동안 유지 되면 이송 정지 (SNET-RTEX)
		/// (tip 1) 
		/// </summary>
		/// <param name="net"> 		: controller index 						</param>
		/// <param name="axis"> 	: 축 번호   							</param>
		/// <param name="pos"> 		: 이송 위치,[um]   						</param>
		/// <param name="vel"> 		: 이송 속도,[mm/min]					</param>
		/// <param name="accel"> 	: 가속시간,[msec]						</param>
		/// <param name="decel"> 	: 감속시간,[msec]						</param>
		/// <param name="jerk"> 	: jerk,[%]								</param>
		/// <param name="time"> 	: 축 이송 중 설정 시간동안 "토오크 제한",
		///							  상태가 유지 되면 정지 ,[msec]			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexMoveAxisUntilTrqLimit(int net, int axis, int pos, int vel, int accel, int decel, int jerk, int time);

        /// <summary>
        /// "eSnetRtexMoveAxisUntilTrqLimit" 지령 후 동작 상태를 확인 합니다. 
        /// (tip 1)
        /// </summary>
        /// <param name="net"> 		: controller index 					</param>
        /// <param name="axis"> 	: 축 번호   							</param>
        /// <param name="status">	: 현재 실행 상태   					</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetRtexGetStatusMoveUntilTrqLimit(int net, int axis, out int status);

        /// <summary>
        ///  첫번째 토오크 제한 설정값을 적용 하고 첫번째 설정 위치로 이송 후 설정 시간 동안 대기 하고 
        ///  두번째 토오크 제한 설정값을 적용 하고 두번째 설정 위치로 이송 합니다. 
        ///  (tip 1) 서보 프레스 어플리케이션에 사용하면 유용 합니다. 
        ///      settrq_1 : 축 하강시 토오크 제한값  
        ///      pos_1	  : 축 하강 목표 위치  
        ///      time     : Press 시간   
        ///      settrq_2 : 축 상승시 토오크 제한값  
        ///      pos_2	  : 축 상승 목표 위치  
        ///  (tip 2) 시퀀스의 실행 상태는 "eSnetRtexGetStatusTrqLimitAndMove()"함수를 사용하여 확인 할 수 있습니다. 
        ///
        /// <param name="net"> 		: controller index 						</param>
        /// <param name="axis"> 	: 축 번호   								</param>
        /// <param name="settrq_1"> : 첫번째 토오크 제한 설정값 				</param>
        /// <param name="settrq_2"> : 두번째 토오크 제한 설정값 				</param>
        /// <param name="time"> 	: 첫번째 이송 완료 후 대기 시간[msec]     </param>
        /// <param name="pos_1"> 	: 첫번째 이송 위치 [um]					</param>
        /// <param name="pos_2"> 	: 두번째 이송 위치,[um]  					</param>
        /// <param name="vel_1"> 	: 첫번째 이송 속도,[mm/min]				</param>
        /// <param name="vel_2"> 	: 두번째 이송 속도,[mm/min]				</param>
        /// <param name="accel"> 	: 가속시간,[msec]						</param>
        /// <param name="decel"> 	: 감속시간,[msec]						</param>
        /// <param name="jerk"> 	: jerk,[%]								</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")		</returns>	
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetRtexSetTrqLimitAndMove(int net, int axis, int settrq_1, int settrq_2, int time,
                                                             int pos_1, int pos_2, int vel_1, int vel_2, int accel, int decel, int jerk);

        /// <summary>
        /// "eSnetRtexSetTrqLimitAndMove" 지령 후 동작 상태를 확인 합니다. 
        ///
        /// </summary>
        /// <param name="net"> 		: controller index 					</param>
        /// <param name="axis"> 	: 축 번호   							</param>
        /// <param name="step">		: 현재 실행 중인 시퀀스 번호 			</param>
        /// <param name="status">	: 현재 실행 상태   					</param>
        ///							  bit 0 : "1"-> "eSnetRtexSetTrqLimitAndMove" 동작 중 
        ///							  bit 1 : "1"-> "eSnetRtexSetTrqLimitAndMove" 동작 완료(종료 상태) 
        ///							  bit 2 : "1"-> 시퀀스 실행중 축 이송 전 정지 상태가 아님 
        ///							  bit 3 : "1"-> 시퀀스 실행중 축 이송 전 축 알람 발생 상태 
        ///							  bit 4 : "1"-> 시퀀스 실행중 축 이송 전 지정 축이 연속 보간에서 사용 중 
        ///							  bit 5 : "1"-> 시퀀스 실행중 축 이송 전 지정 축이 동기 운전의 슬레이브 축으로 사용중 
        ///							  bit 6 : "1"-> 시퀀스 실행중 MPG 모드로 전환됨 	</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetRtexGetStatusTrqLimitAndMove(int net, int axis, out int step, out int status);

        /// <summary>
        ///  "eSnetRtexSetTrqLimitAndMove" 시퀀스 실행을 종료 합니다. 
        /// <param name="net"> 		: controller index 						</param>
        /// <param name="axis"> 	: 축 번호   								</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")		</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetRtexResetTrqLimitAndMove(int net, int axis);

        #endregion

        #region Position Capture 1 (SNET-P / SNET-RTEX)

        /*** Position Capture - High Resolution(1 microsecond capture) ( SNET-P / SNET-RTEX ) ***/

        /// <summary>
        /// capture 기능 설정 하기
        /// </summary>
        /// <param name="net"> 			: controller id													</param>						
        /// <param name="channel">		: 3개까지 capture기능 설정을 할수 있음("0"~'2")				 	</param>						
        /// <param name="enable">		: "1"->이 채널을 사용함,"0"->이 채널을 사용 안함 			 	</param>						
        /// <param name="encoder_axis">	: encoder를 읽을(구동할) 축									 	</param>
        ///									==> SNET-RTEX 일경우 "0(ENC1)" or "1(ENC2)"					
        /// <param name="port">			: input sensor가 연결된 io port									</param>
        ///									==> SNET-RTEX 일경우 "0(INP1)" ~ "5(INP6)"
        /// <param name="point">		: input sensor가 연결된 io port에서 몇번째 bit인가? ("0"~"31") 	</param>
        ///									==> SNET-RTEX 일경우 "0(bit0)" ~ "7(bit7)"	
        /// <returns>					: (see enum "eSnetApiReturnCode")								</returns>	
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetCaptureConfig(int net, int channel, int enable, int encoder_axis, int port, int point);

        /// <summary>
        /// capture 기능 설정값 읽기
        /// </summary>
        /// <param name="net"> 			: controller id													</param>						
        /// <param name="channel">		: 3개까지 capture기능 설정을 할수 있음("0"~"2")				 	</param>						
        /// <param name="state">		: "0": 위치 캡처 disable 상태, "1": 위치 캡처 enable 상태
        ///								  "2": 위치 캡처 실행 중, "3": 위치 캡처 정지 상태					</param>
        /// <param name="encoder_axis">	: encoder를 읽을(구동할) 축									 	</param>
        ///									==> SNET-RTEX 일경우 "0(ENC1)" or "1(ENC2)"					
        /// <param name="port">			: input sensor가 연결된 io port									</param>
        ///									==> SNET-RTEX 일경우 "0(INP1)" ~ "5(INP6)"
        /// <param name="point">		: input sensor가 연결된 io port에서 몇번째 bit인가? ("0"~"31")	</param>
        ///									==> SNET-RTEX 일경우 "0(bit0)" ~ "7(bit7)"	
        /// <returns>					: (see enum "eSnetApiReturnCode")								</returns>	
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetCaptureConfig(int net, int channel, out int state, out int encoder_axis, out int port, out int point);

		/// <summary>
		/// capture 기능 시작 하기
		/// : old version name ->"eSnetSetCaptureStart"
		/// </summary>
		/// <param name="net"> 			: controller id											</param>						
		/// <param name="channel">		: 3개까지 capture기능 설정을 할수 있음("0"~'2")			</param>						
		/// <param name="operate">		: "1"->caputre기능 실행, "0"->capture기능 실행 안함		</param>						
		/// <param name="set_count">	: encoder위치를 capture할 갯수							</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")						</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetStartCapture(int net, int channel, int operate, int set_count);

        /// <summary>
        /// capture 기능 실행할 때 상태 읽기
        /// </summary>
        /// <param name="net"> 				: controller id										</param>						
        /// <param name="channel">			: 3개까지 capture기능 설정을 할수 있음("0"~'2")		</param>						
        /// <param name="state">			: "0": 위치 캡처 disable 상태, "1": 위치 캡처 enable 상태
        ///									  "2": 위치 캡처 실행 중, "3": 위치 캡처 정지 상태		</param>
        /// <param name="set_count">		: capture 설정 횟수 									</param>
        /// <param name="rising_count">		: risigin edge position capture한 갯수				</param>
        /// <param name="falling_count">	: falling edge position capture한 갯수				</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")					</returns>	
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetCaptureStatus(int net, int channel, out int state, out int set_count, out int rising_count, out int falling_count);

		/// <summary>
		/// capture 기능 Capture된 위치값 얻기
		/// </summary>
		/// <param name="net"> 				: controller id									</param>						
		/// <param name="axis">			: 3개까지 capture기능 설정을 할수 있음("0"~'2")	</param>						
		/// <param name="index">			: position값을 읽을 index						</param>						
		/// <param name="rising_position">	: [um] : index에 해당하는 rising position		</param>
		/// <param name="falling_position">	: [um] : risigin 해당하는 falling position		</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")				</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetCapturePosition(int net, int axis, int index, out int rising_position, out int falling_position);

		#endregion

		#region Position Capture 2 (SNET-RTEX / SNET-ECAT)

		/*** Position Capture 2 - Fieldbus Controller ( SNET-RTEX / SNET-ECAT ) ***/

		//
		// SNET-RTEX / SNET-ECAT
		//

		/// <summary>
		/// "Position Capture 2" Encoder 위치 캡처할 축 및 접점 설정 ( SNET-ECAT )
		/// </summary>
		/// <remarks>
		/// * SNET-ECAT에서 해당 API로 축 입력 접점(bit) 설정 시 CiA402 Object 중 Digital Inputs(index: 0x60FD:00)의 Bit 16 ~ 31를 이용한다.
		/// </remarks>
		/// <param name="net"> 			: controller id 					</param>
		/// <param name="axis">			: 축 번호							</param>
		/// <param name="port">			: 캡처 입력 신호를 받을 I/O module 번호. '-1' 대입 시 축 입력 접점으로부터 신호 확인</param>
		/// <param name="point">		: 캡처 입력 신호를 받을 입력 접점(bit)</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetFieldbusCapture(int net, int axis, int port, int point);

		/// <summary>
		/// "Position Capture 2" Encoder 위치 캡처할 축 및 접점 설정 읽기 ( SNET-ECAT )
		/// </summary>
		/// <param name="net"> 			: controller id 					</param>
		/// <param name="axis">			: 축 번호							</param>
		/// <param name="port">			: 캡처 입력 신호를 받을 I/O module 번호. '-1' 설정 시 축 입력 접점으로부터 신호 확인</param>
		/// <param name="point">		: 캡처 입력 신호를 받을 입력 접점(bit)</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetFieldbusCapture(int net, int axis, out int port, out int point);

		/// <summary>
		/// "Position Capture 2" 파라미터 설정 및 기능 시작/정지 ( SNET-RTEX / SNET-ECAT )
		/// </summary>
		/// <remarks>
		/// * 별도 설정이 없으면, 축 입력 접점의 신호 감지 시 Encoder 위치를 캡처한다.
		/// * 'SetFieldbusCapture' API를 이용하면, I/O module의 입력 접점 신호 감지 시, Encoder 위치를 캡처한다(SNET-ECAT).
		/// * SNET-RTEX의 경우, 축 입력 접점 bit 0에서만 입력 신호를 감지하도록 고정되어 있다.
		/// </remarks>
		/// <param name="net"> 			: controller id 					</param>
		/// <param name="axis">			: 축 번호							</param>
		/// <param name="enable">		: "1"->시작, "0"->중지  				</param>
		/// <param name="set_count">	: Capture 횟수(최대 50)				</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetStartFieldbusCapture(int net, int axis, int enable, int set_count);

		/// <summary>
		/// "Position Capture 2" 동작 상태 확인 ( SNET-RTEX / SNET-ECAT )
		/// </summary>
		/// <param name="net"> 				: controller id 					</param>
		/// <param name="axis">				: 축 번호							</param>
		/// <param name="enable">			: "1"->실행 중, "0"->중지 상태 		</param>
		/// <param name="set_count">		: Capture 횟수 "설정 값"  			</param>
		/// <param name="rising_count">		: Rising Edge 에서 캡처된 횟수  		</param>
		/// <param name="falling_count">	: Falling Edge 에서 캡처된 횟수  	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetFieldbusCaptureStatus(int net, int axis, out int enable, out int set_count, out int rising_count, out int falling_count);

		/// <summary>
		/// "Position Capture 2" 캡처된 좌표 읽기 ( SNET-RTEX / SNET-ECAT )
		/// </summary>
		/// <param name="net"> 				: controller id 										</param>
		/// <param name="axis">				: 축 번호												</param>
		/// <param name="index">			: 캡처 순번 												</param>
		/// <param name="rising_position">	: 인자 "index"순번에서 "rising edge" 일때 캡처된 좌표   	</param>
		/// <param name="falling_position">	: 인자 "index"순번에서 "falling edge" 일때 캡처된 좌표	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")					</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetFieldbusCapturePosition(int net, int axis, int index, out int rising_position, out int falling_position);

		//
		// SNET-RTEX
		//

		/// <summary>
		/// "Position Capture 2" 파라미터 설정 및 기능 시작/정지 (SNET-RTEX)
		/// : old version name -->"eSnetSetCaptureStartRtex"
		/// </summary>
		/// <param name="net"> 			: controller id 					</param>
		/// <param name="axis">			: rtex 축 번호,"0 ~ 31"				</param>
		/// <param name="enable">		: "1"->시작, "0"->중지  			</param>
		/// <param name="set_count">	: Capture 횟수   					</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexStartCapture(int net, int axis, int enable, int set_count);

		/// <summary>
		/// "Position Capture 2" 동작 상태 확인 (SNET-RTEX)
		/// : old version name -->"eSnetGetCaptureStatusRtex"
		/// </summary>
		/// <param name="net"> 				: controller id 					</param>
		/// <param name="axis">				: rtex 축 번호,"0 ~ 31"				</param>
		/// <param name="enable">			: "1"->실행 중, "0"->중지 상태 		</param>
		/// <param name="set_count">		: Capture 횟수 "설정 값"  			</param>
		/// <param name="rising_count">		: Rising Edge 에서 캡처된 횟수  	</param>
		/// <param name="falling_count">	: Falling Edge 에서 캡처된 횟수  	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetCaptureStatus(int net, int axis, out int enable, out int set_count, out int rising_count, out int falling_count);

		/// <summary>
		/// "Position Capture 2" 캡처된 좌표 읽기 (SNET-RTEX)
		/// : old version name -->"eSnetGetCapturePositionRtex"
		/// </summary>
		/// <param name="net"> 				: controller id 									</param>
		/// <param name="axis">				: rtex 축 번호,"0 ~ 31"								</param>
		/// <param name="index">			: 캡처 순번 										</param>
		/// <param name="rising_position">	: "index"순번에서 "rising edge" 일때 캡처된 좌표   	</param>
		/// <param name="falling_position">	: "index"순번에서 "falling edge" 일때 캡처된 좌표   </param>
		/// <returns>						: (see enum "eSnetApiReturnCode")					</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetCapturePosition(int net, int axis, int index, out int rising_position, out int falling_position);

		#endregion

		#region Alarm & Warning (SNET-RTEX)

		/// <summary>
		/// RTEX 드라이버에서 Alarm and Warning Code 읽기 
		/// </summary>
		/// <param name="net"> 			: controller id						</param>						
		/// <param name="axis">		    : 축 번호(rtex축)					</param>						
		/// <param name="alarm_main">	: Alarm Code Main					</param>
		/// <param name="alarm_sub">	: Alarm Code Sub					</param>
		/// <param name="warning_main">	: warning Code Main					</param>
		/// <param name="warning_sub">	: warning Code Sub					</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetAlarmWarning(int net, int axis, out int alarm_main, out int alarm_sub, out int warning_main, out int warning_sub);

		#endregion

		#region Driver Information (SNET-RTEX)
		/// <summary>
		/// RTEX 드라이버에 "Monitor Command" 요청 
		/// </summary>
		/// <param name="net"> 			: controller id											</param>						
		/// <param name="axis">		    : 축 번호(rtex축)										</param>						
		/// <param name="type_code">	: 항목 번호 (RTEX 드라이버 메뉴얼 참조-Monitor command)	</param>
		/// <param name="index">		: 항목 세부 번호	(RTEX 드라이버 메뉴얼 참조)				</param>
		/// <param name="information">	: 드라이버에서 읽은 정보 									</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetMonitorCommand(int net, int axis, int type_code, int index, out int information);
		#endregion

		#region Driver System Id (SNET-RTEX)
		/// <summary>
		/// RTEX 드라이버에 "System ID Command" 요청 
		/// </summary>
		/// <param name="net"> 			: controller id												</param>
		/// <param name="axis">		    : 축 번호(rtex축)											</param>
		/// <param name="type_code">	: 항목 번호 (RTEX 드라이버 메뉴얼 참조- System Id command)		</param>
		/// <param name="index">		: 항목 세부 번호	(RTEX 드라이버 메뉴얼 참조)					</param>
		/// <param name="information">	: 드라이버에서 읽은 정보 										</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>			
		[DllImport("EMotionSnetDeviceEx.dll",CallingConvention=CallingConvention.Cdecl)]
		public static extern int eSnetRtexGetSystemIdCommand(int net, int axis, int type_code, int index, out int information);
		#endregion

		#region Positional Command Filter

		/// <summary>
		/// 지령 필터 설정  
		///	(tip 1) 모든 축이 "정지 상태"일때 사용해야 됩니다. 
		/// (tip 2) "연속 보간 이송"중 에만 필터가 적용 됩니다. 
		/// </summary>
		/// <param name="net"> 		: controller id						</param>						
		/// <param name="enable">	: 필터 적용 유무 						
		///								"1"->enable, "0"->disable		</param>
		/// <param name="para">		: 지연 파라미터    					
		///								최대 200(msec)					</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetCmdFilterConfig(int net, int enable, int para);

		/// <summary>
		/// 스무딩 필터 설정(eSnetSetCmdFilterConfig) 확인 
		/// </summary>
		/// <param name="net"> 		: controller id						</param>						
		/// <param name="enable">	: 필터 적용 유무 						
		///								"1"->enable, "0"->disable		</param>
		/// <param name="para">		: 지연 파라미터    					
		///								최대 200(msec)					</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>			
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetCmdFilterConfig(int net, out int enable, out int para);
		#endregion

		#region Linear Compensation

		/// <summary>
		/// 축 좌표 보정 테이블 설정  
		/// (tip 1) "축 상대 좌표 모드(eSnetSetAxisAbsRelMode(..1)" 에서는 사용 금지 
		/// (tip 2) "eSnetSetPositionCompensationEnable" 함수로 기능을 "enable" 상태로 만들면 "jog"이송을 제외한 "이송 명령 지령"시
		///	  보정값이 적용된 "지령 위치(command position)가 제어기로 전달 됩니다. 
		///	  
		/// </summary>
		/// <param name="net"> 				: controller id									</param>						
		/// <param name="axis">				: 축 번호										</param>
		/// <param name="count">			: 보정 테이블 갯수: "2 ~ 512"					</param>
		/// <param name="start_position">	: 보정 테이블 위치 offfset						</param>
		/// <param name="position">			: [array], 배열크기-> count	
		///										보정 테이블 좌표 [um]		
		///										pos[0]은 항상"0"을 설정 하십시요			</param>
		/// <param name="correction">		: [array], 배열크기-> count		
		///										보정 테이블 보정 좌표 [um]		
		///										correction[0]는 항상"0"을 설정 하십시요  	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")				</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetPositionCompensation(int net, int axis, int count, int start_position, out int position, out int correction);

		/// <summary>
		/// "eSnetSetPositionCompensation" 설정값 및 "enable"상태 확인 
		/// </summary>
		/// <param name="net"> 				: controller id						</param>						
		/// <param name="axis">				: 축 번호							</param>						
		/// <param name="enable">			: "1"-> enable 상태 					</param>
		/// <param name="count">			: 보정 테이블 갯수					</param>
		/// <param name="start_position">	: 보정 테이블 위치 offfset			</param>
		/// <param name="position">			: [array], 배열크기-> count	
		///										보정 테이블 좌표 		[um]	</param>
		/// <param name="correction">		: [array], 배열크기-> count		
		///										보정 테이블 보정 좌표 [um]		</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetPositionCompensation(int net, int axis, out int enable, out int count, out int start_position, out int position, out int correction);

		/// <summary>
		/// 축 보정 테이블 적용 실행 및 금지 
		/// (tip 1) "eSnetSetPositionCompensation" 함수로 보정 테이블을 설정하고 "enable" 상태가 되면 이후 "jog"이송을 제외한 
		///			"이송 명령 지령"시 보정값이 적용된 "지령 위치(command position)가 제어기로 전달 됩니다. 
		/// </summary>
		/// <param name="net"> 		: controller id						</param>						
		/// <param name="axis">		: 축 번호							</param>						
		/// <param name="enable">	: "1"-> enable, "0"-> disable		</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetPositionCompensationEnable(int net, int axis, int enable);

		/// <summary>
		/// "eSnetSetPositionCompensationEnable" 확인 
		/// </summary>
		/// <param name="net"> 		: controller id						</param>						
		/// <param name="axis">		: 축 번호							</param>						
		/// <param name="enable">	: "1"-> enable, "0"-> disable		</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetPositionCompensationEnable(int net, int axis, out int enable);

		/// <summary>
		/// 보정 테이블이 적용된 좌표 결과 확인 하기 
		/// (tip 1) "eSnetSetPositionCompensation" 함수를 사용하여 보정 테이블을 설정하고 "축 이송 없이" 특정 좌표에 대한 보정 결과
		///        를 확인하고 싶을때 사용 합니다. 
		/// </summary>
		/// <param name="net"> 						: controller id						</param>						
		/// <param name="axis">						: 축 번호							</param>						
		/// <param name="command_position">			: 목표 좌표							</param>
		/// <param name="compensation_position">	: 보정 테이블이 적용된 return 좌표	</param>
		/// <returns>								: (see enum "eSnetApiReturnCode")	</returns>	
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetPositionCompensationResult(int net, int axis, int command_position, out int compensation_position);

		/// <summary>
		/// "축 좌표 보정" 기능 사용시 "보정값이 적용되지 않은 Command Position(절대 좌표)"과  
		/// "보정값이 적용된 실제 Command Position(절대 좌표)"을 확인 합니다.  
		/// </summary>
		/// <param name="net"> 			: controller id 										</param>						
		/// <param name="axis">			: axis number											</param>						
		/// <param name="real_position">: 보정값이 적용된 실제 제어기 command position, [um]	</param>
		/// <param name="position">		: 보정값이 적용되지 않은 command position, [um]			</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")						</returns>					
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetCommandPositionCompensation(int net, int axis, out int real_position, out int position);

		/// <summary>
		/// "축 좌표 보정" 기능 사용시 "보정값이 적용되지 않은 Actual Position(기계 좌표)"와 
		/// "보정값이 적용된 실제 Actual Position(기계 좌표)"를 확인 합니다. 
		/// </summary>
		/// <param name="net"> 			: controller id 									</param>						
		/// <param name="axis">			: axis index										</param>						
		/// <param name="real_position">: 보정값이 적용된 실제 제어기 Actual position, [um]	</param>
		/// <param name="position">		: 보정값이 적용되지 않은 Actual position ,[um]		</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")					</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetActualPositionCompensation(int net, int axis, out int real_position, out int position);

		#endregion

		#region Interrupt Event

		/// <summary>
		/// Interrupt event table 특정 번호의 설정 정보 쓰기
		/// </summary>
		/// <remarks>
		/// Interrupt event table의 번호는 0 ~ 127 설정이 가능
		/// </remarks>
		/// <param name="net">			: Network id(IPv4 4th address)			</param>
		/// <param name="table_index">	: 설정을 쓸 Interrupt event table 번호	</param>
		/// <param name="enable">		: 'table_index' 에 해당하는 Interrupt event table 활성화 유무. 1: 활성화, 0: 비활성화				</param>
		/// <param name="info">			: 'table_index' 에 해당하는 Interrupt event table 설정 정보. struct 'InterruptEventTableInfo' 참조	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")		</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetInterruptEventTable(int net, int table_index, int enable, InterruptEventTableInfo info);

		/// <summary>
		/// Interrupt event table 특정 번호의 설정 정보 읽기
		/// </summary>
		/// <param name="net">			: Network id(IPv4 4th address)				</param>
		/// <param name="table_index">	: 설정을 읽을 Interrupt event table 번호	</param>
		/// <param name="enable">		: 'table_index' 에 해당하는 Interrupt event table 활성화 유무. 1: 활성화, 0: 비활성화				</param>
		/// <param name="info">			: 'table_index' 에 해당하는 Interrupt event table 설정 정보. struct 'InterruptEventTableInfo' 참조	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")			</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetInterruptEventTable(int net, int table_index, out int enable, out InterruptEventTableInfo info);

		/// <summary>
		/// Interrupt event table 특정 번호 삭제
		/// </summary>
		/// <param name="net">			: Network id(IPv4 4th address)		</param>
		/// <param name="table_index">	: Interrupt event table 번호		</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetEraseInterruptEventTable(int net, int table_index);

		/// <summary>
		/// Interrupt event table 전체 번호 삭제
		/// </summary>
		/// <param name="net">			: Network id(IPv4 4th address)		</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetClearInterruptEventTable(int net);

		/// <summary>
		/// Interrupt event handler(callback function) 등록
		/// </summary>
		/// <remarks>
		/// Interrupt event 발생 시, 자동으로 호출될 Function pointer(handler)를 등록한다.
		/// 이벤트가 발생하여 등록된 Function 호출 시, 'table_index' 인자로 발생한 Interrupt event table 번호가 전달된다
		/// </remarks>
		/// <param name="net">			: Network id(IPv4 4th address)		</param>
		/// <param name="function">		: Interrupt event 발생 시, 호출 될 Function(event handler)	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetInterruptEventFunction(int net, InterruptEventHandler function);

		/// <summary>
		/// Interrupt event 기능 활성화/비활성화 정보 쓰기
		/// </summary>
		/// <param name="net">			: Network id(IPv4 4th address)		</param>
		/// <param name="enable">		: Interrupt event 기능 활성화 유무. 1: 활성화, 0: 비활성화	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetEnableInterruptEvent(int net, int enable);

		/// <summary>
		/// Interrupt event 기능 활성화/비활성화 정보 읽기
		/// </summary>
		/// <param name="net">			: Network id(IPv4 4th address)		</param>
		/// <param name="enable">		: Interrupt event 기능 활성화 유무. 1: 활성화, 0: 비활성화	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetIsInterruptEvent(int net, out int enable);

		/// <summary>
		/// Interrupt event table의 이벤트 발생 대기
		/// </summary>
		/// <remarks>
		/// Interrupt event table 정보에 맞는 event 발생이 있을 때까지 대기한다
		/// </remarks>
		/// <param name="net">			: Network id(IPv4 4th address)		</param>
		/// <param name="table_index">	: Interrupt event table 번호		</param>
		/// <param name="timeout">		: 대기 시간(milliseconds). 0으로 설정 시, 무한 대기	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetWaitInterruptEvent(int net, int table_index, int timeout);

		/// <summary>
		/// Interrupt event table의 이벤트 발생 대기 해제
		/// </summary>
		/// <remarks>
		/// Event 발생 대기 중이면('WaitInterruptEvent' API Function 동작 중), event 발생 대기를 강제로 해제한다
		/// </remarks>
		/// <param name="net">			: Network id(IPv4 4th address)		</param>
		/// <param name="table_index">	: Interrupt event table 번호		</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetReleaseWaitingInterruptEvent(int net, int table_index);

		/// <summary>
		/// Interrupt event table의 이벤트 발생 대기 유무를 확인
		/// </summary>
		/// <remarks>
		/// 'WaitInterruptEvent' API Function이 동작 중인지 확인한다
		/// </remarks>
		/// <param name="net">			: Network id(IPv4 4th address)		</param>
		/// <param name="table_index">	: Interrupt event table 번호		</param>
		/// <param name="waiting">		: 이벤트 발생 대기 유무. 1: 현재 이벤트 발생 대기 중, 0: None	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetIsWaitingInterruptEvent(int net, int table_index, out int waiting);

		#endregion

		#region Gantry Sync

		/// <summary>
		/// 동기 구동 활성화
		/// </summary>
		/// <param name="net">		: Network id(IPv4 4th address)	 </param>
		/// <param name="master">	: master 축 번호					 </param>
		/// <param name="slave">	: slave 축 번호					 </param>	
		/// <param name="enable">	: 동기 구동 활성화/비활성화.	
		///							  "1"-> enable, "0"-> disable	 </param>
		/// <returns>				: (see enum "eSnetApiReturnCode")</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetEnableGantrySync(int net, int master, int slave, int enable);

		/// <summary>
		/// 동기 구동 활성화 여부 반환
		/// </summary>
		/// <param name="net">		: Network id(IPv4 4th address)	 </param>
		/// <param name="master">	: master 축 번호					 </param>
		/// <param name="slave">	: slave 축 번호					 </param>	
		/// <param name="enable">	: 동기 구동 활성화/비활성화.	
		///							  "1"-> enable, "0"-> disable	 </param>
		/// <returns>				: (see enum "eSnetApiReturnCode")</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetIsGantrySync(int net, int master, out int slave, out int enable);

		/// <summary>
		/// 동기 원점 검색 설정
		/// </summary>
		/// <param name="net">				: Network id(IPv4 4th address)		</param>
		/// <param name="master">			: master 축 번호						</param>
		/// <param name="slave">			: slave 축 번호						</param>
		/// <param name="dualUse">			: 원점 검색 실행 축 옵션				
		///									  "0" -> master 축 기준 단독 homing
		///									  "1" -> master 축 기준 homing 이후, slave 축 기준 homing </param>
		/// <param name="compensationUse">	: 보정 이송 옵션						
		///									  "0" -> 홈 센서 간 거리 구하고 보정 이송 하지 않음
		///									  "1" -> 홈 센서 간 거리 및 비틀림 구하고 보정 이송 진행 </param>
		/// <param name="offset">			: 두 축 홈 센서 간 거리				</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")   </returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetSetGantrySyncHoming(int net, int master, int slave, int dualUse, int compensationUse, int offset);

		/// <summary>
		/// 동기 원점 검색 설정 반환
		/// </summary>
		/// <param name="net">				: Network id(IPv4 4th address)		</param>
		/// <param name="master">			: master 축 번호						</param>
		/// <param name="slave">			: slave 축 번호						</param>
		/// <param name="dualUse">			: 원점 검색 실행 축 옵션				
		///									  "0" -> master 축 기준 단독 homing
		///									  "1" -> master 축 기준 homing 이후, slave 축 기준 homing </param>
		/// <param name="compensationUse">	: 보정 이송 옵션						
		///									  "0" -> 홈 센서 간 거리 구하고 보정 이송 하지 않음
		///									  "1" -> 홈 센서 간 거리 및 비틀림 구하고 보정 이송 진행 </param>
		/// <param name="offset">			: 두 축 홈 센서 간 거리				</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")   </returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetGantrySyncHoming(int net, int master, out int slave, out int dualUse, out int compensationUse, out int offset);

		/// <summary>
		/// 동기 원점 검색으로 도출된 두 축 홈 센서 간 거리 반환
		/// </summary>
		/// <param name="net">		: Network id(IPv4 4th address)	 </param>
		/// <param name="master">	: master 축 번호					 </param>
		/// <param name="offset">	: 두 축 홈 센서 간 거리			 </param>
		/// <returns>				: (see enum "eSnetApiReturnCode")</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetGetGantrySyncHomeSensorOffset(int net, int master, out int offset);

		#endregion

		#region EtherCAT

		/// <summary>
		/// EtherCAT Master에 연결된 Slave(Node) 수 읽기  (eSnetEcatGetNodeCount)  
		/// </summary>
		/// <param name="net">		: Network id(IPv4 4th address)		</param>
		/// <param name="count">	: Slave(Node) 수					</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetEcatGetNodeCount(int net, out int count);

        /// <summary>
        /// EtherCAT Master에 연결된 Slave Motion Node 수 읽기
        /// </summary>
        /// <param name="net">				: Network id(IPv4 4th address)											</param>
        /// <param name="count_all_node">	: 전체 Slave(Node) 수													</param>
		/// <param name="count_node_motion">: Slave Motion Node 수													</param>
        /// <param name="node_index">		: Slave Motion Node 수 만큼의 Node Index	
        ///										(ex) 두 번째, 네 번째 Node에 Motion Slave가 있다면 node_inde[0]=1, node_index[1]=3
        ///										(ex) 32개 배열을 선언하고, 읽은 count 갯수만큼 배열값을 읽을 것	</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")										</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetEcatGetNodeMotionCount(int net, out int count_all_node, out int count_node_motion, out int node_index);
		        
        /// <summary>
        /// EtherCAT Master에 연결된 Slave(Node)의 Fixed Address 읽기  (eSnetEcatGetFixedAddress)  
        /// </summary>
        /// <param name="net">			: Network id(IPv4 4th address)		</param>
        /// <param name="node_index">	: 읽어올 Slave(Node)의 시작 번호	</param>
        /// <param name="count">		: 읽어올 Slave(Node) 수				</param>
        /// <param name="address">		: Slave(Node)의 Fixed Address		</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetEcatGetFixedAddress(int net, int node_index, int count, out int address);

		/// <summary>
		/// EtherCAT Master에 연결된 Slave(Node)의 Logical Start Address 읽기  (eSnetEcatGetLogicalAddress)  
		/// </summary>
		/// <param name="net">			: Network id(IPv4 4th address)			</param>
		/// <param name="node_index">	: 읽어올 Slave(Node)의 시작 번호		</param>
		/// <param name="count">		: 읽어올 Slave(Node) 수					</param>
		/// <param name="address">		: Slave(Node)의 Logical Start Address	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")		</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetEcatGetLogicalAddress(int net, int node_index, int count, out int address);

		/// <summary>
		/// EtherCAT Master에 연결된 모든 Slave(Node)의 State를 전환( ‘Init’ 혹은 ‘Operational’ State)
		/// </summary>
		/// <param name="net">		: Network id(IPv4 4th address)		</param>
		/// <param name="state">	: 1 -> 'Init' State
		///							: 8 -> 'Operational' State			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetEcatSetStateMachine(int net, int state);

		/// <summary>
		/// EtherCAT Master에 연결된 Slave(Node)의 AL Status 읽기 (eSnetEcatGetAlStatus)
		/// </summary>
		/// <param name="node_index">	: 읽어올 Slave(Node)의 시작 번호</param>
		/// <param name="count">		: 읽어올 Slave(Node) 수</param>
		/// <param name="status">		: Slave(Node)의 AL Status
		///								  1 -> 'Init' State
		///								  2 -> 'Pre-Operational' State
		///								  3 -> 'Bootstrap' State
		///								  4 -> 'Safe-Operational' State
		///								  8 -> 'Operational' State </param>
		/// <returns>					: (see enum "eSnetApiReturnCode")</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetEcatGetAlStatus(int net, int node_index, int count, out int status);

		/// <summary>
		/// EtherCAT Master에 연결된  Slave(Node)의 AL Status Code 읽기 (eSnetEcatGetAlStatusCode) 
		/// </summary>
		/// <param name="net">			: Network id(IPv4 4th address)		</param>
		/// <param name="node_index">	: 읽어올 Slave(Node)의 시작 번호	</param>
		/// <param name="count">		: 읽어올 Slave(Node) 수				</param>
		/// <param name="status_code">	: Slave(Node)의 AL Status Code		</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int eSnetEcatGetAlStatusCode(int net, int node_index, int count, out int status_code);

        /// <summary>
        /// EtherCAT Master에 연결된 Slave(Node)의 ESC register 읽기
        /// </summary>
        /// <param name="net">			: Network id(IPv4 4th address)			</param>
        /// <param name="node_index">	: 읽어올 Slave(Node)의 번호				</param>
        /// <param name="address">		: 읽어올 ESC register의 주소			</param>
        /// <param name="length">		: 읽어올 ESC register의 크기(byte 단위)	</param>
        /// <param name="data">			: 읽어온 ESC register 값이 저장될 버퍼	</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")		</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetEcatGetEscRegister(int net, int node_index, int address, int length, out int data);

        /// <summary>
        /// EtherCAT Master에 연결된 Slave(Node)의 ESC register 쓰기
        /// </summary>
        /// <param name="net">			: Network id(IPv4 4th address)		</param>
        /// <param name="node_index">	: 쓸 Slave(Node)의 번호				</param>
        /// <param name="address">		: 쓸 ESC register의 주소			</param>
        /// <param name="length">		: 쓸 ESC register의 크기(byte 단위)	</param>
        /// <param name="data">			: 쓸 ESC register 값				</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetEcatSetEscRegister(int net, int node_index, int address, int length, int data);

        /// <summary>
        /// EtherCAT Master에 연결된 Slave(Node)의 Object를 SDO로 읽기
        /// </summary>
        /// <param name="net">			: Network id(IPv4 4th address)		</param>
        /// <param name="node_index">	: 읽어올 Slave(Node)의 번호			</param>
        /// <param name="index">		: 읽어올 Object의 index				</param>
        /// <param name="sub_index">	: 읽어올 Object의 sub-index			</param>
        /// <param name="length"		: 읽어올 Object의 크기(byte 단위)>	</param>
        /// <param name="data">			: 읽어온 Object 값이 저장될 버퍼	</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetEcatGetCoeSdo(int net, int node_index, int index, int sub_index, int length, out int data);

        /// <summary>
        /// EtherCAT Master에 연결된 Slave(Node)의 Object를 SDO로 쓰기
        /// </summary>
        /// <param name="net">			: Network id(IPv4 4th address)		</param>
        /// <param name="node_index">	: 쓸 Slave(Node)의 번호				</param>
        /// <param name="index">		: 쓸 Object의 index					</param>
        /// <param name="sub_index">	: 쓸 Object의 sub-index				</param>
        /// <param name="length"		: 쓸 Object의 크기(byte 단위)>		</param>
        /// <param name="data">			: 쓸 Object 값						</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetEcatSetCoeSdo(int net, int node_index, int index, int sub_index, int length, int data);

        /// <summary>
        /// EtherCAT Master에 연결된 Slave(Node)의 Object를 PDO로 읽기
        /// </summary>
        /// <param name="net">			: Network id(IPv4 4th address)		</param>
        /// <param name="node_index">	: 읽어올 Slave(Node)의 번호			</param>
        /// <param name="type">			: 읽어올 Object의 종류. 1: RxPDO(출력), 2: TxPDO(입력). eSnetEcatCoeObjectType 참고	</param>
        /// <param name="index">		: 읽어올 Object의 index				</param>
        /// <param name="sub_index">	: 읽어올 Object의 sub-index			</param>
        /// <param name="data">			: 읽어온 Object 값이 저장될 버퍼	</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetEcatGetCoePdo(int net, int node_index, int type, int index, int sub_index, out int data);

        /// <summary>
        /// EtherCAT Master에 연결된 Slave(Node)의 Object를 PDO로 쓰기
        /// </summary>
        /// <remarks>
        /// 데이터 출력이 가능한 Object 종류는 RxPDO만 가능
        /// </remarks>
        /// <param name="net">			: Network id(IPv4 4th address)		</param>
        /// <param name="node_index">	: 쓸 Slave(Node)의 번호				</param>
        /// <param name="index">		: 쓸 Object의 index					</param>
        /// <param name="sub_index">	: 쓸 Object의 sub-index				</param>
        /// <param name="data">			: 쓸 Object 값						</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		[DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetEcatSetCoePdo(int net, int node_index, int index, int sub_index, int data);

        /// <summary>
        /// EtherCAT Master에 연결된 Slave Io Input Node Index와 각 Node별 최대 Input 접점 수 읽기
        /// </summary>
        /// <param name="net">				: Network id(IPv4 4th address)											</param>
        /// <param name="count_all_node">	: 전체 Slave(Node) 수													</param>
        /// <param name="count_io_node">	: Slave Motion Node 수													</param>
        /// <param name="node_index">		: Slave Io Node 수 만큼의 Node Index	
        ///										(ex) 두 번째, 네 번째 Node에 Io Slave가 있다면 node_inde[0]=1, node_index[1]=3
        ///										(ex) 32개 배열을 선언하고, 읽은 count 갯수만큼 배열값을 읽을 것		</param>
        ///	<param name="count_max_point">	: 각 node별 최대 접점 수												</param>									
        /// <returns>						: (see enum "eSnetApiReturnCode")										</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetEcatGetNodeIoInputInfo(int net, out int count_all_node, out int count_io_node, out int node_index, out int count_max_point);

        /// <summary>
        /// EtherCAT Master에 연결된 Slave Io Output Node Index와 각 Node별 최대 Output 접점 수 읽기
        /// </summary>
        /// <param name="net">				: Network id(IPv4 4th address)											</param>
        /// <param name="count_all_node">	: 전체 Slave(Node) 수													</param>
        /// <param name="count_io_node">	: Slave Motion Node 수													</param>
        /// <param name="node_index">		: Slave Io Node 수 만큼의 Node Index	
        ///										(ex) 두 번째, 네 번째 Node에 Io Slave가 있다면 node_inde[0]=1, node_index[1]=3
        ///										(ex) 32개 배열을 선언하고, 읽은 count 갯수만큼 배열값을 읽을 것		</param>
		///	<param name="count_max_point">	: 각 node별 최대 접점 수												</param>									
        /// <returns>						: (see enum "eSnetApiReturnCode")										</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetEcatGetNodeIoOutputInfo(int net, out int count_all_node, out int count_io_node, out int node_index, out int count_max_point);

        /// <summary>
        /// EtherCAT Master에 연결된 모든 IO Slave(Node)의 모든 Input 접점(all input bits)을 한번에 읽기
        /// </summary>
        /// <param name="net">				: Network id(IPv4 4th address)													</param>
        /// <param name="count_node">		: Slave Io Node 수																</param>
        /// <param name="index_node">		: 배열, Slave Io Node 수 만큼의 Node Index	
        ///										(ex) 두 번째, 네 번째 Node에 Io Slave가 있다면 node_inde[0]=1, index_node[1]=3
        ///										(ex) 32개 배열을 선언하고, 읽은 count 갯수만큼 배열값을 읽을 것				</param>
        /// <param name="count_max_point">	: 배열, 각 node별 최대 접점(bit) 수										
        ///										(ex) count_max_point[2]는 세번째 Io Node의 최대 접점(max bit) 갯수를 표시	</param>
        /// <param name="data">				: 배열, 각 Io Node 별 input data
        ///										(ex) data[2]는 세번째 Io Node의 bit data값들을 표시							</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")												</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetEcatGetNodeIoInputAll(int net, out int count_node, out int index_node, out int count_max_point, out int data);

        /// <summary>
        /// EtherCAT Master에 연결된 모든 IO Slave(Node)의 모든 Output 접점(all output bits)을 한번에 읽기
        /// </summary>
        /// <param name="net">				: Network id(IPv4 4th address)													</param>
        /// <param name="count_node">		: Slave Io Node 수																</param>
        /// <param name="index_node">		: 배열, Slave Io Node 수 만큼의 Node Index	
        ///										(ex) 두 번째, 네 번째 Node에 Io Slave가 있다면 node_inde[0]=1, index_node[1]=3
        ///										(ex) 32개 배열을 선언하고, 읽은 count 갯수만큼 배열값을 읽을 것				</param>
        /// <param name="count_max_point">	: 배열, 각 node별 최대 접점(bit) 수										
        ///										(ex) count_max_point[2]는 세번째 Io Node의 최대 접점(max bit) 갯수를 표시	</param>
        /// <param name="data">				: 배열, 각 Io Node 별 input data
        ///										(ex) data[2]는 세번째 Io Node의 bit data값들을 표시							</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")												</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetEcatGetNodeIoOutputAll(int net, out int count_node, out int index_node, out int count_max_point, out int data);

        /// <summary>
        /// EtherCAT Master에 연결된 IO Slave(Node)의 Input 모든 bit을 한번에 읽기
        /// </summary>
        /// <param name="net">				: Network id(IPv4 4th address)		</param>
        /// <param name="index_node">		: 쓸 Slave(Node)의 번호				</param>
        /// <param name="count_max_point">	: Slave(Node)의 최대 접점(bit) 갯수	</param>
        /// <param name="data">				: io value							</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetEcatGetNodeIoInputPort(int net, int index_node, out int count_max_point, out int data);

        /// <summary>
        /// EtherCAT Master에 연결된 IO Slave(Node)의 Ouput 모든 bit을 한번에 읽기
        /// </summary>
        /// <param name="net">				: Network id(IPv4 4th address)		</param>
        /// <param name="index_node">		: 쓸 Slave(Node)의 번호				</param>
        /// <param name="count_max_point">	: Slave(Node)의 최대 접점(bit) 갯수	</param>
        /// <param name="data">				: io value							</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetEcatGetNodeIoOutputPort(int net, int index_node, out int count_max_point, out int data);

        /// <summary>
        /// EtherCAT Master에 연결된 IO Slave(Node)의 Input 한 bit를 PDO로 읽기
        /// </summary>
        /// <param name="net">			: Network id(IPv4 4th address)		</param>
        /// <param name="index_node">	: 쓸 Slave(Node)의 번호				</param>
        /// <param name="point">		: io bit number("0"~ )				</param>
        /// <param name="data">			: io value							</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetEcatGetNodeIoInputPoint(int net, int index_node, int point, out int data);

        /// <summary>
        /// EtherCAT Master에 연결된 IO Slave(Node)의 Output 한 bit를 PDO로 읽기
        /// </summary>
        /// <param name="net">			: Network id(IPv4 4th address)		</param>
        /// <param name="index_node">	: 쓸 Slave(Node)의 번호				</param>
        /// <param name="point">		: io bit number("0"~ )				</param>
        /// <param name="data">			: io value							</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetEcatGetNodeIoOutputPoint(int net, int index_node, int point, out int data);

        /// <summary>
        /// EtherCAT Master에 연결된 IO Slave(Node)의 Output 한 bit를 PDO로 쓰기
        /// </summary>
        /// <param name="net">			: Network id(IPv4 4th address)		</param>
        /// <param name="index_node">	: 쓸 Slave(Node)의 번호				</param>
        /// <param name="point">		: io bit number("0"~ )				</param>
        /// <param name="data">			: io value							</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetEcatSetNodeIoOutputPoint(int net, int index_node, int point, int data);

        #endregion

        #region CiA402

        /// <summary>
        /// CiA-402 에러 코드 읽기
        /// </summary>
        /// <param name="net">		: Network id(IPv4 4th address)</param>
        /// <param name="axis">		: 축 번호</param>
        /// <param name="error">	: 읽어서 저장할 에러 코드</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetCiA402GetErrorCode(int net, int axis, out int error);

        #endregion

		#region Custom
        /// <summary>
        /// "서보 On 신호 출력 후" 설정 시간 동안 "서보 알람/ 서보 Ready" 신호 체크 지연
        /// </summary>
        /// <param name="net">		: Network id(IPv4 4th address)				</param>
        /// <param name="delay">	: 지연 시간, 단위: 1msec , 최대 5000(5초)		</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetSetDelayBeforeErrorCheck(int net, int delay);

        /// <summary>
        /// eSnetSetDelayBeforeErrorCheck() 설정 상태 확인 
        /// </summary>
        /// <param name="net">		: Network id(IPv4 4th address)				</param>
        /// <param name="delay">	: 지연 시간, 단위: 1msec , 최대 5000(5초),	</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
        [DllImport("EMotionSnetDeviceEx.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eSnetGetDelayBeforeErrorCheck(int net, out int delay);
        #endregion

        #endregion

        #region Property

        /// <summary>
        /// Device Network ID(IPv4 address 4)
        /// </summary>
        public int NetID { get; protected set; } = 1;

		#endregion

		/// <summary>
		/// SnetDevice Constructor
		/// </summary>
		public SnetDevice()
		{
		}

		/// <summary>
		/// SnetDevice Constructor
		/// </summary>
		/// <param name="net">Network ID(IPv4 4th address)</param>
		public SnetDevice(int net)
		{
			NetID = net;
		}

		#region Operation

		#region Connection

		/// <summary>
		/// 제어기 연결 하기
		/// : old version name -->"함수 이름은 같지만, 인자 속성이 바뀌었음"
		/// </summary>
		/// <param name="net"> 			: controller id 														</param>
		/// <param name="reconnect">	: "false" : 기존 Snet Device가 수행 되고 있으면 Clear시키고 재실행 함	
		///								  "true" : User가 반드시 "eSnetDisconnect"를 해야 함					</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")										</returns>
		public int Connect(int net, bool reconnect = false)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (reconnect ? 1 : 0);

				returnCode = eSnetConnect(net, setValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetConnect'. error : " + returnCode,
						"SnetDevice.Connect");
				}
				else
					NetID = net;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.Connect");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        /// <summary>
        /// 제어기 연결 하기
        /// : IPv4 Address --> ip1.ip2.ip3.ip4
        /// </summary>
        /// <param name="ip1">			: IPv4 Address 1														</param>
        /// <param name="ip2">			: IPv4 Address 2														</param>
        /// <param name="ip3">			: IPv4 Address 3														</param>
        /// <param name="ip4">			: IPv4 Address 4														</param>
        /// <param name="reconnect">	: "0" : 기존 Snet Device가 수행 되고 있으면 Clear시키고 재실행 함	
        ///								  "1" : User가 반드시 "eSnetDisconnect"를 해야 함					</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")									</returns>
		public int ConnectEx(int ip1, int ip2, int ip3, int ip4, bool reconnect = false)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (reconnect ? 1 : 0);

				returnCode = eSnetConnectEx(ip1, ip2, ip3, ip4, setValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetConnectEx'. error : " + returnCode,
						"SnetDevice.ConnectEx");
				}
				else
					NetID = ip4;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.ConnectEx");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 제어기 연결 끊기
		/// </summary>
		/// <returns>	: (see enum "eSnetApiReturnCode")	</returns>
		public int Disconnect()
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetDisconnect(NetID);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetDisconnect'. error : " + returnCode,
						"SnetDevice.Disconnect");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.Disconnect");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Version Info

		/// <summary>
		/// 제어기의 API 버젼 읽기
		/// : old version name -->"eSnetDllVersion"
		/// </summary>
		/// <param name="version">	: api version						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int GetApiVersion(ref int version)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetApiVersion(NetID, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetOsVersion'. error : " + returnCode,
						"SnetDevice.GetApiVersion");
				}
				else
					version = getValue;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetApiVersion");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 제어기의 OS 버젼 읽기
		/// </summary>
		/// <param name="version">		: os version						</param>
		/// <param name="device_id">	: "10" : snet_p, "20" : snet_rtex	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		public int GetOsVersion(ref int version, ref int device_id)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetOsVersion(NetID, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetOsVersion'. error : " + returnCode,
						"SnetDevice.GetOsVersion");
				}
				else
				{
					version = getValue0;
					device_id = getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetOsVersion");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 제어기의 OS 버젼 읽기
		/// </summary>
		/// <param name="version">		: os version						</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		public int GetFpgaVersion(ref int version)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetFpgaVersion(NetID, out int getValue0);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetFpgaVersion'. error : " + returnCode,
						"SnetDevice.GetFpgaVersion");
				}
				else
				{
					version = getValue0;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetFpgaVersion");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region User Log

		/// <summary>
		/// Log 남기기 설정
		/// (tip 1)log 파일 위치 : 실행파일이 있는 디렉토리 -> Log디렉토리 -> 시스템날짜 파일이름
		/// : old version name -->"eSnetSetUseLog"
		/// </summary>
		/// <param name="use">		: "true"-->use, "false"-->unUsed		</param>
		/// <param name="section">	: (see enum eSnetUserLogSection)
		///							  (예) homing sequence와 단축구동시 둘다 내용을 log에 남기고 싶다면 int형 bit or 연산을 해서 인자로 넣으면 된다.
		///							  ((int)eSnetUserLogSection::Origin | (int)eSnetUserLogSection::MoveSingle) == (0x0001 | 0x0010) == 0x0011(decimal: 17)	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		public int SetUserLogSection(bool use, eSnetUserLogSection section = eSnetUserLogSection.NotUsed)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (use ? 1 : 0);

				returnCode = eSnetSetUserLogSection(NetID, setValue, (int)section);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetUserLogSection'. error : " + returnCode,
						"SnetDevice.SetUserLogSection");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetUserLogSection");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Log 남기기 설정값 읽기
		/// (tip 1)log 파일 위치 : 실행파일이 있는 디렉토리 -> Log디렉토리 -> 시스템날짜 파일이름
		/// : old version name -->"eSnetGetUseLog"
		/// </summary>
		/// <param name="use">		: "true"-->use, "false"-->unUsed		</param>
		/// <param name="section">	: (see enum _eSnetUserLogSection)
		///							  (예) homing sequence와 단축구동시 둘다 내용을 log에 남기고 싶다면 int형 bit or 연산을 해서 인자로 넣으면 된다.
		///							  ((int)eSnetLogSection::Origin | (int)eSnetLogSection::MoveSingle) == (0x0001 | 0x0010) == 0x0011(decimal: 17)	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		public int GetUserLogSection(ref bool use, ref eSnetUserLogSection section)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetUserLogSection(NetID, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetUserLogSection'. error : " + returnCode,
						"SnetDevice.GetUserLogSection");
				}
				else
				{
					use = ((getValue0 == 1) ? true : false);
					section = (eSnetUserLogSection)getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetUserLogSection");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Communication Conndition Setting

		/// <summary>
		/// API 통신 Return time out시간과 retry 횟수 설정
		/// </summary>
		/// <param name="time_out">		: API 지령 후 제어기 최대 응답 대기 시간 설정: default "1000"[msec]	</param>
		/// <param name="retry_count">	: time out이 발생시,명령 지령 재 전송 횟수							</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")									</returns>
		public int SetCommunicationConfig(int time_out, int retry_count)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetCommunicationConfig(NetID, time_out, retry_count);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetCommunicationConfig'. error : " + returnCode,
						"SnetDevice.SetCommunicationConfig");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetCommunicationConfig");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// API 통신 Return time out시간과 retry 설정값 읽기
		/// </summary>
		/// <param name="time_out">		: API 지령 후 제어기 최대 응답 대기 시간 설정: default "1000"[msec]	</param>
		/// <param name="retry_count">	: time out이 발생시,명령 지령 재 전송 횟수							</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")									</returns>
		public int GetCommunicationConfig(ref int time_out, ref int retry_count)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetCommunicationConfig(NetID, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetCommunicationConfig'. error : " + returnCode,
						"SnetDevice.GetCommunicationConfig");
				}
				else
				{
					time_out = getValue0;
					retry_count = getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetCommunicationConfig");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Ehternet 통신 "PC측 UDP Port 번호 자동 변경" 기능 사용 유무 설정 (Default "0")
		/// </summary>
		/// <param name="use">	: "true"->사용, "false"->사용하지 않음:connection시에 지정된 하나의 port로만 통신	</param>						
		/// <returns>			: (see enum "eSnetApiEventCode")											</returns>										
		public int SetCommunicationAutoPortChange(bool use)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (use ? 1 : 0);

				returnCode = eSnetSetCommunicationAutoPortChange(NetID, setValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetCommunicationAutoPortChange'. error : " + returnCode,
						"SnetDevice.SetCommunicationAutoPortChange");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetCommunicationAutoPortChange");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Ehternet 통신 "PC측 UDP Port 번호 자동 변경" 기능 설정 상태 읽기  
		/// </summary>
		/// <param name="use">	: "true"->사용, "false"->사용하지 않음:connection시에 지정된 하나의 port로만 통신	</param>
		/// <returns>			: (see enum "eSnetApiEventCode")											</returns>
		public int GetCommunicationAutoPortChange(ref bool use)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetCommunicationAutoPortChange(NetID, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetCommunicationAutoPortChange'. error : " + returnCode,
						"SnetDevice.GetCommunicationAutoPortChange");
				}
				else
				{
					use = ((getValue == 1) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetCommunicationAutoPortChange");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// API 지령 후 함수 Return Time
		/// </summary>
		/// <param name="communication_time">	: [usec] : DLL 함수 지령 후 제어기로 부터 return 되어 오는데까지 걸린 시간	</param>
		/// <returns>							: (see enum "eSnetApiReturnCode")											</returns>										
		public int CheckCommunicationTime(ref int communication_time)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetCheckCommunicationTime(NetID, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetCheckCommunicationTime'. error : " + returnCode,
						"SnetDevice.CheckCommunicationTime");
				}
				else
				{
					communication_time = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.CheckCommunicationTime");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Reset / Stop / Alarm Clear

		/// <summary>
		/// "Fault상태"를 해제(Reset) 하거나 단축 이송시 사용자에 의한 "수동 정지(Stop)"실행 
		/// (tip 1) 이송 정지시 "기본 파라미터 P20(Max. velocity) / P21(Reset Dec.Time)"으로 계산된 감속비가 
		///			적용 됩니다.  
		///	: old version name ->"eSnetResetAxis"
		/// </summary>
		/// <param name="axis">	: axis index						</param>	
		/// <returns>			: (see enum "eSnetApiReturnCode")>	</returns>		
		public int Reset(int axis)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetReset(NetID, axis);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetReset'. error : " + returnCode,
						"SnetDevice.Reset");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.Reset");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 모든 축의 "Fault상태"를 해제(Reset) 하거나 사용자에 의한 "수동 정지(Stop)"실행
		/// (tip 1) 이송 정지시 "기본 파라미터 P20(Max. velocity) / P21(Reset Dec.Time)"으로 계산된 감속비가 
		///			적용 됩니다. 
		///	: old version name ->"eSnetResetAllAxis"
		/// </summary>
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>	
		public int ResetAll()
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetResetAll(NetID);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetResetAll'. error : " + returnCode,
						"SnetDevice.ResetAll");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.ResetAll");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Fault상태"를 해제(Reset) 하거나 단축 이송시 사용자에 의한 "수동 정지(Stop)"실행 
		/// (tip 1) 이송 정지시 "기본 파라미터 P20(Max. velocity) / P21(Reset Dec.Time)"으로 계산된 감속비가 
		///			적용 됩니다.  
		///	: old version name ->"eSnetEmgStopAxis"
		/// </summary>
		/// <param name="axis">	: axis index						</param>	
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>	
		public int EmergencyStop(int axis)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetEmergencyStop(NetID, axis);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetEmergencyStop'. error : " + returnCode,
						"SnetDevice.EmergencyStop");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.EmergencyStop");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 모든 축의 "Fault상태"를 해제(Reset) 하거나 사용자에 의한 "수동 정지(Stop)"실행 
		/// (tip 1) 이송 정지시 "기본 파라미터 P20(Max. velocity) / P21(Reset Dec.Time)"으로 계산된 감속비가 
		///			적용 됩니다.
		///	: old version name ->"eSnetEmgStopAllAxis"
		/// </summary>
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>	
		public int EmergencyStopAll()
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetEmergencyStopAll(NetID);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetEmergencyStopAll'. error : " + returnCode,
						"SnetDevice.EmergencyStopAll");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.EmergencyStopAll");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 단축 이송 중 수동"감속 정지"실행  
		/// : old version name -->"eSnetSStopAxis"
		/// </summary>
		/// <param name="axis">		: axis index						</param>						
		/// <param name="decel">	: 감속 시간 (msec)					</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int SlowStop(int axis, int decel)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSlowStop(NetID, axis, decel);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSlowStop'. error : " + returnCode,
						"SnetDevice.SlowStop");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SlowStop");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 모든 축의 단축 이송 중 수동"감속 정지"실행  
		/// : old version name -->"eSnetSStopAxis"
		/// </summary>
		/// <param name="axis">		: axis index						</param>						
		/// <param name="decel">	: 감속 시간 (msec)					</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int SlowStopAll(int decel)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSlowStopAll(NetID, decel);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSlowStopAll'. error : " + returnCode,
						"SnetDevice.SlowStopAll");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SlowStopAll");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 정지 조건 설정 및 활성화
		/// </summary>
		/// <param name="net">		 : Network id(IPv4 4th address)		   </param>
		/// <param name="axis">		 : 축 번호							   </param>
		/// <param name="enable">	 : 정지 조건 활성화 여부				   </param>
		///							   1 -> enable
		///							   0 -> disable (나머지 파라미터 미적용) </param>
		/// <param name="address">	 : I/O Map Index 번호				   </param>
		/// <param name="bit">		 : 0 ~ 31 bit						   </param>
		/// <param name="level">     : 접점
		///							  0 -> A 접점(Normal Open)		 
		///							  1 -> B 접점(Normal Close)			   </param>
		/// <param name="decelTime"> : 감속 시간(1msec)					   </param>
		/// <returns>				 : (see enum "eSnetApiReturnCode")     </returns>
		public int SetIoStop(int axis, int enable, int address, int bit, int level, int decelTime)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetIoStop(NetID, axis, enable, address, bit, level, decelTime);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetIoStop'. error : " + returnCode,
													"SnetDevice.SetIoStop");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetIoStop");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 정지 조건 설정 확인
		/// </summary>
		/// <param name="net">		 : Network id(IPv4 4th address)		   </param>
		/// <param name="axis">		 : 축 번호							   </param>
		/// <param name="enable">	 : 정지 조건 활성화 여부				   </param>
		///							   1 -> enable
		///							   0 -> disable (나머지 파라미터 미적용) </param>
		/// <param name="address">	 : I/O Map Index 번호				   </param>
		/// <param name="bit">		 : 0 ~ 31 bit						   </param>
		/// <param name="level">	 : 접점
		///							  0 -> A 접점(Normal Open)		 
		///							  1 -> B 접점(Normal Close)			   </param>
		/// <param name="decelTime"> : 감속 시간(1msec)					   </param>
		/// <returns>				 : (see enum "eSnetApiReturnCode")     </returns>
		public int GetIoStop(int axis, ref int enable, ref int address, ref int bit, ref int level, ref int decelTime)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetIoStop(NetID, axis, out int getEnable, out int getAddress, out int getBit, out int getLevel, out int getDecelTime);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetIoStop'. error : " + returnCode,
													"SnetDevice.GetIoStop");
				}
				else
				{
					enable = getEnable;
					address = getAddress;
					bit = getBit;
					level = getLevel;
					decelTime = getDecelTime;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetIoStop");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// bit 단위로 정지 조건 설정 및 활성화
		/// </summary>
		/// <param name="net">		 : Network id(IPv4 4th address)		   </param>
		/// <param name="axis">		 : 축 번호							   </param>
		/// <param name="enable">	 : 정지 조건 활성화 여부				   </param>
		///							   1 -> enable
		///							   0 -> disable (나머지 파라미터 미적용) </param>
		/// <param name="bit">		 : 0 ~ 31 bit						   </param>
		/// <param name="level">	 : 접점
		///							  0 -> A 접점(Normal Open)		 
		///							  1 -> B 접점(Normal Close)			   </param>
		/// <param name="decelTime"> : 감속 시간(1msec)					   </param>
		/// <returns>				 : (see enum "eSnetApiReturnCode")     </returns>
		public int SetIoStopBit(int axis, int enable, int bit, int level, int decelTime)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetIoStopBit(NetID, axis, enable, bit, level, decelTime);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetIoStopBit'. error : " + returnCode,
													"SnetDevice.SetIoStopBit");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetIoStopBit");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// bit 단위로 정지 조건 설정 확인
		/// </summary>
		/// <param name="net">		 : Network id(IPv4 4th address)		   </param>
		/// <param name="axis">		 : 축 번호							   </param>
		/// <param name="enable">	 : 정지 조건 활성화 여부				   </param>
		///							   1 -> enable
		///							   0 -> disable (나머지 파라미터 미적용) </param>
		/// <param name="bit">		 : 0 ~ 31 bit						   </param>
		/// <param name="level">	 : 접점
		///							  0 -> A 접점(Normal Open)		 
		///							  1 -> B 접점(Normal Close)			   </param>
		/// <param name="decelTime"> : 감속 시간(1msec)					   </param>
		/// <returns>				 : (see enum "eSnetApiReturnCode")     </returns>
		public int GetIoStopBit(int axis, ref int enable, ref int bit, ref int level, ref int decelTime)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetIoStopBit(NetID, axis, out int getEnable, out int getBit, out int getLevel, out int getDecelTime);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetIoStopBit'. error : " + returnCode,
													"SnetDevice.GetIoStopBit");
				}
				else
				{
					enable = getEnable;
					bit = getBit;
					level = getLevel;
					decelTime = getDecelTime;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetIoStopBit");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 축별 "Servo driver alarm clear" On/Off신호 출력 (예) panasonic amp의 경우 "1200msec"
		/// (tip 1) "Servo Driver" specification을 참고하여 "on_time" 조정  
		/// : old version name ->"SetAxisServoAlarmClear"
		/// </summary>
		/// <param name="axis">		: axis index										</param>						
		/// <param name="on_off">	: "true"->alarm clear 신호를 "onTime"시간동안 출력	</param>						
		/// <param name="on_time">	: alarm clear on 신호 출력 유지시간(msec) 
		///								(예)panasonic amp의 경우 "1200") 				</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")					</returns>	
		public int SetServoAlarmClear(int axis, bool on_off, int on_time)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (on_off ? 1 : 0);

				returnCode = eSnetSetServoAlarmClear(NetID, axis, setValue, on_time);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetServoAlarmClear'. error : " + returnCode,
						"SnetDevice.SetServoAlarmClear");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetServoAlarmClear");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 모든 축 "Servo driver alarm clear" On/Off신호 출력 
		/// (tip 1) "Servo Driver" specification을 참고하여 "on_time" 조정  (예) panasonic amp의 경우 "1200msec"
		/// : old version name ->"SetAllAxisServoAlarmClear"
		/// </summary>
		/// <param name="on_off">	: "true"->alarm clear 신호를 "on_time"시간동안 출력	</param>						
		/// <param name="on_time">	: alarm clear on 신호 출력 유지시간(msec)			</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")					</returns>	
		public int SetServoAlarmClearAll(bool on_off, int on_time)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (on_off ? 1 : 0);

				returnCode = eSnetSetServoAlarmClearAll(NetID, setValue, on_time);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetServoAlarmClearAll'. error : " + returnCode,
						"SnetDevice.SetServoAlarmClearAll");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetServoAlarmClearAll");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Axis Status

		/// <summary>
		/// 축별 상태 정보 읽기 
		/// : old version name ->"eSnetGetAxisAllStatus"
		/// </summary>
		/// <remarks>
		/// (tip 1) fault가 발생 하였을 경우, fault상태가 해제되어도 제어기는 fault 상태가 계속 유지 됩니다.
		///			사용자가 Reset을 해야만 fault상태가 해제 됩니다. 
		///			(예) limit sensor가 순간적으로 감지되었다가 해제 되었을 경우, 실시간 sensor값은 on->off로 변경 되었지만,
		///		    eSnetGetAxisStatus()함수에서는 "limit Error Fault Status"는 남아 있게 된다
		/// </remarks>
		/// <param name="axis">		: axis index																</param>						
		/// <param name="value">	: 축별 제어기 상태 (see. "enum _eSnetAxisStatus"actual position)	
		///								bit 0: 축 이송 상태 "1"-> moving, "0"-> stop 
		///								bit 1: 서보 드라이버 Ready 신호 입력 상태 
		///										(단 파라미터에서 서보 ready 입력을 enable 상태로 설정 했을 경우 적용) 
		///										"1" ->servo ready / "0" ->servo not ready 	
		///								bit 2: 서보 드라이버 "Servo On" 상태  "1"-> sv on, "0"-> sv off  
		///								bit 3: Fault status(limit error, amp fault, alarm, etc)
		///								bit 4: Profile(command) moving 지령 상태 "1"-> moving, "0"->profile move done
		///								bit 5: Servo alarm 상태 "1"-> servo alarm on 
		///								bit 6: "1"->positive direction move, 
		///									   "0"->negative direction move(Snet-rtex 제어기 미사용)
		///								bit 7: Inposition 상태 "1"->inposition on 
		///								bit 8: Positive hw limit fault  "1"-> +Limit fault  
		///								bit 9: Negative hw limit fault  "1"-> -Limit fault  
		///								bit 10: Inposition check time over "1"-> time over 
		///								bit 11: Following error status "1"-> following error 
		///								bit 12: Positive software limit fault "1"-> +Sw limit fault 
		///								bit 13: Negative software limit fault "1"-> -Sw limit fault				</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	
		public int GetAxisStatus(int axis, ref int value)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetAxisStatus(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetAxisStatus'. error : " + returnCode,
						"SnetDevice.GetAxisStatus");
				}
				else
				{
					value = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetAxisStatus");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 축별 "축 I/O"값과 "축 상태 정보" 읽기
		///	(tip 1) : 이 함수의 sensor값은 단순히 sensor의 감지 여부만을 표시한다.
		///			  sensor가 감지 되었다가 감지 되지 않았을 경우, 이 함수에서는 "입력 Off 상태"지만
		///			  eSnetGetAxisAllStatus(...)함수에서는 limit Error Fault Status가 남아 있게 된다
		///			  --> 실시간 IO 상태를 체크하는 것과, 제어기의 Limit Error Status를 구분해야 한다		
		/// : old version name ->"eSnetGetAxisIoAndStatus"
		/// </summary>
		/// <param name="axis">				: axis index																	</param>						
		/// <param name="io_and_status">	: "1"-> On, "0"-> Off  
		///										I/O 정보	-----------------------------------------------------------		
		///											bit 0:"-Limit Sensor" 입력 정보 
		///											bit 1:"+Limit Sensor" 입력 정보 
		///											bit 2:"원점 센서" 입력 정보  
		///											bit 3:"Servo Alarm" 입력 정보
		///											bit 4:"Servo Ready" 입력 정보
		///											bit 5:"Servo On" 출력 상태 정보 
		///										Status 정보	-------------------------------------------------------
		///											bit 6:"Inposition" 상태 정보 
		///											bit 7:"Fault" 상태 정보 (limit error/alarm/following error status etc...)		
		///													fault 상태가 되었을 때, eSnetGetAxisAllStatus(..) 또는 
		///													eSnetGetErrorCode(..)를 사용하여 추가 정보 확인				</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	
		public int GetAxisSignalStatus(int axis, ref int io_and_status)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetAxisSignalStatus(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetAxisSignalStatus'. error : " + returnCode,
						"SnetDevice.GetAxisSignalStatus");
				}
				else
				{
					io_and_status = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetAxisSignalStatus");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 축별 Motion Done 상태 읽기
		/// </summary>
		/// <param name="axis">			: axis index								</param>						
		/// <param name="motion_done">	: "true"->motion done, "false"-> moving 중	</param>						
		/// <returns>					: (see enum "eSnetApiReturnCode")			</returns>
		public int GetMotionDone(int axis, ref bool motion_done)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetMotionDone(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetMotionDone'. error : " + returnCode,
						"SnetDevice.GetMotionDone");
				}
				else
				{
					motion_done = ((getValue == 1) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetMotionDone");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 제어기의 에러 번호 읽기
		/// : old version name ->"eSnetGetErrorNumber"
		/// </summary>
		/// <param name="axis">			: axis index											</param>						
		/// <param name="error_code">	: error number (see. "enum _eSnetStatusErrorNumber") 	
		/// 								"100" : sv ready error 
		///									"101" : sv alarm error 
		///									"102" : sw limit(+) 
		///									"103" : sw limit(-) 
		///									"104" : hw limit(+)
		///									"105" : hw limit(-)
		///									"106" : following error 
		///									"109" : watch dog error							
		///									"200" : rtex_driver_link_error / ecat slave link error
		///									"210" : ethercat sdo write fail
		///									"211" : ethercat sdo read fail						</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")						</returns>	
		public int GetErrorCode(ref int axis, ref int error_code)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetErrorCode(NetID, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetErrorCode'. error : " + returnCode,
						"SnetDevice.GetErrorCode");
				}
				else
				{
					axis = getValue0;
					error_code = getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetErrorCode");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 축별 이송 지령값과 드라이버의 현재 이송값을 비교하여 최대/평균 이송 오차를 표시하고 마스터 축과 동기 구동
		/// 중일때 마스터 축과 슬레이브축 드라이버의 현재 이송값의 차를 표시 한다. 
		///
		/// </summary>
		/// <param name="axis">			: axis index							</param>
		/// <param name="error">		: [array] 배열크기 ->"3"
		///								  error[0] -> 최대 이송오차	(절대값)							
		///								  error[1] -> 평균 이송오차	(절대값)							
		///								  error[2] -> 동기 이송오차				</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")		</returns>	
		public int GetPositionStatus(int axis, int[] error)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				error[0] = 0;
				error[1] = 0;
				error[2] = 0;
				returnCode = eSnetGetPositionStatus(NetID, axis, out error[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetPositionStatus'. error : " + returnCode,
						"SnetDevice.GetErrorCode");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetPositionStatus");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Driver Connection Status (SNET-RTEX)

		/*** Status ***/

		/// <summary>
		/// RTEX Driver 상태 읽기
		/// : old version name -->"eSnetGetRtexDriverStatus"
		/// </summary>
		/// <param name="driver_status">	: [array] 배열크기 ->"4"
		///										[0] : driver connected count (1~ )
		///										[1] : "1"->driver connection ok, "0"->driver connection fail
		///										[2] : spare
		///										[3] : spare												</param>						
		/// <returns>						: (see enum "eSnetApiReturnCode")
		public int RtexGetDriverStatus(int[] driver_status)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexGetDriverStatus(NetID, out driver_status[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetDriverStatus'. error : " + returnCode,
						"SnetDevice.RtexGetDriverStatus");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetDriverStatus");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Servo On/Off

		/// <summary>
		/// Servo On 상태 설정
		/// : old version name ->"SetAxisServoEnable"
		/// </summary>
		/// <param name="axis">		: axis index							</param>						
		/// <param name="enable">	: "true"->servo on, "false"->servo off	</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		public int SetServoOn(int axis, bool enable)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (enable ? 1 : 0);

				returnCode = eSnetSetServoOn(NetID, axis, setValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetServoOn'. error : " + returnCode,
						"SnetDevice.SetServoOn");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetServoOn");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Servo On 상태 읽기
		/// : old version name ->"GetAxisServoEnable"
		/// </summary>
		/// <param name="axis">		: axis index							</param>						
		/// <param name="enable">	: "true"->servo on, "false"->servo off	</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		public int GetServoOn(int axis, ref bool enable)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetServoOn(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetServoOn'. error : " + returnCode,
						"SnetDevice.GetServoOn");
				}
				else
				{
					enable = (getValue == 1) ? true : false;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetServoOn");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        #endregion

        #region Homing (Step)

        /*** Origin ***/

        /// <summary>
        /// homing sequence motion moving step 설정 하기
        /// : old version name ->"SetOriginStep"
        /// </summary>
        /// <param name="axis">			: axis index																	</param>						
        /// <param name="step">			: "0"~ 부터 시작																	</param>						
        /// <param name="velocity">		: [mm/min]	: 속도																</param>
        /// <param name="accel">		: [msec]	: 가속도																</param>
        /// <param name="direction">	: "1"->(+)방향구동, "-1"->(-)방향 구동											</param>
        /// <param name="sensor">		: "0"->(-)limit_sensor,"1"->(+)limit_sensor, 
        ///								  "2"->origin_sensor, "3"->Z상   												</param>
        /// <param name="edge">			: "true"-->sensor 감지를 원할 때, "false"-->감지된 sensor가 감지 안 될 때를 원할 때	</param>
        /// <param name="dwell">		: [usec] : motion 구동 후 대기 시간(default : 500msec)							</param>		
        /// <returns>					: (see enum "eSnetApiReturnCode")												</returns>	
        public int AddHomingStep(int axis, int step, int velocity, int accel, int direction, int sensor, bool edge, int time_wait = 500)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setEdge = edge ? 1 : 0;
				returnCode = eSnetAddHomingStep(NetID, axis, step, velocity, accel, direction, sensor, setEdge, time_wait);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetAddHomingStep'. error : " + returnCode,
						"SnetDevice.AddHomingStep");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.AddHomingStep");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// homing sequence motion moving step 설정값 읽기
		/// : old version name ->"GetOriginStep"
		/// </summary>
		/// <param name="axis">			: axis index																		</param>						
		/// <param name="step">			: "0"~ 부터 시작																	</param>						
		/// <param name="velocity">		: [mm/min]	: 속도																	</param>
		/// <param name="accel">		: [msec]	: 가속도																</param>
		/// <param name="direction">	: "1"->(+)방향구동, "-1"->(-)방향 구동												</param>
		/// <param name="sensor">		: "0"->(-)limit_sensor, "1"->(+)limit_sensor, "2"->origin_sensor					</param>
		/// <param name="edge">			: "true"-->sensor 감지를 원할 때, "false"-->감지된 sensor가 감지 안 될 때를 원할 때	</param>
		/// <param name="dwell">		: [usec] : motion 구동 후 대기 시간(default : 500msec)								</param>		
		/// <returns>					: (see enum "eSnetApiReturnCode")													</returns>	
		public int GetHomingStep(int axis, int step, ref int velocity, ref int accel, ref int direction, ref int sensor, ref int port, ref int point, ref bool edge, ref int time_wait)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetHomingStep(NetID, axis, step, out int getValue0, out int getValue1, out int getValue2, out int getValue3, out int getValue4, out int getValue5, out int getValue6, out int getValue7);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetHomingStep'. error : " + returnCode,
						"SnetDevice.GetHomingStep");
				}
				else
				{
					velocity = getValue0;
					accel = getValue1;
					direction = getValue2;
					sensor = getValue3;
					port = getValue4;
					point = getValue5;
					edge = (getValue6 == 1);
					time_wait = getValue7;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetHomingStep");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        /// <summary>
        /// homing sequence zero offset 설정 하기
        /// : old version name ->"SetOriginZeroOffset"
        /// </summary>
        /// <param name="axis">			: axis number																			</param>						
        /// <param name="zero_offset">	: 마지막 STEP 완료 위치 부터 이송할 Offset 거리			
        ///								  homing sequence가 끝나고, 설정한 offset만큼 이동 후 "0"점으로 Set 한다				</param>						
        /// <param name="set_time">		: [usec] : offset만큼 이동 후, set_time[msec] 후에,"0"점으로 Set 한다(default: 500msec)	</param>		
        /// <returns>					: (see enum "eSnetApiReturnCode")														</returns>	
        public int SetHomingShift(int axis, int zero_offset, int set_time)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetHomingShift(NetID, axis, zero_offset, set_time);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetHomingShift'. error : " + returnCode,
						"SnetDevice.SetHomingShift");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetHomingShift");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        /// <summary>
        /// homing sequence zero offset 설정 하기
        /// </summary>
        /// <param name="axis">			: axis number																			</param>						
        ///	<param name="velocity">		: 속도(velocity)				: 단위 [mm/min]											</param>
        ///	<param name="accel">		: 가속도(acceleration)			: 단위 [msec]	(normal value = 100 ~ 300)				</param>
        ///	<param name="decel">		: 감속도(deceleration)			: 단위 [msec]	(normal value = 100 ~ 300)				</param>
        ///	<param name="jerk_acc">		: 가속저크(acceleration_jerk)	: 단위 [%]		(normal value = 66)						</param>
        ///	<param name="jerk_dec">		: 감속저크(deceleration_jerk)	: 단위 [%]		(normal value = 66)						</param>
        /// <param name="zero_offset">	: 마지막 STEP 완료 위치 부터 이송할 Offset 거리						
        ///								   homing sequence가 끝나고, 설정한 offset만큼 이동 후 "0"점으로 Set 한다				</param>						
        /// <param name="set_time">		: [usec] : offset만큼 이동 후, set_time[msec] 후에,"0"점으로 Set 한다(default: 500msec)	</param>		
        /// <returns>					: (see enum "eSnetApiReturnCode")														</returns>	
        public int SetHomingShiftEx(int axis, int velocity, int accel, int decel, int jerk_acc, int jerk_dec, int zero_offset, int set_time)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetSetHomingShiftEx(NetID, axis, velocity, accel, decel, jerk_acc, jerk_dec, zero_offset, set_time);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetSetHomingShiftEx'. error : " + returnCode,
                        "SnetDevice.SetHomingShiftEx");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.SetHomingShiftEx");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// homing sequence zero offset 설정값 읽기
        /// : old version name ->"GetOriginZeroOffset"
        /// </summary>
        /// <param name="axis">			: axis number																			</param>						
        /// <param name="zero_offset">	: 마지막 STEP 완료 위치 부터 이송할 Offset 거리			
        ///								  homing sequence가 끝나고, 설정한 offset만큼 이동 후 "0"점으로 Set 한다				</param>						
        /// <param name="set_time">		: [usec] : offset만큼 이동 후, set_time[msec] 후에,"0"점으로 Set 한다(default: 500msec)	</param>		
        /// <returns>					: (see enum "eSnetApiReturnCode")														</returns>	
        public int GetHomingShift(int axis, ref int zero_offset, ref int set_time)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetHomingShift(NetID, axis, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetHomingShift'. error : " + returnCode,
						"SnetDevice.GetHomingShift");
				}
				else
				{
					zero_offset = getValue0;
					set_time = getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetHomingShift");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        /// <summary>
        /// homing sequence zero offset 설정값 읽기
        /// : old version name ->"GetOriginZeroOffset"
        /// </summary>
        /// <param name="axis">			: axis number																			</param>						
        ///	<param name="velocity">		: 속도(velocity)				: 단위 [mm/min]											</param>
        ///	<param name="accel">		: 가속도(acceleration)			: 단위 [msec]	(normal value = 100 ~ 300)				</param>
        ///	<param name="decel">		: 감속도(deceleration)			: 단위 [msec]	(normal value = 100 ~ 300)				</param>
        ///	<param name="jerk_acc">		: 가속저크(acceleration_jerk)	: 단위 [%]		(normal value = 66)						</param>
        ///	<param name="jerk_dec">		: 감속저크(deceleration_jerk)	: 단위 [%]		(normal value = 66)						</param>
        /// <param name="zero_offset">	: 마지막 STEP 완료 위치 부터 이송할 Offset 거리			
        ///							      homing sequence가 끝나고, 설정한 offset만큼 이동 후 "0"점으로 Set 한다				</param>						
        /// <param name="set_time">		: [usec] : offset만큼 이동 후, set_time[msec] 후에,"0"점으로 Set 한다(default: 500msec)	</param>		
        /// <returns>					: (see enum "eSnetApiReturnCode")														</returns>	
        public int GetHomingShiftEx(int axis, ref int velocity, ref int accel, ref int decel, ref int jerk_acc, ref int jerk_dec, ref int zero_offset, ref int set_time)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetGetHomingShiftEx(NetID, axis, out int getValue0, out int getValue1, out int getValue2, out int getValue3,
					out int getValue4, out int getValue5, out int getValue6);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetGetHomingShiftEx'. error : " + returnCode,
                        "SnetDevice.GetHomingShiftEx");
                }
                else
                {
					velocity	= getValue0;
					accel		= getValue1;
					decel		= getValue2;
					jerk_acc	= getValue3;
					jerk_dec	= getValue4;
                    zero_offset = getValue5;
                    set_time	= getValue6;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.GetHomingShiftEx");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// homing sequence 시작 하기
        /// : old version name ->"GoOrigin"
        /// </summary>
        /// <param name="axis">	: axis index						</param>						
        /// <returns>			: (see enum "eSnetApiReturnCode")	</returns>	
        public int StartHoming(int axis)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetStartHoming(NetID, axis);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetStartHoming'. error : " + returnCode,
						"SnetDevice.StartHoming");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.StartHoming");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}
		/// <summary>
		/// homing sequence 중지 
		/// </summary>
		/// <param name="axis">	: axis index						</param>						
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>	

		public int StopHoming(int axis)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetStopHoming(NetID, axis);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetStopHoming'. error : " + returnCode,
						"SnetDevice.StopHoming");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.StopHoming");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// homing sequence 진행율
		/// (tip 1) Homing 도중 Event가 발생해서 Homing Sequence를 빠져 나와도 100(%)가 됩니다. 
		///         따라서, 반드시 "eSnetGetOriginResult"를 통해서 Homing결과를 확인해야 합니다.
		/// : old version name ->"GetOriginRate"
		/// </summary>
		/// <param name="axis">	: axis index						</param>						
		/// <param name="rate">	: [%] : homing 진행율				</param>						
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>	
		public int GetHomingRate(int axis, ref int rate)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetHomingRate(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetHomingRate'. error : " + returnCode,
						"SnetDevice.GetHomingRate");
				}
				else
				{
					rate = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetHomingRate");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// homing sequence 진행 상태와 결과 읽기
		/// (tip 1) "eSnetGetOriginRate"를 통해서 Homing Sequence가 100(%)가 되고 나서, 
		///         이 함수를 통해 잘 끝났는지 다른 Event가 발생했는지 반드시 확인해야 합니다. 
		/// : old version name ->"GetOriginResult"
		/// </summary>
		/// <param name="axis">			: axis index													</param>						
		/// <param name="step">			: 현재 진행중인 Moving Step("0"~:"0"번 부터 시작)				</param>						
		/// <param name="eventCode">	: (see "enum _eSnetOriginEvent")
		///									"0" : homing process 진행 중
		///									"1" : homing process가 error없이 완료
		///									"그 외의 값" : ("300"~) --> (see enum "eSnetApiCodeEvent")	</param>		
		/// <returns>					: (see enum "eSnetApiReturnCode")								</returns>	
		public int GetHomingResult(int axis, ref int step, ref int eventCode)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetHomingResult(NetID, axis, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetHomingResult'. error : " + returnCode,
						"SnetDevice.GetHomingResult");
				}
				else
				{
					step = getValue0;
					eventCode = getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetHomingResult");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 각 축별로 homing 이미 완료 되어 있는지의 상태
		/// : old version name ->"GetIsOriginDone"
		/// </summary>
		/// <param name="axis">	: axis index							</param>						
		/// <param name="done">	: homing완료 여부 : 
		///							"true"  : homing이 완료 되어 있음							
		///							"false" : homing이 완료 되지 않았음	</param>						
		/// <returns>				(see enum "eSnetApiReturnCode")		</returns>	
		public int GetIsHomingDone(int axis, ref bool done)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetIsHomingDone(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetIsHomingDone'. error : " + returnCode,
						"SnetDevice.GetIsHomingDone");
				}
				else
				{
					done = ((getValue == 1) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetIsHomingDone");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Homing (Method)

		/// <summary>
		/// homing method : homing 설정 하기
		/// : old version name ->"eSnetSetOriginMethod"
		/// : 사용을 권하지 않음
		/// </summary>
		/// <param name="axis">			: axis index																					</param>						
		/// <param name="direction">	: 첫번째 구동 Step이 "1"->(+)방향구동, "-1"->(-)방향 구동										</param>
		/// <param name="sensor">		: "0"->(-)limit_sensor, "1"->(+)limit_sensor, "2"->origin_sensor								</param>
		/// <param name="use_z">		: "false"->Z_Phase 사용 안함, "true"->Z_Phase 사용												</param>
		/// <param name="set_time">		: [usec] : offset만큼 이동 후, set_time[msec] 후에,"0"점으로 Set 한다(default: 500msec)			</param>	
		/// <param name="zero_shift">	: user가 Z상 또는 Sensor Off상태에서 얼마큼 더 이동 후에 Origion "0"점을 잡을지에 대한 위치값	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")																</returns>	
		public int SetHomingMethod(int axis, int direction, int sensor, bool use_z, int set_time, int zero_shift)
        {
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setUseZ = use_z ? 1 : 0;
				returnCode = eSnetSetHomingMethod(NetID, axis, direction, sensor, setUseZ, set_time, zero_shift);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetHomingMethod'. error : " + returnCode,
						"SnetDevice.SetHomingMethod");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetHomingMethod");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// homing method : homing 설정값 읽기
		/// : old version name ->"eSnetGetOriginMethod"
		/// : 사용을 권하지 않음
		/// </summary>
		/// <param name="axis">			: axis index																					</param>						
		/// <param name="direction">	: 첫번째 구동 Step이 "1"->(+)방향구동, "-1"->(-)방향 구동										</param>
		/// <param name="sensor">		: "0"->(-)limit_sensor, "1"->(+)limit_sensor, "2"->origin_sensor								</param>
		/// <param name="use_z">		: "false"->Z_Phase 사용 안함, "true"->Z_Phase 사용														</param>
		/// <param name="set_time">		: [usec] : offset만큼 이동 후, set_time[msec] 후에,"0"점으로 Set 한다(default: 500msec)			</param>	
		/// <param name="zero_shift">	: user가 Z상 또는 Sensor Off상태에서 얼마큼 더 이동 후에 Origion "0"점을 잡을지에 대한 위치값	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")																</returns>	
		public int GetHomingMethod(int axis, ref int direction, ref int sensor, ref bool use_z, ref int set_time, ref int zero_shift)
        {
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetHomingMethod(NetID, axis, out int getValue0, out int getValue1, out int getValue2, out int getValue3, out int getValue4);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetHomingMethod'. error : " + returnCode,
						"SnetDevice.GetHomingMethod");
				}
				else
				{
					direction = getValue0;
					sensor = getValue1;
					use_z = (getValue2 == 1);
					set_time = getValue3;
					zero_shift = getValue4;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetHomingMethod");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// homing method : homing 속도 설정 하기
		/// : old version name ->"eSnetSetOriginMethodSpeed"
		/// : 사용을 권하지 않음
		/// </summary>
		/// <param name="axis">			: axis index							</param>						
		/// <param name="velocity_0">	: [mm/min] 첫번째 구동 Step 속도		</param>
		/// <param name="velocity_1">	: [mm/min] 두번째 구동 Step 속도		</param>
		/// <param name="velocity_2">	: [mm/min] 세번재 구동 Step 속도		</param>
		/// <param name="velocity_3">	: [mm/min] 네번째 구동 Step 속도		</param>
		/// <param name="accel_0">		: [mm/min] 첫번째/두번째 구동 가속도	</param>
		/// <param name="accel_1">		: [mm/min] 세번째/네번째 구동 가속도	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")		</returns>	
		public int SetHomingMethodSpeed(int axis, int velocity_0, int velocity_1, int velocity_2, int velocity_3, int accel_0, int accel_1)
        {
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetHomingMethodSpeed(NetID, axis, velocity_0, velocity_1, velocity_2, velocity_3, accel_0, accel_1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetHomingMethodSpeed'. error : " + returnCode,
						"SnetDevice.SetHomingMethodSpeed");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetHomingMethodSpeed");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// homing method : homing 속도 설정값 읽기
		/// : old version name ->"eSnetGetOriginMethodSpeed"
		/// : 사용을 권하지 않음
		/// </summary>
		/// <param name="axis">			: axis index							</param>						
		/// <param name="velocity_0">	: [mm/min] 첫번째 구동 Step 속도		</param>
		/// <param name="velocity_1">	: [mm/min] 두번째 구동 Step 속도		</param>
		/// <param name="velocity_2">	: [mm/min] 세번재 구동 Step 속도		</param>
		/// <param name="velocity_3">	: [mm/min] 네번째 구동 Step 속도		</param>
		/// <param name="accel_0">		: [mm/min] 첫번째/두번째 구동 가속도	</param>
		/// <param name="accel_1">		: [mm/min] 세번째/네번째 구동 가속도	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")		</returns>	
		public int GetHomingMethodSpeed(int axis, ref int velocity_0, ref int velocity_1, ref int velocity_2, ref int velocity_3, ref int accel_0, ref int accel_1)
        {
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetHomingMethodSpeed(NetID, axis, out int getValue0, out int getValue1, out int getValue2,
					out int getValue3, out int getValue4, out int getValue5);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetHomingMethodSpeed'. error : " + returnCode,
						"SnetDevice.GetHomingMethodSpeed");
				}
				else
				{
					velocity_0 = getValue0;
					velocity_1 = getValue1;
					velocity_2 = getValue2;
					velocity_3 = getValue3;
					accel_0 = getValue4;
					accel_1 = getValue4;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetHomingMethodSpeed");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// homing method : homing 시작 하기
		/// : old version name ->"eSnetSetOriginMethodStart"\\
		/// : 사용을 권하지 않음
		/// </summary>
		/// <param name="axis">	: axis index						</param>						
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>	
		public int StartHomingMethod(int axis)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetStartHomingMethod(NetID, axis);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetStartHomingMethod'. error : " + returnCode,
						"SnetDevice.StartHomingMethod");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.StartHomingMethod");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Moving :: Single & Jog

		/// # move_type : moving profile type
		///		(int)eSnetMoveType::Scurve			: "1000"--> s_curve profile moving
		///		(int)eSnetMoveType::Trapezoidal		: "1001"--> trapezoidal profile moving
		///		(int)eSnetMoveType::Velocity		: "1002"--> velocity(jog) profile moving
		///		(int)eSnetMoveType::ScurveIo		: "1003"--> s_curve profile moving until setting input signal
		///		(int)eSnetMoveType::TrapezoidalIo	: "1004"--> trapezoidal profile moving until setting input signal
		///		(int)eSnetMoveType::VelocityIo		: "1005"--> velocity(jog) profile moving until setting input signal
		///				( see. "enum eSnetMoveType")		
		///	# velocity	: 속도(velocity)				: 단위 [mm/min]
		///	# accel		: 가속도(acceleration)			: 단위 [msec]	(normal value = 100 ~ 300)
		///	# decel		: 감속도(deceleration)			: 단위 [msec]	(normal value = 100 ~ 300)
		///	# jerk_acc	: 가속저크(acceleration_jerk)	: 단위 [%]		(normal value = 66)
		///	# jerk_dec	: 감속저크(deceleration_jerk)	: 단위 [%]		(normal value = 66)	
		///	# jerk		: 저크(가속/감속저크 같이 반영) : 단위 [%]		(normal value = 66) 	
		///	# direction : moving방향: "-1"-- > (-)방향, "1"-- > (+)방향
		///	# port		: "I/O 커넥터" 구분 번호 또는 "Remote IO Module"id 번호  
		///	# point		: 접점 번호 
		///	# area		: "0x1"--> IO 감지 영역, "0xf"--> Z(C)상 감지 영역
		///	# edge		: input signal 상태, "1"--> (uncheck--> check), "0"--> (check-->uncheck)
		/// # position	: 목표 위치

		/// <summary>
		/// 단축 구동(구조체 이용)
		/// : old version name ->"eSnetMoveAxis"
		/// </summary>
		/// <param name="axis">			: axis index						</param>
		/// <param name="move_type">	: (see enum "eSnetMoveType")		</param>
		/// <param name="data">			: (see struct "MOVE_COMPONENT_EX")		</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>		
		public int Move(int axis, eSnetMoveType move_type, MOVE_COMPONENT_EX data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				//IntPtr buffer = Marshal.AllocCoTaskMem(Marshal.SizeOf(data));
				//Marshal.StructureToPtr(data, buffer, false);
				returnCode = eSnetMove(NetID, axis, (int)move_type, out data);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetMove'. error : " + returnCode,
						"SnetDevice.Move");
				}
				//Marshal.FreeCoTaskMem(buffer);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.Move");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 단축 이송 강제 실행
		/// [정지 상태]: 구동 가능
		///				기존 이송 명령 실행
		/// [이송 상태]: 현재 위치, 속도부터 새 위치, 속도로 이송 명령 실행
		///				가속, 정속, 감속 구간에서도 구동
		/// </summary>
		/// <param name="net">:				controller id				</param>
		/// <param name="axis">:			axis index ("0"~ )			</param>
		/// <param name="relative">:		상대 이송 유무 				</param>
		/// @ "0" : 절대 이송
		/// @ "1" : 상대 이송
		/// <param name="moving_option">:	이송 중 명령 적용 시점		</param>
		/// @ "0" : 즉시 적용
		/// @ "1" : 이송 중일 경우, 정속 구간 도달 시 적용
		/// <param name="profile_type">:	속도 프로파일					</param>
		/// @ "1000" : S-Curve
		/// @ "1001" : Trapezoidal
		/// <param name="velocity">:		[mm/min] override 속도		</param>
		/// <param name="accel">:			[msec] override 가속 시간	</param>
		/// <param name="decel">:			[msec] override 감속 시간	</param>
		/// <param name="acc_jerk">:		[%] 가속 시 jerk				</param>
		/// <param name="dec_jerk">:		[%] 감속 시 jerk				</param>
		/// <param name="position">:		[um] 변경할 위치				</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")					
		///										MoveExBase					(9000)
		///										MoveExInvalidArgument		(9001)
		///										MoveExInvalidAxis			(9003)
		///										MoveExParameter				(9010)
		///										MoveExInvalidProfile		(9011)
		///										MoveExInterpolation			(9020)
		///										MoveExExSetSlave			(9021)
		///										MoveExNoVelocity			(9050)
		///										MoveExNoAccelTime			(9051)
		///										MoveExNoJerk				(9052)
		///										MoveExInvalidPosition		(9060)
		///										MoveExAlarm					(9072)
		///										MoveExOtherCommand			(9073)
		///										MoveExAxisBusy				(9075)
		///										MoveExSensorLimitPositive	(9078)
		///										MoveExSensorLimitNegative	(9079)	 </returns>
		public int MoveEx(int axis, int relative, int moving_option, int profile_type, int velocity, int accel, int decel, int acc_jerk, int dec_jerk, int position)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetMoveEx(NetID, axis, relative, moving_option, profile_type, velocity, accel, decel, acc_jerk, dec_jerk, position);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetMoveEx'. error : " + returnCode,
						"SnetDevice.MoveEx");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.MoveEx");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 단축 구동(구조체 이용)
		/// : old version name -->"eSnetMoveAxisPast"
		/// </summary>
		/// <param name="axis">			: axis index						</param>
		/// <param name="move_type">	: (see enum "eSnetMoveType")		</param>
		/// <param name="data">			: (see struct "MOVE_COMPONENT")		</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>		
		public int MoveSingle(int axis, eSnetMoveType move_type, MOVE_COMPONENT data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				//IntPtr buffer = Marshal.AllocCoTaskMem(Marshal.SizeOf(data));
				//Marshal.StructureToPtr(data, buffer, false);
				returnCode = eSnetMoveSingle(NetID, axis, (int)move_type, out data);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetMoveSingle'. error : " + returnCode,
						"SnetDevice.MoveSingle");
				}
				//Marshal.FreeCoTaskMem(buffer);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.MoveSingle");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 단축 구동
		/// : old version name -->"eSnetMoveAxisPast2"
		/// </summary>
		/// <param name="axis">			: axis index						</param>
		/// <param name="move_type">	: (see enum "eSnetMoveType")		</param>
		/// <param name="velocity">		: [mm/min]	velocity				</param>
		/// <param name="accel">		: [msec]	acceleration			</param>
		/// <param name="decel">		: [msec]	deceleration			</param>
		/// <param name="jerk">			: [%]		jerk					</param>
		/// <param name="position">		: [um]		position				</param>		
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>			
		public int MoveSingleEx(int axis, eSnetMoveType move_type, int velocity, int accel, int decel, int jerk, int position)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetMoveSingleEx(NetID, axis, (int)move_type, velocity, accel, decel, jerk, position);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetMoveSingleEx'. error : " + returnCode,
						"SnetDevice.MoveSingleEx");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.MoveSingleEx");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 단축 구동(설정된 Input Signal이 입력 될 때까지 구동)
		/// # move_type = 1003 --> "direction"인자로 전달된 "위치"로 S-CURVE 가감속 이송,입력 조건을 만족 하면 정지  
		/// # move_type = 1004 --> "direction"인자로 전달된 "위치"로 사다리꼴 가감속 이송,입력 조건을 만족 하면 정지  
		/// # move_type = 1005 --> "direction"인자로 전달된 "방향(1 or -1)" 으로 입력 조건을 만족 할 때 까지 "무한 이송" 
		/// # 입력 조건을 만족 하여 이송을 정지 할때 감속 시간은 제어기 "21번 파라미터"설정값이 적용 됨. 
		/// : old version name -->"eSnetMoveAxisPast2_IO"
		/// </summary>
		/// <param name="axis">			: axis index											</param>
		/// <param name="move_type">	: (see enum "eSnetMoveType"), 
		///									ScurveIo		(1003),
		///									TrapezoidalIo	(1004),
		///									VelocityIo		(1005),								</param>
		/// <param name="velocity">		: [mm/min]	velocity									</param>
		/// <param name="accel">		: [msec]	acceleration[msec]							</param>
		/// <param name="decel">		: [msec]	deceleration								</param>
		/// <param name="jerk">			: [%]		jerk										</param>
		/// <param name="direction">	: [um]		position									</param>		
		/// <param name="port">			: input port											</param>		
		/// <param name="point">		: input point											</param>		
		/// <param name="area">			: "0x1" or "0xf"(see enum "eSnetOriginEtc")				</param>		
		/// <param name="edge">			: input point edge, low->high:"true", high->low:"false"	</param>				
		/// <returns>					: (see enum "eSnetApiReturnCode")						</returns>	
		public int MoveSingleExIo(int axis, eSnetMoveType move_type, int velocity, int accel, int decel, int jerk, int direction, int port, int point, int area, bool edge)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setEdge = edge ? 1 : 0;
				returnCode = eSnetMoveSingleExIo(NetID, axis, (int)move_type, velocity, accel, decel, jerk, direction, port, point, area, setEdge);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetMoveSingleExIo'. error : " + returnCode,
						"SnetDevice.MoveSingleExIo");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.MoveSingleExIo");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 단축 구동 (jog mode)
		/// : old version name ->"eSnetMoveAxisJog"
		/// </summary>
		/// <param name="axis">			: axis index						</param>		
		/// <param name="velocity">		: [mm/min]	velocity				</param>
		/// <param name="accel">		: [msec]		acceleration		</param>
		/// <param name="decel">		: [msec]		deceleration		</param>
		/// <param name="jerk">			: [%]			jerk				</param>
		/// <param name="direction">	: "-1" : negative direction				
		///								: " 1" : positive direvtion 
		///								(see enum "eSnetMoveDirection")		</param>					
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>		
		public int MoveAxisSingleExJog(int axis, int velocity, int accel, int decel, int jerk, eSnetMoveDirection direction)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetMoveSingleExJog(NetID, axis, velocity, accel, decel, jerk, (int)direction);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetMoveAxisSingleExJog'. error : " + returnCode,
						"SnetDevice.MoveAxisSingleExJog");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.MoveAxisSingleExJog");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 다축 동시 구동 지령 
		/// : old version name ->"eSnetMoveAxisMulti"
		/// </summary>
		/// <param name="axis_count">	: 구동할 축 수												</param>
		/// <param name="axis">			: [array] 배열크기->ax_count,구동할 축 배열					</param>
		/// <param name="move_type">	: [array] 배열크기->ax_count,구동할 축의 Profile Type			
		///									(see "enum eSnetMoveType"),"1003"/"1004"/"1005"만 사용	</param>
		/// <param name="velocity">		: [array] 배열크기->ax_count, [mm/min] velocity				</param>
		/// <param name="accel">		: [array] 배열크기->ax_count, [msec] acceleration			</param>
		/// <param name="decel">		: [array] 배열크기->ax_count, [msec] deceleration			</param>
		/// <param name="jerk_acc">		: [array] 배열크기->ax_count, [%] accel_jerk				</param>
		/// <param name="jerk_dec">		: [array] 배열크기->ax_count, [%] decel_jerk				</param>
		/// <param name="position">		: [array],배열크기->ax_count, [um] target_position			</param>		
		/// <returns>					: (see enum "eSnetApiReturnCode")							</returns>
		public int MoveMultiAxis(int axis_count, int[] axis, int[] move_type, int[] velocity, int[] accel, int[] decel, int[] jerk_acc, int[] jerk_dec, int[] position)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetMoveMultiAxis(NetID, axis_count, out axis[0], out move_type[0], out velocity[0], out accel[0], out decel[0], out jerk_acc[0], out jerk_dec[0], out position[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetMoveMultiAxis'. error : " + returnCode,
						"SnetDevice.MoveMultiAxis");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.MoveMultiAxis");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Moving :: Single with AD

		/// <summary>
		/// 단축 구동 ( 무한 이송 / Ad 입력값 비교 후 정지 )
		/// (tip 1) "SNET-RTEX 옵션 보드 실장 제품" 및 "SNET-P AD 옵션 보드 실장 제품"에 적용 됩니다.  
		/// : old version name -->"eSnetMoveAxisVel_Ad"
		/// </summary>
		/// <param name="axis">			: axis index ("0" ~ )																</param>		
		/// <param name="velocity">		: [mm/min]	velocity																</param>
		/// <param name="accel">		: [msec]	acceleration time														</param>
		/// <param name="decel">		: [msec]	deceleration time														</param>
		/// <param name="direction">	: direction : "1" or "-1"															</param>							
		/// <param name="ad_index">		: AD0(0) ~ AD3(3)																	</param>							
		/// <param name="ad_value">		: [mVolt] Ad voltage value 															</param>							
		/// <returns>					: (see enum "eSnetApiReturnCode")	
		///									CommunicationEventCode,
		///									MoveVelocityAdAxisBusy	= 1901,	// 인수로 전달된 축이 현재 사용 중 
		///									MoveVelocityAdNumber	= 1902,	// AD 번호 설정 오류 ( 0 ~ 3 ) 
		///									MoveVelocityAdDirection	= 1903,	// 인수로 전달된 축 이송 방향 오류 ( 1 or -1 ) 
		///									MoveVelocityAdVelocity	= 1904,	// 인수로 전달된 축 이송 속도 오류 
		///									MoveVelocityAdOthers	= 1999, // 기타 오류 							</returns>	
		public int MoveVelocityAd(int axis, int velocity, int accel, int decel, int direction, int ad_index, int ad_value)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetMoveVelocityAd(NetID, axis, velocity, accel, decel, direction, ad_index, ad_value);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetMoveVelocityAd'. error : " + returnCode,
						"SnetDevice.MoveVelocityAd");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.MoveVelocityAd");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 단축 구동 ( 위치 이송 / Ad 입력값 비교 후 정지 )
		/// : old version name -->"eSnetMoveAxisPos_Ad"
		/// </summary>
		/// <param name="axis">			: axis index ("0" ~ )																	</param>		
		/// <param name="velocity">		: [mm/min]	velocity																	</param>
		/// <param name="accel">		: [msec]	acceleration time															</param>
		/// <param name="decel">		: [msec]	deceleration time															</param>
		/// <param name="jerk">			: jerk																					</param>							
		/// <param name="position">		: target position																		</param>							
		/// <param name="ad_index">		: AD0(0) ~ AD3(3)																		</param>							
		/// <param name="ad_value">		: [mVolt] Ad voltage value																</param>							
		/// <returns>					: (see enum "eSnetApiReturnCode")	
		///									CommunicationEventCode,
		///									MoveVelocityAdAxisBusy	(1901),	// 인수로 전달된 축이 현재 사용 중 
		///									MoveVelocityAdNumber	(1902),	// AD 번호 설정 오류 ( 0 ~ 3 ) 
		///									MoveVelocityAdDirection	(1903),	// 인수로 전달된 축 이송 방향 오류 ( 1 or -1 ) 
		///									MoveVelocityAdVelocity	(1904),	// 인수로 전달된 축 이송 속도 오류 
		///									MoveVelocityAdOthers	(1999), // 기타 오류 								</returns>	
		public int MovePositionAd(int axis, int velocity, int accel, int decel, int jerk, int position, int ad_index, int ad_value)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetMovePositionAd(NetID, axis, velocity, accel, decel, jerk, position, ad_index, ad_value);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetMovePositionAd'. error : " + returnCode,
						"SnetDevice.MovePositionAd");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.MovePositionAd");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Moving :: Interpolation

		/// <summary>
		/// 직선 보간 이송 지령
		/// : 최대 4축 보간 이송
		/// : old version name ->"eSnetMoveAxisInterpolationLine"
		/// </summary>
		/// <param name="axis0">			: 첫 번째 축 번호, 사용하지 않을 때 "0xff"	</param>
		/// <param name="axis1">			: 두 번째 축 번호, 사용하지 않을 때 "0xff"	</param>
		/// <param name="axis2">			: 세 번째 축 번호, 사용하지 않을 때 "0xff"	</param>
		/// <param name="axis3">			: 네 번째 축 번호, 사용하지 않을 때 "0xff"	</param>
		/// <param name="axis0_position">	: 첫 번째 축(axis_1) 이송 좌표				</param>
		/// <param name="axis1_position">	: 두 번째 축(axis_2) 이송 좌표				</param>
		/// <param name="axis2_position">	: 세 번째 축(axis_3) 이송 좌표				</param>
		/// <param name="axis3_position">	: 네 번째 축(axis_4) 이송 좌표				</param>
		/// <param name="velocity">			: [mm/min]	보간 이송 속도					</param>
		/// <param name="accel">			: [msec]	가속시간							</param>
		/// <param name="decel">			: [msec]	감속시간							</param>
		/// <param name="jerk_acc">			: [%]		가속시간 내 jerk 비율			</param>
		/// <param name="jerk_dec">			: [%]       감속시간 내 jerk 비율			</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")
		///										
		///										InterpolationTrajFull (1100) ~
		///										InterpolationMovingPostionZero (1112)	</returns>
		public int MoveLine(int axis0, int axis1, int axis2, int axis3,
			int axis0_position, int axis1_position, int axis2_position, int axis3_position,
			int velocity, int accel, int decel, int jerk_acc, int jerk_dec)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetMoveLine(NetID, axis0, axis1, axis2, axis3, axis0_position, axis1_position, axis2_position, axis3_position,
					velocity, accel, decel, jerk_acc, jerk_dec);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetMoveLine'. error : " + returnCode,
						"SnetDevice.MoveLine");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.MoveLine");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 직선 보간 이송 지령 
		/// : ( "SNET-P8" -> 최대 8축, "SNET-RTEX" -> 최대 32축)
		/// : old version name -->"eSnetMoveAxisInterpolationLine2"
		/// </summary>
		/// <param name="axis_count">	: 보간 이송을 할 축 갯수						</param>
		/// <param name="axis">			: [array]	배열크기->axCount,축 번호			</param>
		/// <param name="position">		: [array]	배열크기->axCount,[um] 이송 위치	</param>
		/// <param name="velocity">		: [mm/min]  보간 이송 속도						</param>
		/// <param name="velocity">		: [mm/min]	보간 이송 속도						</param>
		/// <param name="accel">		: [msec]	가속시간							</param>
		/// <param name="decel">		: [msec]	감속시간							</param>
		/// <param name="jerk_acc">		: [%]		가속시간 내 jerk 비율				</param>
		/// <param name="jerk_dec">		: [%]       감속시간 내 jerk 비율				</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")				
		///									
		///									InterpolationTrajFull (1100) ~
		///									InterpolationMovingPositionZero (1112)		</returns>
		public int MoveLineMultiAxis(int axis_count, int[] axis, int[] position, int velocity, int accel, int decel, int jerk_acc, int jerk_dec)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				if (axis.Length < axis_count)
					return (int)eSnetApiReturnCode.InvalidArgument;
				if (position.Length < axis_count)
					return (int)eSnetApiReturnCode.InvalidArgument;

				returnCode = eSnetMoveLineMultiAxis(NetID, axis_count, out axis[0], out position[0], velocity, accel, decel, jerk_acc, jerk_dec);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetoveLineMultiAxis'. error : " + returnCode,
						"SnetDevice.MoveLineMultiAxis");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.MoveLineMultiAxis");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 반지름과 종점을 이용한 원호 보간
		/// : old version name ->"eSnetMoveAxisInterpolationArcRadius"
		/// </summary>
		/// <param name="axis0">			: XY평면좌표 첫번째 축 번호											</param>
		/// <param name="axis1">			: XY평면좌표 두번째 축 번호											</param>
		/// <param name="axis2">			: XY평면 제외 직선 보간 축, 사용하지 않을 경우 "0xff"입력			</param>
		/// <param name="axis0_position">	: XY평면좌표 첫번째 축 종점 위치									</param>
		/// <param name="axis1_position">	: XY평면좌표 두번째 축 종점 위치									</param>
		/// <param name="axis2_position">	: XY평면 제외 직선 보간 축 종점 위치, 사용하지 않을 경우 "0"입력	</param>
		/// <param name="cw_ccw">			: "1"-->XY평명좌표   시계방향(cw),  (1-->4-->3-->2 사분면 방향), 
		/// 								 "-1"-->XY평면좌표 반시계방향(ccw), (1-->2-->3-->4 사분면 방향)		</param>
		/// <param name="radius">			: [um]		원호 반지름												</param>		 
		/// <param name="velocity">			: [mm/min]	보간 이송 속도											</param>
		/// <param name="accel">			: [msec]	가속시간												</param>
		/// <param name="decel">			: [msec]	감속시간												</param>
		/// <param name="jerk_acc">			: [%]		가속시간 내 jerk 비율									</param>
		/// <param name="jerk_dec">			: [%]       감속시간 내 jerk 비율									</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")					
		///										 
		///										InterpolationTrajFull			(1100) ~ 
		///										InterpolationMovingPositionZero	(1112)							</returns>	
		public int MoveArcRadius(int axis0, int axis1, int axis2,
			int axis0_position, int axis1_position, int axis2_position, int cw_ccw, int radius,
			int velocity, int accel, int decel, int jerk_acc, int jerk_dec)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetMoveArcRadius(NetID, axis0, axis1, axis2, axis0_position, axis1_position, axis2_position,
					cw_ccw, radius, velocity, accel, decel, jerk_acc, jerk_dec);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetMoveArcRadius'. error : " + returnCode,
						"SnetDevice.MoveArcRadius");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.MoveArcRadius");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        /// <summary>
        /// 중심점과 각도를 이용한 원호보간
        /// : old version name ->"eSnetMoveAxisInterpolationArcAngle"
        /// </summary>
        /// <param name="axis0">			: XY평면좌표 첫번째 축 번호									</param>
        /// <param name="axis1">			: XY평면좌표 두번째 축 번호									</param>
        /// <param name="axis2">			: XY평면 제외 직선 보간 축, 사용하지 않을 경우 "0xff"입력		</param>
        /// <param name="center_x">			: XY평면좌표 중심점 X 위치									</param>
        /// <param name="center_y">			: XY평면좌표 중심점 Y 위치									</param>
        /// <param name="angle">			: 각도, (+)각->XY평면에서 ccw방향, (-)각->XY평면에서 cw방향	</param>		
        /// <param name="axis2_position">	: 직선 보간 축 위치, 사용하지 않을 경우 "0"입력				</param>
        /// <param name="velocity">			: [mm/min]	보간 이송 속도									</param>
        /// <param name="accel">			: [msec]	가속시간											</param>
        /// <param name="decel">			: [msec]	감속시간											</param>
        /// <param name="jerk_acc">			: [%]		가속시간 내 jerk 비율							</param>
        /// <param name="jerk_dec">			: [%]       감속시간 내 jerk 비율							</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")					
        ///										 
        ///										InterpolationTrajFull			(1100) ~ 
        ///										InterpolationMovingPositionZero	(1112)					</returns>	
        public int MoveArcAngle(int axis0, int axis1, int axis2,
			int center_x, int center_y, int angle, int axis2_position, int velocity, int accel, int decel, int jerk_acc, int jerk_dec)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetMoveArcAngle(NetID, axis0, axis1, axis2, center_x, center_y, angle, axis2_position,
					velocity, accel, decel, jerk_acc, jerk_dec);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetMoveArcAngle'. error : " + returnCode,
						"SnetDevice.MoveArcAngle");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.MoveArcAngle");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 중간 이동점을 이용한 원호 보간
		/// : old version name ->"eSnetMoveAxisInterpolationArcPoint"
		/// </summary>
		/// <param name="axis0">			: XY평면좌표 첫번째 축 번호									</param>
		/// <param name="axis1">			: XY평면좌표 두번째 축 번호r									</param>
		/// <param name="axis2">			: XY평면 제외 직선 보간 축, 사용하지 않을 경우 "0xff"입력		</param>
		/// <param name="axis0_midpos">		: XY평면좌표 중간점 X 위치									</param>
		/// <param name="axis1_midpos">		: XY평면좌표 중간점 Y 위치									</param>
		/// <param name="axis0_endpos">		: XY평면좌표 종점 X 위치										</param>
		/// <param name="axis1_endpos">		: XY평면좌표 종점 Y 위치										</param>
		/// <param name="axis2_endpos">		: 직선 보간 축 종점 좌표, 사용하지 않을 경우 "0"입력			</param>
		/// <param name="velocity">			: [mm/min]	보간 이송 속도									</param>
		/// <param name="accel">			: [msec]	가속시간											</param>
		/// <param name="decel">			: [msec]	감속시간											</param>
		/// <param name="jerk_acc">			: [%]		가속시간 내 jerk 비율							</param>
		/// <param name="jerk_dec">			: [%]       감속시간 내 jerk 비율							</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")					
		///										 
		///										InterpolationTrajFull			(1100) ~ 
		///										InterpolationMovingPositionZero	(1112)					</returns>	
		public int MoveArcPoint(int axis0, int axis1, int axis2,
			int axis0_midpos, int axis1_midpos, int axis0_endpos, int axis1_endpos, int axis2_endpos,
			int velocity, int accel, int decel, int jerk_acc, int jerk_dec)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetMoveArcPoint(NetID, axis0, axis1, axis2, axis0_midpos, axis1_midpos,
					axis0_endpos, axis1_endpos, axis2_endpos, velocity, accel, decel, jerk_acc, jerk_dec);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetMoveArcPoint'. error : " + returnCode,
						"SnetDevice.MoveArcPoint");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.MoveArcPoint");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        #endregion

        #region Moving :: Helical

        /// <summary>
        /// 헬리컬 구동 하기
        /// </summary>				
        /// <param name="axis0">			: 원호 보간을 수행할 X axis index					</param>						
        /// <param name="axis1">			: 원호 보간을 수행할 Y axis index					</param>						
        /// <param name="axis2">			: 원호 보간과 함께 직선 보간을 수행할 Z axis index	</param>		
        /// <param name="center_x">			: 중심점 X 								</param>						
        /// <param name="center_y">			: 중심점 Y 								</param>					
        /// <param name="rotation_count">	: 회전수									</param>						
        /// <param name="angle">			: "rotation_count" 회전후 추가 이송 각도, (+)각 ccw방향, (-)각 cw방향</param>					
        /// <param name="axis2_position">	: 한 회전 마다 움직일 Z 측거리						</param>						
        /// <param name="cw_ccw">			: 회전 방향 : "-1"->반시계방향, "1"->시계방향		</param>				
        /// <param name="velocity">			: [mm/min]	보간 이송 속도							</param>
        /// <param name="accel">			: [msec]	가속시간								</param>
        /// <param name="decel">			: [msec]	감속시간								</param>
        /// <param name="jerk_acc">			: [%]		가속시간 내 jerk 비율					</param>
        /// <param name="jerk_dec">			: [%]       감속시간 내 jerk 비율					</param>	
        /// <returns>						: (see enum "eSnetApiReturnCode")					
        ///										 
        ///										InterpolationTrajFull			(1100) ~ 
        ///										InterpolationMovingPositionZero	(1112)			</returns>		
		public int MoveHelical(int axis0, int axis1, int axis2, int center_x, int center_y, int rotation_count,
                 int angle, int axis2_position, int cw_ccw, int velocity, int accel, int decel, int jerk_acc, int jerk_dec)
        {
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetMoveHelical(NetID, axis0, axis1, axis2, center_x, center_y,
					rotation_count, angle, axis2_position, cw_ccw,
					velocity, accel, decel, jerk_acc, jerk_dec);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetMoveHelical' . error : " + returnCode,
						"SnetDevice.MoveHelical");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.MoveHelical");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        #endregion

        #region Moving :: Spline

        /// <summary>
        /// 스플라인 구동 하기
        /// </summary>
        /// <param name="axis_x">			: (X, Y)평면좌표에서 스플라인 구동할 X방향 축										</param>
        /// <param name="axis_y">			: (X, Y)평면좌표에서 스플라인 구동할 Y방향 축										</param>						
        /// <param name="point_count">		: spline을 수행할 (x, y) 지령 points, 최소 3점 이상, 40점 이하						</param>
        /// <param name="x_points">			: array 배열, x point, 배열[0]이 첫번째 X 시작점									</param>
        /// <param name="y_points">			: array 배열, y point, 배열[0]dl 첫번째 Y 시작점									</param>
        /// <param name="sampling_count">	: Spline 곡선 계산식으로 산출될 미소 구간 Point 개수, 최소 4개, 최대 200개			</param>
        /// <param name="xs">				: Spline 곡선 계산식으로 산출된 미소 구간 Point의 "axis_x 축" 좌표 저장 배열 
        ///									  배열 크기 -> "sampling_count"													</param>
        /// <param name="ys">				: Spline 곡선 계산식으로 산출된 미소 구간 Point의 "axis_y 축" 좌표 저장 배열  
        ///									  배열 크기 -> "sampling_count"													</param> 	
        /// <param name="type">				: Spline 곡선 종류, "0" 고정(Cubic Spline)										</param> 	
        ///	<param name="axis3">			: Spline 곡선 평면 추과 동기 되어 움직이는, 추가 직선 보간 축 번호					</param> 	
        ///	<param name="axis3_positions">	: "axis3"축의 시작 좌표와 종료 좌표, 배열  
        ///									  배열 크기 ->2, axis3_position[0] ->시작 좌표, axis3_position[1] ->종료 좌표		</param> 	
        /// <param name="velocity">			: [mm/min]	보간 이송 속도														</param>
        /// <param name="accel">			: [msec]	가속시간																</param>
        /// <param name="decel">			: [msec]	감속시간																</param>
        /// <param name="jerk_acc">			: [%]		가속시간 내 jerk 비율													</param>
        /// <param name="jerk_dec">			: [%]       감속시간 내 jerk 비율													</param>	
        /// <returns>						: (see enum "eSnetApiReturnCode")					
        ///										SplineSamplingCountOver				(2501) ~						
        ///										SplineMovingCommandPositionError	(2508)																</returns>
        public int MoveSpline(int axis_x, int axis_y, int point_count, int[] x_points, int[] y_points,
			int sampling_count, float[] xs, float[] ys, eSnetSplineType type,
			int axis3, int[] axis3_positions,
			int velocity, int accel, int decel, int jerk_acc, int jerk_dec)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setType = (int)eSnetSplineType.CubicBasic;
				if (type == eSnetSplineType.CubicHermite)
					setType = 1;

				returnCode = eSnetMoveSpline(NetID, axis_x, axis_y, point_count, out x_points[0], out y_points[0],
					sampling_count, out xs[0], out ys[0], setType,
					axis3, out axis3_positions[0], velocity, accel, decel, jerk_acc, jerk_dec);

				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetMoveSpline' . error : " + returnCode,
						"SnetDevice.MoveSpline");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.MoveSpline");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Moving :: Continuous

		///  연속 보간 이송
		///  # "eSnetBeginContiMakeJob"과 "eSnetEndContiMakeJob" 사이에 설정할 수 있는 함수 
		///		-보간 이송 -------------------------------	
		///		- eSnetMoveLine()
		///		- eSnetMoveLineMultiAxis()
		///		- eSnetMoveAcrRadius()
		///		- eSnetMoveArcAngle()
		///		- eSnetMoveArcPoint()
		///		- eSnetMoveHelical()
		///		- eSnetMoveSpline()
		///		-I/O--------------------------------------
		///		- eSnetSetContiOutputConfig()
		///		- eSnetRtexSetIoOutputPort
		///		- eSnetRtexSetIoOutputPoint()
		///		- eSnetPulseExSetIoOutputPoint()
		///		- eSnetPulseExSetIoOutputPort()
		///     - eSnetPulseSetUserOutputPoint()
		///		- eSnetSetOutputForTime()
		///		-기타--------------------------------------
		///		- eSnetSetAxisAbsRelMode()
		///		- eSnetSetContiDwell()

		/// <summary>
		/// 연속 보간 이송 채널 번호 변경 
		/// </summary>
		/// <param name="channel">	: channel index								</param>
		///							: SNET-P->"0 ~ 1",SNET-RTEX->"0 ~ 3"	
		/// <returns>				: (see enum "eSnetApiReturnCode")
		///								CommunicationEventCode, 
		///								ContinuousOverCount			(1201) ~ 
		///								ContinuousNotBeginCommand	(1203)		</returns>	
		public int SetContiCh(int channel)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetContiCh(NetID, channel);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetContiCh'. error : " + returnCode,
						"SnetDevice.SetContiCh");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetContiCh");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 연속 보간 이송 현재 채널 번호 확인
		/// </summary>
		/// <param name="channel">	: channel index						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		public int GetContiCh(ref int channel)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetContiCh(NetID, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetContiCh'. error : " + returnCode,
						"SnetDevice.GetContiCh");
				}
				else
				{
					channel = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetContiCh");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 연속 보간 이송 Job 만들기 시작
		/// : old version name -->"eSnetSetContiMakeJobBegin"
		/// </summary>
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>
		public int BeginContiMakeJob()
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetBeginContiMakeJob(NetID);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetBeginContiMakeJob'. error : " + returnCode,
						"SnetDevice.BeginContiMakeJob");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.BeginContiMakeJob");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 연속 보간 이송 Job 만들기 종료
		/// : old version name -->"eSnetSetContiMakeJobEnd"
		/// </summary>
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>
		public int EndContiMakeJob()
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetEndContiMakeJob(NetID);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetEndContiMakeJob'. error : " + returnCode,
						"SnetDevice.EndContiMakeJob");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.EndContiMakeJob");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 연속 보간 이송을 위해 설정한 API 전체 갯수와 Moving API 갯수
		/// </summary>
		/// <param name="all_index_count">		: 연속 이송 보간을 위해 설정한 전체 API 갯수									</param>						
		/// <param name="moving_step_count">	: 연속 이송 보간을 위해 설정한 전체 API중에서 Motion Moving에 관련된 API 갯수	</param>						
		/// <returns>							: (see enum "eSnetApiReturnCode")	</returns>
		public int GetContiJobIndexCount(ref int all_index_count, ref int moving_step_count)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetContiJobIndexCount(NetID, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetContiJobIndexCount'. error : " + returnCode,
						"SnetDevice.GetContiJobIndexCount");
				}
				else
				{
					all_index_count = getValue0;
					moving_step_count = getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetContiJobIndexCount");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 선택된 채널("eSnetSetContiCh")의 연속 보간 이송 Job 시작 하기
		/// </summary>
		/// <param name="channel">	: channel index to start job		</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int StartConti(int channel)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetStartConti(NetID, channel);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetStartConti'. error : " + returnCode,
						"SnetDevice.StartConti");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.StartConti");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 연속 보간 이송 정지
		/// </summary>
		/// <param name="channel">	: 연속 보간 이송 채널 번호				
		//								SNET-P->"0 ~ 1",SNET-RTEX->"0 ~ 3"	</param>
		/// <param name="decel">	: 감속 시간								</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		public int StopConti(int channel, int decel)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetStopConti(NetID, channel, decel);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetStopConti'. error : " + returnCode,
						"SnetDevice.StopConti");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.StopConti");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "연속 보간 이송"이 진행 중인지, 진행 중이라면 몇번째 Moving Step 진행 중인지 읽기
		/// </summary>
		/// <param name="is_moving">			: 연속 보간 이송 동작 중인가? "true"->동작중, "false"->동작중 아님	</param>						
		/// <param name="current_motion_step">	: 현재 몇번째 Motion Moving Step 중인가?							</param>						
		/// <returns>							: (see enum "eSnetApiReturnCode")									</returns>
		public int GetContiIsMotion(ref bool is_moving, ref int current_motion_step)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetContiIsMotion(NetID, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetContiIsMotion'. error : " + returnCode,
						"SnetDevice.GetContiIsMotion");
				}
				else
				{
					is_moving = ((getValue0 == 1) ? true : false);
					current_motion_step = getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetContiIsMotion");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "연속 보간 이송"이  진행 중인지, 진행 중이라면 몇번째 Moving Step 진행 중인지, 어떤 Moving Command로 구동 중인지, 연속이송보간의 결과가 어떤지 읽기
		/// </summary>
		/// <param name="channel"> 					: channel index 															</param>						
		/// <param name="is_moving">				: 연속 보간 이송 동작 중인가? "true"->동작중, "false"->동작중 아님			</param>						
		/// <param name="current_motion_step">		: 현재 몇번째 Motion Moving Step 중인가?									</param>						
		/// <param name="current_motion_command">	: 현재 구동 중인 Motion이 어떤 Moving Command Id 인가?		
		///												(see enum "eSnetContiMovingInfo")										</param>
		///												(int)eSnetContiMovingInfo::MovingDoneOk(0),	// 연속 보간 이송 구동이 정상적으로 종료됨
		///												(int)eSnetContiMovingInfo::FuncMoveAxisInterpolationLine		(3300),
		///												(int)eSnetContiMovingInfo::FuncMoveAxisInterpolationArcRadius	(3310),
		///												(int)eSnetContiMovingInfo::FuncMoveAxisInterpolationArcAngle	(3311),
		///												(int)eSnetContiMovingInfo::FuncMoveAxisInterpolationArcPoint	(3312),
		///												(int)eSnetContiMovingInfo::FuncMoveAxisInterpolationLineEx		(3315),
		///												(int)eSnetContiMovingInfo::FuncSetContiDwell					(9379),	
		/// <returns>								: (see enum "eSnetApiReturnCode")	 
		///												CommunicationEventCode, 
		///												InterpolationTrajFull			(1101) ~ 
		///												InterpolationMovingPositionZero	(1112)									</returns>	
		public int GetContiInfoResult(int channel, ref bool is_moving, ref int current_motion_step, ref eSnetContiMovingInfo current_motion_command)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetContiInfoResult(NetID, channel, out int getValue0, out int getValue1, out int getValue2);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetContiInfoResult'. error : " + returnCode,
						"SnetDevice.GetContiInfoResult");
				}
				else
				{
					is_moving = ((getValue0 == 1) ? true : false);
					current_motion_step = getValue1;
					current_motion_command = (eSnetContiMovingInfo)getValue2;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetContiInfoResult");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 연속 보간 이송중 남은 거리가 설정 거리 보다 작으면 출력 접점 ON 또는 OFF 
		/// (tip 1)"eSnetSetContiOutputConfig"다음 지령 명령이 이송 명령이 아니면 실행 되지 않습니다.  
		/// (tip 2)"eSnetSetContiOutputConfig" 다음 보간 이송 명령 지령시 목표 위치가 현재 위치와 동일 하면 보간 이송명령이 
		///			실행되지 않고 종료 됩니다. 이때 만약 "distance_before_moving_step_end = 0" 이면 
		///			"eSnetSetContiOutputConfig"가 실행 되지 않습니다. 
		///			
		/// </summary>
		/// <param name="output_count">						: 출력 접점수 (1 step당), 최대 8 접점 동시 출력 가능 				</param>
		/// <param name="select_distance_or_time">			: "false"->거리, "true"->시간	(미사용)							</param>		
		/// <param name="output_type">						: [array] 배열 크기->outputCount, 
		///														"0"->axis_io, "1"->remote_io, 
		///														"2"->rtex_user_io or Ad option board io							</param>
		/// <param name="port">								: [array] 배열 크기->output_count, port								</param>					
		/// <param name="point">							: [array] 배열크기->output_count, point								</param>						
		/// <param name="on_off">							: [array] 배열크기->output_count, "true"->on, "false"->off			</param>					
		/// <param name="distance_before_moving_step_end">	: [array] 배열크기->output_count, 출력 시작 기준 남은 이송 거리(um) </param>						
		/// <returns>										: (see enum "eSnetApiEventCode")	 
		///														CommunicationEventCode, 
		///														ContinuousOverCount			(1201), 
		///														ContinuousOverIndex			(1202),
		///														ContinuousNotBeginCommand	(1203),								</returns>	
		public int SetContiOutputConfig(int output_count, bool select_distance_or_time,
			int[] output_type, int[] port, int[] point, bool[] on_off, int[] distance_before_moving_step_end)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;
			int[] onOffTemp;

			try
			{
				if (output_type.Length < output_count)
					return (int)eSnetApiReturnCode.InvalidArgument;
				if (port.Length < output_count)
					return (int)eSnetApiReturnCode.InvalidArgument;
				if (point.Length < output_count)
					return (int)eSnetApiReturnCode.InvalidArgument;
				if (on_off.Length < output_count)
					return (int)eSnetApiReturnCode.InvalidArgument;
				if (distance_before_moving_step_end.Length < output_count)
					return (int)eSnetApiReturnCode.InvalidArgument;

				if (output_count > 0)
				{
					onOffTemp = new int[output_count];
					for (int index = 0; index < output_count; index++)
					{
						if (on_off[index])
							onOffTemp[index] = 1;
						else
							onOffTemp[index] = 0;
					}
				}
				else
				{
					return (int)eSnetApiReturnCode.InvalidArgument;
				}

				int setValue = select_distance_or_time ? 1 : 0;
				returnCode = eSnetSetContiOutputConfig(NetID, output_count, setValue,
					out output_type[0], out port[0], out point[0], out onOffTemp[0], out distance_before_moving_step_end[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetContiOutputConfig'. error : " + returnCode,
						"SnetDevice.SetContiOutputConfig");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetContiOutputConfig");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "eSnetSetContiOutputConfig" 설정 값 확인  
		/// </summary>
		/// <param name="output_index">						: 설정 정보를 읽을 index													</param>						
		/// <param name="output_count">						: 설정 출력 접점수 (1 step당)												</param>						
		/// <param name="select_distance_or_time">			: "false"->거리, "true"->시간(미사용)										</param>		
		/// <param name="output_type">						: [array] 배열크기->"8", "0"->axis_io, "1"->remote_io, "2"->rtex_user_io	</param>						
		/// <param name="port">								: [array] 배열크기->"8", port												</param>					
		/// <param name="point">							: [array] 배열크기->"8", point												</param>						
		/// <param name="on_off">							: [array] 배열크기->"8", "1"->on, "0"->off									</param>					
		/// <param name="distance_before_moving_step_end">	: [array] 배열크기->"8", 출력 시작 기준 남은 이송 거리(um) 					</param>						
		/// <returns>										: (see enum "eSnetApiEventCode")											</returns>
		public int GetContiOutputConfig(int output_index, ref int output_count, ref bool select_distance_or_time,
			int[] output_type, int[] port, int[] point, bool[] on_off, int[] distance_before_moving_step_end)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;
			int countTemp = 0;
			int distanceTemp = 0;
			int[] onOffTemp;

			try
			{
				if (output_type.Length < 8)
					return (int)eSnetApiReturnCode.InvalidArgument;
				if (port.Length < 8)
					return (int)eSnetApiReturnCode.InvalidArgument;
				if (point.Length < 8)
					return (int)eSnetApiReturnCode.InvalidArgument;
				if (on_off.Length < 8)
					return (int)eSnetApiReturnCode.InvalidArgument;
				if (distance_before_moving_step_end.Length < 8)
					return (int)eSnetApiReturnCode.InvalidArgument;

				onOffTemp = new int[on_off.Length];

				returnCode = eSnetGetContiOutputConfig(NetID, output_index, out countTemp, out distanceTemp,
					out output_type[0], out port[0], out point[0], out onOffTemp[0], out distance_before_moving_step_end[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetContiOutputConfig'. error : " + returnCode,
						"SnetDevice.GetContiOutputConfig");
				}
				else
				{
					output_count = countTemp;
					select_distance_or_time = (distanceTemp == 1);
					for (int index = 0; index < onOffTemp.Length; index++)
					{
						on_off[index] = ((onOffTemp[index] == 1) ? true : false);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetContiOutputConfig");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 연속 이송보간 중 대기시간 설정 
		/// </summary>
		/// <param name="dwell">	: wait time									</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	 
		///								CommunicationEventCode, 
		///								ContinuousOverIndex			(1202),
		///								ContinuousNotBeginCommand	(1203),		</returns>	
		public int SetContiDwell(int dwell)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetContiDwell(NetID, dwell);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetContiDwell'. error : " + returnCode,
						"SnetDevice.SetContiDwell");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetContiDwell");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 연속 이송 보간 이송시 남은 이송 거리가 설정 거리 보다 작으면 설정 시간 동안 출력 접점 ON  
		/// # (tip 1)"eSnetSetContiOutputConfig"다음 지령 명령이 이송 명령이 아니면 실행 되지 않습니다.  
		///
		/// </summary>
		/// <param name="output_count">				: 연속이송보간시 사용할 output갯수(한 step당), 최대 8개 output까지 가능				</param>						
		/// <param name="output_type">				: [array] 배열 크기->output_count, "0"->axis_io, "1"->remote_io, "2"->rtex_user_io	</param>						
		/// <param name="port">						: [array] 배열 크기->output_count, port												</param>					
		/// <param name="point">					: [array] 배열 크기->output_count, point											</param>						
		/// <param name="distance_before_step_end">	: [array] 배열 크기->output_count, 출력 시작 기준 이송 남은 거리 					</param>						
		/// <param name="on_time">					: [array] 배열 크기->output_count, 출력 ON 유지 시간(msec)							</param>						
		/// <returns>								: (see enum "eSnetApiReturnCode")	 
		///												 
		///												ContinuousOverCount			(1201), 
		///												ContinuousOverIndex			(1202),
		///												ContinuousNotBeginCommand	(1203),											</returns>	
		public int SetContiOutputTrigger(int output_count, int[] output_type, int[] port, int[] point,
			int[] distance_before_step_end, int[] on_time)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				if (output_type.Length < output_count)
					return (int)eSnetApiReturnCode.InvalidArgument;
				if (port.Length < output_count)
					return (int)eSnetApiReturnCode.InvalidArgument;
				if (point.Length < output_count)
					return (int)eSnetApiReturnCode.InvalidArgument;
				if (distance_before_step_end.Length < output_count)
					return (int)eSnetApiReturnCode.InvalidArgument;
				if (on_time.Length < output_count)
					return (int)eSnetApiReturnCode.InvalidArgument;

				returnCode = eSnetSetContiOutputTrigger(NetID, output_count, out output_type[0], out port[0], out point[0], out distance_before_step_end[0], out on_time[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetContiOutputTrigger'. error : " + returnCode,
						"SnetDevice.SetContiOutputTrigger");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetContiOutputTrigger");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "eSnetSetContiOutputTrigger" 설정값 확인 
		/// </summary>
		/// <param name="output_index">				: 설정 정보를 읽을 index													</param>						
		/// <param name="output_count">				: 연속이송보간시 사용할 output갯수(한 step당)								</param>						
		/// <param name="output_type">				: [array] 배열크기->"8", "0"->axis_io, "1"->remote_io, "2"->rtex_user_io	</param>						
		/// <param name="port">						: [array] 배열크기->"8", port												</param>					
		/// <param name="point">					: [array] 배열크기->"8", point												</param>						
		/// <param name="distance_before_step_end">	: [array] 배열크기->"8", 출력 시작 기준 이송 남은 거리 						</param>						
		/// <param name="on_time">					: [array] 배열크기->"8", 출력 ON 유지시간(msec)								</param>						
		/// <returns>								: (see enum "eSnetApiReturnCode")											</returns>
		public int GetContiOutputTrigger(int output_index, ref int output_count, int[] output_type, int[] port, int[] point,
			int[] distance_before_step_end, int[] on_time)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;
			int countTemp = 0;

			try
			{
				if (output_type.Length < 8)
					return (int)eSnetApiReturnCode.InvalidArgument;
				if (port.Length < 8)
					return (int)eSnetApiReturnCode.InvalidArgument;
				if (point.Length < 8)
					return (int)eSnetApiReturnCode.InvalidArgument;
				if (distance_before_step_end.Length < 8)
					return (int)eSnetApiReturnCode.InvalidArgument;
				if (on_time.Length < 8)
					return (int)eSnetApiReturnCode.InvalidArgument;

				returnCode = eSnetGetContiOutputTrigger(NetID, output_index, out countTemp, out output_type[0], out port[0], out point[0], out distance_before_step_end[0], out on_time[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetContiOutputTrigger'. error : " + returnCode,
						"SnetDevice.GetContiOutputTrigger");
				}
				else
				{
					output_count = countTemp;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetContiOutputTrigger");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Moving :: PTP

		/// <summary>
		/// Moving Profile은 "PTP 및 보간 이송" 메뉴얼 참조
		/// /// "Point1"에서 "Point2"로 이동할 때, 상하 축 상승 중 
		/// "Point1.1"지점에서 X축 이송을 시작하고,
		/// "Point1.2"지점에서 하강하여 포인트간 이송 시간을 줄일 수 있다
		/// </summary>
		/// <param name="ptp_index">					: PTP이송구분번호:("0"~"7") : 동시 실행 가능 갯수 8개
		///													단, 동시실행가능 보간이동 갯수(15개) 범위내에서 실행 가능
		///													즉, 현재 10개의 보간 이송이 실행 중이면, 
		///														최대 5개의 PTP 이송을 실행 할 수 있다			</param>
		/// <param name="single_axis">					: single 이송 축 번호 (메뉴얼에서는 Z축(상하이동축))	</param>
		/// <param name="plane_axis1">					: 평면 구성 축 1번 (메뉴얼에서는 X축					</param>
		/// <param name="plane_axis2">					: 평면 구성 축 2번 (사용 하지 않을 경우: 0xff)			</param>
		/// <param name="plane_axis3">					: 평면 구성 축 3번 (사용 하지 않을 경우: 0xff)			</param>
		/// <param name="single_velocity">				: [mm/min]	signle축 이송 속도							</param>
		/// <param name="single_accel">					: [msec]	single축 가속도								</param>
		/// <param name="single_decel">					: [msec]	signle축 감속도								</param>
		/// <param name="single_jerk">					: [%]		single축 저크								</param>
		/// <param name="single_position">				: [um]		signle축 "남은거리", 남은 이송거리가 
		///															  설정거리 보다 작으면 평명 축 이송 시작	</param>
		/// <param name="single_destination_position1">	: single축 첫번째 이송 위치(Point1.1)					</param>
		/// <param name="single_destination_position2">	: single축 두번재 이송 위치(Point1.2)					</param>
		/// <param name="plane_velocity">				: [mm/min]	평면 축 보간이송 속도						</param>
		/// <param name="plane_accel">					: [msec]		평면 축 보간이송 가속시간				</param>
		/// <param name="plane_decel">					: [msec]		평면 축 보간이송 감속시간				</param>
		/// <param name="plane_jerk">					: [%]			평명 축 보간이송 저크					</param>
		/// <param name="plane_position">				: 평면 축 보간이송 남은거리(축 이송거리 벡터합)
		///													남은거리가 설정거리 보다 작으면 single축 이송 시작	</param>
		/// <param name="plane_destination_position1">	: 평면 구성 첫번째 이송 위치 (사용하지 않으면 "0")		</param>
		/// <param name="plane_destination_position2">	: 평면 구성 두번째 이송 위치 (사용하지 않으면 "0")		</param>
		/// <param name="plane_destination_position3">	: 평면 구성 세번째 이송 위치 (사용하지 않으면 "0")		</param>
		/// <returns>									: (see enum "eSnetApiReturnCode")					
		///													 
		///													PtpMoveSetOverCount				(2201) ~ 
		///													PtpMovePlaneAndSigleAxisOvrlap	(2214)				</returns>
		public int MovePtp(int ptp_index, int single_axis,
			int plane_axis1, int plane_axis2, int plane_axis3,
			int single_velocity, int single_accel, int single_decel, int single_jerk,
			int single_position, int single_destination_position1, int single_destination_position2,
			int plane_velocity, int plane_accel, int plane_decel, int plane_jerk,
			int plane_position, int plane_destination_position1, int plane_destination_position2, int plane_destination_position3)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetMovePtp(NetID, ptp_index, single_axis, plane_axis1, plane_axis2, plane_axis3,
					single_velocity, single_accel, single_decel, single_jerk, single_position, single_destination_position1, single_destination_position2,
					plane_velocity, plane_accel, plane_decel, plane_jerk, plane_position, plane_destination_position1, plane_destination_position2, plane_destination_position3);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetMovePtp'. error : " + returnCode,
						"SnetDevice.MovePtp");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.MovePtp");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// PTP 및 보간구동 상태 읽기
		/// </summary>
		/// <param name="ptp_index">	: PTP 이송 구분 번호:("0 ~"7"): 동시 실행 가능 갯수 : 8개
		///									단, "동시 실행 가능 보간 이송 갯수(15개)" 범위 내에서 실행 가능 
		///									즉, 현재 10개의 보간 이송이 실행 중이면 
		///									최대 5개의 PTP이송을 실행할 수 있다							</param>
		/// <param name="status">		: 지령 인자에 대한 return 값,
		///									"0": ready,
		///									"1": PTP이송 중,
		///									"2": PTP이송 완료,
		///									"3": 이송 실패,												</param>
		/// <returns>					(see enum "eSnetApiReturnCode")									</returns>
		public int GetPtpStatus(int ptp_index, ref int status)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetPtpStatus(NetID, ptp_index, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetPtpStatus'. error : " + returnCode,
						"SnetDevice.GetPtpStatus");
				}
				else
				{
					status = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetPtpStatus");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Moving :: Override

		/// <summary>
		/// 단축 이송시 속도 override 실행 
		/// : old version name ->"eSnetSetOverrideVel"
		/// </summary>				
		/// <param name="axis">		: axis index ("0"~ )				</param>						
		/// <param name="velocity">	: [mm/min]: override 속도			</param>						
		/// <param name="accel">	: [msec]	: override 가속 시간		</param>
		/// <param name="decel">	: [msec]	: override 감속 시간		</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")					
		///								CommunicationEventCode, 
		///								OverrideAxisNumber	(1301) ~ 
		///								OverrideVelocity	(1304)		</returns>
		public int OverrideVelocity(int axis, int velocity, int accel, int decel)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetOverrideVelocity(NetID, axis, velocity, accel, decel);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetOverrideVelocity'. error : " + returnCode,
						"SnetDevice.OverrideVelocity");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.OverrideVelocity");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 위치 이송 중 속도 오버라이드 지령
		/// </summary>
		/// <param name="axis">				: axis index </param>
		/// <param name="profile_type">		: "1000"--> S-Curve, "1001"--> Trapezoidal </param>
		/// <param name="velocity">			: [um] override 속도 </param>
		/// <param name="accel">			: [msec] override 가속 시간 </param>
		/// <param name="decel">			: [msec] override 감속 시간 </param>
		/// <param name="acc_jerk">			: [%] 가속 시 jerk </param>
		/// <param name="dec_jerk">			: [%] 감속 시 jerk </param>
		/// <returns>						: (see enum "eSnetApiReturnCode")					
		///										CommunicationEventCode, 
		///										OverrideExInvalidAxis			(6003), 
		///										OverrideExNoVelocity			(6050), 
		///										OverrideExNoJerk				(6052),
		///										OverrideExNotRunning			(6070),	
		/// </returns>
		public int OverrideVelocityEx(int axis, int profile_type, int velocity, int accel, int decel, int acc_jerk, int dec_jerk)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetOverrideVelocityEx(NetID, axis, profile_type, velocity, accel, decel, acc_jerk, dec_jerk);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetOverrideVelocityEx'. error : " + returnCode,
						"SnetDevice.OverrideVelocityEx");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.OverrideVelocityEx");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 보간 이송시 속도 override 실행 
		/// : old version name ->"eSnetSetOverrideLineVel"
		/// </summary>			
		/// <param name="axis">		: axis index(보간 이송에 사용중인 축 중 아무거나 선택)		</param>						
		/// <param name="velocity">	: [mm/min]: override 속도								</param>						
		/// <param name="accel">	: [msec]	: override 가속 시간		 					</param>
		/// <param name="decel">	: [msec]	: override 감속 시간							</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")					
		///								CommunicationEventCode, 
		///								OverrideAxisNumber			(1301),  
		///								OverrideVelocity			(1304),
		///								OverrideInterpolationNotUse (1305),					</returns>
		public int OverrideInterpolationVelocity(int axis, int velocity, int accel, int decel)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetOverrideInterpolationVelocity(NetID, axis, velocity, accel, decel);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetOverrideLInterpolationVelocity'. error : " + returnCode,
						"SnetDevice.OverrideInterpolationVelocity");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.OverrideInterpolationVelocity");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 목표 위치까지 이동하면서 복수의 지정 위치에서 속도를 변경하는 단축 이송 실행 
		/// : old version name ->"eSnetMoveOverrideVelAtMultiPos"
		/// </summary>
		/// <remarks>
		/// (tip 1) 모터 정지 상태서 사용해야 됩니다.  
		/// (tip 2) 좌표 증가 방향으로 이송할 때는 "overide_position" 값이 "현재 좌표"보다 커야 됩니다. 
		///        좌표 감소 방향으로 이송할 때는 "overide_position" 값이 "현재 좌표"보다 작아야 됩니다. 
		///
		///  (예1) 현재 좌표 100mm이고 목표 좌표 150mm 위치로 이송 중(좌표 증가) 110mm, 120mm, 130mm 지점에서 속도를 변경 하는 경우  
		///	     -> array_count=3	
		///      -> overide_position[0]=110, 
		///		    overide_position[1]=120,
		///	  	    overide_position[2]=130 
		///
		///  (예2) 현재 좌표 150mm이고 목표 좌표 100mm 위치로 이송 중(좌표 감소) 140mm, 130mm, 120mm 지점에서 속도를 변경 하는 경우  
		///      -> array_count=3	
		///      -> overide_position[0]=140, 
		///		    overide_position[1]=130,
		///		    overide_position[2]=120 
		/// </remarks>					
		/// <param name="axis">				: axis index											</param>						
		/// <param name="velocity">			: [mm/min]:이송 속도									</param>						
		/// <param name="accel">		    : [msec]	: 가속 시간				 					</param>
		/// <param name="decel">			: [msec]	: 감속 시간									</param>
		/// <param name="jerk">				: [%]		: jerk										</param>
		/// <param name="position">			: [um]	: 이송 좌표 									</param>
		/// <param name="mode">				: 속도 override 적용 위치 source 									
		///									: "false"--> actual posion, "true"-->command position	</param>
		/// <param name="array_count">		: 위치 배열 갯수 -> 최대 10개  							</param>
		/// <param name="override_position">: [array] 배열 크기->array_count,속도 변경 적용 좌표	</param>
		/// <param name="override_velocity">: [array] 배열 크기->array_count,Override 속도			</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")					
		///											CommunicationEventCode, 
		///											OverrideAxisNumber			(1301),  
		///											OverrideAxisBusy			(1306),
		///											OverrideParameter			(1307),				</returns>
		public int OverrideVelocityAtMultiPosition(int axis, int velocity, int accel, int decel, int jerk, int position, bool mode, int array_count, int[] override_position, int[] override_velocity)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				if (override_position.Length < array_count)
					return (int)eSnetApiReturnCode.InvalidArgument;
				if (override_velocity.Length < array_count)
					return (int)eSnetApiReturnCode.InvalidArgument;

				int setMode = mode ? 1 : 0;
				returnCode = eSnetOverrideVelocityAtMultiPosition(NetID, axis, velocity, accel, decel, jerk, position, setMode, array_count, out override_position[0], out override_velocity[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetMoveOverrideVelAtMultiPos'. error : " + returnCode,
						"SnetDevice.OverrideVelocityAtMultiPosition");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.OverrideVelocityAtMultiPosition");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 목표 위치까지 이동하면서 특정 1개의 위치에서 속도및 가/감속 시간이 변경 되는 단축 이송 실행 
		/// : old version name ->"eSnetMoveOverrideAccelVelDecelAtPos"
		/// </summary>
		/// <remarks>
		/// (tip 1) 모터 정지 상태서 사용해야 됩니다.  
		/// </remarks>			
		/// <param name="axis">					: axis index											</param>						
		/// <param name="velocity">				: [mm/min] override 속도								</param>						
		/// <param name="accel">				: [msec]	 override 가속 시간							</param>
		/// <param name="decel">				: [msec]	 override 감속 시간							</param>
		/// <param name="jerk">					: [%]		 jerk										</param>
		/// <param name="position">				: [um]	 이송 좌표 										</param>
		/// <param name="mode">					: 속도 override 적용 위치 source 										
		///										: "false"--> actual posion, "true"-->command position	</param>
		/// <param name="override_position">	: [um] : 속도를 변경할 위치		   						</param>
		/// <param name="override_velocity">	: [mm/min] 변경 속도   	      							</param>
		/// <param name="override_accel">		: [msec]	 속도 override 적용 가속 시간				</param>
		/// <param name="override_decel">		: [msec]	 속도 override 적용 감속 시간				</param>
		/// <returns>							: (see enum "eSnetApiReturnCode")					
		///											CommunicationEventCode, 
		///											OverrideAxisNumber			(1301), 
		///											OverrideVelocity			(1304), 
		///											OverrideAxisBusy			(1306),
		///											OverrideParameter			(1307),					</returns>
		public int OverrideAccelVelocityDecelAtPosition(int axis, int velocity, int accel, int decel, int jerk,
			int position, bool mode, int override_position, int override_velocity, int override_accel, int override_decel)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setMode = mode ? 1 : 0;
				returnCode = eSnetOverrideAccelVelocityDecelAtPosition(NetID, axis, velocity, accel, decel, jerk, position, setMode,
					override_position, override_velocity, override_accel, override_decel);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetOverrideAccelVelocityDecelAtPosition'. error : " + returnCode,
						"SnetDevice.OverrideAccelVelocityDecelAtPosition");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.OverrideAccelVelocityDecelAtPosition");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 모션 구동 중 새로운 목표 위치로 변경
		/// </summary>
		/// <param name="axis">			: axis index</param>
		/// <param name="position">		: 변경할 위치(um)</param>
		/// <param name="relative">		: 'position' 인자 상대 이송 유무. true: 상대 이송, false: 절대 이송</param>
		/// <returns>							: (see enum "eSnetApiReturnCode")					
		///											CommunicationEventCode, 
		///											OverridePositionInvalidAxis			(1351), 
		///											OverridePositionNotRunning			(1352), 
		///											OverridePositionInterpolation		(1353),
		///											OverridePositionInvalidPoisition	(1354),
		///											OverridePositionInvalidRelative		(1355),
		/// </returns>
		public int OverridePosition(int axis, int position, bool relative)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetOverridePosition(NetID, axis, position, relative ? 1 : 0);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetOverridePosition'. error : " + returnCode,
						"SnetDevice.OverridePosition");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.OverridePosition");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Moving :: MPG (SNET-RTEX)

		/*** Move : MPG Mode ***/

		/// <summary>
		/// "MPG로 Motion 구동" 환경 설정
		/// : old version name -->"eSnetSetMpgConfig"
		/// </summary>
		/// <param name="use_mode">		: "true"->MPG Mode 사용, "false"->MPG Mode 사용 안함			</param>
		/// <param name="magnification">: MPG 구동축 번호 지정,bit 단위 "0x01 ~ 0xffffffff"				</param>
		/// <param name="use_axes">		: bit번호 == 축 번호 1:1 매칭					
		///										(예) 0번 축, 2번 축을 MPG로 구동 하고 싶을 때 : 0x05
		///										(예) 3번 축, 7번 축을 MPG로 구동 하고 싶을 때 : 0x0088	</param>	
		/// <returns>					: (see enum "eSnetApiReturnCode")								</returns>
		public int RtexSetMpgConfig(bool use_mode, int magnification, int use_axes)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (use_mode ? 1 : 0);

				returnCode = eSnetRtexSetMpgConfig(NetID, setValue, magnification, use_axes);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexSetMpgConfig'. error : " + returnCode,
						"SnetDevice.RtexSetMpgConfig");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexSetMpgConfig");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "eSnetRtexSetMpgConfig" 사용자 설정값 읽기 
		/// : old version name -->"eSnetGetMpgConfig"
		/// </summary>
		/// <param name="use_mode">			: "true"->MPG Mode 사용, "false"->MPG Mode 사용 안함		</param>
		/// <param name="magnification">	: MPG로 구동할 때 속도 배율, default->"1", max->"100배"		</param>
		/// <param name="use_axes">			: MPG 구동축 번호 지정,bit 단위 "0x01 ~ 0xffffffff"			</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")							</returns>	
		public int RtexGetMpgConfig(ref bool use_mode, ref int magnification, ref int use_axes)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexGetMpgConfig(NetID, out int getValue0, out int getValue1, out int getValue2);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetMpgConfig'. error : " + returnCode,
						"SnetDevice.RtexGetMpgConfig");
				}
				else
				{
					use_mode = ((getValue0 == 1) ? true : false);
					magnification = getValue1;
					use_axes = getValue2;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetMpgConfig");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Absolute / Relative Mode

		/// <summary>
		/// 축별 상대값 구동/절대값 구동 설정 하기
		/// : old version name ->"eSnetSetAxisAbsRelMode"
		/// </summary>
		/// <param name="axis">		: axis index								</param>						
		/// <param name="relative">	: "false"->절대값 구동, "true"->상대값 구동	</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")			</returns>
		public int SetAbsRelMode(int axis, bool relative)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (relative ? 1 : 0);
				returnCode = eSnetSetAbsRelMode(NetID, axis, setValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetAbsRelMode'. error : " + returnCode,
						"SnetDevice.SetAbsRelMode");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetAbsRelMode");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 축별 상대값 구동/절대값 구동 설정값 읽기
		/// : old version name ->"eSnetGetAxisAbsRelMode"
		/// </summary>
		/// <param name="axis">		: axis index								</param>						
		/// <param name="relative">	: "false"->절대값 구동, "true"->상대값 구동	</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")			</returns>
		public int GetAbsRelMode(int axis, ref bool relative)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetAbsRelMode(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetAbsRelMode'. error : " + returnCode,
						"SnetDevice.GetAbsRelMode");
				}
				else
				{
					relative = ((getValue == 1) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetAbsRelMode");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Position Loopback

		/// <summary>
		/// Loopback 기능 설정 하기 
		/// : old version name ->"SetAxisLoopback"
		/// </summary>
		/// <param name="axis">		: axis index							</param>						
		/// <param name="loopback">	: "true"->사용, "false"->사용하지 않음	</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		public int SetLoopback(int axis, bool loopback)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (loopback ? 1 : 0);

				returnCode = eSnetSetLoopback(NetID, axis, setValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetLoopback'. error : " + returnCode,
						"SnetDevice.SetLoopback");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetLoopback");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Loopback 기능 설정값 읽기 
		/// : old version name ->"GetAxisLoopback"
		/// </summary>
		/// <param name="axis">		: axis index							</param>						
		/// <param name="loopback">	: "true"->사용, "false"->사용하지 않음	</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		public int GetLoopback(int axis, ref bool loopback)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetLoopback(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetLoopback'. error : " + returnCode,
						"SnetDevice.GetLoopback");
				}
				else
				{
					loopback = ((getValue == 1) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetLoopback");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        #endregion

        #region Operation ( SNET-ECAT )

        /// <summary>
        /// 축의 동작 모드 읽기 ( SNET-ECAT )
        /// </summary>
        /// <param name="axis">		: 읽을 축 번호</param>
        /// <param name="mode">		: 읽은 mode 정보가 저장될 버퍼 (see enum "eSnetOperationMode")
        /// @ "1"					: Cyclic Position Mode
        /// @ "2"					: Cyclic Velocity Mode
        /// @ "3"					: Cyclic Torque Mode
        /// </param>
        /// <returns>				: (see enum "eSnetApiReturnCode")</returns>
        public int GetOperationMode(int axis, ref int mode)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetGetOperationMode(NetID, axis, out int getMode);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetGetOperationMode'. error : " + returnCode,
                                                    "SnetDevice.GetOperationMode");
                }
                else
                {
                    mode = getMode;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.GetOperationMode");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// 축의 동작 모드 쓰기 ( SNET-ECAT )
        /// </summary>
        /// <param name="axis">		: 저장할 축 번호</param>
        /// <param name="mode">		: 저장할 mode 정보 (see enum "eSnetOperationMode")
        /// @ "1"					: Cyclic Position Mode
        /// @ "2"					: Cyclic Velocity Mode
        /// @ "3"					: Cyclic Torque Mode
		/// </param>
        /// <returns>				: (see enum "eSnetApiReturnCode")</returns>
        public int SetOperationMode(int axis, int mode)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetSetOperationMode(NetID, axis, mode);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetSetOperationMode'. error : " + returnCode,
                                                    "SnetDevice.SetOperationMode");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.SetOperationMode");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        #endregion

        #region Command Position

        /// <summary>
        /// 축 "Command Position(절대 좌표)" 변경  
        /// (tip 1)"Command Position"변경 후, 변경 전후 좌표 편차를 이용하여 "Actual Position" 자동 변경 
        /// (tip 2)"Command Position 과 Actual Position"이 일치하면 "eSnetSetAxisActualPosition" 사용과 
        ///			동일한 결과를 보여 줍니다. 
        /// : old version name ->"eSnetSetAxisCommandPosition" 	
        /// </summary>
        /// <param name="axis">		: ax number							</param>						
        /// <param name="position">	: command position, [um]			</param>						
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
        public int SetCommandPosition(int axis, int position)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetCommandPosition(NetID, axis, position);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetCommandPosition'. error : " + returnCode,
						"SnetDevice.SetCommandPosition");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetCommandPosition");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 축 "Command Position(절대 좌표)" 현재값 확인 
		/// : old version name ->"eSnetGetAxisCommandPosition" 
		/// </summary>
		/// <param name="axis">		: ax number							</param>						
		/// <param name="position">	: command position, [um]			</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int GetCommandPosition(int axis, ref int position)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetCommandPosition(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetCommandPosition'. error : " + returnCode,
						"SnetDevice.GetCommandPosition");
				}
				else
				{
					position = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetCommandPosition");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 축별 "이송 지령 명령 전달 좌표" 확인 
		/// old version name ->"eSnetGetAxisTargetCommandPosition"
		/// </summary>
		/// <param name="axis">		axis index						</param>						
		/// <param name="position">	command position, [um]			</param>
		/// <returns>				(see enum "eSnetApiReturnCode")	</returns>
		public int GetTargetPosition(int axis, ref int position)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetTargetPosition(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetTargetPosition'. error : " + returnCode,
						"SnetDevice.GetTargetPosition");
				}
				else
				{
					position = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetTargetPosition");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Actual Position

		/// <summary>
		/// 축 "Actual Position(기계 좌표)" 변경  
		/// (tip 1) "Actual Position" 변경 후,변경 전후 좌표 편차를 이용하여 "Command Position"자동 변경 
		/// (tip 2)"Command Position 과 Actual Position"이 일치하면 "eSnetSetCommandPosition" 사용과 
		///			동일한 결과를 보여 줍니다.
		///	: old version name ->"eSnetSetAxisActualPosition"
		/// </summary>	
		/// <param name="axis">		: axis index					</param>						
		/// <param name="position">	: actual position				</param>						
		/// <returns>				(see enum "eSnetApiReturnCode")	</returns>
		public int SetActualPosition(int axis, int position)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetActualPosition(NetID, axis, position);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetActualPosition'. error : " + returnCode,
						"SnetDevice.SetActualPosition");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetActualPosition");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 축 "Actual Position(기계 좌표)" 확인 
		/// : old versin name ->"eSnetGetAxisActualPosition"
		/// </summary>
		/// <param name="axis">		: axis index						</param>						
		/// <param name="position">	: actual position,[um]				</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int GetActualPosition(int axis, ref int position)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetActualPosition(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetActualPosition'. error : " + returnCode,
						"SnetDevice.GetActualPosition");
				}
				else
				{
					position = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetActualPosition");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 원하는 축들의 현재 지령 위치와 실제 위치를 한번에 읽음
		/// </summary>
		/// <param name="axisCount">		: 첫번째 축(0번) 부터 위치를 읽고 싶은 축 갯수						</param>
		/// <param name="positionCommand">	: 배열, 현재 지령 위치,	[um],										
		///										(예) positionCommand[0] : "0"번 축(첫번째 축) 현재 지령 위치,
		///										     positionCommand[1] : "1"번 축(두번째 축) 현재 지령 위치,	</param>
		/// <param name="positionActual">	: 배엵, 현재 실제 위치, [um],
		///										(예) positionActual[0] : "0"번 축(첫번째 축) 현재 실제 위치,
		///											 positionActual[1] : "1"번 축(두번째 축) 현재 실제 위치,	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")									</returns>
		public int GetPosition(int axisCount, int[] positionCommand, int[] positionActual)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetPosition(NetID, axisCount, out positionCommand[0], out positionActual[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetPosition'. error : " + returnCode,
						"SnetDevice.GetPosition");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetPosition");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 축 "Command Position"과 "Actual Position"을 동일 한 좌표로 변경 하기 
		/// (tip 1)"원점 이송" 완료시 사용 됩니다. 
		/// : old version name ->"SetAxisHomePosition"
		/// </summary>
		/// <param name="axis">		: axis index						</param>						
		/// <param name="position">	: 변경 좌표, [um]					</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int SetHomePosition(int axis, int position)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetHomePosition(NetID, axis, position);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetHomePosition'. error : " + returnCode,
						"SnetDevice.SetHomePosition");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetHomePosition");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Z("C")상을 Catpure한 위치값 읽기
		/// (tip 1)Z상 이용 원점 이송 동작 시 사용 됩니다.
		/// : old version name ->"GetAxisZCapturePosition"
		/// </summary>
		/// <param name="axis">		: axis index						</param>						
		/// <param name="position">	: Z상 캡처 좌표 					</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int GetLatchPositionZPhase(int axis, ref int position)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetLatchPositionZPhase(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetLatchPositionZPhase'. error : " + returnCode,
						"SnetDevice.GetLatchPositionZPhase");
				}
				else
				{
					position = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetLatchPositionZPhase");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        #endregion

        #region Command Velocity

        /// <summary>
        /// 지령(프로파일) 속도 읽기
        /// </summary>
        /// <param name="net">			: controller id</param>
        /// <param name="axis">			: 축 번호</param>
        /// <param name="unit">			: 읽을 속도 단위, 열거자(enum) 'SnetVelocityUnit' 참조</param>
        /// <param name="velocity">		: 읽어서 저장할 지령(프로파일) 속도</param>
        /// <returns>				    : (see enum "eSnetApiReturnCode")	</returns>
        public int GetCommandVelocity(int axis, int unit, ref int velocity)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetGetCommandVelocity(NetID, axis, unit, out int getVelocity);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetGetCommandVelocity'. error : " + returnCode,
                        "SnetDevice.GetCommandVelocity");
                }
                else
                {
                    velocity = getVelocity;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.GetCommandVelocity");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        #endregion

        #region Torque ( SNET-ECAT )

        ///
        /// Torque : SNET-ECAT
        ///

        /// <summary>
        /// 축의 지령 Torque 값 읽기 ( SNET-ECAT )
        /// </summary>
        /// <param name="axis">		: 읽을 시작 축 번호</param>
        /// <param name="count">	: 읽을 축 개수</param>
        /// <param name="data">		: 읽은 Torque 데이터가 저장될 버퍼</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
        public int GetCommandTorque(int axis, int count, int[] data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetGetCommandTorque(NetID, axis, count, out data[0]);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetGetCommandTorque'. error : " + returnCode,
                                                    "SnetDevice.GetCommandTorque");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.GetCommandTorque");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// 축의 실제 Torque 값 읽기 ( SNET-ECAT )
        /// </summary>
        /// <param name="axis">		: 읽을 시작 축 번호</param>
        /// <param name="count">	: 읽을 축 개수</param>
        /// <param name="data">		: 읽은 Torque 데이터가 축 수만큼 저장될 버퍼</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
        public int GetActualTorque(int axis, int count, int[] data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetGetActualTorque(NetID, axis, count, out data[0]);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetGetActualTorque'. error : " + returnCode,
                                                    "SnetDevice.GetActualTorque");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.GetActualTorque");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// 축의 최대 지령 Torque 값 읽기 ( SNET-ECAT )
		/// * Reset 함수를 호출해야 최대 Torque 데이터가 Clear 됨
        /// </summary>
        /// <param name="axis">				: 읽을 시작 축 번호</param>
        /// <param name="count">			: 읽을 축 개수</param>
        /// <param name="negative_data">	: 읽은 음방향 최대 Torque 데이터가  축 수만큼 저장될 버퍼</param>
        /// <param name="positive_data">	: 읽은 양방향 최대 Torque 데이터가 축 수만큼 저장될 버퍼</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        public int GetCommandMaxTorque(int axis, int count, int[] negative_data, int[] positive_data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetGetCommandMaxTorque(NetID, axis, count, out negative_data[0], out positive_data[0]);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetGetCommandMaxTorque'. error : " + returnCode,
                                                    "SnetDevice.GetCommandMaxTorque");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.GetCommandMaxTorque");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// 축의 평균 지령 Torque 값 읽기 ( SNET-ECAT )
		/// * Reset 함수를 호출해야 최대 Torque 데이터가 Clear 됨
        /// </summary>
        /// <param name="axis">				: 읽을 시작 축 번호</param>
        /// <param name="count">			: 읽을 축 개수</param>
        /// <param name="data">				: 읽은 평균 Torque 데이터가 축 수만큼 저장될 버퍼</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        public int GetCommandAverageTorque(int axis, int count, int[] data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
				returnCode = eSnetGetCommandAverageTorque(NetID, axis, count, out data[0]);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetGetCommandAverageTorque'. error : " + returnCode,
                                                    "SnetDevice.GetCommandAverageTorque");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.GetCommandAverageTorque");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// 축의 최대 Torque 차이값 읽기 ( SNET-ECAT )
		/// * Reset 함수를 호출해야 최대 Torque 데이터가 Clear 됨
        /// </summary>
        /// <param name="axis">				: 읽을 시작 축 번호</param>
        /// <param name="count">			: 읽을 축 개수</param>
        /// <param name="negative_data">	: 읽은 음방향 최대 Torque 차이값이 축 수만큼 저장될 버퍼</param>
        /// <param name="positive_data">	: 읽은 양방향 최대 Torque 차이값이 축 수만큼 저장될 버퍼</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        public int GetDeltaMaxTorque(int axis, int count, int[] negative_data, int[] positive_data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
				returnCode = eSnetGetDeltaMaxTorque(NetID, axis, count, out negative_data[0], out positive_data[0]);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetGetDeltaMaxTorque'. error : " + returnCode,
                                                    "SnetDevice.GetDeltaMaxTorque");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.GetDeltaMaxTorque");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// 축의 평균 Torque 차이값 읽기 ( SNET-ECAT )
		/// * Reset 함수를 호출해야 최대 Torque 데이터가 Clear 됨
        /// </summary>
        /// <param name="axis">				: 읽을 시작 축 번호</param>
        /// <param name="count">			: 읽을 축 개수</param>
        /// <param name="data">				: 읽은 평균 Torque 차이값이 축 수만큼 저장될 버퍼</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        public int GetDeltaAverageTorque(int axis, int count, int[] data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
				returnCode = eSnetGetDeltaAverageTorque(NetID, axis, count, out data[0]);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetGetDeltaAverageTorque'. error : " + returnCode,
                                                    "SnetDevice.GetDeltaAverageTorque");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.GetDeltaAverageTorque");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// 축의 지령 Torque 제한치 읽기 ( SNET-ECAT )
        /// </summary>
        /// <param name="axis">				: 읽을 축 번호</param>
        /// <param name="negative_data">	: 읽은 음방향 지령 Torque 제한치가 저장될 버퍼</param>
        /// <param name="positive_data">	: 읽은 양방향 지령 Torque 제한치가 저장될 버퍼</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        public int GetCommandTorqueLimit(int axis, ref int negative_data, ref int positive_data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetGetCommandTorqueLimit(NetID, axis, out int getNegativeData, out int getPositiveData);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetGetCommandTorqueLimit'. error : " + returnCode,
                                                    "SnetDevice.GetCommandTorqueLimit");
                }
				else
				{
					negative_data = getNegativeData;
					positive_data = getPositiveData;
				}
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.GetCommandTorqueLimit");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// 축의 지령 Torque 제한치 쓰기 ( SNET-ECAT )
		/// * 설정된 제한치를 비활성화 하려면 제한치를 0으로 설정할 것
        /// </summary>
        /// <param name="axis">				: 저장할 축 번호</param>
        /// <param name="negative_data">	: 저장할 음방향 지령 Torque 제한치</param>
        /// <param name="positive_data">	: 저장할 양방향 지령 Torque 제한치</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        public int SetCommandTorqueLimit(int axis, int negative_data, int positive_data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetSetCommandTorqueLimit(NetID, axis, negative_data, positive_data);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetSetCommandTorqueLimit'. error : " + returnCode,
                                                    "SnetDevice.SetCommandTorqueLimit");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.SetCommandTorqueLimit");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// 축의 Torque 차이값 제한치 읽기 ( SNET-ECAT )
        /// </summary>
        /// <param name="axis">				: 읽을 축 번호</param>
        /// <param name="negative_data">	: 읽은 음방향 Torque 차이값 제한치가 저장될 버퍼</param>
        /// <param name="positive_data">	: 읽은 양방향 Torque 차이값 제한치가 저장될 버퍼</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        public int GetDeltaTorqueLimit(int axis, ref int negative_data, ref int positive_data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetGetDeltaTorqueLimit(NetID, axis, out int getNegativeData, out int getPositiveData);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetGetDeltaTorqueLimit'. error : " + returnCode,
                                                    "SnetDevice.GetDeltaTorqueLimit");
                }
                else
                {
                    negative_data = getNegativeData;
                    positive_data = getPositiveData;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.GetDeltaTorqueLimit");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// 축의 Torque 차이값 제한치 쓰기 ( SNET-ECAT )
		/// * 설정된 제한치를 비활성화 하려면 제한치를 0으로 설정할 것
        /// </summary>
        /// <param name="axis">				: 저장할 축 번호</param>
        /// <param name="negative_data">	: 저장할 음방향 Torque 차이값 제한치</param>
        /// <param name="positive_data">	: 저장할 양방향 Torque 차이값 제한치</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        public int SetDeltaTorqueLimit(int axis, int negative_data, int positive_data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetSetDeltaTorqueLimit(NetID, axis, negative_data, positive_data);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetSetDeltaTorqueLimit'. error : " + returnCode,
                                                    "SnetDevice.SetDeltaTorqueLimit");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.SetDeltaTorqueLimit");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// 축의 지령 Torque Gain(%) 읽기 ( SNET-ECAT )
        /// </summary>
        /// <param name="axis">		: 읽을 축 번호</param>
        /// <param name="data">		: 읽은 Gain(%)이 저장될 버퍼
		/// * ex:) [1] -> 0.1% / [500] -> 50%
		/// </param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
        public int GetCommandTorqueGain(int axis, ref int data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetGetCommandTorqueGain(NetID, axis, out int getData);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetGetCommandTorqueGain'. error : " + returnCode,
                                                    "SnetDevice.GetCommandTorqueGain");
                }
                else
                {
                    data = getData;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.GetCommandTorqueGain");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// 축의 지령 Torque Gain(%) 쓰기 ( SNET-ECAT )
		/// * 설정된 Gain을 비활성화 하려면 Gain을 0으로 설정할 것
        /// </summary>
        /// <param name="axis">		: 저장할 축 번호</param>
        /// <param name="data">		: 저장할 Gain(%)
		/// * ex:) [1] -> 0.1% / [500] -> 50%
		/// </param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
        public int SetCommandTorqueGain(int axis, int data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetSetCommandTorqueGain(NetID, axis, data);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetSetCommandTorqueGain'. error : " + returnCode,
                                                    "SnetDevice.SetCommandTorqueGain");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.SetCommandTorqueGain");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// 축의 실제 torque를 지령 torque로 동기 활성화 유무 읽기 ( SNET-ECAT)
        /// </summary>
        /// <param name="axis">		: 읽을 축 번호</param>
        /// <param name="enable">	: 읽은 동기 활성화 유무
		/// @ "1"					: 활성화
		/// @ "2"					: 비활성화
		/// </param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
        public int GetActualTorqueSync(int axis, ref bool enable)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetGetActualTorqueSync(NetID, axis, out int getEnable);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetGetActualTorqueSync'. error : " + returnCode,
                                                    "SnetDevice.GetActualTorqueSync");
                }
                else
                {
                    enable = ((getEnable > 0) ? true : false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.GetActualTorqueSync");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// 축의 실제 torque를 지령 torque로 동기 활성화 유무 쓰기 ( SNET-ECAT)
        /// </summary>
        /// <param name="axis">		: 저장할 축 번호</param>
        /// <param name="enable">	: 동기 활성화 유무
        /// @ "1"					: 활성화
        /// @ "2"					: 비활성화
        /// </param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
        public int SetActualTorqueSync(int axis, bool enable)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetSetActualTorqueSync(NetID, axis, (enable ? 1 : 0));
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetSetActualTorqueSync'. error : " + returnCode,
                                                    "SnetDevice.SetActualTorqueSync");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.SetActualTorqueSync");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        #endregion

        #region Roll Over

        /// <summary>
        /// 축 이송 중 현재 지령 좌표가 설정 좌표 보다 크거나 작으면 "0"으로 변경하고 다시 시작
        /// (tip 1) 무한 회전 운동"이 요구되는 응용 분야에 적용
        /// : old version name -->"eSnetSetAxisRollover"
        /// </summary>
        /// <param name="axis">		: axis index																</param>
        /// <param name="position">	: 좌표 재설정 기준 좌표
        ///								-. 양수
        ///								  -> 현재 좌표가 설정값 보다 크거나 같으면 현재 좌표를 "0"으로 초기화 
        ///								-. 음수
        ///								  -> 현재 좌표가 설정값 보다 작거나 같으면 현재 좌표를 "0"으로 초기화	</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")
        ///								CommunicationEventCode,
        ///								PositionAutoResetOverAxisIndex (2301)	
        public int SetRollover(int axis, int position)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetRollover(NetID, axis, position);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetRollover'. error : " + returnCode,
						"SnetDevice.SetRollover");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetRollover");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "eSnetSetAxisRollover" 설정값 확인 하기 
		/// : old version name -->"eSnetGetAxisRollover"
		/// </summary>
		/// <param name="axis">		: axis index								</param>
		/// <param name="position">	: 좌표 재설정 기준 좌표						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")
		///								CommunicationEventCode,
		///								PositionAutoResetOverAxisIndex (2301)	</returns>	
		public int GetRollover(int axis, ref int position)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetRollover(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetRollover'. error : " + returnCode,
						"SnetDevice.GetRollover");
				}
				else
				{
					position = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetRollover");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region I/O (H/W Address Mapping)

		/// <summary>
		/// 하드웨어적으로 Mapping된 I/O Address에 따른 모든 Input Port 읽기
		/// </summary>
		/// <param name="net"> 		: controller id														</param>						
		/// <param name="ports">	: [array], "배열 크기 -> 4", "1" ->입력 on, "0" -> 입력 off 
		///
		///							[  SNET-P  ]
		///							ports[0] : Axis3 ~ Axis0 / ports[1] : Axis7 ~ Axis4
		///
		///							bit31 : User					bit15 : NC
		///							bit30 : Home	(Axis3/7)		bit14 : NC
		///							bit29 : +Limit	(Axis3/7)		bit13 : NC 
		///							bit28 : -Limit	(Axis3/7)		bit12 : NC 
		///							bit27 : User					bit11 : User  	
		///							bit26 : Home	(Axis2/6)		bit10 : Alarm	(Axis3/7)
		///							bit25 : +Limit	(Axis2/6)	    bit9  : Ready	(Axis3/7) 	
		///							bit24 : -Limit	(Axis2/6)		bit8  : User 
		///							bit23 : User					bit7  : Alarm	(Axis2/6)	
		///							bit22 : Home	(Axis1/5)	 	bit6  : Ready	(Axis2/6)	
		///							bit21 : +Limit	(Axis1/5)		bit5  : User 	
		///							bit20 : -Limit	(Axis1/5)		bit4  : Alarm	(Axis1/5)	
		///							bit19 : User					bit3  : Ready	(Axis1/5)	
		///							bit18 : Home	(Axis0/4)	 	bit2  : User 0
		///							bit17 : +Limit	(Axis0/4)	    bit1  : Alarm	(Axis0/4)	
		///							bit16 : -Limit	(Axis0/4)		bit0  : Ready	(Axis0/4) 	
		/// 
		///							[  SNET-RTEX  ]
		///							ports[0] : Axis7 ~ Axis0	/ ports[1] : Axis15 ~ Axis8 
		///							ports[2] : Axis23 ~ Axis16	/ ports[3] : Axis31 ~ Axis24 	
		/// 
		///							bit31 : User	(Axis7/15/23/31)	bit15 : User	(Axis3/11/19/27)
		///							bit30 : Home	(Axis7/15/23/31)	bit14 : Home	(Axis3/11/19/27)
		///							bit29 : +Limit	(Axis7/15/23/31)	bit13 : +Limit	(Axis3/11/19/27)
		///							bit28 : -Limit	(Axis7/15/23/31)	bit12 : -Limit	(Axis3/11/19/27)
		///							bit27 : User	(Axis6/14/22/30)	bit11 : User  	(Axis2/10/18/26)
		///							bit26 : Home	(Axis6/14/22/30)	bit10 : Home	(Axis2/10/18/26)
		///							bit25 : +Limit	(Axis6/14/22/30)    bit9  : +Limit	(Axis2/10/18/26)
		///							bit24 : -Limit	(Axis6/14/22/30)	bit8  : -Limit	(Axis2/10/18/26)
		///							bit23 : User	(Axis5/13/21/29)	bit7  : User    (Axis1/9/17/25) 
		///							bit22 : Home	(Axis5/13/21/29)	bit6  : Home	(Axis1/9/17/25) 
		///							bit21 : +Limit	(Axis5/13/21/29)	bit5  : +Limit	(Axis1/9/17/25) 
		///							bit20 : -Limit	(Axis5/13/21/29)	bit4  : -Limit	(Axis1/9/17/25) 
		///							bit19 : User	(Axis4/12/20/28)	bit3  : User	(Axis0/8/16/24) 
		///							bit18 : Home	(Axis4/12/20/28)	bit2  : Home	(Axis0/8/16/24) 
		///							bit17 : +Limit	(Axis4/12/20/28)    bit1  : +Limit	(Axis0/8/16/24) 
		///							bit16 : -Limit	(Axis4/12/20/28)	bit0  : -Limit	(Axis0/8/16/24) 
		/// 
		///							[ SNET-EMD-L2K ]
		///                         ports[0] : 상위 워드 -> X1.F ~ X1.0 /하위 워드 -> X0.F ~ X0.0 
		///
		///							bit31 : NC						bit15 : User	(X0.F)
		///							bit30 : NC 						bit14 : User	(X0.E)
		///							bit29 : NC						bit13 : User	(X0.D)
		///							bit28 : NC 						bit12 : User	(X0.C)
		///							bit27 : User		   (X1.B)	bit11 : User	(X0.B)
		///							bit26 : Alarm3	(Axis3)(X1.A)	bit10 : User	(X0.A)
		///							bit25 : Ready	(Axis3)(X1.9)	bit9  : User	(X0.9)	
		///							bit24 : User		   (X1.8)	bit8  : User	(X0.8)	
		///							bit23 : Alarm	(Axis2)(X1.7)	bit7  : User	(X0.7)	
		///							bit22 : Ready	(Axis2)(X1.6)	bit6  : User	(X0.6)	
		///							bit21 : User		   (X1.5)	bit5  : User	(X0.5)	
		///							bit20 : Alarm	(Axis1)(X1.4)	bit4  : User	(X0.4)	
		///							bit19 : Ready	(Axis1)(X1.3)	bit3  : User	(X0.3)	
		///							bit18 : User		   (X1.2)	bit2  : User	(X0.2)	
		///							bit17 : Alarm	(Axis0)(X1.1)	bit1  : User	(X0.1)	
		///							bit16 : Ready	(Axis0)(X1.0)	bit0  : User	(X0.0)	
		///							 
		///							ports[1] : 상위 워드 -> X3.F ~ X3.0 /하위 워드 -> X2.F ~ X2.0 
		///
		///							bit31 : NC					bit15 : NC
		///							bit30 : NC					bit14 : NC
		///							bit29 : NC					bit13 :	NC
		///							bit28 : NC					bit12 : NC
		///							bit27 : NC					bit11 : Home	(Axis3)(X2.B)
		///							bit26 : NC					bit10 : -Limit	(Axis3)(X2.A)
		///							bit25 : NC					bit9  : +Limit	(Axis3)(X2.9)
		///							bit24 : NC					bit8  : Home	(Axis2)(X2.8)
		///							bit23 : User	(X3.7)		bit7  : -Limit	(Axis2)(X2.7)
		///							bit22 : User	(X3.6)		bit6  : +Limit	(Axis2)(X2.6)
		///							bit21 : User	(X3.5)		bit5  : Home	(Axis1)(X2.5)
		///							bit20 : User	(X3.4)		bit4  : -Limit	(Axis1)(X2.4)
		///							bit19 : User	(X3.3)		bit3  :	+Limit	(Axis1)(X2.3)
		///							bit18 : User	(X3.2)		bit2  : Home	(Axis0)(X2.2)
		///							bit17 : User	(X3.1)		bit1  : -Limit	(Axis0)(X2.1)
		///							bit16 : User	(X3.0)		bit0  : +Limit	(Axis0)(X2.0)
		///							   
		///							ports[2] : 상위 워드 -> X5.F ~ X5.0 /하위 워드 -> X4.F ~ X4.0 
		///
		///							bit31 : NC						bit15 : NC
		///							bit30 : NC						bit14 : NC
		///							bit29 : NC						bit13 :	NC
		///							bit28 : NC						bit12 : NC
		///							bit27 : Home	(Axis7)(X5.B)	bit11 : User		   (X4.B)
		///							bit26 : -Limit  (Axis7)(X5.A)	bit10 : Alarm	(Axis7)(X4.A)
		///							bit25 : +Limit	(Axis7)(X5.9)	bit9  : Ready	(Axis7)(X4.9)
		///							bit24 : Home	(Axis6)(X5.8)	bit8  : User		   (X4.8)
		///							bit23 : -Limit	(Axis6)(X5.7)	bit7  : Alarm	(Axis6)(X4.7)
		///							bit22 : +Limit	(Axis6)(X5.6)	bit6  : Ready	(Axis6)(X4.6)
		///							bit21 : Home	(Axis5)(X5.5)	bit5  : User		   (X4.5)
		///							bit20 : -Limit	(Axis5)(X5.4)	bit4  : Alarm	(Axis5)(X4.4)
		///							bit19 : +Limit	(Axis5)(X5.3)	bit3  :	Ready	(Axis5)(X4.3)
		///							bit18 : Home	(Axis4)(X5.2)	bit2  : User		   (X4.2)
		///							bit17 : -Limit	(Axis4)(X5.1)	bit1  : Alarm	(Axis4)(X4.1)
		///							bit16 : +Limit	(Axis4)(X5.0)	bit0  : Ready	(Axis4)(X4.0)	</param>
		///
		/// <returns>				: (see enum "eSnetApiReturnCode")					</returns>
		public int GetMcbUserInput(int[] ports)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetMcbUserInput(NetID, out ports[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetMcbUserInput'. error : " + returnCode,
						"SnetCommunicator.GetMcbUserInput");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetCommunicator.GetMcbUserInput");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 하드웨어적으로 Mapping된 I/O Address에 따른 모든 Output Port 읽기
		/// </summary>
		/// <param name="net"> 		: controller id													</param>						
		/// <param name="ports">	: hardware i/o address port
		///							 [array], "배열 크기 -> 2", "1" ->출력 on, "0" -> 출력 off 
		///							
		///							[  SNET-P  ]
		///							ports[0]    
		///
		///							bit31 : NC			bit15 : Alarm Clear	(Axis7)
		///							bit30 : NC			bit14 : Servo On	(Axis7)
		///							bit29 : NC			bit13 : Alarm Clear	(Axis6)
		///							bit28 : NC			bit12 : Servo On	(Axis6)
		///							bit27 : NC			bit11 : Alarm Clear	(Axis5)
		///							bit26 : NC			bit10 : Servo On	(Axis5)
		///							bit25 : NC			bit9  : Alarm Clear	(Axis4)
		///							bit24 : NC			bit8  : Servo On	(Axis4)
		///							bit23 : NC			bit7  : Alarm Clear	(Axis3)
		///							bit22 : NC			bit6  : Servo On	(Axis3)
		///							bit21 : NC			bit5  : Alarm Clear	(Axis2)
		///							bit20 : NC			bit4  : Servo On	(Axis2)
		///							bit19 : User		bit3  : Alarm Clear	(Axis1) 
		///							bit18 : User		bit2  : Servo On	(Axis1)
		///							bit17 : User		bit1  : Alarm Clear	(Axis0) 
		///							bit16 : User		bit0  : Servo On	(Axis0) 
		///							
		///							[  SNET-RTEX  ]
		///							ports[0] : Axis15 ~ Axis0 /	ports[1] : Axis31 ~ Axis16
		///
		///							bit31 : EX-OUT2	(Axis15/31)		bit15 : EX-OUT2	(Axis7/23)
		///							bit30 : EX-OUT1	(Axis15/31)		bit14 : EX-OUT1	(Axis7/23)
		///							bit29 : EX-OUT2	(Axis14/30)		bit13 : EX-OUT2	(Axis6/22)
		///							bit28 : EX-OUT1	(Axis14/30)		bit12 : EX-OUT1	(Axis6/22)
		///							bit27 : EX-OUT2	(Axis13/29)		bit11 : EX-OUT2	(Axis5/21)
		///							bit26 : EX-OUT1	(Axis13/29)		bit10 : EX-OUT1	(Axis5/21)
		///							bit25 : EX-OUT2	(Axis12/28)		bit9  : EX-OUT2	(Axis4/20)
		///							bit24 : EX-OUT1	(Axis12/28)		bit8  : EX-OUT1	(Axis4/20)
		///							bit23 : EX-OUT2	(Axis11/27)		bit7  : EX-OUT2	(Axis3/19)
		///							bit22 : EX-OUT1	(Axis11/27)		bit6  : EX-OUT1	(Axis3/19)
		///							bit21 : EX-OUT2	(Axis10/26)		bit5  : EX-OUT2	(Axis2/18)
		///							bit20 : EX-OUT1	(Axis10/26)		bit4  : EX-OUT1	(Axis2/18)
		///							bit19 : EX-OUT2	(Axis9/25)		bit3  :	EX-OUT2	(Axis1/17)
		///							bit18 : EX-OUT1	(Axis9/25)		bit2  : EX-OUT1	(Axis1/17)
		///							bit17 : EX-OUT2	(Axis8/24)		bit1  : EX-OUT2	(Axis0/16)
		///							bit16 : EX-OUT1	(Axis8/24)		bit0  : EX-OUT1	(Axis0/16)
		/// 
		///							[ SNET-EMD-L2K ]
		///							ports[0] : 상위 워드-> Y1.F ~ Y1.0, /하위 워드-> Y0.F ~ Y0.0 
		///
		///							bit31 : NC							bit15 : User	(Y0.F)
		///							bit30 : NC							bit14 : User	(Y0.E)
		///							bit29 : NC							bit13 : User	(Y0.D)
		///							bit28 : NC							bit12 : User	(Y0.C)
		///							bit27 : User			   (Y1.B)	bit11 : User	(Y0.B)
		///							bit26 : Alarm Clear (Axis3)(Y1.A)	bit10 : User	(Y0.A)
		///							bit25 : Servo On	(Axis3)(Y1.9)	bit9  : User	(Y0.9)
		///							bit24 : User			   (Y1.8)	bit8  : User	(Y0.8)
		///							bit23 : Alarm Clear (Axis2)(Y1.7)	bit7  : User	(Y0.7)
		///							bit22 : Servo On	(Axis2)(Y1.6)   bit6  : User	(Y0.6)
		///							bit21 : User			   (Y1.5)   bit5  : User	(Y0.5)
		///							bit20 : Alarm Clear	(Axis1)(Y1.4)   bit4  : User	(Y0.4)
		///							bit19 : Servo On	(Axis1)(Y1.3)	bit3  : User	(Y0.3)
		///							bit18 : User			   (Y1.2) 	bit2  : User	(Y0.2)
		///							bit17 : Alarm Clear	(Axis0)(Y1.1)	bit1  : User	(Y0.1)
		///							bit16 : Servo On	(Axis0)(Y1.0)	bit0  : User	(Y0.0)
		/// 
		///							ports[1] : 상위 워드-> Y3.F ~ Y3.0, /하위 워드-> Y2.F ~ Y2.0 
		///
		///							bit31 : NC							bit15 : User	(Y2.F)
		///							bit30 : NC							bit14 : User	(Y2.E)
		///							bit29 : NC							bit13 : User	(Y2.D)
		///							bit28 : NC							bit12 : User	(Y2.C)
		///							bit27 : User			   (Y3.B)	bit11 : NC		(Y2.B)
		///							bit26 : Alarm Clear (Axis7)(Y3.A)	bit10 : User	(Y2.A)
		///							bit25 : Servo On	(Axis7)(Y3.9)	bit9  : User	(Y2.9)
		///							bit24 : User			   (Y3.8)	bit8  : User	(Y2.8)
		///							bit23 : Alarm Clear (Axis6)(Y3.7)	bit7  : NC		(Y2.7)
		///							bit22 : Servo On	(Axis6)(Y3.6)   bit6  : NC		(Y2.6)
		///							bit21 : User			   (Y3.5)   bit5  : NC		(Y2.5)
		///							bit20 : Alarm Clear	(Axis5)(Y3.4)   bit4  : User	(Y2.4)
		///							bit19 : Servo On	(Axis5)(Y3.3)	bit3  : User	(Y2.3)
		///							bit18 : User			   (Y3.2) 	bit2  : User	(Y2.2)
		///							bit17 : Alarm Clear	(Axis4)(Y3.1)	bit1  : User	(Y2.1)
		///							bit16 : Servo On	(Axis4)(Y3.0)	bit0  : User	(Y2.0)	</param>
		///																						
		/// <returns>				: (see enum "eSnetApiReturnCode")							</returns>
		public int GetMcbUserOutput(ref int[] ports)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetMcbUserOutput(NetID, out ports[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetMcbUserOutput'. error : " + returnCode,
						"SnetCommunicator.GetMcbUserOutput");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetCommunicator.GetMcbUserOutput");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 하드웨어적으로 Mapping된 I/O Address에 다른 임의의 한 Point 읽기
		/// </summary>
		/// <param name="net"> 		: controller id												</param>						
		/// <param name="port">		: hardware i/o address port									</param>
		///							 "0" or "1"
		///
		///							[  SNET-P  ]
		///							"port = 0" 
		///
		///							bit31 : NC			bit15 : Alarm Clear	(Axis7)
		///							bit30 : NC			bit14 : Servo On	(Axis7)
		///							bit29 : NC			bit13 : Alarm Clear	(Axis6)
		///							bit28 : NC			bit12 : Servo On	(Axis6)
		///							bit27 : NC			bit11 : Alarm Clear	(Axis5)
		///							bit26 : NC			bit10 : Servo On	(Axis5)
		///							bit25 : NC			bit9  : Alarm Clear	(Axis4)
		///							bit24 : NC			bit8  : Servo On	(Axis4)
		///							bit23 : NC			bit7  : Alarm Clear	(Axis3)
		///							bit22 : NC			bit6  : Servo On	(Axis3)
		///							bit21 : NC			bit5  : Alarm Clear	(Axis2)
		///							bit20 : NC			bit4  : Servo On	(Axis2)
		///							bit19 : User		bit3  : Alarm Clear	(Axis1) 
		///							bit18 : User		bit2  : Servo On	(Axis1)
		///							bit17 : User		bit1  : Alarm Clear	(Axis0) 
		///							bit16 : User		bit0  : Servo On	(Axis0) 
		///							
		///							[  SNET-RTEX  ]
		///							"port = 0" : Axis15 ~ Axis0 /	"port = 1" : Axis31 ~ Axis16
		///
		///							bit31 : EX-OUT2	(Axis15/31)		bit15 : EX-OUT2	(Axis7/23)
		///							bit30 : EX-OUT1	(Axis15/31)		bit14 : EX-OUT1	(Axis7/23)
		///							bit29 : EX-OUT2	(Axis14/30)		bit13 : EX-OUT2	(Axis6/22)
		///							bit28 : EX-OUT1	(Axis14/30)		bit12 : EX-OUT1	(Axis6/22)
		///							bit27 : EX-OUT2	(Axis13/29)		bit11 : EX-OUT2	(Axis5/21)
		///							bit26 : EX-OUT1	(Axis13/29)		bit10 : EX-OUT1	(Axis5/21)
		///							bit25 : EX-OUT2	(Axis12/28)		bit9  : EX-OUT2	(Axis4/20)
		///							bit24 : EX-OUT1	(Axis12/28)		bit8  : EX-OUT1	(Axis4/20)
		///							bit23 : EX-OUT2	(Axis11/27)		bit7  : EX-OUT2	(Axis3/19)
		///							bit22 : EX-OUT1	(Axis11/27)		bit6  : EX-OUT1	(Axis3/19)
		///							bit21 : EX-OUT2	(Axis10/26)		bit5  : EX-OUT2	(Axis2/18)
		///							bit20 : EX-OUT1	(Axis10/26)		bit4  : EX-OUT1	(Axis2/18)
		///							bit19 : EX-OUT2	(Axis9/25)		bit3  :	EX-OUT2	(Axis1/17)
		///							bit18 : EX-OUT1	(Axis9/25)		bit2  : EX-OUT1	(Axis1/17)
		///							bit17 : EX-OUT2	(Axis8/24)		bit1  : EX-OUT2	(Axis0/16)
		///							bit16 : EX-OUT1	(Axis8/24)		bit0  : EX-OUT1	(Axis0/16)
		/// 
		///							[ SNET-EMD-L2K ]
		///							"port = 0"  
		///
		///							bit31 : NC							bit15 : User	(Y0.F)
		///							bit30 : NC							bit14 : User	(Y0.E)
		///							bit29 : NC							bit13 : User	(Y0.D)
		///							bit28 : NC							bit12 : User	(Y0.C)
		///							bit27 : User			   (Y1.B)	bit11 : User	(Y0.B)
		///							bit26 : Alarm Clear (Axis3)(Y1.A)	bit10 : User	(Y0.A)
		///							bit25 : Servo On	(Axis3)(Y1.9)	bit9  : User	(Y0.9)
		///							bit24 : User			   (Y1.8)	bit8  : User	(Y0.8)
		///							bit23 : Alarm Clear (Axis2)(Y1.7)	bit7  : User	(Y0.7)
		///							bit22 : Servo On	(Axis2)(Y1.6)   bit6  : User	(Y0.6)
		///							bit21 : User			   (Y1.5)   bit5  : User	(Y0.5)
		///							bit20 : Alarm Clear	(Axis1)(Y1.4)   bit4  : User	(Y0.4)
		///							bit19 : Servo On	(Axis1)(Y1.3)	bit3  : User	(Y0.3)
		///							bit18 : User			   (Y1.2) 	bit2  : User	(Y0.2)
		///							bit17 : Alarm Clear	(Axis0)(Y1.1)	bit1  : User	(Y0.1)
		///							bit16 : Servo On	(Axis0)(Y1.0)	bit0  : User	(Y0.0)
		/// 
		///							"port = 1"  
		///
		///							bit31 : NC							bit15 : User	(Y2.F)
		///							bit30 : NC							bit14 : User	(Y2.E)
		///							bit29 : NC							bit13 : User	(Y2.D)
		///							bit28 : NC							bit12 : User	(Y2.C)
		///							bit27 : User			   (Y3.B)	bit11 : NC		(Y2.B)
		///							bit26 : Alarm Clear (Axis7)(Y3.A)	bit10 : User	(Y2.A)
		///							bit25 : Servo On	(Axis7)(Y3.9)	bit9  : User	(Y2.9)
		///							bit24 : User			   (Y3.8)	bit8  : User	(Y2.8)
		///							bit23 : Alarm Clear (Axis6)(Y3.7)	bit7  : NC		(Y2.7)
		///							bit22 : Servo On	(Axis6)(Y3.6)   bit6  : NC		(Y2.6)
		///							bit21 : User			   (Y3.5)   bit5  : NC		(Y2.5)
		///							bit20 : Alarm Clear	(Axis5)(Y3.4)   bit4  : User	(Y2.4)
		///							bit19 : Servo On	(Axis5)(Y3.3)	bit3  : User	(Y2.3)
		///							bit18 : User			   (Y3.2) 	bit2  : User	(Y2.2)
		///							bit17 : Alarm Clear	(Axis4)(Y3.1)	bit1  : User	(Y2.1)
		///							bit16 : Servo On	(Axis4)(Y3.0)	bit0  : User	(Y2.0)		</param>
		///
		/// <param name="point">	: point : 0 ~ 31 (bit number)									</param>
		/// <param name="onoff">	: "true"->on, "false"->off										</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")								</returns>
		public int GetMcbUserOutputPoint(int port, int point, ref bool onoff)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetMcbUserOutputPoint(NetID, port, point, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetMcbUserOutputPoint'. error : " + returnCode,
						"SnetCommunicator.GetMcbUserOutputPoint");
				}
				else
				{
					onoff = ((getValue == 1) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetCommunicator.GetMcbUserOutputPoint");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 하드웨어적으로 Mapping된 I/O Address에 다른 임의의 한 Port의 모든 point 쓰기
		/// </summary>
		/// <param name="port">	:	hardware i/o address port, "0 ~ 1"
		///							[  SNET-P  ]
		///							"port = 0" 
		///
		///							bit31 : NC			bit15 : Alarm Clear	(Axis7)
		///							bit30 : NC			bit14 : Servo On	(Axis7)
		///							bit29 : NC			bit13 : Alarm Clear	(Axis6)
		///							bit28 : NC			bit12 : Servo On	(Axis6)
		///							bit27 : NC			bit11 : Alarm Clear	(Axis5)
		///							bit26 : NC			bit10 : Servo On	(Axis5)
		///							bit25 : NC			bit9  : Alarm Clear	(Axis4)
		///							bit24 : NC			bit8  : Servo On	(Axis4)
		///							bit23 : NC			bit7  : Alarm Clear	(Axis3)
		///							bit22 : NC			bit6  : Servo On	(Axis3)
		///							bit21 : NC			bit5  : Alarm Clear	(Axis2)
		///							bit20 : NC			bit4  : Servo On	(Axis2)
		///							bit19 : User		bit3  : Alarm Clear	(Axis1) 
		///							bit18 : User		bit2  : Servo On	(Axis1)
		///							bit17 : User		bit1  : Alarm Clear	(Axis0) 
		///							bit16 : User		bit0  : Servo On	(Axis0) 
		///							
		///							[  SNET-RTEX  ]
		///							"port = 0" : Axis15 ~ Axis0 /	"port = 1" : Axis31 ~ Axis16
		///
		///							bit31 : EX-OUT2	(Axis15/31)		bit15 : EX-OUT2	(Axis7/23)
		///							bit30 : EX-OUT1	(Axis15/31)		bit14 : EX-OUT1	(Axis7/23)
		///							bit29 : EX-OUT2	(Axis14/30)		bit13 : EX-OUT2	(Axis6/22)
		///							bit28 : EX-OUT1	(Axis14/30)		bit12 : EX-OUT1	(Axis6/22)
		///							bit27 : EX-OUT2	(Axis13/29)		bit11 : EX-OUT2	(Axis5/21)
		///							bit26 : EX-OUT1	(Axis13/29)		bit10 : EX-OUT1	(Axis5/21)
		///							bit25 : EX-OUT2	(Axis12/28)		bit9  : EX-OUT2	(Axis4/20)
		///							bit24 : EX-OUT1	(Axis12/28)		bit8  : EX-OUT1	(Axis4/20)
		///							bit23 : EX-OUT2	(Axis11/27)		bit7  : EX-OUT2	(Axis3/19)
		///							bit22 : EX-OUT1	(Axis11/27)		bit6  : EX-OUT1	(Axis3/19)
		///							bit21 : EX-OUT2	(Axis10/26)		bit5  : EX-OUT2	(Axis2/18)
		///							bit20 : EX-OUT1	(Axis10/26)		bit4  : EX-OUT1	(Axis2/18)
		///							bit19 : EX-OUT2	(Axis9/25)		bit3  :	EX-OUT2	(Axis1/17)
		///							bit18 : EX-OUT1	(Axis9/25)		bit2  : EX-OUT1	(Axis1/17)
		///							bit17 : EX-OUT2	(Axis8/24)		bit1  : EX-OUT2	(Axis0/16)
		///							bit16 : EX-OUT1	(Axis8/24)		bit0  : EX-OUT1	(Axis0/16)
		/// 
		///							[ SNET-EMD-L2K ]
		///							"port = 0"  
		///
		///							bit31 : NC							bit15 : User	(Y0.F)
		///							bit30 : NC							bit14 : User	(Y0.E)
		///							bit29 : NC							bit13 : User	(Y0.D)
		///							bit28 : NC							bit12 : User	(Y0.C)
		///							bit27 : User			   (Y1.B)	bit11 : User	(Y0.B)
		///							bit26 : Alarm Clear (Axis3)(Y1.A)	bit10 : User	(Y0.A)
		///							bit25 : Servo On	(Axis3)(Y1.9)	bit9  : User	(Y0.9)
		///							bit24 : User			   (Y1.8)	bit8  : User	(Y0.8)
		///							bit23 : Alarm Clear (Axis2)(Y1.7)	bit7  : User	(Y0.7)
		///							bit22 : Servo On	(Axis2)(Y1.6)   bit6  : User	(Y0.6)
		///							bit21 : User			   (Y1.5)   bit5  : User	(Y0.5)
		///							bit20 : Alarm Clear	(Axis1)(Y1.4)   bit4  : User	(Y0.4)
		///							bit19 : Servo On	(Axis1)(Y1.3)	bit3  : User	(Y0.3)
		///							bit18 : User			   (Y1.2) 	bit2  : User	(Y0.2)
		///							bit17 : Alarm Clear	(Axis0)(Y1.1)	bit1  : User	(Y0.1)
		///							bit16 : Servo On	(Axis0)(Y1.0)	bit0  : User	(Y0.0)
		/// 
		///							"port = 1"  
		///
		///							bit31 : NC							bit15 : User	(Y2.F)
		///							bit30 : NC							bit14 : User	(Y2.E)
		///							bit29 : NC							bit13 : User	(Y2.D)
		///							bit28 : NC							bit12 : User	(Y2.C)
		///							bit27 : User			   (Y3.B)	bit11 : NC		(Y2.B)
		///							bit26 : Alarm Clear (Axis7)(Y3.A)	bit10 : User	(Y2.A)
		///							bit25 : Servo On	(Axis7)(Y3.9)	bit9  : User	(Y2.9)
		///							bit24 : User			   (Y3.8)	bit8  : User	(Y2.8)
		///							bit23 : Alarm Clear (Axis6)(Y3.7)	bit7  : NC		(Y2.7)
		///							bit22 : Servo On	(Axis6)(Y3.6)   bit6  : NC		(Y2.6)
		///							bit21 : User			   (Y3.5)   bit5  : NC		(Y2.5)
		///							bit20 : Alarm Clear	(Axis5)(Y3.4)   bit4  : User	(Y2.4)
		///							bit19 : Servo On	(Axis5)(Y3.3)	bit3  : User	(Y2.3)
		///							bit18 : User			   (Y3.2) 	bit2  : User	(Y2.2)
		///							bit17 : Alarm Clear	(Axis4)(Y3.1)	bit1  : User	(Y2.1)
		///							bit16 : Servo On	(Axis4)(Y3.0)	bit0  : User	(Y2.0)		</param>
		///
		/// <param name="points">	: bit data, "1" -> On, "0" -> Off								</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")								</returns>
		public int SetMcbUserOutputPort(int port, int points)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetMcbUserOutputPort(NetID, port, points);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetMcbUserOutputPort'. error : " + returnCode,
						"SnetCommunicator.SetMcbUserOutputPort");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetCommunicator.SetMcbUserOutputPort");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 하드웨어적으로 Mapping된 I/O Address에 다른 임의의 한 Point 쓰기
		/// </summary>
		/// <param name="net"> 	: controller id														</param>						
		/// <param name="port">		: hardware i/o address port														
		///							[  SNET-P  ]
		///							"port = 0" 
		///
		///							bit31 : NC			bit15 : Alarm Clear	(Axis7)
		///							bit30 : NC			bit14 : Servo On	(Axis7)
		///							bit29 : NC			bit13 : Alarm Clear	(Axis6)
		///							bit28 : NC			bit12 : Servo On	(Axis6)
		///							bit27 : NC			bit11 : Alarm Clear	(Axis5)
		///							bit26 : NC			bit10 : Servo On	(Axis5)
		///							bit25 : NC			bit9  : Alarm Clear	(Axis4)
		///							bit24 : NC			bit8  : Servo On	(Axis4)
		///							bit23 : NC			bit7  : Alarm Clear	(Axis3)
		///							bit22 : NC			bit6  : Servo On	(Axis3)
		///							bit21 : NC			bit5  : Alarm Clear	(Axis2)
		///							bit20 : NC			bit4  : Servo On	(Axis2)
		///							bit19 : User		bit3  : Alarm Clear	(Axis1) 
		///							bit18 : User		bit2  : Servo On	(Axis1)
		///							bit17 : User		bit1  : Alarm Clear	(Axis0) 
		///							bit16 : User		bit0  : Servo On	(Axis0) 
		///							
		///							[  SNET-RTEX  ]
		///							"port = 0" : Axis15 ~ Axis0 /	"port = 1" : Axis31 ~ Axis16
		///
		///							bit31 : EX-OUT2	(Axis15/31)		bit15 : EX-OUT2	(Axis7/23)
		///							bit30 : EX-OUT1	(Axis15/31)		bit14 : EX-OUT1	(Axis7/23)
		///							bit29 : EX-OUT2	(Axis14/30)		bit13 : EX-OUT2	(Axis6/22)
		///							bit28 : EX-OUT1	(Axis14/30)		bit12 : EX-OUT1	(Axis6/22)
		///							bit27 : EX-OUT2	(Axis13/29)		bit11 : EX-OUT2	(Axis5/21)
		///							bit26 : EX-OUT1	(Axis13/29)		bit10 : EX-OUT1	(Axis5/21)
		///							bit25 : EX-OUT2	(Axis12/28)		bit9  : EX-OUT2	(Axis4/20)
		///							bit24 : EX-OUT1	(Axis12/28)		bit8  : EX-OUT1	(Axis4/20)
		///							bit23 : EX-OUT2	(Axis11/27)		bit7  : EX-OUT2	(Axis3/19)
		///							bit22 : EX-OUT1	(Axis11/27)		bit6  : EX-OUT1	(Axis3/19)
		///							bit21 : EX-OUT2	(Axis10/26)		bit5  : EX-OUT2	(Axis2/18)
		///							bit20 : EX-OUT1	(Axis10/26)		bit4  : EX-OUT1	(Axis2/18)
		///							bit19 : EX-OUT2	(Axis9/25)		bit3  :	EX-OUT2	(Axis1/17)
		///							bit18 : EX-OUT1	(Axis9/25)		bit2  : EX-OUT1	(Axis1/17)
		///							bit17 : EX-OUT2	(Axis8/24)		bit1  : EX-OUT2	(Axis0/16)
		///							bit16 : EX-OUT1	(Axis8/24)		bit0  : EX-OUT1	(Axis0/16)
		/// 
		///							[ SNET-EMD-L2K ]
		///							"port = 0"  
		///
		///							bit31 : NC							bit15 : User	(Y0.F)
		///							bit30 : NC							bit14 : User	(Y0.E)
		///							bit29 : NC							bit13 : User	(Y0.D)
		///							bit28 : NC							bit12 : User	(Y0.C)
		///							bit27 : User			   (Y1.B)	bit11 : User	(Y0.B)
		///							bit26 : Alarm Clear (Axis3)(Y1.A)	bit10 : User	(Y0.A)
		///							bit25 : Servo On	(Axis3)(Y1.9)	bit9  : User	(Y0.9)
		///							bit24 : User			   (Y1.8)	bit8  : User	(Y0.8)
		///							bit23 : Alarm Clear (Axis2)(Y1.7)	bit7  : User	(Y0.7)
		///							bit22 : Servo On	(Axis2)(Y1.6)   bit6  : User	(Y0.6)
		///							bit21 : User			   (Y1.5)   bit5  : User	(Y0.5)
		///							bit20 : Alarm Clear	(Axis1)(Y1.4)   bit4  : User	(Y0.4)
		///							bit19 : Servo On	(Axis1)(Y1.3)	bit3  : User	(Y0.3)
		///							bit18 : User			   (Y1.2) 	bit2  : User	(Y0.2)
		///							bit17 : Alarm Clear	(Axis0)(Y1.1)	bit1  : User	(Y0.1)
		///							bit16 : Servo On	(Axis0)(Y1.0)	bit0  : User	(Y0.0)
		/// 
		///							"port = 1"  
		///
		///							bit31 : NC							bit15 : User	(Y2.F)
		///							bit30 : NC							bit14 : User	(Y2.E)
		///							bit29 : NC							bit13 : User	(Y2.D)
		///							bit28 : NC							bit12 : User	(Y2.C)
		///							bit27 : User			   (Y3.B)	bit11 : NC		(Y2.B)
		///							bit26 : Alarm Clear (Axis7)(Y3.A)	bit10 : User	(Y2.A)
		///							bit25 : Servo On	(Axis7)(Y3.9)	bit9  : User	(Y2.9)
		///							bit24 : User			   (Y3.8)	bit8  : User	(Y2.8)
		///							bit23 : Alarm Clear (Axis6)(Y3.7)	bit7  : NC		(Y2.7)
		///							bit22 : Servo On	(Axis6)(Y3.6)   bit6  : NC		(Y2.6)
		///							bit21 : User			   (Y3.5)   bit5  : NC		(Y2.5)
		///							bit20 : Alarm Clear	(Axis5)(Y3.4)   bit4  : User	(Y2.4)
		///							bit19 : Servo On	(Axis5)(Y3.3)	bit3  : User	(Y2.3)
		///							bit18 : User			   (Y3.2) 	bit2  : User	(Y2.2)
		///							bit17 : Alarm Clear	(Axis4)(Y3.1)	bit1  : User	(Y2.1)
		///							bit16 : Servo On	(Axis4)(Y3.0)	bit0  : User	(Y2.0)			</param>
		///
		/// <param name="point">	: point : 0 ~ 31 (bit number)										</param>
		/// <param name="onoff">	: "1"->on, "0"->off													</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")									</returns>		
		public int SetMcbUserOutputPoint(int port, int point, bool onoff)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (onoff ? 1 : 0);
				returnCode = eSnetSetMcbUserOutputPoint(NetID, port, point, setValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetMcbUserOutputPoint'. error : " + returnCode,
						"SnetCommunicator.SetMcbUserOutputPoint");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetCommunicator.SetMcbUserOutputPoint");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Axis I/O

		/// <summary>
		/// 실시간 축 입출력의 감지 여부를 확인
		/// (tip 1) 전기적인 "active high/active low" 상태가 아닌, 파라미터 설정값이 적용된 최종 입출력 ON/OFF 상태를 나타냅니다.  
		/// (tip 2) -limit_sensor / +limit_sensor / origin_sensor의 전기적인 active high / active low설정은 제어기 basic parameter에서 설정 합니다. 
		/// : old version name -->"eSnetGetAxisIO2"
		/// </summary>
		/// <param name="axis">				: axis index														</param>						
		/// <param name="limit_minus">		: (-)limit sensor의 실시간 입력 상태 		: "1"-> On, "0"-> Off, "3"->설정되지 않았음	</param>						
		/// <param name="limit_plus">		: (+)limit sensor의 실시간 입력 상태 		: "1"-> On, "0"-> Off, "3"->설정되지 않았음	</param>						
		/// <param name="sensor_origin">	: home sensor의 실시간 입력 상태 			: "1"-> On, "0"-> Off, "3"->설정되지 않았음	</param>						
		/// <param name="sv_alarm">			: servo alarm signal의 실시간 입력 상태 	: "1"-> On, "0"-> Off, "3"->설정되지 않았음	</param>						
		/// <param name="sv_ready">			: servo ready signal의 실시간 입력 상태 	: "1"-> On, "0"-> Off, "3"->설정되지 않았음	</param>						
		/// <param name="sv_on">			: servo on 출력 signal의 실시간 출력 상태 	: "1"-> On, "0"-> Off, "3"->설정되지 않았음	</param>								
		/// <returns>						: (see enum "eSnetApiReturnCode")														</returns>
		public int GetAxisIo(int axis, ref int limit_minus, ref int limit_plus, ref int sensor_origin, ref int sv_alarm, ref int sv_ready, ref int sv_on)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetAxisIo(NetID, axis, out int getValue0, out int getValue1,
					out int getValue2, out int getValue3, out int getValue4, out int getValue5);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetAxisIo'. error : " + returnCode,
						"SnetDevice.GetAxisIo");
				}
				else
				{
					limit_minus = getValue0;
					limit_plus = getValue1;
					sensor_origin = getValue2;
					sv_alarm = getValue3;
					sv_ready = getValue4;
					sv_on = getValue5;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetAxisIo");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 실시간 hardware 및 software limit 상태 읽기
		/// : old version name ->"eSnetGetAxisLimitStatus"
		/// </summary>
		/// <param name="axis">		: axis index							</param>						
		/// <param name="value">	: 실시간 hw/sw limit 상태, 
		///								(see "enum _eSnetLimitStatus")	
		///								"1"-> Input On / "0" -> Input Off
		///								bit 0: hardware (-)limit 입력 상태
		///								bit 1: hardware (+)limit 입력 상태
		///								bit 2: software (-)limit 입력 상태
		///								bit 3: software (-)limit 입력 상태	</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		public int GetLimitStatus(int axis, ref int value)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetLimitStatus(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetLimitStatus'. error : " + returnCode,
						"SnetDevice.GetLimitStatus");
				}
				else
				{
					value = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetLimitStatus");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// hardware limit sensor 감지시에 servo off를 할 것인지 설정 하기
		/// (tip 1) 전원 인가시 Default 상태는 "servo on 상태 유지" 입니다.  
		/// : old version name ->"SetAxisLimitAction"
		/// </summary>
		/// <param name="axis">		: axis index																				</param>						
		/// <param name="enable">	: "true"->limit Sensor 감지시 "servo off" 수행, sensor 감지 상태를 해제하고, 
		///							     eSnetReset()를 한 다음, servo on을 수행할 수 있습니다. 
		///							  "false"->Limit Sesnor 감지시 servo off를 하지 않고 멈춤, 같은 방향으로는 더 이상 진행 하지 못하고  
		///								 반대 방향으로는 움직일 수 있습니다. 
		///							  (예) (-)limit Sensor감지시 멈춤, (-)방향으로 더 이상 구동 못함, (+)방향으로 구동 가능		</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")															</returns>	
		public int SetLimitAction(int axis, bool enable)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (enable ? 1 : 0);

				returnCode = eSnetSetLimitAction(NetID, axis, setValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetLimitAction'. error : " + returnCode,
						"SnetDevice.SetLimitAction");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetLimitAction");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "eSnetSetAxisLimitAction" 설정값 확인 
		/// : old version name ->"eSnetGetAxisLimitAction"
		/// </summary>
		/// <param name="axis">		: axis index						</param>						
		/// <param name="enable">	: 설정 값							</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int GetLimitAction(int axis, ref bool enable)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetLimitAction(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetLimitAction'. error : " + returnCode,
						"SnetDevice.GetLimitAction");
				}
				else
				{
					enable = ((getValue == 1) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetLimitAction");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Basic Parameter

		/*** Set & Get Basic Parameter ***/
		/// 기본 파라메타는 제어기 전원이 꺼져도 Flash Memory에 남아 있는 Device 설정 값.

		/// <summary>
		/// 한 축당 파라메타 Number에 해당하는 값을 설정
		/// : old version name ->"eSnetWriteSystemParameter"
		/// </summary>
		/// <param name="par_no">	: parameter number ("0"~"72")		</param>
		/// <param name="axis">		: parameter를 적용할 축 번호		</param>
		/// <param name="data">		: parameter 값						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int WriteParameter(int par_no, int axis, int data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetWriteParameter(NetID, par_no, axis, data);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetWriteParameter'. error : " + returnCode,
						"SnetDevice.WriteParameter");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.WriteParameter");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 번호에 해당하는 모든 축의 값을 배열로 일괄 설정 하기
		/// /// old version name ->"eSnetWriteSystemParameters"
		/// </summary>
		/// <param name="par_no">	: parameter number ("0"~"72")										</param>
		/// <param name="data">		: 축 array 배열, "0"번 축 부터 파라메타를 적용할 축 순으로 
		///								data[0] : "0"번 축 data, 
		///								data[1] : "1"번 축 data, 등등...								</param>
		/// <param name="count">	: data의 array 배열 숫자											</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")									</returns>
		public int WriteParameters(int par_no, int[] data, int count)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				if (data.Length < count)
					return (int)eSnetApiReturnCode.InvalidArgument;

				returnCode = eSnetWriteParameters(NetID, par_no, out data[0], count);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetWriteParameters'. error : " + returnCode,
						"SnetDevice.WriteParameters");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.WriteParameters");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 한 축에 해당하는 파라메타 모두를 배열로 일괄 설정 하기
		/// : old version name ->"eSnetWriteSystemParameterByAxis"
		/// </summary>
		/// <param name="axis"> : axis index													</param>
		/// <param name="data"> : parameter array 배열, "1"번 parameter부터 "72"번 parameter까지
		///						  배열 크기: 73
		///						  (예) data[20] : "20"번 Max Speed Parameter						</param>
		/// <returns>			: (see enum "eSnetApiReturnCode")								</returns>
		public int WriteParameterByAxis(int axis, int[] data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetWriteParameterByAxis(NetID, axis, out data[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetWriteParameterByAxis'. error : " + returnCode,
						"SnetDevice.WriteParameterByAxis");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.WriteParameterByAxis");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        /// <summary>
        /// 파라메타 number에 해당하는 모든 축의 설정값을 배열로 읽기
        /// : old version name ->"eSnetReadSystemParameter"
        /// </summary>
        /// <param name="par_no">	: parameter number ("0"~"72")								</param>
        /// <param name="data">		: 축 array 배열, 배열 크기: 32 
        ///							  제품에서 지원 하는 축 수에 상관없이 배열 크기를 32로 할당 
        ///							  data[0] : "0"번 축 data, 
        ///							  data[1] : "1"번 축 data, 등등...						</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")							</returns>
        public int ReadParameter(int par_no, int[] data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetReadParameter(NetID, par_no, out data[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetReadParameter'. error : " + returnCode,
						"SnetDevice.ReadParameter");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.ReadParameter");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 한 축에 해당하는 모든 설정된 파라메타를 배열로 읽기
		/// : old version name ->"eSnetReadSystemParameterByAxis"
		/// </summary>
		/// <param name="axis"> : parameter number ("0"~"72")									</param>
		/// <param name="data">	: parameter array 배열, "1"번 parameter부터 "72"번 parameter까지
		///						  배열 크기:73 이상 
		///						  (예) data[20] : "20"번 Max Speed Parameter					</param>
		/// <returns>			: (see enum "eSnetApiReturnCode")								</returns>
		public int ReadParameterByAxis(int axis, int[] data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetReadParameterByAxis(NetID, axis, out data[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetReadParameterByAxis'. error : " + returnCode,
						"SnetDevice.ReadParameterByAxis");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.ReadParameterByAxis");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        /*** Common Parameters ***/

        public int WriteCommonParameter(int par_no, int data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetWriteCommonParameter(NetID, par_no, data);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetWriteCommonParameter'. error : " + returnCode,
						"SnetDevice.WriteCommonParameter");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.WriteCommonParameter");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        public int ReadCommonParameter(int par_no, int[] data, int count)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetReadCommonParameter(NetID, par_no, out data[0], count);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetReadCommonParameter'. error : " + returnCode,
						"SnetDevice.ReadCommonParameter");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.ReadCommonParameter");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/*** Basic Parameters ***/

		/// <summary>
		/// 파라메타 1번 : 제어 주기 설정
		/// </summary>
		/// <param name="axis">			: axis index													</param>
		/// <param name="control_hz">	: "1", SNET-P 제품은 1 KHz로 고정, 
		///									최대 1 KHz 이내에 모든 축의 지령과 Status 갱신이 이루어짐	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")								</returns>
		public int SetParam001ControlHz(int axis, int control_hz)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam001ControlHz(NetID, axis, control_hz);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam001ControlHz'. error : " + returnCode,
						"SnetDevice.SetParam001ControlHz");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam001ControlHz");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 1번 : 제어 주기 설정값 읽기
		/// </summary>
		/// <param name="axis">			: axis index													</param>
		/// <param name="control_hz">	: "1", SNET 제품은 1 KHz로 고정, 
		///									최대 1 KHz 이내에 모든 축의 지령과 Status 갱신이 이루어짐	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")								</returns>
		public int GetParam001ControlHz(int axis, ref int control_hz)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam001ControlHz(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam001ControlHz'. error : " + returnCode,
						"SnetDevice.GetParam001ControlHz");
				}
				else
				{
					control_hz = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam001ControlHz");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        /// <summary>
        /// 파라메타 2번 : 각 축에 연결된 모터 드라이버 타입(Pulse or RTEX)을 지정 합니다. 
        /// (tip) 적용 제품:SNET-RTEX
		/// </summary>
        /// <param name="axis">				: axis index								</param>
        /// <param name="control_signal">	: "0"->rtex 드라이버, "1"->Pulse 드라이버		</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        public int SetParam002ControlSignal(int axis, int control_signal)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam002ControlSignal(NetID, axis, control_signal);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam002ControlSignal'. error : " + returnCode,
						"SnetDevice.SetParam002ControlSignal");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam002ControlSignal");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        /// <summary>
        /// 파라메타 2번 : 각 축 모터 드라이버 타입(Pulse or RTEX)설정값을 읽습니다. 
        /// </summary>
        /// <param name="axis">				: axis index								</param>
        /// <param name="control_signal">	: "0"->rtex 드라이버, "1"->Pulse 드라이버		</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")			</returns>
        public int GetParam002ControlSignal(int axis, ref int control_signal)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam002ControlSignal(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam002ControlSignal'. error : " + returnCode,
						"SnetDevice.GetParam002ControlSignal");
				}
				else
				{
					control_signal = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam002ControlSignal");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 3번 : 지령 Pulse Format 설정 하기
		/// </summary>
		/// <param name="axis">			: axis index						</param>
		/// <param name="pulse_format"> : "0"-> CW/CCW 방식,
		///								  "1"-> Pulse/Direction 방식, 
		///								  "2"-> AB Phase 방식,				</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		public int SetParam003PulseFormat(int axis, int pulse_format)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam003PulseFormat(NetID, axis, pulse_format);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam003PulseFormat'. error : " + returnCode,
						"SnetDevice.SetParam003PulseFormat");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam003PulseFormat");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 3번 : 지령 Pulse Format 설정
		/// </summary>
		/// <param name="axis">			: axis index						</param>
		/// <param name="pulse_format"> : "0"-> CW/CCW 방식,
		///								  "1"-> Pulse/Direction 방식, 
		///								  "2"-> AB Phase 방식,				</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		public int GetParam003PulseFormat(int axis, ref int pulse_format)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam003PulseFormat(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam003PulseFormat'. error : " + returnCode,
						"SnetDevice.GetParam003PulseFormat");
				}
				else
				{
					pulse_format = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam003PulseFormat");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        /// <summary>
        /// 파라메타 10번 : 모터 샤프트를 1회전 하기 위해 필요한 펄스 수 를 설정 합니다. 
        /// </summary>
        /// <param name="axis">				: axis index															</param>
        /// <param name="command_rotation"> : command pulse count per motor rotation 
        ///										(tip) 일반적으로 Driver에 설정되어 있는 한 회전당 Pulse 수를 입력	</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")										</returns>
        public int SetParam010CommandRotation(int axis, int command_rotation)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam010CommandRotation(NetID, axis, command_rotation);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam010CommandRotation'. error : " + returnCode,
						"SnetDevice.SetParam010CommandRotation");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam010CommandRotation");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        /// <summary>
        /// 파라메타 10번 : "모터 샤프트를 1회전 하기 위해 필요한 펄스 수(회전당 펄스 수)" 확인 
        /// </summary>
        /// <param name="axis">				: axis index															</param>
        /// <param name="command_rotation"> : command pulse count per motor rotation 
        ///										(tip) 일반적으로 Driver에 설정되어 있는 한 회전당 Pulse 수를 입력	</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")										</returns>
        public int GetParam010CommandRotation(int axis, ref int command_rotation)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam010CommandRotation(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam010CommandRotation'. error : " + returnCode,
						"SnetDevice.GetParam010CommandRotation");
				}
				else
				{
					command_rotation = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam010CommandRotation");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 11번 : 모터 한 회전당 실제 구동 거리(um) 설정
		/// </summary>
		/// <param name="axis">					: axis index						</param>
		/// <param name="distance_rotation">	: real distance per motor rotation	</param>
		/// <returns>							: (see enum "eSnetApiReturnCode")	</returns>
		public int SetParam011DistanceRotation(int axis, int distance_rotation)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam011DistanceRotation(NetID, axis, distance_rotation);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam011DistanceRotation'. error : " + returnCode,
						"SnetDevice.SetParam011DistanceRotation");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam011DistanceRotation");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 11번 : 모터 한 회전당 실제 구동 거리(um) 설정값 읽기
		/// </summary>
		/// <param name="axis">					: axis index						</param>
		/// <param name="distance_rotation">	: real distance per motor rotation	</param>
		/// <returns>							: (see enum "eSnetApiReturnCode")	</returns>
		public int GetParam011DistanceRotation(int axis, ref int distance_rotation)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam011DistanceRotation(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam011DistanceRotation'. error : " + returnCode,
						"SnetDevice.GetParam011DistanceRotation");
				}
				else
				{
					distance_rotation = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam011DistanceRotation");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 12번 : 지령 Pulse 출력 Count에 대해 Driver로 부터 제어기로 오는 Actual Pulse(Encoder) Count 비율 설정
		/// </summary>
		/// <param name="axis">				: axis index																		</param>
		/// <param name="feedback_command"> : "command pulse" per "return actual(encoder)count" 
		///										(예) command pulse가 1000일 경우, actual pulse가 250으로 return되어 온다면,
		///											Driver에서 4체배하여 Motor을 구동하는 것이므로, 이 값을 1000/250 = "4"가 됨	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")														</returns>
		public int SetParam012FeedbackCommand(int axis, int feedback_command)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam012FeedbackCommand(NetID, axis, feedback_command);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam012FeedbackCommand'. error : " + returnCode,
						"SnetDevice.SetParam012FeedbackCommand");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam012FeedbackCommand");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 12번 : 지령 Pulse 출력 Count에 대해 Driver로 부터 제어기로 오는 Actual Pulse(Encoder) Count 비율 설정값 읽기
		/// </summary>
		/// <param name="axis">				: axis index																		</param>
		/// <param name="feedback_command">	: "command pulse" per "return actual(encoder)count" 
		///										(예) command pulse가 1000일 경우, actual pulse가 250으로 return되어 온다면,
		///											Driver에서 4체배하여 Motor을 구동하는 것이므로, 이 값을 1000/250 = "4"가 됨	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")													</returns>
		public int GetParam012FeedbackCommand(int axis, ref int feedback_command)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam012FeedbackCommand(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam012FeedbackCommand'. error : " + returnCode,
						"SnetDevice.GetParam012FeedbackCommand");
				}
				else
				{
					feedback_command = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam012FeedbackCommand");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 13번 : Motor 구동 방향 설정 하기
		/// </summary>
		/// <param name="axis">				: axis index						</param>
		/// <param name="motor_direction">	: "0"-> CW방향, "1"-> CCW방향		</param>		
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
		public int SetParam013MotorDirection(int axis, int motor_direction)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam013MotorDirection(NetID, axis, motor_direction);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam013MotorDirection'. error : " + returnCode,
						"SnetDevice.SetParam013MotorDirection");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam013MotorDirection");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 13번 : Motor 구동 방향 설정값 읽기
		/// </summary>
		/// <param name="axis">				: axis index						</param>
		/// <param name="motor_direction">	: "0"-> CW방향, "1"-> CCW방향		</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
		public int GetParam013MotorDirection(int axis, ref int motor_direction)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam013MotorDirection(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam013MotorDirection'. error : " + returnCode,
						"SnetDevice.GetParam013MotorDirection");
				}
				else
				{
					motor_direction = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam013MotorDirection");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 14번 : Encoder Phase 설정 하기
		/// </summary>
		/// <param name="axis">				: axis index						</param>
		/// <param name="encoder_phase">	: "0"->A_Phase, "1"->B_Phase		</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
		public int SetParam014EncoderPhase(int axis, int encoder_phase)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam014EncoderPhase(NetID, axis, encoder_phase);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam014EncoderPhase'. error : " + returnCode,
						"SnetDevice.SetParam014EncoderPhase");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam014EncoderPhase");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 14번 : Encoder Phase 설정값 읽기
		/// </summary>
		/// <param name="axis">				: axis index						</param>
		/// <param name="encoder_phase">	: "0"->A_Phase, "1"->B_Phase		</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
		public int GetParam014EncoderPhase(int axis, ref int encoder_phase)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam014EncoderPhase(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam014EncoderPhase'. error : " + returnCode,
						"SnetDevice.GetParam014EncoderPhase");
				}
				else
				{
					encoder_phase = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam014EncoderPhase");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파마메타 15번 : Encoder Type 설정값 읽기
		/// </summary>
		/// <param name="axis">			: axis index							</param>
		/// <param name="encoder_type"> : "0"->Incremental Encdoer,
		///								  "1"->Absolute Encoder,				</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")		</returns>
		public int SetParam015EncoderType(int axis, int encoder_type)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam015EncoderType(NetID, axis, encoder_type);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam015EncoderType'. error : " + returnCode,
						"SnetDevice.SetParam015EncoderType");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam015EncoderType");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 14번 : Encoder Phase 설정값 읽기
		/// </summary>
		/// <param name="axis">			: axis index						</param>
		/// <param name="encoder_type">	: "0"->A_Phase, "1"->B_Phase		</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		public int GetParam015EncoderType(int axis, ref int encoder_type)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam015EncoderType(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam015EncoderType'. error : " + returnCode,
						"SnetDevice.GetParam015EncoderType");
				}
				else
				{
					encoder_type = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam015EncoderType");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 20번 : 최대 구동 속도 설정 하기
		/// </summary>
		/// <param name="axis">			: axis index						</param>
		/// <param name="max_speed">	: 단위[mm/min]						</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		public int SetParam020MaxSpeed(int axis, int max_speed)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam020MaxSpeed(NetID, axis, max_speed);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam020MaxSpeed'. error : " + returnCode,
						"SnetDevice.SetParam020MaxSpeed");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam020MaxSpeed");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 20번 : 최대 구동 속도 설정값 읽기
		/// </summary>
		/// <param name="axis">			: axis index						</param>
		/// <param name="max_speed">	: 단위[mm/min]						</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		public int GetParam020MaxSpeed(int axis, ref int max_speed)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam020MaxSpeed(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam020MaxSpeed'. error : " + returnCode,
						"SnetDevice.GetParam020MaxSpeed");
				}
				max_speed = getValue;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam020MaxSpeed");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 21번 : 구동 중 Emergence Stop이나 Reset시에 정지시 감속 시간 설정 하기
		/// </summary>
		/// <param name="axis">				: axis index						</param>
		/// <param name="reset_dectime">	: 단위 [msec]						</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
		public int SetParam021ResetDecTime(int axis, int reset_dectime)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam021ResetDecTime(NetID, axis, reset_dectime);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam021ResetDecTime'. error : " + returnCode,
						"SnetDevice.SetParam021ResetDecTime");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam021ResetDecTime");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 21번 : 구동 중 Emergence Stop이나 Reset시에 정지시 감속 시간 설정 하기
		/// </summary>
		/// <param name="axis">				: axis index						</param>
		/// <param name="reset_dectime">	: 단위 [msec]						</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
		public int GetParam021ResetDecTime(int axis, ref int reset_dectime)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam021ResetDecTime(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam021ResetDecTime'. error : " + returnCode,
						"SnetDevice.GetParam021ResetDecTime");
				}
				else
				{
					reset_dectime = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam021ResetDecTime");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 30번 : (+)방향 Software Limit Position 설정 하기
		/// </summary>
		/// <param name="axis">			: axis id							</param>
		/// <param name="plus_swlimit"> : (+)방향 Software Limit 값			</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		public int SetParam030PlusSwLimit(int axis, int plus_swlimit)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam030PlusSwLimit(NetID, axis, plus_swlimit);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam030PlusSwLimit'. error : " + returnCode,
						"SnetDevice.SetParam030PlusSwLimit");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam030PlusSwLimit");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 30번 : (+)방향 Software Limit Position 설정값 읽기
		/// </summary>
		/// <param name="axis">			: axis id							</param>
		/// <param name="plus_swlimit"> : (+)방향 Software Limit 값			</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		public int GetParam030PlusSwLimit(int axis, ref int plus_swlimit)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam030PlusSwLimit(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam030PlusSwLimit'. error : " + returnCode,
						"SnetDevice.GetParam030PlusSwLimit");
				}
				else
				{
					plus_swlimit = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam030PlusSwLimit");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 31번 : (-)방향 Software Limit Position 설정 하기
		/// </summary>
		/// <param name="axis">				: axis id							</param>
		/// <param name="minus_swlimit">	: (-)방향 Software Limit 값			</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
		public int SetParam031MinusSwLimit(int axis, int minus_swlimit)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam031MinusSwLimit(NetID, axis, minus_swlimit);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam031MinusSwLimit'. error : " + returnCode,
						"SnetDevice.SetParam031MinusSwLimit");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam031MinusSwLimit");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 31번 : (-)방향 Software Limit Position 설정 하기
		/// </summary>
		/// <param name="axis">				: axis id							</param>
		/// <param name="minus_swlimit">	: (-)방향 Software Limit 값			</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
		public int GetParam031MinusSwLimit(int axis, ref int minus_swlimit)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam031MinusSwLimit(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam031MinusSwLimit'. error : " + returnCode,
						"SnetDevice.GetParam031MinusSwLimit");
				}
				else
				{
					minus_swlimit = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam031MinusSwLimit");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        /// <summary>
        /// 파라메타 32번 : 모터 구동시 지령 좌표와 인코더 입력으로 계산된 현재 좌표의 편차 한계치 설정 하기
        /// </summary>
        /// <param name="axis">			: axis id							</param>
        /// <param name="run_error">	: 단위 [um]							</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
        public int SetParam032RunFollowingError(int axis, int run_error)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam032RunFollowingError(NetID, axis, run_error);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam032RunFollowingError'. error : " + returnCode,
						"SnetDevice.SetParam032RunFollowingError");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam032RunFollowingError");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 32번 : Motor 구동시 Command Position과 Actucal Position간 Following Error 설정값 일기
		/// </summary>
		/// <param name="axis">			: axis id							</param>
		/// <param name="run_error">	: 단위 [um]							</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		public int GetParam032RunFollowingError(int axis, ref int run_error)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam032RunFollowingError(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam032RunFollowingError'. error : " + returnCode,
						"SnetDevice.GetParam032RunFollowingError");
				}
				else
				{
					run_error = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam032RunFollowingError");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        /// <summary>
        /// 파라메타 33번 : 모터 정지시 지령 좌표와 인코더 입력으로 계산된 현재 좌표의 편차 한계치 설정 하기
        /// </summary>
        /// <param name="axis">			: axis id							</param>
        /// <param name="idle_error">	: 단위 [um]							</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
        public int SetParam033IdleFollowingError(int axis, int idle_error)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam033IdleFollowingError(NetID, axis, idle_error);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam033IdleFollowingError'. error : " + returnCode,
						"SnetDevice.SetParam033IdleFollowingError");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam033IdleFollowingError");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        /// <summary>
        /// 파라메타 33번 : 모터 정지시 지령 좌표와 인코더 입력으로 계산된 현재 좌표의 편차 한계치 설정 상태 읽기 
        /// </summary>
        /// <param name="axis">			: axis id							</param>
        /// <param name="idle_error">	: 단위 [um]							</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
        public int GetParam033IdleFollowingError(int axis, ref int idle_error)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam033IdleFollowingError(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam033IdleFollowingError'. error : " + returnCode,
						"SnetDevice.GetParam033IdleFollowingError");
				}
				else
				{
					idle_error = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam033IdleFollowingError");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 40번 : Servo Ready Singal 사용 여부 설정 하기
		/// </summary>
		/// <param name="axis">		: axis id							</param>
		/// <param name="sv_ready">	: "0"->사용하지 않음,
		///							  "1"->사용함,						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int SetParam040SvReady(int axis, int sv_ready)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam040SvReady(NetID, axis, sv_ready);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam040SvReady'. error : " + returnCode,
						"SnetDevice.SetParam040SvReady");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam040SvReady");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 40번 : Servo Ready Singal 사용 여부 설정값 읽기
		/// </summary>
		/// <param name="axis">		: axis id							</param>
		/// <param name="sv_ready">	: "0"->사용하지 않음,	
		///							  "1"->사용함,						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int GetParam040SvReady(int axis, ref int sv_ready)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam040SvReady(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam040SvReady'. error : " + returnCode,
						"SnetDevice.GetParam040SvReady");
				}
				else
				{
					sv_ready = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam040SvReady");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 41번 : ServoAlarm Singal 사용 여부 설정 하기
		/// </summary>
		/// <param name="axis">		: axis id							</param>
		/// <param name="sv_alarm">	: "0"->사용하지 않음,	
		///							  "1"->사용함,						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int SetParam041SvAlarm(int axis, int sv_alarm)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam041SvAlarm(NetID, axis, sv_alarm);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam041SvAlarm'. error : " + returnCode,
						"SnetDevice.SetParam041SvAlarm");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam041SvAlarm");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 41번 : ServoAlarm Singal 사용 여부 설정 하기
		/// </summary>
		/// <param name="axis">		: axis id							</param>
		/// <param name="sv_alarm">	: "0"->사용하지 않음,	
		///							  "1"->사용함,						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int GetParam041SvAlarm(int axis, ref int sv_alarm)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam041SvAlarm(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam041SvAlarm'. error : " + returnCode,
						"SnetDevice.GetParam041SvAlarm");
				}
				else
				{
					sv_alarm = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam041SvAlarm");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 43번 : Servo_Enable Active_Level 설정 하기
		/// </summary>
		/// <param name="axis"> : axis id </param>
		/// <param name="sv_on"> : "0"->사용하지 않음, 
		///						  "1"->Normal_Open (Active_High) Servo On 방식, 
		///                       "2"->Normal_Close (Active_Low) Servo On 방식,	</param>
		/// <returns>			: (see enum "eSnetApiReturnCode")				</returns>
		public int SetParam043SvOn(int axis, int sv_on)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam043SvOn(NetID, axis, sv_on);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam043SvOn'. error : " + returnCode,
						"SnetDevice.SetParam043SvOn");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam043SvOn");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 43번 : Servo_Enable Active_Level 설정값 읽기
		/// </summary>
		/// <param name="axis"> : axis id </param>
		/// <param name="sv_on"> : "0"->사용하지 않음 
		///						  "1"->Normal_Open (Active_High) Servo On 방식, 
		///                       "2"->Normal_Close (Active_Low) Servo On 방식,	</param>
		/// <returns>			: (see enum "eSnetApiReturnCode")				</returns>
		public int GetParam043SvOn(int axis, ref int sv_on)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam043SvOn(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam043SvOn'. error : " + returnCode,
						"SnetDevice.GetParam043SvOn");
				}
				else
				{
					sv_on = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam043SvOn");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 45번 : (-)Limit Sensor 사용 여부와 Active_Level 설정 하기
		/// </summary>
		/// <param name="axis">				: axis index									</param>
		/// <param name="use_and_level">	: "0"->사용하지 않음,
		///									  "1"->Normal_Open (Active_High) Sensor Level, 
		///									  "2"->Normal_Close (Active_Low) Sensor Level,	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")				</returns>
		public int SetParam045SensorLimitNegative(int axis, int use_and_level)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam045SensorLimitNegative(NetID, axis, use_and_level);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam045SensorLimitNegative'. error : " + returnCode,
						"SnetDevice.SetParam045SensorLimitNegative");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam045SensorLimitNegative");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 45번 : (-)Limit Sensor 사용 여부와 Active_Level 설정값 읽기
		/// </summary>
		/// <param name="axis">				: axis index									</param>
		/// <param name="use_and_level">	: "0"->사용하지 않음,
		///									  "1"->Normal_Open (Active_High) Sensor Level, 
		///									  "2"->Normal_Close (Active_Low) Sensor Level,	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")				</returns>
		public int GetParam045SensorLimitNegative(int axis, ref int use_and_level)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam045SensorLimitNegative(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam045SensorLimitNegative'. error : " + returnCode,
						"SnetDevice.GetParam045SensorLimitNegative");
				}
				else
				{
					use_and_level = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam045SensorLimitNegative");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 46번 : (+)Limit Sensor 사용 여부와 Active_Level 설정 하기
		/// </summary>
		/// <param name="axis">				: axis index									</param>
		/// <param name="use_and_level">	: "0"->사용하지 않음,
		///									  "1"->Normal_Open (Active_High) Sensor Level, 
		///									  "2"->Normal_Close (Active_Low) Sensor Level,	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")				</returns>
		public int SetParam046SensorLimitPositive(int axis, int use_and_level)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam046SensorLimitPositive(NetID, axis, use_and_level);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam046SensorLimitPositive'. error : " + returnCode,
						"SnetDevice.SetParam046SensorLimitPositive");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam046SensorLimitPositive");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 46번 : (+)Limit Sensor 사용 여부와 Active_Level 설정값 읽기
		/// </summary>
		/// <param name="axis">				: axis index									</param>
		/// <param name="use_and_level">	: "0"->사용하지 않음,
		///									  "1"->Normal_Open (Active_High) Sensor Level, 
		///									  "2"->Normal_Close (Active_Low) Sensor Level,	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")				</returns>
		public int GetParam046SensorLimitPositive(int axis, ref int use_and_level)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam046SensorLimitPositive(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam046SensorLimitPositive'. error : " + returnCode,
						"SnetDevice.GetParam046SensorLimitPositive");
				}
				else
				{
					use_and_level = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam046SensorLimitPositive");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 47번 : Origin(=Home) Sensor 사용 여부와 Active_Level 설정 하기
		/// </summary>
		/// <param name="axis">				: axis index									</param>
		/// <param name="use_and_level">	: "0"->사용하지 않음,
		///									  "1"->Normal_Open (Active_High) Sensor Level, 
		///									  "2"->Normal_Close (Active_Low) Sensor Level,	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")				</returns>
		public int SetParam047SensorOrigin(int axis, int use_and_level)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam047SensorOrigin(NetID, axis, use_and_level);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam047SensorOrigin'. error : " + returnCode,
						"SnetDevice.SetParam047SensorOrigin");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam047SensorOrigin");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 47번 : Origin(=Home) Sensor 사용 여부와 Active_Level 설정값 읽기
		/// </summary>
		/// <param name="axis">				: axis index									</param>
		/// <param name="use_and_level">	: "0"->사용하지 않음,
		///									  "1"->Normal_Open (Active_High) Sensor Level, 
		///									  "2"->Normal_Close (Active_Low) Sensor Level,	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")				</returns>
		public int GetParam047SensorOrigin(int axis, ref int use_and_level)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam047SensorOrigin(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam047SensorOrigin'. error : " + returnCode,
						"SnetDevice.GetParam047SensorOrigin");
				}
				else
				{
					use_and_level = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam047SensorOrigin");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 70번 : Inposition Pulse Margin(command & actual point error) 설정 하기
		/// </summary>
		/// <param name="axis">				: axis index											</param>
		/// <param name="inposition_pulse">	: 단위 [pulse], 							
		/// <returns>						: (see enum "eSnetApiReturnCode")						</returns>
		public int SetParam070InPositionPulse(int axis, int inposition_pulse)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam070InPositionPulse(NetID, axis, inposition_pulse);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam070InPositionPulse'. error : " + returnCode,
						"SnetDevice.SetParam070InPositionPulse");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam070InPositionPulse");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 70번 : Inposition Pulse Margin(command & actual point error) 설정값 읽기
		/// </summary>
		/// <param name="axis">				: axis index											</param>
		/// <param name="inposition_pulse">	: 단위 [pulse], 							
		/// <returns>						: (see enum "eSnetApiReturnCode")						</returns>
		public int GetParam070InPositionPulse(int axis, ref int inposition_pulse)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam070InPositionPulse(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam070InPositionPulse'. error : " + returnCode,
						"SnetDevice.GetParam070InPositionPulse");
				}
				else
				{
					inposition_pulse = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam070InPositionPulse");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 71번 : Inposition Monitoring 시간 폭 설정 하기
		/// </summary>
		/// <param name="axis">				: axis id											</param>
		/// <param name="inposition_time">	: inposition을 monitoring 할 시간 범위, 단위 [msec]	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")					</returns>
		public int SetParam071InPositionTime(int axis, int inposition_time)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam071InPositionTime(NetID, axis, inposition_time);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam071InPositionTime'. error : " + returnCode,
						"SnetDevice.SetParam071InPositionTime");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam071InPositionTime");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 71번 : Inposition Monitoring 시간 폭 설정값 읽기
		/// </summary>
		/// <param name="axis">				: axis id											</param>
		/// <param name="inposition_time">	: inposition을 monitoring 할 시간 범위, 단위 [msec]	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")					</returns>
		public int GetParam071InPositionTime(int axis, ref int inposition_time)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam071InPositionTime(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam071InPositionTime'. error : " + returnCode,
						"SnetDevice.GetParam071InPositionTime");
				}
				else
				{
					inposition_time = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam071InPositionTime");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 72번 : Inposition 최대 Monitoring time out 시간 설정 하기
		/// : timeout 시간이 지나면, Inposition이 되지 않아도, inposition monitoring을 끝내고, motion done으로 함
		/// </summary>
		/// <param name="axis">					: axis id							</param>
		/// <param name="inposition_timeout">	: Inposition Timeout, 단위 [msec]	</param>
		/// <returns>							: (see enum "eSnetApiReturnCode")	</returns>
		public int SetParam072InPositionTimeOut(int axis, int inposition_timeout)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetParam072InPositionTimeOut(NetID, axis, inposition_timeout);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetParam072InPositionTimeOut'. error : " + returnCode,
						"SnetDevice.SetParam072InPositionTimeOut");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetParam072InPositionTimeOut");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 72번 : Inposition 최대 Monitoring time out 시간 설정값 읽기
		/// : timeout 시간이 지나면, Inposition이 되지 않아도, inposition monitoring을 끝내고, motion done으로 함
		/// </summary>
		/// <param name="axis">					: axis id							</param>
		/// <param name="inposition_timeout">	: Inposition Timeout, 단위 [msec]	</param>
		/// <returns>							: (see enum "eSnetApiReturnCode")	</returns>
		public int GetParam072InPositionTimeOut(int axis, ref int inposition_timeout)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetParam072InPositionTimeOut(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetParam072InPositionTimeOut'. error : " + returnCode,
						"SnetDevice.GetParam072InPositionTimeOut");
				}
				else
				{
					inposition_timeout = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetParam072InPositionTimeOut");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 파라메타 쓰기를 했던 내용을 물리적으로 Flash에 Writing 시키는 함수
		/// </summary>
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>
		public int FlushFlashMemory()
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetFlushFlashMemory(NetID);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetFlushFlashMemory'. error : " + returnCode,
						"SnetDevice.FlushFlashMemory");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.FlushFlashMemory");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Flash에 갱신 되었던 내용을 Controller를 Reset시키고 RAM에 반영시키는 함수
		/// </summary>
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>
		public int RestartController()
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRestartController(NetID);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRestartController'. error : " + returnCode,
						"SnetDevice.RestartController");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RestartController");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Software Limit

		/*** Virtual Limit Software ***/

		/// <summary>
		/// software (-)limit 설정 하기
		/// (tip 1) 제어기 전원이 꺼지면, 이 함수로 설정된 설정값은 사라진다(Flash 메모리에 저장 되지 않고, RAM 변수 변경)
		/// : old version name -->"eSnetSetAxisNegativeSoftwareLimit","eSnetSetAxisPositiveSoftwareLimit"
		/// </summary>
		/// <param name="axis">				: axis index						</param>						
		/// <param name="negative_limit">	: negative limit position			</param>
		/// <param name="positive_limit">	: positive limit position			</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>		
		public int SetSoftwarePositionLimit(int axis, int negative_limit, int positive_limit)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetSoftwarePositionLimit(NetID, axis, negative_limit, positive_limit);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetSoftwarePositionLimit'. error : " + returnCode,
						"SnetDevice.SetSoftwarePositionLimit");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetSoftwarePositionLimit");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// software (-)limit 설정값 읽기
		/// : old version name -->"eSnetGetAxisNegativeSoftwareLimit","eSnetGetAxisPositiveSoftwareLimit"
		/// </summary>
		/// <param name="axis">				: axis index						</param>						
		/// <param name="negative_limit">	: negative limit position			</param>
		/// <param name="positive_limit">	: positive limit position			</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>		
		public int GetSoftwarePositionLimit(int axis, ref int negative_limit, ref int positive_limit)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetSoftwarePositionLimit(NetID, axis, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetSoftwarePositionLimit'. error : " + returnCode,
						"SnetDevice.GetSoftwarePositionLimit");
				}
				else
				{
					negative_limit = getValue0;
					positive_limit = getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetSoftwarePositionLimit");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Motor Direction

		/// <summary>
		/// 축별 Motor 회전 방향 변경 
		/// (tip 1) 본 함수의 설정값은 제어기 전원 재 인가시 사라집니다(휘발성),항시 적용 하려면 "HMI"를 이용하여    
		///		  "basic parameter의 P13"을 변경하고 제어기에 저장하여(비휘발성) 사용하십시요 
		/// : old version name ->"eSnetSetAxisMotorDirection"
		/// </summary>
		/// <param name="axis">			: axis index						</param>						
		/// <param name="direction">	: "1"->CW, "-1"->CCW  				</param>	
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		public int SetMotorDirection(int axis, eSnetMoveDirection direction)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetMotorDirection(NetID, axis, (int)direction);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetMotorDirection'. error : " + returnCode,
						"SnetDevice.SetMotorDirection");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetMotorDirection");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 축별 Motor 회전 방향 설정값 확인 
		/// : old version name ->"eSnetGetAxisMotorDirection"
		/// </summary>
		/// <param name="axis">			: axis index						</param>						
		/// <param name="direction">	: "1"->CW, "-1"->CCW  				</param>	
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>		
		public int GetMotorDirection(int axis, ref eSnetMoveDirection direction)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetMotorDirection(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetMotorDirection'. error : " + returnCode,
						"SnetDevice.GetMotorDirection");
				}
				else
				{
					direction = (eSnetMoveDirection)getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetMotorDirection");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Stop Setting

		/// <summary>
		/// "기본 파라미터 P21(Reset Dec.Time)" 변경
		/// (tip 1) "eSnetReset 또는 eSnetEmergencyStop"함수 실행시 "감속비" 계산에 사용 됩니다.  
		/// (tip 2) 본 함수의 설정값은 제어기 전원 재 인가시 사라집니다(휘발성),항시 적용 하려면 "HMI"를 
		///			이용하여 "basic parameter의 P13"을 변경하고 제어기에 저장하여(비휘발성) 사용하십시요    
		/// : old version name ->"SetAxisStopDcc"
		/// </summary>
		/// <param name="axis">	: axis index						</param>	
		/// <param name="data">	: 감속 시간 (msec)					</param>	
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>	
		public int SetStopDcc(int axis, int data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetStopDcc(NetID, axis, data);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetStopDcc'. error : " + returnCode,
						"SnetDevice.SetStopDcc");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetStopDcc");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "기본 파라미터 P21(Reset Dec.Time)"설정값 확인 
		/// : old version name ->"eSnetGetAxisStopDcc"
		/// </summary>
		/// <param name="axis">	: axis index						</param>	
		/// <param name="data">	: 감속 시간 (msec)					</param>	
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>	
		public int GetStopDcc(int axis, ref int data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetStopDcc(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetStopDcc'. error : " + returnCode,
						"SnetDevice.GetStopDcc");
				}
				else
				{
					data = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetStopDcc");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Following Error

		/// # Run Following Error   : During drive, the difference between the command position and 
		///							 the actual position is outside the set value 
		/// # Sleep Following Error : During steady state, the difference between the command position and 
		///							 the actual position is outside the set value 

		/// <summary>
		/// "Run following error"기준 거리 설정 
		/// (tip 1)"Basic parameter P32(1th Following Error)"설정값을 변경 합니다. 
		/// (tip 2)설정값이 "0"이면 "Run Following Error"검출 기능이 비활성화 됩니다. 
		/// (tip 3)본 설정값은 제어기 전원 재 인가시 사라집니다(휘발성),영구적으로 적용하기 위해서는 "HMI"을 사용하여
		///        "Basic parameter P32(1th Following Error)"를 변경하고 제어기에 저장하여 사용 하십시요 
		/// : old version name ->"SetAxisRunFollowingError"
		/// </summary>
		/// <param name="axis">				: axis index						</param>						
		/// <param name="position_error">	: [um],지령값과 실제 위치값의 차이	</param>	
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>	
		public int SetRunFollowingError(int axis, int position_error)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetRunFollowingError(NetID, axis, position_error);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetRunFollowingError'. error : " + returnCode,
						"SnetDevice.SetRunFollowingError");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetRunFollowingError");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Run following error" 설정값 읽기
		/// : old version name ->"GetAxisRunFollowingError"
		/// </summary>
		/// <param name="axis">				: axis index						</param>						
		/// <param name="position_error">	: [um] 지령값과 실제 위치값의 차이	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>	
		public int GetRunFollowingError(int axis, ref int position_error)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetRunFollowingError(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetRunFollowingError'. error : " + returnCode,
						"SnetDevice.GetRunFollowingError");
				}
				else
				{
					position_error = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetRunFollowingError");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Sleep(Stop) following error"기준 거리 설정
		/// (tip 1)"Basic parameter P33(2nd Following Error)"설정값을 변경 합니다. 
		/// (tip 2)설정값이 "0"이면 "Sleep(Stop) Following Error"검출 기능이 비활성화 됩니다. 
		/// (tip 3)본 설정값은 제어기 전원 재 인가시 사라집니다(휘발성),영구적으로 적용하기 위해서는 "HMI"을 사용하여
		///        "Basic parameter P33(2nd Following Error)"을 변경하고 제어기에 저장하여 사용 하십시요 
		/// : old version name ->"SetAxisSleepFollowingError"
		/// </summary>
		/// <param name="axis">				: axis index						</param>						
		/// <param name="position_error">	: [um] 지령값과 위치값의 차이		</param>	
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>		
		public int SetSleepFollowingError(int axis, int position_error)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetSleepFollowingError(NetID, axis, position_error);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetSleepFollowingError'. error : " + returnCode,
						"SnetDevice.SetSleepFollowingError");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetAxisSleepFollowingError");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Sleep(Stop) following error" 설정값 읽기
		/// : old version name ->"GetAxisSleepFollowingError"
		/// </summary>
		/// <param name="axis">				: axis index						</param>						
		/// <param name="position_error">	: [um] 지령값과 위치값의 차이		</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>		
		public int GetSleepFollowingError(int axis, ref int position_error)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetSleepFollowingError(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetSleepFollowingError'. error : " + returnCode,
						"SnetDevice.GetSleepFollowingError");
				}
				else
				{
					position_error = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetSleepFollowingError");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Inpostion

		/// <summary>
		/// "Inposiotn 완료"기준 펄스수 설정 
		/// (tip 1)"Basic parameter P70(In-posiotn Pulse)"설정값을 변경 합니다. 
		/// (tip 2)설정값이 "0"이면 "Inposition"신호 출력 기능이 비활성화(항시 ON) 됩니다.  
		/// (tip 3)본 설정값은 제어기 전원 재 인가시 사라집니다(휘발성),영구적으로 적용하기 위해서는 "HMI"을 사용하여
		///        "Basic parameter P70(In-position Pulse)"을 변경하고 제어기에 저장하여 사용 하십시요 
		/// : old version name ->"SetAxisInpositionPulseRange"
		/// </summary>
		/// <param name="axis">	: axis index										</param>						
		/// <param name="data">	: [Pulse],"Inposition 완료"기준 엔코더 입력 펄스값 	</param>	
		/// <returns>			: (see enum "eSnetApiReturnCode")					</returns>	
		public int SetInpositionPulseRange(int axis, int data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetInpositionPulseRange(NetID, axis, data);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetInpositionPulseRange'. error : " + returnCode,
						"SnetDevice.SetInpositionPulseRange");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetInpositionPulseRange");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Inposiotn 완료"기준 펄스수 설정값 확인 
		/// : old version name ->"GetAxisInpositionPulseRange"
		/// </summary>
		/// <param name="axis">	: axis index										</param>						
		/// <param name="data">	: [Pulse],"Inposition 완료"기준 엔코더 입력 펄스값 	</param>	
		/// <returns>			: (see enum "eSnetApiReturnCode")					</returns>		
		public int GetInpositionPulseRange(int axis, ref int data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetInpositionPulseRange(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetInpositionPulseRange'. error : " + returnCode,
						"SnetDevice.GetInpositionPulseRange");
				}
				else
				{
					data = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetInpositionPulseRange");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Inposition Pulse"유지 시간 설정 
		/// (tip 1)"Basic parameter P71(In-posiotn Time)"설정값을 변경 합니다. 
		/// (tip 2)본 설정값은 제어기 전원 재 인가시 사라집니다(휘발성),영구적으로 적용하기 위해서는 "HMI"을 사용하여
		///        "Basic parameter P71(In-position Time)"을 변경하고 제어기에 저장하여 사용 하십시요 
		/// : old version name ->"SetAxisInpositionDurationTime"
		/// </summary>
		/// <param name="axis">	: axis index																		</param>						
		/// <param name="data">	: [msec],"Inposition Pulse" 유지 시간,축 이송 완료 후 엔코더를 통해 입력되는 펄스
		///							갯수가 해당 시간 동안 "Inposition Pulse" 이하로 유지되면 Inposition 신호 ON		</param>	
		/// <returns>			: (see enum "eSnetApiReturnCode")	
		public int SetInpositionDuration(int axis, int data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetInpositionDuration(NetID, axis, data);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetInpositionDuration'. error : " + returnCode,
						"SnetDevice.SetInpositionDuration");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetInpositionDuration");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Inposition Pulse"유지 시간 확인 
		/// : old version name ->"GetAxisInpositionDurationTime"
		/// </summary>
		/// <param name="axis">	: axis index							</param>						
		/// <param name="data">	: [msec],"Inposition Pulse" 유지 시간	</param>	
		/// <returns>			: (see enum "eSnetApiReturnCode")		</returns>		
		public int GetInpositionDuration(int axis, ref int data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetInpositionDuration(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetInpositionDuration'. error : " + returnCode,
						"SnetDevice.GetInpositionDuration");
				}
				else
				{
					data = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetAxisInpositionDurationTime");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Inposition Check Time out" 설정 
		/// (tip 1)"Basic parameter P72(In-posiotn Escape Time)"설정값을 변경 합니다. 
		/// (tip 2)본 설정값은 제어기 전원 재 인가시 사라집니다(휘발성),영구적으로 적용하기 위해서는 "HMI"을 사용하여
		///        "Basic parameter P72(In-position Escape Time)"를 변경하고 제어기에 저장하여 사용 하십시요 
		/// : old version name ->"SetAxisInpositionTimeOut"
		/// </summary>
		/// <param name="axis">	: axis index																			</param>						
		/// <param name="data">	: [msec],"Inposition Check Time out"설정 값,모션 완료 후 해당 시간 동안 "Inposition 완료"
		///							가 않되면 "Inposition 완료"신호를 출력 하고 "out_inpos"신호가 on 됩니다.			</param>	
		/// <returns>			: (see enum "eSnetApiReturnCode")		
		public int SetInpositionTimeout(int axis, int data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetInpositionTimeout(NetID, axis, data);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetInpositionTimeout'. error : " + returnCode,
						"SnetDevice.SetInpositionTimeout");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetInpositionTimeout");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Inposition Check Time out" 설정값 확인 
		/// : old version name ->"GetAxisInpositionTimeOut"
		/// </summary>
		/// <param name="axis">	: axis index									</param>						
		/// <param name="data">	: [msec],"Inposition Check Time out"설정 값"	</param>	
		/// <returns>			: (see enum "eSnetApiReturnCode")				</returns>		
		public int GetInpositionTimeout(int axis, ref int data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetInpositionTimeout(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetInpositionTimeout'. error : " + returnCode,
						"SnetDevice.GetInpositionTimeout");
				}
				else
				{
					data = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetInpositionTimeout");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region I/O MAP

		/// <summary>
		/// I/O MAP 32bit data 읽기
		/// </summary>
		/// <param name="address">	: 읽을 data 주소</param>
		/// <param name="count">	: 읽을 data 수</param>
		/// <param name="data">		: 읽을 data</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")</returns>
		public int GetIoMap(int address, int count, int[] data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetIoMap(NetID, address, count, out data[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetIoMap'. error : " + returnCode,
													"SnetDevice.GetIoMap");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetIoMap");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// I/O MAP 32bit data 쓰기
		/// </summary>
		/// <param name="address">	: data를 쓸 주소</param>
		/// <param name="count">	: 쓸 data 수</param>
		/// <param name="data">		: 쓸 data</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")</returns>
		public int SetIoMap(int address, int count, int[] data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetIoMap(NetID, address, count, out data[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetIoMap'. error : " + returnCode,
													"SnetDevice.SetIoMap");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetIoMap");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// I/O MAP 32bit 주소 내 bit 값 읽기
		/// </summary>
		/// <param name="address">	: bit 값을 읽을 주소					</param>
		/// <param name="bit">		: bit 값을 읽을 bit 번호				</param>
		/// <param name="on">		: bit 값. true: set, false: clear	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")</returns>
		public int GetIoMapOn(int address, int bit, ref bool on)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetIoMapOn(NetID, address, bit, out int getOn);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetIoMapOn'. error : " + returnCode,
													"SnetDevice.GetIoMapOn");
				}
				else
				{
					on = ((getOn == 1) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetIoMapOn");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// I/O MAP 32bit 주소 내 bit 값 쓰기
		/// </summary>
		/// <param name="address">	: bit 값을 쓸 주소					</param>
		/// <param name="bit">		: bit 값을 쓸 bit 번호				</param>
		/// <param name="on">		: bit 값. true: set, false: clear	</param>
		///							  "true": 활성화, "false": 비활성화</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")</returns>
		public int SetIoMapOn(int address, int bit, bool on)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetIoMapOn(NetID, address, bit, (on ? 1 : 0));
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetIoMapOn'. error : " + returnCode,
													"SnetDevice.SetIoMapOn");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetIoMapOn");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// I/O Map 의 특정 bit 번호부터 복수의 bit 값 읽기
		/// </summary>
		/// <param name="start_bit">	: 복수의 bit 값을 읽을 시작 bit 번호</param>
		/// <param name="count">		: 읽을 bit 개수</param>
		/// <param name="data">			: 읽어온 복수의 bit 값이 저장될 16bit 버퍼</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")</returns>
		public int GetIoMapBit(int start_bit, int count, ushort[] data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetIoMapBit(NetID, start_bit, count, out data[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetIoMapBit'. error : " + returnCode,
													"SnetDevice.GetIoMapBit");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetIoMapBit");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// I/O Map 의 특정 bit 번호부터 복수의 bitbit 값 쓰기
		/// </summary>
		/// <param name="start_bit">	: 복수의 bit 값을 쓸 시작 bit 번호</param>
		/// <param name="count">		: 쓸 bit 개수</param>
		/// <param name="data">			: 쓸 복수의 bit 값이 저장된 16bit 버퍼</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")</returns>
		public int SetIoMapBit(int start_bit, int count, ushort[] data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetIoMapBit(NetID, start_bit, count, out data[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetIoMapBit'. error : " + returnCode,
													"SnetDevice.SetIoMapBit");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetIoMapBit");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// I/O Map 의 특정 bit 번호의 값 읽기
		/// </summary>
		/// <param name="bit">			: 읽을 bit 번호</param>
		/// <param name="on">			: 읽어온 bit 값. 1: set, 0: cleared</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")</returns>
		public int GetIoMapBitOn(int bit, ref bool on)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetIoMapBitOn(NetID, bit, out int getOn);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetIoMapBitOn'. error : " + returnCode,
													"SnetDevice.GetIoMapBitOn");
				}
				else
				{
					on = ((getOn == 1) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetIoMapBitOn");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// I/O Map 의 특정 bit 번호의 값 쓰기
		/// </summary>
		/// <param name="bit">			: 쓸 bit 번호</param>
		/// <param name="on">			: 쓸 bit 값. 1: set, 0: clear</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")</returns>
		public int SetIoMapBitOn(int bit, bool on)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetIoMapBitOn(NetID, bit, (on ? 1 : 0));
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetIoMapBitOn'. error : " + returnCode,
													"SnetDevice.SetIoMapBitOn");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetIoMapBitOn");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region User I/O (SNET-P)

		/*** User I/O ***/

		/// <summary>
		/// SNET-P4/P8 모든 축 Axis io,User io 읽기  
		/// (tip 1) 파라미터가(사용 유무,입력 극성..)적용 되지 않은 물리적 입력 상태를 보여 줍니다. 
		/// : old version name -->"eSnetGetPUserInput"
		/// </summary>
		/// <param name="ports"> 	: [array],배열크기 ->"2"											
		///								=> "1"입력 On, "0"입력 Off
		///								=> ports[1]:Axis7 ~ Axis4 / ports[0]:Axis3 ~ Axis0
		///
		///							    bit 15	    14		 13		  12   |       11		 10		  9		   8
		///							 		   	Axis3(Axis7) 			   |           Axis2(Axis6)    
		///							  User Input3   Home   +Limit   -Limit |  User Input2    Home   +Limit   -Limit 
		///							       7		 6		 5		  4    |        3		 2		  1		   0      
		///									   	Axis1(Axis5) 			   |           Axis0(Axis4)    
		///							  User Input1   Home   +Limit   -Limit |  User Input0    Home   +Limit   -Limit   </param>
		/// <returns>				: (see enum "eSnetApiReturnCode")									</returns>
		public int PulseGetUserInput(ref int[] ports)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				if (ports.Length < 2)
					return (int)eSnetApiReturnCode.InvalidArgument;

				returnCode = eSnetPulseGetUserInput(NetID, out ports[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetPulseGetUserInput'. error : " + returnCode,
						"SnetDevice.PulseGetUserInput");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.PulseGetUserInput");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-P4/P8 모든 축 Axis io,User io 읽기  
		/// (tip 1) 파라미터가(사용 유무,입력 극성..)적용 되지 않은 물리적 입력 상태를 보여 줍니다. 
		/// : old version name -->"eSnetGetPUserInputPort2"
		/// </summary>
		/// <param name="port0">	: Input value(Axis3 ~ Axis0)										</param>
		/// <param name="port1"> 	: Input value(Axis7 ~ Axis4)										
		///								=> "1"입력 On, "0"입력 Off
		///
		///							    bit 15	    14		 13		  12   |       11		 10		  9		   8
		///							 		   	Axis3(Axis7) 			   |           Axis2(Axis6)    
		///							  User Input3   Home   +Limit   -Limit |  User Input2    Home   +Limit   -Limit 
		///							       7		 6		 5		  4    |        3		 2		  1		   0      
		///									   	Axis1(Axis5) 			   |           Axis0(Axis4)    
		///							  User Input1   Home   +Limit   -Limit |  User Input0    Home   +Limit   -Limit   </param>
		///
		/// <returns>				: (see enum "eSnetApiReturnCode")									</returns>
		public int PulseGetUserInputPortAll(ref int port0, ref int port1)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetPulseGetUserInputPortAll(NetID, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetPulseGetUserInputPortAll'. error : " + returnCode,
						"SnetDevice.PulseGetUserInputPortAll");
				}
				else
				{
					port0 = getValue0;
					port1 = getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.PulseGetUserInputPortAll");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-P4/P8 모든 축(4축 단위) Axis io,User io 읽기  
		/// : old version name -->"eSnetGetPUserInputPort"
		/// </summary>
		/// <param name="port"> 	: "1"(Axis7 ~ Axis4), "0"(Axis3 ~ Axis0)							</param>						
		/// <param name="points">	: Input Value								
		///
		///							    bit 15	    14		 13		  12   |       11		 10		  9		   8
		///							 		   	Axis3(Axis7) 			   |           Axis2(Axis6)    
		///							  User Input3   Home   +Limit   -Limit |  User Input2    Home   +Limit   -Limit 
		///							       7		 6		 5		  4    |        3		 2		  1		   0      
		///									   	Axis1(Axis5) 			   |           Axis0(Axis4)    
		///							  User Input1   Home   +Limit   -Limit |  User Input0    Home   +Limit   -Limit   </param>
		///
		/// <returns>				: (see enum "eSnetApiReturnCode")									</returns>
		public int PulseGetUserInputPort(int port, ref int points)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetPulseGetUserInputPort(NetID, port, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetPulseGetUserInputPort'. error : " + returnCode,
						"SnetDevice.PulseGetUserInputPort");
				}
				else
				{
					points = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.PulseGetUserInputPort");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-P4/P8 축별 "Axis io 입력 /User io" 입력 상태를 bit 단위로 읽기 
		/// : old version name -->"eSnetGetPUserInputPoint"
		/// </summary>
		/// <param name="port"> 	: "1"(Axis7 ~ Axis4), "0"(Axis3 ~ Axis0)							</param>
		/// <param name="point">	: bit 번호 									
		///
		///							    bit 15	    14		 13		  12   |       11		 10		  9		   8
		///							 		   	Axis3(Axis7) 			   |           Axis2(Axis6)    
		///							  User Input3   Home   +Limit   -Limit |  User Input2    Home   +Limit   -Limit 
		///							       7		 6		 5		  4    |        3		 2		  1		   0      
		///									   	Axis1(Axis5) 			   |           Axis0(Axis4)    
		///							  User Input1   Home   +Limit   -Limit |  User Input0    Home   +Limit   -Limit   </param>
		///
		/// <param name="on_off">	: "1"-->on, "0"-->off												</param>
		/// <returns>				(see enum "eSnetApiReturnCode")										</returns>
		public int PulseGetUserInputPoint(int port, int point, ref bool on_off)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetPulseGetUserInputPoint(NetID, port, point, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetPulseGetUserInputPoint'. error : " + returnCode,
						"SnetDevice.PulseGetUserInputPoint");
				}
				else
				{
					on_off = ((getValue == 1) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.PulseGetUserInputPoint");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-P4/P8 "User Output" 출력 상태값 읽기 
		/// : old version name -->"eSnetGetPUserOutput"
		/// </summary>
		/// <param name="ports"> 	: [array] 배열크기 ->"2" 						
		///								=> ports[0]: User Output_1(bit1), User Output_0(bit0) (SNET-P4 User IO 커넥터)
		///								=> ports[1]: User Output_3(bit1), User Output_2(bit0) (SNET-P8 User IO 커넥터) 
		///										|	  1		|     0	   |
		///										| Output 1	| Output 0 |											</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")												</returns>
		public int PulseGetUserOutput(ref int[] ports)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				if (ports.Length < 2)
					return (int)eSnetApiReturnCode.InvalidArgument;

				returnCode = eSnetPulseGetUserOutput(NetID, out ports[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetPulseGetUserOutput'. error : " + returnCode,
						"SnetDevice.PulseGetUserOutput");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.PulseGetUserOutput");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-P4/P8 "User Output" 출력 상태값 읽기 
		/// : old version name -->"eSnetGetPUserOutputPort2"
		/// </summary>
		/// <param name="port0"> 	: Output_1, User Output_0 출력 상태 (SNET-P4 User IO 커넥터) 	</param>
		/// <param name="port1"> 	: Output_3, User Output_2 출력 상태 (SNET-P8 User IO 커넥터) 	
		///								|	  1		|     0	   |
		///								| Output 1	| Output 0 |									</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")								</returns>
		public int PulseGetUserOutputPortAll(ref int port0, ref int port1)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetPulseGetUserOutputPortAll(NetID, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetPulseGetUserOutputPortAll'. error : " + returnCode,
						"SnetDevice.PulseGetUserOutputPortAll");
				}
				else
				{
					port0 = getValue0;
					port1 = getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.PulseGetUserOUtputPortAll");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-P4/P8 "User Output" 출력 상태값 읽기 
		/// : old version name -->"eSnetGetPUserOutputPort"
		/// </summary>
		/// <param name="port"> 	: "0" or "1"
		///							  "0"->"SNET-P4 User Output1,User Output0" 출력 상태 읽기
		///							  "1"->"SNET-P8 User Output3,User Output2" 출력 상태 읽기	</param>
		/// <param name="points">	: 출력 상태 데이터 (접점1,접점0 ->2bit)						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")							</returns>
		public int PulseGetUserOutputPort(int port, ref int points)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetPulseGetUserOutputPort(NetID, port, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetPulseGetUserOutputPort'. error : " + returnCode,
						"SnetDevice.PulseGetUserOutputPort");
				}
				else
				{
					points = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.PulseGetUserOutputPort");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-P4/P8 "User Output" 출력 상태를 "bit 단위(point)"로 읽기 
		/// : old version name -->"eSnetGetPUserOutputPoint"
		/// </summary>
		/// <param name="port"> 	: "0" or "1"
		///							  "0"->"SNET-P4 User Output1,User Output0" 출력 상태 읽기
		///							  "1"->"SNET-P8 User Output3,User Output2" 출력 상태 읽기	</param>
		/// <param name="point">	: 출력 접점 번호 선택 	
		///							  "0->출력 접점 0, "1"->출력 접점 1							</param>			
		/// <param name="on_off">	: "true"-->on, "false"-->off					
		///								=>	port =0  point= 0 : "Output_0" 읽기 
		///								=>	port =0  point= 1 : "Output_1" 읽기
		///								=>	port =1  point= 0 : "Output_2" 읽기 
		///								=>	port =1  point= 1 : "Output_3" 읽기					</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")							</returns>
		public int PulseGetUserOutputPoint(int port, int point, ref bool on_off)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetPulseGetUserOutputPoint(NetID, port, point, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetPulseGetUserOutputPoint'. error : " + returnCode,
						"SnetDevice.PulseGetUserOutputPoint");
				}
				else
				{
					on_off = ((getValue == 1) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.PulseGetUserOutputPoint");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-P4/P8 "User Output" 출력 접점 쓰기 
		/// : old version name -->"eSnetSetPuserOutputPoint"
		/// </summary>
		/// <param name="port"> 	: "0" or "1"		
		///							  "0"->"SNET-P4 User Output1,User Output0"
		///							  "1"->"SNET-P8 User Output3,User Output2"	</param>
		/// <param name="point">	: "0" or "1"
		///							  "0"-> 출력 접점 0, "1"->출력 접점 1		</param>
		/// <param name="on_off">	: "true"->on, "false"->off													
		///								=>	port =0  point= 0 : "Output_0" 쓰기 
		///								=>	port =0  point= 1 : "Output_1" 쓰기 
		///								=>	port =1  point= 0 : "Output_2" 쓰기 
		///								=>	port =1  point= 1 : "Output_3" 쓰기 </param>
		/// <returns>				: (see enum "eSnetApiReturnCode")			</returns>
		public int PulseSetUserOutputPoint(int port, int point, bool on_off)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (on_off ? 1 : 0);

				returnCode = eSnetPulseSetUserOutputPoint(NetID, port, point, setValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetPulseSetUserOutputPoint'. error : " + returnCode,
						"SnetDevice.PulseSetUserOutputPoint");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.PulseSetUserOutputPoint");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region User I/O (SNET-P AD Option Board)

		/*** Extension Board I/O ***/

		/// <summary>
		/// SNET-P Extension B'D(AD Option Board)의 모든 입력 접점 읽기(INP1 ~ INP4) 
		/// (tip 1)"INP1 ~ INP4"는 제품에 표시된 커넥터 label 이름 입니다. 
		/// (tip 2)각 port(INP1~INP4)당 입력 접점은 8개 입니다. 
		/// </summary>
		/// <param name="ports"> : [array] 배열크기 ->"4"
		///											[0]:INP1 입력값, [1]:INP2 입력값 
		///											[2]:INP2 입력값, [3]:INP4 입력값	</param>
		/// <returns>							: (see enum "eSnetApiReturnCode")		</returns>
		public int PulseExGetIoInput(ref int[] ports)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				if (ports.Length < 4)
					return (int)eSnetApiReturnCode.InvalidArgument;

				returnCode = eSnetPulseExGetIoInput(NetID, out ports[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetPulseExGetIoInput'. error : " + returnCode,
						"SnetDevice.PulseExGetIoInput");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.PulseExGetIoInput");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-P Extension B'D(AD Option Board)의 모든 입력 접점 읽기(INP1 ~ INP4) 
		/// (tip 1)"INP1 ~ INP4"는 제품에 표시된 커넥터 label 이름 입니다. 
		/// (tip 2)각 "port(INP1~INP4)"당 입력 접점은 8개 입니다. 
		/// : old version name -->"eSnetGetPIoInputPort2"
		/// </summary>
		/// <param name="port0"> 	: "INP1" 입력 데이터 				</param>
		/// <param name="port1"> 	: "INP2" 입력 데이터 				</param>
		/// <param name="port2"> 	: "INP3" 입력 데이터 				</param>	
		/// <param name="port3"> 	: "INP4" 입력 데이터 				</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int PulseExGetIoInputPortAll(ref int port0, ref int port1, ref int port2, ref int port3)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetPulseExGetIoInputPortAll(NetID, out int getValue0, out int getValue1, out int getValue2, out int getValue3);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetPulseExGetIoInputPortAll'. error : " + returnCode,
						"SnetDevice.PulseExGetIoInputPortAll");
				}
				else
				{
					port0 = getValue0;
					port1 = getValue1;
					port2 = getValue2;
					port3 = getValue3;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.PulseExGetIoInputPortAll");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "SNET-P Extension B'D"의 "INP1~INP4"중 지정한 port에서 8접점(8bit) 데이터 읽기 
		/// (tip 1) "INP1 ~ INP4"는 제품에 표시된 커넥터 label 이름 입니다. 
		/// : old version name -->"eSnetGetPIoInputPort"
		/// </summary>
		/// <param name="port"> 	: 입력 포트 번호,"0(INP1)"~"3(INP4)"	</param>						
		/// <param name="points">	: 8bit 입력 접점 데이터					</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		public int PulseExGetIoInputPort(int port, ref int points)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetPulseExGetIoInputPort(NetID, port, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetPulseExGetIoInputPort'. error : " + returnCode,
						"SnetDevice.PulseExGetIoInputPort");
				}
				else
				{
					points = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.PulseExGetIoInputPort");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-P Extension B'D "INP1~INP4"중 지정한 port에서 1접점(point)입력값 읽기 
		/// : old version name -->"eSnetGetPIoInputPoint"
		/// </summary>
		/// <remarks>
		/// (tip 1) "INP1 ~ INP4"는 제품에 표시된 커넥터 label 이름 입니다. 
		/// </remarks>
		/// <param name="port"> 	: 입력 포트 번호,"0(INP1)" ~ "3(INP4)"	</param>
		/// <param name="point">	: 접점 번호,"0" ~ "7" 					</param>
		/// <param name="on_off">	: 접점 입력값,"true"->on, "false"->off	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		public int PulseExGetIoInputPoint(int port, int point, ref bool on_off)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetPulseExGetIoInputPoint(NetID, port, point, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetPulseExGetIoInputPoint'. error : " + returnCode,
						"SnetDevice.PulseExGetIoInputPoint");
				}
				else
				{
					on_off = ((getValue == 1) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.PulseExGetIoInputPoint");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-P Extension B'D의 "OUTP1 ~ OUTP3"출력 상태 데이터 읽기 
		/// (tip 1) "OUTP1 ~ OUTP3"은 제품에 표시된 커넥터 label 입니다. 
		/// (tip 2) "OUTP1 ~ OUTP2"의 출력 접점 개수는 port당 8접점이고 "OUTP3"은 4접점 입니다. 
		/// (tip 3) "접점 번호"는 "bit번호"와 1:1 대응 됩니다. 
		/// : old version name -->"eSnetGetPIoOutputPort"  
		/// </summary>
		/// <param name="ports">	: [array] 배열크기 ->"3" 				
		///								[0]:OUTP1 출력 상태 데이터(8bit), 
		///								[1]:OUTP2 출력 상태 데이터(8bit), 
		///								[2]:OUTP3 출력 상태 데이터(4bit),	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		public int PulseExGetIoOutput(ref int[] ports)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				if (ports.Length < 3)
					return (int)eSnetApiReturnCode.InvalidArgument;

				returnCode = eSnetPulseExGetIoOutput(NetID, out ports[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetPulseExGetIoOutput'. error : " + returnCode,
						"SnetDevice.PulseExGetIoOutput");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.PulseExGetIoOutput");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-P Extension B'D의 "OUTP1 ~ OUTP3" 출력 데이터 모두 읽기  
		/// (tip 1) "OUTP1 ~ OUTP3"은 제품에 표시된 커넥터 label 입니다. 
		/// (tip 2) "접점 번호"는 "bit번호"와 1:1 대응 됩니다.   
		/// : old version name -->"eSnetGetPIoOutputPort2"
		/// </summary>
		/// <param name="port0"> 	: "OUTP1" 출력 상태 데이터 			</param>
		/// <param name="port1">	: "OUTP2" 출력 상태 데이터 			</param>
		/// <param name="port2">	: "OUTP3" 출력 상태 데이터 			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int PulseExGetIoOutputPortAll(ref int port0, ref int port1, ref int port2)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetPulseExGetIoOutputPortAll(NetID, out int getValue0, out int getValue1, out int getValue2);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetPulseExGetIoOutputPortAll'. error : " + returnCode,
						"SnetDevice.PulseExGetIoOutputPortAll");
				}
				else
				{
					port0 = getValue0;
					port1 = getValue1;
					port2 = getValue2;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.PulseExGetIoOutputPortAll");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-P Extension B'D의 "OUTP1 ~ OUTP3"중 지정한 port의 출력 데이터 읽기 
		/// (tip 1) "OUTP1 ~ OUTP3"은 제품에 표시된 커넥터 label 입니다. 
		/// (tip 2) "접점 번호"는 "bit번호"와 1:1 대응 됩니다.   
		/// : old version name -->"eSnetGetPIoOutputPort"
		/// </summary>
		/// <param name="port"> 	: 출력 포트 번호,"0(OUTP1)" ~ "2(OUTP3)"	</param>
		/// <param name="points">	: 현재 출력 상태값							</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")			</returns>
		public int PulseExGetIoOutputPort(int port, ref int points)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetPulseExGetIoOutputPort(NetID, port, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetPulseExGetIoOutputPort'. error : " + returnCode,
						"SnetDevice.PulseExGetIoOutputPort");
				}
				else
				{
					points = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.PulseExGetIoOutputPort");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-P Extension B'D의 "OUTP1 ~ OUTP3"출력 접점 중 특정 port의 1접점 출력 상태 읽기 
		/// (tip 1) "OUTP1 ~ OUTP3"은 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetGetPIoOutputPoint"
		/// </summary>
		/// <param name="port"> 	: 출력 포트 번호,"0(OUTP1)" ~ "2(OUTP3)"		</param>
		/// <param name="point">	: 출력 접점 번호 							
		///								port=0,port=1 -> "0(bit0)~7(bit7)"
		///								port=2		  -> "0(bit0)~3(bit3)"			</param>
		/// <param name="on_off">	: 현재 출력 상태값, "true"->on, "false"->off	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")				</returns>
		public int PulseExGetIoOutputPoint(int port, int point, ref bool on_off)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetPulseExGetIoOutputPoint(NetID, port, point, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetPulseExGetIoOutputPoint'. error : " + returnCode,
						"SnetDevice.PulseExGetIoOutputPoint");
				}
				else
				{
					on_off = ((getValue == 1) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.PulseExGetIoOutputPoint");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-P Extension B'D "OUTP1 ~ OUTP3"출력 접점 중 "1 port 단위(8접점 or 4접점)"출력 하기 
		/// (tip 1) "OUTP1 ~ OUTP3"은 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetSetPIoOutputPort"
		/// </summary>
		/// <param name="port"> 	: 출력 포트 번호,"0(OUTP1)" ~ "2(OUTP3)"	</param>
		/// <param name="points">	: 출력 값 									
		///								port=0,port=1-> 0x0 ~ 0xff(8접점:8bit) 
		///								port=2		 -> 0x0 ~ 0x04(4접점:4bit)
		///								"1"->출력 on, "0"->출력 off				</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")			</returns>
		public int PulseExSetIoOutputPort(int port, int points)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetPulseExSetIoOutputPort(NetID, port, points);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetPulseExSetIoOutputPort'. error : " + returnCode,
						"SnetDevice.PulseExSetIoOutputPort");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.PulseExSetIoOutputPort");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-P Extension B'D의 "OUTP1 ~ OUTP3"출력 접점 중 지정 port에서 1접점 단위 출력 하기 
		/// (tip 1) "OUTP1 ~ OUTP3"은 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetSetPIoOutputPoint"
		/// </summary>
		/// <param name="port"> 	: 출력 포트 번호,"0(OUTP1)" ~ "2(OUTP3)"	</param>
		/// <param name="point">	: 출력 접점 번호 							</param>						
		///								port=0,port=1 -> "0(bit0) ~ 7(bit7)"
		///								port=2		  -> "0(bit0) ~ 3(bit3)"		
		/// <param name="on_off">	: 출력값, "true"->on, "false"->off			</param>						
		/// <returns>				: (see enum "eSnetApiReturnCode")			</returns>
		public int PulseExSetIoOutputPoint(int port, int point, bool on_off)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (on_off ? 1 : 0);

				returnCode = eSnetPulseExSetIoOutputPoint(NetID, port, point, setValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetPulseExSetIoOutputPoint'. error : " + returnCode,
						"SnetDevice.PulseExSetIoOutputPoint");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.PulseExSetIoOutputPoint");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region User I/O (SNET-RTEX)

		/*** RTEX I/O ***/

		/// <summary>
		/// SNET-RTEX "INP1~INP6 입력 port데이터" 한번에 읽기 
		/// (tip 1) "INP1 ~ INP6"은 제품에 표시된 커넥터 label 정보 입니다. 
		/// : old version name -->"eSnetGetRtexIoInput"
		/// </summary>
		/// <param name="ports">	: [array] 배열크기 ->"6"				
		///								[0]:"INP1" 8접점 입력 data  
		///								[1]:"INP2" 8접점 입력 data  
		///								[2]:"INP3" 8접점 입력 data  
		///								[3]:"INP4" 8접점 입력 data  
		///								[4]:"INP5" 8접점 입력 data  
		///								[5]:"INP6" 8접점 입력 data		</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		public int RtexGetIoInput(ref int[] ports)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				if (ports.Length < 6)
					return (int)eSnetApiReturnCode.InvalidArgument;

				returnCode = eSnetRtexGetIoInput(NetID, out ports[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetIoInput'. error : " + returnCode,
						"SnetDevice.RtexGetIoInput");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetIoInput");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-RTEX "INP1~INP6 입력 데이터" 한번에 읽기 
		/// (tip 1) "INP1 ~ INP6"은 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetGetRtexIoInput2"
		/// </summary>
		/// <param name="port0">	: "INP1" 8접점 입력 data				</param>
		/// <param name="port1">	: "INP2" 8접점 입력 data				</param>
		/// <param name="port2">	: "INP3" 8접점 입력 data				</param>
		/// <param name="port3">	: "INP4" 8접점 입력 data				</param>
		/// <param name="port4">	: "INP5" 8접점 입력 data				</param>
		/// <param name="port5">	: "INP6" 8접점 입력 data				</param>	
		/// <returns>					(see enum "eSnetApiReturnCode")		</returns>	
		public int RtexGetIoInputPortAll(ref int port0, ref int port1, ref int port2, ref int port3, ref int port4, ref int port5)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexGetIoInputPortAll(NetID, out int getValue0, out int getValue1,
					out int getValue2, out int getValue3, out int getValue4, out int getValue5);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetIoInputPortAll'. error : " + returnCode,
						"SnetDevice.RtexGetIoInputPortAll");
				}
				else
				{
					port0 = getValue0; port1 = getValue1;
					port2 = getValue2; port3 = getValue3;
					port4 = getValue4; port5 = getValue5;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetIoInputPortAll");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-RTEX "INP1~INP6" 입력 중 1개의 port에서 데이터 읽기 
		/// (tip 1)"INP1 ~ INP6"은 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetGetRtexIoInputPort"
		/// </summary>
		/// <param name="port">		: 입력 port 번호, "0"->INP1 ~ "5" ->INP6	</param>
		/// <param name="data">		: 입력 data(8 접점)							</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")			</returns>	
		public int RtexGetIoInputPort(int port, ref int data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexGetIoInputPort(NetID, port, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetIoInputPort'. error : " + returnCode,
						"SnetDevice.RtexGetIoInputPort");
				}
				else
				{
					data = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetIoInputPort");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-RTEX "INP1~INP6" 입력 중 1개의 port에서 1접점(point)읽기 
		/// (tip 1)"INP1 ~ INP6"은 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetGetRtexIoInputPoint"
		/// </summary>
		/// <param name="port">		: 입력 port 번호, "0"->INP1 ~ "5" ->INP6	</param>
		/// <param name="point">	: 입력 접점 번호, "0 ~ 7"					</param> 
		/// <param name="on_off">	: 입력 상태, "true"->on, "false"->off 		</param> 
		/// <returns>				: (see enum "eSnetApiReturnCode")			</returns>
		public int RtexGetIoInputPoint(int port, int point, ref bool on_off)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexGetIoInputPoint(NetID, port, point, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetIoInputPoint'. error : " + returnCode,
						"SnetDevice.RtexGetIoInputPoint");
				}
				else
				{
					on_off = ((getValue == 1) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetIoInputPoint");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-RTEX "OUTP1 ~ OUTP5" 출력 data 한꺼번에 읽기 
		/// (tip 1)"OUTP1 ~ OUTP5"는 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetGetRtexIoOutput"
		/// </summary>
		/// <param name="ports">	: [array] 배열크기 ->"5"
		///								[0]:"OUTP1" 8접점 출력 상태 data
		///								[1]:"OUTP2" 8접점 출력 상태 data
		///								[2]:"OUTP3" 8접점 출력 상태 data
		///								[3]:"OUTP4" 8접점 출력 상태 data
		///								[4]:"OUTP5" 4접점 출력 상태 data	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>	
		public int RtexGetIoOutput(ref int[] ports)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				if (ports.Length < 5)
					return (int)eSnetApiReturnCode.InvalidArgument;

				returnCode = eSnetRtexGetIoOutput(NetID, out ports[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetIoOutput'. error : " + returnCode,
						"SnetDevice.RtexGetIoOutput");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetIoOutput");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-RTEX "OUTP1 ~ OUTP5" 출력 data 한꺼번에 읽기 
		/// (tip 1)"OUTP1 ~ OUTP5"는 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetGetRtexIoOutput2"
		/// </summary>
		/// <param name="port0">	: "OUTP1" 8접점 출력 상태 data		</param>
		/// <param name="port1">	: "OUTP2" 8접점 출력 상태 data		</param>
		/// <param name="port2">	: "OUTP3" 8접점 출력 상태 data		</param>
		/// <param name="port3">	: "OUTP4" 8접점 출력 상태 data		</param>
		/// <param name="port4">	: "OUTP5" 4접점 출력 상태 data		</param>
		/// <returns>				(see enum "eSnetApiReturnCode")		</returns>
		public int RtexGetIoOutputPortAll(ref int port0, ref int port1, ref int port2, ref int port3, ref int port4)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexGetIoOutputPortAll(NetID, out int getValue0, out int getValue1,
					out int getValue2, out int getValue3, out int getValue4);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetIoOutputPortAll'. error : " + returnCode,
						"SnetDevice.RtexGetIoOutputPortAll");
				}
				else
				{
					port0 = getValue0; port1 = getValue1;
					port2 = getValue2; port3 = getValue3;
					port4 = getValue4;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetIoOutputPortAll");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-RTEX "OUTP1 ~ OUTP5"출력 중 1개 port의 출력 data 읽기 
		/// (tip 1)"OUTP1 ~ OUTP5"는 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetGetRtexIoOutputPort"
		/// </summary>
		/// <param name="port">	: 출력 port 번호, "0"->OUTP1 ~ "4" ->OUTP5	</param>
		/// <param name="data">	: 출력 상태 data							</param> 
		/// <returns>			: (see enum "eSnetApiReturnCode")			</returns>	
		public int RtexGetIoOutputPort(int port, ref int data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexGetIoOutputPort(NetID, port, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetIoOutputPort'. error : " + returnCode,
						"SnetDevice.RtexGetIoOutputPort");
				}
				else
				{
					data = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetIoOutputPort");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-RTEX "OUTP1 ~ OUTP5"출력 중 지정 port에서 1접점 출력 상태 데이터 읽기  
		/// (tip 1)"OUTP1 ~ OUTP5"는 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetGetRtexIoOutputPoint"
		/// </summary>
		/// <param name="port">		: 출력 port 번호, "0"->OUTP1 ~ "4" ->OUTP5	</param>
		/// <param name="point">	: 접점 번호 									
		///								"port=0 ~ port=3" : " 0 ~ 7"(8접점)
		///								"port=4	: " 0 ~ 3"(4접점)				</param> 
		///<param name="on_off">	: 출력 상태,"true"->on, "false"->off		</param> 
		/// <returns>				: (see enum "eSnetApiReturnCode")			</returns>	
		public int RtexGetIoOutputPoint(int port, int point, ref bool on_off)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexGetIoOutputPoint(NetID, port, point, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetIoOutputPoint'. error : " + returnCode,
						"SnetDevice.RtexGetIoOutputPoint");
				}
				else
				{
					on_off = ((getValue == 1) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetIoOutputPoint");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-RTEX "OUTP1 ~ OUTP5"출력 중 지정된 port의 모든 접점 출력 하기 
		/// (tip 1)"OUTP1 ~ OUTP5"는 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetSetRtexIoOutputPort"
		/// </summary>
		/// <param name="port">		: 출력 port 번호, "0"->OUTP1 ~ "4" ->OUTP5				</param>
		/// <param name="points">	: 출력 data,"1"->접점 출력 on, "0"-> 접점 출력 off 
		///								접점 번호는 bit번호와 대응 됩니다.		
		///								OUTP1 ~ OUTP4 : 0x00 ~ 0xff / OUTP5: 0X00 ~ 0X0F	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")						</returns>	
		public int RtexSetIoOutputPort(int port, int points)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexSetIoOutputPort(NetID, port, points);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexSetIoOutputPort'. error : " + returnCode,
						"SnetDevice.RtexSetIoOutputPort");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexSetIoOutputPort");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-RTEX "OUTP1 ~ OUTP5"출력 중 지정된 port의 1접점 출력 하기 
		/// (tip 1)"OUTP1 ~ OUTP5"는 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetSetRtexIoOutputPoint"
		/// </summary>
		/// <param name="port">		: 출력 port 번호, "0"->OUTP1 ~ "4" ->OUTP5	</param>
		/// <param name="point">	: 출력 접점 번호									
		///								"port=0 ~ port=3": "0 ~ 7"(8접점)
		///								"port=4 : "0 ~ 3"(4접점)				</param>
		/// <param name="on_off">	: "true"->on, "false"->off					</param>									
		/// <returns>				: (see enum "eSnetApiReturnCode")			</returns>
		public int RtexSetIoOutputPoint(int port, int point, bool on_off)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (on_off ? 1 : 0);
				returnCode = eSnetRtexSetIoOutputPoint(NetID, port, point, setValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexSetIoOutputPoint'. error : " + returnCode,
						"SnetDevice.RtexSetIoOutputPoint");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexSetIoOutputPoint");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Remote Module I/O

		/// <summary>
		/// Remote IO Module 사용 유무 설정
		/// </summary>
		/// <remarks>
		/// (tip 1)본 설정값은 제어기 전원 재 인가시 사라집니다(휘발성),영구적으로 적용하기 위해서는 
		///        "HMI -> i/o -> config_remote"창에서 사용 여부를 변경 하고 제어기에 저장하여 사용 하십시요 
		/// </remarks>					
		/// <param name="module">	: remote io module 번호 ("0"~ "15")					</param>						
		/// <param name="type">		: "16"->remote io module과 연결, "0"->사용하지 않음	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")					</returns>		
		public int SetRemoteIoConfig(int module, int type)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetRemoteIoConfig(NetID, module, type);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetRemoteIoConfig'. error : " + returnCode,
						"SnetDevice.SetRemoteIoConfig");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetRemoteIoConfig");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// Remote IO Module 사용 유무 설정값 읽기
		/// </summary>				
		/// <param name="module">		: remote io module 번호 ("0"~ "15")					</param>						
		/// <param name="type">			: "16"->remote io module, "0"->연결되어 있지 않음 	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")					</returns>		
		public int GetRemoteIoConfig(int module, ref int type)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetRemoteIoConfig(NetID, module, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetRemoteIoConfig'. error : " + returnCode,
						"SnetDevice.GetRemoteIoConfig");
				}
				else
				{
					type = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetRemoteIoConfig");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Remote IO Module 통신 상태 읽기
		/// </summary>					
		/// <param name="module">		: remote io module 번호 ("0"~ "15")			</param>						
		/// <param name="status">		: [array] module status 정보, 
		///									[0] : io type : "16"->사용, "0"->사용하지 않음, 
		///									[1] : status : "0"-->ok,
		///									[2] : communication ok count,  
		///									[3] : communication fail count,  
		///									[4] : spare,							</param>	
		/// <returns>					: (see enum "eSnetApiReturnCode")			</returns>	
		public int GetRemoteIoStatus(int module, ref int[] status)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				if (status.Length < 5)              // status length는 5개임
					return (int)eSnetApiReturnCode.InvalidArgument;

				returnCode = eSnetGetRemoteIoStatus(NetID, module, out status[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetRemoteIoStatus'. error : " + returnCode,
						"SnetDevice.GetRemoteIoStatus");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetRemoteIoStatus");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Remote IO 모듈별 Input/Output 32bit 접점 데이터 읽기  
		/// : old version name -->"eSnetGetRemoteIoPort2"
		/// </summary>
		/// <remarks>
		/// (tip 1)입/출력 접점은 bit 번호와 1:1로 매칭 됩니다.  (예) 첫번째 접점: bit0 ~ 서른두번째 접점(bit31) 
		/// </remarks>			
		/// <param name="module">	: remote io module 번호 ("0"~ "15")	</param>						
		/// <param name="port_in">	: 32입력 접점 데이터				</param>
		/// <param name="port_out">	: 32출력 접점 데이터				</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int GetRemoteIoPortInOut(int module, ref int port_in, ref int port_out)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				if (module > 16)
					return (int)eSnetApiReturnCode.InvalidArgument;

				returnCode = eSnetGetRemoteIoPortInOut(NetID, module, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetRemoteIoPortInOut'. error : " + returnCode,
						"SnetDevice.GetRemoteIoPortInOut");
				}
				else
				{
					port_in = getValue0;
					port_out = getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetRemoteIoPortInOut");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Remote IO 모듈별 입력 접점 읽기 
		/// </summary>
		/// <param name="module">	: remote io module 번호 ("0"~ "15")	</param>
		/// <param name="point">	: 입력 접점 bit번호 ("0"~ "31")		</param>
		/// <param name="on_off">	: "true"->on, "false"->off			</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int GetRemoteIoInputPoint(int module, int point, ref bool on_off)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				if (module > 16)
					return (int)eSnetApiReturnCode.InvalidArgument;

				returnCode = eSnetGetRemoteIoInputPoint(NetID, module, point, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetRemoteIoInputPoint'. error : " + returnCode,
						"SnetDevice.GetRemoteIoInputPoint");
				}
				else
				{
					on_off = ((getValue == 1) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetRemoteIoInputPoint");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Remote IO 모듈별 출력(1접점 단위) 접점 상태 읽기 
		/// </summary>
		/// <param name="module">	: remote io module 번호 ("0"~"15")	</param>						
		/// <param name="point">	: bit number ("0"~"31") 			</param>
		/// <param name="on_off">	: "true"->on, "false"->off			</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int GetRemoteIoOutputPoint(int module, int point, ref bool on_off)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				if (module > 16)
					return (int)eSnetApiReturnCode.InvalidArgument;

				returnCode = eSnetGetRemoteIoOutputPoint(NetID, module, point, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetRemoteIoOutputPoint'. error : " + returnCode,
						"SnetDevice.GetRemoteIoOutputPoint");
				}
				else
				{
					on_off = ((getValue == 1) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetRemoteIoOutputPoint");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Remote IO 모듈 출력 접점(32접점 단위)쓰기 
		/// </summary>
		/// <param name="module">	: remote io module 번호 ("0"~"15")									</param>						
		/// <param name="points">	: 32bit 출력 data												
		///								(예) "data = 0x03" (첫번째,두번째 출력 접점 On 나머지 접점 Off)	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")									</returns>

		public int SetRemoteIoOutputPort(int module, int points)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				if (module > 16)
					return (int)eSnetApiReturnCode.InvalidArgument;

				returnCode = eSnetSetRemoteIoOutputPort(NetID, module, points);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetRemoteIoOutputPort'. error : " + returnCode,
						"SnetDevice.SetRemoteIoOutputPort");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetRemoteIoOutputPort");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Remote IO 모듈 출력 접점(1접점 단위)쓰기 
		/// </summary>
		/// <param name="module">	: remote io module 번호 ("0"~ "15")	</param>						
		/// <param name="point">	: bit number ("0"~"31") 			</param>
		/// <param name="on_off">	: "true"->on, "false"->off			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int SetRemoteIoOutputPoint(int module, int point, bool on_off)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				if (module > 16)
					return (int)eSnetApiReturnCode.InvalidArgument;

				int setValue = (on_off ? 1 : 0);
				returnCode = eSnetSetRemoteIoOutputPoint(NetID, module, point, setValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetRemoteIoOutputPoint'. error : " + returnCode,
						"SnetDevice.SetRemoteIoOutputPoint");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetRemoteIoOutputPoint");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}


		#endregion

		#region Ouput For Time

		/// <summary>
		/// 설정 시간 동안 출력 On
		/// </summary>
		/// <param name="channel">	: 채널(함수) 번호("0"~"4"),동시 실행 가능 함수 개수 5개(채널0 ~ 채널4)		</param>
		/// <param name="out_type">	: 출력 접점 종류 구분					
		///								# SNET-P
		///									"0"->Axis IO, "1"->Remote IO, "2"-> AD/DA Option Board IO 
		///								# SNET-RTEX
		///									"0"->Rtex Driver IO, "1"->Remote IO, "2"-> Rtex IO(제어기 User IO)  </param>
		/// <param name="out_port">	: # SNET-P
		///									"out_type = 0"일 경우: "Only 0" 
		///									"out_type = 1"일 경우: "0"~"15",Remote IO 모듈 번호  
		///									"out_type = 2"일 경우: "0(OUTP1)"~"2(OUTP3)",출력 컨넥터 번호  
		///							  # SNET-RTEX
		///									"out_type = 0"일 경우: "0"~"31",Rtex 서보 드라이버 id 
		///									"out_type = 1"일 경우: "0"~"15",Remote IO 모듈 번호  
		///									"out_type = 2"일 경우: "0(OUTP1)"~"4(OUTP5)",출력 컨넥터 번호		</param>
		/// <param name="out_time">	: [msec],출력 On 유지 시간 													</param>
		/// <param name="out_data">	: 출력 접점 번호,접점 설정값이"1"이면 사용,"0"이면 don't care
		///								(예)"out_type=1,out_port=0" 이고 "out_data= 0x05" 일경우 
		///									-> "remote 모듈 0"의 "첫번째 출력 접점(bit0)과 세번째 출력 접점(bit2)"을 
		///							  		"eSnetSetOutputForTime(...,채널,..)"함수에 사용						</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")					
		///								CommunicationEventCode,
		///								SetOutputForTime (1601) ~
		///								SetOutputForRun  (1604)													</returns>
		public int SetOutputForTime(int channel, int out_type, int out_port, int out_time, int out_data)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetOutputForTime(NetID, channel, out_type, out_port, out_time, out_data);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetOutputForTime'. error : " + returnCode,
						"SnetDevice.SetOutputForTime");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetOutputForTime");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "eSnetSetOutputForTime" 채널별 실행 상태 확인 
		/// </summary>
		/// <param name="channel">		: 채널(함수) 번호("0"~"4")								</param>
		/// <param name="runstatus">	: "1"->출력 on상태,"0"->출력 off상태(출력 종료 상태)	</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")					
		///								CommunicationEventCode,
		///								SetOutputForTime (1601) ~
		///								SetOutputForRun  (1604)													</returns>
		public int GetOutputForTimeRun(int channel, ref int run_status)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetOutputForTimeRun(NetID, channel, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetOutputForTimeRun'. error : " + returnCode,
						"SnetDevice.GetOutputForTimeRun");
				}
				else
				{
					run_status = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetOutputForTimeRun");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "eSnetSetOutputForTime" 채널별 실행을 초기화(종료) 합니다.
		/// </summary>
		/// <param name="channel">	: 채널(함수) 번호("0"~"4")			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int ResetOutputForTime(int channel)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetResetOutputForTime(NetID, channel);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetResetOutputForTime'. error : " + returnCode,
						"SnetDevice.ResetOutputForTime");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.ResetOutputForTime");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        #endregion

        #region Output At Position

        /// <summary>
        /// 설정 좌표(Actual Position)에서 접점 On/Off 기능 실행  
        /// 최대 10개의 내부 Ring 버퍼를 운영 합니다. 
        /// </summary>
        /// <param name="net"> 			: controller id 							</param>
        /// <param name="axis">			: Actual 좌표를 읽을 축 번호 					</param>
        /// <param name="on_posi">		: 출력 On 좌표, "mode=2"인 경우 미 사용 		</param>
        /// <param name="off_posi">		: 출력 Off 좌표,"mode=1"인 경우 미 사용 		</param>
        /// <param name="mode">			: "0"-> 접점 출력 ON, 출력 OFF 실행  	
        ///								  "1"-> 접점 출력 ON 실행 
        ///								  "2"-> 접점 출력 OFF 실행 					</param>
        /// <param name="out_count">	: 출력 접점수 (1 step당), 최대 4 접점 동시 출력 가능 	</param>
        /// <param name="output_type">	: [array] 배열 크기->out_count, 
        ///								  "0"->axis_io, "1"->remote_io, 
        ///								  "2"->rtex_user_io or Ad option board io	</param>
        /// <param name="port">			: [array] 배열 크기->output_count, port		</param>					
        /// <param name="point">		: [array] 배열크기->output_count, point		</param>						
        /// <returns>					: (see enum "eSnetApiReturnCode")				</returns>			
        public int SetOutAtActualPosition(int axis, int on_posi, int off_posi,
										  int mode, int out_count, int[] output_type, int[] port, int[] point)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                if (output_type.Length < out_count)
                    return (int)eSnetApiReturnCode.InvalidArgument;
                if (port.Length < out_count)
                    return (int)eSnetApiReturnCode.InvalidArgument;
                if (point.Length < out_count)
                    return (int)eSnetApiReturnCode.InvalidArgument;

                if (out_count > 0)
                {
					returnCode = eSnetSetOutAtActualPosition(NetID, axis, on_posi,off_posi, mode,out_count,
															out output_type[0], out port[0], out point[0]);
                    if (returnCode != (int)eSnetApiReturnCode.Success)
                    {
                        Debug.WriteLine("Failed to call 'eSnetSetOutAtActualPosition'. error : " + returnCode,
										"SnetDevice.SetOutAtActualPosition");
                    }
                }
                else
                {
                    return (int)eSnetApiReturnCode.InvalidArgument;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.SetOutAtActualPosition");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// "eSnetSetOutAtActualPosition" 동작 상태 확인 
        ///	(tip 1) Ring 버퍼 저장 내용 중 "tail index"번호에 저장된 설정값 입니다. 	
        /// </summary>
        /// <param name="enable">		: "1"-> 기능 실행 상태  	
        ///								  "0"-> 기능 정지 상태					</param>        
        /// <param name="head">			: Ring 버퍼 "선두 index 번호"				</param>   		
        /// <param name="tail">			: Ring 버퍼 "실행 index 번호"				</param>   		
        /// <param name="axis">			: Actual 좌표를 읽을 축 번호 				</param>
        /// <param name="on_posi">		: 출력 On 좌표, "mode=2"인 경우 미 사용 		</param>
        /// <param name="off_posi">		: 출력 Off 좌표,"mode=1"인 경우 미 사용 		</param>
        /// <param name="mode">			: "0"-> 접점 출력 ON, 출력 OFF 실행  	
        ///								  "1"-> 접점 출력 ON 실행 
        ///								  "2"-> 접점 출력 OFF 실행 					</param>
        /// <param name="out_count">	: 출력 접점수 (1 step당), 최대 4 접점 동시 출력 가능 	</param>
        /// <param name="output_type">	: [array] 배열 크기->"4"		</param>
        /// <param name="port">			: [array] 배열 크기->"4"		</param>
        /// <param name="point">		: [array] 배열 크기->"4"		</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")				</returns>	

        public int GetOutAtActualPosition(ref int enable, ref int head, ref int tail, ref int axis, ref int on_posi, ref int off_posi, ref int mode,
                                         ref int out_count, int[] output_type, int[] port, int[] point)
		{
            int returnCode = (int)eSnetApiReturnCode.Success;

            int enable_temp = 0;
            int head_temp = 0;
            int tail_temp = 0;
            int axis_temp = 0;
            int on_posi_temp = 0;
            int off_posi_temp = 0;
            int mode_temp = 0;
            
            int out_count_temp = 0;

            try
            {
                if (output_type.Length < 4)
                    return (int)eSnetApiReturnCode.InvalidArgument;
                if (port.Length < 4)
                    return (int)eSnetApiReturnCode.InvalidArgument;
                if (point.Length < 4)
                    return (int)eSnetApiReturnCode.InvalidArgument;

				returnCode = eSnetGetOutAtActualPosition(NetID, out enable_temp, out head_temp, out tail_temp, out axis_temp, out on_posi_temp, out off_posi_temp, out mode_temp,
                                                         out out_count_temp, out output_type[0], out port[0], out point[0]);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetGetOutAtActualPosition'. error : " + returnCode,
									"SnetDevice.GetOutAtActualPosition");
                }
                else
                {
                    enable = enable_temp;
					head = head_temp;
					tail = tail_temp;
                    axis = axis_temp;
					on_posi = on_posi_temp;
					off_posi = off_posi_temp;
					mode = mode_temp;
					out_count = out_count_temp;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.GetOutAtActualPosition");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// 설정 좌표 에서 접점 출력 하기 기능 초기화 
        /// </summary>
        /// <param name="net"> 			: controller id 						</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")		</returns>			
        public int ResetOutAtActualPosition()
		{
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetResetOutAtActualPosition(NetID);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetResetOutAtActualPosition'. error : " + returnCode,
                        "SnetDevice.EnableOutAtActualPosition");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.ResetOutAtActualPosition");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }
        #endregion

        #region Latch Input

        /// <summary>
        /// Latch 입력 설정 ( "eSnetGetLatchInput"을 실행 할때 까지 입력 상태 유지)
        /// </summary>
        /// <param name="channel">	: 채널(함수) 번호("0"~"4"),동시 실행 가능 함수 개수 5개(채널0~채널4)	</param>
        /// <param name="type">		: 입력 포트의 종류: 
        ///								0->axis_io, 1->remote io, 
        ///								2->Rtex io(SNET-RTEX),SNET-P (User Input)							</param>
        /// <param name="port">		: 입력 포트 번호("in_type" 따라 설정 범위 변경)							</param>
        /// <param name="point">	: 입력 접점 bit 번호 ("port" 따라 설정 범위 변경)						</param>
        /// <param name="edge">		: 입력 극성 : 1 ->A접점 입력, 0-> B접점 입력							</param>
        ///								(예) "channel=0","in_type=1","port=0","point=0x05","edge=1" 일경우 
        ///										-> Remote io 모듈 0의 첫번째 입력 접점과 세번째 입력 접점을 Latch 입력으로 사용 
        /// <returns>				: (see enum "eSnetApiReturnCode")									
        ///								CommunicationEventCode,
        ///								LatchInputChannelIndexFault (2401) ~
        ///								LatchInputChannelPointFault (2404),									</returns>		
        public int SetLatchInput(int channel, int type, int port, int point, bool edge)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setEdge = edge ? 1 : 0;
				returnCode = eSnetSetLatchInput(NetID, channel, type, port, point, setEdge);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetLatchInput'. error : " + returnCode,
						"SnetDevice.SetLatchInput");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetLatchInput");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Latch 입력 설정 초기화(Latch 입력 설정 해제)
		/// </summary>
		/// <param name="channel">	: 채널(함수) 번호("0"~"4")					</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")									
		///								CommunicationEventCode,
		///								LatchInputChannelIndexFault (2401) ~
		///								LatchInputChannelPointFault (2404),		</returns>		
		public int ResetLatchInput(int channel)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetResetLatchInput(NetID, channel);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetResetLatchInput'. error : " + returnCode,
						"SnetDevice.ResetLatchInput");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.ResetLatchInput");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Latch 입력 접점 Data 읽기 
		/// (tip 1) 본 함수를 실행 하면 "Latch 입력 데이터가 초기화"되고 새로운 실시간 입력값으로 업데이트 됩니다. 
		/// </summary>
		/// <param name="channel">	: 채널(함수) 번호("0"~"4")										</param>
		/// <param name="on_off">	: Latch 접점 별 입력 data,"true"->Input On,"false"->Input Off	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")									
		///								CommunicationEventCode,
		///								LatchInputChannelIndexFault (2401) ~
		///								LatchInputChannelPointFault (2404),							</returns>	
		public int GetLatchInput(int channel, ref bool on_off)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetLatchInput(NetID, channel, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetLatchInput'. error : " + returnCode,
						"SnetDevice.GetLatchInput");
				}
				else
				{
					on_off = ((getValue == 1) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetLatchInput");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Latch 입력 설정 상태(eSnetSetLatchInput) 확인 
		/// </summary>
		/// <param name="channel">	: 채널(함수) 번호("0"~"4")										</param>
		/// <param name="in_type">	: 입력 포트의 종류: 0->axis_io, 1->remote io, 2-> Rtex io		</param>
		/// <param name="port">		: 입력 포트 번호("in_type" 따라 설정 범위 변경)					</param>
		/// <param name="point">	: 입력 접점 bit 번호 ("port" 따라 설정 범위 변경)				</param>
		/// <param name="edge">		: 입력 극성 : 1 ->A접점 입력, 0-> B접점 입력					</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")									
		///								CommunicationEventCode,
		///								LatchInputChannelIndexFault (2401) ~
		///								LatchInputChannelPointFault (2404),							</returns>		
		public int GetLatchInConfig(int channel, ref int in_type, ref int port, ref int point, ref bool edge)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetLatchInConfig(NetID, channel, out int getValue0, out int getValue1, out int getValue2, out int getValue3);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetLatchInConfig'. error : " + returnCode,
						"SnetDevice.GetLatchInConfig");
				}
				else
				{
					in_type = getValue0;
					port = getValue1;
					point = getValue2;
					edge = (getValue3 == 1);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetLatchInConfig");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Flag I/O

		/*** Flag IO ***/

		/// <summary>
		/// Flag IO 값 읽기
		/// : old version name ->"eSnetGetFlagIoPort"
		/// </summary>
		/// <param name="ports">	: array 배열 : 모든 Port 값			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int GetFlagIo(ref int[] ports)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				if (ports.Length < 10)
					return (int)eSnetApiReturnCode.InvalidArgument;

				returnCode = eSnetGetFlagIo(NetID, out ports[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetFlagIo'. error : " + returnCode,
						"SnetDevice.GetFlagIo");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetFlagIo");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Flag IO 모든 Port값 읽기
		/// : old version name -->"eSnetGetFlagIoPort2"
		/// </summary>
		/// <param name="port0">	: 첫번째	Port 값					</param>
		/// <param name="port1">	: 두번째	Port 값					</param>
		/// <param name="port2">	: 세번째	Port 값					</param>
		/// <param name="port3">	: 네번째	Port 값					</param>
		/// <param name="port4">	: 다섯번째	Port 값					</param>
		/// <param name="port5">	: 여섯번째	Port 값					</param>
		/// <param name="port6">	: 일곱번째	Port 값					</param>
		/// <param name="port7">	: 여덟번째	Port 값					</param>
		/// <param name="port8">	: 아홉번째	Port 값					</param>
		/// <param name="port9">	: 열번째	Port 값					</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int GetFlagIoPortAll(ref int port0, ref int port1, ref int port2, ref int port3,
			ref int port4, ref int port5, ref int port6, ref int port7, ref int port8, ref int port9)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetFlagIoPortAll(NetID, out int getValue0, out int getValue1, out int getValue2, out int getValue3,
					out int getValue4, out int getValue5, out int getValue6, out int getValue7, out int getValue8, out int getValue9);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetFlagIoPortAll'. error : " + returnCode,
						"SnetDevice.GetFlagIoPortAll");
				}
				else
				{
					port0 = getValue0; port1 = getValue1;
					port2 = getValue2; port3 = getValue3;
					port4 = getValue4; port5 = getValue5;
					port6 = getValue6; port7 = getValue7;
					port8 = getValue8; port9 = getValue9;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetFlagIoPortAll");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Flag IO의 한 Point(bit) 값 쓰기
		/// </summary>
		/// <param name="port">		: 원하는 point가 있는 port			</param>
		/// <param name="point">	: 원하는 point				
		/// <param name="on_off">	: point(bit) 값						</param>
		///								"0"->off, "1"->on				</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int SetFlagIoPoint(int port, int point, bool on_off)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = on_off ? 1 : 0;
				returnCode = eSnetSetFlagIoPoint(NetID, port, point, setValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetFlagIoPoint'. error : " + returnCode,
						"SnetDevice.SetFlagIoPoint");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetFlagIoPoint");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// point(bit) 들의 논리 연산을 통해 Flag Io의 마지막 Port의 Point(bit) 들을 활성화 시키는 부가 기능 설정 하기
		/// </summary>
		/// <param name="point_f9">	: 논리 연산 수행 후 결과를 표시할 Flag IO 의 point(bit) ("0"~"31")	</param>
		/// <param name="op_code">	: 논리 연산,
		///								"0"->unused, "1"->and, "2"->or, "3"->not,
		///								"4"->nand, "5"->nor, "6"->xor									</param>
		/// <param name="port_a">	: 첫번째 항 Port, 
		///								axis input	-> 0  ~ 3  (4  port),
		///								axis output	-> 4  ~ 5  (2  port),
		///								flag io		-> 6  ~ 15 (10 port),
		///								remote io	-> 16 ~ 31 (16 port), 
		///								rtex in		-> 32 ~ 33 (2  port),
		///								rtex out	-> 34 ~ 35 (2  port),								</param>
		/// <param name="point_a">	: 첫번째 항 Point(bit) ("0"~"31")									</param>
		/// <param name="port_b">	: 두번째 항 Port,
		/// 							axis input	-> 0  ~ 3  (4  port),
		///								axis output	-> 4  ~ 5  (2  port),
		///								flag io		-> 6  ~ 15 (10 port),
		///								remote io	-> 16 ~ 31 (16 port), 
		///								rtex in		-> 32 ~ 33 (2  port),
		///								rtex out	-> 34 ~ 35 (2  port),								</param>
		/// <param name="point_b">	: 두번째 항의 Point(bit) ("0"~"31")									</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")									</returns>
		public int SetFlagIoLogicOperation(int point_f9, int op_code, int port_a, int point_a, int port_b, int point_b)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetFlagIoLogicOperation(NetID, point_f9, op_code, port_a, point_a, port_b, point_b);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetFlagIoLogicOperation'. error : " + returnCode,
						"SnetDevice.SetFlagIoLogicOperation");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetFlagIoLogicOperation");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// point(bit) 들의 논리 연산을 통해 Flag Io의 마지막 Port의 Point(bit) 들을 활성화 시키는 부가 기능 설정값 읽기
		/// </summary>
		/// <param name="point_f9">	: 논리 연산 수행 후 결과를 표시할 Flag IO 의 point(bit) ("0"~"31")	</param>
		/// <param name="op_code">	: 논리 연산,
		///								"0"->unused, "1"->and, "2"->or, "3"->not,
		///								"4"->nand, "5"->nor, "6"->xor									</param>
		/// <param name="port_a">	: 첫번째 항 Port, 
		///								axis input	-> 0  ~ 3  (4  port),
		///								axis output	-> 4  ~ 5  (2  port),
		///								flag io		-> 6  ~ 15 (10 port),
		///								remote io	-> 16 ~ 31 (16 port), 
		///								rtex in		-> 32 ~ 33 (2  port),
		///								rtex out	-> 34 ~ 35 (2  port),								</param>
		/// <param name="point_a">	: 첫번째 항 Point(bit) ("0"~"31")									</param>
		/// <param name="port_b">	: 두번째 항 Port,
		/// 							axis input	-> 0  ~ 3  (4  port),
		///								axis output	-> 4  ~ 5  (2  port),
		///								flag io		-> 6  ~ 15 (10 port),
		///								remote io	-> 16 ~ 31 (16 port), 
		///								rtex in		-> 32 ~ 33 (2  port),
		///								rtex out	-> 34 ~ 35 (2  port),								</param>
		/// <param name="point_b">	: 두번째 항의 Point(bit) ("0"~"31")									</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")									</returns>
		public int GetFlagIoLogicOperation(int point_f9, ref int op_code, ref int port_a, ref int point_a, ref int port_b, ref int point_b)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetFlagIoLogicOperation(NetID, point_f9, out int getValue0, out int getValue1, out int getValue2, out int getValue3, out int getValue4);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetFlagIoLogicOperation'. error : " + returnCode,
						"SnetDevice.GetFlagIoLogicOperation");
				}
				else
				{
					op_code = getValue0;
					port_a = getValue1;
					point_a = getValue2;
					port_b = getValue3;
					point_b = getValue4;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetFlagIoLogicOperation");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 설정된 Flag IO 논리 연산 기능을 사용하지 않기
		/// </summary>
		/// <returns>			: (see enum "eSnetApiReturnCode")	</returns>
		public int ClearAllFlagIoLogicOperation()
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetClearAllFlagIoLogicOperation(NetID);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetClearAllFlagIoLogicOperation'. error : " + returnCode,
						"SnetDevice.ClearAllFlagIoLogicOperation");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.ClearAllFlagIoLogicOperation");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region ADC / DAC

		/*** ADC/DAC ***/
		/// ADC Channel 갯수 : 4개
		/// DAC Channel 갯수 : 2개

		/// <summary>
		/// ADC Channel 설정 ("0"~"3")
		/// </summary>
		/// <param name="channel">	: ad channel number("0"~"3")			</param>						
		/// <param name="range">	: "0"--> -10  V  ~  +10  V,
		///							  "1"-->  -5  V  ~   +5  V,
		///							  "2"-->  -2.5V  ~   +2.5V,
		///							  "3"-->   0  V  ~  +10  V,
		///							  "4"-->   0  V  ~   +5  V,				</param>	
		/// <param name="enable">	: "true"->사용, "false"->사용하지 않음	</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")		
		///								CommunicationEventCode, 
		///								AdcNumber	(1801),
		///								AdcRange	(1802),					</returns>	
		public int SetAdcConfig(int channel, int range, bool enable)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (enable ? 1 : 0);

				returnCode = eSnetSetAdcConfig(NetID, channel, range, setValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetAdcConfig'. error : " + returnCode,
						"SnetDevice.SetAdcConfig");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetAdcConfig");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// ADC Channel 설정값 읽기
		/// </summary>
		/// <param name="channel">	: ad channel number						</param>						
		/// <param name="range">	: "0"--> -10  V  ~  +10  V,
		///							: "1"-->  -5  V  ~   +5  V,
		///							: "2"-->  -2.5V  ~   +2.5V,
		///							: "3"-->   0  V  ~  +10  V,
		///							: "4"-->   0  V  ~   +5  V,				</param>	
		/// <param name="enable">	: "true"->사용, "false"->사용하지 않음	</param>			
		/// <returns>				(see enum "eSnetApiReturnCode")			</returns>
		public int GetAdcConfig(int channel, ref int range, ref bool enable)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetAdcConfig(NetID, channel, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetAdcConfig'. error : " + returnCode,
						"SnetDevice.GetAdcConfig");
				}
				else
				{
					range = getValue0;
					enable = ((getValue1 == 1) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetAdcConfig");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// ADC Channel 현재 값 읽기
		/// </summary>
		/// <param name="channel">	: ad channel number					</param>						
		/// <param name="volt">		: [mV]	읽은 전압값					</param>
		/// <param name="digit">	: [16bit] 읽은 디지탈값				</param>			
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		public int GetAdcData(int channel, ref int volt, ref int digit)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetAdcData(NetID, channel, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetAdcData'. error : " + returnCode,
						"SnetDevice.GetAdcData");
				}
				else
				{
					volt = getValue0;
					digit = getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetAdcData");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// DAC Channel 설정("0"~"1")
		/// </summary>
		/// <param name="channel">	: da channel number("0"~"1")				</param>
		/// <param name="range">	: "0"-->    0  V  ~   +5  V,
		///							: "1"-->    0  V  ~  +10  V,
		///							: "2"-->    0  V  ~ +10.8 V,
		///							: "3"-->   -5  V  ~   +5  V,
		///							: "4"-->  -10  V  ~  +10  V,
		///							: "5"-->  -10.8V  ~  +10.8V,				</param>	
		/// <param name="enable">	: "true"->사용, "false"->사용하지 않음		</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")		
		///								CommunicationEventCode, 
		///								DacNumber(1701) ~ DacDigialNumber(1721)	</returns>									
		public int SetDacConfig(int channel, int range, bool enable)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (enable ? 1 : 0);

				returnCode = eSnetSetDacConfig(NetID, channel, range, setValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetDacConfig'. error : " + returnCode,
						"SnetDevice.SetDacConfig");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetDacConfig");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// DAC Channel 설정값 읽기
		/// </summary>
		/// <param name="channel">	: da channel number						</param>
		/// <param name="range">	: "0"-->    0  V  ~   +5  V,
		///							  "1"-->    0  V  ~  +10  V,
		///							  "2"-->    0  V  ~  +10.8V,
		///							  "3"-->   -5  V  ~   +5  V,
		///							  "4"-->  -10  V  ~  +10  V,
		///							  "5"-->  -10.8V  ~  +10.8V,			</param>	
		/// <param name="enable">	: "true"->사용, "false"->사용하지 않음	</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>
		public int GetDacConfig(int channel, ref int range, ref bool enable)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetDacConfig(NetID, channel, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetDacConfig'. error : " + returnCode,
						"SnetDevice.GetDacConfig");
				}
				else
				{
					range = getValue0;
					enable = ((getValue1 == 1) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetDacConfig");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// DAC Channel volt값으로 출력
		/// </summary>
		/// <param name="channel">	: da channel number								</param>
		/// <param name="volt">		: [mV] volt값으로 출력							</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")		
		///								CommunicationEventCode, 
		///								DacNumber(1701) ~ DacDigialNumber (1721),	</returns>
		public int SetDacVolt(int channel, int volt)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetDacVolt(NetID, channel, volt);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetDacVolt'. error : " + returnCode,
						"SnetDevice.SetDacVolt");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetDacVolt");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// DAC Channel digital값으로 출력
		/// </summary>
		/// <param name="channel">	: da channel number								</param>
		/// <param name="digit">	: digitial 값으로 출력							</param>	
		/// <returns>				: (see enum "eSnetApiReturnCode")		
		///								CommunicationEventCode, 
		///								DacNumber(1701) ~ DacDigialNumber (1721),	</returns>
		public int SetDacDigit(int channel, int digit)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetDacDigit(NetID, channel, digit);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetDacDigit'. error : " + returnCode,
						"SnetDevice.SetDacDigit");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetDacDigit");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Trigger Output - High Resolution ( SNET-P / SNET-RTEX / SNET-EMD-L2K )

		/// <summary>
		/// "Trigger (고속 주기 출력)" Encoder를 읽을 축과 output을 내 보낼 축을 설정 하기 
		/// </summary>
		/// <param name="encoder_axis">	: # SNET-P / SNET-EMD-L2K		
		///										-> encoder값을 읽을 축 번호 (구동할 축)
		///								  # SNET-RTEX
		///										-> 인코더 입력 커넥터 선택 
		///										-> "0(ENC1)", "1(ENC2)"				</param>
		/// <param name="output_axis">	: # SNET-P									
		///										-> Trigger 신호 출력 "축 번호"				
		///								  # SNET-RTEX
		///										-> 펄스 출력 커넥터 번호 선택 
		///										-> "0(P1) ~ 5(P6)"					
		///                               # SNET-EMD-L2K 
		///										-> 미사용     						</param>	
		/// <returns>					: (see enum "eSnetApiReturnCode")			</returns>	
		public int SetTriggerPort(int encoder_axis, int output_axis)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetTriggerPort(NetID, encoder_axis, output_axis);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetTriggerPort'. error : " + returnCode,
						"SnetDevice.SetTriggerPort");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetTriggerPort");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Trigger (고속 주기 출력)" Encoder를 읽을 축과 output을 내 보낼 축을 설정값 읽기 
		/// </summary>
		/// <param name="encoder_axis">	: # SNET-P	/ SNET-EMD-L2K	
		///										-> encoder값을 읽을 축 번호 (구동할 축)
		///								  # SNET-RTEX
		///										-> 인코더 입력 커넥터 선택 
		///										-> "0(ENC1)", "1(ENC2)"				</param>
		/// <param name="output_axis">	: # SNET-P									
		///										-> Trigger 신호 출력 "축 번호"				
		///								  # SNET-RTEX
		///										-> 펄스 출력 커넥터 번호 선택 
		///										-> "0(P1) ~ 5(P6)"					</param>	
		///                               # SNET-EMD-L2K 
		///										-> 미사용     						</param>	
		/// <returns>					: (see enum "eSnetApiReturnCode")			</returns>	
		public int GetTriggerPort(ref int encoder_axis, ref int output_axis)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetTriggerPort(NetID, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetTriggerPort'. error : " + returnCode,
						"SnetDevice.GetTriggerPort");
				}
				else
				{
					encoder_axis = getValue0;
					output_axis = getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetTriggerPort");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Trigger (고속 주기 출력)" 관련 파라미터 설정 ( SNET-P / SNET-RTEX )
		/// </summary>
		/// <param name="pulse_count">		: 출력 갯수 												</param>						
		/// <param name="first_position">	: [um] 첫번째 Trigger 신호를 출력할 좌표  					</param>
		/// <param name="pulse_interval">	: [um] Trigger 신호 출력 "거리 Offset"				
		///										"firstPosition"설정 좌표를 기준으로 "pulseInterval" 설정 거리 
		///										마다 Trigger 신호 출력									</param>
		/// <param name="pulse_on_time">	: [msec] 	: 출력 on 유지 시간 							</param>	
		/// <returns>						: (see enum "eSnetApiReturnCode")							</returns>	
		public int SetTriggerParameter(int pulse_count, int first_position, int pulse_interval, int pulse_on_time)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetTriggerParameter(NetID, pulse_count, first_position, pulse_interval, pulse_on_time);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetTriggerParameter'. error : " + returnCode,
						"SnetDevice.SetTriggerParameter");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetTriggerParameter");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Trigger (고속 주기 출력)" 관련 파라미터 설정값 읽기 ( SNET-P / SNET-RTEX)
		/// </summary>
		/// <param name="pulse_count">		: 출력 갯수 												</param>						
		/// <param name="first_position">	: [um] 첫번째 Trigger 신호를 출력할 좌표  					</param>
		/// <param name="pulse_interval">	: [um] Trigger 신호 출력 "거리 Offset"				
		///										"firstPosition"설정 좌표를 기준으로 "pulseInterval" 설정 거리 
		///										마다 Trigger 신호 출력									</param>
		/// <param name="pulse_on_time">	: [msec] 	: 출력 on 유지 시간 							</param>	
		/// <returns>						: (see enum "eSnetApiReturnCode")							</returns>			
		public int GetTriggerParameter(ref int pulse_count, ref int first_position, ref int pulse_interval, ref int pulse_on_time)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetTriggerParameter(NetID, out int getValue0, out int getValue1, out int getValue2, out int getValue3);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetTriggerParameter'. error : " + returnCode,
						"SnetDevice.GetTriggerParameter");
				}
				else
				{
					pulse_count = getValue0;
					first_position = getValue1;
					pulse_interval = getValue2;
					pulse_on_time = getValue3;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetTriggerParameter");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}
		/// <summary>
		/// "Trigger (고속 주기 출력)" 연동 축 번호 설정 ( SNET-RTEX )
		/// (tip 1)지정된 "Rtex 드라이버"의 엔코더 출력 신호를 
		///		   "SetTriggerPort"로 지정된 제어기의 엔코더 입력 단자(ENC1 or ENC2)로 연결 합니다. 
		/// </summary>
		/// <param name="source">	: Rtex축 번호,"0(축0)" ~ "31(축31)"		</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>	
		public int RtexSetTriggerSource(int source)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexSetTriggerSource(NetID, source);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexSetTriggerSource'. error : " + returnCode,
						"SnetDevice.RtexSetTriggerSource");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexSetTriggerSource");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Trigger (고속 주기 출력)" "RtexSetTriggerSource" 사용자 설정값 확인 
		/// </summary>
		/// <param name="source">	: Rtex축 번호,"0(축0)" ~ "31(축31)"		</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>	
		public int RtexGetTriggerSource(ref int source)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexGetTriggerSource(NetID, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetTriggerSource'. error : " + returnCode,
						"SnetDevice.RtexGetTriggerSource");
				}
				else
				{
					source = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetTriggerSource");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Trigger (고속 주기 출력)" Trigger 신호 출력 상태 정보 확인 
		/// </summary>
		/// <param name="run_flag">			: "1"->trigger기능 사용 중, "0"->trigger기능 사용 안함	</param>						
		/// <param name="trigger_count">	: 현재까지 출력된 trigger 신호 갯수						</param>						
		/// <returns>						: (see enum "eSnetApiReturnCode")						</returns>	
		public int GetTriggerStatus(ref bool run_flag, ref int trigger_count)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetTriggerStatus(NetID, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetTriggerStatus'. error : " + returnCode,
						"SnetDevice.GetTriggerStatus");
				}
				else
				{
					run_flag = ((getValue0 == 1) ? true : false);
					trigger_count = getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetTriggerStatus");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Trigger (고속 주기 출력)" Trigger 신호 출력 기능 시작 
		/// (tip 1) StartTrigger() 지령 후 "축 이송 명령"을 사용 하십시요 
		/// : old version name ->"eSnetSetTriggerStart"
		/// </summary>
		/// <param name="enable">	: "1"->trigger기능 사용, "0"->trigger기능 사용 안함	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")					</returns>	
		public int StartTrigger(bool enable)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (enable ? 1 : 0);

				returnCode = eSnetStartTrigger(NetID, setValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetStartTrigger'. error : " + returnCode,
						"SnetDevice.StartTrigger");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.StartTrigger");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Trigger Output - Interpolation

		/// <summary>
		/// 지정한 2축의 벡터 합성 거리를(절대 이동 거리)누적 하여 "설정 거리" 마다 Trigger 신호를 출력 
		/// </summary>
		/// <param name="set_array">: [array] 배열크기 ->"4",		
		///								[0]:첫번째 축 번호, 
		///								[1]:두번째 축 번호,
		///								[2]:Trigger 신호 출력 "거리 Offset" (um),
		///								[3]:"출력 ON" 유지 시간(msec),											</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		
		///								CommunicationEventCode, 
		///								InterpolTriggerNoSetAxis (2601)(축번호 설정 오류),
		///								InterpolTriggerDistance	 (2602)(첫번째 축번호와 두번째 축번호가 같을 경우,
		///								InterpolTriggerTime		 (2603)(거리 설정 오류),
		///								InterpolTriggerTime		 (2604)(출력 ON 유지 시간 설정 오류,			</returns>	
		public int SetInterpolTriggerConfig(int[] set_array)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetInterpolTriggerConfig(NetID, out set_array[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetInterpolTriggerConfig'. error : " + returnCode,
						"SnetDevice.SetInterpolTriggerConfig");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetInterpolTriggerConfig");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// <summary>
		/// "eSnetSetBoganTrigger" 설정값 확인 
		/// </summary>
		/// <param name="get_array">	: [array] 배열크기 ->"4",			</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
		public int GetInterpolTriggerConfig(int[] get_array)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetInterpolTriggerConfig(NetID, out get_array[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetInterpolTriggerConfig'. error : " + returnCode,
						"SnetDevice.GetInterpolTriggerConfig");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetInterpolTriggerConfig");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "보간 이송 거리 기준 Trigger 출력" 기능 시작 하기 
		/// (tip 1) 사용전 "eSnetSetInterpolTriggerConfig()"를 이용하여 적절한 파라미터 설정 후 사용 하십시요 
		/// (tip 2) "보간 이송 벡터 합성 누적 거리가" 초기화 됩니다. 
		/// : old version name ->"eSnetSetInterpolTriggerStart"
		/// </summary>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		public int StartInterpolTrigger()
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetStartInterpolTrigger(NetID);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetStartInterpolTrigger'. error : " + returnCode,
													"SnetDevice.StartInterpolTrigger");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.StartInterpolTrigger");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "보간 이송 거리 기준 Trigger 출력" 기능 중지  
		/// (tip 1) "eSnetSetInterpolTriggerConfig()"를 사용하여 설정한 값은 유지 됩니다. 
		/// (tip 2) "보간 이송 벡터 합성 누적 거리가" 초기화 됩니다. 
		/// : old versin name ->"eSnetSetInterpolStop"
		/// </summary>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		public int StopInterpolTrigger()
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetStopInterpolTrigger(NetID);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetStopInterpolTrigger'. error : " + returnCode,
													"SnetDevice.StopInterpolTrigger");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.StopInterpolTrigger");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "보간 이송 거리 기준 Trigger 출력" 동작 상태 정보 읽기 
		/// (tip 1) 이송 속도가 빠르거나 트리거 신호 출력 거리 간격이 짧고, 트리거 신호 출력 시간이 긴 경우 
		///			트리거 출력 신호가 OFF 되지 못하고 계속 ON 상태가 될 수 있습니다. 
		///			이 상태가 되면 "getarray[1]" 과 "getarray[2]"의 값이 달라집니다. 
		/// </summary>
		/// <param name="get_array" : [array] 배열크기 ->"5",						
		///							  [0]:동작 상태,"1"->동작 중, "0"->중지 상태		 
		///							  [1]:Trigger 신호 출력 횟수, 
		///							  [2]:Trigger 신호 실제 출력 횟수, 
		///							  [3]:보간 이송 누적 거리 [um], 
		///							  [4]:Trigger 신호 출력 거리,					</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")				</returns>	
		public int GetInterpolTriggerStatus(int[] get_array)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetInterpolTriggerStatus(NetID, out get_array[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetInterpolTriggerStatus'. error : " + returnCode,
						"SnetDevice.GetInterpolTriggerStatus");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetInterpolTriggerStatus");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Trigger Output - Acyclic (SNET-P / SNET-RTEX / SNET-ECAT)

		/// <summary>
		/// "eSnetTriggerOnlyAbs()" 함수 사용 전 트리거 출력 채널 번호, 출력 시간, 축 번호, 출력 극성, 기준 위치 소스 의 종류를 설정하기
		/// : old version name -->"eSnetTriggerSetTimeLevel"
		///
		/// (tip 1) "SNET-ECAT" 제품 사용자는 "eSnetSetTriggerTimeLevelEx()"를 사용 하십시요  
		/// (tip 2) 채널별 트리거 신호 출력 접점은 다음과 같습니다. (사용자 메뉴얼 참조)
		///         SNET-P4 : "User IO 커넥터" 17번핀(채널0), 18번핀(채널1)
		///			SNET-P8 : "User IO 커넥터(0 ~ 3축 모듈)" 17번핀(채널0), 18번핀(채널1)  
		///					  "User IO 커넥터(4 ~ 7축 모듈)" 17번핀(채널2), 18번핀(채널3)	
		///			SNET-RTEX: RTEX 드라이버(axis 인자로 지정된 축) "X4 커넥터 SO2" 
		///			SNET-EMD-L2K: Y0.0(채널0) ~ Y0.3(채널3)			
		/// </summary>
		/// <param name="channel">	: 트리거 출력 채널 번호	
		///							  SNET-P4: 0 ~ 1, 그외 다른 제품: 0 ~ 3				</param>
		///								
		/// <param name="axis">		: Trigger 신호 출력 연동 축 번호 						</param>
		/// <param name="time">		: [msec] "출력 ON" 유지 시간							</param>
		/// <param name="level">	: 출력 신호 극성,"0"-> Normal Open(A접점), "1" -> Normal Close(B 접점)	</param>
		/// <param name="mode">		: 좌표 소스 선택,"0"-> Actual position, "1"-> Command Position)			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")							
		///								CommunicationCodeEvent, 
		///								TriggerChannelOverCount		(2101),
		///								TriggerOutPositionOverCount (2102),									</returns>	
		public int SetTriggerTimeLevel(int channel, int axis, int time, bool level, bool mode)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setLevel = level ? 1 : 0;
				int setMode = mode ? 1 : 0;
				returnCode = eSnetSetTriggerTimeLevel(NetID, channel, axis, time, setLevel, setMode);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetTriggerTimeLevel'. error : " + returnCode,
						"SnetDevice.SetTriggerTimeLevel");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetTriggerTimeLevel");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "eSnetSetTriggerTimeLevel()" 설정값 읽기 
		/// : old version name -->"eSnetTriggerGetTimeLevel"
		/// </summary>
		/// <param name="channel">	: 트리거 출력 채널 번호								</param>
		/// <param name="axis">		: Trigger 신호 출력 연동 축 번호 						</param>
		/// <param name="time">		: [msec] "출력 ON" 유지 시간							</param>
		/// <param name="level">	: 출력 신호 극성,"0"-> Normal Open(A접점), "1" -> Normal Close(B 접점)	</param>
		/// <param name="mode">		: 좌표 소스 선택,"0"-> Actual position, "1"-> Command Position)			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")							
		///								CommunicationCodeEvent, 
		///								TriggerChannelOverCount		(2101),
		///								TriggerOutPositionOverCount (2102),									</returns>	
		public int GetTriggerTimeLevel(int channel, ref int axis, ref int time, ref bool level, ref bool mode)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetTriggerTimeLevel(NetID, channel, out int getValue0, out int getValue1, out int getValue2, out int getValue3);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetTriggerTimeLevel'. error : " + returnCode,
						"SnetDevice.GetTriggerTimeLevel");
				}
				else
				{
					axis = getValue0;
					time = getValue1;
					level = (getValue2 == 1);
					mode = (getValue3 == 1);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetTriggerTimeLevel");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 비주기 Trigger 설정 값 쓰기
		/// (tip 1) "remote i/o 모듈(SNET-EMD-L2K제품외 기타 제품)" 또는 "MCU-EL/E 확장 i/o 모듈(SNET-EMD-L2K)" 의 출력 접점 
		///         에서 트리거 신호가 출력 됩니다. 
		/// </summary>
		/// <param name="channel">	: 트리거 출력 채널 번호													</param>
		/// <param name="axis">		: Trigger 신호 출력 연동 축 번호										</param>
		/// <param name="port">		: Trigger 신호를 출력할 "remote io 모듈 id "					
		///							  SNET-EMD-L2K -> "0 or 1"
		///							  SNET-P/SNET-RTEX/SNET-ECAT : "0 ~ 15" 								</param>
		/// <param name="point">	: Trigger 신호 출력 접점 번호(bit)										</param>
		/// <param name="time">		: [msec] "출력 ON" 유지 시간</param>
		/// <param name="level">	: 출력 신호 극성,"0"-> Normal Open(A접점), "1" -> Normal Close(B 접점)	</param>
		/// <param name="mode">		: 좌표 소스 선택,"0"-> Actual position, "1"-> Command Position)			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")							
		///								CommunicationCodeEvent, 
		///								TriggerChannelOverCount		(2101),
		///								TriggerOutPositionOverCount (2102),									</returns>
		public int SetTriggerTimeLevelEx(int channel, int axis, int port, int point, int time, bool level, bool mode)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setLevel = level ? 1 : 0;
				int setMode = mode ? 1 : 0;
				returnCode = eSnetSetTriggerTimeLevelEx(NetID, channel, axis, port, point, time, setLevel, setMode);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetTriggerTimeLevelEx'. error : " + returnCode,
						"SnetDevice.SetTriggerTimeLevelEx");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetTriggerTimeLevelEx");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 비주기 Trigger 설정 값 읽기 
		/// </summary>
		/// <param name="channel">	: 트리거 출력 채널 번호													</param>
		/// <param name="axis">		: Trigger 신호 출력 연동 축 번호										</param>
		/// <param name="port">		: Trigger 신호를 출력할 "remote io 모듈 id "							</param>
		/// <param name="point">	: Trigger 신호 출력 접점 번호(bit)										</param>
		/// <param name="time">		: [msec] "출력 ON" 유지 시간</param>
		/// <param name="level">	: 출력 신호 극성,"0"-> Normal Open(A접점), "1" -> Normal Close(B 접점)	</param>
		/// <param name="mode">		: 좌표 소스 선택,"0"-> Actual position, "1"-> Command Position)			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")							
		///								CommunicationCodeEvent, 
		///								TriggerChannelOverCount		(2101),
		///								TriggerOutPositionOverCount (2102),									</returns>
		public int GetTriggerTimeLevelEx(int channel, ref int axis, ref int port, ref int point, ref int time, ref bool level, ref bool mode)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetTriggerTimeLevelEx(NetID, channel, out int getValue0, out int getValue1, out int getValue2, out int getValue3, out int getValue4, out int getValue5);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetTriggerTimeLevelEx'. error : " + returnCode,
						"SnetDevice.GetTriggerTimeLevelEx");
				}
				else
				{
					axis = getValue0;
					port = getValue1;
					point = getValue2;
					time = getValue3;
					level = (getValue4 == 1);
					mode = (getValue5 == 1);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetTriggerTimeLevelEx");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 사용자가 지정한 위치에서 트리거 신호를 출력합니다.
		/// : old version name -->"eSnetTriggerOnlyAbs"
		/// </summary>
		/// <param name="channel">				: 트리거 출력 채널 번호										</param>
		/// <param name="set_count">			: Trigger 신호 출력 위치 개수 (최대 50 점)					</param>
		/// <param name="set_position">			: [array] 배열 크기->setTrigCount,트리거 출력 위치(배열)	</param>
		/// <returns>							: (see enum "eSnetApiReturnCode")				
		///											CommunicationCodeEvent, 
		///											TriggerChannelOverCount		(2101),
		///											TriggerOutPositionOverCount (2102),						</returns>	
		public int SetTriggerOnlyAbs(int channel, int set_count, int[] set_position)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetTriggerOnlyAbs(NetID, channel, set_count, out set_position[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetTriggerOnlyAbs'. error : " + returnCode,
						"SnetDevice.SetTriggerOnlyAbs");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetTriggerOnlyAbs");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 사용자가 설정한 트리거 출력 기능("eSnetSetTriggerOnlyAbs")을 해제합니다.
		/// : old version name -->"eSnetTriggerSetReset"
		/// </summary>
		/// <param name="channel">	: 트리거 출력 채널 번호				</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
		public int ResetTrigger(int channel)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetResetTrigger(NetID, channel);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetResetTrigger'. error : " + returnCode,
						"SnetDevice.ResetTrigger");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.ResetTrigger");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Trigger Output - Cyclic (SNET-RTEX / SNET-ECAT)

		///
		/// Trigger : SNET-RTEX / SNET-ECAT
		///

		/// <summary>
		/// "Trigger (주기 출력)" 축 번호, 접점 번호 설정 ( SNET-RTEX / SNET-ECAT )
		/// </summary>
		/// <param name="axis">			: Encoder 위치 확인 및 신호를 출력할 축 번호		</param>
		/// <param name="point">		: trigger 신호 출력 접점 번호(bit)				</param>			
		/// <returns>					: (see enum "eSnetApiReturnCode")				</returns>	
		public int SetCyclicTriggerPort(int axis, int point)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetCyclicTriggerPort(NetID, axis, point);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetCyclicTriggerPort'. error : " + returnCode,
						"SnetDevice.SetCyclicTriggerPort");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetCyclicTriggerPort");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Trigger (주기 출력)" 축 번호, 접점 번호 사용자 설정값 확인 ( SNET-RTEX / SNET-ECAT )
		/// </summary>
		/// <param name="axis">			: Encoder 위치 확인 및 신호를 출력할 축 번호		</param>
		/// <param name="point">		: trigger 신호 출력 접점 번호	(bit)				</param>			
		/// <returns>					: (see enum "eSnetApiReturnCode")				</returns>	
		public int GetCyclicTriggerPort(ref int axis, ref int point)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetCyclicTriggerPort(NetID, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetCyclicTriggerPort'. error : " + returnCode,
						"SnetDevice.GetCyclicTriggerPort");
				}
				else
				{
					axis = getValue0;
					point = getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetCyclicTriggerPort");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Trigger (주기 출력)" 축 번호, I/O Module 번호, 접점 번호 설정 ( SNET-ECAT )
		/// </summary>
		/// <param name="axis">			: Encoder 위치를 확인할 축 번호						</param>
		/// <param name="port">			: trigger 신호를 출력할 I/O module 번호('axis' 인자로 지정한 축으로 출력 시 '-1' 대입)</param>
		/// <param name="point">		: trigger 신호 출력 접점 번호(bit)				</param>		
		/// <returns>					: (see enum "eSnetApiReturnCode")				</returns>	
		public int SetCyclicTriggerPortEx(int axis, int port, int point)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetCyclicTriggerPortEx(NetID, axis, port, point);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetCyclicTriggerPortEx'. error : " + returnCode,
						"SnetDevice.SetCyclicTriggerPortEx");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetCyclicTriggerPortEx");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Trigger (주기 출력)" 축 번호, I/O Module 번호, 접점 번호 사용자 설정값 확인 ( SNET-ECAT )
		/// </summary>
		/// <param name="axis">			: Encoder 위치를 확인할 축 번호						</param>
		/// <param name="port">			: trigger 신호를 출력할 I/O module 번호('axis' 인자로 지정한 축으로 출력 시 '-1')</param>
		/// <param name="point">		: trigger 신호 출력 접점 번호(bit)				</param>		
		/// <returns>					: (see enum "eSnetApiReturnCode")				</returns>	
		public int GetCyclicTriggerPortEx(ref int axis, ref int port, ref int point)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetCyclicTriggerPortEx(NetID, out int getValue0, out int getValue1, out int getValue2);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetCyclicTriggerPortEx'. error : " + returnCode,
						"SnetDevice.GetCyclicTriggerPortEx");
				}
				else
				{
					axis = getValue0;
					port = getValue1;
					point = getValue2;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetCyclicTriggerPortEx");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Trigger (주기 출력)" 관련 세부 파라미터 설정 ( SNET-RTEX / SNET-ECAT )
		/// </summary>
		/// <param name="pulse_count">		: trigger 신호 출력 개수 					</param>
		/// <param name="first_position">	: [um],trigger 신호 출력 "시작 좌표"		</param>
		/// <param name="pulse_interval">	: [um],trigger 신호 출력 "거리 간격" 		</param>
		/// <param name="pulse_time">		: [usec],trigger 신호 "출력 ON" 유지시간	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")			</returns>	
		public int SetCyclicTriggerParameter(int pulse_count, int first_position, int pulse_interval, int pulse_time)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetCyclicTriggerParameter(NetID, pulse_count, first_position, pulse_interval, pulse_time);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetCyclicTriggerParameter'. error : " + returnCode,
						"SnetDevice.SetCyclicTriggerParameter");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetCyclicTriggerParameter");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Trigger (주기 출력)" 관련 세부 파라미터 설정값 확인 ( SNET-RTEX / SNET-ECAT )
		/// </summary>
		/// <param name="pulse_count">		: trigger 신호 출력 개수 					</param>
		/// <param name="first_position">	: [um],trigger 신호 출력 "시작 좌표"		</param>
		/// <param name="pulse_interval">	: [um],trigger 신호 출력 "거리 간격" 		</param>
		/// <param name="pulse_time">		: [usec],trigger 신호 "출력 ON" 유지시간	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")			</returns>	
		public int GetCyclicTriggerParameter(ref int pulse_count, ref int first_position, ref int pulse_interval, ref int pulse_time)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetCyclicTriggerParameter(NetID, out int getValue0, out int getValue1, out int getValue2, out int getValue3);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetCyclicTriggerParameter'. error : " + returnCode,
						"SnetDevice.GetCyclicTriggerParameter");
				}
				else
				{
					pulse_count = getValue0;
					first_position = getValue1;
					pulse_interval = getValue2;
					pulse_time = getValue3;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetCyclicTriggerParameter");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Trigger (주기 출력)" 동작 상태 확인 ( SNET-RTEX / SNET-ECAT )
		/// </summary>
		/// <param name="run_flag">			: "true"->동작 중,"false"->정지 상태 			</param>
		/// <param name="trigger_count">	: 현재 까지 출력한 트리거 신호 개수 	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")		</returns>	
		public int GetCyclicTriggerStatus(ref bool run_flag, ref int trigger_count)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetCyclicTriggerStatus(NetID, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetCyclicTriggerStatus'. error : " + returnCode,
						"SnetDevice.GetCyclicTriggerStatus");
				}
				else
				{
					run_flag = ((getValue0 == 1) ? true : false);
					trigger_count = getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetCyclicTriggerStatus");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Trigger (주기 출력)" 시작 ( SNET-RTEX / SNET-ECAT )
		/// </summary>
		/// <param name="enable">	: "true"->기능 시작,"false"->기능 정지		</param>
		/// <returns>				(see enum "eSnetApiReturnCode")		</returns>
		public int StartCyclicTrigger(bool enable)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;
			int value = 0;

			try
			{
				value = (enable ? 1 : 0);

				returnCode = eSnetStartCyclicTrigger(NetID, value);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetStartCyclicTrigger'. error : " + returnCode,
						"SnetDevice.StartCyclicTrigger");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.StartCyclicTrigger");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Trigger (주기 출력)" 축 번호 선택 ( SNET-RTEX )
		/// (tip 1)"ENC1","ENC2","P1 ~ P6"는 제품에 표시된 커넥터 label 입니다. 
		/// : old version name -->"eSnetSetTriggerPortRtex"
		/// </summary>
		/// <param name="encoder_port">	: rtex 축 번호									</param>
		/// <param name="output_port">	: Rtex 드라이버 trigger 신호 출력 접점 설정
		///									"0(SO2)" or "1(SO3)"						</param>			
		/// <returns>					: (see enum "eSnetApiReturnCode")				</returns>
		public int RtexSetTriggerPort(int encoder_port, int output_port)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexSetTriggerPort(NetID, encoder_port, output_port);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexSetTriggerPort'. error : " + returnCode,
						"SnetDevice.RtexSetTriggerPort");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexSetTriggerPort");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Trigger (주기 출력)" "eSnetRtexSetTriggerPort" 사용자 설정값 확인 ( SNET-RTEX )
		/// : old version name -->"eSnetGetTriggerPortRtex"
		/// </summary>
		/// <param name="encoder_port">	: rtex 축 번호								</param>
		/// <param name="output_port">	: Rtex 드라이버 trigger 신호 출력 접점 설정
		///									"0(SO2)" or "1(SO3)"					</param>				
		/// <returns>					: (see enum "eSnetApiReturnCode")			</returns>	
		public int RtexGetTriggerPort(ref int encoder_port, ref int output_port)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexGetTriggerPort(NetID, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetTriggerPort'. error : " + returnCode,
						"SnetDevice.RtexGetTriggerPort");
				}
				else
				{
					encoder_port = getValue0;
					output_port = getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetTriggerPort");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Trigger (주기 출력)" 관련 세부 파라미터 설정 ( SNET-RTEX )
		/// : old version name -->"eSnetSetTriggerParameterRtex"
		/// </summary>
		/// <param name="pulse_count">		: trigger 신호 출력 개수 					</param>
		/// <param name="first_position">	: [um],trigger 신호 출력 "시작 좌표"		</param>
		/// <param name="pulse_interval">	: [um],trigger 신호 출력 "거리 간격" 		</param>
		/// <param name="pulse_on_time">	: [usec],trigger 신호 "출력 ON" 유지시간	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")			</returns>	
		public int RtexSetTriggerParameter(int pulse_count, int first_position, int pulse_interval, int pulse_on_time)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexSetTriggerParameter(NetID, pulse_count, first_position, pulse_interval, pulse_on_time);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexSetTriggerParameter'. error : " + returnCode,
						"SnetDevice.RtexSetTriggerParameter");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexSetTriggerParameter");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Trigger (주기 출력)" "eSnetRtexSetTriggerParameter" 사용자 설정값 확인 ( SNET-RTEX )
		/// : old version name -->"eSnetGetTriggerParameterRtex"
		/// </summary>
		/// <param name="pulse_count">		: trigger 신호 출력 개수 					</param>
		/// <param name="first_position">	: [um],trigger 신호 출력 "시작 좌표"			</param>
		/// <param name="pulse_interval">	: [um],trigger 신호 출력 "거리 간격" 			</param>
		/// <param name="pulse_on_time">	: [usec],trigger 신호 "출력 ON" 유지시간		</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")			</returns>	
		public int RtexGetTriggerParameter(ref int pulse_count, ref int first_position, ref int pulse_interval, ref int pulse_on_time)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexGetTriggerParameter(NetID, out int getValue0, out int getValue1, out int getValue2, out int getValue3);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetTriggerParameter'. error : " + returnCode,
						"SnetDevice.RtexGetTriggerParameter");
				}
				else
				{
					pulse_count = getValue0;
					first_position = getValue1;
					pulse_interval = getValue2;
					pulse_on_time = getValue3;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetTriggerParameter");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Trigger (주기 출력)" 동작 상태 확인 ( SNET-RTEX )
		/// : old version name -->"eSnetGetTriggerStatusRtex"
		/// </summary>
		/// <param name="run_flag">			: "1"->동작 중,"0"->정지 상태 			</param>
		/// <param name="trigger_count">	: 현재 까지 출력한 트리거 신호 개수 	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")		</returns>	
		public int RtexGetTriggerStatus(ref bool run_flag, ref int trigger_count)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexGetTriggerStatus(NetID, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call eSnet'RtexGetTriggerStatus'. error : " + returnCode,
						"SnetDevice.RtexGetTriggerStatus");
				}
				else
				{
					run_flag = ((getValue0 == 1) ? true : false);
					trigger_count = getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetTriggerStatus");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Trigger (주기 출력)" 시작 ( SNET-RTEX )
		/// : old version name -->"eSnetSetTriggerStartRtex"
		/// </summary>
		/// <param name="enable">	: "true"->기능 시작, "false"->기능 정지	</param>
		/// <returns>				(see enum "eSnetApiReturnCode")			</returns>
		public int RtexStartTrigger(bool enable)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (enable ? 1 : 0);

				returnCode = eSnetRtexStartTrigger(NetID, setValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexStartTrigger'. error : " + returnCode,
						"SnetDevice.RtexStartTrigger");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexStartTrigger");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Torque Limit (SNET-RTEX)

		/// <summary>
		/// RTEX Driver "토오크 제한값" 변경 하기 
		/// (tip 1) RTEX 드라이버 "파라미터 Pr0.13"를 변경 합니다. -> 단 Eeprom에는 저장 되지 않습니다. 
		/// </summary>
		/// <param name="axis"> 	: 축 번호   						</param>
		/// <param name="settrq">	: 토오크 제한값,[%]					</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		public int RtexSetTrqLimit(int axis, int settrq)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexSetTrqLimit(NetID, axis, settrq);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexSetTrqLimit'. error : " + returnCode,
						"SnetDevice.RtexSetTrqLimit");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexSetTrqLimit");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// RTEX Driver "토오크 제한값"설정값 확인 
		/// (tip 1) RTEX 드라이버 "파라미터 Pr0.13"설정값을 확인 합니다. 
		/// </summary>
		/// <param name="axis"> 	: 축 번호   						</param>
		/// <param name="gettrq">	: 토오크 제한 설정값,[%]			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		public int RtexGetTrqLimit(int axis, ref int gettrq)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexGetTrqLimit(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetTrqLimit'. error : " + returnCode,
						"SnetDevice.RtexGetTrqLimit");
				}
				else
				{
					gettrq = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetTrqLimit");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "RTEX Driver" 현재 토오크값 확인  
		/// (tip 1) 축별 실시간 토오크 값을 확인 합니다. 단위 0.1[%]( 123 -> 12.3 % )
		/// </summary>
		/// <param name="axis"> 	: 축 번호   						</param>
		/// <param name="curtrq">	: 실시간 토오크값,[0.1%]  			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		public int RtexGetCurTrq(int axis, ref int curtrq)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexGetCurTrq(NetID, axis, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetCurTrq'. error : " + returnCode,
						"SnetDevice.RtexGetCurTrq");
				}
				else
				{
					curtrq = getValue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetCurTrq");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 지정된 목표위치로 단축 이송 단 이송 중 토오크 리밋이 설정 시간 이상 동안 유지 되면 이송 정지 (SNET-RTEX)
		/// (tip 1) 
		/// </summary>
		/// <param name="axis"> 	: 축 번호   							</param>
		/// <param name="pos"> 		: 이송 위치,[um]   						</param>
		/// <param name="vel"> 		: 이송 속도,[mm/min]					</param>
		/// <param name="accel"> 	: 가속시간,[msec]						</param>
		/// <param name="decel"> 	: 감속시간,[msec]						</param>
		/// <param name="jerk"> 	: jerk,[%]								</param>
		/// <param name="time"> 	: 축 이송 중 설정 시간동안 "토오크 제한",
		///							  상태가 유지 되면 정지 ,[msec]			</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>	
		public int RtexMoveAxisUntilTrqLimit(int axis, int pos, int vel, int accel, int decel, int jerk, int time)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexMoveAxisUntilTrqLimit(NetID, axis, pos, vel, accel, decel, jerk, time);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetCurTrq'. error : " + returnCode,
						"SnetDevice.RtexMoveAxisUntilTrqLimit");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexMoveAxisUntilTrqLimit");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}


        /// <summary>
        /// "eSnetRtexMoveAxisUntilTrqLimit" 지령 후 동작 상태를 확인 합니다. 
        /// </summary>
        /// <param name="axis"> 	: 축 번호   							</param>
        /// <param name="status">	: 현재 실행 상태   					</param>
        ///							  bit 0 : "1"-> "eSnetRtexMoveAxisUntilTrqLimit" 동작 중 
        ///							  bit 1 : "1"-> "eSnetRtexMoveAxisUntilTrqLimit" 동작 완료(종료 상태) 
        ///							  bit 2 : "1"-> "eSnetRtexMoveAxisUntilTrqLimit" 동작 중 "토오크 제한 상태"가 설정 시간 동안 유지됨  
        ///							  bit 3 : "1"-> "eSnetRtexMoveAxisUntilTrqLimit" 동작 중 "토오크 제한 상태"가 설정 시간 동안 유지 되지 않고,  
        ///											이송 지령이 끝난 상태 또는 이송 중 사용자가 reset 지령을 내려 정지됨 
        ///							  bit 4 : "1"-> "Rtex 드라이버"의 "Actual Position Set" 동작 완료 상태  </param> 	
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
        public int RtexGetStatusMoveUntilTrqLimit(int axis, ref int status)
		{
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetRtexGetStatusMoveUntilTrqLimit(NetID, axis, out int getValue);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetRtexGetStatusMoveUntilTrqLimit'. error : " + returnCode,
                        "SnetDevice.RtexGetStatusMoveUntilTrqLimit");
                }
                else
                {
                    status = getValue;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.RtexGetStatusMoveUntilTrqLimit");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
       }

        /// <summary>
        ///  첫번째 토오크 제한 설정값을 적용 하고 첫번째 설정 위치로 이송 후 설정 시간 동안 대기 하고 
        ///  두번째 토오크 제한 설정값을 적용 하고 두번째 설정 위치로 이송 합니다. 
        ///  (tip 1) 서보 프레스 어플리케이션에 사용하면 유용 합니다. 
        ///      settrq_1 : 축 하강시 토오크 제한값  
        ///      pos_1	  : 축 하강 목표 위치  
        ///      time     : Press 시간   
        ///      settrq_2 : 축 상승시 토오크 제한값  
        ///      pos_2	  : 축 상승 목표 위치  
        ///  (tip 2) 시퀀스의 실행 상태는 "eSnetRtexGetStatusSetTrqLimitAndMove()"함수를 사용하여 확인 할 수 있습니다. 
        ///
        /// <param name="axis"> 	: 축 번호   								</param>
        /// <param name="settrq_1"> : 첫번째 토오크 제한 설정값 				</param>
        /// <param name="settrq_2"> : 두번째 토오크 제한 설정값 				</param>
        /// <param name="time"> 	: 첫번째 이송 완료 후 대기 시간[msec]     </param>
        /// <param name="pos_1"> 	: 첫번째 이송 위치 [um]					</param>
        /// <param name="pos_2"> 	: 두번째 이송 위치,[um]  					</param>
        /// <param name="vel_1"> 	: 첫번째 이송 속도,[mm/min]				</param>
        /// <param name="vel_2"> 	: 두번째 이송 속도,[mm/min]				</param>
        /// <param name="accel"> 	: 가속시간,[msec]						</param>
        /// <param name="decel"> 	: 감속시간,[msec]						</param>
        /// <param name="jerk"> 	: jerk,[%]								</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")		</returns>	
        public int RtexSetTrqLimitAndMove(int axis, int settrq_1, int settrq_2, int time,
                                         int pos_1, int pos_2, int vel_1, int vel_2, int accel, int decel, int jerk)
		{
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
				returnCode = eSnetRtexSetTrqLimitAndMove(NetID, axis, settrq_1, settrq_2, time, pos_1, pos_2, vel_1, vel_2, accel, decel, jerk);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetRtexSetTrqLimitAndMove'. error : " + returnCode,
                        "SnetDevice.RtexSetTrqLimitAndMove");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.RtexSetTrqLimitAndMove");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }
            return returnCode;
        }

        /// <summary>
        /// "eSnetRtexSetTrqLimitAndMove" 지령 후 동작 상태를 확인 합니다. 
        ///
        /// </summary>
        /// <param name="axis"> 	: 축 번호   							</param>
        /// <param name="step">		: 현재 실행 중인 시퀀스 번호 			</param>
        /// <param name="status">	: 현재 실행 상태   					</param>
        ///							  bit 0 : "1"-> "eSnetRtexSetTrqLimitAndMove" 동작 중 
        ///							  bit 1 : "1"-> "eSnetRtexSetTrqLimitAndMove" 동작 완료(종료 상태) 
        ///							  bit 2 : "1"-> 시퀀스 실행중 축 이송 전 정지 상태가 아님 
        ///							  bit 3 : "1"-> 시퀀스 실행중 축 이송 전 축 알람 발생 상태 
        ///							  bit 4 : "1"-> 시퀀스 실행중 축 이송 전 지정 축이 연속 보간에서 사용 중 
        ///							  bit 5 : "1"-> 시퀀스 실행중 축 이송 전 지정 축이 동기 운전의 슬레이브 축으로 사용중 
        ///							  bit 6 : "1"-> 시퀀스 실행중 MPG 모드로 전환됨 	</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
        public int RtexGetStatusTrqLimitAndMove(int axis, ref int step, ref int status)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;
            try
            {
                returnCode = eSnetRtexGetStatusTrqLimitAndMove(NetID, axis, out int getstep, out int getstatus);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetRtexGetStatusTrqLimitAndMove'. error : " + returnCode,
                        "SnetDevice.RtexGetStatusTrqLimitAndMove");
                }
                else
                {
					step = getstep;
                    status = getstatus;
				}
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.RtexGetStatusTrqLimitAndMove");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        ///  "eSnetRtexSetTrqLimitAndMove" 시퀀스 실행을 종료 합니다. 
        /// <param name="axis"> 	: 축 번호   								</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")		</returns>	
        public int RtexResetTrqLimitAndMove(int axis)
		{
            int returnCode = (int)eSnetApiReturnCode.Success;
            try
            {
                returnCode = eSnetRtexResetTrqLimitAndMove(NetID, axis);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetRtexResetTrqLimitAndMove'. error : " + returnCode,
                        "SnetDevice.RtexResetTrqLimitAndMove");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.RtexResetTrqLimitAndMove");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }
            return returnCode;
        }

        #endregion

        #region Position Capture 1 (SNET-P / SNET-RTEX)

        /*** Position Capture - High Resolution(1 microsecond capture) ( SNET-P / SNET-RTEX ) ***/

        /// <summary>
        /// capture 기능 설정 하기
        /// </summary>
        /// <param name="channel">		: 3개까지 capture기능 설정을 할수 있음("0"~'2")				 	</param>						
        /// <param name="enable">		: "1"->이 채널을 사용함,"0"->이 채널을 사용 안함 			 	</param>						
        /// <param name="encoder_axis">	: encoder를 읽을(구동할) 축									 	</param>
        ///									==> SNET-RTEX 일경우 "0(ENC1)" or "1(ENC2)"					
        /// <param name="port">			: input sensor가 연결된 io port									</param>
        ///									==> SNET-RTEX 일경우 "0(INP1)" ~ "5(INP6)"
        /// <param name="point">		: input sensor가 연결된 io port에서 몇번째 bit인가? ("0"~"31") 	</param>
        ///									==> SNET-RTEX 일경우 "0(bit0)" ~ "7(bit7)"	
        /// <returns>					: (see enum "eSnetApiReturnCode")								</returns>	
        public int SetCaptureConfig(int channel, bool enable, int encoder_axis, int port, int point)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (enable ? 1 : 0);

				returnCode = eSnetSetCaptureConfig(NetID, channel, setValue, encoder_axis, port, point);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetCaptureConfig'. error : " + returnCode,
						"SnetDevice.SetCaptureConfig");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetCaptureConfig");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        /// <summary>
        /// capture 기능 설정값 읽기
        /// </summary>
        /// <param name="channel">		: 3개까지 capture기능 설정을 할수 있음("0"~"2")				 	</param>						
        /// <param name="state">		: "0": 위치 캡처 disable 상태, "1": 위치 캡처 enable 상태
        ///								  "2": 위치 캡처 실행 중, "3": 위치 캡처 정지 상태					</param>
        /// <param name="encoder_axis">	: encoder를 읽을(구동할) 축									 	</param>
        ///									==> SNET-RTEX 일경우 "0(ENC1)" or "1(ENC2)"					
        /// <param name="port">			: input sensor가 연결된 io port									</param>
        ///									==> SNET-RTEX 일경우 "0(INP1)" ~ "5(INP6)"
        /// <param name="point">		: input sensor가 연결된 io port에서 몇번째 bit인가? ("0"~"31")	</param>
        ///									==> SNET-RTEX 일경우 "0(bit0)" ~ "7(bit7)"	
        /// <returns>					: (see enum "eSnetApiReturnCode")								</returns>	
        public int GetCaptureConfig(int channel, ref int state, ref int encoder_axis, ref int port, ref int point)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetCaptureConfig(NetID, channel, out int getValue0, out int getValue1, out int getValue2, out int getValue3);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetCaptureConfig'. error : " + returnCode,
						"SnetDevice.GetCaptureConfig");
				}
				else
				{
					state = getValue0;
                    encoder_axis = getValue1;
					port = getValue2;
					point = getValue3;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetCaptureConfig");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// capture 기능 시작 하기
		/// </summary>
		/// <param name="channel">		: 3개까지 capture기능 설정을 할수 있음("0"~'2")			</param>						
		/// <param name="operate">		: "1"->caputre기능 실행, "0"->capture기능 실행 안함		</param>						
		/// <param name="set_count">	: encoder위치를 capture할 갯수							</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")						</returns>	
		public int StartCapture(int channel, bool operate, int set_count)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (operate ? 1 : 0);
				returnCode = eSnetStartCapture(NetID, channel, setValue, set_count);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call eSnet'StartCapture'. error : " + returnCode,
						"SnetDevice.StartCapture");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.StartCapture");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        /// <summary>
        /// capture 기능 실행할 때 상태 읽기
        /// </summary>
        /// <param name="channel">			: 3개까지 capture기능 설정을 할수 있음("0"~'2")			</param>						
        /// <param name="state">			: "0": 위치 캡처 disable 상태, "1": 위치 캡처 enable 상태
        ///									  "2": 위치 캡처 실행 중, "3": 위치 캡처 정지 상태			</param>
        /// <param name="set_count">		: capture 설정 횟수 										</param>
        /// <param name="rising_count">		: risigin edge position capture한 갯수					</param>
        /// <param name="falling_count">	: falling edge position capture한 갯수					</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")						</returns>	
        public int GetCaptureStatus(int channel, ref int state, ref int set_count, ref int rising_count, ref int falling_count)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetCaptureStatus(NetID, channel, out int getValue0, out int getValue1, out int getValue2, out int getValue3);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetCaptureStatus'. error : " + returnCode,
						"SnetDevice.GetCaptureStatus");
				}
				else
				{
					state = getValue0;
					set_count = getValue1;
					rising_count = getValue2;
					falling_count = getValue3;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetCaptureStatus");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// capture 기능 Capture된 위치값 얻기
		/// </summary>
		/// <param name="channel">			: 3개까지 capture기능 설정을 할수 있음("0"~'2")	</param>						
		/// <param name="index">			: position값을 읽을 index						</param>						
		/// <param name="rising_position">	: [um] : index에 해당하는 rising position		</param>
		/// <param name="falling_position">	: [um] : risigin 해당하는 falling position		</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")				</returns>	
		public int GetCapturePosition(int channel, int index, ref int rising_position, ref int falling_position)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetCapturePosition(NetID, channel, index, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetCapturePosition'. error : " + returnCode,
						"SnetDevice.GetCapturePosition");
				}
				else
				{
					rising_position = getValue0;
					falling_position = getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetCapturePosition");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Position Capture 2 (SNET-RTEX / SNET-ECAT)

		/*** Position Capture 2 - Fieldbus Controller ( SNET-RTEX / SNET-ECAT ) ***/

		//
		// SNET-RTEX / SNET-ECAT
		//

		/// <summary>
		/// "Position Capture 2" Encoder 위치 캡처할 축 및 접점 설정 ( SNET-ECAT )
		/// </summary>
		/// <remarks>
		/// * SNET-ECAT에서 해당 API로 축 입력 접점(bit) 설정 시 CiA402 Object 중 Digital Inputs(index: 0x60FD:00)의 Bit 16 ~ 31를 이용한다.
		/// </remarks>
		/// <param name="axis">			: 축 번호							</param>
		/// <param name="port">			: 캡처 입력 신호를 받을 I/O module 번호. '-1' 대입 시 축 입력 접점으로부터 신호 확인</param>
		/// <param name="point">		: 캡처 입력 신호를 받을 입력 접점(bit)</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>	
		public int SetFieldbusCapture(int axis, int port, int point)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetFieldbusCapture(NetID, axis, port, point);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetFieldbusCapture'. error : " + returnCode,
						"SnetDevice.SetFieldbusCapture");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetFieldbusCapture");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Position Capture 2" Encoder 위치 캡처할 축 및 접점 설정 읽기 ( SNET-ECAT )
		/// </summary>
		/// <param name="axis">			: 축 번호							</param>
		/// <param name="port">			: 캡처 입력 신호를 받을 I/O module 번호. '-1' 설정 시 축 입력 접점으로부터 신호 확인</param>
		/// <param name="point">		: 캡처 입력 신호를 받을 입력 접점(bit)</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>	
		public int GetFieldbusCapture(int axis, ref int port, ref int point)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetFieldbusCapture(NetID, axis, out int value0, out int value1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetFieldbusCapture'. error : " + returnCode,
						"SnetDevice.GetFieldbusCapture");
				}
				else
				{
					port = value0;
					point = value1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetFieldbusCapture");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Position Capture 2" 파라미터 설정 및 기능 시작/정지 ( SNET-RTEX / SNET-ECAT )
		/// </summary>
		/// <remarks>
		/// * 별도 설정이 없으면, 축 입력 접점의 신호 감지 시 Encoder 위치를 캡처한다.
		/// * 'SetFieldbusCapture' API를 이용하면, I/O module의 입력 접점 신호 감지 시, Encoder 위치를 캡처한다(SNET-ECAT).
		/// * SNET-RTEX의 경우, 축 입력 접점 bit 0에서만 입력 신호를 감지하도록 고정되어 있다.
		/// </remarks>
		/// <param name="axis">			: 축 번호							</param>
		/// <param name="enable">		: "1"->시작, "0"->중지  				</param>
		/// <param name="set_count">	: Capture 횟수(최대 50)				</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>	
		public int StartFieldbusCapture(int axis, bool enable, int set_count)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (enable ? 1 : 0);
				returnCode = eSnetStartFieldbusCapture(NetID, axis, setValue, set_count);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetStartFieldbusCapture'. error : " + returnCode,
						"SnetDevice.StartFieldbusCapture");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.StartFieldbusCapture");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Position Capture 2" 동작 상태 확인 ( SNET-RTEX / SNET-ECAT )
		/// </summary>
		/// <param name="axis">				: 축 번호							</param>
		/// <param name="enable">			: "1"->실행 중, "0"->중지 상태 		</param>
		/// <param name="set_count">		: Capture 횟수 "설정 값"  			</param>
		/// <param name="rising_count">		: Rising Edge 에서 캡처된 횟수  		</param>
		/// <param name="falling_count">	: Falling Edge 에서 캡처된 횟수  	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
		public int GetFieldbusCaptureStatus(int axis, ref bool enable, ref int set_count, ref int rising_count, ref int falling_count)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetFieldbusCaptureStatus(NetID, axis, out int value0, out int value1, out int value2, out int value3);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetFieldbusCaptureStatus'. error : " + returnCode,
						"SnetDevice.GetFieldbusCaptureStatus");
				}
				else
				{
					enable = ((value0 > 0) ? true : false);
					set_count = value1;
					rising_count = value2;
					falling_count = value3;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetFieldbusCaptureStatus");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "Position Capture 2" 캡처된 좌표 읽기 ( SNET-RTEX / SNET-ECAT )
		/// </summary>
		/// <param name="axis">				: 축 번호												</param>
		/// <param name="index">			: 캡처 순번 												</param>
		/// <param name="rising_position">	: 인자 "index"순번에서 "rising edge" 일때 캡처된 좌표   	</param>
		/// <param name="falling_position">	: 인자 "index"순번에서 "falling edge" 일때 캡처된 좌표	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")					</returns>	
		public int GetFieldbusCapturePosition(int axis, int index, ref int rising_position, ref int falling_position)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetFieldbusCapturePosition(NetID, axis, index, out int value0, out int value1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetFieldbusCapturePosition'. error : " + returnCode,
						"SnetDevice.GetFieldbusCapturePosition");
				}
				else
				{
					rising_position = value0;
					falling_position = value1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetFieldbusCapturePosition");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		//
		// SNET-RTEX
		//

		/// <summary>
		/// SNET-RTEX "Position Capture 2" 파라미터 설정 및 기능 시작/정지 
		/// : old version name -->"eSnetSetCaptureStartRtex"
		/// </summary>
		/// <param name="axis">			: rtex 축 번호,"0 ~ 31"				</param>
		/// <param name="enable">		: "true"->시작, "false"->중지 		</param>
		/// <param name="set_count">	: Capture 횟수   					</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>	
		public int RtexStartCapture(int axis, bool enable, int set_count)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = (enable ? 1 : 0);
				returnCode = eSnetRtexStartCapture(NetID, axis, setValue, set_count);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexStartCapture'. error : " + returnCode,
						"SnetDevice.RtexStartCapture");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexStartCapture");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-RTEX "Position Capture 2" 동작 상태 확인 
		/// : old version name -->"eSnetGetCaptureStatusRtex"
		/// </summary>
		/// <param name="axis">				: rtex 축 번호,"0 ~ 31"					</param>
		/// <param name="enable">			: "true"->실행 중, "false"->중지 상태	</param>
		/// <param name="set_count">		: Capture 횟수 "설정 값"  				</param>
		/// <param name="rising_count">		: Rising Edge 에서 캡처된 횟수  		</param>
		/// <param name="falling_count">	: Falling Edge 에서 캡처된 횟수  		</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	<	/returns>	
		public int RtexGetCaptureStatus(int axis, ref bool enable, ref int set_count, ref int rising_count, ref int falling_count)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexGetCaptureStatus(NetID, axis, out int getValue0, out int getValue1, out int getValue2, out int getValue3);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetCaptureStatus'. error : " + returnCode,
						"SnetDevice.RtexGetCaptureStatus");
				}
				else
				{
					enable = ((getValue0 == 1) ? true : false);
					set_count = getValue1;
					rising_count = getValue2;
					falling_count = getValue3;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetCaptureStatus");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// SNET-RTEX "Position Capture 2" 캡처된 좌표 읽기 
		/// : old version name -->"eSnetGetCapturePositionRtex"
		/// </summary>
		/// <param name="axis">				: rtex 축 번호,"0 ~ 31"								</param>
		/// <param name="index">			: 캡처 순번 										</param>
		/// <param name="rising_position">	: "index"순번에서 "rising edge" 일때 캡처된 좌표   	</param>
		/// <param name="falling_position">	: "index"순번에서 "falling edge" 일때 캡처된 좌표   </param>
		/// <returns>						: (see enum "eSnetApiReturnCode")					</returns>	
		public int RtexGetCapturePosition(int axis, int index, ref int rising_position, ref int falling_position)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexGetCapturePosition(NetID, axis, index, out int getValue0, out int getValue1);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetCapturePosition'. error : " + returnCode,
						"SnetDevice.RtexGetCapturePosition");
				}
				else
				{
					rising_position = getValue0;
					falling_position = getValue1;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetCapturePosition");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Alarm & Warning (SNET-RTEX)

		/// <summary>
		/// RTEX 드라이버에서 Alarm and Warning Code 읽기 
		/// </summary>
		/// <param name="axis">		    : 축 번호(rtex축)					</param>						
		/// <param name="alarm_main">	: Alarm Code Main					</param>
		/// <param name="alarm_sub">	: Alarm Code Sub					</param>
		/// <param name="warning_main">	: warning Code Main					</param>
		/// <param name="warning_sub">	: warning Code Sub					</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>	
		public int RtexGetAlarmWarning(int axis, ref int alarm_main, ref int alarm_sub, ref int warning_main, ref int warning_sub)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexGetAlarmWarning(NetID, axis, out int getValue0, out int getValue1, out int getValue2, out int getValue3);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetAlarmWarning'. error : " + returnCode,
						"SnetDevice.RtexGetAlarmWarning");
				}
				else
				{
					alarm_main = getValue0;
					alarm_sub = getValue1;
					warning_main = getValue2;
					warning_sub = getValue3;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetAlarmWarning");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Driver Information (SNET-RTEX)
		/// <summary>
		/// RTEX 드라이버에 "Monitor Command" 요청 
		/// </summary>						
		/// <param name="axis">		    : 축 번호(rtex축)										</param>						
		/// <param name="type_code">	: 항목 번호 (RTEX 드라이버 메뉴얼 참조-Monitor command)	</param>
		/// <param name="index">		: 항목 세부 번호	(RTEX 드라이버 메뉴얼 참조)				</param>
		/// <param name="information">	: 드라이버에서 읽은 정보 									</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>	

		public int RtexGetMonitorCommand(int axis, int type_code, int index, ref int information)
        {
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexGetMonitorCommand(NetID, axis, type_code, index, out int getvalue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetMonitorCommand'. error : " + returnCode,
						"SnetDevice.RtexGetMonitorCommand");
				}
				else
				{
					information = getvalue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetMonitorCommand");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}
			return returnCode;
		}
		#endregion

		#region Driver System Id (SNET-RTEX)
		/// <summary>
		/// RTEX 드라이버에 "System ID Command" 요청 
		/// </summary>
		/// <param name="axis">		    : 축 번호(rtex축)											</param>
		/// <param name="type_code">	: 항목 번호 (RTEX 드라이버 메뉴얼 참조- System Id command)		</param>
		/// <param name="index">		: 항목 세부 번호	(RTEX 드라이버 메뉴얼 참조)					</param>
		/// <param name="information">	: 드라이버에서 읽은 정보 										</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")	</returns>			
		public int RtexGetSystemIdCommand(int axis, int type_code, int index, ref int information)
        {
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetRtexGetSystemIdCommand(NetID, axis, type_code, index, out int getvalue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetRtexGetSystemIdCommand'. error : " + returnCode,
						"SnetDevice.RtexGetSystemIdCommand");
				}
				else
				{
					information = getvalue;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.RtexGetSystemIdCommand");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}
			return returnCode;
		}
		#endregion

		#region Positional Command Filter

		/// <summary>
		/// 지령 필터 설정  
		///	(tip 1) 모든 축이 "정지 상태"일때 사용해야 됩니다. 
		/// (tip 2) "연속 보간 이송"중 에만 필터가 적용 됩니다. 
		/// </summary>
		/// <param name="enable">	: 필터 적용 유무 						
		///							  "1"->enable, "0"->disable			</param>
		/// <param name="para">		: 지연 파라미터    					
		///							  최대 200(msec)					</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		public int SetCmdFilterConfig(bool enable, int para)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setValue = enable ? 1 : 0;

				returnCode = eSnetSetCmdFilterConfig(NetID, setValue, para);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetCmdFilterConfig'. error : " + returnCode,
						"SnetDevice.SetCmdFilterConfig");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetCmdFilterConfig");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}
			return returnCode;

		}

		/// <summary>
		/// 스무딩 필터 설정(eSnetSetCmdFilterConfig) 확인 
		/// </summary>
		/// <param name="enable">	: 필터 적용 유무 						
		///							  "1"->enable, "0"->disable			</param>
		/// <param name="para">		: 지연 파라미터    					
		///							  최대 200(msec)					</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		public int GetCmdFilterConfig(ref bool enable, ref int para)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetCmdFilterConfig(NetID, out int getValue, out para);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetCmdFilterConfig'. error : " + returnCode,
						"SnetDevice.GetCmdFilterConfig");
				}
				else
				{
					enable = (getValue == 1 ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetCmdFilterConfig");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}
			return returnCode;
		}

		#endregion

		#region Linear Compensation

		/// <summary>
		/// 축 좌표 보정 테이블 설정  
		/// (tip 1) "축 상대 좌표 모드(eSnetSetAxisAbsRelMode(..1)" 에서는 사용 금지 
		/// (tip 2) "eSnetSetPositionCompensationEnable" 함수로 기능을 "enable" 상태로 만들면 "jog"이송을 제외한 "이송 명령 지령"시
		///	  보정값이 적용된 "지령 위치(command position)가 제어기로 전달 됩니다. 
		/// </summary>
		/// <param name="axis">				:축 번호									</param>
		/// <param name="count">			:보정 테이블 갯수: "2 ~ 512"				</param>
		/// <param name="start_position">	:보정 테이블 위치 offfset					</param>
		/// <param name="position">			:[array], 배열크기-> count	
		///									 보정 테이블 좌표 [um]		
		///									 pos[0]은 항상"0"을 설정 하십시요			</param>
		/// <param name="correction">		:[array], 배열크기-> count		
		///									 보정 테이블 보정 좌표 [um]		
		///									 correction[0]는 항상"0"을 설정 하십시요	</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")			</returns>	
		public int SetPositionCompensation(int axis, int count, int start_position, int[] position, int[] correction)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetPositionCompensation(NetID, axis, count, start_position, out position[0], out correction[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetPositionCompensation'. error : " + returnCode,
									"SnetDevice.SetPositionCompensation");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetPositionCompensation");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "eSnetSetPositionCompensation" 설정값 및 "enable"상태 확인 
		/// </summary>
		/// <param name="axis">				: 축 번호							</param>						
		/// <param name="enable">			: "true"-> enable 상태 				</param>
		/// <param name="count">			: 보정 테이블 갯수					</param>
		/// <param name="start_position">	: 보정 테이블 위치 offfset			</param>
		/// <param name="position">			: [array], 배열크기-> count	
		///										보정 테이블 좌표 		[um]	</param>
		/// <param name="correction">		: [array], 배열크기-> count		
		///										보정 테이블 보정 좌표 [um]		</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")	</returns>	
		public int GetPositionCompensation(int axis, ref bool enable, ref int count, ref int start_position, int[] position, int[] correction)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetPositionCompensation(NetID, axis, out int getEnable, out int getCount, out int getStartPosition, out position[0], out correction[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetPositionCompensation'. error : " + returnCode,
									"SnetDevice.GetPositionCompensation");
				}
				else
				{
					enable = (getEnable == 1 ? true : false);
					count = getCount;
					start_position = getStartPosition;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetPositionCompensation");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 축 보정 테이블 적용 실행 및 금지 
		/// (tip 1) "eSnetSetPositionCompensation" 함수로 보정 테이블을 설정하고 "enable" 상태가 되면 이후 "jog"이송을 제외한 
		///			"이송 명령 지령"시 보정값이 적용된 "지령 위치(command position)가 제어기로 전달 됩니다. 
		/// </summary>
		/// <param name="axis">		: 축 번호							</param>						
		/// <param name="enable">	: "1"-> enable, "0"-> disable		</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")	</returns>	
		public int SetPositionCompensationEnable(int axis, bool enable)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				int setEnable = enable ? 1 : 0;
				returnCode = eSnetSetPositionCompensationEnable(NetID, axis, setEnable);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetPositionCompensationEnable'. error : " + returnCode,
									"SnetDevice.SetPositionCompensationEnable");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetPositionCompensationEnable");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "eSnetSetPositionCompensationEnable" 확인 
		/// </summary>
		/// <param name="axis">		: 축 번호								</param>						
		/// <param name="enable">	: "true"-> enable, "false"-> disable	</param>
		/// <returns>				: (see enum "eSnetApiReturnCode")		</returns>	
		public int GetPositionCompensationEnable(int axis, ref bool enable)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetPositionCompensationEnable(NetID, axis, out int getEnable);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetPositionCompensationEnable'. error : " + returnCode,
													"SnetDevice.GetPositionCompensationEnable");
				}
				else
				{
					enable = (getEnable == 1 ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetPositionCompensationEnable");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 보정 테이블이 적용된 좌표 결과 확인 하기 
		/// (tip 1) "eSnetSetPositionCompensation" 함수를 사용하여 보정 테이블을 설정하고 "축 이송 없이" 특정 좌표에 대한 보정 결과
		///        를 확인하고 싶을때 사용 합니다. 
		/// </summary>
		/// <param name="axis">						: 축 번호							</param>						
		/// <param name="command_position">			: 목표 좌표							</param>
		/// <param name="compensation_position">	: 보정 테이블이 적용된 return 좌표	</param>
		/// <returns>								: (see enum "eSnetApiReturnCode")	</returns>	
		public int GetPositionCompensationResult(int axis, int command_position, ref int compensation_position)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetPositionCompensationResult(NetID, axis, command_position, out int getValue);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetPositionCompensationResult'. error : " + returnCode,
													"SnetDevice.GetPositionCompensationResult");
				}
				else
					compensation_position = getValue;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetPositionCompensationResult");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "축 좌표 보정" 기능 사용시 "보정값이 적용되지 않은 Command Position(절대 좌표)"과  
		/// "보정값이 적용된 실제 Command Position(절대 좌표)"을 확인 합니다.  
		/// </summary>
		/// <param name="axis">			: axis number											</param>						
		/// <param name="real_position">: 보정값이 적용된 실제 제어기 command position, [um]	</param>
		/// <param name="position">		: 보정값이 적용되지 않은 command position, [um]			</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")						</returns>					
		public int GetCommandPositionCompensation(int axis, ref int real_position, ref int position)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetCommandPositionCompensation(NetID, axis, out int getpos1, out int getpos2);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetCommandPositionCompensation'. error : " + returnCode,
													"SnetDevice.GetCommandPositionCompensation");
				}
				else
				{
					real_position = getpos1;
					position = getpos2;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetCommandPositionCompensation");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// "축 좌표 보정" 기능 사용시 "보정값이 적용되지 않은 Actual Position(기계 좌표)"와 
		/// "보정값이 적용된 실제 Actual Position(기계 좌표)"를 확인 합니다. 
		/// </summary>
		/// <param name="axis">			: axis index										</param>						
		/// <param name="real_position">: 보정값이 적용된 실제 제어기 Actual position, [um]	</param>
		/// <param name="position">		: 보정값이 적용되지 않은 Actual position ,[um]		</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")					</returns>
		public int GetActualPositionCompensation(int axis, ref int real_position, ref int position)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetActualPositionCompensation(NetID, axis, out int getpos1, out int getpos2);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetActualPositionCompensation'. error : " + returnCode,
													"SnetDevice.GetActualPositionCompensation");
				}
				else
				{
					real_position = getpos1;
					position = getpos2;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetActualPositionCompensation");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region Interrupt Event

		/// <summary>
		/// Interrupt event table 특정 번호의 설정 정보 쓰기
		/// </summary>
		/// <remarks>
		/// Interrupt event table의 번호는 0 ~ 127 설정이 가능
		/// </remarks>
		/// <param name="table_index">	: 설정을 쓸 Interrupt event table 번호</param>
		/// <param name="enable">		: 'table_index' 에 해당하는 Interrupt event table 활성화 유무. 1: 활성화, 0: 비활성화</param>
		/// <param name="info">			: 'table_index' 에 해당하는 Interrupt event table 설정 정보. struct 'InterruptEventTableInfo' 참조</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")					</returns>
		public int SetInterruptEventTable(int table_index, bool enable, InterruptEventTableInfo info)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;
			int enableTemp = 0;

			try
			{
				enableTemp = (enable ? 1 : 0);

				returnCode = eSnetSetInterruptEventTable(NetID, table_index, enableTemp, info);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetInterruptEventTable'. error : " + returnCode,
													"SnetDevice.SetInterruptEventTable");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetInterruptEventTable");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Interrupt event table 특정 번호의 설정 정보 읽기
		/// </summary>
		/// <param name="table_index">	: 설정을 읽을 Interrupt event table 번호</param>
		/// <param name="enable">		: 'table_index' 에 해당하는 Interrupt event table 활성화 유무. 1: 활성화, 0: 비활성화</param>
		/// <param name="info">			: 'table_index' 에 해당하는 Interrupt event table 설정 정보. struct 'InterruptEventTableInfo' 참조</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")					</returns>
		public int GetInterruptEventTable(int table_index, ref bool enable, ref InterruptEventTableInfo info)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetInterruptEventTable(NetID, table_index, out int enableTemp, out InterruptEventTableInfo infoTemp);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetInterruptEventTable'. error : " + returnCode,
													"SnetDevice.GetInterruptEventTable");
				}
				else
				{
					enable = ((enableTemp > 0) ? true : false);
					info = infoTemp;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetInterruptEventTable");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Interrupt event table 특정 번호 삭제
		/// </summary>
		/// <param name="table_index">	: Interrupt event table 번호</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")					</returns>
		public int EraseInterruptEventTable(int table_index)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetEraseInterruptEventTable(NetID, table_index);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetEraseInterruptEventTable'. error : " + returnCode,
													"SnetDevice.EraseInterruptEventTable");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.EraseInterruptEventTable");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Interrupt event table 전체 번호 삭제
		/// </summary>
		/// <returns>					: (see enum "eSnetApiReturnCode")					</returns>
		public int ClearInterruptEventTable()
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetClearInterruptEventTable(NetID);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetClearInterruptEventTable'. error : " + returnCode,
													"SnetDevice.ClearInterruptEventTable");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.ClearInterruptEventTable");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Interrupt event handler(callback function) 등록
		/// </summary>
		/// <remarks>
		/// Interrupt event 발생 시, 자동으로 호출될 Function pointer(handler)를 등록한다.
		/// 이벤트가 발생하여 등록된 Function 호출 시, 'table_index' 인자로 발생한 Interrupt event table 번호가 전달된다
		/// </remarks>
		/// <param name="function">		: Interrupt event 발생 시, 호출 될 Function(event handler)</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")					</returns>
		public int SetInterruptEventFunction(InterruptEventHandler function)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetInterruptEventFunction(NetID, function);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetInterruptEventFunction'. error : " + returnCode,
													"SnetDevice.SetInterruptEventFunction");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetInterruptEventFunction");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Interrupt event 기능 활성화/비활성화 정보 쓰기
		/// </summary>
		/// <param name="enable">		: Interrupt event 기능 활성화 유무. 1: 활성화, 0: 비활성화</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")					</returns>
		public int EnableInterruptEvent(bool enable)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;
			int enableTemp = 0;

			try
			{
				enableTemp = (enable ? 1 : 0);

				returnCode = eSnetEnableInterruptEvent(NetID, enableTemp);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetEnableInterruptEvent'. error : " + returnCode,
													"SnetDevice.EnableInterruptEvent");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.EnableInterruptEvent");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Interrupt event 기능 활성화/비활성화 정보 읽기
		/// </summary>
		/// <param name="enable">		: Interrupt event 기능 활성화 유무. 1: 활성화, 0: 비활성화</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")					</returns>
		public int IsInterruptEvent(ref bool enable)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetIsInterruptEvent(NetID, out int enableTemp);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetIsInterruptEvent'. error : " + returnCode,
													"SnetDevice.IsInterruptEvent");
				}
				else
				{
					enable = ((enableTemp > 0) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.IsInterruptEvent");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Interrupt event table의 이벤트 발생 대기
		/// </summary>
		/// <remarks>
		/// Interrupt event table 정보에 맞는 event 발생이 있을 때까지 대기한다
		/// </remarks>
		/// <param name="table_index">	: Interrupt event table 번호</param>
		/// <param name="timeout">		: 대기 시간(milliseconds). 0으로 설정 시, 무한 대기</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")					</returns>
		public int WaitInterruptEvent(int table_index, int timeout)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetWaitInterruptEvent(NetID, table_index, timeout);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetWaitInterruptEvent'. error : " + returnCode,
													"SnetDevice.WaitInterruptEvent");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.WaitInterruptEvent");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Interrupt event table의 이벤트 발생 대기 해제
		/// </summary>
		/// <remarks>
		/// Event 발생 대기 중이면('WaitInterruptEvent' API Function 동작 중), event 발생 대기를 강제로 해제한다
		/// </remarks>
		/// <param name="table_index">	: Interrupt event table 번호</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")					</returns>
		public int ReleaseWaitingInterruptEvent(int table_index)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetReleaseWaitingInterruptEvent(NetID, table_index);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetReleaseWaitingInterruptEvent'. error : " + returnCode,
													"SnetDevice.ReleaseWaitingInterruptEvent");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.ReleaseWaitingInterruptEvent");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// Interrupt event table의 이벤트 발생 대기 유무를 확인
		/// </summary>
		/// <remarks>
		/// 'WaitInterruptEvent' API Function이 동작 중인지 확인한다
		/// </remarks>
		/// <param name="table_index">	: Interrupt event table 번호</param>
		/// <param name="waiting">		: 이벤트 발생 대기 유무. true: 현재 이벤트 발생 대기 중, false: None</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")					</returns>
		public int IsWaitingInterruptEvent(int table_index, ref bool waiting)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetIsWaitingInterruptEvent(NetID, table_index, out int waitingTemp);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetIsWaitingInterruptEvent'. error : " + returnCode,
													"SnetDevice.IsWaitingInterruptEvent");
				}
				else
				{
					waiting = ((waitingTemp > 0) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.IsWaitingInterruptEvent");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region GantrySync

		/// <summary>
		/// 동기 구동 활성화
		/// </summary>
		/// <param name="master">	: master 축 번호					 </param>
		/// <param name="slave">	: slave 축 번호					 </param>	
		/// <param name="enable">	: 동기 구동 활성화/비활성화.	
		///							  "1"-> enable, "0"-> disable	 </param>
		/// <returns>				: (see enum "eSnetApiReturnCode")</returns>
		public int EnableGantrySync(int master, int slave, bool enable)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetEnableGantrySync(NetID, master, slave, (enable ? 1 : 0));
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetEnableGantrySync'. error : " + returnCode,
													"SnetDevice.EnableGantrySync");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.EnableGantrySync");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 동기 구동 활성화 여부 반환
		/// </summary>
		/// <param name="master">	: master 축 번호					 </param>
		/// <param name="slave">	: slave 축 번호					 </param>	
		/// <param name="enable">	: 동기 구동 활성화/비활성화.	
		///							  "1"-> enable, "0"-> disable	 </param>
		/// <returns>				: (see enum "eSnetApiReturnCode")</returns>
		public int IsGantrySync(int master, ref int slave, ref bool enable)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetIsGantrySync(NetID, master, out int getSlave, out int getEnable);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetIsGantrySync'. error : " + returnCode,
													"SnetDevice.IsGantrySync");
				}
				else
				{
					slave = getSlave;
					enable = ((getEnable > 0) ? true : false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.IsGantrySync");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 동기 원점 검색 설정
		/// </summary>
		/// <param name="master">			: master 축 번호						</param>
		/// <param name="slave">			: slave 축 번호						</param>
		/// <param name="dualUse">			: 원점 검색 실행 축 옵션				
		///									  "0" -> master 축 기준 단독 homing
		///									  "1" -> master 축 기준 homing 이후, slave 축 기준 homing </param>
		/// <param name="compensationUse">	: 보정 이송 옵션						
		///									  "0" -> 홈 센서 간 거리 구하고 보정 이송 하지 않음
		///									  "1" -> 홈 센서 간 거리 및 비틀림 구하고 보정 이송 진행 </param>
		/// <param name="offset">			: 두 축 홈 센서 간 거리				</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")   </returns>
		public int SetGantrySyncHoming(int master, int slave, int dualUse, int compensationUse, int offset)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetSetGantrySyncHoming(NetID, master, slave, dualUse, compensationUse, offset);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetSetGantrySyncHoming'. error : " + returnCode,
													"SnetDevice.SetGantrySyncHoming");
				}

			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.SetGantrySyncHoming");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 동기 원점 검색 설정 반환
		/// </summary>
		/// <param name="master">			: master 축 번호						</param>
		/// <param name="slave">			: slave 축 번호						</param>
		/// <param name="dualUse">			: 원점 검색 실행 축 옵션				
		///									  "0" -> master 축 기준 단독 homing
		///									  "1" -> master 축 기준 homing 이후, slave 축 기준 homing </param>
		/// <param name="compensationUse">	: 보정 이송 옵션						
		///									  "0" -> 홈 센서 간 거리 구하고 보정 이송 하지 않음
		///									  "1" -> 홈 센서 간 거리 및 비틀림 구하고 보정 이송 진행 </param>
		/// <param name="offset">			: 두 축 홈 센서 간 거리				</param>
		/// <returns>						: (see enum "eSnetApiReturnCode")   </returns>
		public int GetGantrySyncHoming(int master, ref int slave, ref int dualUse, ref int compensationUse, ref int offset)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetGantrySyncHoming(NetID, master, out int getSlave, out int getDualUse, out int getCompensationUse, out int getOffset);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetGantrySyncHoming'. error : " + returnCode,
													"SnetDevice.GetGantrySyncHoming");
				}
				else
				{
					slave = getSlave;
					dualUse = getDualUse;
					compensationUse = getCompensationUse;
					offset = getOffset;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetGantrySyncHoming");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// 동기 원점 검색으로 도출된 두 축 홈 센서 간 거리 반환
		/// </summary>
		/// <param name="master">	: master 축 번호					 </param>
		/// <param name="offset">	: 두 축 홈 센서 간 거리			 </param>
		/// <returns>				: (see enum "eSnetApiReturnCode")</returns>
		public int GetGantrySyncHomeSensorOffset(int master, ref int offset)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetGetGantrySyncHomeSensorOffset(NetID, master, out int getOffset);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetGetGantrySyncHomeSensorOffset'. error : " + returnCode,
													"SnetDevice.GetGantrySyncHomeSensorOffset");
				}
				else
					offset = getOffset;

			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.GetGantrySyncHomeSensorOffset");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		#endregion

		#region EtherCAT Command

		/// <summary>
		/// EtherCAT Master에 연결된 Slave(Node) 수 읽기  (eSnetEcatGetNodeCount)  
		/// </summary>
		/// <param name="count">: Slave(Node) 수</param>
		/// <returns>			: (see enum "eSnetApiReturnCode")</returns>
		public int EcatGetNodeCount(ref int count)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetEcatGetNodeCount(NetID, out int getCount);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetEcatGetNodeCount'. error : " + returnCode,
													"SnetDevice.EcatGetNodeCount");
				}
				else
				{
					count = getCount;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.EcatGetNodeCount");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        /// <summary>
        /// EtherCAT Master에 연결된 Slave Motion Node 수 읽기
        /// </summary>
        /// <param name="count_all_node">	: 전체 Slave(Node) 수													</param>
		/// <param name="count_node_motion">: Slave Motion Node 수													</param>
        /// <param name="node_index">		: Slave Motion Node 수 만큼의 Node Index	
        ///										(ex) 두 번째, 네 번째 Node에 Motion Slave가 있다면 node_inde[0]=1, node_index[1]=3
        ///										(ex) 32개 배열을 선언하고, 읽은 count 갯수만큼 배열값을 읽을 것	</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")										</returns>
        public int EcatGetNodeMotionCount(ref int count_all_node, ref int count_node_motion, int[] node_index)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;
            int[] getNodeIndex = new int[32];

            try
            {
                returnCode = eSnetEcatGetNodeMotionCount(NetID, out int getCountAll, out int getCountMotion, out getNodeIndex[0]);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetEcatGetNodeMotionCount'. error : " + returnCode,
                                                    "SnetDevice.EcatGetNodeMotionCount");
                }
                else
                {
                    count_all_node		= getCountAll;
					count_node_motion	= getCountMotion;

                    for (int i = 0; i < getCountMotion; i++)
                        node_index[i] = getNodeIndex[i];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.EcatGetNodeMotionCount");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }
		
		/// <summary>
        /// EtherCAT Master에 연결된 Slave(Node)의 Fixed Address 읽기  (eSnetEcatGetFixedAddress)  
        /// </summary>
        /// <param name="node_index">	: 읽어올 Slave(Node)의 시작 번호</param>
        /// <param name="count">		: 읽어올 Slave(Node) 수</param>
        /// <param name="address">		: Slave(Node)의 Fixed Address</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")</returns>
        public int EcatGetFixedAddress(int node_index, int count, int[] address)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetEcatGetFixedAddress(NetID, node_index, count, out address[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetEcatGetFixedAddress'. error : " + returnCode,
													"SnetDevice.EcatGetFixedAddress");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.EcatGetFixedAddress");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// EtherCAT Master에 연결된 Slave(Node)의 Logical Start Address 읽기  (eSnetEcatGetLogicalAddress)  
		/// </summary>
		/// <param name="node_index">	: 읽어올 Slave(Node)의 시작 번호</param>
		/// <param name="count">		: 읽어올 Slave(Node) 수</param>
		/// <param name="address">		: Slave(Node)의 Logical Start Address</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")</returns>
		public int EcatGetLogicalAddress(int node_index, int count, int[] address)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetEcatGetLogicalAddress(NetID, node_index, count, out address[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetEcatGetLogicalAddress'. error : " + returnCode,
													"SnetDevice.EcatGetLogicalAddress");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.EcatGetLogicalAddress");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// EtherCAT Master에 연결된 모든 Slave(Node)의 State를 전환( ‘Init’ 혹은 ‘Operational’ State)
		/// </summary>
		/// <param name="state">	1: 'Init' State
		///							8: 'Operational' State</param>
		/// <returns>				 : (see enum "eSnetApiReturnCode")</returns>
		public int EcatSetStateMachine(int state)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetEcatSetStateMachine(NetID, state);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetEcatSetStateMachine'. error : " + returnCode,
													"SnetDevice.EcatSetStateMachine");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.EcatSetStateMachine");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// EtherCAT Master에 연결된 Slave(Node)의 AL Status 읽기 (eSnetEcatGetAlStatus)
		/// </summary>
		/// <param name="node_index">	: 읽어올 Slave(Node)의 시작 번호</param>
		/// <param name="count">		: 읽어올 Slave(Node) 수</param>
		/// <param name="status">		: Slave(Node)의 AL Status
		///								  1 -> 'Init' State
		///								  2 -> 'Pre-Operational' State
		///								  3 -> 'Bootstrap' State
		///								  4 -> 'Safe-Operational' State
		///								  8 -> 'Operational' State </param>
		/// <returns>					: (see enum "eSnetApiReturnCode")</returns>
		public int EcatGetAlStatus(int node_index, int count, int[] status)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetEcatGetAlStatus(NetID, node_index, count, out status[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetEcatGetAlStatus'. error : " + returnCode,
													"SnetDevice.EcatGetAlStatus");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.EcatGetAlStatus");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

		/// <summary>
		/// EtherCAT Master에 연결된  Slave(Node)의 AL Status Code 읽기 (eSnetEcatGetAlStatusCode) 
		/// </summary>
		/// <param name="node_index">	: 읽어올 Slave(Node)의 시작 번호</param>
		/// <param name="count">		: 읽어올 Slave(Node) 수</param>
		/// <param name="status_code">	: Slave(Node)의 AL Status Code</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")</returns>
		public int EcatGetAlStatusCode(int node_index, int count, int[] status_code)
		{
			int returnCode = (int)eSnetApiReturnCode.Success;

			try
			{
				returnCode = eSnetEcatGetAlStatusCode(NetID, node_index, count, out status_code[0]);
				if (returnCode != (int)eSnetApiReturnCode.Success)
				{
					Debug.WriteLine("Failed to call 'eSnetEcatGetAlStatusCode'. error : " + returnCode,
													"SnetDevice.EcatGetAlStatusCode");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, "SnetDevice.EcatGetAlStatusCode");
				returnCode = (int)eSnetApiReturnCode.Exception;
			}

			return returnCode;
		}

        /// <summary>
        /// EtherCAT Master에 연결된 Slave(Node)의 ESC register 읽기
        /// </summary>
        /// <param name="node_index">	: 읽어올 Slave(Node)의 번호</param>
        /// <param name="address">		: 읽어올 ESC register의 주소</param>
        /// <param name="length">		: 읽어올 ESC register의 크기(byte 단위)</param>
        /// <param name="data">			: 읽어온 ESC register 값이 저장될 버퍼</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")</returns>
        public int EcatGetEscRegister(int node_index, int address, int length, ref int data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetEcatGetEscRegister(NetID, node_index, address, length, out int getData);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetEcatGetEscRegister'. error : " + returnCode,
                                                    "SnetDevice.EcatGetEscRegister");
                }
				else
				{
					data = getData;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.EcatGetEscRegister");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

		/// <summary>
		/// EtherCAT Master에 연결된 Slave(Node)의 ESC register 쓰기
		/// </summary>
		/// <param name="node_index">	: 쓸 Slave(Node)의 번호</param>
		/// <param name="address">		: 쓸 ESC register의 주소</param>
		/// <param name="length">		: 쓸 ESC register의 크기(byte 단위)</param>
		/// <param name="data">			: 쓸 ESC register 값</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")</returns>
		public int EcatSetEscRegister(int node_index, int address, int length, int data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetEcatSetEscRegister(NetID, node_index, address, length, data);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetEcatSetEscRegister'. error : " + returnCode,
                                                    "SnetDevice.EcatSetEscRegister");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.EcatSetEscRegister");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }


		/// <summary>
		/// EtherCAT Master에 연결된 Slave(Node)의 Object를 SDO로 읽기
		/// </summary>
		/// <param name="node_index">	: 읽어올 Slave(Node)의 번호</param>
		/// <param name="index">		: 읽어올 Object의 index</param>
		/// <param name="sub_index">	: 읽어올 Object의 sub-index</param>
		/// <param name="length"		: 읽어올 Object의 크기(byte 단위)></param>
		/// <param name="data">			: 읽어온 Object 값이 저장될 버퍼</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")</returns>
		public int EcatGetCoeSdo(int node_index, int index, int sub_index, int length, ref int data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetEcatGetCoeSdo(NetID, node_index, index, sub_index, length, out int getData);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetEcatGetCoeSdo'. error : " + returnCode,
                                                    "SnetDevice.EcatGetCoeSdo");
                }
                else
                {
                    data = getData;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.EcatGetCoeSdo");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

		/// <summary>
		/// EtherCAT Master에 연결된 Slave(Node)의 Object를 SDO로 쓰기
		/// </summary>
		/// <param name="node_index">	: 쓸 Slave(Node)의 번호</param>
		/// <param name="index">		: 쓸 Object의 index</param>
		/// <param name="sub_index">	: 쓸 Object의 sub-index</param>
		/// <param name="length"		: 쓸 Object의 크기(byte 단위)></param>
		/// <param name="data">			: 쓸 Object 값</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")</returns>
		public int EcatSetCoeSdo(int node_index, int index, int sub_index, int length, int data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
			{
                returnCode = eSnetEcatSetCoeSdo(NetID, node_index, index, sub_index, length, data);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetEcatSetCoeSdo'. error : " + returnCode,
                                                    "SnetDevice.EcatSetCoeSdo");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.EcatSetCoeSdo");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// EtherCAT Master에 연결된 Slave(Node)의 Object를 PDO로 읽기
        /// </summary>
        /// <param name="node_index">	: 읽어올 Slave(Node)의 번호</param>
        /// <param name="type">			: 읽어올 Object의 종류. 1: RxPDO(출력), 2: TxPDO(입력). eSnetEcatCoeObjectType 참고</param>
        /// <param name="index">		: 읽어올 Object의 index</param>
        /// <param name="sub_index">	: 읽어올 Object의 sub-index</param>
        /// <param name="data">			: 읽어온 Object 값이 저장될 버퍼</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")</returns>
        public int EcatGetCoePdo(int node_index, eSnetEcatCoeObjectType type, int index, int sub_index, ref int data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetEcatGetCoePdo(NetID, node_index, (int)type, index, sub_index, out int getData);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetEcatGetCoePdo'. error : " + returnCode,
                                                    "SnetDevice.EcatGetCoePdo");
                }
                else
                {
                    data = getData;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.EcatGetCoePdo");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

		/// <summary>
		/// EtherCAT Master에 연결된 Slave(Node)의 Object를 PDO로 쓰기
		/// </summary>
		/// <remarks>
		/// 데이터 출력이 가능한 Object 종류는 RxPDO만 가능
		/// </remarks>
		/// <param name="node_index">	: 쓸 Slave(Node)의 번호</param>
		/// <param name="index">		: 쓸 Object의 index</param>
		/// <param name="sub_index">	: 쓸 Object의 sub-index</param>
		/// <param name="data">			: 쓸 Object 값</param>
		/// <returns>					: (see enum "eSnetApiReturnCode")</returns>
		public int EcatSetCoePdo(int node_index, int index, int sub_index, int data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetEcatSetCoePdo(NetID, node_index, index, sub_index, data);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetEcatSetCoePdo'. error : " + returnCode,
                                                    "SnetDevice.EcatSetCoePdo");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.EcatSetCoePdo");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// EtherCAT Master에 연결된 Slave Io Input Node Index와 각 Node별 최대 Input 접점 수 읽기
        /// </summary>
        /// <param name="count_all_node">	: 전체 Slave(Node) 수												</param>
        /// <param name="count_io_node">	: Slave Motion Node 수												</param>
        /// <param name="node_index">		: Slave Io Node 수 만큼의 Node Index	
        ///										(ex) 두 번째, 네 번째 Node에 Io Slave가 있다면 node_inde[0]=1, node_index[1]=3
        ///										(ex) 32개 배열을 선언하고, 읽은 count 갯수만큼 배열값을 읽을 것	</param>
        ///	<param name="count_max_point">	: Slave Motion Node 수												</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")									</returns>
        public int EcatGetNodeIoInputInfo(ref int count_all_node, ref int count_io_node, ref int[] node_index, ref int[] count_max_point)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;
            int[] getNodeIndex = new int[32];
            int[] getCountMaxPoint = new int[32];

            try
            {
                returnCode = eSnetEcatGetNodeIoInputInfo(NetID, out int getCountAll, out int getCountIo, out getNodeIndex[0], out getCountMaxPoint[0]);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetEcatGetNodeIoInputInfo'. error : " + returnCode,
                                                    "SnetDevice.EcatGetNodeIoInputInfo");
                }
                else
                {
                    count_all_node = getCountAll;
                    count_io_node = getCountIo;

                    for (int i = 0; i < getCountAll; i++)
                    {
                        node_index[i] = getNodeIndex[i];
                        count_max_point[i] = getCountMaxPoint[i];
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.EcatGetNodeIoInputInfo");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// EtherCAT Master에 연결된 Slave Io Output Node Index와 각 Node별 최대 Output 접점 수 읽기
        /// </summary>
        /// <param name="count_all_node">	: 전체 Slave(Node) 수												</param>
        /// <param name="count_io_node">	: Slave Motion Node 수												</param>
        /// <param name="node_index">		: Slave Io Node 수 만큼의 Node Index	
        ///										(ex) 두 번째, 네 번째 Node에 Io Slave가 있다면 node_inde[0]=1, node_index[1]=3
        ///										(ex) 32개 배열을 선언하고, 읽은 count 갯수만큼 배열값을 읽을 것	</param>
		///	<param name="count_max_point">	: Slave Motion Node 수												</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")									</returns>
        public int EcatGetNodeIoOutputInfo(ref int count_all_node, ref int count_io_node, ref int[] node_index, ref int[] count_max_point)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;
            int[] getNodeIndex		= new int[32];
			int[] getCountMaxPoint	= new int[32];

            try
            {
                returnCode = eSnetEcatGetNodeIoOutputInfo(NetID, out int getCountAll, out int getCountIo, out getNodeIndex[0], out getCountMaxPoint[0]);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetEcatGetNodeIoOutputInfo'. error : " + returnCode,
                                                    "SnetDevice.EcatGetNodeIoOutputInfo");
                }
                else
                {
                    count_all_node = getCountAll;
                    count_io_node = getCountIo;

					for (int i = 0; i < getCountAll; i++)
					{
						node_index[i]		= getNodeIndex[i];
						count_max_point[i]	= getCountMaxPoint[i];
					}
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.EcatGetNodeIoOutputInfo");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// EtherCAT Master에 연결된 모든 IO Slave(Node)의 모든 Input 접점(all input bits)을 한번에 읽기
        /// </summary>
        /// <param name="count_node">		: Slave Io Node 수																</param>
        /// <param name="index_node">		: 배열, Slave Io Node 수 만큼의 Node Index	
        ///										(ex) 두 번째, 네 번째 Node에 Io Slave가 있다면 node_inde[0]=1, index_node[1]=3
        ///										(ex) 32개 배열을 선언하고, 읽은 count 갯수만큼 배열값을 읽을 것				</param>
        /// <param name="count_max_point">	: 배열, 각 node별 최대 접점(bit) 수										
        ///										(ex) count_max_point[2]는 세번째 Io Node의 최대 접점(max bit) 갯수를 표시	</param>
        /// <param name="data">				: 배열, 각 Io Node 별 input data
        ///										(ex) data[2]는 세번째 Io Node의 bit data값들을 표시							</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")												</returns>
        public int EcatGetNodeIoInputAll(ref int count_node, ref int[] index_node, ref int[] count_max_point, ref int[] data)
		{
            int returnCode = (int)eSnetApiReturnCode.Success;
            int[] getIndexNode		= new int[32];
            int[] getCountMaxPoint	= new int[32];
			int[] getData			= new int[32];

            try
            {
                returnCode = eSnetEcatGetNodeIoInputAll(NetID, out int getCountNode, out getIndexNode[0], out getCountMaxPoint[0], out getData[0]);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetEcatGetNodeIoInputAll'. error : " + returnCode,
                                                    "SnetDevice.EcatGetNodeIoInputAll");
                }
                else
                {
					count_node = getCountNode;

                    for (int i = 0; i < getCountNode; i++)
                    {
                        index_node[i]		= getIndexNode[i];
                        count_max_point[i]	= getCountMaxPoint[i];
						data[i]				= getData[i];
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.EcatGetNodeIoInputAll");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// EtherCAT Master에 연결된 모든 IO Slave(Node)의 모든 Output 접점(all output bits)을 한번에 읽기
        /// </summary>
        /// <param name="count_node">		: Slave Io Node 수																</param>
        /// <param name="index_node">		: 배열, Slave Io Node 수 만큼의 Node Index	
        ///										(ex) 두 번째, 네 번째 Node에 Io Slave가 있다면 node_inde[0]=1, index_node[1]=3
        ///										(ex) 32개 배열을 선언하고, 읽은 count 갯수만큼 배열값을 읽을 것				</param>
        /// <param name="count_max_point">	: 배열, 각 node별 최대 접점(bit) 수										
        ///										(ex) count_max_point[2]는 세번째 Io Node의 최대 접점(max bit) 갯수를 표시	</param>
        /// <param name="data">				: 배열, 각 Io Node 별 input data
        ///										(ex) data[2]는 세번째 Io Node의 bit data값들을 표시							</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")												</returns>
        public int EcatGetNodeIoOutputAll(ref int count_node, ref int[] index_node, ref int[] count_max_point, ref int[] data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;
            int[] getIndexNode = new int[32];
            int[] getCountMaxPoint = new int[32];
            int[] getData = new int[32];

            try
            {
                returnCode = eSnetEcatGetNodeIoOutputAll(NetID, out int getCountNode, out getIndexNode[0], out getCountMaxPoint[0], out getData[0]);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetEcatGetNodeIoOutputAll'. error : " + returnCode,
                                                    "SnetDevice.EcatGetNodeIoOutputAll");
                }
                else
                {
                    count_node = getCountNode;

                    for (int i = 0; i < getCountNode; i++)
                    {
                        index_node[i]		= getIndexNode[i];
                        count_max_point[i]	= getCountMaxPoint[i];
                        data[i]				= getData[i];
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.EcatGetNodeIoOutputAll");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// EtherCAT Master에 연결된 IO Slave(Node)의 Input 모든 bit을 한번에 읽기
        /// </summary>        
        /// <param name="index_node">		: 쓸 Slave(Node)의 번호				</param>
        /// <param name="count_max_point">	: Slave(Node)의 최대 접점(bit) 갯수	</param>
        /// <param name="data">				: io value							</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        public int EcatGetNodeIoInputPort(int index_node, ref int count_max_point, ref int data)
		{
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetEcatGetNodeIoInputPort(NetID, index_node, out int getCountMaxPoint, out int getData);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetEcatGetNodeIoInputPort'. error : " + returnCode,
                                                    "SnetDevice.EcatGetNodeIoInputPort");
                }
				else
				{
					count_max_point = getCountMaxPoint;
					data			= getData;
				}
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.EcatGetNodeIoInputPort");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// EtherCAT Master에 연결된 IO Slave(Node)의 Ouput 모든 bit을 한번에 읽기
        /// </summary>
        /// <param name="index_node">		: 쓸 Slave(Node)의 번호				</param>
        /// <param name="count_max_point">	: Slave(Node)의 최대 접점(bit) 갯수	</param>
        /// <param name="data">				: io value							</param>
        /// <returns>						: (see enum "eSnetApiReturnCode")	</returns>
        public int EcatGetNodeIoOutputPort(int index_node, ref int count_max_point, ref int data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;
			
            try
            {
                returnCode = eSnetEcatGetNodeIoOutputPort(NetID, index_node, out int getCountMaxPoint, out int getData);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetEcatGetNodeIoOutputPort'. error : " + returnCode,
                                                    "SnetDevice.EcatGetNodeIoOutputPort");
                }
				else
				{
					count_max_point = getCountMaxPoint;
					data			= getData;
				}
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.EcatGetNodeIoOutputPort");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// EtherCAT Master에 연결된 IO Slave(Node)의 Input 한 bit를 PDO로 읽기
        /// </summary>
        /// <param name="index_node">	: 쓸 Slave(Node)의 번호				</param>
        /// <param name="point">		: io bit number("0"~ )				</param>
        /// <param name="data">			: io value							</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
        public int EcatGetNodeIoInputPoint(int index_node, int point, ref bool data)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetEcatGetNodeIoInputPoint(NetID, index_node, point, out int getData);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetEcatGetNodeIoInputPoint'. error : " + returnCode,
                                                    "SnetDevice.EcatGetNodeIoInputPoint");
                }
                else
                {
                    data = (getData == 1) ? true : false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.EcatGetNodeIoInputPoint");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// EtherCAT Master에 연결된 IO Slave(Node)의 Output 한 bit를 PDO로 읽기
        /// </summary>
        /// <param name="index_node">	: 쓸 Slave(Node)의 번호	</param>
        /// <param name="point">		: io bit number("0"~ )	</param>
        /// <param name="data">			: io value				</param>
        /// <returns>					: (see enum "eSnetApiReturnCode")	</returns>
        public int EcatGetNodeIoOutputPoint(int index_node, int point, ref bool data)
		{
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
				returnCode = eSnetEcatGetNodeIoOutputPoint(NetID, index_node, point, out int getData);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetEcatGetNodeIoOutputPoint'. error : " + returnCode,
                                                    "SnetDevice.EcatGetNodeIoOutputPoint");
                }
                else
                {
                    data = (getData == 1) ? true : false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.EcatGetNodeIoOutputPoint");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        /// <summary>
        /// EtherCAT Master에 연결된 IO Slave(Node)의 Output 한 bit를 PDO로 쓰기
        /// </summary>
        /// <param name="index_node">	: 쓸 Slave(Node)의 번호</param>
        /// <param name="point">		: io bit number("0"~ ) </param>
        /// <param name="data">			: io value</param>
        /// <returns></returns>
        public int EcatSetNodeIoOutputPoint(int index_node, int point, bool data)
		{
            int returnCode = (int)eSnetApiReturnCode.Success;
			int setData = 0;

            try
            {
				setData = data ? 1 : 0;
                returnCode = eSnetEcatSetNodeIoOutputPoint(NetID, index_node, point, setData);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetEcatSetNodeIoOutputPoint'. error : " + returnCode,
                                                    "SnetDevice.EcatSetNodeIoOutputPoint");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.EcatSetNodeIoOutputPoint");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }

        #endregion

        #region CiA402

        /// <summary>
        /// CiA-402 에러 코드 읽기
        /// </summary>
        /// <param name="net">		: Network id(IPv4 4th address)</param>
        /// <param name="axis">		: 축 번호</param>
        /// <param name="error">	: 읽어서 저장할 에러 코드</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
        public int CiA402GetErrorCode(int axis, ref int error)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetCiA402GetErrorCode(NetID, axis, out int getError);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetCiA402GetErrorCode'. error : " + returnCode,
                                                    "SnetDevice.CiA402GetErrorCode");
                }
				else
				{
					error = getError;
				}
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.CiA402GetErrorCode");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;
        }
        #endregion

        #region Custom

        /// <summary>
        /// "서보 On 신호 출력 후" 설정 시간 동안 "서보 알람/ 서보 Ready" 신호 체크 지연
        /// </summary>
        /// <param name="net">		: Network id(IPv4 4th address)				</param>
        /// <param name="delay">	: 지연 시간, 단위: 1msec , 최대 5000(5초)		</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>
        public int SetDelayBeforeErrorCheck(int delay)
		{
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetSetDelayBeforeErrorCheck(NetID, delay);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetSetDelayBeforeErrorCheck'. error : " + returnCode,
                        "SnetDevice.SetDelayBeforeErrorCheck");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.SetDelayBeforeErrorCheck");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;

        }
        /// <summary>
        /// SetDelayBeforeErrorCheck() 설정 확인 
        /// </summary>
        /// <param name="net">		: Network id(IPv4 4th address)				</param>
        /// <param name="delay">	: 지연 시간, 단위: 1msec , 최대 5000(5초)		</param>
        /// <returns>				: (see enum "eSnetApiReturnCode")	</returns>

        public int GetDelayBeforeErrorCheck(ref int delay)
        {
            int returnCode = (int)eSnetApiReturnCode.Success;

            try
            {
                returnCode = eSnetGetDelayBeforeErrorCheck(NetID, out int getvalue);
                if (returnCode != (int)eSnetApiReturnCode.Success)
                {
                    Debug.WriteLine("Failed to call 'eSnetGetDelayBeforeErrorCheck'. error : " + returnCode,
                        "SnetDevice.GetDacConfig");
                }
                else
                {
                    delay = getvalue;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "SnetDevice.GetDelayBeforeErrorCheck");
                returnCode = (int)eSnetApiReturnCode.Exception;
            }

            return returnCode;

        }
        #endregion


        #endregion
    }
}