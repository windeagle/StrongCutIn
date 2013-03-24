using System;

namespace StrongCutIn
{
    public class PerformanceMonitor
    {
        public int ID { get; set; }
        public string ThreadCode { get; set; }
        public string TypeName { get; set; }
        public string MethodName { get; set; }
        public DateTime OccorTime { get; set; }
        public long Interval { get; set; }
        public string Params { get; set; }
        public string ReturnValue { get; set; }
        public DateTime ReturnTime { get; set; }
        public bool Flag { get; set; }
        public DateTime AddTime { get; set; }
    }
}
