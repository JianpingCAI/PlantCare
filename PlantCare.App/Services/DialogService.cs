namespace PlantCare.App.Services;

public class DialogService : IDialogService
{
    public async Task<bool> Ask(string title, string message, string trueButtonText = "Yes", string falseButtonText = "No")
    {
        var current = Shell.Current;
        return current != null ? await current.DisplayAlert(title, message, trueButtonText, falseButtonText) : false;
    }

    public async Task Notify(string title, string message, string buttonText = "OK")
    {
        var current = Shell.Current;
        if (current != null)
        {
            await current.DisplayAlert(title, message, buttonText);
        }
    }
}
