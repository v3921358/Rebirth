using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Packets;

namespace Common.Entities
{
    public sealed class CharacterData
    {
        //This stuff i added myself
        public AvatarLook Look { get; set; }
        public MapPos Position { get; set; }

        //Below are copy pasted from Maple

        public GW_CharacterStat Stats { get; set; } //characterStat

        public GW_ItemSlotEquip[] aEquipped { get; } //60
        public GW_ItemSlotEquip[] aEquipped2 { get; } //60

        //ZRef<GW_ItemSlotBase> aEquipped[60];
        //ZRef<GW_ItemSlotBase> aEquipped2[60];
        //ZRef<GW_ItemSlotBase> aDragonEquipped[4];
        //ZRef<GW_ItemSlotBase> aMechanicEquipped[5];
        //ZArray<ZRef<GW_ItemSlotBase>> aaItemSlot[6];

        //_FILETIME aEquipExtExpire[1];
        //ZMap<long, EQUIPPED_SETITEM, long> m_mEquippedSetItem;
        public int nCombatOrders { get; set; }
        //ZMap<long, long, long> mSkillRecord;
        //ZMap<long, long, long> mSkillRecordEx;
        //ZMap<long, long, long> mSkillMasterLev;
        //ZMap<long, _FILETIME, long> mSkillExpired;
        //ZMap<long, unsigned short, long> mSkillCooltime;
        //ZMap<unsigned short, _FILETIME, unsigned short> mQuestComplete;
        //ZMap<unsigned short, _FILETIME, unsigned short> mQuestCompleteOld;
        //ZMap<long, ZRef<GW_MiniGameRecord>, long> mMiniGameRecord;
        public int nFriendMax { get; set; }
        //ZList<GW_CoupleRecord> lCoupleRecord;
        //ZList<GW_FriendRecord> lFriendRecord;
        //ZList<GW_NewYearCardRecord> lNewYearCardRecord;
        //ZList<GW_MarriageRecord> lMarriageRecord;
        public int[] adwMapTransfer { get; private set; }
        public int[] adwMapTransferEx { get; private set; }
        //int bReachMaxLevel;
        //_FILETIME ftReachMaxLevelTime;
        //int nItemTotalNumber[5];
        //ZMap<long, long, long> mAdminShopCommodityCount;
        public string sLinkedCharacter { get; set; }
        //ZRef<GW_WildHunterInfo> pWildHunterInfo;
        //ZMap<long, ZRef<GW_MonsterBookCard>, long> mpMonsterBookCard;
        public int nMonsterBookCoverID { get; set; }
        public int nMonsterCardNormal { get; set; }
        public int nMonsterCardSpecial { get; set; }
        //ZMap<unsigned short, ZXString<char>, unsigned short> mQuestRecord;
        //ZMap<unsigned short, CSimpleStrMap, unsigned short> mQuestRecordEx;
        //ZArray<Additional::SKILL> aSkill;
        //int aMobCategoryDamage[9];
        //int aElemBoost[8];
        //Additional::CRITICAL critical;
        //Additional::BOSS boss;
        //ZMap<ZXString<char>, ZPair<long, long>, ZXString<char>> aUpgradeCountByDamageTheme;
        //ZMap<long, long, long> m_mVisitorQuestLog;

        private CharacterData()
        {
            // Stats = new GW_CharacterStat();

            adwMapTransfer = new int[5];
            adwMapTransferEx = new int[10];

            aEquipped = new GW_ItemSlotEquip[60];
            aEquipped2 = new GW_ItemSlotEquip[60];
        }

        public static CharacterData Create(GW_CharacterStat stats, AvatarLook look)
        {
            var x = new CharacterData();

            x.Stats = stats;
            x.Look = look;
            x.Position = new MapPos();

            return x;
        }

