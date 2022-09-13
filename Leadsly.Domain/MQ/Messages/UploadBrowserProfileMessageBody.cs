using System.Runtime.Serialization;

namespace Leadsly.Domain.MQ.Messages
{
    [DataContract]
    public class UploadBrowserProfileMessageBody : PublishMessageBody
    {
        [DataMember]
        public string BrowserDefaultProfileDir { get; set; }

        [DataMember]
        public string BrowserUserDirectoryPath { get; set; }

        [DataMember]
        public string DefaultBrowserProfileName { get; set; }

        [DataMember]
        public string BucketName { get; set; }
    }
}
