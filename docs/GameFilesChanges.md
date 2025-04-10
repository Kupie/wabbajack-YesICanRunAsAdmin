# Game Files Handling Changes

## Missing Game Files - Installation Continuation

**Implemented In**: Version 1.0.0+

### Overview

Wabbajack has been modified to allow installations to continue even when game files are missing. Previously, if any file was missing during installation (including game files), the installation would fail with a `DownloadFailed` result. With this change, installations will now continue if the only missing files are game files.

### Technical Implementation

The changes were focused on the `StandardInstaller.cs` file. After attempting to download archives and hashing them, missing game files are now filtered out of the critical check that determines installation failure.

Key changes:
- Added a filter to distinguish between missing game files and other types of missing archives
- Installation only fails if there are missing non-game files
- Missing game files are logged as warnings but don't cause installation failure

```csharp
var missing = ModList.Archives.Where(a => !HashedArchives.ContainsKey(a.Hash)).ToList();
var missingNonGameFiles = missing.Where(a => !(a.State is GameFileSource)).ToList();

if (missingNonGameFiles.Count > 0)
{
    // Fail installation - missing non-game files
    return InstallResult.DownloadFailed;
}
else if (missing.Count > 0)
{
    // Continue installation - only game files are missing
}
```

## Missing Files Report - Game File Simplification

**Implemented In**: Version 1.0.0+

### Overview

The Missing Files report (shown to users when archives couldn't be automatically downloaded) has been simplified for game files. Previously, the report would show extensive validation information for different types of game files. This has been simplified to just indicate the file as a game file without additional validation messages.

### Technical Implementation

The changes were focused on the `InstallationVM.cs` file:

Before:
```csharp
case GameFileSource gameFile:
    writer.Write($"<h3>{archive.Name} (Game File)</h3>");
    // Complex validation logic with different messages for different game files
    // including Creation Kit, Curios files, Creation Club content, etc.
    break;
```

After:
```csharp
case GameFileSource gameFile:
    writer.Write($"<h3>{archive.Name} (Game File)</h3>");
    break;
```

### User Impact

- Modlist installations can now complete successfully even if some game files are missing
- Users will still be informed about missing game files through the Missing Files report
- The Missing Files report is now more streamlined for game files
- This change is especially helpful for modlists that include game files that aren't strictly required or for users with different versions of games