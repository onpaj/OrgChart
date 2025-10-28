# OrgChart Standalone Application

Standalone organizational chart visualization application extracted from Anela Heblo project.

## Overview

This is a lightweight, standalone application for visualizing organizational structures. It consists of:
- **Frontend**: React-based interactive organizational chart with filtering, zoom, and hierarchical display
- **Backend**: Minimal ASP.NET Core API that fetches JSON data from configurable external source

## Architecture

### Frontend Features
- Interactive organizational chart with hierarchical tree layout
- Department and level filtering
- Zoom controls (50% - 200%)
- Employee details with clickable profiles
- Responsive design with connection lines between positions
- Real-time position/employee count statistics

### Backend Features
- Single API endpoint: `GET /api/orgchart`
- Configurable JSON data source via appsettings
- Optional authentication support
- CORS enabled for frontend integration
- Minimal dependencies (no database required)

## Data Format

The application expects JSON data in this format:
```json
{
  "organization": {
    "name": "Company Name",
    "positions": [
      {
        "id": "pos-1",
        "title": "CEO",
        "description": "Chief Executive Officer",
        "department": "Management",
        "parentPositionId": null,
        "level": 1,
        "url": "https://optional-position-description-url",
        "employees": [
          {
            "id": "emp-1",
            "name": "John Doe",
            "email": "john.doe@company.com",
            "startDate": "2020-01-01",
            "isPrimary": true,
            "url": "https://optional-employee-profile-url"
          }
        ]
      }
    ]
  }
}
```

## Configuration

### Backend Configuration (`appsettings.json`)
```json
{
  "OrgChart": {
    "DataSourceUrl": "https://your-domain.com/organization-data.json"
  },
  "Authentication": {
    "Enabled": false  // Set to true for authentication
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

### Frontend Configuration
- Environment variables for API endpoint
- Configurable polling intervals
- Customizable UI colors and branding

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- Azure account (for deployment)

### Local Development
```bash
# Backend
cd backend/src
dotnet run

# Frontend (separate terminal)
cd frontend
npm install
npm start
```

### Build & Deploy
```bash
# Build everything
./scripts/build.sh

# Deploy to Azure
./scripts/deploy.sh
```

## Deployment

### GitHub Actions CI/CD Pipeline

This project includes automated CI/CD pipeline that builds, tests, and deploys the application to Azure via DockerHub.

#### Required GitHub Secrets

To enable the deployment pipeline, configure these secrets in your GitHub repository (Settings > Secrets and variables > Actions):

##### DockerHub Configuration
- `DOCKERHUB_USERNAME` - Your DockerHub username
- `DOCKERHUB_TOKEN` - DockerHub access token (recommended over password)
  - Create at: https://hub.docker.com/settings/security

##### Azure Configuration
- `AZURE_WEBAPP_NAME` - Name of your Azure Web App
- `AZURE_CREDENTIALS` - Azure service principal credentials in JSON format

#### Setting up Azure Credentials

1. Create a service principal with contributor role:
```bash
az ad sp create-for-rbac \
  --name "orgchart-github-actions" \
  --role contributor \
  --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group-name} \
  --sdk-auth
```

2. Copy the entire JSON output and paste it into the `AZURE_CREDENTIALS` secret.

#### Pipeline Workflow

The CI/CD pipeline triggers on:
- Push to `main` or `develop` branches
- Pull requests to `main` branch

Pipeline stages:
1. **Build & Test** - Compiles and tests both frontend and backend
2. **Docker Build & Push** - Creates Docker image and pushes to DockerHub (main branch only)
3. **Azure Deployment** - Deploys the application to Azure Web App (main branch only)

#### Manual Deployment

For manual deployment without GitHub Actions:

```bash
# Build Docker image
docker build -t your-username/orgchart:latest .

# Push to DockerHub
docker push your-username/orgchart:latest

# Deploy to Azure
az webapp config container set \
  --name your-webapp-name \
  --resource-group your-resource-group \
  --docker-custom-image-name your-username/orgchart:latest
```

## License

MIT License - See LICENSE file for details.