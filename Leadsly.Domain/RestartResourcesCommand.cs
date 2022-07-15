namespace Leadsly.Domain
{
    public class RestartResourcesCommand : ICommand
    {
        public RestartResourcesCommand(string halId)
        {
            HalId = halId;
        }

        public string HalId { get; private set; }
    }
}
