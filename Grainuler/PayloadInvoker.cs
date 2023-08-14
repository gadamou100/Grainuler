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
               var invokable = Load(payload);
               if (invokable.HasValue)
               {
                   var result = invokable.Value.methodInfo.Invoke(invokable.Value.instance, payload.MethodArguments);
               }
           });
        }

        private static Maybe<(MethodInfo methodInfo, object instance)> Load(Payload payload)
        {
            var assembly = Assembly.LoadFile(payload.AssemblyPath);
                
             var a =   AppDomain.CurrentDomain.GetAssemblies()
                //.SingleOrDefault(assembly => assembly.GetName().Name == payload.AssemblyName)
                ;
            if(assembly == null)
                return Maybe<(MethodInfo methodInfo, object instance)>.None;

            var invokeClass = assembly?.GetTypes().Where(p=>p.Name == payload.ClassName).FirstOrDefault();
            if(invokeClass == null)
                return Maybe<(MethodInfo methodInfo, object instance)>.None;
            var currInsance = Activator.CreateInstance(invokeClass, payload.TypeArguments);
            if(currInsance == null)
                return Maybe<(MethodInfo methodInfo, object instance)>.None;
            var method = currInsance.GetType().GetMethod(payload.MethodName);
            if (method == null)
                return Maybe<(MethodInfo methodInfo, object instance)>.None;
            return (method,currInsance);


        }
    }
}
