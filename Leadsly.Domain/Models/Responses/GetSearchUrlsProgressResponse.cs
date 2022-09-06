using Leadsly.Domain.Models.Networking;
using System.Collections.Generic;

namespace Leadsly.Domain.Models.Responses
{
    public class GetSearchUrlsProgressResponse
    {
        public IList<SearchUrlProgressModel> Items { get; set; }
    }
}
