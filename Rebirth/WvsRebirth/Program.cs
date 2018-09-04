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
            
            WvsLogin login = new WvsLogin();
            WvsGame game = new WvsGame();

            login.Start();
            game.Start();

            Console.ReadLine();

            login.Stop();
            game.Start();
        }
    }
}
