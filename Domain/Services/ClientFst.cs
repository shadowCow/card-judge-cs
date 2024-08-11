namespace Domain.Services;

using System;
using Domain.Fst;
using ClientResult = Either<ClientError, ClientEvent>;

public abstract record ClientCommand
{
    private ClientCommand() {}

    public sealed record CreateRoom() : ClientCommand;
    public sealed record JoinRoom(string RoomId) : ClientCommand;
    public sealed record CloseRoom(string RoomId) : ClientCommand;
    public sealed record LeaveRoom(string RoomId) : ClientCommand;
}

public abstract record ClientEvent
{
    private ClientEvent() {}

    public sealed record RequestedCreateRoom(string PlayerId) : ClientEvent;
    public sealed record RequestedJoinRoom(string PlayerId, string RoomId) : ClientEvent;
    public sealed record RequestedCloseRoom(string PlayerId, string RoomId) : ClientEvent;
    public sealed record RequestedLeaveRoom(string PlayerId, string RoomId) : ClientEvent;
    public sealed record RoomCreated(string RoomId) : ClientEvent;
    public sealed record RoomJoined(string RoomId) : ClientEvent;
    public sealed record RoomClosed(string RoomId) : ClientEvent;
    public sealed record RoomLeft(string RoomId) : ClientEvent;
}

public abstract record ClientError
{
    private ClientError() {}
    public sealed record RoomLimitExceeded() : ClientError;
    public sealed record RoomDoesNotExist(string RoomId) : ClientError;
    public sealed record Unauthorized() : ClientError;
    public sealed record InvalidCommandState(ClientCommand Command, ClientState State) : ClientError;
}

public record ClientContext();

public abstract record ClientState
{
    public string PlayerId { get; init; }

    private ClientState(string playerId) {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerId, nameof(playerId));

        PlayerId = playerId;
    }

    public sealed record NotInRoom(string PlayerId) : ClientState(PlayerId);
    public sealed record CreatingRoom(string PlayerId) : ClientState(PlayerId);
    public sealed record JoiningRoom(string PlayerId, string RoomId) : ClientState(PlayerId);
    public sealed record HostingRoom(string PlayerId, string RoomId, bool IsClosing) : ClientState(PlayerId);
    public sealed record GuestInRoom(string PlayerId, string RoomId, bool IsLeaving) : ClientState(PlayerId);
}

public static class ClientFst
{
    public static Fst<ClientState, ClientCommand, ClientEvent, ClientError, ClientContext> Create(
        ClientContext context,
        ClientState initialState
    )
    {
        return new Fst<ClientState, ClientCommand, ClientEvent, ClientError, ClientContext>(
            HandleCommand,
            Transition,
            context,
            initialState
        );
    }

    public static ClientResult HandleCommand(ClientState s, ClientCommand c, ClientContext ctx)
    {
        return c switch
        {
            ClientCommand.CreateRoom cr => OnCreateRoom(s, cr),
            ClientCommand.JoinRoom jr => OnJoinRoom(s, jr),
            ClientCommand.CloseRoom cr => OnCloseRoom(s, cr),
            ClientCommand.LeaveRoom lr => OnLeaveRoom(s, lr),
            _ => Failure(new ClientError.Unauthorized())
        };
    }

    private static ClientResult OnCreateRoom(ClientState s, ClientCommand.CreateRoom cr)
    {
        return s switch
        {
            ClientState.NotInRoom nir => Success(new ClientEvent.RequestedCreateRoom(s.PlayerId)),
            _ => Failure(new ClientError.InvalidCommandState(cr, s)),
        };
    }

    private static ClientResult OnJoinRoom(ClientState s, ClientCommand.JoinRoom jr)
    {
        return s switch
        {
            ClientState.NotInRoom nir => Success(new ClientEvent.RequestedJoinRoom(s.PlayerId, jr.RoomId)),
            _ => Failure(new ClientError.InvalidCommandState(jr, s)),
        };
    }

    private static ClientResult OnCloseRoom(ClientState s, ClientCommand.CloseRoom cr)
    {
        return s switch
        {
            ClientState.HostingRoom hr => Success(new ClientEvent.RequestedCloseRoom(s.PlayerId, cr.RoomId)),
            _ => Failure(new ClientError.InvalidCommandState(cr, s)),
        };
    }

    private static ClientResult OnLeaveRoom(ClientState s, ClientCommand.LeaveRoom lr)
    {
        return s switch
        {
            ClientState.GuestInRoom gir => Success(new ClientEvent.RequestedLeaveRoom(s.PlayerId, lr.RoomId)),
            _ => Failure(new ClientError.InvalidCommandState(lr, s)),
        };
    }

    public static ClientResult Failure(ClientError err) => Left(err);
    public static ClientResult Success(ClientEvent evt) => Right(evt);

    public static ClientState Transition(ClientState s, ClientEvent e)
    {
        return e switch
        {
            ClientEvent.RequestedCreateRoom rcr => ApplyRequestedCreateRoom(s, rcr),
            ClientEvent.RequestedJoinRoom rjr => ApplyRequestedJoinRoom(s, rjr),
            ClientEvent.RequestedCloseRoom rcr => ApplyRequestedCloseRoom(s, rcr),
            ClientEvent.RequestedLeaveRoom rlr => ApplyRequestedLeaveRoom(s, rlr),
            ClientEvent.RoomCreated rc => ApplyRoomCreated(s, rc),
            ClientEvent.RoomJoined rj => ApplyRoomJoined(s, rj),
            ClientEvent.RoomClosed rc => ApplyRoomClosed(s, rc),
            _ => s,
        };
    }

    private static ClientState ApplyRequestedCreateRoom(ClientState s, ClientEvent.RequestedCreateRoom rcr)
    {
        return new ClientState.CreatingRoom(s.PlayerId);
    }

    private static ClientState ApplyRequestedJoinRoom(ClientState s, ClientEvent.RequestedJoinRoom rjr)
    {
        return new ClientState.JoiningRoom(s.PlayerId, rjr.RoomId);
    }

    private static ClientState ApplyRequestedCloseRoom(ClientState s, ClientEvent.RequestedCloseRoom rcr)
    {
        return new ClientState.HostingRoom(s.PlayerId, rcr.RoomId, true);
    }

    private static ClientState ApplyRequestedLeaveRoom(ClientState s, ClientEvent.RequestedLeaveRoom rlr)
    {
        return new ClientState.GuestInRoom(s.PlayerId, rlr.RoomId, true);
    }

    private static ClientState ApplyRoomCreated(ClientState s, ClientEvent.RoomCreated e)
    {
        return new ClientState.HostingRoom(s.PlayerId, e.RoomId, false);
    }

    private static ClientState ApplyRoomJoined(ClientState s, ClientEvent.RoomJoined e)
    {
        return new ClientState.GuestInRoom(s.PlayerId, e.RoomId, false);
    }

    private static ClientState ApplyRoomClosed(ClientState s, ClientEvent.RoomClosed e)
    {
        return new ClientState.NotInRoom(s.PlayerId);
    }
}