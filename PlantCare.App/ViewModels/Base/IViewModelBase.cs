using CommunityToolkit.Mvvm.Input;

namespace PlantCare.App.ViewModels.Base;

public interface IViewModelBase
{
    IAsyncRelayCommand OnViewAppearingCommand { get; }
}