# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src/API

# Copy csproj first
COPY API/UrlShortner.csproj ./

# Restore dependencies
RUN dotnet restore UrlShortner.csproj

# Copy all source files
COPY API/ ./

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
