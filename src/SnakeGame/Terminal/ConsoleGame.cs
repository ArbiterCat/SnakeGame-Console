using System.Text;
using SnakeGame.Core;

namespace SnakeGame.Terminal;

internal sealed class ConsoleGame
{
    private const int BoardWidth = 20;
    private const int BoardHeight = 20;
    private const int TickMilliseconds = 100;
    private readonly ConsoleRenderer _renderer = new();

    public int Run()
    {
        if (Console.IsInputRedirected || Console.IsOutputRedirected)
        {
            Console.Error.WriteLine("SnakeGame needs an interactive terminal.");
            return 1;
        }

        Console.OutputEncoding = Encoding.UTF8;
        try
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("ЗМЕЙКА");
                Console.WriteLine();
                Console.WriteLine("Enter / S  Начать игру");
                Console.WriteLine("Q / Esc    Выход");
                Console.WriteLine();
                Console.WriteLine("Стрелки / WASD — движение, P — пауза");

                var key = ReadKey();
                if (key is ConsoleKey.Q or ConsoleKey.Escape)
                {
                    return 0;
                }
                if (key is ConsoleKey.Enter or ConsoleKey.S)
                {
                    PlayRound();
                }
            }
        }
        finally
        {
            Console.CursorVisible = true;
        }
    }

    private void PlayRound()
    {
        var state = new GameState(BoardWidth, BoardHeight);
        Console.CursorVisible = false;
        try
        {
            _renderer.BeginRound();
            if (!Countdown(state))
            {
                return;
            }

            var paused = false;
            while (state.Status == RoundStatus.Playing)
            {
                var visible = _renderer.Draw(state, paused ? "Пауза. P — продолжить" : "");
                Thread.Sleep(TickMilliseconds);
                if (ReadControls(state, ref paused))
                {
                    return;
                }
                if (visible && !paused)
                {
                    state.Step();
                }
            }

            var result = state.Status == RoundStatus.Won ? "Победа!" : "Игра окончена.";
            if (!_renderer.Draw(state, $"{result} Любая клавиша — меню"))
            {
                Console.Clear();
                Console.WriteLine($"{result} Счёт: {state.Score}");
                Console.WriteLine("Нажми любую клавишу.");
            }
            Console.ReadKey(intercept: true);
        }
        finally
        {
            Console.CursorVisible = true;
        }
    }

    private bool Countdown(GameState state)
    {
        for (var seconds = 3; seconds > 0; seconds--)
        {
            for (var elapsed = 0; elapsed < 1000; elapsed += TickMilliseconds)
            {
                _renderer.Draw(state, $"Старт через {seconds}…");
                Thread.Sleep(TickMilliseconds);
                while (Console.KeyAvailable)
                {
                    if (ReadKey() == ConsoleKey.Escape)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private static bool ReadControls(GameState state, ref bool paused)
    {
        while (Console.KeyAvailable)
        {
            var key = ReadKey();
            if (key == ConsoleKey.Escape)
            {
                return true;
            }
            if (key == ConsoleKey.P)
            {
                paused = !paused;
                continue;
            }
            if (paused)
            {
                continue;
            }

            Direction? direction = key switch
            {
                ConsoleKey.UpArrow or ConsoleKey.W => Direction.Up,
                ConsoleKey.DownArrow or ConsoleKey.S => Direction.Down,
                ConsoleKey.LeftArrow or ConsoleKey.A => Direction.Left,
                ConsoleKey.RightArrow or ConsoleKey.D => Direction.Right,
                _ => null
            };
            if (direction is Direction turn)
            {
                state.TryTurn(turn);
            }
        }

        return false;
    }

    private static ConsoleKey ReadKey()
    {
        var key = Console.ReadKey(intercept: true);
        // A pseudoterminal can supply a letter without its virtual key code.
        var letter = char.ToUpperInvariant(key.KeyChar);
        return letter is >= 'A' and <= 'Z' ? (ConsoleKey)letter : key.Key;
    }
}
