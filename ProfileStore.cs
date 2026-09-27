using System.Text.Json;
using System.IO;
namespace OpenBackup;
public sealed record BackupProfile(string? Source, string? Destination, BackupSchedule Schedule, DateTime? LastRunLocal, int RetentionCount = 10);
public static class ProfileStore
{
    public static string PathOnDisk => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenBackup", "profile.json");
    public static BackupProfile Load() { try { if (File.Exists(PathOnDisk)) return JsonSerializer.Deserialize<BackupProfile>(File.ReadAllText(PathOnDisk)) ?? Default(); } catch { } return Default(); }
    public static void Save(BackupProfile profile) { Directory.CreateDirectory(System.IO.Path.GetDirectoryName(PathOnDisk)!); File.WriteAllText(PathOnDisk, JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true })); }
    public static BackupProfile Default() => new(null, null, new BackupSchedule(false, TimeSpan.FromDays(1), DateTime.Now), null, 10);
}
