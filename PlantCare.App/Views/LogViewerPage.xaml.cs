using PlantCare.App.ViewModels;

namespace PlantCare.App.Views;

public partial class LogViewerPage : ContentPageBase
{
    public LogViewerPage(LogViewerViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}