using Game;
using Game.GameData;
using static Game.GameData.WeaponType;
using Game.UI;
using Patchwork;
using System;
using System.Collections.Generic;

namespace V1ld_DefaultSortByItemType
{
    [NewType]
    internal static class V1ldInventorySort
    {
        // Weapon sorting order
        private static WeaponType[] weaponOrder = {
            Unarmed,

            Dagger,
            Stiletto,
            Rapier,
            Sabre,
            Sword,
            Estoc,
            GreatSword,

            Spear,
            Pike,

            Hatchet,
            BattleAxe,
            Pollaxe,

            Club,
            Flail,
            Mace,
            MorningStar,
            WarHammer,

            Quarterstaff,

            Wand,
            Sceptre,
            Rod,

            HuntingBow,
            WarBow,

            Crossbow,
            Arbalest,

            Pistol,
            Blunderbuss,
            Arquebus,

            SmallShield,
            MediumShield,
            LargeShield,
        };

        private static Dictionary<WeaponType, int> _weaponSortOrder;
        private static Dictionary<WeaponType, int> weaponSortOrder
        {
            get
            {
                if (_weaponSortOrder == null)
                {
                    _weaponSortOrder = new Dictionary<WeaponType, int>();
                    for (int i = 0; i < weaponOrder.Length; i++)
                    {
                        _weaponSortOrder[weaponOrder[i]] = i;
                    }
                }
                return _weaponSortOrder;
            }
        }

        public static int CompareEquippableWeapons(Equippable equippableA, Equippable equippableB)
        {
            var wpnA = weaponSortOrder[equippableA.EquipmentType];
            var wpnB = weaponSortOrder[equippableB.EquipmentType];

            if (wpnA == wpnB)
                return string.Compare(equippableA.GetName(), equippableB.GetName(), true);
            else
                return wpnA - wpnB;
        }

        private static int CompareEquippableArmors(Equippable equippableA, Equippable equippableB)
        {
            var armorA = equippableA.GetArmorComponent().ArmorCategory;
            var armorB = equippableB.GetArmorComponent().ArmorCategory;

            if (armorA == armorB)
                return string.Compare(equippableA.GetName(), equippableB.GetName(), true);
            else
                return armorA - armorB; // category enum is [light, medium, heavy] so this works
        }

        private static bool IsWeapon(PermittedEquipmentSlot pws)
        {
            switch (pws)
            {
                case PermittedEquipmentSlot.AnyWeapon:
                case PermittedEquipmentSlot.PrimaryWeaponOnly:
                case PermittedEquipmentSlot.SecondaryWeaponOnly:
                case PermittedEquipmentSlot.BothPrimaryAndSecondary:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsArmor(PermittedEquipmentSlot pws)
        {
            return PermittedEquipmentSlot.Armor == pws;
        }

        public static int CompareWithinFilterType(Item a, Item b)
        {
            if (a == b)
                return 0;

            //Game.Console.AddMessage($"1: a={a.GetName()} b={b.GetName()}");
            Equippable equippableA = a as Equippable;
            Equippable equippableB = b as Equippable;

            if (equippableA == null && equippableB == null)
                return string.Compare(a.GetName(), b.GetName(), true);
            if (equippableA == null)
                return 1;
            if (equippableB == null)
                return -1;

            var pwsA = equippableA.PermittedEquipmentSlot;
            var pwsB = equippableB.PermittedEquipmentSlot;

            //Game.Console.AddMessage($"2: a={a.GetName()} pwsA={pwsA} b={b.GetName()} pwsB={pwsB}");
            if (IsWeapon(pwsA) && IsWeapon(pwsB))
                return CompareEquippableWeapons(equippableA, equippableB);
            if (IsArmor(pwsA) && IsArmor(pwsB))
                return CompareEquippableArmors(equippableA, equippableB);

            if (pwsA == pwsB)
                return string.Compare(a.GetName(), b.GetName(), true);
            else
                return pwsA - pwsB;
        }
    }

    [ModifiesType]
    public class V1ld_BaseInventory : BaseInventory
    {
        // Our main sorting function that we will hook in everywhere
        [ModifiesMember("CompareItemsByItemType")]
        new public static int CompareItemsByItemType(Item a, Item b)
        {
            if (a == null)
                return 1;
            if (b == null)
                return -1;

            if (a.ItemData.FilterType == b.ItemData.FilterType)
                return V1ldInventorySort.CompareWithinFilterType(a, b);
            else
                return (a.ItemData.FilterType < b.ItemData.FilterType) ? -1 : 1;
        }

        [ModifiesMember("CompareItemsForShop")]
        new public static int CompareItemsForShop(Item a, Item b)
        {
            /*
            if (a == null)
            {
                return 1;
            }
            if (b == null)
            {
                return -1;
            }
            int num = a.ItemData.FilterType - b.ItemData.FilterType;
            if (num != 0)
            {
                return num;
            }
            Consumable consumable = a as Consumable;
            Consumable consumable2 = b as Consumable;
            if (consumable != null && consumable2 != null)
            {
                num = consumable.ConsumableType - consumable2.ConsumableType;
                if (num != 0)
                {
                    return num;
                }
            }
            ShieldComponent component = a.ItemData.GetComponent<ShieldComponent>();
            ShieldComponent component2 = b.ItemData.GetComponent<ShieldComponent>();
            if (component != null && component2 == null)
            {
                return 1;
            }
            if (component2 != null && component == null)
            {
                return -1;
            }
            num = (int)(b.GetSingleItemValue() - a.GetSingleItemValue());
            if (num != 0)
            {
                return num;
            }
            */

            // Use our item sorting even in shops
            return CompareItemsByItemType(a, b);
        }
    }

    [ModifiesType]
    public class V1ld_UIInventorySortWidget : UIInventorySortWidget
    {
        [ModifiesMember("ReSort")]
        new private void ReSort()
        {
            switch (Type)
            {
                case SortType.None:
                    // Default is to use our item type sort, no more null sorts
                    m_targetGrid.SortComparison = BaseInventory.CompareItemsByItemType;
                    break;
                case SortType.MoneyValue:
                    if (SortAscending)
                    {
                        m_targetGrid.SortComparison = BaseInventory.CompareItemsBySellValue;
                    }
                    else
                    {
                        m_targetGrid.SortComparison = BaseInventory.ReverseCompareItemsBySellValue;
                    }
                    break;
                case SortType.EnchantValue:
                    if (SortAscending)
                    {
                        m_targetGrid.SortComparison = BaseInventory.CompareItemsByEnchantment;
                    }
                    else
                    {
                        m_targetGrid.SortComparison = BaseInventory.ReverseCompareItemsByEnchantment;
                    }
                    break;
                case SortType.Chronological:
                    if (SortAscending)
                    {
                        m_targetGrid.SortComparison = BaseInventory.CompareItemsByTimeAcquired;
                    }
                    else
                    {
                        m_targetGrid.SortComparison = BaseInventory.ReverseCompareItemsByTimeAcquired;
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    [ModifiesType]
    public class V1ld_UIInventoryItemGrid : UIInventoryItemGrid
    {
        [ModifiesMember("get_SortComparison")]
        public Comparison<Item> get_SortComparison()
        {
            // Default is to use our item type sort, no more null sorts
            if (m_sortComparison == null)
                m_sortComparison = BaseInventory.CompareItemsByItemType;
            return m_sortComparison;
        }
    }
}