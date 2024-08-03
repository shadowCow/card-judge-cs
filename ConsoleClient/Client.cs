using Domain.Services;

namespace ConsoleClient;

public class Client
{
    public static void Run(
        Func<Option<User>> authenticator,
        Action<Command> toServer)
    {
        Seq<string> chattyToServer(Command c)
        {
            toServer(c);
            return Seq([$"Sent {c} to server."]);
        }
        
        while (true)
        {
            var i = Console.ReadLine();
            if (i is null)
            {
                return;
            }

            authenticator()
                .Bind(u => ParseCommand(i, u))
                .Match(
                    chattyToServer,
                    HelpText
                )
                .Iter(Console.WriteLine);
        }
    }

    private static Option<Command> ParseCommand(Option<string> input, User user)
    {
        return input
            .Bind(NiceSplit)
            .Bind(sc => Parse(sc, user));
    }

    private static readonly char[] whitespace = [' ', '\t', '\n'];
    private static Option<StringCommand> NiceSplit(string s)
    {
        var split = s.Split(whitespace, StringSplitOptions.RemoveEmptyEntries);
        if (split.Length == 0)
        {
            return None;
        }
        else
        {
            return new StringCommand(split[0], split.Skip(1).ToArray());
        }
    }

    private static Option<Command> Parse(StringCommand c, User user)
    {
        return c.Command.ToLower() switch 
        {
            "createroom" => ParseCreateRoom(c.Args, user),
            "joinroom" => ParseJoinRoom(c.Args, user),
            _ => None,
        };
    }

    private static Option<Command> ParseCreateRoom(string [] args, User user)
    {
        return Some(new Command.CreateRoom(user.Id))
            .Map(c => (Command)c);
    }

    private static Option<Command> ParseJoinRoom(string [] args, User user)
    {
        return args.HeadOrNone()
            .Map(roomId => new Command.JoinRoom(roomId, user.Id))
            .Map(c => (Command)c);
    }

    private record StringCommand(string Command, string[] Args);

    private static Seq<string> HelpText()
    {
        return Seq(["usage goes here"]);
    }

    private record State(

    );
}

public abstract record AuthenticationError
{
    private AuthenticationError() {}

    public sealed record AuthenticationFailed() : AuthenticationError;
}

public record User(string Id);

public class ClientFst
{

}

public abstract record ClientInput()
{

}

public abstract record Command
{
    private Command() {}

    public sealed record CreateRoom(string PlayerId) : Command;
    public sealed record JoinRoom(string RoomId, string PlayerId) : Command;
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
};

public record Room(string Id);
