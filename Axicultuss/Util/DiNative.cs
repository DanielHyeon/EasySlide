using Windows.Win32;
using Windows.Win32.Devices.DeviceAndDriverInstallation;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using DeviceInfo = Windows.Win32.Devices.DeviceAndDriverInstallation.SP_DEVINFO_DATA;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Axicultus.Util;

#region Unmanaged

public unsafe class DiNative
{
    public static IntPtr RegisterDeviceNotification(IntPtr hRecipient, DEV_BROADCAST_DEVICEINTERFACE NotificationFilter, uint Flags)
    {
        ArgumentNullException.ThrowIfNull(NotificationFilter);
        // The native header includes the interface GUID and the first WCHAR of its variable-length name.
        var filter = new DEV_BROADCAST_DEVICEINTERFACE_W
        {
            dbcc_size = (uint)sizeof(DEV_BROADCAST_DEVICEINTERFACE_W),
            dbcc_devicetype = (uint)NotificationFilter.dbcc_devicetype,
            dbcc_classguid = NotificationFilter.dbcc_classguid
        };
        return PInvoke.RegisterDeviceNotification((HANDLE)hRecipient, &filter, (REGISTER_NOTIFICATION_FLAGS)Flags);
    }

    public static uint UnregisterDeviceNotification(IntPtr hHandle) => (uint)PInvoke.UnregisterDeviceNotification((HDEVNOTIFY)hHandle).Value;

    // The original API used a DWORD for the optional enumerator pointer. Zero is valid at either pointer size.
    public static IntPtr SetupDiGetClassDevs(ref Guid gClass, uint iEnumerator, IntPtr hParent, uint nFlags)
    {
        fixed (Guid* id = &gClass)
            return PInvoke.SetupDiGetClassDevs(id, (char*)OptionalPointer(iEnumerator), (HWND)hParent, (SETUP_DI_GET_CLASS_DEVS_FLAGS)nFlags);
    }

    private static nint OptionalPointer(uint value)
    {
        if (IntPtr.Size > sizeof(uint) && value != 0)
            throw new ArgumentOutOfRangeException(nameof(value), "A 32-bit value cannot represent a native pointer on this platform.");
        return (nint)(nuint)value;
    }

    public static int SetupDiDestroyDeviceInfoList(IntPtr lpInfoSet) => PInvoke.SetupDiDestroyDeviceInfoList((HDEVINFO)lpInfoSet).Value;

    public static bool SetupDiEnumDeviceInfo(IntPtr lpInfoSet, uint dwIndex, SP_DEVINFO_DATA devInfoData)
    {
        var entry = ToNative(devInfoData);
        bool success = PInvoke.SetupDiEnumDeviceInfo((HDEVINFO)lpInfoSet, dwIndex, devInfoData == null ? null : &entry);
        if (success)
        {
            devInfoData.cbSize = (int)entry.cbSize;
            devInfoData.classGuid = entry.ClassGuid;
            devInfoData.devInst = (int)entry.DevInst;
            devInfoData.reserved = entry.Reserved;
        }
        return success;
    }

    public static bool SetupDiGetDeviceRegistryProperty(IntPtr deviceSet, SP_DEVINFO_DATA device, uint property,
        byte[] buffer, out uint propertyType, out uint requiredSize)
    {
        var entry = ToNative(device);
        fixed (byte* data = buffer)
        fixed (uint* type = &propertyType, size = &requiredSize)
            return PInvoke.SetupDiGetDeviceRegistryProperty((HDEVINFO)deviceSet, device == null ? null : &entry,
                (SETUP_DI_REGISTRY_PROPERTY)property, type, data, (uint)(buffer?.Length ?? 0), size);
    }

    private static DeviceInfo ToNative(SP_DEVINFO_DATA device) => device == null ? default : new()
    {
        cbSize = (uint)device.cbSize, ClassGuid = device.classGuid, DevInst = (uint)device.devInst, Reserved = device.reserved
    };

