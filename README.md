# Open Banking Project

This repo contains:

- `WebApplication1/` - ASP.NET Core Web API (`net8.0`)
- `open-banking-ui/` - Angular frontend (dev server default: `http://localhost:4200`)

There is also an AI-focused guidance file at the repo root: `AGENTS.md`.

## Requirements

- .NET SDK 8
- Node.js + npm (frontend uses Angular CLI 21.x)
- SQL Server (configured via connection string in `WebApplication1/appsettings.json`)

## How to Run

### Backend (ASP.NET Core)

From `WebApplication1/`:

```powershell
dotnet restore
dotnet build
dotnet run
```

Backend listens on its configured URL(s). Swagger is available in development (see `Program.cs`).

### Frontend (Angular)

From `open-banking-ui/`:

```powershell
npm install
npm start
```

Then open: `http://localhost:4200/`

## Configuration

All backend configuration is in `WebApplication1/appsettings.json` (and `appsettings.Development.json`).

Key settings you will likely need to update:

- Database: `ConnectionStrings:DefaultConnection`
- JWT signing key: `AppSettings:Token`
- Vakifbank integration:
  - `Vakifbank:BaseUrl`
  - `Vakifbank:ClientId`
  - `Vakifbank:ClientSecret`
  - `Vakifbank:SecondClientId`
  - `Vakifbank:SecondClientKey`
- Gemini integration:
  - `Gemini:ApiKey`

Notes:

- JWT validation is configured in `WebApplication1/Program.cs` using the symmetric key from `AppSettings:Token`.
- CORS is currently configured to allow `http://localhost:4200` and to allow credentials (`Program.cs`).
- Avoid committing real secrets; prefer user secrets / environment-specific values for sensitive keys.

## API Routes (Backend)

Base route pattern: `api/[controller]` (so controller name maps directly to the URL segment).

### Auth

- `POST /api/Auth/register`
  - Body: `RegisterDTO` (username, email, password)
- `POST /api/Auth/login`
  - Body: `LoginDTO` (username, password)
  - Response: `{ token, expires }`
- `DELETE /api/Auth/delete-user-account`
  - Auth required (`[Authorize]`)
- `POST /api/Auth/save-vakifbank-consent`
  - Body: plain string `consentId`
  - Auth required

### AI

- `POST /api/Ai/chat`
  - Body: `{ "prompt": "..." }`
  - Response: `{ "reply": "..." }`
- `POST /api/Ai/analyze-spending/{accountNumber}?startDate=...&endDate=...`
  - Body not required (dates are query params)
  - Response: `{ "advice": "..." }`

### Accounts

Controller: `AccountList` (all routes are under `/api/AccountList`).

- `GET /api/AccountList/get-accounts-list`
  - Auth required
- `GET /api/AccountList/{id}get-account`
  - Auth required
  - Note: route is implemented as `"{id}get-account"` (no slash between `{id}` and `get-account`)
- `POST /api/AccountList/create-account`
  - Body: `AccountCreateDTO`
  - Creates an account for the authenticated user
- `POST /api/AccountList/transfer`
  - Body: `TransferDTO`
- `PUT /api/AccountList/{id}update-account`
  - Body: `AccountUpdateDTO`
- `DELETE /api/AccountList/{id}delete-account`
- `GET /api/AccountList/{accountNumber}/transactions?startDate=...&endDate=...`
  - Auth required

### Vakifbank / Bank Integration

Controller: `Vakifbank` (base route `/api/Vakifbank`).

Authorized vs anonymous:

- `[Authorize]` routes: require a valid JWT
- `[AllowAnonymous]` routes: open endpoints (used for catalog/reference data)

Common routes:

- `POST /api/Vakifbank/vakif-accounts` (authorized)
- `POST /api/Vakifbank/account-transactions/{accountNumber}?startDate=...&endDate=...`
- `GET /api/Vakifbank/account-detail/{accountNumber}` (currently implemented as `HttpGet`, but the route is under `VakifbankController`)
- `GET /api/Vakifbank/receipt/{accountNumber}/{transactionId}`
  - Returns `application/pdf`
- Reference data (anonymous, `POST`):
  - `POST /api/Vakifbank/cities`
  - `POST /api/Vakifbank/districts?cityCode=...`
  - `POST /api/Vakifbank/neighborhoods?districtCode=...`
  - `POST /api/Vakifbank/branches?cityCode=...&districtCode=...`
  - `POST /api/Vakifbank/atms?cityCode=...&districtCode=...`
  - `POST /api/Vakifbank/deposit-products`
  - `POST /api/Vakifbank/deposit-calculator` (body: `DepositCalculatorRequest`)
  - `POST /api/Vakifbank/currency-calculator?sourceCurrency=...&amount=...&targetCurrency=...`

## Frontend Notes

Angular lives under `open-banking-ui/` and is a standard Angular CLI app.

Project commands (from `open-banking-ui/`):

```powershell
npm start
npm run build
npm test
```

The frontend typically calls backend endpoints via Angular services under `open-banking-ui/src/app/services`.

## Development Tips / Rules

- Keep backend endpoints thin; move logic into `Services/` and `Repository/`.
- Register new services in `WebApplication1/Program.cs`.
- Keep DTOs in `WebApplication1/Models/DTOs`.
- Keep external provider payload models under `WebApplication1/Models/External/...`.
- Use the AI file `AiGuiedlines.md` as the first place an assistant should look for project context and conventions.