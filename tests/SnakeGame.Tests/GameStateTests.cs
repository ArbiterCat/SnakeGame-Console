using SnakeGame.Core;

namespace SnakeGame.Tests;

public sealed class GameStateTests
{
    [Fact]
    public void StartsWithFiveFoodsInDistinctUnoccupiedCells()
    {
        var game = new GameState(20, 20, new Random(17));
        var cells = Cells(game).ToArray();

        Assert.Equal(5, cells.Count(game.HasFoodAt));
        Assert.Equal(3, cells.Count(game.HasSnakeAt));
        Assert.DoesNotContain(cells, cell => game.HasFoodAt(cell) && game.HasSnakeAt(cell));
        Assert.All(cells.Where(game.HasFoodAt), cell => Assert.InRange(cell.DistanceTo(game.Head), 4, 5));
    }

    [Fact]
    public void RejectsReversingAndQueuesOnlyOneTurnPerTick()
    {
        var game = new GameState(20, 20, new Random(17));

        Assert.False(game.TryTurn(Direction.Left));
        Assert.True(game.TryTurn(Direction.Up));
        Assert.False(game.TryTurn(Direction.Left));

        game.Step();
        Assert.Equal(new Cell(10, 9), game.Head);

        Assert.True(game.TryTurn(Direction.Left));
        game.Step();
        Assert.Equal(new Cell(9, 9), game.Head);
    }

    [Fact]
    public void WrapsAcrossTheTopEdge()
    {
        var game = new GameState(20, 20, new Random(17));
        Assert.True(game.TryTurn(Direction.Up));
        for (var i = 0; i <= 10; i++)
        {
            game.Step();
        }

        Assert.Equal(new Cell(10, 19), game.Head);
        Assert.Equal(RoundStatus.Playing, game.Status);
    }

    [Fact]
    public void EatingTheLastFreeCellWins()
    {
        var game = new GameState(4, 1, new Random(17));
        Assert.True(game.HasFoodAt(new Cell(3, 0)));

        game.Step();

        Assert.Equal(1, game.Score);
        Assert.Equal(RoundStatus.Won, game.Status);
        Assert.All(Cells(game), cell => Assert.True(game.HasSnakeAt(cell)));
        Assert.DoesNotContain(Cells(game), game.HasFoodAt);

        game.Step();
        Assert.Equal(new Cell(3, 0), game.Head);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CanEnterTailCellOnlyWhenItMovesAway(bool grow)
    {
        var snake = new SnakeBody(new Cell(2, 2));
        Assert.True(snake.TryMove(new Cell(3, 2), grow: true));
        Assert.True(snake.TryMove(new Cell(3, 3), grow: false));
        Assert.True(snake.TryMove(new Cell(2, 3), grow: false));

        Assert.False(snake.TryMove(new Cell(3, 2), grow: false));
        Assert.Equal(new Cell(2, 3), snake.Head);
        Assert.Equal(!grow, snake.TryMove(new Cell(2, 2), grow));
        Assert.Equal(grow ? new Cell(2, 3) : new Cell(2, 2), snake.Head);
        Assert.Equal(4, snake.Length);
    }

    private static IEnumerable<Cell> Cells(GameState game)
    {
        for (var x = 0; x < game.Width; x++)
        {
            for (var y = 0; y < game.Height; y++)
            {
                yield return new Cell(x, y);
            }
        }
    }
}
