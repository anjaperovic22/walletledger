namespace WalletLedger.Domain.Entities;

public class StoredEvent
{
    public int Id { get; set; }
    public string AggregateId { get; set; } = string.Empty;
    public string AggregateType { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty; // JSON
    public int Version { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}