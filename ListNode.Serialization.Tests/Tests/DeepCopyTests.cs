using System.Threading.Tasks;
using NUnit.Framework;

namespace ListNode.Serialization.Tests
{
    [TestFixture]
    public sealed class DeepCopyTests
    {
        [Test, Category(TestsHelper.CorrectnessCheck)]
        public async Task DeepCopyTest(
            [Values(1, 2, 3)] int count,
            [Range(0, 100, 25)] int nullsPercentage)
        {
            var sourceNode = TestsHelper.GenerateListNode(count, nullsPercentage);
            var serializer = TestsHelper.CreateSerializer();
            var cloneNode = await serializer.DeepCopy(sourceNode);
            TestsHelper.CheckEqual(sourceNode, cloneNode);
        }


        [Test, Category(TestsHelper.LongRunCheck)]
        public Task DeepCopyLongRunTest(
            [Values(1000)] int count = 1000,
            [Range(0, 100, 25)] int nullsPercentage = 50,
            [Values(100)] int parallels = 100,
            [Values(1000)] int repeats = 1000,
            [Values(30)] int timeoutSeconds = 30)
        {
            return Task.CompletedTask;

            var sourceNode = TestsHelper.GenerateListNode(count, nullsPercentage);
            var serializer = TestsHelper.CreateSerializer();
            return TestsHelper.RunTasks(
                () => serializer.DeepCopy(sourceNode),
                parallels,
                repeats,
                timeoutSeconds);
        }
    }
}
