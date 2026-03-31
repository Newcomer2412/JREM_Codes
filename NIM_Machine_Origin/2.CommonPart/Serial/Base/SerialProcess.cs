using System;
using System.Text;
using System.IO.Ports;
using NLog;

namespace MachineControlBase
{
    /// <summary>
    /// Com Port 로부터 값을 받을경우 Call Back을 받을 Function
    /// </summary>
    /// <param name="recvBuf"></param>
    public delegate void CallbackReceivedData(byte[] recvBuf);

    /// <summary>
    /// Serial 통신 Class
    /// </summary>
    public class CSerialProcess
    {
        /// <summary>
        /// Serail Port Object
        /// </summary>
        private SerialPort cSerialPort = null;

        /// <summary>
        /// Serial Log Class
        /// </summary>
        private Logger cFileDataLogClass = LogManager.GetLogger("SerialLog");

        #region Receive Count, Byte, String

        /// <summary>
        /// Receive Count
        /// </summary>
        private int iReceiveDataCount = 0;

        /// <summary>
        /// Receive Buffer
        /// </summary>
        private byte[] recvBuf = new byte[1024];

        /// <summary>
        /// Command byte를 Clear 한다.
        /// </summary>
        private void ClearRecvByte()
        {
            for (int i = 0; i < 1024; i++)
            {
                recvBuf[i] = 0x00;
            }
        }

        #endregion Receive Count, Byte, String

        #region Receive Data Call Back 을 위한 Delegate 함수

        /// <summary>
        /// Com Port로 부터 값을 받을경우 Call Back을 받을 Function
        /// </summary>
        private CallbackReceivedData callbackReceivedData = null;

        #endregion Receive Data Call Back 을 위한 Delegate 함수

        /// <summary>
        /// Com Port Open
        /// </summary>
        /// <param name="comPort"></param>
        /// <param name="baudRate"></param>
        /// <param name="stopBits"></param>
        /// <param name="dataBits"></param>
        /// <param name="partiy"></param>
        /// <param name="callbackReceivedData"></param>
        /// <returns></returns>
        public bool OpenPort(string comPort, int baudRate, StopBits stopBits, int dataBits, Parity partiy,
                             CallbackReceivedData callbackReceivedData)
        {
            try
            {
                if (cSerialPort != null && cSerialPort.IsOpen)  // 이미 Port가 열려 있으면 복귀한다.
                {
                    return true;
                }
                // Com Port Open
                cSerialPort = new SerialPort(comPort, baudRate);
                cSerialPort.Encoding = Encoding.ASCII;
                cSerialPort.Parity = partiy;
                cSerialPort.DataBits = dataBits;
                cSerialPort.StopBits = stopBits;
                //cSerialPort.RtsEnable = true;
                //cSerialPort.DtrEnable = true;  // 매우 중요
                //cSerialPort.Handshake = Handshake.None;
                cSerialPort.WriteTimeout = 50;
                cSerialPort.ReadTimeout = 3000;
                cSerialPort.DataReceived += new SerialDataReceivedEventHandler(ReceivedData);
                cSerialPort.Open();
                // 자료를 받으면 Call Back 할 Delegate 설정
                this.callbackReceivedData += callbackReceivedData;
                return cSerialPort.IsOpen;
            }
            catch (Exception ex)
            {
                if (cFileDataLogClass != null)
                {
                    cFileDataLogClass.Fatal(string.Format("Serial Connected Exception.\n{0}", ex.ToString()));
                }
                return false;
            }
        }

        /// <summary>
        /// Sreial Port Cloase
        /// </summary>
        public void ClosePort()
        {
            if (cSerialPort != null &&
                cSerialPort.IsOpen)
            {
                cSerialPort.Close();
            }
        }

        /// <summary>
        /// 연결 상태를 확인한다.
        /// </summary>
        /// <returns></returns>
        public bool bCheckConnect()
        {
            return cSerialPort.IsOpen;
        }

        /// <summary>
        /// Com Port로 부터 값이 날라오면 호출된다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReceivedData(object sender, SerialDataReceivedEventArgs e)
        {
            if (cSerialPort.BytesToRead > 0)
            {
                byte[] recvBufTemp = new byte[1024];
                int readCountTemp = cSerialPort.Read(recvBufTemp, 0, 1024);
                for (int i = 0; i < readCountTemp; i++) recvBuf[iReceiveDataCount++] = recvBufTemp[i];

                // Call Back 함수
                callbackReceivedData?.Invoke(recvBuf);
                if (cFileDataLogClass != null)
                {
                    cFileDataLogClass.Info(string.Format("Serial Recieve Data : {0}", recvBuf));
                }
                iReceiveDataCount = 0;
                ClearRecvByte();
            }
        }

        /// <summary>
        /// Serial Port에 Data를 Write 한다.
        /// </summary>
        /// <param name="strCommand"></param>
        public void WriteData(string strCommand)
        {
            cSerialPort.Write(strCommand);
            if (cFileDataLogClass != null)
            {
                cFileDataLogClass.Info(string.Format("Serial Write Data : {0}", strCommand));
            }
        }

        /// <summary>
        /// Serial Port에 Data를 Write 한다.
        /// </summary>
        /// <param name="iCount"></param>
        /// <param name="byteData"></param>
        public void WriteData(int iCount, byte[] byteData)
        {
            cSerialPort.Write(byteData, 0, iCount);
            if (cFileDataLogClass != null)
            {
                cFileDataLogClass.Info(string.Format("Serial Write Data : {0}", byteData));
            }
        }
    }
}