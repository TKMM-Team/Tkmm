#if !SWITCH
using System.Runtime.Versioning;
using Tkmm.Core;
using Tkmm.Core.Helpers;

namespace Tkmm.Helpers;

public interface ISdExportTarget : IDisposable
{
    string Label { get; }
    string LocalIpsDirectory { get; }

    void Publish(
        string mergeOutputFolder,
        bool useRomfsLite,
        IProgress<(int Copied, int Total)>? progress,
        Action? wipeCompleted = null);
}

public static class SdExportTarget
{
    public static ISdExportTarget FromFileSystem(string rootPath)
        => new FileSystemSdExportTarget(rootPath);

    [SupportedOSPlatform("windows")]
    public static ISdExportTarget FromMtp(string deviceId, string deviceName, string mtpRootPath)
        => new MtpSdExportTarget(deviceId, deviceName, mtpRootPath);
}

file sealed class FileSystemSdExportTarget(string rootPath) : ISdExportTarget
{
    private readonly string _rootPath = rootPath;

    public string Label { get; } = rootPath;

    public string LocalIpsDirectory { get; } = Path.Combine(rootPath, "atmosphere", "exefs_patches", "TKMM");

    public void Publish(
        string mergeOutputFolder,
        bool useRomfsLite,
        IProgress<(int Copied, int Total)>? progress,
        Action? wipeCompleted = null)
    {
        var contentPath = Path.Combine(_rootPath, "atmosphere", "contents", "0100F2C0115B6000");
        Directory.CreateDirectory(contentPath);
        TKMM.EmptyMergeOutput(contentPath);
        wipeCompleted?.Invoke();
        DirectoryHelper.CopyMergeOutput(mergeOutputFolder, contentPath, useRomfsLite, overwrite: true, progress);
    }

    public void Dispose()
    {
    }
}

[SupportedOSPlatform("windows")]
file sealed class MtpSdExportTarget : ISdExportTarget
{
    private readonly string _deviceId;
    private readonly string _deviceName;
    private readonly string _mtpRootPath;
    private readonly string _tempIpsDirectory;

    public MtpSdExportTarget(string deviceId, string deviceName, string mtpRootPath)
    {
        _deviceId = deviceId;
        _deviceName = deviceName;
        _mtpRootPath = mtpRootPath;
        _tempIpsDirectory = Path.Combine(Path.GetTempPath(), "tkmm", "mtp-ips", Ulid.NewUlid().ToString());
        Directory.CreateDirectory(_tempIpsDirectory);
        Label = string.IsNullOrWhiteSpace(deviceName)
            ? mtpRootPath
            : $"{deviceName}\\{mtpRootPath.Trim('\\')}";
    }

    public string Label { get; }

    public string LocalIpsDirectory => _tempIpsDirectory;

    public void Publish(
        string mergeOutputFolder,
        bool useRomfsLite,
        IProgress<(int Copied, int Total)>? progress,
        Action? wipeCompleted = null)
    {
        MtpSdCardHelper.Publish(
            _deviceId,
            _deviceName,
            _mtpRootPath,
            mergeOutputFolder,
            useRomfsLite,
            _tempIpsDirectory,
            progress,
            wipeCompleted);
    }

    public void Dispose()
    {
        try {
            if (Directory.Exists(_tempIpsDirectory)) {
                Directory.Delete(_tempIpsDirectory, recursive: true);
            }
        }
        catch {
            // ignored
        }
    }
}
#endif