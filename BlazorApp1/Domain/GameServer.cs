using BlazorApp1.Messaging;
using BlazorApp1.Models;
using BlazorApp1.Services;

namespace BlazorApp1.Domain;

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
    public sealed record CommandUnknown(string CommandName) : CommandResult;
}

public abstract record GameServerEvent
{
    private GameServerEvent() {}

    public sealed record LobbyCreated(string LobbyId) : GameServerEvent;
}

public class GameServer(IGuidService guidService, IMessageChannel<FromServer> outbound) : IGameServer
{
    private readonly Dictionary<string, IGameLobby> _lobbies = [];

    public void Submit(GameServerCommand msg)
    {
        switch (msg)
        {
            case GameServerCommand.CreateGameLobby createGameLobby:
                this.OnCreateGameLobby(createGameLobby);
                break;
            default:
                outbound.HandleMessage(new FromServer.CommandResponse(
                    msg.ClientId,
                    msg.RequestId,
                    new CommandResult.CommandUnknown(msg.GetType().Name))
                );
                break;
        }
    }

    private void OnCreateGameLobby(GameServerCommand.CreateGameLobby msg)
    {
        var lobbyId = guidService.NewGuid();
        _lobbies.Add(lobbyId.ToString(), new GameLobby(lobbyId.ToString()));

        outbound.HandleMessage(new FromServer.CommandResponse(
            msg.ClientId,
            msg.RequestId,
            new CommandResult.CommandSuccess(new GameServerEvent.LobbyCreated(lobbyId.ToString()))
        ));
    }

    public bool HasLobby(string lobbyId)
    {
        throw new NotImplementedException();
    }
}