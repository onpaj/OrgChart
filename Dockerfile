# Multi-stage Dockerfile for standalone OrgChart application
FROM node:18-alpine AS frontend-build

# Build frontend
WORKDIR /app/frontend
COPY frontend/package*.json ./
RUN npm ci

COPY frontend/src ./src
COPY frontend/public ./public
COPY frontend/tsconfig.json ./

# Set production API URL (can be overridden via build arg)
ARG REACT_APP_API_URL=/api
ENV REACT_APP_API_URL=$REACT_APP_API_URL

RUN npm run build

# Build backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /app/backend

COPY backend/src/*.csproj ./
RUN dotnet restore

COPY backend/src/ ./
RUN dotnet publish -c Release -o out

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy backend
COPY --from=backend-build /app/backend/out .

# Copy frontend build to wwwroot
COPY --from=frontend-build /app/frontend/build ./wwwroot

# Create non-root user
RUN adduser --disabled-password --home /app --gecos '' appuser && chown -R appuser /app
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "OrgChart.API.dll"]