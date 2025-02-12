﻿public class Audit : BaseEntity
{
    public string OldValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
    public string Collection { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public ActionType ActionType { get; set; }
}