using FluentModbus;
using System;
using System.IO.Ports;

namespace MachineControlBase
{
    /// <summary>
    /// DH-Robotics 그리퍼 제어 클래스
    /// </summary>
    public class DHRobotics_Gripper
    {
        /// <summary>
        /// Modbus 통신 클래스
        /// </summary>
        private ModbusRtuClient master;

        /// <summary>
        /// Lock
        /// </summary>
        private readonly object oLock = new object();

        /// <summary>
        /// 통신 연결
        /// </summary>
        /// <param name="strCom"></param>
        public void Connect(string strCom)
        {
            if (Define.SIMULATION == true) return;
            // configure serial port
            master.BaudRate = 115200;
            master.Parity = Parity.None;
            master.StopBits = StopBits.One;
            master.Connect(strCom, ModbusEndianness.BigEndian);
        }

        /// <summary>
        /// 해제
        /// </summary>
        public void Free()
        {
            if (Define.SIMULATION == true) return;
            master.Close();
        }

        /// <summary>
        /// 그립퍼 초기화
        /// </summary>
        /// <param name="byteSlaveID"></param>
        /// <param name="bFullyInit"></param>
        public void Initialization(byte byteSlaveID, bool bFullyInit = false)
        {
            if (Define.SIMULATION == true) return;
            lock (oLock)
            {
                if (bFullyInit == false) master.WriteSingleRegister(byteSlaveID, 0x0100, 0x01);
                else master.WriteSingleRegister(byteSlaveID, 0x0100, 0xA5);
            }
        }

        /// <summary>
        /// 그립퍼 초기화 상태
        /// 0：Uninitialized (초기화 안됨)
        /// 1：Initialized (초기화 완료)
        /// 2：Initializing (초기화 중)
        /// </summary>
        /// <param name="byteSlaveID"></param>
        public ushort GetInitialization(byte byteSlaveID)
        {
            if (Define.SIMULATION == true) return 1;
            lock (oLock)
            {
                Span<ushort> usData = master.ReadHoldingRegisters<ushort>(byteSlaveID, 0x0200, 1);
                ushort usRtn = usData[0];
                return usRtn;
            }
        }

        /// <summary>
        /// 그리퍼 포스 설정
        /// </summary>
        /// <param name="byteSlaveID"></param>
        /// <param name="usForce">%</param>
        public void SetGripperForce(byte byteSlaveID, ushort usForce)
        {
            if (Define.SIMULATION == true) return;
            lock (oLock)
            {
                if (usForce > 100) usForce = 100;
                else if (usForce < 20) usForce = 20;
                master.WriteSingleRegister(byteSlaveID, 0x0101, usForce);
            }
        }

        /// <summary>
        /// 그리퍼 포스 읽기
        /// </summary>
        /// <param name="byteSlaveID"></param>
        public ushort GetGripperForce(byte byteSlaveID)
        {
            if (Define.SIMULATION == true) return 50;
            lock (oLock)
            {
                Span<ushort> usData = master.ReadHoldingRegisters<ushort>(byteSlaveID, 0x0101, 1);
                ushort usRtn = usData[0];
                return usRtn;
            }
        }

        /// <summary>
        /// 그리퍼 포지션 설정(단위는 퍼밀. 1000프로가 열림)
        /// </summary>
        /// <param name="byteSlaveID"></param>
        /// <param name="usPosition">1000‰</param>
        public void SetGripperPosition(byte byteSlaveID, ushort usPosition)
        {
            if (Define.SIMULATION == true) return;
            lock (oLock)
            {
                if (usPosition > 1000) usPosition = 1000;
                else if (usPosition < 0) usPosition = 0;
                master.WriteSingleRegister(byteSlaveID, 0x0103, usPosition);
            }
        }

        /// <summary>
        /// 그리퍼 포지션 읽기
        /// </summary>
        /// <param name="byteSlaveID"></param>
        public ushort GetGripperPosition(byte byteSlaveID)
        {
            if (Define.SIMULATION == true) return 0;
            lock (oLock)
            {
                Span<ushort> usData = master.ReadHoldingRegisters<ushort>(byteSlaveID, 0x0202, 1);
                ushort usRtn = usData[0];
                return usRtn;
            }
        }

        /// <summary>
        /// 그리퍼 속도 설정
        /// </summary>
        /// <param name="byteSlaveID"></param>
        /// <param name="usSpeed">%</param>
        public void SetGripperSpeed(byte byteSlaveID, ushort usSpeed)
        {
            if (Define.SIMULATION == true) return;
            lock (oLock)
            {
                if (usSpeed > 100) usSpeed = 100;
                else if (usSpeed < 1) usSpeed = 1;
                master.WriteSingleRegister(byteSlaveID, 0x0104, usSpeed);
            }
        }

        /// <summary>
        /// 그리퍼 속도 읽기
        /// </summary>
        /// <param name="byteSlaveID"></param>
        public ushort GetGripperSpeed(byte byteSlaveID)
        {
            if (Define.SIMULATION == true) return 100;
            lock (oLock)
            {
                Span<ushort> usData = master.ReadHoldingRegisters<ushort>(byteSlaveID, 0x0104, 1);
                ushort usRtn = usData[0];
                return usRtn;
            }
        }

        /// <summary>
        /// 그립퍼 동작 상태
        /// 0 : In motion (이송중)
        /// 1 : Reach position (위치에 도달)
        /// 2 : Object caught (물체 잡힘)
        /// 3 : Object dropped (물체 떨어짐)
        /// </summary>
        /// <param name="byteSlaveID"></param>
        public ushort GetGripperState(byte byteSlaveID)
        {
            if (Define.SIMULATION == true) return 1;
            lock (oLock)
            {
                Span<ushort> usData = master.ReadHoldingRegisters<ushort>(byteSlaveID, 0x0201, 1);
                ushort usRtn = usData[0];
                return usRtn;
            }
        }

