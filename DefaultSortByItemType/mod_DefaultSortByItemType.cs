using Game;
using Game.GameData;
using Game.UI;
using Patchwork;
using System;
using System.Collections.Generic;

namespace V1ld_DefaultSortByItemType
{
    [NewType]
    internal static class V1ldInventorySort
    {
        // Filter sorting order
        public static ItemFilter[] filterOrder = {
            ItemFilter.Weapons,
            ItemFilter.Armor,
            ItemFilter.Clothing,
            ItemFilter.Consumables,
            ItemFilter.Quest,
            ItemFilter.Artifact,
            ItemFilter.Misc,
            ItemFilter.FoodOrDrink,
            ItemFilter.ShipCrew,
            ItemFilter.ShipSails,
            ItemFilter.ShipHulls,
            ItemFilter.ShipFlags,
            ItemFilter.ShipCannons,
            ItemFilter.ShipUpgrades,
            ItemFilter.ShipPermanentUpgrades,
            ItemFilter.Ingredients,
        };

        // Weapon sorting order
        private static WeaponType[] weaponOrder = {
            WeaponType.Unarmed,

            WeaponType.Dagger,
            WeaponType.Stiletto,
            WeaponType.Rapier,
            WeaponType.Sabre,
            WeaponType.Sword,
            WeaponType.Estoc,
            WeaponType.GreatSword,

            WeaponType.Spear,
            WeaponType.Pike,

            WeaponType.Hatchet,
            WeaponType.BattleAxe,
            WeaponType.Pollaxe,

            WeaponType.Club,
            WeaponType.Flail,
            WeaponType.Mace,
            WeaponType.MorningStar,
            WeaponType.WarHammer,

            WeaponType.Quarterstaff,

            WeaponType.Wand,
            WeaponType.Sceptre,
            WeaponType.Rod,

            WeaponType.HuntingBow,
            WeaponType.WarBow,

            WeaponType.Crossbow,
            WeaponType.Arbalest,

            WeaponType.Pistol,
            WeaponType.Blunderbuss,
            WeaponType.Arquebus,

            WeaponType.SmallShield,
            WeaponType.MediumShield,
            WeaponType.LargeShield,
        };

        // Armor sorting order
        private static ArmorCategory[] armorOrder = {
            ArmorCategory.Light,
            ArmorCategory.Medium,
            ArmorCategory.Heavy,
        };

        // Consumable sorting order
        private static ConsumableType[] consumableOrder = {
            ConsumableType.Potion,
            ConsumableType.Drug,
            ConsumableType.Scroll,
            ConsumableType.Figurine,
            ConsumableType.Food,
            ConsumableType.Drink,
            ConsumableType.Explosive,
            ConsumableType.Trap,
        };

        // Annoyingly, Patchwork doesn't seem to like generics so we have to do it the long way
        /* class InventorySorter<T>
        {
            private readonly Dictionary<T, int> m_sortOrder;

            public InventorySorter(T[] ordering)
            {
                m_sortOrder = new Dictionary<T, int>();
                for (int i = 0; i < ordering.Length; i++)
                {
                    m_sortOrder[ordering[i]] = i;
                }
            }

            public int Compare(T a, T b, string nameA, string nameB)
            {
                // return negative if a < b, postive if a > b and compare names if a == b
                if (m_sortOrder[a] != m_sortOrder[b])
                    return m_sortOrder[a] - m_sortOrder[b];
                else
                    return string.Compare(nameA, nameB, true);
            }
        } */

        private static Dictionary<ItemFilter, int> _filterSortOrder;
        private static Dictionary<ItemFilter, int> FilterSortOrder
        {
            get
            {
                if (_filterSortOrder == null)
                {
                    _filterSortOrder = new Dictionary<ItemFilter, int>();
                    for (int i = 0; i < filterOrder.Length; i++)
                    {
                        _filterSortOrder[filterOrder[i]] = i;
                    }
                }
                return _filterSortOrder;
            }
        }

