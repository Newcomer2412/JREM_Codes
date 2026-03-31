using System.IO.Ports;
using System.Threading;

namespace MachineControlBase
{
    public class VisionCoworkLight
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
        /// 조명 On
        /// </summary>
        /// <param name="iCh"></param>
        /// <param name="iValue"></param>
        public void SetLightOn(uint uiCH, uint uiValue)
        {
            int iCH = 0x30 + (byte)uiCH;
            int iValue1 = 0; int iValue2 = 0; int iValue3 = 0;
            if (uiValue.ToString().Length == 3)
            {
                iValue1 = 0x31;
                iValue2 = 0x30;
                iValue3 = 0x30;
            }
            else if (uiValue.ToString().Length == 2)
            {
                iValue1 = 0x30;
                iValue2 = 0x30 + byte.Parse(uiValue.ToString().Substring(0, 1));
                iValue3 = 0x30 + byte.Parse(uiValue.ToString().Substring(1, 1));
            }
            else if (uiValue.ToString().Length == 1)
            {
                iValue1 = 0x30;
                iValue2 = 0x30;
                iValue3 = 0x30 + byte.Parse(uiValue.ToString().Substring(0, 1));
            }

            byte[] SendData = new byte[7]
            {
                0x4E,
                (byte)iCH,
                (byte)iValue1,
                (byte)iValue2,
                (byte)iValue3,
                0x0D,
                0x0A
            };

            SendCommand(SendData.Length, SendData);
            // Wait for Receive event or timeout
            if (hReceiveEvent.WaitOne(iReceiveTimeOut) != true)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Send Light On : No received");
            }
            hReceiveEvent.Reset();
        }

        /// <summary>
        /// 조명 Off
        /// </summary>
        /// <param name="iCh"></param>
        /// <param name="iValue"></param>
        public void SetLightOff(uint uiCH)
        {
            int iCH = 0x30 + (byte)uiCH;
            byte[] SendData = new byte[4]
            {
                0x45,
                (byte)iCH,
                0x0D,
                0x0A
            };

            SendCommand(SendData.Length, SendData);
            // Wait for Receive event or timeout
            if (hReceiveEvent.WaitOne(iReceiveTimeOut) != true)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Send Light Off : No received");
            }
            hReceiveEvent.Reset();
        }

        /// <summary>
        /// 조명 전체 On
        /// </summary>
        /// <param name="iCh"></param>
        /// <param name="iValue"></param>
        public void SetLightAllOn(uint uiValue)
        {
            int iValue1 = 0; int iValue2 = 0; int iValue3 = 0;
            if (uiValue.ToString().Length == 3)
            {
                iValue1 = 0x31;
                iValue2 = 0x30;
                iValue3 = 0x30;
            }
            else if (uiValue.ToString().Length == 2)
            {
                iValue1 = 0x30;
                iValue2 = 0x30 + byte.Parse(uiValue.ToString().Substring(0, 1));
                iValue3 = 0x30 + byte.Parse(uiValue.ToString().Substring(1, 1));
            }
            else if (uiValue.ToString().Length == 1)
            {
                iValue1 = 0x30;
                iValue2 = 0x30;
                iValue3 = 0x30 + byte.Parse(uiValue.ToString().Substring(0, 1));
            }

            byte[] SendData = new byte[7]
            {
                0x4E,
                0x41,
                (byte)iValue1,
                (byte)iValue2,
                (byte)iValue3,
                0x0D,
                0x0A
            };

            SendCommand(SendData.Length, SendData);
            // Wait for Receive event or timeout
            if (hReceiveEvent.WaitOne(iReceiveTimeOut) != true)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "Send Light All On : No received");
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

            if (recvBuf[0] == 0x06)
            {
                hReceiveEvent.Set();
            }
        }

        #endregion Command Send 루틴
    }
}