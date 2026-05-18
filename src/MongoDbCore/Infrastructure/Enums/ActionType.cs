namespace MongoDbCore;

/// <summary>Kinds of mutations recorded by the audit log.</summary>
public enum ActionType
{
    Add,
    Update,
    Delete,
    AddRange,
    UpdateMany,
    DeleteMany,
    DropCollection
}
