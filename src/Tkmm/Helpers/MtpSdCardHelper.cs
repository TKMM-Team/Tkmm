#if !SWITCH
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using MediaDevices;
using Tkmm.Core;
using Tkmm.Core.Helpers;

namespace Tkmm.Helpers;

[SupportedOSPlatform("windows")]
public static class MtpSdCardHelper
{
    private const string CONTENTS_RELATIVE_PATH = "atmosphere/contents/0100F2C0115B6000";
    private const string IPS_RELATIVE_PATH = "atmosphere/exefs_patches/TKMM";
    private const int COPY_FLAGS = 4 | 16 | 512 | 1024; // SILENT | NOCONFIRMATION | NOCONFIRMMKDIR | NOERRORUI
    private const int IN_USE = unchecked((int)0x800700AA);
    private const int ACCESS_DENIED = unchecked((int)0x80070005);

    public static IEnumerable<(string DeviceId, string FriendlyName, string RootPath)> FindAtmosphereRoots()
    {
        if (!OperatingSystem.IsWindows()) {
            return [];
        }

        List<(string DeviceId, string FriendlyName, string RootPath)> roots = [];
        RunSta(() => {
            foreach (var device in MediaDevice.GetDevices()) {
                try {
                    device.Connect();
                    var name = DeviceName(device);
                    foreach (var root in AtmosphereRoots(device)) {
                        roots.Add((device.DeviceId, name, root));
                    }
                }
                catch {
                    // Ignore devices that cannot be queried.
                }
                finally {
                    TryDisconnect(device);
                    device.Dispose();
                }
            }
        });

        return roots;
    }

    public static void Publish(
        string deviceId,
        string deviceName,
        string mtpRootPath,
        string mergeOutputFolder,
        bool useRomfsLite,
        string? localIpsDirectory,
        IProgress<(int Copied, int Total)>? progress,
        Action? wipeCompleted)
    {
        if (!OperatingSystem.IsWindows()) {
            throw new PlatformNotSupportedException("MTP is only supported on Windows.");
        }

        RunSta(() => {
            var contentPath = Combine(mtpRootPath, CONTENTS_RELATIVE_PATH);
            var entries = DirectoryHelper.GetMergeExportEntries(mergeOutputFolder, useRomfsLite);
            var ipsFiles = Directory.Exists(localIpsDirectory) ? Directory.GetFiles(localIpsDirectory) : [];
            var total = DirectoryHelper.GetMergeExportFiles(mergeOutputFolder, useRomfsLite).Count + ipsFiles.Length;

            WithDevice(deviceId, device => {
                DeleteTargets(device, contentPath, TKMM.MergeOutputFolderTargets.Concat(entries));
                DeleteTargets(device, Combine(mtpRootPath, "atmosphere/exefs_patches"), ["TKMM"]);
            });
            wipeCompleted?.Invoke();

            dynamic shell = CreateShell();
            dynamic storage = ResolveStorage(shell, deviceName, mtpRootPath.Trim('\\'));
            dynamic contentFolder = EnsureFolder(storage, CONTENTS_RELATIVE_PATH);
            dynamic? ipsFolder = ipsFiles.Length > 0 ? EnsureFolder(storage, IPS_RELATIVE_PATH) : null;

            var completed = 0;
            progress?.Report((0, total));

            foreach (var entry in entries) {
                var localPath = Path.Combine(mergeOutputFolder, entry);
                if (!File.Exists(localPath) && !Directory.Exists(localPath)) {
                    continue;
                }

                var entryFiles = CountFiles(localPath);
                var completedBeforeEntry = completed;
                CopyHere(contentFolder, localPath);
                WaitForCopy(deviceId, Combine(contentPath, entry), entryFiles,
                    n => progress?.Report((completedBeforeEntry + n, total)));
                completed += entryFiles;
                progress?.Report((completed, total));
            }

            if (ipsFolder is null) {
                return;
            }

            foreach (var file in ipsFiles) {
                CopyHere(ipsFolder, file);
                WaitForCopy(deviceId, Combine(mtpRootPath, $"{IPS_RELATIVE_PATH}/{Path.GetFileName(file)}"), 1, _ => { });
                completed++;
                progress?.Report((completed, total));
            }
        });
    }

