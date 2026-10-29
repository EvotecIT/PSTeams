namespace MessageX;

/// <summary>A short label and value supplied by the product that owns the incident.</summary>
public sealed class MessageNotificationFact {
    public string Label { get; }
    public string Value { get; }

    public MessageNotificationFact(string label, string value) {
        Label = string.IsNullOrWhiteSpace(label) ? throw new ArgumentException("A fact label is required.", nameof(label)) : label;
        Value = string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("A fact value is required.", nameof(value)) : value;
    }
}
