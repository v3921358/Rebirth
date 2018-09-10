using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Entities;
using Common.Log;
using Common.Network;
using MongoDB.Driver;
using WvsRebirth;

namespace Common.Client
{
    public class WvsLoginClient : ClientBase
    {
        public WvsLogin ParentServer { get; }
        public Account Account;
        public List<CharacterData> Avatars;

        public bool LoggedIn => Account != null;

        public int AccId => Account?.AccId ?? -1;

        public WvsLoginClient(WvsLogin login, CClientSocket socket) : base(socket)
        {
            ParentServer = login;
            Avatars = new List<CharacterData>();
        }

        public void LoadAvatars()
        {
            Avatars.Clear();

            Logger.Write(LogLevel.Debug,"My AccId {0}",AccId);

            var db = ParentServer.ParentServer.Db.Get();
            
            var chars = db.GetCollection<CharacterData>("character_data")
                .FindSync(x => x.AccId == AccId)
                .ToList();

            foreach (var entry in chars)
            {
                //var avatar = new AvatarData(AccId,entry.CharId);

                //avatar.Look = db
                //    .GetCollection<AvatarLook>("character_looks")
                //    .FindSync(x => x.CharId == entry.CharId)
                //    .First();

                //avatar.Stats = db
                //    .GetCollection<GW_CharacterStat>("character_stats")
                //    .FindSync(x => x.CharId == entry.CharId)
                //    .First();

                Avatars.Add(entry);
            }
        }
    }
}
