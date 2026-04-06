using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using DailyPlanner.Models;
using DailyPlanner.Services;

namespace DailyPlanner.ViewModels
{
    public class CalendarDayViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        private DayStatus _status;

        public DateTime Date { get; set; }
        public int DayNumber => Date.Day;
        public bool IsToday => Date.Date == DateTime.Today;
        public bool IsPast => Date.Date < DateTime.Today;

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public DayStatus Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string p) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DataService _dataService;
        private DateTime _selectedDate;
        private DateTime _viewMonth;
        private DayPlan _currentDayPlan;
        private PlanPage _selectedPage;
        private bool _isDarkMode;
        private DayPlan _copiedDayPlan;
        private PlanPage _copiedPage;
        private AppSettings _settings;

        public ObservableCollection<CalendarDayViewModel> CalendarDays { get; } = new ObservableCollection<CalendarDayViewModel>();
        public ObservableCollection<PlanPage> Pages { get; } = new ObservableCollection<PlanPage>();

        public string CurrentMonthYear => _viewMonth.ToString("MMMM yyyy", LanguageService.CurrentCulture);
        public string SelectedDateDisplay => _selectedDate.ToString("d MMMM yyyy", LanguageService.CurrentCulture);
        public string SelectedDayOfWeek  => _selectedDate.ToString("dddd", LanguageService.CurrentCulture);

        public void RefreshLocalization()
        {
            OnPropertyChanged(nameof(CurrentMonthYear));
            OnPropertyChanged(nameof(SelectedDateDisplay));
            OnPropertyChanged(nameof(SelectedDayOfWeek));
            RefreshCalendar();
            NotifyProgress();
        }

        public string ProgressSummary
        {
            get
            {
                if (SelectedPage == null) return string.Empty;
                int total = SelectedPage.Tasks.Count;
                int done  = SelectedPage.Tasks.Count(t => t.IsCompleted);
                
                string noTasks = Application.Current.FindResource("Text_NoTasksYet") as string ?? "Henüz görev yok";
                string completedText = Application.Current.FindResource("Text_TasksCompleted") as string ?? "görev tamamlandı";

                return total == 0 ? noTasks : $"{done}/{total} {completedText}";
            }
        }

        public double ProgressPercent
        {
            get
            {
                if (SelectedPage == null || SelectedPage.Tasks.Count == 0) return 0;
                return (double)SelectedPage.Tasks.Count(t => t.IsCompleted) / SelectedPage.Tasks.Count * 100;
            }
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                _isDarkMode = value;
                OnPropertyChanged(nameof(IsDarkMode));
                ThemeService.ApplyTheme(value);
                SaveSettings();
            }
        }

        public PlanPage SelectedPage
        {
            get => _selectedPage;
            set { _selectedPage = value; OnPropertyChanged(nameof(SelectedPage)); }
        }

        public bool HasCopiedDay => _copiedDayPlan != null;
        public bool HasCopiedPage => _copiedPage != null;

        // Commands
        public RelayCommand PreviousMonthCommand { get; }
        public RelayCommand NextMonthCommand { get; }
        public RelayCommand SelectDayCommand { get; }
        public RelayCommand AddTaskCommand { get; }
        public RelayCommand DeleteTaskCommand { get; }
        public RelayCommand AddPageCommand { get; }
        public RelayCommand DeletePageCommand { get; }
        public RelayCommand CopyDayCommand { get; }
        public RelayCommand PasteDayCommand { get; }
        public RelayCommand PasteToMonthCommand { get; }
        public RelayCommand ClearDayCommand { get; }
        public RelayCommand CopyPageCommand { get; }
        public RelayCommand PastePageCommand { get; }
        public RelayCommand ToggleTaskCommand { get; }
        public RelayCommand OpenCalculatorsCommand { get; }
        public RelayCommand SelectPageCommand { get; set; }
        public RelayCommand RenamePageCommand { get; }
        public RelayCommand OpenProfileCommand { get; }
        public RelayCommand OpenNotesCommand { get; }
        public RelayCommand OpenStatsCommand { get; }
        public RelayCommand ExitCommand { get; }
        public RelayCommand SetPageColorCommand { get; }

        public MainViewModel()
        {
            _dataService = new DataService();
            _settings = _dataService.LoadSettings();
            _isDarkMode = _settings.IsDarkMode;
            _selectedDate = DateTime.Today;
            _viewMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            // Wire commands
            PreviousMonthCommand = new RelayCommand(() =>
            {
                _viewMonth = _viewMonth.AddMonths(-1);
                RefreshCalendar();
                OnPropertyChanged(nameof(CurrentMonthYear));
            });

            NextMonthCommand = new RelayCommand(() =>
            {
                _viewMonth = _viewMonth.AddMonths(1);
                RefreshCalendar();
                OnPropertyChanged(nameof(CurrentMonthYear));
            });

            SelectDayCommand = new RelayCommand(obj =>
            {
                if (obj is DateTime dt)
                {
                    SaveCurrentDay();
                    _selectedDate = dt;
                    OnPropertyChanged(nameof(SelectedDateDisplay));
                    OnPropertyChanged(nameof(SelectedDayOfWeek));
                    LoadDay(_selectedDate);
                    RefreshCalendar();
                    NotifyProgress();
                    CheckRemindersForDate(_selectedDate);
                }
            });

            AddTaskCommand = new RelayCommand(() =>
            {
                if (SelectedPage == null) return;
                string newTaskText = Application.Current.FindResource("Text_NewTaskDefault") as string ?? "New task";
                var task = new TaskItem { Description = newTaskText, Order = SelectedPage.Tasks.Count };
                SelectedPage.Tasks.Add(task);
                SaveCurrentDay();
                NotifyProgress();
            });

            DeleteTaskCommand = new RelayCommand(obj =>
            {
                if (obj is TaskItem task && SelectedPage != null)
                {
                    SelectedPage.Tasks.Remove(task);
                    SaveCurrentDay();
                    RefreshCalendarDay(_selectedDate);
                    NotifyProgress();
                }
            });

            ToggleTaskCommand = new RelayCommand(obj =>
            {
                SaveCurrentDay();
                RefreshCalendarDay(_selectedDate);
                NotifyProgress();
            });

            AddPageCommand = new RelayCommand(() =>
            {
                string pagePrefix = Application.Current.FindResource("Text_PagePrefix") as string ?? "Page";
                var page = new PlanPage { Name = $"{pagePrefix} {Pages.Count + 1}" };
                _currentDayPlan.Pages.Add(page);
                Pages.Add(page);
                SelectedPage = page;
                SaveCurrentDay();
            });

            DeletePageCommand = new RelayCommand(obj =>
            {
                if (obj is PlanPage page && Pages.Count > 1)
                {
                    _currentDayPlan.Pages.Remove(page);
                    Pages.Remove(page);
                    SelectedPage = Pages.FirstOrDefault();
                    SaveCurrentDay();
                }
            });

            CopyDayCommand = new RelayCommand(() =>
            {
                SaveCurrentDay();
                _copiedDayPlan = _currentDayPlan;
                OnPropertyChanged(nameof(HasCopiedDay));
                string msgFmt = Application.Current.FindResource("Msg_DayCopied") as string ?? "{0} copied!";
                string titleCopied = Application.Current.FindResource("Msg_CopiedTitle") as string ?? "Copied";
                string dateStr = _selectedDate.ToString("d MMMM yyyy", LanguageService.CurrentCulture);
                MessageBox.Show(string.Format(msgFmt, dateStr), titleCopied,
                    MessageBoxButton.OK, MessageBoxImage.Information);
            });

            PasteDayCommand = new RelayCommand(() =>
            {
                if (_copiedDayPlan == null) return;
                var dialog = new Views.PasteCalendarDialog(_viewMonth, _copiedDayPlan.Date);
                if (dialog.ShowDialog() == true)
                {
                    _dataService.CopyDayToTargets(_copiedDayPlan, dialog.SelectedDates);
                    LoadDay(_selectedDate);
                    RefreshCalendar();
                }
            }, () => HasCopiedDay);

            PasteToMonthCommand = new RelayCommand(() =>
            {
                if (_copiedDayPlan == null) return;
                string msgFmt = Application.Current.FindResource("Msg_PasteToMonthConfirm") as string ?? "Paste to all days in {0}?";
                string titlePaste = Application.Current.FindResource("Msg_PasteToMonthTitle") as string ?? "Paste to Month";
                var result = MessageBox.Show(
                    string.Format(msgFmt, CurrentMonthYear),
                    titlePaste, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _dataService.CopyDayToEntireMonth(_copiedDayPlan, _viewMonth.Year, _viewMonth.Month);
                    LoadDay(_selectedDate);
                    RefreshCalendar();
                }
            }, () => HasCopiedDay);

            ClearDayCommand = new RelayCommand(() =>
            {
                var dialog = new Views.PasteCalendarDialog(_viewMonth, _selectedDate);
                dialog.Title = Application.Current.FindResource("Title_ClearDays") as string ?? "Clear — Select Days";
                // Let's create a specialized dialog or update PasteCalendarDialog 
                // We will create ClearCalendarDialog.cs. For now, calling it:
                var clearDialog = new Views.ClearCalendarDialog(_viewMonth, _selectedDate);
                if (clearDialog.ShowDialog() == true)
                {
                    _dataService.ClearDays(clearDialog.SelectedDates);
                    LoadDay(_selectedDate);
                    RefreshCalendar();
                }
            });

            CopyPageCommand = new RelayCommand(() =>
            {
                if (SelectedPage == null) return;
                _copiedPage = _dataService.DeepClonePage(SelectedPage);
                OnPropertyChanged(nameof(HasCopiedPage));
                string pageMsgFmt = Application.Current.FindResource("Msg_PageCopied") as string ?? "'{0}' page copied!";
                string pageTitleCopied = Application.Current.FindResource("Msg_CopiedTitle") as string ?? "Copied";
                MessageBox.Show(string.Format(pageMsgFmt, SelectedPage.Name), pageTitleCopied,
                    MessageBoxButton.OK, MessageBoxImage.Information);
            });

            PastePageCommand = new RelayCommand(() =>
            {
                if (_copiedPage == null) return;
                var dialog = new Views.PasteCalendarDialog(_viewMonth, _selectedDate);
                dialog.Title = Application.Current.FindResource("Title_PastePageDays") as string ?? "Paste Page — Select Days";
                
                if (dialog.ShowDialog() == true)
                {
                    _dataService.CopyPageToTargets(_copiedPage, dialog.SelectedDates);
                    LoadDay(_selectedDate);
                    RefreshCalendar();
                }
            }, () => HasCopiedPage);

            RenamePageCommand = new RelayCommand(obj =>
            {
                // Triggered when page name text changes
                SaveCurrentDay();
            });

            OpenProfileCommand = new RelayCommand(() =>
            {
                var win = new Views.ProfileWindow(_settings, _dataService);
                win.Owner = Application.Current.MainWindow;
                win.ShowDialog();
                // After profile closes, reload everything as templates might have changed duties
                _settings = _dataService.LoadSettings();
                LoadDay(_selectedDate);
                RefreshCalendar();
            });

            OpenCalculatorsCommand = new RelayCommand(() =>
            {
                var win = new Views.CalculatorsWindow();
                win.Owner = Application.Current.MainWindow;
                win.Show();
            });

            OpenNotesCommand = new RelayCommand(() =>
            {
                var win = new Views.NotesWindow(_dataService);
                win.Owner = Application.Current.MainWindow;
                win.ShowDialog();
            });

            OpenStatsCommand = new RelayCommand(() =>
            {
                var win = new Views.StatsWindow(_dataService);
                win.Owner = Application.Current.MainWindow;
                win.ShowDialog();
            });

            ExitCommand = new RelayCommand(() => Application.Current.Shutdown());

            SetPageColorCommand = new RelayCommand(obj =>
            {
                if (obj is object[] args && args.Length == 2 &&
                    args[0] is Models.PlanPage page)
                {
                    page.Color = args[1] as string; // null = rengi temizle
                    SaveCurrentDay();
                }
            });

            // Initial load
            LoadDay(_selectedDate);
            RefreshCalendar();
            ThemeService.ApplyTheme(_isDarkMode);
        }

        private void LoadDay(DateTime date)
        {
            _currentDayPlan = _dataService.GetDayPlan(date);
            Pages.Clear();
            foreach (var p in _currentDayPlan.Pages)
                Pages.Add(p);
            SelectedPage = Pages.FirstOrDefault();
            NotifyProgress();
        }

        private void SaveCurrentDay()
        {
            if (_currentDayPlan != null)
                _dataService.SaveDayPlan(_currentDayPlan);
        }

        private void RefreshCalendar()
        {
            CalendarDays.Clear();
            int days = DateTime.DaysInMonth(_viewMonth.Year, _viewMonth.Month);
            for (int d = 1; d <= days; d++)
            {
                var dt = new DateTime(_viewMonth.Year, _viewMonth.Month, d);
                CalendarDays.Add(new CalendarDayViewModel
                {
                    Date = dt,
                    IsSelected = dt.Date == _selectedDate.Date,
                    Status = _dataService.GetDayStatus(dt)
                });
            }
        }

        private void RefreshCalendarDay(DateTime date)
        {
            var day = CalendarDays.FirstOrDefault(c => c.Date.Date == date.Date);
            if (day != null) day.Status = _dataService.GetDayStatus(date);
        }

        private void SaveSettings()
        {
            _settings.IsDarkMode = _isDarkMode;
            _dataService.SaveSettings(_settings);
        }

        private void NotifyProgress()
        {
            OnPropertyChanged(nameof(ProgressSummary));
            OnPropertyChanged(nameof(ProgressPercent));
        }

        private void CheckRemindersForDate(DateTime date)
        {
            var notes = _dataService.LoadNotes();
            foreach (var note in notes)
            {
                if (note.ReminderDate.HasValue && note.ReminderDate.Value.Date == date.Date)
                {
                    var win = new Views.ReminderNotificationWindow(note, _dataService);
                    win.Show();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
