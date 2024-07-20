using System.Diagnostics;
using BlazorApp1.Domain;
using BlazorApp1.Messaging;
using BlazorApp1.Models;
using BlazorApp1.Services;

namespace ServiceTests;

public class Tests
{
    // TODO - IGiven implementation would be determined by test configuration and injected.
    private readonly IGiven given = new Given();

    private readonly TimeSpan aShortTime = TimeSpan.FromMilliseconds(50);

    private const string player1Id = "p1";
    private const string player2Id = "p2";

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void CreateANewGameLobby()
    {
        var client = given.NewSystem(player1Id);

        When.ClientCreatesGameLobby(client, TestGames.ticTacToeId);

        Then.Within(aShortTime).Validate(() =>
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

        Then.Within(aShortTime).Validate(() =>
        {
            Validate.ClientIsInLobby(guest, lobbyId);
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

public interface IGiven
{
    public IGameClient NewSystem(string playerId);

    public IGameClient NewGameClient(string playerId);

    public (IGameClient client, string lobbyId) NewSystemWithAGameLobby(string playerId, string gameId);
}

public class Given : IGiven
{
    private Broadcast<FromServer>? broadcast;
    private GameServer? server;

    private readonly InMemoryGameRepository gameRepo = new(TestGames.All);

    public IGameClient NewSystem(string playerId)
    {
        this.broadcast = new();
        this.server = new GameServer(new GuidService(), gameRepo, this.broadcast);
        return NewGameClient(playerId);
    }

    public IGameClient NewGameClient(string playerId)
    {
        if (this.broadcast is null || this.server is null)
        {
            throw new InvalidOperationException("broadcast and server must be initialized before you can create a new game client");
        }
        return new TestClient(playerId, this.server, this.broadcast);
    }

    public (IGameClient client, string lobbyId) NewSystemWithAGameLobby(string playerId, string gameId)
    {
        var client = NewSystem(playerId);
        client.CreateGameLobby(gameId);

        string lobbyId = "";
        Then.Within(Times.AShortTime).Validate(() =>
        {
            lobbyId = Validate.ClientIsInALobby(client);
        });

        return (client, lobbyId);
    }
}

public class When
{
    public static void ClientCreatesGameLobby(IGameClient gameClient, string gameId)
    {
        gameClient.CreateGameLobby(gameId);
    }

    public static void ClientJoinsGameLobby(IGameClient gameClient, string lobbyId)
    {
        gameClient.JoinGameLobby(lobbyId);
    }

    public static string CreateGameSession(IGameService gameService, string hostPlayerId)
    {
        return gameService.CreateGameSession();
    }

    public static EndGameSessionResult EndGameSession(IGameService service, string sessionId)
    {
        return service.EndGameSession(sessionId);
    }
}

public class Then
{
    public static Within Within(TimeSpan timeSpan)
    {
        return new(timeSpan);
    }
}

public class Within(TimeSpan timeSpan)
{
    public void Validate(Action validation)
    {
        var stopwatch = Stopwatch.StartNew();
        Exception? exception;

        do
        {
            try
            {
                validation();
                exception = null;
                break;
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                Thread.Sleep(10);
            }
        }
        while (stopwatch.Elapsed < timeSpan);

        if (exception is not null)
        {
            throw new TimeoutException(
                message: "Validation timed out.",
                innerException: exception
            );
        }
    }
}

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

    public static void SessionExists(IGameService gameService, string sessionId)
    {
        Assert.That(gameService.HasSession(sessionId), Is.True);
    }

    public static void SessionDoesNotExist(IGameService gameService, string sessionId)
    {
        Assert.That(gameService.HasSession(sessionId), Is.False);
    }

    public static void SessionWasAborted(EndGameSessionResult result)
    {
        switch (result)
        {
            case EndGameSessionResult.Success success:
                switch (success.SessionResult.Outcome)
                {
                    case GameSessionOutcome.Aborted aborted:
                        // yay!
                        break;
                    default:
                        Assert.Fail($"expected Aborted but found: {success.SessionResult.Outcome}");
                        break;
                }
                break;
            default:
                Assert.Fail($"expected Success but found: {result}");
                break;
        }
    }

}

class Times
{
    public static readonly TimeSpan AShortTime = TimeSpan.FromMilliseconds(50);
}

class TestGames
{
    public const string ticTacToeId = "tic-tac-toe";
    public static readonly Game ticTacToe = new(ticTacToeId, 2);

    public static readonly Game[] All = [ticTacToe];
}