    public static bool SetupDiGetDeviceRegistryProperty(IntPtr lpInfoSet, SP_DEVINFO_DATA DeviceInfoData, uint Property,
        uint PropertyRegDataType, StringBuilder PropertyBuffer, uint PropertyBufferSize, IntPtr RequiredSize)
    {
        nint typePointer = OptionalPointer(PropertyRegDataType);
        // REG_MULTI_SZ has two trailing NULs. StringBuilder capacity counts neither.
        long maxBytes = PropertyBuffer == null ? 0 : ((long)PropertyBuffer.Capacity + 2) * sizeof(char);
        int size = checked((int)Math.Min(PropertyBufferSize, maxBytes));
        byte[] buffer = size == 0 ? null : new byte[size];
        bool success = SetupDiGetDeviceRegistryProperty(lpInfoSet, DeviceInfoData, Property, buffer,
            out uint type, out uint requiredSize);
        int error = Marshal.GetLastPInvokeError();
        if (RequiredSize != 0) Marshal.WriteInt32(RequiredSize, (int)requiredSize);
        if (typePointer != 0 && success) Marshal.WriteInt32(typePointer, (int)type);
        if (!success)
        {
            Marshal.SetLastPInvokeError(error);
            return false;
        }
        return PropertyBuffer == null || CopyDevicePropertyText(type, buffer.AsSpan(0, checked((int)requiredSize)), PropertyBuffer);
    }

    internal static bool CopyDevicePropertyText(uint type, ReadOnlySpan<byte> data, StringBuilder destination)
    {
        // REG_SZ, REG_EXPAND_SZ and REG_MULTI_SZ are UTF-16. Binary properties use the byte[] overload.
        if (type is not (1 or 2 or 7) || data.Length % sizeof(char) != 0)
        {
            Marshal.SetLastPInvokeError(13); // ERROR_INVALID_DATA
            return false;
        }
        var text = MemoryMarshal.Cast<byte, char>(data).TrimEnd('\0');
        if (text.Length > destination.Capacity)
        {
            Marshal.SetLastPInvokeError(122); // ERROR_INSUFFICIENT_BUFFER
            return false;
        }
        destination.Clear().Append(text);
        return true;
    }

    public static unsafe bool SetupDiSetClassInstallParams(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData,
        ref SP_PROPCHANGE_PARAMS ClassInstallParams, int ClassInstallParamsSize)
    {
        var entry = ToNative(DeviceInfoData);
        DeviceInfo* device = DeviceInfoData == null ? null : &entry;
        if (ClassInstallParams is null)
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(ClassInstallParamsSize, 0);
            return PInvoke.SetupDiSetClassInstallParams((HDEVINFO)DeviceInfoSet, device, null, 0);
        }
        ArgumentOutOfRangeException.ThrowIfNotEqual(ClassInstallParamsSize, sizeof(Windows.Win32.Devices.DeviceAndDriverInstallation.SP_PROPCHANGE_PARAMS));
        var parameters = new Windows.Win32.Devices.DeviceAndDriverInstallation.SP_PROPCHANGE_PARAMS
        {
            ClassInstallHeader = new() { cbSize = (uint)ClassInstallParams.ClassInstallHeader.cbSize,
                InstallFunction = (DI_FUNCTION)ClassInstallParams.ClassInstallHeader.InstallFunction },
            StateChange = (SETUP_DI_STATE_CHANGE)ClassInstallParams.StateChange,
            Scope = (SETUP_DI_PROPERTY_CHANGE_SCOPE)ClassInstallParams.Scope,
            HwProfile = (uint)ClassInstallParams.HwProfile
        };
        return PInvoke.SetupDiSetClassInstallParams((HDEVINFO)DeviceInfoSet, device, &parameters.ClassInstallHeader, (uint)ClassInstallParamsSize);
    }

    public static bool SetupDiCallClassInstaller(uint InstallFunction, IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData)
    {
        var entry = ToNative(DeviceInfoData);
        return PInvoke.SetupDiCallClassInstaller((DI_FUNCTION)InstallFunction, (HDEVINFO)DeviceInfoSet, DeviceInfoData == null ? null : &entry);
    }

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
        public Guid dbcc_classguid;
    }

    //SP_DEVINFO_DATA
    [StructLayout(LayoutKind.Sequential)]
    public class SP_DEVINFO_DATA
    {
        public int cbSize;
        public Guid classGuid;
        public int devInst;
        public nuint reserved;
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
();
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
