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
            var p = new COutPacket();

            p.Encode2((short)SendOps.LP_CheckPasswordResult);
            p.Encode1(0); //v3 = CInPacket::Decode1(iPacket);
            p.Encode1(0); //sMsg._m_pStr[500] = CInPacket::Decode1(iPacket);
            p.Encode4(0); //CInPacket::Decode4(iPacket);

            p.Encode4(accId); // pBlockReason.m_pInterface = (IWzProperty *)CInPacket::Decode4(iPacket);
            p.Encode1(0); //pBlockReasonIter.m_pInterface = (IWzProperty *)(unsigned __int8)CInPacket::Decode1(iPacket);
            p.Encode1(0); //LOBYTE(nGradeCode) = CInPacket::Decode1(iPacket);
            p.Encode2(0); //v39 = (char *)CInPacket::Decode2(iPacket);
            p.Encode1(0); //LOBYTE(nCountryID) = CInPacket::Decode1(iPacket);
            p.EncodeString(accountName); //CInPacket::DecodeStr(iPacket, &sNexonClubID);
            p.Encode1(0); //LOBYTE(nPurchaseExp) = CInPacket::Decode1(iPacket);
            p.Encode1(0); //  LOBYTE(sMsg2._m_pStr) = CInPacket::Decode1(iPacket);
            p.Encode8(0); //CInPacket::DecodeBuffer(iPacket, &dtChatUnblockDate, 8u);
            p.Encode8(0); //CInPacket::DecodeBuffer(iPacket, &dtRegisterDate, 8u);
            p.Encode4(0); //nNumOfCharacter = CInPacket::Decode4(iPacket);
            p.Encode1(1); //v43 = (unsigned __int8)CInPacket::Decode1(iPacket)
            p.Encode1(0); //sMsg._m_pStr[432] = CInPacket::Decode1(iPacket);

            p.Encode8(0);
            p.Encode8(0);
            p.Encode8(0);

            //mplew.Encode4(0);
            //mplew.Encode2(0);
            //mplew.Encode4(accId); //user id
            //mplew.Encode1(gender);
            //mplew.Encode1((byte)(gmLevel > 0 ? 1 : 0)); //admin byte
            //short toWrite = (short)(gmLevel * 32);
            ////toWrite = toWrite |= 0x100; only in higher versions
            //mplew.Encode1((byte)(toWrite > 0x80 ? 0x80 : toWrite)); //0x80 is admin, 0x20 and 0x40 = subgm
            //mplew.Encode1((byte)(gmLevel > 0 ? 1 : 0));
            ////mplew.Encode2(toWrite > 0x80 ? 0x80 : toWrite); only in higher versions...
            //mplew.EncodeString(accountName);
            //mplew.Encode1(0);
            //mplew.Encode1(0); //isquietbanned
            //mplew.Encode8(0); //isquietban time
            //mplew.Encode8(0); //creation time
            //mplew.Encode4(0);
            //mplew.Encode2(2); //PIN

            return p;

        }
    }
}
