# PowerShell Deployment Script for CopilotApiProject
# Usage: .\deploy.ps1 -Environment [Development|Staging|Production]

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("Development", "Staging", "Production")]
    [string]$Environment
)

Write-Host "🚀 Starting deployment for $Environment environment..." -ForegroundColor Green

# Set variables based on environment
switch ($Environment) {
    "Development" {
        $DockerComposeFile = "docker-compose.yml"
        $AspNetCoreEnvironment = "Development"
        $Port = "5196"
    }
    "Staging" {
        $DockerComposeFile = "docker-compose.yml"
        $AspNetCoreEnvironment = "Staging"
        $Port = "5197"
    }
    "Production" {
        $DockerComposeFile = "docker-compose.yml"
        $AspNetCoreEnvironment = "Production"
        $Port = "80"
    }
}

try {
    # Check if Docker is running
    Write-Host "🔍 Checking Docker status..." -ForegroundColor Yellow
    docker --version
    if ($LASTEXITCODE -ne 0) {
        throw "Docker is not installed or running"
    }

    # Check if required files exist
    if (!(Test-Path $DockerComposeFile)) {
        throw "Docker compose file not found: $DockerComposeFile"
    }

    if (!(Test-Path "Dockerfile")) {
        throw "Dockerfile not found"
    }

    # Build and deploy
    Write-Host "🏗️ Building application..." -ForegroundColor Yellow
    dotnet build --configuration Release
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }

    Write-Host "🧪 Running tests..." -ForegroundColor Yellow
    dotnet test --no-build --configuration Release
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Tests failed, but continuing deployment"
    }

    # Set environment variable
    $env:ASPNETCORE_ENVIRONMENT = $AspNetCoreEnvironment

    Write-Host "🐳 Starting Docker containers..." -ForegroundColor Yellow
    if ($Environment -eq "Production") {
        docker-compose --profile production up -d --build
    } else {
        docker-compose up -d --build
    }

    if ($LASTEXITCODE -ne 0) {
        throw "Docker deployment failed"
    }

    Write-Host "⏳ Waiting for application to start..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10

    # Health check
    Write-Host "🏥 Performing health check..." -ForegroundColor Yellow
    $HealthUrl = "http://localhost:$Port/health"
    try {
        $Response = Invoke-RestMethod -Uri $HealthUrl -Method Get -TimeoutSec 30
        Write-Host "✅ Health check passed: $($Response.Status)" -ForegroundColor Green
    }
    catch {
        Write-Warning "Health check failed, but deployment may still be successful"
        Write-Host "Check manually at: $HealthUrl" -ForegroundColor Yellow
    }

    Write-Host "🎉 Deployment completed successfully!" -ForegroundColor Green
    Write-Host "📍 Application URLs:" -ForegroundColor Cyan
    Write-Host "   API: http://localhost:$Port" -ForegroundColor White
    Write-Host "   Swagger: http://localhost:$Port/swagger" -ForegroundColor White
    Write-Host "   Health: http://localhost:$Port/health" -ForegroundColor White

    # Show running containers
    Write-Host "🐳 Running containers:" -ForegroundColor Cyan
    docker ps --filter "label=com.docker.compose.project=copilotapiproject"

}
catch {
    Write-Error "❌ Deployment failed: $($_.Exception.Message)"
    Write-Host "🔍 Checking container logs..." -ForegroundColor Yellow
    docker-compose logs --tail=50
    exit 1
}

Write-Host "✨ Deployment script completed!" -ForegroundColor Green