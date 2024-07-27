using Domain.Services;

namespace ServiceTests.Gwt;

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

    public static void ClientClosesGameLobby(IGameClient gameClient, string lobbyId)
    {
        gameClient.CloseGameLobby(lobbyId);
    }
}