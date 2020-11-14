using System.Collections.Generic;

namespace Obsidian.Items
{
    public class ItemMeta
    {
        internal byte Slot { get; set; }

        internal short Count { get; set; }

        internal int CustomModelData { get; set; }//???? What is this

        public int Durability { get; set; }

        public int RepairCost { get; internal set; }
      
        public string Name { get; set; }

        public string Description { get; set; }

        public bool Unbreakable { get; set; }

        public List<Enchantment> Enchantments { get; set; }
        public List<Enchantment> StoredEnchantments { get; private set; }
        public List<string> CanDestroy { get; set; }

        public void AddEnchantment(Enchantment enchantment) => this.Enchantments.Add(enchantment);

        public void AddEnchantments(params Enchantment[] enchants) => this.Enchantments.AddRange(enchants);

        public void AddStoredEnchantment(Enchantment enchantment) => this.StoredEnchantments.Add(enchantment);
        public void AddStoredEnchantments(params Enchantment[] enchants) => this.StoredEnchantments.AddRange(enchants);
    }
}
