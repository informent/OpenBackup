using System.IO;
namespace OpenBackup;
public static class RetentionEngine
{
    public static int RemoveOlderSnapshots(string destination, int keep)
    {
        if (!Directory.Exists(destination) || keep < 1) return 0;
        var folders = Directory.EnumerateDirectories(destination).Where(x => File.Exists(Path.Combine(x, "manifest.json"))).OrderByDescending(x => x).ToArray(); var removed = 0;
        foreach (var folder in folders.Skip(keep)) { Directory.Delete(folder, true); removed++; }
        return removed;
    }
}
