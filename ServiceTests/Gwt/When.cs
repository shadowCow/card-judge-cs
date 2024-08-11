using Domain.Services;

namespace ServiceTests.Gwt;

public class When
{
    public static void ClientCommand(IGameClient client, ClientCommand command)
    {
        client.Submit(command);
    }
}