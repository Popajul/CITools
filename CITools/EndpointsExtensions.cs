using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CITools
{
    internal static class EndpointsExtensions
    {
        internal static List<EndpointSelected> GetEndpointByIndex(this IEnumerable<Endpoint> endpoints,IEnumerable<int> indexes)
        {
            return indexes.Select(i =>
            {
                var endpoint = endpoints.Single(e => e.Index == i);
                return new EndpointSelected() { Endpoint = endpoint, VerbSelected = endpoint.Verb, InitialIndex = endpoint.Index };
            }).ToList();
        }
        internal static List<EndpointSelected> GetEndpointByKeyword(this IEnumerable<Endpoint> endpoints,string keyword)
        {
            var endpointslist = endpoints.ToList();
            return endpointslist!.Where(e => e.Path.Key.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .Select(e => new EndpointSelected() { Endpoint = e, VerbSelected = e.Verb, InitialIndex = e.Index }).ToList();
        }
    }
}
