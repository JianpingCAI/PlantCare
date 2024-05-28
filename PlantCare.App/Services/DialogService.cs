namespace PlantCare.App.Services;

public class DialogService : IDialogService
{
    public Task<bool> Ask(string title, string message, string trueButtonText = "Yes", string falseButtonText = "No")
    {
        return Shell.Current.DisplayAlert(title, message, trueButtonText, falseButtonText);
    }

    public Task Notify(string title, string message, string buttonText = "OK")
    {
        return Shell.Current.DisplayAlert(title, message, buttonText);
    }
}