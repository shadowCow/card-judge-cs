using Domain.Models;

namespace Domain.Services;

public interface IGameServer
{
    // Commands
    void Submit(GameServerCommand msg);

    // State Queries
    bool HasLobby(string lobbyId);
}

public abstract record GameServerCommand
{
    public string ClientId { get; init; }
    public string RequestId { get; init; }

    private GameServerCommand(string clientId, string requestId) {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId, nameof(clientId));
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId, nameof(requestId));
        
        ClientId = clientId;
        RequestId = requestId;
    }

    public sealed record CreateGameLobby(string ClientId, string RequestId, string GameId, string PlayerId) : GameServerCommand(ClientId, RequestId);
    public sealed record JoinGameLobby(string ClientId, string RequestId, string LobbyId, string PlayerId) : GameServerCommand(ClientId, RequestId);
    public sealed record CloseGameLobby(string ClientId, string RequestId, string LobbyId, string PlayerId) : GameServerCommand(ClientId, RequestId);
    public sealed record CreateGameSession(string ClientId, string RequestId, string LobbyId, string PlayerId) : GameServerCommand(ClientId, RequestId);
    public sealed record ReconnectToGameSession(string ClientId, string RequestId, string SessionId, string PlayerId): GameServerCommand(ClientId, RequestId);
    public sealed record MakeMove(string ClientId, string RequestId, string SessionId, string PlayerId, object Move) : GameServerCommand(ClientId, RequestId);
}

public abstract record FromServer
{
    private FromServer() {}

    public sealed record CommandResponse(string ClientId, string RequestId, CommandResult Result) : FromServer;
    public sealed record Notification(string ClientId, GameServerEvent Evt) : FromServer;
}

public abstract record CommandResult
{
    private CommandResult() {}

    public sealed record CommandSuccess(GameServerEvent Evt) : CommandResult;
    public sealed record CommandFailure(GameServerError Error) : CommandResult;
}

public abstract record GameServerEvent
{
    private GameServerEvent() {}

    public sealed record LobbyCreated(string LobbyId, string PlayerId) : GameServerEvent;
    public sealed record LobbyJoined(string LobbyId, string PlayerId) : GameServerEvent;
    public sealed record LobbyClosed(string LobbyId, string[] PlayerIds) : GameServerEvent;
    public sealed record SessionCreated(string SessionId, string[] PlayerIds) : GameServerEvent;
    public sealed record ConnectedToSession(string SessionId, string PlayerId) : GameServerEvent;
    public sealed record MoveCommitted(string SessionId, string PlayerId, object move, string nextTurnPlayerId, GameStatus status) : GameServerEvent;
}

public abstract record GameServerError
{
    private GameServerError() {}
    
    public sealed record UnknownError(string Detail) : GameServerError;
    public sealed record UnknownCommand(string CommandName) : GameServerError;
    public sealed record GameDoesNotExist(string GameId) : GameServerError;
    public sealed record LobbyIsFull(string LobbyId) : GameServerError;
    public sealed record LobbyDoesNotExist(string LobbyId) : GameServerError;
    public sealed record SessionDoesNotExist(string SessionId) : GameServerError;
    public sealed record InvalidMove(string SessionId, string Reason) : GameServerError;
    public sealed record MoveOutOfTurn(string SessionId, string PlayerId) : GameServerError;
}
