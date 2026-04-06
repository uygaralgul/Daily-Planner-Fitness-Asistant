using System;

namespace DailyPlanner.Models
{
    public class NoteItem
    {
        public Guid   Id        { get; set; } = Guid.NewGuid();
        public string Title     { get; set; } = "Yeni Not";
        public string Content   { get; set; } = string.Empty;  // düz metin (eski notlar)
        public string RtfContent { get; set; } = null;          // RTF formatı (yeni notlar)
        public DateTime  CreatedAt    { get; set; } = DateTime.Now;
        public DateTime  UpdatedAt    { get; set; } = DateTime.Now;
        public DateTime? ReminderDate { get; set; } = null;
    }
}

