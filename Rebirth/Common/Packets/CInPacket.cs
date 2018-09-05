using System;
using System.Text;

namespace Common.Packets
{    
    /// <summary>
    /// TODO: Implement this https://github.com/RajanGrewal/ByteBuffer
    /// </summary>
    public class CInPacket
    {
        private readonly byte[] m_buffer;
        private readonly int m_length;
        private int m_index;
        
        public int Position => m_index;

        public int Available => m_length - m_index;

        public CInPacket(byte[] data)
        {
            m_buffer = data;
            m_index = 0;
            m_length = data.Length;
        }
        public CInPacket(byte[] data,int start,int length)
        {
            m_buffer = data;
            m_index = start;
            m_length = length;
        }

        private void CheckLength(int count)
        {
            if (m_index + count > m_length || count < 0)
                throw new InvalidOperationException("Not enough space");
        }

        public byte Decode1()
        {
            CheckLength(1);
            return m_buffer[m_index++];
        }
        public unsafe short Decode2()
        {
            CheckLength(2);

            short value;

            fixed (byte* ptr = m_buffer)
            {
                value = *(short*)(ptr + m_index);
            }

            m_index += 2;

            return value;
        }
        public unsafe int Decode4()
        {
            CheckLength(4);

            int value;

            fixed (byte* ptr = m_buffer)
            {
                value = *(int*)(ptr + m_index);
            }

            m_index += 4;

            return value;
        }
        public unsafe long Decode8()
        {
            CheckLength(8);

            long value;

            fixed (byte* ptr = m_buffer)
            {
                value = *(long*)(ptr + m_index);
            }

            m_index += 8;

            return value;
        }
        public byte[] DecodeBuffer(int length)
        {
            CheckLength(length);
            var temp = new byte[length];
            Buffer.BlockCopy(m_buffer, m_index, temp, 0, length);
            m_index += length;
            return temp;
        }
        public string DecodeString()
        {
            var size = Decode2();

            var sb = new StringBuilder(size);

            for (int i = 0; i < size; i++)
                sb.Append((char)Decode1());

            return sb.ToString();
        }

        public byte[] ToArray()
        {
            var final = new byte[m_buffer.Length];
            Buffer.BlockCopy(m_buffer, 0, final, 0, m_buffer.Length);
            return final;
        }
    }
}
