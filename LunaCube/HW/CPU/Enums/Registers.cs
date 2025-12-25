using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaCube.HW.CPU.Enums
{
    enum GQRDataType
    {
        Float = 0,
        Byte = 4,
        Ushort = 5,
        Sbyte = 6,
        Short = 7
    }

    enum SpecialReg
    {
        Xer = 1,
        Lr = 8,
        Ctr,
        Dsisr = 18,
        Dar,
        Dec = 22,
        Sdr1 = 25,
        Srr0,
        Srr1,
        Sprg0 = 272,
        Sprg1,
        Sprg2,
        Sprg3,
        Ear = 282,
        Tbl = 284,
        Tbu,
        Pvr = 287,
        Ibat0U = 528,
        Ibat0L,
        Ibat1U,
        Ibat1L,
        Ibat2U,
        Ibat2L,
        Ibat3U,
        Ibat3L,
        Dbat0U,
        Dbat0L,
        Dbat1U,
        Dbat1L,
        Dbat2U,
        Dbat2L,
        Dbat3U,
        Dbat3L,
        Gqr0 = 912,
        Gqr1,
        Gqr2,
        Gqr3,
        Gqr4,
        Gqr5,
        Gqr6,
        Gqr7,
        Hid2,
        Wpar,
        DmaU,
        DmaL,
        Ummcr0 = 936,
        Upmc1,
        Upmc2,
        Usia,
        Ummcr1,
        Upmc3,
        Upmc4,
        Usda,
        Mmcr0 = 952,
        Pmc1,
        Pmc2,
        Sia,
        Mmcr1,
        Pmc3,
        Pmc4,
        Sda,
        Hid0 = 1008,
        Hid1,
        Iabr,
        Dabr = 1013,
        L2cr = 1017,
        Ictc = 1019,
        Thrm1,
        Thrm2,
        Thrm3

    }

    [Flags]
    enum CRBit {
        LessThan,
        GreaterThan,
        Equal,
        SummaryOverflow
    }

}
