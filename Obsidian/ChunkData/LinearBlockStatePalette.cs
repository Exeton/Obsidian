﻿using Obsidian.Blocks;
using Obsidian.Net;
using Obsidian.Util.Registry;
using System;
using System.Threading.Tasks;

namespace Obsidian.ChunkData
{
    public class LinearBlockStatePalette : IBlockStatePalette
    {
        public Block[] BlockStateArray { get; set; }
        public int BlockStateCount { get; set; }

        public bool IsFull => this.BlockStateArray.Length == this.BlockStateCount;

        public LinearBlockStatePalette(byte bitCount)
        {
            this.BlockStateArray = new Block[1 << bitCount];
        }

        public int GetIdFromState(Block blockState)
        {
            if (blockState is null)
                throw new ArgumentNullException(nameof(blockState));

            for (int id = 0; id < BlockStateCount; id++)
            {
                if (this.BlockStateArray[id].Id == blockState.Id)
                    return id;
            }

            if (this.IsFull)
                return -1;

            // Add to palette
            int newId = this.BlockStateCount;
            this.BlockStateArray[newId] = blockState;
            this.BlockStateCount++;
            return newId;
        }

        public Block GetStateFromIndex(int index)
        {
            if (index > this.BlockStateCount - 1 || index < 0)
                return null;

            return this.BlockStateArray[index];
        }

        public async Task WriteToAsync(MinecraftStream stream)
        {
            await stream.WriteVarIntAsync(this.BlockStateCount);

            for (int i = 0; i < this.BlockStateCount; i++)
                await stream.WriteVarIntAsync(this.BlockStateArray[i].Id);
        }

        public async Task ReadFromAsync(MinecraftStream stream)
        {
            var length = await stream.ReadVarIntAsync();

            for (int i = 0; i < length; i++)
            {
                int stateId = await stream.ReadVarIntAsync();

                Block blockState = Registry.GetBlock(stateId);

                this.BlockStateArray[i] = blockState;
                this.BlockStateCount++;
            }
        }
    }
}
