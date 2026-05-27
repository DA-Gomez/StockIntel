namespace StockIntel.Application.Common;

//interface has generic type params. the "in"keyword means contravariance. the "where" is a generic constraint
// ( TCommand must implement ICommand<TResponse> -> public class CreateUserCommand : ICommand<Guid>{ } )
public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse>
{
  //Task means the method is asynchronous. If a ? is found inside the <> the it means its nullable (could return null or the inside "TResponse")
  // CancellationToken is used to cancel async operations
  Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

//Unit means No meaningful return val
public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, Unit> where TCommand : ICommand
{
  
}

//Each use case will be a command type plus a handler type. 