        private static Dictionary<WeaponType, int> _weaponSortOrder;
        private static Dictionary<WeaponType, int> WeaponSortOrder
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

        private static Dictionary<ArmorCategory, int> _armorSortOrder;
        private static Dictionary<ArmorCategory, int> ArmorSortOrder
        {
            get
            {
                if (_armorSortOrder == null)
                {
                    _armorSortOrder = new Dictionary<ArmorCategory, int>();
                    for (int i = 0; i < armorOrder.Length; i++)
                    {
                        _armorSortOrder[armorOrder[i]] = i;
                    }
                }
                return _armorSortOrder;
            }
        }

        private static Dictionary<ConsumableType, int> _consumableSortOrder;
        private static Dictionary<ConsumableType, int> ConsumableSortOrder
        {
            get
            {
                if (_consumableSortOrder == null)
                {
                    _consumableSortOrder = new Dictionary<ConsumableType, int>();
                    for (int i = 0; i < consumableOrder.Length; i++)
                    {
                        _consumableSortOrder[consumableOrder[i]] = i;
                    }
                }
                return _consumableSortOrder;
            }
        }

        public static int Compare(int a, int b, string nameA, string nameB)
        {
            // return negative if a < b, postive if a > b and compare names if a == b
            if (a != b)
                return a - b;
            else
                return string.Compare(nameA, nameB, true);
        }

        private static bool IsWeapon(Equippable a)
        {
            switch (a.PermittedEquipmentSlot)
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

        private static bool IsArmor(Equippable a) => a.PermittedEquipmentSlot == PermittedEquipmentSlot.Armor;

        /* General strategy is to first sort by FilterType and then be more detailed within specific
         * FilterTypes (Weapons, Armor, Consumables, Equippable Items). Default if no other match
         * happens within a FilterType is to compare by name.
         */
        public static int CompareWithinFilterType(Item a, Item b)
        {
            if (a.ItemData.FilterType != b.ItemData.FilterType)
                return FilterSortOrder[a.ItemData.FilterType] - FilterSortOrder[b.ItemData.FilterType];

            Consumable consumableA = a as Consumable;
            Consumable consumableB = b as Consumable;
            if (consumableA != null && consumableB != null)
                return Compare(ConsumableSortOrder[consumableA.ConsumableType], ConsumableSortOrder[consumableB.ConsumableType], a.GetName(), b.GetName());

            Equippable equippableA = a as Equippable;
            Equippable equippableB = b as Equippable;
            if (equippableA != null && equippableB != null)
            {
                if (IsWeapon(equippableA) && IsWeapon(equippableB))
                    return Compare(WeaponSortOrder[equippableA.EquipmentType], WeaponSortOrder[equippableB.EquipmentType], a.GetName(), b.GetName());
                if (IsArmor(equippableA) && IsArmor(equippableB))
                    return Compare(ArmorSortOrder[equippableA.GetArmorComponent().ArmorCategory], ArmorSortOrder[equippableB.GetArmorComponent().ArmorCategory], a.GetName(), b.GetName());

                // Sort by equipment slot if not weapon or armor
                var slotA = equippableA.PermittedEquipmentSlot;
                var slotB = equippableB.PermittedEquipmentSlot;
                if (slotA != slotB)
                    return slotA - slotB;
                else
                    return string.Compare(a.GetName(), b.GetName(), true);
            }

            // Alphabetical by name is the fallback, but equippables get preference
            if (equippableA == null && equippableB == null)
                return string.Compare(a.GetName(), b.GetName(), true);
            else if (equippableA == null)
                return 1;
            else // (equippableB == null)
                return -1;
        }
    }

    [ModifiesType]
    public class V1ld_BaseInventory : BaseInventory
    {
        // The primary sorting function that we will hook in everywhere
        [ModifiesMember("CompareItemsByItemType")]
        new public static int CompareItemsByItemType(Item a, Item b)
        {
            if (a == b)
                return 0;
            if (a == null)
                return 1;
            if (b == null)
                return -1;

            return V1ldInventorySort.CompareWithinFilterType(a, b);
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