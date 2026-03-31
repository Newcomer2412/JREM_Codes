using EDP485;
using System.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// 이레텍 EDB2000 서보 및 BLDC 드라이버 프로토콜
    /// 드라이버에 스크립트 언어로 동작 명령이 정의 되어있어 프로토콜에선 구동 신호만 보냄
    /// </summary>
    public class EDPProcess
    {
        /// <summary>
        /// Read Lock
        /// </summary>
        private readonly object readLock = new object();

        /// <summary>
        /// 통신 포트
        /// </summary>
        private int _portNumber = 1;

        /// <summary>
        /// 통신 보레이트
        /// </summary>
        private int _baudRate = 115200;

        /// <summary>
        /// int형 리턴 값 통신 정상 응답
        /// </summary>
        public const int RETURN_OK = 100;

        /// <summary>
        /// 통신 연결 상태
        /// </summary>
        public bool bConnect = false;

        /// <summary>
        /// 통신 해제
        /// </summary>
        public void Free()
        {
            bool result = EDP.ClosePort(_portNumber);
        }

        /// <summary>
        /// 통신 연결
        /// </summary>
        /// <param name="iCom"></param>
        /// <returns></returns>
        public bool Connect(int iCom)
        {
            _portNumber = iCom;
            if (EDP.OpenPort(_portNumber, _baudRate) == false)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"EDP Driver {iCom} OpenPort Error");
                return false;
            }
            bConnect = true;
            return true;
        }

        /// <summary>
        /// 드라이버 초기화
        /// </summary>
        /// <param name="iDeviceId"></param>
        /// <returns></returns>
        public bool Init(int iDeviceId)
        {
            if (bConnect == false) return false;
            int value = 2;  // RPM, Nm
            if (EDP.WriteUnits(_portNumber, iDeviceId, value) != RETURN_OK)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"EDP Driver P : {_portNumber} D : {iDeviceId} WriteUnits Error");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Fault 상태 확인
        /// </summary>
        /// <param name="iDeviceId"></param>
        /// <returns></returns>
        public bool GetAlarm(int iDeviceId)
        {
            lock (readLock)
            {
                if (bConnect == false) return false;
                bool bRtnFault = false;
                if (EDP.ReadIsFault(_portNumber, iDeviceId, ref bRtnFault) != RETURN_OK)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"EDP Driver P : {_portNumber} D : {iDeviceId} ReadIsFault Error");
                    return false;
                }
                return bRtnFault;
            }
        }

        /// <summary>
        /// Motion moving finish check
        /// </summary>
        /// <param name="iDeviceId"></param>
        /// <returns></returns>
        public bool IsMoveDone(int iDeviceId)
        {
            lock (readLock)
            {
                if (bConnect == false) return false;
                bool bRtnMove = false;
                if (EDP.ReadIsMoving(_portNumber, iDeviceId, ref bRtnMove) != RETURN_OK)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"EDP Driver P : {_portNumber} D : {iDeviceId} ReadIsMoving Error");
                    return false;
                }
                return !bRtnMove;
            }
        }

        /// <summary>
        /// Get actual position
        /// </summary>
        /// <param name="iDeviceId"></param>
        /// <returns></returns>
        public bool GetActPostion(int iDeviceId, ref int iActPos)
        {
            lock (readLock)
            {
                if (bConnect == false) return false;
                if (EDP.ReadPositionActual(_portNumber, iDeviceId, ref iActPos) != RETURN_OK)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"EDP Driver P : {_portNumber} D : {iDeviceId} ReadPositionActual Error");
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 이송 명령 시 적용되는 속도 가감속 설정
        /// </summary>
        /// <param name="iDeviceId"></param>
        /// <param name="iAcc"></param>
        /// <param name="iDec"></param>
        /// <param name="iVel"></param>
        /// <returns></returns>
        public bool SetAccDecVel(int iDeviceId, int iAcc, int iDec, int iVel)
        {
            lock (readLock)
            {
                if (bConnect == false) return false;
                // 속도 설정
                if (EDP.WriteProfileVelocity(_portNumber, iDeviceId, iVel) != RETURN_OK)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"EDP Driver P : {_portNumber} D : {iDeviceId} WriteProfileVelocity Error");
                    return false;
                }
                // 가감속 설정
                if (EDP.WriteProfileAcceleration(_portNumber, iDeviceId, iAcc) != RETURN_OK)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"EDP Driver P : {_portNumber} D : {iDeviceId} WriteProfileAcceleration Error");
                    return false;
                }
                if (EDP.WriteProfileDeceleration(_portNumber, iDeviceId, iDec) != RETURN_OK)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"EDP Driver P : {_portNumber} D : {iDeviceId} WriteProfileDeceleration Error");
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 절대 이송 명령
        /// </summary>
        /// <param name="iDeviceId"></param>
        /// <param name="iPos"></param>
        /// <returns></returns>
        public bool MoveAbsolute(int iDeviceId, int iPos)
        {
            lock (readLock)
            {
                if (bConnect == false) return false;
                if (EDP.WriteTargetPosition(_portNumber, iDeviceId, iPos) != RETURN_OK)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"EDP Driver P : {_portNumber} D : {iDeviceId} WriteTargetPosition Error");
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 속도 이송 명령
        /// </summary>
        /// <param name="iDeviceId"></param>
        /// <param name="dVel"></param>
        /// <returns></returns>
        public bool MoveVelocity(int iDeviceId, double dVel)
        {
            lock (readLock)
            {
                if (bConnect == false) return false;
                if (EDP.WriteTargetVelocity(_portNumber, iDeviceId, dVel) != RETURN_OK)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"EDP Driver P : {_portNumber} D : {iDeviceId} WriteTargetVelocity Error");
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 정지 명령
        /// </summary>
        /// <param name="iDeviceId"></param>
        /// <returns></returns>
        public bool Stop(int iDeviceId)
        {
            lock (readLock)
            {
                if (bConnect == false) return false;
                if (EDP.Stop(_portNumber, iDeviceId) != RETURN_OK)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"EDP Driver P : {_portNumber} D : {iDeviceId} Stop Error");
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 알람 등 리셋
        /// </summary>
        /// <param name="iDeviceId"></param>
        /// <returns></returns>
        public bool Reset(int iDeviceId)
        {
            lock (readLock)
            {
                if (bConnect == false) return false;
                // 알람 리셋
                if (EDP.ClearFault(_portNumber, iDeviceId) != RETURN_OK)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"EDP Driver P : {_portNumber} D : {iDeviceId} ClearFault Error");
                    return false;
                }
                Thread.Sleep(100);
                // 모터 Enable
                if (EDP.SetEnable(_portNumber, iDeviceId) != RETURN_OK)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"EDP Driver P : {_portNumber} D : {iDeviceId} SetEnable Error");
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Negative 토크 설정
        /// </summary>
        /// <param name="iDeviceId"></param>
        /// <param name="dTorque"></param>
        public bool SetTorqueNegative(int iDeviceId, double dTorque)
        {
            lock (readLock)
            {
                if (bConnect == false) return false;
                if (EDP.WriteTorqueLimitNegative(_portNumber, iDeviceId, dTorque) != RETURN_OK)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"EDP Driver P : {_portNumber} D : {iDeviceId} SetTorqueNegative Error");
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Positive 토크 설정
        /// </summary>
        /// <param name="iDeviceId"></param>
        /// <param name="dTorque"></param>
        public bool SetTorquePositive(int iDeviceId, double dTorque)
        {
            lock (readLock)
            {
                if (bConnect == false) return false;
                if (EDP.WriteTorqueLimitPositive(_portNumber, iDeviceId, dTorque) != RETURN_OK)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"EDP Driver P : {_portNumber} D : {iDeviceId} SetTorquePositive Error");
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Negative 토크 읽음
        /// </summary>
        /// <param name="iDeviceId"></param>
        /// <param name="dTorque"></param>
        public bool GetTorqueNegative(int iDeviceId, ref double dTorque)
        {
            lock (readLock)
            {
                if (bConnect == false) return false;
                if (EDP.ReadTorqueLimitNegative(_portNumber, iDeviceId, ref dTorque) != RETURN_OK)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"EDP Driver P : {_portNumber} D : {iDeviceId} GetTorqueNegative Error");
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Positive 토크 읽음
        /// </summary>
        /// <param name="iDeviceId"></param>
        /// <param name="dTorque"></param>
        public bool GetTorquePositive(int iDeviceId, ref double dTorque)
        {
            lock (readLock)
            {
                if (bConnect == false) return false;
                if (EDP.ReadTorqueLimitPositive(_portNumber, iDeviceId, ref dTorque) != RETURN_OK)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"EDP Driver P : {_portNumber} D : {iDeviceId} GetTorquePositive Error");
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 토크 값을 읽음
        /// </summary>
        /// <param name="iDeviceId"></param>
        /// <param name="dTorque"></param>
        /// <returns></returns>
        public bool GetLoadTorque(int iDeviceId, ref double dTorque)
        {
            lock (readLock)
            {
                if (bConnect == false) return false;
                if (EDP.ReadLoadTorque(_portNumber, iDeviceId, ref dTorque) != RETURN_OK)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"EDP Driver P : {_portNumber} D : {iDeviceId} ReadLoadTorque Error");
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 포지션 초기화
        /// </summary>
        /// <param name="iDeviceId"></param>
        /// <returns></returns>
        public bool ResetPositive(int iDeviceId)
        {
            lock (readLock)
            {
                if (bConnect == false) return false;
                if (EDP.ResetPosition(_portNumber, iDeviceId) != RETURN_OK)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"EDP Driver P : {_portNumber} D : {iDeviceId} ResetPosition Error");
                    return false;
                }
                return true;
            }
        }
    }
}