namespace StrongCutIn.Interface.WithReturn.OneParam
{
    public interface IBeginCallHandler<T, TR> : IBeginHandler
    {
        void SetDelegate(Call<T, TR> baseDelegate);
        void ProxyMethod(T obj);
    }
}