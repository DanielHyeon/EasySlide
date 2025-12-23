using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Easislides.Util
{
    #region Unmanaged

    public class DiNative
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr RegisterDeviceNotification(IntPtr
        hRecipient, DEV_BROADCAST_DEVICEINTERFACE NotificationFilter, UInt32 Flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint UnregisterDeviceNotification(IntPtr
        hHandle);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid gClass,
        UInt32 iEnumerator, IntPtr hParent, UInt32 nFlags);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern int SetupDiDestroyDeviceInfoList(IntPtr
        lpInfoSet);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInfo(IntPtr lpInfoSet,
        UInt32 dwIndex, SP_DEVINFO_DATA devInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiGetDeviceRegistryProperty(IntPtr
        lpInfoSet, SP_DEVINFO_DATA DeviceInfoData, UInt32 Property, UInt32
        PropertyRegDataType, StringBuilder PropertyBuffer, UInt32 PropertyBufferSize,
        IntPtr RequiredSize);

        [DllImport("setupapi.dll", SetLastError = true, CharSet =
        CharSet.Auto)]
        public static extern bool SetupDiSetClassInstallParams(IntPtr
        DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, ref SP_PROPCHANGE_PARAMS
        ClassInstallParams, int ClassInstallParamsSize);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern Boolean SetupDiCallClassInstaller(UInt32
        InstallFunction, IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData);

        // Structure with information for RegisterDeviceNotification.
        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_HANDLE
        {
            public int dbch_size;
            public int dbch_devicetype;
            public int dbch_reserved;
            public IntPtr dbch_handle;
            public IntPtr dbch_hdevnotify;
            public Guid dbch_eventguid;
            public long dbch_nameoffset;
            public byte dbch_data;
            public byte dbch_data1;
        }

        // Struct for parameters of the WM_DEVICECHANGE message
        [StructLayout(LayoutKind.Sequential)]
        public class DEV_BROADCAST_DEVICEINTERFACE
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
        }

        //SP_DEVINFO_DATA
        [StructLayout(LayoutKind.Sequential)]
        public class SP_DEVINFO_DATA
        {
            public int cbSize;
            public Guid classGuid;
            public int devInst;
            public ulong reserved;
        };

        [StructLayout(LayoutKind.Sequential)]
        public class SP_DEVINSTALL_PARAMS
        {
            public int cbSize;
            public int Flags;
            public int FlagsEx;
            public IntPtr hwndParent;
            public IntPtr InstallMsgHandler;
            public IntPtr InstallMsgHandlerContext;
            public IntPtr FileQueue;
            public IntPtr ClassInstallReserved;
            public int Reserved;
            [MarshalAs(UnmanagedType.LPTStr)] public string DriverPath;
        };

        [StructLayout(LayoutKind.Sequential)]
        public class SP_PROPCHANGE_PARAMS
        {
            public SP_CLASSINSTALL_HEADER ClassInstallHeader = new
            SP_CLASSINSTALL_HEADER();
            public int StateChange;
            public int Scope;
            public int HwProfile;
        };

        [StructLayout(LayoutKind.Sequential)]
        public class SP_CLASSINSTALL_HEADER
        {
            public int cbSize;
            public int InstallFunction;
        };

        //Param

        public const int DIGCF_ALLCLASSES = (0x00000004);
        public const int DIGCF_PRESENT = (0x00000002);
        public const int INVALID_HANDLE_VALUE = -1;
        public const int SPDRP_DEVICEDESC = (0x00000000);
        public const int MAX_DEV_LEN = 1000;
        public const int DEVICE_NOTIFY_WINDOW_HANDLE = (0x00000000);
        public const int DEVICE_NOTIFY_SERVICE_HANDLE = (0x00000001);
        public const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = (0x00000004);
        public const int DBT_DEVTYP_DEVICEINTERFACE = (0x00000005);
        public const int DBT_DEVNODES_CHANGED = (0x0007);
        public const int WM_DEVICECHANGE = (0x0219);
        public const int DIF_PROPERTYCHANGE = (0x00000012);
        public const int DICS_FLAG_GLOBAL = (0x00000001);
        public const int DICS_FLAG_CONFIGSPECIFIC = (0x00000002);
        public const int DICS_ENABLE = (0x00000001);
        public const int DICS_DISABLE = (0x00000002);
        public const int SPDRP_FRIENDLYNAME = (0x0000000C);

    }

    #endregion

