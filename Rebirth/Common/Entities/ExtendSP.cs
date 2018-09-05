using System.Collections.Generic;
using Common.Packets;

namespace Common.Entities
{
    public class SPSet
    {
        public byte nJobLevel;
        public byte nSP;
    }

    public class ExtendSP : List<SPSet>
    {
        public void Encode(COutPacket p)
        {
            var count = (byte) Count;

            p.Encode1(count);

            for (int i = 0; i < count; i++)
            {
                var sp = this[i];
                p.Encode1(sp.nJobLevel);
                p.Encode1(sp.nSP);
            }
        }
    }
}
