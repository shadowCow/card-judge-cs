using System.Reflection.Metadata;
using Domain.Services;

namespace ConsoleClient;



public class Client
{
    public static Seq<string> OnServerResponse(Either<ServerError, ServerEvent> response)
    {
        return Seq<string>([]);
    }

    public static Seq<string> HelpText()
    {
        return Seq(["usage goes here"]);
    }

    private record State(

    );
}

public record User(string Id);

public class ClientFst(ClientState initialState)
{
    private ClientState currentState = initialState;

    public Seq<ClientEffect> OnInput(ClientInput i)
    {
        (var nextState, var effects) = Transition(currentState, i);
        currentState = nextState;
        return effects;
    }

    private static (ClientState nextS, Seq<ClientEffect> es) Transition(ClientState s, ClientInput i)
    {
        return i switch
        {
            ClientInput.CommandSubmitted cs => HandleCommandSubmitted(s, cs),
            ClientInput.EventOccurred eo => HandleEventOccurred(s, eo),
            _ => (s, Seq<ClientEffect>()),
        };
    }

    private static (ClientState nextS, Seq<ClientEffect> es) HandleCommandSubmitted(ClientState s, ClientInput.CommandSubmitted i)
    {
        return s switch
        {
            
        };
    }

    private static (ClientState nextS, Seq<ClientEffect> es) HandleEventOccurred(ClientState s, ClientInput.EventOccurred i)
    {
        return s switch
        {
            
        };
    }
}

public abstract record ClientInput
{
    private ClientInput() {}

    public sealed record CommandSubmitted(Command Command) : ClientInput;
    public sealed record EventOccurred(ClientEvent Event) : ClientInput;
}

public abstract record ClientState
{
    private ClientState() {}

    public sealed record NotInRoom(User User) : ClientState;
    public sealed record InRoom(User User, string RoomId) : ClientState;
}

public abstract record ClientEffect
{
    private ClientEffect() {}

    public sealed record ToUser(Seq<string> Output) : ClientEffect;
    public sealed record ToServer(Command Command) : ClientEffect;
}

public abstract record Command
{
    private Command() {}

    public sealed record CreateRoom(string PlayerId) : Command;
    public sealed record JoinRoom(string RoomId, string PlayerId) : Command;
}

public abstract record ClientEvent
{
    private ClientEvent() {}
    public sealed record RoomCreated(string RoomId) : ClientEvent;
    public sealed record RoomJoined(string RoomId) : ClientEvent;
}

public abstract record ServerEvent
{
    private ServerEvent() {}
    public sealed record RoomCreated(string RoomId, string PlayerId) : ServerEvent;
    public sealed record RoomJoined(string RoomId, string PlayerId) : ServerEvent;
}

public abstract record ServerError
{
    private ServerError() {}
    public sealed record UnknownCommand(string Command) : ServerError;
    public sealed record RoomDoesNotExist(string RoomId) : ServerError;
}

public record ServerState(Map<string, Room> RoomsById)
{
    public static Either<ServerError, ServerEvent> OnCommand(ServerState s, Command c, Func<string> idGenerator)
    {
        return c switch
        {
            Command.CreateRoom cr => OnCreateRoom(s, cr, idGenerator),
            Command.JoinRoom jr => OnJoinRoom(s, jr),
            _ => Left<ServerError, ServerEvent>(new ServerError.UnknownCommand(c.GetType().Name)),
        };
    }

    private static Either<ServerError, ServerEvent> OnCreateRoom(ServerState s, Command.CreateRoom c, Func<string> idGenerator)
    {
        return Right<ServerError, ServerEvent>(new ServerEvent.RoomCreated(c.PlayerId, idGenerator()));
    }

    private static Either<ServerError, ServerEvent> OnJoinRoom(ServerState s, Command.JoinRoom c)
    {
        return s.RoomsById.Find(c.RoomId).Match(
            r => Right<ServerError, ServerEvent>(new ServerEvent.RoomJoined(c.RoomId, c.PlayerId)),
            () => Left<ServerError, ServerEvent>(new ServerError.RoomDoesNotExist(c.RoomId))
        );
    }

    public static ServerState Apply(ServerState s, ServerEvent e)
    {
        return e switch
        {
            ServerEvent.RoomCreated rc => ApplyRoomCreated(s, rc),
            ServerEvent.RoomJoined rj => ApplyRoomJoined(s, rj),
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
};

public record Room(string Id, Seq<string> UserIds)
{
    public Room WithUser(string userId)
    {
        return this with { UserIds = UserIds.Add(userId) };
    }
}

public class EmbeddedServer(ServerState initialState, Func<string> idGenerator)
{
    private ServerState currentState = initialState;

    public Either<ServerError, ServerEvent> OnCommand(Command c)
    {
        return ServerState.OnCommand(currentState, c, idGenerator)
            .Map(evt => {
                currentState = ServerState.Apply(currentState, evt);
                return evt;
            });
    }
}