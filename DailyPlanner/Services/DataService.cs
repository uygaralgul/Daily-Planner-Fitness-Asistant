using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DailyPlanner.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace DailyPlanner.Services
{
    public class DataService
    {
        private static readonly string AppDataPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DailyPlanner");
        private static readonly string DataFile     = Path.Combine(AppDataPath, "data.json");
        private static readonly string SettingsFile = Path.Combine(AppDataPath, "settings.json");
        private static readonly string NotesFile    = Path.Combine(AppDataPath, "notes.json");

        private Dictionary<string, DayPlan> _allPlans = new Dictionary<string, DayPlan>();

        public DataService()
        {
            Directory.CreateDirectory(AppDataPath);
            LoadAll();
        }

        private void LoadAll()
        {
            if (File.Exists(DataFile))
            {
                var json = File.ReadAllText(DataFile);
                _allPlans = JsonConvert.DeserializeObject<Dictionary<string, DayPlan>>(json)
                            ?? new Dictionary<string, DayPlan>();
            }
        }

        private void SaveAll()
        {
            var json = JsonConvert.SerializeObject(_allPlans, Formatting.Indented);
            File.WriteAllText(DataFile, json);
        }

        public DayPlan GetDayPlan(DateTime date)
        {
            var key = date.ToString("yyyy-MM-dd");
            if (!_allPlans.ContainsKey(key))
            {
                var plan = new DayPlan { Date = date.Date };
                plan.Pages.Add(new PlanPage { Name = "Görevler" });
                _allPlans[key] = plan;
            }
            return _allPlans[key];
        }

        public void SaveDayPlan(DayPlan plan)
        {
            _allPlans[plan.DateKey] = plan;
            SaveAll();
        }

        public void CopyDayToTargets(DayPlan source, List<DateTime> targets)
        {
            foreach (var target in targets)
            {
                var key = target.ToString("yyyy-MM-dd");
                var cloned = DeepClone(source, target.Date);
                
                // Keep TemplateId reference when copying so updates sync to copies
                for (int i = 0; i < source.Pages.Count; i++)
                {
                    if (source.Pages[i].TemplateId.HasValue)
                        cloned.Pages[i].TemplateId = source.Pages[i].TemplateId;
                }

                _allPlans[key] = cloned;
            }
            SaveAll();
        }

        public void CopyPageToTargets(PlanPage sourcePage, List<DateTime> targets)
        {
            foreach (var target in targets)
            {
                var plan = GetDayPlan(target);
                var cloned = DeepClonePage(sourcePage);
                plan.Pages.Add(cloned);
                SaveDayPlan(plan);
            }
        }

        public void CopyDayToEntireMonth(DayPlan source, int year, int month)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var targets = Enumerable.Range(1, daysInMonth)
                .Select(d => new DateTime(year, month, d))
                .Where(d => d.Date != source.Date.Date)
                .ToList();
            CopyDayToTargets(source, targets);
        }

        public bool HasPlan(DateTime date)
        {
            var key = date.ToString("yyyy-MM-dd");
            return _allPlans.ContainsKey(key) && _allPlans[key].Pages.Any(p => p.Tasks.Count > 0);
        }

        public DayStatus GetDayStatus(DateTime date)
        {
            if (!HasPlan(date)) return DayStatus.Empty;
            var plan = _allPlans[date.ToString("yyyy-MM-dd")];
            var allTasks = plan.Pages.SelectMany(p => p.Tasks).ToList();
            if (!allTasks.Any()) return DayStatus.Empty;
            if (allTasks.All(t => t.IsCompleted)) return DayStatus.Completed;
            if (allTasks.Any(t => t.IsCompleted)) return DayStatus.Partial;
            return DayStatus.HasTasks;
        }

        public void ClearDay(DateTime date)
        {
            var key = date.ToString("yyyy-MM-dd");
            if (_allPlans.ContainsKey(key))
            {
                _allPlans.Remove(key);
                SaveAll();
            }
        }

        public void ClearDays(List<DateTime> dates)
        {
            foreach (var date in dates)
            {
                var key = date.ToString("yyyy-MM-dd");
                if (_allPlans.ContainsKey(key))
                    _allPlans.Remove(key);
            }
            SaveAll();
        }

        public void ApplyTemplateUpdate(PageTemplate template)
        {
            // Find all instances of this template across all days
            var allPages = _allPlans.Values.SelectMany(dp => dp.Pages)
                                           .Where(p => p.TemplateId == template.Id)
                                           .ToList();

            foreach (var page in allPages)
            {
                page.Name = template.Name; // Sync name

                // Process tasks
                // 1. Remove tasks that are no longer in the template (but match descriptions of deleted ones)
                // 2. Add new tasks from template
                // We use description matching as tasks have unique IDs per day in cloning
                
                var currentTaskDescriptions = page.Tasks.Select(t => t.Description).ToList();
                var templateTaskDescriptions = template.Tasks.Select(t => t.Description).ToList();

                // Remove deleted ones
                var toRemove = page.Tasks.Where(t => !templateTaskDescriptions.Contains(t.Description)).ToList();
                foreach (var tr in toRemove) page.Tasks.Remove(tr);

                // Add new ones
                foreach (var tTask in template.Tasks)
                {
                    if (!currentTaskDescriptions.Contains(tTask.Description))
                    {
                        page.Tasks.Add(new TaskItem 
                        { 
                            Description = tTask.Description, 
                            Order = tTask.Order,
                            IsCompleted = false
                        });
                    }
                }
            }
            SaveAll();
        }

        private DayPlan DeepClone(DayPlan source, DateTime newDate)
        {
            var json = JsonConvert.SerializeObject(source);
            var clone = JsonConvert.DeserializeObject<DayPlan>(json);
            clone.Date = newDate;
            // Regenerate IDs to avoid collisions; reset completion state for pasted day
            foreach (var p in clone.Pages)
            {
                p.Id = Guid.NewGuid();
                foreach (var t in p.Tasks)
                {
                    t.Id = Guid.NewGuid();
                    t.IsCompleted = false; // Yapıştırılan günde tikler temizlenir
                }
            }
            return clone;
        }

        public PlanPage DeepClonePage(PlanPage source)
        {
            var json = JsonConvert.SerializeObject(source);
            var clone = JsonConvert.DeserializeObject<PlanPage>(json);
            
            // Regenerate IDs to avoid collisions; reset completion state for pasted page
            clone.Id = Guid.NewGuid();
            foreach (var t in clone.Tasks)
            {
                t.Id = Guid.NewGuid();
                t.IsCompleted = false; // Yapıştırılan sayfada tikler temizlenir
            }
            return clone;
        }

        public AppSettings LoadSettings()
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
            return new AppSettings();
        }

        public void SaveSettings(AppSettings settings)
        {
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(SettingsFile, json);
        }

        public ObservableCollection<NoteItem> LoadNotes()
        {
            if (File.Exists(NotesFile))
            {
                var json  = File.ReadAllText(NotesFile);
                var list  = JsonConvert.DeserializeObject<List<NoteItem>>(json);
                if (list != null) return new ObservableCollection<NoteItem>(list);
            }
            return new ObservableCollection<NoteItem>();
        }

        public void SaveNotes(ObservableCollection<NoteItem> notes)
        {
            var json = JsonConvert.SerializeObject(notes, Formatting.Indented);
            File.WriteAllText(NotesFile, json);
        }

        public List<DayStatItem> GetRangeStats(DateTime from, DateTime to)
        {
            var result = new List<DayStatItem>();
            for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
            {
                var key = d.ToString("yyyy-MM-dd");
                int total = 0, done = 0;
                if (_allPlans.TryGetValue(key, out var plan))
                {
                    var tasks = plan.Pages.SelectMany(p => p.Tasks).ToList();
                    total = tasks.Count;
                    done  = tasks.Count(t => t.IsCompleted);
                }
                result.Add(new DayStatItem { Date = d, Total = total, Completed = done });
            }
            return result;
        }

        /// <summary>Belirtilen günün her sayfasının istatistiğini ayrı ayrı döndürür.</summary>
        public List<PageStatItem> GetPageStats(DateTime date)
        {
            var key = date.ToString("yyyy-MM-dd");
            if (!_allPlans.TryGetValue(key, out var plan))
                return new List<PageStatItem>();

            return plan.Pages.Select(p => new PageStatItem
            {
                PageName  = string.IsNullOrWhiteSpace(p.Name) ? "(İsimsiz)" : p.Name,
                PageColor = p.Color,
                Total     = p.Tasks.Count,
                Completed = p.Tasks.Count(t => t.IsCompleted)
            }).ToList();
        }
    }

    public enum DayStatus { Empty, HasTasks, Partial, Completed }

    public class DayStatItem
    {
        public DateTime Date      { get; set; }
        public int      Total     { get; set; }
        public int      Completed { get; set; }
        public double   Percent   => Total == 0 ? 0 : (double)Completed / Total * 100;
        public bool     HasData   => Total > 0;
    }

    public class PageStatItem
    {
        public string PageName  { get; set; }
        public string PageColor { get; set; }  // hex renk, null olabilir
        public int    Total     { get; set; }
        public int    Completed { get; set; }
        public double Percent   => Total == 0 ? 0 : (double)Completed / Total * 100;
        public bool   HasData   => Total > 0;
    }
}
