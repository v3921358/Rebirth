using System.Collections;
using System.Collections.Generic;
using Common.Entities;

namespace Common.Game
{
    /*
    public class CInventory<TKey, TItem> : Dictionary<TKey, TItem> where TItem : GW_ItemSlotBase
    {
        public byte SlotLimit { get; set; }

        public CInventory()
        {
            SlotLimit = 96;
        }
    }
    */

    //Moderator: you want Short for equips, Byte for non-equips
    
    public class CInventory<TKey,TItem> : IEnumerable<KeyValuePair<TKey,TItem>> where TItem : GW_ItemSlotBase
    {
        private readonly Dictionary<TKey, TItem> Items;

        public byte SlotLimit { get; set; }
        
        public int Count => Items.Count;
        
        public CInventory()
        {
            SlotLimit = 96;

            Items = new Dictionary<TKey, TItem>();
        }

        public void Add(KeyValuePair<TKey, TItem> kvp)
        {
            Items.Add(kvp.Key,kvp.Value);
        }
        public void Add(TKey slot, TItem item)
        {
            Items.Add(slot, item);
        }
        public bool Remove(TKey slot)
        {
            return Items.Remove(slot);
        }
        public void Clear()
        {
            Items.Clear();
        }
        public TItem Get(TKey key)
        {
            if (Items.ContainsKey(key))
                return Items[key];

            return default(TItem);
        }
        public KeyValuePair<TKey, TItem> GetKvp(TKey key)
        {
            if (Items.ContainsKey(key))
                return new KeyValuePair<TKey,TItem>(key,Items[key]);

            return default(KeyValuePair<TKey, TItem>);
        }

        public bool Contains(TKey slot)
        {
            return Items.ContainsKey(slot);
        }


        public IEnumerator<KeyValuePair<TKey, TItem>> GetEnumerator()
        {
            return Items.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }    
}
