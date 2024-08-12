namespace Domain.Fst;

using Domain.Ports;
using ServerResult = Either<ServerError, ServerEvent>;

public static class ServerFst
{
    public static Fst<ServerState, ServerCommand, ServerEvent, ServerError, ServerContext> Create(
        ServerContext context,
        ServerState initialState
    )
    {
        return new Fst<ServerState, ServerCommand, ServerEvent, ServerError, ServerContext>(
            HandleCommand,
            Transition,
            context,
            initialState
        );
    }

    public static ServerResult HandleCommand(ServerState s, ServerCommand c, ServerContext context)
    {
        return c switch
        {
            ServerCommand.CreateRoom cr => OnCreateRoom(s, cr, context),
            ServerCommand.JoinRoom jr => OnJoinRoom(s, jr),
            ServerCommand.CloseRoom cr => OnCloseRoom(s, cr),
            _ => Left<ServerError, ServerEvent>(new ServerError.UnknownCommand(c.GetType().Name)),
        };
    }

    private static ServerResult OnCreateRoom(ServerState s, ServerCommand.CreateRoom c, ServerContext ctx)
    {
        return Right<ServerError, ServerEvent>(new ServerEvent.RoomCreated(ctx.GuidService.NewGuid().ToString(), c.PlayerId));
    }

    private static ServerResult OnJoinRoom(ServerState s, ServerCommand.JoinRoom c)
    {
        return s.RoomsById.Find(c.RoomId).Match(
            r => Right<ServerError, ServerEvent>(new ServerEvent.RoomJoined(c.RoomId, c.PlayerId)),
            () => Left<ServerError, ServerEvent>(new ServerError.RoomDoesNotExist(c.RoomId))
        );
    }

    private static ServerResult OnCloseRoom(ServerState s, ServerCommand.CloseRoom c)
    {
        return s.RoomsById.Find(c.RoomId).Match(
            r => Right<ServerError, ServerEvent>(new ServerEvent.RoomClosed(c.RoomId, c.PlayerId)),
            () => Left<ServerError, ServerEvent>(new ServerError.RoomDoesNotExist(c.RoomId))
        );
    }

    public static ServerState Transition(ServerState s, ServerEvent e)
    {
        return e switch
        {
            ServerEvent.RoomCreated rc => ApplyRoomCreated(s, rc),
            ServerEvent.RoomJoined rj => ApplyRoomJoined(s, rj),
            ServerEvent.RoomClosed rc => ApplyRoomClosed(s, rc),
            _ => s,
        };
    }

    private static ServerState ApplyRoomCreated(ServerState s, ServerEvent.RoomCreated e)
    {
        return s with { RoomsById = s.RoomsById.Add(e.RoomId, new Room(e.RoomId, Seq([e.PlayerId])))};
    }

    private static ServerState ApplyRoomJoined(ServerState s, ServerEvent.RoomJoined e)
    {
        return s with { RoomsById = s.RoomsById.Map((k, v) => {
            if (k == e.RoomId)
            {
                return v.WithUser(e.PlayerId);
            }
            else
            {
                return v;
            }
        })};
    }

    private static ServerState ApplyRoomClosed(ServerState s, ServerEvent.RoomClosed e)
    {
        return s with { RoomsById = s.RoomsById.Remove(e.RoomId) };
    }
}

public record ServerContext(IGuidService GuidService) {}

public abstract record ServerCommand
{
    private ServerCommand() {}

    public sealed record CreateRoom(string PlayerId) : ServerCommand();
    public sealed record JoinRoom(string RoomId, string PlayerId) : ServerCommand();
    public sealed record CloseRoom(string RoomId, string PlayerId) : ServerCommand();
    public sealed record LeaveRoom(string RoomId, string PlayerId) : ServerCommand();
}

public abstract record ServerEvent
{
    private ServerEvent() {}
    public sealed record RoomCreated(string RoomId, string PlayerId) : ServerEvent;
    public sealed record RoomJoined(string RoomId, string PlayerId) : ServerEvent;
    public sealed record RoomClosed(string RoomId, string PlayerId) : ServerEvent;
    public sealed record RoomLeft(string RoomId, string PlayerId) : ServerEvent;
}

public abstract record ServerError
{
    private ServerError() {}
    public sealed record UnknownCommand(string Command) : ServerError;
    public sealed record RoomDoesNotExist(string RoomId) : ServerError;
}

public record ServerState(Map<string, Room> RoomsById);

public record Room(string Id, Seq<string> UserIds)
{
    public Room WithUser(string userId)
    {
        return this with { UserIds = UserIds.Add(userId) };
    }
}
