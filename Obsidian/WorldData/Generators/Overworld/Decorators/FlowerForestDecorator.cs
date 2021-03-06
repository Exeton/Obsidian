﻿using Obsidian.API;
using Obsidian.Blocks;
using Obsidian.ChunkData;
using Obsidian.Util.Registry;
using System;

namespace Obsidian.WorldData.Generators.Overworld.Decorators
{
    public class FlowerForestDecorator : BaseDecorator
    {
        public FlowerForestDecorator(Biomes biome) : base(biome)
        {
        }

        public override void Decorate(Chunk chunk, Position pos, OverworldNoise noise)
        {
            int worldX = (chunk.X << 4) + (int)pos.X;
            int worldZ = (chunk.Z << 4) + (int)pos.Z;

            // Flowers
            var grassNoise = noise.Decoration(worldX * 0.1, 8, worldZ * 0.1);
            if (grassNoise > 0 && grassNoise < 0.5) // 50% chance for grass
                chunk.SetBlock(pos + (0, 1, 0), Registry.GetBlock(Materials.Grass));

            if (noise.Decoration(worldX * 0.1, 6, worldZ * 0.1) > 0.8)
                chunk.SetBlock(pos + (0, 1, 0), Registry.GetBlock(Materials.SweetBerryBush));

            if (noise.Decoration(worldX * 0.1, 7, worldZ * 0.1) > 1)
                chunk.SetBlock(pos + (0, 1, 0), Registry.GetBlock(Materials.TallGrass));

            var poppyNoise = noise.Decoration(worldX * 0.1, 9, worldZ * 0.1); // 0.1 makes more groupings
            if (poppyNoise > 1)
                chunk.SetBlock(pos + (0, 1, 0), Registry.GetBlock(Materials.Poppy));

            var dandyNoise = noise.Decoration(worldX * 0.1, 10, worldZ * 0.1); // 0.1 makes more groupings
            if (dandyNoise > 1)
                chunk.SetBlock(pos + (0, 1, 0), Registry.GetBlock(Materials.Dandelion));

            var cornFlowerNoise = noise.Decoration(worldX * 0.1, 11, worldZ * 0.1); // 0.1 makes more groupings
            if (cornFlowerNoise > 1)
                chunk.SetBlock(pos + (0, 1, 0), Registry.GetBlock(Materials.Cornflower));

            var azureNoise = noise.Decoration(worldX * 0.1, 12, worldZ * 0.1); // 0.1 makes more groupings
            if (azureNoise > 1)
                chunk.SetBlock(pos + (0, 1, 0), Registry.GetBlock(Materials.AzureBluet));

            var peonies = noise.Decoration(worldX * 0.1, 13, worldZ * 0.1); // 0.1 makes more groupings
            if (peonies > 1)
                chunk.SetBlock(pos + (0, 1, 0), Registry.GetBlock(Materials.Peony));

            if (noise.Decoration(worldX * 0.1, 14, worldZ * 0.1) > 1)
                chunk.SetBlock(pos + (0, 1, 0), Registry.GetBlock(Materials.WhiteTulip));
            if (noise.Decoration(worldX * 0.1, 15, worldZ * 0.1) > 1)
                chunk.SetBlock(pos + (0, 1, 0), Registry.GetBlock(Materials.RedTulip));
            if (noise.Decoration(worldX * 0.1, 16, worldZ * 0.1) > 1)
                chunk.SetBlock(pos + (0, 1, 0), Registry.GetBlock(Materials.OrangeTulip));
            if (noise.Decoration(worldX * 0.1, 17, worldZ * 0.1) > 1)
                chunk.SetBlock(pos + (0, 1, 0), Registry.GetBlock(Materials.Allium));
            if (noise.Decoration(worldX * 0.1, 18, worldZ * 0.1) > 1)
                chunk.SetBlock(pos + (0, 1, 0), Registry.GetBlock(Materials.LilyOfTheValley));
            if (noise.Decoration(worldX * 0.1, 19, worldZ * 0.1) > 1)
                chunk.SetBlock(pos + (0, 1, 0), Registry.GetBlock(Materials.BlueOrchid));
            if (noise.Decoration(worldX * 0.1, 19, worldZ * 0.1) > 1)
                chunk.SetBlock(pos + (0, 1, 0), Registry.GetBlock(Materials.Lilac));
            if (noise.Decoration(worldX * 0.1, 19, worldZ * 0.1) > 1)
                chunk.SetBlock(pos + (0, 1, 0), Registry.GetBlock(Materials.RoseBush));


            #region Trees
            // Abandon hope all ye who enter here
            var oakLeaves = Registry.GetBlock(Materials.OakLeaves);
            var treeNoise = noise.Decoration(worldX * 0.1, 13, worldZ * 0.1);
            var treeHeight = TreeHeight(treeNoise);
            if (treeHeight > 0)
            {
                //Leaves
                for (int y = treeHeight + 1; y > treeHeight - 2; y--)
                {
                    for (double x = pos.X - 2; x <= pos.X + 2; x++)
                    {
                        for (double z = pos.Z - 2; z <= pos.Z + 2; z++)
                        {
                            var loc = new Position(x, y + pos.Y, z).ChunkClamp();
                            // Skip the top edges.
                            if (y == treeHeight + 1)
                            {
                                if (x != pos.X - 2 && x != pos.X + 2 && z != pos.Z - 2 && z != pos.Z + 2)
                                    chunk.SetBlock(loc, oakLeaves);
                            }
                            else
                            {
                                chunk.SetBlock(loc, oakLeaves);
                            }
                        }
                    }
                }
                for (int y = 1; y <= treeHeight; y++)
                {
                    chunk.SetBlock(pos + (0, y, 0), new Block("minecraft:oak_log", 74, Materials.OakLog));
                }
            }

            // If on the edge of the chunk, check if neighboring chunks need leaves.
            if (pos.X == 0)
            {
                // Check out to 2 blocks into the neighboring chunk's noisemap and see if there's a tree center (top log)
                var neighborTree1 = TreeHeight(noise.Decoration((worldX - 1) * 0.1, 13, worldZ * 0.1));
                var neighborTree2 = TreeHeight(noise.Decoration((worldX - 2) * 0.1, 13, worldZ * 0.1));
                var rowsToDraw = neighborTree1 > 0 ? 2 : neighborTree2 > 0 ? 1 : 0;
                var treeY = Math.Max(neighborTree1, neighborTree2) + (int)noise.Terrain(worldX - 1, worldZ);

                for (int x = 0; x < rowsToDraw; x++)
                {
                    for (double z = pos.Z - 2; z <= pos.Z + 2; z++)
                    {
                        for (int y = treeY + 1; y > treeY - 2; y--)
                        {
                            var loc = new Position(x, y, z).ChunkClamp();
                            // Skip the top edges.
                            if (y == treeY + 1)
                            {
                                if (x != rowsToDraw - 1 && z != pos.Z - 2 && z != pos.Z + 2)
                                    chunk.SetBlock(loc, oakLeaves);
                            }
                            else
                            {
                                chunk.SetBlock(loc, oakLeaves);
                            }
                        }
                    }
                }
            }
            else if (pos.X == 15)
            {
                // Check out to 2 blocks into the neighboring chunk's noisemap and see if there's a tree center (top log)
                var neighborTree1 = TreeHeight(noise.Decoration((worldX + 1) * 0.1, 13, worldZ * 0.1));
                var neighborTree2 = TreeHeight(noise.Decoration((worldX + 2) * 0.1, 13, worldZ * 0.1));
                var rowsToDraw = neighborTree1 > 0 ? 2 : neighborTree2 > 0 ? 1 : 0;
                var treeY = Math.Max(neighborTree1, neighborTree2) + (int)noise.Terrain(worldX + 1, worldZ);

                for (int x = 15; x > 15 - rowsToDraw; x--)
                {
                    for (double z = pos.Z - 2; z <= pos.Z + 2; z++)
                    {
                        for (int y = treeY + 1; y > treeY - 2; y--)
                        {
                            var loc = new Position(x, y, z).ChunkClamp();
                            // Skip the top edges.
                            if (y == treeY + 1)
                            {
                                if (x != 16 - rowsToDraw && z != pos.Z - 2 && z != pos.Z + 2)
                                    chunk.SetBlock(loc, oakLeaves);
                            }
                            else
                            {
                                chunk.SetBlock(loc, oakLeaves);
                            }
                        }
                    }
                }
            }
            if (pos.Z == 0)
            {
                // Check out to 2 blocks into the neighboring chunk's noisemap and see if there's a tree center (top log)
                var neighborTree1 = TreeHeight(noise.Decoration(worldX * 0.1, 13, (worldZ - 1) * 0.1));
                var neighborTree2 = TreeHeight(noise.Decoration(worldX * 0.1, 13, (worldZ - 2) * 0.1));
                var rowsToDraw = neighborTree1 > 0 ? 2 : neighborTree2 > 0 ? 1 : 0;
                var treeY = Math.Max(neighborTree1, neighborTree2) + (int)noise.Terrain(worldX, worldZ - 1);

                for (int z = 0; z < rowsToDraw; z++)
                {
                    for (double x = pos.X - 2; x <= pos.X + 2; x++)
                    {
                        for (int y = treeY + 1; y > treeY - 2; y--)
                        {
                            var loc = new Position(x, y, z).ChunkClamp();
                            // Skip the top edges.
                            if (y == treeY + 1)
                            {
                                if (x != pos.X - 2 && x != pos.X + 2 && z != rowsToDraw - 1)
                                    chunk.SetBlock(loc, oakLeaves);
                            }
                            else
                            {
                                chunk.SetBlock(loc, oakLeaves);
                            }
                        }
                    }
                }
            }
            else if (pos.Z == 15)
            {
                // Check out to 2 blocks into the neighboring chunk's noisemap and see if there's a tree center (top log)
                var neighborTree1 = TreeHeight(noise.Decoration(worldX * 0.1, 13, (worldZ + 1) * 0.1));
                var neighborTree2 = TreeHeight(noise.Decoration(worldX * 0.1, 13, (worldZ + 2) * 0.1));
                var rowsToDraw = neighborTree1 > 0 ? 2 : neighborTree2 > 0 ? 1 : 0;
                var treeY = Math.Max(neighborTree1, neighborTree2) + (int)noise.Terrain(worldX, worldZ + 1);

                for (int z = 15; z > 15 - rowsToDraw; z--)
                {
                    for (double x = pos.X - 2; x <= pos.X + 2; x++)
                    {
                        for (int y = treeY + 1; y > treeY - 2; y--)
                        {
                            var loc = new Position(x, y, z).ChunkClamp();
                            // Skip the top edges.
                            if (y == treeY + 1)
                            {
                                if (x != pos.X - 2 && x != pos.X + 2 && z != 16 - rowsToDraw)
                                    chunk.SetBlock(loc, oakLeaves);
                            }
                            else
                            {
                                chunk.SetBlock(loc, oakLeaves);
                            }
                        }
                    }
                }
            }
            #endregion
        }

        private static int TreeHeight(double value)
        {
            return value > 0.04 && value <= 0.05 ? (int)(value * 100) : 0;
        }
    }
}
