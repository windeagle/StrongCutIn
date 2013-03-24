namespace StrongCutIn.Interface.Common
{
    public interface ICallHandler : IAroundHandler
    {
        Call GetDelegate(Call innerDelegate, Call baseDelegate, object baseObj);
        object ProxyMethod(object[] objs);
    }
}