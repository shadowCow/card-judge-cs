namespace Domain.Services;

public interface IGameClient
{
    void Submit(GameClientCommand c);

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

public abstract record GameClientCommand
{
    private GameClientCommand() {}

    public sealed record CreateGameRoom() : GameClientCommand;
    public sealed record JoinGameRoom(string RoomId) : GameClientCommand;
    public sealed record CloseGameRoom() : GameClientCommand;
    public sealed record ListAvailableGames() : GameClientCommand;
    public sealed record SelectGame(string GameId) : GameClientCommand;
    public sealed record StartGameSession() : GameClientCommand;
    public sealed record MakeMove(object Move) : GameClientCommand;
}
