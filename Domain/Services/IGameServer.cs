using Domain.Fst;

namespace Domain.Services;

public interface IGameServer
{
    void Submit(ToServer msg);
}

public abstract record ToServer
{
    private ToServer() {}
    public sealed record Command(string RequestId, ServerCommand Cmd) : ToServer;
}

public abstract record FromServer
{
    private FromServer() {}
    public sealed record CommandSuccess(string RequestId, ServerEvent Event) : FromServer;
    public sealed record CommandFailure(string RequestId, ServerError Error) : FromServer;
    public sealed record Notification(ServerEvent Event) : FromServer;
}