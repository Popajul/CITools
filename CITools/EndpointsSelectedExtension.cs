namespace CITools
{
    internal static class EndpointsSelectedExtension
    {
        internal static IEnumerable<EndpointSelected> GetEndpointSelectedByKeyword(this IEnumerable<EndpointSelected> endpointsSelected, string keyword)
        {
            return endpointsSelected.Where(e => e.Endpoint!.Path.Key.Contains(keyword));
        }
    }
}
