using System;
using static ConsoleUI.UI;

namespace Snake
{
    public static class Drawer
    {
        public static void Draw(int x, int y, char ch, bool IsPlayer = false)
        {
            Console.SetCursorPosition(x, y);

            if (IsPlayer) Console.ForegroundColor = ConsoleColor.Green;

            Console.Write(ch);

            Console.ForegroundColor = StandartColor;
        }

        public static void Draw(Point point, char ch, bool IsPlayer = false)
        {
            Console.SetCursorPosition(point.x, point.y);

            if (IsPlayer) Console.ForegroundColor = ConsoleColor.Green;

            Console.Write(ch);

            Console.ForegroundColor = StandartColor;
        }

        public static void Clear(int x, int y)
        {
            Console.SetCursorPosition(x, y);

            Console.Write(" ");
        }

        public static void Clear(Point point)
        {
            Console.SetCursorPosition(point.x, point.y);

            Console.Write(" ");
        }
    }
}
