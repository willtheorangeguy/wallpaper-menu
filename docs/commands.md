# Commands

Run the installed executable from PowerShell to register or remove the menu without opening the installer dialog.

| Argument | Type | Default | Description |
| --- | --- | --- | --- |
| `--install` | Flag | Off | Install or update the menu without a success dialog. |
| `--uninstall` | Flag | Off | Remove the context-menu registration for the current account. |

With no arguments, the executable installs the menu and shows a success dialog. The registered menu command supplies `--remove`, which acts on the wallpaper under the current pointer.

## Install from PowerShell

Run from a directory containing the release installer:

```powershell
& ".\WallpaperMenu-Setup.exe" --install
```

## Remove the menu

```powershell
& "$env:LOCALAPPDATA\WallpaperMenu\WallpaperMenu.exe" --uninstall
```
