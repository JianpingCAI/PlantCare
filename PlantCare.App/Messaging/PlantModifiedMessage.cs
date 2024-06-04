namespace PlantCare.App.Messaging;

internal class PlantModifiedMessage(Guid id)
{
    public Guid PlantId { get; set; } = id;
}

internal class PlantAddedMessage(Guid id)
{
    public Guid PlantId { get; set; } = id;
}