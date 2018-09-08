using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Packets;

namespace Common.Entities
{
    public abstract class GW_ItemSlotBase
    {
        public int nItemID;
        public long liCashItemSN;
        public long dateExpire;

        public virtual void RawEncode(COutPacket p)
        {
            p.Encode4(nItemID);

            var v3 = liCashItemSN != 0;

            p.Encode1(v3);

            if (v3)
                p.Encode8(liCashItemSN);

            p.Encode8(dateExpire);
        }
        public virtual void RawDecode(CInPacket p)
        {
            nItemID = p.Decode4();

            var v3 = p.Decode1() != 0;

            if (v3)
                liCashItemSN = p.Decode8();

            dateExpire = p.Decode8();
        }

        public static GW_ItemSlotBase Decode(CInPacket p)
        {
            var type = p.Decode1();
            GW_ItemSlotBase ret = null;

            if (type == 1)
                ret = new GW_ItemSlotEquip();

            if (type == 2)
                ret = new GW_ItemSlotBundle();

            if (type == 2)
                ret = new GW_ItemSlotPet();

            if(ret == null)
                throw new Exception("Unknown item type");

            ret.RawDecode(p);
            return ret;
        }
    }

    public class GW_ItemSlotBundle : GW_ItemSlotBase
    {
        public short nNumber;
        public short nAttribute;
        long liSN;
        public string sTitle; //char sTitle[13];

        public override void RawEncode(COutPacket p)
        {
            base.RawEncode(p);

            p.Encode2(nNumber);
            p.EncodeString(sTitle);
            p.Encode2(nAttribute);

            if (nItemID / 10000 == 207)
                p.Encode8(liSN);
        }
    }

    public class GW_ItemSlotPet : GW_ItemSlotBase
    {
        public string sPetName;//char sPetName[13];
        public byte nLevel;
        public short nTameness;
        public byte nRepleteness;
        public short nPetAttribute;
        public short usPetSkill;
        public long dateDead;

        public override void RawEncode(COutPacket p)
        {
            base.RawEncode(p);

            p.EncodeFixedString(sPetName, 13);
            p.Encode1(nLevel);
            p.Encode2(nTameness);
            p.Encode1(nRepleteness);
            p.Encode8(dateDead);
            p.Encode2(nPetAttribute);
            p.Encode2(usPetSkill);
        }
    }

    public class GW_ItemSlotEquip : GW_ItemSlotBase
    {
        public char nRUC;
        public char nCUC;
        public short niSTR;
        public short niDEX;
        public short niINT;
        public short niLUK;
        public short niMaxHP;
        public short niMaxMP;
        public short niPAD;
        public short niMAD;
        public short niPDD;
        public short niMDD;
        public short niACC;
        public short niEVA;
        public short niCraft;
        public short niSpeed;
        public short niJump;
        public short nAttribute;
        public long liSN;
        public string sTitle;//char sTitle[13];


        public override void RawEncode(COutPacket p)
        {
            base.RawEncode(p);

            //COutPacket::Encode1(oPacket, v2->nRUC);
            //COutPacket::Encode1(oPacket, v2->nCUC);
            //COutPacket::Encode2(oPacket, v2->niSTR);
            //COutPacket::Encode2(oPacket, v2->niDEX);
            //COutPacket::Encode2(oPacket, v2->niINT);
            //COutPacket::Encode2(oPacket, v2->niLUK);
            //COutPacket::Encode2(oPacket, v2->niMaxHP);
            //COutPacket::Encode2(oPacket, v2->niMaxMP);
            //COutPacket::Encode2(oPacket, v2->niPAD);
            //COutPacket::Encode2(oPacket, v2->niMAD);
            //COutPacket::Encode2(oPacket, v2->niPDD);
            //COutPacket::Encode2(oPacket, v2->niMDD);
            //COutPacket::Encode2(oPacket, v2->niACC);
            //COutPacket::Encode2(oPacket, v2->niEVA);
            //COutPacket::Encode2(oPacket, v2->niCraft);
            //COutPacket::Encode2(oPacket, v2->niSpeed);
            //COutPacket::Encode2(oPacket, v2->niJump);
            //v4._m_pStr = v3;
            //ZXString<char>::ZXString<char>(v2->sTitle, 0xFFFFFFFF);
            //COutPacket::EncodeStr(oPacket, v4);
            //COutPacket::Encode2(oPacket, v2->nAttribute);
            //if (!v2->liCashItemSN.QuadPart)
            //    COutPacket::EncodeBuffer(oPacket, &v2->liSN, 8u);
        }
    }
}