﻿using Obsidian.API;
using Obsidian.Entities;
using Obsidian.Serializer.Attributes;
using System.Threading.Tasks;

namespace Obsidian.Net.Packets.Play.Client
{
    public class EntityRotation : IPacket
    {
        [Field(0, Type = Serializer.Enums.DataType.VarInt)]
        public int EntityId { get; set; }

        [Field(1)]
        public Angle Yaw { get; set; }

        [Field(2)]
        public Angle Pitch { get; set; }

        [Field(3)]
        public bool OnGround { get; set; }

        public int Id => 0x29;

        public EntityRotation() { }

        public Task WriteAsync(MinecraftStream stream) => Task.CompletedTask;

        public Task ReadAsync(MinecraftStream stream) => Task.CompletedTask;

        public Task HandleAsync(Obsidian.Server server, Player player) => Task.CompletedTask;
    }
}
