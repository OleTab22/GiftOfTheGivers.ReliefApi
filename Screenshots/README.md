# Screenshots Directory
## Gift of the Givers Relief API - Testing and Deployment

This directory contains all screenshots required for the POE submission.

---

## Screenshot Checklist (29 Screenshots Total)

### Unit Testing Screenshots (01-05)

- [ ] `01_Unit_Tests_Passing.png` - Test Explorer showing all 45 unit tests passing
- [ ] `02_Unit_Tests_Details.png` - Detailed test execution view with individual test results
- [ ] `03_Code_Coverage_Summary.png` - Overall code coverage metrics (90.8% line, 68.9% branch)
- [ ] `04_Code_Coverage_Controllers.png` - Controller-level coverage details
- [ ] `05_Sample_Test_Code.png` - Example test code structure (AuthControllerTests.cs)

### Integration Testing Screenshots (06-08)

- [ ] `06_Integration_Tests_Passing.png` - Integration test results showing all 18 tests passing
- [ ] `07_Integration_Test_Details.png` - Detailed integration test execution
- [ ] `08_Cross_Module_Tests.png` - Cross-module integration test results (8 tests)

### Load Testing Screenshots (09-12)

- [ ] `09_Load_Test_Configuration.png` - JMeter test plan configuration (100 concurrent users)
- [ ] `10_Load_Test_Execution.png` - Load test in progress showing live metrics
- [ ] `11_Load_Test_Results.png` - Load test results summary (response times, throughput)
- [ ] `12_Load_Test_Metrics.png` - Performance metrics and graphs

### Stress Testing Screenshots (13-16)

- [ ] `13_Stress_Test_Configuration.png` - Stress test configuration (multi-stage: 50, 150, 300, 500 users)
- [ ] `14_Stress_Test_Stage1.png` - Stage 1 results (50 users) - baseline performance
- [ ] `15_Stress_Test_Stage4.png` - Stage 4 results (500 users) - extreme load performance
- [ ] `16_Stress_Test_Analysis.png` - Bottleneck analysis and degradation points

### UI Testing Screenshots (17-21)

- [ ] `17_Swagger_UI_Overview.png` - Swagger UI interface showing all endpoints
- [ ] `18_Swagger_Registration.png` - User registration through Swagger UI
- [ ] `19_Swagger_Authentication.png` - Bearer token authorization in Swagger
- [ ] `20_Swagger_CSV_Export.png` - CSV export functionality and results
- [ ] `21_UI_Test_Results.png` - UI test execution results (23/23 passing)

### Pipeline and Deployment Screenshots (22-29)

- [ ] `22_Pipeline_Overview.png` - Complete pipeline visualization in Azure DevOps
- [ ] `23_Build_Stage.png` - Build stage execution showing compile, test, coverage
- [ ] `24_Pipeline_Tests.png` - Test results displayed in pipeline
- [ ] `25_Pipeline_Coverage.png` - Code coverage metrics in pipeline dashboard
- [ ] `26_Staging_Deployment.png` - Staging deployment execution and success
- [ ] `27_Production_Approval.png` - Production approval gate interface
- [ ] `28_Pipeline_YAML.png` - Pipeline configuration (azure-pipelines.yml) in editor
- [ ] `29_Rollback_Config.png` - Rollback mechanism configuration and error handling

---

## Instructions for Capturing Screenshots

### Unit Testing Screenshots

**01_Unit_Tests_Passing.png:**
1. Open Visual Studio or VS Code
2. Run all unit tests: `dotnet test Tests/GiftOfTheGivers.ReliefApi.Tests.csproj --filter "FullyQualifiedName~UnitTests"`
3. Take screenshot of Test Explorer showing all tests passing

**02_Unit_Tests_Details.png:**
1. Click on a specific test to show details
2. Show execution time, test output, etc.

**03_Code_Coverage_Summary.png:**
1. Generate coverage report: `dotnet test --collect:"XPlat Code Coverage"`
2. Generate HTML: `reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"TestResults/Coverage"`
3. Open `TestResults/Coverage/index.html` in browser
4. Take screenshot of coverage summary

### Integration Testing Screenshots

**06_Integration_Tests_Passing.png:**
1. Run integration tests: `dotnet test --filter "FullyQualifiedName~IntegrationTests"`
2. Capture test results window

### Load Testing Screenshots

**09_Load_Test_Configuration.png:**
1. Open JMeter: `LoadTests/ReliefAPI_LoadTest.jmx`
2. Show thread group configuration (100 users, 10 requests)
3. Take screenshot

**10_Load_Test_Execution.png:**
1. Run load test in JMeter
2. Show live results with response times
3. Capture during execution

### Pipeline Screenshots

**22_Pipeline_Overview.png:**
1. Open Azure DevOps
2. Navigate to Pipelines
3. Show complete pipeline visualization
4. Capture all stages

**28_Pipeline_YAML.png:**
1. Open `azure-pipelines.yml` in editor
2. Show complete configuration
3. Highlight key sections

---

## Screenshot Naming Convention

Format: `##_Description.png`

- Use two-digit numbers (01, 02, etc.)
- Descriptive names with underscores
- PNG format for clarity

---

## File Organization

All screenshots should be placed directly in this `Screenshots/` directory with the exact names listed above.

---

**Last Updated:** November 3, 2025

