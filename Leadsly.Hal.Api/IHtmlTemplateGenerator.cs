namespace Leadsly.Hal.Api
{
    public interface IHtmlTemplateGenerator
    {
        string GenerateBodyFor(EmailTemplateTypes templateType);
    }
}
