using PlantCare.App.Services;
using PlantCare.Data.DbModels;

namespace PlantCare.App.Views;

public partial class RegisterView : ContentPage
{
    private readonly IAuthService _authService;

    public RegisterView(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var user = new User
        {
            Username = usernameEntry.Text,
            Email = emailEntry.Text,
            PasswordHash = passwordEntry.Text // This should be hashed in the AuthService
        };

        try
        {
            var result = await _authService.SignUpAsync(user);
            if (result)
            {
                // Handle successful registration (e.g., navigate to login or main app)
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Registration Failed", ex.Message, "OK");
        }
    }
}