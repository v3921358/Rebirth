using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Client;
using Common.Network;
// ReSharper disable InconsistentNaming

namespace Common
{
    public static class CPacket
    {
        public static COutPacket CheckPasswordResult(int accId, byte gender, byte gmLevel, string accountName)
        {
            var p = new COutPacket();

            p.Encode2((short)SendOps.LP_CheckPasswordResult);
            p.Encode1(0); //v3 = CInPacket::Decode1(iPacket);
            p.Encode1(0); //sMsg._m_pStr[500] = CInPacket::Decode1(iPacket);
            p.Encode4(0); //CInPacket::Decode4(iPacket);

            p.Encode4(accId); // pBlockReason.m_pInterface = (IWzProperty *)CInPacket::Decode4(iPacket);
            p.Encode1(0); //pBlockReasonIter.m_pInterface = (IWzProperty *)(unsigned __int8)CInPacket::Decode1(iPacket);
            p.Encode1(0); //LOBYTE(nGradeCode) = CInPacket::Decode1(iPacket);
            p.Encode2(0); //v39 = (char *)CInPacket::Decode2(iPacket);
            p.Encode1(0); //LOBYTE(nCountryID) = CInPacket::Decode1(iPacket);
            p.EncodeString(accountName); //CInPacket::DecodeStr(iPacket, &sNexonClubID);
            p.Encode1(0); //LOBYTE(nPurchaseExp) = CInPacket::Decode1(iPacket);
            p.Encode1(0); //  LOBYTE(sMsg2._m_pStr) = CInPacket::Decode1(iPacket);
            p.Encode8(0); //CInPacket::DecodeBuffer(iPacket, &dtChatUnblockDate, 8u);
            p.Encode8(0); //CInPacket::DecodeBuffer(iPacket, &dtRegisterDate, 8u);
            p.Encode4(1); //nNumOfCharacter = CInPacket::Decode4(iPacket);
            p.Encode1(1); //v43 = (unsigned __int8)CInPacket::Decode1(iPacket)
            p.Encode1(0); //sMsg._m_pStr[432] = CInPacket::Decode1(iPacket);

            //I dont remember if this is extra padding or from the .idb
            p.Encode8(0);
            p.Encode8(0);
            p.Encode8(0);

            return p;
        }

        public static COutPacket WorldRequest(byte serverId)
        {
            var p = new COutPacket();

            //v6 = ZArray<CLogin::WORLDITEM>
            p.Encode2((short)SendOps.LP_WorldInformation);
            p.Encode1(serverId); //v4 [Server ID]  
            p.EncodeString("Server Name"); //WORLDITEM->sName
            p.Encode1(0); //v6->nWorldState
            p.EncodeString("Event Message?"); //sWorldEventDesc
            p.Encode2(100); //v6->nWorldEventEXP_WSE
            p.Encode2(100); //v6->nWorldEventDrop_WSE
            p.Encode1(0); //v6->nBlockCharCreation

            const byte channelCount = 1;

            p.Encode1(channelCount); //v9 ChannelCount

            for (int i = 0; i < channelCount; i++)
            {
                //v11 =  ZArray<CLogin::CHANNELITEM>
                p.EncodeString($"World {i + 1}");
                p.Encode4(200); //v11->nUserNo
                p.Encode1(serverId); //v11->nWorldID
                p.Encode1(0); //v11->nChannelID
                p.Encode1(0); //v11->bAdultChannel
            }

            p.Encode2(0); //v2->m_nBalloonCount 

            return p;
        }
        public static COutPacket WorldRequestEnd()
        {
            var p = new COutPacket();

            p.Encode2((short)SendOps.LP_WorldInformation);
            p.Encode1(0xff); //server id

            return p;
        }

        public static COutPacket SelectWorldResult(params Character[] chars)
        {
            var p = new COutPacket(SendOps.LP_SelectWorldResult);

            var charCount = (byte)chars.Length;

            p.Encode1(0);
            p.Encode1(charCount); //chars count

            foreach (var x in chars)
            {
                AddCharEntry(x, p);
            }

            p.Encode1(2); //m_bLoginOpt | spw request?

            //p.Encode8(3);
            p.Encode4(3); //m_nSlotCount
            p.Encode4(0); //m_nBuyCharCount | https://i.imgur.com/DMynDxG.png

            return p;
        }

