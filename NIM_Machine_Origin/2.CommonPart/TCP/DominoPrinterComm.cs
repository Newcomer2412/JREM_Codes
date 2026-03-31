using System;
using System.Text;
using System.Threading;

namespace MachineControlBase
{
    /// <summary>
    /// 도미노 프린터 통신 프로토콜
    /// Standard non-printable Special printable
    /// <SOH> 1 (0x01) [ 91 (0x5B)
    /// <STX> 2 (0x02) # 35 (0x23)
    /// <ETX> 3 (0x03) | 124 (0x7C)
    /// <LF> 10 (0x0A) ; 59 (0x3B)
    /// <CR> 13 (0x0D) ] 93 (0x5D)
    /// <ETB> 23 (0x17) ^ 94 (0x5E)
    /// </summary>
    public class DominoPrinterComm
    {
        /// <summary>
        /// 클라이언트 통신
        /// </summary>
        public CTCPAsyncClient cTCPClient = null;

        /// <summary>
        /// 스레드 충돌 방지를 위한 Lock
        /// </summary>
        private object readLock = new object();

        /// <summary>
        /// 통신 이벤트
        /// </summary>
        private AutoResetEvent hEvent = new AutoResetEvent(false);

        /// <summary>
        /// 통신 응답 데이터
        /// </summary>
        private byte[] byteArrCommRtn = new byte[256];

        /// <summary>
        /// 도미노 프린터 번호
        /// </summary>
        private int iNo = 0;

        /// <summary>
        /// 로그 타입 설정
        /// </summary>
        private eLogType eLogType;

        /// <summary>
        /// 서버 IP 주소
        /// </summary>
        private string strIP = null;

        /// <summary>
        /// 통신 연결 상태
        /// </summary>
        public bool bConnect = false;

        /// <summary>
        /// 시작 비트 확인
        /// </summary>
        private bool bStartRead = false;

        /// <summary>
        /// 전달 받은 데이터 저장 위치
        /// </summary>
        private int iStepIndex = 0;

        /// <summary>
        /// 통신 응답 대기 시간
        /// </summary>
        private int iWaitTime = 1000;

        /// <summary>
        /// 생성자
        /// </summary>
        public DominoPrinterComm()
        {
            cTCPClient = new CTCPAsyncClient();
        }

        public void Free()
        {
            // 통신 종료
            if (cTCPClient != null)
            {
                cTCPClient.Disconnect();
                cTCPClient.CloseTCPClient();
            }
        }

        /// <summary>
        /// 통신을 연결합니다.
        /// </summary>
        /// <param name="iNo"></param>
        /// <param name="eLogType"></param>
        /// <param name="strIP"></param>
        /// <param name="uiPort"></param>
        public void Connect(int iNo, eLogType eLogType, string strIP, uint uiPort)
        {
            // TCP Client Start
            this.iNo = iNo;
            this.eLogType = eLogType;
            this.strIP = strIP;
            cTCPClient.SetLog(NLogger.GetLogClass(eLogType));
            if (cTCPClient.Connect(strIP, uiPort,
                                   bServerConnectStatus,
                                   null,
                                   OnVisionReceiveClient,
                                   false) == false)
            {
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, $"{strIP} Domino Printer Not Server Connect");
            }
        }

