# Azure Deployment Setup Checklist

## ✅ Step 1: Azure Resources Created
- **Deployment**: Microsoft.Web-ASP-Portal-a4a3f848-a04f
- **Subscription**: Azure subscription 1
- **Resource Group**: `rg-gotg-reliefapp-01`
- **Deployment Time**: 04/11/2025, 01:04:51
- **Status**: ✅ COMPLETE

## 📋 Next Steps to Complete CI/CD

### Step 2: Get Your App Service Details

1. Go to Azure Portal: https://portal.azure.com
2. Navigate to Resource Groups → `rg-gotg-reliefapp-01`
3. Find your App Service (should be named something like `gotg-reliefapp` or similar)
4. Note down:
   - **App Service Name**: `_____________________`
   - **App Service URL**: `https://_____________________.azurewebsites.net`
   - **App Service Plan**: `_____________________`

### Step 3: Configure App Settings

In Azure Portal, go to your App Service:
1. Click "Configuration" → "Application settings"
2. Add these settings:

```
Name: Jwt__Key
Value: your-super-secret-jwt-key-at-least-32-characters-long-for-production

Name: Jwt__Issuer
Value: GiftOfTheGivers.ReliefApi

Name: Jwt__Audience
Value: GiftOfTheGivers.Users

Name: Jwt__ExpiryMinutes
Value: 60

Name: ASPNETCORE_ENVIRONMENT
Value: Production
```

3. Click "Save"

### Step 4: Create Service Connection in Azure DevOps

1. Go to: https://dev.azure.com/ST10104079/GiftOfTheGivers-ReliefApp
2. Click "Project settings" (bottom left corner)
3. Under "Pipelines", click "Service connections"
4. Click "New service connection"
5. Select "Azure Resource Manager" → Click "Next"
6. Authentication method: "Service principal (automatic)"
7. Scope level: "Subscription"
8. Subscription: Select "Azure subscription 1"
9. Resource group: Select `rg-gotg-reliefapp-01`
10. Service connection name: `Azure-GOTG-Connection`
11. ✅ Check "Grant access permission to all pipelines"
12. Click "Save"

### Step 5: Update Pipeline Variables

Update `azure-pipelines.yml` with your values:

```yaml
variables:
  azureSubscription: 'Azure-GOTG-Connection'
  webAppName: 'YOUR-APP-SERVICE-NAME'  # Replace with actual name
  resourceGroupName: 'rg-gotg-reliefapp-01'
  projectName: 'GiftOfTheGivers.ReliefApi'
  testProject: 'Tests/GiftOfTheGivers.ReliefApi.Tests.csproj'
  dotnetSdkVersion: '8.x'
```

### Step 6: Create Environments in Azure DevOps

1. Go to Pipelines → Environments
2. Create these environments:

**Development Environment:**
- Click "New environment"
- Name: `Development`
- Resource: None
- Click "Create"

**Staging Environment:**
- Click "New environment"
- Name: `Staging`
- Resource: None
- Click "Create"
- After creation, click "..." → "Approvals and checks"
- Add yourself as approver

**Production Environment:**
- Click "New environment"
- Name: `Production`
- Resource: None
- Click "Create"
- After creation, click "..." → "Approvals and checks"
- Add yourself as approver

### Step 7: Create the Pipeline

1. In Azure DevOps, go to: Pipelines → Pipelines
2. Click "New pipeline"
3. Select "Azure Repos Git"
4. Select your repository: `GiftOfTheGivers-ReliefApp`
5. Select "Existing Azure Pipelines YAML file"
6. Branch: `main`
7. Path: `/azure-pipelines.yml`
8. Click "Continue"
9. Review the YAML
10. Click "Run"

### Step 8: Monitor Pipeline Execution

Watch the pipeline execute these stages:
1. **Build** - Compile and test
2. **Test** - Security and quality checks
3. **Deploy_Dev** - Auto-deploy to Development
4. **Deploy_Staging** - Deploy to Staging (may need approval)
5. **Deploy_Production** - Deploy to Production (needs approval)

### Step 9: Verify Deployment

Once deployed, test your application:

1. **Visit App URL**: `https://YOUR-APP-NAME.azurewebsites.net/swagger`
2. **Test Authentication**:
   - Register a user
   - Login to get token
3. **Test API Endpoints**:
   - Create an incident
   - List incidents
   - Export to CSV

### Step 10: Test Automated Deployment

Make a small change and push to test auto-deployment:

```bash
# Make a small change
echo "Updated: $(date)" >> README.md

# Commit and push
git add README.md
git commit -m "Test automated CI/CD deployment"
git push azure main
```

