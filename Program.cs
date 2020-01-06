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

        public static void CreateWalls(Point size, char ch)
        {
            Walls_ = new List<Point>();
            PlayingField = new List<Point>();

            HorizontalWalls(size.x, 0, ch);
            VerticalWalls(0, size.y, ch);
            VerticalWalls(size.x - 1, size.y, ch);
            HorizontalWalls(size.x, size.y - 1, ch);

            EndWall = Walls_[^1];

            for (int y = 1; y < size.y - 1; y++)
            {
                for (int x = 1; x < size.x - 1; x++) PlayingField.Add((x, y));
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
        /// <summary>
        /// Returns the bool (true/false) from string entered by the user
        /// </summary>
        /// <param name="message">This message will be displayed</param>
        public static bool Answer(string message)
        {
            string input = Read(message);

            if (bool.TryParse(input, out bool answer)) return answer;

            input = input.ToLower();

            return input == "yes" || input == "y";
        }

        /// <summary>
        /// Returns the string entered by the user
        /// </summary>
        /// <param name="message">This message will be displayed</param>
        public static string Read(string message = default)
        {
            if (!string.IsNullOrEmpty(message)) WriteLine(message);
            return ReadLine();
        }
    }

    static class SaveSystem
    {
        public static void Save(string filePath, GameData gameData)
        {
            if (!File.Exists(filePath))
            {
                string directoryPath = filePath.Substring(0, filePath.LastIndexOf('\\'));
                Directory.CreateDirectory(directoryPath);
                File.Create(filePath).Close();
            }

            using StreamWriter writer = new StreamWriter(filePath);

            int key = new Random().Next(1024);

            writer.Write($"{EnDecrypt(gameData, key)} {EncryptKey(key, gameData.snakeSpeed)}");
        }

        public static GameData Load(string filePath)
        {
            using StreamReader reader = new StreamReader(filePath);

            string[] data = reader.ReadToEnd().Split();

            if (data.Length < 7) return default;

            GameData gameData = new GameData((data[0], data[1]), data[2..5].Select(x => char.Parse(x)).ToArray(), int.Parse(data[5]));

            return EnDecrypt(gameData, DecryptKey(int.Parse(data[^1]), gameData.snakeSpeed));
        }

        private static GameData EnDecrypt(GameData data, int key)
        {
            data.wallsSize ^= key;

            for (int i = 0; i < data.chars.Length; i++) data.chars[i] ^= (char)key;

            data.snakeSpeed ^= key;

            return data;
        }

        private static int EncryptKey(int key, int salt) => (int)Math.Pow(key, 2) * (salt ^ key);
        private static int DecryptKey(int key, int salt) => (int)Math.Sqrt(key / salt);
}

    class Program
    {
        private static async Task Main(string[] args)
        {
            string directoryPath = "SnakeSettings";
            string filePath = $@"{directoryPath}\Settings.txt";

            int speed = 0;
            Point coords = default;
            char[] chars = null;

            SetWindowSize(LargestWindowWidth, LargestWindowHeight);
            SetWindowPosition(0, 0);

            bool load = false;

            if (Directory.Exists(directoryPath) && File.Exists(filePath))
            {
                load = UI.Answer("Would you like load game settings?");

                if (load)
                {
                    GameData data = SaveSystem.Load(filePath);

                    if (data.Equals(default(GameData)))
                    {
                        WriteLine("Can not load settings - empty or damaged file");
                        load = false;
                    }
                    else
                    {
                        coords = data.wallsSize;
                        chars = data.chars;
                        speed = data.snakeSpeed;
                    }
                }
            }

            if (!load)
            {
                bool create = UI.Answer("Would you like create new game settings?");

                do
                {
                    int c = 0;
                    coords = UI.Read("Enter walls' length (X and Y)").Split().Where(x => int.TryParse(x, out c)).Select(x => c).ToArray();

                    if (coords == default) WriteLine("Invalid input: requires 2 numbers - X and Y lengths");
                }
                while (coords == default);

                do
                {
                    char ch = default;
                    chars = UI.Read("Enter walls', snake's and food's characters").Split().Where(x => char.TryParse(x, out ch)).Select(x => ch).ToArray();

                    if (chars.Length < 3) WriteLine("Invalid input: requires 3 symbols - walls', snake's and food's characters");
                }
                while (chars.Length < 3);

                speed = int.Parse(UI.Read("Enter snake's speed (1 - the slowest, 999 - the fastest, -1 - default)"));

                if (create) SaveSystem.Save(filePath, new GameData(coords, (char[])chars.Clone(), speed));
            }

            Clear();

            Walls.CreateWalls(coords, chars[0]);

            Snake snake = new Snake(speed, chars[1]);

            Food food = new Food(chars[2], snake.Points);

            CursorVisible = false;

            await Task.Run(snake.Move);

            SetCursorPosition(0, Walls.EndWall.y + 1);

            WriteLine($"Game over\nTotal snake's length - {snake.Points.Count}");

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

    struct GameData
    {
        public GameData(Point wallsSize, char[] chars, int snakeSpeed)
        {
            this.wallsSize = wallsSize;
            this.chars = chars;
            this.snakeSpeed = snakeSpeed;
        }

        public Point wallsSize;
        public char[] chars;
        public int snakeSpeed;

        public override string ToString()
        {
            string symbols = "";

            foreach (var ch in chars) symbols += $"{ch} ";

            symbols = symbols.Trim();

            return $"{wallsSize} {symbols} {snakeSpeed}";
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
        public static implicit operator Point((string x, string y) coords)
        {
            if (!int.TryParse(coords.x, out int x) || !int.TryParse(coords.y, out int y)) return default;

            return new Point(x, y);
        }
        public static implicit operator Point(int[] coords)
        {
            if (coords.Length < 2) return default;

            return new Point(coords[0], coords[1]);
        }

        public static Point operator +(Point p1, Point p2) => new Point(p1.x + p2.x, p1.y + p2.y);
        public static Point operator ^(Point p, int key) => new Point(p.x ^ key, p.y ^ key);

        public static bool operator ==(Point p1, Point p2) => p1.x == p2.x && p1.y == p2.y;
        public static bool operator !=(Point p1, Point p2) => p1.x != p2.x || p1.y != p2.y;

        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();

        public override string ToString() => $"{x} {y}";
    }
}