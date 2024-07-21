using ServiceTests.Adapters;
using ServiceTests.Gwt;
using ServiceTests.Util;

namespace ServiceTests;

public class Tests
{
    // TODO - IGiven implementation would be determined by test configuration and injected.
    private readonly IGiven given = new Given();

    private const string player1Id = "p1";
    private const string player2Id = "p2";
    private const string player3Id = "p3";

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void CreateANewGameLobby()
    {
        var client = given.NewSystem(player1Id);

        When.ClientCreatesGameLobby(client, TestGames.ticTacToeId);

        Then.Within(Time.AShortTime).Validate(() =>
        {
            var lobbyId = Validate.ClientIsInALobby(client);
            Validate.ClientIsInLobby(client, lobbyId);
        });
    }

    [Test]
    public void JoinAGameLobby()
    {
        var (host, lobbyId) = given.NewSystemWithAGameLobby(player1Id, TestGames.ticTacToeId);
        var guest = given.NewGameClient(player2Id);
        
        When.ClientJoinsGameLobby(guest, lobbyId);

        Then.Within(Time.AShortTime).Validate(() =>
        {
            Validate.ClientIsInLobby(guest, lobbyId);
        });
    }

    [Test]
    public void JoinGameLobbyFailureLobbyIsFull()
    {
        var (host, lobbyId) = given.NewSystemWithAFullGameLobby(player1Id, TestGames.ticTacToeId);
        var guest = given.NewGameClient(player3Id);

        When.ClientJoinsGameLobby(guest, lobbyId);

        Then.Within(Time.AShortTime).Validate(() =>
        {
            Validate.ClientReceivedLobbyIsFullError(guest, lobbyId);
        });
    }

    // [Test]
    // public void EndAGameSession()
    // {
    //     throw new NotImplementedException();
    //     // var (service, sessionId, _) = given.ASessionExists();

    //     // var result = when.EndGameSession(service, sessionId);

    //     // then.SessionDoesNotExist(service, sessionId);
    //     // then.SessionWasAborted(result);
    // }

    // [Test]
    // public void JoinAGameSession()
    // {
    //     throw new NotImplementedException();
    //     // (var service, var sessionId, var hostPlayerId) = given.ASessionExists();
    //     // var playerId = "p2";

    //     // TBD - need to decide...
    //     // is there a 'session lobby',
    //     // where players join and wait
    //     // for everyone else to join?
    //     // and once everyone has joined,
    //     // the host can create the session?
    //     // or in other words...
    //     // SessionLobby can be created by the host
    //     // without knowing the player list yet.
    //     // GameSession must be created with
    //     // the player list already determined.
    // }
}
