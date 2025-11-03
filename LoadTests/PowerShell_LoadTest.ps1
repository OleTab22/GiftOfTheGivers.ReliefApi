# PowerShell Load Testing Script for Relief API
# Alternative to JMeter for simple load testing using built-in tools

param(
    [Parameter(Mandatory=$false)]
    [string]$BaseUrl = "http://localhost:5268",
    
    [Parameter(Mandatory=$false)]
    [int]$ConcurrentUsers = 50,
    
    [Parameter(Mandatory=$false)]
    [int]$RequestsPerUser = 10,
    
    [Parameter(Mandatory=$false)]
    [string]$OutputFile = "LoadTests\Results\powershell_results.csv"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Relief API Load Test" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Base URL: $BaseUrl"
Write-Host "Concurrent Users: $ConcurrentUsers"
Write-Host "Requests Per User: $RequestsPerUser"
Write-Host "Total Requests: $($ConcurrentUsers * $RequestsPerUser)"
Write-Host ""

# Create results directory if it doesn't exist
$resultsDir = Split-Path $OutputFile -Parent
if (-not (Test-Path $resultsDir)) {
    New-Item -ItemType Directory -Path $resultsDir -Force | Out-Null
}

# Initialize results array
$results = @()

# Function to make HTTP request and measure time
function Test-Endpoint {
    param(
        [string]$Url,
        [string]$Method = "GET",
        [object]$Body = $null,
        [int]$ThreadId
    )
    
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $success = $false
    $statusCode = 0
    $errorMessage = ""
    
    try {
        $headers = @{
            "Content-Type" = "application/json"
        }
        
        if ($Method -eq "GET") {
            $response = Invoke-WebRequest -Uri $Url -Method $Method -Headers $headers -UseBasicParsing
        } else {
            $jsonBody = $Body | ConvertTo-Json
            $response = Invoke-WebRequest -Uri $Url -Method $Method -Headers $headers -Body $jsonBody -UseBasicParsing
        }
        
        $statusCode = $response.StatusCode
        $success = $true
    }
    catch {
        $errorMessage = $_.Exception.Message
        if ($_.Exception.Response) {
            $statusCode = [int]$_.Exception.Response.StatusCode
        }
    }
    
    $stopwatch.Stop()
    
    return [PSCustomObject]@{
        ThreadId = $ThreadId
        Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss.fff"
        Url = $Url
        Method = $Method
        ResponseTime = $stopwatch.ElapsedMilliseconds
        StatusCode = $statusCode
        Success = $success
        ErrorMessage = $errorMessage
    }
}

# Test scenarios
$testScenarios = @(
    @{ Name = "Get Incidents"; Url = "$BaseUrl/api/incidents"; Method = "GET" },
    @{ Name = "Get Donations"; Url = "$BaseUrl/api/donations"; Method = "GET" },
    @{ Name = "Register User"; Url = "$BaseUrl/api/auth/register"; Method = "POST"; 
       Body = @{ fullName = "Load Test User"; email = "loadtest@example.com"; password = "password123" } }
)

Write-Host "Starting load test..." -ForegroundColor Yellow
Write-Host ""

# Progress tracking
$totalRequests = $ConcurrentUsers * $RequestsPerUser * $testScenarios.Count
$completedRequests = 0
$startTime = Get-Date

# Run load test with concurrent jobs
$jobs = @()

for ($user = 1; $user -le $ConcurrentUsers; $user++) {
    $job = Start-Job -ScriptBlock {
        param($BaseUrl, $RequestsPerUser, $ThreadId, $TestScenarios)
        
        $threadResults = @()
        
        for ($i = 1; $i -le $RequestsPerUser; $i++) {
            foreach ($scenario in $TestScenarios) {
                $url = $scenario.Url
                $method = $scenario.Method
                $body = $scenario.Body
                
                # Add unique identifier to avoid duplicates
                if ($body) {
                    $body = $body.Clone()
                    $body.email = "loadtest_${ThreadId}_${i}@example.com"
                }
                
                $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
                $success = $false
                $statusCode = 0
                $errorMessage = ""
                
                try {
                    $headers = @{ "Content-Type" = "application/json" }
                    
                    if ($method -eq "GET") {
                        $response = Invoke-WebRequest -Uri $url -Method $method -Headers $headers -UseBasicParsing
                    } else {
                        $jsonBody = $body | ConvertTo-Json
                        $response = Invoke-WebRequest -Uri $url -Method $method -Headers $headers -Body $jsonBody -UseBasicParsing
                    }
                    
                    $statusCode = $response.StatusCode
                    $success = $true
                }
                catch {
                    $errorMessage = $_.Exception.Message
                    if ($_.Exception.Response) {
                        $statusCode = [int]$_.Exception.Response.StatusCode
                    }
                }
                
                $stopwatch.Stop()
                
                $threadResults += [PSCustomObject]@{
                    ThreadId = $ThreadId
                    Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss.fff"
                    Url = $url
                    Method = $method
                    ResponseTime = $stopwatch.ElapsedMilliseconds
                    StatusCode = $statusCode
                    Success = $success
                    ErrorMessage = $errorMessage
                }
                
                # Small delay to simulate real user behavior
                Start-Sleep -Milliseconds 100
            }
        }
        
        return $threadResults
    } -ArgumentList $BaseUrl, $RequestsPerUser, $user, $testScenarios
    
    $jobs += $job
}

# Wait for all jobs to complete and collect results
Write-Host "Waiting for all requests to complete..." -ForegroundColor Yellow

while ($jobs | Where-Object { $_.State -eq 'Running' }) {
    $completed = ($jobs | Where-Object { $_.State -eq 'Completed' }).Count
    $progress = [math]::Round(($completed / $ConcurrentUsers) * 100, 2)
    Write-Progress -Activity "Load Testing in Progress" -Status "$completed of $ConcurrentUsers users completed" -PercentComplete $progress
    Start-Sleep -Seconds 1
}

Write-Progress -Activity "Load Testing in Progress" -Completed

# Collect all results
foreach ($job in $jobs) {
    $jobResults = Receive-Job -Job $job
    $results += $jobResults
    Remove-Job -Job $job
}

$endTime = Get-Date
$duration = ($endTime - $startTime).TotalSeconds

# Calculate statistics
$successfulRequests = ($results | Where-Object { $_.Success }).Count
$failedRequests = $results.Count - $successfulRequests
$avgResponseTime = ($results | Measure-Object -Property ResponseTime -Average).Average
$minResponseTime = ($results | Measure-Object -Property ResponseTime -Minimum).Minimum
$maxResponseTime = ($results | Measure-Object -Property ResponseTime -Maximum).Maximum
$throughput = [math]::Round($results.Count / $duration, 2)

# Calculate percentiles
$sortedTimes = $results | Sort-Object ResponseTime
$p50Index = [math]::Floor($sortedTimes.Count * 0.50)
$p95Index = [math]::Floor($sortedTimes.Count * 0.95)
$p99Index = [math]::Floor($sortedTimes.Count * 0.99)

$p50 = $sortedTimes[$p50Index].ResponseTime
$p95 = $sortedTimes[$p95Index].ResponseTime
$p99 = $sortedTimes[$p99Index].ResponseTime

# Export results to CSV
$results | Export-Csv -Path $OutputFile -NoTypeInformation

# Display summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Load Test Results" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Test Duration: $([math]::Round($duration, 2)) seconds"
Write-Host "Total Requests: $($results.Count)"
Write-Host "Successful Requests: $successfulRequests ($(([math]::Round(($successfulRequests / $results.Count) * 100, 2)))%)"
Write-Host "Failed Requests: $failedRequests ($(([math]::Round(($failedRequests / $results.Count) * 100, 2)))%)"
Write-Host ""
Write-Host "Response Times:" -ForegroundColor Yellow
Write-Host "  Average: $([math]::Round($avgResponseTime, 2)) ms"
Write-Host "  Minimum: $minResponseTime ms"
Write-Host "  Maximum: $maxResponseTime ms"
Write-Host "  50th Percentile (P50): $p50 ms"
Write-Host "  95th Percentile (P95): $p95 ms"
Write-Host "  99th Percentile (P99): $p99 ms"
Write-Host ""
Write-Host "Throughput: $throughput requests/second"
Write-Host ""
Write-Host "Results saved to: $OutputFile" -ForegroundColor Green

# Status by endpoint
Write-Host ""
Write-Host "Performance by Endpoint:" -ForegroundColor Yellow
$results | Group-Object Url | ForEach-Object {
    $endpointStats = $_.Group | Measure-Object -Property ResponseTime -Average
    $successRate = (($_.Group | Where-Object { $_.Success }).Count / $_.Group.Count) * 100
    Write-Host "  $($_.Name)"
    Write-Host "    Requests: $($_.Count)"
    Write-Host "    Avg Response Time: $([math]::Round($endpointStats.Average, 2)) ms"
    Write-Host "    Success Rate: $([math]::Round($successRate, 2))%"
}

Write-Host ""
Write-Host "Load test completed successfully!" -ForegroundColor Green


