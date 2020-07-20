using System.IO;
using System.Threading.Tasks;

namespace ListNode.Serialization
{
    internal sealed class ListSerializer : IListSerializer
    {
        private readonly SerializeDelegateFactory _serializerFactory;
        private readonly DeserializeDelegateFactory _deserializerFactory;

        internal ListSerializer(SerializeDelegateFactory serializerFactory,
                                DeserializeDelegateFactory deserializerFactory)
        {
            _serializerFactory = serializerFactory;
            _deserializerFactory = deserializerFactory;
        }

        async Task IListSerializer.Serialize(ListNode head, Stream stream)
        {
            await using var writer = new StreamWriter(stream, leaveOpen: true);
            var serializer = _serializerFactory(writer);

            var node = head;
            while (node != null)
            {
                int? offset = null;
                if (node.Random != null)
                    offset = node.FindRandomNodeOffset();

                await serializer(offset, node.Data);
                node = node.Next;
            }
        }

        async Task<ListNode> IListSerializer.Deserialize(Stream stream)
        {
            ListNode head = null;
            ListNode current = null;
            ListNode last = null;

            void Deserialize(int? randomId = null, in string data = null)
            {
                if (current == null || ReferenceEquals(current, last))
                {
                    current = new ListNode { Previous = last, Data = data };
                    if (last != null)
                        last.Next = current;
                    last = current;
                    head ??= current;
                }
                else
                {
                    current = current.Next;
                    current.Data = data;
                }

                switch (randomId)
                {
                    case null:
                        return;
                    case 0:
                        current.Random = current;
                        break;
                    default:
                    {
                        if (randomId > 0)
                        {
                            var node = current.Next;
                            do
                            {
                                if (node != null)
                                    node = node.Next;
                                else
                                {
                                    node = new ListNode {Previous = last};
                                    if (last != null)
                                        last.Next = node;
                                    last = node;
                                    node = null;
                                }
                                randomId--;
                            } while (randomId != 0);
                            current.Random = last;
                        }
                        else
                        {
                            var node = current;
                            do
                            {
                                node = node.Previous;
                                randomId++;
                            } while (randomId != 0);
                            current.Random = node;
                        }
                        break;
                    }
                }
            }

            using var reader = new StreamReader(stream, leaveOpen: true);
            var deserializer = _deserializerFactory(reader);
            await deserializer(Deserialize);

            return head;
        }

        Task<ListNode> IListSerializer.DeepCopy(ListNode head)
        {
            var node = head;
            ListNode tail = null;
            ListNode copy = null;
            while (node != null)
            {
                tail = node;
                var listNode = new ListNode { Previous = copy, Data = node.Data.DeepCopy() };
                if (copy != null)
                    copy.Next = listNode;
                copy = listNode;
                node = node.Next;
            }

            ListNode result = null;
            while (tail != null)
            {
                result = copy;
                var randomId = tail.FindRandomNodeOffset();
                if (randomId != null)
                {
                    var random = copy;
                    if (randomId > 0)
                    {
                        do
                        {
                            random = random.Next;
                            randomId--;
                        } while (randomId != 0);
                    }
                    else if (randomId < 0)
                    {
                        do
                        {
                            random = random.Previous;
                            randomId++;
                        } while (randomId != 0);
                    }
                    copy.Random = random;
                }
                tail = tail.Previous;
                copy = copy.Previous;
            }

            return Task.FromResult(result);
        }
    }
}
