using System.ComponentModel;

namespace PlantCare.App.Utils;

public enum Language
{
    [Description("English")]
    English,

    [Description("中文(简)")]
    ChineseSimplified
}

public static class LanguageProvider
{
    public static List<Language> All => Enum.GetValues(typeof(Language)).Cast<Language>().ToList();
}
