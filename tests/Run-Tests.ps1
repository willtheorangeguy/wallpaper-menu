param(
    [string]$InstallerPath
)

$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $PSScriptRoot
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
$source = Join-Path $repo 'WallpaperMenu.cs'
$testSource = Join-Path $PSScriptRoot 'WallpaperMenuTests.cs'
$output = Join-Path ([IO.Path]::GetTempPath()) ('WallpaperMenuTests-' + [Guid]::NewGuid())
$app = Join-Path $output 'WallpaperMenu.exe'
$tests = Join-Path $output 'WallpaperMenuTests.exe'

New-Item -ItemType Directory -Path $output | Out-Null
try {
    & $compiler /nologo /warnaserror /target:winexe /optimize+ "/out:$app" $source
    if ($LASTEXITCODE -ne 0) { throw 'Wallpaper Menu failed to build.' }

    & $compiler /nologo /warnaserror /target:exe /main:WallpaperMenuTests "/out:$tests" $source $testSource
    if ($LASTEXITCODE -ne 0) { throw 'The tests failed to build.' }

    & $tests
    if ($LASTEXITCODE -ne 0) { throw 'Wallpaper Menu tests failed.' }

    if ($InstallerPath) {
        [IO.File]::Copy($app, $InstallerPath, $true)
    }
}
finally {
    foreach ($file in @($app, $tests)) {
        if (Test-Path -LiteralPath $file) { Remove-Item -LiteralPath $file }
    }
    [IO.Directory]::Delete($output)
}
