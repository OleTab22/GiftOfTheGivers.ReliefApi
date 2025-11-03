# Azure Deployment Guide - CI/CD Pipeline Setup

This guide walks through setting up automated deployment to Azure App Services using Azure DevOps CI/CD pipeline.

## Prerequisites

- Azure Account with active subscription
- Azure DevOps Organization and Project
- Repository pushed to Azure DevOps

## Step 1: Create Azure Resources

### Option A: Using Azure Portal (Recommended for beginners)

1. **Login to Azure Portal**: https://portal.azure.com

2. **Create Resource Group**
   - Click "Resource groups" → "Create"
   - Subscription: Select your subscription
   - Resource group name: `rg-giftofthegivers-reliefapp`
   - Region: `South Africa North` or your preferred region
   - Click "Review + Create" → "Create"

3. **Create App Service Plan**
   - Search for "App Service plans" → "Create"
   - Resource Group: `rg-giftofthegivers-reliefapp`
   - Name: `asp-reliefapp`
   - Operating System: `Linux`
   - Region: `South Africa North` (same as resource group)
   - Pricing tier: `Free F1` (for testing) or `Basic B1` (for production)
   - Click "Review + Create" → "Create"

4. **Create App Service**
   - Search for "App Services" → "Create" → "Web App"
   - Resource Group: `rg-giftofthegivers-reliefapp`
   - Name: `giftofthegivers-reliefapp` (must be globally unique)
   - Publish: `Code`
   - Runtime stack: `.NET 8 (LTS)`
   - Operating System: `Linux`
   - Region: `South Africa North`
   - App Service Plan: Select `asp-reliefapp` (created above)
   - Click "Review + Create" → "Create"

5. **Note your App Service URL**: `https://giftofthegivers-reliefapp.azurewebsites.net`

### Option B: Using Azure CLI

```bash
# Login to Azure
az login

# Set your subscription (if you have multiple)
az account set --subscription "Your-Subscription-Name"

# Create Resource Group
az group create \
  --name rg-giftofthegivers-reliefapp \
  --location "southafricanorth"

# Create App Service Plan
az appservice plan create \
  --name asp-reliefapp \
  --resource-group rg-giftofthegivers-reliefapp \
  --sku B1 \
  --is-linux

# Create Web App
az webapp create \
  --name giftofthegivers-reliefapp \
  --resource-group rg-giftofthegivers-reliefapp \
  --plan asp-reliefapp \
  --runtime "DOTNETCORE:8.0"

# Configure app settings
az webapp config appsettings set \
  --name giftofthegivers-reliefapp \
  --resource-group rg-giftofthegivers-reliefapp \
  --settings ASPNETCORE_ENVIRONMENT=Production
```

## Step 2: Create Service Connection in Azure DevOps

1. **Navigate to Azure DevOps Project**
   - Go to https://dev.azure.com/ST10104079/GiftOfTheGivers-ReliefApp

2. **Create Service Connection**
   - Click "Project settings" (bottom left)
   - Under "Pipelines", click "Service connections"
   - Click "New service connection"
   - Select "Azure Resource Manager"
   - Click "Next"

3. **Configure Service Principal**
   - Authentication method: "Service principal (automatic)"
   - Scope level: "Subscription"
   - Subscription: Select your Azure subscription
   - Resource group: `rg-giftofthegivers-reliefapp`
   - Service connection name: `Azure-GiftOfTheGivers-Connection`
   - Grant access permission to all pipelines: ✅ Checked
   - Click "Save"

## Step 3: Update Pipeline Variables

Update `azure-pipelines.yml` with your actual values:

```yaml
variables:
  azureSubscription: 'Azure-GiftOfTheGivers-Connection'  # Your service connection name
  webAppName: 'giftofthegivers-reliefapp'  # Your App Service name
  resourceGroupName: 'rg-giftofthegivers-reliefapp'
```

## Step 4: Set Up Pipeline in Azure DevOps

1. **Navigate to Pipelines**
   - In Azure DevOps, click "Pipelines" → "Pipelines"
   - Click "New pipeline"

