/*
namespace Common.Entities
{
 public class AvatarData
 {
     public static AvatarData Create(int accId, int charId,string name,byte gender,byte skinColor,int face,int hair,short job,short subJob)
     {
         var x = new AvatarData(accId, charId);

         var s = x.Stats;
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

     public int AccId { get; set; }
     public int CharId { get; set; }
     public GW_CharacterStat Stats { get; set; }
     public AvatarLook Look { get; set; }

     //public AvatarData()
     //{
     //    Stats = new GW_CharacterStat();
     //    Look = new AvatarLook();
     //}

     public AvatarData(int accId, int charId)
     {
         AccId = accId;
         CharId = charId;

         Stats = new GW_CharacterStat(accId, charId);
         Look = new AvatarLook(accId, charId);
     }
 }
}
*/