using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Gml.Launcher.Models;
using Gml.Web.Api.Domains.System;
using Hardware.Info;
using static System.OperatingSystem;

namespace Gml.Launcher.Core.Services;

public class SystemService : ISystemService
{
    private const string NotSupportedMessage = "The operating system is not supported.";
    private readonly HardwareInfo _hardwareInfo = new();
    const int ERROR_DISK_FULL = 0x70;

    public ulong GetMaxRam()
    {
        if (!(IsWindows() || IsLinux() || IsMacOS())) throw new NotSupportedException(NotSupportedMessage);

        _hardwareInfo.RefreshMemoryStatus();

        return _hardwareInfo.MemoryStatus.TotalPhysical / (1024 * 1024);
    }

    public string GetApplicationFolder()
    {
        if (IsWindows()) return Path.GetFullPath(GetFolderPath(Environment.SpecialFolder.ApplicationData));

        if (IsLinux() || IsMacOS()) return Path.GetFullPath(GetFolderPath(Environment.SpecialFolder.UserProfile));

        throw new NotSupportedException(NotSupportedMessage);
    }

    public string GetGameFolder(string additionalPath, bool needCreate)
    {
        var directoryInfo = new DirectoryInfo(Path.Combine(GetApplicationFolder(), additionalPath));

        if (needCreate && !directoryInfo.Exists)
            directoryInfo.Create();

        return directoryInfo.FullName;
    }

    public string GetHwid()
    {
        // Данные железа собираются в фоне со старта; ждём их только когда реально нужен HWID
        try
        {
            (_systemDataTask ?? StartSystemDataLoading()).GetAwaiter().GetResult();
        }
        catch
        {
            // WMI может быть недоступен — HWID соберётся из того, что есть
        }

        var cpuIdentifier = _hardwareInfo.CpuList.FirstOrDefault()?.ProcessorId ?? "NOT_FOUND";
        var motherboardIdentifier = _hardwareInfo.MotherboardList.FirstOrDefault()?.SerialNumber ?? "NOT_FOUND";

        var diskIdentifiers = string.Join("-", _hardwareInfo.DriveList.Select(d => d.SerialNumber));

        return $"{cpuIdentifier}-{motherboardIdentifier}-{diskIdentifiers}";
    }

    private readonly object _systemDataSync = new();
    private Task? _systemDataTask;

    /// <summary>
    /// Запускает сбор данных о железе в фоне и сразу возвращает управление: WMI-запросы на Windows
    /// могут занимать десятки секунд, а сплэш-экран раньше ждал их полностью.
    /// </summary>
    public Task LoadSystemData()
    {
        StartSystemDataLoading();
        return Task.CompletedTask;
    }

    private Task StartSystemDataLoading()
    {
        lock (_systemDataSync)
        {
            return _systemDataTask ??= Task.WhenAll([
                Task.Run(() => _hardwareInfo.RefreshDriveList()),
                Task.Run(() => _hardwareInfo.RefreshMotherboardList()),
                // Загрузку CPU (два замера с паузой) для HWID считать не нужно
                Task.Run(() => _hardwareInfo.RefreshCPUList(includePercentProcessorTime: false))
            ]);
        }
    }

    public bool IsDiskFull(IOException ioException)
    {
        int hr = Marshal.GetHRForException(ioException);
        return (hr & 0xFFFF) == ERROR_DISK_FULL;
    }

    public string GetOsArchitecture()
    {
        return RuntimeInformation.OSArchitecture.ToString().ToLower().Replace("x", "");
    }

    public OsType GetOsType()
    {
        if (IsWindows()) return OsType.Windows;

        if (IsLinux()) return OsType.Linux;

        if (IsMacOS()) return OsType.OsX;

        return OsType.Undefined;
    }

    public IEnumerable<Language> GetAvailableLanguages()
    {
        return
        [
            new Language("/Assets/Images/lang-ru.svg", "Русский", new CultureInfo("ru-RU")),
            new Language("/Assets/Images/lang-us.svg", "English", new CultureInfo("en-US")),
            new Language("/Assets/Images/lang-zh.svg", "中文", new CultureInfo("zh-CN"))
        ];
    }

    private static string GetFolderPath(Environment.SpecialFolder folder)
    {
        return Environment.GetFolderPath(folder);
    }
}
