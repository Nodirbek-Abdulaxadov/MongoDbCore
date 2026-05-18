---
sidebar_position: 6
---

# Relationships (`Include` / `IncludeRef`)

MongoDbCore is a document store at heart, but it can resolve cross-collection references on demand — EF-style.

## Forward reference — `Include`

A parent collection eager-loads its children via `[ForeignKeyTo]` on the child:

```csharp
public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public List<Book> Books { get; set; } = [];
}

public class Book : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    [ForeignKeyTo(nameof(Category))]
    public string CategoryId { get; set; } = string.Empty;
}

// Load a category together with its books:
var category = db.Categories
    .Include(c => c.Books)
    .FirstOrDefault(c => c.Id == id);
```

## Backward reference — `IncludeRef`

A child collection eager-loads its parent via `[ReferenceTo]` on the foreign-key string:

```csharp
public class Book : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    [ReferenceTo(nameof(Category))]
    public string CategoryId { get; set; } = string.Empty;

    public Category? Category { get; set; }
}

// Load a book together with its category:
var book = db.Books
    .IncludeRef(b => b.Category)
    .FirstOrDefault(b => b.Id == id);
```

## Attributes summary

| Attribute            | Where             | Purpose                                                                 |
| -------------------- | ----------------- | ----------------------------------------------------------------------- |
| `[ForeignKeyTo(...)]` | on the child's FK | Marks the foreign-key field for `Include` to follow from parent → child. |
| `[ReferenceTo(...)]`  | on the child's FK | Tags the field as a Mongo ObjectId reference for `IncludeRef` to follow from child → parent. |
| `[IgnoreThis]`        | on any property  | Excludes the property from BSON serialization.                          |