//    //The 2 functions I hope will work to one day disable/enable device
//    //Name: HW_Set_State
//    //Inputs: string[],bool
//    //Outputs: bool
//    //Errors: This method may log the following errors.
//    // NONE
//    //Remarks: This is code I cobbled together from a number of newsgroup threads
//// as well as some C++ stuff I translated off of MSDN. Seems to work.
//// The idea is to find all device descriptions that contain all the
//// substrings and enable or disable them
//    public bool HW_Set_State(string[] match, bool bEnable)
//    {
//        try
//        {
//            Guid myGUID = System.Guid.Empty;
//            IntPtr hDevInfo = Native.SetupDiGetClassDevs(ref myGUID, 0,
//            IntPtr.Zero, Native.DIGCF_ALLCLASSES | Native.DIGCF_PRESENT);
//            if (hDevInfo.ToInt32() == Native.INVALID_HANDLE_VALUE)
//            {
//                return false;
//            }
//            Native.SP_DEVINFO_DATA DeviceInfoData;
//            DeviceInfoData = new Native.SP_DEVINFO_DATA();
//            DeviceInfoData.cbSize = 28;
//            //is devices exist for class
//            DeviceInfoData.devInst = 0;
//            DeviceInfoData.classGuid = System.Guid.Empty;
//            DeviceInfoData.reserved = 0;
//            UInt32 i;
//            StringBuilder DeviceName = new StringBuilder("");
//            DeviceName.Capacity = Native.MAX_DEV_LEN;
//            for (i = 0; Native.SetupDiEnumDeviceInfo(hDevInfo, i,
//            DeviceInfoData); i++)
//            {
//                //Declare vars
//                while (!Native.SetupDiGetDeviceRegistryProperty(hDevInfo,

//                DeviceInfoData,

//                Native.SPDRP_DEVICEDESC,
//                0,

//                DeviceName,

//                Native.MAX_DEV_LEN,

//                IntPtr.Zero))
//                {
//                    //Skip
//                }
//                bool bMatch = true;
//                foreach (string search in match)
//                {
//                    if
//                    (!DeviceName.ToString().ToLower().Contains(search.ToLower()))
//                    {
//                        bMatch = false;
//                        break;
//                    }
//                }
//                if (bMatch)
//                {
//                    ChangeDeviceState(hDevInfo, DeviceInfoData, bEnable);
//                }
//            }
//            Native.SetupDiDestroyDeviceInfoList(hDevInfo);
//        }
//        catch (Exception ex)
//        {
//            return false;
//        }
//        return true;
//    }


//    //Name: ChangeDeviceState
//    //Inputs: pointer to hdev, bool on or off
//    //Outputs: boll
//    //Errors: This method may log the following errors.
//    // NONE
//    //Remarks: Attempts to enable or disable a device driver.
//    private bool ChangeDeviceState(IntPtr hDevInfo,
//    Native.SP_DEVINFO_DATA devInfoData, bool bEnable)
//    {
//        try
//        {
//            Native.SP_PROPCHANGE_PARAMS pcp = new
//            Native.SP_PROPCHANGE_PARAMS();
//            if (bEnable)
//            {
//                pcp.ClassInstallHeader.cbSize = sizeof(int) * 2;
//                pcp.ClassInstallHeader.InstallFunction =
//                Native.DIF_PROPERTYCHANGE;
//                pcp.StateChange = Native.DICS_ENABLE;
//                pcp.Scope = Native.DICS_FLAG_GLOBAL;
//                pcp.HwProfile = 0;
//                if (Native.SetupDiSetClassInstallParams(hDevInfo, ref
//                devInfoData, ref pcp, sizeof(int) * 5))
//                {

//                    Native.SetupDiCallClassInstaller(Native.DIF_PROPERTYCHANGE, hDevInfo, ref
//                    devInfoData);
//                }
//                pcp.ClassInstallHeader.cbSize = sizeof(int) * 2;
//                pcp.ClassInstallHeader.InstallFunction =
//                Native.DIF_PROPERTYCHANGE;
//                pcp.StateChange = Native.DICS_ENABLE;
//                pcp.Scope = Native.DICS_FLAG_CONFIGSPECIFIC;
//                pcp.HwProfile = 0;
//            }
//            else
//            {
//                pcp.ClassInstallHeader.cbSize = sizeof(int) * 2;
//                pcp.ClassInstallHeader.InstallFunction =
//                Native.DIF_PROPERTYCHANGE;
//                pcp.StateChange = Native.DICS_DISABLE;
//                pcp.Scope = Native.DICS_FLAG_CONFIGSPECIFIC;
//                pcp.HwProfile = 0;
//            }
//            bool rslt1 = Native.SetupDiSetClassInstallParams(hDevInfo,
//            ref devInfoData, ref pcp, sizeof(int) * 5);
//            bool rstl2 =
//            Native.SetupDiCallClassInstaller(Native.DIF_PROPERTYCHANGE, hDevInfo, ref
//            devInfoData);
//            if ((!rslt1) || (!rstl2))
//            {
//                return false;
//            }
//            else
//            {
//                return true;
//            }
//        }
//        catch (Exception ex)
//        {
//            return false;
//        }
//    }

