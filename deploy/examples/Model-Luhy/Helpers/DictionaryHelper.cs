using System.Collections.Generic;

namespace ModelLuhy.Helpers
{
    public static class DictionaryHelper
    {
        public static void AddToDictionary<T>(this Dictionary<string, List<T>> dictionary, string key, T value)
        {
            List<T> collection;
            if (dictionary.TryGetValue(key, out collection))
            {
                collection.Add(value);
            }
            else
            {
                collection = new List<T> { value };
                dictionary.Add(key, collection);
            }
        }
    }
}