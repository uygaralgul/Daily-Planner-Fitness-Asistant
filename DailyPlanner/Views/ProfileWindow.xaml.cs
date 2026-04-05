using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using DailyPlanner.Models;
using DailyPlanner.Services;

namespace DailyPlanner.Views
{
    public partial class ProfileWindow : Window, INotifyPropertyChanged
    {
        private readonly AppSettings _settings;
        private readonly DataService _dataService;
        private PageTemplate _selectedTemplate;

        public ObservableCollection<PageTemplate> PageTemplates { get; set; }

        public PageTemplate SelectedTemplate
        {
            get => _selectedTemplate;
            set
            {
                _selectedTemplate = value;
                OnPropertyChanged(nameof(SelectedTemplate));
                OnPropertyChanged(nameof(IsTemplateSelected));
            }
        }

        public bool IsTemplateSelected => _selectedTemplate != null;

        public ProfileWindow(AppSettings settings, DataService dataService)
        {
            InitializeComponent();
            _settings = settings;
            _dataService = dataService;

            if (_settings.PageTemplates == null)
            {
                _settings.PageTemplates = new System.Collections.Generic.List<PageTemplate>();
            }

            PageTemplates = new ObservableCollection<PageTemplate>(_settings.PageTemplates);
            DataContext = this;
        }

        private void AddTemplate_Click(object sender, RoutedEventArgs e)
        {
            bool isEn = LanguageService.CurrentLanguage == "en";
            var newTemplate = new PageTemplate { Name = isEn ? $"New Template {PageTemplates.Count + 1}" : $"Yeni Şablon {PageTemplates.Count + 1}" };
            PageTemplates.Add(newTemplate);
            SelectedTemplate = newTemplate;
        }

        private void DeleteTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is PageTemplate template)
            {
                bool isEn = LanguageService.CurrentLanguage == "en";
                if (MessageBox.Show(isEn 
                        ? $"Are you sure you want to delete template '{template.Name}'? (This won't delete existing days, just detaches them from the template.)"
                        : $"'{template.Name}' şablonunu silmek istediğinize emin misiniz? (Bu işlem mevcut günleri silmez, sadece şablon bağını koparır.)", 
                    isEn ? "Delete Template" : "Şablonu Sil", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    PageTemplates.Remove(template);
                    if (SelectedTemplate == template) SelectedTemplate = null;
                }
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTemplate != null)
            {
                bool isEn = LanguageService.CurrentLanguage == "en";
                SelectedTemplate.Tasks.Add(new TaskItem 
                { 
                    Description = isEn ? "New Task/Exercise" : "Yeni Hareket/Görev",
                    Order = SelectedTemplate.Tasks.Count 
                });
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is TaskItem task && SelectedTemplate != null)
            {
                SelectedTemplate.Tasks.Remove(task);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Update settings list
            _settings.PageTemplates = PageTemplates.ToList();
            _dataService.SaveSettings(_settings);

            // If a template was edited, apply its changes to all pages that use it
            if (SelectedTemplate != null)
            {
                bool isEn = LanguageService.CurrentLanguage == "en";
                _dataService.ApplyTemplateUpdate(SelectedTemplate);
                MessageBox.Show(isEn 
                    ? $"Template '{SelectedTemplate.Name}' saved and applied to relevant days." 
                    : $"'{SelectedTemplate.Name}' şablonu kaydedildi ve ilgili günlere uygulandı.", 
                    isEn ? "Success" : "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
