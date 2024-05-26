using PlantCare.App.ViewModels;

namespace PlantCare.App.Views;

public partial class SettingsView : ContentPage
{
    public SettingsView(SettingsViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    private void RadioButton_CheckedChanged(System.Object sender, CheckedChangedEventArgs e)
    {
        if (!(sender is RadioButton button && button.Value != null))
            return;

        AppTheme val = (AppTheme)((RadioButton)sender).Value;

        if (App.Current.UserAppTheme == val)
            return;

        App.Current.UserAppTheme = val;
    }
}