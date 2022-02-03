using Amazon.SimpleEmailV2.Model;

namespace Leadsly.Application.Api
{
    public class ComposeEmailSettingsModel
    {
        public Destination Destination { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string HtmlBody { get; set; }
        public string TextBody { get; set; }
        public string ConfigurationSetName { get; set; }
    }
}
