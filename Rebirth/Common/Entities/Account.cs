using System;
using MongoDB.Bson;

namespace Common.Entities
{
    /// <summary>
    /// Mongo Class
    /// 
    /// Be cautiaus of all public members
    /// And initializing them in the ctor
    /// </summary>
    public class Account
    {
        public ObjectId Id { get; set; }
        public int AccId;
        public string Username;
        public string Password;
        public byte Gender;
        public int Ban;
        public int Admin;
        public DateTime Creation;
        public DateTime LastLogin;
        public string LastIP;
    }
}
