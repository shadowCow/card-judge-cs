using Domain.Services;
using Domain.Messaging;

namespace ServiceTests.Adapters;


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
    private GameServerError? _lastError;

    public void CreateGameLobby(string gameId)
    {
        server.Submit(new GameServerCommand.CreateGameLobby(this.playerId, NextRequestId, gameId, playerId));
    }

    public void JoinGameLobby(string lobbyId)
    {
        server.Submit(new GameServerCommand.JoinGameLobby(this.playerId, NextRequestId, lobbyId, playerId));
    }

    public void CloseGameLobby(string lobbyId)
    {
        server.Submit(new GameServerCommand.CloseGameLobby(this.playerId, NextRequestId, lobbyId, playerId));
    }

    public string? GetLobbyId()
    {
        return _lobbyId;
    }

    public bool IsInLobby(string lobbyId)
    {
        return _lobbyId is not null && _lobbyId == lobbyId;
    }

    public GameServerError? GetLastError()
    {
        return _lastError;
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
                            outer._lastError = null;
                            switch (success.Evt)
                            {
                                case GameServerEvent.LobbyCreated lobbyCreated:
                                    outer._lobbyId = lobbyCreated.LobbyId;
                                    break;
                                case GameServerEvent.LobbyJoined lobbyJoined:
                                    outer._lobbyId = lobbyJoined.LobbyId;
                                    break;
                                case GameServerEvent.LobbyClosed lobbyClosed:
                                    if (outer._lobbyId == lobbyClosed.LobbyId)
                                    {
                                        outer._lobbyId = null;
                                    }
                                    break;
                                default:
                                    throw new ArgumentException($"unrecognized GameServerEvent ${success.Evt}");
                            }
                            break;
                        case CommandResult.CommandFailure failure:
                            switch (failure.Error)
                            {
                                case GameServerError.GameDoesNotExist:
                                case GameServerError.LobbyIsFull:
                                case GameServerError.LobbyDoesNotExist:
                                case GameServerError.UnknownCommand:
                                case GameServerError.UnknownError:
                                    outer._lastError = failure.Error;
                                    break;
                                default:
                                    throw new ArgumentException($"unrecognized GameServerError ${failure.Error}");
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

