using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunaCube.HW.CPU;
using LunaCube.HW.CPU.Data;
using LunaCube.HW.CPU.Enums;

namespace LunaCube.Disassembler
{
    //:)

    public enum MnemonicType
    {
        None,
        Lis,
        Subis,
        Li,
        Subi,
        Subic,
        SubicDot,
        Mr,
        Nop,
        Rotlw,
        Clrrwi,
        Clrlwi,
        Rotlwi,
        Rotrwi,
        Slwi,
        Srwi,
        Clrlslwi,
        Extlwi,
        Extrwi,
        Cmpwi,
        CmpwiCrf,
        Cmpw,
        CmpwCrf,
        Cmplwi,
        CmplwiCrf,
        Cmplw,
        CmplwCrf,
        Cmpdi,
        CmpdiCrf,
        Cmpd,
        CmpdCrf,
        Cmpldi,
        CmpldiCrf,
        Cmpld,
        CmpldCrf,
        Crset,
        Crclr,
        Crmove,
        Crnot,
        Tweq,
        Twlge,
        Trap,
        Twgti,
        Twllei,
        Twui,
        Dync,
        Lwsync,
        Ptesync,
        Mtxer,
        Mtlr,
        Mtctr,
        Mtdsisr,
        Mtdar,
        Mtdec,
        Mtsdr1,
        Mtsrr0,
        Mtsrr1,
        Mtsprg,
        Mtear,
        Mttbl,
        Mttbu,
        Mtibatu,
        Mtibatl,
        Mtdbatu,
        Mtdbatl,
        Mfxer,
        Mflr,
        Mfctr,
        Mfdsisr,
        Mfdar,
        Mfdec,
        Mfsdr1,
        Mfsrr0,
        Mfsrr1,
        Mfsprg,
        Mfear,
        Mfibatu,
        Mfibatl,
        Mfdbatu,
        Mfdbatl,
        Blt,
        BltCrf,
        Ble,
        BleCrf,
        Beq,
        BeqCrf,
        Bge,
        BgeCrf,
        Bgt,
        BgtCrf,
        Bne,
        BneCrf,
        Bso,
        BsoCrf,
        Bns,
        BnsCrf,
        Bdnz,
        Bdnzt,
        Bdnzf,
        Bdz,
        Bdzt,
        Bdzf,
        Bctr,
        Bltctr,
        BltctrCrf,
        Blectr,
        BlectrCrf,
        Beqctr,
        BeqctrCrf,
        Bgectr,
        BgectrCrf,
        Bgtctr,
        BgtctrCrf,
        Bnectr,
        BnectrCrf,
        Bsoctr,
        BsoctrCrf,
        Bnsctr,
        BnsctrCrf,
        Blr,
        Bltlr,
        BltlrCrf,
        Blelr,
        BlelrCrf,
        Beqlr,
        BeqlrCrf,
        Bgelr,
        BgelrCrf,
        Bgtlr,
        BgtlrCrf,
        Bnelr,
        BnelrCrf,
        Bsolr,
        BsolrCrf,
        Bnslr,
        BnslrCrf,
        Bdnzlr,
        Bdnztlr,
        Bdnzflr,
        Bdzlr,
        Bdztlr,
        Bdzflr
    }

    public struct Mnemonic
    {
        public InstructionType baseInstruction;
        public string name;
        public Operand[] operands;
        public Modifier modifiers;

        public Mnemonic(InstructionType baseInstruction, string name, Operand[] operands, Modifier modifiers = Modifier.None)
        {
            this.baseInstruction = baseInstruction;
            this.name = name;
            this.operands = operands;
            this.modifiers = modifiers;
        }
    }

