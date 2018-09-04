using System;
using System.Linq;
using Common;
using Common.Client;
using Common.Entities;
using Common.Log;
using Common.Network;
using Common.Server;

namespace WvsRebirth
{
    public class WvsLogin : ServerBase<WvsLoginClient>
    {
        private static readonly Func<CClientSocket, WvsLoginClient> ClientCreator
            = ccs => new WvsLoginClient(ccs);
        //-----------------------------------------------------------------------------
        public WvsLogin() : base("WvsLogin", 8484, ClientCreator)
        {
            //Eventually do something L0L
        }
        //-----------------------------------------------------------------------------
        public bool IsUsernameTaken(string user)
        {
            return false; //TOPPEST OF THE KEKKEST
        }
        //-----------------------------------------------------------------------------
        protected override void HandlePacket(WvsLoginClient socket, CInPacket packet)
        {
            base.HandlePacket(socket, packet);
            var opcode = (RecvOps)packet.Decode2();

            /*
            
            var name = opcode.ToString();
            var valid = string.IsNullOrWhiteSpace(name);

            name = name.Remove(0, 3);

            if (!valid)
            {
                Logger.Write(LogLevel.Warning, "[{0}] RECV UnkOpcode {1:x}", socket.Host, opcode);
                return;
            }

            var handler =
                GetType()
                .GetMethods()
                .FirstOrDefault(x => x.Name == $"Handle_{name}");

            if (handler == null)
            {
                Logger.Write(LogLevel.Warning, "[{0}] RECV UnkHandler {1}", socket.Host, name);
                return;
            }

            handler.Invoke(this, new object[] { socket, packet });
            */

            switch (opcode)
            {
                case RecvOps.CP_ClientDumpLog:
                    //Handle_ClientDumpLog(socket,packet);
                    break;
                case RecvOps.CP_CheckPassword:
                    Handle_CheckPassword(socket, packet);
                    break;
                case RecvOps.CP_CreateSecurityHandle:
                    //In GMS this packet was sent when the client
                    //loaded to the login screen. This means HackShield
                    //initialized successfully. This packet would trigger
                    //the HackShield heartbeat. Luckily back in the day
                    //if you never sent this packet you were never kicked,
                    //so you could stay on the [LOGIN] server forever.
                    break;
                case RecvOps.CP_WorldRequest:
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
            }
        }

        private void Handle_ClientDumpLog(WvsLoginClient c, CInPacket p)
        {
            var callType = p.Decode2();
            var errorCode = p.Decode4();
            var backupBufferSize = p.Decode2();
            var rawSeq = p.Decode4();
            var type = p.Decode2();
            var backupBuffer = p.DecodeBuffer(backupBufferSize - 6);

            var callTypeName = Enum.GetName(typeof(CrashCallType), callType);

            Logger.Write(LogLevel.Trace,
                "RawSeq: {0} CallType: {1} ErrorCode: {2} BackupBufferSize: {3} Type: {4} - {5} Packet: {6}",
                rawSeq, callTypeName, errorCode, backupBufferSize, type,
                type,//LoginOperation.getByType(type).name(),
                Constants.GetString(backupBuffer)
                );
        }

