# joshuaAlonzo/API

A pharmacy management REST API built with **ASP.NET Core (.NET 10)**, **SQLite**, and **JWT authentication**.

---

## What the API does

The API manages the core operations of a pharmacy system:

| Domain | What it covers |
|---|---|
| **Users** | Registration, login, profile management |
| **User Roles** | Role definitions (admin, mod, user) |
| **Medicines** | Inventory — listing, adding, editing medicines |
| **Categories** | Medicine category management |
| **Cart** | Per-user shopping cart |
| **Orders** | Order creation and order item tracking |
| **Activity Log** | Audit trail of user actions |

---

## Module Structure

```
Api/
├── ActivityLogModule/     # Activity log entity, repo, controller
├── CartModule/            # Cart entity, repo
├── CategoriesModule/      # Category entity, repo
├── Controllers/           # HTTP controllers for all endpoints
├── DTOs/                  # Request/response data transfer objects + JSON converters
├── Main/                  # MyCon (DB connection), ConnEnvFile (env loader)
├── MedicineModule/        # Medicine entity, repo
├── OrderModule/           # Order + OrderItem entities and repos
├── Security/              # JWT configuration and token service
├── UserModule/            # User entity, repo
├── UserRoleModule/        # UserRole entity, repo
├── Program.cs             # App entry point, middleware pipeline, DI registration
├── conn.env.example       # Template — copy to conn.env and fill in real values
└── .gitignore
```

---

## Auth Flow

Authentication uses **JWT Bearer tokens**.

1. **Login** — `POST /api/user/login` returns a signed JWT.
2. **Token claims** — The token includes a `user_role_id` claim.
3. **Authorization policies** — Endpoints are protected by claim-based policies:

| Policy | Required `user_role_id` |
|---|---|
| `AdminAccess` | `1` |
| `ModAccess` | `1` or `2` |
| `UserAccess` | `1`, `2`, or `3` |

---

## Local Setup

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Git

### Steps

1. **Clone the repo**
   ```bash
   git clone https://github.com/joshuaAlonzo/API.git
   cd API
   ```

2. **Create your local config file**
   ```bash
   copy conn.env.example conn.env   # Windows
   # or
   cp conn.env.example conn.env     # macOS / Linux
   ```

3. **Fill in `conn.env`** — open it and set:
   - `DB_FILE_PATH` — path to your SQLite file (default `pharmacy.db` works out of the box)
   - `Jwt__Key` — a strong random secret (32+ bytes). Generate one with:
     ```powershell
     # PowerShell
     -join (1..32 | ForEach-Object { '{0:X2}' -f (Get-Random -Maximum 256) })
     ```
     ```bash
     # bash
     openssl rand -hex 32
     ```

4. **Run the API**
   ```bash
   dotnet run
   ```

---

## Swagger UI

In development, Swagger UI is served at the root URL:

```
http://localhost:<port>/
```

Use the **Authorize** button to paste a Bearer token and test protected endpoints.

> [!NOTE]
> Swagger UI is only available when `ASPNETCORE_ENVIRONMENT=Development`. In production this env var must be set to `Production`.
