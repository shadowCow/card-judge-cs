// See https://aka.ms/new-console-template for more information
using ConsoleClient;

Console.WriteLine("Welcome to game client");

var idGenerator = () => Guid.NewGuid().ToString();

var embeddedServer = new EmbeddedServer(new ServerState([]), idGenerator);

var toServer = (Command c) => {
    var result = embeddedServer.OnCommand(c);
    var output = ConsoleClient.Client.OnServerResponse(result);
    output.Iter(Console.WriteLine);
};

var authenticator = () => Some(new User("billy"));

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
        .Bind(u => CommandParser.Parse(i, u))
        .Match(
            chattyToServer,
            ConsoleClient.Client.HelpText
        )
        .Iter(Console.WriteLine);
}
