using Domain.Services;
using FluentAssertions;

namespace DomainTests;

public class ClientTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void CreateRoom()
    {
        // Arrange
        var playerId = "p1";
        var fst = ClientFst.Create(new ClientContext(), new ClientState.NotInRoom(playerId));

        // Act
        var result = fst.HandleCommand(new ClientCommand.CreateRoom());

        // Assert
        var expectedResult = ClientFst.Success(new ClientEvent.RequestedCreateRoom(playerId));
        var expectedState = new ClientState.CreatingRoom(playerId);
        result.Should().BeEquivalentTo(expectedResult);
        fst.GetState().Should().BeStrictlyEquivalentTo(expectedState);
    }

    [Test]
    public void JoinRoom()
    {
        // Arrange
        var playerId = "p1";
        var roomId = "r1";
        var fst = ClientFst.Create(new ClientContext(), new ClientState.NotInRoom(playerId));

        // Act
        var result = fst.HandleCommand(new ClientCommand.JoinRoom(roomId));

        // Assert
        var expectedResult = ClientFst.Success(new ClientEvent.RequestedJoinRoom(playerId, roomId));
        var expectedState = new ClientState.JoiningRoom(playerId, roomId);
        result.Should().BeEquivalentTo(expectedResult);
        fst.GetState().Should().BeEquivalentTo(expectedState);
    }

    [Test]
    public void CloseRoom()
    {
        // Arrange
        var playerId = "p1";
        var roomId = "r1";
        var fst = ClientFst.Create(new ClientContext(), new ClientState.HostingRoom(playerId, roomId, false));

        // Act
        var result = fst.HandleCommand(new ClientCommand.CloseRoom(roomId));

        // Assert
        var expectedResult = ClientFst.Success(new ClientEvent.RequestedCloseRoom(playerId, roomId));
        var expectedState = new ClientState.HostingRoom(playerId, roomId, true);
        result.Should().BeEquivalentTo(expectedResult);
        fst.GetState().Should().BeEquivalentTo(expectedState);
    }

    [Test]
    public void LeaveRoom()
    {
        // Arrange
        var playerId = "p1";
        var roomId = "r1";
        var fst = ClientFst.Create(new ClientContext(), new ClientState.GuestInRoom(playerId, roomId, false));

        // Act
        var result = fst.HandleCommand(new ClientCommand.LeaveRoom(roomId));

        // Assert
        var expectedResult = ClientFst.Success(new ClientEvent.RequestedLeaveRoom(playerId, roomId));
        var expectedState = new ClientState.GuestInRoom(playerId, roomId, true);
        result.Should().BeEquivalentTo(expectedResult);
        fst.GetState().Should().BeEquivalentTo(expectedState);
    }
}