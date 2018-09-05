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
        public AvatarLook Look; //I added this myself
        public GW_CharacterStat Stats; //characterStat
        
        //ZRef<GW_ItemSlotBase> aEquipped[60];
        //ZRef<GW_ItemSlotBase> aEquipped2[60];
        //ZRef<GW_ItemSlotBase> aDragonEquipped[4];
        //ZRef<GW_ItemSlotBase> aMechanicEquipped[5];
        //ZArray<ZRef<GW_ItemSlotBase>> aaItemSlot[6];
        //_FILETIME aEquipExtExpire[1];
        //ZMap<long, EQUIPPED_SETITEM, long> m_mEquippedSetItem;
        public int nCombatOrders;
        //ZMap<long, long, long> mSkillRecord;
        //ZMap<long, long, long> mSkillRecordEx;
        //ZMap<long, long, long> mSkillMasterLev;
        //ZMap<long, _FILETIME, long> mSkillExpired;
        //ZMap<long, unsigned short, long> mSkillCooltime;
        //ZMap<unsigned short, _FILETIME, unsigned short> mQuestComplete;
        //ZMap<unsigned short, _FILETIME, unsigned short> mQuestCompleteOld;
        //ZMap<long, ZRef<GW_MiniGameRecord>, long> mMiniGameRecord;
        public int nFriendMax;
        //ZList<GW_CoupleRecord> lCoupleRecord;
        //ZList<GW_FriendRecord> lFriendRecord;
        //ZList<GW_NewYearCardRecord> lNewYearCardRecord;
        //ZList<GW_MarriageRecord> lMarriageRecord;
        public int[] adwMapTransfer;
        public int[] adwMapTransferEx;
        //int bReachMaxLevel;
        //_FILETIME ftReachMaxLevelTime;
        //int nItemTotalNumber[5];
        //ZMap<long, long, long> mAdminShopCommodityCount;
        public string sLinkedCharacter;
        //ZRef<GW_WildHunterInfo> pWildHunterInfo;
        //ZMap<long, ZRef<GW_MonsterBookCard>, long> mpMonsterBookCard;
        public int nMonsterBookCoverID;
        public int nMonsterCardNormal;
        public int nMonsterCardSpecial;
        //ZMap<unsigned short, ZXString<char>, unsigned short> mQuestRecord;
        //ZMap<unsigned short, CSimpleStrMap, unsigned short> mQuestRecordEx;
        //ZArray<Additional::SKILL> aSkill;
        //int aMobCategoryDamage[9];
        //int aElemBoost[8];
        //Additional::CRITICAL critical;
        //Additional::BOSS boss;
        //ZMap<ZXString<char>, ZPair<long, long>, ZXString<char>> aUpgradeCountByDamageTheme;
        //ZMap<long, long, long> m_mVisitorQuestLog;

        public CharacterData()
        {
            Stats = new GW_CharacterStat();

            adwMapTransfer = new int[5];
            adwMapTransferEx = new int[10];
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
            for (byte i = 1; i <= 5; i++)
            {
                p.Encode1(96);
                //mplew.write(chr.getInventory(MapleInventoryType.getByType(i)).getSlotLimit());
            }
            p.Encode8(-2);//getTime(-2));
            //MapleInventory iv = chr.getInventory(MapleInventoryType.EQUIPPED);
            //Collection<Item> equippedC = iv.list();
            //List<Item> equipped = new ArrayList<>(equippedC.size());
            //List<Item> equippedCash = new ArrayList<>(equippedC.size());
            //for (Item item : equippedC)
            //{
            //    if (item.getPosition() <= -100)
            //    {
            //        equippedCash.add((Item)item);
            //    }
            //    else
            //    {
            //        equipped.add((Item)item);
            //    }
            //}
            //Collections.sort(equipped);
            //for (Item item : equipped)
            //{
            //    addItemInfo(mplew, item);
            //}
            p.Encode2(0); // End Of Equipped
            //for (Item item : equippedCash)
            //{
            //    addItemInfo(mplew, item);
            //}
            p.Encode2(0); // End Of Equip Cash
            //for (Item item : chr.getInventory(MapleInventoryType.EQUIP).list())
            //{
            //    addItemInfo(mplew, item);
            //}
            p.Encode4(0);  // End of Equip 
            //for (Item item : chr.getInventory(MapleInventoryType.USE).list())
            //{
            //    addItemInfo(mplew, item);
            //}
            p.Encode1(0); // End of Use 
            //for (Item item : chr.getInventory(MapleInventoryType.SETUP).list())
            //{
            //    addItemInfo(mplew, item);
            //}
            p.Encode1(0); // End of Set Up
            //for (Item item : chr.getInventory(MapleInventoryType.ETC).list())
            //{
            //    addItemInfo(mplew, item);
            //}
            p.Encode1(0);  // End of Etc
            //for (Item item : chr.getInventory(MapleInventoryType.CASH).list())
            //{
            //    addItemInfo(mplew, item);
            //}
            p.Encode1(0); // End of Cash
        }
    }
}
