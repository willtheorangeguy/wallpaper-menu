# Installation

## Requirements

Use Windows 11 and the Windows account where you want the menu to appear. Building from source uses the .NET Framework C# compiler in the Windows system directory.

## Install Wallpaper Menu

=== "Release installer"

    Download and run **WallpaperMenu-Setup.exe**. [Latest release](https://github.com/willtheorangeguy/wallpaper-menu/releases/latest)

=== "Build from source"

    Clone the repository and run the build and install commands from [Developer guide](./development.md).

The installer copies the executable to `%LOCALAPPDATA%\WallpaperMenu\WallpaperMenu.exe` and registers the desktop action for your account.

## Verify the installation

Right-click an empty spot on the desktop and open **Show more options**. Check for **Remove this wallpaper**. Select it when the display shows a photo you want to remove.

## Upgrading

Run the installer from the newer release. It replaces the installed executable and registers the same menu item.

## Uninstalling

Open PowerShell and run:

<!-- markdownlint-disable-next-line MD046 -->
```powershell
& "$env:LOCALAPPDATA\WallpaperMenu\WallpaperMenu.exe" --uninstall
```

The command removes the menu registration. To remove the executable too, delete the `WallpaperMenu` folder from `%LOCALAPPDATA%` after the command finishes.
