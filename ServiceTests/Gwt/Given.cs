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
}