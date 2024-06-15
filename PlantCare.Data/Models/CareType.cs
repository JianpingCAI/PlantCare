namespace PlantCare.App.ViewModels
{
    public enum CareType
    {
        Watering,
        Fertilization
    }

    public static class CareTypeExtension
    {
        public static string GetActionName(this CareType careType)
        {
            string actionName = string.Empty;
            switch (careType)
            {
                case CareType.Watering:
                    {
                        actionName = "Water";
                    }
                    break;

                case CareType.Fertilization:
                    {
                        actionName = "Fertilize";
                    }
                    break;

                default:
                    break;
            }

            return actionName;
        }
    }
}