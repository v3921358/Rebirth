using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Client;
using Common.Packets;
using Common.Scripts.Npc.Query;

namespace Common.Scripts.Npc
{
    public class NpcDefault : NpcScript
    {
        public NpcDefault(int npcId, WvsGameClient client) : base(npcId, client)
        {
        }

        public override void Execute()
        {
            SendOk("Hello I am the default script");
        }

        //Ignore this pseudo weird npc script idea impl
        //public override IEnumerable<NpcActionResult> DoScript()
        //{
        //    yield return new NpcActionResult(1,m_client);
        //    yield return new NpcActionResult(1, m_client);
        //    yield return new NpcActionResult(2, m_client);

        //}

    }
}
