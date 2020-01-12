using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ConsoleUI.UI;

namespace Snake
{
    public class Client
    {
        private const string READY = "READY";
        private const string CHECK_PLAYERS = "CHECK_PLAYERS";
        public const string DISCONNECT = "DISCONNECT";

        private Client() {}
        private Client(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            ClientSocket = new Socket(addressFamily, socketType, protocolType);
        }

        public Socket ClientSocket { get; private set; }
        public Snake Snake { get; private set; }

        public void InitializeSnake(Snake snake) => Snake = snake;

        public static Client InitializeClient()
        {
            try
            {
                Client client = new Client(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                client.ClientSocket.Connect(IPAddress.Parse(Input("Enter server's IP: ")), Input<int>("Enter server's port: "));

                if (client.ClientSocket.Connected)
                {
                    Snake.Moved += client.Send;
                    Food.FoodGenerated += client.Send;

                    new Thread(client.Receive).Start();

                    Log("Connected");

                    return client;
                }

                WarningLog("Not connected");
                return null;
            }
            catch (Exception ex)
            {
                ErrorLog(ex.Message);
                return null;
            }
        }

        public async Task StartGame()
        {
            ClientSocket.Send(Encoding.Unicode.GetBytes(READY));
            ClientSocket.Send(Encoding.Unicode.GetBytes(CHECK_PLAYERS));
            ClientSocket.Receive(new byte[16]);

            Log("Get ready");

            await Task.Delay(1000);

            for (int i = 3; i >= 1; --i)
            {
                Log(i);

                await Task.Delay(1000);
            }

            Log("Start!");
        }

        public void Send(Snake snake)
        {
            try
            {
                byte[] bytes = Encoding.Unicode.GetBytes($"{snake.Head}|{snake.Tail}|{snake.SnakeSymbol}");

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();

                args.SetBuffer(bytes);

                ClientSocket.SendAsync(args);
            }
            catch (Exception ex)
            {
                ErrorLog($"{ex.Message} ### {ex.StackTrace}");
            }
        }

        public void Send(Point point)
        {
            try
            {
                byte[] bytes = Encoding.Unicode.GetBytes($"{point}|{Food.FoodSymbol}");

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();

                args.SetBuffer(bytes);

                ClientSocket.SendAsync(args);
            }
            catch (Exception ex)
            {
                ErrorLog($"{ex.Message} ### {ex.StackTrace}");
            }
        }

        public void Send(string message)
        {
            try
            {
                byte[] bytes = Encoding.Unicode.GetBytes(message);

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();

                args.SetBuffer(bytes);

                ClientSocket.SendAsync(args);
            }
            catch (Exception ex)
            {
                ErrorLog($"{ex.Message} ### {ex.StackTrace}");
            }
        }

        public void Receive()
        {
            try
            {
                while (ClientSocket.Connected)
                {
                    byte[] numArray = new byte[1024];
                    string message = "";
                    do
                    {
                        int count = ClientSocket.Receive(numArray);
                        message += Encoding.Unicode.GetString(numArray, 0, count);
                    }
                    while (ClientSocket.Available > 0);

                    Task.Run(() => ProcessData(message));
                }
            }
            catch (SocketException ex) when (ex.ErrorCode == 10004)
            {
                return;
            }
            catch (Exception ex)
            {
                ErrorLog($"{ex.Message} ### {ex.StackTrace}");
            }
        }

        private void ProcessData(string message)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrWhiteSpace(message)) return;

            string[] receivedData = message.Split('|');
            if (receivedData.Length == 1)
            {
                Log(message);
            }

            if (receivedData.Length == 2)
            {
                if (!Point.TryParse(receivedData[0], out Point point) || !char.TryParse(receivedData[1], out char ch)) return;

                Console.ForegroundColor = ConsoleColor.Red;

                Drawer.Draw(point, ch);

                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                if (char.TryParse(receivedData[2], out char ch) && Point.TryParse(receivedData[0], out Point head) && Point.TryParse(receivedData[1], out Point tail))
                {
                    Drawer.Clear(tail);

                    Console.ForegroundColor = ConsoleColor.Red;

                    Drawer.Draw(head, ch);

                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    if (receivedData[0] != "Message") return;

                    Log(receivedData[1]);

                    if (receivedData[2] == "NEWSPAWN") Snake = new Snake(Snake.Speed, Snake.SnakeSymbol, Field.PlayingField[^1], Snake.Rotation.Left);
                }
            }
        }
    }
}
