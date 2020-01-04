using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Console;

namespace SnakeGame
{
    static class Walls
    {
        public static List<Point> Walls_ { get; private set; }
        public static List<Point> PlayingField { get; private set; }
        public static Point EndWall { get; private set; }

        public static void CreateWalls(int x, int y, char ch)
        {
            Walls_ = new List<Point>();
            PlayingField = new List<Point>();

            HorizontalWalls(x, 0, ch);
            VerticalWalls(0, y, ch);
            VerticalWalls(x - 1, y, ch);
            HorizontalWalls(x, y - 1, ch);

            EndWall = Walls_[^1];

            for (int y_ = 1; y_ < y - 1; y_++)
            {
                for (int x_ = 1; x_ < x - 1; x_++) PlayingField.Add((x_, y_));
            }

            Snake.Moved += CheckCollision;

            static void HorizontalWalls(int x, int y, char ch)
            {
                for (int i = 0; i < x; i++)
                {
                    Drawer.Draw(i, y, ch);

                    Walls_.Add((i, y));
                }
            }
            static void VerticalWalls(int x, int y, char ch)
            {
                for (int i = 0; i < y; i++)
                {
                    Drawer.Draw(x, i, ch);

                    Walls_.Add((x, i));
                }
            }
        }
        private static void CheckCollision(Snake snake)
        {
            if (Walls_.Contains(snake.Head)) Snake.CanMove = false;
        }
    }

    static class Drawer
    {
        public static void Draw(int x, int y, char ch)
        {
            SetCursorPosition(x, y);

            Write(ch);
        }
        public static void Draw(Point point, char ch)
        {
            SetCursorPosition(point.x, point.y);

            Write(ch);
        }

        public static void Clear(int x, int y)
        {
            SetCursorPosition(x, y);

            Write(" ");
        }
        public static void Clear(Point point)
        {
            SetCursorPosition(point.x, point.y);

            Write(" ");
        }
    }

    static class UI
    {
        public static bool Answer(string message)
        {
            string input = Read(message);

            if (bool.TryParse(input, out bool answer)) return answer;

            input = input.ToLower();

            return input == "yes" || input == "y";
        }

        public static string Read(string message = default)
        {
            if (!string.IsNullOrEmpty(message)) WriteLine(message);
            return ReadLine();
        }
    }

    class Program
    {
        private static async Task Main(string[] args)
        {
            string directoryPath = @"C:\SnakeSettings";
            string filePath = $@"{directoryPath}\Settings.ini";

            int speed = 0;
            int[] coords = null;
            char[] wallChars = null;

            SetWindowSize(LargestWindowWidth, LargestWindowHeight);
            SetWindowPosition(0, 0);

            bool load = false;

            if (Directory.Exists(directoryPath) && File.Exists(filePath))
            {
                load = UI.Answer("Would you like load game settings?");

                if (load)
                {
                    using StreamReader reader = new StreamReader(filePath);
                    string[] output = reader.ReadToEnd().Split(new char[] { ' '}, StringSplitOptions.RemoveEmptyEntries);

                    if (output == null || output.Length < 5)
                    {
                        WriteLine("Can not load settings - empty file");
                        load = false;
                    }
                    else
                    {
                        coords = new int[] { int.Parse(output[0]), int.Parse(output[1]) };
                        wallChars = new char[] { char.Parse(output[2]), char.Parse(output[3]), char.Parse(output[4]) };
                        speed = int.Parse(output[5]);
                    }
                }
            }

            if (!load)
            {
                bool create = UI.Answer("Would you like create new game settings?");

                WriteLine("Enter walls' length (X and Y)");

                do
                {
                    int c = 0;
                    coords = ReadLine().Split().Where(x => int.TryParse(x, out c)).Select(x => c).ToArray();

                    if (coords.Length < 2) WriteLine("Invalid input: requires 2 numbers - X and Y lengths");
                }
                while (coords.Length < 2);

                WriteLine("Enter walls', snake's and food's characters");

                do
                {
                    char ch = default;
                    wallChars = ReadLine().Split().Where(x => char.TryParse(x, out ch)).Select(x => ch).ToArray();

                    if (wallChars.Length < 3) WriteLine("Invalid input: requires 3 symbols - walls', snake's and food's characters");
                }
                while (wallChars.Length < 3);

                WriteLine("Enter snake's speed (1 - the slowest, 999 - the fastest, -1 - default)");

                speed = int.Parse(ReadLine());

                if (create)
                {
                    Directory.CreateDirectory(directoryPath);

                    File.Create(filePath).Close();

                    using StreamWriter writer = new StreamWriter(filePath);

                    string coords_ = default;
                    string wallChars_ = default;
                    foreach (var c in coords) coords_ += $"{c.ToString()} ";
                    foreach (var wc in wallChars) wallChars_ += $"{wc.ToString()} ";

                    writer.WriteLine($"{coords_} {wallChars_} {speed}");
                }
            }

            Clear();

            Walls.CreateWalls(coords[0], coords[1], wallChars[0]);

            Snake snake = new Snake(speed, wallChars[1]);

            Food food = new Food(wallChars[2], snake.Points);

            CursorVisible = false;

            await Task.Run(snake.Move);

            SetCursorPosition(0, Walls.EndWall.y + 1);

            WriteLine("Game over");

            ReadKey();
        }
    }

