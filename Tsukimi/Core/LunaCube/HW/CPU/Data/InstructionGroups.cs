using SixLabors.ImageSharp.ColorSpaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using Tsukimi.Core.LunaGB;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Tsukimi.Core.LunaCube.HW.CPU.Data
{
    /* Arrays for the different groups of instructions with the same main opcode to help
    make instruction decoding easier/more efficient. Unfortunately, because PPC instructions
    don't all have unique opcodes (instead having a main opcode + sub opcode for ones sharing
    the same opcode), and several instructions in groups use some of the zero bits in the sub opcode
    for other operands, this, in addition to a list of bitmasks for each instruction is necessary
    for efficiently decoding them :p */

    internal class InstructionGroups
    {

        //Instructions with unique main opcodes
        public static InstructionType[] uniqueOpcodeGroup =
        {
            InstructionType.Twi,
            InstructionType.Mulli,
            InstructionType.Subfic,
            InstructionType.Cmpli,
            InstructionType.Cmpi,
            InstructionType.Addic,
            InstructionType.AddicCnd,
            InstructionType.Addi,
            InstructionType.Addis,
            InstructionType.Bc,
            InstructionType.Sc,
            InstructionType.B,
            InstructionType.Rlwimi,
            InstructionType.Rlwinm,
            InstructionType.Rlwnm,
            InstructionType.Ori,
            InstructionType.Oris,
            InstructionType.Xori,
            InstructionType.Xoris,
            InstructionType.AndiCnd,
            InstructionType.AndisCnd,
            InstructionType.Lwz,
            InstructionType.Lwzu,
            InstructionType.Lbz,
            InstructionType.Lbzu,
            InstructionType.Stw,
            InstructionType.Stwu,
            InstructionType.Stb,
            InstructionType.Stbu,
            InstructionType.Lhz,
            InstructionType.Lhzu,
            InstructionType.Lha,
            InstructionType.Lhau,
            InstructionType.Sth,
            InstructionType.Sthu,
            InstructionType.Lmw,
            InstructionType.Stmw,
            InstructionType.Lfs,
            InstructionType.Lfsu,
            InstructionType.Lfd,
            InstructionType.Lfdu,
            InstructionType.Stfs,
            InstructionType.Stfsu,
            InstructionType.Stfd,
            InstructionType.Stfdu,
            InstructionType.PsqL,
            InstructionType.PsqLu,
            InstructionType.PsqSt,
            InstructionType.PsqStu
        };

        public static InstructionType[] instructionGroup4 =
        {
            InstructionType.PsCmpu0,
            InstructionType.PsqLx,
            InstructionType.PsqStx,
            InstructionType.PsSum0,
            InstructionType.PsSum1,
            InstructionType.PsMuls0,
            InstructionType.PsMuls1,
            InstructionType.PsMadds0,
            InstructionType.PsMadds1,
            InstructionType.PsDiv,
            InstructionType.PsSub,
            InstructionType.PsAdd,
            InstructionType.PsSel,
            InstructionType.PsRes,
            InstructionType.PsMul,
            InstructionType.PsRsqrte,
            InstructionType.PsMsub,
            InstructionType.PsMadd,
            InstructionType.PsNmsub,
            InstructionType.PsNmadd,
            InstructionType.PsCmpo0,
            InstructionType.PsqLux,
            InstructionType.PsqStux,
            InstructionType.PsNeg,
            InstructionType.PsCmpu1,
            InstructionType.PsMr,
            InstructionType.PsCmpo1,
            InstructionType.PsNabs,
            InstructionType.PsAbs,
            InstructionType.PsMerge00,
            InstructionType.PsMerge01,
            InstructionType.PsMerge10,
            InstructionType.PsMerge11,
            InstructionType.DcbzL
        };

        public static InstructionType[] instructionGroup19 =
        {
            InstructionType.Mcrf,
            InstructionType.Bclr,
            InstructionType.Crnor,
            InstructionType.Rfi,
            InstructionType.Crandc,
            InstructionType.Isync,
            InstructionType.Crxor,
            InstructionType.Crnand,
            InstructionType.Crand,
            InstructionType.Creqv,
            InstructionType.Crorc,
            InstructionType.Cror,
            InstructionType.Bcctr
        };

        public static InstructionType[] instructionGroup31 =
        {
            InstructionType.Cmp,
            InstructionType.Tw,
            InstructionType.Subfc,
            InstructionType.Addc,
            InstructionType.Mulhwu,
            InstructionType.Mfcr,
            InstructionType.Lwarx,
            InstructionType.Lwzx,
            InstructionType.Slw,
            InstructionType.Cntlzw,
            InstructionType.And,
            InstructionType.Cmpl,
            InstructionType.Subf,
            InstructionType.Dcbst,
            InstructionType.Lwzux,
            InstructionType.Andc,
            InstructionType.Mulhw,
            InstructionType.Mfmsr,
            InstructionType.Dcbf,
            InstructionType.Lbzx,
            InstructionType.Neg,
            InstructionType.Lbzux,
            InstructionType.Nor,
            InstructionType.Subfe,
            InstructionType.Adde,
            InstructionType.Mtcrf,
            InstructionType.Mtmsr,
            InstructionType.StwcxCnd,
            InstructionType.Stwx,
            InstructionType.Stwux,
            InstructionType.Subfze,
            InstructionType.Addze,
            InstructionType.Mtsr,
            InstructionType.Stbx,
            InstructionType.Subfme,
            InstructionType.Addme,
            InstructionType.Mullw,
            InstructionType.Mtsrin,
            InstructionType.Dcbtst,
            InstructionType.Stbux,
            InstructionType.Add,
            InstructionType.Dcbt,
            InstructionType.Lhzx,
            InstructionType.Eqv,
            InstructionType.Tlbie,
            InstructionType.Eciwx,
            InstructionType.Lhzux,
            InstructionType.Xor,
            InstructionType.Mfspr,
            InstructionType.Lhax,
            InstructionType.Mftb,
            InstructionType.Lhaux,
            InstructionType.Sthx,
            InstructionType.Orc,
            InstructionType.Ecowx,
            InstructionType.Sthux,
            InstructionType.Or,
            InstructionType.Divwu,
            InstructionType.Mtspr,
            InstructionType.Dcbi,
            InstructionType.Nand,
            InstructionType.Divw,
            InstructionType.Mcrxr,
            InstructionType.Lswx,
            InstructionType.Lwbrx,
            InstructionType.Lfsx,
            InstructionType.Srw,
            InstructionType.Tlbsync,
            InstructionType.Lfsux,
            InstructionType.Mfsr,
            InstructionType.Lswi,
            InstructionType.Sync,
            InstructionType.Lfdx,
            InstructionType.Lfdux,
            InstructionType.Mfsrin,
            InstructionType.Stswx,
            InstructionType.Stwbrx,
            InstructionType.Stfsx,
            InstructionType.Stfsux,
            InstructionType.Stswi,
            InstructionType.Stfdx,
            InstructionType.Stfdux,
            InstructionType.Lhbrx,
            InstructionType.Sraw,
            InstructionType.Srawi,
            InstructionType.Eieio,
            InstructionType.Sthbrx,
            InstructionType.Extsh,
            InstructionType.Extsb,
            InstructionType.Icbi,
            InstructionType.Stfiwx,
            InstructionType.Dcbz
        };

        public static InstructionType[] instructionGroup59 =
        {
            InstructionType.Fdivs,
            InstructionType.Fsubs,
            InstructionType.Fadds,
            InstructionType.Fres,
            InstructionType.Fmuls,
            InstructionType.Fmsubs,
            InstructionType.Fmadds,
            InstructionType.Fnmsubs,
            InstructionType.Fnmadds
        };

        public static InstructionType[] instructionGroup63 =
        {
            InstructionType.Fcmpu,
            InstructionType.Frsp,
            InstructionType.Fctiw,
            InstructionType.Fctiwz,
            InstructionType.Fdiv,
            InstructionType.Fsub,
            InstructionType.Fadd,
            InstructionType.Fsel,
            InstructionType.Fmul,
            InstructionType.Frsqrte,
            InstructionType.Fmsub,
            InstructionType.Fmadd,
            InstructionType.Fnmsub,
            InstructionType.Fnmadd,
            InstructionType.Fcmpo,
            InstructionType.Mtfsb1,
            InstructionType.Fneg,
            InstructionType.Mcrfs,
            InstructionType.Mtfsb0,
            InstructionType.Fmr,
            InstructionType.Mtfsfi,
            InstructionType.Fnabs,
            InstructionType.Fabs,
            InstructionType.Mffs,
            InstructionType.Mtfsf
        };
    }
}
