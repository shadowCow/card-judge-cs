namespace Domain.Models;

public interface IGameSession
{
    string GetId();
    MoveResult MakeMove(string playerId, object move);
}

public class GameSession(string Id) : IGameSession
{
    public string GetId()
    {
        return Id;
    }

    public MoveResult MakeMove(string playerId, object move)
    {
        return new MoveResult.MoveCommitted(playerId, move, playerId, new GameStatus.Active());
    }
}