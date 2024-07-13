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

        var sessionId = when.CreateGameSession(service);

        then.SessionExists(service, sessionId);
    }

    [Test]
    public void EndAGameSession()
    {
        (var service, var sessionId) = given.ASessionExists();

        var result = when.EndGameSession(service, sessionId);

        then.SessionDoesNotExist(service, sessionId);
        then.SessionWasAborted(result);
    }
}

public interface IGiven
{
    IGameService NewGameService();
    (IGameService, string) ASessionExists();
}

public class DomainGiven : IGiven
{
    public IGameService NewGameService()
    {
        return new GameService(new GuidService());
    }

    public (IGameService, string) ASessionExists()
    {
        var service = NewGameService();
        return (service, service.CreateGameSession());
    }
}

public interface IWhen
{
    string CreateGameSession(IGameService gameService);
    EndGameSessionResult EndGameSession(IGameService service, string sessionId);
}

public class DomainWhen : IWhen
{
    public string CreateGameSession(IGameService gameService)
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