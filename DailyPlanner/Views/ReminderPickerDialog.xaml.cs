using System;
using System.Windows;

namespace DailyPlanner.Views
{
    public partial class ReminderPickerDialog : Window
    {
        /// <summary>Seçilen tarih. null = hatırlatıcı kaldırıldı. Confirmed=false ise dialog iptal edildi.</summary>
        public DateTime? SelectedDate { get; private set; }
        public bool Confirmed { get; private set; }

        public ReminderPickerDialog(DateTime? current = null)
        {
            InitializeComponent();
            if (current.HasValue && current.Value.Date >= DateTime.Today)
            {
                ReminderCalendar.SelectedDate = current.Value.Date;
                ReminderCalendar.DisplayDate  = current.Value.Date;
            }
            else
            {
                ReminderCalendar.SelectedDate = DateTime.Today;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (ReminderCalendar.SelectedDate == null)
            {
                bool isEn = DailyPlanner.Services.LanguageService.CurrentLanguage == "en";
                MessageBox.Show(isEn ? "Please select a date." : "Lütfen bir tarih seçin.", 
                                isEn ? "Date Selection" : "Tarih Seçimi", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            SelectedDate = ReminderCalendar.SelectedDate.Value.Date;
            Confirmed    = true;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Confirmed    = false;
            DialogResult = false;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            SelectedDate = null;   // Hatırlatıcı kaldırsın
            Confirmed    = true;
            DialogResult = true;
        }
    }
}
