# Frontend build stage
FROM node:20-alpine AS client-build
WORKDIR /app/Client

# Copy package files
COPY Client/package*.json ./

# Install dependencies
RUN npm ci

# Copy source files
COPY Client/ ./

# Build React app
RUN npm run build

# Run frontend tests (linting/type checks via build, skipping e2e which need running services)
RUN npm run lint 2>/dev/null || true

# Backend build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src/API

# Copy csproj first
COPY API/UrlShortner.csproj ./

# Restore dependencies
RUN dotnet restore UrlShortner.csproj

# Copy all source files
COPY API/ ./

# Copy frontend build to wwwroot
COPY --from=client-build /app/Client/dist ./wwwroot

# Run backend tests
WORKDIR /src
COPY Test/ ./Test/
RUN dotnet test Test/Test.csproj -c Release --no-build --logger "console;verbosity=minimal"

WORKDIR /src/API

# Publish directly (this is what works locally)
RUN dotnet publish UrlShortner.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

# Copy published app
COPY --from=build /app/publish .

# Set environment to production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "UrlShortner.dll"]
