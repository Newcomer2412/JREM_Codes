using System.Net;
using static EMotionSnetBase.SnetDevice;
using static EMotionUzDevice.UzDevice;
using static FASTECH.EziMOTIONPlusELib;

namespace MachineControlBase
{
    public interface IOProcMgr
    {
        bool Init();

        void Free();

        void First_Output();

        void GetRepeatInput();

        bool GetMultiInput(int iModuleNo);

        bool GetSingleInput(int iBitNum, ref int iOnOff);

        bool GetSingleOutput(int iBitNum, ref int iOnOff);

        bool GetMultiOutput(int iModuleNo);

        void SetRepeatOutput();

        bool SetOutput(int iBitNum, int iOnOff);
    }

    /// <summary>
    /// RTX 모듈 외부 I/O 클래스
    /// </summary>
    public class RTX_IO_Module : IOProcMgr
    {
        /// <summary>
        /// Main Lib
        /// </summary>
        private CMainLib ml;

        /// <summary>
        /// RTX Class
        /// </summary>
        private eMotionTekRTEX rtex;

        public RTX_IO_Module(eMotionTekRTEX rtex)
        {
            ml = CMainLib.Ins;
            this.rtex = rtex;
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public bool Init()
        {
            return true;
        }

        /// <summary>
        /// 연결 해제
        /// </summary>
        public void Free()
        {
        }

        /// <summary>
        /// Get Repeat Intput IO
        /// </summary>
        public void GetRepeatInput()
        {
            int iModule = Define.INPUT_TOTAL_BIT / Define.INPUT_DEFINE_BIT;

            for (int i = 0; i < iModule; i++)
            {
                GetMultiInput(i);
            }
        }

        /// <summary>
        /// Read Multi Input IO
        /// </summary>
        /// <param name="iModuleNo"></param>
        /// <param name="iInputData"></param>
        /// <returns></returns>
        public bool GetMultiInput(int iModuleNo)
        {
            if (Define.SIMULATION == true) return true;
            bool bReturn = true;

            if (rtex.bConnected() == false) return false;

            int iPort = 0;
            int iInput = 0, iOutput = 0;
            iPort = iModuleNo / 2;
            int iStatus = eSnetGetRemoteIoPortInOut(rtex.iNet, iPort, out iInput, out iOutput);
            if (iModuleNo % 2 == 0) Seq_IO.IO_Input[iModuleNo] = (uint)(iInput & 0xFFFF);
            else Seq_IO.IO_Input[iModuleNo] = (uint)((iInput & 0xFFFF0000) >> 16);  // 이모션텍에 확인이 필요함. 부호있는 INT라 안됨.

            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == (int)eSnetApiReturnCode.Disconnected ||
                    iStatus == (int)eSnetApiReturnCode.TimeOut) rtex.bConnect = false;
                string strRtn = string.Format("GetMultiInput Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }

            return bReturn;
        }

        /// <summary>
        /// Read Single Input IO
        /// </summary>
        /// <param name="iBitNum"></param>
        /// <param name="iOnOff"></param>
        /// <returns></returns>
        public bool GetSingleInput(int iBitNum, ref int iOnOff)
        {
            if (Define.SIMULATION == true)
            {
                iOnOff = 1;
                return true;
            }
            bool bReturn = true;

            if (rtex.bConnect == false) return false;
            int iStatus = -1;
            int iPort = iBitNum / 32;
            int iPoint = iBitNum % 32;

            iStatus = eSnetGetRemoteIoInputPoint(rtex.iNet, iPort, iPoint, out iOnOff);

            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == (int)eSnetApiReturnCode.Disconnected ||
                    iStatus == (int)eSnetApiReturnCode.TimeOut) rtex.bConnect = false;
                string strRtn = string.Format("GetSingleInput Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }

            return bReturn;
        }

        /// <summary>
        /// Read Single Output IO
        /// </summary>
        /// <param name="iBitNum"></param>
        /// <param name="iOnOff"></param>
        /// <returns></returns>
        public bool GetSingleOutput(int iBitNum, ref int iOnOff)
        {
            if (Define.SIMULATION == true)
            {
                iOnOff = 1;
                return true;
            }
            bool bReturn = true;

            if (rtex.bConnect == false) return false;
            int iStatus = -1;
            int iPort = iBitNum / 8;
            int iPoint = iBitNum % 8;
            int iData = 0;

            iStatus = eSnetRtexGetIoOutputPort(rtex.iNet, iPort, out iData);
            if (((iData >> iPoint) & 0x1) == 0x1) iOnOff = 1;
            else iOnOff = 0;

            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == (int)eSnetApiReturnCode.Disconnected ||
                    iStatus == (int)eSnetApiReturnCode.TimeOut) rtex.bConnect = false;
                string strRtn = string.Format("GetSingleInput Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }

            return bReturn;
        }

        /// <summary>
        /// Get Multi Output
        /// </summary>
        /// <param name="iModuleNo"></param>
        /// <returns></returns>
        public bool GetMultiOutput(int iModuleNo)
        {
            if (Define.SIMULATION == true) return true;
            bool bReturn = true;
            if (rtex.bConnect == false) return false;

            int iStatus = -1;
            int iPort = 0;
            int iInput = 0, iOutput = 0;
            iPort = iModuleNo / 2;
            iStatus = eSnetGetRemoteIoPortInOut(rtex.iNet, iPort, out iInput, out iOutput);
            if (iModuleNo % 2 == 0) Seq_IO.IO_Output[iModuleNo] = (uint)(iOutput & 0xFFFF);
            else Seq_IO.IO_Output[iModuleNo] = (uint)((iOutput & 0xFFFF0000) >> 16);  // 이모션텍에 확인이 필요함. 부호있는 INT라 안됨.

            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == (int)eSnetApiReturnCode.Disconnected ||
                    iStatus == (int)eSnetApiReturnCode.TimeOut) rtex.bConnect = false;
                string strRtn = string.Format("GetMultiOutput Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }
            return bReturn;
        }

        /// <summary>
        /// 프로그램 초기 구동 Output를 읽어온다.
        /// </summary>
        public void First_Output()
        {
            int iModule = Define.OUTPUT_TOTAL_BIT / Define.OUTPUT_DEFINE_BIT;

            for (int i = 0; i < iModule; i++)
            {
                GetMultiOutput(i);
                Seq_IO.IO_Output_Old[i] = Seq_IO.IO_Output[i];
            }
        }

        /// <summary>
        /// Output Channel
        /// </summary>
        public void SetRepeatOutput()
        {
            int iModule = Define.OUTPUT_TOTAL_BIT / Define.OUTPUT_DEFINE_BIT;

            for (int i = 0; i < iModule; i++)
            {
                if (Seq_IO.IO_Output[i] != Seq_IO.IO_Output_Old[i])
                {
                    for (int j = 0; j < Define.OUTPUT_DEFINE_BIT; j++)
                    {
                        uint iBitData = (uint)(0x1 << j);
                        bool bOutput = (Seq_IO.IO_Output[i] & iBitData) == iBitData;
                        bool bOutput_Old = (Seq_IO.IO_Output_Old[i] & iBitData) == iBitData;
                        if (bOutput_Old == false && bOutput == true)
                        {
                            if (Define.SIMULATION == false)
                            {
                                if (SetOutput((i * Define.OUTPUT_DEFINE_BIT) + j, 1) == true) Seq_IO.IO_Output_Old[i] |= iBitData;
                            }
                            else
                            {
                                Seq_IO.IO_Output_Old[i] |= iBitData;
                            }
                        }
                        else if (bOutput_Old == true && bOutput == false)
                        {
                            if (Define.SIMULATION == false)
                            {
                                if (SetOutput((i * Define.OUTPUT_DEFINE_BIT) + j, 0) == true) Seq_IO.IO_Output_Old[i] &= ~iBitData;
                            }
                            else
                            {
                                Seq_IO.IO_Output_Old[i] &= ~iBitData;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Write Output IO
        /// </summary>
        /// <param name="iBitNum"></param>
        /// <param name="iOnOff"></param>
        /// <returns></returns>
        public bool SetOutput(int iBitNum, int iOnOff)
        {
            bool bReturn = true;
            if (rtex.bConnect == false) return false;
            int iStatus = -1;
            int iPort = iBitNum / 32;
            int iPoint = iBitNum % 32;

            iStatus = eSnetSetRemoteIoOutputPoint(rtex.iNet, iPort, iPoint, iOnOff);

            if (iStatus != (int)eSnetApiReturnCode.Success)
            {
                if (iStatus == (int)eSnetApiReturnCode.Disconnected ||
                    iStatus == (int)eSnetApiReturnCode.TimeOut) rtex.bConnect = false;
                string strRtn = string.Format("SetOutput Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }
            return bReturn;
        }
    }

    /// <summary>
    /// UZ 모듈 I/O 클래스
    /// </summary>
    public class UZ_IO_Module : IOProcMgr
    {
        /* Uz Type IO 보드 변수 */
        private int INPUT_INIT_START_NO = 1;    // Input 시작 IP국번
        private int INPUT_INIT_END_NO = 6;     // Input 마지막 IP국번
        private int OUTPUT_INIT_START_NO = 7;  // Output 시작 IP국번
        private int OUTPUT_INIT_END_NO = 10;    // Output 마지막 IP국번

        /// <summary>
        /// Main Lib
        /// </summary>
        private CMainLib ml;

        /// <summary>
        /// 생성자
        /// </summary>
        public UZ_IO_Module()
        {
            ml = CMainLib.Ins;
        }

        /// <summary>
        /// 초기화(통신 연결)
        /// </summary>
        public bool Init()
        {
            if (Define.SIMULATION == true) return true;

            for (int i = INPUT_INIT_START_NO; i <= INPUT_INIT_END_NO; i++)
            {
                int iStatus = eUzConnect(i, 10025);
                if (iStatus != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    if (CCommon.ShowMessage(0, $"이모션텍 Input 보드 '{i}' 연결 실패...\n프로그램을 종료합니다.") == (int)eMBoxRtn.A_OK)
                    {
                        NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("eMotionTek Input Uz{0} Connect Fail : RESULT NUM : {1}", i, iStatus));
                        return false;
                    }
                }
            }

            for (int i = OUTPUT_INIT_START_NO; i <= OUTPUT_INIT_END_NO; i++)
            {
                int iStatus = eUzConnect(i, 10025);
                if (iStatus != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    if (CCommon.ShowMessage(0, $"이모션텍 Output 보드 '{i}' 연결 실패...\n프로그램을 종료합니다.") == (int)eMBoxRtn.A_OK)
                    {
                        NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("eMotionTek Output Uz{0} Connect Fail : RESULT NUM : {1}", i, iStatus));
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 연결 해제
        /// </summary>
        public void Free()
        {
            if (Define.SIMULATION == true) return;

            for (int i = INPUT_INIT_START_NO; i <= INPUT_INIT_END_NO; i++)
            {
                if (bConnected(i) == true)
                {
                    int iStatus = eUzDisconnect(i);
                    if (iStatus != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                    {
                        NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                            string.Format("eMotionTek Uz{0} Disconnect Fail : RESULT NUM : {0}", i, ((eUzErrorCode)iStatus).ToString()));
                    }
                }
            }

            for (int i = OUTPUT_INIT_START_NO; i <= OUTPUT_INIT_END_NO; i++)
            {
                if (bConnected(i) == true)
                {
                    int iStatus = eUzDisconnect(i);
                    if (iStatus != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                    {
                        NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR,
                            string.Format("eMotionTek Uz{0} Disconnect Fail : RESULT NUM : {0}", i, ((eUzErrorCode)iStatus).ToString()));
                    }
                }
            }
        }

        /// <summary>
        /// 통신 제대로 되는지 체크
        /// </summary>
        /// <returns></returns>
        public bool bConnected(int iModuleNo)
        {
            bool bRtn = true;
            if (Define.SIMULATION == false &&
                eUzIsConnected(iModuleNo, out bRtn) != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
            {
                ml.AddError(eErrorCode.UZ_IO_NOTCONNECT);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("UzIO Net {0} Not Connection", iModuleNo));
            }
            return bRtn;
        }

        /// <summary>
        /// Get Repeat Input IO
        /// </summary>
        public void GetRepeatInput()
        {
            int iModule = INPUT_INIT_END_NO - INPUT_INIT_START_NO + 1;

            for (int i = 0; i < iModule; i++)
            {
                GetMultiInput(i + INPUT_INIT_START_NO);
            }
        }

        /// <summary>
        /// Read Multi Input IO
        /// </summary>
        /// <param name="iModuleNo"></param>
        /// <returns></returns>
        public bool GetMultiInput(int iModuleNo)
        {
            if (Define.SIMULATION == true) return true;
            bool bReturn = true;

            if (bConnected(iModuleNo) == false) return false;

            int iStatus = -1;
            int[] iData = new int[16];
            iStatus = eUzGetInput(iModuleNo, 0, 16, out iData[0]);
            uint uiDataTemp = 0;
            for (int i = iData.Length - 1; i >= 0; i--)
            {
                uiDataTemp += (uint)(iData[i] << i);
            }
            Seq_IO.IO_Input[iModuleNo - INPUT_INIT_START_NO] = uiDataTemp;

            if (iStatus != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
            {
                string strRtn = string.Format("GetMultiInput Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }

            return bReturn;
        }

        /// <summary>
        /// Read Single Input IO
        /// </summary>
        /// <param name="iBitNum"></param>
        /// <param name="iOnOff"></param>
        /// <returns></returns>
        public bool GetSingleInput(int iBitNum, ref int iOnOff)
        {
            if (Define.SIMULATION == true)
            {
                iOnOff = 1;
                return true;
            }
            bool bReturn = true;

            int iStatus = -1;
            int iNet = (iBitNum / 16) + INPUT_INIT_START_NO;
            int iPoint = iBitNum % 16;

            if (bConnected(iNet) == false) return false;

            iStatus = eUzGetInput(iNet, iPoint, 1, out iOnOff);

            if (iStatus != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
            {
                string strRtn = string.Format("GetSingleInput Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }

            return bReturn;
        }

        /// <summary>
        /// Get Multi Output
        /// </summary>
        /// <param name="iModuleNo"></param>
        /// <returns></returns>
        public bool GetMultiOutput(int iModuleNo)
        {
            if (Define.SIMULATION == true) return true;
            bool bReturn = true;

            if (bConnected(iModuleNo) == false) return false;

            int iStatus = -1;
            int[] iData = new int[16];
            iStatus = eUzGetOutput(iModuleNo, 0, 16, out iData[0]);
            for (int i = iData.Length - 1; i >= 0; i--)
            {
                Seq_IO.IO_Output[iModuleNo - OUTPUT_INIT_START_NO] += (uint)(iData[i] << i);
            }

            if (iStatus != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
            {
                string strRtn = string.Format("GetMultiInput Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }

            return bReturn;
        }

        /// <summary>
        /// Read Single Input IO
        /// </summary>
        /// <param name="iBitNum"></param>
        /// <param name="iOnOff"></param>
        /// <returns></returns>
        public bool GetSingleOutput(int iBitNum, ref int iOnOff)
        {
            if (Define.SIMULATION == true)
            {
                iOnOff = 1;
                return true;
            }
            bool bReturn = true;

            int iStatus = -1;
            int iNet = (iBitNum / 16) + INPUT_INIT_START_NO;
            int iPoint = iBitNum % 16;

            if (bConnected(iNet) == false) return false;

            iStatus = eUzGetOutput(iNet, iPoint, 1, out iOnOff);

            if (iStatus != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
            {
                string strRtn = string.Format("GetSingleInput Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }

            return bReturn;
        }

        /// <summary>
        /// 프로그램 초기 구동 Output를 읽어온다.
        /// </summary>
        public void First_Output()
        {
            int iModule = OUTPUT_INIT_END_NO - OUTPUT_INIT_START_NO + 1;

            for (int i = 0; i < iModule; i++)
            {
                GetMultiOutput(i + OUTPUT_INIT_START_NO);
            }
        }

        /// <summary>
        /// Output Channel
        /// </summary>
        public void SetRepeatOutput()
        {
            int iModule = OUTPUT_INIT_END_NO - OUTPUT_INIT_START_NO + 1;

            for (int i = 0; i < iModule; i++)
            {
                if (Seq_IO.IO_Output[i] != Seq_IO.IO_Output_Old[i])
                {
                    for (int j = 0; j < Define.OUTPUT_DEFINE_BIT; j++)
                    {
                        uint iBitData = (uint)(0x1 << j);
                        bool bOutput = (Seq_IO.IO_Output[i] & iBitData) == iBitData;
                        bool bOutput_Old = (Seq_IO.IO_Output_Old[i] & iBitData) == iBitData;
                        if (bOutput_Old == false && bOutput == true)
                        {
                            if (Define.SIMULATION == false)
                            {
                                if (SetOutput((i * Define.OUTPUT_DEFINE_BIT) + j, 1) == true) Seq_IO.IO_Output_Old[i] |= iBitData;
                            }
                            else
                            {
                                Seq_IO.IO_Output_Old[i] |= iBitData;
                            }
                        }
                        else if (bOutput_Old == true && bOutput == false)
                        {
                            if (Define.SIMULATION == false)
                            {
                                if (SetOutput((i * Define.OUTPUT_DEFINE_BIT) + j, 0) == true) Seq_IO.IO_Output_Old[i] &= ~iBitData;
                            }
                            else
                            {
                                Seq_IO.IO_Output_Old[i] &= ~iBitData;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Write Output IO
        /// </summary>
        /// <param name="iBitNum"></param>
        /// <param name="iOnOff"></param>
        /// <returns></returns>
        public bool SetOutput(int iBitNum, int iOnOff)
        {
            bool bReturn = true;
            int iStatus = -1;
            int iNet = (iBitNum / 16) + OUTPUT_INIT_START_NO;
            int iPoint = iBitNum % 16;

            if (bConnected(iNet) == false) return false;

            iStatus = eUzSetOutput(iNet, iPoint, 1, out iOnOff);

            if (iStatus != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
            {
                string strRtn = string.Format("SetOutput Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }
            return bReturn;
        }

        /// <summary>
        /// 트리거 Out Put Get
        /// </summary>
        /// <param name="iBitNum"></param>
        /// <param name="iCount"></param>
        /// <param name="iEnable"></param>
        /// <returns></returns>
        public bool GetOutputTriggerEnable(int iBitNum, int iCount, ref int[] iEnable)
        {
            if (Define.SIMULATION == true) return true;

            bool bReturn = true;
            int iStatus = -1;
            int iNet = (iBitNum / 16) + OUTPUT_INIT_START_NO;
            int iPoint = iBitNum % 16;

            if (bConnected(iNet) == false) return false;

            iStatus = eUzGetTriggerEnable(iNet, iPoint, iCount, out iEnable[0]);

            if (iStatus != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
            {
                string strRtn = string.Format("GetOutputTriggerEnable Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }
            return bReturn;
        }

        /// <summary>
        /// 트리거 Out Put Set
        /// </summary>
        /// <param name="iBitNum"></param>
        /// <param name="iCount"></param>
        /// <returns></returns>
        public bool SetOutputTriggerPeriod(int iBitNum, int iCount, int[] iMsTime)
        {
            if (Define.SIMULATION == true) return true;

            int[] Period = new int[4];
            for (int i = 0; i < 4; i++)
            {
                Period[i] = iMsTime[i] + 1;
            }

            bool bReturn = true;
            int iStatus = -1;
            int iNet = (iBitNum / 16) + OUTPUT_INIT_START_NO;
            int iPoint = iBitNum % 16;

            if (bConnected(iNet) == false) return false;

            iStatus = eUzSetTriggerPeriod(iNet, iPoint, iCount, out Period[0]);

            if (iStatus != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
            {
                string strRtn = string.Format("SetOutputTriggerForTime Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }
            return bReturn;
        }

        /// <summary>
        /// 트리거 Out Put Set
        /// </summary>
        /// <param name="iBitNum"></param>
        /// <param name="iCount"></param>
        /// <param name="iMsTime"></param>
        /// <returns></returns>
        public bool SetOutputTriggerForTime(int iBitNum, int iCount, int[] iMsTime)
        {
            if (Define.SIMULATION == true) return true;

            bool bReturn = true;
            int iStatus = -1;
            int iNet = (iBitNum / 16) + OUTPUT_INIT_START_NO;
            int iPoint = iBitNum % 16;

            if (bConnected(iNet) == false) return false;

            iStatus = eUzSetTriggerOnTime(iNet, iPoint, iCount, out iMsTime[0]);

            if (iStatus != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
            {
                string strRtn = string.Format("SetOutputTriggerForTime Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }
            return bReturn;
        }

        /// <summary>
        /// 사용할 트리거 Count Set
        /// </summary>
        /// <param name="iBitNum"></param>
        /// <param name="iCount"></param>
        /// <returns></returns>
        public bool SetTriggerCount(int iBitNum, int iCount)
        {
            if (Define.SIMULATION == true) return true;

            int[] iTriggerCount = new int[4] { 1, 1, 1, 1 };

            bool bReturn = true;
            int iStatus = -1;
            int iNet = (iBitNum / 16) + OUTPUT_INIT_START_NO;
            int iPoint = iBitNum % 16;

            if (bConnected(iNet) == false) return false;

            iStatus = eUzSetTriggerTotalCount(iNet, iPoint, iCount, out iTriggerCount[0]);

            if (iStatus != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
            {
                string strRtn = string.Format("SetTriggerCount Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }
            return bReturn;
        }

        /// <summary>
        /// 트리거 Out Put 시작
        /// </summary>
        /// <param name="iBitNum"></param>
        /// <param name="iCount"></param>
        /// <returns></returns>
        public bool StartOutputTrigger(int iBitNum, int iCount)
        {
            if (Define.SIMULATION == true) return true;

            bool bReturn = true;
            int iStatus = -1;
            int iNet = (iBitNum / 16) + OUTPUT_INIT_START_NO;
            int iPoint = iBitNum % 16;

            if (bConnected(iNet) == false) return false;

            iStatus = eUzStartTrigger(iNet, iPoint, iCount);

            if (iStatus != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
            {
                string strRtn = string.Format("StartOutputTrigger Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }
            return bReturn;
        }

        /// <summary>
        /// 트리거 Out Put 클리어
        /// </summary>
        /// <param name="iBitNum"></param>
        /// <param name="iCount"></param>
        /// <returns></returns>
        public bool ClearOutputTrigger(int iBitNum, int iCount)
        {
            if (Define.SIMULATION == true) return true;

            bool bReturn = true;
            int iStatus = -1;
            int iNet = (iBitNum / 16) + OUTPUT_INIT_START_NO;
            int iPoint = iBitNum % 16;

            if (bConnected(iNet) == false) return false;

            iStatus = eUzClearTriggerCount(iNet, iPoint, iCount);

            if (iStatus != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
            {
                string strRtn = string.Format("ClearOutputTrigger Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }
            return bReturn;
        }

        /// <summary>
        /// Set Latch
        /// </summary>
        /// <param name="iBitNum">설정할 Input 시작 번호</param>
        /// <param name="iCount">설정할 시작 번호부터의 개 수</param>
        /// <param name="bOnOff">0 : Rising, 1 : Falling</param>
        /// <returns></returns>
        public bool SetLatch(int iBitNum, int iCount, bool bOnOff = false)
        {
            if (Define.SIMULATION == true) return true;
            int[] iOnOff = new int[iCount];
            if (bOnOff == true)
            {
                for (int i = 0; i < iCount; i++)
                {
                    iOnOff[0] = 1;
                }
            }

            bool bReturn = true;
            int iStatus = -1;
            int iNet = (iBitNum / 16) + INPUT_INIT_START_NO;
            int iPoint = iBitNum % 16;

            if (bConnected(iNet) == false) return false;

            iStatus = eUzSetLatchSetting(iNet, iPoint, iCount, out iOnOff[0]);

            if (iStatus != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
            {
                string strRtn = string.Format("SetLatch Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }

            return bReturn;
        }

        /// <summary>
        /// GetLatch
        /// </summary>
        /// <param name="iBitNum">읽어올 시작 번호</param>
        /// <param name="iCount">시작 번호부터의 개 수</param>
        /// <param name="bWork">Latch 여부</param>
        /// <returns></returns>
        public bool GetLatch(int iBitNum, int iCount, ref bool[] bWork)
        {
            if (Define.SIMULATION == true)
            {
                for (int i = 0; i < bWork.Length; i++)
                {
                    bWork[i] = true;
                }
                return true;
            }

            int[] iOnOff = new int[iCount];
            bool bReturn = true;

            int iStatus = -1;
            int iNet = (iBitNum / 16) + INPUT_INIT_START_NO;
            int iPoint = iBitNum % 16;

            if (bConnected(iNet) == false) return false;

            iStatus = eUzGetLatch(iNet, iPoint, iCount, out iOnOff[0]);

            if (iStatus != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
            {
                string strRtn = string.Format("SetLatch Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }

            for (int i = 0; i < iCount; i++)
            {
                if (iOnOff[i] == 1) bWork[i] = true;
                else bWork[i] = false;
            }

            return bReturn;
        }

        /// <summary>
        /// LatchClear
        /// </summary>
        /// <param name="iBitNum">시작할 번호</param>
        /// <param name="iCount">시작 번호부터 걔수</param>
        /// <returns></returns>
        public bool LatchClear(int iBitNum, int iCount)
        {
            if (Define.SIMULATION == true) return true;

            bool bReturn = true;

            int iStatus = -1;
            int iNet = (iBitNum / 16) + INPUT_INIT_START_NO;
            int iPoint = iBitNum % 16;

            if (bConnected(iNet) == false) return false;

            iStatus = eUzClearLatchCount(iNet, iPoint, iCount);

            if (iStatus != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
            {
                string strRtn = string.Format("LatchClear Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }

            return bReturn;
        }
    }

    /// <summary>
    /// 파스텍 Ezi I/O 클래스
    /// </summary>
    public class Ezi_IO_Module : IOProcMgr
    {
        private int INPUT_INIT_START_NO = 2;    // InPut 시작 Bit
        private int INPUT_INIT_END_NO = 15;     // InPut 마지막 Bit
        private int OUTPUT_INIT_START_NO = 16;  // OutPut 시작 Bit
        private int OUTPUT_INIT_END_NO = 28;    // OutPut 마지막 Bit

        /// <summary>
        /// Main Lib
        /// </summary>
        private CMainLib ml;

        /// <summary>
        /// 생성자
        /// </summary>
        public Ezi_IO_Module()
        {
            ml = CMainLib.Ins;
        }

        /// <summary>
        /// 초기화(통신 연결)
        /// </summary>
        public bool Init()
        {
            if (Define.SIMULATION == true) return true;

            IPAddress ipEziIO = null;
            for (int i = INPUT_INIT_START_NO; i <= INPUT_INIT_END_NO; i++)
            {
                ipEziIO = IPAddress.Parse("192.168.0." + i);
                bool bStatus = FAS_Connect(ipEziIO, i);
                if (bStatus == false)
                {
                    if (CCommon.ShowMessage(0, $"파스텍 Input 보드 '{i}' 연결 실패...\n프로그램을 종료합니다.") == (int)eMBoxRtn.A_OK)
                    {
                        NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("FASTECH Input Ezi {0} Connect Fail : RESULT NUM : {1}", "192.168.0." + i, i));
                        return false;
                    }
                }
            }

            for (int i = OUTPUT_INIT_START_NO; i <= OUTPUT_INIT_END_NO; i++)
            {
                ipEziIO = IPAddress.Parse("192.168.0." + i);
                bool bStatus = FAS_Connect(ipEziIO, i);
                if (bStatus == false)
                {
                    if (CCommon.ShowMessage(0, $"파스텍 Output 보드 '{i}' 연결 실패...\n프로그램을 종료합니다.") == (int)eMBoxRtn.A_OK)
                    {
                        NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("FASTECH Output Ezi {0} Connect Fail : RESULT NUM : {1}", "192.168.0." + i, i));
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 연결 해제
        /// </summary>
        public void Free()
        {
            if (Define.SIMULATION == true) return;

            for (int i = INPUT_INIT_START_NO; i <= INPUT_INIT_END_NO; i++)
            {
                if (bConnected(i) == true)
                {
                    FAS_Close(i);
                }
            }

            for (int i = OUTPUT_INIT_START_NO; i <= OUTPUT_INIT_END_NO; i++)
            {
                if (bConnected(i) == true)
                {
                    FAS_Close(i);
                }
            }
        }

        /// <summary>
        /// 통신 제대로 되는지 체크
        /// </summary>
        /// <returns></returns>
        public bool bConnected(int iModuleNo)
        {
            IPAddress isIPEziIO = IPAddress.Parse("192.168.0." + iModuleNo);
            bool bRtn = FAS_IsIPAddressExist(isIPEziIO, ref iModuleNo);
            if (bRtn == false)
            {
                ml.AddError(eErrorCode.EZI_IO_NOTCONNECT);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, string.Format("Ezi IO Net {0} Not Connection", "192.168.0." + iModuleNo));
            }
            return bRtn;
        }

        /// <summary>
        /// Get Repeat Input IO
        /// </summary>
        public void GetRepeatInput()
        {
            int iModule = Define.INPUT_TOTAL_BIT / Define.INPUT_DEFINE_BIT;
            for (int i = 0; i < iModule; i++)
            {
                GetMultiInput(i);
            }
        }

        /// <summary>
        /// Read Multi Input IO
        /// </summary>
        /// <param name="iModuleNo"></param>
        /// <returns></returns>
        public bool GetMultiInput(int iModuleNo)
        {
            if (Define.SIMULATION == true) return true;
            bool bReturn = true;

            int iNet = iModuleNo / 2 + INPUT_INIT_START_NO;
            if (bConnected(iNet) == false) return false;

            int iStatus = -1;
            uint uInputData = 0; uint uLoatchData = 0;
            iStatus = FAS_GetInput(iNet, ref uInputData, ref uLoatchData);

            if (iModuleNo % 2 == 0)
                Seq_IO.IO_Input[iModuleNo] = 0x0000FFFF & uInputData;
            else
                Seq_IO.IO_Input[iModuleNo] = (0xFFFF0000 & uInputData) >> 16;

            if (iStatus != FMM_OK)
            {
                string strRtn = string.Format("GetMultiInput Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }

            return bReturn;
        }

        /// <summary>
        /// Read Single Input IO
        /// </summary>
        /// <param name="iBitNum"></param>
        /// <param name="iOnOff"></param>
        /// <returns></returns>
        public bool GetSingleInput(int iBitNum, ref int iOnOff)
        {
            if (Define.SIMULATION == true)
            {
                iOnOff = 1;
                return true;
            }
            bool bReturn = true;

            int iStatus = -1;
            int iNet = iBitNum / 32 + INPUT_INIT_START_NO;
            int iPoint = iBitNum % 32;

            if (bConnected(iNet) == false) return false;

            uint uInputData = 0; uint uLoatchData = 0;
            iStatus = FAS_GetInput(iNet, ref uInputData, ref uLoatchData);

            uint iBitData = (uint)(0x1 << iPoint);
            bool bInput = (uInputData & iBitData) == iBitData;

            if (bInput == true)
                iOnOff = 1;
            else
                iOnOff = 0;

            if (iStatus != FMM_OK)
            {
                string strRtn = string.Format("GetSingleInput Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }

            return bReturn;
        }

        /// <summary>
        /// Get Multi Output
        /// </summary>
        /// <param name="iModuleNo"></param>
        /// <returns></returns>
        public bool GetMultiOutput(int iModuleNo)
        {
            if (Define.SIMULATION == true) return true;
            bool bReturn = true;

            int iNet = (iModuleNo / 2) + OUTPUT_INIT_START_NO;
            if (bConnected(iNet) == false) return false;

            int iStatus = -1;
            uint uOutputData = 0; uint uLoatchData = 0;
            iStatus = FAS_GetOutput(iNet, ref uOutputData, ref uLoatchData);

            if (iModuleNo % 2 == 0)
                Seq_IO.IO_Output[iModuleNo] = 0x0000FFFF & uOutputData;
            else
                Seq_IO.IO_Output[iModuleNo] = (0xFFFF0000 & uOutputData) >> 16;

            if (iStatus != FMM_OK)
            {
                string strRtn = string.Format("GetMultiInput Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }

            return bReturn;
        }

        /// <summary>
        /// Read Single Input IO
        /// </summary>
        /// <param name="iBitNum"></param>
        /// <param name="iOnOff"></param>
        /// <returns></returns>
        public bool GetSingleOutput(int iBitNum, ref int iOnOff)
        {
            if (Define.SIMULATION == true)
            {
                iOnOff = 1;
                return true;
            }
            bool bReturn = true;

            int iStatus = -1;
            int iNet = (iBitNum / 32) + OUTPUT_INIT_START_NO;
            int iPoint = iBitNum % 32;

            if (bConnected(iNet) == false) return false;

            uint uOutputData = 0; uint uLoatchData = 0;
            iStatus = FAS_GetOutput(iNet, ref uOutputData, ref uLoatchData);
            uint iBitData = (uint)(0x1 << iPoint);
            bool bOutput = (uOutputData & iBitData) == iBitData;

            if (bOutput == true)
                iOnOff = 1;
            else
                iOnOff = 0;

            if (iStatus != FMM_OK)
            {
                string strRtn = string.Format("GetSingleInput Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }

            return bReturn;
        }

        /// <summary>
        /// 프로그램 초기 구동 Output를 읽어온다.
        /// </summary>
        public void First_Output()
        {
            int iModule = Define.OUTPUT_TOTAL_BIT / Define.OUTPUT_DEFINE_BIT;
            for (int i = 0; i < iModule; i++)
            {
                GetMultiOutput(i);
            }
        }

        /// <summary>
        /// Output Channel
        /// </summary>
        public void SetRepeatOutput()
        {
            int iOutputCount = Define.OUTPUT_TOTAL_BIT / Define.OUTPUT_DEFINE_BIT;

            for (int i = 0; i < iOutputCount; i++)
            {
                if (Seq_IO.IO_Output[i] != Seq_IO.IO_Output_Old[i])
                {
                    uint iBitData = 0;
                    if (Define.SIMULATION == false)
                    {
                        if (i % 2 == 0)
                        {
                            iBitData = Seq_IO.IO_Output[i] + (Seq_IO.IO_Output[i + 1] << 16);
                        }
                        else
                        {
                            iBitData = Seq_IO.IO_Output[i - 1] + (Seq_IO.IO_Output[i] << 16);
                        }

                        int iNet = (i / 2) + OUTPUT_INIT_START_NO;
                        uint iClearOutput = ~iBitData;
                        FAS_SetOutput(iNet, iBitData, iClearOutput);
                        Seq_IO.IO_Output_Old[i] = Seq_IO.IO_Output[i];
                    }
                }
            }
        }

        /// <summary>
        /// Write Output IO
        /// </summary>
        /// <param name="iBitNum"></param>
        /// <param name="iOnOff"></param>
        /// <returns></returns>
        public bool SetOutput(int iBitNum, int iOnOff)
        {
            bool bReturn = true;
            int iStatus = -1;
            int iNet = (iBitNum / 32) + OUTPUT_INIT_START_NO;

            if (bConnected(iNet) == false) return false;

            uint iOutputNo = 0;
            if (iNet > 0) iOutputNo = (uint)(0x1 << (iBitNum % 32));
            else iOutputNo = (uint)(0x1 << iBitNum);

            uint iClearOutput = ~iOutputNo;
            iStatus = FAS_SetOutput(iNet, iOutputNo, iClearOutput);

            if (iStatus != FMM_OK)
            {
                string strRtn = string.Format("SetOutput Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }
            return bReturn;
        }

        /// <summary>
        /// 트리거 Out Put Get
        /// </summary>
        /// <param name="iBitNum"></param>
        /// <param name="iCount"></param>
        /// <returns></returns>
        public bool GetOutputTrigger(int iBitNum, uint uCount)
        {
            if (Define.SIMULATION == true) return true;

            bool bReturn = true;
            int iStatus = -1;
            int iNet = (iBitNum / 32) + OUTPUT_INIT_START_NO;
            uint uPoint = (uint)iBitNum % 32;

            if (bConnected(iNet) == false) return false;

            iStatus = FAS_GetTriggerCount(iNet, uPoint, ref uCount);

            if (iStatus != FMM_OK)
            {
                string strRtn = string.Format("GetOutputTriggerEnable Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }
            return bReturn;
        }

        private TRIGGER_INFO TriggerInfo = null;

        /// <summary>
        /// 트리거 Out Put Set
        /// </summary>
        /// <param name="iBitNum"></param>
        /// <param name="iCount"></param>
        /// <returns></returns>
        public bool SetOutputTrigger(int iBitNum, int iCount, int iTime, int iPeriod)
        {
            if (Define.SIMULATION == true) return true;

            bool bReturn = true;
            int iStatus = -1;
            int iNet = (iBitNum / 32) + OUTPUT_INIT_START_NO;
            int iPoint = iBitNum % 32;

            if (bConnected(iNet) == false) return false;

            if (TriggerInfo == null) TriggerInfo = new TRIGGER_INFO();
            TriggerInfo.wCount = (uint)iCount;
            TriggerInfo.wOnTime = (ushort)iTime;
            TriggerInfo.wPeriod = (ushort)iPeriod;
            TriggerInfo.wReserved1 = 0;
            TriggerInfo.wReserved2 = 0;

            iStatus = FAS_SetTrigger(iNet, (byte)iPoint, TriggerInfo);

            if (iStatus != FMM_OK)
            {
                string strRtn = string.Format("SetOutputTriggerForTime Error : {0}", iStatus);
                NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.ERROR, strRtn);
                bReturn = false;
            }
            return bReturn;
        }
    }
}