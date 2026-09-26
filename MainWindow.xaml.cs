using System.IO;
using System.Windows;
using Forms = System.Windows.Forms;

namespace OpenBackup;

public partial class MainWindow : Window
{
    private string? source; private string? destination; private BackupSnapshot? selected;
    public MainWindow() => InitializeComponent();
    private void ChooseSource_Click(object sender, RoutedEventArgs e) { source = ChooseFolder("Choose the folder to back up"); if (source is not null) UpdateProfile(); }
    private void ChooseDestination_Click(object sender, RoutedEventArgs e) { destination = ChooseFolder("Choose where snapshots should be stored"); if (destination is not null) UpdateProfile(); }
    private string? ChooseFolder(string description) { using var dialog = new Forms.FolderBrowserDialog { Description = description }; return dialog.ShowDialog() == Forms.DialogResult.OK ? dialog.SelectedPath : null; }
    private void UpdateProfile() { ProfileName.Text = source is null || destination is null ? "Profile in progress" : new DirectoryInfo(source).Name; ProfilePath.Text = $"{source ?? "Source not selected"}  →  {destination ?? "Destination not selected"}"; }
    private void CreateSnapshot_Click(object sender, RoutedEventArgs e)
    {
        if (source is null || destination is null) { StatusText.Text = "Choose source and destination first"; return; }
        try { StatusText.Text = "Creating snapshot…"; selected = BackupEngine.CreateSnapshot(source, destination); StatusText.Text = "Snapshot verified"; LatestText.Text = selected.Id; FileCountText.Text = selected.Files.Count.ToString("N0"); VerificationText.Text = $"{selected.Files.Count:N0} files copied with SHA-256 manifest."; RefreshSnapshots(); } catch (Exception ex) { StatusText.Text = "Backup failed"; VerificationText.Text = ex.Message; }
    }
    private void RefreshSnapshots() { if (destination is null) return; SnapshotList.ItemsSource = Directory.EnumerateDirectories(destination).OrderByDescending(x => x).Select(Path.GetFileName).ToArray(); }
    private void SnapshotList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) { if (destination is null || SnapshotList.SelectedItem is not string id) return; try { selected = BackupEngine.PreviewRestore(Path.Combine(destination, id)); SelectedText.Text = id; VerificationText.Text = $"{selected.Files.Count:N0} files · source {selected.Source}\nRestore preview only; no files will be changed."; } catch (Exception ex) { VerificationText.Text = ex.Message; } }
    private void VerifySnapshot_Click(object sender, RoutedEventArgs e) { if (destination is null || selected is null) { VerificationText.Text = "Select a snapshot first."; return; } var ok = BackupEngine.VerifySnapshot(Path.Combine(destination, selected.Id)); VerificationText.Text = ok ? "Integrity verified. Every manifest entry matches." : "Verification failed. One or more files differ."; StatusText.Text = ok ? "Verified" : "Integrity issue"; }
}
