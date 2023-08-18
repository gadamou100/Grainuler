using CSharpFunctionalExtensions;
using Grainuler.DataTransferObjects;
using OneOf;

namespace Grainuler.Abstractions
{
    public interface IPayloadInvoker
    {
        Task<object?> Invoke(Payload payload);
    }
}