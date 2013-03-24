using StrongCutIn.Interface.Common;

namespace StrongCutIn.Impl.Common
{
    public class BaseBeginCallHandler : IBeginCallHandler
    {
        public Call BaseDelegate { get; set; }
        public object BaseObj { get; set; }

        public void SetDelegate(Call baseDelegate, object baseObj)
        {
            BaseDelegate = baseDelegate;
            BaseObj = baseObj;
        }

        public virtual void ProxyMethod(object[] objs)
        {
        }
    }
}