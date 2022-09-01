using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public class HalWorkCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public HalWorkCommandHandlerDecorator(ICommandHandler<TCommand> decorated)
        {
            _decorated = decorated;
        }

        private readonly ICommandHandler<TCommand> _decorated;

        public async Task HandleAsync(TCommand command)
        {
            // check to see if right now is during hal's work day
            await this._decorated.HandleAsync(command);
        }
    }
}
