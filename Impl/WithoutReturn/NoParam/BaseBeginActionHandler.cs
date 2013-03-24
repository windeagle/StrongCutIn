using StrongCutIn.Interface.WithoutReturn.NoParam;

namespace StrongCutIn.Impl.WithoutReturn.NoParam
{
    public class BaseBeginActionHandler : IBeginActionHandler
    {
        public Action BaseDelegate { get; set; }

        public void SetDelegate(Action baseDelegate)
        {
            BaseDelegate = baseDelegate;
        }

        public virtual void ProxyMethod()
        {
        }
    }
}