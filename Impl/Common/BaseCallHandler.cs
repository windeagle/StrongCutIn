using StrongCutIn.Interface.Common;

namespace StrongCutIn.Impl.Common
{
    public class BaseCallHandler : ICallHandler
    {
        public Call InnerDelegate { get; set; }
        public object BaseObj { get; set; }
        public Call BaseDelegate { get; set; }

        #region ICallHandler Members

        public Call GetDelegate(Call innerDelegate, Call baseDelegate, object baseObj)
        {
            BaseDelegate = baseDelegate;
            BaseObj = baseObj;
            InnerDelegate = innerDelegate;
            return ProxyMethod;
        }

        #endregion

        public virtual object ProxyMethod(object[] objs)
        {
            return InnerDelegate(objs);
        }
    }
}