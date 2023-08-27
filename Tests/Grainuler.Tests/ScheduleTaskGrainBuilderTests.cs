using Grainuler.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using Orleans;
using System.Reflection;
using Grainuler.DataTransferObjects.Triggers;

namespace Grainuler.Tests
{
    public class ScheduleTaskGrainBuilderTests
    {
        [Fact]
        public async Task AddPayloadFromType()
        {
            var clusterClient = Substitute.For<IClusterClient>();
            var builder = new ScheduleTaskGrainBuilder(clusterClient,"test");
            var type = typeof(TestWriteToDiskJob);
            var constructorAruments = new[] { "testTypeArgs" };
            var methodName =nameof(TestWriteToDiskJob.Execute);
            var methoAruments = new[] { "testMethodArgs" };

            builder.AddPayload(typeof(TestWriteToDiskJob), methodName,constructorAruments,methoAruments,false);

            var parameter = builder.ScheduleTaskGrainConstructorParameter;
            Assert.Equal(type.Assembly.FullName, parameter.Payload.AssemblyName);
            Assert.Equal(type.Assembly.Location, parameter.Payload.AssemblyPath);
            Assert.Equal(type.Name, parameter.Payload.ClassName);
            Assert.Equal(methodName, parameter.Payload.MethodName);
            Assert.False(parameter.Payload.IsStatic);
            Assert.True(constructorAruments.SequenceEqual(parameter.Payload.ConstructorParameters));
            Assert.True(methoAruments.SequenceEqual(parameter.Payload.MethodParameters));
        }

        [Fact]
        public async Task AddPayload()
        {
            var clusterClient = Substitute.For<IClusterClient>();
            var builder = new ScheduleTaskGrainBuilder(clusterClient, "test");
            var type = typeof(TestWriteToDiskJob);
            var constructorAruments = new[] { "testTypeArgs" };
            var methodName = nameof(TestWriteToDiskJob.Execute);
            var methoAruments = new[] { "testMethodArgs" };

            builder.AddPayload("Grainuler.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", Assembly.GetAssembly(typeof(TestWriteToDiskJob)).Location, nameof(TestWriteToDiskJob), methodName, constructorAruments, methoAruments, false);

            var parameter = builder.ScheduleTaskGrainConstructorParameter;
            Assert.Equal(type.Assembly.FullName, parameter.Payload.AssemblyName);
            Assert.Equal(type.Assembly.Location, parameter.Payload.AssemblyPath);
            Assert.Equal(type.Name, parameter.Payload.ClassName);
            Assert.Equal(methodName, parameter.Payload.MethodName);
            Assert.False(parameter.Payload.IsStatic);
            Assert.True(constructorAruments.SequenceEqual(parameter.Payload.ConstructorParameters));
            Assert.True(methoAruments.SequenceEqual(parameter.Payload.MethodParameters));
        }

        [Fact]
        public async Task AddOnScheduleTrigger()
        {
            var clusterClient = Substitute.For<IClusterClient>();
            var builder = new ScheduleTaskGrainBuilder(clusterClient, "test");
            var triggerTimeSpan = TimeSpan.FromSeconds(10);
            var repeatTimeSpan = TimeSpan.FromSeconds(60);
            var isExpnentailBackoffRetry = true;
            ushort maxRetryNumber = 5;
            var maxRetryPeriod=TimeSpan.FromMinutes(60);
            var waitTimeWithinRetries = TimeSpan.FromSeconds(4);
            var expireTimeSpan =    TimeSpan.FromDays(1);
            var expiredDate = DateTime.Today.AddDays(1);

            builder.AddOnScheduleTrigger(triggerTimeSpan, repeatTimeSpan, isExpnentailBackoffRetry, maxRetryNumber, maxRetryPeriod, waitTimeWithinRetries ,expireTimeSpan, expiredDate);

            var trigger = builder.ScheduleTaskGrainConstructorParameter.Triggers.FirstOrDefault() as ScheduleTrigger;

            Assert.Equal(triggerTimeSpan, trigger.TriggerTime);
            Assert.Equal(repeatTimeSpan, trigger.RepeatTime);
            Assert.Equal(isExpnentailBackoffRetry, trigger.IsExpnentialBackoffRetry);
            Assert.Equal(maxRetryNumber, trigger.MaxRetryNumber);
            Assert.Equal(maxRetryPeriod, trigger.MaxRetryPeriod);
            Assert.Equal(waitTimeWithinRetries, trigger.WaitTimeWithinRetries);
            Assert.Equal(expireTimeSpan, trigger.ExpireTimeSpan);
            Assert.Equal(expiredDate, trigger.ExpireDate);
        }


