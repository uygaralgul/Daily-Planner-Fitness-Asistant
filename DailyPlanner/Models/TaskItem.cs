using System;
using System.ComponentModel;

namespace DailyPlanner.Models
{
    public class TaskItem : INotifyPropertyChanged
    {
        private string _description;
        private bool _isCompleted;
        private int _order;

        public Guid Id { get; set; } = Guid.NewGuid();

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set { _isCompleted = value; OnPropertyChanged(nameof(IsCompleted)); }
        }

        public int Order
        {
            get => _order;
            set { _order = value; OnPropertyChanged(nameof(Order)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
