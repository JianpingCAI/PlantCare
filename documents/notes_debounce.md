```CSharp

   private readonly object _lock = new object();
    private CancellationTokenSource _cts = new CancellationTokenSource();

    private string _searchText;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                // Debounced search method is called when the SearchText property changes
                if (value.Length == 0)
                {
                    DebounceResetSearch();
                }
            }
        }
    }

    private void DebounceResetSearch()
    {
        lock (_lock)
        {
            _cts.Cancel(); // Cancel the previous task
            _cts.Dispose(); // Dispose the previous token source
            _cts = new CancellationTokenSource(); // Create a new token source
        }

        // Run the search with the new token
        SearchTextChangedCommand.Execute(null);
    }

    [RelayCommand]
    private async Task SearchTextChanged()
    {
        CancellationTokenSource localCts;
        lock (_lock)
        {
            localCts = _cts;
        }

        try
        {
            IsLoading = true;

            // Await with the local token
            await Task.Delay(500, localCts.Token);

            // Check if the local token has had cancellation requested
            if (localCts.Token.IsCancellationRequested)
            {
                return;
            }

            // Perform the reset action
            await ResetPlantsAsync();
        }
        catch (TaskCanceledException)
        {
            // This catch is expected to be empty as it's normal for tasks to be cancelled
            Debug.WriteLine("is cancelled");
        }
        catch (Exception ex)
        {
            // Handle other exceptions, e.g., show an error message
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
            });
        }
        finally
        {
            // Ensure IsLoading is updated on the UI thread
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                IsLoading = false;
            });
        }
    }

```