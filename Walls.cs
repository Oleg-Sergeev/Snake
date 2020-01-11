using System.Collections.Generic;

namespace Snake
{
    public static class Walls
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
                for (int x = 1; x < size.x - 1; x++)
                {
                    PlayingField.Add((x, y));
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
}
