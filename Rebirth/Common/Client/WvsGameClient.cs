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
using Common.Scripts.Npc;
using Common.Server;

namespace Common.Client
{
    public class WvsGameClient : ClientBase
    {
        public WvsGame ParentServer { get; }
        public bool Initialized { get; private set; }
        public bool SentCharData { get; set; }

        public CharacterData Character { get; private set; }
        public NpcScript NpcScript { get; set; }


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

        public void SetField(int nMapId, byte nPortal, short nFh)
        {
            var newField = ParentServer.GetField(nMapId);

            if (newField != null)
            {
                var oldField = GetCharField();
                oldField.Remove(this);

                Character.Stats.dwPosMap = nMapId;
                Character.Stats.nPortal = nPortal;
                Character.Position.Foothold = nFh;

                newField.Add(this);
            }
        }

        public void HandleCommand(string[] split)
        {
            split[0] = split[0].Remove(0, 1);

            switch (split[0])
            {
                case "snail":
                    {
                        var mob = new CMob(100101);
                        mob.Position.Position = Character.Position.Position;
                        mob.Position.Foothold = Character.Position.Foothold;

                        Logger.Write(LogLevel.Debug, "MrSnail {0}", mob.Position);

                        var p1 = CPacket.MobEnterField(mob);

                        var field = GetCharField();
                        field.Broadcast(p1);

                        SendPacket(CPacket.MobChangeController(mob, 1));
                        break;
                    }
                case "pos":
                    {
                        var msg = $"Map: {Character.Stats.dwPosMap} - {Character.Position}";
                        SendPacket(CPacket.BroadcastPinkMsg(msg));
                        break;
                    }
                case "map":
                    {
                        var mapId = Convert.ToInt32(split[1]);
                        SetField(mapId, 0, 0);
                        break;
                    }
            }
        }
    }
}
