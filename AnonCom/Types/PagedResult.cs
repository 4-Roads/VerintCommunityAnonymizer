using System.Collections.Generic;

namespace FourRoads.TelligentCommunity.Utilities.VerintCommunityAnonymizer.Types
{
    internal class PagedResult<T> : Result where T : class, new()
    {
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public int TotalCount { get; set; }
        public List<T> Users { get; set; }
    }
}