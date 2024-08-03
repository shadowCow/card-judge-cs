namespace Domain.Models;

public interface IGame
{
    MoveResult MakeMove(string playerId, object move);
}

public abstract record MoveResult
{
    private MoveResult() {}

    public sealed record MoveCommitted(string playerId, object move, string nextTurnPlayerId, GameStatus status) : MoveResult() {}
    public sealed record MoveFailed(string playerId, object move, string reason) : MoveResult() {}
}

public abstract record GameStatus
{
    private GameStatus() {}

    public sealed record Active() : GameStatus() {}
    public sealed record Completed(GameOutcome Outcome) : GameStatus() {}
}

public abstract record GameOutcome
{
    private GameOutcome() {}
    public sealed record Winner(string PlayerId) : GameOutcome {}
    public sealed record Tie() : GameOutcome {}
    public sealed record Aborted() : GameOutcome {}
}