2. **Connect to Repository**
   - Select "Azure Repos Git"
   - Select your repository: `GiftOfTheGivers-ReliefApp`

3. **Configure Pipeline**
   - Select "Existing Azure Pipelines YAML file"
   - Branch: `main`
   - Path: `/azure-pipelines.yml`
   - Click "Continue"

4. **Review and Run**
   - Review the pipeline YAML
   - Click "Run"

## Step 5: Configure Deployment Environments

### Create Environments in Azure DevOps

1. **Navigate to Environments**
   - Click "Pipelines" → "Environments"
   - Click "New environment"

2. **Create Development Environment**
   - Name: `Development`
   - Resource: None (leave empty)
   - Click "Create"

3. **Create Staging Environment**
   - Name: `Staging`
   - Add Approvals:
     - Click on environment → "..." → "Approvals and checks"
     - Click "Approvals"
     - Add yourself as approver
     - Click "Create"

4. **Create Production Environment**
   - Name: `Production`
   - Add Approvals:
     - Require 2 approvers (if possible)
     - Add pre-deployment approvals
     - Click "Create"

## Step 6: Verify Pipeline Configuration

Your `azure-pipelines.yml` should include these stages:

### ✅ Build Stage
```yaml
- stage: Build
  jobs:
  - job: BuildJob
    steps:
    - task: UseDotNet@2
    - task: DotNetCoreCLI@2  # Restore
    - task: DotNetCoreCLI@2  # Build
    - task: DotNetCoreCLI@2  # Test with coverage
    - task: PublishCodeCoverageResults@1
    - task: DotNetCoreCLI@2  # Publish
    - task: PublishBuildArtifacts@1
```

### ✅ Test Stage
```yaml
- stage: Test
  jobs:
  - job: SecurityScan
  - job: StaticAnalysis
```

### ✅ Deploy Stages
```yaml
- stage: Deploy_Dev
  jobs:
  - deployment: DeployWeb
    environment: 'Development'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureWebApp@1
```

## Step 7: Configure App Settings in Azure

Add required configuration to your App Service:

```bash
# Set JWT configuration
az webapp config appsettings set \
  --name giftofthegivers-reliefapp \
  --resource-group rg-giftofthegivers-reliefapp \
  --settings \
    Jwt__Key="your-super-secret-key-min-32-characters-long" \
    Jwt__Issuer="GiftOfTheGivers.ReliefApi" \
    Jwt__Audience="GiftOfTheGivers.Users" \
    Jwt__ExpiryMinutes="60" \
    ASPNETCORE_ENVIRONMENT="Production"
```

Or via Azure Portal:
1. Go to your App Service
2. Click "Configuration" → "Application settings"
3. Add the settings above
4. Click "Save"

## Step 8: Enable Continuous Deployment

The pipeline is configured to trigger automatically on push to `main`:

```yaml
trigger:
  branches:
    include:
    - main
  paths:
    exclude:
    - README.md
    - Documentation/**
```

## Step 9: Test the Pipeline

1. **Push Code Changes**
```bash
git add .
git commit -m "Configure Azure deployment pipeline"
git push azure main
```

2. **Monitor Pipeline Execution**
   - Go to Azure DevOps → Pipelines → Pipelines
   - Click on the running pipeline
   - Monitor each stage: Build → Test → Deploy_Dev

3. **Verify Deployment**
   - Once deployment completes, visit: `https://giftofthegivers-reliefapp.azurewebsites.net`
   - Test the API: `https://giftofthegivers-reliefapp.azurewebsites.net/swagger`

## Step 10: Test Automated Deployment

Make a small change to test auto-deployment:

```bash
# Make a change
echo "# Updated" >> README.md

# Commit and push
git add README.md
git commit -m "Test automated deployment"
git push azure main
```

Watch the pipeline automatically trigger and deploy!

## Pipeline Stages Explained

### 🔨 Build Stage (Automated)
- Restores NuGet packages
- Compiles the application
- Runs all unit and integration tests
- Collects code coverage (target: >80%)
- Publishes build artifacts

