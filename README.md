# TechHive User Management API

A comprehensive RESTful API built with ASP.NET Core 9.0 for managing user records in TechHive Solutions internal tools. This API provides full CRUD operations for user management with authentication, validation, caching, and comprehensive logging.

## 🚀 Features

- **User Management**: Complete CRUD operations for user records
- **Authentication**: JWT token-based authentication and authorization
- **Validation**: Comprehensive data validation with custom attributes
- **Caching**: In-memory caching for improved performance
- **Logging**: Request/response logging and error handling middleware
- **Documentation**: Swagger/OpenAPI documentation
- **Containerization**: Docker support for easy deployment
- **CI/CD**: GitHub Actions workflow for automated deployment

## 🛠️ Tech Stack

- **Framework**: ASP.NET Core 9.0
- **Language**: C# 12
- **Authentication**: JWT (JSON Web Tokens)
- **Validation**: Data Annotations + Custom Validators
- **Caching**: In-Memory Caching
- **Documentation**: Swagger/OpenAPI
- **Containerization**: Docker
- **CI/CD**: GitHub Actions

## 📋 Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/products/docker-desktop) (for containerized deployment)
- [Git](https://git-scm.com/)

## 🏃‍♂️ Quick Start

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/ismailandao/CourseraCopilotAPI.git
   cd CourseraCopilotAPI
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the application**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the API**
   - API Base URL: `http://localhost:5196`
   - Swagger Documentation: `http://localhost:5196/swagger`
   - Health Check: `http://localhost:5196/health`

### Docker Deployment

1. **Build and run with Docker Compose**
   ```bash
   docker-compose up --build
   ```

2. **Access the application**
   - API: `http://localhost:5196`
   - HTTPS: `https://localhost:7196`

### Production Deployment

1. **Set up environment variables**
   ```bash
   cp .env.example .env
   # Edit .env file with your production settings
   ```

2. **Build production image**
   ```bash
   docker build -t copilot-api:latest .
   ```

3. **Deploy with Docker Compose (Production)**
   ```bash
   docker-compose --profile production up -d
   ```

## 🔧 Configuration

### Environment Variables

Key environment variables for production deployment:

```env
ASPNETCORE_ENVIRONMENT=Production
JWT_SECRET_KEY=your-super-secret-jwt-key-here
JWT_ISSUER=TechHive.Production.API
JWT_AUDIENCE=TechHive.Production.Client
CONNECTION_STRING=your-database-connection-string
ALLOWED_ORIGINS=https://yourdomain.com
```

See `.env.example` for a complete list of configuration options.

### Application Settings

- `appsettings.Development.json` - Development configuration
- `appsettings.Production.json` - Production configuration
- `appsettings.json` - Base configuration

## 📚 API Documentation

### Available Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/users` | Get all users | ✅ |
| GET | `/api/users/{id}` | Get user by ID | ✅ |
| GET | `/api/users/email/{email}` | Get user by email | ✅ |
| GET | `/api/users/department/{dept}` | Get users by department | ✅ |
| GET | `/api/users/search?searchTerm={term}` | Search users | ✅ |
| POST | `/api/users` | Create new user | ✅ |
| PUT | `/api/users/{id}` | Update user | ✅ |
| DELETE | `/api/users/{id}` | Delete user | ✅ |
| GET | `/health` | Health check | ❌ |
| GET | `/api` | API information | ❌ |

### Authentication

The API uses JWT Bearer tokens for authentication:

```bash
Authorization: Bearer <your-jwt-token>
```

### Sample API Calls

See `test-requests.http` file for comprehensive API testing examples.

## 🔐 Security Features

- JWT token authentication
- Input validation and sanitization
- SQL injection prevention
- Rate limiting (in production with Nginx)
- HTTPS enforcement
- Security headers
- CORS configuration

## 🐳 Docker Support

### Development
```bash
docker-compose up
```

### Production
```bash
docker-compose --profile production up -d
```

### Manual Docker Commands
```bash
# Build image
docker build -t copilot-api:latest .

# Run container
docker run -p 8080:8080 -p 8081:8081 copilot-api:latest
```

## 🔄 CI/CD Pipeline

The project includes a comprehensive GitHub Actions workflow that:

1. **Builds** the application
2. **Runs** tests
3. **Performs** security analysis
4. **Publishes** artifacts
5. **Deploys** to staging/production environments

### Deployment Environments

- **Staging**: Triggered on pushes to `develop` branch
- **Production**: Triggered on pushes to `main` branch

## 🧪 Testing

### Run Tests
```bash
dotnet test
```

### Test Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### API Testing
Use the provided `test-requests.http` file with REST Client extension in VS Code.

## 📊 Monitoring & Health Checks

### Health Check Endpoint
```
GET /health
```

Response:
```json
{
  "status": "Healthy",
  "service": "TechHive User Management API",
  "timestamp": "2025-11-20T10:30:00.000Z"
}
```

### Logging

The application includes comprehensive logging:
- Request/response logging
- Error handling and logging
- Performance monitoring
- Security event logging

## 🚀 Deployment Options

### 1. GitHub Actions (Recommended)
- Automatic deployment on push to main/develop
- Includes testing and security scanning
- Supports multiple environments

### 2. Docker Deployment
- Containerized application
- Nginx reverse proxy
- SSL/TLS termination
- Health checks

### 3. Azure App Service
- Direct deployment to Azure
- Managed service
- Auto-scaling capabilities

### 4. AWS ECS/Fargate
- Container-based deployment
- Serverless compute
- Load balancing

## 🛡️ Security Best Practices

- Always use HTTPS in production
- Rotate JWT secrets regularly
- Implement proper CORS policies
- Use rate limiting
- Enable security headers
- Regular security updates
- Environment variable management

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For support and questions:

- Create an issue in the [GitHub repository](https://github.com/ismailandao/CourseraCopilotAPI/issues)
- Contact: [your-email@domain.com]

## 📝 Changelog

### Version 1.0.0
- Initial release
- User management CRUD operations
- JWT authentication
- Docker support
- CI/CD pipeline
- Comprehensive documentation

---

**Built with ❤️ by TechHive Solutions**