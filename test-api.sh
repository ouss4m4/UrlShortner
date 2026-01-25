#!/bin/bash

echo "🚀 URL Shortener API Test Script"
echo "================================="
echo ""

API_URL="http://localhost:5011"

echo "Testing API endpoints..."
echo ""

# Test 1: Create a URL with auto-generated short code
echo "📝 Test 1: Create URL (auto-generated short code)"
RESPONSE=$(curl -s -X POST "${API_URL}/api/url" \
  -H "Content-Type: application/json" \
  -d '{
    "originalUrl": "https://github.com/dotnet/aspnetcore",
    "userId": 1
  }')
echo "Response: $RESPONSE"
SHORT_CODE=$(echo $RESPONSE | grep -o '"shortCode":"[^"]*' | cut -d'"' -f4)
echo "Generated short code: $SHORT_CODE"
echo ""

# Test 2: Get URL by short code (should hit cache on second call)
echo "📖 Test 2: Get URL by short code (first call - DB)"
curl -s "${API_URL}/api/url/short/${SHORT_CODE}" | jq '.'
echo ""

echo "📖 Test 3: Get URL by short code again (second call - should be from cache)"
curl -s "${API_URL}/api/url/short/${SHORT_CODE}" | jq '.'
echo ""

# Test 3: Create URL with custom short code
echo "📝 Test 4: Create URL with custom short code 'github'"
curl -s -X POST "${API_URL}/api/url" \
  -H "Content-Type: application/json" \
  -d '{
    "originalUrl": "https://github.com",
    "shortCode": "github",
    "userId": 1
  }' | jq '.'
echo ""

# Test 4: Try to create duplicate (should get 409)
echo "❌ Test 5: Try to create duplicate short code (should fail with 409)"
curl -s -w "\nHTTP Status: %{http_code}\n" -X POST "${API_URL}/api/url" \
  -H "Content-Type: application/json" \
  -d '{
    "originalUrl": "https://google.com",
    "shortCode": "github",
    "userId": 1
  }' | jq '.'
echo ""

# Test 5: Redirect test - NEW ROOT LEVEL ENDPOINT!
echo "🔗 Test 6: Test ROOT redirect endpoint (/{shortCode})"
echo "Accessing: ${API_URL}/${SHORT_CODE}"
echo "Following redirect with -L flag:"
curl -L -s -o /dev/null -w "HTTP Status: %{http_code}\nFinal URL: %{url_effective}\n" "${API_URL}/${SHORT_CODE}"
echo ""

# Test 6: Create multiple URLs to test base62 encoding
echo "📝 Test 7: Create multiple URLs to see different short codes"
for i in {1..5}; do
  RESPONSE=$(curl -s -X POST "${API_URL}/api/url" \
    -H "Content-Type: application/json" \
    -d "{
      \"originalUrl\": \"https://example.com/page${i}\",
      \"userId\": 1
    }")
  SHORT=$(echo $RESPONSE | grep -o '"shortCode":"[^"]*' | cut -d'"' -f4)
  echo "  URL ${i}: https://example.com/page${i} → ${SHORT}"
done
echo ""

echo "✅ Test script completed!"
echo ""
echo "💡 To test Redis caching manually:"
echo "   1. Call GET /api/url/short/${SHORT_CODE} multiple times"
echo "   2. First call queries DB (~10-20ms)"
echo "   3. Subsequent calls use Redis cache (~1-2ms)"
echo ""
echo "🔍 To check Redis cache:"
echo "   docker exec -it urlshortner_redis redis-cli"
echo "   Then run: KEYS url:shortcode:*"
echo "   And: GET url:shortcode:${SHORT_CODE}"