        //CharacterData::Decode
        public void Encode(COutPacket p)
        {
            const long dbcharFlag = -1;

            p.Encode8(dbcharFlag);
            p.Encode1((byte)nCombatOrders);
            p.Encode1(0); //Some Loop.

            //if ((dbcharFlag & 1) > 0)
            //{
            Stats.Encode(p);   //addCharStats(mplew, chr);
            p.Encode1((byte)nFriendMax);//chr.getBuddylist().getCapacity()

            bool linkedChar = false;//string.IsNullOrWhiteSpace(sLinkedCharacter);

            p.Encode1(linkedChar);

            if (linkedChar)
            {
                p.EncodeString(sLinkedCharacter);
            }
            //}

            //if ((dbcharFlag & 2) > 0)
            //{
            Stats.EncodeMoney(p);
            //}

            AddInventoryInfo(p);
            AddSkillInfo(p);
            AddQuestInfo(p);
            AddMiniGameInfo(p);
            AddRingInfo(p);
            AddTeleportInfo(p);
            AddMonsterBookInfo(p);
            AddNewYearInfo(p); // Short 
            AddAreaInfo(p); // Short 
            p.Encode2(0);
            p.Encode2(0); //m_mVisitorQuestLog
        }

        private void AddAreaInfo(COutPacket p)
        {
            p.Encode2(0);
        }
        private void AddNewYearInfo(COutPacket p)
        {
            p.Encode2(0);
        }
        private void AddMonsterBookInfo(COutPacket p)
        {
            p.Encode4(nMonsterBookCoverID);//mplew.writeInt(chr.getMonsterBookCover()); // cover
            p.Encode1(0);//mplew.write(0);
            //Map<Integer, Integer> cards = chr.getMonsterBook().getCards();
            p.Encode2(0);//mplew.writeShort(cards.size());
            //for (Entry<Integer, Integer> all : cards.entrySet())
            //{
            //    mplew.writeShort(all.getKey() % 10000); // Id
            //    mplew.write(all.getValue()); // Level
            //}
        }
        private void AddTeleportInfo(COutPacket p)
        {
            for (int i = 0; i < adwMapTransfer.Length; i++)
                p.Encode4(adwMapTransfer[i]);

            for (int i = 0; i < adwMapTransferEx.Length; i++)
                p.Encode4(adwMapTransferEx[i]);
        }
        private void AddRingInfo(COutPacket p)
        {
            p.Encode2(0); //getCrushRings
            p.Encode2(0); //getFriendshipRings
            p.Encode2(0); //getMarriageRing
        }
        private void AddMiniGameInfo(COutPacket p)
        {
            p.Encode2(0);
        }
        private void AddQuestInfo(COutPacket p)
        {
            p.Encode2(0);//mplew.writeShort(chr.getStartedQuestsSize());
            //for (MapleQuestStatus q : chr.getStartedQuests())
            //{
            //    mplew.writeShort(q.getQuest().getId());
            //    mplew.writeMapleAsciiString(q.getQuestData());
            //    if (q.getQuest().getInfoNumber() > 0)
            //    {
            //        mplew.writeShort(q.getQuest().getInfoNumber());
            //        mplew.writeMapleAsciiString(q.getQuestData());
            //    }
            //}
            //List<MapleQuestStatus> completed = chr.getCompletedQuests();
            //mplew.writeShort(completed.size());
            p.Encode2(0);//for (MapleQuestStatus q : completed)
            //{
            //    mplew.writeShort(q.getQuest().getId());
            //    mplew.writeLong(getTime(q.getCompletionTime()));
            //}
        }
        private void AddSkillInfo(COutPacket p)
        {
            //Map<Skill, MapleCharacter.SkillEntry> skills = chr.getSkills();
            //int skillsSize = skills.size();
            //// We don't want to include any hidden skill in this, so subtract them from the size list and ignore them.
            //for (Iterator<Entry<Skill, SkillEntry>> it = skills.entrySet().iterator(); it.hasNext();)
            //{
            //    Entry<Skill, MapleCharacter.SkillEntry> skill = it.next();
            //    if (GameConstants.isHiddenSkills(skill.getKey().getId()))
            //    {
            //        skillsSize--;
            //    }
            //}
            p.Encode2(0);//mplew.writeShort(skillsSize);
            //for (Iterator<Entry<Skill, SkillEntry>> it = skills.entrySet().iterator(); it.hasNext();)
            //{
            //    Entry<Skill, MapleCharacter.SkillEntry> skill = it.next();
            //    if (GameConstants.isHiddenSkills(skill.getKey().getId()))
            //    {
            //        continue;
            //    }
            //    mplew.writeInt(skill.getKey().getId());
            //    mplew.writeInt(skill.getValue().skillevel);
            //    addExpirationTime(mplew, skill.getValue().expiration);
            //    if (skill.getKey().isFourthJob())
            //    {
            //        mplew.writeInt(skill.getValue().masterlevel);
            //    }
            //}
            p.Encode2(0);//mplew.writeShort(chr.getAllCooldowns().size());
            //for (PlayerCoolDownValueHolder cooling : chr.getAllCooldowns())
            //{
            //    mplew.writeInt(cooling.skillId);
            //    int timeLeft = (int)(cooling.length + cooling.startTime - System.currentTimeMillis());
            //    mplew.writeShort(timeLeft / 1000);
            //}
        }
        private void AddInventoryInfo(COutPacket p)
        {
            //EQUIP, CONSUME, INSTALL, ETC, CASH

            for (byte i = 1; i <= 5; i++)
            {
                p.Encode1(96);
                //mplew.write(chr.getInventory(MapleInventoryType.getByType(i)).getSlotLimit());
            }

            p.Encode8(Constants.SomeFileTime);//getTime(-2)); | EQUIPEXTEXPIRE 

            p.Encode2(0); //equippedNormal
            p.Encode2(0); //equippedCash
            p.Encode2(0); //equipTab
            p.Encode2(0); //equipExt
            p.Encode2(0); //????

            //

            p.Encode1(1);
            var v1 = new GW_ItemSlotBundle();
            v1.nItemID = 2000007;
            v1.nNumber = 5;
            v1.RawEncode(p);

            p.Encode1(2);
            var v2 = new GW_ItemSlotBundle();
            v2.nItemID = 2000010;
            v2.nNumber = 500;
            v2.RawEncode(p);

            p.Encode1(0); //use | CONSUME

            //

            p.Encode1(0); //setup | INSTALL
            p.Encode1(0); //etc | ETC
            p.Encode1(0); //cash | CASH

        }
    }

