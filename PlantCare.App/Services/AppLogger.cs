using Serilog;

namespace PlantCare.App.Services;

public class AppLogger<T> : IAppLogger<T>
{
    private readonly ILogger _logger;

    public AppLogger()
    {
        _logger = Log.ForContext<T>();
    }

    public void LogInformation(string message)
    {
        _logger.Information(message);
    }

    public void LogWarning(string message)
    {
        _logger.Warning(message);
    }

    public void LogError(string message, Exception exception)
    {
        _logger.Error(message, exception);
    }
}