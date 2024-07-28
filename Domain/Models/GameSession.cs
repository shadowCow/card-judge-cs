namespace Domain.Models;

public interface IGameSession
{
    string GetId();
}

public class GameSession(string Id) : IGameSession
{
    public string GetId()
    {
        return Id;
    }
}