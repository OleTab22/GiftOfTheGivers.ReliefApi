# Fix and Run UI Tests Script
# This script fixes build issues and runs UI tests

Write-Host "=== Step 1: Stopping API processes ===" -ForegroundColor Cyan
Get-Process | Where-Object {
    $_.ProcessName -like "*GiftOfTheGivers*" -or 
    $_.ProcessName -like "*ReliefApi*" -or 
    $_.Id -eq 13924
} | ForEach-Object {
    Write-Host "Stopping process: $($_.ProcessName) (PID: $($_.Id))" -ForegroundColor Yellow
    Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
}
Start-Sleep -Seconds 2

Write-Host "`n=== Step 2: Cleaning build artifacts ===" -ForegroundColor Cyan
$dotnetPath = "C:\Program Files\dotnet\dotnet.exe"
& $dotnetPath clean Tests/GiftOfTheGivers.ReliefApi.Tests.csproj
& $dotnetPath clean GiftOfTheGivers.ReliefApi.csproj

Write-Host "`n=== Step 3: Removing bin/obj folders ===" -ForegroundColor Cyan
$foldersToRemove = @(
    "Tests\bin",
    "Tests\obj",
    "bin",
    "obj"
)

foreach ($folder in $foldersToRemove) {
    if (Test-Path $folder) {
        Write-Host "Removing: $folder" -ForegroundColor Yellow
        Remove-Item -Path $folder -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "`n=== Step 4: Building test project (isolated) ===" -ForegroundColor Cyan
& $dotnetPath build Tests/GiftOfTheGivers.ReliefApi.Tests.csproj --no-incremental --verbosity minimal

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n=== Step 5: Running UI Tests ===" -ForegroundColor Green
    & $dotnetPath test Tests/GiftOfTheGivers.ReliefApi.Tests.csproj --filter "FullyQualifiedName~UITests" --verbosity normal --logger "trx;LogFileName=UITests.trx" --no-build
} else {
    Write-Host "`nBuild failed! Please check the errors above." -ForegroundColor Red
}

Write-Host "`n=== Done ===" -ForegroundColor Cyan

