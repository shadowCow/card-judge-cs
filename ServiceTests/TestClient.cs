using BlazorApp1.Domain;
using BlazorApp1.Messaging;

namespace ServiceTests;


public class TestClient : IGameClient
{
    private readonly string playerId;
    private readonly IGameServer server;
    private readonly ISubscribable<FromServer> subscribable;
    private int currentRequestId = 1;
    private string NextRequestId
    {
        get
        {
            var idToReturn = currentRequestId.ToString();
            currentRequestId += 1;
            return idToReturn;
        }
    }

    public TestClient(string PlayerId, IGameServer Server, ISubscribable<FromServer> subscribable)
    {
        this.playerId = PlayerId;
        this.server = Server;
        this.subscribable = subscribable;

        this.subscribable.Subscribe(new Subscriber(this));
    }
    
    // state
    private string? _lobbyId;

    public void CreateGameLobby(string gameId)
    {
        server.Submit(new GameServerCommand.CreateGameLobby(this.playerId, NextRequestId, gameId, playerId));
    }

    public string? GetLobbyId()
    {
        return _lobbyId;
    }

    public bool IsInLobby(string lobbyId)
    {
        return _lobbyId is not null && _lobbyId == lobbyId;
    }

    private class Subscriber(TestClient outer) : ISubscriber<FromServer>
    {
        public void OnMessage(FromServer msg)
        {
            switch (msg)
        {
            case FromServer.CommandResponse response:
                switch (response.Result)
                {
                    case CommandResult.CommandSuccess success:
                        switch (success.Evt)
                        {
                            case GameServerEvent.LobbyCreated lobbyCreated:
                                outer._lobbyId = lobbyCreated.LobbyId;
                                break;
                            default:
                                throw new ArgumentException($"unrecognized GameServerEvent ${success.Evt}");
                        }
                        break;
                    default:
                        throw new ArgumentException($"unrecognized result: {msg}");
                }
                break;
            default:
                throw new ArgumentException($"unrecognized message to client: {msg}");
        }
        }
    }
}

