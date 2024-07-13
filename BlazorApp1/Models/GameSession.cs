namespace BlazorApp1.Models;

public interface IGameSession
{
    string GetId();

    GameSessionResult End();
}

public sealed class GameSessionImpl(string Id) : IGameSession
{

    public string GetId()
    {
        return Id;
    }

    public GameSessionResult End()
    {
        return new(Id, new GameSessionOutcome.Aborted());
    }
}

public record GameSessionResult(string SessionId, GameSessionOutcome Outcome);

public abstract record GameSessionOutcome
{
    private GameSessionOutcome(){}

    public sealed record Winner(string PlayerId) : GameSessionOutcome;
    public sealed record Tie : GameSessionOutcome;
    public sealed record Aborted : GameSessionOutcome;
}