using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WvsRebirth;

namespace Common.Server
{
    public class WvsCenter
    {
        private WvsLogin m_login;
        private WvsGame[] m_games;

        public WvsCenter(int channels)
        {
            m_login = new WvsLogin();

            m_games = new WvsGame[channels];

            for (int i = 0; i < m_games.Length; i++)
            {
                var channel = (byte) i;
                m_games[i] = new WvsGame(channel);
            }
        }

        public void Start()
        {
            m_login.Start();
            m_games.ToList().ForEach(x => x.Start());
        }
        public void Stop()
        {
            m_login.Stop();
            m_games.ToList().ForEach(x => x.Stop());
        }
    }
}
