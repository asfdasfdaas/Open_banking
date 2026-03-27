# AI Guidelines for This Repository

This file gives AI assistants and contributors the minimum context needed to work safely and consistently in this project.

## Project Overview

- Monorepo with:
  - `WebApplication1/` -> ASP.NET Core Web API (`net8.0`)
  - `open-banking-ui/` -> Angular app (`@angular/*` 21.x)
- Domain: open banking features (auth, account data, bank integrations, AI chat/spending analysis).

## High-Level Architecture

### Backend (`WebApplication1`)

- Entry point: `Program.cs`
- API style: Controller-based REST (`Controllers/`)
- Persistence: Entity Framework Core + SQL Server (`ApplicationDBContext`, migrations in `Migrations/`)
- Auth: JWT Bearer in `Program.cs` + auth endpoints in `AuthController`
- Integrations:
  - Bank provider integration services
  - Gemini integration for AI features
- DI registrations are centralized in `Program.cs`.

Main backend folders:

- `Controllers/` -> API endpoints (`AiController`, `AuthController`, `AccountListController`, `VakifbankController`)
- `Services/` and `Services/Providers/` -> business logic and third-party integrations
- `Repository/` -> data access layer
- `Models/` -> DTOs and external payload models
- `Migrations/` -> EF Core migrations
- `Middleware/` -> cross-cutting behavior (exception handling, etc.)

### Frontend (`open-banking-ui`)

- Angular 21 app
- Main app code: `open-banking-ui/src/app/`
- Contains components, services, interceptors, and pipes
- Dev server default: `http://localhost:4200`

## Run and Build

### Backend

From `WebApplication1/`:

- `dotnet restore`
- `dotnet build`
- `dotnet run`

### Frontend

From `open-banking-ui/`:

- `npm install`
- `npm start` (or `ng serve`)
- `npm run build`
- `npm test` (Angular test command configured)

## Configuration and Secrets

- Backend config in:
  - `WebApplication1/appsettings.json`
  - `WebApplication1/appsettings.Development.json`
- Do not hardcode secrets/tokens in code.
- Use environment-specific config or user secrets for sensitive values.
- JWT and integration keys should be injected through configuration.

## API and Integration Notes

- AI chat endpoint exists in `AiController`:
  - `POST /api/ai/chat`
- Spending analysis endpoint exists in `AiController`:
  - `POST /api/ai/analyze-spending/{accountNumber}?startDate=...&endDate=...`
- CORS currently allows localhost Angular origin in `Program.cs`.
- Keep controller actions thin; place business logic in services.

## Development Rules

1. Respect existing structure
- Keep business logic in services and repositories, not controllers.
- Keep DTOs in `Models/DTOs`.
- Keep external provider payloads under `Models/External/...`.
- Services must return DTOs to Controllers. Controllers should not pass raw Database Entities to the frontend.

2. Dependency injection first
- Add new interfaces and implementations in the right folders.
- Register all new services/repositories in `Program.cs`.

3. Backward-compatible API changes
- Avoid breaking response shapes unless explicitly requested.
- If changing response contracts, also update frontend services/components.

4. Validation and errors
- Validate request input at the API boundary.
- Return meaningful HTTP status codes (`400`, `401`, `404`, `500`).
- Use central exception handling patterns already in project.

5. Data changes
- For schema updates, create EF migration and keep names descriptive.
- Do not manually edit generated migration snapshots unless required.

6. Security
- Never log tokens, secrets, or sensitive personal/banking data.
- Keep JWT-protected endpoints protected.
- Check CORS/auth effects when adding new endpoints.

## Frontend Rules

- Keep API calls in service layer (`src/app/services`), not components.
- Keep auth token handling in interceptors, not duplicated across components.
- Keep UI components focused on rendering and interaction.
- Update/add tests for service and component behavior when changing logic.
- Use RxJS observables for asynchronous data streams and ensure subscriptions are cleaned up in ngOnDestroy to prevent memory leaks.

## AI Assistant Working Agreement

When an AI assistant makes changes, it should:

1. Read relevant files first (`Program.cs`, target controller/service/repository, related frontend service/component).
2. Make the smallest safe change that solves the request.
3. Keep naming and folder conventions consistent with current code.
4. Run or suggest validation steps after edits.
5. Avoid touching unrelated files.
6. Document assumptions briefly when requirements are unclear.

## Change Validation Checklist

Backend changes:

- `dotnet build` succeeds
- New/changed endpoints compile and route correctly
- DI registrations are complete
- Auth/CORS behavior still correct

Frontend changes:

- `npm run build` succeeds
- No obvious runtime errors on affected screens
- API integration paths and request/response mapping still match backend

## Suggested Future Improvements

- Add endpoint-level tests for critical controllers (auth, account list, AI endpoints)
- Add lint/format scripts and CI checks for both projects
- Consider response envelope consistency across API endpoints

---

If you rename this file later, prefer `AiGuidelines.md` (correct spelling) or `AGENTS.md` for broader tooling compatibility.
