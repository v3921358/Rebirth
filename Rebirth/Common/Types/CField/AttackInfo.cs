using System;
using System.Collections.Generic;
using Common;
using Common.Log;
using Common.Packets;

namespace Common.Types.CField
{
    public struct AttackArea
    {
        public int nStart;
        public int nAreaCount;
        public int nAttackCount;
        public int nWidth;
        public SecRect rcAttack;
    }

    //One day....
    public class MobAttackInfo
    {
        public int nType;
        public int bInactive;
        public int nConMP;
        public int nPAD;
        public int bMagicAttack;
        public int nBulletNumber;
        public int nMagicElemAttr;
        public int bJumpAttack;

        //union $18DEE8667A60322B3DC3026574857C96
        //{
        public SecRect rcRange;
        public RangeBox tzRange;
        public AttackArea tzArea;
        //};

        public int nBulletSpeed;
        public int bDeadlyAttack;
        public int bTremble;
        //Ztl_bstr_t sEffect;
        //Ztl_bstr_t sHit;
        //Ztl_bstr_t sBall;
        //Ztl_bstr_t sAreaWarning;
        public int bHitAttach;
        public int tEffectAfter;
        public int tAttackAfter;
        public int bDoFirst;
        public int nMPBurn;
        public int bKnockBack;
        public int bFacingAttatch;
        public int tRandDelayAttack;
        public int bRush;
        public int bSpeicalAttack;
    }

    public class AttackInfo
    {
        //{ union $2515A21D497DCF65601599E4799020CF
        public int dwMobID;
        //CMob* pMob;
        //}

        public int nHitAction;
        public int nForeAction;
        public int nFrameIdx;
        public int tDelay;
        public int nAttackCount;
        public int[] aDamage;
        public int[] abCritical;
        public TagPoint ptHit;

        public AttackInfo()
        {
            aDamage = new int[15];
            abCritical = new int[15];
            ptHit = new TagPoint();
        }
    }

    public class MapleAttackNew
    {
        public byte nDamagePerMob { get; set; }
        public byte nMobCount { get; set; }
        public int nSkillID { get; set; }
        public int tKeyDown { get; set; } //"Charge" 
        public int bLeft { get; set; }
        public int nAction { get; set; }
        public int nAttackType { get; set; }
        public int tStart { get; set; }
        public int nSpeedDegree { get; set; }
        public int tAttackTime { get; set; }
        public AttackInfo[] aAttackInfo { get; set; }


        public byte nActionSpeed { get; set; }
        public byte nMastery { get; set; }
        public int nBulletItemID { get; set; }

        public byte tByte1 { get; set; }
        public short tByte2 { get; set; }
        public short tByte3 { get; set; }

        public byte nAttackActionType { get; set; }
        public byte nAttackSpeed { get; set; }

        public MapleAttackNew()
        {
            aAttackInfo = new AttackInfo[15];
        }

        //nType:
        //
        public static MapleAttackNew Parse(CInPacket p, int nType)
        {
            MapleAttackNew ret = new MapleAttackNew();

            p.Skip(8); // -1

            ret.tByte1 = p.Decode1();
            ret.nDamagePerMob = (byte)(ret.tByte1 & 0xF);
            ret.nMobCount = (byte)((ret.tByte1 >> 4) & 0xF);

            p.Skip(8); //-1

            var v11 = p.Decode4();
            ret.nSkillID = v11;
            p.Skip(1); // 0.94
            p.Skip(4); // 0.74
            p.Skip(4); // 0.74
            p.Skip(8); // 0.88 (0)

            //is_keydown_skill
            if (v11 == 2121001 || v11 == 2221001 || v11 == 2321001 || v11 == 3221001 || v11 == 3121004)
                ret.tKeyDown = p.Decode4();
            else
                ret.tKeyDown = -1;

            /*
            switch (ret.skill)
            {
                case 5101004: // Corkscrew
                case 15101003: // Cygnus corkscrew
                case 5201002: // Grenade
                case 14111006: // Poison bomb
                case 4341002: // Final Cut
                case 4341003: // Monster Bomb
                    ret.charge = p.Decode4();
                    break;
                default:
                    ret.charge = 0;
                    break;
            }
            */

            p.Decode1(); //  bFinalAfterSlashBlast | 8 * bShadowPartner | 16 * v674 | 32 * (nSerialAttackSkillID != 0) | ((_BYTE)v683 << 7));

            ret.tByte2 = p.Decode2();
            ret.bLeft = (ret.tByte2 >> 15) & 1;
            ret.nAction = ret.tByte2 & 0x7FFF;
            p.Skip(4); //CRC i think
            ret.nAttackActionType = p.Decode1();
            ret.nAttackSpeed = p.Decode1();

            ret.tAttackTime = p.Decode4();
            p.Skip(4); //bmage?

            //More decode for bullets here

            //if (ret.skill == 4211006)
            //{ // Meso Explosion
            //    return parseMesoExplosion(lea, ret);
            //}

            for (int i = 0; i < ret.nMobCount; i++)
            {
                var info = new AttackInfo();

                info.dwMobID = p.Decode4();

                //COutPacket::Encode1(&oPacket, v567->nHitAction);
                //todo fill the reste in
                p.Skip(14);

                for (int j = 0; j < ret.nDamagePerMob; j++)
                {
                    info.aDamage[j] = p.Decode4();

                    Logger.Write(LogLevel.Debug, "Attack Mob {0} Dmg {1}", info.dwMobID, info.aDamage[j]);
                }

                p.Skip(4); // CRC of monster [Wz Editing]

                ret.aAttackInfo[i] = info;
            }

            //ret.position = p.DecodePos();

            //if greneade read pos 

            return ret;
        }
    }
}
