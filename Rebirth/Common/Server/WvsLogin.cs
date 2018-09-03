using System;
using System.Linq;
using Common;
using Common.Client;
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
        //-----------------------------------------------------------------------------
        private void Handle_CheckPassword(WvsLoginClient c, CInPacket p)
        {
            var pwd = p.DecodeString();
            var user = p.DecodeString();

            //TODO: Login logic

            c.SendPacket(CPacket.CheckPasswordResult(1337, 0, 0, user));
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

            var x = new Character
            {
                Uid = Constants.Rand.Next(5000, 6000),
                Name = name,
                Gender = gender_maybe,
                SkinColor = (byte)skinColor,
                Face = face,
                Hair = hair + hairColor,
                //Pets
                Level = 10,
                Job = (short)job,
                //db
                StatStr = 4,
                StatDex = 4,
                StatInt = 4,
                StatLuk = 4,
                StatCurHp = 10,
                StatMaxHp = 50,
                StatCurMp = 10,
                StatMaxMp = 50,
                Ap = 0,
                Sp = 0,
                Exp = 0,
                Fame = 0,
                MapId = 180000000,
                MapSpawn = 0,
            };

            //c.Characters.Add(x);
            c.SendPacket(CPacket.CreateNewCharacter(name, true, x));
        }
    }
}
