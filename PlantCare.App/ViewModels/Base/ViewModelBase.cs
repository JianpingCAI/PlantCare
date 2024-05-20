using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PlantCare.App.ViewModels.Base;

public abstract partial class ViewModelBase : ObservableObject, IViewModelBase
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
                await Loading(LoadAsync);
                IsLoading = false;
            });
    }

    protected async Task Loading(Func<Task> workFunc)
    {
        await workFunc();
    }

    public virtual Task LoadAsync() => Task.CompletedTask;

    #endregion For the loading indicator
}