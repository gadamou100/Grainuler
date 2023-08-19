using Grainuler.DataTransferObjects;
using SampleTestJobs;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Grainuler.Tests
{
    public class PayloadInvokerTests
    {
        [Fact]
        public async Task TestPayloadInvokerWithInstantClassSuccess()
        {
            //Arrange
            var invoker = new PayloadInvoker();
            var payloadType = typeof(TestClass);
            var now = DateTime.Now;
            var payload = new Payload { AssemblyName = payloadType.Assembly.FullName, AssemblyPath = payloadType.Assembly.Location, ClassName = payloadType.Name, IsStatic = false, ConstructorArguments = new object[] { "testInstance1" }, MethodArguments = new object[] { now }, MethodName = "Run" };
            //Act
            var result = await invoker.Invoke(payload);
            //Assert
            Assert.IsType<string>(result);
        }

        [Fact]
        public async Task TestPayloadInvokerWithStaticClassSuccess()
        {
            //Arrange
            var invoker = new PayloadInvoker();
            var payloadType = typeof(ReactiveTestClass);
            var now = DateTime.Now;
            var payload = new Payload { AssemblyName = payloadType.Assembly.FullName, AssemblyPath = payloadType.Assembly.Location, ClassName = payloadType.Name, IsStatic = true, MethodArguments = new object[] { now }, MethodName = "ReactiveRun" };
            //Act
            var result = await invoker.Invoke(payload);
            //Assert
            Assert.IsType<string>(result);
        }

        [Fact]
        public async Task TestPayloadInvokerWithInstantClassException()
        {
            //Arrange
            var invoker = new PayloadInvoker();
            var payloadType = typeof(TestClass);
            var now = DateTime.Now;
            var payload = new Payload { AssemblyName = payloadType.Assembly.FullName, AssemblyPath = payloadType.Assembly.Location, ClassName = payloadType.Name, IsStatic = false, ConstructorArguments = new object[] { "testInstance1" }, MethodArguments = new object[] { now }, MethodName = "RunFail" };
            //Act
            Task result() => invoker.Invoke(payload);
            //Assert
            await Assert.ThrowsAsync<TargetInvocationException>(result);
        }
    }
}