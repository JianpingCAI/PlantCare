namespace PlantCare.App.Services;

public class DialogService : IDialogService
{
    public Task<bool> Ask(string title, string message, string trueButtonText = "Yes", string falseButtonText = "No")
        => Shell.Current.DisplayAlert(title, message, trueButtonText, falseButtonText);

    public Task Notify(string title, string message, string buttonText = "OK")
        => Shell.Current.DisplayAlert(title, message, buttonText);
}