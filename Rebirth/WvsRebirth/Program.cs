using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Network;

namespace WvsRebirth
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Rebirth v95";
            Console.ForegroundColor = ConsoleColor.White;

            WvsLogin x = new WvsLogin();
            x.Start();
            Console.ReadLine();
            x.Stop();
        }
    }
}
