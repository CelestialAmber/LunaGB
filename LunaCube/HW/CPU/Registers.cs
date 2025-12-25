using System;
using System.Reflection.Emit;
using Tsukimi.Utils;

namespace LunaCube.HW.CPU
{

    //Register structs
    //If only there were a better way :/

    internal struct GQR
    {
        public uint regValue;

        //load scale, bits 24-29
        public byte LD_SCALE {
            get{ return (byte)regValue.GetBits(24, 29); }
            set{ regValue.SetBits(24, 29, value); }
        }
        //load type, bits 16-18
        public byte LD_TYPE {
            get { return (byte)regValue.GetBits(16, 18); }
            set { regValue.SetBits(16, 18, value); }
        }
        //store scale, bits 8-13
        public byte ST_SCALE {
            get { return (byte)regValue.GetBits(8, 13); }
            set { regValue.SetBits(8, 13, value); }
        }
        //store type, bits 0-2
        public byte ST_TYPE {
            get { return (byte)regValue.GetBits(0, 2); }
            set { regValue.SetBits(0, 2, value); }
        }

        public GQR() {
            regValue = 0;
        }
    }

    internal struct BATReg
    {
        public uint batl;
        public uint batu;

        //protection bits, bits 0-1 (lower)
        //00: no access, x1: read only, 10: read/write
        public uint PP
        {
            get { return batl.GetBits(0, 1); }
            set { batl.SetBits(0, 1, value); }
        }
        //storage access controls, bits 3-6 (lower)
        public uint WIMG
        {
            get { return batl.GetBits(3, 6); }
            set { batl.SetBits(3, 6, value); }
        }
        //block real page number, bits 17-31 (lower)
        public uint BRPN
        {
            get { return batl.GetBits(17, 31); }
            set { batl.SetBits(17, 31, value); }
        }
        //problem state valid bit
        public uint VP;
        //supervisor state valid bit
        public uint VS;
        //block length mask
        public uint BL;
        //block effective page index
        public uint BEPI;

        public BATReg()
        {
            batl = 0;
            batu = 0;
        }
    }

    internal struct MSR
    {

    }

    internal struct PVR
    {

    }

    internal struct DABR
    {

    }

    internal struct XER
    {

    }

    internal class Registers
    {
        //General purpose registers
        public uint[] gprs = new uint[32];
        //Floating point registers
        public double[] fprs = new double[32];
        //Graphics quantization registers

        /* Only 4 of each BAT register exist on Gekko vs Broadway, but all 8 are included here to make things
        easier. (this should be handled in other cpu files, maybe through an enum for the system) */
        //Instruction BAT registers (IBAT0-7 U/L)
        public BATReg[] ibatRegs = new BATReg[8];
        //Data BAT registers (DBAT0-7 U/L)
        public BATReg[] dbatRegs = new BATReg[8];
        //Machine State Register
        public MSR msr;


        public Registers()
        {
            
        }

        public void Init()
        {
            //TODO: figure out how to properly init bat registers
            for(int i = 0; i < 8; i++)
            {
                //Init ibat
                BATReg ibat = ibatRegs[i];
                //Init dbat
                BATReg dbat = dbatRegs[i];
            }
        }

    }
}
