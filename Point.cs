namespace Snake
{
    public readonly struct Point
    {
        public Point((int x, int y) coords)
        {
            x = coords.x;
            y = coords.y;
        }
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public readonly int x;
        public readonly int y;

        public static implicit operator Point((int x, int y) coords) => new Point(coords);
        public static implicit operator Point((string x, string y) coords) =>
            int.TryParse(coords.x, out int x) && int.TryParse(coords.y, out int y) ? new Point(x, y) : default;
        public static implicit operator Point(int[] coords) => coords.Length < 2 ? default : new Point(coords[0], coords[1]);

        public static Point operator +(Point p1, Point p2) => new Point(p1.x + p2.x, p1.y + p2.y);
        public static Point operator ^(Point p, int key) => new Point(p.x ^ key, p.y ^ key);
        public static bool operator ==(Point p1, Point p2) => p1.x == p2.x && p1.y == p2.y;
        public static bool operator !=(Point p1, Point p2) => p1.x != p2.x || p1.y != p2.y;

        public override bool Equals(object obj) => base.Equals(obj);

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString() => $"{x} {y}";

        public static bool TryParse(string s, out Point result)
        {
            string[] coords = s.Split();

            result = default;

            if (coords.Length < 2) return false;

            result = (coords[0], coords[1]);

            return true;
        }
    }
}
