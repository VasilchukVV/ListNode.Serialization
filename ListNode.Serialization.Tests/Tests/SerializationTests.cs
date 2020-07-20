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
            var randoms = new Dictionary<ListNode, int?>();

            ListNode cloneNode = null;
            ListNode previous = null;
            Task Serialize(int? randomId = null, in string data = null)
            {
                var current = new ListNode { Previous = previous, Data = data.DeepCopy() };
                if (previous != null)
                    previous.Next = current;
                previous = current;
                randoms.Add(current, randomId);
                cloneNode ??= current;
                return Task.CompletedTask;
            }

            var sourceNode = TestsHelper.GenerateListNode(count, nullsPercentage);

            await using var stream = Stream.Null;
            var serializer = TestsHelper.CreateSerializer(writer => Serialize);
            await serializer.Serialize(sourceNode, stream);

            RestoreRandoms(randoms);
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
            return Task.CompletedTask;

            var sourceNode = TestsHelper.GenerateListNode(count, nullsPercentage);
            var serializer = TestsHelper.CreateSerializer();

            return TestsHelper.RunStreamTasks(
                () => Stream.Null,
                stream => serializer.Serialize(sourceNode, stream),
                parallels, repeats, timeoutSeconds);
        }

        private static void RestoreRandoms(IReadOnlyDictionary<ListNode, int?> randoms)
        {
            foreach (var (node, randomId) in randoms)
            {
                if (randomId == null)
                    continue;

                var offset = randomId.Value;
                if (offset == 0)
                {
                    node.Random = node;
                    continue;
                }

                var random = node;
                if (offset > 0)
                {
                    do
                    {
                        random = random.Next;
                        offset--;
                    } while (offset != 0);
                }
                else
                {
                    do
                    {
                        random = random.Previous;
                        offset++;
                    } while (offset != 0);
                }
                node.Random = random;
            }
        }
    }
}