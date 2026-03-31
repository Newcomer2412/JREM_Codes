using System.IO.Ports;

namespace MachineControlBase
{
    /// <summary>
    /// 하니웰 바코드 통신 프로토콜
    /// </summary>
    public class CHoneywellBarcode
    {
        /// <summary>
        /// 리시브 값 바코드 넘버
        /// </summary>
        public string recvNo = string.Empty;

        /// <summary>
        /// 바코드 리딩 상태
        /// </summary>
        public bool bReadDone = false;

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
            bool bRtn = cSerialProcess.OpenPort(strPort, 115200, StopBits.One, 8, Parity.None, ReceiveSreialPort);
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
        /// 바코드 트리거
        /// </summary>
        /// <param name="iCh"></param>
        /// <param name="iValue"></param>
        public void SetBarcodeTrigger(bool bOnOff)
        {
            byte[] writeCmd;
            if (bOnOff == true)       // SYN    T     CR
                writeCmd = new byte[3] { 0x16, 0x54, 0x0D };
            else                      // SYN    U     CR
                writeCmd = new byte[3] { 0x16, 0x55, 0x0D };
            bReadDone = false;
            SendCommand(writeCmd.Length, writeCmd);
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
            recvNo = string.Empty;
            cReceiveSerialPort?.Invoke(recvBuf);

            for (int i = 0; i <= recvBuf.Length; i++)
            {
                if (recvBuf[i] == 0) break;
                char GetNo = (char)recvBuf[i];
                recvNo += GetNo;
            }
            bReadDone = true;
        }

        #endregion Command Send 루틴
    }
}