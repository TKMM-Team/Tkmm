using System.Text;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using TkSharp;
using TkSharp.Core;
using TkSharp.Core.Models;
using TkSharp.IO.Writers;
using TkSharp.Merging;
using TkSharp.Packaging;

namespace Tkmm.Components;

public static class TkProjectExporter
{
	private static readonly TkThumbnailProvider ThumbnailProvider = TkThumbnailProvider.Instance;
	private static string? _modsFolderPath;
	private static TkProject? _project;

	public static async Task ExportAsync(TkMod mod, ITkSystemProvider systemProvider, ITkRom rom,
		string outputProjectFolderPath, CancellationToken ct = default)
	{
		if (_modsFolderPath is null && systemProvider is TkModManager mgr) {
			_modsFolderPath = mgr.ModsFolderPath;
		}
		Directory.CreateDirectory(outputProjectFolderPath);

		_project = new TkProject(outputProjectFolderPath) {
			Mod = new TkMod {
				Id = mod.Id,
				Name = mod.Name,
				Description = mod.Description,
				Version = mod.Version,
				Author = mod.Author,
				Changelog = mod.Changelog,
				Thumbnail = mod.Thumbnail
			}
		};

		await RebuildContentAsync(mod, rom, outputProjectFolderPath, ct);
		CleanResourceTables(Path.Combine(outputProjectFolderPath, "romfs"));
		
		var baseExefs = Path.Combine(outputProjectFolderPath, "exefs");
		
		if (Directory.Exists(baseExefs)) {
			Directory.Delete(baseExefs, true);
		}
		
		if (mod.Changelog.PatchFiles.Count > 0 || mod.Changelog.ExeFiles.Count > 0) {
			CopyExefsFolder(mod.Id.ToString(), baseExefs);
		}

		await RebuildOptionsAsync(mod, rom, outputProjectFolderPath, ct);
		await RestoreThumbnailsAsync(mod, outputProjectFolderPath, ct);

		DeleteEmptyDirectories(outputProjectFolderPath);

		_project.Save();
	}

	private static async Task RebuildContentAsync(TkMod mod, ITkRom rom, string projectFolderPath, CancellationToken ct)
	{
		FolderModWriter writer = new(projectFolderPath);
		TkMerger merger = new(writer, rom);
		await merger.MergeAsync([mod.Changelog], ct);
	}

	private static void DeleteEmptyDirectories(string rootFolder)
	{
		try {
			if (!Directory.Exists(rootFolder)) {
				return;
			}

			foreach (var dir in Directory.GetDirectories(rootFolder, "*", SearchOption.AllDirectories)
				.OrderByDescending(p => p.Length)) {
				if (!Directory.EnumerateFileSystemEntries(dir).Any()) {
					Directory.Delete(dir, false);
				}
			}
		}
		catch (Exception ex) {
			TkLog.Instance.LogWarning(ex, "Failed to clean up empty directories in '{Root}'", rootFolder);
		}
	}

	private static async Task RebuildOptionsAsync(TkMod mod, ITkRom rom, string projectFolderPath, CancellationToken ct)
	{
		foreach (var group in mod.OptionGroups) {
			var groupFolderName = SanitizeFolderName(group.Name);
			var groupFolderPath = Path.Combine(projectFolderPath, "options", groupFolderName);
			
			Directory.CreateDirectory(groupFolderPath);

			var clonedGroup = new TkModOptionGroup {
				Name = group.Name,
				Description = group.Description,
				Thumbnail = group.Thumbnail,
				Type = group.Type,
				IconName = group.IconName,
				Priority = group.Priority
			};
			_project!.RegisterItem(clonedGroup, groupFolderPath);
			_project.Mod.OptionGroups.Add(clonedGroup);

			foreach (var option in group.Options) {
				var optionFolderName = SanitizeFolderName(option.Name);
				var optionFolderPath = Path.Combine(groupFolderPath, optionFolderName);

				Directory.CreateDirectory(optionFolderPath);

				_project!.RegisterItem(option, optionFolderPath);
				clonedGroup.Options.Add(option);

				FolderModWriter writer = new(projectFolderPath);
				writer.SetRelativeFolder(Path.Combine("options", groupFolderName, optionFolderName));

				TkMerger merger = new(writer, rom);
				await merger.MergeAsync([option.Changelog], ct);

				CleanResourceTables(Path.Combine(optionFolderPath, "romfs"));

				var optionExefs = Path.Combine(optionFolderPath, "exefs");

				if (Directory.Exists(optionExefs)) {
					Directory.Delete(optionExefs, true);
				}

				if (option.Changelog.PatchFiles.Count > 0 || option.Changelog.ExeFiles.Count > 0) {
					CopyExefsFolder(mod.Id.ToString(), optionExefs, option.Id.ToString());
				}
			}
		}
	}

