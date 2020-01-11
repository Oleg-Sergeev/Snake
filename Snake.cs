using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ConsoleUI.UI;

namespace Snake
{
    public class Snake
    {
        public Snake(int speed, char symbol, Point spawnPoint = default, Rotation rotation = Rotation.Up)
        {
            Points = new List<Point>() { (1, 1) };
            if (spawnPoint != default) Points[0] = spawnPoint;

            CanMove = true;

            List<Point> points = Points;

            Head = points[points.Count - 1];

            this.rotation = Rotation.Right;
            if (rotation != Rotation.Up) this.rotation = rotation;

            if (speed >= 1000 || speed <= 0) speed = 750;

            Speed = speed;

            SnakeSymbol = symbol;
        }

        public static event MoveEventHandler Moved;

        private Rotation rotation;
        private bool hasRotated;
        public static bool CanMove { get; set; }
        public List<Point> Points { get; private set; }
        public Point Head { get; private set; }
        public Point Tail { get; private set; }
        public char SnakeSymbol { get; private set; }
        public int Speed { get; private set; }

        public async Task Move()
        {
            if (!CanMove) return;

            ChangeRotation();

            while (CanMove)
            {
                (int x, int y) bias = rotation switch
                {
                    Rotation.Up => (0, -1),
                    Rotation.Right => (1, 0),
                    Rotation.Down => (0, 1),
                    Rotation.Left => (-1, 0)
                };

                hasRotated = false;

                char? symbol = GetSymbol(Head.x + bias.x, Head.y + bias.y);

                if (symbol != null && symbol != ' ' && symbol != Food.FoodSymbol)
                {
                    CanMove = false;
                    return;
                }
                
                Tail = Points[0];

                Points.Add(Head + bias);

                Drawer.Clear(Tail);

                Head = Points[^1];

                Points.RemoveAt(0);

                Drawer.Draw(Head, SnakeSymbol, true);

                Moved?.Invoke(this);

                await Task.Delay(1000 - Speed);
            }

            async void ChangeRotation()
            {
                while (CanMove)
                {
                    await Task.Delay(1);

                    if (!hasRotated)
                    {
                        Rotation currentRotation = Console.ReadKey(true).Key switch
                        {
                            ConsoleKey.LeftArrow => Rotation.Left,
                            ConsoleKey.UpArrow => Rotation.Up,
                            ConsoleKey.RightArrow => Rotation.Right,
                            ConsoleKey.DownArrow => Rotation.Down,
                            _ => rotation
                        };

                        hasRotated = true;

                        if (
                            rotation == Rotation.Left && currentRotation != Rotation.Right
                            || rotation == Rotation.Right && currentRotation != Rotation.Left
                            || rotation == Rotation.Down && currentRotation != Rotation.Up
                            || rotation == Rotation.Up && currentRotation != Rotation.Down
                            )
                            rotation = currentRotation;
                    }
                }
            }
        }

        public void CreateTail() => Points.Add(Points[^1]);

        public delegate void MoveEventHandler(Snake snake);

        public enum Rotation
        {
            Up,
            Right,
            Down,
            Left,
        }
    }
}
