# Comprehensive Stress Test Script
# Pushes application to limits to identify bottlenecks and failure points

param(
    [string]$BaseUrl = "http://localhost:5268",
    [int]$Stage1Users = 50,
    [int]$Stage2Users = 150,
    [int]$Stage3Users = 300,
    [int]$Stage4Users = 500,
    [int]$RequestsPerUser = 10,
    [int]$RampUpSeconds = 5,
    [string]$OutputFile = "LoadTests\Results\stress_test_detailed.csv"
)

$results = @()
$overallStart = Get-Date

# Test Configuration
Write-Host "========================================" -ForegroundColor Red
Write-Host "STRESS TEST - Pushing Application to Limits" -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Red
Write-Host "Base URL: $BaseUrl"
Write-Host "Stages: 50 -> 150 -> 300 -> 500 concurrent users"
Write-Host "Requests per user: $RequestsPerUser"
Write-Host ""

function Invoke-StressStage {
    param(
        [int]$ConcurrentUsers,
        [int]$StageNumber,
        [string]$StageName
    )
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Yellow
    Write-Host "STAGE $StageNumber : $StageName" -ForegroundColor Yellow
    Write-Host "Concurrent Users: $ConcurrentUsers" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Yellow
    
    $stageStart = Get-Date
    $stageResults = @()
    $completed = 0
    $totalRequests = $ConcurrentUsers * $RequestsPerUser
    
    # Simulate sudden spike by starting all users rapidly
    $jobs = @()
    for ($user = 1; $user -le $ConcurrentUsers; $user++) {
        $scriptBlock = {
            param($baseUrl, $userId, $reqCount, $stageNumber, $stageName)
            $userResults = @()
            
            for ($req = 1; $req -le $reqCount; $req++) {
                $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
                $success = $false
                $statusCode = 0
                $errorMsg = ""
                
                try {
                    # Mix of endpoints for realistic stress
                    $endpoints = @(
                        "/api/incidents",
                        "/api/donations",
                        "/api/incidents",
                        "/api/incidents"
                    )
                    $endpointIndex = ($req - 1) % $endpoints.Length
                    $endpoint = $endpoints[$endpointIndex]
                    
                    $response = Invoke-WebRequest -Uri "$baseUrl$endpoint" -Method GET -UseBasicParsing -TimeoutSec 30 -ErrorAction Stop
                    $statusCode = $response.StatusCode
                    $success = $true
                }
                catch {
                    $errorMsg = $_.Exception.Message
                    if ($_.Exception.Response) {
                        $statusCode = [int]$_.Exception.Response.StatusCode
                    }
                    else {
                        $statusCode = 0
                    }
                }
                
                $stopwatch.Stop()
                
                $userResults += [PSCustomObject]@{
                    ThreadId = $userId
                    RequestNumber = $req
                    Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss.fff"
                    Url = "$baseUrl$endpoint"
                    Method = "GET"
                    ResponseTime = $stopwatch.ElapsedMilliseconds
                    StatusCode = $statusCode
                    Success = $success
                    ErrorMessage = $errorMsg
                    Stage = $stageNumber
                    StageName = $stageName
                }
            }
            
            return $userResults
        }
        
        $job = Start-Job -ScriptBlock $scriptBlock -ArgumentList $BaseUrl, $user, $RequestsPerUser, $StageNumber, $StageName
        $jobs += $job
        
        # Small delay to simulate ramp-up
        if ($user % 10 -eq 0) {
            Start-Sleep -Milliseconds 50
        }
    }
    
    Write-Host "All $ConcurrentUsers users started. Waiting for completion..." -ForegroundColor Cyan
    
    # Wait for all jobs and collect results
    $jobResults = $jobs | Wait-Job | Receive-Job
    $jobs | Remove-Job
    
    foreach ($jobResult in $jobResults) {
        if ($jobResult) {
            $stageResults += $jobResult
            $completed++
            if ($completed % 50 -eq 0) {
                Write-Host "  Completed $completed of $totalRequests requests..."
            }
        }
    }
    
    Write-Host ""
    $stageEnd = Get-Date
    $stageDuration = ($stageEnd - $stageStart).TotalSeconds
    
    # Calculate statistics
    $successful = if ($stageResults.Count -gt 0) { ($stageResults | Where-Object { $_.Success -eq $true }).Count } else { 0 }
    $failed = $stageResults.Count - $successful
    $avgResponse = if ($stageResults.Count -gt 0) { [math]::Round(($stageResults | Measure-Object -Property ResponseTime -Average).Average, 2) } else { 0 }
    $minResponse = if ($stageResults.Count -gt 0) { ($stageResults | Measure-Object -Property ResponseTime -Minimum).Minimum } else { 0 }
    $maxResponse = if ($stageResults.Count -gt 0) { ($stageResults | Measure-Object -Property ResponseTime -Maximum).Maximum } else { 0 }
    
    $sortedTimes = @()
    if ($stageResults.Count -gt 0) {
        $sortedTimes = ($stageResults | Sort-Object ResponseTime).ResponseTime
    }
    
    $p50 = 0
    $p95 = 0
    $p99 = 0
    
    if ($sortedTimes.Count -gt 0) {
        $p50Index = [math]::Max(0, [math]::Min([math]::Floor($sortedTimes.Count * 0.50), $sortedTimes.Count - 1))
        $p95Index = [math]::Max(0, [math]::Min([math]::Floor($sortedTimes.Count * 0.95), $sortedTimes.Count - 1))
        $p99Index = [math]::Max(0, [math]::Min([math]::Floor($sortedTimes.Count * 0.99), $sortedTimes.Count - 1))
        $p50 = [math]::Round($sortedTimes[$p50Index], 2)
        $p95 = [math]::Round($sortedTimes[$p95Index], 2)
        $p99 = [math]::Round($sortedTimes[$p99Index], 2)
    }
    
    $throughput = if ($stageDuration -gt 0) { [math]::Round($stageResults.Count / $stageDuration, 2) } else { 0 }
    $errorRate = if ($stageResults.Count -gt 0) { [math]::Round(($failed / $stageResults.Count) * 100, 2) } else { 0 }
    
    # Display stage summary
    Write-Host ""
    Write-Host "Stage $StageNumber Results:" -ForegroundColor Green
    Write-Host "  Duration: $([math]::Round($stageDuration, 2)) seconds"
    Write-Host "  Total Requests: $($stageResults.Count)"
    $successPercent = if ($stageResults.Count -gt 0) { [math]::Round(($successful / $stageResults.Count) * 100, 2) } else { 0 }
    Write-Host "  Successful: $successful ($successPercent%)"
    Write-Host "  Failed: $failed ($errorRate%)"
    Write-Host "  Error Rate: $errorRate%"
    Write-Host "  Average Response Time: $avgResponse ms"
    Write-Host "  50th Percentile: $p50 ms"
    Write-Host "  95th Percentile: $p95 ms"
    Write-Host "  99th Percentile: $p99 ms"
    Write-Host "  Min/Max: $minResponse / $maxResponse ms"
    Write-Host "  Throughput: $throughput requests/second"
    
    # Check for degradation
    if ($errorRate -gt 5) {
        Write-Host "  ⚠️  HIGH ERROR RATE - System showing stress!" -ForegroundColor Red
    }
    if ($p95 -gt 1000) {
        Write-Host "  ⚠️  SLOW RESPONSE TIMES - Performance degradation detected!" -ForegroundColor Yellow
    }
    
    return @{
        Results = $stageResults
        Stats = @{
            Duration = $stageDuration
            Total = $stageResults.Count
            Successful = $successful
            Failed = $failed
            ErrorRate = $errorRate
            AvgResponse = $avgResponse
            P50 = $p50
            P95 = $p95
            P99 = $p99
            MinResponse = $minResponse
            MaxResponse = $maxResponse
            Throughput = $throughput
        }
    }
}

