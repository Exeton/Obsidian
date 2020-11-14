using Obsidian.API;
using Obsidian.Serializer.Attributes;
using Obsidian.Serializer.Enums;
using Obsidian.API;
using System.Threading.Tasks;
using Obsidian.Entities;
using Obsidian.Events.EventArgs;
using Obsidian.Util.Registry;
using Obsidian.Blocks;
using Obsidian.Items;
using Obsidian.Net.Packets.Play.Client;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using Newtonsoft.Json;

namespace Obsidian.Net.Packets.Play.Server
{
    public class PlayerBlockPlacement : IPacket
    {
        [Field(0, Type = DataType.VarInt)]
        public Hand Hand { get; set; } // hand it was placed from. 0 is main, 1 is off

        [Field(1)]
        public Position Location { get; set; }

        [Field(2, Type = DataType.VarInt)]
        public BlockFace Face { get; set; }

        [Field(3)]
        public float CursorX { get; set; }

        [Field(4)]
        public float CursorY { get; set; }

        [Field(5)]
        public float CursorZ { get; set; }

        [Field(6)]
        public bool InsideBlock { get; set; }

        public int Id => 0x2E;

        public PlayerBlockPlacement() { }

        public Task WriteAsync(MinecraftStream stream) => Task.CompletedTask;

        public async Task ReadAsync(MinecraftStream stream)
        {
            this.Hand = (Hand)await stream.ReadVarIntAsync();
            this.Location = await stream.ReadPositionAsync();
            this.Face = (BlockFace)await stream.ReadVarIntAsync();
            this.CursorX = await stream.ReadFloatAsync();
            this.CursorY = await stream.ReadFloatAsync();
            this.CursorZ = await stream.ReadFloatAsync();
            this.InsideBlock = await stream.ReadBooleanAsync();
        }

        public async Task HandleAsync(Obsidian.Server server, Player player)
        {
            var currentItem = player.GetHeldItem();

            var block = Registry.GetBlock(currentItem.Type);

            var location = this.Location;

            var interactedBlock = server.World.GetBlock(location);

            if (interactedBlock.CanInteract() && !player.Sneaking)
            {
                interactedBlock.Location = location;
                player.LastInteractedBlock = interactedBlock;

                var arg = await server.Events.InvokeBlockInteractAsync(new BlockInteractEventArgs(player, block, this.Location));

                if (arg.Cancel)
                    return;

                var maxId = (byte)Math.Max(1, ++Inventory.LastSetId);
                if (maxId == byte.MaxValue)
                    maxId = 1;

                if(server.World.GetBlockMetadataValue(location, "inventory_id") is Guid uuid)
                {
                    if(server.CachedWindows.TryGetValue(uuid, out var inventory))
                    {
                        Console.WriteLine($"Opened window with id of: {uuid} {JsonConvert.SerializeObject(inventory.GetItems(), Formatting.Indented)}");

                        await player.OpenInventoryAsync(inventory);
                        await player.client.QueuePacketAsync(new BlockAction
                        {
                            Location = location,
                            ActionId = 1,
                            ActionParam = 1,
                            BlockType = interactedBlock.Id
                        });
                        await player.SendSoundAsync(Sounds.BlockChestOpen, location.SoundPosition, SoundCategory.Blocks);

                        player.OpenedInventory = inventory;
                    }

                    return;
                }

                if (interactedBlock.Type == Materials.Chest)
                {
                    var inventory = new Inventory(null, 9 * 3)
                    {
                        Title = "Chest",
                        Type = InventoryType.Generic,
                        Id = maxId
                    };

                    await player.OpenInventoryAsync(inventory);
                    await player.client.QueuePacketAsync(new BlockAction
                    {
                        Location = location,
                        ActionId = 1,
                        ActionParam = 1,
                        BlockType = interactedBlock.Id
                    });
                    await player.SendSoundAsync(Sounds.BlockChestOpen, location.SoundPosition, SoundCategory.Blocks);

                    server.World.SetBlockMetadataValue(location, "inventory_id", inventory.Uuid);

                    server.CachedWindows.TryAdd(inventory.Uuid, inventory);

                    player.OpenedInventory = inventory;
                }
                else if (interactedBlock.Type == Materials.CraftingTable)
                {
                    await player.OpenInventoryAsync(new Inventory(null)
                    {
                        Title = "Crafting Table",
                        Type = InventoryType.Crafting,
                        Id = maxId
                    });
                }

                return;
            }

            if (player.Gamemode != Gamemode.Creative)
                player.Inventory.RemoveItem(player.CurrentSlot);

            switch (this.Face) // TODO fix this for logs
            {
                case BlockFace.Bottom:
                    location.Y -= 1;
                    break;

                case BlockFace.Top:
                    location.Y += 1;
                    break;

                case BlockFace.North:
                    location.Z -= 1;
                    break;

                case BlockFace.South:
                    location.Z += 1;
                    break;

                case BlockFace.West:
                    location.X -= 1;
                    break;

                case BlockFace.East:
                    location.X += 1;
                    break;

                default:
                    break;
            }

            block.Location = location;
            server.World.SetBlock(location, block);

            await server.BroadcastBlockPlacementAsync(player, block, location);
        }
    }

}