@echo off
setlocal enabledelayedexpansion

REM PickMeUp Startup Script
REM This script starts the required Docker services and runs the application

echo =========================================
echo   PickMeUp - Starting Application
echo =========================================
echo.

REM Check if Docker is installed
docker --version >nul 2>&1
if errorlevel 1 (
    echo [X] Error: Docker is not installed or not in PATH
    echo Please install Docker from https://docs.docker.com/get-docker/
    pause
    exit /b 1
)

REM Check if Docker Compose is available
docker-compose --version >nul 2>&1
if errorlevel 1 (
    docker compose version >nul 2>&1
    if errorlevel 1 (
        echo [X] Error: Docker Compose is not installed or not in PATH
        echo Please install Docker Compose
        pause
        exit /b 1
    )
    set DOCKER_COMPOSE_CMD=docker compose
) else (
    set DOCKER_COMPOSE_CMD=docker-compose
)

REM Navigate to docker-compose directory
set SCRIPT_DIR=%~dp0
set DOCKER_COMPOSE_DIR=%SCRIPT_DIR%src\PickMeUp.Web\.deploy

if not exist "%DOCKER_COMPOSE_DIR%" (
    echo [X] Error: Docker compose directory not found at %DOCKER_COMPOSE_DIR%
    pause
    exit /b 1
)

cd /d "%DOCKER_COMPOSE_DIR%"

echo [Package] Starting Docker services (PostgreSQL ^& Mailpit)...
echo.

REM Start Docker services
%DOCKER_COMPOSE_CMD% up -d

if errorlevel 1 (
    echo [X] Error: Failed to start Docker services
    pause
    exit /b 1
)

echo.
echo [OK] Docker services started successfully!
echo.
echo [Chart] Service Status:
echo    - PostgreSQL: http://localhost:5432
echo    - Mailpit Web UI: http://localhost:8025
echo    - Mailpit SMTP: localhost:1025
echo.

REM Navigate to Web project
cd /d "%SCRIPT_DIR%src\PickMeUp.Web"

echo [Rocket] Starting PickMeUp Web Application...
echo.

REM Check if dotnet is installed
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [X] Error: .NET SDK is not installed or not in PATH
    echo Please install .NET SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

REM Run the application
dotnet run

REM Note: The application will keep running. Press Ctrl+C to stop it.
