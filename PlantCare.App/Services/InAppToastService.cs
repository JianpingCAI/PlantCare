using CommunityToolkit.Maui.Core;
using PlantCare.App.Components;
using System.Diagnostics;

namespace PlantCare.App.Services;

/// <summary>
/// Implementation of in-app toast service that displays toast messages using InAppToastView.
/// The service automatically creates and manages the toast view overlay.
/// </summary>
public class InAppToastService : IInAppToastService
{
    private InAppToastView? _toastView;
    private WeakReference<Page>? _currentPageRef;

    public async Task ShowInfoAsync(string message, int durationMs = 3000)
    {
        await ShowAsync(message, ToastType.Info, durationMs);
    }

    public async Task ShowSuccessAsync(string message, int durationMs = 3000)
    {
        await ShowAsync(message, ToastType.Success, durationMs);
    }

    public async Task ShowWarningAsync(string message, int durationMs = 3000)
    {
        await ShowAsync(message, ToastType.Warning, durationMs);
    }

    public async Task ShowErrorAsync(string message, int durationMs = 4000)
    {
        await ShowAsync(message, ToastType.Error, durationMs);
    }

    public async Task ShowAsync(string message, ToastType type, int durationMs = 3000)
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                EnsureToastViewExists();

                if (_toastView != null)
                {
                    Debug.WriteLine($"[InAppToast] Showing toast: {message}");
                    await _toastView.ShowAsync(message, type, durationMs);
                }
                else
                {
                    Debug.WriteLine($"[InAppToast] Toast view is null, falling back to system toast");
                    await FallbackToSystemToastAsync(message);
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[InAppToast] Error showing toast: {ex.Message}");
            await FallbackToSystemToastAsync(message);
        }
    }

    public async Task DismissAsync()
    {
        if (_toastView != null)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await _toastView.DismissAsync();
            });
        }
    }

    private void EnsureToastViewExists()
    {
        try
        {
            Page? page = GetCurrentPage();
            if (page == null)
            {
                Debug.WriteLine("[InAppToast] Current page is null");
                return;
            }

            // Check if we're on the same page
            Page? cachedPage = null;
            if (_currentPageRef?.TryGetTarget(out cachedPage) == true && cachedPage == page && _toastView != null)
            {
                // Same page, reuse existing toast view
                return;
            }

            // New page - need to create new toast view
            _toastView = null;
            _currentPageRef = new WeakReference<Page>(page);

            // Find the root Grid layout
            Grid? rootGrid = FindRootGrid(page);
            if (rootGrid == null)
            {
                Debug.WriteLine("[InAppToast] Could not find root Grid in page");
                return;
            }

            // Check if toast view already exists in this layout
            _toastView = FindExistingToastView(rootGrid);
            
            if (_toastView == null)
            {
                Debug.WriteLine("[InAppToast] Creating new toast view");
                
                // Create and add toast view to the root grid
                _toastView = new InAppToastView
                {
                    VerticalOptions = LayoutOptions.End,
                    HorizontalOptions = LayoutOptions.Fill,
                    Margin = new Thickness(0, 0, 0, 16),
                    ZIndex = 10000
                };

                // If the grid has row definitions, span all rows
                if (rootGrid.RowDefinitions.Count > 0)
                {
                    Grid.SetRowSpan(_toastView, rootGrid.RowDefinitions.Count);
                }

                rootGrid.Children.Add(_toastView);
                Debug.WriteLine("[InAppToast] Toast view added to grid");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[InAppToast] Error ensuring toast view exists: {ex.Message}");
            _toastView = null;
        }
    }

    private static Grid? FindRootGrid(Page page)
    {
        // For ContentPage, get the Content
        if (page is ContentPage contentPage)
        {
            if (contentPage.Content is Grid grid)
            {
                return grid;
            }
            
            // If content is wrapped in another layout, try to find a Grid
            if (contentPage.Content is Layout layout)
            {
                Grid? childGrid = layout.Children.OfType<Grid>().FirstOrDefault();
                if (childGrid != null)
                    return childGrid;
            }
        }

        return null;
    }

    private static InAppToastView? FindExistingToastView(Layout layout)
    {
        // Direct children
        InAppToastView? toastView = layout.Children.OfType<InAppToastView>().FirstOrDefault();
        if (toastView != null)
            return toastView;

        // Check nested layouts (one level deep)
        foreach (IView? child in layout.Children)
        {
            if (child is Layout childLayout)
            {
                toastView = childLayout.Children.OfType<InAppToastView>().FirstOrDefault();
                if (toastView != null)
                    return toastView;
            }
        }

        return null;
    }

    private static Page? GetCurrentPage()
    {
        try
        {
            if (Application.Current?.Windows?.FirstOrDefault()?.Page is Shell shell)
            {
                return shell.CurrentPage;
            }

            return Application.Current?.Windows?.FirstOrDefault()?.Page;
        }
        catch
        {
            return null;
        }
    }

    private static async Task FallbackToSystemToastAsync(string message)
    {
        try
        {
            IToast toast = CommunityToolkit.Maui.Alerts.Toast.Make(message, CommunityToolkit.Maui.Core.ToastDuration.Short);
            await toast.Show();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[InAppToast] Fallback toast also failed: {ex.Message}");
        }
    }
}
