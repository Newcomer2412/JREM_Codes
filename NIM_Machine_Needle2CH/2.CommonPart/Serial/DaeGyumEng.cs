using System.IO.Ports;

namespace MachineControlBase
{
    /// <summary>
    /// 대겸 Eng 조명 컨트롤러 통신 프로토콜
    /// </summary>
    public class CDaeGyumEng
    {
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
        /// <param name="receiveSreialTextFromCompPort">Data 수신시 호출될 Call Back 함수 Delegate</param>
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
        /// Light 밝기 제어
        /// </summary>
        /// <param name="uiCh"></param>
        /// <param name="uiValue"></param>
        public void SetLightValue(uint uiCh, uint uiValue)
        {
            uiCh++;
            string writeCmd = "[" + uiCh.ToString("D2");
            writeCmd += uiValue.ToString("D3");
            SendCommand(writeCmd);
        }

        /// <summary>
        /// Light On/Off 제어
        /// </summary>
        /// <param name="uiCh"></param>
        /// <param name="bOnOff"></param>
        public void SetLightOnOff(uint uiCh, bool bOnOff)
        {
            uiCh++;
            string writeCmd = "]" + uiCh.ToString("D2");
            writeCmd += string.Format("{0}", bOnOff ? 1 : 0);
            SendCommand(writeCmd);
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
        /// Serial Port로 부터 Data를 받으면 호출되는 Call Back 함수
        /// </summary>
        /// <param name="recvBuf"></param>
        private void ReceiveSreialPort(byte[] recvBuf)
        {
            cReceiveSerialPort?.Invoke(recvBuf);
        }

        #endregion Command Send 루틴
    }
}