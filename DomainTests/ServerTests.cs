
using Domain.Adapters;
using Domain.Fst;
using FluentAssertions;

namespace DomainTests;

[TestFixture]
public class YourClassNameTests
{
    [Test]
    public void CreateARoom()
    {
        // Arrange
        var id = GuidServiceFixed.allZeroGuid;
        var serverFst = ServerFst.Create(
            new ServerContext(GuidServiceFixed.AllZero()),
            new ServerState([])
        );
        var playerId = "p1";

        // Act
        var result = serverFst.HandleCommand(new ServerCommand.CreateRoom(playerId));

        // Assert
        var expectedResult = Right<ServerError, ServerEvent>(new ServerEvent.RoomCreated(playerId, id));
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void JoinARoom()
    {
        // Arrange
        var playerId = "p1";
        var player2Id = "p2";
        var roomId = GuidServiceFixed.allZeroGuid;
        var serverFst = ServerFst.Create(
            new ServerContext(GuidServiceFixed.AllZero()),
            new ServerState(Map((roomId, new Room(roomId, Seq<string>([playerId])))))
        );

        // Act
        var result = serverFst.HandleCommand(new ServerCommand.JoinRoom(roomId, player2Id));

        // Assert
        var expectedResult = Right<ServerError, ServerEvent>(new ServerEvent.RoomJoined(roomId, player2Id));
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void JoinANonExistentRoom()
    {
        // Arrange
        var playerId = "p1";
        var player2Id = "p2";
        var roomId = GuidServiceFixed.allZeroGuid;
        var noSuchRoomId = "no-such-room";
        var serverFst = ServerFst.Create(
            new ServerContext(GuidServiceFixed.AllZero()),
            new ServerState(Map((roomId, new Room(roomId, Seq<string>([playerId])))))
        );

        // Act
        var result = serverFst.HandleCommand(new ServerCommand.JoinRoom(noSuchRoomId, player2Id));

        // Assert
        var expectedResult = Left<ServerError, ServerEvent>(new ServerError.RoomDoesNotExist(noSuchRoomId));
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void CloseARoom()
    {
        // Arrange
        var playerId = "p1";
        var roomId = GuidServiceFixed.allZeroGuid;
        var serverFst = ServerFst.Create(
            new ServerContext(GuidServiceFixed.AllZero()),
            new ServerState(Map((roomId, new Room(roomId, Seq<string>([playerId])))))
        );

        // Act
        var result = serverFst.HandleCommand(new ServerCommand.CloseRoom(roomId, playerId));

        // Assert
        var expectedResult = Right<ServerError, ServerEvent>(new ServerEvent.RoomClosed(roomId, playerId));
        result.Should().BeEquivalentTo(expectedResult);
    }
}
