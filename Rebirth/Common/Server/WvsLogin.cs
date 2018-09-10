using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Common;
using Common.Client;
using Common.Entities;
using Common.Log;
using Common.Network;
using Common.Packets;
using Common.Server;
using MongoDB.Driver;

namespace WvsRebirth
{
    public class WvsLogin : ServerBase<WvsLoginClient>
    {
        //-----------------------------------------------------------------------------
        private Dictionary<WvsLoginClient, Account> m_loginPool;
        //-----------------------------------------------------------------------------
        public WvsLogin(WvsCenter parent) : base("WvsLogin", Constants.LoginPort, parent)
        {
            m_loginPool = new Dictionary<WvsLoginClient, Account>();
        }
        //-----------------------------------------------------------------------------
        public bool IsUsernameTaken(string userName)
        {
            return false;
            //return Db.Get()
            //    .GetCollection<GW_CharacterStat>("chararcter_looks")
            //    .FindSync(x => string.Compare(x.sCharacterName,userName,StringComparison.OrdinalIgnoreCase) == 0)
            //    .Any();
        }
        public void AddNewChar(AvatarData avatar)
        {
            var name = avatar.Stats.sCharacterName;

            var db = Db.Get();

            var entry = new CharacterEntry
            {
                AccId = avatar.AccId,
                CharId = avatar.CharId,
                Name = name
            };

            db.GetCollection<CharacterEntry>("character")
                .InsertOne(entry);

            db.GetCollection<AvatarLook>("character_looks")
                .InsertOne(avatar.Look);

            db.GetCollection<GW_CharacterStat>("character_stats")
                .InsertOne(avatar.Stats);
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

            m_loginPool.Add(c, tmp);

            c.Account = tmp;

            return 0;
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

                        if (socket.Host.Contains("127.0.0.1"))
                        {
                            COutPacket o = new COutPacket();
                            o.EncodeString("123456");
                            o.EncodeString("admin");

                            var buffer = o.ToArray();
                            packet = new CInPacket(buffer);

                            Handle_CheckPassword(socket, packet);
                        }
                        break;
                    }
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

            c.SendPacket(CPacket.SelectCharacterResult(uid));
        }
        private void Handle_CheckPassword(WvsLoginClient c, CInPacket p)
        {
            var pwd = p.DecodeString();
            var user = p.DecodeString();

            var result = c.ParentServer.Login(c, user, pwd);

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

            //TODO: Bind world to client

            c.SendPacket(CPacket.CheckUserLimit(0));
        }
        private void Handle_SelectWorld(WvsLoginClient c, CInPacket p)
        {
            var world = p.Decode1();
            var channel = p.Decode1();
            var unk = p.Decode1();
            var hwid_maybe = p.Decode4();

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
            var job = (short)p.Decode4();// 1 = Adventurer, 0 = Cygnus, 2 = Aran, 3 = evan
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
            var newCharacter = AvatarData.Create(c.AccId, name, gender, skinColor, face, hair, realJob, subJob);

            c.ParentServer.AddNewChar(newCharacter);

            c.SendPacket(CPacket.CreateNewCharacter(name, true, newCharacter));
        }
        private void Handle_DeleteCharacter(WvsLoginClient c, CInPacket p)
        {
            var uid = p.Decode4();
            c.SendPacket(CPacket.DeleteCharacter(uid, 0));
            //Because I have no pin or pic the game wont let me dlete chars lol
        }
    }
}
