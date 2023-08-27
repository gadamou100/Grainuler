using Grainuler.Abstractions;
using Grainuler.DataTransferObjects;
using Grainuler.DataTransferObjects.Triggers;
using Orleans.TestingHost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Grainuler.Tests
{
    [Collection(ClusterCollection.Name)]
    public class ScheduleTaskGrainTests
    {
        private readonly TestCluster _cluster;

        public ScheduleTaskGrainTests(ClusterFixture fixture)
        {
            _cluster = fixture.Cluster;
        }
        [Fact]
        public async Task TestGetStreamId()
        {
            var grain = _cluster.GrainFactory.GetGrain<IScheduleTaskGrain>("test_job");
            var streamId = await grain.GetStreamId();

            Assert.NotEqual(Guid.Empty,streamId);
        }


        [Fact]
        public async Task TestInitiate()
        {
            //Arrange
            var grain = _cluster.GrainFactory.GetGrain<IScheduleTaskGrain>("test_job");

            var fileName = "file.txt";
            var fileContent = "Hello world";
            var fileInfo = new FileInfo(fileName);
            if(fileInfo.Exists)
                fileInfo.Delete();

            var parameter = new ScheduleTaskGrainInitiationParameter
            {
                Payload = GetPayload(fileName, fileContent),
                Triggers = new[]
                 {
                    new ScheduleTrigger
                    {
                        RepeatTime = TimeSpan.FromMinutes(1),
                        TriggerTime = TimeSpan.FromSeconds(5)
                    }
                }
            };

            //Act
            await grain.Initiate(parameter);
            await Task.Delay(7000);

            //Assert
            Assert.True(fileInfo.Exists);
            Assert.Equal(fileContent, File.ReadAllText(fileInfo.FullName));

            //Act
            await Task.Delay(60300);
            string newContent = File.ReadAllText(fileInfo.FullName);
            //Assert
            Assert.NotEqual(fileContent, newContent);
            Assert.Contains(fileContent, newContent);

        }


       

        private Payload GetPayload(string fileName, string fileContent, string methodName= "Execute")
        {
            return new Payload
            {
                AssemblyName = typeof(TestWriteToDiskJob).Assembly.FullName,
                AssemblyPath = typeof(TestWriteToDiskJob).Assembly.Location,
                ClassName = nameof(TestWriteToDiskJob),
                ConstructorParameters = new[] { fileName },
                IsStatic = false,
                MethodParameters = new[] { fileContent },
                MethodName = methodName
            };
        }
    }
}
