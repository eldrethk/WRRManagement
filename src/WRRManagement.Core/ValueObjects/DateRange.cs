using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRRManagement.Core.ValueObjects
{
    public record DateRange
    {
        public DateTime Start { get; }
        public DateTime End { get; }

        public int TotalNights => (End.Date - Start.Date).Days;

        public int TotalDays => TotalNights + 1;

        public DateRange(DateTime start, DateTime end)
        {
            if (end <= start)
                throw new ArgumentException("End Date must be after start date");

            //Normallize to dates only
            Start = start; 
            End = end;
        }

        //checks if two dateranges overlap with each other
        public bool Overlap(DateRange other)
        {
            return Start < other.End && End > other.Start;
        }

        public bool Contains(DateTime date)
        {
            var dateOnly = date.Date;
            return dateOnly >= Start && dateOnly <= End;
        }

        public IEnumerable<DateTime> GetAllDates()
        {
            for (var date = Start; date < End; date = date.AddDays(1))
            {
                yield return date;
            }
        }

        public IEnumerable<DateTime> GetWeekendDates()
        {
            IEnumerable<DateTime> dates = GetAllDates();

            return dates.Where(d => d.DayOfWeek == DayOfWeek.Friday || d.DayOfWeek == DayOfWeek.Saturday);
        }

        public int NumOfWeekendNights => GetWeekendDates().Count();
    }
}
