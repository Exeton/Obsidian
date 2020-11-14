using Obsidian.Entities;
using System;
using System.Collections.Generic;

namespace Obsidian.Items
{
    public class Inventory
    {
        private short[] items { get; }
        private ItemMeta[] metadata { get; }

        internal static int LastSetId { get; set; } = 1;

        internal byte Id { get; set; }

        internal int ActionsNumber { get; set; }

        /// <summary>
        /// The owner of this inventory (its always a players UUID or null if it has no owner)
        /// </summary>
        public Guid? Owner { get; private set; }

        public Guid Uuid { get; private set; } = Guid.NewGuid();

        public InventoryType Type { get; set; }

        public string Title { get; set; }

        /// <summary>
        /// The size you want the inventory to be only used when creating a generic inventory
        /// Default size is 45
        /// </summary>
        public int Size { get; }

        public List<Player> Viewers { get; private set; } = new List<Player>();

        public Inventory(Guid? owner, int size = 9 * 5)
        {
            if (size % 9 != 0)
                throw new InvalidOperationException($"Generic inventory size must be divisible by 9");
            else if (size > 9 * 6)
                throw new InvalidOperationException($"Generic inventory size must not be greater than ({9 * 6})");

            this.Size = size;
            this.items = new short[size - 1];
            this.metadata = new ItemMeta[size - 1];
            this.Owner = owner;
        }

        public void AddItems(params ItemStack[] items)
        {
            foreach (var item in items)
                this.AddItem(item);
        }

        public int AddItem(ItemStack item)
        {
            if (this.Owner != null)
            {
                for (int i = 36; i < 45; i++)
                {
                    var invItem = this.items[i];
                    var itemMeta = this.metadata[i];

                    if (invItem > 0 && invItem == item.Id)//TODO match item meta
                    {
                        if (itemMeta.Count >= 64)
                            continue;

                        itemMeta.Count += item.Count;

                        return i;
                    }

                    this.items[i] = item.Id;

                    return i;
                }

                for (int i = 9; i < 36; i++)
                {
                    var invItem = this.items[i];
                    var itemMeta = this.metadata[i];

                    if (invItem > 0)
                    {
                        if (itemMeta.Count >= 64)
                            continue;

                        itemMeta.Count += item.Count;

                        return i;
                    }

                    this.items[i] = item.Id;

                    return i;
                }
            }
            else
            {
                for (int i = 0; i < this.Size; i++)
                {
                    var invItem = this.items[i];
                    var itemMeta = this.metadata[i];

                    if (invItem > 0 && invItem == item.Id)//TODO match item meta
                    {
                        if (itemMeta.Count >= 64)
                            continue;

                        itemMeta.Count += item.Count;

                        return i;
                    }

                    this.items[i] = item.Id;
                }
            }

            return 0;
        }

        public void SetItem(int slot, ItemStack item)
        {
            if (slot > this.Size - 1 || slot < 0)
                throw new IndexOutOfRangeException($"{slot} > {this.Size - 1}");

            this.items[slot] = item.Id;
        }

        public ItemStack GetItem(int slot)
        {
            if (slot > this.Size - 1 || slot < 0)
                throw new IndexOutOfRangeException($"{slot} > {this.Size - 1}");

            return ItemStack.Air;
        }

        public bool RemoveItem(int slot, short amount = 1)
        {
            if (slot > this.Size - 1 || slot < 0)
                throw new IndexOutOfRangeException($"{slot} > {this.Size - 1}");

            if (this.items[slot] > 0 && (amount >= 64 || this.metadata[slot].Count - amount <= 0))
                this.items[slot] = 0;
            else
                this.metadata[slot].Count -= amount;

            return true;
        }

        public ItemStack[] GetItems() => new ItemStack[this.Size];
    }

    

    public enum InventoryType
    {
        Generic,
        Anvil,
        Beacon,
        BlastFurnace,
        BrewingStand,
        Crafting,
        Enchantment,
        Furnace,
        Grindstone,
        Hopper,
        Lectern,
        Loom,
        Merchant,
        ShulkerBox,
        Smoker,
        CartographyTable,
        Stonecutter
    }
}
