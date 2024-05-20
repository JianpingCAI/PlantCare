namespace PlantCare.App.Messaging;

public enum WateredStatus
{
    Unknown = 0,
    Watered,
    NotWatered
}

public class StatusChangedMessage
{
    public Guid PlantId { get; }
    public string Name { get; set; }
    public WateredStatus Status { get; }

    public StatusChangedMessage(
        Guid id,
        string name,
        WateredStatus status)
    {
        PlantId = id;
        Name = name;
        Status = status;
    }
}