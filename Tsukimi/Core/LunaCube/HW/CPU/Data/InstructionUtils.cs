using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsukimi.Core.LunaCube.HW.CPU.Data
{
    internal class InstructionUtils
    {
        public static bool IsBranchInstruction(InstructionType type)
        {
            return type == InstructionType.B || type == InstructionType.Bc || type == InstructionType.Bclr || type == InstructionType.Bcctr;
        }

        public static bool IsConditionalBranchInstruction(InstructionType type)
        {
            return type == InstructionType.Bc || type == InstructionType.Bclr || type == InstructionType.Bcctr;
        }
    }
}
