namespace SAFDFR.Data
{
    public class AmbientUnit
    {
        public string UnitID { get; set; }
        public string UnitName { get; set; }
        public UnitStatus Status { get; set; }
        public string Location { get; set; }
        public Jurisdiction Jurisdiction { get; set; }
        public string AssignedCallID { get; set; }
        public int Mileage { get; set; }
    }

    public enum UnitStatus
    {
        Available,
        EnRoute,
        OnScene,
        Transport,
        OutOfService,
        Maintenance
    }
}
