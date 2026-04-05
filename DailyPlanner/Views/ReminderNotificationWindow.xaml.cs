using System.Windows;
using DailyPlanner.Models;
using DailyPlanner.Services;

namespace DailyPlanner.Views
{
    public partial class ReminderNotificationWindow : Window
    {
        private readonly NoteItem _note;
        private readonly DataService _dataService;

        public ReminderNotificationWindow(NoteItem note, DataService dataService)
        {
            InitializeComponent();
            _note = note;
            _dataService = dataService;
            NoteTitle.Text = note.Title;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Show_Click(object sender, RoutedEventArgs e)
        {
            Close();
            // NotesWindow'u aç ve ilgili notu seçili göster
            var notesWin = new NotesWindow(_dataService, _note.Id);
            notesWin.Show();
        }
    }
}
