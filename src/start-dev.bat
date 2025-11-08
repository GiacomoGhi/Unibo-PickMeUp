@echo off
echo.
echo 🚀 Starting PickMeUp Development Environment
echo.

REM Check if Docker is running
docker info >nul 2>&1
if errorlevel 1 (
    echo ❌ Docker is not running. Please start Docker Desktop first.
    pause
    exit /b 1
)

echo ✓ Docker is running
echo.

REM Navigate to Deploy directory
cd /d "%~dp0PickMeUp.Web\Deploy"

echo 📦 Starting Docker services (PostgreSQL + Mailpit)...
docker-compose up -d

REM Wait for services to be ready
echo.
echo ⏳ Waiting for services to be ready...
timeout /t 5 /nobreak >nul

REM Check if services are running
docker-compose ps | findstr "Up" >nul
if errorlevel 1 (
    echo ❌ Failed to start services
    echo Check Docker logs: docker-compose logs
    pause
    exit /b 1
)

echo ✓ Docker services are running
echo.
echo 📊 Service Status:
docker-compose ps
echo.
echo 🌐 Access URLs:
echo    - Application:  http://localhost:5000
echo    - Mailpit UI:   http://localhost:8025
echo    - PostgreSQL:   localhost:5432
echo.
echo 📧 All emails will be captured by Mailpit
echo    Open http://localhost:8025 to view them
echo.
echo 🎯 Test Users (pre-seeded):
echo    Email: john.doe@example.com ^| Password: Password123!
echo    Email: jane.smith@example.com ^| Password: Password123!
echo.
echo To run the application:
echo    cd PickMeUp.Web
echo    dotnet run
echo.
echo To stop services:
echo    cd PickMeUp.Web\Deploy
echo    docker-compose down
echo.
pause
