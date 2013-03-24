using StrongCutIn.Interface.WithReturn.OneParam;

namespace StrongCutIn.Impl.WithReturn.OneParam
{
    public class BaseBeginCallHandler<T, TR> : IBeginCallHandler<T, TR>
    {
        public Call<T, TR> BaseDelegate { get; set; }

        public void SetDelegate(Call<T, TR> baseDelegate)
        {
            BaseDelegate = baseDelegate;
        }

        public virtual void ProxyMethod(T obj)
        {
        }
    }
}