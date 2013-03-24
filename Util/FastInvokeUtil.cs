using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace StrongCutIn.Util
{
    public static class TypeUtility
    {
        private static readonly IDictionary<MethodInfo, FastInvokeHandler> ExecutorCache = new Dictionary<MethodInfo, FastInvokeHandler>();
        private static readonly object LockObj = new object();
        private static readonly IDictionary<PropertyInfo, MemberGetDelegate> GetDelegateCache = new Dictionary<PropertyInfo, MemberGetDelegate>();
        private static readonly object GetDelegateLockObj = new object();
        private static readonly IDictionary<PropertyInfo, MemberSetDelegate> SetDelegateCache = new Dictionary<PropertyInfo, MemberSetDelegate>();
        private static readonly object SetDelegateLockObj = new object();

        public delegate object MemberGetDelegate(object target);

        public delegate object MemberSetDelegate(object target, object value);

        public delegate object FastInvokeHandler(object target, object[] paramters);

        public static MemberGetDelegate GetMemberGetDelegate(PropertyInfo propertyInfo)
        {
            if (GetDelegateCache.ContainsKey(propertyInfo))
            {
                return GetDelegateCache[propertyInfo];
            }

            // Member is a Property...
            Type objectType = propertyInfo.DeclaringType;

            MethodInfo mi = propertyInfo.GetGetMethod();

            if (mi != null)
            {
                var inner = new DynamicMethodExecutor(mi);
                MemberGetDelegate invoker = target => inner.Execute(target, new object[] { });

                lock (GetDelegateLockObj)
                {
                    if (!GetDelegateCache.ContainsKey(propertyInfo))
                    {
                        GetDelegateCache.Add(propertyInfo, invoker);
                    }
                }
                return invoker;
            }
            throw new Exception(String.Format(
                "Property: '{0}' of Type: '{1}' does" +
                " not have a Public Get accessor",
                propertyInfo.Name, objectType.Name));
        }
        public static MemberSetDelegate GetMemberSetDelegate(PropertyInfo propertyInfo)
        {
            if (SetDelegateCache.ContainsKey(propertyInfo))
            {
                return SetDelegateCache[propertyInfo];
            }

            // Member is a Property...
            Type objectType = propertyInfo.DeclaringType;

            MethodInfo mi = propertyInfo.GetSetMethod();

            if (mi != null)
            {
                var inner = new DynamicMethodExecutor(mi);
                MemberSetDelegate invoker = (target, value) => inner.Execute(target, new[] { value });

                lock (SetDelegateLockObj)
                {
                    if (!SetDelegateCache.ContainsKey(propertyInfo))
                    {
                        SetDelegateCache.Add(propertyInfo, invoker);
                    }
                }
                return invoker;
            }
            throw new Exception(String.Format(
                "Property: '{0}' of Type: '{1}' does" +
                " not have a Public Set accessor",
                propertyInfo.Name, objectType.Name));
        }

        //public static MemberGetDelegate
        //    GetMemberGetDelegate(FieldInfo fieldInfo)
        //{
        //        // Member is a Field...
        //        Type objectType = fieldInfo.DeclaringType;

        //        try
        //        {
        //            DynamicMethod dm = new DynamicMethod("Get" + fieldInfo.Name,
        //                                                 typeof(Object), new[] { typeof(Object) }, objectType);
        //            ILGenerator il = dm.GetILGenerator();
        //            // Load the instance of the object (argument 0) onto the stack
        //            il.Emit(OpCodes.Ldarg_0);
        //            // Load the value of the object's field (fi) onto the stack
        //            il.Emit(OpCodes.Ldfld, fieldInfo);
        //            // return the value on the top of the stack
        //            il.Emit(OpCodes.Ret);

        //            return (MemberGetDelegate)
        //                   dm.CreateDelegate(typeof(MemberGetDelegate));
        //        }
        //        catch
        //        {
        //            throw new Exception(String.Format(
        //                "Member: '{0}' is not a Public Property or Field of Type: '{1}'",
        //                fieldInfo.Name, objectType.Name));
        //        }
        //}

        ////有问题的
        //public static MemberSetDelegate
        //    GetMemberSetDelegate(FieldInfo fieldInfo)
        //{
        //    // Member is a Field...
        //    Type objectType = fieldInfo.DeclaringType;

        //    try
        //    {
        //        DynamicMethod dm = new DynamicMethod("Set" + fieldInfo.Name,
        //                                             typeof(Object), new[] { typeof(Object) }, objectType);
        //        ILGenerator il = dm.GetILGenerator();
        //        // Load the instance of the object (argument 0) onto the stack
        //        il.Emit(OpCodes.Ldarg_0);
        //        // Load the value of the object's field (fi) onto the stack
        //        il.Emit(OpCodes.Ldfld, fieldInfo);
        //        // return the value on the top of the stack
        //        il.Emit(OpCodes.Ret);

        //        return (MemberSetDelegate)
        //               dm.CreateDelegate(typeof(MemberSetDelegate));
        //    }
        //    catch
        //    {
        //        throw new Exception(String.Format(
        //            "Member: '{0}' is not a Public Property or Field of Type: '{1}'",
        //            fieldInfo.Name, objectType.Name));
        //    }
        //}

        #region DynamicMethod 辅助方法
        public static FastInvokeHandler GetMethodInvoker(MethodInfo methodInfo)
        {
            if (ExecutorCache.ContainsKey(methodInfo))
            {
                return ExecutorCache[methodInfo];
            }

            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, typeof(object), new[] { typeof(object), typeof(object[]) }, methodInfo.DeclaringType.Module);
            ILGenerator il = dynamicMethod.GetILGenerator();

            ParameterInfo[] ps = methodInfo.GetParameters();

            Type[] paramTypes = new Type[ps.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    paramTypes[i] = ps[i].ParameterType.GetElementType();
                else
                    paramTypes[i] = ps[i].ParameterType;
            }

            LocalBuilder[] locals = new LocalBuilder[paramTypes.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                locals[i] = il.DeclareLocal(paramTypes[i], true);
            }

            for (int i = 0; i < paramTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                EmitFastInt(il, i);
                il.Emit(OpCodes.Ldelem_Ref);
                EmitCastToReference(il, paramTypes[i]);
                il.Emit(OpCodes.Stloc, locals[i]);
            }

            if (!methodInfo.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
            }

            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    il.Emit(OpCodes.Ldloca_S, locals[i]);
                else
                    il.Emit(OpCodes.Ldloc, locals[i]);
            }

            if (methodInfo.IsStatic)
                il.EmitCall(OpCodes.Call, methodInfo, null);
            else
                il.EmitCall(OpCodes.Callvirt, methodInfo, null);

            if (methodInfo.ReturnType == typeof(void))
                il.Emit(OpCodes.Ldnull);
            else
                EmitBoxIfNeeded(il, methodInfo.ReturnType);

            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                {
                    il.Emit(OpCodes.Ldarg_1);
                    EmitFastInt(il, i);
                    il.Emit(OpCodes.Ldloc, locals[i]);
                    if (locals[i].LocalType.IsValueType)
                        il.Emit(OpCodes.Box, locals[i].LocalType);
                    il.Emit(OpCodes.Stelem_Ref);
                }
            }

            il.Emit(OpCodes.Ret);
            FastInvokeHandler invoder = (FastInvokeHandler)dynamicMethod.CreateDelegate(typeof(FastInvokeHandler));

            lock (LockObj)
            {
                if (!ExecutorCache.ContainsKey(methodInfo))
                {
                    ExecutorCache.Add(methodInfo, invoder);
                }
            }
            return invoder;
        }

        private static void EmitCastToReference(ILGenerator il, Type type)
        {
            il.Emit(type.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, type);
        }

        private static void EmitBoxIfNeeded(ILGenerator il, Type type)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Box, type);
            }
        }

        private static void EmitFastInt(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1:
                    il.Emit(OpCodes.Ldc_I4_M1);
                    return;
                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    return;
                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    return;
                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    return;
                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    return;
                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    return;
                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    return;
                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    return;
                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    return;
                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    return;
            }

            if (value > -129 && value < 128)
            {
                il.Emit(OpCodes.Ldc_I4_S, (SByte)value);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, value);
            }
        }
        #endregion
    }

    public class DynamicMethodExecutor
    {
        private static readonly IDictionary<MethodInfo, Func<object, object[], object>> ExecutorCache = new Dictionary<MethodInfo, Func<object, object[], object>>();
        private static readonly object LockObj = new object();

        private readonly Func<object, object[], object> _execute;

        public DynamicMethodExecutor(MethodInfo methodInfo)
        {
            if (ExecutorCache.ContainsKey(methodInfo))
            {
                _execute = ExecutorCache[methodInfo];
                return;
            }
            _execute = GetExecuteDelegate(methodInfo);
            lock (LockObj)
            {
                if (!ExecutorCache.ContainsKey(methodInfo))
                {
                    ExecutorCache.Add(methodInfo, _execute);
                }
            }
        }

        public object Execute(object instance, object[] parameters)
        {
            return _execute(instance, parameters);
        }

        private Func<object, object[], object> GetExecuteDelegate(MethodInfo methodInfo)
        {
            // parameters to execute
            ParameterExpression instanceParameter =
                Expression.Parameter(typeof(object), "instance");
            ParameterExpression parametersParameter =
                Expression.Parameter(typeof(object[]), "parameters");

            // build parameter list
            List<Expression> parameterExpressions = new List<Expression>();
            ParameterInfo[] paramInfos = methodInfo.GetParameters();
            for (int i = 0; i < paramInfos.Length; i++)
            {
                // (Ti)parameters[i]
                BinaryExpression valueObj = Expression.ArrayIndex(
                    parametersParameter, Expression.Constant(i));

                var parameterType = paramInfos[i].ParameterType;
                UnaryExpression valueCast = Expression.Convert(valueObj, parameterType.IsByRef ? parameterType.GetElementType() : parameterType);

                parameterExpressions.Add(valueCast);
            }

            // non-instance for static method, or ((TInstance)instance)
            Expression instanceCast = methodInfo.IsStatic ? null :
                Expression.Convert(instanceParameter, methodInfo.ReflectedType);

            // static invoke or ((TInstance)instance).Method
            MethodCallExpression methodCall = Expression.Call(
                instanceCast, methodInfo, parameterExpressions);

            // ((TInstance)instance).Method((T0)parameters[0], (T1)parameters[1], ...)
            if (methodCall.Type == typeof(void))
            {
                Expression<Action<object, object[]>> lambda =
                    Expression.Lambda<Action<object, object[]>>(
                        methodCall, instanceParameter, parametersParameter);

                Action<object, object[]> execute = lambda.Compile();
                return (instance, parameters) =>
                {
                    execute(instance, parameters);
                    return null;
                };
            }
            else
            {
                UnaryExpression castMethodCall = Expression.Convert(
                    methodCall, typeof(object));
                Expression<Func<object, object[], object>> lambda =
                    Expression.Lambda<Func<object, object[], object>>(
                        castMethodCall, instanceParameter, parametersParameter);

                return lambda.Compile();
            }
        }
    }
}