# Execute stress test stages
$stage1 = Invoke-StressStage -ConcurrentUsers $Stage1Users -StageNumber 1 -StageName "Baseline"
$results += $stage1.Results

Write-Host ""
Write-Host "Waiting 5 seconds before next stage..." -ForegroundColor Cyan
Start-Sleep -Seconds 5

$stage2 = Invoke-StressStage -ConcurrentUsers $Stage2Users -StageNumber 2 -StageName "Moderate Load"
$results += $stage2.Results

Write-Host ""
Write-Host "Waiting 5 seconds before next stage..." -ForegroundColor Cyan
Start-Sleep -Seconds 5

$stage3 = Invoke-StressStage -ConcurrentUsers $Stage3Users -StageNumber 3 -StageName "High Load"
$results += $stage3.Results

Write-Host ""
Write-Host "Waiting 5 seconds before extreme stage..." -ForegroundColor Cyan
Start-Sleep -Seconds 5

$stage4 = Invoke-StressStage -ConcurrentUsers $Stage4Users -StageNumber 4 -StageName "Extreme Load"
$results += $stage4.Results

# Export results
$resultsDir = Split-Path $OutputFile -Parent
if (-not (Test-Path $resultsDir)) {
    New-Item -ItemType Directory -Path $resultsDir -Force | Out-Null
}
$results | Export-Csv -Path $OutputFile -NoTypeInformation

