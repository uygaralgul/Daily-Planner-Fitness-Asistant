using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using DailyPlanner.Services;

namespace DailyPlanner.Views
{
    // ViewModel for a single bar in the chart
    public class BarItem
    {
        public string   DayLabel     { get; set; }
        public string   PercentLabel { get; set; }
        public double   BarHeight    { get; set; }   // 0–180 px
        public Brush    BarColor     { get; set; }
        public string   Tooltip      { get; set; }
        public DateTime Date         { get; set; }   // tıklama için tarih
        public bool     IsMonth      { get; set; }   // aylık mod: tıklama desteklenmez
    }

    // Sayfa bazlı istatistik satırı
    public class PageStatRow
    {
        public string PageName     { get; set; }
        public double Percent      { get; set; }
        public int    Completed    { get; set; }
        public int    Total        { get; set; }
        public Brush  AccentBrush  { get; set; }
        public double BarWidth     { get; set; }  // 0-200 px
        public string Label        { get; set; }  // "%72  (8/11)"
    }

    public partial class StatsWindow : Window
    {
        private readonly DataService _dataService;
        // 0 = Weekly, 1 = Monthly, 2 = Yearly
        private int      _mode = 0;
        private DateTime _anchor;                   // start of current period

        private static readonly Brush GreenBrush  = new SolidColorBrush(Color.FromRgb(0x6E, 0xE7, 0xB7));
        private static readonly Brush YellowBrush = new SolidColorBrush(Color.FromRgb(0xFC, 0xD3, 0x4D));
        private static readonly Brush RedBrush    = new SolidColorBrush(Color.FromRgb(0xFC, 0xA5, 0xA5));
        private static readonly Brush GrayBrush   = new SolidColorBrush(Color.FromRgb(0xD1, 0xD5, 0xDB));

        public StatsWindow(DataService dataService)
        {
            InitializeComponent();
            _dataService = dataService;
            _anchor      = StartOfWeek(DateTime.Today);
            _mode        = 0;
            Refresh();
        }

        // ── Mode toggling ─────────────────────────────────────────────
        private void WeeklyBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (MonthlyBtn != null) MonthlyBtn.IsChecked = false;
            if (YearlyBtn  != null) YearlyBtn.IsChecked  = false;
            _mode   = 0;
            _anchor = StartOfWeek(DateTime.Today);
            Refresh();
        }

