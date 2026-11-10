#Requires -Version 5.1
[CmdletBinding()]
param(
    [ValidateSet('release', 'debug')]
    [string]$BuildType = 'release',
    [string]$AndroidNdk = $env:ANDROID_NDK,
    [string]$CMakePath = 'cmake',
    [string]$NinjaPath = 'ninja',
    [string]$ArtifactDirectory,
    [string]$OutputDirectory,
    [ValidateRange(1, 256)]
    [int]$Jobs = 4
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
if (!$ArtifactDirectory) { $ArtifactDirectory = Join-Path $repoRoot 'artifacts~/android' }
if (!$OutputDirectory) { $OutputDirectory = Join-Path $repoRoot 'Runtime/Plugins/Android' }
$ArtifactDirectory = [IO.Path]::GetFullPath($ArtifactDirectory)
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
if (!$AndroidNdk) { $AndroidNdk = $env:ANDROID_NDK_HOME }
if (!$AndroidNdk) { $AndroidNdk = $env:ANDROID_NDK_ROOT }
if (!$AndroidNdk -or !(Test-Path -LiteralPath (Join-Path $AndroidNdk 'build/cmake/android.toolchain.cmake'))) {
    throw 'Set ANDROID_NDK or pass -AndroidNdk pointing to an Android NDK installation.'
}
$AndroidNdk = (Resolve-Path -LiteralPath $AndroidNdk).Path
$CMakePath = (Get-Command $CMakePath -ErrorAction Stop).Source
$NinjaPath = (Get-Command $NinjaPath -ErrorAction Stop).Source
$configuration = if ($BuildType -eq 'debug') { 'Debug' } else { 'Release' }
$release = 'M116-20250805'
$archiveUrl = "https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/$release/webrtc-android.zip"
$archiveSha256 = 'F561153CF06DC5D0C95CB7272168582DF9CB37A960C8FB7907DD775547766D4A'
$downloadDirectory = Join-Path $ArtifactDirectory 'downloads'
$archivePath = Join-Path $downloadDirectory "webrtc-android-$release.zip"
$dependencyDirectory = Join-Path $ArtifactDirectory "dependencies/$release"
$dependencyMarker = Join-Path $dependencyDirectory '.archive-sha256'
$packageDirectory = Join-Path $ArtifactDirectory "package/$configuration"
$logDirectory = Join-Path $ArtifactDirectory "logs/$configuration"
New-Item -ItemType Directory -Path $downloadDirectory, $packageDirectory, $logDirectory -Force | Out-Null

if (!(Test-Path -LiteralPath $archivePath)) {
    $partialPath = "$archivePath.partial"
    & curl.exe --fail --location --retry 3 --silent --show-error --output $partialPath $archiveUrl
    if ($LASTEXITCODE -ne 0) { throw "Download failed: $archiveUrl" }
    Move-Item -LiteralPath $partialPath -Destination $archivePath -Force
}
$actualSha256 = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash
if ($actualSha256 -ne $archiveSha256) { throw "SHA256 mismatch for $archivePath" }
Add-Type -AssemblyName System.IO.Compression.FileSystem
if (!(Test-Path -LiteralPath $dependencyMarker)) {
    # The upstream archive contains unused toolchain headers differing only in
    # case. Extract entry by entry with overwrite on Windows; this also permits
    # retrying an interrupted extraction before the completion marker is written.
    New-Item -ItemType Directory -Path $dependencyDirectory -Force | Out-Null
    $archive = [IO.Compression.ZipFile]::OpenRead($archivePath)
    try {
        foreach ($entry in $archive.Entries) {
            $entryPath = Join-Path $dependencyDirectory $entry.FullName
            if (!$entry.Name) {
                New-Item -ItemType Directory -Path $entryPath -Force | Out-Null
            } else {
                New-Item -ItemType Directory -Path (Split-Path -Parent $entryPath) -Force | Out-Null
                [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $entryPath, $true)
            }
        }
    } finally {
        $archive.Dispose()
    }
    Set-Content -LiteralPath $dependencyMarker -Value $archiveSha256 -Encoding Ascii
} elseif ((Get-Content -LiteralPath $dependencyMarker -Raw).Trim() -ne $archiveSha256) {
    throw 'The extracted dependency does not match the pinned archive.'
}

function Invoke-LoggedCommand {
    param([string]$Executable, [string[]]$CommandArguments, [string]$LogPath)
    & $Executable @CommandArguments 2>&1 | Tee-Object -FilePath $LogPath -Append | Write-Host
    if ($LASTEXITCODE -ne 0) { throw "Command failed ($LASTEXITCODE). See $LogPath" }
}

$abis = @('arm64-v8a', 'x86_64')
foreach ($abi in $abis) {
    $buildDirectory = Join-Path $ArtifactDirectory "build/$configuration/$abi"
    $pluginDirectory = Join-Path $buildDirectory 'plugin'
    $logPath = Join-Path $logDirectory "$abi.log"
    $configureArguments = @(
        '-S', (Join-Path $repoRoot 'Plugin~'), '-B', $buildDirectory, '-G', 'Ninja',
        "-DCMAKE_MAKE_PROGRAM=$NinjaPath",
        "-DCMAKE_TOOLCHAIN_FILE=$AndroidNdk/build/cmake/android.toolchain.cmake",
        "-DANDROID_NDK=$AndroidNdk", "-DANDROID_ABI=$abi", '-DANDROID_PLATFORM=android-24',
        '-DANDROID_STL=c++_static', '-DANDROID_SUPPORT_FLEXIBLE_PAGE_SIZES=ON',
        '-DCMAKE_SHARED_LINKER_FLAGS=-Wl,-z,max-page-size=16384',
        "-DCMAKE_BUILD_TYPE=$configuration", '-DBUILD_TESTING=OFF',
        "-DWEBRTC_DIR=$dependencyDirectory", "-DWEBRTC_PLUGIN_OUTPUT_DIR=$pluginDirectory"
    )
    Invoke-LoggedCommand $CMakePath $configureArguments $logPath
    Invoke-LoggedCommand $CMakePath @('--build', $buildDirectory, '--target', 'WebRTCPlugin', '--parallel', "$Jobs") $logPath
    if (!(Test-Path -LiteralPath (Join-Path $pluginDirectory 'libwebrtc.so'))) {
        throw "Missing rebuilt native plugin for $abi."
    }
}

# Only publish after every ABI succeeded. Retain upstream Java classes/resources.
$baseAar = if ($BuildType -eq 'debug') { 'libwebrtc-debug.aar' } else { 'libwebrtc.aar' }
$packagedAar = Join-Path $packageDirectory 'libwebrtc.aar'
Copy-Item -LiteralPath (Join-Path $dependencyDirectory "lib/$baseAar") -Destination $packagedAar -Force
$zip = [IO.Compression.ZipFile]::Open($packagedAar, [IO.Compression.ZipArchiveMode]::Update)
try {
    foreach ($abi in $abis) {
        $entryName = "jni/$abi/libwebrtc.so"
        $entry = $zip.GetEntry($entryName)
        if ($entry) { $entry.Delete() }
        $pluginPath = Join-Path $ArtifactDirectory "build/$configuration/$abi/plugin/libwebrtc.so"
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $pluginPath, $entryName) | Out-Null
    }
} finally {
    $zip.Dispose()
}

