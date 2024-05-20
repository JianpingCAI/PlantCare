using CommunityToolkit.Mvvm.ComponentModel;

namespace PlantCare.App.ViewModels;

public abstract class BaseViewModel : ObservableObject
{
    private bool isBusy;

    public bool IsBusy
    {
        get => isBusy;
        set
        {
            if (SetProperty(ref isBusy, value))
            {
                OnPropertyChanged(nameof(IsNotBusy));
            }
        }
    }

    public bool IsNotBusy => !IsBusy;

    //[ObservableProperty]
    //[AlsoNotifyChangeFor(nameof(IsNotBusy))]
    //private bool isBusy;
}