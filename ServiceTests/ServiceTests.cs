using Domain.Adapters;
using Domain.Services;
using ServiceTests.Adapters;
using ServiceTests.Gwt;
using ServiceTests.Util;

namespace ServiceTests;

public class ServiceTests
{
    // TODO - IGiven implementation would be determined by test configuration and injected.
    private readonly IGiven given = new Given(() => new LoggerNoOp());

    private const string player1Id = "p1";
    private const string player2Id = "p2";
    private const string player3Id = "p3";

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void CreateANewRoom()
    {
        var client = given.NewSystem(player1Id);

        When.ClientCommand(client, new ClientCommand.CreateRoom());

        Then.WithinAShortTime().Validate(() =>
        {
            Validate.ClientIsInARoom(client);
        });
    }

    [Test]
    public void CannotCreateTwoRoomsAtOnce()
    {
        (var client, var roomId) = given.NewSystemWithARoom(player1Id);

        When.ClientCommand(client, new ClientCommand.CreateRoom());

        Then.WithinAShortTime().Validate(() =>
        {
            Validate.ClientHasInvalidCommandStateError(client);
        });
    }

    [Test]
    public void JoinARoom()
    {
        (var host, var roomId) = given.NewSystemWithARoom(player1Id);
        var guest = given.NewGameClient(player2Id);

        When.ClientCommand(guest, new ClientCommand.JoinRoom(roomId));

        Then.WithinAShortTime().Validate(() =>
        {
            Validate.ClientIsInRoom(guest, roomId);
        });
    }

}
