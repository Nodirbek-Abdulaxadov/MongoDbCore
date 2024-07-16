﻿namespace WebApplication1.Data;

public class ClassC : BaseEntity
{
    public int Number { get; set; }
    public string Address { get; set; } = string.Empty;
    [ForeignKey(Entity = "ClassA")]
    public string ClassAId { get; set; } = string.Empty;
}