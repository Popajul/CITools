using System.Dynamic;

namespace CITools
{
    internal class EndpointSelected
    {
        // To get EndPoint Path select Key
        internal Endpoint? Endpoint { get; set; }
        internal KeyValuePair<string, ExpandoObject> VerbSelected { get; set; }
    }

}