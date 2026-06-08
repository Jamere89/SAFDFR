using System;

namespace SAFDFR.Data
{
    public class RadioTransmission
    {
        public string TransmissionID { get; set; }
        public string Unit { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }
        public string Channel { get; set; }
        public int SignalStrength { get; set; }
    }
}
