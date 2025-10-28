# OrgChart Standalone Migration Guide

This guide explains how to extract the standalone OrgChart application to a separate repository and deploy it independently.

## What's Been Isolated

### 1. Backend (.NET 8 API)
- **Single controller**: `OrgChartController` with one endpoint
- **Minimal service**: `OrgChartService` fetches JSON from external URL
- **Configuration**: Simple `appsettings.json` with data source URL
- **No dependencies**: No database, no complex authentication
- **Optional auth**: Can enable/disable authentication via config

### 2. Frontend (React 18)
- **Standalone component**: Complete OrgChart visualization
- **API client**: Simple fetch-based client
- **React Query**: For data fetching and caching
- **TailwindCSS**: For styling (via CDN)
- **TypeScript**: Full type safety

### 3. Deployment Options
- **Docker**: Multi-stage build with frontend + backend
- **Azure Scripts**: Deploy to existing App Service Plan
- **Local development**: Simple run scripts

## Migration Steps

### Step 1: Create New Repository
```bash
# Create new repository
git init orgchart-standalone
cd orgchart-standalone

# Copy standalone application
cp -r /path/to/anela.heblo/orgchart-standalone/* .

# Initialize git
git add .
git commit -m "Initial commit: Standalone OrgChart application"
```

### Step 2: Configure Data Source
Update `backend/src/appsettings.json`:
```json
{
  "OrgChart": {
    "DataSourceUrl": "https://your-actual-data-source.com/org-structure.json"
  }
}
```

### Step 3: Deploy to Azure
```bash
# Configure environment variables
export AZURE_RESOURCE_GROUP="your-rg"
export AZURE_APP_SERVICE_PLAN="your-plan"
export AZURE_APP_NAME="your-orgchart-app"

# Deploy
./scripts/deploy-azure.sh
```

### Step 4: Configure Authentication (Optional)
For authentication, update `appsettings.json`:
```json
{
  "Authentication": {
    "Enabled": true,
    "Authority": "https://login.microsoftonline.com/{tenant-id}",
    "Audience": "your-app-registration-client-id"
  }
}
```

## Data Source Requirements

Your JSON data source must follow this format:
```json
{
  "organization": {
    "name": "Company Name",
    "positions": [
      {
        "id": "unique-position-id",
        "title": "Position Title",
        "description": "Position description",
        "department": "Department Name",
        "parentPositionId": "parent-id-or-null",
        "level": 1,
        "url": "optional-position-url",
        "employees": [
          {
            "id": "unique-employee-id",
            "name": "Employee Name",
            "email": "employee@company.com",
            "startDate": "2021-01-01",
            "isPrimary": true,
            "url": "optional-employee-profile-url"
          }
        ]
      }
    ]
  }
}
```

## Cost Optimization

### Option 1: Use Existing Azure Plan (€0 additional)
- Deploy to your existing App Service Plan
- Share infrastructure with other applications
- Recommended for internal use

### Option 2: Static Site + Azure Functions (€5-20/month)
- Frontend as static site on Azure Storage
- Backend as Azure Functions
- Good for public-facing applications

### Option 3: Container Apps (€15-50/month)
- Serverless container hosting
- Auto-scale to zero
- Professional deployment option

## Customization

### Frontend Branding
Update `frontend/src/components/OrgChart.tsx`:
- Change colors in `getLevelColor()` function
- Modify header text and styling
- Customize department/level labels

### Backend Configuration
Update `backend/src/appsettings.json`:
- Data source URL
- Authentication settings
- CORS origins
- Logging levels

## Monitoring & Maintenance

### Health Checks
- Backend API: `https://your-app.azurewebsites.net/api/orgchart`
- Frontend: Application loads and displays data
- Data source: External JSON URL is accessible

### Updating Data
- Data updates happen automatically when external JSON changes
- Cache duration: 30 minutes (configurable in React Query)
- No database updates required

### Scaling
- Application is stateless
- Can handle multiple instances
- Auto-scaling supported in Azure App Service

## Security Considerations

### Public Deployment (No Authentication)
- Remove sensitive employee information
- Use generic titles and descriptions
- Consider data anonymization

### Private Deployment (With Authentication)
- Configure Azure AD integration
- Set appropriate user groups
- Enable HTTPS only

## Support & Maintenance

### Dependencies
- .NET 8: LTS until November 2026
- React 18: Stable, long-term support
- Azure services: Enterprise-grade reliability

### Updates
- Backend: Standard .NET updates
- Frontend: npm package updates
- Infrastructure: Azure platform updates

### Backup & Recovery
- Configuration: Store in Azure Key Vault
- Application: Git repository
- Data: External source (owner responsibility)

This standalone application is completely independent and ready for production deployment!