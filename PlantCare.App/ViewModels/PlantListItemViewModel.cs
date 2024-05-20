namespace PlantCare.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using System;

public partial class PlantListItemViewModel : BaseViewModel
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
}