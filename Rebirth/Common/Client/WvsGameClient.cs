using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public CharacterData Character { get; private set; }

        public WvsGameClient(WvsGame game, CClientSocket socket) : base(socket)
        {
            ParentServer = game;
            Initialized = false;
            SentCharData = false;
            Character = null;
        }

        public void LoadCharacter(int uid)
        {
            //TODO: Real database lol
            var temp = AvatarData.Default();

            Character = CharacterData.Create(temp.Stats, temp.Look);
            Initialized = true;
        }

        public CField GetCharField() => ParentServer.GetField(Character.Stats.dwPosMap);

        public void UsePortal(Portal portal)
        {
            var oldField = GetCharField();
            oldField.Remove(this);

            Character.Stats.dwPosMap = portal.nTMap;

            var newField = GetCharField();

            var spawn = newField.Portals.GetByName(portal.sTName);
            Character.Stats.nPortal = spawn == null ? newField.Portals.GetRandomSpawn() : (byte)spawn.nIdx;

            //var foothold = newField.Footholds.FindBelow(portal.ptPos);
            Character.Position.Foothold = 0;//(short)(foothold?.Id ?? 0);

            newField.Add(this);
        }
    }
}
