namespace StrongCutIn.Interface.WithReturn.NoParam
{
    public interface ICallHandler<TR> : IAroundHandler
    {
        Call<TR> GetDelegate(Call<TR> innerDelegate, Call<TR> baseDelegate);
        TR ProxyMethod();
    }
}