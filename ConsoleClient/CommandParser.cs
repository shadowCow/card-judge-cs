namespace ConsoleClient;

public static class CommandParser
{
    public static Option<Command> Parse(Option<string> input, User user)
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
}