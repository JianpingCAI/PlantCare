using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantCare.App.ViewModels.Base;

namespace PlantCare.App.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string password;

    public LoginViewModel()
    {
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        try
        {
            // Simulate login logic
            await Task.Delay(1000);  // Simulate a login delay

            // Navigate to the main application page or dashboard
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void NavigateRegister()
    {
        // Navigate to the registration page
    }
}