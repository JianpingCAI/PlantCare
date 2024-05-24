namespace PlantCare.App.Messaging;

internal class PlantAddedOrChangedMessage
{
    public Guid? PlantId { get; set; }
}