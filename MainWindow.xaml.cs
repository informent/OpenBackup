using System.IO;
using System.Windows;
using System.Windows.Controls;
using Forms = System.Windows.Forms;

namespace OpenBackup;

public partial class MainWindow : Window
{
    private string? source;
    private string? destination;
    private BackupSnapshot? selected;
    private BackupProfile profile = ProfileStore.Load();
    private static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenBackup", "schedule.json");

    public MainWindow()
    {
        InitializeComponent();
        source = profile.Source; destination = profile.Destination; if (source is not null || destination is not null) UpdateProfile();
        ScheduleInterval.SelectedIndex = 2;
        LoadSchedule();
    }

    private void ChooseSource_Click(object sender, RoutedEventArgs e) { source = ChooseFolder("Choose the folder to back up"); if (source is not null) { UpdateProfile(); SaveProfile(); } }
    private void ChooseDestination_Click(object sender, RoutedEventArgs e) { destination = ChooseFolder("Choose where snapshots should be stored"); if (destination is not null) { UpdateProfile(); SaveProfile(); } }
    private string? ChooseFolder(string description) { using var dialog = new Forms.FolderBrowserDialog { Description = description }; return dialog.ShowDialog() == Forms.DialogResult.OK ? dialog.SelectedPath : null; }
    private void UpdateProfile() { ProfileName.Text = source is null || destination is null ? "Profile in progress" : new DirectoryInfo(source).Name; ProfilePath.Text = $"{source ?? "Source not selected"}  →  {destination ?? "Destination not selected"}"; }

    private void CreateSnapshot_Click(object sender, RoutedEventArgs e)
    {
        if (source is null || destination is null) { StatusText.Text = "Choose source and destination first"; return; }
        try { StatusText.Text = "Creating snapshot…"; selected = BackupEngine.CreateSnapshot(source, destination); StatusText.Text = "Snapshot verified"; LatestText.Text = selected.Id; FileCountText.Text = selected.Files.Count.ToString("N0"); VerificationText.Text = $"{selected.Files.Count:N0} files copied with SHA-256 manifest."; RefreshSnapshots(); } catch (Exception ex) { StatusText.Text = "Backup failed"; VerificationText.Text = ex.Message; }
    }

    private void RefreshSnapshots() { if (destination is null) return; SnapshotList.ItemsSource = Directory.EnumerateDirectories(destination).OrderByDescending(x => x).Select(Path.GetFileName).ToArray(); }
    private void SnapshotList_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (destination is null || SnapshotList.SelectedItem is not string id) return; try { selected = BackupEngine.PreviewRestore(Path.Combine(destination, id)); SelectedText.Text = id; VerificationText.Text = $"{selected.Files.Count:N0} files · source {selected.Source}\nRestore preview only; no files will be changed."; } catch (Exception ex) { VerificationText.Text = ex.Message; } }
    private void VerifySnapshot_Click(object sender, RoutedEventArgs e) { if (destination is null || selected is null) { VerificationText.Text = "Select a snapshot first."; return; } var ok = BackupEngine.VerifySnapshot(Path.Combine(destination, selected.Id)); VerificationText.Text = ok ? "Integrity verified. Every manifest entry matches." : "Verification failed. One or more files differ."; StatusText.Text = ok ? "Verified" : "Integrity issue"; }
    private void RestoreSnapshot_Click(object sender, RoutedEventArgs e)
    {
        if (destination is null || selected is null) { VerificationText.Text = "Select a snapshot first."; return; }
        var target = ChooseFolder("Choose an empty folder for the restored files");
        if (target is null) return;
        try { var count = BackupEngine.RestoreSnapshot(Path.Combine(destination, selected.Id), target); VerificationText.Text = $"Restored {count:N0} files to {target}. Existing files were never overwritten."; StatusText.Text = "Restore complete"; } catch (Exception ex) { VerificationText.Text = ex.Message; StatusText.Text = "Restore stopped"; }
    }

    private void ScheduleChanged(object sender, RoutedEventArgs e)
    {
        if (ScheduleInterval.SelectedItem is not ComboBoxItem item || item.Tag is not string hoursText || !double.TryParse(hoursText, out var hours)) return;
        var schedule = new BackupSchedule(ScheduleEnabled.IsChecked == true, TimeSpan.FromHours(hours), DateTime.Now);
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        File.WriteAllText(SettingsPath, System.Text.Json.JsonSerializer.Serialize(schedule)); profile = profile with { Source = source, Destination = destination, Schedule = schedule }; ProfileStore.Save(profile);
        UpdateNextRun(schedule);
    }

    private void SaveProfile() => ProfileStore.Save(profile with { Source = source, Destination = destination });

    private void LoadSchedule()
    {
        try { if (File.Exists(SettingsPath)) { var schedule = System.Text.Json.JsonSerializer.Deserialize<BackupSchedule>(File.ReadAllText(SettingsPath)); if (schedule is not null) { ScheduleEnabled.IsChecked = schedule.Enabled; ScheduleInterval.SelectedIndex = schedule.Interval.TotalHours switch { 1 => 0, 6 => 1, 24 => 2, 168 => 3, _ => 2 }; UpdateNextRun(schedule); return; } } } catch { }
        ScheduleEnabled.IsChecked = false;
        UpdateNextRun(new BackupSchedule(false, TimeSpan.FromDays(1), DateTime.Now));
    }

    private void UpdateNextRun(BackupSchedule schedule)
    {
        var next = ScheduleEngine.NextRun(schedule, DateTime.Now);
        NextRunText.Text = next is null ? "Not scheduled" : $"Next snapshot {next.Value:ddd, MMM d · h:mm tt}";
    }
}
