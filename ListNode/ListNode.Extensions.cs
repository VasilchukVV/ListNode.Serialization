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
    }
}