$manifest = [ordered]@{
    libwebrtcRelease = $release
    dependencyUrl = $archiveUrl
    dependencySha256 = $actualSha256
    configuration = $configuration
    abis = $abis
    ndk = (Get-Content -LiteralPath (Join-Path $AndroidNdk 'source.properties') -Raw).Trim()
    aarSha256 = (Get-FileHash -LiteralPath $packagedAar -Algorithm SHA256).Hash
    plugins = @($abis | ForEach-Object {
        [ordered]@{ abi = $_; sha256 = (Get-FileHash -LiteralPath (Join-Path $ArtifactDirectory "build/$configuration/$_/plugin/libwebrtc.so") -Algorithm SHA256).Hash }
    })
}
$manifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $packageDirectory 'build-manifest.json') -Encoding UTF8
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$destination = Join-Path $OutputDirectory 'libwebrtc.aar'
if ([IO.Path]::GetFullPath($packagedAar) -ne [IO.Path]::GetFullPath($destination)) {
    Copy-Item -LiteralPath $packagedAar -Destination "$destination.partial" -Force
    Move-Item -LiteralPath "$destination.partial" -Destination $destination -Force
}
Write-Host "Built $destination ($configuration, $($abis -join ', '))."
Write-Host "Build provenance: $(Join-Path $packageDirectory 'build-manifest.json')"
