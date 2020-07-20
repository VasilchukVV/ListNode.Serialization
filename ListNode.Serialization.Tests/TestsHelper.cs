using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ListNode.Serialization.Tests
{
    public static class TestsHelper
    {
        public const string LongRunCheck = "Long Run Check";
        public const string CorrectnessCheck = "Correctness Check";

        public static IListSerializer CreateSerializer()
        {
            return ListSerializerFactory.CreateSerializer(NullSerialize, NullDeserialize);
        }

        public static IListSerializer CreateSerializer(SerializeDelegateFactory serializeFactory)
        {
            return ListSerializerFactory.CreateSerializer(serializeFactory, NullDeserialize);
        }

        public static IListSerializer CreateSerializer(DeserializeDelegateFactory deserializeFactory)
        {
            return ListSerializerFactory.CreateSerializer(NullSerialize, deserializeFactory);
        }

        private static SerializeDelegate NullSerialize(StreamWriter writer)
        {
            return (int? id, in string data) => Task.CompletedTask;
        }

        private static DeserializeDelegate NullDeserialize(StreamReader reader)
        {
            return action => Task.CompletedTask;
        }


        public static void CheckEqual(ListNode source, ListNode clone)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (clone == null)
                throw new ArgumentNullException(nameof(clone));


            var sourceIds = source.GenerateIds();
            var cloneIds = clone.GenerateIds();

            if (sourceIds.Count != cloneIds.Count)
                Assert.IsTrue(sourceIds.Count == cloneIds.Count);

            Assert.IsTrue(sourceIds.Count == cloneIds.Count);

            while (source != null && clone != null)
            {
                Assert.IsTrue(sourceIds.GetId(source) == cloneIds.GetId(clone));
                Assert.IsTrue(sourceIds.GetId(source.Previous) == cloneIds.GetId(clone.Previous));
                Assert.IsTrue(sourceIds.GetId(source.Random) == cloneIds.GetId(clone.Random));

                Assert.IsTrue(!ReferenceEquals(source.Data, clone.Data) || source.Data == null && clone.Data == null);
                Assert.IsTrue(source.Data == clone.Data);

                source = source.Next;
                clone = clone.Next;
            }
            Assert.IsNull(source);
            Assert.IsNull(clone);
        }


        public static ListNode GenerateListNode(int count, int nullsPercentage = 0)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "more than 0");

            var nodes = new ListNode[count];

            var root = new ListNode();
            if (nullsPercentage < 100)
                root.Data = "0";

            nodes[0] = root;

            var listNode = root;
            for (var index = 1; index < count; index++)
            {
                var node = new ListNode { Previous = listNode };
                if (nullsPercentage < 100)
                    node.Data = index.ToString();
                listNode.Next = node;
                listNode = node;
                nodes[index] = node;
            }

            if (nullsPercentage > 0 && nullsPercentage < 100)
                DataReset(nodes, nullsPercentage);

            nodes.GenerateRandom(nullsPercentage);
            return root;
        }

        private static void DataReset(this IList<ListNode> nodes, int nullsPercentage = 0)
        {
            var count = nodes.Count;
            var limit = count - CalculateLimit(count, nullsPercentage);
            if (limit == 0)
                return;

            var rnd = new Random();
            for (var index = 0; index < limit; index++)
            {
                var node = nodes[index];
                var random = rnd.Next(index, count);
                nodes[index] = nodes[random];
                nodes[random] = node;
                nodes[index].Data = null;
            }
        }

        private static void GenerateRandom(this IList<ListNode> nodes, int nullsPercentage = 0)
        {
            var count = nodes.Count;
            var limit = CalculateLimit(count, nullsPercentage);
            if (limit == 0)
                return;

            var rnd = new Random();
            for (var index = 0; index < limit; index++)
            {
                var node = nodes[index];
                var random = rnd.Next(index, count);
                nodes[index] = nodes[random];
                nodes[random] = node;

                random = rnd.Next(count);
                nodes[index].Random = nodes[random];
            }
        }

        public static int CalculateLimit(int count, int nullsPercentage = 0)
        {
            if (nullsPercentage < 0 || nullsPercentage > 100)
                throw new ArgumentOutOfRangeException(nameof(nullsPercentage), "0..100");

            return (int)Math.Round(count * (100 - nullsPercentage) / 100.0f);
        }

        public static Task RunStreamTasks(Func<Stream> streamFactory,
                                          Func<Stream, Task> taskFactory,
                                          int parallels, int repeats, int timeoutSeconds)
        {
            var timeout = timeoutSeconds * 1000;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var tokenSource = new CancellationTokenSource();
            var tasks = new List<Task>();
            for (var index = 0; index < parallels; index++)
            {
                var task = Task.Run(async () =>
                {
                    await using var stream = streamFactory();
                    for (var repeat = 0; repeat < repeats; repeat++)
                    {
                        await taskFactory(stream);

                        if (stopwatch.ElapsedMilliseconds > timeout)
                            tokenSource.Cancel(true);
                    }
                }, tokenSource.Token);
                tasks.Add(task);
            }
            return Task.WhenAll(tasks);
        }

        public static Task RunTasks(Func<Task> taskFactory,
                                    int parallels, int repeats, int timeoutSeconds)
        {
            var timeout = timeoutSeconds * 1000;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var tokenSource = new CancellationTokenSource();
            var tasks = new List<Task>();
            for (var index = 0; index < parallels; index++)
            {
                var task = Task.Run(async () =>
                {
                    for (var repeat = 0; repeat < repeats; repeat++)
                    {
                        await taskFactory();

                        if (stopwatch.ElapsedMilliseconds > timeout)
                            tokenSource.Cancel(true);
                    }
                }, tokenSource.Token);
                tasks.Add(task);
            }
            return Task.WhenAll(tasks);
        }

        public struct NodeInfo
        {
            [DefaultValue(null)]
            public int? RandomId;

            [DefaultValue(null)]
            public string Data;
        }
    }
}
