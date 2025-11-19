# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

# Build and publish stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file and restore dependencies
COPY "XProject.Integrations.CRM.csproj" .
RUN dotnet restore "./XProject.Integrations.CRM.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "./XProject.Integrations.CRM.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
RUN dotnet publish "./XProject.Integrations.CRM.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish . 
ENTRYPOINT ["dotnet", "XProject.Integrations.CRM.dll"]
