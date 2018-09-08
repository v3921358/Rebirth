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

        public static void EncodeDateTime(this COutPacket packet, DateTime dt)
        {
            packet.Encode8(dt.ToFileTime());
        }
    }
}
