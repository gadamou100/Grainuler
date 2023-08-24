namespace Grainuler.DataTransferObjects
{
    public struct Payload
    {
        public string AssemblyName { get; set; }
        public string AssemblyPath { get; set; }

        public string ClassName { get; set; }
        public object[] ConstructorArguments { get; set; }
        public string MethodName { get; set; }
        public object[] MethodArguments { get; set; }
        public GenericArgument[] GenericMethodArguments { get; set; }
        public GenericArgument[] ConstructorGenericMethodArguments { get; set; }


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