//    //Finally a simple sample of calling the method
//    string[] pattern_to_match = { "Intel", "Network" };
//    HW_Set_State(pattern_to_match, false);
//    authorwjf16 years ago
//    PermalinkAlright, as is often the case after all weekend of looking for the answer to
//    this, shortly after posting my problem I stumbled across the answer. For
//    anyone interested, I'll include it briefly below. In the future, I will make
//the entire library available via codeproject. Regards!

////First change the declaration of the two functions in the SDK
//    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
//public static extern bool SetupDiSetClassInstallParams(IntPtr
//DeviceInfoSet, IntPtr DeviceInfoData, IntPtr ClassInstallParams, int
//ClassInstallParamsSize);

//    [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
//    public static extern Boolean SetupDiCallClassInstaller(UInt32
//    InstallFunction, IntPtr DeviceInfoSet, IntPtr DeviceInfoData);

//    //Now marshall the params prior to calling the functions
//    //Name: ChangeDeviceState
//    //Inputs: pointer to hdev, bool on or off
//    //Outputs: boll
//    //Errors: This method may log the following errors.
//    // NONE
//    //Remarks: Attempts to enable or disable a device driver.
//    private bool ChangeDeviceState(IntPtr hDevInfo,
//    Native.SP_DEVINFO_DATA devInfoData, bool bEnable)
//    {
//        try
//        {
//            //Marshalling vars
//            int szOfPcp;
//            IntPtr ptrToPcp;
//            int szDevInfoData;
//            IntPtr ptrToDevInfoData;

//            Native.SP_PROPCHANGE_PARAMS pcp = new
//            Native.SP_PROPCHANGE_PARAMS();
//            if (bEnable)
//            {
//                pcp.ClassInstallHeader.cbSize =
//                Marshal.SizeOf(typeof(Native.SP_CLASSINSTALL_HEADER));
//                pcp.ClassInstallHeader.InstallFunction =
//                Native.DIF_PROPERTYCHANGE;
//                pcp.StateChange = Native.DICS_ENABLE;
//                pcp.Scope = Native.DICS_FLAG_GLOBAL;
//                pcp.HwProfile = 0;

//                //Marshal the params
//                szOfPcp = Marshal.SizeOf(pcp);
//                ptrToPcp = Marshal.AllocHGlobal(szOfPcp);
//                Marshal.StructureToPtr(pcp, ptrToPcp, true);
//                szDevInfoData = Marshal.SizeOf(devInfoData);
//                ptrToDevInfoData = Marshal.AllocHGlobal(szDevInfoData);

//                if (Native.SetupDiSetClassInstallParams(hDevInfo,
//                ptrToDevInfoData, ptrToPcp,
//                Marshal.SizeOf(typeof(Native.SP_PROPCHANGE_PARAMS))))
//                {

//                    Native.SetupDiCallClassInstaller(Native.DIF_PROPERTYCHANGE, hDevInfo,
//                    ptrToDevInfoData);
//                }
//                pcp.ClassInstallHeader.cbSize =
//                Marshal.SizeOf(typeof(Native.SP_CLASSINSTALL_HEADER));
//                pcp.ClassInstallHeader.InstallFunction =
//                Native.DIF_PROPERTYCHANGE;
//                pcp.StateChange = Native.DICS_ENABLE;
//                pcp.Scope = Native.DICS_FLAG_CONFIGSPECIFIC;
//                pcp.HwProfile = 0;
//            }
//            else
//            {
//                pcp.ClassInstallHeader.cbSize =
//                Marshal.SizeOf(typeof(Native.SP_CLASSINSTALL_HEADER));
//                pcp.ClassInstallHeader.InstallFunction =
//                Native.DIF_PROPERTYCHANGE;
//                pcp.StateChange = Native.DICS_DISABLE;
//                pcp.Scope = Native.DICS_FLAG_CONFIGSPECIFIC;
//                pcp.HwProfile = 0;
//            }
//            //Marshal the params
//            szOfPcp = Marshal.SizeOf(pcp);
//            ptrToPcp = Marshal.AllocHGlobal(szOfPcp);
//            Marshal.StructureToPtr(pcp, ptrToPcp, true);
//            szDevInfoData = Marshal.SizeOf(devInfoData);
//            ptrToDevInfoData = Marshal.AllocHGlobal(szDevInfoData);
//            Marshal.StructureToPtr(devInfoData, ptrToDevInfoData, true);

//            bool rslt1 = Native.SetupDiSetClassInstallParams(hDevInfo,
//            ptrToDevInfoData, ptrToPcp,
//            Marshal.SizeOf(typeof(Native.SP_PROPCHANGE_PARAMS)));
//            bool rstl2 =
//            Native.SetupDiCallClassInstaller(Native.DIF_PROPERTYCHANGE, hDevInfo,
//            ptrToDevInfoData);
//            if ((!rslt1) || (!rstl2))
//            {
//                return false;
//            }
//            else
//            {
//                return true;
//            }
//        }
//        catch (Exception ex)
//        {
//            return false;
//        }
//    }
}
