using StrongCutIn.Interface.WithoutReturn.NoParam;

namespace StrongCutIn.Impl.WithoutReturn.NoParam
{
    public class BaseEndActionHandler : IEndActionHandler
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