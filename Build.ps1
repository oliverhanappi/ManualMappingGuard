param
(
    [string] $Configuration = "Release",
    [string] $Version = "0.0.0.0",
    [string] $PackageVersion = "0.0.0",
    [switch] $SkipTests = $false
)

Push-Location $PSScriptRoot
try
{
    Write-Output "Cleaning..."
    dotnet clean --configuration $Configuration --verbosity minimal --nologo
    Get-ChildItem -Path src -Filter *.nupkg -Recurse | Remove-Item -Force

    Write-Output "Restoring dependencies..."
    dotnet restore

    Write-Output "Building..."
    dotnet build --configuration $Configuration --verbosity minimal --nologo -p:Version=$Version -p:PackageVersion=$PackageVersion
    
    if (-not $SkipTests)
    {
        Write-Output "Running unit tests..."
        dotnet test --configuration $Configuration --no-build --nologo
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
