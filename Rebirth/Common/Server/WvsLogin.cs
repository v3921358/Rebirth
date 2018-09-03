using System;
using Common;
using Common.Network;
using Common.Server;

namespace WvsRebirth
{
    public class WvsLogin : ServerBase
    {
        public WvsLogin() : base("WvsLogin", 8484)
        {
                
        }

        protected override void HandlePacket(CClientSocket socket, CInPacket packet)
        {
            base.HandlePacket(socket,packet);

            var opcode = (RecvOps)packet.Decode2();
             
            switch (opcode)
            {
                case RecvOps.CP_CheckPassword:
                    Handle_CheckPassword(socket, packet);
                    break;
                case RecvOps.CP_CreateSecurityHandle:
                    //In GMS this packet was sent when the client
                    //loaded to the login screen. This means HackShield
                    //initialized successfully. This packet would trigger
                    //the HackShield heartbeat. Luckily back in the day
                    //if you never sent this packet you were never kicked,
                    //so you could stay on the [LOGIN] server only.
                    break;
                case RecvOps.CP_WorldRequest:

                    Handle_WorldRequest(socket,packet);

                    break;



            }
        }

        private void Handle_WorldRequest(CClientSocket c, CInPacket p)
        {
            SendPacket(c,CPacketFactory.WorldRequest());
            SendPacket(c, CPacketFactory.WorldRequestEnd());
       }

        private void Handle_CheckPassword(CClientSocket c, CInPacket p)
        {
            var pwd = p.DecodeString();
            var user = p.DecodeString();
          
            //Console.WriteLine(pwd);
            //Console.WriteLine(user);
            
            SendPacket(c,CPacketFactory.CheckPasswordResult(1337,0,0,user));
        }
    }
}
