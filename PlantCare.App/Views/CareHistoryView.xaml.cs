using PlantCare.App.ViewModels;

namespace PlantCare.App.Views;

public partial class CareHistoryView : ContentPageBase
{
    public CareHistoryView(CareHistoryViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}