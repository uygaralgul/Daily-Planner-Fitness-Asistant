using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DailyPlanner.Views
{

    public partial class ClearCalendarDialog : Window
    {
        private DateTime _viewMonth;
        private readonly DateTime _sourceDate;
        private List<DialogDayItem> _days = new List<DialogDayItem>();

        public List<DateTime> SelectedDates { get; private set; } = new List<DateTime>();

        public ClearCalendarDialog(DateTime initialMonth, DateTime sourceDate)
        {
            InitializeComponent();
            _viewMonth = new DateTime(initialMonth.Year, initialMonth.Month, 1);
            _sourceDate = sourceDate;
            bool isEn = DailyPlanner.Services.LanguageService.CurrentLanguage == "en";
            var culture = new CultureInfo(isEn ? "en-US" : "tr-TR");
            SubTitle.Text = isEn ? $"Target Date: {sourceDate.ToString("d MMMM yyyy, dddd", culture)}" : $"Geçerli Tarih: {sourceDate.ToString("d MMMM yyyy, dddd", culture)}";
            RebuildCalendar();
        }

        private void RebuildCalendar()
        {
            _days.Clear();
            bool isEn = DailyPlanner.Services.LanguageService.CurrentLanguage == "en";
            var culture = new CultureInfo(isEn ? "en-US" : "tr-TR");
            MonthLabel.Text = _viewMonth.ToString("MMMM yyyy", culture);

            int firstDow = ((int)_viewMonth.DayOfWeek + 6) % 7; // Mon=0
            for (int i = 0; i < firstDow; i++)
                _days.Add(new DialogDayItem { IsEmpty = true });

            int daysInMonth = DateTime.DaysInMonth(_viewMonth.Year, _viewMonth.Month);
            for (int d = 1; d <= daysInMonth; d++)
            {
                var dt = new DateTime(_viewMonth.Year, _viewMonth.Month, d);
                _days.Add(new DialogDayItem
                {
                    Date = dt,
                    IsSelected = SelectedDates.Any(x => x.Date == dt.Date),
                    IsEmpty = false
                });
            }
            DayGrid.ItemsSource = _days;
        }

        private void Day_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.Border border &&
                border.Tag is DialogDayItem item && !item.IsEmpty)
            {
                item.IsSelected = !item.IsSelected;
                if (item.IsSelected)
                    SelectedDates.Add(item.Date);
                else
                    SelectedDates.RemoveAll(d => d.Date == item.Date);
            }
        }

        private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _days.Where(d => !d.IsEmpty))
            {
                if (!item.IsSelected)
                {
                    item.IsSelected = true;
                    SelectedDates.Add(item.Date);
                }
            }
            DayGrid.Items.Refresh();
        }

        private void BtnSelectNone_Click(object sender, RoutedEventArgs e)
        {
            var monthDates = _days.Where(d => !d.IsEmpty).Select(d => d.Date.Date).ToHashSet();
            SelectedDates.RemoveAll(d => monthDates.Contains(d.Date));
            foreach (var item in _days.Where(d => !d.IsEmpty))
                item.IsSelected = false;
            DayGrid.Items.Refresh();
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)

        {
            _viewMonth = _viewMonth.AddMonths(-1);
            RebuildCalendar();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            _viewMonth = _viewMonth.AddMonths(1);
            RebuildCalendar();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
