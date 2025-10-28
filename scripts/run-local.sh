#!/bin/bash
set -e

echo "🚀 Starting OrgChart Application Locally"

# Function to handle cleanup
cleanup() {
    echo "🛑 Stopping services..."
    if [ ! -z "$BACKEND_PID" ]; then
        kill $BACKEND_PID 2>/dev/null || true
    fi
    if [ ! -z "$FRONTEND_PID" ]; then
        kill $FRONTEND_PID 2>/dev/null || true
    fi
}

# Set up cleanup on script exit
trap cleanup EXIT

# Start backend
echo "Starting backend on http://localhost:5000..."
cd backend/src
dotnet run &
BACKEND_PID=$!
cd ../..

# Wait for backend to start
echo "Waiting for backend to start..."
sleep 5

# Start frontend
echo "Starting frontend on http://localhost:3000..."
cd frontend
npm start &
FRONTEND_PID=$!
cd ..

echo ""
echo "✅ Application started!"
echo "- Frontend: http://localhost:3000"
echo "- Backend API: http://localhost:5000/api/orgchart"
echo "- Swagger UI: http://localhost:5000/swagger"
echo ""
echo "Press Ctrl+C to stop both services"

# Wait for processes
wait