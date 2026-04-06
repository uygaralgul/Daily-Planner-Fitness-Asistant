using System;
using System.Windows;

namespace DailyPlanner.Services
{
    public static class ThemeService
    {
        private const string LightThemeUri = "Themes/LightTheme.xaml";
        private const string DarkThemeUri  = "Themes/DarkTheme.xaml";

        public static void ApplyTheme(bool isDark)
        {
            var dicts = Application.Current.Resources.MergedDictionaries;

            // Remove old theme
            ResourceDictionary toRemove = null;
            foreach (var d in dicts)
            {
                if (d.Source != null &&
                    (d.Source.OriginalString.Contains("LightTheme") ||
                     d.Source.OriginalString.Contains("DarkTheme")))
                {
                    toRemove = d;
                    break;
                }
            }
            if (toRemove != null) dicts.Remove(toRemove);

            // Add new theme
            var uri = new Uri(isDark ? DarkThemeUri : LightThemeUri, UriKind.Relative);
            dicts.Add(new ResourceDictionary { Source = uri });
        }
    }
}
