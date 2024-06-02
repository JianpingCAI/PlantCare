namespace PlantCare.App.Utils
{
    public static class PlantState
    {
        public static double GetCurrentStateValue(DateTime scheduledTime)
        {
            return Math.Min(1.0, Math.Max(0, (scheduledTime - DateTime.Now).TotalMinutes / (10080/*7 * 24 * 60*/)));
        }
    }
}
