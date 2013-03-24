namespace StrongCutIn.Interface.WithReturn.NoParam
{
    public interface IEndCallHandler<TR> : IEndHandler
    {
        void SetDelegate(Call<TR> baseDelegate, TR result);
        void ProxyMethod();
    }
}