        [Fact]
        public async Task AddOnSuccededTrigger()
        {
            var clusterClient = Substitute.For<IClusterClient>();
            var builder = new ScheduleTaskGrainBuilder(clusterClient, "test");
            var taskId = "testTask";
            var isExpnentailBackoffRetry = true;
            ushort maxRetryNumber = 5;
            var maxRetryPeriod = TimeSpan.FromMinutes(60);
            var waitTimeWithinRetries = TimeSpan.FromSeconds(4);
            var expireTimeSpan = TimeSpan.FromDays(1);
            var expiredDate = DateTime.Today.AddDays(1);

            builder.AddOnSuccededTrigger("testTask", isExpnentailBackoffRetry, maxRetryNumber, maxRetryPeriod, waitTimeWithinRetries, expireTimeSpan, expiredDate);

            var trigger = builder.ScheduleTaskGrainConstructorParameter.Triggers.FirstOrDefault() as ReactiveTrigger;

            Assert.Equal(taskId, trigger.TaskId);
            Assert.Equal(isExpnentailBackoffRetry, trigger.IsExpnentialBackoffRetry);
            Assert.Equal(maxRetryNumber, trigger.MaxRetryNumber);
            Assert.Equal(maxRetryPeriod, trigger.MaxRetryPeriod);
            Assert.Equal(waitTimeWithinRetries, trigger.WaitTimeWithinRetries);
            Assert.Equal(expireTimeSpan, trigger.ExpireTimeSpan);
            Assert.Equal(expiredDate, trigger.ExpireDate);
        }

        [Fact]
        public async Task AddOnFailureTrigger()
        {
            var clusterClient = Substitute.For<IClusterClient>();
            var builder = new ScheduleTaskGrainBuilder(clusterClient, "test");
            var taskId = "testTask";
            var isExpnentailBackoffRetry = true;
            ushort maxRetryNumber = 5;
            var maxRetryPeriod = TimeSpan.FromMinutes(60);
            var waitTimeWithinRetries = TimeSpan.FromSeconds(4);
            var expireTimeSpan = TimeSpan.FromDays(1);
            var expiredDate = DateTime.Today.AddDays(1);

            builder.AddOnFailureTrigger("testTask", isExpnentailBackoffRetry, maxRetryNumber, maxRetryPeriod, waitTimeWithinRetries, expireTimeSpan, expiredDate);

            var trigger = builder.ScheduleTaskGrainConstructorParameter.Triggers.FirstOrDefault() as ReactiveTrigger;

            Assert.Equal(taskId, trigger.TaskId);
            Assert.Equal(isExpnentailBackoffRetry, trigger.IsExpnentialBackoffRetry);
            Assert.Equal(maxRetryNumber, trigger.MaxRetryNumber);
            Assert.Equal(maxRetryPeriod, trigger.MaxRetryPeriod);
            Assert.Equal(waitTimeWithinRetries, trigger.WaitTimeWithinRetries);
            Assert.Equal(expireTimeSpan, trigger.ExpireTimeSpan);
            Assert.Equal(expiredDate, trigger.ExpireDate);
        }
    }
}
