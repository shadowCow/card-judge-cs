using Domain.Models;

namespace ServiceTests.Adapters;

class TestGames
{
    public const string ticTacToeId = "tic-tac-toe";
    public static readonly GameDefinition ticTacToe = new(ticTacToeId, 2);

    public static readonly GameDefinition[] All = [ticTacToe];
}