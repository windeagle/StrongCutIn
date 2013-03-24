using StrongCutIn.Interface.WithReturn.NoParam;

namespace StrongCutIn.Impl.WithReturn.NoParam
{
    public class BaseBeginCallHandler<TR> : IBeginCallHandler<TR>
    {
        public Call<TR> BaseDelegate { get; set; }

        public void SetDelegate(Call<TR> baseDelegate)
        {
            BaseDelegate = baseDelegate;
        }

        public virtual void ProxyMethod()
        {
        }
    }
}