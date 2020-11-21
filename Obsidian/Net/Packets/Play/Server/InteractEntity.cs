using Obsidian.API;
using Obsidian.API._Types;
using Obsidian.Entities;
using Obsidian.Serializer.Attributes;
using Obsidian.Serializer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obsidian.Net.Packets.Play.Server
{
    class InteractEntity : IPacket
    {
        public int Id => 0x0E;

        [Field(0, Type = DataType.VarInt)]
        public int EntityId { get; set; }

        [Field(1, Type = DataType.VarInt)]
        public InteractionType Type { get; set; }

        [Field(2, Type = DataType.Float)]
        public float? TargetX { get; set; }

        [Field(3, Type = DataType.Float)]
        public float? TargetY { get; set; }

        [Field(4, Type = DataType.Float)]
        public float? TargetZ { get; set; }

        [Field(5, Type = DataType.VarInt)]
        public Hand? Hand { get; set; }

        [Field(6, Type = DataType.Boolean)]
        public bool Sneaking { get; set; }

        public Task WriteAsync(MinecraftStream stream) => Task.CompletedTask;

        public async Task ReadAsync(MinecraftStream stream)
        {
            EntityId = await stream.ReadVarIntAsync();
            Type = (InteractionType)await stream.ReadVarIntAsync();
            if (Type == InteractionType.InteractAt)
            {
                TargetX = await stream.ReadFloatAsync();
                TargetY = await stream.ReadFloatAsync();
                TargetZ = await stream.ReadFloatAsync();
                Hand = (Hand)(Convert.ToInt32(stream.ReadBooleanAsync()));//How does await work with cast?
            }
            Sneaking = await stream.ReadBooleanAsync();
        }

        public Task HandleAsync(Obsidian.Server server, Player player)
        {

            Console.WriteLine("Interaction type: " + Type);
            if (Type == InteractionType.Attack)
            {
                //throw new NotImplementedException();
            }
            else if (Type == InteractionType.Interact)
            {

            }
            else if (Type == InteractionType.InteractAt)
            {

            }
            return Task.CompletedTask;

        }
    }
}
