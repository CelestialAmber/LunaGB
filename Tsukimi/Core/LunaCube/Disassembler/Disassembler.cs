using System;
using System.Text;
using Tsukimi.Core.LunaCube.HW.CPU;
using Tsukimi.Core.LunaCube.HW.CPU.Data;
using Tsukimi.Core.LunaCube.HW.CPU.Enums;
using Tsukimi.Utils;

namespace Tsukimi.Core.LunaCube.Disassembler
{
    public class Disassembler
    {
        public static string DisassembleInstruction(uint instruction)
        {
            InstructionDecoder decoder = new InstructionDecoder();
            decoder.DecodeInstruction(instruction);
            InstructionFields fields = decoder.GetFields();
            InstructionType type = fields.instruction;

            StringBuilder sb = new StringBuilder();

            //Check if the instruction is illegal
            if(type == InstructionType.Illegal)
            {
                sb.AppendFormat("illegal instruction (0x{0:X8})", instruction);
                return sb.ToString();
            }

            MnemonicType mnemonicType = MnemonicData.TryFindMnemonicType(fields);

            Operand[] operands;
            Modifier modifiers;
            string name;

            //If this instruction matches a mnemonic, use the mnemonic's arguments/modifiers/name instead. Otherwise,
            //use the data from the instruction itself.
            if (mnemonicType != MnemonicType.None)
            {
                Mnemonic mnemonic = MnemonicData.GetMnemonic(mnemonicType);
                operands = mnemonic.operands;
                modifiers = mnemonic.modifiers;
                name = mnemonic.name;
            }
            else
            {
                Instruction instrData = InstructionData.GetInstruction(type);
                operands = instrData.operands;
                modifiers = instrData.modifiers;
                name = instrData.name;
            }

            sb.Append(name);

            //Check for suffix characters
            string suffix = CalculateSuffix(modifiers,fields);
            if(suffix != "") sb.Append(suffix);

            sb.Append(" ");

            bool useOffsetParentheses = false;
            
            //Add each operand to the string
            for(int i = 0; i < operands.Length; i++)
            {
                Operand operand = operands[i];

                string operandString = ConvertOperandToString(fields, operands[i], mnemonicType);

                if (useOffsetParentheses)
                {
                    sb.AppendFormat("({0})", operandString);
                    useOffsetParentheses = false;
                } else sb.Append(operandString);

                //If this operand is an offset, set the flag to use parentheses for the next operand
                if (operand == Operand.offset || operand == Operand.ps_offset)
                {
                    useOffsetParentheses = true;
                }

                //Skip the comma if the current operand is an offset
                if (i < operands.Length - 1 && !useOffsetParentheses) sb.Append(", ");

            }

            return sb.ToString();
        }

        //Calculates the suffix for the disassembled instruction by checking the modifier flags/other fields.
        static string CalculateSuffix(Modifier modifiers, InstructionFields fields)
        {
            string suffix = "";

            //Check modifiers

            //Link bit
            if (modifiers.HasFlag(Modifier.LK))
            {
                suffix += "l";
            }

            //Record bit, affects conditional register
            if (modifiers.HasFlag(Modifier.Rc))
            {
                suffix += ".";
            }

            //Check for the prediction +/- suffix if this instruction is a conditional branch (bc/bcctr/bclr), and isn't unconditional (one of condition flags disabled)
            if (InstructionUtils.IsConditionalBranchInstruction(fields.instruction))
            {
                BranchOptions bo = fields.BO;

                if (!bo.CheckBit(BranchOptions.NoConditionCheck) || !bo.CheckBit(BranchOptions.NoCtrDecrementCheck))
                {
                    suffix += bo.CheckBit(BranchOptions.Prediction) ? "+" : "-";
                }
            }

            return suffix;
        }

        static bool IsSubtractMnemonic(MnemonicType mnemonic)
        {
            return mnemonic == MnemonicType.Subi || mnemonic == MnemonicType.Subis || mnemonic == MnemonicType.Subic || mnemonic == MnemonicType.SubicDot;
        }

