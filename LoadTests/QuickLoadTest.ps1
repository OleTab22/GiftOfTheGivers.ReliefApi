# Simplified synchronous load test for quick execution
param(
    [string]$BaseUrl = "http://localhost:5268",
    [int]$ConcurrentUsers = 10,
    [int]$RequestsPerUser = 3,
    [string]$OutputFile = "LoadTests\Results\quick_load_results.csv"
)

$results = @()
$startTime = Get-Date
$totalRequests = $ConcurrentUsers * $RequestsPerUser
$completed = 0

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Quick Load Test - $ConcurrentUsers users, $RequestsPerUser requests each" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

for ($user = 1; $user -le $ConcurrentUsers; $user++) {
    for ($req = 1; $req -le $RequestsPerUser; $req++) {
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        $success = $false
        $statusCode = 0
        $errorMsg = ""
        
        try {
            $response = Invoke-WebRequest -Uri "$BaseUrl/api/incidents" -Method GET -UseBasicParsing -TimeoutSec 10
            $statusCode = $response.StatusCode
            $success = $true
        }
        catch {
            $errorMsg = $_.Exception.Message
            if ($_.Exception.Response) {
                $statusCode = [int]$_.Exception.Response.StatusCode
            }
        }
        
        $stopwatch.Stop()
        $completed++
        
        $results += [PSCustomObject]@{
            ThreadId = $user
            Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss.fff"
            Url = "$BaseUrl/api/incidents"
            Method = "GET"
            ResponseTime = $stopwatch.ElapsedMilliseconds
            StatusCode = $statusCode
            Success = $success
            ErrorMessage = $errorMsg
        }
        
        Write-Host "Completed $completed of $totalRequests requests..." -NoNewline
        Write-Host "`r" -NoNewline
    }
}

Write-Host ""

$endTime = Get-Date
$duration = ($endTime - $startTime).TotalSeconds

# Export results
$resultsDir = Split-Path $OutputFile -Parent
if (-not (Test-Path $resultsDir)) {
    New-Item -ItemType Directory -Path $resultsDir -Force | Out-Null
}
$results | Export-Csv -Path $OutputFile -NoTypeInformation

# Calculate statistics
$successful = ($results | Where-Object { $_.Success }).Count
$failed = $results.Count - $successful
$avgResponse = [math]::Round(($results | Measure-Object -Property ResponseTime -Average).Average, 2)
$minResponse = ($results | Measure-Object -Property ResponseTime -Minimum).Minimum
$maxResponse = ($results | Measure-Object -Property ResponseTime -Maximum).Maximum
$throughput = [math]::Round($results.Count / $duration, 2)

# Calculate percentiles
$sortedTimes = ($results | Sort-Object ResponseTime).ResponseTime
$p50Index = [math]::Floor($sortedTimes.Count * 0.50)
$p95Index = [math]::Floor($sortedTimes.Count * 0.95)
$p99Index = [math]::Floor($sortedTimes.Count * 0.99)
$p50 = [math]::Round($sortedTimes[$p50Index], 2)
$p95 = [math]::Round($sortedTimes[$p95Index], 2)
$p99 = [math]::Round($sortedTimes[$p99Index], 2)

# Display summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Load Test Results" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Duration: $([math]::Round($duration, 2)) seconds"
Write-Host "Total Requests: $($results.Count)"
Write-Host "Successful: $successful ($([math]::Round(($successful / $results.Count) * 100, 2))%)"
Write-Host "Failed: $failed"
Write-Host ""
Write-Host "Response Times:" -ForegroundColor Yellow
Write-Host "  Average: $avgResponse ms"
Write-Host "  Minimum: $minResponse ms"
Write-Host "  Maximum: $maxResponse ms"
Write-Host "  50th Percentile: $p50 ms"
Write-Host "  95th Percentile: $p95 ms"
Write-Host "  99th Percentile: $p99 ms"
Write-Host ""
Write-Host "Throughput: $throughput requests/second"
Write-Host ""
Write-Host "Results saved to: $OutputFile" -ForegroundColor Green

