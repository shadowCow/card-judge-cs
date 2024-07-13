using BlazorApp1.Models;
using BlazorApp1.Services;
using Microsoft.VisualBasic;

namespace ServiceTests;

public class Tests
{
    private IGiven given = new DomainGiven();
    private IWhen when = new DomainWhen();
    private IThen then = new DomainThen();

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void CreateANewGameSession()
    {
        var service = given.NewGameService();
        var hostPlayerId = "p1";

        var sessionId = when.CreateGameSession(service, hostPlayerId);

        then.SessionExists(service, sessionId);
    }

    [Test]
    public void EndAGameSession()
    {
        var (service, sessionId, _) = given.ASessionExists();

        var result = when.EndGameSession(service, sessionId);

        then.SessionDoesNotExist(service, sessionId);
        then.SessionWasAborted(result);
    }

    [Test]
    public void JoinAGameSession()
    {
        (var service, var sessionId, var hostPlayerId) = given.ASessionExists();
        var playerId = "p2";

        // TBD - need to decide...
        // is there a 'session lobby',
        // where players join and wait
        // for everyone else to join?
        // and once everyone has joined,
        // the host can create the session?
        // or in other words...
        // SessionLobby can be created by the host
        // without knowing the player list yet.
        // GameSession must be created with
        // the player list already determined.
    }
}

public interface IGiven
{
    IGameService NewGameService();
    (IGameService Service, string SessionId, string HostPlayerId) ASessionExists();
}

public class DomainGiven : IGiven
{
    public IGameService NewGameService()
    {
        return new GameService(new GuidService());
    }

    public (IGameService Service, string SessionId, string HostPlayerId) ASessionExists()
    {
        var service = NewGameService();
        var hostPlayerId = "p1";
        return (service, service.CreateGameSession(), hostPlayerId);
    }
}

public interface IWhen
{
    string CreateGameSession(IGameService gameService, string hostPlayerId);
    EndGameSessionResult EndGameSession(IGameService service, string sessionId);
}

public class DomainWhen : IWhen
{
    public string CreateGameSession(IGameService gameService, string hostPlayerId)
    {
        return gameService.CreateGameSession();
    }

    public EndGameSessionResult EndGameSession(IGameService service, string sessionId)
    {
        return service.EndGameSession(sessionId);
    }
}

public interface IThen
{
    void SessionExists(IGameService gameService, string sessionId);
    void SessionDoesNotExist(IGameService gameService, string sessionId);
    public void SessionWasAborted(EndGameSessionResult result);
}

public class DomainThen : IThen
{
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