using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using StrongCutIn.Util;

namespace StrongCutIn
{
    public class AOPMethodInfo
    {
        public MethodInfo OrgMethodInfo { get; set; }

        /// <summary>
        /// 格式如：System.Int32 i, System.String s
        /// </summary>
        public string ParamsStr { get; set; }

        // 格式如：<System.Int32, System.String>
        public string ParamsTypeStr { get; set; }

        // 格式如：<System.Int32, System.String, System.String> 假设返回类型为 System.String
        public string ParamsTypeStrWithReturnType { get; set; }

        /// <summary>
        /// 格式如：i, s
        /// </summary>
        public string ParamsNameStr { get; set; }

        /// <summary>
        /// 格式如：void/System.String
        /// </summary>
        public string ReturnTypeName { get; set; }
        // 为空或者为<System.String>
        public string ReturnTypeNameWithBlank { get; set; }
        /// <summary>
        /// 为空或者: "(System.String)"
        /// </summary>
        public string AsReturnTypeName { get; set; }
        /// <summary>
        /// 为空或者: "return (System.String)"
        /// </summary>
        public string AsReturnTypeNameWithReturn { get; set; }
        /// <summary>
        /// 为空或者: ",(System.String)ret"
        /// </summary>
        public string RetAsReturnTypeName { get; set; }
        /// <summary>
        /// 为空或者: " (System.String)ret"
        /// </summary>
        public string RetAsReturnTypeName2 { get; set; }

        /// <summary>
        /// 所用的接口名：Call/Action
        /// </summary>
        public string InterfaceName { get; set; }

        /// <summary>
        /// 原方法的名称（包括泛型定义）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 生成随机内部类所用的HashCode
        /// </summary>
        public string HashCode { get; set; }

        /// <summary>
        /// 注释标志
        /// </summary>
        public string Flag { get; set; }

        public List<string> ParamsTypeNameStrList { get; set; }
        public List<string> BeginHandlerNameStrList { get; set; }
        public List<string> EndHandlerNameStrList { get; set; }
        public List<string> AroundHandlerNameStrList { get; set; }  

        public AOPMethodInfo(MethodInfo methodInfo)
        {
            OrgMethodInfo = methodInfo;
            var parameters = methodInfo.GetParameters().OrderBy(p => p.Position);
            var isAction = methodInfo.ReturnType == typeof (void);
            var returnTypeFullName = (isAction ? "" : ProxyGen.GetFullName(methodInfo.ReturnType));

            var paramsStr = string.Empty;
            var paramsTypeStr = string.Empty;
            var paramsNameStr = string.Empty;
            ParamsTypeNameStrList = new List<string>();
            foreach (var param in parameters)
            {
                //TODO: 带默认值的可选会被变成不可选参数
                paramsStr += (param.IsOut ? " out" : " ") + ProxyGen.GetFullName(param.ParameterType) + " " + param.Name + ",";
                paramsTypeStr += ProxyGen.GetFullName(param.ParameterType) + ",";
                ParamsTypeNameStrList.Add(ProxyGen.GetFullName(param.ParameterType));
                paramsNameStr += param.Name + ",";
            }
            ParamsStr = paramsStr.TrimEnd(',');
            ParamsTypeStr = ("<" + paramsTypeStr.TrimEnd(',') + ">");
            ParamsTypeStr = ParamsTypeStr.Equals("<>") ? "" : ParamsTypeStr;
            ParamsTypeStrWithReturnType = "<" + (paramsTypeStr.TrimEnd(',') + (isAction ? "" : ("," + returnTypeFullName))).TrimStart(',') + ">";
            ParamsTypeStrWithReturnType = ParamsTypeStrWithReturnType.Equals("<>") ? "" : ParamsTypeStrWithReturnType;
            ParamsNameStr = paramsNameStr.TrimEnd(',');

            ReturnTypeName = (isAction ? "void" : returnTypeFullName);
            ReturnTypeNameWithBlank = isAction ? "" : ("<" + returnTypeFullName + ">");

            Name = ProxyGen.GetFullName(methodInfo);
            HashCode = Math.Abs(methodInfo.GetHashCode()).ToString(CultureInfo.InvariantCulture);

            InterfaceName = isAction ? "Action" : "Call";

            AsReturnTypeName = (isAction ? "" : ("(" + returnTypeFullName + ")"));
            RetAsReturnTypeName = (isAction ? "" : (", (" + returnTypeFullName + ")ret"));
            RetAsReturnTypeName2 = (isAction ? "" : (" (" + returnTypeFullName + ")ret"));
            AsReturnTypeNameWithReturn = (isAction ? "" : ("return (" + returnTypeFullName + ")"));

            var paramArray = ProxyGen.GetParameterTypes(methodInfo);
            Flag = paramArray.Length > 1 ? "//" : "";

            BeginHandlerNameStrList = new List<string>();
            AroundHandlerNameStrList = new List<string>();
            EndHandlerNameStrList = new List<string>();
        } 
    }
}
