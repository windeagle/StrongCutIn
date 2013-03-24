namespace StrongCutIn.Interface.WithoutReturn.NoParam
{
    public interface IBeginActionHandler : IBeginHandler
    {
        void SetDelegate(Action baseDelegate);
        void ProxyMethod();
    }
}