namespace Leadsly.Domain.Exceptions
{
    public interface ILeadslyApiException
    {
        string Type { get; }
        string Title { get; }
        int Status { get; }
        string Detail { get; }
        string Instance { get; }
    }
}
