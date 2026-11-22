using Microsoft.Win32;

namespace Axicultus.Util;

internal static class MonitorRegistrySettings
{
    private const string MonitorSection = "monitors";
    private const string LegacySection = "options";

    private static readonly string[] DualMonitorValues =
    [
        RegUtil.Reg_DMAlwaysUse,
        RegUtil.Reg_DMOption,
        RegUtil.Reg_DMOptionCusLeft,
        RegUtil.Reg_DMOptionCusTop,
        RegUtil.Reg_DMOptionCusWidth,
    ];

    internal static void MigrateLegacyDualMonitorValues(RegistrySettings settings)
    {
        if (!HasLegacyDualMonitorValues(settings))
            return;

        foreach (string name in DualMonitorValues)
        {
            if (!settings.TryReadValue(MonitorSection, name, out object current))
                return;
            if (current != null)
                continue;

            if (!settings.TryReadValue(LegacySection, name, out object legacy))
                return;
            if (legacy != null && !settings.TryWriteValue(MonitorSection, name, DataUtil.ObjToInt(legacy), RegistryValueKind.DWord))
                return;
        }

        // 정리 중 실패하면 다음 실행에서 다시 시도할 수 있도록 이전 기준 값은 마지막에 삭제한다.
        foreach (string name in DualMonitorValues)
        {
            if (name != RegUtil.Reg_DMAlwaysUse && !settings.TryDeleteValue(LegacySection, name))
                return;
        }
        settings.TryDeleteValue(LegacySection, RegUtil.Reg_DMAlwaysUse);
    }

    internal static int ReadDualMonitorValue(RegistrySettings settings, string name, int defaultValue)
    {
        settings.TryReadValue(MonitorSection, name, out object current);
        if (current == null && HasLegacyDualMonitorValues(settings))
            settings.TryReadValue(LegacySection, name, out current);
        return current != null ? DataUtil.ObjToInt(current) : defaultValue;
    }

    internal static int ReadNotationsFontSize(RegistrySettings settings)
    {
        settings.TryReadValue(MonitorSection, RegUtil.Reg_LMNotationsFontSize, out object stored);
        if (stored == null)
            settings.TryReadValue(LegacySection, RegUtil.Reg_LMNotationsFontSize, out stored);
        int size = stored != null ? DataUtil.ObjToInt(stored) : 22;
        return size is >= 8 and <= 40 ? size : 20;
    }

    // Seconds between rotated items; a stored value outside 0..999 is read as 0.
    internal static int ReadRotateGap(RegistrySettings settings)
    {
        settings.TryReadValue(LegacySection, "RotateGap", out object stored);
        int gap = stored != null ? DataUtil.ObjToInt(stored) : 0;
        return gap is >= 0 and <= 999 ? gap : 0;
    }

    // A stored value outside the known gap screens means "None", which is what the options dialog shows for it.
    internal static Axicultus.Module.GapType ReadGapItemOption(RegistrySettings settings)
    {
        settings.TryReadValue(LegacySection, RegUtil.Reg_GapItemOption, out object stored);
        int value = stored != null ? DataUtil.ObjToInt(stored) : 0;
        return value is >= (int)Axicultus.Module.GapType.None and <= (int)Axicultus.Module.GapType.User
            ? (Axicultus.Module.GapType)value
            : Axicultus.Module.GapType.None;
    }

    internal static void SaveNotationsFontSize(RegistrySettings settings, int size) =>
        settings.TryWriteValue(MonitorSection, RegUtil.Reg_LMNotationsFontSize, size, RegistryValueKind.DWord);

    private static bool HasLegacyDualMonitorValues(RegistrySettings settings) =>
        settings.TryReadValue(LegacySection, RegUtil.Reg_DMAlwaysUse, out object marker)
        && int.TryParse(marker?.ToString(), out int alwaysUseValue) && alwaysUseValue >= 0;
}
