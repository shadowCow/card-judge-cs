using BlazorApp1.Models;

namespace BlazorApp1.Services;

public interface IGameService
{
    string CreateGameLobby(string gameId, string playerId);
    bool HasLobby(string lobbyId);
    string CreateGameSession();
    bool HasSession(string sessionId);
    EndGameSessionResult EndGameSession(string sessionId);
}

public class GameService(IGuidService guidService) : IGameService
{
    private readonly IGuidService _guidService = guidService;
    private readonly Dictionary<string, IGameSession> _gameSessions = [];
    private readonly Dictionary<string, IGameLobby> _lobbies = [];

    public string CreateGameLobby(string gameId, string playerId)
    {
        var lobbyId = this._guidService.NewGuid().ToString();

        return lobbyId;
    }

    public bool HasLobby(string lobbyId)
    {
        return this._lobbies.ContainsKey(lobbyId);
    }

    public string CreateGameSession()
    {
        var gameId = this._guidService.NewGuid().ToString();
        var gs = new GameSessionImpl(gameId);
        _gameSessions.Add(gs.GetId(), gs);

        return gs.GetId();
    }

    public bool HasSession(string sessionId)
    {
        return _gameSessions.ContainsKey(sessionId);
    }

    public EndGameSessionResult EndGameSession(string sessionId)
    {
        var session = _gameSessions[sessionId];

        if (session is null)
        {
            return new EndGameSessionResult.NotFound(sessionId);
        }
        else
        {
            _gameSessions.Remove(sessionId);
            return new EndGameSessionResult.Success(session.End());
        }
    }
}

public abstract record EndGameSessionResult
{
    private EndGameSessionResult(){}

    public sealed record NotFound(string SessionId) : EndGameSessionResult;
    public sealed record Success(GameSessionResult SessionResult) : EndGameSessionResult;
}