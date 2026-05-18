# Changelog

All notable changes to this project are documented in this file.
The format is loosely based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [2.0.0] — Unreleased

Major cleanup, modernization and bug-fix release.

### Fixed

- **Audit class name mismatch** — class was named `Audit` but referenced everywhere as `AuditEntity`; the package didn't compile.
- **`Collection.Add` id-generation logic** — previously regenerated the id when the supplied value *was* a valid `ObjectId`. Now regenerates only when the id is empty or invalid.
- **`OnInitializedAsync` was fire-and-forget** — wrapping `OnInitialized` invoked the async method without awaiting; renamed to `OnInitializedAsync` and now awaited on context initialization.
- **Double `MongoClient` allocation** — `AddMongoDbContext` created a second, unused client after the context had already opened one.
- **`IAuditService` registration was ignored** — extensions instantiated `new AuditService()` instead of resolving from DI.
- **`Collection.Get` no longer captures stack traces / writes to `Console`** — was on every single query, even in DEBUG.
- **`Collection.Select` now projects server-side** via `AsQueryable().Select(...)`. The previous implementation materialised the whole collection into memory.
- **`FirstOrDefaultAsync` return type** — now correctly `Task<T?>` instead of `Task<T>`.
- **`UpdateManyAsync` now also bumps `UpdatedAt`**, matching the synchronous variant.
- **`BaseEntity.CreatedAt` / `UpdatedAt`** — switched from `DateTime.Now` to `DateTimeOffset.UtcNow` for consistent cross-timezone storage.

### Changed

- **Auditing is opt-in.** Set `MongoDbCoreOptions.EnableAuditing = true` (default `false`). Previously the `audits` collection was created on every context regardless of usage.
- **`MaxConnectionPoolSize` is now a public `set` property**, bindable from `IConfiguration.Get<MongoDbCoreOptions>()`.
- **Multi-targets `net8.0` and `net9.0`**.
- **Bumped `MongoDB.Driver` to `2.30.0`.**
- **Dropped `Microsoft.AspNetCore.Components.Web` dependency** — replaced with the much smaller `Microsoft.Extensions.DependencyInjection.Abstractions`.
- **Bumped `BCrypt.Net` to `BCrypt.Net-Next 4.0.3`** in `MongoDbCore.Identity` (was a long-obsolete `BCrypt.Net 0.1.0` which only supported .NET Framework).
- All public types now live in the `MongoDbCore` namespace and ship XML documentation.
- Folder typos fixed: `Infrastucture` → `Infrastructure`, `Releationships` → `Relationships`.
- `IUserManager.FindByIdAsync` / `FindByNameAsync` and `IRoleManager.FindByIdAsync` / `FindByNameAsync` now correctly return `Task<T?>`.

### Added

- xUnit test project (`tests/MongoDbCore.Tests`) with Inflector / BaseEntity / Options coverage.
- GitHub Actions: `.github/workflows/ci.yml` (build + test + pack on push/PR) and `.github/workflows/publish.yml` (NuGet push on `v*` tags).
- `Directory.Build.props` and `Microsoft.SourceLink.GitHub` for deterministic, source-linked builds.
- `CHANGELOG.md`.
