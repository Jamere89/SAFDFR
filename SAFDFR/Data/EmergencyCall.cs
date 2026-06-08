using System;

namespace SAFDFR.Data
{
    public class EmergencyCall
    {
        public string CallID { get; set; }
        public string CallType { get; set; }
        public string Location { get; set; }
        public Jurisdiction Jurisdiction { get; set; }
        public CallPriority Priority { get; set; }
        public int PatientCount { get; set; }
        public CallStatus Status { get; set; }
        public DateTime TimeReceived { get; set; }
        public DateTime? TimeDispatched { get; set; }
        public DateTime? TimeArrived { get; set; }
        public DateTime? TimeCompleted { get; set; }
        public string Description { get; set; }
        public string Caller { get; set; }
        public string CallerPhone { get; set; }
    }

    public enum CallPriority
    {
        Low,
        Medium,
        High
    }

    public enum CallStatus
    {
        Pending,
        Dispatched,
        EnRoute,
        OnScene,
        Transport,
        Completed,
        Cancelled
    }

    public enum Jurisdiction
    {
        LosSantos,
        SanFierro,
        LasVenturas,
        CountyRural
    }
}
