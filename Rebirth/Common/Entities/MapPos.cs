namespace Common.Entities
{
    public class MapPos
    {
        public TagPoint Position { get; set; }
        public short Foothold { get; set; }
        public byte Stance { get; set; }

        public MapPos()
        {
            Position = new TagPoint();
        }

        public MapPos(TagPoint position)
        {
            Position = position;
        }
    }
}
