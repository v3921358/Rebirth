using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Entities;
using Common.Network;
using MongoDB.Driver;
using WvsRebirth;

namespace Common.Client
{
    public class WvsLoginClient : ClientBase
    {
        public WvsLogin ParentServer { get; }
        public Account Account;
        public List<AvatarData> Avatars;

        public bool LoggedIn => Account != null;

        public int AccId => Account?.AccId ?? 2000;

        public WvsLoginClient(WvsLogin login, CClientSocket socket) : base(socket)
        {
            ParentServer = login;
            Avatars = new List<AvatarData>();
        }

        public void LoadAvatars()
        {
            Avatars.Clear();

            var db = ParentServer.ParentServer.Db.Get();
            
            var chars = db.GetCollection<CharacterEntry>("character")
                .FindSync(x => x.AccId == AccId)
                .ToList();

            foreach (var entry in chars)
            {
                var avatar = new AvatarData(AccId,entry.CharId);

                avatar.Look = db
                    .GetCollection<AvatarLook>("character_looks")
                    .FindSync(x => x.CharId == entry.CharId)
                    .First();

                avatar.Stats = db
                    .GetCollection<GW_CharacterStat>("character_stats")
                    .FindSync(x => x.CharId == entry.CharId)
                    .First();

                Avatars.Add(avatar);
            }
        }
    }
}
