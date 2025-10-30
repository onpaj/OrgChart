# Azure Web App Configuration

This document describes the environment variables required for the OrgChart application when deployed to Azure Web App.

## Required Environment Variables

Configure these Application Settings in your Azure Web App:

### Azure AD Authentication (Backend)

| Variable | Description | Example              |
|----------|-------------|----------------------|
| `AzureAd__TenantId` | Azure AD Tenant ID | `xxxxxx`             |
| `AzureAd__ClientId` | Backend API Application Client ID | `xxxxxxxx`           |
| `AzureAd__ClientSecret` | Backend API Application Client Secret | `your-client-secret` |
| `AzureAd__Audience` | Backend API Audience (same as Client ID) | `api://xxxxxxxxx`    |

### Frontend Configuration

| Variable | Description | Example     |
|----------|-------------|-------------|
| `Frontend__ClientId` | Frontend SPA Application Client ID | `xxxxxxxxx` |

### Optional Configuration

| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `UseMockAuth` | Enable mock authentication for testing | `false` | `true` |
| `OrgChart__DataSourceType` | Data source type | `AzureStorage` | `Url` or `AzureStorage` |

### Data Source Configuration (Azure Storage)

| Variable | Description | Example |
|----------|-------------|---------|
| `OrgChart__AzureStorage__ConnectionString` | Azure Storage connection string | `DefaultEndpointsProtocol=https;AccountName=...` |
| `OrgChart__AzureStorage__ContainerName` | Storage container name | `documents` |
| `OrgChart__AzureStorage__BlobName` | JSON file name | `organization-structure.json` |
| `OrgChart__AzureStorage__UseManagedIdentity` | Use managed identity | `false` |

### Data Source Configuration (URL)

| Variable | Description | Example |
|----------|-------------|---------|
| `OrgChart__UrlStorage__Url` | URL to organization JSON data | `https://example.com/org-data.json` |
| `OrgChart__UrlStorage__TimeoutSeconds` | Request timeout | `30` |

## Azure AD Application Registration

### Backend API Application

1. Register a new application in Azure AD
2. Set **Application ID URI** to `api://{client-id}`
3. Configure **API Permissions**:
   - Add `User.Read` (Microsoft Graph)
   - Expose an API scope: `access_as_user`
4. Generate a client secret
5. Use the Client ID as `AzureAd__ClientId` and `AzureAd__Audience`

### Frontend SPA Application

1. Register a new application in Azure AD
2. Set **Platform** to "Single-page application"
3. Add **Redirect URIs**:
   - `https://your-app.azurewebsites.net` (production)
   - `http://localhost:3000` (development)
   - `http://localhost:3001` (development)
4. Configure **API Permissions**:
   - Add the backend API scope: `api://{backend-client-id}/access_as_user`
   - Add `User.Read` (Microsoft Graph)
5. Use the Client ID as `Frontend__ClientId`

## Security Notes

- **Never commit secrets** to the repository
- Use Azure Key Vault for production secrets
- Backend Client Secret is sensitive - store securely
- Frontend Client ID is public - safe to expose
- All other configuration values are non-sensitive

## Configuration Flow

1. **Build time**: Docker image is built without MSAL credentials
2. **Runtime**: Backend reads configuration from environment variables
3. **Frontend startup**: Calls `/api/config` to get MSAL configuration
4. **Authentication**: Frontend uses dynamic config to initialize MSAL

This approach allows the same Docker image to work in all environments while keeping credentials secure.

## Debugging

To verify configuration is working:

1. Check Azure Web App logs for backend startup messages
2. Open browser console and check for configuration fetch logs
3. Verify MSAL configuration in browser console
4. Check that tokens are being acquired successfully

### Common Issues

- **Empty tokens**: Usually means frontend config is not loaded properly
- **401 errors**: Check that redirect URLs are configured in Azure AD
- **CORS errors**: Verify CORS configuration includes your domain
- **Configuration not found**: Check that Application Settings are configured correctly