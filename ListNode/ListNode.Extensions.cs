using System.Collections.Generic;
using System.IO;

namespace ListNode
{
    public static class Extensions
    {
        public static Dictionary<ListNode, int> GenerateIds(this ListNode node)
        {
            if (node == null)
                throw new InvalidDataException(nameof(node));

            var result = new Dictionary<ListNode, int>();   //TODO: use pools instead of creating
            var index = 0;
            do
            {
                result.Add(node, index);
                node = node.Next;
                index++;
            } while (node != null);

            return result;
        }

        public static int GetId(this Dictionary<ListNode, int> dictionary, ListNode node = null)
        {
            return node != null ? dictionary[node] : -1;
        }

        public static string DeepCopy(this string source)
        {
            //TODO:resolve deprecated string.Copy method
            return source == null ? null : string.Copy(source);
            //return source?.Substring(source.Length);
        }

        public static int? FindRandomNodeOffset(this ListNode node)
        {
            if (node?.Random == null)
                return null;

            if (ReferenceEquals(node, node.Random))
                return 0;

            var random = node.Random;

            var previous = node.Previous;
            var next = node.Next;

            var offsetPrevious = -1;
            var offsetNext = 1;

            int? OffsetPrevious()
            {
                while (previous != null)
                {
                    if (ReferenceEquals(previous, random))
                        return offsetPrevious;

                    offsetPrevious--;
                    previous = previous.Previous;
                }
                return null;
            }

            int? OffsetNext()
            {
                while (next != null)
                {
                    if (ReferenceEquals(next, random))
                        return offsetNext;

                    offsetNext++;
                    next = next.Next;
                }
                return null;
            }

            while (previous != null && next != null)
            {
                if (ReferenceEquals(previous, random))
                    return offsetPrevious;

                offsetPrevious--;
                previous = previous.Previous;

                if (ReferenceEquals(next, random))
                    return offsetNext;

                offsetNext++;
                next = next.Next;
            }

            var offset = previous != null
                ? OffsetPrevious() 
                : OffsetNext();

            return offset;
        }
    }
}
