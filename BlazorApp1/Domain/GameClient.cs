namespace BlazorApp1.Domain;

public interface IGameClient
{
    // Actions
    void CreateGameLobby(string gameId);

    // State Queries
    string? GetLobbyId();
    bool IsInLobby(string lobbyId);
}