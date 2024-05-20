using PlantCare.Data.Models;

namespace PlantCare.App.Services;

public class DialogService : IDialogService
{
    public Task<Plant> OpenAddPlantDialog()
    {
        return null;
    }

    public async Task ShowAlertAsync(string title, string message, string cancel)
    {
        await Application.Current.MainPage.DisplayAlert(title, message, cancel);
    }

    public async Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel)
    {
        return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
    }
}