﻿using Obsidian.Entities;
using Obsidian.Serializer.Attributes;
using Obsidian.Serializer.Enums;
using System.Threading.Tasks;

namespace Obsidian.Net.Packets.Play.Client
{
    public class ChangeGameState : IPacket
    {
        [Field(0, Type = DataType.UnsignedByte)]

        public ChangeGameStateReason Reason { get; set; }

        [Field(1, Type = DataType.Float)]
        public float Value { get; set; }

        public int Id => 0x1D;

        public ChangeGameState() { }

        public ChangeGameState(ChangeGameStateReason reason, float value)
        {
            this.Reason = reason;
            this.Value = value;
        }

        public Task WriteAsync(MinecraftStream stream) => Task.CompletedTask;

        public Task ReadAsync(MinecraftStream stream) => Task.CompletedTask;

        public Task HandleAsync(Obsidian.Server server, Player player) => Task.CompletedTask;
    }
    public enum ChangeGameStateReason : byte
    {
        NoRespawnBlockAvailable,

        EndRaining,
        BeginRaining,

        ChangeGamemode,

        WinGame,

        DemoEvent,

        ArrowHitPlayer,

        RainLevelChange,
        ThunderLevelChange,

        PlayerPufferfishStingSound,

        PlayElderGuardianMobAppearance,

        EnableRespawnScreen
    }
}