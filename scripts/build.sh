#!/bin/bash
set -e

echo "🏗️  Building OrgChart Standalone Application"

# Build backend
echo "Building backend..."
cd backend/src
dotnet restore
dotnet build -c Release
cd ../..

# Build frontend
echo "Building frontend..."
cd frontend
npm install
npm run build
cd ..

echo "✅ Build completed successfully!"
echo ""
echo "Next steps:"
echo "- Run locally: ./scripts/run-local.sh"
echo "- Build Docker: docker build -t orgchart-standalone ."
echo "- Deploy to Azure: ./scripts/deploy-azure.sh"