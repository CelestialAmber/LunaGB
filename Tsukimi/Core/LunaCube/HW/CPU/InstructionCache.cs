using System.Collections.Generic;
using Tsukimi.Core.LunaCube.HW.CPU.Data;

namespace Tsukimi.Core.LunaCube.HW.CPU
{
    internal struct CachedInstruction
    {
        public InstructionType type;
        public uint value;

        public CachedInstruction(InstructionType type, uint value)
        {
            this.type = type;
            this.value = value;
        }
    }

    internal struct CacheBlock
    {
        public List<CachedInstruction> instructions;

        public void AddInstruction(uint instruction)
        {

        }
    }

    /* Stores a list of already decoded instructions, which allows the CPU to only need to decode new address instructions.
    Since decoding takes a decent amount of time, this is important to speed up the CPU emulation. */
    internal struct InstructionCache
    {
        //Cache block dictionary which uses the instruction address as the key.
        Dictionary<uint, CacheBlock> cachedBlocks;

        public InstructionCache()
        {
            cachedBlocks = new Dictionary<uint, CacheBlock>();
        }

        public void AddToCache(uint instrVal, InstructionType instrType)
        {
            CachedInstruction instruction = new CachedInstruction(instrType, instrVal);
        }

        //Checks if a cache block already exists for the given address. 
        public bool CheckIfDecoded(uint address)
        {
            return cachedBlocks.ContainsKey(address);
        }

        public CacheBlock GetCacheBlock(uint address)
        {
            return cachedBlocks[address];
        }

        //Clears the instruction cache.
        public void Clear()
        {
            cachedBlocks.Clear();
        }
    }
}
