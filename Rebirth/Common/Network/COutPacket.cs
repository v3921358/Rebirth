using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Network
{
    public class COutPacket : IDisposable
    {
        public const int DefaultBufferSize = 1024;

        private MemoryStream m_stream;
        private bool m_disposed;

        public COutPacket(int bufferSize = DefaultBufferSize)
        {
            m_stream = new MemoryStream(DefaultBufferSize);
            m_disposed = false;
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

            Append(value.Length,2);
            
            foreach (char c in value)
                Append(c, 1);
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

            if (m_stream != null)
                m_stream.Dispose();

            m_stream = null;
        }
    }
}
