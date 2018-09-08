namespace Common.Entities
{
    public class AvatarData
    {
        public static AvatarData Default()
        {
            var x = new AvatarData();
            
            var s = x.Stats;
            s.dwCharacterID = Constants.GetUniqueId();
            s.sCharacterName = Constants.Names.Random();
            s.nGender = 0;
            s.nSkin = 0;
            s.nFace = 20000;
            s.nHair = 30000;
            s.nLevel = 10;
            s.nJob = 0;

            s.nSTR = 10;
            s.nDEX = 10;
            s.nINT = 10;
            s.nLUK = 10;
            s.nHP = 50;
            s.nMHP = 50;
            s.nMP = 50;
            s.nMMP = 50;
            s.nAP = 1;
            s.nSP = 1;
            s.nEXP = 0;
            s.nPOP = 0;
            s.dwPosMap = 100010000;//100000000;//180000000;
            s.nPortal = 1;
            s.nPlaytime = 0;
            s.nSubJob = 0;

            s.nMoney = 1337;

            var l = x.Look;
            l.nGender = 0;
            l.nSkin = 0;
            l.nFace = 20000;
            l.nWeaponStickerID = 0;
            l.anHairEquip[0] = 30000;

            l.anHairEquip[5] = 1041002;
            l.anHairEquip[6] = 1061002;
            l.anHairEquip[7] = 1072001;
            l.anHairEquip[11] = 1302000;
            
            return x;
        }
        
        public static AvatarData Create(string name,byte gender,byte skinColor,int face,int hair,short job,short subJob)
        {
            var x = new AvatarData();

            var s = x.Stats;
            s.dwCharacterID = Constants.GetUniqueId();
            s.sCharacterName = name;
            s.nGender = gender;
            s.nSkin = skinColor;
            s.nFace = face;
            s.nHair = hair;
            s.nLevel = 10;
            s.nJob = job;

            s.nSTR = 10;
            s.nDEX = 10;
            s.nINT = 10;
            s.nLUK = 10;
            s.nHP = 50;
            s.nMHP = 50;
            s.nMP = 50;
            s.nMMP = 50;
            s.nAP = 1;
            s.nSP = 1;
            s.nEXP = 0;
            s.nPOP = 0;
            s.dwPosMap = 100000000;
            s.nPortal = 1;
            s.nPlaytime = 0;
            s.nSubJob = subJob;

            var l = x.Look;
            l.nGender = gender;
            l.nSkin = skinColor;
            l.nFace = face;
            l.nWeaponStickerID = 0;
            l.anHairEquip[0] = hair;

            return x;
        }
        
        public GW_CharacterStat Stats { get; }
        public AvatarLook Look { get; }

        public AvatarData()
        {
            Stats = new GW_CharacterStat();
            Look = new AvatarLook();
        }
    }
}
