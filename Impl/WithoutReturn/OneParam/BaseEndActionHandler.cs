using StrongCutIn.Interface.WithoutReturn.OneParam;

namespace StrongCutIn.Impl.WithoutReturn.OneParam
{
    public class BaseEndActionHandler<T> : IEndActionHandler<T>
    {
        public Action<T> BaseDelegate { get; set; }

        public void SetDelegate(Action<T> baseDelegate)
        {
            BaseDelegate = baseDelegate;
        }

        public virtual void ProxyMethod(T p)
        {
        }
    }
}