        //-----------------------------------------------------------------------------
        private void Handle_CheckPassword(WvsLoginClient c, CInPacket p)
        {
            var pwd = p.DecodeString();
            var user = p.DecodeString();

            //TODO: Login logic

            c.SendPacket(CPacket.CheckPasswordResult(5000, 0, 0, user));
        }
        private void Handle_WorldRequest(WvsLoginClient c, CInPacket p)
        {
            const byte Scania = 0;

            //TODO: World logic

            c.SendPacket(CPacket.WorldRequest(Scania));
            c.SendPacket(CPacket.WorldRequestEnd());
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
            
            //var hex =
            //    "0B 00 00 02 35 00 00 00 5B 35 33 5D 44 61 72 74 65 72 00 00 00 01 00 08 52 00 00 18 79 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 04 00 04 00 04 00 04 00 32 00 32 00 32 00 32 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 E1 F5 05 00 00 00 00 00 00 00 01 00 08 52 00 00 00 18 79 00 00 05 6A E2 0F 00 06 8A 30 10 00 07 81 5B 10 00 0B F0 DD 13 00 FF FF F0 DD 13 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 36 00 00 00 5B 35 34 5D 4D 6F 72 64 72 65 64 00 00 01 00 08 52 00 00 18 79 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 04 00 04 00 04 00 04 00 32 00 32 00 32 00 32 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 E1 F5 05 00 00 00 00 00 00 00 01 00 08 52 00 00 00 18 79 00 00 05 6A E2 0F 00 07 81 5B 10 00 0B F0 DD 13 00 FF FF F0 DD 13 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 02 03 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00";

            //var packet = Constants.GetBytes(hex);

            //c.SendPacket(packet);


            //var character = Character.Default();
            var character = Array.Empty<Character>();
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
            var job = p.Decode4();// 1 = Adventurer, 0 = Cygnus, 2 = Aran, 3 = evan
            var db = p.Decode2();//whether dual blade = 1 or adventurer = 0
            var face = p.Decode4();
            var hair = p.Decode4();
            var hairColor = p.Decode4();
            var skinColor = p.Decode4();
            var top = p.Decode4();
            var bottom = p.Decode4();
            var shoes = p.Decode4();
            var weapon = p.Decode4();
            var gender_maybe = p.Decode1();

            //var x = new Character
            //{
            //    Uid = Constants.Rand.Next(1245, 5432),
            //    Name = name,
            //    Gender = 1,//gender_maybe,
            //    SkinColor = 0,//(byte)skinColor,
            //    Face = 21000,//face,
            //    Hair = 31000,//hair + hairColor,
            //    //Pets
            //    Level = 10,
            //    Job = 0,//Job = (short)job,
            //    //db
            //    StatStr = 4,
            //    StatDex = 4,
            //    StatInt = 4,
            //    StatLuk = 4,
            //    StatCurHp = 10,
            //    StatMaxHp = 50,
            //    StatCurMp = 10,
            //    StatMaxMp = 50,
            //    Ap = 10,
            //    Sp = 20,
            //    Exp = 500,
            //    Fame = 255,
            //    MapId = 180000000,
            //    MapSpawn = 0,
            //};

            var x = new Character
            {
                Uid = 7876,
                Name = name,
                Gender = 1,//gender_maybe,
                SkinColor = 0,//(byte)skinColor,
                Face = 21000,//face,
                Hair = 31000,//hair + hairColor,
                //Pets
                Level = 10,
                Job = 0,//Job = (short)job,
                //db
                StatStr = 4,
                StatDex = 4,
                StatInt = 4,
                StatLuk = 4,
                StatCurHp = 50,
                StatMaxHp = 50,
                StatCurMp = 50,
                StatMaxMp = 50,
                Ap = 10,
                Sp = 20,
                Exp = 2,
                Fame = 255,
                MapId = 100000000,
                MapSpawn = 0,
            };

            //var buffer =
            //   Constants.GetBytes("0E 00 00 36 00 00 00 5B 35 34 5D 4D 6F 72 64 72 65 64 00 00 01 00 08 52 00 00 18 79 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 04 00 04 00 04 00 04 00 32 00 32 00 32 00 32 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 E1 F5 05 00 00 00 00 00 00 00 01 00 08 52 00 00 00 18 79 00 00 05 6A E2 0F 00 06 8A 30 10 00 07 81 5B 10 00 0B F0 DD 13 00 FF FF F0 DD 13 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");

            //c.SendPacket(buffer);

            //c.Characters.Add(x);

            //This shit right here does not work
            c.SendPacket(CPacket.CreateNewCharacter(name, true, x));
        }
    }
}
