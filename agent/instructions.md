# Instructions

## Project Setup

- .NET minimal API project
- Use RESTful endpoints
- Use a relational database (e.g., PostgreSQL, SQL Server)
- Integrate Redis for caching shortcodes

## Development Guidelines

- Follow standard .NET coding conventions
- Use minimal, clear, and maintainable code
- Document endpoints and core logic
- Use migration tools for DB schema

## API Endpoints

- CRUD for User (registration optional, allow anonymous)
- CRUD for URL (shorten, expand, delete, list)
- Analytics endpoints (for registered users and admin)
- Admin endpoints (CRUD on all, analytics)

## Security & Rate Limiting

- Implement basic rate limiting
- Secure admin endpoints

## Testing

- Write unit and integration tests for core logic

## Deployment

- Use Docker for containerization
- Prepare for scaling (Redis, DB)

## Next Steps

- Refer to prd.md for requirements
- Use design.png for UI inspiration (if building frontend later)
