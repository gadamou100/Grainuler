namespace Grainuler.DataTransferObjects
{
    public struct Payload
    {
        public string AssemblyName { get; set; }
        public string AssemblyPath { get; set; }

        public string ClassName { get; set; }
        public object[] TypeArguments { get; set; }
        public string MethodName { get; set; }
        public object[] MethodArguments { get; set; }
        public bool IsStatic { get; set; }
    }

}
