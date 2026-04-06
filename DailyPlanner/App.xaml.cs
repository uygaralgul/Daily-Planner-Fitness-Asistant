using System.Windows;
using DailyPlanner.Services;
using DailyPlanner.Views;

namespace DailyPlanner
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Global Exception Handlers
            this.DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show($"Unexpected Error (UI): {args.Exception.Message}\n\n{args.Exception.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            System.AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                if (args.ExceptionObject is System.Exception ex)
                {
                    MessageBox.Show($"Critical Error (Background): {ex.Message}\n\n{ex.StackTrace}", "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            var dataService = new DataService();
            var settings = dataService.LoadSettings();
            LanguageService.SetLanguage(settings.Language);

            // Start the main window first so it becomes the owner for notification popups
            var mainWindow = new MainWindow();
            mainWindow.Show();
            MainWindow = mainWindow;

            // Check reminders after the main window is visible
            var reminderService = new ReminderService(dataService);
            reminderService.CheckAndNotify();
        }
    }
}
