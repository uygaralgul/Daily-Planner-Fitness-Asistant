using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using DailyPlanner.Views;

namespace DailyPlanner.Services
{
    /// <summary>
    /// Uygulama başlangıcında ve her gün gece yarısında hatırlatıcıları kontrol eder.
    /// Tarihi bugün olan notlar için bildirim penceresi gösterir.
    /// </summary>
    public class ReminderService
    {
        private readonly DataService    _dataService;
        private readonly DispatcherTimer _timer;

        public ReminderService(DataService dataService)
        {
            _dataService = dataService;

            // Gece yarısına kadar kaç saniye kaldığını hesapla
            var now         = DateTime.Now;
            var midnight    = now.Date.AddDays(1);
            var initialDelay = midnight - now;

            _timer = new DispatcherTimer
            {
                Interval = initialDelay
            };
            _timer.Tick += (s, e) =>
            {
                // İlk çalışmadan sonra tam 24 saatte bir çalışsın
                _timer.Interval = TimeSpan.FromHours(24);
                CheckAndNotify();
            };
            _timer.Start();
        }

        /// <summary>Uygulama açılışında çağrılır. Bugünün hatırlatıcılarını gösterir.</summary>
        public void CheckAndNotify()
        {
            var notes = _dataService.LoadNotes();
            var dueNotes = notes
                .Where(n => n.ReminderDate.HasValue && n.ReminderDate.Value.Date == DateTime.Today)
                .ToList();

            foreach (var note in dueNotes)
            {
                var win = new ReminderNotificationWindow(note, _dataService);
                win.Show();
            }
        }
    }
}