Watch the pipeline automatically trigger! 🚀

## 📸 Screenshots to Capture for Submission

### Azure Portal Screenshots:
1. ✅ Resource group showing all resources
2. ✅ App Service overview page
3. ✅ App Service configuration (Application settings)
4. ✅ App Service URL in browser showing Swagger UI

### Azure DevOps Screenshots:
5. ✅ Service connection configuration
6. ✅ Environment setup (Development, Staging, Production)
7. ✅ Pipeline overview showing all stages
8. ✅ Successful pipeline run (all stages green)
9. ✅ Build stage details
10. ✅ Test results with code coverage
11. ✅ Deployment stage showing successful deployment
12. ✅ Pipeline triggers configuration

### Application Screenshots:
13. ✅ Swagger UI running on Azure URL
14. ✅ API endpoint test (POST /api/auth/register)
15. ✅ API endpoint test (POST /api/auth/login showing JWT token)
16. ✅ API endpoint test (GET /api/incidents with auth)
17. ✅ Application logs showing successful deployment

## ✅ Success Criteria Checklist

- [ ] Azure resources created successfully
- [ ] App Service accessible via URL
- [ ] Service connection configured in Azure DevOps
- [ ] Environments created (Dev, Staging, Production)
- [ ] Pipeline created and linked to repository
- [ ] Pipeline triggers automatically on git push
- [ ] Build stage completes successfully
- [ ] Tests run and pass (with code coverage)
- [ ] Application deploys to Development automatically
- [ ] Application deploys to Staging (with approval)
- [ ] Application deploys to Production (with approval)
- [ ] Swagger UI accessible on Azure URL
- [ ] API endpoints respond correctly
- [ ] JWT authentication works
- [ ] All screenshots captured

## 🎯 What You're Demonstrating (10 Marks)

### Continuous Integration (CI):
✅ Automated build on git push  
✅ Automated testing (unit + integration)  
✅ Code quality checks  
✅ Code coverage reporting  

### Continuous Deployment (CD):
✅ Automated deployment to Azure App Service  
✅ Multi-environment deployment (Dev/Staging/Prod)  
✅ Approval gates for production  
✅ Rollback capabilities  

### Pipeline Components:
✅ Build tasks (restore, compile, publish)  
✅ Test tasks (unit tests, integration tests)  
✅ Package tasks (create deployment artifacts)  
✅ Deploy tasks (deploy to Azure App Service)  

## 📝 Evidence Document Structure

Create a document with:

1. **Introduction**
   - Project overview
   - CI/CD goals

2. **Azure Resources**
   - Resource group details
   - App Service configuration
   - Screenshots

3. **Pipeline Configuration**
   - azure-pipelines.yml breakdown
   - Stage descriptions
   - Screenshots

4. **Deployment Process**
   - How deployment works
   - Approval workflow
   - Screenshots of successful deployments

5. **Testing Integration**
   - Unit test execution in pipeline
   - Code coverage results
   - Screenshots

6. **Monitoring and Verification**
   - Application running on Azure
   - API testing results
   - Screenshots

## 🔄 Rollback Procedure (For Documentation)

If deployment fails, rollback using:

```bash
# Via Azure CLI
az webapp deployment slot swap \
  --name YOUR-APP-NAME \
  --resource-group rg-gotg-reliefapp-01 \
  --slot staging \
  --target-slot production
```

Or via Azure Portal:
1. Go to App Service → Deployment slots
2. Click "Swap"
3. Select slots to swap
4. Click "Swap"

## 💰 Cost Management

**Current Setup Cost** (Free/Basic tier):
- Resource Group: Free
- App Service Plan (Basic B1): ~$13/month
- App Service: Included in plan
- Azure DevOps (Basic): Free for 5 users

**After demonstration:**
```bash
# Delete all resources to avoid charges
az group delete --name rg-gotg-reliefapp-01 --yes --no-wait
```

## 📚 References

- Deployment Guide: `Documentation/Azure_Deployment_Guide.md`
- Pipeline Configuration: `azure-pipelines.yml`
- Test Reports: `Documentation/TestReports/`
- README: `README.md`

---

**Deployment ID**: 6b3d2602-bd8a-4934-b611-324aeb7e4464  
**Completion Date**: November 4, 2025  
**Status**: ✅ READY FOR PIPELINE CONFIGURATION

## Next Immediate Action

👉 **Go to Azure Portal and get your App Service name, then update `azure-pipelines.yml`!**

