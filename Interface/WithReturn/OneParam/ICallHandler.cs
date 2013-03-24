namespace StrongCutIn.Interface.WithReturn.OneParam
{
    public interface ICallHandler<T, TR> : IAroundHandler
    {
        Call<T, TR> GetDelegate(Call<T, TR> innerDelegate, Call<T, TR> baseDelegate);
        TR ProxyMethod(T obj);
    }
}