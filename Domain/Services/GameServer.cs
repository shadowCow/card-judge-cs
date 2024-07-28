using Domain.Messaging;
using Domain.Models;
using Domain.MonadUtil;
using Domain.Ports;

namespace Domain.Services;

using ServerResult = Result<GameServerEvent, GameServerError>;

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
}

public abstract record GameServerError
{
    private GameServerError() {}
    
    public sealed record UnknownError(string Detail) : GameServerError;
    public sealed record UnknownCommand(string CommandName) : GameServerError;
    public sealed record GameDoesNotExist(string GameId) : GameServerError;
    public sealed record LobbyIsFull(string LobbyId) : GameServerError;
    public sealed record LobbyDoesNotExist(string LobbyId) : GameServerError;
}

public class GameServer(
    IGuidService guidService,
    IGameRepository gameRepo,
    IMessageChannel<FromServer> outbound) : IGameServer
{
    private readonly Dictionary<string, IGameLobby> _lobbies = [];
    private readonly Dictionary<string, IGameSession> _sessions = [];

    public void Submit(GameServerCommand msg)
    {
        ServerResult result = msg switch
        {
            GameServerCommand.CreateGameLobby createGameLobby => OnCreateGameLobby(createGameLobby),
            GameServerCommand.JoinGameLobby joinGameLobby => OnJoinGameLobby(joinGameLobby),
            GameServerCommand.CloseGameLobby closeGameLobby => OnCloseGameLobby(closeGameLobby),
            GameServerCommand.CreateGameSession createGameSession => OnCreateGameSession(createGameSession),
            _ => ServerResult.Error(new GameServerError.UnknownCommand(msg.GetType().Name))
        };

        switch (result)
        {
            case ServerResult.SuccessResult success:
                OnSuccess(msg, success.Value);
                break;
            case ServerResult.ErrorResult err:
                OnFailure(msg, err.Err);
                break;
            default:
                OnFailure(msg, new GameServerError.UnknownCommand(msg.GetType().Name));
                break;
        }
    }

    private ServerResult OnCreateGameLobby(GameServerCommand.CreateGameLobby msg)
    {
        var game = gameRepo.GetById(msg.GameId);
        if (game is null)
        {
            return ServerResult.Error(new GameServerError.GameDoesNotExist(msg.GameId));
        }

        var lobbyId = guidService.NewGuid();
        _lobbies.Add(lobbyId.ToString(), new GameLobby(lobbyId.ToString(), msg.GameId, game.MaxPlayers, msg.PlayerId));

        return ServerResult.Success(new GameServerEvent.LobbyCreated(lobbyId.ToString(), msg.PlayerId));
    }

    private ServerResult OnJoinGameLobby(GameServerCommand.JoinGameLobby msg)
    {
        if (_lobbies.TryGetValue(msg.LobbyId, out var lobby))
        {
            var err = lobby.Join(msg.PlayerId);
            if (err is not null)
            {
                GameServerError serverErr = err switch
                {
                    GameLobbyError.LobbyIsFullError lobbyErr => new GameServerError.LobbyIsFull(lobbyErr.LobbyId),
                    _ => new GameServerError.UnknownError($"unexpected GameLobbyError: ${err}")
                };

                return ServerResult.Error(serverErr);
            }
            else
            {
                return ServerResult.Success(new GameServerEvent.LobbyJoined(msg.LobbyId, msg.PlayerId));
            }
        }
        else
        {
            return ServerResult.Error(new GameServerError.LobbyDoesNotExist(msg.LobbyId));
        }
    }

    private ServerResult OnCloseGameLobby(GameServerCommand.CloseGameLobby msg)
    {
        if (_lobbies.TryGetValue(msg.LobbyId, out var lobby))
        {
            var playerIds = lobby.ListPlayers().ToArray();
            _lobbies.Remove(msg.LobbyId);
            return ServerResult.Success(new GameServerEvent.LobbyClosed(msg.LobbyId, playerIds));
        }
        else
        {
            return ServerResult.Error(new GameServerError.LobbyDoesNotExist(msg.LobbyId));
        }
    }

    private ServerResult OnCreateGameSession(GameServerCommand.CreateGameSession msg)
    {
        if (_lobbies.TryGetValue(msg.LobbyId, out var lobby))
        {
            var playerIds = lobby.ListPlayers().ToArray();
            _lobbies.Remove(msg.LobbyId);
            
            var sessionId = guidService.NewGuid().ToString();
            _sessions.Add(sessionId, new GameSession(sessionId));
            return ServerResult.Success(new GameServerEvent.SessionCreated(msg.LobbyId, playerIds));
        }
        else
        {
            return ServerResult.Error(new GameServerError.LobbyDoesNotExist(msg.LobbyId));
        }
    }

    public bool HasLobby(string lobbyId)
    {
        throw new NotImplementedException();
    }

    private void OnSuccess(GameServerCommand msg, GameServerEvent evt)
    {
        outbound.HandleMessage(new FromServer.CommandResponse(
            msg.ClientId,
            msg.RequestId,
            new CommandResult.CommandSuccess(evt)
        ));
    }

    private void OnFailure(GameServerCommand msg, GameServerError err)
    {
        outbound.HandleMessage(new FromServer.CommandResponse(
            msg.ClientId,
            msg.RequestId,
            new CommandResult.CommandFailure(err)
        ));
    }
}