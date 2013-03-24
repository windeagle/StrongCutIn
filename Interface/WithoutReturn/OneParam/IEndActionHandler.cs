namespace StrongCutIn.Interface.WithoutReturn.OneParam
{
    public interface IEndActionHandler<T> : IEndHandler
    {
        void SetDelegate(Action<T> baseDelegate);
        void ProxyMethod(T p);
    }
}