using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace TaskManager
{
    public class Tasks
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool Neverend { get; set; }
        public bool Notify { get; set; }
        public DateTime WhenNotify { get; set; }
        public ObservableCollection<DateTime> Dates { get; set; }
        public ObservableCollection<DayOfWeek> NotifyDays { get; set; }
        public int ID { get; set; }

        public Tasks()
        {
            Dates = new ObservableCollection<DateTime>();
        }
    }
}
