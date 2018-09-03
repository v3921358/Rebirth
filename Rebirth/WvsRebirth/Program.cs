using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Common.Network;

namespace WvsRebirth
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleLog.InitConsole("Rebirth v95");
            Logger.Add(new ConsoleLog());
            
            WvsLogin x = new WvsLogin();
            x.Start();
            Console.ReadLine();
            x.Stop();
        }
    }
}
