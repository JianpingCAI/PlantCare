﻿using CommunityToolkit.Mvvm.ComponentModel;
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
                await LoadingDataWhenViewAppearingAsync(LoadDataWhenViewAppearingAsync);
                IsLoading = false;

                await OnDataLoadedWhenViewAppearingAsync();
            });
    }

    public virtual Task OnDataLoadedWhenViewAppearingAsync()
    {
        return Task.CompletedTask;
    }

    protected async Task LoadingDataWhenViewAppearingAsync(Func<Task> workFunc)
    {
        await workFunc();
    }

    public virtual Task LoadDataWhenViewAppearingAsync() => Task.CompletedTask;

    #endregion For the loading indicator
}