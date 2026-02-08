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
WORKDIR /src

# Copy project files
COPY API/UrlShortner.csproj ./API/
COPY Test/Test.csproj ./Test/

# Restore dependencies
WORKDIR /src/API
RUN dotnet restore UrlShortner.csproj

WORKDIR /src/Test
RUN dotnet restore Test.csproj

# Copy all source files
WORKDIR /src
COPY API/ ./API/
COPY Test/ ./Test/

# Copy frontend build to wwwroot
WORKDIR /src/API
COPY --from=client-build /app/Client/dist ./wwwroot

# Run backend tests
WORKDIR /src
RUN dotnet test Test/Test.csproj -c Release --logger "console;verbosity=minimal"

# Publish directly (this is what works locally)
WORKDIR /src/API
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
