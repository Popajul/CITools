using System.Dynamic;

namespace CITools
{
    internal class Endpoint
    {
        // To get EndPoint Path select Key
        internal KeyValuePair<string, ExpandoObject> Path { get; set; }
        // To get Endpoint verb select Key of an element
        internal IEnumerable<KeyValuePair<string, ExpandoObject>>? Verbs { get => Path.Value.Select(a => new KeyValuePair<string, ExpandoObject>(a.Key, (a.Value! as ExpandoObject) ?? throw new DeserializeException())); }
        internal KeyValuePair<string, ExpandoObject> Verb { get; set; }

        internal int Index { get; set; }
    }

}