        private void MonthlyBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (WeeklyBtn != null) WeeklyBtn.IsChecked = false;
            if (YearlyBtn != null) YearlyBtn.IsChecked = false;
            _mode   = 1;
            _anchor = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            Refresh();
        }

        private void YearlyBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (WeeklyBtn  != null) WeeklyBtn.IsChecked  = false;
            if (MonthlyBtn != null) MonthlyBtn.IsChecked = false;
            _mode   = 2;
            _anchor = new DateTime(DateTime.Today.Year, 1, 1);
            Refresh();
        }

        // ── Navigation ────────────────────────────────────────────────
        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            _anchor = _mode == 0 ? _anchor.AddDays(-7)
                    : _mode == 1 ? _anchor.AddMonths(-1)
                                 : _anchor.AddYears(-1);
            Refresh();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            _anchor = _mode == 0 ? _anchor.AddDays(7)
                    : _mode == 1 ? _anchor.AddMonths(1)
                                 : _anchor.AddYears(1);
            Refresh();
        }

        // ── Core refresh ──────────────────────────────────────────────
        private void Bar_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is BarItem bar && !bar.IsMonth)
                ShowPageDetail(bar.Date);
        }

        private void ShowPageDetail(DateTime date)
        {
            var pages = _dataService.GetPageStats(date);
            if (!pages.Any())
            {
                DetailPanel.Visibility = Visibility.Collapsed;
                return;
            }

            bool isEn = LanguageService.CurrentLanguage == "en";
            var culture = new System.Globalization.CultureInfo(isEn ? "en-US" : "tr-TR");
            DetailDateLabel.Text = date.ToString("d MMMM yyyy, dddd", culture);

            var rows = pages.Select(p =>
            {
                Brush accent;
                if (!string.IsNullOrWhiteSpace(p.PageColor))
                {
                    try
                    {
                        var c = (Color)ColorConverter.ConvertFromString(p.PageColor);
                        accent = new SolidColorBrush(c);
                    }
                    catch { accent = GreenBrush; }
                }
                else
                {
                    accent = p.Percent >= 80 ? GreenBrush
                           : p.Percent >= 40 ? YellowBrush
                           : p.Total == 0   ? GrayBrush
                                             : RedBrush;
                }

                return new PageStatRow
                {
                    PageName    = p.PageName,
                    Percent     = p.Percent,
                    Completed   = p.Completed,
                    Total       = p.Total,
                    AccentBrush = accent,
                    BarWidth    = p.Total == 0 ? 0 : Math.Max(4, p.Percent / 100.0 * 200),
                    Label       = p.Total == 0
                        ? (isEn ? "No tasks" : "Görev yok")
                        : $"%{p.Percent:F0}  ({p.Completed}/{p.Total})"
                };
            }).ToList();

            PageDetailList.ItemsSource = rows;
            DetailPanel.Visibility = Visibility.Visible;
        }

        private void CloseDetail_Click(object sender, RoutedEventArgs e)
        {
            DetailPanel.Visibility = Visibility.Collapsed;
        }

        private void Refresh()
        {
            // Guard against early calls during InitializeComponent (before controls are created)
            if (ChartItems == null || AvgChip == null) return;
            DetailPanel.Visibility = Visibility.Collapsed;

            bool isEn = LanguageService.CurrentLanguage == "en";
            var culture = new CultureInfo(isEn ? "en-US" : "tr-TR");
            List<BarItem> bars;

            if (_mode == 2) // ── Yearly: 12 month bars ─────────────────
            {
                int year = _anchor.Year;
                PeriodLabel.Text = year.ToString();

                var from  = new DateTime(year, 1, 1);
                var to    = new DateTime(year, 12, 31);
                var stats = _dataService.GetRangeStats(from, to);

                // Group by month, average completion %
                string[] monthNames = isEn 
                    ? new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" }
                    : new[] { "Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara" };
                bars = new List<BarItem>();
                for (int m = 1; m <= 12; m++)
                {
                    var monthDays = stats.Where(s => s.Date.Month == m).ToList();
                    var withData  = monthDays.Where(s => s.HasData).ToList();
                    double avgPct = withData.Any() ? withData.Average(s => s.Percent) : 0;
                    bool   hasAny = withData.Any();
                    int    totalDone  = monthDays.Sum(s => s.Completed);
                    int    totalTasks = monthDays.Sum(s => s.Total);

                    Brush color;
                    if (!hasAny)          color = GrayBrush;
                    else if (avgPct >= 80) color = GreenBrush;
                    else if (avgPct >= 40) color = YellowBrush;
                    else                   color = RedBrush;

                    bars.Add(new BarItem
                    {
                        DayLabel     = monthNames[m - 1],
                        PercentLabel = hasAny ? $"%{avgPct:F0}" : "",
                        BarHeight    = hasAny ? Math.Max(4, avgPct / 100.0 * 180) : 2,
                        BarColor     = color,
                        Tooltip      = hasAny
                            ? (isEn 
                                ? $"{monthNames[m-1]} {year}\n{totalDone}/{totalTasks} tasks completed ({avgPct:F0}% avg)"
                                : $"{monthNames[m-1]} {year}\n{totalDone}/{totalTasks} görev tamamlandı (%{avgPct:F0} ort.)")
                            : (isEn 
                                ? $"{monthNames[m-1]} {year}\nNo task records"
                                : $"{monthNames[m-1]} {year}\nGörev kaydı yok")
                    });
                }

                // Summary
                var allData = stats.Where(s => s.HasData).ToList();
                AvgChip.Text   = allData.Any() ? $"%{allData.Average(s => s.Percent):F0}" : "%—";
                BestChip.Text  = allData.Any() ? $"%{allData.Max(s => s.Percent):F0}"     : "%—";
                TotalChip.Text = $"{stats.Sum(s => s.Completed)}/{stats.Sum(s => s.Total)}";
            }
            else // ── Weekly / Monthly: day bars ────────────────────────
            {
                DateTime from, to;
                if (_mode == 0) // weekly
                {
                    from = _anchor;
                    to   = _anchor.AddDays(6);
                    PeriodLabel.Text = $"{from:dd MMM} – {to:dd MMM yyyy}";
                }
                else             // monthly
                {
                    from = _anchor;
                    to   = new DateTime(_anchor.Year, _anchor.Month,
                               DateTime.DaysInMonth(_anchor.Year, _anchor.Month));
                    PeriodLabel.Text = _anchor.ToString("MMMM yyyy", culture);
                }

                var stats    = _dataService.GetRangeStats(from, to);
                bars         = BuildDayBars(stats);

                var withData = stats.Where(s => s.HasData).ToList();
                double avg  = withData.Any() ? withData.Average(s => s.Percent) : 0;
                double best = withData.Any() ? withData.Max(s => s.Percent) : 0;
                AvgChip.Text   = $"%{avg:F0}";
                BestChip.Text  = $"%{best:F0}";
                TotalChip.Text = $"{stats.Sum(s => s.Completed)}/{stats.Sum(s => s.Total)}";
            }

            ChartItems.ItemsSource = bars;
        }

        // Renamed from BuildBars → BuildDayBars (used for weekly/monthly)
        private List<BarItem> BuildDayBars(List<DayStatItem> stats)
        {
            const double maxHeight = 180.0;
            bool isEn = LanguageService.CurrentLanguage == "en";
            var culture = new CultureInfo(isEn ? "en-US" : "tr-TR");
            var bars = new List<BarItem>();

            foreach (var s in stats)
            {
                double height = s.HasData ? Math.Max(4, s.Percent / 100.0 * maxHeight) : 2;

                Brush color;
                if (!s.HasData)          color = GrayBrush;
                else if (s.Percent >= 80) color = GreenBrush;
                else if (s.Percent >= 40) color = YellowBrush;
                else                      color = RedBrush;

                string dayLabel = _mode == 0
                    ? s.Date.ToString("ddd\ndd", culture)
                    : s.Date.ToString("dd", culture);

                string pctLabel = s.HasData ? $"%{s.Percent:F0}" : "";

                string tooltip = s.HasData
                    ? (isEn 
                        ? $"{s.Date:d MMMM yyyy}\n{s.Completed}/{s.Total} tasks completed ({s.Percent:F0}%)\nClick for details"
                        : $"{s.Date:d MMMM yyyy}\n{s.Completed}/{s.Total} görev tamamlandı (%{s.Percent:F0})\nDetay için tıklayın")
                    : (isEn 
                        ? $"{s.Date:d MMMM yyyy}\nNo tasks"
                        : $"{s.Date:d MMMM yyyy}\nGörev yok");

                bars.Add(new BarItem
                {
                    DayLabel     = dayLabel,
                    PercentLabel = pctLabel,
                    BarHeight    = height,
                    BarColor     = color,
                    Tooltip      = tooltip,
                    Date         = s.Date,
                    IsMonth      = false
                });
            }
            return bars;
        }

        // ── Helpers ───────────────────────────────────────────────────
        private static DateTime StartOfWeek(DateTime dt)
        {
            // Week starts Monday
            int diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday)) % 7;
            return dt.AddDays(-diff).Date;
        }
    }
}
