using System.Collections.Generic;

namespace TestApp.Utils
{
    public static class CollectionUtils
    {
        public static T RandomElement<T>(this IList<T> list, bool remove = false)
        {
            if (list.Count == 0)
                return default(T);

            var index = UnityEngine.Random.Range(0, list.Count);
            var result = list[index];

            if (remove)
                list.RemoveAt(index);

            return result;
        }
    }
}
