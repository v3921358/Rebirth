// ReSharper disable InconsistentNaming

using Common.Packets;

namespace Common.Entities
{
    public class GW_CharacterStat
    {
        public GW_CharacterStat()
        {
            aliPetLockerSN = new long[3];
            extendSP = new ExtendSP();
        }

        public int dwCharacterID;
        public string sCharacterName;
        public byte nGender;
        public byte nSkin;
        public int nFace;
        public int nHair;
        public long[] aliPetLockerSN;
        public byte nLevel;
        public short nJob;

        public short nSTR;
        public short nDEX;
        public short nINT;
        public short nLUK;
        public int nHP;
        public int nMHP;
        public int nMP;
        public int nMMP;

        public short nAP;
        public short nSP;

        public int nEXP;
        public short nPOP;
        public int nMoney;
        public int nTempEXP;

        public ExtendSP extendSP;

        public int dwPosMap;
        public byte nPortal;
        //public int nCheckSum;             //Not sure where this is used
        //public byte nItemCountCheckSum;   //Not sure where this is used
        public int nPlaytime;
        public short nSubJob;

        public void Encode(COutPacket p)
        {
            p.Encode4(dwCharacterID);
            p.EncodeFixedString(sCharacterName, 13);
            p.Encode1(nGender);
            p.Encode1(nSkin);
            p.Encode4(nFace);
            p.Encode4(nHair);

            for (int i = 0; i < aliPetLockerSN.Length; i++)
                p.Encode8(aliPetLockerSN[i]);

            p.Encode1(nLevel);
            p.Encode2(nJob);
            p.Encode2(nSTR);
            p.Encode2(nDEX);
            p.Encode2(nINT);
            p.Encode2(nLUK);
            p.Encode4(nHP);
            p.Encode4(nMHP);
            p.Encode4(nMP);
            p.Encode4(nMMP);
            p.Encode2(nAP);
            
            if (Constants.IsNotExtendedSp(nJob))
                p.Encode2(nSP);
            else
                extendSP.Encode(p);
            
            p.Encode4(nEXP); 
            p.Encode2(nPOP); 
            p.Encode4(nTempEXP); //Gachapon
            p.Encode4(dwPosMap);
            p.Encode1(nPortal); 
            p.Encode4(nPlaytime); 
            p.Encode2(nSubJob); //Here for dual blade?
        }
        public void EncodeMoney(COutPacket p)
        {
            p.Encode4(nMoney);
        }
    }
}
