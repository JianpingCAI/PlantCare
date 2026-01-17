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

    public IAsyncRelayCommand OnViewAppearingCommand { get; }

    public ViewModelBase()
    {
        OnViewAppearingCommand = new AsyncRelayCommand(
            async () =>
            {
                IsLoading = true;
                try
                {
                    await LoadDataWhenViewAppearingAsync();
                }
                catch (Exception)
                {
                }
                finally
                {
                    IsLoading = false;
                }
            });
    }

    public virtual Task LoadDataWhenViewAppearingAsync() => Task.CompletedTask;

    #endregion For the loading indicator

    #region Thread-Safe Property Updates

    /// <summary>
    /// Sets any property safely, dispatching to the main thread if necessary.
    /// Used for properties that trigger UI updates.
    /// </summary>
    protected async Task SetPropertyAsync<T>(Action<T> setter, T value)
    {
        if (MainThread.IsMainThread)
        {
            setter(value);
        }
        else
        {
            await MainThread.InvokeOnMainThreadAsync(() => setter(value));
        }
    }

    /// <summary>
    /// Sets the IsLoading property safely, dispatching to the main thread if necessary.
    /// Used for loading indicator state management.
    /// </summary>
    protected async Task SetLoadingStateAsync(bool isLoading)
    {
        await SetPropertyAsync(v => IsLoading = v, isLoading);
    }

    /// <summary>
    /// Sets the IsBusy property safely, dispatching to the main thread if necessary.
    /// Used for button and user interaction disable/enable state management.
    /// </summary>
    protected async Task SetBusyStateAsync(bool isBusy)
    {
        await SetPropertyAsync(v => IsBusy = v, isBusy);
    }

    #endregion Thread-Safe Property Updates
}
