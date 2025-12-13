using System.Diagnostics;

namespace PlantCare.App.Utils;

/// <summary>
/// Helper class for handling async operations in event handlers safely
/// </summary>
public static class AsyncEventHelper
{
    /// <summary>
    /// Executes an async task from an event handler and handles exceptions
    /// </summary>
    /// <param name="task">The async task to execute</param>
    /// <param name="onException">Optional exception handler</param>
    public static async void FireAndForget(
        Func<Task> task,
        Action<Exception>? onException = null)
    {
        try
        {
            await task();
        }
        catch (Exception ex)
        {
            if (onException != null)
            {
                onException(ex);
            }
            else
            {
                // Default: log to debug console
                Debug.WriteLine($"Unhandled exception in fire-and-forget task: {ex}");
                
#if DEBUG
                // In debug mode, rethrow to help catch issues during development
                throw;
#endif
            }
        }
    }

    /// <summary>
    /// Executes an async task with a return value from an event handler
    /// </summary>
    /// <typeparam name="T">The return type</typeparam>
    /// <param name="task">The async task to execute</param>
    /// <param name="onException">Optional exception handler</param>
    /// <param name="defaultValue">Default value to return on exception</param>
    public static async void FireAndForget<T>(
        Func<Task<T>> task,
        Action<Exception>? onException = null,
        T? defaultValue = default)
    {
        try
        {
            await task();
        }
        catch (Exception ex)
        {
            if (onException != null)
            {
                onException(ex);
            }
            else
            {
                Debug.WriteLine($"Unhandled exception in fire-and-forget task: {ex}");
                
#if DEBUG
                throw;
#endif
            }
        }
    }

    /// <summary>
    /// Wrapper for collection changed events that need async handling
    /// </summary>
    /// <param name="asyncHandler">The async handler</param>
    /// <param name="onException">Optional exception handler</param>
    /// <returns>A synchronous event handler that safely executes the async code</returns>
    public static System.Collections.Specialized.NotifyCollectionChangedEventHandler WrapAsync(
        Func<object?, System.Collections.Specialized.NotifyCollectionChangedEventArgs, Task> asyncHandler,
        Action<Exception>? onException = null)
    {
        return (sender, e) =>
        {
            FireAndForget(
                () => asyncHandler(sender, e),
                onException);
        };
    }
}

/// <summary>
/// Extension methods for Task to help with async patterns
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Fire and forget extension for Task - use sparingly and with caution
    /// </summary>
    /// <param name="task">The task to fire and forget</param>
    /// <param name="onException">Optional exception handler</param>
    public static void SafeFireAndForget(
        this Task task,
        Action<Exception>? onException = null)
    {
        AsyncEventHelper.FireAndForget(
            () => task,
            onException);
    }

    /// <summary>
    /// Runs a task synchronously with timeout - useful for transitioning async code
    /// </summary>
    /// <param name="task">The task to run</param>
    /// <param name="timeoutMs">Timeout in milliseconds (default: 30 seconds)</param>
    /// <returns>The task result</returns>
    public static T RunSynchronously<T>(this Task<T> task, int timeoutMs = 30000)
    {
        if (task.Wait(timeoutMs))
        {
            return task.Result;
        }
        throw new TimeoutException($"Task did not complete within {timeoutMs}ms");
    }

    /// <summary>
    /// Runs a task synchronously with timeout - useful for transitioning async code
    /// </summary>
    /// <param name="task">The task to run</param>
    /// <param name="timeoutMs">Timeout in milliseconds (default: 30 seconds)</param>
    public static void RunSynchronously(this Task task, int timeoutMs = 30000)
    {
        if (!task.Wait(timeoutMs))
        {
            throw new TimeoutException($"Task did not complete within {timeoutMs}ms");
        }
    }
}

/// <summary>
/// Cancellation token helper for operations that may take time
/// </summary>
public static class CancellationHelper
{
    /// <summary>
    /// Creates a linked token source with a timeout
    /// </summary>
    /// <param name="timeoutMs">Timeout in milliseconds</param>
    /// <param name="linkedTokens">Additional tokens to link</param>
    /// <returns>A new CancellationTokenSource</returns>
    public static CancellationTokenSource CreateWithTimeout(
        int timeoutMs,
        params CancellationToken[] linkedTokens)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(linkedTokens);
        cts.CancelAfter(timeoutMs);
        return cts;
    }
}