        public static COutPacket CheckUserLimit(byte status)
        {
            var p = new COutPacket();

            p.Encode2((short)SendOps.LP_CheckUserLimitResult);

            /* 
             * 0 - Normal
             * 1 - Highly populated
             * 2 - Full
             */

            p.Encode1(0); //bOverUserLimit
            p.Encode1(status); //bPopulateLevel

            return p;
        }

        public static COutPacket CheckDuplicatedIDResult(string name, bool nameTaken)
        {
            var p = new COutPacket(SendOps.LP_CheckDuplicatedIDResult);
            p.EncodeString(name);
            p.Encode1(nameTaken);
            return p;
        }

        public static COutPacket CreateNewCharacter(string name, bool worked, Character c)
        {
            var p = new COutPacket(SendOps.LP_CreateNewCharacterResult);
            p.Encode1((byte)(worked ? 0 : 1));

            if (worked)
            {
                AddCharEntry(c, p);
            }

            return p;
        }

        public static COutPacket SelectCharacterResult(int uid)
        {
            var p = new COutPacket(SendOps.LP_SelectCharacterResult);

            p.Encode1(0); //v3 | World
            p.Encode1(0); //dwCharacterID | Selectec Char

            var ip = new byte[] { 127, 0, 0, 1 };
            p.EncodeBuffer(ip, 0, ip.Length);
            p.Encode2(Constants.GamePort);
            p.Encode4(uid);
            p.Encode1(0);
            p.Encode4(0);

            //v8 = CInPacket::Decode4(iPacket);
            //v9 = CInPacket::Decode2(iPacket);
            //v10 = CInPacket::Decode4(iPacket);
            //bAuthenCode = CInPacket::Decode1(iPacket);
            //v12 = CInPacket::Decode4(iPacket);
            //ZInetAddr::ZInetAddr((ZInetAddr*)&addrNet, v9);

            return p;
        }

        public static void AddCharEntry(Character c, COutPacket p)
        {
            //  const bool ranking = false;

            c.EncodeStats(p);
            c.EncodeLook(p);

            p.Encode1(0);
            p.Encode1(0); //ranking


            //if (ranking)
            //{
            //    mplew.writeInt(chr.getRank());
            //    mplew.writeInt(chr.getRankMove());
            //    mplew.writeInt(chr.getJobRank());
            //    mplew.writeInt(chr.getJobRankMove());
            //}

        }

        public static COutPacket SetField(Character c)
        {
            const bool bCharacterData = true;

            var p = new COutPacket(SendOps.LP_SetField);

            p.Encode4(0); //  *((_DWORD *)TSingleton<CWvsContext>::ms_pInstance._m_pStr + 2072)
            p.Encode4(0); //  *((_DWORD *)TSingleton<CWvsContext>::ms_pInstance._m_pStr + 3942)

            p.Encode1(0); //sNotifierMessage
            p.Encode1(bCharacterData); //bCharacterData

            p.Encode2(0); //Enables ChatBlockReason

            if (bCharacterData)
            {
                var seed1 = Constants.Rand.Next();
                var seed2 = Constants.Rand.Next();
                var seed3 = Constants.Rand.Next();

                p.Encode4(seed1);
                p.Encode4(seed2);
                p.Encode4(seed3);



                //    CharacterData::Decode((CharacterData *)(v11 + 16), iPacket, 0);
                long dbcharFlag = 1;


                p.Encode8(dbcharFlag); //dbcharFlag
                p.Encode1(0); //v3->nCombatOrders
                p.Encode1(0); //v5 some loop

                if ((dbcharFlag & 1) != 0)
                {
                    c.EncodeStats(p);
                    p.Encode1(10); //v3->nFriendMax
                    p.Encode1(0); // if ( CInPacket::Decode1(v4) )
                    /*
                        GW_CharacterStat::Decode(&v3->characterStat, v4, bBackwardUpdate);
                        v3->nFriendMax = (unsigned __int8)CInPacket::Decode1(v4);
                        if ( CInPacket::Decode1(v4) )
                        {
                          v14 = CInPacket::DecodeStr(v4, (ZXString<char> *)&result.p);
                          LOBYTE(iPacketa) = 1;
                          ZXString<char>::operator=(&v3->sLinkedCharacter, v14);
                          LOBYTE(iPacketa) = 0;
                          if ( result.p )
                            ZXString<char>::_Release((ZXString<char>::_ZXStringData *)((char *)&result.p[-1].liCashItemSN.QuadPart + 4));
                        }
                    */
                }

                if ((dbcharFlag & 2) != 0)
                    p.Encode4(100000);//GW_CharacterStat::DecodeMoney(&v3->characterStat, v4);



                //CWvsContext::OnSetLogoutGiftConfig(v10, iPacket);
            }
            else
            {

            }

            return p;
        }