    public class MnemonicData
    {
        static Dictionary<MnemonicType, Mnemonic> mnemonicDictionary = new()
        {
            {MnemonicType.Lis,  new Mnemonic(InstructionType.Addis, "lis", [ Operand.rD, Operand.uimm ])},
            {MnemonicType.Subis,  new Mnemonic(InstructionType.Addis, "subis", [ Operand.rD, Operand.rA, Operand.simm ])},
            {MnemonicType.Li,  new Mnemonic(InstructionType.Addi, "li", [ Operand.rD, Operand.simm ])},
            {MnemonicType.Subi,  new Mnemonic(InstructionType.Addi, "subi", [ Operand.rD, Operand.rA, Operand.simm ])},
            {MnemonicType.Subic,  new Mnemonic(InstructionType.Addic, "subic", [ Operand.rD, Operand.rA, Operand.simm ])},
            {MnemonicType.SubicDot,  new Mnemonic(InstructionType.AddicCnd, "subic.", [ Operand.rD, Operand.rA, Operand.simm ])},
            {MnemonicType.Mr,  new Mnemonic(InstructionType.Or, "mr", [ Operand.rA, Operand.rS ])},
            {MnemonicType.Nop,  new Mnemonic(InstructionType.Ori, "nop", [])},
            {MnemonicType.Rotlw,  new Mnemonic(InstructionType.Rlwnm, "rotlw", [ Operand.rA, Operand.rS, Operand.rB ])},
            {MnemonicType.Clrrwi,  new Mnemonic(InstructionType.Rlwinm, "clrrwi", [ Operand.rA, Operand.rS, Operand.ME ])},
            {MnemonicType.Clrlwi,  new Mnemonic(InstructionType.Rlwinm, "clrlwi", [ Operand.rA, Operand.rS, Operand.MB ])},
            {MnemonicType.Rotlwi,  new Mnemonic(InstructionType.Rlwinm, "rotlwi", [ Operand.rA, Operand.rS, Operand.SH ])},
            {MnemonicType.Rotrwi,  new Mnemonic(InstructionType.Rlwinm, "rotrwi", [ Operand.rA, Operand.rS, Operand.SH ])},
            {MnemonicType.Slwi,  new Mnemonic(InstructionType.Rlwinm, "slwi", [ Operand.rA, Operand.rS, Operand.SH ])},
            {MnemonicType.Srwi,  new Mnemonic(InstructionType.Rlwinm, "srwi", [ Operand.rA, Operand.rS, Operand.MB ])},
            {MnemonicType.Clrlslwi,  new Mnemonic(InstructionType.Rlwinm, "clrlslwi", [ Operand.rA, Operand.rS, Operand.MB, Operand.SH ])},
            {MnemonicType.Extlwi,  new Mnemonic(InstructionType.Rlwinm, "extlwi", [ Operand.rA, Operand.rS, Operand.ME, Operand.SH ])},
            {MnemonicType.Extrwi,  new Mnemonic(InstructionType.Rlwinm, "extrwi", [ Operand.rA, Operand.rS, Operand.MB, Operand.SH ])},
            {MnemonicType.Cmpwi,  new Mnemonic(InstructionType.Cmpi, "cmpwi", [ Operand.rA, Operand.simm ])},
            {MnemonicType.CmpwiCrf,  new Mnemonic(InstructionType.Cmpi, "cmpwi", [ Operand.crfD, Operand.rA, Operand.simm ])},
            {MnemonicType.Cmpw,  new Mnemonic(InstructionType.Cmp, "cmpw", [ Operand.rA, Operand.rB ])},
            {MnemonicType.CmpwCrf,  new Mnemonic(InstructionType.Cmp, "cmpw", [ Operand.crfD, Operand.rA, Operand.rB ])},
            {MnemonicType.Cmplwi,  new Mnemonic(InstructionType.Cmpli, "cmplwi", [ Operand.rA, Operand.uimm ])},
            {MnemonicType.CmplwiCrf,  new Mnemonic(InstructionType.Cmpli, "cmplwi", [ Operand.crfD, Operand.rA, Operand.uimm ])},
            {MnemonicType.Cmplw,  new Mnemonic(InstructionType.Cmpl, "cmplw", [ Operand.rA, Operand.rB ])},
            {MnemonicType.CmplwCrf,  new Mnemonic(InstructionType.Cmpl, "cmplw", [ Operand.crfD, Operand.rA, Operand.rB ])},
            {MnemonicType.Cmpdi,  new Mnemonic(InstructionType.Cmpi, "cmpdi", [ Operand.rA, Operand.simm ])},
            {MnemonicType.CmpdiCrf,  new Mnemonic(InstructionType.Cmpi, "cmpdi", [ Operand.crfD, Operand.rA, Operand.simm ])},
            {MnemonicType.Cmpd,  new Mnemonic(InstructionType.Cmp, "cmpd", [ Operand.rA, Operand.rB ])},
            {MnemonicType.CmpdCrf,  new Mnemonic(InstructionType.Cmp, "cmpd", [ Operand.crfD, Operand.rA, Operand.rB ])},
            {MnemonicType.Cmpldi,  new Mnemonic(InstructionType.Cmpli, "cmpldi", [ Operand.rA, Operand.uimm ])},
            {MnemonicType.CmpldiCrf,  new Mnemonic(InstructionType.Cmpli, "cmpldi", [ Operand.crfD, Operand.rA, Operand.uimm ])},
            {MnemonicType.Cmpld,  new Mnemonic(InstructionType.Cmpl, "cmpld", [ Operand.rA, Operand.rB ])},
            {MnemonicType.CmpldCrf,  new Mnemonic(InstructionType.Cmpl, "cmpld", [ Operand.crfD, Operand.rA, Operand.rB ])},
            {MnemonicType.Crset,  new Mnemonic(InstructionType.Creqv, "crset", [ Operand.crbD ])},
            {MnemonicType.Crclr,  new Mnemonic(InstructionType.Crxor, "crclr", [ Operand.crbD ])},
            {MnemonicType.Crmove,  new Mnemonic(InstructionType.Cror, "crmove", [ Operand.crbD, Operand.crbA ])},
            {MnemonicType.Crnot,  new Mnemonic(InstructionType.Crnor, "crnot", [ Operand.crbD, Operand.crbA ])},
            {MnemonicType.Tweq,  new Mnemonic(InstructionType.Tw, "tweq", [ Operand.rA, Operand.rB ])},
            {MnemonicType.Twlge,  new Mnemonic(InstructionType.Tw, "twlge", [ Operand.rA, Operand.rB ])},
            {MnemonicType.Trap,  new Mnemonic(InstructionType.Tw, "trap", [])},
            {MnemonicType.Twgti,  new Mnemonic(InstructionType.Twi, "twgti", [ Operand.rA, Operand.simm ])},
            {MnemonicType.Twllei,  new Mnemonic(InstructionType.Twi, "twllei", [ Operand.rA, Operand.simm ])},
            {MnemonicType.Twui,  new Mnemonic(InstructionType.Twi, "twui", [ Operand.rA, Operand.simm ])},
            {MnemonicType.Mtxer,  new Mnemonic(InstructionType.Mtspr, "mtxer", [ Operand.rS ])},
            {MnemonicType.Mtlr,  new Mnemonic(InstructionType.Mtspr, "mtlr", [ Operand.rS ])},
            {MnemonicType.Mtctr,  new Mnemonic(InstructionType.Mtspr, "mtctr", [ Operand.rS ])},
            {MnemonicType.Mtdsisr,  new Mnemonic(InstructionType.Mtspr, "mtdsisr", [ Operand.rS ])},
            {MnemonicType.Mtdar,  new Mnemonic(InstructionType.Mtspr, "mtdar", [ Operand.rS ])},
            {MnemonicType.Mtdec,  new Mnemonic(InstructionType.Mtspr, "mtdec", [ Operand.rS ])},
            {MnemonicType.Mtsdr1,  new Mnemonic(InstructionType.Mtspr, "mtsdr1", [ Operand.rS ])},
            {MnemonicType.Mtsrr0,  new Mnemonic(InstructionType.Mtspr, "mtsrr0", [ Operand.rS ])},
            {MnemonicType.Mtsrr1,  new Mnemonic(InstructionType.Mtspr, "mtsrr1", [ Operand.rS ])},
            {MnemonicType.Mtsprg,  new Mnemonic(InstructionType.Mtspr, "mtsprg", [ Operand.spr_SPRG, Operand.rS ])},
            {MnemonicType.Mtear,  new Mnemonic(InstructionType.Mtspr, "mtear", [ Operand.rS ])},
            {MnemonicType.Mttbl,  new Mnemonic(InstructionType.Mtspr, "mttbl", [ Operand.rS ])},
            {MnemonicType.Mttbu,  new Mnemonic(InstructionType.Mtspr, "mttbu", [ Operand.rS ])},
            {MnemonicType.Mtibatu,  new Mnemonic(InstructionType.Mtspr, "mtibatu", [ Operand.spr_BAT, Operand.rS ])},
            {MnemonicType.Mtibatl,  new Mnemonic(InstructionType.Mtspr, "mtibatl", [ Operand.spr_BAT, Operand.rS ])},
            {MnemonicType.Mtdbatu,  new Mnemonic(InstructionType.Mtspr, "mtdbatu", [ Operand.spr_BAT, Operand.rS ])},
            {MnemonicType.Mtdbatl,  new Mnemonic(InstructionType.Mtspr, "mtdbatl", [ Operand.spr_BAT, Operand.rS ])},
            {MnemonicType.Mfxer,  new Mnemonic(InstructionType.Mfspr, "mfxer", [ Operand.rD ])},
            {MnemonicType.Mflr,  new Mnemonic(InstructionType.Mfspr, "mflr", [ Operand.rD ])},
            {MnemonicType.Mfctr,  new Mnemonic(InstructionType.Mfspr, "mfctr", [ Operand.rD ])},
            {MnemonicType.Mfdsisr,  new Mnemonic(InstructionType.Mfspr, "mfdsisr", [ Operand.rD ])},
            {MnemonicType.Mfdar,  new Mnemonic(InstructionType.Mfspr, "mfdar", [ Operand.rD ])},
            {MnemonicType.Mfdec,  new Mnemonic(InstructionType.Mfspr, "mfdec", [ Operand.rD ])},
            {MnemonicType.Mfsdr1,  new Mnemonic(InstructionType.Mfspr, "mfsdr1", [ Operand.rD ])},
            {MnemonicType.Mfsrr0,  new Mnemonic(InstructionType.Mfspr, "mfsrr0", [ Operand.rD ])},
            {MnemonicType.Mfsrr1,  new Mnemonic(InstructionType.Mfspr, "mfsrr1", [ Operand.rD ])},
            {MnemonicType.Mfsprg,  new Mnemonic(InstructionType.Mfspr, "mfsprg", [ Operand.rD, Operand.spr_SPRG ])},
            {MnemonicType.Mfear,  new Mnemonic(InstructionType.Mfspr, "mfear", [ Operand.rD ])},
            {MnemonicType.Mfibatu,  new Mnemonic(InstructionType.Mfspr, "mfibatu", [ Operand.rD, Operand.spr_BAT ])},
            {MnemonicType.Mfibatl,  new Mnemonic(InstructionType.Mfspr, "mfibatl", [ Operand.rD, Operand.spr_BAT ])},
            {MnemonicType.Mfdbatu,  new Mnemonic(InstructionType.Mfspr, "mfdbatu", [ Operand.rD, Operand.spr_BAT ])},
            {MnemonicType.Mfdbatl,  new Mnemonic(InstructionType.Mfspr, "mfdbatl", [ Operand.rD, Operand.spr_BAT ])},
            {MnemonicType.Blt,  new Mnemonic(InstructionType.Bc, "blt", [ Operand.BD ])},
            {MnemonicType.BltCrf,  new Mnemonic(InstructionType.Bc, "blt", [ Operand.crfS, Operand.BD ])},
            {MnemonicType.Ble,  new Mnemonic(InstructionType.Bc, "ble", [ Operand.BD ])},
            {MnemonicType.BleCrf,  new Mnemonic(InstructionType.Bc, "ble", [ Operand.crfS, Operand.BD ])},
            {MnemonicType.Beq,  new Mnemonic(InstructionType.Bc, "beq", [ Operand.BD ])},
            {MnemonicType.BeqCrf,  new Mnemonic(InstructionType.Bc, "beq", [ Operand.crfS, Operand.BD ])},
            {MnemonicType.Bge,  new Mnemonic(InstructionType.Bc, "bge", [ Operand.BD ])},
            {MnemonicType.BgeCrf,  new Mnemonic(InstructionType.Bc, "bge", [ Operand.crfS, Operand.BD ])},
            {MnemonicType.Bgt,  new Mnemonic(InstructionType.Bc, "bgt", [ Operand.BD ])},
            {MnemonicType.BgtCrf,  new Mnemonic(InstructionType.Bc, "bgt", [ Operand.crfS, Operand.BD ])},
            {MnemonicType.Bne,  new Mnemonic(InstructionType.Bc, "bne", [ Operand.BD ])},
            {MnemonicType.BneCrf,  new Mnemonic(InstructionType.Bc, "bne", [ Operand.crfS, Operand.BD ])},
            {MnemonicType.Bso,  new Mnemonic(InstructionType.Bc, "bso", [ Operand.BD ])},
            {MnemonicType.BsoCrf,  new Mnemonic(InstructionType.Bc, "bso", [ Operand.crfS, Operand.BD ])},
            {MnemonicType.Bns,  new Mnemonic(InstructionType.Bc, "bns", [ Operand.BD ])},
            {MnemonicType.BnsCrf,  new Mnemonic(InstructionType.Bc, "bns", [ Operand.crfS, Operand.BD ])},
            {MnemonicType.Bdnz,  new Mnemonic(InstructionType.Bc, "bdnz", [ Operand.BD ])},
            {MnemonicType.Bdnzt,  new Mnemonic(InstructionType.Bc, "bdnzt", [ Operand.BI, Operand.BD ])},
            {MnemonicType.Bdnzf,  new Mnemonic(InstructionType.Bc, "bdnzf", [ Operand.BI, Operand.BD ])},
            {MnemonicType.Bdz,  new Mnemonic(InstructionType.Bc, "bdz", [ Operand.BD ])},
            {MnemonicType.Bdzt,  new Mnemonic(InstructionType.Bc, "bdzt", [ Operand.BI, Operand.BD ])},
            {MnemonicType.Bdzf,  new Mnemonic(InstructionType.Bc, "bdzf", [ Operand.BI, Operand.BD ])},
            {MnemonicType.Bctr,  new Mnemonic(InstructionType.Bcctr, "bctr", [], Modifier.LK)},
            {MnemonicType.Bltctr,  new Mnemonic(InstructionType.Bcctr, "bltctr", [])},
            {MnemonicType.BltctrCrf,  new Mnemonic(InstructionType.Bcctr, "bltctr", [ Operand.crfS ])},
            {MnemonicType.Blectr,  new Mnemonic(InstructionType.Bcctr, "blectr", [])},
            {MnemonicType.BlectrCrf,  new Mnemonic(InstructionType.Bcctr, "blectr", [ Operand.crfS ])},
            {MnemonicType.Beqctr,  new Mnemonic(InstructionType.Bcctr, "beqctr", [])},
            {MnemonicType.BeqctrCrf,  new Mnemonic(InstructionType.Bcctr, "beqctr", [ Operand.crfS ])},
            {MnemonicType.Bgectr,  new Mnemonic(InstructionType.Bcctr, "bgectr", [])},
            {MnemonicType.BgectrCrf,  new Mnemonic(InstructionType.Bcctr, "bgectr", [ Operand.crfS ])},
            {MnemonicType.Bgtctr,  new Mnemonic(InstructionType.Bcctr, "bgtctr", [])},
            {MnemonicType.BgtctrCrf,  new Mnemonic(InstructionType.Bcctr, "bgtctr", [ Operand.crfS ])},
            {MnemonicType.Bnectr,  new Mnemonic(InstructionType.Bcctr, "bnectr", [])},
            {MnemonicType.BnectrCrf,  new Mnemonic(InstructionType.Bcctr, "bnectr", [ Operand.crfS ])},
            {MnemonicType.Bsoctr,  new Mnemonic(InstructionType.Bcctr, "bsoctr", [])},
            {MnemonicType.BsoctrCrf,  new Mnemonic(InstructionType.Bcctr, "bsoctr", [ Operand.crfS ])},
            {MnemonicType.Bnsctr,  new Mnemonic(InstructionType.Bcctr, "bnsctr", [])},
            {MnemonicType.BnsctrCrf,  new Mnemonic(InstructionType.Bcctr, "bnsctr", [ Operand.crfS ])},
            {MnemonicType.Blr,  new Mnemonic(InstructionType.Bclr, "blr", [], Modifier.LK)},
            {MnemonicType.Bltlr,  new Mnemonic(InstructionType.Bclr, "bltlr", [])},
            {MnemonicType.BltlrCrf,  new Mnemonic(InstructionType.Bclr, "bltlr", [ Operand.crfS ])},
            {MnemonicType.Blelr,  new Mnemonic(InstructionType.Bclr, "blelr", [])},
            {MnemonicType.BlelrCrf,  new Mnemonic(InstructionType.Bclr, "blelr", [ Operand.crfS ])},
            {MnemonicType.Beqlr,  new Mnemonic(InstructionType.Bclr, "beqlr", [])},
            {MnemonicType.BeqlrCrf,  new Mnemonic(InstructionType.Bclr, "beqlr", [ Operand.crfS ])},
            {MnemonicType.Bgelr,  new Mnemonic(InstructionType.Bclr, "bgelr", [])},
            {MnemonicType.BgelrCrf,  new Mnemonic(InstructionType.Bclr, "bgelr", [ Operand.crfS ])},
            {MnemonicType.Bgtlr,  new Mnemonic(InstructionType.Bclr, "bgtlr", [])},
            {MnemonicType.BgtlrCrf,  new Mnemonic(InstructionType.Bclr, "bgtlr", [ Operand.crfS ])},
            {MnemonicType.Bnelr,  new Mnemonic(InstructionType.Bclr, "bnelr", [])},
            {MnemonicType.BnelrCrf,  new Mnemonic(InstructionType.Bclr, "bnelr", [ Operand.crfS ])},
            {MnemonicType.Bsolr,  new Mnemonic(InstructionType.Bclr, "bsolr", [])},
            {MnemonicType.BsolrCrf,  new Mnemonic(InstructionType.Bclr, "bsolr", [ Operand.crfS ])},
            {MnemonicType.Bnslr,  new Mnemonic(InstructionType.Bclr, "bnslr", [])},
            {MnemonicType.BnslrCrf,  new Mnemonic(InstructionType.Bclr, "bnslr", [ Operand.crfS ])},
            {MnemonicType.Bdnzlr,  new Mnemonic(InstructionType.Bclr, "bdnzlr", [])},
            {MnemonicType.Bdnztlr,  new Mnemonic(InstructionType.Bclr, "bdnztlr", [ Operand.BI ])},
            {MnemonicType.Bdnzflr,  new Mnemonic(InstructionType.Bclr, "bdnzflr", [ Operand.BI ])},
            {MnemonicType.Bdzlr,  new Mnemonic(InstructionType.Bclr, "bdzlr", [])},
            {MnemonicType.Bdztlr,  new Mnemonic(InstructionType.Bclr, "bdztlr", [ Operand.BI ])},
            {MnemonicType.Bdzflr,  new Mnemonic(InstructionType.Bclr, "bdzflr", [ Operand.BI ])},
        };

