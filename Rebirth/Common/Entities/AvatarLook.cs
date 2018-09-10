using Common.Packets;

namespace Common.Entities
{    
    /// <summary>
    /// Mongo Class
    /// 
    /// Be cautiaus of all public members
    /// And initializing them in the ctor
    /// </summary>
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

            for (int i = 1; i < anHairEquip.Length; i++)
            {
                var value = anHairEquip[i];

                if (value != 0)
                {
                    p.Encode1((byte)i);
                    p.Encode4(value);
                }
            }
            p.Encode1(0xFF); //anHairEquip end

            for (int i = 0; i < anUnseenEquip.Length; i++)
            {
                var value = anUnseenEquip[i];

                if (value != 0)
                {
                    p.Encode1((byte)i);
                    p.Encode4(value);
                }
            }
            p.Encode1(0xFF); //anUnseenEquip end

            p.Encode4(nWeaponStickerID);//Cash Weapon

            foreach (int nPetId in anPetID) //3
                p.Encode4(nPetId);
        }
    }

}