    class Snake
    {
        public Snake(int speed, char symbol)
        {
            Points = new List<Point> { (1, 1) };

            CanMove = true;

            Head = Points[^1];

            rotation = Rotation.Right;

            if (speed >= 1000 || speed <= 0) speed = 750;

            this.speed = speed;

            snakeSymbol = symbol;

            Drawer.Draw(1, 1, snakeSymbol);
        }

        public delegate void MoveEventHandler(Snake snake);

        public static event MoveEventHandler Moved;

        public static bool CanMove { get; set; }
        public List<Point> Points { get; private set; }
        public Point Head { get; private set; }
        private Rotation rotation;
        private int speed;
        private char snakeSymbol;
        private bool hasRotated;

        public async Task Move()
        {
            if (!CanMove) return;

            ChangeRotation();

            while (CanMove)
            {
                (int x, int y) bias = rotation switch
                {
                    Rotation.Up => (0, -1),
                    Rotation.Down => (0, 1),
                    Rotation.Left => (-1, 0),
                    Rotation.Right => (1, 0)
                };

                hasRotated = false;

                if (Points.Contains(Points[^1] + bias))
                {
                    CanMove = false;
                    return;
                }

                Points.Add(Points[^1] + bias);

                Drawer.Clear(Points[0]);

                Head = Points[^1];

                Points.RemoveAt(0);

                Drawer.Draw(Points[^1], snakeSymbol);

                Moved?.Invoke(this);

                await Task.Delay(1000 - speed);
            }

            async void ChangeRotation()
            {
                while (CanMove)
                {
                    await Task.Delay(1);

                    if (hasRotated) continue;

                    Rotation newRotation = ReadKey(true).Key switch
                    {
                        ConsoleKey.UpArrow => Rotation.Up,
                        ConsoleKey.DownArrow => Rotation.Down,
                        ConsoleKey.LeftArrow => Rotation.Left,
                        ConsoleKey.RightArrow => Rotation.Right,
                        _ => rotation
                    };

                    hasRotated = true;


                    if (
                        newRotation == Rotation.Left && rotation != Rotation.Right
                        || newRotation == Rotation.Right && rotation != Rotation.Left
                        || newRotation == Rotation.Down && rotation != Rotation.Up
                        || newRotation == Rotation.Up && rotation != Rotation.Down
                       )
                        rotation = newRotation;
                }
            }
        }

        public void CreateTail() => Points.Add(Points[^1]);

        public enum Rotation
        {
            Up,
            Right,
            Down,
            Left
        }
    }

    class Food
    {
        public Food(char foodSymbol, List<Point> snakeBody)
        {
            this.foodSymbol = foodSymbol;
            GenerateFood(snakeBody);
        }

        public Point FoodCoord { get; private set; }
        private char foodSymbol;

        public void GenerateFood(List<Point> snakeBody)
        {
            Point[] emptyPoints = Walls.PlayingField.Except(snakeBody).ToArray();

            FoodCoord = emptyPoints[new Random().Next(0, emptyPoints.Length)];

            Drawer.Draw(FoodCoord, foodSymbol);

            Snake.Moved += CheckCollision;
        }

        private async void CheckCollision(Snake snake)
        {
            if (FoodCoord == snake.Head)
            {
                FoodCoord = (0, 0);

                snake.CreateTail();

                Snake.Moved -= CheckCollision;

                await Task.Delay(2000);

                GenerateFood(snake.Points);
            }
        }
    }

    readonly struct Point
    {
        public Point((int x, int y) coords)
        {
            x = coords.x;
            y = coords.y;
        }
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public readonly int x;
        public readonly int y;

        public static implicit operator Point((int x, int y) coords) => new Point(coords);

        public static Point operator +(Point p1, Point p2) => new Point(p1.x + p2.x, p1.y + p2.y);

        public static bool operator ==(Point p1, Point p2) => p1.x == p2.x && p1.y == p2.y;
        public static bool operator !=(Point p1, Point p2) => p1.x != p2.x || p1.y != p2.y;

        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();

        public override string ToString() => $"{x} {y}";
    }
}