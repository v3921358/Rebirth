using System;
using Common.Log;

namespace WvsRebirth
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var mapleSvc = new MapleService())
            {
                if (Environment.UserInteractive)
                {
                    ConsoleLog.InitConsole("Rebirth v95");
                    Logger.Add(new ConsoleLog());

                    //mapleSvc.WvsCenter.InsertAccount(20000,1,"admin","123456");
                    //mapleSvc.WvsCenter.InsertAccount(20001, 1, "hontale", "123456");
                    //mapleSvc.WvsCenter.InsertAccount(20002, 1, "123456", "123456");

                    mapleSvc.Start();
                    Console.ReadLine();
                    mapleSvc.Stop();
                } else { /*TODO: Window Service Code*/ }
            }
        }
    }
}