	private static void CleanResourceTables(string romfsRootPath)
	{
		try {
			var systemFolder = Path.Combine(romfsRootPath, "System");
			var resourceFolder = Path.Combine(systemFolder, "Resource");

			if (Directory.Exists(resourceFolder)) {
				Directory.Delete(resourceFolder, true);
			}
		}
		catch (Exception ex) {
			TkLog.Instance.LogWarning(ex, "Failed to clean resource tables in '{RomfsRoot}'", romfsRootPath);
		}
	}

	private static void CopyExefsFolder(string itemId, string targetExefs, string? optionId = null)
	{
		if (_modsFolderPath is not { } modsPath) {
			return;
		}
		
		var sourceExefsPath = optionId is null
			? Path.Combine(modsPath, itemId, "exefs")
			: Path.Combine(modsPath, itemId, optionId, "exefs");

		if (!Directory.Exists(sourceExefsPath)) {
			return;
		}
		
		Directory.CreateDirectory(targetExefs);
		
		foreach (var filePath in Directory.GetFiles(sourceExefsPath)) {
			var dest = Path.Combine(targetExefs, Path.GetFileName(filePath));
			File.Copy(filePath, dest, true);
		}
	}

	private static async Task RestoreThumbnailsAsync(TkMod mod, string projectFolderPath, CancellationToken ct)
	{
		await ThumbnailProvider.ResolveThumbnail(mod, ct);
		await SaveThumbnailToFolder(mod, projectFolderPath);

		foreach (var group in mod.OptionGroups) {
			var groupFolderPath = Path.Combine(projectFolderPath, "options", SanitizeFolderName(group.Name));
			await SaveThumbnailToFolder(group, groupFolderPath);

			foreach (var option in group.Options) {
				var optionFolderPath = Path.Combine(groupFolderPath, SanitizeFolderName(option.Name));
				await SaveThumbnailToFolder(option, optionFolderPath);
			}
		}
	}

	private static async Task SaveThumbnailToFolder(TkItem item, string targetFolder)
	{
		if (item.Thumbnail?.Bitmap is not Bitmap bmp) {
			return;
		}

		Directory.CreateDirectory(targetFolder);
		var outputFile = Path.Combine(targetFolder, "thumbnail.png");
		
		await using var fs = File.Create(outputFile);
		bmp.Save(fs);
		item.Thumbnail.ThumbnailPath = outputFile;
	}

	private static string SanitizeFolderName(string name)
	{
		if (string.IsNullOrWhiteSpace(name)) {
			return "Untitled";
		}

		StringBuilder builder = new(name.Length);
		foreach (var c in name) {
			builder.Append(IsInvalid(c) ? '_' : c);
		}
		var result = builder.ToString().Trim();
		return string.IsNullOrEmpty(result) ? "Untitled" : result;

		static bool IsInvalid(char c)
		{
			return c is '"' or '*' or '/' or ':' or '<' or '>' or '?' or '\\' or '|' || char.IsControl(c);
		}
	}
}