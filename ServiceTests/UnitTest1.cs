using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
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
    public void CreateGameLobbyFailureGameDoesNotExist()
    {
        var invalidGameId = "invalid-game";
        var client = given.NewSystem(player1Id);

        When.ClientCreatesGameLobby(client, invalidGameId);

        Then.Within(Time.AShortTime).Validate(() =>
        {
            Validate.ClientReceivedGameDoesNotExistError(client, invalidGameId);
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
    public void JoinGameLobbyFailureLobbyDoesNotExist()
    {
        var (host, lobbyId) = given.NewSystemWithAGameLobby(player1Id, TestGames.ticTacToeId);
        var guest = given.NewGameClient(player2Id);
        var invalidLobbyId = "invalid-lobby";

        When.ClientJoinsGameLobby(guest, invalidLobbyId);

        Then.Within(Time.AShortTime).Validate(() =>
        {
            Validate.ClientReceivedLobbyDoesNotExistError(guest, invalidLobbyId);
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

    [Test]
    public void CloseLobby()
    {
        var (host, lobbyId) = given.NewSystemWithAGameLobby(player1Id, TestGames.ticTacToeId);
        
        When.ClientClosesGameLobby(host, lobbyId);

        Then.Within(Time.AShortTime).Validate(() =>
        {
            Validate.ClientIsNotInALobby(host);

            When.ClientJoinsGameLobby(host, lobbyId);
            
            Then.Within(Time.AShortTime).Validate(() =>
            {
                Validate.ClientReceivedLobbyDoesNotExistError(host, lobbyId);
            });
        });
    }

    [Test]
    public void CloseNonexistantLobbyReceivesError()
    {
        var host = given.NewSystem(player1Id);
        var lobbyId = "does-not-exist";
        
        When.ClientClosesGameLobby(host, lobbyId);

        Then.Within(Time.AShortTime).Validate(() =>
        {
            Validate.ClientReceivedLobbyDoesNotExistError(host, lobbyId);
        });
    }

    [Test]
    public void CreateAGameSession()
    {
        var (host, guest, lobbyId) = given.NewSystemWithAFullTwoPlayerGameLobby(player1Id, TestGames.ticTacToeId);

        When.ClientCreatesGameSession(host, lobbyId);

        Then.Within(Time.AShortTime).Validate(() =>
        {
            var sessionId = Validate.ClientIsInASession(host);
            Validate.ClientIsInSession(host, sessionId);
            Validate.ClientIsInSession(guest, sessionId);

            When.ClientJoinsGameLobby(guest, lobbyId);

            Then.Within(Time.AShortTime).Validate(() =>
            {
                Validate.ClientReceivedLobbyDoesNotExistError(guest, lobbyId);
            });
        });
    }

    [Test]
    public void JoinAGameSession()
    {
        var (host, guest, sessionId) = given.NewSystemWithATwoPlayerGameSession(player1Id, player2Id, TestGames.ticTacToeId);
        
        When.ClientReconnectsToGameSession(guest, sessionId);

        Then.Within(Time.AShortTime).Validate(() =>
        {
            Validate.ClientIsInSession(guest, sessionId);
        });
    }

    [Test]
    public void MakeAValidMove()
    {
        var (host, guest, sessionId) = given.NewSystemWithATwoPlayerGameSession(player1Id, player2Id, TestGames.ticTacToeId);
        var move = new object();

        When.ClientMakesAValidMove(host, sessionId, move);

        Then.Within(Time.AShortTime).Validate(() =>
        {
            Validate.ClientReceivedMoveCommittedEvent(host, sessionId);
            Validate.ClientReceivedMoveCommittedEvent(guest, sessionId);
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

}
