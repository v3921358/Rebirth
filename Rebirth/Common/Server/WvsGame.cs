using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Common.Client;
using Common.Game;
using Common.Log;
using Common.Network;
using Common.Packets;
using Common.Scripts.Npc;

namespace Common.Server
{
    public class WvsGame : ServerBase<WvsGameClient>
    {
        //-----------------------------------------------------------------------------
        public byte ChannelId { get; }

        public Dictionary<int, CField> CFieldMan { get; }
        //-----------------------------------------------------------------------------
        public WvsGame(WvsCenter parent, byte channel) : base($"WvsGame{channel}", Constants.GamePort + channel, parent)
        {
            ChannelId = channel;
            CFieldMan = new Dictionary<int, CField>();
        }
        //-----------------------------------------------------------------------------
        public CField GetField(int id)
        {
            if (!CFieldMan.ContainsKey(id))
            {
                var field = CField.Load(id, ParentServer);

                CFieldMan.Add(id, field);
            }

            return CFieldMan[id];
        }
        //-----------------------------------------------------------------------------
        protected override WvsGameClient CreateClient(CClientSocket socket)
        {
            return new WvsGameClient(this, socket)
            {
                ChannelId = ChannelId,
            };
        }

        protected override void HandlePacket(WvsGameClient socket, CInPacket packet)
        {
            base.HandlePacket(socket, packet);
            var opcode = (RecvOps)packet.Decode2();

            if (socket.Initialized)
            {
                switch (opcode)
                {
                    case RecvOps.CP_UserChat:
                        Handle_UserChat(socket, packet);
                        break;
                    case RecvOps.CP_UserMove:
                        Handle_UserMove(socket, packet);
                        break;
                    case RecvOps.CP_UserTransferFieldRequest:
                        Handle_UserTransferFieldRequest(socket, packet);
                        break;
                    case RecvOps.CP_UserEmotion:
                        Handle_UserEmotion(socket, packet);
                        break;
                    case RecvOps.CP_UserHit:
                        //Handle_UserHit(socket, packet);
                        break;
                    case RecvOps.CP_UserSelectNpc:
                        Handle_UserSelectNpc(socket, packet);
                        break;
                    case RecvOps.CP_UserScriptMessageAnswer:
                        Handle_UserScriptMessageAnswer(socket, packet);
                        break;
                    case RecvOps.CP_UserCharacterInfoRequest:
                        Handle_UserCharacterInfoRequest(socket, packet);
                        break;
                        
                    case RecvOps.CP_MobMove:
                        Handle_MobMove(socket, packet);
                        break;
                }
            }
            else
            {
                switch (opcode)
                {
                    case RecvOps.CP_MigrateIn:
                        Handle_MigrateIn(socket, packet);
                        break;
                    case RecvOps.CP_AliveAck:
                        break;
                    default:
                        //An invalid packet was sent before the client migrated in
                        socket.Disconnect();
                        break;
                }
            }
        }
        protected override void HandleDisconnect(WvsGameClient client)
        {
            base.HandleDisconnect(client);

            client.SentCharData = false;

            if (client.Initialized)
            {
                client.GetCharField().Remove(client);
            }
        }
        //-----------------------------------------------------------------------------
        private void Handle_MigrateIn(WvsGameClient c, CInPacket p)
        {
            var uid = p.Decode4();

            c.LoadCharacter(uid);

            var character = c.Character;
            character.Stats.dwCharacterID = Constants.GetUniqueId(); //AGAIN

            GetField(character.Stats.dwPosMap).Add(c);

            c.SendPacket(CPacket.BroadcastServerMsg(Constants.ServerMessage));
        }
        private void Handle_UserChat(WvsGameClient c, CInPacket p)
        {
            var tick = p.Decode4();
            var msg = p.DecodeString();
            var show = p.Decode1() != 0;

            if (msg.StartsWith("!"))
            {
                var split = msg.Split(' ');
                c.HandleCommand(split);
            }
            else
            {
                var stats = c.Character.Stats;
                c.GetCharField().Broadcast(CPacket.UserChat(stats.dwCharacterID, msg, true, show));
            }
        }
        private void Handle_UserMove(WvsGameClient c, CInPacket p)
        {
            var v1 = p.Decode8();
            var portalCount = p.Decode1(); //CField::GetFieldKey(v20);
            var v2 = p.Decode8();
            var mapCrc = p.Decode4();
            var dwKey = p.Decode4();
            var dwKeyCrc = p.Decode4();

            var movePath = p.DecodeBuffer(p.Available);
            c.Character.Position.DecodeMovePath(movePath);

            var stats = c.Character.Stats;

            c.GetCharField().Broadcast(CPacket.UserMovement(stats.dwCharacterID, movePath), c);
        }
        private void Handle_UserTransferFieldRequest(WvsGameClient c, CInPacket p)
        {
            if (p.Available == 0)
            {
                //Cash Shop Related
                return;
            }

            //TODO: Portal count checks
            //TODO: XY rect checks
            //TODO: Keep track if player spawns when entering a field

            var portalCount = p.Decode1(); //CField::GetFieldKey(v20);
            var destination = p.Decode4(); //
            var portalName = p.DecodeString();
            var x = p.Decode2();
            var y = p.Decode2();
            //var extra = p.DecodeBuffer(3); idk | prem | chase

            var portal =
                c.GetCharField()
                    .Portals
                    .GetByName(portalName);

            if (portal == null)
            {
                Logger.Write(LogLevel.Warning, "Client tried to enter non existant portal {0}", portalName);
            }
            else
            {
                c.UsePortal(portal);
            }
        }
        private void Handle_UserEmotion(WvsGameClient c, CInPacket p)
        {
            var nEmotion = p.Decode4();
            var nDuration = p.Decode4();
            var bByItemOption = p.Decode1();

            //if (emote > 7)
            //{
            //    int emoteid = 5159992 + emote;
            //    //TODO: As if i care check if the emote is in CS inventory, if not return
            //}

            var stats = c.Character.Stats;

            c.GetCharField().Broadcast(CPacket.UserEmoticon(stats.dwCharacterID, nEmotion, nDuration, bByItemOption), c);
        }
        private void Handle_UserHit(WvsGameClient c, CInPacket p)
        {
            const sbyte bump_damage = -1;
            const sbyte map_damage = -2;

            var tick = p.Decode4();
            var type = (sbyte)p.Decode1(); //-4 is mist, -3 and -2 are map damage.
            var unk = p.Decode1(); // Element - 0x00 = elementless, 0x01 = ice, 0x02 = fire, 0x03 = lightning
            int damage = p.Decode4();


            //bool damage_applied = false;
            //bool deadly_attack = false;
            //uint8_t hit = 0;
            //uint8_t stance = 0;
            //game_mob_skill_id disease = 0;
            //game_mob_skill_level level = 0;
            //game_health mp_burn = 0;
            //game_map_object map_mob_id = 0;
            //game_mob_id mob_id = 0;
            //game_skill_id no_damage_id = 0;
            //return_damage_data pgmr;


            var field = c.GetCharField();

            if (type != map_damage)
            {
                var mob_id = p.Decode4(); //mob template id
                var map_mob_id = p.Decode4();

                var mob = field.Mobs.Get(map_mob_id);

                //Look at me guys, sanitary checks!
                if (mob?.dwTemplateId != mob_id)
                {
                    return;
                }

                //if (type != bump_damage)
                //{
                //    if (mob == null)
                //    {
                //        // TODO FIXME: Restructure so the attack works fine even if the mob dies?
                //        return;
                //    }

                //    auto attack = channel_server::get_instance().get_mob_data_provider().get_mob_attack(mob->get_mob_id_or_link(), type);
                //    if (attack == nullptr)
                //    {
                //        // Hacking
                //        return;
                //    }
                //    disease = attack->disease;
                //    level = attack->level;
                //    mp_burn = attack->mp_burn;
                //    deadly_attack = attack->deadly_attack;
                //}

                var direction = p.Decode1();
            }
            

        }
        private void Handle_UserSelectNpc(WvsGameClient c, CInPacket p)
        {
            var dwNpcId = p.Decode4();
            var nPosX = p.Decode2();
            var nPosY = p.Decode2();

            if (c.NpcScript != null)
            {
                Logger.Write(LogLevel.Warning, "Npc script already in progress?");
            }

            var field = c.GetCharField();
            var npc = field.Npcs.Get(dwNpcId);

            if (npc != null)
            {
                //if (npc.hasShop())
                //{
                //    chr.setConversation(1);
                //    npc.sendShop(c);
                //}
                //else
                //{
                //    NPCScriptManager.getInstance().start(c, npc.getId());
                //}5


                c.NpcScript = Constants.GetScript(npc.Id, c);
                c.NpcScript.Execute();
            }
            else
            {
                Logger.Write(LogLevel.Warning, "Unable to find NPC {0}", dwNpcId);
            }
        }
        private void Handle_UserScriptMessageAnswer(WvsGameClient c, CInPacket p)
        {
            //CScriptSysFunc::OnScriptMessageAnswer

            var npc = c.NpcScript;

            if (npc == null)
            {
                Logger.Write(LogLevel.Warning, "UserScriptMessageAnswer with NO CONTEXT");
                return;
            }

            //Previous send dialog type
            var type = (NpcDialogOptions)p.Decode1();

            //if (type != npc->get_sent_dialog())
            //{
            //    // Hacking
            //    return;
            //}

            switch (type)
            {
                case NpcDialogOptions.quiz:
                case NpcDialogOptions.question:
                    {
                        var txt = p.DecodeString();
                        npc.proceed_text(txt);
                        npc.check_end();
                        return;
                    }
            }

            var choice = p.Decode1();

            switch (type)
            {
                case NpcDialogOptions.normal:
                    {
                        switch (choice)
                        {
                            case 0:
                                npc.proceed_back();
                                break;
                            case 1:
                                npc.proceed_next();
                                break;
                            default:
                                npc.end();
                                break;
                        }
                        break;
                    }
                case NpcDialogOptions.yes_no:
                case NpcDialogOptions.accept_decline:
                case NpcDialogOptions.accept_decline_no_exit:
                    {
                        switch (choice)
                        {
                            case 0:
                                npc.proceed_selection(0);
                                break;
                            case 1:
                                npc.proceed_selection(1);
                                break;
                            default:
                                npc.end();
                                break;
                        }
                        break;
                    }
                case NpcDialogOptions.get_text:
                    {
                        if (choice != 0)
                        {
                            var txt = p.DecodeString();
                            npc.proceed_text(txt);
                        }
                        else
                        {
                            npc.end();
                        }
                        break;
                    }
                case NpcDialogOptions.get_number:
                    {
                        if (choice == 1)
                        {
                            var num = p.Decode4();
                            npc.proceed_number(num);
                        }
                        else
                        {
                            npc.end();
                        }
                        break;
                    }
                case NpcDialogOptions.simple:
                    {
                        if (choice == 0)
                        {
                            npc.end();
                        }
                        else
                        {
                            var selection = p.Decode1();
                            npc.proceed_selection(selection);
                        }
                        break;
                    }
                case NpcDialogOptions.style:
                    {
                        if (choice == 1)
                        {
                            var selection = p.Decode1();
                            npc.proceed_selection(selection);
                        }
                        else
                        {
                            npc.end();
                        }
                        break;
                    }
                default:
                    {
                        npc.end();
                        break;
                    }
            }

            npc.check_end();
        }
        private void Handle_MobMove(WvsGameClient c, CInPacket p)
        {
            int dwMobId = p.Decode4();

            var nMobCtrlSN = p.Decode2();
            var v7 = p.Decode1(); //v85 = nDistance | 4 * (v184 | 2 * ((unsigned __int8)retaddr | 2 * v72)); [ CONFIRMED ]

            var pOldSplit = (v7 & 0xF0) != 0; //this is a type of CFieldSplit
            var bMobMoveStartResult = (v7 & 0xF) != 0;

            var pCurSplit = p.Decode1();
            var bIllegealVelocity = p.Decode4();
            var v8 = p.Decode1();

            var bCheatedRandom = (v8 & 0xF0) != 0;
            var bCheatedCtrlMove = (v8 & 0xF) != 0;

            p.Decode4(); //Loopy Decode 1
            p.Decode4(); //Loopy Decode 2

            p.DecodeBuffer(16);

            var movePath = p.DecodeBuffer(p.Available);

            //if (pMob->m_pController->pUser != pCtrl
            //    && (!pOldSplit
            //        || pMob->m_bNextAttackPossible
            //        || !CLifePool::ChangeMobController(&v5->m_lifePool, pCtrl->m_dwCharacterID, pMob, 1)))
            //{
            //    CMob::SendChangeControllerPacket(v9, v10, 0);
            //    return;
            //}

            c.SendPacket(CPacket.MobMoveAck(dwMobId, nMobCtrlSN, bMobMoveStartResult, 0, 0, 0));

            var mobMove = CPacket.MobMove(dwMobId, bMobMoveStartResult, pCurSplit, bIllegealVelocity, movePath);
            c.GetCharField().Broadcast(mobMove, c);

        }


        private void Handle_UserCharacterInfoRequest(WvsGameClient c, CInPacket p)
        {
            var tick = p.Decode4();
            var uid = p.Decode4();



        }


    }
}