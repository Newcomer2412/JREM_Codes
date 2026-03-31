using System;

namespace MachineControlBase
{
    public class AjinADDACtrlLib
    {
        private static readonly object readLock = new object();
        private static AjinADDACtrlLib instance = null;

        public static AjinADDACtrlLib Ins
        {
            get
            {
                lock (readLock)
                {
                    if (instance == null) instance = new AjinADDACtrlLib();
                    return instance;
                }
            }
            set
            {
                instance = value;
            }
        }

        /// <summary>
        /// AD보드의 값을 취득하는 Value 값
        /// </summary>
        private double[] arrayValue = new double[100];

        #region DAQ 보드 Initial

        /// <summary>
        /// Device initialization
        /// </summary>
        public string InitDeviceInformation()
        {
            // Library initialization.
            if (CAXL.AxlOpen(7) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
            {
                uint uStatus = 1;
                // The inspection to have AIO module.
                if (CAXA.AxaInfoIsAIOModule(ref uStatus) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
                {
                    if ((AXT_EXISTENCE)uStatus == AXT_EXISTENCE.STATUS_EXIST)
                    {
                        int nModuleCounts = 0;
                        // Get the cardinality of the module.
                        if (CAXA.AxaInfoGetModuleCount(ref nModuleCounts) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
                        {
                            int iAdcChannelCounts = 0;
                            uint Code = CAXA.AxaiInfoGetChannelCount(ref iAdcChannelCounts);    // 아날로그 입력 개수 확인
                            Code = CAXA.AxaoInfoGetChannelCount(ref iAdcChannelCounts);         // 아날로그 출력 개수 확인
                        }
                        else return "Failed in geting the cardinality of the module.";
                    }
                    else return "Analog module does not exist.";
                }
                else return "AxaIsAIModule Error!!";
            }
            else return "Could not do the open the driver.";

            return "Analog Board Load OK.";
        }

        /// <summary>
        /// 아진 연결 종료
        /// </summary>
        public void AxlClose()
        {
            CAXL.AxlClose();
        }

        #endregion DAQ 보드 Initial

        #region DAQ 보드 Analog Input

        /// <summary>
        /// Input MIN ,MAX값 설정
        /// </summary>
        /// <param name="ich"></param>
        /// <param name="dMinVolt"></param>
        /// <param name="dMaxVolt"></param>
        public void SetInputMinMaxVolt(int ich, double dMinVolt, double dMaxVolt)
        {
            uint uiReturn = CAXA.AxaiSetRange(ich, dMinVolt, dMaxVolt);
        }

        /// <summary>
        /// Input MIN ,MAX값 취득
        /// </summary>
        /// <param name="ich"></param>
        /// <param name="dMinVolt"></param>
        /// <param name="dMaxVolt"></param>
        public void GetInputMinMaxVolt(int ich, double dMinVolt, double dMaxVolt)
        {
            uint uiReturn = CAXA.AxaiGetRange(ich, ref dMinVolt, ref dMaxVolt);
        }

        /// <summary>
        /// Input Multi MIN ,MAX값 설정
        /// </summary>
        /// <param name="iSize"></param>
        /// <param name="iChannelNo"></param>
        /// <param name="dMinVolt"></param>
        /// <param name="dMaxVolt"></param>
        public void SetMultiInputMinMaxVolt(int iSize, int[] iChannelNo, double dMinVolt, double dMaxVolt)
        {
            uint uiReturn = CAXA.AxaiSetMultiRange(iSize, iChannelNo, dMinVolt, dMaxVolt);
        }

        /// <summary>
        /// 특정 채널값을 얻어온다
        /// </summary>
        /// <param name="ich">읽을 채널값</param>
        /// <returns></returns>
        public double GetADValue(int ich)
        {
            if (ich < 0)
            {
                return 0.0;
            }

            const int maxCount = 100;
            for (int i = 0; i < maxCount; i++)
            {
                arrayValue[i] = 0.0;
            }

            double dVal = 0.0;
            double dSumVal = 0.0;

            // 1. 100 번을 읽어 와서 합과, 최대 최소값을 구한다.
            for (int i = 0; i < maxCount; i++)
            {
                CAXA.AxaiSwReadVoltage(ich, ref dVal);

                arrayValue[i] = dVal;
            }
            Array.Sort(arrayValue);

            // 2. 최대 값과 최소 값 10개씩을 제외하여 합 구함.
            for (int i = 10; i < 90; i++)
            {
                dSumVal += arrayValue[i];
            }

            // 3. 합 구한 값을 80으로 나눠서 평균을 구함.
            double result = Math.Round(dSumVal / 80.0, 12);
            return result;
        }

        /// <summary>
        /// 다이렉트로 값을 읽어옴
        /// </summary>
        /// <param name="ich"></param>
        /// <returns></returns>
        public double GetDirectADValue(int ich)
        {
            double dVal = 0.0;
            CAXA.AxaiSwReadVoltage(ich, ref dVal);
            return dVal;
        }

        #endregion DAQ 보드 Analog Input

        #region DAQ 보드 Analog Output

        /// <summary>
        /// Output MIN ,MAX값 설정
        /// </summary>
        /// <param name="ich"></param>
        /// <param name="dMinVolt"></param>
        /// <param name="dMaxVolt"></param>
        public void SetOutputMinMaxVolt(int ich, double dMinVolt, double dMaxVolt)
        {
            uint uiReturn = CAXA.AxaoSetRange(ich, dMinVolt, dMaxVolt);
        }

        /// <summary>
        /// Input MIN ,MAX값 취득
        /// </summary>
        /// <param name="ich"></param>
        /// <param name="dMinVolt"></param>
        /// <param name="dMaxVolt"></param>
        public void GetOutputMinMaxVolt(int ich, double dMinVolt, double dMaxVolt)
        {
            uint uiReturn = CAXA.AxaoGetRange(ich, ref dMinVolt, ref dMaxVolt);
        }

        /// <summary>
        /// Output Multi MIN ,MAX값 설정
        /// </summary>
        /// <param name="iSize"></param>
        /// <param name="iChannelNo"></param>
        /// <param name="dMinVolt"></param>
        /// <param name="dMaxVolt"></param>
        public void SetMultiOutputMinMaxVolt(int iSize, int[] iChannelNo, double dMinVolt, double dMaxVolt)
        {
            uint uiReturn = CAXA.AxaoSetMultiRange(iSize, iChannelNo, dMinVolt, dMaxVolt);
        }

        /// <summary>
        /// 출력 Voltage 셋팅
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="Volt"></param>
        public void SetVoltage(int ich, double dVolt)
        {
            CAXA.AxaoWriteVoltage(ich, dVolt);
        }

        /// <summary>
        /// 출력 Voltage 취득
        /// </summary>
        /// <param name="ich"></param>
        /// <returns></returns>
        public double GetVoltage(int ich)
        {
            double GetVolt = 0;
            CAXA.AxaoReadVoltage(ich, ref GetVolt);
            return GetVolt;
        }

        #endregion DAQ 보드 Analog Output
    }
}