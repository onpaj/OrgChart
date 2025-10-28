#!/bin/bash
set -e

# Azure deployment script for OrgChart Standalone Application
# This script deploys to your existing Azure App Service Plan

# Configuration
RESOURCE_GROUP="${AZURE_RESOURCE_GROUP:-anela-heblo-rg}"
APP_SERVICE_PLAN="${AZURE_APP_SERVICE_PLAN:-anela-heblo-plan}"
APP_NAME="${AZURE_APP_NAME:-anela-orgchart}"
DOCKER_IMAGE="${DOCKER_IMAGE:-orgchart-standalone:latest}"

echo "🚀 Deploying OrgChart to Azure App Service"
echo "Resource Group: $RESOURCE_GROUP"
echo "App Service Plan: $APP_SERVICE_PLAN"
echo "App Name: $APP_NAME"
echo ""

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo "❌ Azure CLI is not installed. Please install it first."
    exit 1
fi

# Login check
if ! az account show &> /dev/null; then
    echo "🔑 Please login to Azure CLI"
    az login
fi

# Build Docker image
echo "📦 Building Docker image..."
docker build -t $DOCKER_IMAGE .

# Create container registry if it doesn't exist
ACR_NAME="${AZURE_ACR_NAME:-anelaorgchart}"
echo "🏗️  Checking/creating Azure Container Registry..."
az acr create --resource-group $RESOURCE_GROUP --name $ACR_NAME --sku Basic --location westeurope || echo "ACR already exists"

# Login to ACR
echo "🔑 Logging in to Azure Container Registry..."
az acr login --name $ACR_NAME

# Tag and push image
ACR_IMAGE="$ACR_NAME.azurecr.io/orgchart:latest"
echo "📤 Pushing image to ACR..."
docker tag $DOCKER_IMAGE $ACR_IMAGE
docker push $ACR_IMAGE

# Create App Service if it doesn't exist
echo "🏗️  Checking/creating App Service..."
az webapp create \
    --resource-group $RESOURCE_GROUP \
    --plan $APP_SERVICE_PLAN \
    --name $APP_NAME \
    --deployment-container-image-name $ACR_IMAGE || echo "App Service already exists"

# Configure App Service for container
echo "⚙️  Configuring App Service..."
az webapp config container set \
    --resource-group $RESOURCE_GROUP \
    --name $APP_NAME \
    --docker-custom-image-name $ACR_IMAGE \
    --docker-registry-server-url https://$ACR_NAME.azurecr.io

# Configure App Settings
echo "📝 Configuring application settings..."
az webapp config appsettings set \
    --resource-group $RESOURCE_GROUP \
    --name $APP_NAME \
    --settings \
        "OrgChart__DataSourceUrl=https://your-data-source.com/organization.json" \
        "Authentication__Enabled=false" \
        "WEBSITES_PORT=8080"

echo ""
echo "✅ Deployment completed!"
echo "🌐 Your application is available at: https://$APP_NAME.azurewebsites.net"
echo ""
echo "Next steps:"
echo "1. Update OrgChart__DataSourceUrl in Azure App Settings to point to your actual data source"
echo "2. Configure custom domain if needed: https://$APP_NAME.azurewebsites.net"
echo "3. Enable authentication if required by setting Authentication__Enabled=true"