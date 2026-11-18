namespace SnakeGame.Core;

internal readonly record struct Direction(int X, int Y)
{
    public static Direction Up { get; } = new(0, -1);
    public static Direction Down { get; } = new(0, 1);
    public static Direction Left { get; } = new(-1, 0);
    public static Direction Right { get; } = new(1, 0);

    public bool IsOpposite(Direction other) => X == -other.X && Y == -other.Y;
}