        public static COutPacket SetFieldBeta(Character c)
        {
            var p = new COutPacket(SendOps.LP_SetField);
            
            CClientOptMan__EncodeOpt(p,2);

            //  *((_DWORD *)TSingleton<CWvsContext>::ms_pInstance._m_pStr + 2072) = CInPacket::Decode4(iPacket);
            //*((_DWORD*)TSingleton < CWvsContext >::ms_pInstance._m_pStr + 3942) = CInPacket::Decode4(iPacket);
            p.Encode4(1);//chr.getClient().getChannel() - 1);
            p.Encode4(0);

            const bool bCharacterData = true;

            //BYTE3(sNotifierMessage._m_pStr) = CInPacket::Decode1(iPacket);
            //bCharacterData = (unsigned __int8)CInPacket::Decode1(iPacket);
            //v4 = CInPacket::Decode2(iPacket);
            p.Encode1(1);
            p.Encode1(bCharacterData); //  bCharacterData 
            p.Encode2(0); //  nNotifierCheck

            if (bCharacterData)
            {
                var seed1 = Constants.Rand.Next();
                var seed2 = Constants.Rand.Next();
                var seed3 = Constants.Rand.Next();

                p.Encode4(seed1);
                p.Encode4(seed2);
                p.Encode4(seed3);

                AddCharacterData(p,c); //CharacterData::Decode

                //CWvsContextOnSetLogoutGiftConfig
                //
                p.Encode4(0); // Lucky Logout Gift packet. Received/do not show = 1; not received/show = 0
                p.Encode4(0); // SN 1
                p.Encode4(0); // SN 2
                p.Encode4(0); // SN 3
                //
            }
            p.Encode8(Environment.TickCount); //some other sort of time

            return p;
        }
        public static COutPacket SetField2(Character c, int mapId, byte spawnPoint)
        {
            var p = new COutPacket(SendOps.LP_SetField);

            CClientOptMan__EncodeOpt(p, 2);

            //  *((_DWORD *)TSingleton<CWvsContext>::ms_pInstance._m_pStr + 2072) = CInPacket::Decode4(iPacket);
            //*((_DWORD*)TSingleton < CWvsContext >::ms_pInstance._m_pStr + 3942) = CInPacket::Decode4(iPacket);
            p.Encode4(1);//chr.getClient().getChannel() - 1);
            p.Encode4(0);

            const bool bCharacterData = false;

            //BYTE3(sNotifierMessage._m_pStr) = CInPacket::Decode1(iPacket);
            //bCharacterData = (unsigned __int8)CInPacket::Decode1(iPacket);
            //v4 = CInPacket::Decode2(iPacket);
            p.Encode1(1);
            p.Encode1(bCharacterData); //  bCharacterData 
            p.Encode2(0); //  nNotifierCheck

            if (!bCharacterData)
            {
                p.Encode1(0); // Portal Count
                p.Encode1(0); // not connect packet
                p.Encode2(0); // Messages		
                p.Encode1(0); // revive stuffs?.
                p.Encode4(mapId);
                p.Encode1(spawnPoint);
                p.Encode2(c.StatCurHp);
                p.Encode1(0);
            }

            p.Encode8(Environment.TickCount);//(getTime(System.currentTimeMillis()));
            return p;
        }

