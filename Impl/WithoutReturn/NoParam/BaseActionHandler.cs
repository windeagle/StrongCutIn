using StrongCutIn.Interface.WithoutReturn.NoParam;

namespace StrongCutIn.Impl.WithoutReturn.NoParam
{
    public class BaseActionHandler : IActionHandler
    {
        public Action InnerDelegate { get; set; }
        public Action BaseDelegate { get; set; }

        public Action GetDelegate(Action innerDelegate, Action baseDelegate)
        {
            BaseDelegate = baseDelegate;
            InnerDelegate = innerDelegate;
            return ProxyMethod;
        }

        public virtual void ProxyMethod()
        {
            InnerDelegate();
        }
    }
}