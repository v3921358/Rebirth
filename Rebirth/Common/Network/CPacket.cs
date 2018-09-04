using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Client;
using Common.Network;
// ReSharper disable InconsistentNaming

namespace Common
{
    public static class CPacket
    {
        public static COutPacket CheckPasswordResult(int accId, byte gender, byte gmLevel, string accountName)
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
            p.Encode4(1); //nNumOfCharacter = CInPacket::Decode4(iPacket);
            p.Encode1(1); //v43 = (unsigned __int8)CInPacket::Decode1(iPacket)
            p.Encode1(0); //sMsg._m_pStr[432] = CInPacket::Decode1(iPacket);

            //I dont remember if this is extra padding or from the .idb
            p.Encode8(0);
            p.Encode8(0);
            p.Encode8(0);

            return p;
        }

        public static COutPacket WorldRequest(byte serverId)
        {
            var p = new COutPacket();

            //v6 = ZArray<CLogin::WORLDITEM>
            p.Encode2((short)SendOps.LP_WorldInformation);
            p.Encode1(serverId); //v4 [Server ID]  
            p.EncodeString("Server Name"); //WORLDITEM->sName
            p.Encode1(0); //v6->nWorldState
            p.EncodeString("Event Message?"); //sWorldEventDesc
            p.Encode2(100); //v6->nWorldEventEXP_WSE
            p.Encode2(100); //v6->nWorldEventDrop_WSE
            p.Encode1(0); //v6->nBlockCharCreation

            const byte channelCount = 1;

            p.Encode1(channelCount); //v9 ChannelCount

            for (int i = 0; i < channelCount; i++)
            {
                //v11 =  ZArray<CLogin::CHANNELITEM>
                p.EncodeString($"World {i + 1}");
                p.Encode4(200); //v11->nUserNo
                p.Encode1(serverId); //v11->nWorldID
                p.Encode1(0); //v11->nChannelID
                p.Encode1(0); //v11->bAdultChannel
            }

            p.Encode2(0); //v2->m_nBalloonCount 

            return p;
        }
        public static COutPacket WorldRequestEnd()
        {
            var p = new COutPacket();

            p.Encode2((short)SendOps.LP_WorldInformation);
            p.Encode1(0xff); //server id

            return p;
        }

        public static COutPacket SelectWorldResult(params Character[] chars)
        {
            var p = new COutPacket(SendOps.LP_SelectWorldResult);

            var charCount = (byte)chars.Length;

            p.Encode1(0);
            p.Encode1(charCount); //chars count

            foreach (var x in chars)
            {
                AddCharEntry(x,p);
            }

            p.Encode1(2); //m_bLoginOpt | spw request?

            p.Encode8(3);
            //p.Encode4(3); //m_nSlotCount
            //p.Encode4(0); //m_nBuyCharCount | https://i.imgur.com/DMynDxG.png
            
            return p;
        }

        public static COutPacket CheckUserLimit(byte status)
        {
            var p = new COutPacket();

            p.Encode2((short)SendOps.LP_CheckUserLimitResult);

            /* 
             * 0 - Normal
             * 1 - Highly populated
             * 2 - Full
             */

            p.Encode1(0); //bOverUserLimit
            p.Encode1(status); //bPopulateLevel

            return p;
        }

        public static COutPacket CheckDuplicatedIDResult(string name,bool nameTaken)
        {
            var p = new COutPacket(SendOps.LP_CheckDuplicatedIDResult);
            p.EncodeString(name);
            p.Encode1(nameTaken);
            return p;
        }

        public static COutPacket CreateNewCharacter(string name, bool worked,Character c)
        {
            var p = new COutPacket(SendOps.LP_CreateNewCharacterResult);
            p.Encode1((byte)(worked ? 0 : 1));

            if (worked)
            {
               AddCharEntry(c,p);
            }

            return p;
        }

        public static void AddCharEntry(Character c, COutPacket p)
        {
            //const bool ranking = false;

            c.EncodeStats(p);
            c.EncodeLook(p);

            //p.Encode8(0); //idk

            p.Encode1(0);
            p.Encode1(0); //ranking

            //if (ranking)
            //{
            //    mplew.writeInt(chr.getRank());
            //    mplew.writeInt(chr.getRankMove());
            //    mplew.writeInt(chr.getJobRank());
            //    mplew.writeInt(chr.getJobRankMove());
            //}
            
        }
    }
}
