using Domain.Services;

namespace ServiceTests.Gwt;

public class Validate
{
    public static string ClientIsInARoom(IGameClient client)
    {
        var roomId = client.GetRoomId();
        if (string.IsNullOrWhiteSpace(roomId))
        {
            
            throw new AssertionException("expected client to be in a room");
        }
        else
        {
            return roomId;
        }
    }

    public static void ClientIsInRoom(IGameClient client, string expectedRoomId)
    {
        var roomId = client.GetRoomId();
        if (string.IsNullOrWhiteSpace(roomId))
        {
            
            throw new AssertionException("expected client to be in a room");
        }
        else
        {
            Assert.That(roomId, Is.EqualTo(expectedRoomId));
        }
    }

    public static void ClientHasInvalidCommandStateError(IGameClient client)
    {
        var error = client.GetLastError();
        switch (error)
        {
            case ClientError.InvalidCommandState e:
                break;
            default:
                Assert.Fail($"expected last error to be InvalidCommandState, but was ${error}");
                break;
        }
    }

    public static void ClientHasRoomLimitExceededError(IGameClient client)
    {
        var error = client.GetLastError();
        switch (error)
        {
            case ClientError.RoomLimitExceeded e:
                break;
            default:
                Assert.Fail($"expected last error to be RoomLimitExceeded, but was ${error}");
                break;
        }
    }

    // public static void ClientReceivedGameDoesNotExistError(IGameClient client, string gameId)
    // {
    //     var error = client.GetLastError();
    //     switch (error)
    //     {
    //         case GameServerError.GameDoesNotExist gameNotExist:
    //             Assert.That(gameNotExist.GameId, Is.EqualTo(gameId));
    //             break;
    //         default:
    //             Assert.Fail($"expected last error to be LobbyIsFull, but was ${error}");
    //             break;
    //     }
    // }
}
