using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Client;

namespace Common.Game
{
    public sealed class CInventoryManipulator
    {
        public static void UnEquip(WvsGameClient wvsGameClient, short src, short dst)
        {

        }

        public static void Equip(WvsGameClient c, short src, short dst)
        {
            //final MapleItemInformationProvider ii = MapleItemInformationProvider.getInstance();
            //final MapleCharacter chr = c.getPlayer();
            //if (chr == null)
            //{
            //    return;
            //}
            //final PlayerStats statst = c.getPlayer().getStat();
            //Equip source = (Equip)chr.getInventory(MapleInventoryType.EQUIP).getItem(src);
            //Equip target = (Equip)chr.getInventory(MapleInventoryType.EQUIPPED).getItem(dst);

            var character = c.Character;

            var source = character.aInvEquip.Get(src);
            var target = character.aInvEquip.Get(dst);

            if (source == null || source.nDurability == 0)
            {
                //c.getSession().write(MaplePacketCreator.enableActions());
                return;
            }

            //c.getSession().write(MaplePacketCreator.moveInventoryItem(MapleInventoryType.EQUIP, src, dst, (byte)2));


            //final Map<String, Integer> stats = ii.getEquipStats(source.getItemId());
            //if (dst < -999 && !GameConstants.isEvanDragonItem(source.getItemId()))
            //{
            //    c.getSession().write(MaplePacketCreator.enableActions());
            //    return;
            //}
            //else if (dst >= -999 && dst < -99 && stats.get("cash") == 0)
            //{
            //    c.getSession().write(MaplePacketCreator.enableActions());
            //    return;
            //}
            //if (!ii.canEquip(stats, source.getItemId(), chr.getLevel(), chr.getJob(), chr.getFame(), statst.getTotalStr(), statst.getTotalDex(), statst.getTotalLuk(), statst.getTotalInt(), c.getPlayer().getStat().levelBonus))
            //{
            //    c.getSession().write(MaplePacketCreator.enableActions());
            //    return;
            //}
            //x
            //if (GameConstants.isWeapon(source.getItemId()) && dst != -10 && dst != -11)
            //{
            //    AutobanManager.getInstance().autoban(c, "Equipment hack, itemid " + source.getItemId() + " to slot " + dst);
            //    return;
            //}
            //if (!ii.isCash(source.getItemId()) && !GameConstants.isMountItemAvailable(source.getItemId(), c.getPlayer().getJob()))
            //{
            //    c.getSession().write(MaplePacketCreator.enableActions());
            //    return;
            //}
            //if (GameConstants.isKatara(source.getItemId()))
            //{
            //    dst = (byte)-10; //shield slot
            //}
            //if (GameConstants.isEvanDragonItem(source.getItemId()) && (chr.getJob() < 2200 || chr.getJob() > 2218))
            //{
            //    c.getSession().write(MaplePacketCreator.enableActions());
            //    return;
            //}

            //switch (dst)
            //{
            //    case -6:
            //    { // Top
            //        final IItem top = chr.getInventory(MapleInventoryType.EQUIPPED).getItem((byte)-5);
            //        if (top != null && GameConstants.isOverall(top.getItemId()))
            //        {
            //            if (chr.getInventory(MapleInventoryType.EQUIP).isFull())
            //            {
            //                c.getSession().write(MaplePacketCreator.getInventoryFull());
            //                c.getSession().write(MaplePacketCreator.getShowInventoryFull());
            //                return;
            //            }
            //            unequip(c, (byte)-5, chr.getInventory(MapleInventoryType.EQUIP).getNextFreeSlot());
            //        }
            //        break;
            //    }
            //    case -5:
            //    {
            //        final IItem top = chr.getInventory(MapleInventoryType.EQUIPPED).getItem((byte)-5);
            //        final IItem bottom = chr.getInventory(MapleInventoryType.EQUIPPED).getItem((byte)-6);
            //        if (top != null && GameConstants.isOverall(source.getItemId()))
            //        {
            //            if (chr.getInventory(MapleInventoryType.EQUIP).isFull(bottom != null && GameConstants.isOverall(source.getItemId()) ? 1 : 0))
            //            {
            //                c.getSession().write(MaplePacketCreator.getInventoryFull());
            //                c.getSession().write(MaplePacketCreator.getShowInventoryFull());
            //                return;
            //            }
            //            unequip(c, (byte)-5, chr.getInventory(MapleInventoryType.EQUIP).getNextFreeSlot());
            //        }
            //        if (bottom != null && GameConstants.isOverall(source.getItemId()))
            //        {
            //            if (chr.getInventory(MapleInventoryType.EQUIP).isFull())
            //            {
            //                c.getSession().write(MaplePacketCreator.getInventoryFull());
            //                c.getSession().write(MaplePacketCreator.getShowInventoryFull());
            //                return;
            //            }
            //            unequip(c, (byte)-6, chr.getInventory(MapleInventoryType.EQUIP).getNextFreeSlot());
            //        }
            //        break;
            //    }
            //    case -10:
            //    { // Shield
            //        IItem weapon = chr.getInventory(MapleInventoryType.EQUIPPED).getItem((byte)-11);
            //        if (GameConstants.isKatara(source.getItemId()))
            //        {
            //            if ((chr.getJob() != 900 && (chr.getJob() < 430 || chr.getJob() > 434)) || weapon == null || !GameConstants.isDagger(weapon.getItemId()))
            //            {
            //                c.getSession().write(MaplePacketCreator.getInventoryFull());
            //                c.getSession().write(MaplePacketCreator.getShowInventoryFull());
            //                return;
            //            }
            //        }
            //        else if (weapon != null && GameConstants.isTwoHanded(weapon.getItemId()))
            //        {
            //            if (chr.getInventory(MapleInventoryType.EQUIP).isFull())
            //            {
            //                c.getSession().write(MaplePacketCreator.getInventoryFull());
            //                c.getSession().write(MaplePacketCreator.getShowInventoryFull());
            //                return;
            //            }
            //            unequip(c, (byte)-11, chr.getInventory(MapleInventoryType.EQUIP).getNextFreeSlot());
            //        }
            //        break;
            //    }
            //    case -11:
            //    { // Weapon
            //        IItem shield = chr.getInventory(MapleInventoryType.EQUIPPED).getItem((byte)-10);
            //        if (shield != null && GameConstants.isTwoHanded(source.getItemId()))
            //        {
            //            if (chr.getInventory(MapleInventoryType.EQUIP).isFull())
            //            {
            //                c.getSession().write(MaplePacketCreator.getInventoryFull());
            //                c.getSession().write(MaplePacketCreator.getShowInventoryFull());
            //                return;
            //            }
            //            unequip(c, (byte)-10, chr.getInventory(MapleInventoryType.EQUIP).getNextFreeSlot());
            //        }
            //        break;
            //    }
            //}
            //source = (Equip)chr.getInventory(MapleInventoryType.EQUIP).getItem(src); // Equip
            //target = (Equip)chr.getInventory(MapleInventoryType.EQUIPPED).getItem(dst); // Currently equipping
            //if (source == null)
            //{
            //    c.getSession().write(MaplePacketCreator.enableActions());
            //    return;
            //}
            //if (stats.get("equipTradeBlock") == 1)
            //{ // Block trade when equipped.
            //    byte flag = source.getFlag();
            //    if (!ItemFlag.UNTRADEABLE.check(flag))
            //    {
            //        flag |= ItemFlag.UNTRADEABLE.getValue();
            //        source.setFlag(flag);
            //        c.getSession().write(MaplePacketCreator.updateSpecialItemUse_(source, GameConstants.getInventoryType(source.getItemId()).getType()));
            //    }
            //}

            //chr.getInventory(MapleInventoryType.EQUIP).removeSlot(src);
            //if (target != null)
            //{
            //    chr.getInventory(MapleInventoryType.EQUIPPED).removeSlot(dst);
            //}
            //source.setPosition(dst);
            //chr.getInventory(MapleInventoryType.EQUIPPED).addFromDB(source);
            //if (target != null)
            //{
            //    target.setPosition(src);
            //    chr.getInventory(MapleInventoryType.EQUIP).addFromDB(target);
            //}
            //if (GameConstants.isWeapon(source.getItemId()))
            //{
            //    if (chr.getBuffedValue(MapleBuffStat.BOOSTER) != null)
            //    {
            //        chr.cancelBuffStats(MapleBuffStat.BOOSTER);
            //    }
            //    if (chr.getBuffedValue(MapleBuffStat.SPIRIT_CLAW) != null)
            //    {
            //        chr.cancelBuffStats(MapleBuffStat.SPIRIT_CLAW);
            //    }
            //    if (chr.getBuffedValue(MapleBuffStat.SOULARROW) != null)
            //    {
            //        chr.cancelBuffStats(MapleBuffStat.SOULARROW);
            //    }
            //    if (chr.getBuffedValue(MapleBuffStat.WK_CHARGE) != null)
            //    {
            //        chr.cancelBuffStats(MapleBuffStat.WK_CHARGE);
            //    }
            //    if (chr.getBuffedValue(MapleBuffStat.LIGHTNING_CHARGE) != null)
            //    {
            //        chr.cancelBuffStats(MapleBuffStat.LIGHTNING_CHARGE);
            //    }
            //}
            //if (GameConstants.isDragonItem(source.getItemId()))
            //{
            //    chr.finishAchievement(8);
            //}
            //else if (GameConstants.isReverseItem(source.getItemId()))
            //{
            //    chr.finishAchievement(9);
            //}
            //else if (GameConstants.isTimelessItem(source.getItemId()))
            //{
            //    chr.finishAchievement(10);
            //}
            //c.getSession().write(MaplePacketCreator.moveInventoryItem(MapleInventoryType.EQUIP, src, dst, (byte)2));
            //chr.equipChanged();
            //if (source.getItemId() == 1122017)
            //{
            //    chr.startFairySchedule();
            //}
        }

        public static void Drop(WvsGameClient wvsGameClient, byte type, short src, short quantity)
        {

        }

        public static void Move(WvsGameClient wvsGameClient, byte type, short src, short dst)
        {

        }
    }
}
