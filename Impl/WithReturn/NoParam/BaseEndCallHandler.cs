using StrongCutIn.Interface.WithReturn.NoParam;

namespace StrongCutIn.Impl.WithReturn.NoParam
{
    public class BaseEndCallHandler<TR> : IEndCallHandler<TR>
    {
        public Call<TR> BaseDelegate { get; set; }
        public TR Result { get; set; }

        public void SetDelegate(Call<TR> baseDelegate, TR result)
        {
            BaseDelegate = baseDelegate;
            Result = result;
        }

        public virtual void ProxyMethod()
        {
        }
    }
}