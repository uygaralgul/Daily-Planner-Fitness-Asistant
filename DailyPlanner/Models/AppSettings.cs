using System.Collections.Generic;

namespace DailyPlanner.Models
{
    public class AppSettings
    {
        public bool IsDarkMode { get; set; } = false;
        public UserProfile UserProfile { get; set; } = new UserProfile();
        public List<PageTemplate> PageTemplates { get; set; } = new List<PageTemplate>();
        // Not defteri tercihleri
        // Ana ekran sidebar rengi
        public string MainSidebarColor { get; set; } = null; // null = varsayılan tema rengi
        // Not defteri tercihleri
        public string NotesSidebarColor { get; set; } = null; // null = varsayılan tema rengi
        public string NotesDefaultTextColor { get; set; } = null; // null = varsayılan
        public string Language { get; set; } = "tr"; // "tr" for Turkish, "en" for English
    }
}

