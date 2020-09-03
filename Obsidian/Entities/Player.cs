﻿// This would be saved in a file called [playeruuid].dat which holds a bunch of NBT data.
// https://wiki.vg/Map_Format
using Obsidian.Boss;
using Obsidian.Chat;
using Obsidian.Concurrency;
using Obsidian.Items;
using Obsidian.Net;
using Obsidian.Net.Packets.Play;
using Obsidian.PlayerData;
using Obsidian.Sounds;
using Obsidian.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Obsidian.Entities
{
    public class Player : Living
    {
        internal readonly Client client;

        public Inventory Inventory { get; private set; } = new Inventory();

        public Inventory OpenedInventory { get; set; }

        public Guid Uuid { get; set; }


        internal Position LastPosition { get; set; } = new Position();

        internal Angle LastPitch { get; set; }

        internal Angle LastYaw { get; set; }


        public Position Position { get; set; } = new Position();

        public Angle Pitch { get; set; }

        public Angle Yaw { get; set; }


        // Properties set by Minecraft (official)
        public PlayerBitMask PlayerBitMask { get; set; }

        public bool OnGround { get; set; }
        public bool Sleeping { get; set; }

        public short AttackTime { get; set; }
        public short DeathTime { get; set; }
        public short HurtTime { get; set; }
        public short SleepTimer { get; set; }
        public short HeldItemSlot { get; set; }

        public Gamemode Gamemode { get; set; }

        public int Ping => this.client.ping;

        public int Dimension { get; set; }
        public int FoodLevel { get; set; }
        public int FoodTickTimer { get; set; }
        public int XpLevel { get; set; }
        public int XpTotal { get; set; }

        public float AdditionalHearts { get; set; } = 0;
        public float FallDistance { get; set; }
        public float FoodExhastionLevel { get; set; } // not a type, it's in docs like this
        public float FoodSaturationLevel { get; set; }
        public float XpP { get; set; } = 0; // idfk, xp points?

        public Hand MainHand { get; set; } = Hand.MainHand;

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
        public ConcurrentHashSet<string> Permissions { get; } = new ConcurrentHashSet<string>();

        public string Username { get; }

        public World.World World;

        internal Player(Guid uuid, string username, Client client)
        {
            this.Uuid = uuid;
            this.Username = username;
            this.client = client;
        }

        public void UpdatePosition(Position pos, bool onGround = true)
        {
            this.CopyPosition();
            this.Position.X = pos.X;
            this.Position.Y = pos.Y;
            this.Position.Z = pos.Z;
            this.OnGround = onGround;
        }

        public void UpdatePosition(Position pos, Angle pitch, Angle yaw, bool onGround = true)
        {
            this.CopyPosition(true);
            this.Position.X = pos.X;
            this.Position.Y = pos.Y;
            this.Position.Z = pos.Z;
            this.Pitch = pitch;
            this.Yaw = yaw;
            this.OnGround = onGround;
        }

        public void UpdatePosition(double x, double y, double z, bool onGround = true)
        {
            this.CopyPosition();
            this.Position.X = x;
            this.Position.Y = y;
            this.Position.Z = z;
            this.OnGround = onGround;
        }

        public void UpdatePosition(Angle pitch, Angle yaw, bool onGround = true)
        {
            this.CopyLook();
            this.Pitch = pitch;
            this.Yaw = yaw;
            this.OnGround = onGround;
        }

        private void CopyPosition(bool withLook = false)
        {
            this.LastPosition.X = this.Position.X;
            this.LastPosition.Y = this.Position.Y;
            this.LastPosition.Z = this.Position.Z;

            if (withLook)
            {
                this.LastYaw = this.Yaw;
                this.LastPitch = this.Pitch;
            }
            
            this.LastPosition = this.Position;
        }

        private void CopyLook()
        {
            this.LastYaw = this.Yaw;
            this.LastPitch = this.Pitch;
        }

        public Task SendMessageAsync(string message, sbyte position = 0) => client.QueuePacketAsync(new ChatMessagePacket(ChatMessage.Simple(message), position));

        public Task SendMessageAsync(ChatMessage message) => client.QueuePacketAsync(new ChatMessagePacket(message, 0));

        public Task SendSoundAsync(int soundId, SoundPosition position, SoundCategory category = SoundCategory.Master, float pitch = 1f, float volume = 1f) => client.QueuePacketAsync(new SoundEffect(soundId, position, category, pitch, volume));

        public Task SendNamedSoundAsync(string name, SoundPosition position, SoundCategory category = SoundCategory.Master, float pitch = 1f, float volume = 1f) => client.QueuePacketAsync(new NamedSoundEffect(name, position, category, pitch, volume));

        public Task SendBossBarAsync(Guid uuid, BossBarAction action) => client.QueuePacketAsync(new BossBar(uuid, action));

        public Task KickAsync(string reason) => this.client.DisconnectAsync(ChatMessage.Simple(reason));

        public void LoadPerms(List<string> permissions)
        {
            foreach (var perm in permissions)
            {
                Permissions.Add(perm);
            }
        }

        public Task DisconnectAsync(ChatMessage reason) => this.client.DisconnectAsync(reason);

        public override async Task WriteAsync(MinecraftStream stream)
        {
            await stream.WriteEntityMetdata(11, EntityMetadataType.Float, AdditionalHearts);

            await stream.WriteEntityMetdata(12, EntityMetadataType.VarInt, XpP);

            await stream.WriteEntityMetdata(13, EntityMetadataType.Byte, (int)PlayerBitMask);

            await stream.WriteEntityMetdata(14, EntityMetadataType.Byte, (byte)1);
        }
    }

    [Flags]
    public enum PlayerBitMask : byte
    {
        Unused = 0x80,

        CapeEnabled = 0x01,
        JacketEnabled = 0x02,

        LeftSleeveEnabled = 0x04,
        RightSleeveEnabled = 0x08,

        LeftPantsLegEnabled = 0x10,
        RIghtPantsLegEnabled = 0x20,

        HatEnabled = 0x40
    }
}