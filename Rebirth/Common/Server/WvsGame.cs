using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Client;
using Common.Entities;
using Common.Log;
using Common.Network;

namespace Common.Server
{
    public class WvsGame : ServerBase<WvsGameClient>
    {
        private static readonly Func<CClientSocket, WvsGameClient> ClientCreator
            = ccs => new WvsGameClient(ccs);
        //-----------------------------------------------------------------------------
        public byte ChannelId { get; private set; }
        //-----------------------------------------------------------------------------
        public WvsGame(byte channel) : base($"WvsGame{channel}", Constants.GamePort + channel, ClientCreator)
        {
            ChannelId = channel;
        }
        //-----------------------------------------------------------------------------
        protected override void HandlePacket(WvsGameClient socket, CInPacket packet)
        {
            base.HandlePacket(socket, packet);
            var opcode = (RecvOps)packet.Decode2();

            //TODO: Some migrate flag before accepted other packets

            switch (opcode)
            {
                case RecvOps.CP_MigrateIn:
                    Handle_MigrateIn(socket, packet);
                    break;
            }
        }
        //-----------------------------------------------------------------------------
        private void Handle_MigrateIn(WvsGameClient c, CInPacket p)
        {
            var uid = p.Decode4();
            var character = Character.Default();
            c.SendPacket(CPacket.SetFieldComplete(character,true));
        }
    }
}
