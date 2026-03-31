using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace EMotionUzDevice
{

    public class UzDevice
    {
        #region Define UZ API

        /*** Error code for EMOTION UZ API ***/
        public enum eUzErrorCode
        {
            UZ_ERROR_SUCCESS = 0,
            UZ_ERROR_NOT_CONNECTED,
            UZ_ERROR_ALREADY_CONNECTED,
            UZ_ERROR_DISCONNECTED,
            UZ_ERROR_FAILED_COMMUNICATION,
            UZ_ERROR_INVALID_COMMUNICATION_FORMAT,
            UZ_ERROR_INVALID_NETWORK_ID,
            UZ_ERROR_ID_INVALID_PORT_NO = 128,
            UZ_ERROR_COM_OPERATION_TIMEOUT = 129,
            UZ_ERROR_COM_NOT_OPERATION = 130,
            UZ_ERROR_COM_FAILED_OPERATION = 131,
            UZ_ERROR_COM_INVALID_ADDRESS = 132,
            UZ_ERROR_COM_INVALID_DATA_COUNT = 133,
            UZ_ERROR_COM_INVALID_DATA_VALUE = 134,
            UZ_ERROR_COM_FAILED_WRITE = 135,
            UZ_ERROR_COM_FAILED_READ = 136,
            UZ_ERROR_COM_INVALID_COMMAND = 137,
            UZ_ERROR_COM_INVALID_IOTYPE = 138,
            UZ_ERROR_COM_INIT_ERROR = 144,
            UZ_ERROR_COM_UDP_ERROR = 145,
            UZ_ERROR_COM_INVALID_IP_ADDRESS = 146,
            UZ_ERROR_COM_I2C_ERROR  = 147,
            UZ_ERROR_COM_SPI_ERROR = 148,
            UZ_ERROR_COM_BOARD_DETACHED = 149,
            UZ_ERROR_COM_NOT_CONFIGURATION = 160,
            UZ_ERROR_INVALID_ARGUMENT = 1000,
            UZ_ERROR_INVALID_COUNT,
            UZ_ERROR_INVALID_DIRECTORY,
            UZ_ERROR_FAILED_LOGIC,
            UZ_ERROR_DATA_OVERFLOW,
            UZ_ERROR_ARGUMENT_NULL_POINTER,
            UZ_ERROR_ENVIRONMENT,
            UZ_ERROR_SYSTEM = 10000
        }

        #region Property
        public int NetworkId { get; set; }

        public int Interval { get; set; }
        #endregion

        /*** Latch type for EMOTION UZ API ***/
        public enum eUzLatchType
        {
            UZ_LATCH_TYPE_RISING = 0,
            UZ_LATCH_TYPE_FALLING = 1,
        }


        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzConnect(int netId, int port);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzDisconnect(int netId);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzReconnect(int netId);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzIsConnected(int netId, [MarshalAs(UnmanagedType.I1)] out bool connected);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzCheckConnection(int netId);


        /*** Log ***/

        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzIsLoggable(int netId, [MarshalAs(UnmanagedType.I1)] out bool loggable);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzEnableLog(int netId, [MarshalAs(UnmanagedType.I1)] bool enable, StringBuilder logPath);


        /*** Infomation ***/

        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzGetOsVersion(int netId, out int version);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzGetApiVersion();
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzGetDeviceType(int netId, out int productType, out int hardwareType);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)] 
        public static extern int eUzGetDeviceTypeStr(int netId, [MarshalAs(UnmanagedType.BStr)] out string type);


        /*** Input ***/

        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzGetInput(int netId, int inputNo, int inputCount, out int data);

        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzGetInput2(int netId, out int data);

        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzGetInputMode(int netId, int inputNo, int inputCount, out int value);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzSetInputMode(int netId, int inputNo, int inputCount, out int value);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzGetInputSamplingTime(int netId, int inputNo, int inputCount, out int time);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzSetInputSamplingTime(int netId, int inputNo, int inputCount, out int time);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzGetLatch(int netId, int inputNo, int inputCount, out int data);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzClearLatch(int netId, int inputNo, int inputCount);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzGetLatchSetting(int netId, int inputNo, int inputCount, out int latchSetting);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzSetLatchSetting(int netId, int inputNo, int inputCount, out int latchSetting);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzGetLatchCount(int netId, int inputNo, int inputCount, out int latchCount);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzClearLatchCount(int netId, int inputNo, int inputCount);


        /*** Output ***/

        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzGetOutput(int netId, int outputNo, int outputCount, out int data);

        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzGetOutput2(int netId, out int data);

        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzSetOutput(int netId, int outputNo, int outputCount, out int data);

        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzSetOutput2(int netId, int data);

        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzGetOutputMode(int netId, int outputNo, int outputCount, out int value);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzSetOutputMode(int netId, int outputNo, int outputCount, out int value);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzGetTriggerPeriod(int netId, int outputNo, int outputCount, out int period);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzSetTriggerPeriod(int netId, int outputNo, int outputCount, out int period);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzGetTriggerOnTime(int netId, int outputNo, int outputCount, out int ontime);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzSetTriggerOnTime(int netId, int outputNo, int outputCount, out int ontime);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzGetTriggerTotalCount(int netId, int outputNo, int outputCount, out int totalCount);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzSetTriggerTotalCount(int netId, int outputNo, int outputCount, out int totalCount);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzGetTriggerEnable(int netId, int outputNo, int outputCount, out int enable);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzStartTrigger(int netId, int outputNo, int outputCount);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzStopTrigger(int netId, int outputNo, int outputCount);
        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzGetTriggerCount(int netId, int outputNo, int outputCount, out int count);

        [DllImport("EMotionUzDevice.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int eUzClearTriggerCount(int netId, int outputNo, int outputCount);

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public UzDevice()
        {
        }

        #region Method

        /// <summary>
        /// Connect to device
        /// </summary>
        /// <param name="netId">The device network ip4</param>
        /// <param name="port">The device network port</param>
        /// <returns>Error Code</returns>
        public int Connect(int netId, int port)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzConnect(netId, port);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.Connect:" +
                        "Failed to connect device" +
                        "(Network ID: " + netId.ToString() +
                        ", Port number: " + port.ToString() + ")");
                }
                this.NetworkId = netId;
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UzDevice.Connect:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Disconnect to device
        /// </summary>
        /// <returns>Error Code</returns>
        public int Disconnect()
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzDisconnect(this.NetworkId);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.Disconnect:" +
                        "Failed to disconnect device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")",
                        0);
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UzDevice.Disconnect:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Reconnect to device
        /// </summary>
        /// <returns>Error Code</returns>
        public int Reconnect()
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzReconnect(this.NetworkId);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.Reconnect:",
                    "Failed to reconnect to device" +
                    "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UzDevice.Reconnect:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Check whether the class is connected or disconnected to device
        /// </summary>
        /// <param name="connected">Whether the class is connected or disconnected to device</param>
        /// <returns>Error Code</returns>
        public int IsConnected(out bool connected)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzIsConnected(this.NetworkId, out connected);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.IsConnected:" +
                    "Failed to get connection from device" +
                    "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                connected = false;
                Debug.WriteLine("UzDevice.IsConnected:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Check connection to device
        /// </summary>
        /// <returns>Error Code</returns>
        public int CheckConnection()
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzCheckConnection(this.NetworkId);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.CheckConnection:" + 
                    "Failed to check connection to device" +
                    "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UzDevice.CheckConnection:"+ ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Get to be loggable form API
        /// </summary>
        /// <param name="loggable">Whether the API is loggable or not loggable</param>
        /// <returns></returns>
        public int IsLoggable(out bool loggable)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzIsLoggable(this.NetworkId, out loggable);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.IsLoggable:" +
                        "Failed to get to be loggable from device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                loggable = false;
                Debug.WriteLine("UzDevice.IsLoggable:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Enable to be loggable to API
        /// </summary>
        /// <param name="enable">Whether the API is loggable or not loggable</param>
        /// <param name="logPath">Log path</param>
        /// <returns></returns>
        public int EnableLog(bool enable, StringBuilder logPath)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzEnableLog(this.NetworkId, enable, logPath);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.EnableLog:" +
                        "Failed to enable to be loggable to device" +
                        "(Network ID: " + this.NetworkId.ToString() +
                        ", Path: " + logPath.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UzDevice.EnableLog:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Get OS version from device
        /// </summary>
        /// <param name="version">Os version</param>
        /// <returns>Error Code</returns>
        public int GetOsVersion(out int version)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzGetOsVersion(this.NetworkId, out version);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.GetOsVersion:",
                        "Failed to get os version from device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }

                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                version = 0;
                Debug.WriteLine("UzDevice.GetOsVersion:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Get API version
        /// </summary>
        /// <returns>Error Code</returns>
        public int GetApiVersion()
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzGetApiVersion();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UzDevice.GetApiVersion:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }


        /// <summary>
        /// Get device type
        /// </summary>
        /// <param name="productType">
        /// Bit 0 ~ 7   : Hardware minor version
        /// Bit 8 ~ 11  : Hardware major version 
        /// Bit 12 ~ 15 : Communication Type
        /// Bit 16 ~ 23 : Output count
        /// Bit 24 ~ 31 : Input count
        /// </param>
        /// <param name="hardwareType"></param>
        /// <returns></returns>
        public int GetDeviceType(out int productType, out int hardwareType)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzGetDeviceType(this.NetworkId, out productType, out hardwareType);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.GetDeviceType:" +
                        "Failed to get type from device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                productType = 0;
                hardwareType = 0;
                Debug.WriteLine("UzDevice.GetDeviceType:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Get device type
        /// </summary>
        /// <param name="productType">
        /// Bit 0 ~ 7   : Hardware minor version
        /// Bit 8 ~ 11  : Hardware major version 
        /// Bit 12 ~ 15 : Communication Type
        /// Bit 16 ~ 23 : Output count
        /// Bit 24 ~ 31 : Input count
        /// </param>
        /// <param name="hardwareType"></param>
        /// <returns></returns>
        public int GetDeviceTypeStr(out string type)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzGetDeviceTypeStr(this.NetworkId, out type);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.GetDeviceType:" +
                        "Failed to get type from device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                type = "Unknown";
                Debug.WriteLine("UzDevice.GetDeviceType:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Get input value
        /// </summary>
        /// <param name="portNo">The number of input to get</param>
        /// <param name="portCount">The count of input to get</param>
        /// <param name="data">The input value</param>
        /// <returns>Error Code</returns>
        public int GetInput(int inputNo, int inputCount, out int data)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzGetInput(this.NetworkId, inputNo, inputCount, out data);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.GetInput:" +
                        "Failed to get input value from device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                data = 0;
                Debug.WriteLine("UzDevice.GetInput:"+ ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Get input value
        /// </summary>
        /// <param name="data">The input value</param>
        /// <returns>Error Code</returns>
        public int GetInput(out int data)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzGetInput2(this.NetworkId, out data);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.GetInput2:" +
                        "Failed to get input value from device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                data = 0;
                Debug.WriteLine("UzDevice.GetInput2:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Get the input mode
        /// </summary>
        /// <param name="portNo">The number of input to get</param>
        /// <param name="portCount">The count of input to get</param>
        /// <param name="value">
        /// Whether the input mode is active high or active low
        /// 0 : Active high
        /// 1 : Active low
        /// </param>
        /// <returns>Error Code</returns>
        public int GetInputMode(int inputNo, int inputCount, out int mode)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzGetInputMode(this.NetworkId, inputNo, inputCount, out mode);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.GetInputMode:" +
                        "Failed to get latch setting from device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                mode = 0;
                Debug.WriteLine("UzDevice.GetInputMode:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Set the input mode(type)
        /// </summary>
        /// <param name="portNo">The number of input to set</param>
        /// <param name="portCount">The count of input to set</param>
        /// <param name="value">
        /// Whether the input mode is active high or active low
        /// 0 : Active High
        /// 1 : Active Low
        /// </param>
        /// <returns>Error Code</returns>
        public int SetInputMode(int inputNo, int inputCount, out int mode)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzSetInputMode(this.NetworkId, inputNo, inputCount, out mode);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.SetInputMode:" +
                        "Failed to set input mode to device" +
                        "(Network ID: " + this.NetworkId.ToString() +
                        ", Type: " + mode.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                mode = 0;
                Debug.WriteLine("UzDevice.SetInputMode:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Get sampling time about input
        /// </summary>
        /// <param name="portNo">The number of input to get</param>
        /// <param name="portCount">The count of input to get</param>
        /// <param name="time">The input sampling time</param>
        /// <returns>Error Code</returns>
        public int GetInputSamplingTime(int inputNo, int inputCount, out int time)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzGetInputSamplingTime(this.NetworkId, inputNo, inputCount, out time);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.GetInputSamplingTime:" +
                        "Failed to get input sampling count from device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }

                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                time = 0;
                Debug.WriteLine("UzDevice.GetInputSamplingTime:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Set sampling time about input
        /// </summary>
        /// <param name="portNo">The number of input to set</param>
        /// <param name="portCount">The count of input to set</param>
        /// <param name="time">The input sampling time</param>
        /// <returns>Error Code</returns>
        public int SetInputSamplingTime(int inputNo, int inputCount, out int time)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzSetInputSamplingTime(this.NetworkId, inputNo, inputCount, out time);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.SetInputSamplingTime:" +
                        "Failed to set input sampling time to device" +
                        "(Network ID: " + this.NetworkId.ToString() +
                        ", Time: " + time.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                time = 0;
                Debug.WriteLine("UzDevice.SetInputSamplingTime:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Get the latch is occured
        /// </summary>
        /// <param name="portNo">The number of input to get</param>
        /// <param name="portCount">The count of input to get</param>
        /// <param name="data">Whether the latch is occured or not occured</param>
        /// <returns>Error Code</returns>
        public int GetLatch(int inputNo, int inputCount, out int latched)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzGetLatch(this.NetworkId, inputNo, inputCount, out latched);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.GetLatch:" +
                        "Failed to get latch from device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                latched = 0;
                Debug.WriteLine("UzDevice.GetLatch:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Clear the latch is occured
        /// </summary>
        /// <param name="portNo">The number of input to clear</param>
        /// <param name="portCount">The count of input to clear</param>
        /// <returns>Error Code</returns>
        public int ClearLatch(int inputNo, int inputCount)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzClearLatch(this.NetworkId, inputNo, inputCount);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.ClearLatch:",
                        "Failed to clear being latched to device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UzDevice.ClearLatch:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Get the latch setting(type)
        /// </summary>
        /// <param name="portNo">The number of input to get</param>
        /// <param name="portCount">The count of input to get</param>
        /// <param name="value">
        /// Whether the latch type is rising or falling
        /// 0 : Rising
        /// 1 : Falling
        /// </param>
        /// <returns>Error Code</returns>
        public int GetLatchSetting(int inputNo, int inputCount, out int latchSetting)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzGetLatchSetting(this.NetworkId, inputNo, inputCount, out latchSetting);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.GetLatchSetting:" +
                        "Failed to get latch setting from device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                latchSetting = 0;
                Debug.WriteLine("UzDevice.GetLatchSetting:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Set the latch setting(type)
        /// </summary>
        /// <param name="portNo">The number of input to set</param>
        /// <param name="portCount">The count of input to set</param>
        /// <param name="value">
        /// Whether the latch type is rising or falling
        /// 0 : Rising
        /// 1 : Falling
        /// </param>
        /// <returns>Error Code</returns>
        public int SetLatchSetting(int inputNo, int inputCount, out int latchSetting)
        {
            int returnCode = 0;


            try
            {
                returnCode = eUzSetLatchSetting(this.NetworkId, inputNo, inputCount, out latchSetting);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.SetLatchSetting:" +
                        "Failed to set latch setting to device" +
                        "(Network ID: " + this.NetworkId.ToString() +
                        ", Type: " + latchSetting.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                latchSetting = 0;
                Debug.WriteLine("UzDevice.SetLatchSetting:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Get the count of occuring latch
        /// </summary>
        /// <param name="portNo">The number of input to get</param>
        /// <param name="portCount">The count of input to get</param>
        /// <param name="latchCount">The count of occuring latch</param>
        /// <returns>Error Code</returns>
        public int GetLatchCount(int inputNo, int inputCount, out int latchCount)
        {
            int returnCode = 0;


            try
            {
                returnCode = eUzGetLatchCount(this.NetworkId, inputNo, inputCount, out latchCount);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.GetLatchCount:" +
                        "Failed to get latch count from device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")",
                        0);
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                latchCount = 0;
                Debug.WriteLine("UzDevice.GetLatchCount:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Clear the latch count
        /// </summary>
        /// <param name="portNo">The number of input to clear</param>
        /// <param name="portCount">The count of input to clear</param>
        /// <returns>Error Code</returns>
        public int ClearLatchCount(int inputNo, int inputCount)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzClearLatchCount(this.NetworkId, inputNo, inputCount);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.ClearLatchCount:" +
                        "Failed to clear latch count to device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }

                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UzDevice.ClearLatchCount:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Get output value
        /// </summary>
        /// <param name="portNo">The number of output to get</param>
        /// <param name="portCount">The count of output to get</param>
        /// <param name="data">The output value</param>
        /// <returns>Error Code</returns>
        public int GetOutput(int outputNo, int outputCount, out int data)
        {
            int returnCode = 0;


            try
            {
                returnCode = eUzGetOutput(this.NetworkId, outputNo, outputCount, out data);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.GetOutput:" +
                        "Failed to get output value from device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                data = 0;
                Debug.WriteLine("UzDevice.GetOutput:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Get output value
        /// </summary>
        /// <param name="data">The output value</param>
        /// <returns>Error Code</returns>
        public int GetOutput(out int data)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzGetOutput2(this.NetworkId, out data);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.GetOutput2:" +
                        "Failed to get output value from device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                data = 0;
                Debug.WriteLine("UzDevice.GetOutput:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Set output value
        /// </summary>
        /// <param name="portNo">The number of output to set</param>
        /// <param name="portCount">The count of output to set</param>
        /// <param name="data">The value of output to set</param>
        /// <returns>Error Code</returns>
        public int SetOutput(int outputNo, int outputCount, out int data)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzSetOutput(this.NetworkId, outputNo, outputCount, out data);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.SetOutput:" +
                        "Failed to set output to device" +
                        "(Network ID: " + this.NetworkId.ToString() +
                        ", Value: " + data.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                data = 0;
                Debug.WriteLine("UzDevice.SetOutput:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Set output value
        /// </summary>
        /// <param name="data">The value of output to set</param>
        /// <returns>Error Code</returns>
        public int SetOutput2(int data)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzSetOutput2(this.NetworkId, data);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.SetOutput2:" +
                        "Failed to set output to device" +
                        "(Network ID: " + this.NetworkId.ToString() +
                        ", Value: " + data.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                data = 0;
                Debug.WriteLine("UzDevice.SetOutput2:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Get the output mode
        /// </summary>
        /// <param name="outputNo">The number of output to get</param>
        /// <param name="outputCount">The count of output to get</param>
        /// <param name="mode">
        /// Whether the output mode is active high or active low
        /// 0 : Active high
        /// 1 : Active low
        /// </param>
        /// <returns>Error Code</returns>
        public int GetOutputMode(int outputNo, int outputCount, out int mode)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzGetOutputMode(this.NetworkId, outputNo, outputCount, out mode);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.GetOutputMode:" +
                        "Failed to get output mode from device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                mode = 0;
                Debug.WriteLine("UzDevice.GetOutputMode:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Set the output mode(type)
        /// </summary>
        /// <param name="outputNo">The number of output to set</param>
        /// <param name="outputCount">The count of output to set</param>
        /// <param name="mode">
        /// Whether the output mode is active high or active low
        /// 0 : Active High
        /// 1 : Active Low
        /// </param>
        /// <returns>Error Code</returns>
        public int SetOutputMode(int outputNo, int outputCount, out int mode)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzSetOutputMode(this.NetworkId, outputNo, outputCount, out mode);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.SetOutputMode:" +
                        "Failed to set input mode to device" +
                        "(Network ID: " + this.NetworkId.ToString() +
                        ", Type: " + mode.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                mode = 0;
                Debug.WriteLine("UzDevice.SetOutputMode:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Get trigger period
        /// </summary>
        /// <param name="portNo">The number of output to get</param>
        /// <param name="portCount">The count of output to get</param>
        /// <param name="period">The trigger period time(msec)</param>
        /// <returns>Error Code</returns>
        public int GetTriggerPeriod(int outputNo, int outputCount, out int period)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzGetTriggerPeriod(this.NetworkId, outputNo, outputCount, out period);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.GetTriggerPeriod:" +
                        "Failed to get trigger period from device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                period = 0;
                Debug.WriteLine("UzDevice.GetTriggerPeriod:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Set trigger period
        /// </summary>
        /// <param name="portNo">The number of output to set</param>
        /// <param name="portCount">The count of output to set</param>
        /// <param name="period">The period time of trigger to set</param>
        /// <returns>Error Code</returns>
        public int SetTriggerPeriod(int outputNo, int outputCount, out int period)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzSetTriggerPeriod(this.NetworkId, outputNo, outputCount, out period);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.SetTriggerPeriod:" +
                        "Failed to set trigger period to device" +
                        "(Network ID: " + this.NetworkId.ToString() +
                        ", Period: " + period.ToString() + ")",
                        0);
                }

                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                period = 0;
                Debug.WriteLine("UzDevice.SetTriggerPeriod:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Get trigger on time
        /// </summary>
        /// <param name="portNo">The number of output to get</param>
        /// <param name="portCount">The count of output to get</param>
        /// <param name="ontime">The trigger on-time(msec)</param>
        /// <returns>Error Code</returns>
        public int GetTriggerOnTime(int outputNo, int outputCount, out int ontime)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzGetTriggerOnTime(this.NetworkId, outputNo, outputCount, out ontime);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.GetTriggerOnTime:" +
                        "Failed to get trigger period from device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                ontime = 0;
                Debug.WriteLine("UzDevice.GetTriggerOnTime:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Set trigger on-time
        /// </summary>
        /// <param name="portNo">The number of output to set</param>
        /// <param name="portCount">The count of output to set</param>
        /// <param name="ontime">The on-time in period of trigger to set</param>
        /// <returns>Error Code</returns>
        public int SetTriggerOnTime(int outputNo, int outputCount, out int ontime)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzSetTriggerOnTime(this.NetworkId, outputNo, outputCount, out ontime);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.SetTriggerOnTime:" +
                        "Failed to set trigger on-time to device" +
                        "(Network ID: " + this.NetworkId.ToString() +
                        ", On-time: " + ontime.ToString() + ")");
                }

                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                ontime = 0;
                Debug.WriteLine("UzDevice.SetTriggerOnTime:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Get trigger total count
        /// </summary>
        /// <param name="portNo">The number of output to get</param>
        /// <param name="portCount">The count of output to get</param>
        /// <param name="totalCount">The trigger total count</param>
        /// <returns>Error Code</returns>
        public int GetTriggerTotalCount(int outputNo, int outputCount, out int totalCount)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzGetTriggerTotalCount(this.NetworkId, outputNo, outputCount, out totalCount);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.GetTriggerTotalCount:" +
                        "Failed to get trigger total count from device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                totalCount = 0;
                Debug.WriteLine("UzDevice.GetTriggerTotalCount:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Set trigger total count
        /// </summary>
        /// <param name="portNo">The number of output to set</param>
        /// <param name="portCount">The count of output to set</param>
        /// <param name="totalCount">The total count of trigger to set</param>
        /// <returns>Error Code</returns>
        public int SetTriggerTotalCount(int outputNo, int outputCount, out int totalCount)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzSetTriggerTotalCount(this.NetworkId, outputNo, outputCount, out totalCount);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.SetTriggerTotalCount:" +
                        "Failed to set trigger total count to device" +
                        "(Network ID: " + this.NetworkId.ToString() +
                        ", Total count: " + totalCount.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                totalCount = 0;
                Debug.WriteLine("UzDevice.SetTriggerTotalCount:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Get the trigger is enable
        /// </summary>
        /// <param name="portNo">The number of output to get</param>
        /// <param name="portCount">The count of output to get</param>
        /// <param name="enable">Whether the trigger is enable or disable</param>
        /// <returns>Error Code</returns>
        public int GetTriggerEnable(int outputNo, int outputCount, out int enable)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzGetTriggerEnable(this.NetworkId, outputNo, outputCount, out enable);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.GetTriggerEnable:" +
                        "Failed to get trigger enable from device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                enable = 0;
                Debug.WriteLine("UzDevice.GetTriggerEnable:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Start trigger
        /// </summary>
        /// <param name="portNo">The number of output trigger to start</param>
        /// <param name="portCount">The count of output trigger to start</param>
        /// <returns>Error Code</returns>
        public int StartTrigger(int outputNo, int outputCount)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzStartTrigger(this.NetworkId, outputNo, outputCount);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.StartTrigger:" +
                        "Failed to start trigger to device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }

                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UzDevice.StartTrigger:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Stop trigger
        /// </summary>
        /// <param name="portNo">The number of output trigger to stop</param>
        /// <param name="portCount">The count of output trigger to stop</param>
        /// <returns>Error Code</returns>
        public int StopTrigger(int outputNo, int outputCount)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzStopTrigger(this.NetworkId, outputNo, outputCount);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.StopTrigger:" +
                        "Failed to stop trigger to device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UzDevice.StopTrigger:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Get trigger count
        /// </summary>
        /// <param name="portNo">The number of output to get</param>
        /// <param name="portCount">The count of output to get</param>
        /// <param name="count">The count of output trigger</param>
        /// <returns>Error Code</returns>
        public int GetTriggerCount(int outputNo, int outputCount, out int count)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzGetTriggerCount(this.NetworkId, outputNo, outputCount, out count);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.GetTriggerCount:" +
                        "Failed to get trigger count from device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }
                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                count = 0;
                Debug.WriteLine("UzDevice.GetTriggerCount:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }

        /// <summary>
        /// Clear trigger count
        /// </summary>
        /// <param name="portNo">The number of output trigger to clear count</param>
        /// <param name="portCount">The count of output trigger to clear count</param>
        /// <returns>Error Code</returns>
        public int ClearTriggerCount(int outputNo, int outputCount)
        {
            int returnCode = 0;

            try
            {
                returnCode = eUzClearTriggerCount(this.NetworkId, outputNo, outputCount);
                if (returnCode != (int)eUzErrorCode.UZ_ERROR_SUCCESS)
                {
                    Debug.WriteLine("UzDevice.ClearTriggerCount:" +
                        "Failed to clear trigger to device" +
                        "(Network ID: " + this.NetworkId.ToString() + ")");
                }

                Thread.Sleep(Interval);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UzDevice.ClearTriggerCount:" + ex.Message);
                returnCode = -1;
            }

            return returnCode;
        }
        #endregion
    }
}
