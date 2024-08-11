namespace Domain.Services;

public interface IGameClient
{
    void Submit(ClientCommand c);

    // state queries
    string? GetRoomId();
}
