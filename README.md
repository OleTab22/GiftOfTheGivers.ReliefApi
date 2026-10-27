# Disaster Relief Management API

**An ASP.NET Core 8 REST API for incident response, volunteer coordination, donations, and assignments.**

This project models the backend workflows of a disaster-relief organisation. It demonstrates API design, authentication, Entity Framework Core, automated testing, and deployment-related documentation.

It is an **independent educational software project inspired by humanitarian relief work** and is not presented as an official system operated by Gift of the Givers.

## Core capabilities

| Module | What it manages |
| --- | --- |
| **Incidents** | Disaster incidents and severity information |
| **Volunteers** | Volunteer records and skills |
| **Donations** | Donation records |
| **Assignments** | Assigning volunteers to relief incidents or tasks |
| **Accounts** | User registration/login and JWT-based authentication |
| **API discovery** | Interactive Swagger/OpenAPI interface |

## Technology stack

- **Runtime:** C#, .NET 8, ASP.NET Core Web API
- **Data:** Entity Framework Core with its In-Memory provider
- **Authentication:** JWT Bearer and BCrypt password hashing
- **Documentation:** Swagger / Swashbuckle
- **Testing:** xUnit, Moq, WebApplicationFactory, Coverlet
- **Automation:** Azure Pipelines configuration and PowerShell load/stress-test scripts

## Project structure

| Path | Responsibility |
| --- | --- |
| [`Controllers/`](Controllers/) | API endpoints for incidents, volunteers, donations, assignments, and authentication |
| [`Models/`](Models/) | Data and request models |
| [`Data/`](Data/) | EF Core database context |
| [`Services/`](Services/) | Application services |
| [`Tests/`](Tests/) | Unit, integration, and UI-oriented test sources |
| [`LoadTests/`](LoadTests/) | PowerShell load/stress testing |
| [`Documentation/TestReports/`](Documentation/TestReports/) | Historical test and pipeline reports |
| [`Screenshots/`](Screenshots/) | Captures from development and verification |

## Run locally

**Prerequisites:** .NET 8 SDK and Git. Visual Studio or VS Code is optional.

1. Clone the repository and enter the actual repository directory:

   ```bash
   git clone https://github.com/OleTab22/GiftOfTheGivers.ReliefApi.git
   cd GiftOfTheGivers.ReliefApi
   ```

2. Restore, build, and start the API:

   ```bash
   dotnet restore GiftOfTheGivers.ReliefApi.csproj
   dotnet build GiftOfTheGivers.ReliefApi.csproj
   dotnet run --project GiftOfTheGivers.ReliefApi.csproj --launch-profile http
   ```

3. Open the Swagger interface at http://localhost:5268/swagger.

The default data store uses **EF Core In-Memory**: records are not persisted across restarts. This configuration is suitable for demonstrations and tests, not production data storage.

## Authentication

The API provides account registration and login endpoints. Use the JWT returned at login as a **Bearer token in the Authorization header** for protected requests. Swagger includes a Bearer authentication control.

The current repository contains development-oriented JWT settings and a query-string token fallback. Review and harden these before exposing the service publicly: use a secret store or environment configuration, validate expected issuers/audiences, avoid tokens in URLs, and configure a persistent database.

## Testing

Run the dedicated test project:

```bash
dotnet test Tests/GiftOfTheGivers.ReliefApi.Tests.csproj
```

Collect coverage (where supported by the test environment):

```bash
dotnet test Tests/GiftOfTheGivers.ReliefApi.Tests.csproj --collect:"XPlat Code Coverage"
```

The source includes unit tests for controllers and token services, integration tests using ASP.NET Core test hosting, and other testing resources. Reports in [`Documentation/TestReports/`](Documentation/TestReports/) document earlier test exercises; **their pass rates and performance measurements should not be treated as current without rerunning the tests**.

PowerShell test scripts are available in [`LoadTests/`](LoadTests/). The [`azure-pipelines.yml`](azure-pipelines.yml) file describes pipeline automation; a configuration file alone is not proof of a successful deployment.

## Project status and limitations

This repository demonstrates backend implementation and testing practices. It currently uses an in-memory database and development authentication defaults, so production deployment would require security hardening, durable storage, monitored builds, and new verification of the tests and performance.

## Author and acknowledgement

Developed by **Olebogeng Tabane**. The humanitarian use case was inspired by the work of Gift of the Givers; the project is not affiliated with or endorsed by the organisation.
