﻿using System;
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
            return indexes.Select(i => endpoints.SelectMany(e => e.Verbs!, (e, v) => new { Endpoint = e, Verb = v }).ElementAt(i)).Select(e => new EndpointSelected() { Endpoint = e.Endpoint, VerbSelected = e.Verb }).ToList();
        }
        internal static List<EndpointSelected> GetEndpointByKeyword(this IEnumerable<Endpoint> endpoints,string keyword)
        {
            return endpoints!.Where(e => e.Path.Key.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .SelectMany(e => e.Verbs!, (e, v) => new { Endpoint = e, Verb = v })
                .Select(e => new EndpointSelected() { Endpoint = e.Endpoint, VerbSelected = e.Verb }).ToList();
        }
    }
}
