namespace Leadsly.Application.Api
{
    public interface IHtmlTemplateGenerator
    {
        string GenerateBodyFor(EmailTemplateTypes templateType);
    }
}
