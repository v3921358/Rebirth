using System;
using Common.Packets;

namespace Common.Entities
{
    //All these are updated to v95

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

            if (ret == null)
                throw new Exception("Unknown item type");

            ret.RawDecode(p);
            return ret;
        }
    }

    public class GW_ItemSlotBundle : GW_ItemSlotBase
    {
        public short nNumber;
        public short nAttribute;
        public long liSN;
        public string sTitle = string.Empty; //char sTitle[13];

        public override void RawEncode(COutPacket p)
        {
            p.Encode1(2);

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
        public string sPetName = string.Empty;//char sPetName[13];
        public byte nLevel;
        public short nTameness;
        public byte nRepleteness;
        public short nPetAttribute;
        public short usPetSkill;
        public long dateDead = Constants.PERMANENT;
        public int nRemainLife;
        public short nAttribute;

        public override void RawEncode(COutPacket p)
        {
            p.Encode1(3);

            base.RawEncode(p);

            p.EncodeFixedString(sPetName, 13);
            p.Encode1(nLevel);
            p.Encode2(nTameness);
            p.Encode1(nRepleteness);
            p.Encode8(dateDead);
            p.Encode2(nPetAttribute);
            p.Encode2(usPetSkill);
            p.Encode4(nRemainLife);
            p.Encode2(nAttribute);
        }
    }

    public class GW_ItemSlotEquip : GW_ItemSlotBase
    {
        public byte nRUC;
        public byte nCUC;
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
        public string sTitle = string.Empty;//char sTitle[13];
        public byte nLevelUpType;
        public byte nLevel;
        public int nEXP;
        public int nDurability = -1;
        public int nIUC;
        public byte nGrade;
        public byte nCHUC;
        public short nOption1;
        public short nOption2;
        public short nOption3;
        public short nSocket1;
        public short nSocket2;
        public long ftEquipped = Constants.PERMANENT;
        public int nPrevBonusExpRate = -1;


        public override void RawEncode(COutPacket p)
        {
            p.Encode1(1);

            base.RawEncode(p);

            p.Encode1(nRUC);
            p.Encode1(nCUC);
            p.Encode2(niSTR);
            p.Encode2(niDEX);
            p.Encode2(niINT);
            p.Encode2(niLUK);
            p.Encode2(niMaxHP);
            p.Encode2(niMaxMP);
            p.Encode2(niPAD);
            p.Encode2(niMAD);
            p.Encode2(niPDD);
            p.Encode2(niMDD);
            p.Encode2(niACC);
            p.Encode2(niEVA);
            p.Encode2(niCraft);
            p.Encode2(niSpeed);
            p.Encode2(niJump);
            p.EncodeString(sTitle);
            p.Encode2(nAttribute);

            p.Encode1(nLevelUpType);
            p.Encode1(nLevel);
            p.Encode4(nEXP);
            p.Encode4(nDurability);
            p.Encode4(nIUC);
            p.Encode1(nGrade);
            p.Encode1(nCHUC);
            p.Encode2(nOption1);
            p.Encode2(nOption2);
            p.Encode2(nOption3);
            p.Encode2(nSocket1);
            p.Encode2(nSocket2);

            if (liCashItemSN == 0)
                p.Encode8(liSN);

            p.Encode8(ftEquipped);
            p.Encode4(nPrevBonusExpRate);
        }
    }
}