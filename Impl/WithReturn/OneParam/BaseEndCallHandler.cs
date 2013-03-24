using StrongCutIn.Interface.WithReturn.OneParam;

namespace StrongCutIn.Impl.WithReturn.OneParam
{
    public class BaseEndCallHandler<T, TR> : IEndCallHandler<T, TR>
    {
        public Call<T, TR> BaseDelegate { get; set; }
        public TR Result { get; set; }

        public void SetDelegate(Call<T, TR> baseDelegate, TR result)
        {
            BaseDelegate = baseDelegate;
            Result = result;
        }

        public virtual void ProxyMethod(T obj)
        {
        }
    }
}