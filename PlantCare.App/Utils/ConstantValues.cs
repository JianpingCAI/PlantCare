namespace PlantCare.App.Utils
{
    public static class ConstantValues
    {
        public const int CalendarWidth = 400;
        internal const int CalendarHeight = 380;

        public static string LogFilePath => Path.Combine(Microsoft.Maui.Storage.FileSystem.AppDataDirectory, "app.log");
    }
}