        /// <summary>
        /// 로테이션 초기화
        /// </summary>
        /// <param name="byteSlaveID"></param>
        /// <param name="bMoveInit">True로 할 경우 360도 회전은 초기화하고 나머지 각도를 회전하여 0도로 초기화한다.
        ///                         False로 할 경우 360도로 회전은 초기화하고 나머지 각도로 각도를 설정한다.</param>
        public void RotationInitialization(byte byteSlaveID, bool bMoveInit = true)
        {
            if (Define.SIMULATION == true) return;
            lock (oLock)
            {
                if (bMoveInit == false) master.WriteSingleRegister(byteSlaveID, 0x0506, 0x01);
                else master.WriteSingleRegister(byteSlaveID, 0x0506, 0xA5);
            }
        }

        /// <summary>
        /// 로타리 앵글 설정 (각도 값, 기준 0도)
        /// </summary>
        /// <param name="byteSlaveID"></param>
        /// <param name="sAngle">Value</param>
        public void SetRotationAngle(byte byteSlaveID, short sAngle)
        {
            if (Define.SIMULATION == true) return;
            lock (oLock)
            {
                if (sAngle > 32767) sAngle = 32767;
                else if (sAngle < -32768) sAngle = -32768;
                master.WriteSingleRegister(byteSlaveID, 0x0105, (ushort)sAngle);
            }
        }

        /// <summary>
        /// 로타리 앵글 읽기
        /// </summary>
        /// <param name="byteSlaveID"></param>
        public short GetRotationAngle(byte byteSlaveID)
        {
            if (Define.SIMULATION == true) return 0;
            lock (oLock)
            {
                Span<short> sData = master.ReadHoldingRegisters<short>(byteSlaveID, 0x0208, 1);
                short sRtn = sData[0];
                return sRtn;
            }
        }

        /// <summary>
        /// 로타리 앵글 STOP
        /// </summary>
        /// <param name="byteSlaveID"></param>
        public void RotationStop(byte byteSlaveID, bool bOn)
        {
            if (Define.SIMULATION == true) return;
            lock (oLock)
            {
                if (bOn == true) master.WriteSingleRegister(byteSlaveID, 0x0502, 0x01);
                else master.WriteSingleRegister(byteSlaveID, 0x0502, 0x00);
            }
        }

        /// <summary>
        /// 로타리 스피드 설정
        /// </summary>
        /// <param name="byteSlaveID"></param>
        /// <param name="usSpeed">Value</param>
        public void SetRotationSpeed(byte byteSlaveID, ushort usSpeed)
        {
            if (Define.SIMULATION == true) return;
            lock (oLock)
            {
                if (usSpeed > 100) usSpeed = 100;
                else if (usSpeed < 1) usSpeed = 1;
                master.WriteSingleRegister(byteSlaveID, 0x0107, usSpeed);
            }
        }

        /// <summary>
        /// 로타리 스피드 읽기
        /// </summary>
        /// <param name="byteSlaveID"></param>
        public ushort GetRotationSpeed(byte byteSlaveID)
        {
            if (Define.SIMULATION == true) return 100;
            lock (oLock)
            {
                Span<ushort> usData = master.ReadHoldingRegisters<ushort>(byteSlaveID, 0x0107, 1);
                ushort usRtn = usData[0];
                return usRtn;
            }
        }

        /// <summary>
        /// 로타리 포스 설정
        /// </summary>
        /// <param name="byteSlaveID"></param>
        /// <param name="usForce">Value</param>
        public void SetRotationForce(byte byteSlaveID, ushort usForce)
        {
            if (Define.SIMULATION == true) return;
            lock (oLock)
            {
                if (usForce > 100) usForce = 100;
                else if (usForce < 20) usForce = 20;
                master.WriteSingleRegister(byteSlaveID, 0x0108, usForce);
            }
        }

        /// <summary>
        /// 로타리 포스 읽기
        /// </summary>
        /// <param name="byteSlaveID"></param>
        public ushort GetRotationForce(byte byteSlaveID)
        {
            if (Define.SIMULATION == true) return 50;
            lock (oLock)
            {
                Span<ushort> usData = master.ReadHoldingRegisters<ushort>(byteSlaveID, 0x0108, 1);
                ushort usRtn = usData[0];
                return usRtn;
            }
        }

        /// <summary>
        /// 로타리 초기화 상태 읽기
        /// 0 : Uninitialized (초기화 되지 않음)
        /// 1 : Initialized successfully (초기화 완료)
        /// 2 : Initializing (초기화 중)
        /// </summary>
        /// <param name="byteSlaveID"></param>
        public ushort GetRotatingInitializationState(byte byteSlaveID)
        {
            if (Define.SIMULATION == true) return 1;
            lock (oLock)
            {
                Span<ushort> usData = master.ReadHoldingRegisters<ushort>(byteSlaveID, 0x020A, 1);
                ushort usRtn = usData[0];
                return usRtn;
            }
        }

        /// <summary>
        /// 로타리 상태 읽기
        /// 0 : In motion(동작중)
        /// 1 : reaching the angle(위치에 도달)
        /// 2 : blocking(막힘)
        /// 3 : blocked during reaching the specified position.(지정된 위치에 도달하는 동안 막힘)
        /// </summary>
        /// <param name="byteSlaveID"></param>
        public ushort GetRotatingState(byte byteSlaveID)
        {
            if (Define.SIMULATION == true) return 1;
            lock (oLock)
            {
                Span<ushort> usData = master.ReadHoldingRegisters<ushort>(byteSlaveID, 0x020B, 1);
                ushort usRtn = usData[0];
                return usRtn;
            }
        }
    }
}