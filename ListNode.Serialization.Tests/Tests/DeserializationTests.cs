using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ListNode.Serialization.Tests
{
    [TestFixture]
    public sealed class DeserializationTests
    {
        [Test, Category(TestsHelper.CorrectnessCheck)]
        public async Task DeserializationTest(
            [Values(1, 2, 3)] int count,
            [Range(0, 100, 25)] int nullsPercentage)
        {
            var sourceNode = TestsHelper.GenerateListNode(count, nullsPercentage);
            var deserializationInfo = CreateDeserializationInfo(sourceNode);

            Task Deserialize(DeserializeAction deserialize)
            {
                foreach (var nodeInfo in deserializationInfo)
                    deserialize(nodeInfo.RandomId, nodeInfo.Data);
                return Task.CompletedTask;
            }

            await using var stream = Stream.Null;
            var serializer = TestsHelper.CreateSerializer(reader => Deserialize);
            var cloneNode = await serializer.Deserialize(stream);

            TestsHelper.CheckEqual(sourceNode, cloneNode);
        }


        [Test, Category(TestsHelper.LongRunCheck)]
        public Task DeserializationLongRunTest(
            [Values(1000)] int count = 1000,
            [Range(0, 100, 25)] int nullsPercentage = 50,
            [Values(100)] int parallels = 100,
            [Values(1000)] int repeats = 1000,
            [Values(30)] int timeoutSeconds = 30)
        {
            return Task.CompletedTask;

            var sourceNode = TestsHelper.GenerateListNode(count, nullsPercentage);
            var deserializationInfo = CreateDeserializationInfo(sourceNode);

            Task Deserialize(DeserializeAction deserialize)
            {
                foreach (var nodeInfo in deserializationInfo)
                    deserialize(nodeInfo.RandomId, nodeInfo.Data);
                return Task.CompletedTask;
            }

            var serializer = TestsHelper.CreateSerializer(reader => Deserialize);
            return TestsHelper.RunStreamTasks(
                () => Stream.Null,  
                stream => serializer.Deserialize(stream),
                parallels, repeats, timeoutSeconds);
        }

        private static TestsHelper.NodeInfo[] CreateDeserializationInfo(ListNode node)
        {
            var list = new List<TestsHelper.NodeInfo>();
            while (node != null)
            {
                var nodeInfo = new TestsHelper.NodeInfo
                {
                    RandomId = node.FindRandomNodeOffset(),
                    Data = node.Data.DeepCopy()
                };
                list.Add(nodeInfo);
                node = node.Next;
            }

            return list.ToArray();
        }
    }
}
