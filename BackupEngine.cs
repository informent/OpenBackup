using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace OpenBackup;

public sealed record BackupFile(string RelativePath, long Length, string Sha256);
public sealed record BackupSnapshot(string Id, DateTime CreatedUtc, string Source, string Destination, IReadOnlyList<BackupFile> Files);

public static class BackupEngine
{
    public static BackupSnapshot CreateSnapshot(string source, string destination)
    {
        if (!Directory.Exists(source)) throw new DirectoryNotFoundException(source);
        var id = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"); var root = Path.Combine(destination, id); Directory.CreateDirectory(root);
        var files = new List<BackupFile>();
        foreach (var path in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(source, path); var target = Path.Combine(root, relative); Directory.CreateDirectory(Path.GetDirectoryName(target)!); File.Copy(path, target, true);
            files.Add(new BackupFile(relative, new FileInfo(path).Length, Hash(path)));
        }
        var snapshot = new BackupSnapshot(id, DateTime.UtcNow, source, destination, files);
        File.WriteAllText(Path.Combine(root, "manifest.json"), JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true })); return snapshot;
    }
    public static BackupSnapshot PreviewRestore(string snapshotRoot)
    {
        var manifest = Path.Combine(snapshotRoot, "manifest.json"); if (!File.Exists(manifest)) throw new FileNotFoundException("Snapshot manifest not found", manifest);
        return JsonSerializer.Deserialize<BackupSnapshot>(File.ReadAllText(manifest)) ?? throw new InvalidDataException("Invalid snapshot manifest");
    }
    public static bool VerifySnapshot(string snapshotRoot)
    {
        var snapshot = PreviewRestore(snapshotRoot); return snapshot.Files.All(file => { var path = Path.Combine(snapshotRoot, file.RelativePath); return File.Exists(path) && new FileInfo(path).Length == file.Length && Hash(path).Equals(file.Sha256, StringComparison.OrdinalIgnoreCase); });
    }
    private static string Hash(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)); }
}
