param
(
    [string] $Configuration = "Release",
    [string] $Version = "0.0.0.0",
    [string] $PackageVersion = "0.0.0",
    [switch] $SkipTests = $false
)

$ErrorActionPreference = "Stop"

function Exec([scriptblock] $Command)
{
    $global:LastExitCode = 0
    & $Command

    if ($global:LastExitCode -ne 0)
    {
        Throw "Command failed: $Command"
    }
}

Push-Location $PSScriptRoot
try
{
    Write-Output "Cleaning..."
    Exec { dotnet clean --configuration $Configuration --verbosity minimal --nologo }
    Get-ChildItem -Path src -Filter *.nupkg -Recurse | Remove-Item -Force

    Write-Output "Restoring dependencies..."
    Exec { dotnet restore }

    Write-Output "Building..."
    Exec { dotnet build --configuration $Configuration --verbosity minimal --no-restore --nologo -p:Version=$Version -p:PackageVersion=$PackageVersion }
    
    if (-not $SkipTests)
    {
        Write-Output "Running unit tests..."
        Exec { dotnet test --configuration $Configuration --no-build --nologo }
    }
    
    Write-Output "Creating artifacts..."
    if (Test-Path dist)
    {
        Remove-Item dist -Force -Recurse
    }
    
    New-Item dist -ItemType Directory | Out-Null
    Copy-Item src\Analyzers\bin\$Configuration\ManualMappingGuard.$PackageVersion.nupkg dist
}
finally
{
    Pop-Location
}
