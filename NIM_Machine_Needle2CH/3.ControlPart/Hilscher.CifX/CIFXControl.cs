using System;
using Hilscher.CifX;

namespace MachineControlBase
{
    /// <summary>
    /// 힐셔 CIFX 이더켓 마스터 보드 제어
    /// </summary>
    public class CIFXControl
    {
        public IntPtr _hDriver;
        public IntPtr _hChannel;
        public IntPtr _hSysdevice;

        /// <summary>
        /// 드라이버 Open
        /// </summary>
        public void OpenDriver()
        {
            Int32 lret = 0;
            lret = cifXUser.xDriverOpen(ref _hDriver);
            if (lret != 0) NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "CIFX Driver open failed with " + string.Format("0x{0:X8}", lret.ToString("x")));
            lret = cifXUser.xSysdeviceOpen(_hDriver, "cifx0", ref _hSysdevice);
            //Open the channel to get the handle
            lret = cifXUser.xChannelOpen(_hDriver, "cifx0", 0, ref _hChannel);
            if (lret != 0) NLogger.AddLog(eLogType.PROGRAM, NLogger.eLogLevel.INFO, "CIFX xChannelOpen open failed");
        }

        /// <summary>
        /// 드라이버 해제
        /// </summary>
        public void Free()
        {
            if (_hChannel != IntPtr.Zero)
            {
                cifXUser.xChannelClose(_hChannel);
                _hChannel = IntPtr.Zero;
            }
            if (_hSysdevice != IntPtr.Zero)
            {
                cifXUser.xSysdeviceClose(_hSysdevice);
                _hSysdevice = IntPtr.Zero;
            }
            if (_hDriver != IntPtr.Zero)
            {
                cifXUser.xDriverClose(_hDriver);
                _hDriver = IntPtr.Zero;
            }
        }
    }
}