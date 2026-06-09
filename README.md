### Running locally

Prerequisites: Docker Desktop, .NET 8 SDK, Git.

```bash
git clone <link>
cd stockintel
docker compose up --build
```
`docker compose up` starts all services defined in `docker-compose.yml`.
`--build` rebuilds the API's Docker image from your current source code before starting,
use it whenever theres code changes. Don't use it if you just want to restart without rebuilding.

The API is available at http://localhost:8080/swagger once both containers are healthy.

To stop everything:

```bash
docker compose down
```

This stops and removes the containers. Your data is not deleted unless you explicitly pass `--volumes`.




Sanity check

`$ dotnet build`

Run the api

`$ dotnet run --project src/StockIntel.Api`

Run development dependencies

`$ docker compose up -d`, where -d means detached (runs in background)

Run Postgres

`$ docker compose up -d`

check if its running with 

`$ docker compose ps`

Migration

`$ dotnet ef migrations add InitialSchema --project src/StockIntel.Infrastructure --startup-project src/StockIntel.Api`. What this command does: EF Core inspects  DbContext and entity configs, compares them to the current database schema, and generates C# code that will create the tables when executed. The output goes to src/StockIntel.Infrastructure/Migrations/.

adding migration to db

`$ dotnet ef database update --project src/StockIntel.Infrastructure --startup-project src/StockIntel.Api`

Connect to postgres

`$ docker exec -it stockintel-postgres psql -U stockintel -d stockintel` you can then do commands like `\dt` and `\q`