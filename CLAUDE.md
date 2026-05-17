# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SpeedRunnersLab is a fan website for the Steam game [SpeedRunners](https://store.steampowered.com/app/207140/SpeedRunners) (AppId `207140`). It shows live game stats, ladder rankings, player profiles, news and hosts community MODs. Production domain: `speedrunners.cn` (UI) / `api.speedrunners.cn` (API).

The repository contains three independently deployable services plus a MySQL container, orchestrated by `docker-compose.yml`:

| Service               | Stack                              | Purpose                                                    |
| --------------------- | ---------------------------------- | ---------------------------------------------------------- |
| `SpeedRunners.API`    | ASP.NET Core 3.1 (`netcoreapp3.1`) | REST API. Multi-project solution (Web/BLL/DAL/Model/Utils).|
| `SpeedRunners.UI`     | Vue 2.6 + Vuetify 2.3 + vuex       | SPA frontend, based on `vue-admin-template`.               |
| `SpeedRunners.Scheduler` | .NET Core 3.1 console app + FluentScheduler | Background jobs (rank updates, log snapshots).         |

## Common Commands

### Backend API (`SpeedRunners.API/`)
```powershell
# Restore + build the full solution
dotnet build SpeedRunners.API/SpeedRunners.API.sln

# Run the Web host (default dev URL is http://localhost:10340 via IIS Express,
# or https://localhost:5001 / http://localhost:5000 via Kestrel — see launchSettings.json)
dotnet run --project SpeedRunners.API/SpeedRunners/SpeedRunners.csproj

# Publish for Docker (Dockerfile copies ./publish)
dotnet publish SpeedRunners.API/SpeedRunners/SpeedRunners.csproj -c Release -o SpeedRunners.API/publish
```

### Scheduler (`SpeedRunners.Scheduler/`)
```powershell
dotnet build  SpeedRunners.Scheduler/SpeedRunners.Scheduler.csproj
dotnet run    --project SpeedRunners.Scheduler/SpeedRunners.Scheduler.csproj
```

### Frontend (`SpeedRunners.UI/`) — uses **yarn**, not npm
```powershell
yarn install
yarn dev                # vue-cli serve, opens browser, --openssl-legacy-provider required
yarn lint               # eslint src/**/*.{js,vue}
yarn build:prod         # production build → dist/
yarn build:stage        # staging build
yarn test:unit          # jest (clears cache then runs)
yarn test:unit -- path/to/file.spec.js   # single test file
```
Vue CLI uses `--openssl-legacy-provider` because the toolchain is on Node 16+ but Webpack 4. Don't drop that flag.

### Whole stack
```powershell
docker-compose up -d    # mysql + api + ui (nginx) + scheduler
```
Before bringing up the stack you must (a) populate `SpeedRunners.API/publish/` and `SpeedRunners.UI/dist/`, and (b) provide a real `appsettings.Production.json` — these are `.gitignore`d.

## Architecture

### API request lifecycle
```
HTTP request
  └─► SRLabTokenAuthMidd (Middleware/SRLabTokenAuthMidd.cs)
        ├── reads `srlab-token` header
        ├── inspects endpoint for [User] / [Persona] attributes
        ├── populates scoped MUser (the per-request "current user" service)
        └── short-circuits with MResponse.Fail("not_login") on auth failure
  └─► Controller : BaseController<TBLL>
        └── resolves TBLL via DI, attaches CurrentUser + HttpContext + IStringLocalizer<TBLL>
  └─► BLL : BLLHelper<TDAL>
        └── BeginDb(dal => ...) opens a MySqlConnection, wraps it in DbHelper, instantiates TDAL via Activator
  └─► DAL : DALBase  (Dapper, raw SQL with `?paramName` placeholders)
  └─► ResponseFilter (Filter/ResponseFilter.cs)
        ├── wraps any non-MResponse return value into MResponse via `.Success()` extension
        ├── refreshes the access token (rotates after `Refresh` minutes, returns new value in body)
        └── final shape: { code, message, token, data }
```

**Response code convention**: `code == 666` is success. The frontend (`src/utils/request.js`) treats anything else as an error and toasts `message`. `50008/50012/50014` triggers an automatic re-login.

**Auth attributes** (`SpeedRunners.Model/`):
- `[User]` — request rejected if no/invalid token.
- `[Persona]` — token optional. Anonymous gets public data; logged-in users get personalized data layered in.
- No attribute — endpoint is fully public.

**DI auto-registration**: `Service/ServiceHelper.cs::AddAllBLL` scans the `SpeedRunners.BLL` assembly and registers every concrete `BaseBLL` subclass as **scoped**. To add a new BLL, just create the class — no Startup edit needed.

### Project layout inside `SpeedRunners.API/`
- `SpeedRunners/` — ASP.NET Core entry (`Program.cs`, `Startup.cs`, `Controllers/`, `Middleware/`, `Filter/`, `Service/`, `Resources/`).
- `SpeedRunners.BLL/` — Business logic. One BLL per domain (User, Rank, Steam, Profile, Comment, Notification, Asset). Each has matching `.resx` files in `BLL/Resources/` for localization.
- `SpeedRunners.DAL/` — Dapper-backed repositories. Always parameterize queries (`Db.Execute($"... ?{nameof(x)}", new { x })`).
- `SpeedRunners.Model/` — POCOs grouped by domain (`User/`, `Rank/`, `Steam/`, etc.) plus the response envelope (`MResponse`) and the auth attributes (`UserAttribute`, `PersonaAttribute`).
- `SpeedRunners.Utils/` — `BLLHelper<TDAL>`, `DALBase`, `DbHelper` (Dapper wrapper with transaction support), `AppSettings`, `HttpHelper` (with optional proxy from `appsettings`), `CommonUtils` (token generator), `SignatureHelper`.

