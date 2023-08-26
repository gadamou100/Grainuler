namespace Grainuler.DataTransferObjects
{
    public class Payload
    {
        public string AssemblyName { get; set; }
        public string AssemblyPath { get; set; }

        public string ClassName { get; set; }
        public object[]? ConstructorParameters { get; set; }
        public string MethodName { get; set; }
        public object[]? MethodParameters { get; set; }
        public GenericArgument[]? GenericMethodArguments { get; set; }
        public GenericArgument[]? ConstructorGenericArguments { get; set; }


        public bool IsStatic { get; set; }
    }

    public record struct GenericArgument(string TypeFullName, string TypeAssembly)
    {
        public static implicit operator (string TypeFullName, string TypeAssembly)(GenericArgument value)
        {
            return (value.TypeFullName, value.TypeAssembly);
        }

        public static implicit operator GenericArgument((string TypeFullName, string TypeAssembly) value)
        {
            return new GenericArgument(value.TypeFullName, value.TypeAssembly);
        }
    }
}
