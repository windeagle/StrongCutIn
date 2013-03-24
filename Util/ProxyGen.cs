using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using NVelocity;
using NVelocity.App;
using NVelocity.Runtime;
using StrongCutIn.Interface;

namespace StrongCutIn.Util
{
    public static class ProxyGen
    {
        //得到一个方法所有参数的Type数组，这个方法在以后会被多次调用
        public static Type[] GetParameterTypes(MethodInfo methodInfo)
        {
            ParameterInfo[] args = methodInfo.GetParameters();
            Type[] argsType = new Type[args.Length];
            for (Int32 j = 0; j < args.Length; j++) { argsType[j] = args[j].ParameterType; }
            return argsType;
        }

        public static Type MakeGenericType(Type type, MethodInfo m)
        {
            var args = type.GetGenericArguments().OrderBy(p => p.GenericParameterPosition);
            if (args.Count() > 2)
                return null;
            var types = new List<Type>();
            foreach (var a in args)
            {
                if (a.Name.Equals("T"))
                {
                    var paramArray = GetParameterTypes(m);
                    if (paramArray.Length == 0)
                        return null;
                    types.Add(paramArray[0]);
                }
                else if (a.Name.Equals("TR"))
                {
                    if (m.ReturnType == typeof(void))
                        return null;
                    types.Add(m.ReturnType);
                }
                else
                    return null;
            }
            //type.GetGenericParameterConstraints();
            //type.GetGenericTypeDefinition();
            return type.MakeGenericType(types.ToArray());
        }

        public static string GetFullName(MethodInfo methodInfo)
        {
            if (methodInfo.IsGenericMethod)
            {
                string fullName = methodInfo.Name;
                var args = methodInfo.GetGenericArguments();
                fullName += "<";
                fullName = args.Aggregate(fullName, (current, a) => current + (GetFullName(a) + ","));
                fullName = fullName.TrimEnd(',');
                fullName += ">";
                return fullName;
            }
            return methodInfo.Name;
        }

        public static string GetFullName(Type type)
        {
            if (type.IsGenericType)
            {
                if (type.FullName != null)
                {
                    string fullName = type.FullName.Substring(0, type.FullName.IndexOf('`'));
                    var args = type.GetGenericArguments();
                    fullName += "<";
                    fullName = args.Aggregate(fullName, (current, a) => current + (GetFullName(a) + ","));
                    fullName = fullName.TrimEnd(',');
                    fullName += ">";
                    return fullName;
                }
            }
            if(type.IsNested)
            {
                if (type.FullName != null) return type.FullName.Replace('+', '.');
            }
            return type.FullName;
        }

        private static Boolean MethodInfoEqual(MethodInfo mi, MethodInfo mii)
        {
            if (mi.IsSpecialName || mii.IsSpecialName) return false;
            if (mi.Name != mii.Name) return false;
            if (mi.ReturnType != mii.ReturnType) return false;
            ParameterInfo[] pis = mi.GetParameters();
            ParameterInfo[] pisi = mii.GetParameters();
            if (pis.Length != pisi.Length) return false;
            for (int i = 0; i < pis.Length; i++)
            {
                ParameterInfo pi = pis[i];
                ParameterInfo pii = pisi[i];
                if (pi.ParameterType != pii.ParameterType) return false;
            }
            return true;
        }

        public static IDictionary<Type, KeyValuePair<Type, IList<object>>> ProxyTypeCache = new Dictionary<Type, KeyValuePair<Type, IList<object>>>();
        private static object _lockObj = new object();

        public static object Gen(object theObj)
        {
            var ret = GenPrivate(theObj);
            if (ret != null)
                return ret;
            ret = GenPrivate(theObj);
            if (ret != null)
                return ret;
            return theObj;
        }

        private static object GenPrivate(object theObj)
        {
            Type theType = theObj.GetType();
            if (ProxyTypeCache.ContainsKey(theType))
            {
                var typeSame = theType == ProxyTypeCache[theType].Key;
                foreach (dynamic existsProxyInstance in ProxyTypeCache[theType].Value)
                {
                    if (typeSame)
                    {
                        if (theObj == existsProxyInstance)
                            return existsProxyInstance;
                    }
                    else
                    {
                        if (theObj == existsProxyInstance.Target)
                            return existsProxyInstance;
                    }
                }
                if (typeSame) return theObj;
            }
            return Gen(theType, theObj);
        }

