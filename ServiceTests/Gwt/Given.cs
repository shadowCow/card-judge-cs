using Domain.Messaging;
using Domain.Ports;
using Domain.Services;
using ServiceTests.Adapters;
using ServiceTests.Util;

namespace ServiceTests.Gwt;

public interface IGiven
{
    public IGameClient NewSystem(string playerId);

    public IGameClient NewGameClient(string playerId);

    public (IGameClient client, string lobbyId) NewSystemWithAGameLobby(string playerId, string gameId);

    public (IGameClient host, IGameClient guest, string lobbyId) NewSystemWithAFullTwoPlayerGameLobby(string playerId, string gameId);

    public (IGameClient client, string lobbyId) NewSystemWithAFullGameLobby(string playerId, string gameId);

    public (IGameClient host, IGameClient guest, string sessionId) NewSystemWithATwoPlayerGameSession(string hostPlayerId, string guestPlayerId, string gameId);
}

public class Given : IGiven
{
    private Broadcast<FromServer>? broadcast;
    private GameServer? server;

    private readonly InMemoryGameRepository gameRepo = new(TestGames.All);

    public IGameClient NewSystem(string playerId)
    {
        this.broadcast = new();
        this.server = new GameServer(new GuidService(), gameRepo, this.broadcast);
        return NewGameClient(playerId);
    }

    public IGameClient NewGameClient(string playerId)
    {
        if (this.broadcast is null || this.server is null)
        {
            throw new InvalidOperationException("broadcast and server must be initialized before you can create a new game client");
        }
        return new TestClient(playerId, this.server, this.broadcast);
    }

    public (IGameClient client, string lobbyId) NewSystemWithAGameLobby(string playerId, string gameId)
    {
        var client = NewSystem(playerId);
        client.CreateGameLobby(gameId);

        string lobbyId = "";
        Then.Within(Time.AShortTime).Validate(() =>
        {
            lobbyId = Validate.ClientIsInALobby(client);
        });

        return (client, lobbyId);
    }

    public (IGameClient host, IGameClient guest, string lobbyId) NewSystemWithAFullTwoPlayerGameLobby(string playerId, string gameId)
    {
        var host = NewSystem(playerId);
        host.CreateGameLobby(gameId);

        string lobbyId = "";
        Then.Within(Time.AShortTime).Validate(() =>
        {
            lobbyId = Validate.ClientIsInALobby(host);
        });

        var player2Id = "p2";
        var guest = NewGameClient(player2Id);
        
        When.ClientJoinsGameLobby(guest, lobbyId);

        Then.Within(Time.AShortTime).Validate(() =>
        {
            Validate.ClientIsInLobby(guest, lobbyId);
        });

        return (host, guest, lobbyId);
    }

    public (IGameClient client, string lobbyId) NewSystemWithAFullGameLobby(string playerId, string gameId)
    {
        var client = NewSystem(playerId);
        client.CreateGameLobby(gameId);

        string lobbyId = "";
        Then.Within(Time.AShortTime).Validate(() =>
        {
            lobbyId = Validate.ClientIsInALobby(client);
        });

        var player2Id = "p2";
        var guest = NewGameClient(player2Id);
        
        When.ClientJoinsGameLobby(guest, lobbyId);

        Then.Within(Time.AShortTime).Validate(() =>
        {
            Validate.ClientIsInLobby(guest, lobbyId);
        });

        return (client, lobbyId);
    }

    public (IGameClient host, IGameClient guest, string sessionId) NewSystemWithATwoPlayerGameSession(string hostPlayerId, string guestPlayerId, string gameId)
    {
        var (host, guest, lobbyId) = NewSystemWithAFullTwoPlayerGameLobby(hostPlayerId, gameId);

        When.ClientCreatesGameSession(host, lobbyId);

        string sessionId = "";
        Then.Within(Time.AShortTime).Validate(() =>
        {
            sessionId = Validate.ClientIsInASession(host);
            Validate.ClientIsInSession(host, sessionId);
            Validate.ClientIsInSession(guest, sessionId);
        });

        return (host, guest, sessionId);
    }
}