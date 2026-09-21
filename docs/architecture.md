# Expense Tracker API — Architecture & Implementation Plan

> A production-grade ASP.NET Core Web API built as a **Modular Monolith** with **Clean Architecture**, targeting **.NET 10 (LTS)** and **PostgreSQL**. React frontend to follow once the backend is fully functional.

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Architecture Decisions](#2-architecture-decisions)
3. [Software Architecture Explained](#3-software-architecture-explained)
4. [Project Structure & Types](#4-project-structure--types)
5. [Technology Stack](#5-technology-stack)
6. [Step-by-Step Implementation Plan](#6-step-by-step-implementation-plan)
7. [API Endpoint Specification](#7-api-endpoint-specification)
8. [Data Model](#8-data-model)
9. [Authentication Design](#9-authentication-design)
10. [Testing Strategy](#10-testing-strategy)
11. [Deployment Plan](#11-deployment-plan)
12. [React Frontend (Phase 2)](#12-react-frontend-phase-2)
13. [Common Pitfalls to Avoid](#13-common-pitfalls-to-avoid)

---

## 1. Project Overview

An API for an expense tracker application. Users can sign up, log in, and manage their own set of expenses.

### Features

- Sign up as a new user
- Generate and validate JWTs for authentication and user sessions
- List and filter past expenses:
  - Past week
  - Past month
  - Last 3 months
  - Custom (start and end date)
- Add a new expense
- Remove existing expenses
- Update existing expenses

### Constraints

- JWT used to protect endpoints and identify the requester
- Expense categories: `Groceries`, `Leisure`, `Electronics`, `Utilities`, `Clothing`, `Health`, `Others`
- Database of choice (PostgreSQL selected)

---

## 2. Architecture Decisions

| Decision | Choice | Rationale |
|---|---|---|
| **Monolith Style** | Modular Monolith | Single deployable, one developer, no distributed-systems overhead. Clean seams for future extraction. |
| **Code Structure** | Clean Architecture (layered per module) | Domain has no framework dependencies. EF Core stays in Infrastructure. Business logic is testable. |
| **Database** | PostgreSQL | Free, robust, industry standard. Pairs with EF Core via Npgsql. |
| **.NET Version** | .NET 10 (LTS) | Supported until November 2028. Stable for new projects. |
| **API Style** | Minimal API (auth) + Controllers (expenses) | Minimal APIs are lean for small endpoint groups. Controllers are clearer for resource-heavy CRUD. |
| **Auth** | JWT access token + refresh token | Production-grade session handling. Short-lived access token, long-lived rotating refresh token. |
| **Password Hashing** | BCrypt.Net-Next | Industry standard, adaptive cost factor. |
| **Validation** | FluentValidation | Declarative, testable, separates validation from DTOs. |
| **Logging** | Serilog | Structured logging, easy sinks (console, file, Seq). |
| **Testing** | xUnit + WebApplicationFactory | Unit + integration tests. |

### Why Modular Monolith (not Microservices)

- **Microservices** would add network latency, distributed tracing, service discovery, and deployment complexity — with no benefit at this scale.
- **Modular Monolith** keeps everything in one process but enforces module boundaries through project references and interfaces. If you ever need to extract a service, the seams are already there.
- **Traditional Monolith** (folders instead of projects) is simpler but allows accidental coupling. Modular enforces discipline.

### Why Clean Architecture

```

Domain  ←  Application  ←  Infrastructure
↑
API (Host)

```

- **Domain** — entities, enums, value objects. No dependencies.
- **Application** — use cases, DTOs, interfaces. Depends only on Domain.
- **Infrastructure** — EF Core, repositories, external services. Implements Application interfaces.
- **API / Host** — composition root. Wires DI, middleware, endpoints.

This means: your business logic never references EF Core, and you can swap the database without touching the domain.

---

## 3. Software Architecture Explained

### Module Isolation

Each module (Identity, Expenses) owns its own Domain, Application, and Infrastructure layers. Modules never directly query another module's tables. They communicate through:

- Interfaces defined in the consuming module's Application layer
- Domain events (optional, for future)
- SharedKernel for common primitives (`Result<T>`, `BaseEntity`)

### SharedKernel Contents

| Type | Purpose |
|---|---|
| `Result<T>` | Return success/failure without exceptions |
| `Error` | Structured error (code + message) |
| `BaseEntity` | Id, CreatedAtUtc, UpdatedAtUtc |
| `ICurrentUser` | Abstraction to get authenticated user id |
| `IDateTimeProvider` | Testable time abstraction |

### Request Flow (Example: Create Expense)

```

HTTP POST /api/expenses
↓
ExpensesController (API)
↓
CreateExpenseHandler (Application)
↓
IExpenseRepository (Application interface)
↓
ExpenseRepository (Infrastructure, EF Core)
↓
PostgreSQL

```

The controller never touches EF Core. The handler never touches HTTP. Each layer has one job.

---

## 4. Project Structure & Types

### Solution Layout

```

ExpenseTracker/
├── src/
│   ├── Api/                              # webapi
│   ├── Shared/
│   │   └── SharedKernel/                 # classlib
│   └── Modules/
│       ├── Identity/
│       │   ├── Identity.Domain/          # classlib
│       │   ├── Identity.Application/     # classlib
│       │   └── Identity.Infrastructure/  # classlib
│       └── Expenses/
│           ├── Expenses.Domain/          # classlib
│           ├── Expenses.Application/     # classlib
│           └── Expenses.Infrastructure/  # classlib
├── tests/
│   ├── UnitTests/                        # xunit
│   └── IntegrationTests/                 # xunit
├── docker-compose.yml
├── .gitignore
├── Directory.Build.props
└── README.md

```
```

ExpenseTracker/
├── src/
│   ├── Api/                              # Host, composition root, Program.cs
│   ├── Shared/
│   │   └── ExpenseTracker.SharedKernel/  # Result<T>, BaseEntity, common types
│   └── Modules/
│       ├── Identity/
│       │   ├── Identity.Domain/          # User, RefreshToken entities
│       │   ├── Identity.Application/     # Auth handlers, DTOs
│       │   └── Identity.Infrastructure/  # EF config, TokenService
│       └── Expenses/
│           ├── Expenses.Domain/          # Expense, ExpenseCategory
│           ├── Expenses.Application/     # CRUD + Filter handlers
│           └── Expenses.Infrastructure/  # EF config, Repositories
├── tests/
│   ├── ExpenseTracker.UnitTests/
│   └── ExpenseTracker.IntegrationTests/
├── docker-compose.yml
└── README.md

```

### Project Types Reference

| Project | `dotnet new` Template | Why |
|---|---|---|
| `ExpenseTracker.Api` | `webapi` | Host / composition root. Only project with an entry point. |
| `ExpenseTracker.SharedKernel` | `classlib` | Pure abstractions. No ASP.NET, no EF Core. |
| `Identity.Domain` | `classlib` | Entities: `User`, `RefreshToken`. No framework deps. |
| `Identity.Application` | `classlib` | Handlers, DTOs, validators, interfaces. |
| `Identity.Infrastructure` | `classlib` | EF Core configs, `TokenService`, `PasswordHasher`. |
| `Expenses.Domain` | `classlib` | `Expense` entity, `ExpenseCategory` enum. |
| `Expenses.Application` | `classlib` | CRUD + filter handlers, DTOs. |
| `Expenses.Infrastructure` | `classlib` | EF configs, repository implementations. |
| `ExpenseTracker.UnitTests` | `xunit` | Fast tests, no external dependencies. |
| `ExpenseTracker.IntegrationTests` | `xunit` | Spins up API + test database. |

### Project References

```

ExpenseTracker.Api
→ Identity.Application
→ Identity.Infrastructure
→ Expenses.Application
→ Expenses.Infrastructure
→ SharedKernel

Identity.Application
→ Identity.Domain
→ SharedKernel

Identity.Infrastructure
→ Identity.Application
→ Identity.Domain
→ SharedKernel

Expenses.Application
→ Expenses.Domain
→ SharedKernel

Expenses.Infrastructure
→ Expenses.Application
→ Expenses.Domain
→ SharedKernel

```

**Rule:** Domain projects reference nothing. Application references Domain + SharedKernel. Infrastructure references Application + Domain. Api references Infrastructure (to register implementations) and Application (to call handlers).

### Creation Commands

```bash
# Solution
dotnet new sln -n ExpenseTracker

# Host
dotnet new webapi -n ExpenseTracker.Api -o src/Api

# SharedKernel
dotnet new classlib -n ExpenseTracker.SharedKernel -o src/Shared/SharedKernel

# Identity Module
dotnet new classlib -n Identity.Domain -o src/Modules/Identity/Identity.Domain
dotnet new classlib -n Identity.Application -o src/Modules/Identity/Identity.Application
dotnet new classlib -n Identity.Infrastructure -o src/Modules/Identity/Identity.Infrastructure

# Expenses Module
dotnet new classlib -n Expenses.Domain -o src/Modules/Expenses/Expenses.Domain
dotnet new classlib -n Expenses.Application -o src/Modules/Expenses/Expenses.Application
dotnet new classlib -n Expenses.Infrastructure -o src/Modules/Expenses/Expenses.Infrastructure

# Tests
dotnet new xunit -n ExpenseTracker.UnitTests -o tests/UnitTests
dotnet new xunit -n ExpenseTracker.IntegrationTests -o tests/IntegrationTests

# Add all to solution
dotnet sln add (ls -r **/*.csproj)
```

---

## 5. Technology Stack

### Backend

| Category | Package / Tool |
|---|---|
| Runtime | .NET 10 SDK |
| Web Framework | ASP.NET Core |
| ORM | Entity Framework Core 10 |
| DB Provider | `Npgsql.EntityFrameworkCore.PostgreSQL` |
| Auth | `Microsoft.AspNetCore.Authentication.JwtBearer` |
| Password Hashing | `BCrypt.Net-Next` |
| Validation | `FluentValidation.AspNetCore` |
| Logging | `Serilog.AspNetCore` |
| API Docs | `Swashbuckle.AspNetCore` (Swagger) |
| DB Migrations | `Microsoft.EntityFrameworkCore.Design` |

### Database & Tooling

| Tool | Purpose |
|---|---|
| PostgreSQL 16+ | Primary database |
| pgAdmin or DBeaver | DB GUI |
| Docker Desktop | Local Postgres container |
| Postman / Insomnia | Manual API testing |
| VS Code + C# Dev Kit | Editor |
| Git + GitHub | Version control |

### Frontend (Phase 2)

| Tool | Purpose |
|---|---|
| Node.js LTS | Runtime |
| Vite | Build tool |
| React + TypeScript | UI framework |
| React Router | Routing |
| TanStack Query | Server state |
| Axios | HTTP client |
| Zod | Schema validation |
| shadcn/ui or Mantine | Component library |

---

## 6. Step-by-Step Implementation Plan

### Phase 1 — Foundation

**Step 1: Install prerequisites**

- .NET 10 SDK
- PostgreSQL (or Docker Desktop)
- VS Code + C# Dev Kit
- Git

**Step 2: Create the solution and projects**
Run the creation commands from Section 4.

**Step 3: Wire project references**
Add references following the diagram in Section 4.

**Step 4: Add `Directory.Build.props`**
Centralize target framework, nullable, implicit usings.

```
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
```

**Step 5: Start PostgreSQL with Docker Compose**

```
services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_USER: expense_user
      POSTGRES_PASSWORD: expense_pass
      POSTGRES_DB: expense_tracker
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data

volumes:
  pgdata:
```

---

### Phase 2 — SharedKernel

**Step 6: Implement SharedKernel primitives**

- `Result<T>` and `Result`
- `Error` record
- `BaseEntity` with `Id`, `CreatedAtUtc`, `UpdatedAtUtc`
- `ICurrentUser` interface
- `IDateTimeProvider` interface

---

### Phase 3 — Identity Module

**Step 7: Domain layer**

- `User` entity: `Id`, `Email`, `PasswordHash`, `CreatedAtUtc`
- `RefreshToken` entity: `Id`, `UserId`, `Token`, `ExpiresAtUtc`, `RevokedAtUtc`
- `Email` value object (optional but recommended)

**Step 8: Application layer**

- `RegisterUserHandler`, `LoginHandler`, `RefreshTokenHandler`
- DTOs: `RegisterRequest`, `LoginRequest`, `AuthResponse`
- Interfaces: `IUserRepository`, `IPasswordHasher`, `ITokenService`, `ICurrentUser`
- FluentValidation validators for each request

**Step 9: Infrastructure layer**

- EF Core configurations for `User` and `RefreshToken`
- `UserRepository` implementation
- `BCryptPasswordHasher`
- `JwtTokenService` — generates access + refresh tokens

**Step 10: Minimal API endpoints**

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- `GET /api/auth/me` (protected)

---

### Phase 4 — Expenses Module

**Step 11: Domain layer**

- `Expense` entity: `Id`, `UserId`, `Amount`, `Description`, `Category`, `Date`, `CreatedAtUtc`, `UpdatedAtUtc`
- `ExpenseCategory` enum

**Step 12: Application layer**

- Handlers: `CreateExpenseHandler`, `UpdateExpenseHandler`, `DeleteExpenseHandler`, `GetExpensesHandler`, `GetExpenseByIdHandler`
- DTOs: `CreateExpenseRequest`, `UpdateExpenseRequest`, `ExpenseResponse`, `ExpenseFilterRequest`
- Interfaces: `IExpenseRepository`
- Validators

**Step 13: Infrastructure layer**

- EF Core configuration for `Expense`
- `ExpenseRepository` implementation with filter logic
- Query translation for week / month / 3months / custom

**Step 14: Controllers**

- `ExpensesController` with:

- `GET /api/expenses`
- `GET /api/expenses/{id}`
- `POST /api/expenses`
- `PUT /api/expenses/{id}`
- `DELETE /api/expenses/{id}`

---

### Phase 5 — Cross-Cutting Concerns

**Step 15: Configure `Program.cs`**

- Register DbContexts
- Register module services
- Add JWT authentication
- Add authorization
- Add Swagger with Bearer auth
- Add Serilog
- Add CORS policy
- Add health checks
- Add rate limiting on auth endpoints
- Map endpoints and controllers

**Step 16: Global error handling**

- `IExceptionHandler` implementation
- ProblemDetails (RFC 7807) responses

**Step 17: Migrations**

```
dotnet ef migrations add InitialCreate -p src/Modules/Identity/Identity.Infrastructure -s src/Api
dotnet ef database update -p src/Modules/Identity/Identity.Infrastructure -s src/Api
```

---

### Phase 6 — Testing

**Step 18: Unit tests**

- Handler tests with mocked repositories
- Validator tests
- Domain logic tests

**Step 19: Integration tests**

- `WebApplicationFactory<Program>` setup
- Testcontainers for PostgreSQL
- Full flow tests: register → login → create → list → update → delete

---

### Phase 7 — Deployment

**Step 20: Prepare for production**

- Move secrets to environment variables
- Configure production connection string
- Add health check endpoint
- Configure CORS for the React origin

**Step 21: Deploy**

- Backend: Railway / Render / Fly.io / Azure App Service
- Database: Neon / Supabase / Railway Postgres
- Run migrations in deployment pipeline

---

## 7. API Endpoint Specification

### Auth (Minimal API)

| Method ↕▾ | Route ↕▾ | Auth ↕▾ | Description ↕▾ |
|---|---|---|---|
| −POST | `/api/auth/register` | Anonymous | Create new user |
| POST | `/api/auth/login` | Anonymous | Get access + refresh tokens |
| POST | `/api/auth/refresh` | Anonymous | Rotate refresh token |
| POST | `/api/auth/logout` | Bearer | Revoke refresh token |
| GET | `/api/auth/me` | Bearer | Current user info |
⚙

### Expenses (Controllers)

| Method ↕▾ | Route ↕▾ | Auth ↕▾ | Description ↕▾ |
|---|---|---|---|
| −GET | `/api/expenses` | Bearer | List + filter expenses |
| −GET | `/api/expenses/{id}` | Bearer | Get one expense |
| −POST | `/api/expenses` | Bearer | Create expense |
| PUT | `/api/expenses/{id}` | Bearer | Update expense |
| DELETE | `/api/expenses/{id}` | Bearer | Delete expense |
⚙

### Filter Query Parameters (GET /api/expenses)

| Parameter ↕▾ | Type ↕▾ | Description ↕▾ |
|---|---|---|
| −`filter` | string | `week` | `month` | `3months` | `custom` |
| −`start` | date | Required if `filter=custom` |
| `end` | date | Required if `filter=custom` |
| `category` | string | Optional, one of the enum values |
| `page` | int | Default 1 |
| `pageSize` | int | Default 20, max 100 |
⚙

### Example Request

```
GET /api/expenses?filter=month&category=Groceries&page=1&pageSize=20
Authorization: Bearer <access_token>
```

### Example Response

```
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "amount": 1250.50,
      "description": "Weekly groceries",
      "category": "Groceries",
      "date": "2026-09-15",
      "createdAtUtc": "2026-09-15T08:30:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1
}
```

---

## 8. Data Model

### User

| Column ↕▾ | Type ↕▾ | Notes ↕▾ |
|---|---|---|
| −Id | uuid | PK |
| −Email | varchar(255) | Unique index, required |
| −PasswordHash | text | BCrypt hash |
| −CreatedAtUtc | timestamptz | Required |
| UpdatedAtUtc | timestamptz | Nullable |
⚙

### RefreshToken

| Column ↕▾ | Type ↕▾ | Notes ↕▾ |
|---|---|---|
| −Id | uuid | PK |
| −UserId | uuid | FK → User, cascade delete |
| Token | varchar(512) | Unique index |
| ExpiresAtUtc | timestamptz | Required |
| RevokedAtUtc | timestamptz | Nullable |
| CreatedAtUtc | timestamptz | Required |
⚙

### Expense

| Column ↕▾ | Type ↕▾ | Notes ↕▾ |
|---|---|---|
| −Id | uuid | PK |
| −UserId | uuid | FK → User, cascade delete |
| −Amount | numeric(18,2) | Required |
| −Description | varchar(500) | Required |
| −Category | varchar(50) | Stored as string |
| Date | date | Required |
| CreatedAtUtc | timestamptz | Required |
| UpdatedAtUtc | timestamptz | Nullable |
⚙

### ExpenseCategory Enum

```
public enum ExpenseCategory
{
    Groceries,
    Leisure,
    Electronics,
    Utilities,
    Clothing,
    Health,
    Others
}
```

### EF Core Configuration Notes

- `Amount`: `.HasColumnType("numeric(18,2)")`
- `Category`: `.HasConversion<string>()` for readability in DB
- `Email`: `.HasIndex(u => u.Email).IsUnique()`
- `Expense.UserId`: `.HasIndex()` — filter queries will hit this
- Global query filter: `modelBuilder.Entity<Expense>().HasQueryFilter(e => e.UserId == _currentUser.Id)` — optional but consider explicit `Where` in the repository for clarity

---

## 9. Authentication Design

### Token Strategy

| Token ↕▾ | Lifetime ↕▾ | Storage ↕▾ | Purpose ↕▾ |
|---|---|---|---|
| −Access token | 15 minutes | Client memory | Authorize API calls |
| −Refresh token | 7 days | httpOnly cookie (or secure storage) | Obtain new access token |
⚙

### JWT Claims

| Claim ↕▾ | Value ↕▾ |
|---|---|
| −`sub` | User Id (GUID) |
| −`email` | User email |
| −`jti` | Token Id (for revocation) |
| `iat` | Issued at |
| `exp` | Expiration |
⚙

### Refresh Token Rotation

1. Client sends refresh token to `/api/auth/refresh`
2. Server validates it exists, isn't revoked, isn't expired
3. Server revokes the old token
4. Server issues a new access + refresh token pair
5. If a revoked token is reused → revoke **all** user tokens (breach detection)

### Password Requirements

- Minimum 8 characters
- At least one uppercase, one lowercase, one digit
- Hashed with BCrypt (work factor 12)

### Security Rules

- **Never** return the `PasswordHash` in any response
- **Never** accept a `UserId` from the client on expense endpoints — always read from `HttpContext.User`
- **Never** store tokens in `localStorage` in production
- Signing key must be at least 256 bits, stored in user-secrets locally, env vars in production
- HTTPS only

---

## 10. Testing Strategy

### Unit Tests

- One test class per handler
- Mock repositories and `ICurrentUser`
- Cover happy path + edge cases (invalid input, not found, unauthorized)
- Fast — no DB, no HTTP

### Integration Tests

- `WebApplicationFactory<Program>` with a real (containerized) Postgres
- Test full request/response cycle
- Cover:

- Register → login → get token
- Create expense → list → filter → update → delete
- Access another user's expense → expect 404
- Expired token → 401
- Invalid refresh token → 401

### Coverage Targets

- Domain logic: 100%
- Application handlers: ≥ 90%
- Controllers/endpoints: covered via integration tests

---

## 11. Deployment Plan

### Backend Hosting Options

| Platform ↕▾ | Notes ↕▾ |
|---|---|
| −Railway | Easy .NET + Postgres |
| Render | Free tier available |
| Fly.io | Global edge, Docker-based |
| Azure App Service | Full Microsoft stack |
⚙

### Database Hosting

| Provider ↕▾ | Notes ↕▾ |
|---|---|
| −Neon | Serverless Postgres, generous free tier |
| −Supabase | Postgres + extras |
| Railway Postgres | Simple, paired with Railway hosting |
⚙

### Deployment Checklist

- □  
Secrets moved to environment variables
- □  
`appsettings.Production.json` configured
- □  
Connection string set via env var
- □  
JWT signing key set via env var
- □  
CORS configured for React origin
- □  
Health check endpoint enabled
- □  
Migrations applied in pipeline
- □  
HTTPS enforced
- □  
Rate limiting active
- □  
Logging sink configured

---

## 12. React Frontend (Phase 2)

### Setup

```
npm create vite@latest expense-tracker-ui -- --template react-ts
cd expense-tracker-ui
npm install axios @tanstack/react-query react-router-dom zod jwt-decode
```

### Structure

```
src/
├── api/              # Axios instance + endpoint functions
├── components/       # Reusable UI
├── features/
│   ├── auth/         # Login, Register pages
│   └── expenses/     # List, filters, create/edit modal
├── hooks/            # useAuth, useExpenses
├── lib/              # Utilities
├── routes/           # Route definitions + guards
└── main.tsx
```

### Key Patterns

- Axios interceptor to attach access token
- Interceptor to refresh on 401 and retry
- TanStack Query for caching and pagination
- Protected route wrapper
- Zod schemas mirroring backend validation

### Pages to Build

1. Login
2. Register
3. Dashboard (expense list + filter tabs)
4. Create / Edit expense modal
5. Profile (logout)

---

## 13. Common Pitfalls to Avoid

1. **Trusting client-supplied `UserId`** — always read from JWT claims.
2. **Returning 403 instead of 404** when a user requests another user's resource — 403 leaks that the resource exists.
3. **Using `double` or `float` for money** — always `decimal`.
4. **Mixing timezones** — store UTC, filter in UTC, format on the client.
5. **Skipping database indexes** — index `Expense.UserId` and `Expense.Date` (filters will hammer these).
6. **Putting EF Core in the Domain layer** — Domain should have zero infrastructure dependencies.
7. **Giant `Program.cs`** — use extension methods per module: `services.AddIdentityModule(config)`, `services.AddExpensesModule(config)`.
8. **Storing JWTs in `localStorage`** — XSS risk. Use memory + httpOnly refresh cookie.
9. **No refresh token rotation** — a leaked refresh token becomes permanent access.
10. **Forgetting migrations** — commit them, and run them in the deploy pipeline.
11. **Not testing unauthorized paths** — the most common production bug in this kind of API.
12. **Leaving CORS wide open** — restrict to the React dev/prod origins.

---

## Summary Cheat Sheet

| Item ↕▾ | Value ↕▾ |
|---|---|
| −Architecture | Modular Monolith + Clean Architecture |
| −Runtime | .NET 10 (LTS) |
| Database | PostgreSQL 16+ |
| Auth | JWT access (15 min) + refresh (7 days) |
| Password hashing | BCrypt (work factor 12) |
| API style | Minimal API (auth) + Controllers (expenses) |
| Validation | FluentValidation |
| Logging | Serilog |
| Testing | xUnit + Testcontainers |
| Frontend | React + Vite + TypeScript + TanStack Query |
| Categories | Groceries, Leisure, Electronics, Utilities, Clothing, Health, Others |
⚙

---

*End of document. Keep this file in the repo root as `docs/architecture.md`.*

