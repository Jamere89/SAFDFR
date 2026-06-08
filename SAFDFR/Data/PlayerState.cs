using System;

namespace SAFDFR.Data
{
    public class PlayerState
    {
        public string PlayerName { get; set; }
        public bool IsOnDuty { get; set; }
        public string Role { get; set; }
        public Jurisdiction Jurisdiction { get; set; }
        public int Experience { get; set; }
        public int Level { get; set; }
        public int Reputation { get; set; }
        public int CallsResponded { get; set; }
        public int CallsCompleted { get; set; }
        public DateTime DutyStartTime { get; set; }
        public float TotalHoursWorked { get; set; }
    }
}
