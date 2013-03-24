namespace StrongCutIn.Interface.WithoutReturn.NoParam
{
    public interface IActionHandler : IAroundHandler
    {
        Action GetDelegate(Action innerDelegate, Action baseDelegate);
        void ProxyMethod();
    }
}