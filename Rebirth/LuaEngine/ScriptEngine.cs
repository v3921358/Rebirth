using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLua;

namespace LuaEngine
{
    public class ScriptEngine
    {
        private Lua m_ctx;

        public ScriptEngine()
        {
            m_ctx = new Lua();
       
        }

    }
}
