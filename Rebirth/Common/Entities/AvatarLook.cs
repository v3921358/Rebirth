using Common.Packets;

namespace Common.Entities
{
    public class AvatarLook
    {
        public AvatarLook()
        {
            anHairEquip = new int[60];
            anUnseenEquip = new int[60];
            anPetID = new int[3];
        }

        public byte nGender;
        public int nSkin;
        public int nFace;
        public int nWeaponStickerID;
        public int[] anHairEquip;
        public int[] anUnseenEquip;
        public int[] anPetID;

        public void Encode(COutPacket p)
        {
            p.Encode1(nGender);
            p.Encode1((byte)nSkin);
            p.Encode4(nFace);
            p.Encode1(false);
            p.Encode4(anHairEquip[0]);
            
            p.Encode1(0xFF); //anHairEquip end
            p.Encode1(0xFF); //anUnseenEquip end

            p.Encode4(nWeaponStickerID);//Cash Weapon

            for (int i = 0; i < anPetID.Length; i++)
                p.Encode4(anPetID[i]);
        }
    }

}
