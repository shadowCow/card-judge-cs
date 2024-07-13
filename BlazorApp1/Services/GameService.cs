using BlazorApp1.Models;

namespace BlazorApp1.Services;

public interface IGameService
{
    string CreateGameSession();
    bool HasSession(string sessionId);
}

public class GameService : IGameService
{
    private readonly IGuidService _guidService;
    private Dictionary<string, GameSession> _gameSessions = new Dictionary<string, GameSession>();
    public GameService(IGuidService guidService)
    {
        this._guidService = guidService;
    }

    public string CreateGameSession()
    {
        var gameId = this._guidService.NewGuid().ToString();
        var gs = new GameSession(gameId);
        _gameSessions.Add(gs.GetId(), gs);

        return gs.GetId();
    }

    public bool HasSession(string sessionId)
    {
        return _gameSessions.ContainsKey(sessionId);
    }
}
