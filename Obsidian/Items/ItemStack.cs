using Obsidian.Blocks;
using Obsidian.Util.Registry;

namespace Obsidian.Items
{
    public struct ItemStack
    {
        public static ItemStack Air = new ItemStack(0, new ItemMeta { Name = "Air" });

        public short Id { get; internal set; }
        public short Count { get; private set; }
        internal bool Present { get; set; }
        public Materials Type { get; }

        public ItemMeta Metadata { get; set; }

        public ItemStack(short itemId, ItemMeta meta)
        {
            this.Id = itemId;
            this.Metadata = meta;
            this.Count = meta != null ? meta.Count : 0;
            this.Type = Registry.GetItem(itemId).Type;
            this.Present = true;
        }

        public static ItemStack operator -(ItemStack item, short value)
        {
            if (item.Count <= 0)
                return new ItemStack(0, new ItemMeta { Name = "Air" });

            item.Count -= value;
            return item;
        }
    }
}
