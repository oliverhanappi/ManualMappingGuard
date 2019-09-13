param
(
    $Configuration = "Release",
    $Version = "0.0.0.0",
    $PackageVersion = "0.0.0"
)

Push-Location $PSScriptRoot
try
{
    Write-Output "Cleaning..."
    dotnet clean --configuration $Configuration --verbosity minimal --nologo
    Get-ChildItem -Path src -Filter *.nupkg -Recurse | Remove-Item -Force

    Write-Output "Building..."
    dotnet build --configuration $Configuration --verbosity minimal --nologo -p:Version=$Version -p:PackageVersion=$PackageVersion
    
    Write-Output "Running unit tests..."
    dotnet test --configuration $Configuration --no-build --nologo
    
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
