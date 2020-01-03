using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Console;

namespace SnakeGame
{
    class Snake
    {
        public delegate void MoveEventHandler(Snake snake);

        public static event MoveEventHandler Moved;
        public static bool CanMove { get; set; }
        public Point Head { get; private set; }
        public List<Point> Points { get; private set; }
        private Rotation rotation;
        private int speed;
        private char snakeSymbol;
        private bool hasRotated;

        public Snake(int speed, char symbol)
        {
            Points = new List<Point> { (1, 1) };

            CanMove = true;

            Head = Points[^1];

            rotation = Rotation.Right;

            this.speed = speed;

            snakeSymbol = symbol;

            Drawer.Draw(1, 1, snakeSymbol);
        }

        public async void Move()
        {
            if (!CanMove) return;

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
                Drawer.Clear(Points[0].x, Points[0].y);

                Head = Points[^1];

                Points.RemoveAt(0);
                Drawer.Draw(Points[^1].x, Points[^1].y, snakeSymbol);

                Moved?.Invoke(this);

                await Task.Delay(1000 - speed);
            }
        }

        public void ChangeRotation()
        {
            while (CanMove)
            {
                if (hasRotated) continue;

                hasRotated = true;

                Rotation newRotation = ReadKey(true).Key switch
                {
                    ConsoleKey.UpArrow => Rotation.Up,
                    ConsoleKey.DownArrow => Rotation.Down,
                    ConsoleKey.LeftArrow => Rotation.Left,
                    ConsoleKey.RightArrow => Rotation.Right,
                    _ => rotation
                };

                if (
                    newRotation == Rotation.Left && rotation != Rotation.Right
                    || newRotation == Rotation.Right && rotation != Rotation.Left
                    || newRotation == Rotation.Down && rotation != Rotation.Up
                    || newRotation == Rotation.Up && rotation != Rotation.Down
                   )
                    rotation = newRotation;
            }
        }

        public void CreateNewTail() => Points.Add(Points[^1]);

        public enum Rotation
        {
            Up,
            Right,
            Down,
            Left
        }
    }

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

            for (int i = 1; i < y - 1; i++)
            {
                for (int j = 1; j < x - 1; j++)
                {
                    PlayingField.Add((i, j));
                }
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

        public static void Clear(int x, int y)
        {
            SetCursorPosition(x, y);
            Write(" ");
        }
    }

    static class Food
    {
        public static Point FoodCoord { get; private set; }
        private static char foodSymbol;

        public static void SetFoodSymbol(char symbol) => foodSymbol = symbol;

        public static void CreateFood(List<Point> snakeTail)
        {
            Random random = new Random();

            var emptyPoints = Walls.PlayingField.Except(snakeTail).ToArray();

            FoodCoord = emptyPoints[random.Next(0, emptyPoints.Length)];

            Drawer.Draw(FoodCoord.x, FoodCoord.y, foodSymbol);

            Snake.Moved += CheckCollision;
        }

        private async static void CheckCollision(Snake snake)
        {
            if (FoodCoord == snake.Head)
            {
                FoodCoord = (0, 0);
                snake.CreateNewTail();
                await Task.Delay(2000);
                CreateFood(snake.Points);
            }
        }
    }

    struct Point
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

        public int x;
        public int y;

        public static implicit operator Point((int x, int y) coords) => new Point(coords);

        public static Point operator +(Point p1, Point p2) => new Point(p1.x + p2.x, p1.y + p2.y);

        public static bool operator ==(Point p1, Point p2) => p1.x == p2.x && p1.y == p2.y;
        public static bool operator !=(Point p1, Point p2) => p1.x != p2.x || p1.y != p2.y;

        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();

        public override string ToString() => $"{x} {y}";
    }

    class Game
    {
        public static bool EndGame = false;

        static void Main(string[] args)
        {
            SetWindowSize(LargestWindowWidth, LargestWindowHeight);
            SetWindowPosition(0, 0);

            WriteLine("Enter walls' length (X and Y)");

            int[] coords;

            do
            {
                coords = ReadLine().Split().Where(x => int.TryParse(x, out int x_)).Select(x => int.Parse(x)).ToArray();

                if (coords.Length < 2) WriteLine("Invalid input: requires 2 numbers - X and Y lengths");
            }
            while (coords.Length < 2);

            WriteLine("Enter walls', snake's and food's characters");

            char[] wallChars;

            do
            {
                wallChars = ReadLine().Split().Where(x => char.TryParse(x, out char x_)).Select(x => char.Parse(x)).ToArray();

                if (wallChars.Length < 3) WriteLine("Invalid input: requires 3 symbols - walls', snake's and food's characters");
            }
            while (wallChars.Length < 3);

            WriteLine("Enter snake's speed (0 - the slowest, 999 - the fastest, -1 - default)");

            int speed = int.Parse(ReadLine());

            Clear();

            Walls.CreateWalls(coords[0], coords[1], wallChars[0]);

            Snake snake = new Snake(speed >= 0 && speed < 1000 ? speed : 750, wallChars[1]);

            Food.SetFoodSymbol(wallChars[2]);
            Food.CreateFood(snake.Points);

            CursorVisible = false;

            Task.Run(snake.Move);

            Task.Run(snake.ChangeRotation);

            while (Snake.CanMove) { } // как бы это исправить

            SetCursorPosition(0, Walls.EndWall.y + 1);
            WriteLine("Game over");

            ReadKey();
        }
    }
}
