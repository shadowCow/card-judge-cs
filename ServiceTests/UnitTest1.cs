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
}

public interface IGiven
{
    IGameService NewGameService();
}

public class DomainGiven : IGiven
{
    public IGameService NewGameService()
    {
        return new GameService(new GuidService());
    }
}

public interface IWhen
{
    string CreateGameSession(IGameService gameService);
}

public class DomainWhen : IWhen
{
    public string CreateGameSession(IGameService gameService)
    {
        return gameService.CreateGameSession();
    }
}

public interface IThen
{
    void SessionExists(IGameService gameService, string sessionId);
}

public class DomainThen : IThen
{
    public void SessionExists(IGameService gameService, string sessionId)
    {
        Assert.That(gameService.HasSession(sessionId), Is.True);
    }
}