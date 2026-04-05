using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DailyPlanner.Models
{
    public class PlanPage : INotifyPropertyChanged
    {
        private string _name;
        private string _color;

        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? TemplateId { get; set; }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        /// <summary>Hex renk kodu, örneğin "#6366F1". null = varsayılan tema rengi.</summary>
        public string Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(nameof(Color)); }
        }

        private string _backgroundImage;
        /// <summary>Sayfa arka planında gösterilecek desenin dosya adı. Örn: "bg_sport.png". null = arka plan yok.</summary>
        public string BackgroundImage
        {
            get => _backgroundImage;
            set { _backgroundImage = value; OnPropertyChanged(nameof(BackgroundImage)); }
        }

        public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
