using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace ListNode.Serialization.Tests
{
    class Program
    {
        private const string Usage = "Usage [deepcopy] [serialization] [deserialization] [streaming]";

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(Usage);
                return;
            }

            var tasks = args
                .Select(arg => arg.ToLower())
                .Select(testId =>
                {
                    Console.WriteLine($"{testId} test executing...");
                    return ExecuteTest(testId); //TODO: use advanced params: parallels, repeats, timeoutSeconds
                }).ToArray();

            Task.WhenAll(tasks).Wait();
            Console.WriteLine("...completed");
        }

        private static Task ExecuteTest(string testId)
        {
            switch (testId)
            {
                case "deepcopy":
                {
                    var testFixture = new DeepCopyTests();
                    return testFixture.DeepCopyTest(5, 0);
                    //return testFixture.DeepCopyLongRunTest();
                }

                case "serialization":
                {
                    var testFixture = new SerializationTests();
                    return testFixture.SerializationTest(5, 0);
                    //return testFixture.SerializationLongRunTest();
                }

                case "deserialization":
                {
                    var testFixture = new DeserializationTests();
                    return testFixture.DeserializationTest(5, 0);
                    //return testFixture.DeserializationLongRunTest();
                }

                case "streaming":
                {
                    var testFixture = new StreamingTests();
                    return testFixture.StreamingTest(5, 0);
                    //return testFixture.StreamingLongRunTest();
                }

                default:
                    throw new InvalidDataException(Usage);
            }
        }
    }
}
