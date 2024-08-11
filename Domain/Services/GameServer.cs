using Domain.Messaging;
using Domain.Models;
using Domain.MonadUtil;
using Domain.Ports;

namespace Domain.Services;

using ServerResult = Result<GameServerEvent, GameServerError>;

public class GameServer(
    IGuidService guidService,
    IGameRepository gameRepo,
    IMessageChannel<FromServer> outbound) : IGameServer
{
    private readonly Dictionary<string, IGameLobby> _lobbies = [];
    private readonly Dictionary<string, IGameSession> _sessions = [];

    public void Submit(GameServerCommand msg)
    {
        ServerResult result = msg switch
        {
            GameServerCommand.CreateGameLobby createGameLobby => OnCreateGameLobby(createGameLobby),
            GameServerCommand.JoinGameLobby joinGameLobby => OnJoinGameLobby(joinGameLobby),
            GameServerCommand.CloseGameLobby closeGameLobby => OnCloseGameLobby(closeGameLobby),
            GameServerCommand.CreateGameSession createGameSession => OnCreateGameSession(createGameSession),
            GameServerCommand.MakeMove makeMove => OnMakeMove(makeMove),
            _ => ServerResult.Error(new GameServerError.UnknownCommand(msg.GetType().Name))
        };

        switch (result)
        {
            case ServerResult.SuccessResult success:
                OnSuccess(msg, success.Value);
                break;
            case ServerResult.ErrorResult err:
                OnFailure(msg, err.Err);
                break;
            default:
                OnFailure(msg, new GameServerError.UnknownCommand(msg.GetType().Name));
                break;
        }
    }

    private ServerResult OnCreateGameLobby(GameServerCommand.CreateGameLobby msg)
    {
        var game = gameRepo.GetById(msg.GameId);
        if (game is null)
        {
            return ServerResult.Error(new GameServerError.GameDoesNotExist(msg.GameId));
        }

        var lobbyId = guidService.NewGuid();
        _lobbies.Add(lobbyId.ToString(), new GameLobby(lobbyId.ToString(), msg.GameId, game.MaxPlayers, msg.PlayerId));

        return ServerResult.Success(new GameServerEvent.LobbyCreated(lobbyId.ToString(), msg.PlayerId));
    }

    private ServerResult OnJoinGameLobby(GameServerCommand.JoinGameLobby msg)
    {
        if (_lobbies.TryGetValue(msg.LobbyId, out var lobby))
        {
            var err = lobby.Join(msg.PlayerId);
            if (err is not null)
            {
                GameServerError serverErr = err switch
                {
                    GameLobbyError.LobbyIsFullError lobbyErr => new GameServerError.LobbyIsFull(lobbyErr.LobbyId),
                    _ => new GameServerError.UnknownError($"unexpected GameLobbyError: ${err}")
                };

                return ServerResult.Error(serverErr);
            }
            else
            {
                return ServerResult.Success(new GameServerEvent.LobbyJoined(msg.LobbyId, msg.PlayerId));
            }
        }
        else
        {
            return ServerResult.Error(new GameServerError.LobbyDoesNotExist(msg.LobbyId));
        }
    }

    private ServerResult OnCloseGameLobby(GameServerCommand.CloseGameLobby msg)
    {
        if (_lobbies.TryGetValue(msg.LobbyId, out var lobby))
        {
            var playerIds = lobby.ListPlayers().ToArray();
            _lobbies.Remove(msg.LobbyId);
            return ServerResult.Success(new GameServerEvent.LobbyClosed(msg.LobbyId, playerIds));
        }
        else
        {
            return ServerResult.Error(new GameServerError.LobbyDoesNotExist(msg.LobbyId));
        }
    }

    private ServerResult OnCreateGameSession(GameServerCommand.CreateGameSession msg)
    {
        if (_lobbies.TryGetValue(msg.LobbyId, out var lobby))
        {
            var playerIds = lobby.ListPlayers().ToArray();
            _lobbies.Remove(msg.LobbyId);
            
            var sessionId = guidService.NewGuid().ToString();
            _sessions.Add(sessionId, new GameSession(sessionId));
            return ServerResult.Success(new GameServerEvent.SessionCreated(sessionId, playerIds));
        }
        else
        {
            return ServerResult.Error(new GameServerError.LobbyDoesNotExist(msg.LobbyId));
        }
    }

    private ServerResult OnMakeMove(GameServerCommand.MakeMove msg)
    {
        if (_sessions.TryGetValue(msg.SessionId, out var session))
        {
            var result = session.MakeMove(msg.PlayerId, msg.Move);
            return result switch
            {
                MoveResult.MoveCommitted moveCommitted => ServerResult.Success(new GameServerEvent.MoveCommitted(msg.SessionId, msg.PlayerId, msg.Move, moveCommitted.nextTurnPlayerId, moveCommitted.status)),
                MoveResult.MoveFailed moveFailed => ServerResult.Error(new GameServerError.InvalidMove(msg.SessionId, moveFailed.reason)),
                _ => ServerResult.Error(new GameServerError.UnknownError($"unknown MoveResult {result}"))
            };
        }
        else
        {
            return ServerResult.Error(new GameServerError.SessionDoesNotExist(msg.SessionId));
        }
    }

    public bool HasLobby(string lobbyId)
    {
        throw new NotImplementedException();
    }

    private void OnSuccess(GameServerCommand msg, GameServerEvent evt)
    {
        outbound.HandleMessage(new FromServer.CommandResponse(
            msg.ClientId,
            msg.RequestId,
            new CommandResult.CommandSuccess(evt)
        ));
    }

    private void OnFailure(GameServerCommand msg, GameServerError err)
    {
        outbound.HandleMessage(new FromServer.CommandResponse(
            msg.ClientId,
            msg.RequestId,
            new CommandResult.CommandFailure(err)
        ));
    }
}