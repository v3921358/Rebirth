using System;
using Common.Log;

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
