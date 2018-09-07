using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Client;
using Common.Scripts.Npc.Query;

namespace Common.Scripts.Npc
{
    public class Npc9900000 : NpcScript
    {
        public Npc9900000(WvsGameClient client) : base(9900000, client)
        {
        }


        public override void Execute()
        {
            SendOk("Hello Kacey, Shortcuts, Vazdias");
        }
    }
}
