using System;
using System.IO;
using Common.Network;

namespace Common.Packets
{
    /// <summary>
    /// TODO: Implement this https://github.com/RajanGrewal/ByteBuffer
    /// </summary>
    public class COutPacket : IDisposable
    {
        private MemoryStream m_stream;
        private bool m_disposed;

        public COutPacket()
        {
            m_stream = new MemoryStream(1024);
            m_disposed = false;
        }
        public COutPacket(SendOps opCode) : this()
        {
            Encode2((short)opCode);
        }
       
        //From LittleEndianByteConverter by Shoftee
        private void Append(long value, int byteCount)
        {
            for (int i = 0; i < byteCount; i++)
            {
                m_stream.WriteByte((byte)value);
                value >>= 8;
            }
        }

        public void Encode1(byte value)
        {
            ThrowIfDisposed();
            m_stream.WriteByte(value);
        }
        public void Encode1(bool value)
        {
            ThrowIfDisposed();
            var x = (byte)(value ? 1 : 0);
            m_stream.WriteByte(x);
        }
        public void Encode2(short value)
        {
            ThrowIfDisposed();
            Append(value, 2);
        }
        public void Encode4(int value)
        {
            ThrowIfDisposed();
            Append(value, 4);
        }
        public void Encode8(long value)
        {
            ThrowIfDisposed();
            Append(value, 8);
        }
        public void EncodeBuffer(byte[] value, int start, int length)
        {
            ThrowIfDisposed();
            m_stream.Write(value, start, length);
        }
        public void EncodeString(string value)
        {
            ThrowIfDisposed();

            Append(value.Length, 2);

            foreach (char c in value)
                Append(c, 1);
        }

        public void Skip(int count)
        {
            ThrowIfDisposed();
            var value = new byte[count];
            m_stream.Write(value, 0, value.Length);
        }

        public byte[] ToArray()
        {
            ThrowIfDisposed();
            return m_stream.ToArray();
        }

        private void ThrowIfDisposed()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public void Dispose()
        {
            m_disposed = true;
            m_stream?.Dispose();
            m_stream = null;
        }
    }
}
