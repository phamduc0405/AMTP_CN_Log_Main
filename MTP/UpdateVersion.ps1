param (
    [string]$assemblyInfoPath
)

if (-not (Test-Path $assemblyInfoPath)) {
    Write-Error "AssemblyInfo.cs file not found!"
    exit 1
}

$versionPattern = 'AssemblyVersion\("(\d+)\.(\d+)\.(\d+)\.(\d+)"\)'
$fileContent = Get-Content $assemblyInfoPath

foreach ($line in $fileContent) {
    if ($line -match $versionPattern) {
        $major = [int]$matches[1]
        $minor = [int]$matches[2]
        $build = [int]$matches[3]
        $revision = [int]$matches[4] + 1
        $newVersion = "AssemblyVersion(`"$major.$minor.$build.$revision`")"
        $newFileContent = $fileContent -replace $versionPattern, $newVersion

        Set-Content $assemblyInfoPath -Value $newFileContent
        Write-Output "Updated AssemblyVersion to $major.$minor.$build.$revision"
        break
    }
}