        public static Mnemonic GetMnemonic(MnemonicType type)
        {
            return mnemonicDictionary[type];
        }

        static Dictionary<SpecialReg, MnemonicType> mtsprMnemonicsDictionary = new()
        {
            {SpecialReg.Xer, MnemonicType.Mtxer},
            {SpecialReg.Lr, MnemonicType.Mtlr},
            {SpecialReg.Ctr, MnemonicType.Mtctr},
            {SpecialReg.Dsisr, MnemonicType.Mtdsisr},
            {SpecialReg.Dar, MnemonicType.Mtdar},
            {SpecialReg.Dec, MnemonicType.Mtdec},
            {SpecialReg.Sdr1, MnemonicType.Mtsdr1},
            {SpecialReg.Srr0, MnemonicType.Mtsrr0},
            {SpecialReg.Srr1, MnemonicType.Mtsrr1},
            {SpecialReg.Sprg0, MnemonicType.Mtsprg},
            {SpecialReg.Sprg1, MnemonicType.Mtsprg},
            {SpecialReg.Sprg2, MnemonicType.Mtsprg},
            {SpecialReg.Sprg3, MnemonicType.Mtsprg},
            {SpecialReg.Ear, MnemonicType.Mtear},
            {SpecialReg.Tbl, MnemonicType.Mttbl},
            {SpecialReg.Tbu, MnemonicType.Mttbu},
            {SpecialReg.Ibat0U, MnemonicType.Mtibatu},
            {SpecialReg.Ibat0L, MnemonicType.Mtibatl},
            {SpecialReg.Ibat1U, MnemonicType.Mtibatu},
            {SpecialReg.Ibat1L, MnemonicType.Mtibatl},
            {SpecialReg.Ibat2U, MnemonicType.Mtibatu},
            {SpecialReg.Ibat2L, MnemonicType.Mtibatl},
            {SpecialReg.Ibat3U, MnemonicType.Mtibatu},
            {SpecialReg.Ibat3L, MnemonicType.Mtibatl},
            {SpecialReg.Dbat0U, MnemonicType.Mtdbatu},
            {SpecialReg.Dbat0L, MnemonicType.Mtdbatl},
            {SpecialReg.Dbat1U, MnemonicType.Mtdbatu},
            {SpecialReg.Dbat1L, MnemonicType.Mtdbatl},
            {SpecialReg.Dbat2U, MnemonicType.Mtdbatu},
            {SpecialReg.Dbat2L, MnemonicType.Mtdbatl},
            {SpecialReg.Dbat3U, MnemonicType.Mtdbatu},
            {SpecialReg.Dbat3L, MnemonicType.Mtdbatl},
        };

