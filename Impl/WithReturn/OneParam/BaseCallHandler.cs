using StrongCutIn.Interface.WithReturn.OneParam;

namespace StrongCutIn.Impl.WithReturn.OneParam
{
    public class BaseCallHandler<T, TR> : ICallHandler<T, TR>
    {
        public Call<T, TR> InnerDelegate { get; set; }
        public Call<T, TR> BaseDelegate { get; set; }

        public Call<T, TR> GetDelegate(Call<T, TR> innerDelegate, Call<T, TR> baseDelegate)
        {
            BaseDelegate = baseDelegate;
            InnerDelegate = innerDelegate;
            return ProxyMethod;
        }

        public virtual TR ProxyMethod(T obj)
        {
            return InnerDelegate(obj);
        }
    }
}