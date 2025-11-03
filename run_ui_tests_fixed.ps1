# Fixed UI Test Runner - Builds projects separately to avoid path issues

Write-Host "=== Step 1: Stopping API processes ===" -ForegroundColor Cyan
Get-Process | Where-Object {
    $_.ProcessName -like "*GiftOfTheGivers*" -or 
    $_.ProcessName -like "*ReliefApi*"
} | ForEach-Object {
    Write-Host "Stopping process: $($_.ProcessName) (PID: $($_.Id))" -ForegroundColor Yellow
    Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
}
Start-Sleep -Seconds 2

Write-Host "`n=== Step 2: Cleaning everything ===" -ForegroundColor Cyan
$dotnetPath = "C:\Program Files\dotnet\dotnet.exe"

# Clean both projects
& $dotnetPath clean GiftOfTheGivers.ReliefApi.csproj
& $dotnetPath clean Tests/GiftOfTheGivers.ReliefApi.Tests.csproj

Write-Host "`n=== Step 3: Removing all bin/obj folders ===" -ForegroundColor Cyan
Get-ChildItem -Path . -Recurse -Directory -Filter "bin" -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Get-ChildItem -Path . -Recurse -Directory -Filter "obj" -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "`n=== Step 4: Building main project FIRST (isolated) ===" -ForegroundColor Cyan
& $dotnetPath build GiftOfTheGivers.ReliefApi.csproj --no-incremental --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host "Main project build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== Step 5: Building test project (with reference to already-built main project) ===" -ForegroundColor Cyan
& $dotnetPath build Tests/GiftOfTheGivers.ReliefApi.Tests.csproj --no-incremental --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host "Test project build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== Step 6: Running UI Tests ===" -ForegroundColor Green
& $dotnetPath test Tests/GiftOfTheGivers.ReliefApi.Tests.csproj --filter "FullyQualifiedName~UITests" --verbosity normal --logger "trx;LogFileName=UITests.trx" --no-build

Write-Host "`n=== Done ===" -ForegroundColor Cyan

