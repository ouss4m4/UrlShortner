#!/bin/bash

echo "🚀 Starting URL Shortener API Server..."
echo "======================================="
echo ""

# Check if Docker services are running
echo "📦 Checking Docker services..."
if ! docker ps | grep -q urlshortner_postgres; then
    echo "❌ PostgreSQL is not running!"
    echo "   Please run: docker-compose up -d"
    exit 1
fi

if ! docker ps | grep -q urlshortner_redis; then
    echo "❌ Redis is not running!"
    echo "   Please run: docker-compose up -d"
    exit 1
fi

echo "✅ PostgreSQL is running"
echo "✅ Redis is running"
echo ""

# Navigate to API directory
cd "$(dirname "$0")/API"

echo "🔨 Building API..."
dotnet build UrlShortner.csproj --nologo --verbosity quiet

if [ $? -ne 0 ]; then
    echo "❌ Build failed!"
    exit 1
fi

echo "✅ Build successful"
echo ""
echo "🌐 Starting server on http://localhost:5000"
echo ""
echo "📝 Available endpoints:"
echo "   POST   /api/url              - Create URL"
echo "   GET    /api/url/{id}         - Get URL by ID"
echo "   GET    /api/url/short/{code} - Get URL by short code (cached)"
echo "   GET    /api/url/redirect/{code} - Redirect to original URL"
echo ""
echo "🧪 To test the API, open a new terminal and run:"
echo "   ./test-api.sh"
echo ""
echo "⏹️  Press Ctrl+C to stop the server"
echo ""
echo "======================================="
echo ""

# Run the server
dotnet run --no-build
