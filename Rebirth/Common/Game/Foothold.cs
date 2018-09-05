using System;

namespace Common.Game
{
    public class Foothold : IComparable
    {
        public int Id { get; set; }
        public int Prev { get; set; }
        public int Next { get; set; }
        public short X1 { get; set; }
        public short Y1 { get; set; }
        public short X2 { get; set; }
        public short Y2 { get; set; }

        public bool Wall => X1 == X2;

        public int CompareTo(object obj)
        {
            Foothold foothold = (Foothold)obj;

            if (Y2 < foothold.Y1)
            {
                return -1;
            }
            if (Y1 > foothold.Y2)
            {
                return 1;
            }

            return 0;
        }
    }
}