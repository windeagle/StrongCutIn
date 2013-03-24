using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using StrongCutIn.Impl.Common;

namespace StrongCutIn.Util
{

    public class PerformanceMonitorCallHandler : BaseCallHandler
    {
        public override object ProxyMethod(object[] objs)
        {
            var performanceMonitor = new PerformanceMonitor
            {
                ThreadCode = Thread.CurrentThread.GetHashCode().ToString(CultureInfo.InvariantCulture),
                TypeName = BaseObj.GetType().Name,
                MethodName = BaseDelegate.Method.Name,
                OccorTime = DateTime.Now,
                ReturnTime = DateTime.Now
            };
            var sb = new StringBuilder();
            foreach (var o in objs)
            {
                sb.AppendFormat("{0}: {1}", o.GetType().Name, o).AppendLine();
            }
            performanceMonitor.Params = sb.ToString();
            performanceMonitor.Flag = true;

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
                performanceMonitors = new List<PerformanceMonitor> { performanceMonitor };
                System.Runtime.Remoting.Messaging.CallContext.SetData("PerformanceMonitorList",
                                                                      performanceMonitors);
            }
            else
            {
                performanceMonitors = performanceMonitorList as List<PerformanceMonitor>;
                performanceMonitors.Add(performanceMonitor);
            }

            //System.Runtime.Remoting.Messaging.CallContext.FreeNamedDataSlot("PerformanceMonitorList");

            //Console.WriteLine();
            //Console.WriteLine(GetType().Name);
            //Console.WriteLine("调用类型：" + BaseObj.GetType().Name);
            //Console.WriteLine("调用方法：" + ((MethodInfo)(BaseDelegate.Target)).Name);
            //Console.WriteLine("调用时间：" + DateTime.Now);
            //Console.WriteLine("调用参数：");
            //foreach (var o in objs)
            //{
            //    Console.WriteLine("    {0}: {1}", o.GetType().Name, o);
            //}

            var result = InnerDelegate(objs);

            //Console.WriteLine("返回值：");
            //Console.WriteLine("    {0}: {1}", result.GetType().Name, result);
            //Console.WriteLine("返回时间：" + DateTime.Now);

            var endPerformanceMonitor = new PerformanceMonitor
            {
                ThreadCode = Thread.CurrentThread.GetHashCode().ToString(CultureInfo.InvariantCulture),
                TypeName = BaseObj.GetType().Name,
                MethodName = BaseDelegate.Method.Name,
                OccorTime = DateTime.Now,
                ReturnTime = DateTime.Now
            };

            if (result != null)
                endPerformanceMonitor.ReturnValue = String.Format("{0}: {1}", result.GetType().Name, result);
            endPerformanceMonitor.Flag = false;
            timer = System.Runtime.Remoting.Messaging.CallContext.GetData("PerformanceMonitorTimer");
            t = timer as Stopwatch;
            if (t != null)
            {
                t.Stop();
                endPerformanceMonitor.Interval = t.ElapsedTicks;
            }
            t = new Stopwatch();
            t.Start();
            System.Runtime.Remoting.Messaging.CallContext.SetData("PerformanceMonitorTimer",
                                                                  t);

            performanceMonitors.Add(endPerformanceMonitor);

            return result;
        }
    }
}
