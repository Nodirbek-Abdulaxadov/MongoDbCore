---
sidebar_position: 1
---

# Overview

MongoDbCore is a lightweight micro-ORM that makes working with MongoDB from .NET feel as natural as Entity Framework: declare a typed context, expose `Collection<T>` properties, and call `.Add` / `.ToList` / `.Update` / `.Delete` directly.

**Current version: 2.0.0** — supports .NET 8 and .NET 9.

## Key Features

- **Easy setup** — register one `MongoDbContext` derivative and you're done; conventional pluralized snake-case collection names by default (`TodoItem` → `todo_items`).
- **CRUD operations** — sync and async overloads for every operation, including bulk variants.
- **Asynchronous support** — `Task<T?>`-returning queries with proper nullable annotations and `CancellationToken` plumbing throughout.
- **Bulk operations** — `AddRange`, `UpdateMany`, `DeleteRange`, `DeleteAll`, and partial-update by property selector.
- **Relationships** — `Include` / `IncludeRef` eager-load related entities, with `[ForeignKeyTo]` and `[ReferenceTo]` attributes for the wiring.
- **Auditing (opt-in)** — mark a class with `[Auditable]` and enable it on the options to record every Create / Update / Delete to a dedicated `audits` collection.
- **`SelfCachedCollection<T>`** — in-memory snapshot collection for read-heavy lookup tables.
- **UTC timestamps** — `BaseEntity.CreatedAt` and `UpdatedAt` are written with `DateTimeOffset.UtcNow` so storage is timezone-independent.
