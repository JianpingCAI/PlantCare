namespace PlantCare.App.Messaging;

internal class PlantDeletedMessage
{
    public PlantDeletedMessage(Guid id, string name)
    {
        PlantId = id;
        Name = name;
    }

    public Guid PlantId { get; }
    public string Name { get; }
}