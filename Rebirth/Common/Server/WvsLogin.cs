using System;
using System.Linq;
using Common;
using Common.Client;
using Common.Entities;
using Common.Log;
using Common.Network;
using Common.Packets;
using Common.Server;

namespace WvsRebirth
{
    public class WvsLogin : ServerBase<WvsLoginClient>
    {   
        public WvsLogin(WvsCenter parent) : base("WvsLogin", Constants.LoginPort,parent)
        {
            //Eventually do something L0L
        }
        //-----------------------------------------------------------------------------
        public bool IsUsernameTaken(string user)
        {
            return false; //TOPPEST OF THE KEKKEST
        }
        //-----------------------------------------------------------------------------
        protected override WvsLoginClient CreateClient(CClientSocket socket)
        {
            return new WvsLoginClient(this,socket);
        }
        protected override void HandlePacket(WvsLoginClient socket, CInPacket packet)
        {
            //base.HandlePacket(socket, packet);
            var opcode = (RecvOps)packet.Decode2();

           switch (opcode)
            {
                case RecvOps.CP_ClientDumpLog:
                    Handle_ClientDumpLog(socket,packet);
                    break;
                case RecvOps.CP_CheckPassword:
                    Handle_CheckPassword(socket, packet);
                    break;
                case RecvOps.CP_CreateSecurityHandle:

                    //TODO: REMOVE AUTOLOGIN NASTY HACK
                    Handle_CheckPassword(socket, null); 

                    //In GMS this packet was sent when the client
                    //loaded to the login screen. This means HackShield
                    //initialized successfully. This packet would trigger
                    //the HackShield heartbeat. Luckily back in the day
                    //if you never sent this packet you were never kicked,
                    //so you could stay on the [LOGIN] server forever.
                    break;
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
            //var pwd = p.DecodeString();
            //var user = p.DecodeString();

            var user = "Tester123";

            const int AccountId = 5000;
            //TODO: Login logic

            c.SendPacket(CPacket.CheckPasswordResult(AccountId, 0, 0, user));
        }
        private void Handle_WorldRequest(WvsLoginClient c, CInPacket p)
        {
            const byte Scania = 0;

            //TODO: World logic

            c.SendPacket(CPacket.WorldRequest(Scania,"Scania"));
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
           
            var character = AvatarData.Default();
            c.SendPacket(CPacket.SelectWorldResult(character));
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
            var hair = p.Decode4() + p.Decode4(); //var hairColor = p.Decode4();
            var skinColor = (byte)p.Decode4();
            var top = p.Decode4();
            var bottom = p.Decode4();
            var shoes = p.Decode4();
            var weapon = p.Decode4();
            var gender = p.Decode1();

            var realJob = Constants.GetRealJobFromCreation(job);
            var newCharacter = AvatarData.Create(name, gender, skinColor, face, hair, realJob, subJob);

            //c.Characters.Add(x);
            c.SendPacket(CPacket.CreateNewCharacter(name, true, newCharacter));
        }
        private void Handle_DeleteCharacter(WvsLoginClient c, CInPacket p)
        {
            var uid = p.Decode4();
            c.SendPacket(CPacket.DeleteCharacter(uid,0));
        }
    }
}
