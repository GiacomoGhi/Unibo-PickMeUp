#!/bin/bash

echo "🚀 Starting PickMeUp Development Environment"
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker Desktop first."
    exit 1
fi

echo "✓ Docker is running"
echo ""

# Navigate to Deploy directory
cd "$(dirname "$0")/PickMeUp.Web/Deploy"

echo "📦 Starting Docker services (PostgreSQL + Mailpit)..."
docker-compose up -d

# Wait for services to be ready
echo ""
echo "⏳ Waiting for services to be ready..."
sleep 5

# Check if services are running
if docker-compose ps | grep -q "Up"; then
    echo "✓ Docker services are running"
    echo ""
    echo "📊 Service Status:"
    docker-compose ps
    echo ""
    echo "🌐 Access URLs:"
    echo "   - Application:  http://localhost:5000"
    echo "   - Mailpit UI:   http://localhost:8025"
    echo "   - PostgreSQL:   localhost:5432"
    echo ""
    echo "📧 All emails will be captured by Mailpit"
    echo "   Open http://localhost:8025 to view them"
    echo ""
    echo "🎯 Test Users (pre-seeded):"
    echo "   Email: john.doe@example.com | Password: Password123!"
    echo "   Email: jane.smith@example.com | Password: Password123!"
    echo ""
    echo "To run the application:"
    echo "   cd PickMeUp.Web"
    echo "   dotnet run"
    echo ""
    echo "To stop services:"
    echo "   cd PickMeUp.Web/Deploy"
    echo "   docker-compose down"
else
    echo "❌ Failed to start services"
    echo "Check Docker logs:"
    echo "   docker-compose logs"
    exit 1
fi
