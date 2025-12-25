using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace LunaCube.HW.CPU.Data
{

    //Instruction operand types
    public enum Operand
    {
        simm, //Signed immediate
        uimm, //Unsigned immediate
        offset, //Offset
        BO, //Branch options
        BI, //Branch cr bits
        BD, //Branch destination (14 bit)
        LI, //Branch destination (24 bit)
        SH, //Shift amount
        MB, //Mask begin
        ME, //Mask end
        rS, //Source register
        rD, //Destination register
        rA, //Register a
        rB, //Register b
        sr, //Segment register
        spr, //Special purpose register
        frS, //Source float register
        frD, //Destination float register
        frA, //Float register a
        frB, //Float register b
        frC, //Float register c
        crbD, //Condition register bit destination
        crbA, //Condition register bit a
        crbB, //Condition register bit b
        crfD, //Condition register field destination
        crfS, //Condition register field source
        crm, //Condition register mask
        NB, //Number of bytes to move for string load/store
        tbr, //Time base
        mtfsf_FM, //mtfsf field mask
        mtfsf_IMM, //mtfsf immediate value
        TO, //tw/twi bitset
        L, //cmp(l)/cmp(l)i bitset
        ps_offset,
        ps_W,
        ps_WX,
        ps_I,
        ps_IX,
        spr_SPRG,
        spr_BAT
    }

    //Instruction modifiers
    [Flags]
    public enum Modifier : byte
    {
        None = 0,
        OE = 1 << 0, //Used by XO instructions to allow setting OV/SO in XER (operation extend?)
        Rc = 1 << 1, //Record bit
        LK = 1 << 2, //Link bit
        AA = 1 << 3, //Absolute address bit
        BP = 1 << 4, //Predict branch to take
        BP_ND = 1 << 5  //Predict branch to take (implicit LR/CTR dest)
    }

    public struct Instruction
    {
        public InstructionType instruction;
        public string name;
        public uint bitmask; //Mask for opcode values/zero bits
        public uint pattern; //Contains opcode value bits
        public Modifier modifiers;
        public Operand[] operands;

        public Instruction(string name, uint bitmask, uint pattern, Operand[] operands, Modifier modifiers = Modifier.None)
        {
            this.name = name;
            this.bitmask = bitmask;
            this.pattern = pattern;
            this.operands = operands;
            this.modifiers = modifiers;
        }


        /*
        public bool IsConditionalBranchInstruction()
        {
            return modifiers.HasFlag(;
        }
        */
    }

    public class InstructionData
    {
        public static Instruction GetInstruction(InstructionType type)
        {
            if(type == InstructionType.Illegal)
            {
                throw new Exception("Error: tried to get entry for illegal instruction");
            }
            
            return instructionDataDictionary[type];
        }

        public static Dictionary<InstructionType, Instruction> instructionDataDictionary = new()
        {
            //Name, mask, pattern, operands, modifiers
            //I hate my life :>
            {
                InstructionType.Twi,
                new Instruction("twi", 0xfc000000, 0x0c000000, [Operand.TO, Operand.rA, Operand.simm])
            },
            {
                InstructionType.PsCmpu0,
                new Instruction("ps_cmpu0", 0xfc6007ff, 0x10000000, [Operand.crfD, Operand.frA, Operand.frB])
            },
            {
                InstructionType.PsqLx,
                new Instruction("psq_lx", 0xfc00007f, 0x1000000c, [Operand.frD, Operand.rA, Operand.rB, Operand.ps_WX, Operand.ps_IX])
            },
            {
                InstructionType.PsqStx,
                new Instruction("psq_stx", 0xfc00007f, 0x1000000e, [Operand.frS, Operand.rA, Operand.rB, Operand.ps_WX, Operand.ps_IX])
            },
            {
                InstructionType.PsSum0,
                new Instruction("ps_sum0", 0xfc00003e, 0x10000014, [Operand.frD, Operand.frA, Operand.frC, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsSum1,
                new Instruction("ps_sum1", 0xfc00003e, 0x10000016, [Operand.frD, Operand.frA, Operand.frC, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsMuls0,
                new Instruction("ps_muls0", 0xfc00f83e, 0x10000018, [Operand.frD, Operand.frA, Operand.frC], Modifier.Rc)
            },
            {
                InstructionType.PsMuls1,
                new Instruction("ps_muls1", 0xfc00f83e, 0x1000001a, [Operand.frD, Operand.frA, Operand.frC], Modifier.Rc)
            },
            {
                InstructionType.PsMadds0,
                new Instruction("ps_madds0", 0xfc00003e, 0x1000001c, [Operand.frD, Operand.frA, Operand.frC, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsMadds1,
                new Instruction("ps_madds1", 0xfc00003e, 0x1000001e, [Operand.frD, Operand.frA, Operand.frC, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsDiv,
                new Instruction("ps_div", 0xfc0007fe, 0x10000024, [Operand.frD, Operand.frA, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsSub,
                new Instruction("ps_sub", 0xfc0007fe, 0x10000028, [Operand.frD, Operand.frA, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsAdd,
                new Instruction("ps_add", 0xfc0007fe, 0x1000002a, [Operand.frD, Operand.frA, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsSel,
                new Instruction("ps_sel", 0xfc00003e, 0x1000002e, [Operand.frD, Operand.frA, Operand.frC, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsRes,
                new Instruction("ps_res", 0xfc1f07fe, 0x10000030, [Operand.frD, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsMul,
                new Instruction("ps_mul", 0xfc00f83e, 0x10000032, [Operand.frD, Operand.frA, Operand.frC], Modifier.Rc)
            },
            {
                InstructionType.PsRsqrte,
                new Instruction("ps_rsqrte", 0xfc1f07fe, 0x10000034, [Operand.frD, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsMsub,
                new Instruction("ps_msub", 0xfc00003e, 0x10000038, [Operand.frD, Operand.frA, Operand.frC, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsMadd,
                new Instruction("ps_madd", 0xfc00003e, 0x1000003a, [Operand.frD, Operand.frA, Operand.frC, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsNmsub,
                new Instruction("ps_nmsub", 0xfc00003e, 0x1000003c, [Operand.frD, Operand.frA, Operand.frC, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsNmadd,
                new Instruction("ps_nmadd", 0xfc00003e, 0x1000003e, [Operand.frD, Operand.frA, Operand.frC, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsCmpo0,
                new Instruction("ps_cmpo0", 0xfc6007ff, 0x10000040, [Operand.crfD, Operand.frA, Operand.frB])
            },
            {
                InstructionType.PsqLux,
                new Instruction("psq_lux", 0xfc00007f, 0x1000004c, [Operand.frD, Operand.rA, Operand.rB, Operand.ps_WX, Operand.ps_IX])
            },
            {
                InstructionType.PsqStux,
                new Instruction("psq_stux", 0xfc00007f, 0x1000004e, [Operand.frS, Operand.rA, Operand.rB, Operand.ps_WX, Operand.ps_IX])
            },
            {
                InstructionType.PsNeg,
                new Instruction("ps_neg", 0xfc1f07fe, 0x10000050, [Operand.frD, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsCmpu1,
                new Instruction("ps_cmpu1", 0xfc6007ff, 0x10000080, [Operand.crfD, Operand.frA, Operand.frB])
            },
            {
                InstructionType.PsMr,
                new Instruction("ps_mr", 0xfc1f07fe, 0x10000090, [Operand.frD, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsCmpo1,
                new Instruction("ps_cmpo1", 0xfc6007ff, 0x100000c0, [Operand.crfD, Operand.frA, Operand.frB])
            },
            {
                InstructionType.PsNabs,
                new Instruction("ps_nabs", 0xfc1f07fe, 0x10000110, [Operand.frD, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsAbs,
                new Instruction("ps_abs", 0xfc1f07fe, 0x10000210, [Operand.frD, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsMerge00,
                new Instruction("ps_merge00", 0xfc0007fe, 0x10000420, [Operand.frD, Operand.frA, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsMerge01,
                new Instruction("ps_merge01", 0xfc0007fe, 0x10000460, [Operand.frD, Operand.frA, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsMerge10,
                new Instruction("ps_merge10", 0xfc0007fe, 0x100004a0, [Operand.frD, Operand.frA, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsMerge11,
                new Instruction("ps_merge11", 0xfc0007fe, 0x100004e0, [Operand.frD, Operand.frA, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.DcbzL,
                new Instruction("dcbz_l", 0xffe007ff, 0x100007ec, [Operand.rA, Operand.rB])
            },
            {
                InstructionType.Mulli,
                new Instruction("mulli", 0xfc000000, 0x1c000000, [Operand.rD, Operand.rA, Operand.simm])
            },
            {
                InstructionType.Subfic,
                new Instruction("subfic", 0xfc000000, 0x20000000, [Operand.rD, Operand.rA, Operand.simm])
            },
            {
                InstructionType.Cmpli,
                new Instruction("cmpli", 0xfc400000, 0x28000000, [Operand.crfD, Operand.L, Operand.rA, Operand.uimm])
            },
            {
                InstructionType.Cmpi,
                new Instruction("cmpi", 0xfc400000, 0x2c000000, [Operand.crfD, Operand.L, Operand.rA, Operand.simm])
            },
            {
                InstructionType.Addic,
                new Instruction("addic", 0xfc000000, 0x30000000, [Operand.rD, Operand.rA, Operand.simm])
            },
            {
                InstructionType.AddicCnd,
                new Instruction("addic.", 0xfc000000, 0x34000000, [Operand.rD, Operand.rA, Operand.simm])
            },
            {
                InstructionType.Addi,
                new Instruction("addi", 0xfc000000, 0x38000000, [Operand.rD, Operand.rA, Operand.simm])
            },
            {
                InstructionType.Addis,
                new Instruction("addis", 0xfc000000, 0x3c000000, [Operand.rD, Operand.rA, Operand.uimm])
            },
            {
                InstructionType.Bc,
                new Instruction("bc", 0xfc000000, 0x40000000, [Operand.BO, Operand.BI, Operand.BD], Modifier.LK | Modifier.AA | Modifier.BP)
            },
            {
                InstructionType.Sc,
                new Instruction("sc", 0xffffffff, 0x44000002, [])
            },
            {
                InstructionType.B,
                new Instruction("b", 0xfc000000, 0x48000000, [Operand.LI], Modifier.LK | Modifier.AA)
            },
            {
                InstructionType.Mcrf,
                new Instruction("mcrf", 0xfc63ffff, 0x4c000000, [Operand.crfD, Operand.crfS])
            },
            {
                InstructionType.Bclr,
                new Instruction("bclr", 0xfc00fffe, 0x4c000020, [Operand.BO, Operand.BI], Modifier.LK | Modifier.BP_ND)
            },
            {
                InstructionType.Crnor,
                new Instruction("crnor", 0xfc0007ff, 0x4c000042, [Operand.crbD, Operand.crbA, Operand.crbB])
            },
            {
                InstructionType.Rfi,
                new Instruction("rfi", 0xffffffff, 0x4c000064, [])
            },
            {
                InstructionType.Crandc,
                new Instruction("crandc", 0xfc0007ff, 0x4c000102, [Operand.crbD, Operand.crbA, Operand.crbB])
            },
            {
                InstructionType.Isync,
                new Instruction("isync", 0xffffffff, 0x4c00012c, [])
            },
            {
                InstructionType.Crxor,
                new Instruction("crxor", 0xfc0007ff, 0x4c000182, [Operand.crbD, Operand.crbA, Operand.crbB])
            },
            {
                InstructionType.Crnand,
                new Instruction("crnand", 0xfc0007ff, 0x4c0001c2, [Operand.crbD, Operand.crbA, Operand.crbB])
            },
            {
                InstructionType.Crand,
                new Instruction("crand", 0xfc0007ff, 0x4c000202, [Operand.crbD, Operand.crbA, Operand.crbB])
            },
            {
                InstructionType.Creqv,
                new Instruction("creqv", 0xfc0007ff, 0x4c000242, [Operand.crbD, Operand.crbA, Operand.crbB])
            },
            {
                InstructionType.Crorc,
                new Instruction("crorc", 0xfc0007ff, 0x4c000342, [Operand.crbD, Operand.crbA, Operand.crbB])
            },
            {
                InstructionType.Cror,
                new Instruction("cror", 0xfc0007ff, 0x4c000382, [Operand.crbD, Operand.crbA, Operand.crbB])
            },
            {
                InstructionType.Bcctr,
                new Instruction("bcctr", 0xfc00fffe, 0x4c000420, [Operand.BO, Operand.BI], Modifier.LK | Modifier.BP_ND)
            },
            {
                InstructionType.Rlwimi,
                new Instruction("rlwimi", 0xfc000000, 0x50000000, [Operand.rA, Operand.rS, Operand.SH, Operand.MB, Operand.ME], Modifier.Rc)
            },
            {
                InstructionType.Rlwinm,
                new Instruction("rlwinm", 0xfc000000, 0x54000000, [Operand.rA, Operand.rS, Operand.SH, Operand.MB, Operand.ME], Modifier.Rc)
            },
            {
                InstructionType.Rlwnm,
                new Instruction("rlwnm", 0xfc000000, 0x5c000000, [Operand.rA, Operand.rS, Operand.rB, Operand.MB, Operand.ME], Modifier.Rc)
            },
            {
                InstructionType.Ori,
                new Instruction("ori", 0xfc000000, 0x60000000, [Operand.rA, Operand.rS, Operand.uimm])
            },
            {
                InstructionType.Oris,
                new Instruction("oris", 0xfc000000, 0x64000000, [Operand.rA, Operand.rS, Operand.uimm])
            },
            {
                InstructionType.Xori,
                new Instruction("xori", 0xfc000000, 0x68000000, [Operand.rA, Operand.rS, Operand.uimm])
            },
            {
                InstructionType.Xoris,
                new Instruction("xoris", 0xfc000000, 0x6c000000, [Operand.rA, Operand.rS, Operand.uimm])
            },
            {
                InstructionType.AndiCnd,
                new Instruction("andi.", 0xfc000000, 0x70000000, [Operand.rA, Operand.rS, Operand.uimm])
            },
            {
                InstructionType.AndisCnd,
                new Instruction("andis.", 0xfc000000, 0x74000000, [Operand.rA, Operand.rS, Operand.uimm])
            },
            {
                InstructionType.Cmp,
                new Instruction("cmp", 0xfc4007ff, 0x7c000000, [Operand.crfD, Operand.L, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Tw,
                new Instruction("tw", 0xfc0007ff, 0x7c000008, [Operand.TO, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Subfc,
                new Instruction("subfc", 0xfc0003fe, 0x7c000010, [Operand.rD, Operand.rA, Operand.rB], Modifier.OE | Modifier.Rc)
            },
            {
                InstructionType.Addc,
                new Instruction("addc", 0xfc0003fe, 0x7c000014, [Operand.rD, Operand.rA, Operand.rB], Modifier.OE | Modifier.Rc)
            },
            {
                InstructionType.Mulhwu,
                new Instruction("mulhwu", 0xfc0007fe, 0x7c000016, [Operand.rD, Operand.rA, Operand.rB], Modifier.Rc)
            },
            {
                InstructionType.Mfcr,
                new Instruction("mfcr", 0xfc1fffff, 0x7c000026, [Operand.rD])
            },
            {
                InstructionType.Lwarx,
                new Instruction("lwarx", 0xfc0007ff, 0x7c000028, [Operand.rD, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Lwzx,
                new Instruction("lwzx", 0xfc0007ff, 0x7c00002e, [Operand.rD, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Slw,
                new Instruction("slw", 0xfc0007fe, 0x7c000030, [Operand.rA, Operand.rS, Operand.rB], Modifier.Rc)
            },
            {
                InstructionType.Cntlzw,
                new Instruction("cntlzw", 0xfc00fffe, 0x7c000034, [Operand.rA, Operand.rS], Modifier.Rc)
            },
            {
                InstructionType.And,
                new Instruction("and", 0xfc0007fe, 0x7c000038, [Operand.rA, Operand.rS, Operand.rB], Modifier.Rc)
            },
            {
                InstructionType.Cmpl,
                new Instruction("cmpl", 0xfc4007ff, 0x7c000040, [Operand.crfD, Operand.L, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Subf,
                new Instruction("subf", 0xfc0003fe, 0x7c000050, [Operand.rD, Operand.rA, Operand.rB], Modifier.OE | Modifier.Rc)
            },
            {
                InstructionType.Dcbst,
                new Instruction("dcbst", 0xffe007ff, 0x7c00006c, [Operand.rA, Operand.rB])
            },
            {
                InstructionType.Lwzux,
                new Instruction("lwzux", 0xfc0007ff, 0x7c00006e, [Operand.rD, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Andc,
                new Instruction("andc", 0xfc0007fe, 0x7c000078, [Operand.rA, Operand.rS, Operand.rB], Modifier.Rc)
            },
            {
                InstructionType.Mulhw,
                new Instruction("mulhw", 0xfc0007fe, 0x7c000096, [Operand.rD, Operand.rA, Operand.rB], Modifier.Rc)
            },
            {
                InstructionType.Mfmsr,
                new Instruction("mfmsr", 0xfc1fffff, 0x7c0000a6, [Operand.rD])
            },
            {
                InstructionType.Dcbf,
                new Instruction("dcbf", 0xffe007ff, 0x7c0000ac, [Operand.rA, Operand.rB])
            },
            {
                InstructionType.Lbzx,
                new Instruction("lbzx", 0xfc0007ff, 0x7c0000ae, [Operand.rD, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Neg,
                new Instruction("neg", 0xfc00fbfe, 0x7c0000d0, [Operand.rD, Operand.rA], Modifier.OE | Modifier.Rc)
            },
            {
                InstructionType.Lbzux,
                new Instruction("lbzux", 0xfc0007ff, 0x7c0000ee, [Operand.rD, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Nor,
                new Instruction("nor", 0xfc0007fe, 0x7c0000f8, [Operand.rA, Operand.rS, Operand.rB], Modifier.Rc)
            },
            {
                InstructionType.Subfe,
                new Instruction("subfe", 0xfc0003fe, 0x7c000110, [Operand.rD, Operand.rA, Operand.rB], Modifier.OE | Modifier.Rc)
            },
            {
                InstructionType.Adde,
                new Instruction("adde", 0xfc0003fe, 0x7c000114, [Operand.rD, Operand.rA, Operand.rB], Modifier.OE | Modifier.Rc)
            },
            {
                InstructionType.Mtcrf,
                new Instruction("mtcrf", 0xfc100fff, 0x7c000120, [Operand.crm, Operand.rS])
            },
            {
                InstructionType.Mtmsr,
                new Instruction("mtmsr", 0xfc1fffff, 0x7c000124, [Operand.rS])
            },
            {
                InstructionType.StwcxCnd,
                new Instruction("stwcx.", 0xfc0007ff, 0x7c00012d, [Operand.rS, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Stwx,
                new Instruction("stwx", 0xfc0007ff, 0x7c00012e, [Operand.rS, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Stwux,
                new Instruction("stwux", 0xfc0007ff, 0x7c00016e, [Operand.rS, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Subfze,
                new Instruction("subfze", 0xfc00fbfe, 0x7c000190, [Operand.rD, Operand.rA], Modifier.OE | Modifier.Rc)
            },
            {
                InstructionType.Addze,
                new Instruction("addze", 0xfc00fbfe, 0x7c000194, [Operand.rD, Operand.rA], Modifier.OE | Modifier.Rc)
            },
            {
                InstructionType.Mtsr,
                new Instruction("mtsr", 0xfc10ffff, 0x7c0001a4, [Operand.sr, Operand.rS])
            },
            {
                InstructionType.Stbx,
                new Instruction("stbx", 0xfc0007ff, 0x7c0001ae, [Operand.rS, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Subfme,
                new Instruction("subfme", 0xfc00fbfe, 0x7c0001d0, [Operand.rD, Operand.rA], Modifier.OE | Modifier.Rc)
            },
            {
                InstructionType.Addme,
                new Instruction("addme", 0xfc00fbfe, 0x7c0001d4, [Operand.rD, Operand.rA], Modifier.OE | Modifier.Rc)
            },
            {
                InstructionType.Mullw,
                new Instruction("mullw", 0xfc0003fe, 0x7c0001d6, [Operand.rD, Operand.rA, Operand.rB], Modifier.OE | Modifier.Rc)
            },
            {
                InstructionType.Mtsrin,
                new Instruction("mtsrin", 0xfc1f07ff, 0x7c0001e4, [Operand.rS, Operand.rB])
            },
            {
                InstructionType.Dcbtst,
                new Instruction("dcbtst", 0xffe007ff, 0x7c0001ec, [Operand.rA, Operand.rB])
            },
            {
                InstructionType.Stbux,
                new Instruction("stbux", 0xfc0007ff, 0x7c0001ee, [Operand.rS, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Add,
                new Instruction("add", 0xfc0003fe, 0x7c000214, [Operand.rD, Operand.rA, Operand.rB], Modifier.OE | Modifier.Rc)
            },
            {
                InstructionType.Dcbt,
                new Instruction("dcbt", 0xffe007ff, 0x7c00022c, [Operand.rA, Operand.rB])
            },
            {
                InstructionType.Lhzx,
                new Instruction("lhzx", 0xfc0007ff, 0x7c00022e, [Operand.rD, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Eqv,
                new Instruction("eqv", 0xfc0007fe, 0x7c000238, [Operand.rA, Operand.rS, Operand.rB], Modifier.Rc)
            },
            {
                InstructionType.Tlbie,
                new Instruction("tlbie", 0xffff07ff, 0x7c000264, [Operand.rB])
            },
            {
                InstructionType.Eciwx,
                new Instruction("eciwx", 0xfc0007ff, 0x7c00026c, [Operand.rD, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Lhzux,
                new Instruction("lhzux", 0xfc0007ff, 0x7c00026e, [Operand.rD, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Xor,
                new Instruction("xor", 0xfc0007fe, 0x7c000278, [Operand.rA, Operand.rS, Operand.rB], Modifier.Rc)
            },
            {
                InstructionType.Mfspr,
                new Instruction("mfspr", 0xfc0007ff, 0x7c0002a6, [Operand.rD, Operand.spr])
            },
            {
                InstructionType.Lhax,
                new Instruction("lhax", 0xfc0007ff, 0x7c0002ae, [Operand.rD, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Mftb,
                new Instruction("mftb", 0xfc0007ff, 0x7c0002e6, [Operand.rD, Operand.tbr])
            },
            {
                InstructionType.Lhaux,
                new Instruction("lhaux", 0xfc0007ff, 0x7c0002ee, [Operand.rD, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Sthx,
                new Instruction("sthx", 0xfc0007ff, 0x7c00032e, [Operand.rS, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Orc,
                new Instruction("orc", 0xfc0007fe, 0x7c000338, [Operand.rA, Operand.rS, Operand.rB], Modifier.Rc)
            },
            {
                InstructionType.Ecowx,
                new Instruction("ecowx", 0xfc0007ff, 0x7c00036c, [Operand.rS, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Sthux,
                new Instruction("sthux", 0xfc0007ff, 0x7c00036e, [Operand.rS, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Or,
                new Instruction("or", 0xfc0007fe, 0x7c000378, [Operand.rA, Operand.rS, Operand.rB], Modifier.Rc)
            },
            {
                InstructionType.Divwu,
                new Instruction("divwu", 0xfc0003fe, 0x7c000396, [Operand.rD, Operand.rA, Operand.rB], Modifier.OE | Modifier.Rc)
            },
            {
                InstructionType.Mtspr,
                new Instruction("mtspr", 0xfc0007ff, 0x7c0003a6, [Operand.spr, Operand.rS])
            },
            {
                InstructionType.Dcbi,
                new Instruction("dcbi", 0xffe007ff, 0x7c0003ac, [Operand.rA, Operand.rB])
            },
            {
                InstructionType.Nand,
                new Instruction("nand", 0xfc0007fe, 0x7c0003b8, [Operand.rA, Operand.rS, Operand.rB], Modifier.Rc)
            },
            {
                InstructionType.Divw,
                new Instruction("divw", 0xfc0003fe, 0x7c0003d6, [Operand.rD, Operand.rA, Operand.rB], Modifier.OE | Modifier.Rc)
            },
            {
                InstructionType.Mcrxr,
                new Instruction("mcrxr", 0xfc7fffff, 0x7c000400, [Operand.crfD])
            },
            {
                InstructionType.Lswx,
                new Instruction("lswx", 0xfc0007ff, 0x7c00042a, [Operand.rD, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Lwbrx,
                new Instruction("lwbrx", 0xfc0007ff, 0x7c00042c, [Operand.rD, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Lfsx,
                new Instruction("lfsx", 0xfc0007ff, 0x7c00042e, [Operand.frD, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Srw,
                new Instruction("srw", 0xfc0007fe, 0x7c000430, [Operand.rA, Operand.rS, Operand.rB], Modifier.Rc)
            },
            {
                InstructionType.Tlbsync,
                new Instruction("tlbsync", 0xffffffff, 0x7c00046c, [])
            },
            {
                InstructionType.Lfsux,
                new Instruction("lfsux", 0xfc0007ff, 0x7c00046e, [Operand.frD, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Mfsr,
                new Instruction("mfsr", 0xfc10ffff, 0x7c0004a6, [Operand.rD, Operand.sr])
            },
            {
                InstructionType.Lswi,
                new Instruction("lswi", 0xfc0007ff, 0x7c0004aa, [Operand.rD, Operand.rA, Operand.NB])
            },
            {
                InstructionType.Sync,
                new Instruction("sync", 0xFFFFFFFF, 0x7c0004ac, [])
            },
            {
                InstructionType.Lfdx,
                new Instruction("lfdx", 0xfc0007ff, 0x7c0004ae, [Operand.frD, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Lfdux,
                new Instruction("lfdux", 0xfc0007ff, 0x7c0004ee, [Operand.frD, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Mfsrin,
                new Instruction("mfsrin", 0xfc1f07ff, 0x7c000526, [Operand.rD, Operand.rB])
            },
            {
                InstructionType.Stswx,
                new Instruction("stswx", 0xfc0007ff, 0x7c00052a, [Operand.rS, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Stwbrx,
                new Instruction("stwbrx", 0xfc0007ff, 0x7c00052c, [Operand.rS, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Stfsx,
                new Instruction("stfsx", 0xfc0007ff, 0x7c00052e, [Operand.frS, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Stfsux,
                new Instruction("stfsux", 0xfc0007ff, 0x7c00056e, [Operand.frS, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Stswi,
                new Instruction("stswi", 0xfc0007ff, 0x7c0005aa, [Operand.rS, Operand.rA, Operand.NB])
            },
            {
                InstructionType.Stfdx,
                new Instruction("stfdx", 0xfc0007ff, 0x7c0005ae, [Operand.frS, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Stfdux,
                new Instruction("stfdux", 0xfc0007ff, 0x7c0005ee, [Operand.frS, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Lhbrx,
                new Instruction("lhbrx", 0xfc0007ff, 0x7c00062c, [Operand.rD, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Sraw,
                new Instruction("sraw", 0xfc0007fe, 0x7c000630, [Operand.rA, Operand.rS, Operand.rB], Modifier.Rc)
            },
            {
                InstructionType.Srawi,
                new Instruction("srawi", 0xfc0007fe, 0x7c000670, [Operand.rA, Operand.rS, Operand.SH], Modifier.Rc)
            },
            {
                InstructionType.Eieio,
                new Instruction("eieio", 0xffffffff, 0x7c0006ac, [])
            },
            {
                InstructionType.Sthbrx,
                new Instruction("sthbrx", 0xfc0007ff, 0x7c00072c, [Operand.rS, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Extsh,
                new Instruction("extsh", 0xfc00fffe, 0x7c000734, [Operand.rA, Operand.rS], Modifier.Rc)
            },
            {
                InstructionType.Extsb,
                new Instruction("extsb", 0xfc00fffe, 0x7c000774, [Operand.rA, Operand.rS], Modifier.Rc)
            },
            {
                InstructionType.Icbi,
                new Instruction("icbi", 0xffe007fe, 0x7c0007ac, [Operand.rA, Operand.rB], Modifier.Rc)
            },
            {
                InstructionType.Stfiwx,
                new Instruction("stfiwx", 0xfc0007ff, 0x7c0007ae, [Operand.frS, Operand.rA, Operand.rB])
            },
            {
                InstructionType.Dcbz,
                new Instruction("dcbz", 0xffe007ff, 0x7c0007ec, [Operand.rA, Operand.rB])
            },
            {
                InstructionType.Lwz,
                new Instruction("lwz", 0xfc000000, 0x80000000, [Operand.rD, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Lwzu,
                new Instruction("lwzu", 0xfc000000, 0x84000000, [Operand.rD, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Lbz,
                new Instruction("lbz", 0xfc000000, 0x88000000, [Operand.rD, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Lbzu,
                new Instruction("lbzu", 0xfc000000, 0x8c000000, [Operand.rD, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Stw,
                new Instruction("stw", 0xfc000000, 0x90000000, [Operand.rS, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Stwu,
                new Instruction("stwu", 0xfc000000, 0x94000000, [Operand.rS, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Stb,
                new Instruction("stb", 0xfc000000, 0x98000000, [Operand.rS, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Stbu,
                new Instruction("stbu", 0xfc000000, 0x9c000000, [Operand.rS, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Lhz,
                new Instruction("lhz", 0xfc000000, 0xa0000000, [Operand.rD, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Lhzu,
                new Instruction("lhzu", 0xfc000000, 0xa4000000, [Operand.rD, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Lha,
                new Instruction("lha", 0xfc000000, 0xa8000000, [Operand.rD, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Lhau,
                new Instruction("lhau", 0xfc000000, 0xac000000, [Operand.rD, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Sth,
                new Instruction("sth", 0xfc000000, 0xb0000000, [Operand.rS, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Sthu,
                new Instruction("sthu", 0xfc000000, 0xb4000000, [Operand.rS, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Lmw,
                new Instruction("lmw", 0xfc000000, 0xb8000000, [Operand.rD, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Stmw,
                new Instruction("stmw", 0xfc000000, 0xbc000000, [Operand.rS, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Lfs,
                new Instruction("lfs", 0xfc000000, 0xc0000000, [Operand.frD, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Lfsu,
                new Instruction("lfsu", 0xfc000000, 0xc4000000, [Operand.frD, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Lfd,
                new Instruction("lfd", 0xfc000000, 0xc8000000, [Operand.frD, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Lfdu,
                new Instruction("lfdu", 0xfc000000, 0xcc000000, [Operand.frD, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Stfs,
                new Instruction("stfs", 0xfc000000, 0xd0000000, [Operand.frS, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Stfsu,
                new Instruction("stfsu", 0xfc000000, 0xd4000000, [Operand.frS, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Stfd,
                new Instruction("stfd", 0xfc000000, 0xd8000000, [Operand.frS, Operand.offset, Operand.rA])
            },
            {
                InstructionType.Stfdu,
                new Instruction("stfdu", 0xfc000000, 0xdc000000, [Operand.frS, Operand.offset, Operand.rA])
            },
            {
                InstructionType.PsqL,
                new Instruction("psq_l", 0xfc000000, 0xe0000000, [Operand.frD, Operand.ps_offset, Operand.rA, Operand.ps_W, Operand.ps_I])
            },
            {
                InstructionType.PsqLu,
                new Instruction("psq_lu", 0xfc000000, 0xe4000000, [Operand.frD, Operand.ps_offset, Operand.rA, Operand.ps_W, Operand.ps_I])
            },
            {
                InstructionType.Fdivs,
                new Instruction("fdivs", 0xfc0007fe, 0xec000024, [Operand.frD, Operand.frA, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Fsubs,
                new Instruction("fsubs", 0xfc0007fe, 0xec000028, [Operand.frD, Operand.frA, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Fadds,
                new Instruction("fadds", 0xfc0007fe, 0xec00002a, [Operand.frD, Operand.frA, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Fres,
                new Instruction("fres", 0xfc1f07fe, 0xec000030, [Operand.frD, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Fmuls,
                new Instruction("fmuls", 0xfc00f83e, 0xec000032, [Operand.frD, Operand.frA, Operand.frC], Modifier.Rc)
            },
            {
                InstructionType.Fmsubs,
                new Instruction("fmsubs", 0xfc00003e, 0xec000038, [Operand.frD, Operand.frA, Operand.frC, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Fmadds,
                new Instruction("fmadds", 0xfc00003e, 0xec00003a, [Operand.frD, Operand.frA, Operand.frC, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Fnmsubs,
                new Instruction("fnmsubs", 0xfc00003e, 0xec00003c, [Operand.frD, Operand.frA, Operand.frC, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Fnmadds,
                new Instruction("fnmadds", 0xfc00003e, 0xec00003e, [Operand.frD, Operand.frA, Operand.frC, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.PsqSt,
                new Instruction("psq_st", 0xfc000000, 0xf0000000, [Operand.frS, Operand.ps_offset, Operand.rA, Operand.ps_W, Operand.ps_I])
            },
            {
                InstructionType.PsqStu,
                new Instruction("psq_stu", 0xfc000000, 0xf4000000, [Operand.frS, Operand.ps_offset, Operand.rA, Operand.ps_W, Operand.ps_I])
            },
            {
                InstructionType.Fcmpu,
                new Instruction("fcmpu", 0xfc6007ff, 0xfc000000, [Operand.crfD, Operand.frA, Operand.frB])
            },
            {
                InstructionType.Frsp,
                new Instruction("frsp", 0xfc1f07fe, 0xfc000018, [Operand.frD, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Fctiw,
                new Instruction("fctiw", 0xfc1f07fe, 0xfc00001c, [Operand.frD, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Fctiwz,
                new Instruction("fctiwz", 0xfc1f07fe, 0xfc00001e, [Operand.frD, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Fdiv,
                new Instruction("fdiv", 0xfc0007fe, 0xfc000024, [Operand.frD, Operand.frA, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Fsub,
                new Instruction("fsub", 0xfc0007fe, 0xfc000028, [Operand.frD, Operand.frA, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Fadd,
                new Instruction("fadd", 0xfc0007fe, 0xfc00002a, [Operand.frD, Operand.frA, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Fsel,
                new Instruction("fsel", 0xfc00003e, 0xfc00002e, [Operand.frD, Operand.frA, Operand.frC, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Fmul,
                new Instruction("fmul", 0xfc00f83e, 0xfc000032, [Operand.frD, Operand.frA, Operand.frC], Modifier.Rc)
            },
            {
                InstructionType.Frsqrte,
                new Instruction("frsqrte", 0xfc1f07fe, 0xfc000034, [Operand.frD, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Fmsub,
                new Instruction("fmsub", 0xfc00003e, 0xfc000038, [Operand.frD, Operand.frA, Operand.frC, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Fmadd,
                new Instruction("fmadd", 0xfc00003e, 0xfc00003a, [Operand.frD, Operand.frA, Operand.frC, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Fnmsub,
                new Instruction("fnmsub", 0xfc00003e, 0xfc00003c, [Operand.frD, Operand.frA, Operand.frC, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Fnmadd,
                new Instruction("fnmadd", 0xfc00003e, 0xfc00003e, [Operand.frD, Operand.frA, Operand.frC, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Fcmpo,
                new Instruction("fcmpo", 0xfc6007ff, 0xfc000040, [Operand.crfD, Operand.frA, Operand.frB])
            },
            {
                InstructionType.Mtfsb1,
                new Instruction("mtfsb1", 0xfc1ffffe, 0xfc00004c, [Operand.crbD], Modifier.Rc)
            },
            {
                InstructionType.Fneg,
                new Instruction("fneg", 0xfc1f07fe, 0xfc000050, [Operand.frD, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Mcrfs,
                new Instruction("mcrfs", 0xfc63ffff, 0xfc000080, [Operand.crfD, Operand.crfS])
            },
            {
                InstructionType.Mtfsb0,
                new Instruction("mtfsb0", 0xfc1ffffe, 0xfc00008c, [Operand.crbD], Modifier.Rc)
            },
            {
                InstructionType.Fmr,
                new Instruction("fmr", 0xfc1f07fe, 0xfc000090, [Operand.frD, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Mtfsfi,
                new Instruction("mtfsfi", 0xfc7f0ffe, 0xfc00010c, [Operand.crfD, Operand.mtfsf_IMM], Modifier.Rc)
            },
            {
                InstructionType.Fnabs,
                new Instruction("fnabs", 0xfc1f07fe, 0xfc000110, [Operand.frD, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Fabs,
                new Instruction("fabs", 0xfc1f07fe, 0xfc000210, [Operand.frD, Operand.frB], Modifier.Rc)
            },
            {
                InstructionType.Mffs,
                new Instruction("mffs", 0xfc1ffffe, 0xfc00048e, [Operand.frD], Modifier.Rc)
            },
            {
                InstructionType.Mtfsf,
                new Instruction("mtfsf", 0xfe0107fe, 0xfc00058e, [Operand.mtfsf_FM, Operand.frB], Modifier.Rc)
            }
        };
    }

}
