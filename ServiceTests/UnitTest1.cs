using System.Diagnostics;
using BlazorApp1.Domain;
using BlazorApp1.Messaging;
using BlazorApp1.Models;
using BlazorApp1.Services;
using Microsoft.VisualBasic;

namespace ServiceTests;

public class Tests
{
    // TODO - IGiven implementation would be determined by test configuration and injected.
    private readonly IGiven given = new Given();
    private readonly When when = new();
    private readonly Then then = new();
    private readonly Validate validate = new();

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void CreateANewGameLobby()
    {
        var playerId = "p1";
        var gameId = "card_judge";
        var client = given.NewSystem(playerId);

        when.ClientCreatesGameLobby(client, gameId);

        then.Within(TimeSpan.FromMilliseconds(50)).Validate(() => {
            var lobbyId = validate.ClientIsInALobby(client);
            validate.ClientIsInLobby(client, lobbyId);
        });
    }

    // [Test]
    // public void JoinAGameLobby()
    // {
    //     throw new NotImplementedException();
    // }

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
}

public class Given : IGiven
{
    private Broadcast<FromServer>? broadcast;
    private GameServer? server;

    public IGameClient NewSystem(string playerId)
    {
        this.broadcast = new();
        this.server = new GameServer(new GuidService(), this.broadcast);
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
}

public class When
{
    public void ClientCreatesGameLobby(IGameClient gameClient, string gameId)
    {
        gameClient.CreateGameLobby(gameId);
    }

    public string CreateGameSession(IGameService gameService, string hostPlayerId)
    {
        return gameService.CreateGameSession();
    }

    public EndGameSessionResult EndGameSession(IGameService service, string sessionId)
    {
        return service.EndGameSession(sessionId);
    }
}

public class Then
{
    public Within Within(TimeSpan timeSpan)
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
    
    public void LobbyExists(IGameServer server, string lobbyId)
    {
        Assert.That(server.HasLobby(lobbyId), Is.True);
    }

    public string ClientIsInALobby(IGameClient client)
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

    public void ClientIsInLobby(IGameClient client, string lobbyId)
    {
        Assert.That(client.IsInLobby(lobbyId), Is.True);
    }

    public void SessionExists(IGameService gameService, string sessionId)
    {
        Assert.That(gameService.HasSession(sessionId), Is.True);
    }

    public void SessionDoesNotExist(IGameService gameService, string sessionId)
    {
        Assert.That(gameService.HasSession(sessionId), Is.False);
    }

    public void SessionWasAborted(EndGameSessionResult result)
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