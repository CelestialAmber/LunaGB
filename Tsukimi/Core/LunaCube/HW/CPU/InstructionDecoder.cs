using System;
using System.Linq;
using Tsukimi.Core.LunaCube.HW.CPU.Data;
using Tsukimi.Core.LunaCube.HW.CPU.Enums;
using Tsukimi.Utils;

namespace Tsukimi.Core.LunaCube.HW.CPU
{

    //Why does PPC encoding have to be such a mess :<

    public struct InstructionFields
    {
        public uint instrVal;
        public InstructionType instruction;

        //Main opcode
        public uint opcd => instrVal.GetBits(0, 5);

        /// <summary>
        /// SIMM/offset
        /// </summary>
        public int signedImmediate
        {
            get { return (short)instrVal.GetBits(16, 31); }
        }
        /// <summary>
        /// UIMM
        /// </summary>
        public uint unsignedImmedate => instrVal.GetBits(16, 31);

        //Common 5 bit argument fields

        /// <summary>
        /// BO/TO/rD/rS/frD/frS/crbD
        /// </summary>
        public uint commonArg1 => GetFiveBitField(6);
        /// <summary>
        /// BI/rA/frA/crbA
        /// </summary>
        public uint commonArg2 => GetFiveBitField(11);
        /// <summary>
        /// SH/rB/frB/crbB/NB
        /// </summary>
        public uint commonArg3 => GetFiveBitField(16);
        /// <summary>
        /// MB/frC
        /// </summary>
        public uint commonArg4 => GetFiveBitField(21);
        /// <summary>
        /// ME
        /// </summary>
        public uint commonArg5 => GetFiveBitField(26);

        //Others

        public BranchOptions BO
        {
            get { return (BranchOptions)commonArg1; }
        }

        //BI format:
        //Bits 0-3: condition register
        //Bits 4-5: condition
        //public byte BI;

        public int BD
        {
            get { return (int)instrVal.GetBits(16, 29); }
        }

        public int LI
        {
            get { return (int)(instrVal.GetBits(6, 29) << 2); }
        }

        public TrapOptions TO
        {
            get { return (TrapOptions)commonArg1; }
        }

        public uint sr => instrVal.GetBits(12, 15);

        //Split fields (lower half: bits 11-16, upper half: bits 16-20)
        public uint spr => GetSplitField();
        public uint tbr => GetSplitField();

        public uint crfD => instrVal.GetBits(6, 8);

        public uint crfS => instrVal.GetBits(11, 13);
        public uint crm => instrVal.GetBits(12, 19);

        public uint mtfsfFm => instrVal.GetBits(7, 14);
        public uint mtfsfImm => instrVal.GetBits(16, 19);
        public bool L => instrVal.GetBits(10, 10) == 1 ? true : false;

        //Paired singles

        public int psOffset
        {
            get { return (int)instrVal.GetBits(20, 31); }
        }

        public uint psI => instrVal.GetBits(17, 19);
        public uint psIX => instrVal.GetBits(22, 24);
        public uint psW => instrVal.GetBits(16, 16);
        public uint psWX => instrVal.GetBits(21, 21);

        //Mnemonic specific

        //SPRG m(t/f)spr mnemonics index field
        public uint spr_SPRG => instrVal.GetBits(14, 15);
        //BAT m(t/f)spr mnemonics index field
        public uint spr_BAT => instrVal.GetBits(13, 14);

        public InstructionFields()
        {
            instrVal = 0;
        }

        public InstructionFields(uint instrVal)
        {
            this.instrVal = instrVal;
        }

        //Utility functions

        uint GetFiveBitField(int startBit)
        {
            int endBit = startBit + 4;
            return instrVal.GetBits(startBit, endBit);
        }

        uint GetSplitField()
        {
            //Reverse the order of the two 5 bit parts
            uint val = instrVal.GetBits(11, 20);
            uint lo = val >> 5 & 0b11111;
            uint hi = (val & 0b11111);
            return (hi << 5) | lo;
        }

    }
    public class InstructionDecoder
    {
        uint instrVal;
        InstructionFields fields;

        public InstructionDecoder()
        {
            instrVal = 0;
            fields = new InstructionFields();
        }

        public InstructionFields GetFields()
        {
            return fields;
        }

        public void DecodeInstruction(uint value)
        {
            instrVal = value;
            fields.instrVal = instrVal;
            fields.instruction = FindInstructionType();
        }

        InstructionType FindInstructionType()
        {
            uint opcode = fields.opcd;
            //Shift the opcode value left by 10 to match the enum
            InstructionType type = (InstructionType)(opcode << 10);

            //First, check if the opcode matches any of the unique opcodes
            if (InstructionGroups.uniqueOpcodeGroup.Contains(type)) return type;


            //If not, check if it matches any of the other instruction groups

            //Get the corresponding group array if the opcode matches one of the
            //group values. If not, then the opcode is invalid.
            InstructionType[]? groupArray = GetGroupArray(opcode);

            if (groupArray == null) return InstructionType.Illegal;

            //Check each instruction in the group to find a match, using the bitmask/pattern
            //values.
            for(int i = 0; i < groupArray.Length; i++)
            {
                InstructionType curType = groupArray[i];
                Instruction instruction = InstructionData.GetInstruction(curType);

                uint bitmask = instruction.bitmask;
                uint pattern = instruction.pattern;

                //If the masked instruction matches the pattern, we found a match
                if((instrVal & bitmask) == pattern)
                {
                    return curType;
                }
            }
            
            Console.WriteLine("Somehow reached here :<");
            return InstructionType.Illegal;

        }

        InstructionType[]? GetGroupArray(uint opcode)
        {
            switch (opcode)
            {
                case 4:
                    return InstructionGroups.instructionGroup4;
                case 19:
                    return InstructionGroups.instructionGroup19;
                case 31:
                    return InstructionGroups.instructionGroup31;
                case 59:
                    return InstructionGroups.instructionGroup59;
                case 63:
                    return InstructionGroups.instructionGroup63;
                default:
                    return null;
            }
        }

    }
}
