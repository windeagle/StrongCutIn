using StrongCutIn.Interface.WithoutReturn.OneParam;

namespace StrongCutIn.Impl.WithoutReturn.OneParam
{
    public class BaseActionHandler<T> : IActionHandler<T>
    {
        public Action<T> InnerDelegate { get; set; }
        public Action<T> BaseDelegate { get; set; }

        public Action<T> GetDelegate(Action<T> innerDelegate, Action<T> baseDelegate)
        {
            BaseDelegate = baseDelegate;
            InnerDelegate = innerDelegate;
            return ProxyMethod;
        }

        public virtual void ProxyMethod(T obj)
        {
            InnerDelegate(obj);
        }
    }
}