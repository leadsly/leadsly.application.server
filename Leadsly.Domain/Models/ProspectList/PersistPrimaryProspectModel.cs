using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.ProspectList
{
    [DataContract]
    public class PersistPrimaryProspectModel
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string ProfileUrl { get; set; }
        [DataMember]
        public long AddedTimestamp { get; set; }
        [DataMember]
        public string PrimaryProspectListId { get; set; }
        [DataMember]
        public string Area { get; set; }
        [DataMember]
        public string EmploymentInfo { get; set; }
        [DataMember]
        public string SearchResultAvatarUrl { get; set; }
    }
}
