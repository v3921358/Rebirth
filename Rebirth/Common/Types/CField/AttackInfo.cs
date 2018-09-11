using System;
using System.Collections.Generic;
using Common.Log;
using Common.Packets;

namespace Common.Types.CField
{
    public class AttackPair
    {
        public int MobId { get; }
        public List<Tuple<int, bool>> Attack { get; }

        public AttackPair(int mobId)
        {
            MobId = mobId;
            Attack = new List<Tuple<int, bool>>();
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
        public byte display;
        public int lastAttackTickCount;
        public List<AttackPair> allDamage;
        public TagPoint position;

        /// <summary>
        /// Not yet working 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static AttackInfo ParseMelee(CInPacket p)
        {
            AttackInfo ret = new AttackInfo();

            ret.portals = p.Decode1();

            bool unkk = p.Decode1() == 0xFF;
            p.Skip(unkk ? 7 : 8);

            ret.tbyte = p.Decode1();
            ret.targets = (byte)((ret.tbyte >> 4) & 0xF);
            ret.hits = (byte)(ret.tbyte & 0xF);

            p.Skip(8); //-1
            
            ret.skill = p.Decode4();
            p.Skip(1); // 0.94
            p.Skip(4); // 0.74
            p.Skip(4); // 0.74
            p.Skip(8); // 0.88 (0)

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
            
            ret.display = p.Decode1(); // Always zero?
            ret.direction = p.Decode1();
            ret.stance = p.Decode1();
            p.Skip(4);
            p.Skip(1); // Weapon class
            ret.speed = p.Decode1(); // Confirmed
            ret.lastAttackTickCount = p.Decode4(); // Ticks
            p.Skip(4); //0

            ret.allDamage = new List<AttackPair>();

            //if (ret.skill == 4211006)
            //{ // Meso Explosion
            //    return parseMesoExplosion(lea, ret);
            //}
            
            for (int i = 0; i < ret.targets; i++)
            {
                var mobId = p.Decode4();
                var atkPair = new AttackPair(mobId);

                p.Skip(14);

                for (int j = 0; j < ret.hits; j++)
                {
                    var damage = p.Decode4();
                    atkPair.Attack.Add(new Tuple<int, bool>(damage,false));

                    Logger.Write(LogLevel.Debug, "Attack Mob {0} Dmg {1}", mobId, damage);
                }

                p.Skip(4); // CRC of monster [Wz Editing]
                ret.allDamage.Add(atkPair);
            }

            ret.position = p.DecodePos();

            return ret;
        }
    }
}
