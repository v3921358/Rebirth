using System;
using Common.Client;
using Common.Entities;
using Common.Game;
using Common.Network;

// ReSharper disable InconsistentNaming

namespace Common.Packets
{
    public static class CPacket
    {
        //WvsLogin----------------------------------------------------------------------------------------------------
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
        public static COutPacket WorldRequest(byte nWorldID,string sName)
        {
            var p = new COutPacket();

            //v6 = ZArray<CLogin::WORLDITEM>
            p.Encode2((short)SendOps.LP_WorldInformation);
            p.Encode1(nWorldID); //v4 [Server ID]  
            p.EncodeString(sName); //WORLDITEM->sName
            p.Encode1(0); //v6->nWorldState
            p.EncodeString("Event Message?"); //sWorldEventDesc
            p.Encode2(100); //v6->nWorldEventEXP_WSE
            p.Encode2(100); //v6->nWorldEventDrop_WSE
            p.Encode1(0); //v6->nBlockCharCreation

            const byte nChannelNo = 1;

            p.Encode1(nChannelNo); //v9

            for (byte i = 0; i < nChannelNo; i++)
            {
                //v11 =  ZArray<CLogin::CHANNELITEM>
                p.EncodeString($"{sName}-{i}");
                p.Encode4(200); //v11->nUserNo
                p.Encode1(nWorldID); //v11->nWorldID
                p.Encode1(i); //v11->nChannelID
                p.Encode1(0); //v11->bAdultChannel
            }

            p.Encode2(0); //v2->m_nBalloonCount 

            return p;
        }
        public static COutPacket WorldRequestEnd()
        {
            var p = new COutPacket(SendOps.LP_WorldInformation);
            p.Encode1(0xFF); //nWorldID
            return p;
        }
        public static COutPacket LatestConnectedWorld(byte nWorldID)
        {
            var p = new COutPacket(SendOps.LP_LatestConnectedWorld);
            p.Encode4(nWorldID);
            return p;
        }
        public static COutPacket SelectWorldResult(params AvatarData[] chars)
        {
            var p = new COutPacket(SendOps.LP_SelectWorldResult);

            var charCount = (byte)chars.Length;

            p.Encode1(0);
            p.Encode1(charCount); //chars count

            foreach (var x in chars)
            {
                AddCharEntry(p, x);
            }

            p.Encode1(2); //m_bLoginOpt | spw request?

            //p.Encode8(3);
            p.Encode4(3); //m_nSlotCount
            p.Encode4(0); //m_nBuyCharCount | https://i.imgur.com/DMynDxG.png

            return p;
        }
        public static COutPacket CheckUserLimit(byte status)
        {
            var p = new COutPacket(SendOps.LP_CheckUserLimitResult);

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
        public static COutPacket CreateNewCharacter(string name, bool worked, AvatarData c)
        {
            var p = new COutPacket(SendOps.LP_CreateNewCharacterResult);
            p.Encode1((byte)(worked ? 0 : 1));

            if (worked)
            {
                AddCharEntry(p, c);
            }

            return p;
        }

        public static COutPacket DeleteCharacter(int uid, byte result)
        {
            var p = new COutPacket(SendOps.LP_DeleteCharacterResult);
            p.Encode4(uid);

            // 6 : Trouble logging in? Try logging in again from maplestory.nexon.net.
            // 9 : Failed due to unknown reason.
            // 10 : Could not be processed due to too many connection requests to the server. Please try again later.
            // 18 : The 8-digit birthday code you have entered is incorrect.
            // 20 : You have entered an incorrect PIC.
            // 22 : Cannot delete Guild Master character.
            // 24 : You may not delete a character that has been engaged or booked for a wedding.
            // 26 : You cannot delete a character that is currently going through the transfer.
            // 29 : You may not delete a character that has a family.

            p.Encode1(result);
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
     
        //WvsGame-----------------------------------------------------------------------------------------------------
        public static COutPacket SetField(CharacterData c, bool bCharacterData, int nChannel)
        {
            var p = new COutPacket(SendOps.LP_SetField);

            CClientOptMan__EncodeOpt(p, 2);

            //  *((_DWORD *)TSingleton<CWvsContext>::ms_pInstance._m_pStr + 2072) = CInPacket::Decode4(iPacket);
            //*((_DWORD*)TSingleton < CWvsContext >::ms_pInstance._m_pStr + 3942) = CInPacket::Decode4(iPacket);
            p.Encode4(nChannel);//chr.getClient().getChannel() - 1);
            p.Encode4(0);

            p.Encode1(1); //sNotifierMessage._m_pStr
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

                c.Encode(p); //CharacterData::Decode

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
                p.Encode1(0); // Revival Shit
                p.Encode4(c.Stats.dwPosMap);
                p.Encode1(c.Stats.nPortal);
                p.Encode4(c.Stats.nHP);
                p.Encode1(0); //Enables two Encode4's
            }

            p.Encode8(Environment.TickCount); //Odin GameTime(-2)

            return p;
        }

        public static COutPacket UserChat(int uid, string text, bool gm, bool visable)
        {
            var p = new COutPacket(SendOps.LP_UserChat);
            p.Encode4(uid);
            p.Encode1(gm);
            p.EncodeString(text);
            p.Encode1(visable);
            return p;
        }
        public static COutPacket UserMovement(int uid, byte[] movePath)
        {
            var p = new COutPacket(SendOps.LP_UserMove);
            p.Encode4(uid);
            p.EncodeBuffer(movePath, 0, movePath.Length);
            return p;
        }

        public static COutPacket UserEnterField(CharacterData c)
        {
            var p = new COutPacket(SendOps.LP_UserEnterField);
            p.Encode4(c.Stats.dwCharacterID);

            // CUserRemote::Init(v12, iPacket, v13, 1);
            p.Encode1(c.Stats.nLevel);
            p.EncodeString(c.Stats.sCharacterName);

            //if (c.getGuildId() < 1)
            {
                p.Skip(8); //p.EncodeString("");
            }

            //TODO:  SecondaryStat::DecodeForRemote(&v4->m_secondaryStat, &result, iPacket);
            //
            p.Encode8(0); p.Encode8(0);
            p.Encode1(0); p.Encode1(0);
            //

            p.Encode2(c.Stats.nJob); //v4->m_nJobCode = CInPacket::Decode2(iPacket);
            c.Look.Encode(p); //AvatarLook::AvatarLook(&v87, iPacket);

            p.Encode4(0); //  v4->m_dwDriverID
            p.Encode4(0); //  v4->m_dwPassenserID
            p.Encode4(0); //  nChocoCount
            p.Encode4(0); //  nActiveEffectItemID
            p.Encode4(0); //  v4->m_nCompletedSetItemID
            p.Encode4(0); //  v4->m_nPortableChairID

            p.Encode2(0); //x and bPrivate ?

            p.Encode2(0); //m_pStr
            p.Encode1(c.Position.Stance); //v4->m_nMoveAction
            p.Encode2(c.Position.Foothold); //dwSN ( Foothold? )
            p.Encode1(0); //bShowAdminEffect

            p.Encode1(0); //Some loop [PETS I THINK]

            p.Encode4(0); //m_nTamingMobLevel
            p.Encode4(0); //m_nTamingMobExp
            p.Encode4(0); //m_nTamingMobFatigue

            p.Encode1(0); //m_nMiniRoomType (Flag)

            p.Encode1(0); //v4->m_bADBoardRemote ( If true write a string )
            p.Encode1(0); //CUserPool::OnCoupleRecordAdd loop flag
            p.Encode1(0); //CUserPool::OnFriendRecordAdd loop flag
            p.Encode1(0); //CUserPool::OnMarriageRecordAdd

            //Dark Force, Dragon, Swallowing Effect?
            byte someLoadingBitflag = 0;

            p.Encode1(someLoadingBitflag);

            p.Encode1(0); //CUserPool::OnNewYearCardRecordAdd loop
            p.Encode4(0); //m_nPhase

            return p;
        }
        public static COutPacket UserLeaveField(CharacterData c)
        {
            var p = new COutPacket(SendOps.LP_UserLeaveField);
            p.Encode4(c.Stats.dwCharacterID);
            return p;
        }

        public static COutPacket UserEmoticon(int uid, int nEmotion, int nDuration, byte bByItemOption)
        {
            var p = new COutPacket(SendOps.LP_UserEmotion);
            p.Encode4(uid);
            p.Encode4(nEmotion); //nEmoticon
            p.Encode4(nDuration); //tDuration
            p.Encode1(bByItemOption); //CUser->m_bEmotionByItemOption
            return p;
        }

        //WvsGame::MobPool--------------------------------------------------------------------------------------------
        public static COutPacket MobEnterField(CMob mob)
        {
            var p = new COutPacket(SendOps.LP_MobEnterField);
            mob.EncodeInitData(p);
            return p;
        }
        public static COutPacket MobLeaveField(CMob mob, byte nDeadType)
        {
            var p = new COutPacket(SendOps.LP_MobEnterField);
            p.Encode4(mob.dwMobId);
            p.Encode1(nDeadType); // 0 = dissapear, 1 = fade out, 2+ = special

            if (nDeadType == 4)
                p.Encode4(-1); //m_dwSwallowCharacterID

            return p;
        }
        public static COutPacket MobChangeController(CMob mob, byte nLevel)
        {
            var p = new COutPacket(SendOps.LP_MobChangeController);
            p.Encode1(nLevel); // 1 = remove i think

            if (nLevel == 0)
            {
                p.Encode4(mob.dwMobId);
            }
            else
            {
                mob.EncodeInitData(p);
            }

            return p;
        }
        public static COutPacket MobMoveAck(int dwMobId, short nMobCtrlSN, bool bMobMoveStartResult, short nMP, byte nSkillCommand, byte nSLV)
        {
            var p = new COutPacket(SendOps.LP_MobCtrlAck);
            p.Encode4(dwMobId);
            p.Encode2(nMobCtrlSN);
            p.Encode1(bMobMoveStartResult);
            p.Encode2(nMP); //CMob->nMP lol
            p.Encode1(nSkillCommand);
            p.Encode1(nSLV);
            return p;
        }
        public static COutPacket MobMove(int dwMobId, bool bMobMoveStartResult, byte pCurSplit, int bIllegealVelocity, byte[] movePath)
        {
            var p = new COutPacket(SendOps.LP_MobMove);
            p.Encode4(dwMobId); 

            //Section 1 - BMS / PDB Combined Version ?
            p.Encode1(bMobMoveStartResult); //bNotForceLandingWhenDiscard
            p.Encode1(pCurSplit);
            p.Encode1(0);
            p.Encode1(0);
            p.Encode4(bIllegealVelocity);

            //Section 1 - Mordred Version
            //packet.Encode2(actionAndDirection);
            //packet.Encode1(sourceMob.isNextAttackPossible());
            //packet.Encode1(mobSkillActionMaybe);//needs confirmation
            //packet.Encode4(skillData);

            p.Encode4(0); //multiTargetForBall LOOP
            p.Encode4(0); //randTimeForAreaAttack LOOP

            p.EncodeBuffer(movePath, 0, movePath.Length);
            return p;

            //BMS v???
            //COutPacket::COutPacket((COutPacket*)&oPacketMove, 151, 0);
            //v39 = (int*)v9->m_dwId;
            //LOBYTE(v63) = 1;
            //COutPacket::Encode4((COutPacket*)&oPacketMove, (unsigned int)v39);

            //COutPacket::Encode1((COutPacket*)&oPacketMove, bMobMoveStartResult);
            //COutPacket::Encode1((COutPacket*)&oPacketMove, (char)pCurSplit);
            //COutPacket::Encode4((COutPacket*)&oPacketMove, bIllegealVelocity);
            //CMovePath::CMovePath((CMovePath*)&mp);
            //LOBYTE(v63) = 2;
            //CMovePath::Decode((CMovePath*)&mp, iPacket);
            //CMovePath::Encode((CMovePath*)&mp, (COutPacket*)&oPacketMove);
        }

        //WvsGame::NpcPool--------------------------------------------------------------------------------------------
        public static COutPacket NpcEnterField(CNpc npc)
        {
            var p = new COutPacket(SendOps.LP_NpcEnterField);
            p.Encode4(npc.dwNpcId);
            p.Encode4(npc.Id);

            //CNpc::Init
            p.Encode2((short)npc.X); //m_ptPosPrev.x
            p.Encode2((short)npc.Cy); //m_ptPosPrev.y

            p.Encode1(0);// & 1 | 2 * 2); //m_nMoveAction | life.getF() == 1 ? 0 : 1
            p.Encode2((short)npc.Foothold); //dwSN Foothold

            p.Encode2((short)npc.Rx0); //m_ptPosPrev.x
            p.Encode2((short)npc.Rx1); //m_ptPosPrev.y

            p.Encode1(true); //I hope this works lol | //mplew.write((show && !life.isHidden()) ? 1 : 0);

            return p;
        }
        public static COutPacket NpcScriptMessage(int npc, byte msgType, String talk, String endBytes, byte type, int OtherNPC)
        {
            var p = new COutPacket(SendOps.LP_ScriptMessage);

            //CScriptMan::OnScriptMessage
            
            p.Encode1(4);
            p.Encode4(npc);
            p.Encode1(msgType); //NpcDialogOptions | todo make this
            p.Encode1(type); // 1 = No ESC, 3 = show character + no sec

            if (type >= 4 && type <= 5)
            {
                p.Encode4(OtherNPC);
            }

            p.EncodeString(talk);

            if (!string.IsNullOrWhiteSpace(endBytes))
            {
                var extra = Constants.GetBytes(endBytes);
                p.EncodeBuffer(extra, 0, extra.Length);
            }

            return p;
        }
    
        //--------------------------------------------------------------------------------------------
        public static COutPacket BroadcastPinkMsg(string msg)
        {
            return BroadcastMsg(5, msg);
        }
        public static COutPacket BroadcastServerMsg(string msg)
        {
            return BroadcastMsg(4, msg);
        }
        public static COutPacket BroadcastPopupMsg(string msg)
        {
            return BroadcastMsg(1, msg);
        }
        private static COutPacket BroadcastMsg(byte nType,string message)
        {
            var p = new COutPacket(SendOps.LP_BroadcastMsg);
            p.Encode1(nType);

            // 0: [Notice] <Msg>
            // 1: Popup <Msg>
            // 2: Megaphone
            // 3: Super Megaphone 
            // 4: Server Message
            // 5: Pink Text
            // 6: LightBlue Text ({} as Item)
            // 7: [int] -> Keep Wz Error
            // 8: Item Megaphone
            // 9: Item Megaphone
            // 10: Three Line Megaphone
            // 11: Weather Effect
            // 12: Green Gachapon Message
            // 13: Yellow Twin Dragon's Egg
            // 14: Green Twin Dragon's Egg
            // 15: Lightblue Text
            // 16: Lightblue Text
            // 18: LightBlue Text ({} as Item)
            // 20: (Red Message) : Skull?
            
            if (nType == 4)
            { 
                p.Encode1(true); // Server Message
            }

            p.EncodeString(message);

            //switch (nType)
            //{
            //    case 3: // Super Megaphone
            //    case 20: // Skull Megaphone
            //        mplew.write(channel - 1);
            //        mplew.write(whisper ? 1 : 0);
            //        break;
            //    case 9: // Like Item Megaphone (Without Item)
            //        mplew.write(channel - 1);
            //        break;
            //    case 11: // Weather Effect
            //        mplew.writeInt(channel); // item id
            //        break;
            //    case 13: // Yellow Twin Dragon's Egg
            //    case 14: // Green Twin Dragon's Egg
            //        mplew.writeMapleAsciiString("NULL"); // Name
            //        PacketHelper.addItemInfo(mplew, null, true, true);
            //        break;
            //    case 6:
            //    case 18:
            //        mplew.writeInt(channel >= 1000000 && channel < 6000000 ? channel : 0); // Item Id
            //        //E.G. All new EXP coupon {Ruby EXP Coupon} is now available in the Cash Shop!
            //        break;
            //}

            return p;
        }
     
        //WvsCommon---------------------------------------------------------------------------------------------------
        private static void AddCharEntry(COutPacket p, AvatarData c)
        {
            const bool ranking = false;

            c.Stats.Encode(p);
            c.Look.Encode(p);

            p.Encode1(0); //VAC
            p.Encode1(ranking); //ranking

            if (ranking)
            {
                p.Skip(16);
            }
        }
        private static void CClientOptMan__EncodeOpt(COutPacket p, short optCount)
        {
            p.Encode2(optCount);

            for (int i = 0; i < optCount; i++)
            {
                p.Encode8(i + 1);
                //dwType = CInPacket::Decode4(v3);
                //iPacket = (CInPacket*)CInPacket::Decode4(v3);
            }
        }
        public static void MobStat__EncodeTemporary(COutPacket p, long dwFlag1, long dwFlag2, int tCur)
        {
            p.Encode8(dwFlag1);
            p.Encode8(dwFlag2);

            //Code below is old from BMS when flag was just an int

            /*
              v4 = this;
              dwToSend = 0;
              if ( dwFlag & 1 && this->nPAD_ )
                dwToSend = 1;
              if ( dwFlag & 2 && this->nPDD_ )
                dwToSend |= 2u;
              if ( dwFlag & 4 && this->nMAD_ )
                dwToSend |= 4u;
              if ( dwFlag & 8 && this->nMDD_ )
                dwToSend |= 8u;
              if ( dwFlag & 0x10 && this->nACC_ )
                dwToSend |= 0x10u;
              if ( dwFlag & 0x20 && this->nEVA_ )
                dwToSend |= 0x20u;
              if ( dwFlag & 0x40 && this->nSpeed_ )
                dwToSend |= 0x40u;
              if ( dwFlag & 0x80 && this->nStun_ )
                LOBYTE(dwToSend) = dwToSend | 0x80;
              if ( dwFlag & 0x100 && this->nFreeze_ )
                dwToSend |= 0x100u;
              if ( dwFlag & 0x200 && this->nPoison_ )
                dwToSend |= 0x200u;
              if ( dwFlag & 0x400 && this->nSeal_ )
                dwToSend |= 0x400u;
              if ( dwFlag & 0x800 && this->nDarkness_ )
                dwToSend |= 0x800u;
              if ( dwFlag & 0x1000 && this->nPowerUp_ )
                dwToSend |= 0x1000u;
              if ( dwFlag & 0x2000 && this->nMagicUp_ )
                dwToSend |= 0x2000u;
              if ( dwFlag & 0x4000 && this->nPGuardUp_ )
                dwToSend |= 0x4000u;
              if ( dwFlag & 0x8000 && this->nMGuardUp_ )
                dwToSend |= 0x8000u;
              if ( dwFlag & 0x40000 && this->nPImmune_ )
                dwToSend |= 0x40000u;
              if ( dwFlag & 0x80000 && this->nMImmune_ )
                dwToSend |= 0x80000u;
              if ( dwFlag & 0x10000 && this->nDoom_ )
                dwToSend |= 0x10000u;
              if ( dwFlag & 0x20000 && this->nWeb_ )
                dwToSend |= 0x20000u;
              if ( dwFlag & 0x200000 && this->nHardSkin_ )
                dwToSend |= 0x200000u;
              if ( dwFlag & 0x400000 && this->nAmbush_ )
                dwToSend |= 0x400000u;
              if ( dwFlag & 0x1000000 && this->nVenom_ )
                dwToSend |= 0x1000000u;
              if ( dwFlag & 0x2000000 && this->nBlind_ )
                dwToSend |= 0x2000000u;
              if ( dwFlag & 0x4000000 && this->nSealSkill_ )
                dwToSend |= 0x4000000u;
              COutPacket::Encode4(oPacket, dwToSend);
              if ( dwToSend & 1 )
              {
                COutPacket::Encode2(oPacket, v4->nPAD_);
                COutPacket::Encode4(oPacket, v4->rPAD_);
                COutPacket::Encode2(oPacket, (v4->tPAD_ - tCur) / 500);
              }
              if ( dwToSend & 2 )
              {
                COutPacket::Encode2(oPacket, v4->nPDD_);
                COutPacket::Encode4(oPacket, v4->rPDD_);
                COutPacket::Encode2(oPacket, (v4->tPDD_ - tCur) / 500);
              }
              if ( dwToSend & 4 )
              {
                COutPacket::Encode2(oPacket, v4->nMAD_);
                COutPacket::Encode4(oPacket, v4->rMAD_);
                COutPacket::Encode2(oPacket, (v4->tMAD_ - tCur) / 500);
              }
              if ( dwToSend & 8 )
              {
                COutPacket::Encode2(oPacket, v4->nMDD_);
                COutPacket::Encode4(oPacket, v4->rMDD_);
                COutPacket::Encode2(oPacket, (v4->tMDD_ - tCur) / 500);
              }
              if ( dwToSend & 0x10 )
              {
                COutPacket::Encode2(oPacket, v4->nACC_);
                COutPacket::Encode4(oPacket, v4->rACC_);
                COutPacket::Encode2(oPacket, (v4->tACC_ - tCur) / 500);
              }
              if ( dwToSend & 0x20 )
              {
                COutPacket::Encode2(oPacket, v4->nEVA_);
                COutPacket::Encode4(oPacket, v4->rEVA_);
                COutPacket::Encode2(oPacket, (v4->tEVA_ - tCur) / 500);
              }
              if ( dwToSend & 0x40 )
              {
                COutPacket::Encode2(oPacket, v4->nSpeed_);
                COutPacket::Encode4(oPacket, v4->rSpeed_);
                COutPacket::Encode2(oPacket, (v4->tSpeed_ - tCur) / 500);
              }
              if ( dwToSend & 0x80 )
              {
                COutPacket::Encode2(oPacket, v4->nStun_);
                COutPacket::Encode4(oPacket, v4->rStun_);
                COutPacket::Encode2(oPacket, (v4->tStun_ - tCur) / 500);
              }
              if ( BYTE1(dwToSend) & 1 )
              {
                COutPacket::Encode2(oPacket, v4->nFreeze_);
                COutPacket::Encode4(oPacket, v4->rFreeze_);
                COutPacket::Encode2(oPacket, (v4->tFreeze_ - tCur) / 500);
              }
              if ( BYTE1(dwToSend) & 2 )
              {
                COutPacket::Encode2(oPacket, v4->nPoison_);
                COutPacket::Encode4(oPacket, v4->rPoison_);
                COutPacket::Encode2(oPacket, (v4->tPoison_ - tCur) / 500);
              }
              if ( BYTE1(dwToSend) & 4 )
              {
                COutPacket::Encode2(oPacket, v4->nSeal_);
                COutPacket::Encode4(oPacket, v4->rSeal_);
                COutPacket::Encode2(oPacket, (v4->tSeal_ - tCur) / 500);
              }
              if ( BYTE1(dwToSend) & 8 )
              {
                COutPacket::Encode2(oPacket, v4->nDarkness_);
                COutPacket::Encode4(oPacket, v4->rDarkness_);
                COutPacket::Encode2(oPacket, (v4->tDarkness_ - tCur) / 500);
              }
              if ( BYTE1(dwToSend) & 0x10 )
              {
                COutPacket::Encode2(oPacket, v4->nPowerUp_);
                COutPacket::Encode4(oPacket, v4->rPowerUp_);
                COutPacket::Encode2(oPacket, (v4->tPowerUp_ - tCur) / 500);
              }
              if ( BYTE1(dwToSend) & 0x20 )
              {
                COutPacket::Encode2(oPacket, v4->nMagicUp_);
                COutPacket::Encode4(oPacket, v4->rMagicUp_);
                COutPacket::Encode2(oPacket, (v4->tMagicUp_ - tCur) / 500);
              }
              if ( BYTE1(dwToSend) & 0x40 )
              {
                COutPacket::Encode2(oPacket, v4->nPGuardUp_);
                COutPacket::Encode4(oPacket, v4->rPGuardUp_);
                COutPacket::Encode2(oPacket, (v4->tPGuardUp_ - tCur) / 500);
              }
              if ( BYTE1(dwToSend) & 0x80 )
              {
                COutPacket::Encode2(oPacket, v4->nMGuardUp_);
                COutPacket::Encode4(oPacket, v4->rMGuardUp_);
                COutPacket::Encode2(oPacket, (v4->tMGuardUp_ - tCur) / 500);
              }
              if ( BYTE2(dwToSend) & 4 )
              {
                COutPacket::Encode2(oPacket, v4->nPImmune_);
                COutPacket::Encode4(oPacket, v4->rPImmune_);
                COutPacket::Encode2(oPacket, (v4->tPImmune_ - tCur) / 500);
              }
              if ( BYTE2(dwToSend) & 8 )
              {
                COutPacket::Encode2(oPacket, v4->nMImmune_);
                COutPacket::Encode4(oPacket, v4->rMImmune_);
                COutPacket::Encode2(oPacket, (v4->tMImmune_ - tCur) / 500);
              }
              if ( BYTE2(dwToSend) & 1 )
              {
                COutPacket::Encode2(oPacket, v4->nDoom_);
                COutPacket::Encode4(oPacket, v4->rDoom_);
                COutPacket::Encode2(oPacket, (v4->tDoom_ - tCur) / 500);
              }
              if ( BYTE2(dwToSend) & 2 )
              {
                COutPacket::Encode2(oPacket, v4->nWeb_);
                COutPacket::Encode4(oPacket, v4->rWeb_);
                COutPacket::Encode2(oPacket, (v4->tWeb_ - tCur) / 500);
              }
              if ( BYTE2(dwToSend) & 0x20 )
              {
                COutPacket::Encode2(oPacket, v4->nHardSkin_);
                COutPacket::Encode4(oPacket, v4->rHardSkin_);
                COutPacket::Encode2(oPacket, (v4->tHardSkin_ - tCur) / 500);
              }
              if ( BYTE2(dwToSend) & 0x40 )
              {
                COutPacket::Encode2(oPacket, v4->nAmbush_);
                COutPacket::Encode4(oPacket, v4->rAmbush_);
                COutPacket::Encode2(oPacket, (v4->tAmbush_ - tCur) / 500);
              }
              if ( BYTE3(dwToSend) & 1 )
              {
                COutPacket::Encode2(oPacket, v4->nVenom_);
                COutPacket::Encode4(oPacket, v4->rVenom_);
                COutPacket::Encode2(oPacket, (v4->tVenom_ - tCur) / 500);
              }
              if ( BYTE3(dwToSend) & 2 )
              {
                COutPacket::Encode2(oPacket, v4->nBlind_);
                COutPacket::Encode4(oPacket, v4->rBlind_);
                COutPacket::Encode2(oPacket, (v4->tBlind_ - tCur) / 500);
              }
              if ( BYTE3(dwToSend) & 4 )
              {
                COutPacket::Encode2(oPacket, v4->nSealSkill_);
                COutPacket::Encode4(oPacket, v4->rSealSkill_);
                COutPacket::Encode2(oPacket, (v4->tSealSkill_ - tCur) / 500);
              }
             */

        }
    }
}
