using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Client;
using Common.Entities;
using Common.Log;
using Common.Network;
using Common.Packets;
using Common.Server;
using Common.Types.CLogin;
using MongoDB.Driver;

namespace WvsRebirth
{
    public class WvsLogin : ServerBase<WvsLoginClient>
    {
        //-----------------------------------------------------------------------------

        //Needs to be WvsCenter cuz when u migrate to WvsGame the login thinks
        //u logged out
        private readonly Dictionary<WvsLoginClient, Account> m_loginPool;

        //-----------------------------------------------------------------------------
        public WvsLogin(WvsCenter parent) : base("WvsLogin", Constants.LoginPort, parent)
        {
            m_loginPool = new Dictionary<WvsLoginClient, Account>();
        }
        //-----------------------------------------------------------------------------
        public bool IsUsernameTaken(string userName)
        {
            return Db
                .GetCollection<CharacterData>("character_data")
                .FindSync(x => x.Stats.sCharacterName == userName)
                .Any();
        }
        public int FetchNewCharId()
        {
            var tmp = Db
                 .GetCollection<CharacterData>("character_data")
                 .FindSync(x => x.CharId > 0)
                 .ToList();

            if (tmp.Count != 0)
                return tmp.Max(x => x.CharId) + 1;

            return 10000;
        }
        public void AddNewChar(CharacterData avatar)
        {
            Db.GetCollection<CharacterData>("character_data")
                .InsertOne(avatar);
        }
        public byte Login(WvsLoginClient c, string user, string pass)
        {
            //TODO: string any case compare this shit later

            var tmp = ParentServer.Db.Get()
                .GetCollection<Account>("account")
                .FindSync(x => x.Username == user)
                .FirstOrDefault();

            if (tmp == null)
                return 5; //Not a registered id

            if (tmp.Password != pass)
                return 4;

            foreach (var kvp in m_loginPool)
            {
                var cmp = kvp.Value;

                if (cmp.Username == user)
                    return 7; //Already logged in

            }

            var p = ParentServer.LoggedIn.FirstOrDefault(x => x.Account.Username == user);

            if (p != null)
            {
                if (p.Migrated)
                    return 7;

                if ((DateTime.Now - p.Requested).TotalSeconds >= Constants.MigrateTimeoutSec)
                {
                    ParentServer.LoggedIn.Remove(p);
                }
            }

            m_loginPool.Add(c, tmp);
            c.Account = tmp;

            return 0; //success


        }
        //-----------------------------------------------------------------------------
        protected override WvsLoginClient CreateClient(CClientSocket socket)
        {
            return new WvsLoginClient(this, socket);
        }
        protected override void HandlePacket(WvsLoginClient socket, CInPacket packet)
        {
            //base.HandlePacket(socket, packet);
            var opcode = (RecvOps)packet.Decode2();

            if (socket.LoggedIn)
            {
                switch (opcode)
                {
                    case RecvOps.CP_WorldRequest:
                    case RecvOps.CP_WorldInfoRequest:
                        Handle_WorldRequest(socket, packet);
                        break;
                    case RecvOps.CP_CheckUserLimit:
                        Handle_CheckUserLimit(socket, packet);
                        break;
                    case RecvOps.CP_SelectWorld:
                        Handle_SelectWorld(socket, packet);
                        break;
                    case RecvOps.CP_CheckDuplicatedID:
                        Handle_CheckDuplicatedID(socket, packet);
                        break;
                    case RecvOps.CP_CreateNewCharacter:
                        Handle_CreateNewCharacter(socket, packet);
                        break;
                    case RecvOps.CP_DeleteCharacter:
                        Handle_DeleteCharacter(socket, packet);
                        break;
                    case RecvOps.CP_SelectCharacter:
                        Handle_SelectCharacter(socket, packet);
                        break;
                }
            }
            else
            {
                switch (opcode)
                {
                    case RecvOps.CP_ClientDumpLog:
                        Handle_ClientDumpLog(socket, packet);
                        break;
                    case RecvOps.CP_CheckPassword:
                        Handle_CheckPassword(socket, packet);
                        break;
                    case RecvOps.CP_CreateSecurityHandle:
                        {
                            //In GMS this packet was sent when the client
                            //loaded to the login screen. This means HackShield
                            //initialized successfully. This packet would trigger
                            //the HackShield heartbeat. Luckily back in the day
                            //if you never sent this packet you were never kicked,
                            //so you could stay on the [LOGIN] server forever.

                            //This is a curse for testing mutli client on 1 pc lol
                            //if (socket.Host.Contains("127.0.0.1"))
                            //{
                            //    COutPacket o = new COutPacket();
                            //    o.EncodeString("123456");
                            //    o.EncodeString("admin");

                            //    var buffer = o.ToArray();
                            //    packet = new CInPacket(buffer);

                            //    Handle_CheckPassword(socket, packet);
                            //}
                            break;
                        }
                }
            }

        }
        protected override void HandleDisconnect(WvsLoginClient client)
        {
            base.HandleDisconnect(client);

            //var acc = client.Account;

            //if (acc != null)
            //{
            m_loginPool.Remove(client);
            //}
        }
        //-----------------------------------------------------------------------------
        private void Handle_ClientDumpLog(WvsLoginClient c, CInPacket p)
        {
            //Thank you to Mordred for this handler!

            var callType = p.Decode2();
            var errorCode = p.Decode4();
            var backupBufferSize = p.Decode2();
            var rawSeq = p.Decode4();
            var type = p.Decode2();
            var backupBuffer = p.DecodeBuffer(backupBufferSize - 6);

            var callTypeName = Enum.GetName(typeof(CrashCallType), callType);
            var logTypeName = Enum.GetName(typeof(SendOps), type);

            Logger.Write(LogLevel.Trace,
                "RawSeq: {0} CallType: {1} ErrorCode: {2} BackupBufferSize: {3} Type: {4} - {5} Packet: {6}",
                rawSeq, callTypeName, errorCode, backupBufferSize,
                type, logTypeName,
                Constants.GetString(backupBuffer)
            );
        }
        private void Handle_SelectCharacter(WvsLoginClient c, CInPacket p)
        {
            //var v1 = p.Decode1();
            //var v2 = p.Decode1(); //dwCharacterID
            var uid = p.Decode4();
            //var hwid1 = p.DecodeString();
            //var hwid2 = p.DecodeBuffer(6);
            //var hwid3 = p.DecodeString();

            ParentServer.LoggedIn.Add(new PendingLogin
            {
                CharId = uid,
                Account = c.Account,
                Requested = DateTime.Now,
                Migrated = false
            });

            c.SendPacket(CPacket.SelectCharacterResult(uid));
        }
        private void Handle_CheckPassword(WvsLoginClient c, CInPacket p)
        {
            var pwd = p.DecodeString();
            var user = p.DecodeString();

            var result = c.Login(user, pwd);

            if (result == 0)
            {
                var acc = c.Account;
                c.SendPacket(CPacket.CheckPasswordResult(acc.AccId, acc.Gender, 0, user));
            }
            else
            {
                c.SendPacket(CPacket.CheckPasswordResult(result));
            }
        }
        private void Handle_WorldRequest(WvsLoginClient c, CInPacket p)
        {
            const byte Scania = 0;

            //TODO: World logic

            c.SendPacket(CPacket.WorldRequest(Scania, "Scania"));
            c.SendPacket(CPacket.WorldRequestEnd());
            c.SendPacket(CPacket.LatestConnectedWorld(Scania));
        }
        private void Handle_CheckUserLimit(WvsLoginClient c, CInPacket p)
        {
            var world = p.Decode2();
            c.SendPacket(CPacket.CheckUserLimit(0));
        }
        private void Handle_SelectWorld(WvsLoginClient c, CInPacket p)
        {
            //var world = p.Decode1();
            //var channel = p.Decode1();
            //var unk = p.Decode1();
            //var hwid_maybe = p.Decode4();

            c.LoadAvatars();
            c.SendPacket(CPacket.SelectWorldResult(c.Avatars));
        }
        private void Handle_CheckDuplicatedID(WvsLoginClient c, CInPacket p)
        {
            var charName = p.DecodeString();
            bool nameTaken = IsUsernameTaken(charName);

            c.SendPacket(CPacket.CheckDuplicatedIDResult(charName, nameTaken));
        }
        private void Handle_CreateNewCharacter(WvsLoginClient c, CInPacket p)
        {
            var name = p.DecodeString();
            var job = (short)p.Decode4();
            var subJob = p.Decode2();//whether dual blade = 1 or adventurer = 0
            var face = p.Decode4();
            var hairColor = p.Decode4();
            var hair = p.Decode4() + hairColor;
            var skinColor = (byte)p.Decode4();
            var top = p.Decode4();
            var bottom = p.Decode4();
            var shoes = p.Decode4();
            var weapon = p.Decode4();
            var gender = p.Decode1();

            var realJob = Constants.GetRealJobFromCreation(job);
            var charId = c.ParentServer.FetchNewCharId();

            var newChar = new CharacterData(charId, c.AccId);

            newChar.Stats.dwCharacterID = charId;
            newChar.Stats.sCharacterName = name;
            newChar.Stats.nGender = gender;
            newChar.Stats.nSkin = skinColor;
            newChar.Stats.nFace = face;
            newChar.Stats.nHair = hair;
            newChar.Stats.nLevel = 10;
            newChar.Stats.nJob = realJob;
            newChar.Stats.nSTR = 10;
            newChar.Stats.nDEX = 10;
            newChar.Stats.nINT = 10;
            newChar.Stats.nLUK = 10;
            newChar.Stats.nHP = 50;
            newChar.Stats.nMHP = 50;
            newChar.Stats.nMP = 50;
            newChar.Stats.nMMP = 50;
            newChar.Stats.nAP = 1;
            newChar.Stats.nSP = 1;
            newChar.Stats.nEXP = 0;
            newChar.Stats.nPOP = 0;
            newChar.Stats.dwPosMap = 100000000;
            newChar.Stats.nPortal = 0;
            newChar.Stats.nPlaytime = 0;
            newChar.Stats.nSubJob = subJob;

            newChar.Look.nGender = gender;
            newChar.Look.nSkin = skinColor;
            newChar.Look.nFace = face;
            newChar.Look.nWeaponStickerID = 0;

            newChar.Look.anHairEquip[0] = hair;
            newChar.Look.anHairEquip[5] = top;
            newChar.Look.anHairEquip[6] = bottom;
            newChar.Look.anHairEquip[7] = shoes;
            newChar.Look.anHairEquip[11] = weapon;

            newChar.aInvEquippedNormal.Add(-5, new GW_ItemSlotEquip { nItemID = top });
            newChar.aInvEquippedNormal.Add(-6, new GW_ItemSlotEquip { nItemID = bottom });
            newChar.aInvEquippedNormal.Add(-7, new GW_ItemSlotEquip { nItemID = shoes });
            newChar.aInvEquippedNormal.Add(-11, new GW_ItemSlotEquip { nItemID = weapon });

            //newChar.aInvEquippedNormal.Add(1, new GW_ItemSlotEquip { nItemID = 1002080 });
            newChar.aInvEquip.Add(1, new GW_ItemSlotEquip { nItemID = 1302016 });
            newChar.aInvConsume.Add(1, new GW_ItemSlotBundle { nItemID = 2000007, nNumber = 100 });

            c.ParentServer.AddNewChar(newChar);
            c.SendPacket(CPacket.CreateNewCharacter(name, true, newChar));
        }
        private void Handle_DeleteCharacter(WvsLoginClient c, CInPacket p)
        {
            var uid = p.Decode4();
            c.SendPacket(CPacket.DeleteCharacter(uid, 0));
            //Because I have no pin or pic the game wont let me dlete chars lol
        }
    }
}
