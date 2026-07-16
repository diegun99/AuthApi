# AGENTS.md

## Quick start
```powershell
dotnet restore
dotnet ef database update   # requires PostgreSQL running
dotnet run --project AuthApi # http://localhost:5262, https://localhost:7094
dotnet watch run --project AuthApi  # hot reload
```

## Solution (`AuthBackend.slnx`)
- Located at `WebApiBackend/AuthBackend.slnx` (parent of this project)
- Use `dotnet build`, `dotnet test` etc. from the solution root or with `--project AuthApi`

## Key commands
| Action | Command |
|--------|---------|
| Build | `dotnet build` |
| Run | `dotnet run` |
| Hot reload | `dotnet watch run` |
| Add migration | `dotnet ef migrations add <Name>` |
| Apply migration | `dotnet ef database update` |
| OpenAPI/Swagger | `http://localhost:5262/openapi/v1.json` (dev only) |
| Test | no test project exists yet |

## Architecture
- **Minimal API** — all routes defined via extension methods in `Endpoints/AuthEndpoints.cs`, not controllers
- **Entrypoint**: `Program.cs` — services, middleware, pipeline
- **DB**: EF Core + Npgsql (PostgreSQL), Identity tables lowercased in `AppDbContext.OnModelCreating`
- **Auth stack**: JWT Bearer (default) + Google OAuth — `MapInboundClaims = false`, `ClockSkew = TimeSpan.Zero`
- **Auto-seed**: `SeedData.Initialize` runs on every startup (creates roles Admin/User/Manager + admin user `admin@authapp.com` / `Admin123!`)

## Critical gotchas
1. **Endpoints NOT wired** — `// app.MapAuthEndpoints();` is commented out in `Program.cs:124`. Uncomment to enable routes.
2. **Port mismatch**: `launchSettings.json` uses `5262`/`7094`. README says `5000`/`5001` — trust `launchSettings.json`.
3. **CORS locked** to `http://localhost:4200` — change in `Program.cs:96` for other frontends.
4. **Password policy**: 8+ chars, digit, lower, upper, non-alphanumeric — enforced by Identity config in `Program.cs`.
5. **DTOs are record types** defined inline in `AuthEndpoints.cs` — add new DTOs there or in `Models/`.
6. **Table names** forced to lowercase in `AppDbContext.OnModelCreating` for PostgreSQL compatibility.
7. **`.gitignore` excludes `appsettings.*.json`** — secrets like `appsettings.Production.json` won't be committed.

## File layout
```
AuthApi/
├── Program.cs              # entrypoint, DI, middleware pipeline
├── Endpoints/AuthEndpoints.cs   # all API routes + DTO records
├── Data/
│   ├── AppDbContext.cs      # EF Core context (lowercases table names)
│   └── SeedData.cs          # startup seed (roles + admin user)
├── Models/ApplicationUser.cs    # custom IdentityUser (FirstName, LastName, GoogleId)
├── Services/TokenService.cs     # JWT creation + JwtSettings config class
├── Migrations/              # auto-generated EF migrations
└── Properties/launchSettings.json  # dev URLs
```

## Tech stack
.NET 10, ASP.NET Core Minimal APIs, EF Core + Npgsql, ASP.NET Core Identity, JWT Bearer, Google OAuth, OpenAPI.
