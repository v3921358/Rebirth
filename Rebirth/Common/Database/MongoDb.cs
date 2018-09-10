using MongoDB.Driver;

namespace Database
{
    public class MongoDb
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
    }
}
