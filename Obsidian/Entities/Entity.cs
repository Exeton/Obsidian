﻿using Obsidian.API;
using Obsidian.Chat;
using Obsidian.Net;
using Obsidian.Net.Packets.Play.Client;
using Obsidian.WorldData;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Timers;

namespace Obsidian.Entities
{
    public class Entity : IEquatable<Entity>, IEntity
    {
        public readonly Timer TickTimer = new Timer();

        public World World { get; set; }
        public IWorld WorldLocation => World;

        #region Location properties
        internal Position LastLocation { get; set; } = new Position();

        internal Angle LastPitch { get; set; }

        internal Angle LastYaw { get; set; }

        internal int TeleportId { get; set; }

        public Position Location { get; set; } = new Position();

        public Angle Pitch { get; set; }

        public Angle Yaw { get; set; }

        public Velocity velocity { get; set; } = new Velocity(0, 0, 0);
        #endregion Location properties


        private static int EntityIdCounter = 50;
        private static object EntityIdLock = new object();

        public int EntityId { get; internal set; }

        public Guid EntityUUID { get; set; } = Guid.NewGuid();

        public EntityBitMask EntityBitMask { get; set; }

        public Pose Pose { get; set; } = Pose.Standing;

        public int Air { get; set; } = 300;

        public ChatMessage CustomName { get; private set; }

        public bool CustomNameVisible { get; private set; }
        public bool Silent { get; private set; }
        public bool NoGravity { get; set; }
        public bool OnGround { get; set; }

        public Entity() 
        {
            lock (EntityIdLock)
            {
                EntityId = ++EntityIdCounter;
            }
        }

        #region Update methods
        internal virtual async Task UpdateAsync(Server server, Position position, bool onGround)
        {
            var newPos = position * 32 * 64;
            var lastPos = this.LastLocation * 32 * 64;

            short newX = (short)(newPos.X - lastPos.X);
            short newY = (short)(newPos.Y - lastPos.Y);
            short newZ = (short)(newPos.Z - lastPos.Z);

            var isNewLocation = position != this.LastLocation;

            if (isNewLocation)
            {
                await server.BroadcastPacketWithoutQueueAsync(new EntityPosition
                {
                    EntityId = this.EntityId,

                    DeltaX = newX,
                    DeltaY = newY,
                    DeltaZ = newZ,

                    OnGround = onGround
                }, this.EntityId);

                this.UpdatePosition(position, onGround);
            }
        }

        internal virtual async Task UpdateAsync(Server server, Angle yaw, Angle pitch, bool onGround)
        {
            var isNewRotation = yaw != this.LastYaw || pitch != this.LastPitch;

            if (isNewRotation)
            {
                await server.BroadcastPacketWithoutQueueAsync(new EntityRotation
                {
                    EntityId = this.EntityId,
                    OnGround = onGround,
                    Yaw = yaw,
                    Pitch = pitch
                }, this.EntityId);

                this.CopyLook();
                this.UpdatePosition(yaw, pitch, onGround);
            }
        }

        internal virtual async Task UpdateAsync(Server server, Position position, Angle yaw, Angle pitch, bool onGround)
        {
            var newPos = position * 32 * 64;
            var lastPos = this.LastLocation * 32 * 64;

            short newX = (short)(newPos.X - lastPos.X);
            short newY = (short)(newPos.Y - lastPos.Y);
            short newZ = (short)(newPos.Z - lastPos.Z);

            var isNewLocation = position != this.LastLocation;

            var isNewRotation = yaw != this.LastYaw || pitch != this.LastPitch;


            if (isNewLocation)
            {
                if (isNewRotation)
                {
                    await server.BroadcastPacketWithoutQueueAsync(new EntityPositionAndRotation
                    {
                        EntityId = this.EntityId,

                        DeltaX = newX,
                        DeltaY = newY,
                        DeltaZ = newZ,

                        Yaw = yaw,

                        Pitch = pitch,

                        OnGround = onGround
                    }, this.EntityId);

                    this.CopyLook();
                }
                else
                {
                    await server.BroadcastPacketWithoutQueueAsync(new EntityPosition
                    {
                        EntityId = this.EntityId,

                        DeltaX = newX,
                        DeltaY = newY,
                        DeltaZ = newZ,

                        OnGround = onGround
                    }, this.EntityId);
                }

                this.UpdatePosition(position, yaw, pitch, onGround);
            }
        }

        internal void CopyPosition(bool withLook = false)
        {
            this.LastLocation = this.Location;

            if (withLook)
                this.CopyLook();
        }

        internal void CopyLook()
        {
            this.LastYaw = this.Yaw;
            this.LastPitch = this.Pitch;
        }

        public void UpdatePosition(Position pos, bool onGround = true)
        {
            this.CopyPosition();
            this.Location = pos;
            this.OnGround = onGround;
        }

        public void UpdatePosition(Position pos, Angle yaw, Angle pitch, bool onGround = true)
        {
            this.CopyPosition(true);
            this.Location = pos;
            this.Yaw = yaw;
            this.Pitch = pitch;
            this.OnGround = onGround;
        }

        public void UpdatePosition(double x, double y, double z, bool onGround = true)
        {
            this.CopyPosition();
            this.Location = new Position(x, y, z);
            this.OnGround = onGround;
        }

        public void UpdatePosition(Angle yaw, Angle pitch, bool onGround = true)
        {
            this.CopyLook();
            this.Yaw = yaw;
            this.Pitch = pitch;
            this.OnGround = onGround;
        }
        #endregion

        public Task RemoveAsync() => this.World.DestroyEntityAsync(this);

        public virtual async Task WriteAsync(MinecraftStream stream)
        {
            await stream.WriteEntityMetdata(0, EntityMetadataType.Byte, (byte)this.EntityBitMask);

            await stream.WriteEntityMetdata(1, EntityMetadataType.VarInt, this.Air);

            await stream.WriteEntityMetdata(2, EntityMetadataType.OptChat, this.CustomName, this.CustomName != null);

            await stream.WriteEntityMetdata(3, EntityMetadataType.Boolean, this.CustomNameVisible);
            await stream.WriteEntityMetdata(4, EntityMetadataType.Boolean, this.Silent);
            await stream.WriteEntityMetdata(5, EntityMetadataType.Boolean, this.NoGravity);
            await stream.WriteEntityMetdata(6, EntityMetadataType.Pose, this.Pose);
        }

        public virtual Task TickAsync() => Task.CompletedTask;

        public bool Equals([AllowNull] Entity other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return this.EntityId == other.EntityId;
        }

        public override bool Equals(object obj) => Equals(obj as Entity);

        public static implicit operator int(Entity entity) => entity.EntityId;

        public static bool operator ==(Entity a, Entity b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }
        public static bool operator !=(Entity a, Entity b) => !(a == b);

        public override int GetHashCode() => this.EntityId.GetHashCode();
    }
}