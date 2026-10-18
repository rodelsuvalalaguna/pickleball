# Pickleball Court Booking

- `backend/` ASP.NET Core 8 Web API, EF Core 8 (SQL Server), JWT + refresh tokens
- `frontend/` Nuxt 3, Tailwind

## Run locally

```powershell
# backend (appsettings.Development.json is gitignored; create it from the values below)
cd backend
dotnet run --launch-profile https      # Swagger at /swagger; migrates + seeds on startup

# frontend
cd frontend
copy .env.example .env
npm install; npm run dev
```

Required config (user secrets / env vars; never commit):

| Setting | Env var |
|---|---|
| `ConnectionStrings:Default` | `ConnectionStrings__Default` |
| `Jwt:Key` (>= 32 chars) | `Jwt__Key` |
| `Cors:AllowedOrigins:0` (deployed Nuxt origin) | `Cors__AllowedOrigins__0` |
| `Seed:AdminEmail`, `Seed:AdminPassword` (first run only) | `Seed__AdminEmail`, `Seed__AdminPassword` |
| `Booking:TimeZone` (e.g. `America/New_York`) | `Booking__TimeZone` |

Local dev with user secrets: `dotnet user-secrets set "Jwt:Key" "<random 64 chars>"`.

## Deployment security checklist

- SQL Server/Azure SQL: no public endpoint; private VNet/private endpoint or firewall allowlist of the API host only; prefer managed identity auth.
- Behind a reverse proxy, configure `UseForwardedHeaders` so rate limiting sees client IPs.
- Terminate HTTPS at the API/proxy; HSTS is enabled outside Development.
