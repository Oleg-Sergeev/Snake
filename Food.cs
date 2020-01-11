using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snake
{
    public class Food
    {
        public Food(char foodSymbol)
        {
            FoodSymbol = foodSymbol;
        }

        public static event FoodGeneratedEventHandler FoodGenerated;

        public Point FoodCoord { get; private set; }
        public static char FoodSymbol { get; private set; }

        public void GenerateFood(List<Point> snakeBody)
        {
            Point[] array = Walls.PlayingField.Except(snakeBody).ToArray();

            FoodCoord = array[new Random().Next(0, array.Length)];

            Drawer.Draw(FoodCoord, FoodSymbol, true);

            Snake.Moved += CheckCollision;

            FoodGenerated?.Invoke(FoodCoord);
        }

        private async void CheckCollision(Snake snake)
        {
            if (FoodCoord != snake.Head) return;

            FoodCoord = (0, 0);

            snake.CreateTail();

            Snake.Moved -= CheckCollision;

            await Task.Delay(2000);

            GenerateFood(snake.Points);
        }

        public delegate void FoodGeneratedEventHandler(Point point);
    }
}
