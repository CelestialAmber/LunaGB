using LunaCube.HW.CPU.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;

namespace LunaCube.HW.CPU.Data {
    internal class ArgumentNames {
        static Dictionary<SpecialReg, string> sprNames = new(){
            { SpecialReg.Xer, "XER" },
            { SpecialReg.Lr, "LR" },
            { SpecialReg.Ctr, "CTR" },
            { SpecialReg.Dsisr, "DSISR" },
            { SpecialReg.Dar, "DAR" },
            { SpecialReg.Dec, "DEC" },
            { SpecialReg.Sdr1, "SDR1" },
            { SpecialReg.Srr0, "SRR0" },
            { SpecialReg.Srr1, "SRR1" },
            { SpecialReg.Sprg0, "SPRG0" },
            { SpecialReg.Sprg1, "SPRG1" },
            { SpecialReg.Sprg2, "SPRG2" },
            { SpecialReg.Sprg3, "SPRG3" },
            { SpecialReg.Ear, "EAR" },
            { SpecialReg.Tbl, "TBL" },
            { SpecialReg.Tbu, "TBU" },
            { SpecialReg.Pvr, "PVR" },
            { SpecialReg.Ibat0U, "IBAT0U" },
            { SpecialReg.Ibat0L, "IBAT0L" },
            { SpecialReg.Ibat1U, "IBAT1U" },
            { SpecialReg.Ibat1L, "IBAT1L" },
            { SpecialReg.Ibat2U, "IBAT2U" },
            { SpecialReg.Ibat2L, "IBAT2L" },
            { SpecialReg.Ibat3U, "IBAT3U" },
            { SpecialReg.Ibat3L, "IBAT3L" },
            { SpecialReg.Dbat0U, "DBAT0U" },
            { SpecialReg.Dbat0L, "DBAT0L" },
            { SpecialReg.Dbat1U, "DBAT1U" },
            { SpecialReg.Dbat1L, "DBAT1L" },
            { SpecialReg.Dbat2U, "DBAT2U" },
            { SpecialReg.Dbat2L, "DBAT2L" },
            { SpecialReg.Dbat3U, "DBAT3U" },
            { SpecialReg.Dbat3L, "DBAT3L" },
            { SpecialReg.Gqr0, "GQR0" },
            { SpecialReg.Gqr1, "GQR1" },
            { SpecialReg.Gqr2, "GQR2" },
            { SpecialReg.Gqr3, "GQR3" },
            { SpecialReg.Gqr4, "GQR4" },
            { SpecialReg.Gqr5, "GQR5" },
            { SpecialReg.Gqr6, "GQR6" },
            { SpecialReg.Gqr7, "GQR7" },
            { SpecialReg.Hid2, "HID2" },
            { SpecialReg.Wpar, "WPAR" },
            { SpecialReg.DmaU, "DMA_U" },
            { SpecialReg.DmaL, "DMA_L" },
            { SpecialReg.Ummcr0, "UMMCR0" },
            { SpecialReg.Upmc1, "UPMC1" },
            { SpecialReg.Upmc2, "UPMC2" },
            { SpecialReg.Usia, "USIA" },
            { SpecialReg.Ummcr1, "UMMCR1" },
            { SpecialReg.Upmc3, "UPMC3" },
            { SpecialReg.Upmc4, "UPMC4" },
            { SpecialReg.Usda, "USDA" },
            { SpecialReg.Mmcr0, "MMCR0" },
            { SpecialReg.Pmc1, "PMC1" },
            { SpecialReg.Pmc2, "PMC2" },
            { SpecialReg.Sia, "SIA" },
            { SpecialReg.Mmcr1, "MMCR1" },
            { SpecialReg.Pmc3, "PMC3" },
            { SpecialReg.Pmc4, "PMC4" },
            { SpecialReg.Sda, "SDA" },
            { SpecialReg.Hid0, "HID0" },
            { SpecialReg.Hid1, "HID1" },
            { SpecialReg.Iabr, "IABR" },
            { SpecialReg.Dabr, "DABR" },
            { SpecialReg.L2cr, "L2CR" },
            { SpecialReg.Ictc, "ICTC" },
            { SpecialReg.Thrm1, "THRM1" },
            { SpecialReg.Thrm2, "THRM2" },
            { SpecialReg.Thrm3, "THRM3" }
        };

        public static string GetSpecialRegName(SpecialReg reg) {
            if (!sprNames.ContainsKey(reg)) {
                Console.WriteLine("Error: register value {0} not found in list", reg);
                return ((int)reg).ToString();
            }
            return sprNames[reg];
        }

        public static string GetCRBitName(CRBit bit) {
            switch (bit) {
                case CRBit.LessThan: return "lt";
                case CRBit.GreaterThan: return "gt";
                case CRBit.Equal: return "eq";
                case CRBit.SummaryOverflow: return "un"; //Should this be un or so?
                default:
                    Console.WriteLine("This will never get called but C# dumb");
                    return "";
            }
        }

        public static string ConvertCRArgumentToString(uint crArg) {
            int crRegIndex = InstructionFields.GetCrbArgumentRegIndex(crArg);
            CRBit bit = (CRBit)InstructionFields.GetCrbArgumentCrBit(crArg);

            //Only include the condition bit name if the register is cr0
            if(crRegIndex == 0) {
                return GetCRBitName(bit);
            }

            return string.Format("cr{0}{1}", crRegIndex, GetCRBitName(bit));
        }
    }
}
