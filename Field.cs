using System.Collections.Generic;

namespace Snake
{
    public static class Field
    {
        public static List<Point> Borders { get; private set; }
        public static List<Point> PlayingField { get; private set; }
        public static Point EndWall { get; private set; }
        public static char Symbol { get; private set; }

        public static void InitializeBorders(Point size, char ch)
        {
            Borders = new List<Point>();
            PlayingField = new List<Point>();

            Symbol = ch;

            CreateBorders(size.x, 0, ch, true);
            CreateBorders(0, size.y, ch, false);
            CreateBorders(size.x, size.y - 1, ch, true);
            CreateBorders(size.x, size.y, ch, false);

            EndWall = Borders[^1];

            for (int x = 1; x < size.x - 1; x++)
            {
                for (int y = 1; y < size.y - 1; y++)
                {
                    PlayingField.Add((x, y));
                }
            }
        }

        public static void DrawBorders()
        {
            if (Borders == null) return;

            foreach (var border in Borders) Drawer.Draw(border, Symbol);
        }

        private static void CreateBorders(int x, int y, char ch, bool isHorizontal)
        {
            if (isHorizontal)
            {
                for (int i = 0; i < x; i++) Borders.Add((i, y));
            }
            else
            {
                for (int i = 0; i < y; i++) Borders.Add((x, i));
            }
        }

        private static void CheckCollision(Snake snake)
        {
            if (Borders.Contains(snake.Head)) Snake.CanMove = false;
        }
    }
}
