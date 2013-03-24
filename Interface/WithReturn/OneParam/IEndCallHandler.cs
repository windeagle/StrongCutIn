namespace StrongCutIn.Interface.WithReturn.OneParam
{
    public interface IEndCallHandler<T, TR> : IEndHandler
    {
        void SetDelegate(Call<T, TR> baseDelegate, TR result);
        void ProxyMethod(T obj);
    }
}