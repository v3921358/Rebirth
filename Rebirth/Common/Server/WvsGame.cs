using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Common.Client;
using Common.Game;
using Common.Log;
using Common.Network;
using Common.Packets;

namespace Common.Server
{
    public class WvsGame : ServerBase<WvsGameClient>
    {
        //-----------------------------------------------------------------------------
        public byte ChannelId { get; }

        public Dictionary<int, CField> CFieldMan { get; }
        //-----------------------------------------------------------------------------
        public WvsGame(WvsCenter parent, byte channel) : base($"WvsGame{channel}", Constants.GamePort + channel, parent)
        {
            ChannelId = channel;
            CFieldMan = new Dictionary<int, CField>();
        }
        //-----------------------------------------------------------------------------
        public CField GetField(int id)
        {
            if (!CFieldMan.ContainsKey(id))
            {
                var field = new CField(id);
                field.Load(ParentServer);

                CFieldMan.Add(id, field);
            }

            return CFieldMan[id];
        }
        //-----------------------------------------------------------------------------
        protected override WvsGameClient CreateClient(CClientSocket socket)
        {
            return new WvsGameClient(this, socket)
            {
                ChannelId = ChannelId,
            };
        }

        protected override void HandlePacket(WvsGameClient socket, CInPacket packet)
        {
            base.HandlePacket(socket, packet);
            var opcode = (RecvOps)packet.Decode2();

            if (socket.Initialized)
            {
                switch (opcode)
                {
                    case RecvOps.CP_UserChat:
                        Handle_UserChat(socket, packet);
                        break;
                    case RecvOps.CP_UserMove:
                        Handle_UserMove(socket, packet);
                        break;
                    case RecvOps.CP_UserTransferFieldRequest:
                        Handle_UserTransferFieldRequest(socket, packet);
                        break;
                    case RecvOps.CP_UserEmotion:
                        Handle_UserEmotion(socket, packet);
                        break;
                    case RecvOps.CP_MobMove:
                        Handle_MobMove(socket, packet);
                        break;
                }
            }
            else
            {
                switch (opcode)
                {
                    case RecvOps.CP_MigrateIn:
                        Handle_MigrateIn(socket, packet);
                        break;
                    case RecvOps.CP_AliveAck:
                        break;
                    default:
                        //An invalid packet was sent before the client migrated in
                        socket.Disconnect();
                        break;
                }
            }
        }
        protected override void HandleDisconnect(WvsGameClient client)
        {
            base.HandleDisconnect(client);

            client.SentCharData = false;

            if (client.Initialized)
            {
                client.GetCharField().Remove(client);
            }
        }
        //-----------------------------------------------------------------------------
        private void Handle_MigrateIn(WvsGameClient c, CInPacket p)
        {
            var uid = p.Decode4();

            c.LoadCharacter(uid);

            var character = c.Character;
            character.Stats.dwCharacterID = Constants.Rand.Next(1000, 9999); //AGAIN

            GetField(character.Stats.dwPosMap).Add(c);
        }
        private void Handle_UserChat(WvsGameClient c, CInPacket p)
        {
            var tick = p.Decode4();
            var msg = p.DecodeString();
            var show = p.Decode1() != 0;

            if (msg.StartsWith("!"))
            {
                var split = msg.Split(' ');
                c.HandleCommand(split);
            }
            else
            {
                var stats = c.Character.Stats;
                c.GetCharField().Broadcast(CPacket.UserChat(stats.dwCharacterID, msg, true, show));
            }
        }
        private void Handle_UserMove(WvsGameClient c, CInPacket p)
        {
            var v1 = p.Decode8();
            var portalCount = p.Decode1(); //CField::GetFieldKey(v20);
            var v2 = p.Decode8();
            var mapCrc = p.Decode4();
            var dwKey = p.Decode4();
            var dwKeyCrc = p.Decode4();

            var movePath = p.DecodeBuffer(p.Available);
            c.Character.Position.DecodeMovePath(movePath);

            var stats = c.Character.Stats;

            c.GetCharField().Broadcast(CPacket.UserMovement(stats.dwCharacterID, movePath), c);
        }
        private void Handle_UserTransferFieldRequest(WvsGameClient c, CInPacket p)
        {
            if (p.Available == 0)
            {
                //Cash Shop Related
                return;
            }

            //TODO: Portal count checks
            //TODO: XY rect checks
            //TODO: Keep track if player spawns when entering a field

            var portalCount = p.Decode1(); //CField::GetFieldKey(v20);
            var destination = p.Decode4(); //
            var portalName = p.DecodeString();
            var x = p.Decode2();
            var y = p.Decode2();
            //var extra = p.DecodeBuffer(3); idk | prem | chase

            var portal =
                c.GetCharField()
                    .Portals
                    .GetByName(portalName);

            if (portal == null)
            {
                Logger.Write(LogLevel.Warning, "Client tried to enter non existant portal {0}", portalName);
            }
            else
            {
                c.UsePortal(portal);
            }
        }
        private void Handle_UserEmotion(WvsGameClient c, CInPacket p)
        {
            var nEmotion = p.Decode4();
            var nDuration = p.Decode4();
            var bByItemOption = p.Decode1();

            //if (emote > 7)
            //{
            //    int emoteid = 5159992 + emote;
            //    //TODO: As if i care check if the emote is in CS inventory, if not return
            //}

            var stats = c.Character.Stats;

            c.GetCharField().Broadcast(CPacket.UserEmoticon(stats.dwCharacterID, nEmotion, nDuration, bByItemOption), c);
        }
        private void Handle_MobMove(WvsGameClient c, CInPacket p)
        {
            int dwMobId = p.Decode4();

            var nMobCtrlSN = p.Decode2();
            var v7 = p.Decode1(); //v85 = nDistance | 4 * (v184 | 2 * ((unsigned __int8)retaddr | 2 * v72)); [ CONFIRMED ]

            var pOldSplit = (v7 & 0xF0) != 0; //this is a type of CFieldSplit
            var bMobMoveStartResult = (v7 & 0xF) != 0;

            var pCurSplit = p.Decode1();
            var bIllegealVelocity = p.Decode4();
            var v8 = p.Decode1();

            var bCheatedRandom = (v8 & 0xF0) != 0;
            var bCheatedCtrlMove = (v8 & 0xF) != 0;

            p.Decode4(); //Loopy Decode 1
            p.Decode4(); //Loopy Decode 2

            p.DecodeBuffer(16);

            var movePath = p.DecodeBuffer(p.Available);

            //if (pMob->m_pController->pUser != pCtrl
            //    && (!pOldSplit
            //        || pMob->m_bNextAttackPossible
            //        || !CLifePool::ChangeMobController(&v5->m_lifePool, pCtrl->m_dwCharacterID, pMob, 1)))
            //{
            //    CMob::SendChangeControllerPacket(v9, v10, 0);
            //    return;
            //}

            c.SendPacket(CPacket.MobMoveAck(dwMobId, nMobCtrlSN, bMobMoveStartResult, 0, 0, 0));
        }
    }
}