        static Dictionary<SpecialReg, MnemonicType> mfsprMnemonicsDictionary = new()
        {
            {SpecialReg.Xer, MnemonicType.Mfxer},
            {SpecialReg.Lr, MnemonicType.Mflr},
            {SpecialReg.Ctr, MnemonicType.Mfctr},
            {SpecialReg.Dsisr, MnemonicType.Mfdsisr},
            {SpecialReg.Dar, MnemonicType.Mfdar},
            {SpecialReg.Dec, MnemonicType.Mfdec},
            {SpecialReg.Sdr1, MnemonicType.Mfsdr1},
            {SpecialReg.Srr0, MnemonicType.Mfsrr0},
            {SpecialReg.Srr1, MnemonicType.Mfsrr1},
            {SpecialReg.Sprg0, MnemonicType.Mfsprg},
            {SpecialReg.Sprg1, MnemonicType.Mfsprg},
            {SpecialReg.Sprg2, MnemonicType.Mfsprg},
            {SpecialReg.Sprg3, MnemonicType.Mfsprg},
            {SpecialReg.Ear, MnemonicType.Mfear},
            {SpecialReg.Ibat0U, MnemonicType.Mfibatu},
            {SpecialReg.Ibat0L, MnemonicType.Mfibatl},
            {SpecialReg.Ibat1U, MnemonicType.Mfibatu},
            {SpecialReg.Ibat1L, MnemonicType.Mfibatl},
            {SpecialReg.Ibat2U, MnemonicType.Mfibatu},
            {SpecialReg.Ibat2L, MnemonicType.Mfibatl},
            {SpecialReg.Ibat3U, MnemonicType.Mfibatu},
            {SpecialReg.Ibat3L, MnemonicType.Mfibatl},
            {SpecialReg.Dbat0U, MnemonicType.Mfdbatu},
            {SpecialReg.Dbat0L, MnemonicType.Mfdbatl},
            {SpecialReg.Dbat1U, MnemonicType.Mfdbatu},
            {SpecialReg.Dbat1L, MnemonicType.Mfdbatl},
            {SpecialReg.Dbat2U, MnemonicType.Mfdbatu},
            {SpecialReg.Dbat2L, MnemonicType.Mfdbatl},
            {SpecialReg.Dbat3U, MnemonicType.Mfdbatu},
            {SpecialReg.Dbat3L, MnemonicType.Mfdbatl},
        };

