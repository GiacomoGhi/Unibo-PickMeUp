#!/bin/bash

# PickMeUp Startup Script
# This script starts the required Docker services and runs the application

echo "========================================="
echo "  PickMeUp - Starting Application"
echo "========================================="
echo ""

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Error: Docker is not installed or not in PATH"
    echo "Please install Docker from https://docs.docker.com/get-docker/"
    exit 1
fi

# Check if Docker Compose is available
if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null 2>&1; then
    echo "❌ Error: Docker Compose is not installed or not in PATH"
    echo "Please install Docker Compose"
    exit 1
fi

# Navigate to docker-compose directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DOCKER_COMPOSE_DIR="$SCRIPT_DIR/src/PickMeUp.Web/.deploy"

if [ ! -d "$DOCKER_COMPOSE_DIR" ]; then
    echo "❌ Error: Docker compose directory not found at $DOCKER_COMPOSE_DIR"
    exit 1
fi

cd "$DOCKER_COMPOSE_DIR"

echo "📦 Starting Docker services (PostgreSQL & Mailpit)..."
echo ""

# Use docker compose or docker-compose depending on what's available
if docker compose version &> /dev/null 2>&1; then
    DOCKER_COMPOSE_CMD="docker compose"
else
    DOCKER_COMPOSE_CMD="docker-compose"
fi

# Start Docker services
$DOCKER_COMPOSE_CMD up -d

if [ $? -ne 0 ]; then
    echo "❌ Error: Failed to start Docker services"
    exit 1
fi

echo ""
echo "✅ Docker services started successfully!"
echo ""
echo "📊 Service Status:"
echo "   - PostgreSQL: http://localhost:5432"
echo "   - Mailpit Web UI: http://localhost:8025"
echo "   - Mailpit SMTP: localhost:1025"
echo ""

# Navigate back to project root and then to Web project
cd "$SCRIPT_DIR/src/PickMeUp.Web"

echo "🚀 Starting PickMeUp Web Application..."
echo ""

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ Error: .NET SDK is not installed or not in PATH"
    echo "Please install .NET SDK from https://dotnet.microsoft.com/download"
    exit 1
fi

# Run the application
dotnet run

# Note: The application will keep running. Press Ctrl+C to stop it.
