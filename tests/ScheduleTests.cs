using OpenBackup;
var start = new DateTime(2026, 1, 1, 8, 0, 0);
var next = ScheduleEngine.NextRun(new BackupSchedule(true, TimeSpan.FromHours(24), start), start.AddHours(25));
if (next != start.AddHours(48)) throw new Exception("Next-run calculation is incorrect.");
if (ScheduleEngine.NextRun(new BackupSchedule(false, TimeSpan.FromHours(1), start), start) is not null) throw new Exception("Disabled schedule returned a run.");
Console.WriteLine("PASS: schedule next-run calculation");
var root = Path.Combine(Path.GetTempPath(), "openbackup-scheduled-" + Guid.NewGuid().ToString("N")); var source = Path.Combine(root, "source"); var destination = Path.Combine(root, "snapshots"); Directory.CreateDirectory(source); File.WriteAllText(Path.Combine(source, "scheduled.txt"), "scheduled"); var profile = new BackupProfile(source, destination, new BackupSchedule(true, TimeSpan.FromHours(1), DateTime.Now.AddHours(-2)), null); if (!ScheduledRunner.RunIfDue(profile, DateTime.Now) || !Directory.Exists(destination)) throw new Exception("Scheduled runner did not create a snapshot."); if (ScheduledRunner.RunIfDue(profile with { LastRunLocal = DateTime.Now }, DateTime.Now)) throw new Exception("Scheduled runner ignored interval guard."); Directory.Delete(root, true); Console.WriteLine("PASS: unattended scheduled runner and interval guard");
