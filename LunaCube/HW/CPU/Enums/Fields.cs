using System;

namespace LunaCube.HW.CPU.Enums {

    /* On Broadway/Gekko, the following combinations are specified, which gives details
    on how the bits are expected to be set:
    y: prediction bit, z: ignored bit (should be 0)

    0000y: ctr--, if(ctr != 0 && cnd == false) branch
    0001y: ctr--, if(ctr == 0 && cnd == false) branch
    001zy: if(cnd == false) branch
    0100y: ctr--, if(ctr != 0 && cnd == false) branch
    0101y: ctr--, if(ctr == 0 && cnd == false) branch
    011zy: if(cnd == true) branch
    1z00y: ctr--, if(ctr != 0) branch
    1z01y: ctr--, if(ctr == 0) branch
    1z1zz: always branch

    Essentially, if either of the condition disable bits are set (bis 0/2), the corresponding
    condition option bits (bits 1/3) should be ignored/set to zero. Additionally, if both
    are disabled, the prediction bit should be ignored/set to zero. */
    [Flags]
    public enum BranchOptions : byte
    {
        Prediction = 1 << 0, //If 1, signifies that the non-default prediction should be used
        CheckCtrEqualZero = 1 << 1, //0: ctr != 0, 1: ctr == 0
        NoCtrDecrementCheck = 1 << 2, //If 0, decrement ctr, and check its value based on the above flag
        CheckConditionTrue = 1 << 3, //0: condition bit == false, condition bit == true
        NoConditionCheck = 1 << 4 //If 0, check condition bit
    }

    [Flags]
    public enum TrapOptions : byte
    {
        LessThanSigned = 1 << 0,
        GreaterThanSigned = 1 << 1,
        Equal = 1 << 2,
        LessThanUnsigned = 1 << 3,
        GreaterThanUnsigned = 1 << 4,
        Unconditional = LessThanSigned | GreaterThanSigned | Equal | LessThanUnsigned | GreaterThanUnsigned
    }

    public static class FieldEnumExtensions
    {
        public static bool CheckBit(this BranchOptions options, BranchOptions bit)
        {
            return ((int)options & (int)bit) != 0;
        }

        public static int GetBit(this BranchOptions options, BranchOptions bit)
        {
            return ((int)options >> (int)bit) & 1;
        }
    }
}
