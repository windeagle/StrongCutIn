namespace StrongCutIn.Interface.WithReturn.NoParam
{
    public interface IBeginCallHandler<TR> : IBeginHandler
    {
        void SetDelegate(Call<TR> baseDelegate);
        void ProxyMethod();
    }
}