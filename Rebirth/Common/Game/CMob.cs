using Common.Entities;
using Common.Packets;
using Common.Types;

namespace Common.Game
{
    public class CMob
    {
        public int dwMobId { get; set; } //uid i think
        public int dwTemplateId { get; set; } //mob id
        
        public MapPos Position { get; }
        public int Controller { get; set; } //Char id or zero
        
        //WOAH
        public int CurHp { get; set; } = 5;

        public CMob(int mobId)
        {
            dwTemplateId = mobId;
            Position = new MapPos();
        }

        public void EncodeInitData(COutPacket p)
        {
            p.Encode4(dwMobId);
            p.Encode1(5); //  nCalcDamageIndex | Controller
            p.Encode4(dwTemplateId);

            //CMob::SetTemporaryStat
            CPacket.MobStat__EncodeTemporary(p, 0, 0, 0);

            //CMob::Init
            p.Encode2(Position.Position.X); //m_ptPosPrev.x
            p.Encode2(Position.Position.Y); //m_ptPosPrev.y
            p.Encode1(0 & 1 | 2 * 2);//mob.Position.Stance); //m_nMoveAction
            p.Encode2(0); //  m_nFootholdSN
            p.Encode2(Position.Foothold); //  m_nHomeFoothold

            var m_nSummonType = unchecked((byte)-2);
            p.Encode1(m_nSummonType); //m_nSummonType

            //if(m_nSummonType == -3 || m_nSummonType >= 0)
            //   p.Encode4(0); //m_dwSummonOption

            p.Encode1(0); //m_nTeamForMCarnival
            p.Encode4(0); //nEffectItemID
            p.Encode4(0); //m_nPhase
        }
    }
}
