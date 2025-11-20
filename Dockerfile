# Use the official ASP.NET Core runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project file and restore dependencies
COPY ["CopilotApiProject.csproj", "."]
RUN dotnet restore "CopilotApiProject.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/."
RUN dotnet build "CopilotApiProject.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CopilotApiProject.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage: copy published app to runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user for security
RUN addgroup --system --gid 1001 dotnetuser \
    && adduser --system --uid 1001 --ingroup dotnetuser dotnetuser

# Change ownership of the app directory
RUN chown -R dotnetuser:dotnetuser /app
USER dotnetuser

ENTRYPOINT ["dotnet", "CopilotApiProject.dll"]