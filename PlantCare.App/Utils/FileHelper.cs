using PlantCare.Data;

namespace PlantCare.App.Utils
{
    public static class FileHelper
    {
        public static void DeleteDatabaseFile()
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, Consts.DatabaseFileName);
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }
}