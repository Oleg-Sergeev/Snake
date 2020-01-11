using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using static ConsoleUI.UI;
using static System.Console;

namespace Snake
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            SetWindowSize(LargestWindowWidth, LargestWindowHeight);
            SetWindowPosition(0, 0);

            StandartColor = ConsoleColor.White;

            Snake snake = new Snake(900, 'o', new Point(), Snake.Rotation.Up);
            Client client = null;
            Point coords = default;
            char[] chars = new char[3];
            int speed = 0;

            client = Client.InitializeClient();

            if (client != null)
            {
                speed = 850;
                coords = new Point(100, 12);
                chars[0] = '#';
                chars[1] = 'o';
                chars[2] = Input<char>("Enter food symbol: ", false);
                client.InitializeSnake(snake);
                await client.StartGame();
            }
            else
            {
                string path = "SnakeSettings";
                string str = path + "\\Settings.txt";

                bool load = false;

                if (Directory.Exists(path) && File.Exists(str))
                {
                    load = Input<bool>("Would you like load game settings?(True / False)");
                    if (load)
                    {
                        GameData gameData = SaveSystem.Load(str);
                        if (gameData.Equals(new GameData()))
                        {
                            WarningLog("Can not load settings - empty or damaged file");
                            load = false;
                        }
                        else
                        {
                            coords = gameData.wallsSize;
                            chars = gameData.chars;
                            speed = gameData.snakeSpeed;
                        }
                    }
                }
                if (!load)
                {
                    bool create = Input<bool>("Would you like create new game settings?(True / False)");

                    do
                    {
                        int c = 0;
                        coords = (Input("Enter walls' length (X and Y)").Split()).Where(x => int.TryParse(x, out c)).Select(x => c).ToArray();

                        if (coords == default)  WarningLog("Invalid input: requires 2 numbers - X and Y lengths");
                    }
                    while (coords == default);

                    do
                    {
                        char ch = char.MinValue;
                        chars = (Input("Enter walls', snake's and food's characters").Split()).Where(x => char.TryParse(x, out ch)).Select(x => ch).ToArray();

                        if (chars.Length < 3) WarningLog("Invalid input: requires 3 symbols - walls', snake's and food's characters:");
                    }
                    while (chars.Length < 3);

                    speed = Input<int>("Enter snake's speed (1 - the slowest, 999 - the fastest, -1 - default):");

                    if (create) SaveSystem.Save(str, new GameData(coords, (char[])chars.Clone(), speed));
                }
            }
            Food food = new Food(chars[2]);

            Clear();

            Walls.CreateWalls(coords, chars[0]);

            if (client == null)
            {
                snake = new Snake(speed, chars[1], new Point(), Snake.Rotation.Up);
                food.GenerateFood(snake.Points);
            }
            else food.GenerateFood(client.Snake.Points);

            CursorVisible = false;

            if (client != null) await Task.Run(client.Snake.Move);
            else await Task.Run(snake.Move);

            SetCursorPosition(0, Walls.EndWall.y + 1);

            if (client != null)
            {
                WarningLog("Game over");
                Log($"Total snake's length - {client.Snake.Points.Count}");

                client.Send(Client.DISCONNECT);
                client.ClientSocket.Shutdown(SocketShutdown.Both);
                client.ClientSocket.Disconnect(false);
                client.ClientSocket.Close();
            }
            else
            {
                WarningLog("Game over");
                Log($"Total snake's length - {snake.Points.Count}");
            }

            ReadKey(true);
        }
    }
}