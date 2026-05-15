### Run 

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