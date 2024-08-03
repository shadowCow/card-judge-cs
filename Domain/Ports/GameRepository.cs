using Domain.Models;

namespace Domain.Ports;

public interface IGameRepository
{
    IQueryable<GameDefinition> GetAll();
    GameDefinition? GetById(string id);
}

public class InMemoryGameRepository(GameDefinition[] games) : IGameRepository
{
    public IQueryable<GameDefinition> GetAll()
    {
        return games.AsQueryable();
    }

    public GameDefinition? GetById(string id)
    {
        return games.FirstOrDefault(x => x.Id == id);
    }
}