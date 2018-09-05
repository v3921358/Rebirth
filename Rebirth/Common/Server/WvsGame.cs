using System;
using System.Collections.Generic;
using System.Linq;
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

            var stats = c.Character.Stats;

            c.GetCharField().Broadcast(CPacket.UserChat(stats.dwCharacterID, msg, true, show));
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

            //TODO: Move this out later lol
            {
                var iPacket = new CInPacket(movePath);
                var x = iPacket.Decode2();
                var y = iPacket.Decode2();
                var vx = iPacket.Decode2();
                var vy = iPacket.Decode2();
                var size = iPacket.Decode1();

                //LOL
                c.Character.Position.Position.X = x;
                c.Character.Position.Position.Y = y;

                for (int i = 0; i < size; i++)
                {
                    var cmd = iPacket.Decode1();
                    
                    if (cmd == 0)
                    {
                        c.Character.Position.Position.X = iPacket.Decode2();
                        c.Character.Position.Position.Y = iPacket.Decode2();
                        var xwob = iPacket.Decode2();
                        var ywob = iPacket.Decode2();
                        c.Character.Position.Foothold = iPacket.Decode2();
                        var xoff = iPacket.Decode2();
                        var yoff = iPacket.Decode2();
                        c.Character.Position.Stance = iPacket.Decode1();
                        var duration = iPacket.Decode2();
                    }
                    else
                    {
                        Logger.Write(LogLevel.Debug, "Unparsed Movement SubOp {0}", cmd);
                        break; //break loop because we didnt parse subop
                    }
                }
            }

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
    }
}
