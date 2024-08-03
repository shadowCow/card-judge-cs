namespace Domain.Services;

public interface IGameClient
{
    // Actions
    void CreateGameLobby(string gameId);
    void JoinGameLobby(string lobbyId);
    void CloseGameLobby(string lobbyId);
    void CreateGameSession(string lobbyId);
    void ReconnectToGameSession(string sessionId);
    void MakeMove(string sessionId, object move);

    // State Queries
    string? GetLobbyId();
    bool IsInLobby(string lobbyId);
    string? GetSessionId();
    bool IsInSession(string sessionId);
    GameServerError? GetLastError();
    GameServerEvent? GetLastEvent();
}