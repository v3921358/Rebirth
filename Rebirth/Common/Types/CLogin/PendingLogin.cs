using System;
using Common.Entities;

namespace Common.Types.CLogin
{
    public class PendingLogin
    {
        public int CharId { get; set; }
        public Account Account { get; set; }
        public DateTime Requested { get; set; }

        public bool Migrated { get; set; }
    }
}
