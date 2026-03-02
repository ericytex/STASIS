# STASIS

This project is the Specimen Tracking And Storage Information System (STASIS).

## Local Setup with PostgreSQL

These steps are OS agnostic. You can develop on macOS with VS Code or on Windows with Visual Studio or VS Code. The shared requirement is a supported .NET SDK and a local PostgreSQL instance.

### 1. Install prerequisites

Install the following:

- .NET SDK from `https://dotnet.microsoft.com/download`
- PostgreSQL 17 from `https://www.postgresql.org/download/`
- An editor or IDE such as Visual Studio, VS Code, or JetBrains Rider

Verify the installs from a terminal:

```bash
dotnet --info
psql --version
```

### 2. Create the PostgreSQL database and bootstrap tables

Create the application login if you have not already done so:

```bash
psql postgres
CREATE ROLE stasis_app WITH LOGIN PASSWORD 'YOUR_POSTGRES_PASSWORD';
\q
```

Create the local database:

```bash
createdb -O stasis_app stasis
```

Confirm that it exists:

```bash
psql -lqt | grep stasis
```

Run the STASIS PostgreSQL bootstrap script from the repository root:

```bash
psql -U stasis_app -d stasis -f STASIS/STASIS_create_tables_postgres.sql
```

The bootstrap script drops and recreates the STASIS tables inside the `stasis` database, so you can rerun it during early development without dropping the database itself.

Longer term, prefer EF Core migrations over a hand-maintained SQL script.

### 3. Configure the application

Store the local PostgreSQL connection string in .NET User Secrets instead of `appsettings.Development.json`:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=stasis;Username=stasis_app;Password=YOUR_POSTGRES_PASSWORD"
```

Verify the stored value if needed:

```bash
dotnet user-secrets list
```

Keep `appsettings.Development.json` free of passwords and other secrets. The application is configured to use `UseNpgsql(...)` with the `Npgsql.EntityFrameworkCore.PostgreSQL` provider.

### 4. Open the project

Open the repository in your preferred tool:

- Visual Studio: open `STASIS.sln`
- VS Code: install `ms-dotnettools.csdevkit`, then run `code .`

If you use VS Code, the C# Dev Kit walkthrough can detect the installed .NET SDK and configure launch settings.

### 5. Build and run the app

From the repository root:

```bash
dotnet restore
dotnet build STASIS.sln
dotnet watch run --project STASIS.csproj
```

If `net10.0` does not build on your machine, update `STASIS.csproj` to a supported target framework such as `net9.0`.
