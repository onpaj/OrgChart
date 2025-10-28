# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

OrgChart is a React frontend + .NET minimal API application for organizational chart visualization.

## Architecture

- **Frontend**: React with TypeScript (port 3001)
  - Located in `/` (root directory)
  - Uses React Query for data fetching
  - Tailwind CSS for styling
- **Backend**: ASP.NET Core Minimal API (port 5001)
  - Located in `/backend/src/`
  - Provides organizational chart data via REST API

## Development Commands

### Frontend
```bash
npm install          # Install dependencies
npm start           # Start dev server on port 3001
npm run build       # Build for production
npm test            # Run tests
```

### Backend
```bash
cd backend/src
dotnet restore      # Restore dependencies
dotnet run          # Start API on port 5001
dotnet build        # Build project
```

## JetBrains Rider Setup

The project includes pre-configured run configurations:

1. **Backend API** - Runs the .NET API on port 5001
2. **Frontend React** - Runs React dev server on port 3001
3. **Full Application** - Compound configuration that starts both frontend and backend

To run:
1. Open `OrgChart.sln` in JetBrains Rider
2. Select run configuration from dropdown
3. Click Run button

## Project Structure

```
/
├── src/                    # React frontend source
│   ├── components/         # React components
│   ├── services/          # API services
│   └── types/             # TypeScript types
├── backend/src/           # .NET API source
│   ├── Controllers/       # API controllers
│   ├── Models/           # Data models
│   └── Services/         # Business logic
├── public/               # Static files
└── OrgChart.sln         # Solution file
```

## API Endpoints

- `GET /api/orgchart` - Returns organizational chart data