namespace SnakeGame.Core;

internal sealed class SnakeBody
{
    private readonly LinkedList<Cell> _segments;
    private readonly HashSet<Cell> _occupied;

    public Cell Head => _segments.First!.Value;
    public int Length => _segments.Count;

    public SnakeBody(Cell head)
    {
        Cell[] segments = [head, new(head.X - 1, head.Y), new(head.X - 2, head.Y)];
        _segments = new LinkedList<Cell>(segments);
        _occupied = new HashSet<Cell>(segments);
    }

    public bool Occupies(Cell cell) => _occupied.Contains(cell);

    public bool TryMove(Cell next, bool grow)
    {
        var tail = _segments.Last!;
        // The tail's cell is available when that segment moves away this tick.
        if (_occupied.Contains(next) && (grow || next != tail.Value))
        {
            return false;
        }

        if (!grow)
        {
            _occupied.Remove(tail.Value);
            _segments.RemoveLast();
            tail.Value = next;
            _segments.AddFirst(tail);
        }
        else
        {
            _segments.AddFirst(next);
        }

        _occupied.Add(next);
        return true;
    }
}
