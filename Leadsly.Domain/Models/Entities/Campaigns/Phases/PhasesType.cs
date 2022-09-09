namespace Leadsly.Domain.Models.Entities.Campaigns.Phases
{
    public enum PhaseType
    {
        None,
        ProspectList,
        MonitorNewConnections,
        Networking,
        ScanForReplies,
        DeepScan,
        FollowUpMessage,
        SendConnectionRequests,
        SendEmailInvites,
        ConnectionWithdraw,
        RescrapeSearchUrls
    }
}
