using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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
            var buffer = packet.ToArray();
            var opcode = (RecvOps)packet.Decode2();

            var name = Enum.GetName(typeof(RecvOps), opcode);
            var str = BitConverter.ToString(buffer);

            Console.WriteLine("Recv [{0}] {1}",name, str);

            switch (opcode)
            {
                case RecvOps.LP_GuestIDLoginResult:
                    Handle_GuestIDLoginResult(socket, packet);
                    break;

            }

        }

        private void Handle_GuestIDLoginResult(CClientSocket c, CInPacket p)
        {
            var v1 = p.DecodeString();
            var v2 = p.DecodeString();

            Console.WriteLine(v1);
            Console.WriteLine(v2);

        }

    }
}
