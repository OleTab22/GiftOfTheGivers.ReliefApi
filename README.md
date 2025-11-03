# Gift of the Givers Relief API

A comprehensive ASP.NET Core 8.0 Web API for managing disaster relief operations, including incident tracking, volunteer coordination, donation management, and assignment distribution.

## 🔗 Repository Links

- **GitHub**: [https://github.com/OleTab22/GiftOfTheGivers.ReliefApi](https://github.com/OleTab22/GiftOfTheGivers.ReliefApi.git)
- **Azure DevOps**: [https://dev.azure.com/ST10104079/GiftOfTheGivers-ReliefApp](https://dev.azure.com/ST10104079/GiftOfTheGivers-ReliefApp/_git/GiftOfTheGivers-ReliefApp)

## 📋 Features

- **Incident Management**: Track and manage disaster incidents with severity levels
- **Volunteer Coordination**: Register and manage volunteers with their skills
- **Donation Tracking**: Record and track donations from various sources
- **Assignment System**: Assign volunteers to specific incidents and tasks
- **User Authentication**: JWT-based secure authentication system
- **Interactive API Documentation**: Swagger/OpenAPI integration

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Authentication**: JWT (JSON Web Tokens)
- **Database**: Entity Framework Core with In-Memory Database
- **API Documentation**: Swagger/Swashbuckle
- **Password Hashing**: BCrypt.Net
- **Testing**: xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing

## 🧪 Comprehensive Testing Suite

This project includes extensive testing coverage across multiple testing types:

### Unit Testing (✅ 100% Pass Rate)
- **Coverage**: 31 unit tests
- **Framework**: xUnit with Moq
- **Tests**: Controllers, Services, Authentication, Data Access Layer
- **Code Coverage**: Detailed reports in `TestResults/CoverageReport/`

### Integration Testing (✅ 100% Pass Rate)
- **Coverage**: 23 integration tests
- **Tests**: API endpoints, database interactions, cross-module communication
- **Approach**: In-memory testing with WebApplicationFactory

### Load Testing (✅ Successfully Completed)
- **Tool**: PowerShell automated load testing
- **Metrics**: Response times, throughput, resource utilization
- **Results**: Baseline performance established with 100 concurrent users

### Stress Testing (✅ Successfully Completed)
- **Stages**: 50 → 150 → 300 → 500 concurrent users
- **Analysis**: Bottleneck identification, breaking point estimation
- **Results**: System capacity and degradation points identified

### UI Testing (✅ 23 Tests Pass)
- **Coverage**: Swagger interface, API endpoints, navigation, error handling
- **Framework**: Microsoft.AspNetCore.Mvc.Testing

### Usability Testing (✅ Completed)
- **Participants**: Real user feedback sessions
- **Focus**: Navigation, layout, accessibility, user experience
- **Report**: Detailed findings with improvement recommendations

## 📊 Test Reports

Comprehensive test reports are available in the `Documentation/TestReports/` directory:
- `Comprehensive_Test_Report.md` - Master report with all test results
- `Unit_Test_Report.md` - Detailed unit test results
- `Integration_Test_Report.md` - Integration test analysis
- `Load_Test_Report.md` - Load testing metrics
- `Stress_Test_Report.md` - Stress test analysis
- `UI_Test_Report.md` - UI testing results
- `Pipeline_Configuration_Report.md` - CI/CD pipeline documentation

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code
- Git

### Installation

1. Clone the repository:
```bash
git clone https://github.com/OleTab22/GiftOfTheGivers.ReliefApi.git
cd GiftOfTheGivers-ReliefApp
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the project:
```bash
dotnet build
```

4. Run the application:
```bash
dotnet run
```

The API will be available at `http://localhost:5268` or `https://localhost:7268`

### Access Swagger UI
Navigate to `http://localhost:5268/swagger` to access the interactive API documentation.

## 🧪 Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Unit Tests Only
```bash
dotnet test --filter "FullyQualifiedName~UnitTests"
```

### Run Integration Tests Only
```bash
dotnet test --filter "FullyQualifiedName~IntegrationTests"
```

### Run with Code Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Generate Coverage Report
```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:TestResults/CoverageReport -reporttypes:Html
```

### Run Load/Stress Tests
```powershell
# Quick load test
PowerShell -ExecutionPolicy Bypass -File "LoadTests\QuickLoadTest.ps1"

# Comprehensive stress test
PowerShell -ExecutionPolicy Bypass -File "LoadTests\StressTest.ps1"
```

## 📁 Project Structure

```
GiftOfTheGivers-ReliefApp/
├── Controllers/              # API Controllers
├── Models/                   # Data Models
├── Services/                 # Business Logic Services
├── Data/                     # Database Context
├── Swagger/                  # Swagger Configuration
├── Tests/                    # Test Projects
│   ├── UnitTests/           # Unit Tests
│   ├── IntegrationTests/    # Integration Tests
│   └── UITests/             # UI Tests
├── LoadTests/               # Load & Stress Testing Scripts
├── Documentation/           # Project Documentation
│   ├── TestReports/        # Test Reports
│   └── Usability_Testing_Report.md
├── azure-pipelines.yml      # CI/CD Pipeline Configuration
└── README.md               # This file
```

## 🔐 Authentication

The API uses JWT (JSON Web Token) authentication. To access protected endpoints:

1. Register a new user via `/api/auth/register`
2. Login via `/api/auth/login` to receive a JWT token
3. Include the token in Swagger UI or as a query parameter: `?token=YOUR_TOKEN`

## 🚀 CI/CD Pipeline

The project includes a comprehensive Azure DevOps CI/CD pipeline with:
- **Build Stage**: Restore, build, and compile
- **Test Stage**: Unit tests, integration tests with code coverage
- **Security Scan**: Dependency and security analysis
- **Multi-Environment Deployment**: Dev, Staging, Production
- **Automated Testing**: Post-deployment verification
- **Rollback Mechanisms**: Automatic rollback on failure

View the pipeline configuration in `azure-pipelines.yml`

## 📈 Performance Metrics

- **Unit Test Coverage**: High coverage across all controllers and services
- **Integration Test Success Rate**: 100%
- **API Response Time (Baseline)**: Average < 50ms
- **Load Capacity**: Successfully handles 100+ concurrent users
- **System Breaking Point**: Identified at ~500 concurrent users

## 📖 Documentation

- **API Documentation**: Available via Swagger UI at `/swagger`
- **Test Reports**: `Documentation/TestReports/`
- **Usability Report**: `Documentation/Usability_Testing_Report.md`
- **Pipeline Guide**: `Documentation/TestReports/Pipeline_Configuration_Report.md`

## 🤝 Contributing

This project was developed as part of a POE (Portfolio of Evidence) assignment demonstrating:
- RESTful API development
- Comprehensive testing strategies
- Load and stress testing
- CI/CD pipeline implementation
- Azure cloud deployment

## 👨‍💻 Author

**Student ID**: ST10104079

## 📝 License

This project is developed for educational purposes as part of a software development course.

## 🙏 Acknowledgments

- Gift of the Givers Foundation for the project inspiration
- Course instructors and teaching staff
- Testing frameworks and tools communities

---

**Note**: This is an educational project demonstrating comprehensive software testing and deployment practices for disaster relief management systems.
