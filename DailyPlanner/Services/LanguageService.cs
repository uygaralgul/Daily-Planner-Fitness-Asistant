using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Linq;

namespace DailyPlanner.Services
{
    public static class LanguageService
    {
        private static ResourceDictionary _currentLanguageDictionary;
        public static event Action LanguageChanged;
        public static string CurrentLanguage { get; private set; } = "tr";

        public static CultureInfo CurrentCulture => CurrentLanguage == "en" 
            ? new CultureInfo("en-US") 
            : new CultureInfo("tr-TR");

        public static void SetLanguage(string languageCode)
        {
            CurrentLanguage = languageCode;
            
            // Set thread culture so DateTime.ToString() and others use the correct language
            var culture = CurrentCulture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            var app = Application.Current;
            if (app == null) return;

            // Remove old language dictionary if it exists
            if (_currentLanguageDictionary != null)
            {
                app.Resources.MergedDictionaries.Remove(_currentLanguageDictionary);
            }

            // Relative URIs to locate resources in WPF
            string dictPath = languageCode == "en" 
                ? "Resources/Languages/English.xaml" 
                : "Resources/Languages/Turkish.xaml";

            // Load and add new dictionary
            try
            {
                _currentLanguageDictionary = new ResourceDictionary
                {
                    Source = new Uri(dictPath, UriKind.Relative)
                };
                app.Resources.MergedDictionaries.Add(_currentLanguageDictionary);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Language file load error: {ex.Message}", "Language Error", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Error loading language dictionary: {ex.Message}");
                return;
            }

            // Update Language property on all open windows so WPF DatePicker etc. use correct locale
            try
            {
                var xmlLang = XmlLanguage.GetLanguage(culture.IetfLanguageTag);
                var windows = app.Windows.Cast<Window>().ToArray();
                foreach (var w in windows)
                    w.Language = xmlLang;
            }
            catch { /* Ignore if no windows yet */ }

            // Notify subscribers (ViewModels, Windows) that language changed
            LanguageChanged?.Invoke();
        }
    }
}
