# Testing

Run the suite from the repository root in PowerShell on Windows.

```powershell
./tests/Run-Tests.ps1
```

The script compiles the application and test executable with warnings treated as errors. A build or test failure makes the command fail. The current suite reports:

```text
12 passed, 0 failed
```

## Coverage

| Area | Cases |
| --- | --- |
| Display selection | Negative coordinates, shared edges, gaps and disconnected displays |
| Slideshow advancement | Targeted advancement, fallback order and retry bound, failed advancement and an already changed wallpaper |
| File validation | Valid nested files, case-insensitive extensions, similar sibling folders, traversal, missing and unsupported paths |

Tests use a simulated desktop and temporary files. To test the desktop integration directly, set up a slideshow with a disposable photo, use the context menu and verify the file appears in the Recycle Bin.

## Keep the tested installer

```powershell
./tests/Run-Tests.ps1 -InstallerPath (Join-Path $PWD 'WallpaperMenu-Setup.exe')
```

With `-InstallerPath` set, the runner keeps a copy of the successfully built executable after the tests pass.
