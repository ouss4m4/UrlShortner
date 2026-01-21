# Project Structure Requirements

For this Bitly-style .NET API, use a layered structure:

API/
Controllers/
UserController.cs
UrlController.cs
...
Models/
User.cs
Url.cs
Visit.cs
Analytics.cs
Services/
IUserService.cs
UserService.cs
IUrlService.cs
UrlService.cs
...
Data/
UrlShortnerDbContext.cs
...
Program.cs
appsettings.json
UrlShortner.csproj
...

- Models: All EF Core entities (one per file)
- Controllers: All API endpoints (one per resource)
- Services: Business logic, interfaces, and implementations
- Data: DbContext and data access logic

This structure is required for maintainability, testability, and scalability.
