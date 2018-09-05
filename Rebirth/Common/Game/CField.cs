using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Client;
using Common.Entities;
using Common.Log;
using Common.Network;
using Common.Packets;
using Common.Server;
using reWZ.WZProperties;
using Tools;

namespace Common.Game
{
    public sealed class CField
    {
        public int MapId { get; }

        public List<AvatarData> Characters { get; }
        public Dictionary<AvatarData, WvsGameClient> Sockets { get; }

        public List<Portal> Portals { get; }

        public CField(int mapId)
        {
            MapId = mapId;

            Characters = new List<AvatarData>();
            Sockets = new Dictionary<AvatarData, WvsGameClient>();

            Portals = new List<Portal>();
        }

        public void Add(WvsGameClient c)
        {
            //Send SetField | But how do i know the connect one or the smaller one ?
            var character = c.Character;

            if (c.SentCharData)
            {
                c.SendPacket(CPacket.SetField(character, false));
            }
            else
            {
                c.SentCharData = true;
                character.Stats.nPortal = GetRandomSpawn();
                c.SendPacket(CPacket.SetField(character, true));
            }

            //Send client being added all the existing characters in the map
            Characters.ForEach(x => c.SendPacket(CPacket.UserEnterField(x)));

            Characters.Add(c.Character);
            Sockets.Add(c.Character, c);
            
            //Broadcast everyone already in the map that you have arrived
            Broadcast(CPacket.UserEnterField(character), c);
        }
        public void Remove(WvsGameClient c)
        {
            var character = c.Character;
            Broadcast(CPacket.UserLeaveField(character), c);

            Characters.Remove(c.Character);
            Sockets.Remove(c.Character);
        }

        public void Broadcast(COutPacket packet)
        {
            foreach (var kvp in Sockets)
            {
                kvp.Value.SendPacket(packet);
            }

            packet.Dispose();
        }
        public void Broadcast(COutPacket packet, params WvsGameClient[] excludes)
        {
            foreach (var kvp in Sockets)
            {
                if (!excludes.Contains(kvp.Value))
                    kvp.Value.SendPacket(packet);
            }

            packet.Dispose();
        }

        public void LoadPortals(WvsCenter parentServer)
        {
            var wz = parentServer.WzMan["Map.wz"];
            var path = $"Map/Map{MapId / 100000000}/{MapId}.img/portal";
            var portals = wz.ResolvePath(path);

            foreach (WZObject x in portals)
            {
                var p = new Portal
                {
                    nIdx = Convert.ToInt32(x.Name),
                    sName = x["pn"].ValueOrDie<string>(),
                    nType = x["pt"].ValueOrDie<int>(),
                    nTMap = x["tm"].ValueOrDie<int>(),
                    sTName = x["tn"].ValueOrDie<string>(),
                    ptPos =
                    {
                        X = (short)x["x"].ValueOrDie<int>(),
                        Y = (short)x["y"].ValueOrDie<int>()
                    }
                };

                Portals.Add(p);
            }

        }

        public byte GetRandomSpawn()
        {
            var list = Portals.Where(p => p.sName == "sp").ToArray();

            if (list.Length == 0)
                return 0;

            return (byte)list.Random().nIdx;
        }

    }
}