    private static void WithDevice(string deviceId, Action<MediaDevice> action)
        => WithDevice(deviceId, device => {
            action(device);
            return 0;
        });

    private static T WithDevice<T>(string deviceId, Func<MediaDevice, T> action)
    {
        using var device = MediaDevice.GetDevices().FirstOrDefault(d => d.DeviceId == deviceId)
            ?? throw new InvalidOperationException("The selected MTP device is no longer connected.");
        device.Connect();
        try {
            return action(device);
        }
        finally {
            TryDisconnect(device);
        }
    }

    private static void DeleteTargets(MediaDevice device, string directory, IEnumerable<string> targets)
    {
        foreach (var target in targets) {
            var path = Combine(directory, target);
            try {
                if (device.DirectoryExists(path)) {
                    device.DeleteDirectory(path, recursive: true);
                }
                else if (device.FileExists(path)) {
                    device.DeleteFile(path);
                }
            }
            catch {
                // Continue wiping remaining targets.
            }
        }
    }

    private static void WaitForCopy(string deviceId, string remotePath, int expectedFiles, Action<int> onProgress)
    {
        if (expectedFiles <= 0) {
            return;
        }

        var sw = Stopwatch.StartNew();
        var timeout = TimeSpan.FromMinutes(180);
        var lastCount = -1;
        var stableSince = (TimeSpan?)null;

        while (sw.Elapsed < timeout) {
            int count;
            try {
                count = WithDevice(deviceId, device => CountRemoteFiles(device, remotePath));
            }
            catch (Exception ex) when (IsTransient(ex)) {
                Thread.Sleep(1000);
                continue;
            }

            onProgress(Math.Min(count, expectedFiles));

            if (count != lastCount) {
                lastCount = count;
                stableSince = sw.Elapsed;
            }

            if (count >= expectedFiles && stableSince is { } since
                && sw.Elapsed - since >= TimeSpan.FromMilliseconds(500)) {
                onProgress(expectedFiles);
                return;
            }

            Thread.Sleep(400);
        }

        throw new TimeoutException(
            $"Timed out waiting for MTP copy of '{remotePath}' ({lastCount}/{expectedFiles} files).");
    }

    private static int CountRemoteFiles(MediaDevice device, string remotePath)
    {
        try {
            if (device.FileExists(remotePath)) {
                return 1;
            }

            if (!device.DirectoryExists(remotePath)) {
                return 0;
            }

            return device.EnumerateFiles(remotePath, "*", SearchOption.AllDirectories).Count();
        }
        catch (Exception ex) when (
            ex is DirectoryNotFoundException ||
            ex is FileNotFoundException ||
            ex is IOException ||
            IsTransient(ex))
        {
            return -1;
        }
    }

    private static int CountFiles(string path)
        => File.Exists(path) ? 1
            : Directory.Exists(path) ? Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Count()
            : 0;

    [SupportedOSPlatform("windows")]
    private static dynamic CreateShell()
    {
        var type = Type.GetTypeFromProgID("Shell.Application")
            ?? throw new InvalidOperationException("Shell.Application is unavailable.");
        return Activator.CreateInstance(type)
            ?? throw new InvalidOperationException("Failed to create Shell.Application.");
    }

    private static dynamic ResolveStorage(dynamic shell, string deviceName, string storageName)
    {
        dynamic computer = shell.NameSpace(0x11)
            ?? throw new InvalidOperationException("Could not open This PC.");
        dynamic device = FindChildFolder(computer, deviceName)
            ?? throw new DirectoryNotFoundException($"Could not find MTP device '{deviceName}' under This PC.");
        return FindChildFolder(device, storageName)
            ?? throw new DirectoryNotFoundException(
                $"Could not find storage '{storageName}' on MTP device '{deviceName}'.");
    }

    private static dynamic EnsureFolder(dynamic root, string relativePath)
    {
        dynamic current = root;
        foreach (var segment in relativePath.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries)) {
            var parent = current;
            current = RetryInUse(() => FindChildFolder(parent, segment) ?? CreateChildFolder(parent, segment));
        }

