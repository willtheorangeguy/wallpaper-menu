# Wallpaper Menu

Wallpaper Menu adds **Remove this wallpaper** to the Windows 11 desktop context menu. It is designed for a slideshow made from copies of your photos in `Pictures\Wallpapers`.

1. In Windows **Settings → Personalization → Background**, choose **Slideshow** and select `Pictures\Wallpapers` as the source folder.
2. Run `WallpaperMenu.exe` to install the menu for your Windows account.
3. On the monitor showing the photo you want to remove, right-click an empty spot on the desktop and choose **Show more options → Remove this wallpaper**.

The menu sends the photo to the Recycle Bin without an app confirmation. On systems where Windows only advances the next scheduled display, other displays may advance before the selected one changes.

## Build

From the repository folder, run this command in PowerShell on Windows 11. It uses the compiler that ships with .NET Framework.

```powershell
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /target:winexe /optimize+ /out:WallpaperMenu.exe WallpaperMenu.cs
```

To remove the context menu, run `%LOCALAPPDATA%\WallpaperMenu\WallpaperMenu.exe --uninstall` from the Windows Run dialog.

## Tests

Run `./tests/Run-Tests.ps1` in PowerShell. It builds the app and tests file validation, monitor selection, and slideshow advancement using temporary files and simulated displays. GitHub Actions runs the same script on Windows for pushes and pull requests.
