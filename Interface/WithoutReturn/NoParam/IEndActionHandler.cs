namespace StrongCutIn.Interface.WithoutReturn.NoParam
{
    public interface IEndActionHandler : IEndHandler
    {
        void SetDelegate(Action baseDelegate);
        void ProxyMethod();
    }
}