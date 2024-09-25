namespace MongoDbCore.Helpers;

public struct Datetime
{
    private readonly DateTime _dateTime;

    // Public properties for Year, Month, and Day
    [BsonElement("Year")]
    public int Year => _dateTime.Year;

    [BsonElement("Month")]
    public int Month => _dateTime.Month;

    [BsonElement("Day")]
    public int Day => _dateTime.Day;

    // Hour, Minute, and Second can be optional
    [BsonIgnoreIfDefault]
    public int Hour => _dateTime.Hour;

    [BsonIgnoreIfDefault]
    public int Minute => _dateTime.Minute;

    [BsonIgnoreIfDefault]
    public int Second => _dateTime.Second;

    // Constructor
    public Datetime(DateTime dateTime)
    {
        _dateTime = dateTime;
    }

    public Datetime(DateTimeOffset dateTimeOffset)
    {
        _dateTime = dateTimeOffset.DateTime;
    }

    // Implicit conversions
    public static implicit operator Datetime(DateTime dateTime) => new Datetime(dateTime);
    public static implicit operator Datetime(DateTimeOffset dateTimeOffset) => new Datetime(dateTimeOffset);
    public static implicit operator DateTime(Datetime datetime) => datetime._dateTime;

    // Override ToString to show formatted date
    public override string ToString() => $"{Year}-{Month:D2}-{Day:D2}";

    // Custom serialization to MongoDB as an embedded document
    public BsonDocument ToBsonDocument()
    {
        return new BsonDocument
        {
            { "Year", Year },
            { "Month", Month },
            { "Day", Day },
            { "Hour", Hour },
            { "Minute", Minute },
            { "Second", Second }
        };
    }

    // Equals and GetHashCode
    public override bool Equals(object? obj) => obj is Datetime datetime && _dateTime.Equals(datetime._dateTime);
    public override int GetHashCode() => _dateTime.GetHashCode();
}