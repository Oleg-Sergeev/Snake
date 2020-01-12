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

            Snake snake = new Snake(900, 'o');
            Client client = null;
            Point borderSize = default;
            char[] chars = new char[3];
            int speed = 0;

            client = Client.InitializeClient();

            if (client != null)
            {
                speed = 850;
                borderSize = new Point(100, 12);
                chars[0] = '#';
                chars[1] = 'o';
                chars[2] = Input<char>("Enter food symbol: ", false);

                Field.InitializeBorders(borderSize, chars[2]);

                client.InitializeSnake(snake);
                await client.StartGame();
            }
            else
            {
                string directoryPath = "SnakeSettings";
                string filePath = $@"{directoryPath}\Settings.txt";

                bool load = false;

                if (Directory.Exists(directoryPath) && File.Exists(filePath))
                {
                    load = Input<bool>("Would you like load game settings?(True / False)");
                    if (load)
                    {
                        GameData gameData = SaveSystem.Load(filePath);

                        if (!gameData.Equals(default))
                        {
                            borderSize = gameData.wallsSize;
                            chars = gameData.chars;
                            speed = gameData.snakeSpeed;
                        }
                        else
                        {
                            WarningLog("Can not load settings - empty or damaged file");
                            load = false;
                        }
                    }
                }
                if (!load)
                {
                    bool create = Input<bool>("Would you like create new game settings?(True / False)");

                    do
                    {
                        int c = 0;
                        borderSize = (Input("Enter walls' length (X and Y)").Split()).Where(x => int.TryParse(x, out c)).Select(x => c).ToArray();

                        if (borderSize == default)  WarningLog("Invalid input: requires 2 numbers - X and Y lengths");
                    }
                    while (borderSize == default);

                    do
                    {
                        char ch = default;
                        chars = (Input("Enter walls', snake's and food's characters").Split()).Where(x => char.TryParse(x, out ch)).Select(x => ch).ToArray();

                        if (chars.Length < 3) WarningLog("Invalid input: requires 3 symbols - walls', snake's and food's characters:");
                    }
                    while (chars.Length < 3);

                    speed = Input<int>("Enter snake's speed (1 - the slowest, 999 - the fastest, -1 - default):");

                    if (create) SaveSystem.Save(filePath, new GameData(borderSize, (char[])chars.Clone(), speed));
                }
                Field.InitializeBorders(borderSize, chars[0]);
            }

            Clear();

            Food food = new Food(chars[2]);

            Field.DrawBorders();

            if (client == null)
            {
                snake = new Snake(speed, chars[1]);
                food.GenerateFood(snake.Points);
            }
            else food.GenerateFood(client.Snake.Points);

            CursorVisible = false;

            if (client != null) await Task.Run(client.Snake.Move);
            else await Task.Run(snake.Move);

            SetCursorPosition(0, Field.EndWall.y + 1);

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