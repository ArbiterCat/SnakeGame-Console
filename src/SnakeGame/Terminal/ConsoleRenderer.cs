using System.Text;
using SnakeGame.Core;

namespace SnakeGame.Terminal;

internal sealed class ConsoleRenderer
{
    private const string Controls = "Стрелки / WASD · P пауза · Esc меню";
    private const int MinimumWidth = 40;
    private readonly StringBuilder _frame = new(680);
    private bool _sizeWarning;

    public void BeginRound()
    {
        _sizeWarning = false;
        Console.Clear();
    }

    public bool Draw(GameState game, string status)
    {
        if (Console.WindowWidth < MinimumWidth || Console.WindowHeight < game.Height + 4)
        {
            if (!_sizeWarning)
            {
                Console.Clear();
                Console.WriteLine($"Увеличь окно до {MinimumWidth}×{game.Height + 4}.");
                Console.WriteLine("Esc — меню.");
                _sizeWarning = true;
            }
            return false;
        }

        if (_sizeWarning)
        {
            BeginRound();
        }

        _frame.Clear();
        _frame.Append('#', game.Width + 2).AppendLine();
        for (var y = 0; y < game.Height; y++)
        {
            _frame.Append('#');
            for (var x = 0; x < game.Width; x++)
            {
                var cell = new Cell(x, y);
                _frame.Append(SymbolAt(game, cell));
            }
            _frame.AppendLine("#");
        }
        _frame.Append('#', game.Width + 2).AppendLine();
        _frame.Append("Счёт: ").Append(game.Score).AppendLine("    ");
        _frame.Append((status.Length == 0 ? Controls : status).PadRight(MinimumWidth - 1));

        Console.SetCursorPosition(0, 0);
        Console.Write(_frame.ToString());
        return true;
    }

    private static char SymbolAt(GameState game, Cell cell)
    {
        if (cell == game.Head)
        {
            return '@';
        }
        if (game.HasSnakeAt(cell))
        {
            return 'o';
        }
        return game.HasFoodAt(cell) ? '*' : '.';
    }
}
