using PlantCare.App.Services;

namespace PlantCare.App.Views;

public partial class LoginView : ContentPage
{
    private readonly IAuthService _authService;

    public LoginView(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = emailEntry.Text;
        var password = passwordEntry.Text;

        try
        {
            var user = await _authService.LoginAsync(email, password);
            // Navigate to dashboard or main app page
        }
        catch (Exception ex)
        {
            // Handle errors (e.g., show an alert)
            await DisplayAlert("Login Failed", ex.Message, "OK");
        }
    }

    private void OnNavigateToRegisterClicked(object sender, EventArgs e)
    {
        // Navigate to registration page
    }
}