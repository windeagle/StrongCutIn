namespace StrongCutIn.Interface.Common
{
    public interface IEndCallHandler : IEndHandler
    {
        void SetDelegate(Call baseDelegate, object baseObj, object result);
        void ProxyMethod(object[] objs);
    }
}