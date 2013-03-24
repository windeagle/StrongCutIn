namespace StrongCutIn.Interface.WithoutReturn.OneParam
{
    public interface IBeginActionHandler<T> : IBeginHandler
    {
        void SetDelegate(Action<T> baseDelegate);
        void ProxyMethod(T p);
    }
}