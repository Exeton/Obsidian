﻿// This would be saved in a file called [playeruuid].dat which holds a bunch of NBT data.
// https://wiki.vg/Map_Format
using Newtonsoft.Json;
using Obsidian.API;
using Obsidian.API.Events;
using Obsidian.Blocks;
using Obsidian.Boss;
using Obsidian.Chat;
using Obsidian.Concurrency;
using Obsidian.Items;
using Obsidian.Net;
using Obsidian.Net.Packets.Play.Client;
using Obsidian.Util;
using Obsidian.Util.Extensions;
using Obsidian.Util.Registry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Obsidian.Entities
{
    public class Player : Living, IPlayer
    {
        internal readonly Client client;

        public IServer Server => client.Server;
        public bool IsOperator => Server.Operators.IsOperator(this);

        public string Username { get; }

        /// <summary>
        /// The players inventory
        /// </summary>
        public Inventory Inventory { get;  }
        public Inventory OpenedInventory { get; set; }

        public Block LastInteractedBlock { get; set; }

        public Guid Uuid { get; set; }

        public PlayerBitMask PlayerBitMask { get; set; }
        public Gamemode Gamemode { get; set; }
        public Hand MainHand { get; set; } = Hand.MainHand;

        public bool Sleeping { get; set; }
        public bool Sneaking { get; set; }
        public bool Sprinting { get; set; }
        public bool FlyingWithElytra { get; set; }
        public bool InHorseInventory { get; set; }

        public bool IsDragging { get; set; }

        public short AttackTime { get; set; }
        public short DeathTime { get; set; }
        public short HurtTime { get; set; }
        public short SleepTimer { get; set; }
        public short CurrentSlot { get; set; } = 36;

        public int Ping => this.client.ping;
        public int Dimension { get; set; }
        public int FoodLevel { get; set; }
        public int FoodTickTimer { get; set; }
        public int XpLevel { get; set; }
        public int XpTotal { get; set; }

        public double HeadY { get; private set; }

        public float AdditionalHearts { get; set; } = 0;
        public float FallDistance { get; set; }
        public float FoodExhastionLevel { get; set; } // not a type, it's in docs like this
        public float FoodSaturationLevel { get; set; }
        public float XpP { get; set; } = 0; // idfk, xp points?

        public Entity LeftShoulder { get; set; }
        public Entity RightShoulder { get; set; }

        /* Missing for now:
            NbtCompound(inventory)
            NbtList(Motion)
            NbtList(Pos)
            NbtList(Rotation)
        */

        // Properties set by Obsidian (unofficial)
        // Not sure whether these should be saved to the NBT file.
        // These could be saved under nbt tags prefixed with "obsidian_"
        // As minecraft might just ignore them.
        public Permission PlayerPermissions { get; private set; } = new Permission("root");

        internal ItemStack LastClickedItem { get; set; }

        internal Player(Guid uuid, string username, Client client)
        {
            this.Uuid = uuid;
            this.Username = username;
            this.client = client;
            this.EntityId = client.id;
            this.Inventory = new Inventory(uuid);
        }

        internal override async Task UpdateAsync(Server server, Position position, bool onGround)
        {
            await base.UpdateAsync(server, position, onGround);

            this.HeadY = position.Y + 1.62;

            foreach (var entity in this.World.GetEntitiesNear(this.Location, 1))
            {
                if (entity is ItemEntity item)
                {
                    if (!item.CanPickup)
                        continue;

                    await server.BroadcastPacketWithoutQueueAsync(new CollectItem
                    {
                        CollectedEntityId = item.EntityId,
                        CollectorEntityId = this.EntityId,
                        PickupItemCount = item.Count
                    });

                    var slot = this.Inventory.AddItem(new ItemStack((short)item.Id, new ItemMeta { Name = Registry.GetItem(item.Id).TrimmedName,  })
                    {
                        Present = true,
                    });

                    await this.client.SendPacketAsync(new SetSlot
                    {
                        Slot = (short)slot,

                        WindowId = 0,

                        SlotData = this.Inventory.GetItem(slot)
                    });

                    await item.RemoveAsync();
                }
            }
        }

        internal override async Task UpdateAsync(Server server, Position position, Angle yaw, Angle pitch, bool onGround)
        {
            await base.UpdateAsync(server, position, yaw, pitch, onGround);

            this.HeadY = position.Y + 1.62;

            foreach (var entity in this.World.GetEntitiesNear(this.Location, .8))
            {
                if (entity is ItemEntity item)
                {
                    if (!item.CanPickup)
                        continue;

                    await server.BroadcastPacketWithoutQueueAsync(new CollectItem
                    {
                        CollectedEntityId = item.EntityId,
                        CollectorEntityId = this.EntityId,
                        PickupItemCount = item.Count
                    });
                    var slot = this.Inventory.AddItem(new ItemStack((short)item.Id, item.Nbt)
                    {
                        Present = true
                    });

                    await this.client.SendPacketAsync(new SetSlot
                    {
                        Slot = (short)slot,

                        WindowId = 0,

                        SlotData = this.Inventory.GetItem(slot)
                    });

                    await item.RemoveAsync();
                }
            }
        }

        internal override async Task UpdateAsync(Server server, Angle yaw, Angle pitch, bool onGround)
        {
            await base.UpdateAsync(server, yaw, pitch, onGround);

            foreach (var entity in this.World.GetEntitiesNear(this.Location, 2))
            {
                if (entity is ItemEntity item)
                {
                    await server.BroadcastPacketWithoutQueueAsync(new CollectItem
                    {
                        CollectedEntityId = item.EntityId,
                        CollectorEntityId = this.EntityId,
                        PickupItemCount = item.Count
                    });

                    var slot = this.Inventory.AddItem(new ItemStack((short)item.Id, item.Nbt)
                    {
                        Present = true
                    });

                    await this.client.SendPacketAsync(new SetSlot
                    {
                        Slot = (short)slot,

                        WindowId = 0,

                        SlotData = this.Inventory.GetItem(slot)
                    });
                    _ = Task.Run(() => item.RemoveAsync());
                }
            }
        }

        public ItemStack GetHeldItem() => this.Inventory.GetItem(this.CurrentSlot);

        public void LoadPerms()
        {
            // Load a JSON file that contains all permissions
            var server = (Server)this.Server;
            var dir = Path.Combine($"Server-{server.Id}", "permissions");
            var user = server.Config.OnlineMode ? this.Uuid.ToString() : this.Username;
            var file = Path.Combine(dir, $"{user}.json");

            if (File.Exists(file))
                this.PlayerPermissions = JsonConvert.DeserializeObject<Permission>(File.ReadAllText(file));
        }

        public void SavePerms()
        {
            // Save permissions to JSON file
            var server = (Server)this.Server;
            var dir = Path.Combine($"Server-{server.Id}", "permissions");
            var user = server.Config.OnlineMode ? this.Uuid.ToString() : this.Username;
            var file = Path.Combine(dir, $"{user}.json");

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (!File.Exists(file))
                File.Create(file).Close();

            File.WriteAllText(file, JsonConvert.SerializeObject(this.PlayerPermissions, Formatting.Indented));
        }

        public async Task TeleportAsync(Position pos)
        {
            this.LastLocation = this.Location;
            this.Location = pos;
            await this.client.Server.World.ResendBaseChunksAsync(this.client);

            var tid = Globals.Random.Next(0, 999);

            await client.Server.Events.InvokePlayerTeleportedAsync(
                new PlayerTeleportEventArgs
                (
                    this,
                    this.Location,
                    pos
                ));

            await this.client.QueuePacketAsync(new ClientPlayerPositionLook
            {
                Position = pos,
                Flags = PositionFlags.NONE,
                TeleportId = tid
            });
            this.TeleportId = tid;

        }

        public async Task TeleportAsync(IPlayer to) => await TeleportAsync(to as Player);
        public async Task TeleportAsync(Player to)
        {
            LastLocation = this.Location;
            this.Location = to.Location;
            await this.client.Server.World.ResendBaseChunksAsync(this.client);
            var tid = Globals.Random.Next(0, 999);
            await this.client.QueuePacketAsync(new ClientPlayerPositionLook
            {
                Position = to.Location,
                Flags = PositionFlags.NONE,
                TeleportId = tid
            });
            this.TeleportId = tid;
        }

        public Task SendMessageAsync(string message, MessageType type = MessageType.Chat, Guid? sender = null) => client.QueuePacketAsync(new ChatMessagePacket(ChatMessage.Simple(message), type, sender ?? Guid.Empty));

        public Task SendMessageAsync(IChatMessage message, MessageType type = MessageType.Chat, Guid? sender = null)
        {
            if (!(message is ChatMessage chatMessage))
                return Task.FromException(new Exception("Message was of the wrong type or null. Expected instance supplied by IChatMessage.CreateNew."));

            return this.SendMessageAsync(chatMessage, type, sender);
        }

        public Task SendMessageAsync(ChatMessage message, MessageType type = MessageType.Chat, Guid? sender = null) =>
            client.QueuePacketAsync(new ChatMessagePacket(message, type, sender ?? Guid.Empty));

        public Task SendSoundAsync(Sounds soundId, SoundPosition position, SoundCategory category = SoundCategory.Master, float pitch = 1f, float volume = 1f) =>
            client.QueuePacketAsync(new SoundEffect(soundId, position, category, pitch, volume));

        public Task SendNamedSoundAsync(string name, SoundPosition position, SoundCategory category = SoundCategory.Master, float pitch = 1f, float volume = 1f) =>
            client.QueuePacketAsync(new NamedSoundEffect(name, position, category, pitch, volume));

        public Task SendBossBarAsync(Guid uuid, BossBarAction action) => client.QueuePacketAsync(new BossBar(uuid, action));

        public Task KickAsync(string reason) => this.client.DisconnectAsync(ChatMessage.Simple(reason));
        public Task KickAsync(IChatMessage reason)
        {
            if (reason is not ChatMessage chatMessage)
                return Task.FromException(new Exception("Message was of the wrong type or null. Expected instance supplied by IChatMessage.CreateNew."));

            return KickAsync(chatMessage);
        }
        public Task KickAsync(ChatMessage reason) => this.client.DisconnectAsync(reason);

        public override async Task WriteAsync(MinecraftStream stream)
        {
            await base.WriteAsync(stream);

            await stream.WriteEntityMetdata(14, EntityMetadataType.Float, this.AdditionalHearts);

            await stream.WriteEntityMetdata(15, EntityMetadataType.VarInt, this.XpP);

            await stream.WriteEntityMetdata(16, EntityMetadataType.Byte, (int)this.PlayerBitMask);

            await stream.WriteEntityMetdata(17, EntityMetadataType.Byte, (byte)this.MainHand);

            if (this.LeftShoulder != null)
                await stream.WriteEntityMetdata(18, EntityMetadataType.Nbt, this.LeftShoulder);

            if (this.RightShoulder != null)
                await stream.WriteEntityMetdata(19, EntityMetadataType.Nbt, this.RightShoulder);
        }

        public async Task OpenInventoryAsync(Inventory inventory)
        {
            await this.client.QueuePacketAsync(new OpenWindow(inventory));

            if (inventory.GetItems().Count() > 0)
            {
                await this.client.QueuePacketAsync(new WindowItems
                {
                    WindowId = inventory.Id,
                    Count = (short)inventory.GetItems().Length,
                    Items = inventory.GetItems().ToList()
                });
            }
        }
        public override string ToString() => this.Username;

        public async Task SetGamemodeAsync(Gamemode gamemode)
        {
            await client.ChangeGameState(ChangeGameStateReason.ChangeGamemode, (byte)gamemode);
            this.Gamemode = gamemode;
        }


        public async Task<bool> GrantPermission(string permission)
        {
            // trim and split permission string
            permission = permission.ToLower().Trim();
            string[] split = permission.Split('.');

            // Set root node and whether we created a new permission (still false)
            var parent = this.PlayerPermissions;
            var result = false;

            foreach(var i in split)
            {
                // no such child, this permission is new!
                if(!parent.Children.Any(x => x.Name == i))
                {
                    // create the new child, add it to its parent and set parent to the next value to continue the loop
                    var child = new Permission(i);
                    parent.Children.Add(child);
                    parent = child;
                    // yes, new permission!
                    result = true;
                    continue;
                }

                // child already exists, set parent to existing child to continue loop
                parent = parent.Children.First(x => x.Name == i);
            }

            this.SavePerms();

            if (result)
                await this.client.Server.Events.InvokePermissionGrantedAsync(new PermissionGrantedEventArgs(this, permission));
            return result;
        }

        public async Task<bool> RevokePermission(string permission)
        {
            // trim and split permission string
            permission = permission.ToLower().Trim();
            string[] split = permission.Split('.');

            // Set root node and whether we created a new permission (still false)
            var parent = this.PlayerPermissions;
            var childToRemove = this.PlayerPermissions;
            var result = true;

            foreach(var i in split)
            {
                if(parent.Children.Any(x => x.Name == i))
                {
                    // child exists, set its parent node and mark it to be removed
                    parent = childToRemove;
                    childToRemove = parent.Children.First(x => x.Name == i);
                    continue;
                }
                // no such child node, result false and break
                result = false;
                break;
            }

            if (result)
            {
                parent.Children.Remove(childToRemove);
                this.SavePerms();
                await this.client.Server.Events.InvokePermissionRevokedAsync(new PermissionRevokedEventArgs(this, permission));
            }
            return result;
        }

        public Task<bool> HasPermission(string permission)
        {
            // trim and split permission string
            permission = permission.ToLower().Trim();
            string[] split = permission.Split('.');

            // Set root node and whether we created a new permission (still false)
            var result = false;
            var parent = this.PlayerPermissions;

            foreach(var i in split)
            {
                if(parent.Children.Any(x => x.Name == "*"))
                {
                    // WILDCARD! all child permissions are granted here.
                    result = true;
                    break;
                }
                if(parent.Children.Any(x => x.Name == i))
                {
                    parent = parent.Children.First(x => x.Name == i);
                    result = true;
                }
                else
                {
                    // no such child. break loop and stop searching.
                    result = false;
                    break;
                }
            }

            return Task.FromResult(result);
        }
    }
}