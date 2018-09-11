using System;
using Common.Client;
using Common.Entities;
using Common.Log;
using Common.Packets;
using Common.Types;

namespace Common.Game
{
    public sealed class CInventoryManipulator
    {
        public static void UnEquip(WvsGameClient c, short src, short dst)
        {
            var character = c.Character;

            var source = character.aInvEquippedNormal.GetKvp(src);
            //var target = character.aInvEquip.GetKvp(dst);


            if (character.aInvEquippedNormal.Remove(src))
            {
                character.aInvEquip.Add(dst, source.Value);
                character.Look.anHairEquip[Math.Abs(src)] = 0;
            }

            c.SendPacket(CPacket.InventoryMoveItem(1, src, dst, 1));
            Broadcast_UserAvatarModified(c);
        }

        public static void Equip(WvsGameClient c, short src, short dst)
        {
            var character = c.Character;

            var source = character.aInvEquip.GetKvp(src); // in equip 
            var target = character.aInvEquippedNormal.GetKvp(dst); //EQUIPPED

            bool r1 = character.aInvEquippedNormal.Remove(dst);
            bool r2 = character.aInvEquip.Remove(src);

            if (r1)
            {
                character.aInvEquip.Add(src, target.Value);
                character.Look.anHairEquip[Math.Abs(dst)] = 0;
            }

            if (r2)
            {
                character.aInvEquippedNormal.Add(dst, source.Value);
                character.Look.anHairEquip[Math.Abs(dst)] = source.Value.nItemID;
            }

            c.SendPacket(CPacket.InventoryMoveItem((byte)InventoryType.EQUIP, src, dst, 2));
            Broadcast_UserAvatarModified(c);
        }

        public static void Drop(WvsGameClient c, byte type, short src, short qty)
        {
            var ret = GetInventory(type, c.Character);

            if (ret is CInventory<short, GW_ItemSlotEquip> v1)
            {
                var source = v1.Get(src);

                if (source == null)
                    return;

                v1.Remove(src);

                //This pocket is not working :(
                c.SendPacket(CPacket.InventoryDropItem(type,src, qty));
                c.SendPacket(CPacket.BroadcastPinkMsg("Deleted item, doesnt drop yet"));

            }
            else if (ret is CInventory<byte, GW_ItemSlotBundle> v2)
            {
                var source = v2.Get((byte)src);

                if (source == null)
                    return;

                source.nNumber -= qty;

                if (source.nNumber <= 0)
                {
                    v2.Remove((byte) src);
                    c.SendPacket(CPacket.BroadcastPinkMsg("Deleted item"));
                }

                //This pocket is not working :(
                c.SendPacket(CPacket.InventoryDropItem(type, src, qty));
            }
            else
            {
                Logger.Write(LogLevel.Error, "What the fuck have you done?");
            }
        }

        public static void Move(WvsGameClient c, byte type, short src, short dst)
        {
            var ret = GetInventory(type, c.Character);

            if (ret is CInventory<short, GW_ItemSlotEquip> v1)
            {
                var source = v1.GetKvp(src);
                var target = v1.GetKvp(dst);

                if (v1.Remove(target.Key))
                    v1.Add(src, target.Value);

                if (v1.Remove(source.Key))
                    v1.Add(dst, source.Value);

            }
            else if (ret is CInventory<byte, GW_ItemSlotBundle> v2)
            {
                var source = v2.GetKvp((byte)src);
                var target = v2.GetKvp((byte)dst);

                if (v2.Remove(target.Key))
                    v2.Add((byte)src, target.Value);

                if (v2.Remove(source.Key))
                    v2.Add((byte)dst, source.Value);
            }
            else
            {
                Logger.Write(LogLevel.Error, "What the fuck have you done?");
            }

            c.SendPacket(CPacket.InventoryMoveItem(type, src, dst, 0xFF));
        }

        private static void Broadcast_UserAvatarModified(WvsGameClient c)
        {
            c.GetCharField().Broadcast(CPacket.UserAvatarModified(c.Character), c);

            //stats.recalcLocalStats();
            //if (getMessenger() != null)
            //{
            //    World.Messenger.updateMessenger(getMessenger().getId(), getName(), client.getChannel());
            //}
        }

        private static object GetInventory(byte nType, CharacterData data)
        {
            var inv = (InventoryType)nType;

            switch (inv)
            {
                case InventoryType.EQUIP:
                    return data.aInvEquip;
                case InventoryType.USE:
                    return data.aInvConsume;
                case InventoryType.SETUP:
                    return data.aInvInstall;
                case InventoryType.ETC:
                    return data.aInvEtc;
                case InventoryType.CASH:
                    return data.aInvCash;
                case InventoryType.EQUIPPED:
                    return data.aInvEquippedNormal;
                default:
                    return null;
            }
        }
    }
}
