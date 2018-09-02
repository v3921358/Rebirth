using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Network
{
    public class CInPacket
    {
        private readonly byte[] m_buffer;
        private int m_index;

        public int Position
        {
            get
            {
                return m_index;
            }
        }
        public int Available
        {
            get
            {
                return m_buffer.Length - m_index;
            }
        }

        public CInPacket(byte[] packet)
        {
            m_buffer = packet;
            m_index = 0;
        }

        private void CheckLength(int length)
        {
            if (m_index + length > m_buffer.Length || length < 0)
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
        public unsafe int Encode4()
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
        public unsafe long Encode8()
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
        public byte[] EncodeBuffer(byte[] value, int start, int length)
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
