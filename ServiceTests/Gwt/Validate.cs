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

    public static void ClientReceivedLobbyIsFullError(IGameClient client, string lobbyId)
    {
        var error = client.GetLastError();
        switch (error)
        {
            case GameServerError.LobbyIsFull lobbyFull:
                Assert.That(lobbyFull.LobbyId, Is.EqualTo(lobbyId));
                break;
            default:
                Assert.Fail($"expected last error to be LobbyIsFull, but was ${error}");
                break;
        }
    }
}
