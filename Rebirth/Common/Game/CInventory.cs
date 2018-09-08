using System.Collections;
using System.Collections.Generic;
using Common.Entities;

namespace Common.Game
{
    //Moderator: you want Short for equips, Byte for non-equips
    public class CInventory<TKey,TItem> : IEnumerable<KeyValuePair<TKey,TItem>> where TItem : GW_ItemSlotBase
    {
        //Dont use this publlically im telling you...
        public readonly Dictionary<TKey, TItem> m_items;

        public byte SlotLimit { get; set; }
        
        public int Count => m_items.Count;
        
        public CInventory()
        {
            SlotLimit = 96;

            m_items = new Dictionary<TKey, TItem>();
        }

        public void Add(TKey slot, TItem item)
        {
            m_items.Add(slot, item);
        }
        public bool Remove(TKey slot)
        {
            return m_items.Remove(slot);
        }
        
        public IEnumerator<KeyValuePair<TKey, TItem>> GetEnumerator()
        {
            return m_items.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
