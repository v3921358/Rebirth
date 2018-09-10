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
    public abstract class NpcScript
    {
        private readonly int m_npcId;
        protected readonly WvsGameClient m_client;
        
        private int lastMsg;

        protected NpcScript(int npcId,WvsGameClient client)
        {
            m_npcId = npcId;
            m_client = client;
        }

        public abstract void Execute();
        
        protected void SendOk(string text)
        {
            m_client.SendPacket(CPacket.NpcScriptMessage(m_npcId, 0, text, "00 00", 0, 0));
        }
        
        public void proceed_back() { }
        public void proceed_next() { }
        
        public void proceed_text(string t) { }
        public void proceed_selection(int s) { }
        public void proceed_number(int n) { }

        public bool check_end()
        {
            //if (is_end())
            //{
            //    if (m_next_npc != 0)
            //    {
            //        npc* next_npc = new npc { m_next_npc, m_player, m_script };
            //        next_npc->run();
            //    }
            //    m_player->set_npc(nullptr);
            //    return true;
            //}

            return false;
        }

        public void end()
        {
            //m_cend = true;
        }


        public static NpcScript GetScript(int npcId, WvsGameClient c)
        {
            switch (npcId)
            {
                case 9900000:
                    return new Npc9900000(c);
                default:
                    return new NpcDefault(npcId, c);
            }
        }
    }
}