        public static COutPacket SetFieldComplete(Character c,bool bCharacterData)
        {
            var p = new COutPacket(SendOps.LP_SetField);

            CClientOptMan__EncodeOpt(p, 2);

            //  *((_DWORD *)TSingleton<CWvsContext>::ms_pInstance._m_pStr + 2072) = CInPacket::Decode4(iPacket);
            //*((_DWORD*)TSingleton < CWvsContext >::ms_pInstance._m_pStr + 3942) = CInPacket::Decode4(iPacket);
            p.Encode4(1);//chr.getClient().getChannel() - 1);
            p.Encode4(0);
            
            //BYTE3(sNotifierMessage._m_pStr) = CInPacket::Decode1(iPacket);
            //bCharacterData = (unsigned __int8)CInPacket::Decode1(iPacket);
            //v4 = CInPacket::Decode2(iPacket);
            p.Encode1(1);
            p.Encode1(bCharacterData); //  bCharacterData 
            p.Encode2(0); //  nNotifierCheck

            if (bCharacterData)
            {
                var seed1 = Constants.Rand.Next();
                var seed2 = Constants.Rand.Next();
                var seed3 = Constants.Rand.Next();

                p.Encode4(seed1);
                p.Encode4(seed2);
                p.Encode4(seed3);

                AddCharacterData(p, c); //CharacterData::Decode

                //CWvsContextOnSetLogoutGiftConfig
                //
                p.Encode4(0); // Lucky Logout Gift packet. Received/do not show = 1; not received/show = 0
                p.Encode4(0); // SN 1
                p.Encode4(0); // SN 2
                p.Encode4(0); // SN 3
                //
            }
            else
            {
                p.Encode1(0); // Portal Count
                p.Encode1(0); // not connect packet
                p.Encode2(0); // Messages		
                p.Encode1(0); // revive stuffs?.
                p.Encode4(c.MapId);
                p.Encode1(c.MapSpawn);
                p.Encode2(c.StatCurHp);
                p.Encode1(0);
            }

            p.Encode8(Environment.TickCount); //some other sort of time

            return p;
        }

        private static void AddCharacterData(COutPacket p, Character c)
        {
            p.Encode8(-1); //the bit flag (all anwaays)
            p.Encode1(0); //v3->nCombatOrders
            p.Encode1(0); //some loop?

            c.EncodeStats(p);   //addCharStats(mplew, chr);
            p.Encode1(10);//chr.getBuddylist().getCapacity()

            p.Encode1(0); //chr.getLinkedName() == null
            //else
            //{
            //    mplew.write(1);
            //    mplew.writeMapleAsciiString(chr.getLinkedName());
            //}


            AddInventoryInfo(p, c);
            AddSkillInfo(p, c);
            AddQuestInfo(p, c);
            AddMiniGameInfo(p, c);
            AddRingInfo(p, c);
            AddTeleportInfo(p, c);
            AddMonsterBookInfo(p, c);
            AddNewYearInfo(p, c); // Short 
            AddAreaInfo(p, c); // Short 
            p.Encode2(0);
            p.Encode2(0);
        }
        private static void AddAreaInfo(COutPacket p, Character c)
        {
            p.Encode2(0);
        }
        private static void AddNewYearInfo(COutPacket p, Character c)
        {
            p.Encode2(0);
        }
        private static void AddMonsterBookInfo(COutPacket p, Character c)
        {
            p.Encode4(0);//mplew.writeInt(chr.getMonsterBookCover()); // cover
            p.Encode1(0);//mplew.write(0);
            //Map<Integer, Integer> cards = chr.getMonsterBook().getCards();
            p.Encode2(0);//mplew.writeShort(cards.size());
            //for (Entry<Integer, Integer> all : cards.entrySet())
            //{
            //    mplew.writeShort(all.getKey() % 10000); // Id
            //    mplew.write(all.getValue()); // Level
            //}
        }
        private static void AddTeleportInfo(COutPacket p, Character c)
        {
            for (int i = 0; i < 5; i++)
                p.Encode4(0);

            for (int i = 0; i < 10; i++)
                p.Encode4(0);
        }
        private static void AddRingInfo(COutPacket p, Character c)
        {
            p.Encode2(0); //getCrushRings
            p.Encode2(0); //getFriendshipRings
            p.Encode2(0); //getMarriageRing
        }
        private static void AddMiniGameInfo(COutPacket p, Character c)
        {
            p.Encode2(0);
        }
        private static void AddQuestInfo(COutPacket p, Character c)
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
        private static void AddSkillInfo(COutPacket p, Character c)
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
        private static void AddInventoryInfo(COutPacket p, Character c)
        {
            p.Encode4(50000); //Meso

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
        private static void CClientOptMan__EncodeOpt(COutPacket p,short optCount)
        {
            p.Encode2(optCount);

            for (int i = 0; i < optCount; i++)
                p.Encode8(i + 1);

            //dwType = CInPacket::Decode4(v3);
            //iPacket = (CInPacket*)CInPacket::Decode4(v3);
        }
    }
}
