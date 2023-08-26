namespace Grainuler.Abstractions
{
    public interface IPayloadedScheduleTaskGrainBuilder : IScheduleTaskGrainBuilder
    {
        IPayloadedScheduleTaskGrainBuilder AddPayloadType(Type type);

        IPayloadedScheduleTaskGrainBuilder AddPayloadMethod(string methodName);
        IPayloadedScheduleTaskGrainBuilder AddConstructorParameters(params object[] constructorArguments);
        IPayloadedScheduleTaskGrainBuilder AddMethodParameters(params object[] methodParameters);
        IPayloadedScheduleTaskGrainBuilder AddConstructorGenericArguments(params Type[]? constructorGenericArguments);
        IPayloadedScheduleTaskGrainBuilder AddMethodGenericArguments(params Type[]? methodGenericArguments);
        IPayloadedScheduleTaskGrainBuilder AddIsMethodStatic(bool value);



    }

}