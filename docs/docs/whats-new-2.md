---
sidebar_position: 2
---

# What's new in 2.0

A focused cleanup release. Highlights:

## The library finally compiles from a clean restore

The `Audit` class was referenced everywhere as `AuditEntity`, but declared as `Audit`. A fresh `dotnet restore` of 1.x failed with seven CS0246 errors. Fixed.

## Auditing is opt-in

Set `MongoDbCoreOptions.EnableAuditing = true` (off by default). In 1.x the `audits` collection was created on every context whether you used it or not. See the dedicated [Auditing](./auditing.md) page.

## Correctness fixes

- **`Collection.Add` id generation was inverted** — it regenerated the id when the supplied value *was* a valid `ObjectId`. Now it regenerates only when the id is empty or malformed.
- **`OnInitializedAsync` is now awaited**. Previously the wrapper called the async method fire-and-forget, so any seeding in there was racing the rest of startup.
- **`UpdateManyAsync` now bumps `UpdatedAt`** (the sync variant always did).
- **`FirstOrDefaultAsync` returns `Task<T?>`** — the type now matches the runtime behaviour.
- **`Collection.Select` projects server-side** via `IQueryable`. The 1.x implementation materialised the entire collection client-side.
- **`Collection.Get` no longer captures a stack trace** and writes to `Console` on every single query (it did in DEBUG builds).

## Modernization

- Multi-targets `net8.0` and `net9.0`.
- `MongoDB.Driver` bumped to `2.30.0`.
- **Dropped the `Microsoft.AspNetCore.Components.Web` dependency** — replaced with the much smaller `Microsoft.Extensions.DependencyInjection.Abstractions`. There's no reason an ORM should pull in Blazor.
- `BaseEntity.CreatedAt` / `UpdatedAt` use `DateTimeOffset.UtcNow`. 1.x wrote `DateTime.Now` (local time) into a `DateTimeOffset` field — lossy if your servers and clients aren't in the same timezone.
- `MaxConnectionPoolSize` is now a public `set` property — it binds correctly from `IConfiguration.Get<MongoDbCoreOptions>()`.
- Folder typos fixed: `Infrastucture` → `Infrastructure`, `Releationships` → `Relationships`.
- Public types live in `namespace MongoDbCore` and ship XML documentation.

## `MongoDbCore.Identity`

- Multi-targets `net8.0;net9.0`.
- `BCrypt.Net 0.1.0` → **`BCrypt.Net-Next 4.0.3`** (the old package was .NET Framework only).
- `IUserManager` / `IRoleManager` `FindBy*Async` methods now return `Task<T?>`.

## Tooling

- xUnit test project at `tests/MongoDbCore.Tests`.
- GitHub Actions: `ci.yml` (build + test + pack on push/PR) and `publish.yml` (NuGet push on `v*` tag).
- `Directory.Build.props` + `Microsoft.SourceLink.GitHub` for deterministic, source-linked builds and a symbol package.

Read the full [CHANGELOG](https://github.com/Nodirbek-Abdulaxadov/MongoDbCore/blob/master/CHANGELOG.md) on GitHub.
