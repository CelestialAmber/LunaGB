using LunaCube.HW.CPU;
using LunaCube.HW.CPU.Data;
using LunaCube.HW.CPU.Enums;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Tsukimi.Utils;

namespace LunaCube.Disassembler
{
    public struct DisassembledInstruction
    {
        public uint instructionBytes;
        public uint address;
        public string disasmString;
        public bool isBranchInstruction;
        public uint branchDestAddress;

        public DisassembledInstruction(uint instructionBytes, uint address)
        {
            this.instructionBytes = instructionBytes;
            this.address = address;
            disasmString = "";
            isBranchInstruction = false;
            branchDestAddress = 0;
        }
    }

    public class Disassembler
    {
        const bool useMnemonics = true;
        const bool showRawBranchOffsets = false;
        uint baseAddress = 0;
        uint currentAddress = 0;
        bool isBranchInstruction = false;
        uint currentBranchDestAddress = 0;
        public List<uint> branchDestAddresses = new List<uint>(); //List of branch destinations to help with printing branch info for disassembled code
        public List<DisassembledInstruction> disassembledInstructions = new List<DisassembledInstruction>();

        public void Disassemble(uint[] instructions, uint startAddress)
        {
            baseAddress = startAddress;
            currentAddress = baseAddress;
            disassembledInstructions.Clear();
            branchDestAddresses.Clear();

            List<string> result = new();

            foreach(uint instruction in instructions)
            {
                disassembledInstructions.Add(DisassembleInstruction(instruction, useMnemonics));
                //If this instruction was a branch instruction, add its destination address to the list
                if(isBranchInstruction) branchDestAddresses.Add(currentBranchDestAddress);
                currentAddress += 4;
            }
        }

