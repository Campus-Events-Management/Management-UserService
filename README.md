# EventManagement.UserService

This is the User Service microservice for the Event Management System, handling user authentication, registration, and profile management.

## Tech Stack
- ASP.NET Core 7.0 Web API
- Entity Framework Core with SQLite
- JWT Authentication
- Swagger/OpenAPI documentation

## Features
- User registration and login
- JWT token authentication
- User profile management
- Secure password hashing

## API Endpoints

### Authentication
- `POST /api/users/register` - Register a new user
- `POST /api/users/login` - Authenticate a user and receive a JWT token

### User Profile
- `GET /api/users/me` - Get the current user's profile (requires authentication)
- `PUT /api/users/me` - Update the current user's profile (requires authentication)

## Setup Instructions

### Prerequisites
- .NET 7.0 SDK or later
- Visual Studio 2022, VS Code, or other IDE with C# support

### Configuration
1. Clone the repository
2. Update the JWT key in `appsettings.json`:
   ```json
   "Jwt": {
     "Key": "REPLACE_WITH_YOUR_SECRET_KEY_AT_LEAST_32_CHARS",
     ...
   }
   ```
3. The database will be created automatically on first run (SQLite)

### Running the Application
```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

The API will be available at `http://localhost:5002` by default.

## Swagger Documentation
When running in development mode, Swagger UI is available at:
`http://localhost:5002/swagger`

## Security Notes
- Replace the placeholder JWT key with a strong secret key
- In production, consider using a more robust database system
- Set up proper HTTPS in production environments

## License
[MIT](LICENSE) 