        //Mapping index: ((BI & 0b11100) != 0 ? 0b1000 : 0) | ((BO & 0b1000) >> 1) | (BI & 0b11)

        static MnemonicType[] bcConditionalMnemonics =
        {
            MnemonicType.Bge,
            MnemonicType.Ble,
            MnemonicType.Bne,
            MnemonicType.Bns,
            MnemonicType.Blt,
            MnemonicType.Bgt,
            MnemonicType.Beq,
            MnemonicType.Bso,
            MnemonicType.BgeCrf,
            MnemonicType.BleCrf,
            MnemonicType.BneCrf,
            MnemonicType.BnsCrf,
            MnemonicType.BltCrf,
            MnemonicType.BgtCrf,
            MnemonicType.BeqCrf,
            MnemonicType.BsoCrf
        };

        static MnemonicType[] bcctrConditionalMnemonics =
        {
            MnemonicType.Bgectr,
            MnemonicType.Blectr,
            MnemonicType.Bnectr,
            MnemonicType.Bnsctr,
            MnemonicType.Bltctr,
            MnemonicType.Bgtctr,
            MnemonicType.Beqctr,
            MnemonicType.Bsoctr,
            MnemonicType.BgectrCrf,
            MnemonicType.BlectrCrf,
            MnemonicType.BnectrCrf,
            MnemonicType.BnsctrCrf,
            MnemonicType.BltctrCrf,
            MnemonicType.BgtctrCrf,
            MnemonicType.BeqctrCrf,
            MnemonicType.BsoctrCrf
        };

