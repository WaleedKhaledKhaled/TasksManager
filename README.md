# Tasks Manager API

This project is a simple yet complete task management REST API that uses ASP.NET Core 9, Entity Framework Core, and SQL Server. It includes JWT based login, advanced task filtering, cached reporting, health checks, Docker support, and unit tests. The goal of this README is to explain how to run the app without overwhelming detail.

## What you need

- [.NET 9 SDK (preview)](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or Docker
- [Docker](https://www.docker.com/products/docker-desktop/) if you prefer containers

The repository includes a `global.json` file that locks the SDK version so you get the same tooling used during development.

## Getting started quickly

Clone the project and open the folder:

```bash
git clone https://github.com/WaleedKhaledKhaled/TasksManager.git
cd TasksManager
```

Restore packages and run the API from the command line:

```bash
cd src/TasksManager.Api
dotnet restore
dotnet run
```

The API listens on `http://localhost:5045` and `https://localhost:7045`. Swagger UI is available at `/swagger` so you can explore every endpoint.

## Configure your environment

The app reads its settings from environment variables. Create a `.env` file or export the values below. Defaults are included in `docker-compose.yml`.

| Variable | Purpose | Default value |
| --- | --- | --- |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | `Server=localhost,1433;Database=TasksManager;User Id=sa;Password=Your_password123;TrustServerCertificate=True;` |
| `Jwt__Issuer` | JWT token issuer | `TasksManager` |
| `Jwt__Audience` | JWT audience | `TasksManagerClient` |
| `Jwt__Secret` | Symmetric signing key (use 32+ characters) | `SuperSecretDevelopmentKeyChangeMe1234567890` |
| `Jwt__TokenLifetimeMinutes` | Access token lifetime | `60` |

The database schema is created automatically on startup through `Database.Migrate()`. You can still add migrations manually with `dotnet ef` if you want full control.

## Run with Docker

Use Docker Compose to launch both the API and SQL Server with a single command:

```bash
docker compose up --build
```

The API becomes available at `http://localhost:8080`. Data files live in a Docker volume so your tasks persist between restarts.

## Run the tests

```bash
cd tests/TasksManager.Api.Tests
dotnet test --collect:"XPlat Code Coverage"
```

The suite covers task CRUD logic, user registration validation, and login flows. Tests follow the Arrange Act Assert style and run independently.

## API at a glance

Authentication endpoints:

- `POST /api/auth/register`
- `POST /api/auth/login`

Task endpoints:

- `GET /api/tasks`
- `GET /api/tasks/{id}`
- `POST /api/tasks`
- `PUT /api/tasks/{id}`
- `DELETE /api/tasks/{id}`
- `GET /api/tasks/filter`

Reporting and health:

- `GET /api/reports/summary`
- `GET /api/health`

The filter endpoint supports status lists, priority lists, date ranges, keyword search, custom sorting, and pagination. The reporting endpoint caches per user for five minutes so repeated requests stay fast.

Every response follows the same friendly envelope so clients always know what to expect:

```json
{
  "status": "success",
  "result": { "...": "payload" },
  "errors": []
}
```

List endpoints place their data inside a `result` object with `items` and `totalCount` (the filter endpoint also includes page details). When something goes wrong the `errors` array explains why while `status` flips to `error`.

## Deployment notes

- The `Dockerfile` uses a multi stage build to keep the final image small.
- `docker-compose.yml` wires up the API, database, environment variables, and persistent storage.
- Serilog logging is enabled so you can follow activity in the console or forward logs to your platform of choice.
- Swagger documentation is part of the default build so you always have interactive API docs.

## Contributing

Issues and pull requests are welcome. Please open a branch, make your changes with tests when possible, and describe the updates in plain language when you submit a PR.
