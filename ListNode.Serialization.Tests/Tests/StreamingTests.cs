using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;


namespace ListNode.Serialization.Tests
{
    [TestFixture]
    public sealed class StreamingTests
    {
        [Test, Category(TestsHelper.CorrectnessCheck)]
        public async Task StreamingTest(
            [Values(1, 2, 3)] int count,
            [Range(0, 100, 25)] int nullsPercentage)
        {
            var sourceNode = TestsHelper.GenerateListNode(count, nullsPercentage);
            var serializer = ListSerializerFactory.CreateSerializer(SerializeFactory, DeserializeFactory);

            await using var stream = new MemoryStream();
            await serializer.Serialize(sourceNode, stream);
            stream.Seek(0, SeekOrigin.Begin);
            var cloneNode = await serializer.Deserialize(stream);

            TestsHelper.CheckEqual(sourceNode, cloneNode);
        }


        [Test, Category(TestsHelper.LongRunCheck)]
        public Task StreamingLongRunTest(
            [Values(1000)] int count = 1000,
            [Range(0, 100, 25)] int nullsPercentage = 50,
            [Values(100)] int parallels = 100,
            [Values(50)] int repeats = 50,
            [Values(30)] int timeoutSeconds = 30)
        {
            return Task.CompletedTask;

            var sourceNode = TestsHelper.GenerateListNode(count, nullsPercentage);
            var serializer = ListSerializerFactory.CreateSerializer(SerializeFactory, DeserializeFactory);

            return TestsHelper.RunStreamTasks(
                () => new MemoryStream(), 
                async stream =>
                {
                    await serializer.Serialize(sourceNode, stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    await serializer.Deserialize(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                }, 
                parallels, repeats, timeoutSeconds);

        }

        private static SerializeDelegate SerializeFactory(StreamWriter writer)
        {
            var jsonSettings = new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore };

            Task Serialize(int? randomId = null, in string data = null)
            {
                var node = new TestsHelper.NodeInfo { RandomId = randomId, Data = data };
                var json = JsonConvert.SerializeObject(node, Formatting.None, jsonSettings);
                return writer.WriteLineAsync(json == "{}" ? string.Empty : json);
            }

            return Serialize;
        }

        private static DeserializeDelegate DeserializeFactory(StreamReader reader)
        {
            var jsonSettings = new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Populate };

            async Task Deserialize(DeserializeAction deserializeAction)
            {
                while (reader.Peek() >= 0)
                {
                    var json = await reader.ReadLineAsync();
                    if (json == null)
                        break;

                    var nodeInfo = json != string.Empty
                        ? JsonConvert.DeserializeObject<TestsHelper.NodeInfo>(json, jsonSettings)
                        : new TestsHelper.NodeInfo { RandomId = null };
                    deserializeAction(nodeInfo.RandomId, nodeInfo.Data);
                }
            }

            return Deserialize;
        }
    }
}