        static MnemonicType[] bclrConditionalMnemonics =
        {
            MnemonicType.Bgelr,
            MnemonicType.Blelr,
            MnemonicType.Bnelr,
            MnemonicType.Bnslr,
            MnemonicType.Bltlr,
            MnemonicType.Bgtlr,
            MnemonicType.Beqlr,
            MnemonicType.Bsolr,
            MnemonicType.BgelrCrf,
            MnemonicType.BlelrCrf,
            MnemonicType.BnelrCrf,
            MnemonicType.BnslrCrf,
            MnemonicType.BltlrCrf,
            MnemonicType.BgtlrCrf,
            MnemonicType.BeqlrCrf,
            MnemonicType.BsolrCrf
        };

        //Tries to see if the instruction data matches a mnemonic, and returns
        //the type of the matching mnemonic if so. Otherwise it just returns None.
        //TODO: this maybe should be split up/cleaned up more if possible
        public static MnemonicType TryFindMnemonicType(InstructionFields fields)
        {
            MnemonicType type = MnemonicType.None;

            //Read all the necessary fields
            int simm = fields.signedImmediate;
            uint uimm = fields.unsignedImmedate;
            uint commonArg1 = fields.commonArg1;
            uint commonArg2 = fields.commonArg2;
            uint commonArg3 = fields.commonArg3;
            BranchOptions BO = fields.BO;
            uint BI = commonArg2;
            uint rS = commonArg1;
            uint rA = commonArg2;
            uint rB = commonArg3;
            uint crbD = commonArg1;
            uint crbA = commonArg2;
            uint crbB = commonArg3;
            uint crfD = fields.crfD;
            uint SH = commonArg3;
            uint MB = fields.commonArg4;
            uint ME = fields.commonArg5;
            bool L = fields.L == 1 ? true : false;
            uint TO = commonArg1;

            InstructionType instrType = fields.instruction;
            SpecialReg spr = (SpecialReg)fields.spr;

            switch (instrType)
            {
                case InstructionType.Addis:
                case InstructionType.Addi:
                case InstructionType.Addic:
                case InstructionType.AddicCnd:
                    if(simm < 0 && simm != -0x8000)
                    {
                        //Negative value -> subtract immediate instruction
                        if (instrType == InstructionType.Addis) type = MnemonicType.Subis;
                        else if (instrType == InstructionType.Addi) type = MnemonicType.Subi;
                        else if (instrType == InstructionType.Addic) type = MnemonicType.Subic;
                        else type = MnemonicType.SubicDot;
                    }

                    if(rA == 0)
                    {
                        //Add amount = 0 -> load immediate instruction
                        if (instrType == InstructionType.Addis) type = MnemonicType.Lis;
                        else if (instrType == InstructionType.Addi) type = MnemonicType.Li;
                    }
                    break;
                case InstructionType.Or:
                    if (rB == rS) type = MnemonicType.Mr;
                    break;
                case InstructionType.Ori:
                    if (rA == 0 && rS == 0 & uimm == 0) type = MnemonicType.Nop;
                    break;
                case InstructionType.Rlwnm:
                    if (MB == 0 && ME == 31) type = MnemonicType.Rotlw;
                    break;
                case InstructionType.Rlwinm:
                    //Rlwinm mnemonics
                    if (SH == 0 && MB == 0) type = MnemonicType.Clrrwi;
                    else if (SH == 0 && ME == 31) type = MnemonicType.Clrlwi;
                    else if (MB == 0 && ME == 31 && SH <= 16) type = MnemonicType.Rotlwi;
                    else if (MB == 0 && ME == 31 && SH > 16) type = MnemonicType.Rotrwi;
                    else if (MB == 0 && ME == 31 - SH) type = MnemonicType.Slwi;
                    else if (ME == 31 && SH == 32 - MB) type = MnemonicType.Srwi;
                    else if (SH < 32 && ME == 31 - SH) type = MnemonicType.Clrlslwi;
                    else if (MB == 0) type = MnemonicType.Extlwi;
                    else if (ME == 31 && SH >= 32 - MB) type = MnemonicType.Extrwi;
                    break;
                case InstructionType.Cmpi:
                case InstructionType.Cmp:
                case InstructionType.Cmpli:
                case InstructionType.Cmpl:
                    //Compare mnemonics
                    if(crfD == 0)
                    {
                        if (instrType == InstructionType.Cmpi) type = L == false ? MnemonicType.Cmpwi : MnemonicType.Cmpdi;
                        else if (instrType == InstructionType.Cmp) type = L == false ? MnemonicType.Cmpw : MnemonicType.Cmpd;
                        else if (instrType == InstructionType.Cmpli) type = L == false ? MnemonicType.Cmplwi : MnemonicType.Cmpldi;
                        else if (instrType == InstructionType.Cmpl) type = L == false ? MnemonicType.Cmplw : MnemonicType.Cmpld;
                    }
                    else
                    {
                        if (instrType == InstructionType.Cmpi) type = L == false ? MnemonicType.CmpwiCrf : MnemonicType.CmpdiCrf;
                        else if (instrType == InstructionType.Cmp) type = L == false ? MnemonicType.CmpwCrf : MnemonicType.CmpdCrf;
                        else if (instrType == InstructionType.Cmpli) type = L == false ? MnemonicType.CmplwiCrf : MnemonicType.CmpldiCrf;
                        else if (instrType == InstructionType.Cmpl) type = L == false ? MnemonicType.CmplwCrf : MnemonicType.CmpldCrf;
                    }
                    break;
                case InstructionType.Creqv:
                    if (crbA == crbD && crbB == crbD) type = MnemonicType.Crset;
                    break;
                case InstructionType.Crxor:
                    if (crbA == crbD && crbB == crbD) type = MnemonicType.Crclr;
                    break;
                case InstructionType.Cror:
                    if (crbA == crbB) type = MnemonicType.Crmove;
                    break;
                case InstructionType.Crnor:
                    if (crbA == crbB) type = MnemonicType.Crnot;
                    break;
                case InstructionType.Tw:
                    if (TO == 4) type = MnemonicType.Tweq;
                    else if (TO == 5) type = MnemonicType.Twlge;
                    else if (TO == 31 && rA == 0 && rB == 0) type = MnemonicType.Trap;
                    break;
                case InstructionType.Twi:
                    if (TO == 6) type = MnemonicType.Twllei;
                    else if (TO == 8) type = MnemonicType.Twgti;
                    else if (TO == 31) type = MnemonicType.Twui;
                    break;
                case InstructionType.Mtspr:
                    if (mtsprMnemonicsDictionary.ContainsKey(spr))
                    {
                        type = mtsprMnemonicsDictionary[spr];
                    }
                    break;
                case InstructionType.Mfspr:
                    if (mfsprMnemonicsDictionary.ContainsKey(spr))
                    {
                        type = mfsprMnemonicsDictionary[spr];
                    }
                    break;
                case InstructionType.Bc:
                case InstructionType.Bcctr:
                case InstructionType.Bclr:
                    //Conditional branch mnemonics
                    type = FindConditionalBranchMnemonic(BO, BI, instrType);
                    break;
                default:
                    break;
            }

            return type;
        }