        static string ConvertOperandToString(InstructionFields fields, Operand operand, MnemonicType mnemonic)
        {
            switch (operand)
            {
                case Operand.simm:
                    int simm = fields.signedImmediate;
                    //Flip the simm value for subtract mnemonics
                    if (IsSubtractMnemonic(mnemonic)) simm = -simm;
                    return simm.ToHexString();
                case Operand.offset:
                case Operand.ps_offset:
                    //Offset
                    int offset = operand == Operand.offset ? fields.signedImmediate : fields.psOffset;
                    return offset.ToHexString();
                case Operand.BO:
                    //Branch options
                    return fields.commonArg1.ToString();
                case Operand.BI:
                    //Branch cr bits
                    return fields.commonArg2.ToString();
                //TODO: these should use the instruction address to display the actual target address
                case Operand.BD:
                case Operand.LI:
                    //Branch destination
                    int branchOffset = operand == Operand.BD ? fields.BD : fields.LI;
                    return branchOffset.ToHexString();
                case Operand.SH:
                    //Shift
                    return fields.commonArg3.ToString();
                case Operand.MB:
                    //Mask begin
                    return fields.commonArg4.ToString();
                case Operand.ME:
                    //Mask end
                    return fields.commonArg5.ToString();
                case Operand.rS:
                case Operand.rD:
                case Operand.rA:
                case Operand.rB:
                    //GPRs
                    uint regIndex = 0;
                    if (operand == Operand.rD || operand == Operand.rS) regIndex = fields.commonArg1;
                    else if (operand == Operand.rA) regIndex = fields.commonArg2;
                    else regIndex = fields.commonArg3;
                    return "r" + regIndex;
                case Operand.sr:
                    //Segment register
                    return fields.sr.ToString();
                case Operand.spr:
                    //Special purpose registers
                    return fields.spr.ToString();
                case Operand.frS:
                case Operand.frD:
                case Operand.frA:
                case Operand.frB:
                case Operand.frC:
                    //FPRs
                    uint floatRegIndex = 0;
                    if (operand == Operand.frD || operand == Operand.frS) floatRegIndex = fields.commonArg1;
                    else if (operand == Operand.frA) floatRegIndex = fields.commonArg2;
                    else if (operand == Operand.frB) floatRegIndex = fields.commonArg3;
                    else floatRegIndex = fields.commonArg4;
                    return "f" + floatRegIndex;
                case Operand.crbD:
                case Operand.crbA:
                case Operand.crbB:
                    //CR bits
                    uint crBit = 0;
                    if (operand == Operand.crbD) crBit = fields.commonArg1;
                    else if (operand == Operand.crbA) crBit = fields.commonArg2;
                    else crBit = fields.commonArg3;
                    return crBit.ToString();
                case Operand.crfS:
                case Operand.crfD:
                    //CRs
                    uint crIndex = operand == Operand.crfS ? fields.crfS : fields.crfD;
                    return "cr" + crIndex;
                case Operand.crm:
                    return fields.crm.ToString();
                case Operand.NB:
                    return fields.commonArg3.ToString();
                case Operand.tbr:
                    return fields.tbr.ToString();
                case Operand.mtfsf_FM:
                    return fields.mtfsfFm.ToString();
                case Operand.mtfsf_IMM:
                    return fields.mtfsfImm.ToString();
                case Operand.TO:
                    return fields.commonArg1.ToString();
                case Operand.L:
                    return fields.L.ToString();
                case Operand.ps_W:
                    return fields.psW.ToString();
                case Operand.ps_WX:
                    return fields.psWX.ToString();
                case Operand.ps_I:
                    return fields.psI.ToString();
                case Operand.ps_IX:
                    return fields.psIX.ToString();
                case Operand.spr_BAT:
                    return fields.spr_BAT.ToString();
                case Operand.spr_SPRG:
                    return fields.spr_SPRG.ToString();
                default:
                    Console.WriteLine("Somehow missed an operand ({0}), oops :3", operand.ToString());
                    return "";
            }
        }
    }
}