    /* 466 */
    enum DBCHAR_FLAGS
    {
        DBCHAR_CHARACTER = 0x1,
        DBCHAR_MONEY = 0x2,
        DBCHAR_ITEMSLOTEQUIP = 0x4,
        DBCHAR_ITEMSLOTCONSUME = 0x8,
        DBCHAR_ITEMSLOTINSTALL = 0x10,
        DBCHAR_ITEMSLOTETC = 0x20,
        DBCHAR_ITEMSLOTCASH = 0x40,
        DBCHAR_INVENTORYSIZE = 0x80,
        DBCHAR_SKILLRECORD = 0x100,
        DBCHAR_QUESTRECORD = 0x200,
        DBCHAR_MINIGAMERECORD = 0x400,
        DBCHAR_COUPLERECORD = 0x800,
        DBCHAR_MAPTRANSFER = 0x1000,
        DBCHAR_AVATAR = 0x2000,
        DBCHAR_QUESTCOMPLETE = 0x4000,
        DBCHAR_SKILLCOOLTIME = 0x8000,
        DBCHAR_MONSTERBOOKCARD = 0x10000,
        DBCHAR_MONSTERBOOKCOVER = 0x20000,
        DBCHAR_NEWYEARCARD = 0x40000,
        DBCHAR_QUESTRECORDEX = 0x80000,
        DBCHAR_ADMINSHOPCOUNT = 0x100000,
        DBCHAR_EQUIPEXT = 0x100000,
        DBCHAR_WILDHUNTERINFO = 0x200000,
        DBCHAR_QUESTCOMPLETE_OLD = 0x400000,
        DBCHAR_VISITORLOG = 0x800000,
        DBCHAR_VISITORLOG1 = 0x1000000,
        DBCHAR_VISITORLOG2 = 0x2000000,
        DBCHAR_VISITORLOG3 = 0x4000000,
        DBCHAR_VISITORLOG4 = 0x8000000,
        DBCHAR_ALL = 0xFFFFFFFF,
        DBCHAR_ITEMSLOT = 0x7C,
    };
}
