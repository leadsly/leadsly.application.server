using System.Threading.Tasks;

namespace Leadsly.Domain.PhaseConsumers
{
    public interface IConsumeCommandHandler<TCommand> where TCommand : IConsumeCommand
    {
        Task ConsumeAsync(TCommand command);
    }
}