$overallEnd = Get-Date
$totalDuration = ($overallEnd - $overallStart).TotalSeconds

# Overall summary
Write-Host ""
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "STRESS TEST SUMMARY" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Total Duration: $([math]::Round($totalDuration, 2)) seconds"
Write-Host "Total Requests: $($results.Count)"
Write-Host ""

$overallStats = @{
    Stage1 = $stage1.Stats
    Stage2 = $stage2.Stats
    Stage3 = $stage3.Stats
    Stage4 = $stage4.Stats
}

Write-Host "Stage-by-Stage Comparison:" -ForegroundColor Cyan
Write-Host ("{0,-10} {1,-8} {2,-10} {3,-12} {4,-10} {5,-12} {6,-10}" -f "Stage", "Users", "Requests", "Success%", "Error%", "Avg(ms)", "P95(ms)")
Write-Host ("-" * 80)

foreach ($stageNum in 1..4) {
    $stage = "Stage$stageNum"
    $stats = $overallStats[$stage]
    $successPercent = if ($stats.Total -gt 0) { [math]::Round(($stats.Successful / $stats.Total) * 100, 1) } else { 0 }
    Write-Host ("{0,-10} {1,-8} {2,-10} {3,-12} {4,-10} {5,-12} {6,-10}" -f `
        $stageNum, `
        $(if ($stageNum -eq 1) { $Stage1Users } elseif ($stageNum -eq 2) { $Stage2Users } elseif ($stageNum -eq 3) { $Stage3Users } else { $Stage4Users }), `
        $stats.Total, `
        "$successPercent%", `
        "$($stats.ErrorRate)%", `
        $stats.AvgResponse, `
        $stats.P95)
}

Write-Host ""
Write-Host "Bottleneck Analysis:" -ForegroundColor Yellow

# Identify degradation point
if ($stage2.Stats.ErrorRate -gt $stage1.Stats.ErrorRate * 1.5) {
    Write-Host "  ⚠️  Error rate increased significantly at Stage 2 ($Stage2Users users)" -ForegroundColor Red
}
if ($stage3.Stats.ErrorRate -gt $stage2.Stats.ErrorRate * 1.5) {
    Write-Host "  ⚠️  Error rate increased significantly at Stage 3 ($Stage3Users users)" -ForegroundColor Red
}
if ($stage4.Stats.ErrorRate -gt $stage3.Stats.ErrorRate * 1.5) {
    Write-Host "  ⚠️  Error rate increased significantly at Stage 4 ($Stage4Users users)" -ForegroundColor Red
}

# Response time degradation
if ($stage2.Stats.P95 -gt $stage1.Stats.P95 * 2) {
    Write-Host "  ⚠️  Response times doubled at Stage 2 ($Stage2Users users)" -ForegroundColor Yellow
}
if ($stage3.Stats.P95 -gt $stage2.Stats.P95 * 2) {
    Write-Host "  ⚠️  Response times doubled at Stage 3 ($Stage3Users users)" -ForegroundColor Yellow
}
if ($stage4.Stats.P95 -gt $stage3.Stats.P95 * 2) {
    Write-Host "  ⚠️  Response times doubled at Stage 4 ($Stage4Users users)" -ForegroundColor Yellow
}

# Breaking point estimation
if ($stage4.Stats.ErrorRate -gt 10) {
    Write-Host ""
    Write-Host "  🔴 BREAKING POINT: System exceeded at $Stage4Users users" -ForegroundColor Red
    Write-Host "     Estimated capacity: $($Stage3Users - 50) to $Stage3Users users" -ForegroundColor Yellow
}
elseif ($stage3.Stats.ErrorRate -gt 10) {
    Write-Host ""
    Write-Host "  🔴 BREAKING POINT: System exceeded at $Stage3Users users" -ForegroundColor Red
    Write-Host "     Estimated capacity: $($Stage2Users - 50) to $Stage2Users users" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Results saved to: $OutputFile" -ForegroundColor Green

# Generate summary JSON
$summaryFile = $OutputFile -replace "\.csv$", "_summary.json"
$overallStats | ConvertTo-Json -Depth 10 | Out-File $summaryFile
Write-Host "Summary saved to: $summaryFile" -ForegroundColor Green