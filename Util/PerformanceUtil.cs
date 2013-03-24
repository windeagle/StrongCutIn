using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace StrongCutIn.Util
{
    public static class PerformanceUtil
    {
        public static void InsertPerformanceAnchor(string name = "")
        {
            var performanceMonitor = new PerformanceMonitor
                                         {
                                             AddTime = DateTime.Now,
                                             Flag = false,
                                             MethodName = name,
                                             OccorTime = DateTime.Now,
                                             ReturnTime = DateTime.Now,
                                             ThreadCode = Thread.CurrentThread.GetHashCode().ToString(CultureInfo.InvariantCulture),
                                             TypeName = "T"
                                         };

            List<PerformanceMonitor> performanceMonitors;
            var performanceMonitorList = System.Runtime.Remoting.Messaging.CallContext.GetData("PerformanceMonitorList");
            var timer = System.Runtime.Remoting.Messaging.CallContext.GetData("PerformanceMonitorTimer");
            var t = timer as Stopwatch;
            if (t != null)
            {
                t.Stop();
                performanceMonitor.Interval = t.ElapsedTicks;
            }
            t = new Stopwatch();
            t.Start();
            System.Runtime.Remoting.Messaging.CallContext.SetData("PerformanceMonitorTimer",
                                                                  t);

            if (performanceMonitorList == null || (performanceMonitorList as List<PerformanceMonitor>) == null)
            {
                performanceMonitors = new List<PerformanceMonitor> {performanceMonitor};
                System.Runtime.Remoting.Messaging.CallContext.SetData("PerformanceMonitorList",
                                                                      performanceMonitors);
            }
            else
            {
                performanceMonitors = performanceMonitorList as List<PerformanceMonitor>;
                performanceMonitors.Add(performanceMonitor);
            }
        }
    }
}