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
        public static COutPacket SetField(CharacterData c, bool bCharacterData)
        {
            var p = new COutPacket(SendOps.LP_SetField);

            CClientOptMan__EncodeOpt(p, 2);

            //  *((_DWORD *)TSingleton<CWvsContext>::ms_pInstance._m_pStr + 2072) = CInPacket::Decode4(iPacket);
            //*((_DWORD*)TSingleton < CWvsContext >::ms_pInstance._m_pStr + 3942) = CInPacket::Decode4(iPacket);
            p.Encode4(1);//chr.getClient().getChannel() - 1);
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
        public static COutPacket MobLeaveField(CMob mob,byte nDeadType)
        {
            var p = new COutPacket(SendOps.LP_MobEnterField);
            p.Encode4(mob.dwMobId);
            p.Encode1(nDeadType); // 0 = dissapear, 1 = fade out, 2+ = special

            if (nDeadType == 4)
                p.Encode4(-1); //m_dwSwallowCharacterID

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
