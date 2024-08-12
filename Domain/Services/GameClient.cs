using Domain.Fst;
using Domain.Messaging;
using Domain.Ports;

namespace Domain.Services;

using GameClientFst = Domain.Fst.Fst<ClientState, ClientCommand, ClientEvent, ClientError, ClientContext>;

public class GameClient : IGameClient
{
    private readonly string _playerId;
    private readonly GameClientFst _fst;
    private readonly IMessageChannel<ToServer> _outbound;
    private readonly IGuidService _guidService;
    private readonly ILogger _logger;
    private ClientError? _lastError;
    
    public GameClient(
        string playerId,
        IMessageChannel<ToServer> outbound,
        ISubscribable<FromServer> inbound,
        IGuidService guidService,
        ILogger logger)
    {
        _playerId = playerId;
        _fst = ClientFst.Create(new ClientContext(), new ClientState.NotInRoom(playerId));
        _outbound = outbound;
        _guidService = guidService;
        _logger = logger;

        inbound.Subscribe(new Subscriber(this));
    }

    public void Submit(ClientCommand c)
    {
        _fst.HandleCommand(c)
            .Match(
                OnEvent,
                OnError
            );
    }

    private void OnEvent(ClientEvent evt)
    {
        Option<ToServer> serverCommand = evt switch
        {
            ClientEvent.RequestedCreateRoom e =>
                Some<ToServer>(new ToServer.Command(_guidService.NewGuid().ToString(), new ServerCommand.CreateRoom(_playerId))),
            ClientEvent.RequestedJoinRoom e =>
                Some<ToServer>(new ToServer.Command(_guidService.NewGuid().ToString(), new ServerCommand.JoinRoom(e.RoomId, _playerId))),
            ClientEvent.RequestedCloseRoom e =>
                Some<ToServer>(new ToServer.Command(_guidService.NewGuid().ToString(), new ServerCommand.CloseRoom(e.RoomId, _playerId))),
            ClientEvent.RequestedLeaveRoom e =>
                Some<ToServer>(new ToServer.Command(_guidService.NewGuid().ToString(), new ServerCommand.LeaveRoom(e.RoomId, _playerId))),
            _ => None,
        };

        _logger.Info($"GameClient sending {serverCommand}");
        serverCommand.Iter(_outbound.HandleMessage);
    }

    private void OnError(ClientError err)
    {
        _lastError = err;
    }

    public string? GetRoomId()
    {
        return _fst.GetState() switch
        {
            ClientState.HostingRoom s => s.RoomId,
            ClientState.GuestInRoom s => s.RoomId,
            _ => null,
        };
    }

    public ClientError? GetLastError()
    {
        return _lastError;
    }

    private class Subscriber(GameClient outer) : ISubscriber<FromServer>
    {
        public void OnMessage(FromServer msg)
        {
            outer._logger.Info($"GameClient received {msg}");
            switch (msg)
            {
                case FromServer.CommandSuccess cs:
                    OnCommandSuccess(cs);
                    break;
                case FromServer.CommandFailure cf:
                    OnCommandFailure(cf);
                    break;
                case FromServer.Notification n:
                    OnNotification(n);
                    break;
                default:
                    throw new ArgumentException($"unrecognized message to client: {msg}");
            }
        }

        private void OnCommandSuccess(FromServer.CommandSuccess cs)
        {
            switch (cs.Event)
            {
                case ServerEvent.RoomCreated rc:
                    outer._fst.ApplyEvent(new ClientEvent.RoomCreated(rc.RoomId));
                    break;
                case ServerEvent.RoomJoined rj:
                    outer._fst.ApplyEvent(new ClientEvent.RoomJoined(rj.RoomId));
                    break;
                default:
                    break;
            }
        }

        private void OnCommandFailure(FromServer.CommandFailure cf)
        {
            outer._lastError = new ClientError.RoomLimitExceeded();
        }

        private void OnNotification(FromServer.Notification n)
        {

        }
    }
}