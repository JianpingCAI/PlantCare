namespace PlantCare.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.App.ViewModels.Base;
using System;

public partial class PlantListItemViewModel : ViewModelBase, IRecipient<StatusChangedMessage>
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string? _name = string.Empty;

    [ObservableProperty]
    private string? _species = string.Empty;

    [ObservableProperty]
    private int _age;

    [ObservableProperty]
    private string _lastWatered = string.Empty;

    [ObservableProperty]
    private string _photoPath = string.Empty;

    public PlantListItemViewModel()
    {
        WeakReferenceMessenger.Default.Register(this);
    }

    // Implement IRecipient interface
    public void Receive(StatusChangedMessage message)
    {
        if (message.PlantId == Id && message.Name == Name)
        {
            Name = "StatusChanged";
        }
    }
}