using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Common.Network;

namespace Common.Client
{
    //TODO: Gotta make this guy and his parent disposable
    public abstract class ClientBase
    {
        private readonly CClientSocket m_socket;

        public string Host => m_socket.Host;

        protected ClientBase(CClientSocket socket)
        {
            m_socket = socket;
        }

        public void SendPacket(COutPacket packet)
        {
            var buffer = packet.ToArray();
            var opcode = (SendOps)BitConverter.ToInt16(buffer, 0);

            var name = Enum.GetName(typeof(SendOps), opcode);
            var str = BitConverter.ToString(buffer);

            Logger.Write(LogLevel.Info, "Send [{0}] {1}", name, str);

            m_socket.Send(packet);
        }

        public void Disconnect()
        {
            m_socket.Dispose();
        }
    }
}
