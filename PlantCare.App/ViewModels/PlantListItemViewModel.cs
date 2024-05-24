
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;

namespace PlantCare.App.ViewModels;
public partial class PlantListItemViewModel : PlantViewModelBase, IRecipient<StatusChangedMessage>
{
    public PlantListItemViewModel()
    {
        WeakReferenceMessenger.Default.Register(this);
    }

    void IRecipient<StatusChangedMessage>.Receive(StatusChangedMessage message)
    {
        if (message.PlantId == Id && message.Name == Name)
        {
            Name = "StatusChanged";
        }
    }
}