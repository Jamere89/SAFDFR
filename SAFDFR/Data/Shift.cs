using System;

namespace SAFDFR.Data
{
    public class Shift
    {
        public string ShiftID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public Jurisdiction Jurisdiction { get; set; }
        public string UnitAssignment { get; set; }
        public ShiftStatus Status { get; set; }
        public int CallsCompleted { get; set; }
        public float HoursWorked { get; set; }
        public int BreaksCount { get; set; }
        public int MealsCount { get; set; }
    }

    public enum ShiftStatus
    {
        Active,
        Paused,
        Completed,
        Cancelled
    }
}