        public static object Gen(Type theType, object theObj = null)
        {
            if(ProxyTypeCache.ContainsKey(theType))
            {
                var proxyType = ProxyTypeCache[theType].Key;
                var typeSame = theType == ProxyTypeCache[theType].Key;
                var instance = (!typeSame && theObj == null) ? Activator.CreateInstance(proxyType) : Activator.CreateInstance(proxyType, theObj);
                //if(!typeSame && theObj != null)
                //    proxyType.InvokeMember("Target",
                //                       BindingFlags.Instance | BindingFlags.Instance | BindingFlags.Public,
                //                       null, instance, new object[] { theObj });
                if (ProxyTypeCache[theType].Value.Count <= 10)
                {
                    lock (_lockObj)
                    {
                        ProxyTypeCache[theType].Value.Add(instance);
                    }
                }
                return instance;
            }

            List<AOPMethodInfo> mList = new List<AOPMethodInfo>();

            List<string> pList = new List<string>();
            var pisInterface = theType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var p in pisInterface)
            {
                var sbp = new StringBuilder();
                sbp.Append("        public").Append(" ").Append(GetFullName(p.PropertyType)).Append(" ").AppendLine(p.Name);
                sbp.AppendLine("        {");
                if (p.CanRead && (p.GetGetMethod() != null && !p.GetGetMethod().IsPrivate))
                    sbp.Append("            get { return _target.").Append(p.Name).AppendLine("; }");
                if (p.CanWrite && (p.GetSetMethod() != null && !p.GetSetMethod().IsPrivate))
                    sbp.Append("            set { _target.").Append(p.Name).AppendLine(" = value; }");
                sbp.AppendLine("        }");
                pList.Add(sbp.ToString());
            }

            var miList = theType.GetMethods().Where(p => p.IsVirtual);
            foreach (var m in miList)
            {
                var theM = new AOPMethodInfo(m);

                var tmpH = theType.GetCustomAttributes(typeof(AOPHelperAttribute), true).ToList();
                tmpH.AddRange(m.GetCustomAttributes(typeof(AOPHelperAttribute), true));
                var interfaces = theType.GetInterfaces();
                foreach (var inter in interfaces)
                {
                    var mis = inter.GetMethods();
                    foreach (var mmmm in mis)
                    {
                        if (MethodInfoEqual(mmmm, m))
                        {
                            tmpH.AddRange(mmmm.GetCustomAttributes(typeof(AOPHelperAttribute), false));
                            tmpH.AddRange(inter.GetCustomAttributes(typeof(AOPHelperAttribute), false));
                            break;
                        }
                    }
                }

                var aopHelperAttributes = tmpH.Cast<AOPHelperAttribute>().OrderBy(p => p.SortIndex).ToList();

                List<Type> handlerTypes = new List<Type>();
                var tempType = typeof (AOPHandlerBagBase);
                foreach (var aopHelperAttribute in aopHelperAttributes)
                {
                    if (aopHelperAttribute.AopHandlerType.IsSubclassOf(tempType))
                    {
                        var tmpBag = Activator.CreateInstance(aopHelperAttribute.AopHandlerType);
                        var aopHandlerBagBase = tmpBag as AOPHandlerBagBase;
                        if (aopHandlerBagBase != null)
                            handlerTypes.AddRange(aopHandlerBagBase.InnerAOPHanderList.OrderBy(p=>p.Value).Select(p=>p.Key));
                    }
                    else
                    {
                        if (aopHelperAttribute.AopHandlerType != null)
                            handlerTypes.Add(aopHelperAttribute.AopHandlerType);
                    }
                }

                //handlerTypes = tmpH.Cast<AOPHelperAttribute>().OrderBy(p => p.SortIndex).Select(p => p.AopHandlerType).ToList();

                foreach (var t in handlerTypes)
                {
                    var type = t;
                    if (type != null && type.ContainsGenericParameters)
                    {
                        type = MakeGenericType(type, m);
                    }

                    //if (type != null && type.GetInterfaces().Contains(typeof(IAroundHandler)))
                    if (type != null && type.GetInterface(typeof(IAroundHandler).FullName) != null)
                    {
                        theM.AroundHandlerNameStrList.Add(GetFullName(type));
                    }
                    if (type != null && type.GetInterface(typeof(IBeginHandler).FullName) != null)
                    {
                        theM.BeginHandlerNameStrList.Add(GetFullName(type));
                    }
                    if (type != null && type.GetInterface(typeof(IEndHandler).FullName) != null)
                    {
                        theM.EndHandlerNameStrList.Add(GetFullName(type));
                    }
                }

                mList.Add(theM);
            }

