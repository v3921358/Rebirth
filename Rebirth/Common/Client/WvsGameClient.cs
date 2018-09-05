using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using Common.Entities;
using Common.Game;
using Common.Log;
using Common.Network;
using Common.Server;

namespace Common.Client
{
    public class WvsGameClient : ClientBase
    {
        public WvsGame ParentServer { get; }
        public bool Initialized { get; private set; }
        public bool SentCharData { get; set; }

        //public CharacterData Character { get; private set; }
        public AvatarData Character { get; private set; }


        public WvsGameClient(WvsGame game,CClientSocket socket) :base(socket)
        {
            ParentServer = game;
            Initialized = false;
            SentCharData = false;
            Character = null;
        }

        public void LoadCharacter(int uid)
        {
            Character = AvatarData.Default();
            Initialized = true;
        }

        public CField GetCharField() => ParentServer.GetField(Character.Stats.dwPosMap);

        public void UsePortal(Portal portal)
        {
            var oldField = GetCharField();
            oldField.Remove(this);

            Character.Stats.dwPosMap = portal.nTMap;
            var newField = GetCharField();

            var spawn = newField.Portals.FirstOrDefault(x => portal.sTName == x.sName);
            
            //TODO: Verify this in the future
            Character.Stats.nPortal = spawn == null ? newField.GetRandomSpawn() : (byte)spawn.nIdx; 
            
            newField.Add(this);
        }
    }
}
