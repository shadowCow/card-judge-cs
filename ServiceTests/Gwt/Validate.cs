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

    public static void ClientIsNotInALobby(IGameClient client)
    {
        var lobbyId = client.GetLobbyId();
        if (lobbyId is not null)
        {
            throw new AssertionException($"expected client not to be in a lobby, but was in {lobbyId}");
        }
    }

    public static void ClientIsInLobby(IGameClient client, string lobbyId)
    {
        Assert.That(client.IsInLobby(lobbyId), Is.True);
    }

    public static string ClientIsInASession(IGameClient client)
    {
        var sessionId = client.GetSessionId();
        if (sessionId is null)
        {
            throw new AssertionException("expected client to be in a session");
        }
        else
        {
            return sessionId;
        }
    }


    public static void ClientIsInSession(IGameClient client, string sessionId)
    {
        Assert.That(client.IsInSession(sessionId), Is.True);
    }

    public static void ClientReceivedGameDoesNotExistError(IGameClient client, string gameId)
    {
        var error = client.GetLastError();
        switch (error)
        {
            case GameServerError.GameDoesNotExist gameNotExist:
                Assert.That(gameNotExist.GameId, Is.EqualTo(gameId));
                break;
            default:
                Assert.Fail($"expected last error to be LobbyIsFull, but was ${error}");
                break;
        }
    }

    public static void ClientReceivedLobbyDoesNotExistError(IGameClient client, string lobbyId)
    {
        var error = client.GetLastError();
        switch (error)
        {
            case GameServerError.LobbyDoesNotExist lobbyNotExist:
                Assert.That(lobbyNotExist.LobbyId, Is.EqualTo(lobbyId));
                break;
            default:
                Assert.Fail($"expected last error to be LobbyDoesNotExist, but was ${error}");
                break;
        }
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

    public static void ClientReceivedMoveCommittedEvent(IGameClient client, string sessionId)
    {
        var evt = client.GetLastEvent();
        switch (evt)
        {
            case GameServerEvent.MoveCommitted moveCommitted:
                Assert.That(moveCommitted.SessionId, Is.EqualTo(sessionId));
                break;
            default:
                Assert.Fail($"expected last event to be MoveCommitted, but was {evt}");
                break;
        }
    }
}
