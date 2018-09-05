using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    public class CharMapPos
    {
        public TagPoint Position { get; set; }
        public short Foothold { get; set; }
        public byte Stance { get; set; }

        public CharMapPos()
        {
            Position = new TagPoint();
        }

        public CharMapPos(TagPoint position)
        {
            Position = position;
        }
    }
}
