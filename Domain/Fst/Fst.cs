using LanguageExt;

namespace Domain.Fst;

public class Fst<TState, TCommand, TEvent, TError, TContext>(
    Func<TState, TCommand, TContext, Either<TError, TEvent>> commandHandler,
    Func<TState, TEvent, TState> transition,
    TContext context, 
    TState initialState
)
{
    private TState _currentState = initialState;

    public Either<TError, TEvent> HandleCommand(TCommand c)
    {
        return commandHandler(_currentState, c, context)
            .Do(evt => {
                _currentState = transition(_currentState, evt);
            });
    }

    public TState GetState()
    {
        return _currentState;
    }
}
