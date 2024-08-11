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
    
    public GameClient(
        string playerId,
        IMessageChannel<ToServer> outbound,
        ISubscribable<FromServer> inbound,
        IGuidService guidService)
    {
        _playerId = playerId;
        _fst = ClientFst.Create(new ClientContext(), new ClientState.NotInRoom(playerId));
        _outbound = outbound;
        _guidService = guidService;

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
                Some<ToServer>(new ToServer.Command(_guidService.NewGuid().ToString(), new ServerCommand.JoinRoom(_playerId, e.RoomId))),
            ClientEvent.RequestedCloseRoom e =>
                Some<ToServer>(new ToServer.Command(_guidService.NewGuid().ToString(), new ServerCommand.CloseRoom(_playerId, e.RoomId))),
            ClientEvent.RequestedLeaveRoom e =>
                Some<ToServer>(new ToServer.Command(_guidService.NewGuid().ToString(), new ServerCommand.LeaveRoom(_playerId, e.RoomId))),
            _ => None,
        };

        serverCommand.Iter(_outbound.HandleMessage);
    }

    private void OnError(ClientError err)
    {
        // TODO - stays local, but should 'display'
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

    private class Subscriber(GameClient outer) : ISubscriber<FromServer>
    {
        public void OnMessage(FromServer msg)
        {
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
                default:
                    break;
            }
        }

        private void OnCommandFailure(FromServer.CommandFailure cf)
        {

        }

        private void OnNotification(FromServer.Notification n)
        {

        }
    }
}