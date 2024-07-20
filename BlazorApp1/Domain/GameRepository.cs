using BlazorApp1.Models;

namespace BlazorApp1.Domain;

public interface IGameRepository
{
    IQueryable<Game> GetAll();
    Game? GetById(string id);
}

public class InMemoryGameRepository(Game[] games) : IGameRepository
{
    public IQueryable<Game> GetAll()
    {
        return games.AsQueryable();
    }

    public Game? GetById(string id)
    {
        return games.First(x => x.Id == id);
    }
}