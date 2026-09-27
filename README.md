# OpenBackup

OpenBackup is a local-first Windows backup application focused on one promise: a backup is not complete until it can be verified.

OpenBackup creates versioned snapshots with SHA-256 manifests, safe verified restores, persisted profiles, and schedule-aware unattended execution. Use the dashboard's **Install task** action to register Windows Task Scheduler, or run the published executable with `--run-scheduled`. It never deletes source files or uploads data.

For a local install, place `OpenBackup.exe` beside `Install.ps1` and run `powershell -ExecutionPolicy Bypass -File .\Install.ps1`. The installer creates a desktop shortcut. `Uninstall.ps1` removes only the application and shortcut; it never removes backup snapshots.

MIT licensed.
