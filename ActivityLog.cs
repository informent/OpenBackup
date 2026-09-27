using System.IO;
namespace OpenBackup;
public static class ActivityLog
{
    public static string PathOnDisk => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenBackup", "activity.log");
    public static void Write(string message) { Directory.CreateDirectory(System.IO.Path.GetDirectoryName(PathOnDisk)!); File.AppendAllText(PathOnDisk, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {message}{Environment.NewLine}"); }
}