        public DisassembledInstruction DisassembleInstruction(uint instrValue, bool useMnemonics)
        {
            InstructionDecoder decoder = new InstructionDecoder();
            decoder.DecodeInstruction(instrValue);
            InstructionFields fields = decoder.GetFields();
            InstructionType type = fields.instruction;

            StringBuilder sb = new StringBuilder();

            DisassembledInstruction result = new DisassembledInstruction(instrValue, currentAddress);

            //Check if the instruction is illegal
            if (type == InstructionType.Illegal)
            {
                sb.AppendFormat("<illegal> (0x{0:X8})", instrValue);
                result.disasmString = sb.ToString();
                return result;
            }

            isBranchInstruction = false;
            currentBranchDestAddress = 0;

            Instruction instruction = InstructionData.GetInstruction(type);
            MnemonicType mnemonicType = MnemonicType.None;
            Operand[] operands = instruction.operands;
            Modifier modifiers = instruction.modifiers;
            string name = instruction.name;

            //If this instruction matches a mnemonic, use the mnemonic's arguments/name/modifiers instead. Otherwise,
            //use the data from the instruction itself.
            if (useMnemonics)
            {
                mnemonicType = MnemonicData.TryFindMnemonicType(fields);
                if (mnemonicType != MnemonicType.None) {
                    Mnemonic mnemonic = MnemonicData.GetMnemonic(mnemonicType);
                    operands = mnemonic.operands;
                    name = mnemonic.name;
                    //Only use the mnemonic's modifiers if not zero
                    if (mnemonic.modifiers != Modifier.None) modifiers = mnemonic.modifiers;
                }
            }

            sb.Append(name);

            //Check for suffix characters
            string suffix = CalculateSuffix(modifiers,fields);
            if(suffix != "") sb.Append(suffix);

            bool useOffsetParentheses = false;
            
            //Add each operand to the string
            for(int i = 0; i < operands.Length; i++)
            {
                if(i == 0) sb.Append(" ");

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

            result.disasmString = sb.ToString();
            result.isBranchInstruction = isBranchInstruction;
            result.branchDestAddress = currentBranchDestAddress;
            return result;
        }

        //Calculates the suffix for the disassembled instruction by checking the modifier flags/other fields.
        string CalculateSuffix(Modifier modifiers, InstructionFields fields)
        {
            string suffix = "";

            //Check modifiers

            //XO bit
            if (modifiers.HasFlag(Modifier.OE) && fields.OE)
            {
                suffix += "o";
            }

            //Link bit
            if (modifiers.HasFlag(Modifier.LK) && fields.LK)
            {
                suffix += "l";
            }

            //Absolute address bit
            if (modifiers.HasFlag(Modifier.AA) && fields.AA)
            {
                suffix += "a";
            }

            //Record bit, affects conditional register
            if (modifiers.HasFlag(Modifier.Rc) && fields.Rc)
            {
                suffix += ".";
            }

            /* If this is a conditional branch instruction and the prediction bit is enabled, add the +/- prediction
            suffix matching the opposite prediction to the default one (+/- represent a positive/negative prediction
            respectively, or that the branch should/shouldn't be taken). The default prediction is decided as follows:

            Regular conditional branch (bc): positive if branch goes backwards (+), negative if forwards (-)
            Ctr/lr conditional branch (bcctr/bclr): always negative (-)
            
            As such, the instruction having the matching prediction suffix represents the same thing as it not having it.
            */
            bool isRegularCondBranch = modifiers.HasFlag(Modifier.BP);
            bool isCtrLrCondBranch = modifiers.HasFlag(Modifier.BP_ND);

            if ((isRegularCondBranch || isCtrLrCondBranch) && fields.BO.CheckBit(BranchOptions.Prediction)){
                //Add the suffix for the opposite prediction
                if (isCtrLrCondBranch || fields.BD >= 0) suffix += "+";
                else suffix += "-";
            }

            return suffix;
        }

        bool IsSubtractMnemonic(MnemonicType mnemonic)
        {
            return mnemonic == MnemonicType.Subi || mnemonic == MnemonicType.Subis || mnemonic == MnemonicType.Subic || mnemonic == MnemonicType.SubicDot;
        }

        string ConvertOperandToString(InstructionFields fields, Operand operand, MnemonicType mnemonic)
        {
            switch (operand)
            {
                case Operand.uimm:
                    uint uimm = fields.unsignedImmedate;
                    return uimm.ToHexString();
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
                    return ArgumentNames.ConvertCRArgumentToString(fields.commonArg2);
                //TODO: these should use the instruction address to display the actual target address
                case Operand.BD:
                case Operand.LI:
                    //Branch destination
                    int branchOffset = operand == Operand.BD ? fields.BD : fields.LI;
                    //If AA (absolute address) is off, address = current address + offset, otherwise the offset is the address
                    long address = !fields.AA ? currentAddress + branchOffset : branchOffset;
                    //Save the destination address in case it's needed later
                    isBranchInstruction = true;
                    currentBranchDestAddress = (uint)address;
                    return showRawBranchOffsets ? branchOffset.ToHexString() : address.ToHexString();
                case Operand.SH:
                    //Shift
                    uint sh = fields.commonArg3;
                    if (mnemonic == MnemonicType.Rotrwi) sh = 32 - sh;
                    else if(mnemonic == MnemonicType.Extrwi)
                    {
                        //extrwi: sh = sh - (32 - mb)
                        uint tempMb = fields.commonArg4;
                        sh -= 32 - tempMb;
                    }
                    return sh.ToString();
                case Operand.MB:
                    //Mask begin
                    uint mb = fields.commonArg4;
                    if (mnemonic == MnemonicType.Clrlslwi)
                    {
                        //clrlslwi: mb = sh + mb
                        uint tempSh = fields.commonArg3;
                        mb += tempSh;
                    }
                    else if (mnemonic == MnemonicType.Extrwi) mb = 32 - mb;
                    return mb.ToString();
                case Operand.ME:
                    //Mask end
                    uint me = fields.commonArg5;
                    if (mnemonic == MnemonicType.Clrrwi) me = 31 - me;
                    else if (mnemonic == MnemonicType.Extlwi) me++;
                    return me.ToString();
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
                    return ArgumentNames.GetSpecialRegName((SpecialReg)fields.spr);
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
                    return ArgumentNames.ConvertCRArgumentToString(crBit);
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
                    return "qr" + fields.psI;
                case Operand.ps_IX:
                    return "qr" + fields.psIX;
                case Operand.spr_BAT:
                    return fields.spr_BAT.ToString();
                case Operand.spr_SPRG:
                    return fields.spr_SPRG.ToString();
                default:
                    Console.WriteLine("Lol forgor code for {0}", operand);
                    return "";
            }
        }
    }
}
