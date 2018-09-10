using System;
using Common.Log;
using Common.Network;
using Common.Packets;

namespace Common.Client
{
    //TODO: Gotta make this guy and his child* disposable
    public abstract class ClientBase
    {
        private readonly CClientSocket m_socket;

        public string Host => m_socket.Host;

        public bool LoggedIn { get; protected set; }

        public byte ServerId { get; set; }
        public byte ChannelId { get; set; }

        protected ClientBase(CClientSocket socket)
        {
            m_socket = socket;
        }

        public void SendPacket(COutPacket packet)
        {
            SendPacket(packet.ToArray());
        }
        private void SendPacket(byte[] packet)
        {
            var buffer = packet;
            var opcode = (SendOps)BitConverter.ToInt16(buffer, 0);

            if (Constants.FilterSendOpCode(opcode) == false)
            {
                var name = Enum.GetName(typeof(SendOps), opcode);
                var str = Constants.GetString(buffer);

                Logger.Write(LogLevel.Info, "Send [{0}] {1}", name, str);
            }

            m_socket.SendPacket(packet);
        }

        public void Disconnect()
        {
            m_socket.Dispose();
        }
    }
}
