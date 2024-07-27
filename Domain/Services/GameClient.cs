namespace Domain.Services;

public interface IGameClient
{
    // Actions
    void CreateGameLobby(string gameId);
    void JoinGameLobby(string lobbyId);
    void CloseGameLobby(string lobbyId);

    // State Queries
    string? GetLobbyId();
    bool IsInLobby(string lobbyId);
    GameServerError? GetLastError();
}