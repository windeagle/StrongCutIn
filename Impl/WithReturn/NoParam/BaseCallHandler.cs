using StrongCutIn.Interface.WithReturn.NoParam;

namespace StrongCutIn.Impl.WithReturn.NoParam
{
    public class BaseCallHandler<TR> : ICallHandler<TR>
    {
        public Call<TR> InnerDelegate { get; set; }
        public Call<TR> BaseDelegate { get; set; }

        public Call<TR> GetDelegate(Call<TR> innerDelegate, Call<TR> baseDelegate)
        {
            BaseDelegate = baseDelegate;
            InnerDelegate = innerDelegate;
            return ProxyMethod;
        }

        public virtual TR ProxyMethod()
        {
            return InnerDelegate();
        }
    }
}