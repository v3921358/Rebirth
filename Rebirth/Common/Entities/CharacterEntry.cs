using MongoDB.Bson;

namespace Common.Entities
{
    public class CharacterEntry
    {
        public ObjectId Id { get; set; }
        public int AccId;
        public int CharId;
        public string Name;
    }
}
