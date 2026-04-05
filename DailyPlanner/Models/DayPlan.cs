using System;
using System.Collections.Generic;

namespace DailyPlanner.Models
{
    public class DayPlan
    {
        public DateTime Date { get; set; }
        public List<PlanPage> Pages { get; set; } = new List<PlanPage>();

        public string DateKey => Date.ToString("yyyy-MM-dd");
    }
}
