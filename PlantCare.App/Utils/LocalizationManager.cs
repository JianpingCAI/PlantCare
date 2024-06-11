using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace PlantCare.App.Utils;

public class LocalizationManager : INotifyPropertyChanged
{
    public static LocalizationManager Instance { get; } = new LocalizationManager();

    private readonly ResourceManager _resourceManager = new("PlantCare.App.Resources.LocalizationResources", typeof(App).Assembly);
    private CultureInfo _currentCulture = CultureInfo.CurrentCulture;

    public string? this[string key] => _resourceManager?.GetString(key, _currentCulture);

    public event EventHandler? LanguageChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void SetCulture(CultureInfo culture)
    {
        if (_currentCulture != culture)
        {
            _currentCulture = culture;

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            LanguageChanged?.Invoke(this, EventArgs.Empty);

            WeakReferenceMessenger.Default.Send<LanguageChangedMessage>(new LanguageChangedMessage
            {
                CultureCode = culture.Name
            });
        }
    }

    internal void SetLanguage(Language selectedLanguage)
    {
        switch (selectedLanguage)
        {
            case Language.English:
                SetCulture(new CultureInfo("en"));
                break;

            case Language.ChineseSimplified:
                SetCulture(new CultureInfo("zh-CN"));

                break;

            default:
                break;
        }
    }
}