using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Entities;
using Common.Game;
using Common.Log;
using Common.Network;
using Common.Packets;
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

        public void HandleCommand(string[] split)
        {
            //100% for testing

            switch (split[0])
            {
                case "!snail":
                    {
                        var mob = new CMob(100101);
                        mob.Position.Position = Character.Position.Position;
                        mob.Position.Foothold = Character.Position.Foothold;
                        
                        Logger.Write(LogLevel.Debug,"MrSnail {0}",mob.Position);

                        SendPacket(CPacket.MobEnterField(mob));
                        SendPacket(CPacket.MobChangeController(mob, 1));

                        break;
                    }
                case "!mobs":
                    {
                        GetCharField().SendSpawnMobs(this);
                        break;
                    }
                case "!npcs":
                    {
                        GetCharField().SendSpawnNpcs(this);
                        break;
                    }
            }

        }
    }
}
