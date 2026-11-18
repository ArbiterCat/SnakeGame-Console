namespace SnakeGame.Core;

internal enum RoundStatus
{
    Playing,
    Lost,
    Won
}

internal sealed class GameState
{
    private const int InitialFoodCount = 5;
    private readonly SnakeBody _snake;
    private readonly HashSet<Cell> _food = [];
    private readonly Random _random;
    private Direction _direction = Direction.Right;
    private Direction? _queuedTurn;

    public int Width { get; }
    public int Height { get; }
    public int Score { get; private set; }
    public RoundStatus Status { get; private set; } = RoundStatus.Playing;
    public Cell Head => _snake.Head;

    public GameState(int width, int height, Random? random = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 3);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);
        Width = width;
        Height = height;
        _random = random ?? Random.Shared;
        _snake = new SnakeBody(new Cell(Math.Max(2, width / 2), height / 2));
        if (_snake.Length == Width * Height)
        {
            Status = RoundStatus.Won;
        }

        for (var index = 0; index < InitialFoodCount; index++)
        {
            if (!SpawnFood())
            {
                break;
            }
        }
    }

    public bool HasSnakeAt(Cell cell) => _snake.Occupies(cell);
    public bool HasFoodAt(Cell cell) => _food.Contains(cell);

    public bool TryTurn(Direction direction)
    {
        if (Status != RoundStatus.Playing || _queuedTurn.HasValue ||
            direction == _direction || _direction.IsOpposite(direction))
        {
            return false;
        }

        _queuedTurn = direction;
        return true;
    }

    public void Step()
    {
        if (Status != RoundStatus.Playing)
        {
            return;
        }

        _direction = _queuedTurn ?? _direction;
        _queuedTurn = null;
        var next = new Cell(
            (Head.X + _direction.X + Width) % Width,
            (Head.Y + _direction.Y + Height) % Height);
        var grows = _food.Contains(next);
        if (!_snake.TryMove(next, grows))
        {
            Status = RoundStatus.Lost;
            return;
        }

        if (grows)
        {
            _food.Remove(next);
            Score++;
            if (_snake.Length == Width * Height)
            {
                Status = RoundStatus.Won;
            }
            else
            {
                SpawnFood();
            }
        }
    }

    private bool SpawnFood()
    {
        List<Cell> nearby = [];
        Cell? farthest = null;
        var farthestDistance = -1;

        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                var cell = new Cell(x, y);
                if (_snake.Occupies(cell) || _food.Contains(cell))
                {
                    continue;
                }

                var distance = cell.DistanceTo(Head);
                if (distance is 4 or 5)
                {
                    nearby.Add(cell);
                }
                if (distance > farthestDistance)
                {
                    farthest = cell;
                    farthestDistance = distance;
                }
            }
        }

        var position = nearby.Count > 0 ? nearby[_random.Next(nearby.Count)] : farthest;
        return position is Cell freeCell && _food.Add(freeCell);
    }
}