### Login flow
`UserBLL.Login` performs Steam OpenID validation against `https://steamcommunity.com/openid/login`. On success it generates a token (`CommonUtils.CreateToken` — format `<random>&<timestamp>`) and stores an `AccessToken` row. The `ResponseFilter` then rotates the token after `Refresh` (default 5) minutes; clients always read the latest token from the `token` field in every response and overwrite their cookie.

### Localization
- **API**: `services.AddLocalization(options => options.ResourcesPath = "Resources")`. Each BLL/Middleware class has sibling `.resx` files (e.g. `UserBLL.zh.resx`, `UserBLL.en.resx`). Culture is chosen by the `locale` header via `LocaleHeaderRequestCultureProvider` — only `en` and `zh` are wired in `MiddlewareHelpers.UseHeaderRequestLocalization`, but resx files exist for many more languages.
- **UI**: `vue-i18n` with 19 language JSONs in `src/i18n/lang/`. Browser language → supported code via `langMap` in `src/i18n/index.js`; user choice persisted in `localStorage.lang`. Route titles localize through `i18n.t('routes.' + route.meta.title)`.

### Frontend architecture
- Built on **vue-admin-template** but Element UI was replaced with **Vuetify**. Toast notifications use `vuetify-toast-snackbar-ng` exposed as `Vue.prototype.$toast`.
- `src/permission.js` is the router guard. On first navigation it calls `isInChina()` and decides which route set to mount: users outside China or logged in get the `asyncRoutes` (e.g. `/plaza`); users inside China without a token don't.
- `src/utils/request.js` is the axios singleton. Every request attaches `srlab-token` + `locale`; every response auto-updates the token via `setToken`/`removeToken`.
- Vuex modules are auto-registered from `src/store/modules/*.js` (`store/index.js` uses `require.context`).
- Notifications are polled every 30s by `store/modules/notification.js::startPolling`.
- **Cache-busting on deploy**: `build/version-create.js` runs at the top of `vue.config.js` and writes a timestamp into `public/verify.json`. The frontend (`src/utils/version.js`) polls that file after route changes and on `visibilitychange`; when the timestamp differs it forces a reload with a `_v=` cache-buster. Production builds also use `[contenthash]` filenames (only in non-development builds — see `chainWebpack` block in `vue.config.js`).

### Scheduler jobs (`SpeedRunners.Scheduler/Task.cs`)
FluentScheduler `Registry`:
- `UpdateScore` — every 10 minutes (live ladder pull).
- `UpdateOldScore` — daily at 18:00 (snapshot previous scores).
- `UpdatePlayTime` — daily at 05:00 (Steam playtime sync).
- `InsertRankLog` — daily at 17:30 (writes `RankLog` rows where score changed).
- `UpdateSteamState` — runs in an infinite loop in `Program.cs` (continuous online-state polling).

The scheduler talks to the same MySQL via its own `DbHelper.cs` (separate from the API's `DbHelper`) and reads config from `App.config`.

## Configuration

`appsettings.Development.json` / `appsettings.Production.json` are git-ignored. `appsettings.json` is a placeholder showing the required keys:
- `ConnectionString` — MySQL.
- `ApiKey` — Steam Web API key.
- `AccessKey` / `SecretKey` — Qiniu storage (used by `AssetBLL` for MOD file upload/download).
- `Refresh` — minutes before the access token is rotated.
- `AfdianUserID` / `AfdianToken` — sponsor list API.
- `Proxy.Enable` / `Proxy.Address` — outbound proxy for `HttpHelper` (needed in CN to reach Steam from the API host).

Frontend dev API URL: `http://localhost:10340/api` (`.env.development`). Production: `//api.speedrunners.cn/api`.

## Conventions

- **DAL SQL**: Use Dapper with named parameters; the `?` prefix (`?{nameof(x)}`) is the MySqlConnector binding style used throughout — keep it consistent.
- **Adding an endpoint**: create or extend a `*BLL` in `SpeedRunners.BLL` (it gets auto-registered), expose it on a controller via `BaseController<TBLL>`, decorate with `[User]` / `[Persona]` when auth is needed, return a plain DTO or `MResponse` — the `ResponseFilter` will wrap it.
- **Adding a frontend page**: drop a `.vue` in `src/views/<feature>/`, register the route in `src/router/index.js` (use `meta.title` matching a key under `routes.*` in every JSON in `src/i18n/lang/`), then call backend via a thin wrapper in `src/api/<feature>.js`.
- **Localization keys for new BLL strings**: add the same key to every `<BLL>.<lang>.resx` in `SpeedRunners.BLL/Resources/`. The C# code reads them via `Localizer["key"]`.
- **Nested `SpeedRunnersLab/` folder at repo root**: a leftover empty directory, not a real project. Ignore it.
