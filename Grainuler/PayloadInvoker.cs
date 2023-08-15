using CSharpFunctionalExtensions;
using Grainuler.Abstractions;
using Grainuler.DataTransferObjects;
using System.Reflection;

namespace Grainuler
{
    internal class PayloadInvoker
    {
        public static  Task Invoke(Payload payload)
        {
           return Task.Run(()=> { 
               var invokable = payload.IsStatic ? LoadStatic(payload) : Load(payload);
               if (invokable.HasValue)
               {
                   //todo: after loading the invocable cache it to redis, and try to retrieve it from there on next call.
                   var result = invokable.Value.methodInfo.Invoke(invokable.Value.instance, payload.MethodArguments);
               }
           });
        }

        private static Maybe<(MethodInfo methodInfo, object? instance)> Load(Payload payload)
        {
            var invokeClass = LoadInvokeClassFromAssembly(payload);
            if (invokeClass.HasNoValue)
                return Maybe<(MethodInfo methodInfo, object? instance)>.None;

            var currInsance = Activator.CreateInstance(invokeClass.Value, payload.TypeArguments);
            if(currInsance == null)
                return Maybe<(MethodInfo methodInfo, object? instance)>.None;
            var method = currInsance.GetType().GetMethod(payload.MethodName);
            if (method == null)
                return Maybe<(MethodInfo methodInfo, object? instance)>.None;
            return (method,currInsance);


        }
        private static Maybe<(MethodInfo methodInfo, object? instance)> LoadStatic(Payload payload)
        {

            var invokeClass = LoadInvokeClassFromAssembly(payload);
            if(invokeClass.HasNoValue)
                return Maybe<(MethodInfo methodInfo, object? instance)>.None;
            var method = invokeClass.Value.GetMethod(payload.MethodName);
            if (method == null)
                return Maybe<(MethodInfo methodInfo, object? instance)>.None;
            return (method, null);


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
