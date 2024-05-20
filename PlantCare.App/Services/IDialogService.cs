using PlantCare.Data.Models;

namespace PlantCare.App.Services;

public interface IDialogService
{
    Task<Plant> OpenAddPlantDialog();

    Task ShowAlertAsync(string title, string message, string cancel);

    Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel);
}