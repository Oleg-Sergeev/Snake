using System;
using System.IO;
using System.Linq;

namespace Snake
{
    public static class SaveSystem
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
}
