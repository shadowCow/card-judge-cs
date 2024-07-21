using Domain.MonadUtil;

namespace Domain.Models;

public interface IGameLobby
{
    // Actions
    GameLobbyError? Join(string playerId);

    // State Queries
    string GetId();
    string GetGameId();
    string HasPlayer(string playerId);
}

public abstract record GameLobbyError
{
    private GameLobbyError() {}

    public sealed record LobbyIsFullError(string LobbyId) : GameLobbyError;

    public static GameLobbyError LobbyIsFull(string LobbyId) => new LobbyIsFullError(LobbyId);
}

public sealed class GameLobby(string id, string gameId, int maxPlayers, string hostPlayerId) : IGameLobby
{
    private readonly HashSet<string> playerIds = [hostPlayerId];

    public GameLobbyError? Join(string playerId)
    {
        if (this.playerIds.Count == maxPlayers)
        {
            return GameLobbyError.LobbyIsFull(playerId);
        }
        else
        {
            this.playerIds.Add(playerId);
            return null;
        }
    }

    public string GetId()
    {
        return id;
    }

    public string GetGameId()
    {
        return gameId;
    }

    public string HasPlayer(string playerId)
    {
        throw new NotImplementedException();
    }
}