using Domain.Services;

namespace ServiceTests.Gwt;

public class Validate
{
    
    public static void LobbyExists(IGameServer server, string lobbyId)
    {
        Assert.That(server.HasLobby(lobbyId), Is.True);
    }

    public static string ClientIsInALobby(IGameClient client)
    {
        var lobbyId = client.GetLobbyId();
        if (lobbyId is null)
        {
            throw new AssertionException("expected client to be in a lobby");
        }
        else
        {
            return lobbyId;
        }
    }

    public static void ClientIsInLobby(IGameClient client, string lobbyId)
    {
        Assert.That(client.IsInLobby(lobbyId), Is.True);
    }

}
