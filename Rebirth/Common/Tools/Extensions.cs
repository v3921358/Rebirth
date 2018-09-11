using System;
using System.Collections.Generic;
using System.Linq;
using Common.Packets;

namespace Common
{
    public static class Extensions
    {
        public static readonly Random Rand = new Random();

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (T current in collection)
            {
                action(current);
            }
        }
        public static void ForEach<T>(this T[] collection, Action<T> action)
        {
            foreach (T current in collection)
            {
                action(current);
            }
        }

        public static T Random<T>(this IEnumerable<T> collection)
        {
            var count = collection.Count();
            var rand = Rand.Next(count);
            return collection.ElementAt(rand);
        }
        public static T Random<T>(this T[] collection)
        {
            var count = collection.Length;
            var rand = Rand.Next(count);
            return collection[rand];
        }

        public static void Skip(this CInPacket packet, int length)
        {
            packet.DecodeBuffer(length);
        }
        public static void Encode1(this COutPacket packet, bool value)
        {
            packet.Encode1((byte)(value ? 1 : 0));
        }
        public static void EncodeDateTime(this COutPacket packet, DateTime dt)
        {
            packet.Encode8(dt.ToFileTime());
        }
        public static void EncodeFixedString(this COutPacket packet, string value, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (i < value.Length)
                {
                    packet.Encode1((byte)value[i]);
                }
                else
                {
                    packet.Encode1(0);
                }
            }
        }

        public static void EncodePos(this COutPacket packet, TagPoint value)
        {
            packet.Encode2(value?.X ?? 0);
            packet.Encode2(value?.Y ?? 0);
        }
        public static TagPoint DecodePos(this CInPacket packet)
        {
            var x = packet.Decode2();
           var y = packet.Decode2();
            return new TagPoint(x, y);
        }

        public static bool EqualsIgnoreCase(this string input,string comperand)
        {
            return string.Compare(input, comperand, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
