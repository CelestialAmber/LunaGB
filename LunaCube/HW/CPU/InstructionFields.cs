using LunaCube.HW.CPU.Data;
using LunaCube.HW.CPU.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Tsukimi.Utils;

namespace LunaCube.HW.CPU {
    //Why does PPC encoding have to be such a mess :<

    public struct InstructionFields {
        public uint instrVal;
        public InstructionType instruction;

        //Main opcode
        public uint opcd => instrVal.GetBits(0, 5);

        /// <summary>
        /// SIMM/offset
        /// </summary>
        public int signedImmediate {
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

        public BranchOptions BO {
            get { return (BranchOptions)commonArg1; }
        }

        //BI format:
        //Bits 0-3: condition register
        //Bits 4-5: condition
        //public byte BI;

        //14 bit signed address offset (branch displacement)
        public int BD {
            get {
                //The raw value encodes the number of instructions
                uint rawValue = instrVal.GetBits(16, 29) * 4;
                //Sign extend to 32 bits
                return BitUtils.SignExtend(rawValue, 16);
            }
        }

        //24 bit signed address offset
        public int LI {
            get {
                //The raw value encodes the number of instructions
                uint rawValue = instrVal.GetBits(6, 29) * 4;
                //Sign extend to 32 bits
                return BitUtils.SignExtend(rawValue, 26);
            }
        }

        public TrapOptions TO {
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
        public int L => (int)instrVal.GetBits(10, 10);

        //Paired singles

        public int psOffset {
            get { return (int)instrVal.GetBits(20, 31); }
        }

        public uint psI => instrVal.GetBits(17, 19);
        public uint psIX => instrVal.GetBits(22, 24);
        public uint psW => instrVal.GetBits(16, 16);
        public uint psWX => instrVal.GetBits(21, 21);

        //Modifiers
        public bool OE => instrVal.GetBits(21, 21) == 1 ? true : false;
        public bool AA => instrVal.GetBits(30, 30) == 1 ? true : false;
        public bool LK => instrVal.GetBits(31, 31) == 1 ? true : false;
        public bool Rc => instrVal.GetBits(31, 31) == 1 ? true : false;

        //Mnemonic specific

        //SPRG m(t/f)spr mnemonics index field
        public uint spr_SPRG => instrVal.GetBits(14, 15);
        //BAT m(t/f)spr mnemonics index field
        public uint spr_BAT => instrVal.GetBits(13, 14);

        public InstructionFields() {
            instrVal = 0;
        }

        public InstructionFields(uint instrVal) {
            this.instrVal = instrVal;
        }

        //Utility functions

        uint GetFiveBitField(int startBit) {
            int endBit = startBit + 4;
            return instrVal.GetBits(startBit, endBit);
        }

        uint GetSplitField() {
            //Reverse the order of the two 5 bit parts
            uint val = instrVal.GetBits(11, 20);
            uint lo = val >> 5 & 0b11111;
            uint hi = (val & 0b11111);
            return (hi << 5) | lo;
        }
        
        //Idk where to put these functions so ig i'll keep them here for now
        /* CRB argument utility functions. Using a dedicated struct to allow the individual parts to be easily accessed might be cleaner,
        but doing it like this is more efficient. */

        //Get conditional register number from cr argument (bits 0-2)
        public static int GetCrbArgumentRegIndex(uint crbArg) {
            return (int)((crbArg >> 2) & 0b111);
        }

        //Get cr bit number from cr argument (bits 3-4)
        public static int GetCrbArgumentCrBit(uint crbArg) {
            return (int)(crbArg & 0b11);
        }

    }
}
