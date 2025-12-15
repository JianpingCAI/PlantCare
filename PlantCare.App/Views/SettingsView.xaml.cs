using PlantCare.App.ViewModels;

namespace PlantCare.App.Views;

public partial class SettingsView : ContentPageBase
{
    public SettingsView(SettingsViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    private void RadioButton_CheckedChanged(System.Object sender, CheckedChangedEventArgs e)
    {
        if (!(sender is RadioButton button && button.Value != null) || App.Current is null)
        {
            return;
        }

        AppTheme theme = (AppTheme)((RadioButton)sender).Value;

        if (App.Current.UserAppTheme == theme)
        {
            return;
        }

        App.Current.UserAppTheme = theme;
    }
}