        return current;
    }

    private static dynamic CreateChildFolder(dynamic parent, string name)
    {
        parent.NewFolder(name);
        return FindChildFolder(parent, name)
            ?? throw new IOException($"Failed to create MTP folder '{name}'.");
    }

    private static void CopyHere(dynamic destFolder, string localPath)
        => RetryInUse(() => {
            destFolder.CopyHere(Path.GetFullPath(localPath), COPY_FLAGS);
            return 0;
        });

    private static dynamic? FindChildFolder(dynamic folder, string name)
    {
        foreach (var item in folder.Items()) {
            try {
                if (string.Equals((string)item.Name, name, StringComparison.OrdinalIgnoreCase)) {
                    return item.GetFolder;
                }
            }
            catch {
                // ignored
            }
        }

        return null;
    }

    private static IEnumerable<string> AtmosphereRoots(MediaDevice device)
    {
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
        foreach (var candidate in CandidateRoots(device)) {
            if (device.DirectoryExists(Combine(candidate, "atmosphere")) && seen.Add(candidate)) {
                yield return candidate;
            }
        }
    }

    private static IEnumerable<string> CandidateRoots(MediaDevice device)
    {
        yield return @"\";

        MediaDriveInfo[]? drives = null;
        try {
            drives = device.GetDrives();
        }
        catch {
            // ignored
        }

        if (drives is not null) {
            foreach (var drive in drives) {
                string? root = null;
                try {
                    root = drive.RootDirectory?.FullName;
                }
                catch {
                    // ignored
                }

                if (!string.IsNullOrWhiteSpace(root)) {
                    yield return Normalize(root);
                }
            }
        }

        string[] topLevel = [];
        try {
            topLevel = device.GetDirectories(@"\") ?? [];
        }
        catch {
            // ignored
        }

        foreach (var directory in topLevel) {
            yield return Normalize(directory);
        }
    }

    private static string DeviceName(MediaDevice device)
    {
        foreach (var getter in (Func<string?>[])[() => device.Description, () => device.FriendlyName, () => device.Model]) {
            try {
                var value = getter();
                if (!string.IsNullOrWhiteSpace(value)) {
                    return value;
                }
            }
            catch {
                // ignored
            }
        }

        try {
            return device.DeviceId;
        }
        catch {
            return string.Empty;
        }
    }

    private static string Combine(string root, string relative)
    {
        root = Normalize(root);
        relative = relative.Replace('\\', '/').Trim('/');
        return string.IsNullOrEmpty(relative) ? root
            : root is @"\" or "" ? @"\" + relative.Replace('/', '\\')
            : Normalize($"{root.TrimEnd('\\')}\\{relative.Replace('/', '\\')}");
    }

    private static string Normalize(string path)
    {
        path = path.Replace('/', '\\');
        return string.IsNullOrWhiteSpace(path) || path == @"\" ? @"\" : @"\" + path.Trim('\\');
    }

    private static T RetryInUse<T>(Func<T> action)
    {
        for (var attempt = 0; ; attempt++) {
            try {
                return action();
            }
            catch (COMException ex) when (ex.HResult == IN_USE && attempt < 90) {
                Thread.Sleep(1000);
            }
        }
    }

    private static bool IsTransient(Exception ex)
        => ex is UnauthorizedAccessException or COMException { HResult: IN_USE or ACCESS_DENIED };

    private static void TryDisconnect(MediaDevice device)
    {
        try {
            if (device.IsConnected) {
                device.Disconnect();
            }
        }
        catch {
            // ignored
        }
    }

    private static void RunSta(Action action)
    {
        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA) {
            action();
            return;
        }

        var error = new StrongBox<Exception?>(null);
        var thread = new Thread(() => {
            try {
                action();
            }
            catch (Exception ex) {
                error.Value = ex;
            }
        }) {
            IsBackground = true,
            Name = "TKMM MTP"
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (error.Value is not null) {
            ExceptionDispatchInfo.Capture(error.Value).Throw();
        }
    }
}
#endif