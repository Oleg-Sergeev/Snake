namespace Snake
{
    public struct GameData
    {
        public GameData(Point wallsSize, char[] chars, int snakeSpeed)
        {
            this.wallsSize = wallsSize;
            this.chars = chars;
            this.snakeSpeed = snakeSpeed;
        }

        public Point wallsSize;
        public char[] chars;
        public int snakeSpeed;

        public override string ToString()
        {
            string symbols = "";

            foreach (char ch in chars) symbols += $"{ch} ";

            symbols = symbols.Trim();

            return $"{wallsSize} {symbols} {snakeSpeed}";
        }
    }
}
