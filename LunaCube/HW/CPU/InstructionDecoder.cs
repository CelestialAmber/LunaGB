using System;
using System.Linq;
using LunaCube.HW.CPU.Data;
using LunaCube.HW.CPU.Enums;
using Tsukimi.Utils;

namespace LunaCube.HW.CPU
{
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