### 🧪 Test Stage (Automated)
- Security vulnerability scanning
- Static code analysis
- Dependency checks

### 🚀 Deploy to Development (Automated)
- Deploys to Dev environment
- No approval required
- Runs basic smoke tests

### 🏗️ Deploy to Staging (Manual Approval)
- Requires 1 approver
- Deploys to Staging slot
- Runs integration tests
- Performance testing

### 🎯 Deploy to Production (Manual Approval)
- Requires 2 approvers (if available)
- Blue-green deployment
- Rollback capability
- Post-deployment verification

## Monitoring and Troubleshooting

### View Pipeline Logs
1. Go to Azure DevOps → Pipelines
2. Click on pipeline run
3. Click on specific job to see logs

### View App Service Logs
```bash
# Enable logging
az webapp log config \
  --name giftofthegivers-reliefapp \
  --resource-group rg-giftofthegivers-reliefapp \
  --application-logging filesystem \
  --level information

# Stream logs
az webapp log tail \
  --name giftofthegivers-reliefapp \
  --resource-group rg-giftofthegivers-reliefapp
```

### Common Issues and Solutions

**Issue: Pipeline fails at build**
- Check .NET SDK version matches (8.0)
- Verify all NuGet packages restore correctly

**Issue: Deployment fails**
- Verify service connection is valid
- Check App Service name is correct
- Ensure resource group exists

**Issue: App doesn't start**
- Check App Service logs
- Verify appsettings are configured
- Check runtime stack matches (.NET 8)

## Rollback Procedure

If deployment fails:

```bash
# List deployment history
az webapp deployment list \
  --name giftofthegivers-reliefapp \
  --resource-group rg-giftofthegivers-reliefapp

# Rollback to previous version
az webapp deployment slot swap \
  --name giftofthegivers-reliefapp \
  --resource-group rg-giftofthegivers-reliefapp \
  --slot staging \
  --target-slot production
```

## Success Criteria

✅ Pipeline triggers automatically on git push  
✅ All tests pass before deployment  
✅ Code coverage meets threshold (>80%)  
✅ Application deploys successfully  
✅ Application is accessible via Azure URL  
✅ Swagger UI loads correctly  
✅ API endpoints respond as expected  
✅ JWT authentication works  

## Evidence for Submission

### Screenshots to Capture:

1. **Azure Portal**
   - Resource group with all resources
   - App Service overview page
   - App Service URL working

2. **Azure DevOps**
   - Pipeline overview showing stages
   - Successful pipeline run
   - Test results summary
   - Code coverage results

3. **Pipeline Configuration**
   - azure-pipelines.yml content
   - Service connection configuration
   - Environment setup

4. **Deployment Proof**
   - Swagger UI running on Azure URL
   - API response from deployed app
   - Application logs showing successful start

### Documentation to Include:

1. This deployment guide
2. Pipeline configuration (azure-pipelines.yml)
3. Screenshots of successful deployments
4. Test execution reports
5. Performance metrics

## Cost Optimization

- **Development**: Use Free F1 tier
- **Testing**: Use Basic B1 tier
- **Production**: Scale to Standard S1 or Premium as needed

Remember to delete resources after demonstration:
```bash
az group delete --name rg-giftofthegivers-reliefapp --yes
```

## Additional Resources

- [Azure App Service Documentation](https://docs.microsoft.com/azure/app-service/)
- [Azure DevOps Pipelines](https://docs.microsoft.com/azure/devops/pipelines/)
- [ASP.NET Core Deployment](https://docs.microsoft.com/aspnet/core/host-and-deploy/azure-apps/)

---

**Note**: Replace `giftofthegivers-reliefapp` with your unique App Service name throughout this guide.

## Summary

Your CI/CD pipeline provides:
- ✅ Automated builds on every push
- ✅ Comprehensive testing before deployment
- ✅ Multi-environment deployment (Dev/Staging/Prod)
- ✅ Manual approval gates
- ✅ Rollback capabilities
- ✅ Monitoring and logging

This demonstrates enterprise-grade deployment practices! 🚀

