using PlantCare.App.ViewModels;

namespace PlantCare.App.Messaging;

internal class PlantCareHistoryChangedMessage(Guid plantId, CareType careType)
{
    public Guid PlantId { get; } = plantId;
    public CareType CareType { get; } = careType;
}