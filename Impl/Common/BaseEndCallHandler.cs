using StrongCutIn.Interface.Common;

namespace StrongCutIn.Impl.Common
{
    public class BaseEndCallHandler : IEndCallHandler
    {
        public Call BaseDelegate { get; set; }
        public object BaseObj { get; set; }
        public object Result { get; set; }

        public void SetDelegate(Call baseDelegate, object baseObj, object result)
        {
            BaseDelegate = baseDelegate;
            BaseObj = baseObj;
            Result = result;
        }

        public virtual void ProxyMethod(object[] objs)
        {
        }
    }
}