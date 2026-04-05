using System;
using System.Windows;

namespace DailyPlanner.Services
{
    public static class LanguageService
    {
        private static ResourceDictionary _currentLanguageDictionary;
        public static string CurrentLanguage { get; private set; } = "tr";

        public static void SetLanguage(string languageCode)
        {
            CurrentLanguage = languageCode;
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
                MessageBox.Show($"Dil dosyası yüklenirken hata oluştu: {ex.Message}", "Dil Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Error loading language dictionary: {ex.Message}");
            }
        }
    }
}
