namespace Common
{
    public sealed class TagPoint
    {
        public short X { get; set; }
        public short Y { get; set; }
        
        public TagPoint(short x, short y) { X = x; Y = y; }
        public TagPoint() { }

        public override string ToString()
        {
            return $"{X},{Y}";
        }
    }
}
