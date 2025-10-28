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

## Deployment Options

### Option 1: Azure App Service (Recommended)
- Deploy to existing Azure App Service Plan (€0 additional cost)
- Single container with both frontend and backend
- Subdomain or path-based routing

### Option 2: Static Site + Azure Functions
- Frontend as static site on Azure Storage Account
- Backend as Azure Functions for API
- Cost: €1-20/month

### Option 3: Container Apps
- Serverless container hosting
- Auto-scale to zero
- Cost: €15-50/month

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

## Future Migration

This standalone application is designed for easy extraction to a separate repository:
- Self-contained dependencies
- Minimal external references
- Documented configuration
- Standard deployment patterns
- No shared code with parent project

## License

MIT License - See LICENSE file for details.