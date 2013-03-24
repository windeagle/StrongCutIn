namespace StrongCutIn.Interface.WithoutReturn.OneParam
{
    public interface IActionHandler<T> : IAroundHandler
    {
        Action<T> GetDelegate(Action<T> innerDelegate, Action<T> baseDelegate);
        void ProxyMethod(T p);
    }
}