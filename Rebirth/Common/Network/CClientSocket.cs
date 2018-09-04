using System;
using System.Net.Sockets;
using Common.Crypto;

namespace Common.Network
{
    /// <summary>
    /// To be fair, A lot of people may accuse me of copy pasting 
    /// from other public C# sources, but in reality I wrote the 
    /// code in those sources. ( Networking Stuff )
    /// </summary>
    public class CClientSocket : IDisposable
    {
        public const int ReceiveSize = 8192;

        private readonly Socket m_socket;

        private byte[] m_recvBuffer;
        private byte[] m_buffer;
        private int m_offset;

        private ushort m_version;
        private MapleIV m_siv;
        private MapleIV m_riv;

        private object m_sendSync;

        public string Host { get; set; }
        public bool Disposed { get; private set; }

        public CClientSocket(Socket socket)
        {
            m_socket = socket;

            Host = TcpHelp.SetSockOpt(ref m_socket);
            Disposed = false;

            m_recvBuffer = new byte[ReceiveSize];
            m_buffer = new byte[ReceiveSize];
            m_offset = 0;
            
            m_sendSync = new object();
        }

        //Eventually move this out of socket
        public void Initialize(ushort version)
        {
            m_siv = new MapleIV(0xBADF00D);
            m_riv = new MapleIV(0XDEADBEEF);

            m_version = version;

            //m_siv = new MapleIV(0x52616A61); //Raja
            //m_riv = new MapleIV(0x6E523078); //nR0x

            using (var p = new COutPacket())
            {
                p.Encode2(0x0E);
                p.Encode2((short)version);
                p.EncodeString("1");
                p.Encode4((int)m_riv.Value);
                p.Encode4((int)m_siv.Value);
                p.Encode1(8);

                var buffer = p.ToArray();

                SendRaw(buffer);
            }

            Receive();
        }

        public event Action<CInPacket> OnPacket;
        public event Action OnDisconnected;

        internal void Receive()
        {
            if (Disposed)
                return;

            SocketError errorCode = SocketError.Success;

            m_socket.BeginReceive(m_recvBuffer, 0, m_recvBuffer.Length, SocketFlags.None, out errorCode, EndReceive, null);

            if (errorCode != SocketError.Success)
                Dispose();
        }
        private void EndReceive(IAsyncResult ar)
        {
            if (!Disposed)
            {
                SocketError errorCode = SocketError.Success;

                int length = m_socket.EndReceive(ar, out errorCode);

                if (errorCode != SocketError.Success || length == 0)
                {
                    Dispose();
                }
                else
                {
                    Append(length);
                    ManipulateBuffer();
                    Receive();
                }
            }
        }

        private void Append(int length)
        {
            if (m_buffer.Length - m_offset < length)
            {
                int newSize = m_buffer.Length * 2;

                while (newSize < m_offset + length)
                    newSize *= 2;

                Array.Resize<byte>(ref m_buffer, newSize);
            }

            Buffer.BlockCopy(m_recvBuffer, 0, m_buffer, m_offset, length);

            m_offset += length;
        }
        private void ManipulateBuffer()
        {
            while (m_offset >= 4 && Disposed == false)
            {
                int size = MapleAes.GetLength(m_buffer);

                if (size <= 0)
                {
                    Dispose();
                    return;
                }

                if (m_offset < size + 4)
                {
                    break;
                }

                var packetBuffer = new byte[size];
                Buffer.BlockCopy(m_buffer, 4, packetBuffer, 0, size);

                MapleAes.Transform(packetBuffer, m_riv);
                MapleCustom.DecryptTransform(packetBuffer);
    
                m_offset -= size + 4;

                if (m_offset > 0)
                {
                    Buffer.BlockCopy(m_buffer, size + 4, m_buffer, 0, m_offset);
                }

                OnPacket?.Invoke(new CInPacket(packetBuffer));
            }

        }

        internal void Send(byte[] packet)
        {
            if (Disposed)
                return;

            lock (m_sendSync)
            {
                if (Disposed)
                    return;

                //byte[] packet = outPacket.ToArray();
                byte[] final = new byte[packet.Length + 4];

                MapleAes.GetHeader(final, m_siv, m_version);

                MapleCustom.EncryptTransform(packet);
                MapleAes.Transform(packet, m_siv);

                Buffer.BlockCopy(packet, 0, final, 4, packet.Length);

                SendRaw(final);
            }
        }
        internal void SendRaw(byte[] final)
        {
            int offset = 0;

            while (offset < final.Length)
            {
                SocketError errorCode = SocketError.Success;
                int sent = m_socket.Send(final, offset, final.Length - offset, SocketFlags.None, out errorCode);

                if (sent == 0 || errorCode != SocketError.Success)
                {
                    Dispose();
                }

                offset += sent;
            }
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                Disposed = true;

                try
                {
                    m_socket.Shutdown(SocketShutdown.Both);
                }
                catch { /*Nothing*/ }

                try
                {
                    m_socket.Dispose();
                }
                catch { /*Nothing*/ }

                m_buffer = null;
                m_offset = 0;

                m_siv = null;
                m_riv = null;

                OnDisconnected?.Invoke();
            }
        }
    }
}