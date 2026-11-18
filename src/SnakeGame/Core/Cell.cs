namespace SnakeGame.Core;

internal readonly record struct Cell(int X, int Y)
{
    public int DistanceTo(Cell other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
}
