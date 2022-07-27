namespace Leadsly.Domain.Services.Interfaces
{
    public interface IUrlService
    {
        public string GetHalsBaseUrl(string namespaceName, string serviceDiscName);
    }
}
