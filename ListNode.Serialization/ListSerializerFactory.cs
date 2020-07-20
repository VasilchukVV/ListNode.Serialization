using System;
using System.IO;
using System.Threading.Tasks;

namespace ListNode.Serialization
{
    public delegate Task SerializeDelegate(int? randomId = null, in string data = null);
    public delegate SerializeDelegate SerializeDelegateFactory(StreamWriter writer);

    public delegate void DeserializeAction(int? randomId = null, in string data = null);
    public delegate Task DeserializeDelegate(DeserializeAction deserializeAction);
    public delegate DeserializeDelegate DeserializeDelegateFactory(StreamReader reader);

    public static class ListSerializerFactory
    {
        public static IListSerializer CreateSerializer(
            SerializeDelegateFactory serializeFactory,
            DeserializeDelegateFactory deserializeFactory)
        {
            if (serializeFactory == null)
                throw new ArgumentNullException(nameof(serializeFactory));

            if (deserializeFactory == null)
                throw new ArgumentNullException(nameof(deserializeFactory));

            return new ListSerializer(serializeFactory, deserializeFactory);
        }
    }
}
