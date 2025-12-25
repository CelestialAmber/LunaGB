using LunaCube.Disassembler;

namespace Tsukimi.Tests;

[TestClass]
public class PSTest
{
    Disassembler disassembler = new Disassembler();

    void DisasmTest(uint instrValue, string disasmString, bool useMnemonics = true) {
        DisassembledInstruction result = disassembler.DisassembleInstruction(instrValue, useMnemonics);
        Assert.AreEqual(disasmString, result.disasmString);
    }

    [TestMethod]
    [DataRow(0x10061FECu, "dcbz_l r6, r3")]
    public void test_ins_dcbz_l(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0xE02500ACu, "psq_l f1, 0xac(r5), 0, qr0")]
    public void test_ins_psq_l(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0xE5435010u, "psq_lu f10, 0x10(r3), 0, qr5")]
    public void test_ins_psq_lu(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x1000000Cu, "psq_lx f0, r0, r0, 0, qr0")]
    public void test_ins_psq_lx(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0xF1230210u, "psq_st f9, 0x210(r3), 0, qr0")]
    [DataRow(0xF1238008u, "psq_st f9, 0x8(r3), 1, qr0")]
    public void test_ins_psq_st(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0xF40A0020u, "psq_stu f0, 0x20(r10), 0, qr0")]
    public void test_ins_psq_stu(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x13E1000Eu, "psq_stx f31, r1, r0, 0, qr0")]
    public void test_ins_psq_stx(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x10A03210u, "ps_abs f5, f6")]
    public void test_ins_ps_abs(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x1006382Au, "ps_add f0, f6, f7")]
    public void test_ins_ps_add(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x10070840u, "ps_cmpo0 cr0, f7, f1")]
    public void test_ins_ps_cmpo0(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x10003000u, "ps_cmpu0 cr0, f0, f6")]
    public void test_ins_ps_cmpu0(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x10003080u, "ps_cmpu1 cr0, f0, f6")]
    public void test_ins_ps_cmpu1(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x112141FAu, "ps_madd f9, f1, f7, f8")]
    public void test_ins_ps_madd(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x10AC299Cu, "ps_madds0 f5, f12, f6, f5")]
    public void test_ins_ps_madds0(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x110640DEu, "ps_madds1 f8, f6, f3, f8")]
    public void test_ins_ps_madds1(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x10400420u, "ps_merge00 f2, f0, f0")]
    public void test_ins_ps_merge00(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x10400C60u, "ps_merge01 f2, f0, f1")]
    public void test_ins_ps_merge01(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x104004A0u, "ps_merge10 f2, f0, f0")]
    public void test_ins_ps_merge10(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x10AA14E0u, "ps_merge11 f5, f10, f2")]
    public void test_ins_ps_merge11(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x10200090u, "ps_mr f1, f0")]
    public void test_ins_ps_mr(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x10A53778u, "ps_msub f5, f5, f29, f6")]
    public void test_ins_ps_msub(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x10000032u, "ps_mul f0, f0, f0")]
    public void test_ins_ps_mul(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x100002D8u, "ps_muls0 f0, f0, f11")]
    public void test_ins_ps_muls0(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x10A2005Au, "ps_muls1 f5, f2, f1")]
    public void test_ins_ps_muls1(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x10803210u, "ps_abs f4, f6")]
    public void test_ins_ps_nabs(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x10E03850u, "ps_neg f7, f7")]
    public void test_ins_ps_neg(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x10CB30FEu, "ps_nmadd f6, f11, f3, f6")]
    public void test_ins_ps_nmadd(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x107E083Cu, "ps_nmsub f3, f30, f0, f1")]
    public void test_ins_ps_nmsub(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x106428EEu, "ps_sel f3, f4, f3, f5")]
    public void test_ins_ps_sel(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x10A92828u, "ps_sub f5, f9, f5")]
    public void test_ins_ps_sub(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x10230854u, "ps_sum0 f1, f3, f1, f1")]
    public void test_ins_ps_sum0(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }

    [TestMethod]
    [DataRow(0x10A12956u, "ps_sum1 f5, f1, f5, f5")]
    public void test_ins_ps_sum1(uint instrValue, string disasmString) {
        DisasmTest(instrValue, disasmString);
    }
}
