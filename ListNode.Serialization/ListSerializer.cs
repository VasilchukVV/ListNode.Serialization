using System.Collections;
using System.Collections.Generic;
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
            var node2Id = head.GenerateIds();
            foreach (var node in node2Id.Keys)
            {
                var randomId = node2Id.GetId(node.Random);
                await serializer(randomId, node.Data);
            }
        }

        async Task<ListNode> IListSerializer.Deserialize(Stream stream)
        {
            var list = new List<ListNode>();    //TODO: use pools instead of creating
            ListNode listNode = null;
            var index = 0;

            void Deserialize(int randomId = -1, in string data = null)
            {
                if (index >= list.Count)
                {
                    var node = new ListNode { Previous = listNode, Data = data };
                    list.Add(node);

                    if (listNode != null)
                        listNode.Next = node;
                    listNode = node;
                }
                else
                    list[index].Data = data;

                if (randomId != -1)
                {
                    while (randomId >= list.Count)
                    {
                        var node = new ListNode { Previous = listNode };
                        list.Add(node);

                        if (listNode != null)
                            listNode.Next = node;
                        listNode = node;
                    }

                    list[index].Random = list[randomId];
                }

                index++;
            }

            using var reader = new StreamReader(stream, leaveOpen: true);
            var deserializer = _deserializerFactory(reader);
            await deserializer(Deserialize);

            return list.Count > 0 ? list[0] : null;
        }

        Task<ListNode> IListSerializer.DeepCopy(ListNode head)
        {
            var hashtable = new Hashtable();    //TODO: use pools instead of creating

            var oldNode = head;
            ListNode newNode = null;
            while (oldNode != null)
            {
                newNode = new ListNode
                {
                    Previous = newNode,
                    Data = oldNode.Data.DeepCopy(),
                    Random = oldNode.Random
                };
                hashtable.Add(oldNode, newNode);
                oldNode = oldNode.Next;
            }

            ListNode node = null; 
            while (newNode != null)
            {
                newNode.Next = node;
                node = newNode;

                if (newNode.Random != null)
                    newNode.Random = (ListNode)hashtable[newNode.Random];

                newNode = newNode.Previous;
            }

            return Task.FromResult(node);
        }
    }
}
