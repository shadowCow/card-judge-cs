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
            .Do(ApplyEvent);
    }

    public void ApplyEvent(TEvent e)
    {
        _currentState = transition(_currentState, e);
    }

    public TState GetState()
    {
        return _currentState;
    }
}
