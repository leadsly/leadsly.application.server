using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.ScanProspectsForReplies
{
    [DataContract]
    public class NewMessageModel
    {
        [DataMember]
        public string ResponseMessage { get; set; }

        /// <summary>
        /// This isnt the actual resopnse message timestamp, rather the timestamp of when hal created this request object
        /// </summary>
        [DataMember]
        public long ResponseMessageTimestamp { get; set; }
        [DataMember]
        public string ProspectName { get; set; }
    }
}