        /// <summary>
        /// Server와 연결 상태
        /// </summary>
        /// <param name="bStatus"></param>
        public void bServerConnectStatus(bool bStatus)
        {
            if (bStatus == true)
            {
                bConnect = true;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, $"{strIP} [Client] The client has connected to the Domino Printer server.", false);
            }
            else
            {
                if (CMainLib.Ins.McState == eMachineState.RUN) CMainLib.Ins.AddError(eErrorCode.UNKNOWN_ALARM + iNo);
                bConnect = false;
                NLogger.AddLog(eLogType, NLogger.eLogLevel.INFO, $"{strIP} [Client] The client has been disconnected from the the Domino Printer server.", false);
            }
        }

        /// <summary>
        /// 데이터 전달
        /// </summary>
        /// <param name="byteSendData"></param>
        /// <returns></returns>
        public bool SendData(byte[] byteSendData)
        {
            Array.Clear(byteArrCommRtn, 0x0, byteArrCommRtn.Length);
            return cTCPClient.SendData(byteSendData);
        }

        private static object ReceiveLock = new object();

        /// <summary>
        /// 서버에서 클라이언트로 보내온 데이터
        /// </summary>
        /// <param name="byteReceiveData"></param>
        private void OnVisionReceiveClient(byte[] byteReceiveData)
        {
            lock (ReceiveLock)
            {
                try
                {
                    for (int i = 0; i < byteReceiveData.Length; i++)
                    {
                        if (bStartRead == false &&
                            byteReceiveData[i] == 0x01)
                        {
                            iStepIndex = 0;
                            bStartRead = true;
                        }
                        if (bStartRead == true)
                        {
                            byteArrCommRtn[iStepIndex] = byteReceiveData[i];
                            iStepIndex++;
                        }
                        if (bStartRead == true &&
                            byteReceiveData[i] == 0x0D)
                        {
                            bStartRead = false;
                            hEvent.Set();
                        }
                    }
                }
                catch
                {
                    string strMsg = Encoding.Default.GetString(byteReceiveData, 0, byteReceiveData.Length);
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.FATAL, "Client receive exception : " + strMsg);
                }
            }
        }

        /// <summary>
        /// 응답 데이터를 검증한다.
        /// </summary>
        /// <param name="uiCRPoint"></param>
        /// <returns></returns>
        private bool CheckReceiveData(uint uiCRPoint)
        {
            bool byteRtn = true;
            // 시작 및 끝 비트 확인
            if (byteArrCommRtn[0] != 0x01 ||
                byteArrCommRtn[uiCRPoint] != 0x0D) byteRtn = false;

            if (byteRtn == false)
            {
                NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, $"Domino Printer : {strIP} No.{iNo} CheckReceiveData Error");
            }
            return byteRtn;
        }

        /// <summary>
        /// 프린트 시작 명령
        /// </summary>
        /// <param name="strPrintData"></param>
        /// <returns></returns>
        public bool PrintStart(string strPrintData)
        {
            lock (readLock)
            {
                if (Define.SIMULATION == true) return true;

                bool bRtn = true;
                if (bConnect == false) return false;

                byte[] bytePrintWrite = Encoding.Default.GetBytes(strPrintData);
                byte[] byteSendData = new byte[15 + bytePrintWrite.Length];

                byteSendData[0] = 0x02;                               // <STX>
                byteSendData[1] = (byte)'0';                          // Command 41 – Send dynamic data
                byteSendData[2] = (byte)'4';
                byteSendData[3] = (byte)'1';
                byteSendData[4] = (byte)'C';                          // Clear queue
                byteSendData[5] = (byte)'1';                          // 0 = no, 1 = yes.
                byteSendData[6] = (byte)'E';                          // Label layout storage number in PCU
                byteSendData[7] = (byte)'1';                          // number
                byteSendData[8] = (byte)'Q';                          // Quantity of labels to print. “0” will just add label to queue, external
                byteSendData[9] = (byte)'1';                          // print signal or separate command must order the print.Defaults to 0.
                byteSendData[10] = 0x17;                              // <ETB>
                byteSendData[11] = (byte)'D';                         // A Line Feed separated list of the dynamic text strings to populate the label with.
                for (int i = 12; i < bytePrintWrite.Length + 12; i++) // Data
                {
                    byteSendData[i] = bytePrintWrite[i - 12];
                }
                byteSendData[bytePrintWrite.Length + 12] = (byte)'?'; // Check Sum
                byteSendData[bytePrintWrite.Length + 13] = (byte)'?';
                byteSendData[bytePrintWrite.Length + 14] = 0x0D;      // <CR>

                SendData(byteSendData);
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(iWaitTime) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, $"Domino Printer : {strIP} No.{iNo} StartPrint TimeOut");
                    hEvent.Reset();
                    return false;
                }
                // 통신 받은 데이터 처리
                if (CheckReceiveData(7) == false)
                {
                    string strData = Encoding.Default.GetString(byteArrCommRtn, 1, 4);
                    if (strData != "0A41")
                    {
                        bRtn = false;
                    }
                }
                else
                {
                    bRtn = false;
                }
                hEvent.Reset();
                return bRtn;
            }
        }

        /// <summary>
        /// Sets the printer offline, or back online again
        /// </summary>
        /// <param name="bOnline"></param>
        /// <returns></returns>
        public bool SetOnOffline(bool bOnline)
        {
            lock (readLock)
            {
                if (Define.SIMULATION == true) return true;

                bool bRtn = true;
                if (bConnect == false) return false;

                byte byteOnline = bOnline == true ? (byte)1 : (byte)0;
                byte[] byteSendData = new byte[8];

                byteSendData[0] = 0x02;                               // <STX>
                byteSendData[1] = (byte)'0';                          // Command 27 – Set Offline/Online
                byteSendData[2] = (byte)'2';
                byteSendData[3] = (byte)'7';
                byteSendData[4] = byteOnline;                         // State to set; 0 = Offline, 1 = Online
                byteSendData[5] = (byte)'?';                          // Check Sum
                byteSendData[6] = (byte)'?';
                byteSendData[7] = 0x0D;                               // <CR>

                SendData(byteSendData);
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(iWaitTime) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, $"Domino Printer : {strIP} No.{iNo} SetOnOffline TimeOut");
                    hEvent.Reset();
                    return false;
                }
                // 통신 받은 데이터 처리
                if (CheckReceiveData(7) == false)
                {
                    string strData = Encoding.Default.GetString(byteArrCommRtn, 1, 4);
                    if (strData != "0A27")
                    {
                        bRtn = false;
                    }
                }
                else
                {
                    bRtn = false;
                }
                hEvent.Reset();
                return bRtn;
            }
        }

        /// <summary>
        /// System reset
        /// 시스템을 재설정하면 프린터에 저장된 모든 라벨이 삭제됩니다!
        /// 글꼴 및 그래픽 목록은 삭제되지 않으며 프린터 설정도 삭제되지 않습니다.
        /// </summary>
        /// <returns></returns>
        public bool SystemReset()
        {
            lock (readLock)
            {
                if (Define.SIMULATION == true) return true;

                bool bRtn = true;
                if (bConnect == false) return false;

                byte[] byteSendData = new byte[7];

                byteSendData[0] = 0x02;                               // <STX>
                byteSendData[1] = (byte)'0';                          // Command 01 – System reset
                byteSendData[2] = (byte)'0';
                byteSendData[3] = (byte)'1';
                byteSendData[4] = (byte)'?';                          // Check Sum
                byteSendData[5] = (byte)'?';
                byteSendData[6] = 0x0D;                               // <CR>

                SendData(byteSendData);
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(iWaitTime) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, $"Domino Printer : {strIP} No.{iNo} Systemreset TimeOut");
                    hEvent.Reset();
                    return false;
                }
                // 통신 받은 데이터 처리
                if (CheckReceiveData(7) == false)
                {
                    string strData = Encoding.Default.GetString(byteArrCommRtn, 1, 4);
                    if (strData != "0A01")
                    {
                        bRtn = false;
                    }
                }
                else
                {
                    bRtn = false;
                }
                hEvent.Reset();
                return bRtn;
            }
        }

        /// <summary>
        /// Reset printer
        /// 프린터와 도포기를 유휴 시작 위치로 다시 재설정합니다.
        /// 프린터 설정에 따라 라벨 대기열을 삭제할 수 있습니다.
        /// </summary>
        /// <returns></returns>
        public bool Reset()
        {
            lock (readLock)
            {
                if (Define.SIMULATION == true) return true;

                bool bRtn = true;
                if (bConnect == false) return false;

                byte[] byteSendData = new byte[7];

                byteSendData[0] = 0x02;                               // <STX>
                byteSendData[1] = (byte)'0';                          // Command 02 – Reset printer
                byteSendData[2] = (byte)'0';
                byteSendData[3] = (byte)'2';
                byteSendData[4] = (byte)'?';                          // Check Sum
                byteSendData[5] = (byte)'?';
                byteSendData[6] = 0x0D;                               // <CR>

                SendData(byteSendData);
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(iWaitTime) != true)
                {
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, $"Domino Printer : {strIP} No.{iNo} Reset TimeOut");
                    hEvent.Reset();
                    return false;
                }
                // 통신 받은 데이터 처리
                if (CheckReceiveData(7) == false)
                {
                    string strData = Encoding.Default.GetString(byteArrCommRtn, 1, 4);
                    if (strData != "0A02")
                    {
                        bRtn = false;
                    }
                }
                else
                {
                    bRtn = false;
                }
                hEvent.Reset();
                return bRtn;
            }
        }

        /// <summary>
        /// Status request
        /// </summary>
        /// <param name="bReady"></param>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        public bool StatusRequest(ref bool bReady, ref string strMsg)
        {
            lock (readLock)
            {
                if (Define.SIMULATION == true)
                {
                    bReady = true;
                    return true;
                }

                bool bRtn = true;
                if (bConnect == false) return false;

                byte[] byteSendData = new byte[7];

                byteSendData[0] = 0x02;                               // <STX>
                byteSendData[1] = (byte)'0';                          // Command 00 – Status request
                byteSendData[2] = (byte)'0';
                byteSendData[3] = (byte)'0';
                byteSendData[4] = (byte)'?';                          // Check Sum
                byteSendData[5] = (byte)'?';
                byteSendData[6] = 0x0D;                               // <CR>

                SendData(byteSendData);
                // Wait for Receive event or timeout
                if (hEvent.WaitOne(iWaitTime) != true)
                {
                    NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"Domino Printer : {strIP} No.{iNo} StatusRequest TimeOut");
                    NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, $"Domino Printer : {strIP} No.{iNo} StatusRequest TimeOut");
                    hEvent.Reset();
                    return false;
                }
                // 통신 받은 데이터 처리
                if (CheckReceiveData(10) == false)
                {
                    string strData = Encoding.Default.GetString(byteArrCommRtn, 1, 4);
                    if (strData == "0A00")
                    {
                        string strStatusNo = Encoding.Default.GetString(byteArrCommRtn, 5, 3);
                        int iStatusNo = 0;
                        int.TryParse(strStatusNo, out iStatusNo);
                        string strStatusInfo = Enum.GetName(typeof(PrinterStatus), iStatusNo);
                        if (strStatusNo == "999")
                        {
                            bReady = true;
                            strMsg = strStatusInfo;
                        }
                        else
                        {
                            bReady = false;
                            if (strStatusInfo != string.Empty) strMsg = strStatusInfo;
                            else strMsg = strStatusNo;
                            NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, $"Domino Printer : {strIP} No.{iNo} Error Status No:{strStatusNo}," +
                                                                                      $"Info : {strMsg}");
                            NLogger.AddLog(eLogType, NLogger.eLogLevel.ERROR, $"Domino Printer : {strIP} No.{iNo} Error Status No:{strStatusNo}," +
                                                                                      $"Info : {strMsg}");
                        }
                    }
                    else
                    {
                        bRtn = false;
                    }
                }
                else
                {
                    bRtn = false;
                }
                hEvent.Reset();
                return bRtn;
            }
        }

        /// <summary>
        /// 도미노 프린터 상태 값
        /// </summary>
        private enum PrinterStatus
        {
            Printhead_up = 801,
            Printhead_overheated = 802,
            Label_low = 803,
            Ribbon_low = 804,
            No_labels = 805,
            No_ribbon = 806,
            _5volt_to_printhead_is_missing = 807,
            _24_volt_to_printhead_is_missing = 808,
            _36_volt_to_printhead_is_missing = 809,
            Applicator_error = 810,
            Label_control = 811,
            Barcode_not_readable = 812,
            The_printhead_is_down_film_printer = 814,
            The_printhead_is_up_film_printer = 815,
            No_power_to_printer = 816,
            Door_control = 817,
            Emergency_control = 818,
            Pneumatic_Door = 821,
            No_Air_Pressure = 822,
            Tag_Not_Writable = 823,
            Special_for_Tetra_Pak = 824,
            ACC_Test_Mode = 825,
            Warning_Product_hit_control = 826,
            Error_Product_hit_control = 827,
            Apply_Timeout = 828,
            Part_Pallet_if_offline_setup_989 = 829,
            Bad_applicator_type = 830,
            Peel_roller_down = 831,
            Cover_off = 832,
            Waiting_for_label_positioning = 949,
            Dynamic_queue_empty = 950,
            Dynamic_Bitmap_queue_full = 951,
            Dynamic_queue_calc_bitmap_empty = 952,
            Offline_if_setup_989 = 989,
            Calculation_in_progress = 990,
            Label_printed = 991,
            Cylinder_out = 992,
            Get_label_in_PC = 993,
            Application_in_progress = 994,
            Reset_in_progress = 997,
            Printing_in_progress = 998,
            Ok_idle = 999,
        }
    }
}