using CommunityToolkit.Mvvm.ComponentModel;
using PlantCare.App.Services;
using PlantCare.App.Utils;
using PlantCare.App.ViewModels.Base;
using PlantCare.Data;

namespace PlantCare.App.ViewModels
{
    public partial class LogViewerViewModel : ViewModelBase
    {
        public LogViewerViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        [ObservableProperty]
        private string _logText = string.Empty;

        private readonly IDialogService _dialogService;

        public override async Task LoadDataWhenViewAppearingAsync()
        {
            try
            {
                LogText = string.Empty;
                await LoadLogs();
            }
            catch (Exception ex)
            {
                LogText = string.Empty;
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
            }
        }

        public async Task LoadLogs()
        {
            try
            {
                string logDirectory = FileSystem.AppDataDirectory;
                string logFilePattern = "app*.log";

                var logFiles = Directory.GetFiles(logDirectory, logFilePattern)
                                        .OrderByDescending(f => f)
                                        .ToList();

                if (logFiles.Any())
                {
                    string latestLogFile = logFiles.First();

                    await ReadLogFileWithRetry(latestLogFile);

                    //using var streamReader = new StreamReader(latestLogFile);
                    //string logContent = await streamReader.ReadToEndAsync();
                    //LogText = logContent;
                }
                else
                {
                    LogText = "No logs available.";
                }
            }
            catch (Exception ex)
            {
                LogText = $"Error loading logs: {ex.Message}";
            }
        }

        /// <summary>
        /// A retry mechanism for reading the log file. 
        /// This will help handle scenarios where the file is temporarily locked by Serilog.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="retryCount"></param>
        /// <param name="delayMilliseconds"></param>
        /// <returns></returns>
        private async Task ReadLogFileWithRetry(string filePath, int retryCount = 5, int delayMilliseconds = 200)
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    //using var streamReader = new StreamReader(filePath);
                    //string logContent = await streamReader.ReadToEndAsync();
                    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var streamReader = new StreamReader(fileStream);
                    string logContent = await streamReader.ReadToEndAsync();
                    LogText = logContent;
                    return;
                }
                catch (IOException)
                {
                    if (i == retryCount - 1)
                    {
                        throw;
                    }
                    await Task.Delay(delayMilliseconds);
                }
            }
        }
    }
}