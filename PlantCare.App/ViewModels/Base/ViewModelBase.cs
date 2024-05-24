using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PlantCare.App.ViewModels.Base;

public abstract partial class ViewModelBase : ObservableValidator/*ObservableObject*/, IViewModelBase
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isLoading;

    public bool IsNotBusy => !IsBusy;

    #region For the loading indicator

    public IAsyncRelayCommand InitializeAsyncCommand { get; }

    public ViewModelBase()
    {
        InitializeAsyncCommand = new AsyncRelayCommand(
            async () =>
            {
                IsLoading = true;
                await LoadingDataWhenViewAppearing(LoadDataWhenViewAppearingAsync);
                IsLoading = false;
            });
    }

    protected async Task LoadingDataWhenViewAppearing(Func<Task> workFunc)
    {
        await workFunc();
    }

    public virtual Task LoadDataWhenViewAppearingAsync() => Task.CompletedTask;

    #endregion For the loading indicator
}