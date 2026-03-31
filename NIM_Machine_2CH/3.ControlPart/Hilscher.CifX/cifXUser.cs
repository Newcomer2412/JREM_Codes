using System;
using System.Runtime.InteropServices;

namespace Hilscher.CifX
{
    public class cifXUser
    {
        #region Structure definitions
        /// <exclude/>
        private const int NXDRV_NAME_LENGTH = 64;
        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct NXDRV_DEVICE_INFORMATIONtag
        {
            /// <exclude/>
            public IntPtr hDevice;                                      /*!< Device handle */
            /// <exclude/>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NXDRV_NAME_LENGTH)]
            public byte[] szDeviceName;//!< Device name
            /// <exclude/>
            public SYSTEM_CHANNEL_SYSTEM_INFO_BLOCKtag tSystemInfoBlock;/*!< Device System Info Block */
        }

        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DRIVER_INFORMATIONtag
        {
            /// <exclude/>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] abDriverVersion;                              /*!< Driver version                 */
            /// <exclude/>
            public UInt32 ulBoardCnt;                                   /*!< Number of available Boards     */
        }

        public UInt32 CIFx_MAX_INFO_NAME_LENGTH = 16;

        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CIFX_DIRECTORYENTRYtag                            /*! Directory Information structure for enumerating directories */
        {
            /// <exclude/>
            public IntPtr hList;                                        /*!< Handle from Enumeration function, do not touch */
            /// <exclude/>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] szFilename;                                   /*!< Returned file name. */
            /// <exclude/>
            public byte bFiletype;                                      /*!< Returned file type. */
            /// <exclude/>
            public UInt32 ulFilesize;                                   /*!< Returned file size. */
        }

        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CIFX_EXTENDED_MEMORY_INFORMATIONtag
        {
            /// <exclude/>
            public IntPtr pvMemoryID;                                   /*!< Identification of the memory area       */
                                                                        /// <exclude/>
            public IntPtr pvMemoryPtr;                                  /*!< Memory pointer                          */
                                                                        /// <exclude/>
            public UInt32 ulMemorySize;                                 /*!< Memory size of the Extended memory area */
                                                                        /// <exclude/>
            public UInt32 ulMemoryType;                                 /*!< Memory type information                 */
        }

        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SYSTEM_CHANNEL_SYSTEM_INFORMATIONtag            /*! System Channel Information structure*/
        {
            /// <exclude/>
            public UInt32 ulSystemError;                                /*!< Global system error            */
                                                                        /// <exclude/>
            public UInt32 ulDpmTotalSize;                               /*!< Total size dual-port memory in bytes */
                                                                        /// <exclude/>
            public UInt32 ulMBXSize;                                    /*!< System mailbox data size [Byte]*/
                                                                        /// <exclude/>
            public UInt32 ulDeviceNumber;                               /*!< Global device number           */
                                                                        /// <exclude/>
            public UInt32 ulSerialNumber;                               /*!< Global serial number           */
                                                                        /// <exclude/>
            public UInt32 ulOpenCnt;                                    /*!< Channel open counter           */
        }

        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SYSTEM_CHANNEL_SYSTEM_INFO_BLOCKtag              /* System Channel: System Information Block */
        {
            /// <exclude/>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] abCookie;                                     /*!< 0x00 "netX" cookie */
            /// <exclude/>
            public UInt32 ulDpmTotalSize;                               /*!< 0x04 Total Size of the whole dual-port memory in bytes */
            /// <exclude/>
            public UInt32 ulDeviceNumber;                               /*!< 0x08 Device number */
            /// <exclude/>
            public UInt32 ulSerialNumber;                               /*!< 0x0C Serial number */
            /// <exclude/>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public UInt16[] ausHwOptions;                               /*!< 0x10 Hardware options, xC port 0..3 */
            /// <exclude/>
            public UInt16 usManufacturer;                               /*!< 0x18 Manufacturer Location */
            /// <exclude/>
            public UInt16 usProductionDate;                             /*!< 0x1A Date of production */
            /// <exclude/>
            public UInt32 ulLicenseFlags1;                              /*!< 0x1C License code flags 1 */
            /// <exclude/>
            public UInt32 ulLicenseFlags2;                              /*!< 0x20 License code flags 2 */
            /// <exclude/>
            public UInt16 usNetxLicenseID;                              /*!< 0x24 netX license identification */
            /// <exclude/>
            public UInt16 usNetxLicenseFlags;                           /*!< 0x26 netX license flags */
            /// <exclude/>
            public UInt16 usDeviceClass;                                /*!< 0x28 netX device class */
            /// <exclude/>
            public byte bHwRevision;                                    /*!< 0x2A Hardware revision index */
            /// <exclude/>
            public byte bHwCompatibility;                               /*!< 0x2B Hardware compatibility index */
            /// <exclude/>
            public byte bDevIdNumber;                                   /*!< Device Identification number (rotary switch) */
            /// <exclude/>
            public byte bReserved;                                      /*!< unused/reserved */
            /// <exclude/>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public UInt16[] ausReserved;                                /*!< 0x2C:0x2F Reserved */
        }

        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SYSTEM_CHANNEL_CHANNEL_INFO_BLOCKtag
        {
            /// <exclude/>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = CIFX_MAX_NUMBER_OF_CHANNEL_DEFINITION)]
            public SYSTEM_CHANNEL_CHANNEL_INFO_BLOCKSubtag[] abInfoBlock;
        }

        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SYSTEM_CHANNEL_CHANNEL_INFO_BLOCKSubtag
        {
            /// <exclude/>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = CIFX_SYSTEM_CHANNEL_DEFAULT_INFO_BLOCK_SIZE)]
            public byte[] abInfoBlock;

            /// <exclude/>
            public byte this[int i]
            {
                get { return abInfoBlock[i]; }
                set { abInfoBlock[i] = value; }
            }
        }

        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SYSTEM_CHANNEL_SYSTEM_CONTROL_BLOCKtag            /* System Channel: System Control Block */
        {
            /// <exclude/>
            public UInt32 ulSystemCommandCOS;                           /*!< 0x00 System channel change of state command */
            /// <exclude/>
            public UInt32 ulReserved;                                   /*!< 0x04 Reserved */
        }

        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SYSTEM_CHANNEL_SYSTEM_STATUS_BLOCKtag             /* System Channel: System Status Block */
        {
            /// <exclude/>
            public UInt32 ulSystemCOS;                                  /*!< 0x00 System channel change of state acknowledge */
            /// <exclude/>
            public UInt32 ulSystemStatus;                               /*!< 0x04 Actual system state */
            /// <exclude/>
            public UInt32 ulSystemError;                                /*!< 0x08 Actual system error */
            /// <exclude/>
            public UInt32 ulBootError;                                  /*!< 0x0C Bootup error code (only valid if Cookie="BOOT") */
            /// <exclude/>
            public UInt32 ulTimeSinceStart;                             /*!< 0x10 time since start in seconds */
            /// <exclude/>
            public UInt16 usCpuLoad;                                    /*!< 0x14 cpu load in 0,01% units (10000 => 100%) */
            /// <exclude/>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
            public byte[] abReserved;                                   /*!< 0x16:3F Reserved */
        }

        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BOARD_INFORMATIONtag                              /*! Board Information structure                                              */
        {
            /// <exclude/>
            public UInt32 lBoardError;                                  /*!< Global Board error. Set when device specific data must not be used */
            /// <exclude/>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = CIFx_MAX_INFO_NAME_LENTH)]
            public byte[] abBoardName;                                  /*!< Global board name              */
            /// <exclude/>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = CIFx_MAX_INFO_NAME_LENTH)]
            public byte[] abBoardAlias;                                 /*!< Global board alias name        */
            /// <exclude/>
            public UInt32 ulBoardID;                                    /*!< Unique board ID, driver created*/
            /// <exclude/>
            public UInt32 ulSystemError;                                /*!< System error                   */
            /// <exclude/>
            public UInt32 ulPhysicalAddress;                            /*!< Physical memory address        */
            /// <exclude/>
            public UInt32 ulIrqNumber;                                  /*!< Hardware interrupt number      */
            /// <exclude/>
            public byte bIrqEnabled;                                    /*!< Hardware interrupt enable flag */
            /// <exclude/>
            public UInt32 ulChannelCnt;                                 /*!< Number of available channels   */
            /// <exclude/>
            public UInt32 ulDpmTotalSize;                               /*!< Dual-Port memory size in bytes */
            /// <exclude/>
            public SYSTEM_CHANNEL_SYSTEM_INFO_BLOCKtag tSystemInfo;     /*!< System information             */
        }

        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CHANNEL_INFORMATIONtag                            /*! Channel Information structure                                            */
        {
            /// <exclude/>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = CIFx_MAX_INFO_NAME_LENTH)]
            public byte[] abBoardName;                                  /*!< Global board name              */
            /// <exclude/>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = CIFx_MAX_INFO_NAME_LENTH)]
            public byte[] abBoardAlias;                                 /*!< Global board alias name        */
            /// <exclude/>
            public UInt32 ulDeviceNumber;                               /*!< Global board device number     */
            /// <exclude/>
            public UInt32 ulSerialNumber;                               /*!< Global board serial number     */
            /// <exclude/>
            public UInt16 usFWMajor;                                    /*!< Major Version of Channel Firmware  */
            /// <exclude/>
            public UInt16 usFWMinor;                                    /*!< Minor Version of Channel Firmware  */
            /// <exclude/>
            public UInt16 usFWBuild;                                    /*!< Build number of Channel Firmware   */
            /// <exclude/>
            public UInt16 usFWRevision;                                 /*!< Revision of Channel Firmware       */
            /// <exclude/>
            public byte bFWNameLength;                                  /*!< Length  of FW Name                 */
            /// <exclude/>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 63)]
            public byte[] abFWName;                                     /*!< Firmware Name                      */
            /// <exclude/>
            public UInt16 usFWYear;                                     /*!< Build Year of Firmware             */
            /// <exclude/>
            public byte bFWMonth;                                       /*!< Build Month of Firmware (1..12)    */
            /// <exclude/>
            public byte bFWDay;                                         /*!< Build Day of Firmware (1..31)      */
            /// <exclude/>
            public UInt32 ulChannelError;                               /*!< Channel error                  */
            /// <exclude/>
            public UInt32 ulOpenCnt;                                    /*!< Channel open counter           */
            /// <exclude/>
            public UInt32 ulPutPacketCnt;                               /*!< Number of put packet commands  */
            /// <exclude/>
            public UInt32 ulGetPacketCnt;                               /*!< Number of get packet commands  */
            /// <exclude/>
            public UInt32 ulMailboxSize;                                /*!< Mailbox packet size in bytes   */
            /// <exclude/>
            public UInt32 ulIOInAreaCnt;                                /*!< Number of IO IN areas          */
            /// <exclude/>
            public UInt32 ulIOOutAreaCnt;                               /*!< Number of IO OUT areas         */
            /// <exclude/>
            public UInt32 ulHskSize;                                    /*!< Size of the handshake cells    */
            /// <exclude/>
            public UInt32 ulNetxFlags;                                  /*!< Actual netX state flags        */
            /// <exclude/>
            public UInt32 ulHostFlags;                                  /*!< Actual Host flags              */
            /// <exclude/>
            public UInt32 ulHostCOSFlags;                               /*!< Actual Host COS flags          */
            /// <exclude/>
            public UInt32 ulDeviceCOSFlags;                             /*!< Actual Device COS flags        */
        }

        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CHANNEL_IO_INFORMATIONtag             /*! IO Area Information structure                                            */
        {
            /// <exclude/>
            public UInt32 ulTotalSize;                      /*!< Total IO area size in byte */
            /// <exclude/>
            public UInt32 ulUsedSize;                       /*!< Used IO area size in byte */
            /// <exclude/>
            public UInt32 ulIOMode;                         /*!< Exchange mode */
        }

        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MEMORY_INFORMATIONtag
        {
            /// <exclude/>
            public IntPtr pvMemoryID;                         /*!< Identification of the memory area      */
            /// <exclude/>                                                              
            public PPV_MEMORY_POINTERSubtag ppvMemoryPtr;                      /*!< Memory pointer                         */
            /// <exclude/>
            public UIntPtr pulMemorySize;                     /*!< Complete size of the mapped memory     */
            /// <exclude/>
            public UIntPtr ulChannel;                          /*!< Requested channel number               */
            /// <exclude/>
            public UIntPtr pulChannelStartOffset;             /*!< Start offset of the requested channel  */
            /// <exclude/>
            public UIntPtr pulChannelSize;                    /*!< Memory size of the requested channel   */
        }

        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PLC_MEMORY_INFORMATIONtag      /*! PLC Memory Information structure */
        {
            /// <exclude/>
            public IntPtr pvMemoryID;                       /*!< Identification of the memory area */
            /// <exclude/>
            public PPV_MEMORY_POINTERSubtag ppvMemoryPtr;                    /*!< Memory pointer                   */
            /// <exclude/>
            public UInt32 ulAreaDefinition;                 /*!< Input/output area                  */
            /// <exclude/>
            public UInt32 ulAreaNumber;                     /*!< Area number                        */
            /// <exclude/>
            public UIntPtr pulIOAreaStartOffset;            /*!< Start offset                       */
            /// <exclude/>
            public UIntPtr pulIOAreaSize;                   /*!< Memory size                        */
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PPV_MEMORY_POINTERSubtag
        {
            public IntPtr pvMemoryPtr;
        }

        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CIFX_PACKET_HEADERtag     /*! Packet header     */
        {
            /// <exclude/>
            public UInt32 ulDest;                           /*!< destination of packet, process queue */
            /// <exclude/>
            public UInt32 ulSrc;                            /*!< source of packet, process queue */
            /// <exclude/>
            public UInt32 ulDestId;                         /*!< destination reference of packet */
            /// <exclude/>
            public UInt32 ulSrcId;                          /*!< source reference of packet */
            /// <exclude/>
            public UInt32 ulLen;                            /*!< length of packet data without header */
            /// <exclude/>
            public UInt32 ulId;                             /*!< identification handle of sender */
            /// <exclude/>
            public UInt32 ulState;                          /*!< status code of operation */
            /// <exclude/>
            public UInt32 ulCmd;                            /*!< packet command defined in TLR_Commands.h */
            /// <exclude/>
            public UInt32 ulExt;    /*!< extension */
            /// <exclude/>
            public UInt32 ulRout;   /*!< router */
        }

        /// <exclude/>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CIFX_PACKETtag                        /*! Definition of the rcX Packet                                             */
        {
            /// <exclude/>
            public CIFX_PACKET_HEADERtag tHeader;           /**! */
            /// <exclude/>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = CIFX_MAX_DATA_SIZE)]
            public byte[] abData;
        }

        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PFN_PROGRESS_CALLBACK(UInt32 ulStep, UInt32 ulMaxStep, UIntPtr pvUser, char bFinished, Int32 lError);
        private static PFN_PROGRESS_CALLBACK pFN_PROGRESS_CALLBACK;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PFN_RECV_PKT_CALLBACK(ref CIFX_PACKETtag ptRecvPkt, UIntPtr pvUser);
        private static PFN_RECV_PKT_CALLBACK pFN_RECV_PKT_CALLBACK;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PFN_NOTIFY_CALLBACK(UInt32 ulNotification, UInt32 ulDataLen, UIntPtr pvData, UIntPtr pvUser);
        private static PFN_NOTIFY_CALLBACK[] pFN_NOTIFY_CALLBACKs = new PFN_NOTIFY_CALLBACK[8];

        public const UInt32 CIFX_CALLBACK_ACTIVE = 0;
        public const UInt32 CIFX_CALLBACK_FINISHED = 1;

        public const UInt32 DOWNLOAD_MODE_FIRMWARE = 1;
        public const UInt32 DOWNLOAD_MODE_CONFIG = 2;
        public const UInt32 DOWNLOAD_MODE_FILE = 3;
        public const UInt32 DOWNLOAD_MODE_BOOTLOADER = 4; /*!< Download bootloader update to target. */
        public const UInt32 DOWNLOAD_MODE_LICENSECODE = 5; /*!< License update code.                  */
        #endregion

        #region NXAPI Calls
        [DllImport("cifx32dll.dll", EntryPoint = "nxDrvInit")]
        private static extern UInt32 _nxDrvInit();

        [DllImport("cifX32dll.dll", EntryPoint = "nxDrvFindDevice")]
        private static extern UInt32 _nxDrvFindDevice(UInt32 uiCommand, UInt32 uiSize, ref NXDRV_DEVICE_INFORMATIONtag tDeviceInfo, ref UInt32 uiSearchIndex);

        [DllImport("cifX32dll.dll", EntryPoint = "nxDrvDownload")]
        private static extern UInt32 _nxDrvDownload(UIntPtr hDevice, UInt32 ulChannel, UInt32 ulMode, UInt32 ulFileSize, [MarshalAs(UnmanagedType.LPStr)] string sFileName, Byte[] pabFileData,
                                                    UIntPtr pvUser, string PFN1);

        [DllImport("cifX32dll.dll", EntryPoint = "nxDrvStart")]
        private static extern UInt32 _nxDrvStart(UIntPtr hDevice, UInt32 uiChannel);
        #endregion

        #region CIFX API Function Calls
        /*----------------------------*/
        /* Global driver functions    */
        /*----------------------------*/
        [DllImport("cifx32dll.dll", EntryPoint = "xDriverOpen")]
        private static extern Int32 _xDriverOpen(ref IntPtr hDriver);

        [DllImport("cifx32dll.dll", EntryPoint = "xDriverClose")]
        private static extern Int32 _xDriverClose(IntPtr hDriver);

        [DllImport("cifx32dll.dll", EntryPoint = "xDriverGetInformation")]
        private static extern Int32 _xDriverGetInformation(IntPtr hDriver, UInt32 ulSize, ref DRIVER_INFORMATIONtag pvDriverInfo);

        [DllImport("cifx32dll.dll", EntryPoint = "xDriverGetErrorDescription")]
        private static extern Int32 _xDriverGetErrorDescription(Int32 lError, [Out, MarshalAs(UnmanagedType.LPArray)] byte[] szBuffer, UInt32 ulBufferLen);

        [DllImport("cifx32dll.dll", EntryPoint = "xDriverEnumBoards")]
        private static extern Int32 _xDriverEnumBoards(IntPtr hDriver, UInt32 ulBoard, UInt32 ulSize, ref BOARD_INFORMATIONtag pvBoardInfo);

        [DllImport("cifx32dll.dll", EntryPoint = "xDriverEnumChannels")]
        private static extern Int32 _xDriverEnumChannels(IntPtr hDriver, UInt32 ulBoard, UInt32 ulChannel, UInt32 ulSize, ref CHANNEL_INFORMATIONtag pvChannelInfo);

        [DllImport("cifx32dll.dll", EntryPoint = "xDriverMemoryPointer")]
        private static extern Int32 _xDriverMemoryPointer(IntPtr hDriver, UInt32 ulBoard, UInt32 ulCmd, ref MEMORY_INFORMATIONtag pvMemoryInfo);

        [DllImport("cifx32dll.dll", EntryPoint = "xDriverRestartDevice")]
        private static extern Int32 _xDriverRestartDevice(IntPtr hDriver, string szBoardName, UIntPtr pvData);

        /*----------------------------------*/
        /* System device depending functions*/
        /*----------------------------------*/
        [DllImport("cifx32dll.dll", EntryPoint = "xSysdeviceOpen")]
        private static extern Int32 _xSysdeviceOpen(IntPtr hDriver, string szBoard, ref IntPtr phSysdevice);

        [DllImport("cifx32dll.dll", EntryPoint = "xSysdeviceClose")]
        private static extern Int32 _xSysdeviceClose(IntPtr hSysdevice);

        [DllImport("cifx32dll.dll", EntryPoint = "xSysdeviceGetMBXState")]
        private static extern Int32 _xSysdeviceGetMBXState(IntPtr hSysdevice, ref UInt32 pulRecvPktCount, ref UInt32 pulSendPktCount);

        [DllImport("cifx32dll.dll", EntryPoint = "xSysdevicePutPacket")]
        private static extern Int32 _xSysdevicePutPacket(IntPtr hSysdevice, ref CIFX_PACKETtag ptSendPkt, UInt32 ulTimeout);

        [DllImport("cifx32dll.dll", EntryPoint = "xSysdeviceGetPacket")]
        private static extern Int32 _xSysdeviceGetPacket(IntPtr hSysdevice, UInt32 ulSize, ref CIFX_PACKETtag ptRecvPkt, UInt32 ulTimeout);

        [DllImport("cifx32dll.dll", EntryPoint = "xSysdeviceInfo")]
        private static extern Int32 _xSysdeviceInfo(IntPtr hSysdevice, UInt32 ulCmd, UInt32 ulSize, [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pvzData);

        [DllImport("cifx32dll.dll", EntryPoint = "xSysdeviceFindFirstFile")]
        private static extern Int32 _xSysdeviceFindFirstFile(IntPtr hSysdevice, UInt32 ulChannel, ref CIFX_DIRECTORYENTRYtag ptDirectoryInfo, PFN_RECV_PKT_CALLBACK PFN1, UIntPtr pvUser);

        [DllImport("cifx32dll.dll", EntryPoint = "xSysdeviceFindNextFile")]
        private static extern Int32 _xSysdeviceFindNextFile(IntPtr hSysdevice, UInt32 ulChannel, ref CIFX_DIRECTORYENTRYtag ptDirectoryInfo, PFN_RECV_PKT_CALLBACK PFN1, UIntPtr pvUser);

        [DllImport("cifx32dll.dll", EntryPoint = "xSysdeviceDownload")]
        private static extern Int32 _xSysdeviceDownload(IntPtr hSysdevice, UInt32 ulChannel, UInt32 ulMode, [MarshalAs(UnmanagedType.LPStr)] string pszFileName, byte[] pabFileData,
                                                                UInt32 ulFileSize, PFN_PROGRESS_CALLBACK PFN1, PFN_RECV_PKT_CALLBACK PFN2, UIntPtr pvUser);

        [DllImport("cifx32dll.dll", EntryPoint = "xSysdeviceUpload")]
        private static extern Int32 _xSysdeviceUpload(IntPtr hSysdevice, UInt32 ulChannel, UInt32 ulMode, [MarshalAs(UnmanagedType.LPStr)] string pszFileName, Byte[] pabFileData, UInt32 pulFileSize,
                                                                PFN_PROGRESS_CALLBACK PFN1, PFN_RECV_PKT_CALLBACK PFN2, UIntPtr pvUser);

        [DllImport("cifx32dll.dll", EntryPoint = "xSysdeviceReset")]
        private static extern Int32 _xSysdeviceReset(IntPtr hSysdevice, UInt32 ulTimeout);

        [DllImport("cifx32dll.dll", EntryPoint = "xSysdeviceBootstart")]
        private static extern Int32 _xSysdeviceBootstart(IntPtr hSysdevice, UInt32 ulTimeout);

        [DllImport("cifx32dll.dll", EntryPoint = "xSysdeviceExtendedMemory")]
        private static extern Int32 _xSysdeviceExtendedMemory(IntPtr hSysdevice, UInt32 ulCmd, ref CIFX_EXTENDED_MEMORY_INFORMATIONtag ptExtMemData);

        /*-----------------------------*/
        /* Channel depending functions */
        /*-----------------------------*/
        [DllImport("cifx32dll.dll", EntryPoint = "xChannelOpen")]
        private static extern Int32 _xChannelOpen(IntPtr hChannel, [MarshalAs(UnmanagedType.LPStr)] string szBoard, UInt32 ulChannel, ref IntPtr phChannel);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelClose")]
        private static extern Int32 _xChannelClose(IntPtr hChannel);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelFindFirstFile")]
        private static extern Int32 _xChannelFindFirstFile(IntPtr hChannel, ref CIFX_DIRECTORYENTRYtag ptDirectoryInfo, PFN_RECV_PKT_CALLBACK PFN1, UIntPtr pvUser);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelFindNextFile")]
        private static extern Int32 _xChannelFindNextFile(IntPtr hChannel, ref CIFX_DIRECTORYENTRYtag ptDirectoryInfo, PFN_RECV_PKT_CALLBACK PFN1, UIntPtr pvUser);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelDownload")]
        private static extern Int32 _xChannelDownload(IntPtr hChannel, UInt32 ulMode, [MarshalAs(UnmanagedType.LPStr)] string sFileName, Byte[] pabFileData, UInt32 ulFileSize,
                                                                    PFN_PROGRESS_CALLBACK PFN1, PFN_RECV_PKT_CALLBACK PFN2, UIntPtr pvUser);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelUpload")]
        private static extern Int32 _xChannelUpload(IntPtr hChannel, UInt32 ulMode, [MarshalAs(UnmanagedType.LPStr)] string pszFileName, Byte[] pabFileData, UInt32 pulFileSize,
                                                                    PFN_PROGRESS_CALLBACK PFN1, PFN_RECV_PKT_CALLBACK PFN2, UIntPtr pvUser);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelGetMBXState")]
        private static extern Int32 _xChannelGetMBXState(IntPtr hChannel, ref UInt32 pulRecvPktCount, ref UInt32 pulSendPktCount);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelPutPacket")]
        private static extern Int32 _xChannelPutPacket(IntPtr hChannel, ref CIFX_PACKETtag ptSendPkt, UInt32 ulTimeout);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelGetPacket")]
        private static extern Int32 _xChannelGetPacket(IntPtr hChannel, UInt32 ulSize, ref CIFX_PACKETtag ptRecvPkt, UInt32 ulTimeout);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelGetSendPacket")]
        private static extern Int32 _xChannelGetSendPacket(IntPtr hChannel, UInt32 ulSize, ref CIFX_PACKETtag ptRecvPkt);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelConfigLock")]
        private static extern Int32 _xChannelConfigLock(IntPtr hChannel, UInt32 ulCmd, ref UInt32 pulState, UInt32 ulTimeout);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelReset")]
        private static extern Int32 _xChannelReset(IntPtr hChannel, UInt32 ulResetMode, UInt32 ulTimeout);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelInfo")]
        private static extern Int32 _xChannelInfo(IntPtr hChannel, UInt32 ulSize, ref CHANNEL_INFORMATIONtag pvChannelInfo);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelWatchdog")]
        private static extern Int32 _xChannelWatchdog(IntPtr hChannel, UInt32 ulCmd, ref UInt32 pulTrigger);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelHostState")]
        private static extern Int32 _xChannelHostState(IntPtr hChannel, UInt32 ulCmd, ref UInt32 pulState, UInt32 ulTimeout);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelBusState")]
        private static extern Int32 _xChannelBusState(IntPtr hChannel, UInt32 ulCmd, ref UInt32 pulState, UInt32 ulTimeout);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelDMAState")]
        private static extern Int32 _xChannelDMAState(IntPtr hChannel, UInt32 ulCmd, ref UInt32 pulState);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelIOInfo")]
        private static extern Int32 _xChannelIOInfo(IntPtr hChannel, UInt32 ulCmd, UInt32 ulAreaNumber, UInt32 ulSize, ref CHANNEL_IO_INFORMATIONtag pvData);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelIORead")]
        private static extern Int32 _xChannelIORead(IntPtr hChannel, UInt32 ulAreaNumber, UInt32 ulOffset, UInt32 ulDataLen, [In, MarshalAs(UnmanagedType.LPArray)] byte[] pvData, UInt32 ulTimeout);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelIOWrite")]
        private static extern Int32 _xChannelIOWrite(IntPtr hChannel, UInt32 ulAreaNumber, UInt32 ulOffset, UInt32 ulDataLen, [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pvData, UInt32 ulTimeout);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelIOReadSendData")]
        private static extern Int32 _xChannelIOReadSendData(IntPtr hChannel, UInt32 ulAreaNumber, UInt32 ulOffset, UInt32 ulDataLen, [In, MarshalAs(UnmanagedType.LPArray)] byte[] pvData);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelControlBlock")]
        private static extern Int32 _xChannelControlBlock(IntPtr hChannel, UInt32 ulCmd, UInt32 ulOffset, UInt32 ulDataLen, [In, MarshalAs(UnmanagedType.LPArray)] byte[] pvData);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelCommonStatusBlock")]
        private static extern Int32 _xChannelCommonStatusBlock(IntPtr hChannel, UInt32 ulCmd, UInt32 ulOffset, UInt32 ulDataLen, [In, MarshalAs(UnmanagedType.LPArray)] byte[] pvData);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelExtendedStatusBlock")]
        private static extern Int32 _xChannelExtendedStatusBlock(IntPtr hChannel, UInt32 ulCmd, UInt32 ulOffset, UInt32 ulDataLen, [In, MarshalAs(UnmanagedType.LPArray)] byte[] pvData);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelUserBlock")]
        private static extern Int32 _xChannelUserBlock(IntPtr hChannel, UInt32 ulAreaNumber, UInt32 ulCmd, UInt32 ulOffset, UInt32 ulDataLen, [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pvData);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelPLCMemoryPtr")]
        private static extern Int32 _xChannelPLCMemoryPtr(IntPtr hChannel, UInt32 ulCmd, ref PLC_MEMORY_INFORMATIONtag pvMemoryInfo);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelPLCIsReadReady")]
        private static extern Int32 _xChannelPLCIsReadReady(IntPtr hChannel, UInt32 ulAreaNumber, ref UInt32 pulReadState);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelPLCIsWriteReady")]
        private static extern Int32 _xChannelPLCIsWriteReady(IntPtr hChannel, UInt32 ulAreaNumber, ref UInt32 pulWriteState);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelPLCActivateWrite")]
        private static extern Int32 _xChannelPLCActivateWrite(IntPtr hChannel, UInt32 ulAreaNumber);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelPLCActivateRead")]
        private static extern Int32 _xChannelPLCActivateRead(IntPtr hChannel, UInt32 ulAreaNumber);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelRegisterNotification")]
        private static extern Int32 _xChannelRegisterNotification(IntPtr hChannel, UInt32 ulNotification, PFN_NOTIFY_CALLBACK pfnCallback, UIntPtr pvUser);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelUnregisterNotification")]
        private static extern Int32 _xChannelUnregisterNotification(IntPtr hChannel, UInt32 ulNotification);

        [DllImport("cifx32dll.dll", EntryPoint = "xChannelSyncState")]
        private static extern Int32 _xChannelSyncState(IntPtr hChannel, UInt32 ulCmd, UInt32 ulTimeout, ref UInt32 pulErrorCount);
        #endregion

        #region global constants
        /* DPM memory validation */
        public const ulong CIFX_DPM_NO_MEMORY_ASSIGNED = 0x0BAD0BADUL;
        public const ulong CIFX_DPM_INVALID_CONTENT = 0xFFFFFFFFUL;

        /* CIFx global timeouts in milliseconds */
        public const ulong CIFX_TO_WAIT_HW_RESET_ACTIVE = 2000UL;
        public const ulong CIFX_TO_WAIT_HW = 2000UL;
        public const ulong CIFX_TO_WAIT_COS_CMD = 10UL;
        public const ulong CIFX_TO_WAIT_COS_ACK = 10UL;
        public const ulong CIFX_TO_SEND_PACKET = 5000UL;
        public const ulong CIFX_TO_1ST_PACKET = 1000UL;
        public const ulong CIFX_TO_CONT_PACKET = 1000UL;
        public const ulong CIFX_TO_LAST_PACKET = 1000UL;
        public const ulong CIFX_TO_FIRMWARE_START = 10000UL;

        public const uint CIFX_IO_WAIT_TIMEOUT = 40U;
        public const uint CIFX_PACKET_WAIT_TIMEOUT = 40U;

        /* Maximum channel number */
        public const int CIFX_MAX_NUMBER_OF_CHANNEL_DEFINITION = 8;
        public const UInt32 CIFX_MAX_NUMBER_OF_CHANNELS = 6;
        public const UInt32 CIFX_NO_CHANNEL = 0xFFFFFFFF;

        /* Maximum file name length */
        public UInt32 CIFX_MAX_FILE_NAME_LENGTH = 260;
        public UInt32 CIFX_MIN_FILE_NAME_LENGTH = 5;

        /* The system device port number */
        public UInt32 CIFX_SYSTEM_DEVICE = 0xFFFFFFFF;

        /* Information commands */
        public const UInt32 CIFX_INFO_CMD_SYSTEM_INFORMATION = 1;
        public const UInt32 CIFX_INFO_CMD_SYSTEM_INFO_BLOCK = 2;
        public const UInt32 CIFX_INFO_CMD_SYSTEM_CHANNEL_BLOCK = 3;
        public const UInt32 CIFX_INFO_CMD_SYSTEM_CONTROL_BLOCK = 4;
        public const UInt32 CIFX_INFO_CMD_SYSTEM_STATUS_BLOCK = 5;

        /* General commands */
        public const UInt32 CIFX_CMD_READ_DATA = 1;
        public const UInt32 CIFX_CMD_WRITE_DATA = 2;

        /* HOST mode definition */
        public const UInt32 CIFX_HOST_STATE_NOT_READY = 0;
        public const UInt32 CIFX_HOST_STATE_READY = 1;
        public const UInt32 CIFX_HOST_STATE_READ = 2;

        /* WATCHDOG commands*/
        public const UInt32 CIFX_WATCHDOG_STOP = 0;
        public const UInt32 CIFX_WATCHDOG_START = 1;

        /* Configuration Lock commands*/
        public const UInt32 CIFX_CONFIGURATION_UNLOCK = 0;
        public const UInt32 CIFX_CONFIGURATION_LOCK = 1;
        public const UInt32 CIFX_CONFIGURATION_GETLOCKSTATE = 2;

        /* BUS state commands*/
        public const UInt32 CIFX_BUS_STATE_OFF = 0;
        public const UInt32 CIFX_BUS_STATE_ON = 1;
        public const UInt32 CIFX_BUS_STATE_GETSTATE = 2;

        /* Memory pointer commands*/
        public const UInt32 CIFX_MEM_PTR_OPEN = 1;
        public const UInt32 CIFX_MEM_PTR_OPEN_USR = 2;
        public const UInt32 CIFX_MEM_PTR_CLOSE = 3;

        /* I/O area definition */
        public const UInt32 CIFX_IO_INPUT_AREA = 1;
        public const UInt32 CIFX_IO_OUTPUT_AREA = 2;

        /* Reset definitions */
        public const UInt32 CIFX_SYSTEMSTART = 1;
        public const UInt32 CIFX_CHANNELINIT = 2;

        /* Dimentions */
        public const int CIFx_MAX_INFO_NAME_LENTH = 16;
        public const int CIFX_SYSTEM_CHANNEL_DEFAULT_INFO_BLOCK_SIZE = 16;
        public const int CIFX_PACKET_HEADER_SIZE = 40;                                               /*!< Maximum size of the RCX packet header in bytes */
        public const int CIFX_MAX_DATA_SIZE = CIFX_MAX_PACKET_SIZE - CIFX_PACKET_HEADER_SIZE;   /*!< Maximum RCX packet data size */
        public const int CIFX_MAX_PACKET_SIZE = 1600;

        #endregion

        #region Driver Specific Functions
        ///<exclude/>
        public static Int32 xDriverOpen(ref IntPtr phDriver)
        {
            return _xDriverOpen(ref phDriver);
        }
        ///<exclude/>
        public static Int32 xDriverGetInformation(IntPtr hDriver, UInt32 size, ref DRIVER_INFORMATIONtag DriverInformation)
        {
            return _xDriverGetInformation(hDriver, size, ref DriverInformation);
        }
        ///<exclude/>
        public static Int32 xDriverEnumBoards(IntPtr hDriver, UInt32 BoardNumber, UInt32 size, ref BOARD_INFORMATIONtag BoardInformation)
        {
            return _xDriverEnumBoards(hDriver, BoardNumber, size, ref BoardInformation);
        }
        ///<exclude/>
        public static Int32 xDriverEnumChannels(IntPtr hDriver, UInt32 BoardNumber, UInt32 ChannelNumber, UInt32 size, ref CHANNEL_INFORMATIONtag ChannelInformation)
        {
            return _xDriverEnumChannels(hDriver, BoardNumber, ChannelNumber, size, ref ChannelInformation);
        }
        ///<exclude/>
        public static Int32 xDriverClose(IntPtr hDriver)
        {
            return _xDriverClose(hDriver);
        }
        ///<exclude/>
        public static Int32 xDriverGetErrorDescription(Int32 ErrorNumber, ref byte[] szBuffer, UInt32 size)
        {
            return _xDriverGetErrorDescription(ErrorNumber, szBuffer, size);
        }
        ///<exclude/>
        public static Int32 xDriverRestartDevice(IntPtr hDriver, string BoardName)
        {
            return _xDriverRestartDevice(hDriver, BoardName, (UIntPtr)null);
        }
        ///<exclude/>
        public static Int32 xDriverMemoryPointer(IntPtr hDriver, UInt32 ulBoard, UInt32 ulCmd, ref MEMORY_INFORMATIONtag pvMemoryInfo)
        {
            return _xDriverMemoryPointer(hDriver, ulBoard, ulCmd, ref pvMemoryInfo);
        }
        #endregion

        #region Sysdevice specific functions
        ///<exclude/>
        public static Int32 xSysdeviceOpen(IntPtr hDriver, string BoardName, ref IntPtr phSysDevice)
        {
            return _xSysdeviceOpen(hDriver, BoardName, ref phSysDevice);
        }

        ///<exclude/>
        public static Int32 xSysdeviceClose(IntPtr hSysDevice)
        {
            return _xSysdeviceClose(hSysDevice);
        }

        ///<exclude/>
        public static Int32 xSysdeviceReset(IntPtr hSysDevice, UInt32 Timeout)
        {
            return _xSysdeviceReset(hSysDevice, Timeout);
        }

        ///<exclude/>
        public static Int32 xSysdeviceInfo<T>(IntPtr hSysDevice, UInt32 ulCmd, UInt32 size, ref T pvData)
        {
            byte[] data = new byte[size];
            Int32 lret = _xSysdeviceInfo(hSysDevice, ulCmd, size, data);
            IntPtr ptr = Marshal.AllocHGlobal((int)size);
            Marshal.Copy(data, 0, ptr, (int)size);

            pvData = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);
            return lret;
        }


        ///<exclude/>
        public static Int32 xSysdeviceDownload(IntPtr hSysDevice, UInt32 ulChannel, UInt32 Mode, string FileName, ref byte[] abFileData)
        {
            return _xSysdeviceDownload(hSysDevice, ulChannel, Mode, FileName, abFileData, (UInt32)abFileData.Length,
                                        null, null, (UIntPtr)0);
        }
        ///<exclude/>
        public static Int32 xSysdeviceFindFirstFile(IntPtr hSysdevice, UInt32 ChannelNumber, ref CIFX_DIRECTORYENTRYtag CifXDirectoryEntry)
        {
            return _xSysdeviceFindFirstFile(hSysdevice, ChannelNumber, ref CifXDirectoryEntry, null, (UIntPtr)0);
        }
        ///<exclude/>
        public static Int32 xSysdeviceFindNextFile(IntPtr hSysdevice, UInt32 ChannelNumber, ref CIFX_DIRECTORYENTRYtag CifXDirectoryEntry)
        {
            return _xSysdeviceFindNextFile(hSysdevice, ChannelNumber, ref CifXDirectoryEntry, null, (UIntPtr)0);
        }
        ///<exclude/>
        public static Int32 xSysdeviceGetMBXState(IntPtr hSysDevice, ref UInt32 RcvPktCnt, ref UInt32 SndPktCnt)
        {
            return _xSysdeviceGetMBXState(hSysDevice, ref RcvPktCnt, ref SndPktCnt);
        }
        ///<exclude/>
        public static Int32 xSysdevicePutPacket(IntPtr hSysDevice, ref CIFX_PACKETtag tPacket, UInt32 Timeout)
        {
            return _xSysdevicePutPacket(hSysDevice, ref tPacket, Timeout);
        }
        ///<exclude/>
        public static Int32 xSysdeviceGetPacket(IntPtr hSysDevice, ref CIFX_PACKETtag tPacket, UInt32 Timeout)
        {
            return _xSysdeviceGetPacket(hSysDevice, (UInt32)Marshal.SizeOf(tPacket), ref tPacket, Timeout);
        }
        ///<exclude/>
        public static Int32 xSysdeviceExtendedMemory(IntPtr hSysdevice, UInt32 ulCmd, ref CIFX_EXTENDED_MEMORY_INFORMATIONtag ptExtMemData)
        {
            return _xSysdeviceExtendedMemory(hSysdevice, ulCmd, ref ptExtMemData);
        }
        #endregion

        #region Channel Specific Functions
        ///<exclude/>
        public static Int32 xChannelInfo(IntPtr hChannel, UInt32 size, ref CHANNEL_INFORMATIONtag ChannelInformation)
        {
            return _xChannelInfo(hChannel, size, ref ChannelInformation);
        }
        ///<exclude/>
        public static Int32 xChannelOpen(IntPtr hDriver, string BoardName, UInt32 ChannelNumber, ref IntPtr phChannel)
        {
            return _xChannelOpen(hDriver, BoardName, ChannelNumber, ref phChannel);
        }
        ///<exclude/>
        public static Int32 xChannelClose(IntPtr hChannel)
        {
            return _xChannelClose(hChannel);
        }
        ///<exclude/>
        public static Int32 xChannelReset(IntPtr hChannel, UInt32 Command, UInt32 Timeout)
        {
            return _xChannelReset(hChannel, Command, Timeout);
        }
        ///<exclude/>
        public static Int32 xChannelDownload(IntPtr hChannel, UInt32 Mode, string FileName, ref byte[] abFileData, PFN_PROGRESS_CALLBACK pfnCallback, PFN_RECV_PKT_CALLBACK pfnRecvPktCallback, UIntPtr pvUser)
        {
            pFN_PROGRESS_CALLBACK = pfnCallback;
            pFN_RECV_PKT_CALLBACK = pfnRecvPktCallback;
            return _xChannelDownload(hChannel, Mode, FileName, abFileData, (UInt32)abFileData.Length, pfnCallback, pfnRecvPktCallback, pvUser);
        }
        ///<exclude/>
        public static Int32 xChannelUpload(IntPtr hChannel, UInt32 Mode, string FileName, UInt32 FileSize, ref byte[] abFileData, PFN_PROGRESS_CALLBACK pfnCallback, PFN_RECV_PKT_CALLBACK pfnRecvPktCallback, UIntPtr pvUser)
        {
            pFN_PROGRESS_CALLBACK = pfnCallback;
            pFN_RECV_PKT_CALLBACK = pfnRecvPktCallback;
            return _xChannelUpload(hChannel, Mode, FileName, abFileData, FileSize, pfnCallback, pfnRecvPktCallback, pvUser);
        }
        ///<exclude/>
        public static Int32 xChannelGetMBXState(IntPtr hChannel, ref UInt32 RcvPktCnt, ref UInt32 SndPktCnt)
        {
            return _xChannelGetMBXState(hChannel, ref RcvPktCnt, ref SndPktCnt);
        }
        ///<exclude/>
        public static Int32 xChannelHostState(IntPtr hChannel, UInt32 Command, ref UInt32 pulState, UInt32 Timeout)
        {
            return _xChannelHostState(hChannel, Command, ref pulState, Timeout);
        }
        ///<exclude/>
        public static Int32 xChannelBusState(IntPtr hChannel, UInt32 Command, ref UInt32 pulState, UInt32 Timeout)
        {
            return _xChannelBusState(hChannel, Command, ref pulState, Timeout);
        }
        ///<exclude/>
        public static Int32 xChannelConfigLock(IntPtr hChannel, UInt32 Command, ref UInt32 pulState, UInt32 Timeout)
        {
            return _xChannelConfigLock(hChannel, Command, ref pulState, Timeout);
        }
        ///<exclude/>
        public static Int32 xChannelWatchdog(IntPtr hChannel, UInt32 Command, ref UInt32 pulTrigger)
        {
            return _xChannelWatchdog(hChannel, Command, ref pulTrigger);
        }
        ///<exclude/>
        public static Int32 xChannelFindFirstFile(IntPtr hChannel, ref CIFX_DIRECTORYENTRYtag CifXDirectoryEntry, UIntPtr pvUserParam)
        {
            return _xChannelFindFirstFile(hChannel, ref CifXDirectoryEntry, null, pvUserParam);
        }
        ///<exclude/>
        public static Int32 xChannelFindNextFile(IntPtr hChannel, ref CIFX_DIRECTORYENTRYtag CifXDirectoryEntry, UIntPtr pvUserParam)
        {
            return _xChannelFindNextFile(hChannel, ref CifXDirectoryEntry, null, pvUserParam);
        }
        ///<exclude/>
        public static Int32 xChannelIORead(IntPtr hChannel, UInt32 AreaNumber, UInt32 Offset, UInt32 DataLen, ref byte[] pvData, UInt32 Timeout)
        {
            return _xChannelIORead(hChannel, AreaNumber, Offset, DataLen, pvData, Timeout);
        }
        ///<exclude/>
        public static Int32 xChannelIOWrite(IntPtr hChannel, UInt32 AreaNumber, UInt32 Offset, UInt32 DataLen, ref byte[] pvData, UInt32 Timeout)
        {
            return _xChannelIOWrite(hChannel, AreaNumber, Offset, DataLen, pvData, Timeout);
        }
        ///<exclude/>
        public static Int32 xChannelIOInfo(IntPtr hChannel, UInt32 Command, UInt32 AreaNumber, UInt32 size, ref CHANNEL_IO_INFORMATIONtag ChannelIOInformation)
        {
            return _xChannelIOInfo(hChannel, Command, AreaNumber, size, ref ChannelIOInformation);
        }
        ///<exclude/>
        public static Int32 xChannelPutPacket(IntPtr hChannel, ref CIFX_PACKETtag tPacket, UInt32 Timeout)
        {
            return _xChannelPutPacket(hChannel, ref tPacket, Timeout);
        }
        ///<exclude/>
        public static Int32 xChannelGetPacket(IntPtr hChannel, ref CIFX_PACKETtag tPacket, UInt32 Timeout)
        {
            return _xChannelGetPacket(hChannel, (UInt32)Marshal.SizeOf(tPacket), ref tPacket, Timeout);
        }
        ///<exclude/>
        public static Int32 xChannelControlBlock(IntPtr hChannel, ref byte[] DataBuffer, UInt32 Command, UInt32 Offset, UInt32 DataLen)
        {
            return _xChannelControlBlock(hChannel, Command, Offset, DataLen, DataBuffer);
        }
        ///<exclude/>
        public static Int32 xChannelCommonStatusBlock(IntPtr hChannel, ref byte[] DataBuffer, UInt32 Command, UInt32 Offset, UInt32 DataLen)
        {
            return _xChannelCommonStatusBlock(hChannel, Command, Offset, DataLen, DataBuffer);
        }
        ///<exclude/>
        public static Int32 xChannelExtendedStatusBlock(IntPtr hChannel, ref byte[] DataBuffer, UInt32 Command, UInt32 Offset, UInt32 DataLen)
        {
            return _xChannelExtendedStatusBlock(hChannel, Command, Offset, DataLen, DataBuffer);
        }
        ///<exclude/>
        public static Int32 xChannelUserBlock(IntPtr hChannel, UInt32 ulAreaNumber, UInt32 ulCmd, UInt32 ulOffset, UInt32 ulDataLen, [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pvData)
        {
            return _xChannelUserBlock(hChannel, ulAreaNumber, ulCmd, ulOffset, ulDataLen, pvData);
        }
        ///<exclude/>
        public static Int32 xChannelPLCMemoryPtr(IntPtr hChannel, UInt32 ulCmd, ref PLC_MEMORY_INFORMATIONtag pvMemoryInfo)
        {
            return _xChannelPLCMemoryPtr(hChannel, ulCmd, ref pvMemoryInfo);
        }
        ///<exclude/>
        public static Int32 xChannelPLCIsReadReady(IntPtr hChannel, UInt32 ulAreaNumber, ref UInt32 pulReadState)
        {
            return _xChannelPLCIsReadReady(hChannel, ulAreaNumber, ref pulReadState);
        }
        ///<exclude/>
        public static Int32 xChannelPLCIsWriteReady(IntPtr hChannel, UInt32 ulAreaNumber, ref UInt32 pulWriteState)
        {
            return _xChannelPLCIsWriteReady(hChannel, ulAreaNumber, ref pulWriteState);
        }
        ///<exclude/>
        public static Int32 xChannelPLCActivateRead(IntPtr hChannel, UInt32 ulAreaNumber)
        {
            return _xChannelPLCActivateRead(hChannel, ulAreaNumber);
        }
        ///<exclude/>
        public static Int32 xChannelPLCActivateWrite(IntPtr hChannel, UInt32 ulAreaNumber)
        {
            return _xChannelPLCActivateWrite(hChannel, ulAreaNumber);
        }
        ///<exclude/>
        public static Int32 xChannelRegisterNotification(IntPtr hChannel, UInt32 ulNotification, PFN_NOTIFY_CALLBACK pfnCallback, UIntPtr pvUser)
        {
            pFN_NOTIFY_CALLBACKs[ulNotification - 1] = pfnCallback;
            return _xChannelRegisterNotification(hChannel, ulNotification, pfnCallback, pvUser);
        }
        ///<exclude/>
        public static Int32 xChannelUnregisterNotification(IntPtr hChannel, UInt32 ulNotification)
        {
            pFN_NOTIFY_CALLBACKs[ulNotification - 1] = null;
            return _xChannelUnregisterNotification(hChannel, ulNotification);
        }
        #endregion
    }
}
