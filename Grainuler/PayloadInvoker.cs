using CSharpFunctionalExtensions;
using Grainuler.Abstractions;
using Grainuler.DataTransferObjects;
using OneOf;
using System.Reflection;

namespace Grainuler
{
    public class PayloadInvoker : IPayloadInvoker
    {
        public async Task<object?> Invoke(Payload payload)
        {
            object? result = null;
            await Task.Run(() =>
            {
                try
                {
                    var invokable = payload.IsStatic ? LoadStatic(payload) : Load(payload);
                    if (invokable.HasValue)
                    {
                        //todo: after loading the invocable cache it to an object that implements IMemoryCache  interface, and try to retrieve it from there on next call.
                        result = invokable.Value.methodInfo.Invoke(invokable.Value.instance, payload.MethodParameters);
                    }
                }
                catch (Exception e)
                {
                    throw;
                  
                }
            });
            return result;
        }

        private static Maybe<(MethodInfo methodInfo, object? instance)> Load(Payload payload)
        {
            var invokeClass = LoadInvokeClassFromAssembly(payload);
            if (invokeClass.HasNoValue)
                return Maybe<(MethodInfo methodInfo, object? instance)>.None;
            var instanceType = invokeClass.Value.IsGenericType 
                ? invokeClass.Value.MakeGenericType(GetGenericTypes(payload.ConstructorGenericArguments)) 
                : invokeClass.Value;
            var currInsance = Activator.CreateInstance(instanceType, payload.ConstructorParameters);
            if (currInsance == null)
                return Maybe<(MethodInfo methodInfo, object? instance)>.None;
            var method = currInsance.GetType().GetMethod(payload.MethodName);
            if (method == null)
                return Maybe<(MethodInfo methodInfo, object? instance)>.None;
            method = MakeMethodGenericIfThereAreGenericArguments(payload, method);
            return (method, currInsance);


        }

        private static Maybe<(MethodInfo methodInfo, object? instance)> LoadStatic(Payload payload)
        {

            var invokeClass = LoadInvokeClassFromAssembly(payload);
            if (invokeClass.HasNoValue)
                return Maybe<(MethodInfo methodInfo, object? instance)>.None;
            var method = invokeClass.Value.GetMethod(payload.MethodName);
            if (method == null)
                return Maybe<(MethodInfo methodInfo, object? instance)>.None;
            method = MakeMethodGenericIfThereAreGenericArguments(payload, method);
            return (method, null);
        }

        private static MethodInfo MakeMethodGenericIfThereAreGenericArguments(Payload payload, MethodInfo? method)
        {
            if (payload.GenericMethodArguments != null && payload.GenericMethodArguments.Any())
            {
                Type[] types = GetGenericTypes(payload.GenericMethodArguments);
                return method.MakeGenericMethod(types);

            }
            return method;
        }
        private static Type[] GetGenericTypes(GenericArgument[] genericMethodArguments)
        {
            var result = new Type[genericMethodArguments.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Type.GetType($"{genericMethodArguments[i].TypeFullName}, {genericMethodArguments[i].TypeAssembly}");
            }
            return result;
        }

        private static Maybe<Type> LoadInvokeClassFromAssembly(Payload payload)
        {
            var assembly = Assembly.LoadFile(payload.AssemblyPath);

            if (assembly == null)
                return Maybe<Type>.None;

            var invokeClass = assembly?.GetTypes().Where(p => p.Name == payload.ClassName).FirstOrDefault();
            if (invokeClass == null)
                return Maybe<Type>.None;
            return invokeClass;
        }
    }
}
