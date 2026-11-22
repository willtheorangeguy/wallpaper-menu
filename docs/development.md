# Developer guide

Build the single-file C# application on Windows 11 with the .NET Framework compiler. Git is used to download the source.

## Get the source and build

```powershell
git clone https://github.com/willtheorangeguy/wallpaper-menu.git
Set-Location wallpaper-menu
& "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe" /nologo /target:winexe /optimize+ /out:WallpaperMenu.exe WallpaperMenu.cs
```

A successful build creates `WallpaperMenu.exe` in the repository directory. Run it to install the local build for your account:

```powershell
& ".\WallpaperMenu.exe"
```

## Run the tests

```powershell
./tests/Run-Tests.ps1
```

See [Testing](./testing.md) for coverage and installer output.

## Preview the docs

From the repository root, use the shared preview script with Python 3.12 and Git available:

```powershell
git clone --depth 1 https://github.com/willtheorangeguy/mkdocs .mkdocs-shared
& ".\.mkdocs-shared\scripts\docs-serve.ps1" -Build
```

The script stages the shared design files and runs a strict build. Omit `-Build` to start the local preview server.

## Contribute

Use the [account-wide contributing guide](https://github.com/willtheorangeguy/.github/blob/main/CONTRIBUTING.md).
