using System;
using System.IO;
using NUnit.Framework;

namespace ListNode.Serialization.Tests
{
    [TestFixture]
    public sealed class GenerationTests
    {
        [Test, Category(TestsHelper.CorrectnessCheck)]
        public void GenerationTest(
            [Values(1, 2, 3)] int count,
            [Range(0, 100, 25)] int nullsPercentage)
        {
            var node = TestsHelper.GenerateListNode(count, nullsPercentage);
            CheckRandomNulls(node, count, nullsPercentage);
        }


        [Test, Category(TestsHelper.CorrectnessCheck)]
        public void GenerationCountFailTest(
            [Values(-1, 0)] int count,
            [Values(0, 100)] int nullsPercentage)
        {
            Assert.Throws(Is.TypeOf<ArgumentOutOfRangeException>().And.Property("ParamName").EqualTo("count"),
                () => TestsHelper.GenerateListNode(count, nullsPercentage));
        }


        [Test, Category(TestsHelper.CorrectnessCheck)]
        public void GenerationPercentageFailTest(
            [Values(1)] int count,
            [Values(-1, 101)] int nullsPercentage)
        {
            Assert.Throws(Is.TypeOf<ArgumentOutOfRangeException>().And.Property("ParamName").EqualTo("nullsPercentage"),
                () => TestsHelper.GenerateListNode(count, nullsPercentage));
        }

        private static void CheckRandomNulls(ListNode node, int count, int nullsPercentage)
        {
            var randomNullsCount = node.Random != null ? 0 : 1;
            var dataNullsCount = node.Data != null ? 0 : 1;

            var index = 1;
            while (node.Next != null && index < count)
            {
                node = node.Next;
                if (node.Random == null)
                    randomNullsCount++;
                if (node.Data == null)
                    dataNullsCount++;
                index++;
            }

            if (node.Next != null || index != count)
                throw new InvalidDataException();

            var limit = TestsHelper.CalculateLimit(count, nullsPercentage);
            var nulls = count - limit;
            Assert.IsTrue(nulls == randomNullsCount);
            Assert.IsTrue(nulls == dataNullsCount);
        }
    }
}
