namespace Domain.Services;

public class GameClient : IGameClient
{
    public void Submit(GameClientCommand c)
    {
        throw new NotImplementedException();
    }

    public void CloseGameLobby(string lobbyId)
    {
        throw new NotImplementedException();
    }

    public void CreateGameLobby(string gameId)
    {
        throw new NotImplementedException();
    }

    public void CreateGameSession(string lobbyId)
    {
        throw new NotImplementedException();
    }

    public GameServerError? GetLastError()
    {
        throw new NotImplementedException();
    }

    public GameServerEvent? GetLastEvent()
    {
        throw new NotImplementedException();
    }

    public string? GetLobbyId()
    {
        throw new NotImplementedException();
    }

    public string? GetSessionId()
    {
        throw new NotImplementedException();
    }

    public bool IsInLobby(string lobbyId)
    {
        throw new NotImplementedException();
    }

    public bool IsInSession(string sessionId)
    {
        throw new NotImplementedException();
    }

    public void JoinGameLobby(string lobbyId)
    {
        throw new NotImplementedException();
    }

    public void MakeMove(string sessionId, object move)
    {
        throw new NotImplementedException();
    }

    public void ReconnectToGameSession(string sessionId)
    {
        throw new NotImplementedException();
    }

    
}