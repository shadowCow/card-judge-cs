using Domain.Adapters;
using Domain.Fst;
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

    public (IGameClient client, string roomId) NewSystemWithARoom(string playerId);
}

public class Given : IGiven
{
    private Broadcast<FromServer>? broadcast;
    private GameServer? server;

    private readonly InMemoryGameRepository gameRepo = new(TestGames.All);

    private class ChannelToServer(IGameServer server) : IMessageChannel<ToServer>
    {
        public void HandleMessage(ToServer msg)
        {
            server.Submit(msg);
        }
    }

    public IGameClient NewSystem(string playerId)
    {
        this.broadcast = new();
        this.server = new GameServer(new GuidServiceSystem(), gameRepo, this.broadcast);

        return NewGameClient(playerId);
    }

    public IGameClient NewGameClient(string playerId)
    {
        if (this.broadcast is null || this.server is null)
        {
            throw new InvalidOperationException("broadcast and server must be initialized before you can create a new game client");
        }
        return new GameClient(playerId, new ChannelToServer(server), this.broadcast, new GuidServiceSystem());
    }

    public (IGameClient client, string roomId) NewSystemWithARoom(string playerId)
    {
        var client = NewSystem(playerId);

        When.ClientCommand(client, new ClientCommand.CreateRoom());

        var roomId = "";
        Then.WithinAShortTime().Validate(() =>
        {
            roomId = Validate.ClientIsInARoom(client);
        });

        return (client, roomId);
    }
}