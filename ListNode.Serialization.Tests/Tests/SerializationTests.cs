using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ListNode.Serialization.Tests
{
    [TestFixture]
    public sealed class SerializationTests
    {
        [Test, Category(TestsHelper.CorrectnessCheck)]
        public async Task SerializationTest(
            [Values(1, 2, 3)] int count,
            [Range(0, 100, 25)] int nullsPercentage)
        {
            var nodes = new List<ListNode>();
            var randoms = new Dictionary<ListNode, int>();

            ListNode node = null;
            Task Serialize(int randomId = -1, in string data = null)
            {
                node = new ListNode { Previous = node, Data = data.DeepCopy() };
                nodes.Add(node);
                randoms.Add(node!, randomId);
                return Task.CompletedTask;
            }

            var sourceNode = TestsHelper.GenerateListNode(count, nullsPercentage);

            await using var stream = Stream.Null;
            var serializer = TestsHelper.CreateSerializer(writer => Serialize);
            await serializer.Serialize(sourceNode, stream);

            var cloneNode = RestoreCloneNode(node, randoms, nodes);
            TestsHelper.CheckEqual(sourceNode, cloneNode);
        }


        [Test, Category(TestsHelper.LongRunCheck)]
        public Task SerializationLongRunTest(
            [Values(1000)] int count = 1000,
            [Range(0, 100, 25)] int nullsPercentage = 50,
            [Values(100)] int parallels = 100,
            [Values(1000)] int repeats = 1000,
            [Values(30)] int timeoutSeconds = 30)
        {
            var sourceNode = TestsHelper.GenerateListNode(count, nullsPercentage);
            var serializer = TestsHelper.CreateSerializer();

            return TestsHelper.RunStreamTasks(
                () => Stream.Null,
                stream => serializer.Serialize(sourceNode, stream),
                parallels, repeats, timeoutSeconds);
        }

        private static ListNode RestoreCloneNode(ListNode node, IReadOnlyDictionary<ListNode, int> randoms, IReadOnlyList<ListNode> nodes)
        {
            ListNode cloneNode = null;
            while (node != null)
            {
                node.Next = cloneNode;
                cloneNode = node;

                var randomId = randoms[node];
                if (randomId != -1)
                    node.Random = nodes[randomId];

                node = node.Previous;
            }
            return cloneNode;
        }
    }
}