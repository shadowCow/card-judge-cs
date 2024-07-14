namespace BlazorApp1.Models;

public interface IGameLobby
{
    string GetId();
}

public sealed class GameLobby(string id) : IGameLobby
{
    public string GetId()
    {
        return id;
    }
}