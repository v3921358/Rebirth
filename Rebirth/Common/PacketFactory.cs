using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Network;

namespace Common
{
    public static class PacketFactory
    {
        public static COutPacket getAuthSuccess(int accId, byte gender, byte gmLevel, string accountName)
        {
            var mplew = new COutPacket();

            mplew.Encode2((short)SendOps.LP_CheckPasswordResult);
            mplew.Encode4(0);
            mplew.Encode2(0);
            mplew.Encode4(accId); //user id
            mplew.Encode1(gender);
            mplew.Encode1((byte)(gmLevel > 0 ? 1 : 0)); //admin byte
            short toWrite = (short)(gmLevel * 32);
            //toWrite = toWrite |= 0x100; only in higher versions
            mplew.Encode1((byte)(toWrite > 0x80 ? 0x80 : toWrite)); //0x80 is admin, 0x20 and 0x40 = subgm
            mplew.Encode1((byte)(gmLevel > 0 ? 1 : 0));
            //mplew.Encode2(toWrite > 0x80 ? 0x80 : toWrite); only in higher versions...
            mplew.EncodeString(accountName);
            mplew.Encode1(0);
            mplew.Encode1(0); //isquietbanned
            mplew.Encode8(0); //isquietban time
            mplew.Encode8(0); //creation time
            mplew.Encode4(0);
            mplew.Encode2(2); //PIN

            return mplew;

        }
    }
}
