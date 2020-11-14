using Obsidian.Items;
using Obsidian.Net;
using Obsidian.Util.Registry;
using System;
using System.Threading.Tasks;

namespace Obsidian.Entities
{
    public class ItemEntity : Entity
    {
        public int Id { get; set; }

        public sbyte Count { get; set; }

        public ItemMeta Nbt { get; set; }

        public bool CanPickup { get; set; }

        public DateTimeOffset TimeDropped { get; private set; } = DateTimeOffset.UtcNow;

        public override async Task WriteAsync(MinecraftStream stream)
        {
            await base.WriteAsync(stream);

            var newItem = this.Nbt;

            newItem.Name = Registry.GetItem(this.Id).TrimmedName;

            await stream.WriteEntityMetdata(7, EntityMetadataType.Slot, new ItemStack((short)this.Id, newItem)
            {
                Present = true,
            });
        }

        public override async Task TickAsync()
        {
            await base.TickAsync();

            if (!CanPickup && this.TimeDropped.Subtract(DateTimeOffset.UtcNow).TotalSeconds > 5)
                this.CanPickup = true;

            foreach (var ent in this.World.GetEntitiesNear(this.Location, 1.5))
            {
                if (ent is ItemEntity item)
                {
                    if (item == this)
                        continue;

                    this.Count += item.Count;

                    await item.RemoveAsync();//TODO find a better way to removed item entities that merged
                }
            }
        }
    }
}
