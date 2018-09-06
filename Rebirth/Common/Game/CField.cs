﻿using System;
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

namespace Common.Game
{
    public sealed class CField
    {
        public int MapId { get; }

        public List<CharacterData> Characters { get; }
        public Dictionary<CharacterData, WvsGameClient> Sockets { get; }

        public CPortalMan Portals { get; }
        public CFootholdMan Footholds { get; }
        public CMobPool Mobs { get; }
        public CNpcPool Npcs { get; set; }

        public CField(int mapId)
        {
            MapId = mapId;

            Characters = new List<CharacterData>();
            Sockets = new Dictionary<CharacterData, WvsGameClient>();

            Portals = new CPortalMan();
            Footholds = new CFootholdMan();
            Mobs = new CMobPool();
            Npcs = new CNpcPool();
        }

        public void Add(WvsGameClient c)
        {
            //Send SetField | But how do i know the connect one or the smaller one ?
            var character = c.Character;

            if (c.SentCharData)
            {
                c.SendPacket(CPacket.SetField(character, false, c.ChannelId));
            }
            else
            {
                c.SentCharData = true;
                character.Stats.nPortal = Portals.GetRandomSpawn();
                c.SendPacket(CPacket.SetField(character, true, c.ChannelId));
            }

            //Send client being added all the existing characters in the map
            Characters.ForEach(x => c.SendPacket(CPacket.UserEnterField(x)));

            SendSpawnMobs(c);
            SendSpawnNpcs(c);

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

        public void SendSpawnMobs(WvsGameClient c)
        {
            foreach (var mob in Mobs.Spawns)
            {
                var x = new CMob(mob.Id);
                x.Position.Position.X = (short)mob.X;
                x.Position.Position.Y = (short)mob.Cy;
                x.Position.Foothold = (short) mob.Foothold;

                c.SendPacket(CPacket.MobEnterField(x));

                //TODO: Remove this controller shit its for testing right now single player
                c.SendPacket(CPacket.MobChangeController(x,1));
            }
        }
        public void SendSpawnNpcs(WvsGameClient c)
        {
            foreach (var npc in Npcs.Spawns)
            {
                c.SendPacket(CPacket.NpcEnterField(npc));
            }
        }

        public void Load(WvsCenter parentServer)
        {
            var wzMan = parentServer.WzMan;
            Portals.Load(MapId, wzMan);
            Footholds.Load(MapId, wzMan);
            Mobs.Load(MapId, wzMan);
            Npcs.Load(MapId, wzMan);
        }
    }
}
