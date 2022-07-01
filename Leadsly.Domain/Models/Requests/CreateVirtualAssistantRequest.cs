using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.Requests
{
    [DataContract]
    public class CreateVirtualAssistantRequest
    {
        [DataMember(Name = "userId", IsRequired = true)]
        public string UserId { get; set; } = string.Empty;

        [DataMember(Name = "timeZoneId", IsRequired = true)]
        public string TimeZoneId { get; set; } = string.Empty;
    }
}
