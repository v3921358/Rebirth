using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Common.Packets;

namespace Common.Types.CField
{
    public class AttackPair
    {

        public int objectid;
        public List<Tuple<int, bool>> attack;

        public AttackPair(int objectid, List<Tuple<int, bool>> attack)
        {
            this.objectid = objectid;
            this.attack = attack;
        }
    }

    public class AttackInfo
    {
        public byte portals;
        public byte tbyte;
        public byte hits;
        public byte targets;
        public int skill;
        public int charge;
        public byte unk;
        public byte direction;
        public byte stance;
        public byte speed;
        public int lastAttackTickCount;
        public List<AttackPair> allDamage;

        public TagPoint position;

        /// <summary>
        /// Not yet working 
        /// </summary>
        /// <param name="lea"></param>
        /// <returns></returns>
        public static AttackInfo ParseMelee(CInPacket lea)
        {
            AttackInfo ret = new AttackInfo();

            ret.portals = lea.Decode1();
            bool unkk = lea.Decode1() == 0xFF;
            lea.Skip(unkk ? 7 : 8);
            ret.tbyte = lea.Decode1();
            //System.out.println("TBYTE: " + tbyte);
            ret.targets = (byte)((ret.tbyte >> 4) & 0xF);
            ret.hits = (byte)(ret.tbyte & 0xF);
            lea.Skip(8);


            ret.skill = lea.Decode4();
            lea.Skip(1); // 0.94
            lea.Skip(4); // 0.74
            lea.Skip(4); // 0.74
            lea.Skip(8); // 0.88

            switch (ret.skill)
            {
                case 5101004: // Corkscrew
                case 15101003: // Cygnus corkscrew
                case 5201002: // Grenade
                case 14111006: // Poison bomb
                case 4341002: // Final Cut
                case 4341003: // Monster Bomb
                    ret.charge = lea.Decode4();
                    break;
                default:
                    ret.charge = 0;
                    break;
            }
            
            ret.direction = lea.Decode1(); // Always zero?
            ret.stance = lea.Decode1();
            lea.Skip(4);
            lea.Skip(1); // Weapon class
            ret.speed = lea.Decode1(); // Confirmed
            ret.lastAttackTickCount = lea.Decode4(); // Ticks
            lea.Skip(4); //0

            ret.allDamage = new List<AttackPair>();

            //if (ret.skill == 4211006)
            //{ // Meso Explosion
            //    return parseMesoExplosion(lea, ret);
            //}

            //good bottom up so far

            for (int i = 0; i < ret.targets; i++)
            {
                var oid = lea.Decode4();
                //	    System.out.println(tools.HexTool.toString(lea.read(14)));
                lea.Skip(14); // [1] Always 6?, [3] unk, [4] Pos1, [4] Pos2, [2] seems to change randomly for some attack

                var allDamageNumbers = new List<Tuple<int, bool>>();

                for (int j = 0; j < ret.hits; j++)
                {
                    var damage = lea.Decode4();
                    Logger.Write(LogLevel.Debug, "Attack Mob {0} Dmg {1}", oid, damage);
                    // System.out.println("Damage: " + damage);
                    allDamageNumbers.Add(new Tuple<int, bool>(damage,false));
                }
                lea.Skip(4); // CRC of monster [Wz Editing]
                ret.allDamage.Add(new AttackPair(oid, allDamageNumbers));
            }
            ret.position = lea.DecodePos();
            return ret;
        }
    }
}
