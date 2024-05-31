namespace PlantCare.App.Messaging;

public enum WateredStatus
{
    Unknown = 0,
    Watered,
    NotWatered
}

public class StatusChangedMessage(
    Guid id,
    string name,
    WateredStatus status)
{
    public Guid PlantId { get; } = id;
    public string Name { get; set; } = name;
    public WateredStatus Status { get; } = status;
}