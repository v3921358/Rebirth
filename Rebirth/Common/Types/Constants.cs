using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Client;
using Common.Network;
using Common.Packets;
using Common.Scripts.Npc;

namespace Common
{
    public static class Constants
    {
        public static Random Rand { get; } = new Random();

        public const ushort Version = 95;

        public const int LoginPort = 8484;
        public const int GamePort = 8585;

        //public static readonly long MaxTime = new DateTime(2040, 1, 1).ToFileTime();

        public const long MAX_TIME = 150842304000000000L;
        public const long ZERO_TIME = 94354848000000000L;
        public const long PERMANENT = 150841440000000000L;
        
        public const string ServerMessage = @"Welcome to the stream! http://twitch.tv/rajan710";

        public const int MigrateTimeoutSec = 15;

        //I have SUPER optomized versions of these methods just
        //on another computer and i cant b bothered rn this is bad but like
        //its really not that bad. ill put a cool hextool in later
        private static byte HexToByte(string hex)
        {
            if (hex.Length > 2 || hex.Length <= 0)
                throw new ArgumentException("hex must be 1 or 2 characters in length");

            return byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        }
        public static string GetString(byte[] buffer)
        {
            return BitConverter.ToString(buffer).Replace("-", " ");
        }
        public static byte[] GetBytes(string hexString)
        {
            string newString = hexString
                .Replace(" ", "")
                .Replace("-", "");

            // if odd number of characters, discard last character
            if (newString.Length % 2 != 0)
            {
                newString = newString.Substring(0, newString.Length - 1);
            }

            int byteLength = newString.Length / 2;
            byte[] bytes = new byte[byteLength];
            string hex;
            int j = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                hex = new String(new Char[] { newString[j], newString[j + 1] });
                bytes[i] = HexToByte(hex);
                j = j + 2;
            }
            return bytes;
        }

        public static short GetRealJobFromCreation(int job)
        {
            switch (job)
            {
                case 0:
                    return 3000; //citizen
                case 1:
                    return 0; //beginner
                case 2: 
                    return 1000; //noblese
                case 3:
                    return 2000; //legend
                case 4:
                    return 2001; //evan
            }
            return 0;
        }
        public static bool IsNotExtendedSp(short job)
        {
            return job / 1000 != 3 && job / 100 != 22 && job != 2001;
        }

        public static bool FilterRecvOpCode(RecvOps recvOp)
        {
            switch (recvOp)
            {
                case RecvOps.CP_ClientDumpLog:
                case RecvOps.CP_ExceptionLog:
                case RecvOps.CP_UserMove:
                case RecvOps.CP_MobMove:
                case RecvOps.CP_UserEmotion:
                    return true;
            }
            return false;
        }
        public static bool FilterSendOpCode(SendOps sendOp)
        {
            switch (sendOp)
            {
                case SendOps.LP_UserMove:
                case SendOps.LP_MobCtrlAck:
                case SendOps.LP_MobMove:
                case SendOps.LP_UserEmotion:
                    return true;
            }
            return false;
        }
    }
}
