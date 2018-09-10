using System.Collections.Generic;
using Common.Entities;
using Common.Network;
using MongoDB.Driver;
using WvsRebirth;

namespace Common.Client
{
    public class WvsLoginClient : ClientBase
    {
        public WvsLogin ParentServer { get; }
        public Account Account { get; set; }
        public List<CharacterData> Avatars { get; set; }
        
        public int AccId => Account?.AccId ?? -1;

        public WvsLoginClient(WvsLogin login, CClientSocket socket) : base(socket)
        {
            ParentServer = login;
            Avatars = new List<CharacterData>();
        }

        public byte Login(string user, string pass)
        {
            var ret = ParentServer.Login(this, user, pass);

            if (ret == 0)
                LoggedIn = true;

            return ret;
        }

        public void LoadAvatars()
        {
            Avatars?.Clear();

            Avatars = ParentServer.Db
                .GetCollection<CharacterData>("character_data")
                .FindSync(x => x.AccId == AccId)
                .ToList();
        }
    }
}
