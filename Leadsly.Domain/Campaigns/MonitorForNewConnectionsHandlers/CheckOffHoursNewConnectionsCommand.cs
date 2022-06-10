namespace Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandlers
{
    public class CheckOffHoursNewConnectionsCommand : ICommand
    {
        public CheckOffHoursNewConnectionsCommand()
        {
        }

        public CheckOffHoursNewConnectionsCommand(string halId)
        {
            HalId = halId;
        }
        public string HalId { get; private set; }
    }
}
