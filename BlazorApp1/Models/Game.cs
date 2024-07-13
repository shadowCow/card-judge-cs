namespace BlazorApp1.Models;

public class GameSession
{
    private string _id;
    public GameSession(string id)
    {
        this._id = id;
    }

    public string GetId()
    {
        return this._id;
    }
}

public class GameRound
{
    private Card prompt;
    private Dictionary<string, Card> submissions = new Dictionary<string, Card>();
    public GameRound(Card prompt)
    {
        this.prompt = prompt;   
    }

    public Card GetPrompt()
    {
        return this.prompt;
    }

    public void SubmitCard(string playerId, Card card)
    {
        this.submissions.Add(playerId, card);
    }

    public int CountSubmissions()
    {
        return this.submissions.Count;
    }
}

public struct Card
{
    public readonly CardKind kind;
    public readonly string value;

    public Card(CardKind kind, string value)
    {
        this.kind = kind;
        this.value = value;
    }
}

public enum CardKind {
    Prompt,
    Answer,
}
