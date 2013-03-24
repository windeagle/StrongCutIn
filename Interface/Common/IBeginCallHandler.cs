namespace StrongCutIn.Interface.Common
{
    public interface IBeginCallHandler : IBeginHandler
    {
        void SetDelegate(Call baseDelegate, object baseObj);
        void ProxyMethod(object[] objs);
    }
}