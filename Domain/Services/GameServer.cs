using Domain.Fst;
using Domain.Messaging;
using Domain.Models;
using Domain.MonadUtil;
using Domain.Ports;

namespace Domain.Services;

using GameServerFst = Fst<ServerState, ServerCommand, ServerEvent, ServerError, ServerContext>;

public class GameServer : IGameServer
{
    private readonly GameServerFst _fst;
    private readonly IGuidService _guidService;
    private readonly IGameRepository _gameRepo;
    private readonly IMessageChannel<FromServer> _outbound;

    public GameServer(
        IGuidService guidService,
        IGameRepository gameRepo,
        IMessageChannel<FromServer> outbound)
    {
        _guidService = guidService;
        _gameRepo = gameRepo;
        _outbound = outbound;

        _fst = ServerFst.Create(new ServerContext(_guidService), new ServerState(Empty));
    }

    public void Submit(ToServer msg)
    {
        switch (msg)
        {
            case ToServer.Command cmd:
                HandleCommand(cmd);
                break;
            default:
                // TODO - respond error
                break;
        }
        
    }

    private void HandleCommand(ToServer.Command msg)
    {
        _fst.HandleCommand(msg.Cmd)
            .Match(
                evt => OnCommandSuccess(msg.RequestId, evt),
                err => OnCommandFailure(msg.RequestId, err)
            );
    }

    private void OnCommandSuccess(string requestId, ServerEvent evt)
    {
        var fromServer = evt switch
        {
            ServerEvent.RoomCreated rc => Some<FromServer>(new FromServer.CommandSuccess(requestId, evt)),
            _ => None,
        };

        fromServer.Iter(Send);
    }

    private void OnCommandFailure(string requestId, ServerError err)
    {
        // TODO respond
    }

    private void Send(FromServer msg)
    {
        _outbound.HandleMessage(msg);
    }
}