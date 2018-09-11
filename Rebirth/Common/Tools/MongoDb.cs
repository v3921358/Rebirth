using System;
using MongoDB.Driver;

namespace Common.Tools
{
    public class MongoDb : IDisposable//This class is such a fucking joke L0L
    {
        private readonly MongoClient m_client;
        
        public MongoDb()
        {
            m_client  = new MongoClient();
        }

        public IMongoDatabase Get(string db = "rebirth")
        {
            return m_client.GetDatabase(db);
        }

        public void Dispose()
        {

        }
    }
}
