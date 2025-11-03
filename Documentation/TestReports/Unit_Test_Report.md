# Unit Test Report
## Gift of the Givers Relief API

---

## Summary
- Total Unit + Integration Tests Executed (combined run): 49
- Passed: 49
- Failed: 0
- Pass Rate: 100%
- Execution Time: ~6 seconds
- Line Coverage (overall): 90.8%

Coverage source (HTML dashboard): `TestResults/CoverageReport/index.html`
Raw coverage file: `TestResults/**/coverage.cobertura.xml`

---

## Environment
- .NET SDK: 8.0.415
- Test Framework: xUnit
- OS: Windows 10 (Build 26100)

---

## Test Areas Covered
- AuthController (registration, login, invalid credentials)
- IncidentsController (create, get, list, filter, status update, export CSV)
- VolunteersController (create, get)
- AssignmentsController (create, complete, list by volunteer, validation)
- DonationsController (create, get, list, filter, status update)
- JwtTokenService (claims, expiry, structure)

---

## Screenshots (to attach)
- ![All unit tests passing](../Screenshots/01_Unit_Tests_Passing.png)
- ![Unit tests details](../Screenshots/02_Unit_Tests_Details.png)
- ![Coverage summary](../Screenshots/03_Code_Coverage_Summary.png)
- ![Controller coverage](../Screenshots/04_Code_Coverage_Controllers.png)
- ![Sample unit test code](../Screenshots/05_Sample_Test_Code.png)

---

## Notes & Observations
- One analyzer warning (xUnit2002) flagged use of Assert.NotNull on value type in a unit test; non-blocking.
- Controllers and services show strong coverage; branch coverage opportunities exist in error paths.

---

## Recommendations
- Add more negative-path tests for validation errors and authorization failures.
- Increase branch coverage in `Program.cs` auth configuration branches.

