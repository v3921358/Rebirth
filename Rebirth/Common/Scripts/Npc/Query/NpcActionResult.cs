using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Client;
using Common.Packets;

namespace Common.Scripts.Npc.Query
{
    public class NpcActionResult
    {

        public WvsGameClient Client { get; }
        public int Type { get; }

        public NpcActionResult(int type, WvsGameClient client)
        {
            Type = type;
            Client = client;
        }

        public void Work()
        {

            Client.SendPacket(CPacket.BroadcastPinkMsg("NPC Action Result " + Type));

            switch (Type)
            {
                case 0:

                    break;
                case 1:

                    break;
            }
        }

    }
}
