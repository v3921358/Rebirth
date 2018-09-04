using System;
using Common.Log;
using Common.Server;

namespace WvsRebirth
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleLog.InitConsole("Rebirth v95");
            Logger.Add(new ConsoleLog());
            
            var server = new WvsCenter(1);
            server.Start();
            Console.ReadLine();
            server.Stop();
        }
    }
}
