namespace OpenBackup;
public static class ScheduledRunner
{
    public static bool RunIfDue(BackupProfile profile, DateTime nowLocal)
    {
        if (!profile.Schedule.Enabled || string.IsNullOrWhiteSpace(profile.Source) || string.IsNullOrWhiteSpace(profile.Destination)) return false;
        if (profile.LastRunLocal is not null && nowLocal < profile.LastRunLocal.Value.Add(profile.Schedule.Interval)) return false;
        BackupEngine.CreateSnapshot(profile.Source, profile.Destination); ProfileStore.Save(profile with { LastRunLocal = nowLocal }); return true;
    }
}
