---
sidebar_position: 5
---

# Auditing

Every `Create` / `Update` / `Delete` against an auditable entity is automatically recorded to a dedicated `audits` collection.

> Auditing is **opt-in** in 2.0. In 1.x the `audits` collection was created on every context whether you used it or not.

## 1. Enable auditing on the options

```csharp
new MongoDbCoreOptions
{
    Connection = "mongodb://localhost:27017",
    Database   = "myapp",
    EnableAuditing = true
};
```

Or from configuration:

```jsonc
"MongoDB": {
  "Connection": "mongodb://localhost:27017",
  "Database":   "myapp",
  "EnableAuditing": true
}
```

## 2. Mark the entity with `[Auditable]`

```csharp
[Auditable]
public class Invoice : BaseEntity
{
    public string Number { get; set; } = string.Empty;
    public decimal Total  { get; set; }
}
```

Every mutation that goes through `Collection<Invoice>` is now recorded.

## 3. Inspect the audit log

`IAuditService` is registered automatically:

```csharp
public class AuditsController(IAuditService audits) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(int page = 1)
    {
        var (items, total) = await audits.GetAllAsync(pageSize: 20, page: page);
        return Ok(new { items, total });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var audit = await audits.GetAsync(id);
        return audit is null ? NotFound() : Ok(audit);
    }
}
```

## Audit record shape

| Field      | Description                                                  |
| ---------- | ------------------------------------------------------------ |
| `Id`       | ObjectId (auto)                                              |
| `ActionType` | `Add` / `Update` / `Delete` / `AddRange` / `UpdateMany` / `DeleteMany` / `DropCollection` |
| `Collection` | Collection name (e.g. `invoices`)                          |
| `OldValue` | JSON snapshot of the previous state (Update / Delete)        |
| `NewValue` | JSON snapshot of the new state (Add / Update)                |
| `User`     | `IAuditService.User`. Set per request from your auth layer.  |
| `CreatedAt` / `UpdatedAt` | Inherited from `BaseEntity` — UTC.            |

Attach the current user to every audit entry by injecting `IAuditService` early in the request and setting `audits.User = User.Identity?.Name`.
