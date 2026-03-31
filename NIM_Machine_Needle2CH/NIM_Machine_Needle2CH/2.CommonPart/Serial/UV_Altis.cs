using System;
using System.IO.Ports;
using System.Threading;

namespace MachineControlBase
{
    public class UV_Altis
    {
        /// <summary>
        /// 통신 응답 대기 이벤트
        /// </summary>
        private static AutoResetEvent hReceiveEvent = new AutoResetEvent(false);

        /// <summary>
        /// 통신 응답 타임 아웃
        /// </summary>
        private int iReceiveTimeOut = 2000;

        /// <summary>
        /// Serial Port를 제어 하기 위한 Control
        /// </summary>
        private CSerialProcess cSerialProcess = new CSerialProcess();

        /// <summary>
        /// 시리얼 Port를 Open하여 통신 가능 여부
        /// </summary>
        private bool bPortOpened = false;

        public bool _bPortOpend
        {
            get
            {
                if (cSerialProcess != null) bPortOpened = cSerialProcess.bCheckConnect();
                else bPortOpened = false;
                return bPortOpened;
            }
        }

        /// <summary>
        /// Com Port 로부터 값을 받을경우 Call Back을 받을 Function
        /// </summary>
        /// <param name="strRecvMsg"></param>
        public delegate void ReceiveSerialPort(byte[] strRecvMsg);

        /// <summary>
        /// Serial Port로 부터 Data를 받으면 호출되는 Call Back 함수 Degegate
        /// </summary>
        public ReceiveSerialPort cReceiveSerialPort = null;

        /// <summary>
        /// Serial 통신 Open
        /// </summary>
        /// <param name="strPort">Comp Port Name</param>
        /// <param name="cReceiveSerialPort">Data 수신시 호출될 Call Back 함수 Delegate</param>
        public bool Open(string strPort, ReceiveSerialPort cReceiveSerialPort)
        {
            this.cReceiveSerialPort = cReceiveSerialPort;
            bool bRtn = cSerialProcess.OpenPort(strPort, 9600, StopBits.One, 8, Parity.None, ReceiveSreialPort);
            return bRtn;
        }

        /// <summary>
        /// Serial Port Close
        /// </summary>
        public void Close()
        {
            cSerialProcess.ClosePort();
        }

        #region Command Send 루틴

        /// <summary>
        /// UV 커멘드 데이터 변수 선언
        /// </summary>
        private byte[] SendData = new byte[10] { 0x75, 0x76, 0x6C, 0x65, 0x64, 0x23, 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// UV 체크섬
        /// </summary>
        private byte[] CheckSum = new byte[6] { 0x3A, 0x24, 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// UV 체크섬
        /// </summary>
        private int iCheckSum = 0;

        /// <summary>
        /// 리시브 체크섬
        /// </summary>
        private int iRecvChecksum = 0;

        /// <summary>
        /// UV On
        /// </summary>
        public void UV_On()
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return;

            byte[] TurnOn = new byte[4] { 0x0E, 0x00, 0x00, 0x01 };
            for (int i = 6; i < SendData.Length; i++)
            {
                SendData[i] = TurnOn[i - 6];
                CheckSum[i - 4] = TurnOn[i - 6];
            }
            for (int i = 0; i < CheckSum.Length; i++)
            {
                iCheckSum += CheckSum[i];
            }

            SendCommand(SendData.Length, SendData);
            // Wait for Receive event or timeout
            if (hReceiveEvent.WaitOne(iReceiveTimeOut) != true)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Send UV On : No received");
                iCheckSum = iRecvChecksum = 0;
                return;
            }
            hReceiveEvent.Reset();

            SendData[7] = CheckSum[3] = 0x01; // 2번채널 설정

            for (int i = 0; i < CheckSum.Length; i++)
            {
                iCheckSum += CheckSum[i];
            }

            SendCommand(SendData.Length, SendData);
            // Wait for Receive event or timeout
            if (hReceiveEvent.WaitOne(iReceiveTimeOut) != true)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Send UV On : No received");
                iCheckSum = iRecvChecksum = 0;
                return;
            }
            hReceiveEvent.Reset();
        }

        /// <summary>
        /// UV Off
        /// </summary>
        /// <param name="iCh"></param>
        /// <param name="iValue"></param>
        public void UV_Off()
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return;

            byte[] TurnOff = new byte[4] { 0x0E, 0x00, 0x00, 0x00 };
            for (int i = 6; i < SendData.Length; i++)
            {
                SendData[i] = TurnOff[i - 6];
                CheckSum[i - 4] = TurnOff[i - 6];
            }
            for (int i = 0; i < CheckSum.Length; i++)
            {
                iCheckSum += CheckSum[i];
            }