        static MnemonicType FindConditionalBranchMnemonic(BranchOptions BO, uint BI, InstructionType instrType)
        {
            //Thanks ibm very cool

            bool noCtrDecrementFlag = BO.CheckBit(BranchOptions.NoCtrDecrementCheck);
            bool noConditionCheckFlag = BO.CheckBit(BranchOptions.NoConditionCheck);
            bool checkCtrEqualZeroFlag = BO.CheckBit(BranchOptions.CheckCtrEqualZero);
            bool checkConditionTrueFlag = BO.CheckBit(BranchOptions.CheckConditionTrue);

            MnemonicType type = MnemonicType.None;


            if (noCtrDecrementFlag)
            {
                if (noConditionCheckFlag && instrType != InstructionType.Bc)
                {
                    //Unconditional branches

                    //No mnemonic for bc, since that would just be b again
                    if (instrType == InstructionType.Bcctr) type = MnemonicType.Bctr;
                    else type = MnemonicType.Blr; //bclr -> blr
                }
                else
                {
                    //Regular condition branches (beq, bge...)

                    /* Instead of checking every case individually, three arrays for each base instruction are used
                    that make use of a special index made from the different values from BO and BI. This is kind of
                    janky but PowerPC doesn't really leave a better option :/ */

                    //Compute the index for the branch mnemonic arrays
                    int index = CalculateBranchMnemonicArrayIndex(checkConditionTrueFlag, BI);

                    if (instrType == InstructionType.Bc) type = bcConditionalMnemonics[index];
                    else if (instrType == InstructionType.Bcctr) type = bcctrConditionalMnemonics[index];
                    else type = bclrConditionalMnemonics[index]; //bclr
                }
            }
            //The rest only apply to bc/bclr
            else if (instrType != InstructionType.Bcctr)
            {
                if (noConditionCheckFlag)
                {
                    if (BI == 0)
                    {
                        //bdnz/bdz
                        if (instrType == InstructionType.Bc) type = checkCtrEqualZeroFlag ? MnemonicType.Bdz : MnemonicType.Bdnz;
                        else type = checkCtrEqualZeroFlag ? MnemonicType.Bdzlr : MnemonicType.Bdnzlr; //bclr
                    }
                }
                else
                {
                    if (checkConditionTrueFlag && checkCtrEqualZeroFlag)
                    {
                        //1010 -> bdzt
                        if (instrType == InstructionType.Bc) type = MnemonicType.Bdzt;
                        else type = MnemonicType.Bdztlr; //bclr
                    }
                    else if (checkConditionTrueFlag)
                    {
                        //1000 -> bdnzt
                        if (instrType == InstructionType.Bc) type = MnemonicType.Bdnzt;
                        else type = MnemonicType.Bdnztlr; //bclr
                    }
                    else if (checkCtrEqualZeroFlag)
                    {
                        //0010 -> bdzf
                        if (instrType == InstructionType.Bc) type = MnemonicType.Bdzf;
                        else type = MnemonicType.Bdzflr; //bclr
                    }
                    else
                    {
                        //0000 -> bdnzf
                        if (instrType == InstructionType.Bc) type = MnemonicType.Bdnzf;
                        else type = MnemonicType.Bdnzflr; //bclr
                    }
                }
            }

            return type;
        }

        static int CalculateBranchMnemonicArrayIndex(bool conditionFlag, uint BI)
        {
            //Bits 0-1: condition, bit 2: BO condition true/false flag, bit 3: 0 for cr0, otherwise 1
            int crRegIndex = InstructionFields.GetCrbArgumentRegIndex(BI);
            int crBit = InstructionFields.GetCrbArgumentCrBit(BI);
            int index = (crRegIndex != 0 ? 0b1000 : 0) | ((conditionFlag ? 1 : 0) << 2) | crBit;
            return index;
        }

    }
}
