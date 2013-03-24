using System;
using System.Collections.Generic;
using System.Reflection;
using StrongCutIn.Impl.Common;
using StrongCutIn.Impl.WithReturn.NoParam;
using StrongCutIn.Impl.WithReturn.OneParam;
using StrongCutIn.Impl.WithoutReturn.NoParam;
using StrongCutIn.Impl.WithoutReturn.OneParam;

namespace StrongCutIn.Util
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AOPHelperAttribute : Attribute
    {
        public AOPHelperAttribute(Type aopHandlerType, int sortIndex)
        {
            SortIndex = sortIndex;
            AopHandlerType = aopHandlerType;
        }

        public AOPHelperAttribute(string assemblyName, string handlerTypeFullName, int sortIndex)
        {
            SortIndex = sortIndex;
            try
            {
                AopHandlerType = Assembly.Load(assemblyName).GetType(handlerTypeFullName);
            }
            catch
            {}
        }

        public int SortIndex{ get; set;}        
        public Type AopHandlerType{ get; set;}
    }

    public abstract class AOPHandlerBagBase
    {
        public IList<KeyValuePair<Type, int>> InnerAOPHanderList { get; set; }

        protected AOPHandlerBagBase()
        {
            InnerAOPHanderList = new List<KeyValuePair<Type, int>>();
        }
    }

    public class AOPHandlerBag : AOPHandlerBagBase
    {
        public AOPHandlerBag()
        {
            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseBeginCallHandler<string, string>), 0));
            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseBeginCallHandler<int, string>), 0));
            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseBeginCallHandler<string>), 0));
            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseBeginCallHandler), 0));

            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseEndCallHandler<string, string>), 0));
            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseEndCallHandler<int, string>), 0));
            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseEndCallHandler<string>), 0));
            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseEndCallHandler), 0));

            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseCallHandler<string, string>), 0));
            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseCallHandler<int, string>), 0));
            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseCallHandler<string>), 0));
            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseCallHandler), 0));

            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseBeginActionHandler<string>), 0));
            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseBeginActionHandler<int>), 0));
            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseBeginActionHandler), 0));

            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseEndActionHandler<string>), 0));
            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseEndActionHandler<string>), 0));
            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseEndActionHandler), 0));

            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseActionHandler<string>), 0));
            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseActionHandler<int>), 0));
            InnerAOPHanderList.Add(new KeyValuePair<Type, int>(typeof (BaseActionHandler), 0));
        }
    }
}