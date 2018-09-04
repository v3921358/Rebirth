using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Network;

namespace Common.Client
{
    public class WvsLoginClient : ClientBase
    {


        public List<Character> Characters { get; set; }

        public WvsLoginClient(CClientSocket socket) : base(socket)
        {
            Characters = new List<Character>();
        }
    }
}