            if (mList.All(p => !p.AroundHandlerNameStrList.Any() && !p.BeginHandlerNameStrList.Any() && !p.EndHandlerNameStrList.Any()))
            {
                var instance = theObj ?? Activator.CreateInstance(theType);

                if (!ProxyTypeCache.ContainsKey(theType))
                {
                    lock (_lockObj)
                    {
                        if (!ProxyTypeCache.ContainsKey(theType))
                            ProxyTypeCache.Add(theType, new KeyValuePair<Type, IList<object>>(theType, new List<object> { instance }));
                    }
                }
                return instance;
            }

            object proxyInstance = null;
            if (!ProxyTypeCache.ContainsKey(theType))
            {
                lock (_lockObj)
                {
                    if (!ProxyTypeCache.ContainsKey(theType))
                    {
                        proxyInstance = GenProxy(GetProxyClassStr(theType, mList, pList), theType.Name + theType.GetHashCode(), null, theObj);
                        ProxyTypeCache.Add(theType, new KeyValuePair<Type, IList<object>>(proxyInstance.GetType(), new List<object> { proxyInstance }));
                    }
                }
            }

            return proxyInstance;
        }

        //private static readonly IDictionary<string, Template> DataGramTemplates = new Dictionary<string, Template>();
        //private static readonly VelocityEngine VltEngine = new VelocityEngine();

        //static ProxyGen()
        //{
        //    // 文件型模板, 还可以是 "assembly", 则使用资源文件
        //    VltEngine.SetProperty(RuntimeConstants.RESOURCE_LOADER, "file");
        //    // 模板存放目录
        //    //vltEngine.SetProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, Path.GetDirectoryName(HttpContext.Current.Server.MapPath("./")));
        //    //vltEngine.SetProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase));
        //    VltEngine.SetProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH,
        //                          AppDomain.CurrentDomain.BaseDirectory + "Resources\\NVelocity\\");
        //    VltEngine.Init();
        //}

        private static string GetProxyClassStr(Type innerClass, List<AOPMethodInfo> mList, List<string> pList)
        {
            VelocityEngine vltEngine = new VelocityEngine();
            vltEngine.Init();

            var vltContext = new VelocityContext();
            vltContext.Put("InnerClassName", GetFullName(innerClass));
            vltContext.Put("InnerClassName2", innerClass.Name + innerClass.GetHashCode());
            vltContext.Put("Methods", mList);
            vltContext.Put("Properties", pList);

            var vltWriter = new StringWriter();

            vltEngine.Evaluate(vltContext, vltWriter, null, AOPClassTemp.ProxyClassTemp);

            //Template template;
            //if (DataGramTemplates.ContainsKey("ClassTemp"))
            //    template = DataGramTemplates["ClassTemp"];
            //else
            //{
            //    template = VltEngine.GetTemplate("AOPClassTemp.vm");
            //    DataGramTemplates.Add("ClassTemp", template);
            //}
            //template.Merge(vltContext, vltWriter);

            return vltWriter.GetStringBuilder().ToString();
        }

        private static object GenProxy(string code, string className, IEnumerable<string> arrReferencedAssemblies, object theObj = null)
        {
            CSharpCodeProvider objCSharpCodePrivoder = new CSharpCodeProvider();

            CompilerParameters objCompilerParameters = new CompilerParameters();
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    objCompilerParameters.ReferencedAssemblies.Add(a.Location);
                }
                catch { }
            }

            if (arrReferencedAssemblies != null)
                foreach (string item in arrReferencedAssemblies)
                    objCompilerParameters.ReferencedAssemblies.Add(item);
            objCompilerParameters.GenerateExecutable = false;
            objCompilerParameters.GenerateInMemory = true;

            CompilerResults cr = (objCSharpCodePrivoder.CompileAssemblyFromSource(objCompilerParameters, new[] { code }));

            if (cr.Errors.HasErrors)
            {
                StringBuilder errsb = new StringBuilder("编译错误：");
                foreach (CompilerError err in cr.Errors)
                {
                    errsb.AppendLine(err.ErrorText);
                }
                throw new Exception("动态代理生成出错：" + errsb);
            }

            Assembly objAssembly = cr.CompiledAssembly;

            if (theObj != null)
            {
                Type tmpClassType = objAssembly.GetType("AOPTmpClassNameSpace." + className + "_TmpAOP");
                return Activator.CreateInstance(tmpClassType, theObj);
            }
            
            var tempObj = objAssembly.CreateInstance("AOPTmpClassNameSpace." + className + "_TmpAOP");

            //Type tmpClassType = objAssembly.GetType("AOPTmpClassNameSpace." + className + "_TmpAOP");
            //if (theObj != null)
            //    tmpClassType.InvokeMember("Target",
            //                       BindingFlags.Instance | BindingFlags.Instance | BindingFlags.Public,
            //                       null, tempObj, new object[] { theObj });
            return tempObj;
        }
    } 
}
