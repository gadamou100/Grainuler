using System.IO;

namespace Grainuler.Tests
{
    public class TestWriteToDiskJob
    {
        private readonly string _fileName;
        public TestWriteToDiskJob(string fileName)
        {
            _fileName = fileName;
        }
        public void Execute(string fileContent)
        {
            File.AppendAllText(_fileName, fileContent);
        }
    }
}