            SendCommand(SendData.Length, SendData);
            // Wait for Receive event or timeout
            if (hReceiveEvent.WaitOne(iReceiveTimeOut) != true)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Send UV Off : No received");
                iCheckSum = iRecvChecksum = 0;
                return;
            }
            hReceiveEvent.Reset();

            SendData[7] = CheckSum[3] = 0x01; // 2번채널 설정

            for (int i = 0; i < CheckSum.Length; i++)
            {
                iCheckSum += CheckSum[i];
            }

            SendCommand(SendData.Length, SendData);
            // Wait for Receive event or timeout
            if (hReceiveEvent.WaitOne(iReceiveTimeOut) != true)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Send UV Off : No received");
                iCheckSum = iRecvChecksum = 0;
                return;
            }
            hReceiveEvent.Reset();
        }

        /// <summary>
        /// UV 파워
        /// </summary>
        /// <param name="iCh"></param>
        /// <param name="iValue"></param>
        public void UV_PowerSet(uint uiPower)
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return;

            byte[] PowerValue = new byte[4] { 0x00, 0x00, 0xFF, 0x00 };
            PowerValue[3] = (byte)uiPower; //byte.Parse(uiPower.ToString("X"));

            for (int i = 6; i < SendData.Length; i++)
            {
                SendData[i] = PowerValue[i - 6];
                CheckSum[i - 4] = PowerValue[i - 6];
            }
            for (int i = 0; i < CheckSum.Length; i++)
            {
                iCheckSum += CheckSum[i];
            }

            SendCommand(SendData.Length, SendData);
            // Wait for Receive event or timeout
            if (hReceiveEvent.WaitOne(iReceiveTimeOut) != true)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Send UV Powet Set : No received");
                iCheckSum = iRecvChecksum = 0;
                return;
            }
            hReceiveEvent.Reset();

            SendData[7] = CheckSum[3] = 0x01; // 2번채널 설정
            for (int i = 0; i < CheckSum.Length; i++)
            {
                iCheckSum += CheckSum[i];
            }
            SendCommand(SendData.Length, SendData);
            // Wait for Receive event or timeout
            if (hReceiveEvent.WaitOne(iReceiveTimeOut) != true)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Send UV Power Set : No received");
                iCheckSum = iRecvChecksum = 0;
                return;
            }
            hReceiveEvent.Reset();
        }

        /// <summary>
        /// UV 설정된 시간만큼 On
        /// </summary>
        /// <param name="iCh"></param>
        /// <param name="iValue"></param>
        public void UV_OnTime(uint uiTime)
        {
            if (Define.SIMULATION == true || CMainLib.Ins.cOptionData.bDryRunUse == true) return;

            byte[] OnTime = new byte[4] { 0x10, 0x00, 0x00, 0x00 };
            string strHex = uiTime.ToString("X");
            if (strHex.Length == 1 || strHex.Length == 2)
            {
                OnTime[3] = Convert.ToByte(strHex.Substring(0, 2), 16);
            }
            else if (strHex.Length == 3)
            {
                OnTime[2] = Convert.ToByte(strHex.Substring(0, 1), 16);
                OnTime[3] = Convert.ToByte(strHex.Substring(1, 2), 16);
            }

            for (int i = 6; i < SendData.Length; i++)
            {
                SendData[i] = OnTime[i - 6];
                CheckSum[i - 4] = OnTime[i - 6];
            }
            for (int i = 0; i < CheckSum.Length; i++)
            {
                iCheckSum += CheckSum[i];
            }

            SendCommand(SendData.Length, SendData);
            // Wait for Receive event or timeout
            if (hReceiveEvent.WaitOne(iReceiveTimeOut) != true)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Send UV On Time : No received");
                iCheckSum = iRecvChecksum = 0;
                return;
            }
            hReceiveEvent.Reset();

            SendData[7] = CheckSum[3] = 0x01; // 2번채널 설정
            for (int i = 0; i < CheckSum.Length; i++)
            {
                iCheckSum += CheckSum[i];
            }
            SendCommand(SendData.Length, SendData);
            // Wait for Receive event or timeout
            if (hReceiveEvent.WaitOne(iReceiveTimeOut) != true)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Send UV On Time : No received");
                iCheckSum = iRecvChecksum = 0;
                return;
            }
            hReceiveEvent.Reset();
        }

        /// <summary>
        /// Command를 보낸다.
        /// </summary>
        /// <param name="command"></param>
        public void SendCommand(string command)
        {
            cSerialProcess.WriteData(command);
        }

        /// <summary>
        /// Command를 보낸다.
        /// </summary>
        /// <param name="command"></param>
        public void SendCommand(int iCount, byte[] Data)
        {
            cSerialProcess.WriteData(iCount, Data);
        }

        /// <summary>
        /// Serial Port로 부터 Data를 받으면 호출되는 Call Back 함수
        /// </summary>
        /// <param name="recvBuf"></param>
        private void ReceiveSreialPort(byte[] recvBuf)
        {
            cReceiveSerialPort?.Invoke(recvBuf);

            for (int i = 0; i < 6; i++)
            {
                iRecvChecksum += recvBuf[i];
            }

            if (iCheckSum == iRecvChecksum)
            {
                iCheckSum = iRecvChecksum = 0;
                hReceiveEvent.Set();
            }
        }

        #endregion Command Send 루틴
    }
}