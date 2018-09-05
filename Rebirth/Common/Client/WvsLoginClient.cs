using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Entities;
using Common.Network;
using WvsRebirth;

namespace Common.Client
{
    public class WvsLoginClient : ClientBase
    {
        public WvsLogin ParentServer { get; }
        public List<AvatarData> Avatars { get; set; }

        public WvsLoginClient(WvsLogin login,CClientSocket socket) : base(socket)
        {
            ParentServer = login;
            Avatars = new List<AvatarData>();
        }
    }
}
