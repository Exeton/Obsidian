using Obsidian.Blocks;

namespace Obsidian.Items
{
    public class Item
    {
        public string TrimmedName => this.UnlocalizedName.Replace("minecraft:", "");

        public string UnlocalizedName { get; set; }

        public Materials Type { get; internal set; }

        public int Id { get; set; }
        public ItemMeta Nbt { get; set; }
    }
}
