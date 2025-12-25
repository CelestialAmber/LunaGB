using LunaCube.Disassembler;

namespace Tsukimi.Tests.LunaCube.Disasm {
    [TestClass]
    public sealed class GeneralInstructionTest {
        Disassembler disassembler = new Disassembler();

        void DisasmTest(uint instrValue, string disasmString, bool useMnemonics = true) {
            DisassembledInstruction result = disassembler.DisassembleInstruction(instrValue, useMnemonics);
            Assert.AreEqual(disasmString, result.disasmString);
        }

        [TestMethod]
        [DataRow(0x7C432214u, "add r2, r3, r4")]
        [DataRow(0x7CE62A15u, "add. r7, r6, r5")]
        [DataRow(0x7FFFFE14u, "addo r31, r31, r31")]
        [DataRow(0x7F9DF615u, "addo. r28, r29, r30")]
        public void TestInsAdd(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7c002014u, "addc r0, r0, r4")]
        [DataRow(0x7C432014u, "addc r2, r3, r4")]
        [DataRow(0x7CE62815u, "addc. r7, r6, r5")]
        [DataRow(0x7FFFFC14u, "addco r31, r31, r31")]
        [DataRow(0x7F9DF415u, "addco. r28, r29, r30")]
        public void TestInsAddc(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x38010140u, "addi r0, r1, 0x140")]
        [DataRow(0x38010008u, "addi r0, r1, 0x8")]
        [DataRow(0x38010010u, "addi r0, r1, 0x10")]
        [DataRow(0x38010018u, "addi r0, r1, 0x18")]
        [DataRow(0x38049000u, "subi r0, r4, 0x7000")]
        [DataRow(0x38a00000u, "li r5, 0x0")]
        [DataRow(0x38a00000u, "addi r5, r0, 0x0", false)]
        public void TestInsAddi(uint instrValue, string disasmString, bool useMnemonics = true) {
            DisasmTest(instrValue, disasmString, useMnemonics);
        }

        [TestMethod]
        [DataRow(0x7c006114u, "adde r0, r0, r12")]
        [DataRow(0x7C432114u, "adde r2, r3, r4")]
        [DataRow(0x7CE62915u, "adde. r7, r6, r5")]
        [DataRow(0x7FFFFD14u, "addeo r31, r31, r31")]
        [DataRow(0x7F9DF515u, "addeo. r28, r29, r30")]
        public void TestInsAdde(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x3060ffffu, "subic r3, r0, 0x1")]
        [DataRow(0x30840800u, "addic r4, r4, 0x800")]
        [DataRow(0x30a50008u, "addic r5, r5, 0x8")]
        [DataRow(0x37DF001Cu, "addic. r30, r31, 0x1c")]
        [DataRow(0x37E06278u, "addic. r31, r0, 0x6278")]
        [DataRow(0x37E3FFFFu, "subic. r31, r3, 0x1")]
        public void TestInsAddic(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x3C030000u, "addis r0, r3, 0x0")]
        [DataRow(0x3C038000u, "addis r0, r3, 0x8000")]
        [DataRow(0x3D00EFCEu, "lis r8, 0xefce")]
        public void TestInsAddis(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C000194u, "addze r0, r0")]
        public void TestInsAddze(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C001838u, "and r0, r0, r3")]
        [DataRow(0x7C001839u, "and. r0, r0, r3")]
        public void TestInsAnd(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C001878u, "andc r0, r0, r3")]
        public void TestInsAndc(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x70000009u, "andi. r0, r0, 0x9")]
        public void TestInsAndi(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x77c802ffu, "andis. r8, r30, 0x2ff")]
        public void TestInsAndis(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x48000000u, "b 0x0")]
        [DataRow(0x48000004u, "b 0x4")]
        [DataRow(0x4800A5C9u, "bl 0xa5c8")]
        [DataRow(0x4823B4D9u, "bl 0x23b4d8")]
        [DataRow(0x4BE03C99u, "bl -0x1fc368")]
        [DataRow(0x4BDC1A59u, "bl -0x23e5a8")]
        [DataRow(0x48000063u, "bla 0x60")]
        [DataRow(0x48000002u, "ba 0x0")]
        public void TestInsB(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x40800008u, "bge 0x8")]
        [DataRow(0x40802350u, "bge 0x2350")]
        [DataRow(0x4080FC7Cu, "bge -0x384")]
        [DataRow(0x408100ACu, "ble 0xac")]
        [DataRow(0x4081F788u, "ble -0x878")]
        [DataRow(0x40821BA0u, "bne 0x1ba0")]
        [DataRow(0x4082E3C4u, "bne -0x1c3c")]
        [DataRow(0x408600D8u, "bne cr1, 0xd8")]
        [DataRow(0x4086FECCu, "bne cr1, -0x134")]
        [DataRow(0x409C000Cu, "bge cr7, 0xc")]
        [DataRow(0x4180000Cu, "blt 0xc")]
        [DataRow(0x4180F9C0u, "blt -0x640")]
        [DataRow(0x4181021Cu, "bgt 0x21c")]
        [DataRow(0x4181FD80u, "bgt -0x280")]
        [DataRow(0x41822304u, "beq 0x2304")]
        [DataRow(0x4182FE3Cu, "beq -0x1c4")]
        [DataRow(0x418401ACu, "blt cr1, 0x1ac")]
        [DataRow(0x4184FCE4u, "blt cr1, -0x31c")]
        [DataRow(0x418500C0u, "bgt cr1, 0xc0")]
        [DataRow(0x418502E4u, "bgt cr1, 0x2e4")]
        [DataRow(0x419A0138u, "beq cr6, 0x138")]
        [DataRow(0x419C0008u, "blt cr7, 0x8")]
        [DataRow(0x4240FFF0u, "bdz -0x10")]
        [DataRow(0x4200F560u, "bdnz -0xaa0")]
        [DataRow(0x40010014u, "bdnzf gt, 0x14")]
        [DataRow(0x40410035u, "bdzfl gt, 0x34")]
        [DataRow(0x41430023u, "bdztla un, 0x20")]
        [DataRow(0x4108FFE3u, "bdnztla cr2lt, -0x20")]
        [DataRow(0x40A20008u, "bne+ 0x8")]
        public void TestInsBc(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x4E800420u, "bctr")]
        [DataRow(0x4E800421u, "bctrl")]
        [DataRow(0x4D820420u, "beqctr")]
        [DataRow(0x4D8D0421u, "bgtctrl cr3")]
        [DataRow(0x4DA20420u, "beqctr+")]
        [DataRow(0x4DB90421u, "bgtctrl+ cr6")]
        public void TestInsBcctr(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x4C800020u, "bgelr")]
        [DataRow(0x4CA00020u, "bgelr+")]
        [DataRow(0x4C810020u, "blelr")]
        [DataRow(0x4C820020u, "bnelr")]
        [DataRow(0x4C9E0020u, "bnelr cr7")]
        [DataRow(0x4D800020u, "bltlr")]
        [DataRow(0x4D810020u, "bgtlr")]
        [DataRow(0x4D820020u, "beqlr")]
        [DataRow(0x4D860020u, "beqlr cr1")]
        [DataRow(0x4E800020u, "blr")]
        [DataRow(0x4E800021u, "blrl")]
        [DataRow(0x4D000020u, "bdnztlr lt")]
        [DataRow(0x4C1F0021u, "bdnzflrl cr7un")]
        public void TestInsBclr(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C030000u, "cmpw r3, r0")]
        public void TestInsCmp(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x2C050D00u, "cmpwi r5, 0xd00")]
        [DataRow(0x2F1F0000u, "cmpwi cr6, r31, 0x0")]
        public void TestInsCmpi(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C9A2040u, "cmplw cr1, r26, r4")]
        [DataRow(0x7f295840u, "cmpld cr6, r9, r11")]
        public void TestInsCmpl(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x2803FFF3u, "cmplwi r3, 0xfff3")]
        [DataRow(0x2884F8F0u, "cmplwi cr1, r4, 0xf8f0")]
        public void TestInsCmpli(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C030034u, "cntlzw r3, r0")]
        public void TestInsCntlzw(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x4C853202u, "crand cr1lt, cr1gt, cr1eq")]
        public void TestInsCrand(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x4C642902u, "crandc un, cr1lt, cr1gt")]
        public void TestInsCrandc(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x4CE00A42u, "creqv cr1un, lt, gt")]
        public void TestInsCreqv(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x4C2219C2u, "crnand gt, eq, un")]
        public void TestInsCrnand(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x4C411382u, "cror eq, gt, eq")]
        [DataRow(0x4CA63B82u, "cror cr1gt, cr1eq, cr1un")]
        public void TestInsCror(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x4C432342u, "crorc eq, un, cr1lt")]
        public void TestInsCrorc(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x4C011042u, "crnor lt, gt, eq")]
        [DataRow(0x4CA63042u, "crnot cr1gt, cr1eq")]
        public void TestInsCrnor(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x4CC70182u, "crxor cr1eq, cr1un, lt")]
        public void TestInsCrxor(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C0028ACu, "dcbf r0, r5")]
        public void TestInsDcbf(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C001BACu, "dcbi r0, r3")]
        public void TestInsDcbi(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C00286Cu, "dcbst r0, r5")]
        public void TestInsDcbst(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C001A2Cu, "dcbt r0, r3")]
        public void TestInsDcbt(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C001FECu, "dcbz r0, r3")]
        public void TestInsDcbz(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C8073D6u, "divw r4, r0, r14")]
        public void TestInsDivw(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C69E396u, "divwu r3, r9, r28")]
        public void TestInsDivwu(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C650774u, "extsb r5, r3")]
        [DataRow(0x7C650775u, "extsb. r5, r3")]
        public void TestInsExtsb(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C000734u, "extsh r0, r0")]
        [DataRow(0x7C000735u, "extsh. r0, r0")]
        public void TestInsExtsh(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xFC000A10u, "fabs f0, f1")]
        public void TestInsFabs(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xFC00282Au, "fadd f0, f0, f5")]
        public void TestInsFadd(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xEC41602Au, "fadds f2, f1, f12")]
        public void TestInsFadds(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xFC00C840u, "fcmpo cr0, f0, f25")]
        public void TestInsFcmpo(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xFC00D000u, "fcmpu cr0, f0, f26")]
        public void TestInsFcmpu(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xFC20001Eu, "fctiwz f1, f0")]
        public void TestInsFctiwz(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xFC200024u, "fdiv f1, f0, f0")]
        public void TestInsFdiv(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xEC01F824u, "fdivs f0, f1, f31")]
        public void TestInsFdivs(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xEC0200FAu, "fmadds f0, f2, f3, f0")]
        public void TestInsFmadds(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xFC000028u, "fsub f0, f0, f0")]
        public void TestInsFmsub(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xEC00B828u, "fsubs f0, f0, f23")]
        public void TestInsFmsubs(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xFC0000B2u, "fmul f0, f0, f2")]
        [DataRow(0xFC0000F2u, "fmul f0, f0, f3")]
        public void TestInsFmul(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xEC0007B2u, "fmuls f0, f0, f30")]
        public void TestInsFmuls(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xFCE00050u, "fneg f7, f0")]
        public void TestInsFneg(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xFCC640BCu, "fnmsub f6, f6, f2, f8")]
        public void TestInsFnmsub(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xEC022B3Cu, "fnmsubs f0, f2, f12, f5")]
        public void TestInsFnmsubs(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xEC000830u, "fres f0, f1")]
        public void TestInsFres(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xFC000018u, "frsp f0, f0")]
        public void TestInsFrsp(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xFC000834u, "frsqrte f0, f1")]
        public void TestInsFrsqrte(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xFC01F82Eu, "fsel f0, f1, f0, f31")]
        public void TestInsFsel(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xFC000828u, "fsub f0, f0, f1")]
        public void TestInsFsub(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xEC000828u, "fsubs f0, f0, f1")]
        public void TestInsFsubs(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C001FACu, "icbi r0, r3")]
        public void TestInsIcbi(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x4C00012Cu, "isync")]
        public void TestInsIsync(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x880104CCu, "lbz r0, 0x4cc(r1)")]
        [DataRow(0x8802801Bu, "lbz r0, -0x7fe5(r2)")]
        public void TestInsLbz(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x8D9DCA10u, "lbzu r12, -0x35f0(r29)")]
        [DataRow(0x8E3053ECu, "lbzu r17, 0x53ec(r16)")]
        public void TestInsLbzu(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C0400EEu, "lbzux r0, r4, r0")]
        public void TestInsLbzux(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C0300AEu, "lbzx r0, r3, r0")]
        public void TestInsLbzx(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xC80140C8u, "lfd f0, 0x40c8(r1)")]
        [DataRow(0xC8028090u, "lfd f0, -0x7f70(r2)")]
        public void TestInsLfd(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xCC03FFC0u, "lfdu f0, -0x40(r3)")]
        public void TestInsLfdu(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C0404AEu, "lfdx f0, r4, r0")]
        public void TestInsLfdx(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xC001027Cu, "lfs f0, 0x27c(r1)")]
        [DataRow(0xC0028000u, "lfs f0, -0x8000(r2)")]
        public void TestInsLfs(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xC404FFF4u, "lfsu f0, -0xc(r4)")]
        [DataRow(0xC4170084u, "lfsu f0, 0x84(r23)")]
        public void TestInsLfsu(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C03846Eu, "lfsux f0, r3, r16")]
        public void TestInsLfsux(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C03042Eu, "lfsx f0, r3, r0")]
        public void TestInsLfsx(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xA861000Eu, "lha r3, 0xe(r1)")]
        [DataRow(0xA80D9F64u, "lha r0, -0x609c(r13)")]
        public void TestInsLha(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xAC060006u, "lhau r0, 0x6(r6)")]
        [DataRow(0xAC06FFFAu, "lhau r0, -0x6(r6)")]
        public void TestInsLhau(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C0402AEu, "lhax r0, r4, r0")]
        public void TestInsLhax(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xA00104D6u, "lhz r0, 0x4d6(r1)")]
        [DataRow(0xA00296DAu, "lhz r0, -0x6926(r2)")]
        public void TestInsLhz(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xA40A0004u, "lhzu r0, 0x4(r10)")]
        public void TestInsLhzu(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C04026Eu, "lhzux r0, r4, r0")]
        public void TestInsLhzux(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C03022Eu, "lhzx r0, r3, r0")]
        public void TestInsLhzx(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xBB210444u, "lmw r25, 0x444(r1)")]
        public void TestInsLmw(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7D80242Cu, "lwbrx r12, r0, r4")]
        public void TestInsLwbrx(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x800294F4u, "lwz r0, -0x6b0c(r2)")]
        [DataRow(0x80011254u, "lwz r0, 0x1254(r1)")]
        public void TestInsLwz(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x84038608u, "lwzu r0, -0x79f8(r3)")]
        [DataRow(0x873E5058u, "lwzu r25, 0x5058(r30)")]
        public void TestInsLwzu(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C03006Eu, "lwzux r0, r3, r0")]
        public void TestInsLwzux(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C03002Eu, "lwzx r0, r3, r0")]
        public void TestInsLwzx(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x4E1C0000u, "mcrf cr4, cr7")]
        public void TestInsMcrf(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xFE1C0080u, "mcrfs cr4, cr7")]
        public void TestInsMcrfs(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7F800400u, "mcrxr cr7")]
        public void TestInsMcrxr(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C000026u, "mfcr r0")]
        public void TestInsMfcr(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xFC00048Eu, "mffs f0")]
        public void TestInsMffs(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C0000A6u, "mfmsr r0")]
        public void TestInsMfmsr(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7E1A02A6u, "mfsrr0 r16")]
        [DataRow(0x7C70FAA6u, "mfspr r3, HID0")]
        [DataRow(0x7C7482A6u, "mfibatu r3, 2")]
        [DataRow(0x7C7782A6u, "mfibatl r3, 3")]
        public void TestInsMfspr(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7E0004A6u, "mfsr r16, 0")]
        public void TestInsMfsr(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C8C42E6u, "mftb r4, 268")]
        public void TestInsMftb(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C6FF120u, "mtcrf 255, r3")]
        public void TestInsMtcrf(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xFFA0008Cu, "mtfsb0 cr7gt")]
        public void TestInsMtfsb0(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xFFA0004Cu, "mtfsb1 cr7gt")]
        public void TestInsMtfsb1(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xFDFE058Eu, "mtfsf 255, f0")]
        [DataRow(0xFDFEFD8Eu, "mtfsf 255, f31")]
        public void TestInsMtfsf(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C000124u, "mtmsr r0")]
        public void TestInsMtmsr(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7E75FBA6u, "mtspr DABR, r19")]
        [DataRow(0x7C70FBA6u, "mtspr HID0, r3")]
        [DataRow(0x7C7603A6u, "mtdec r3")]
        [DataRow(0x7C7043A6u, "mtsprg 0, r3")]
        [DataRow(0x7C7143A6u, "mtsprg 1, r3")]
        [DataRow(0x7C7343A6u, "mtsprg 3, r3")]
        [DataRow(0x7C7083A6u, "mtibatu 0, r3")]
        [DataRow(0x7C7483A6u, "mtibatu 2, r3")]
        [DataRow(0x7C7783A6u, "mtibatl 3, r3")]
        [DataRow(0x7C7D83A6u, "mtdbatl 2, r3")]
        public void TestInsMtspr(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7E0001A4u, "mtsr 0, r16")]
        public void TestInsMtsr(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C7F2096u, "mulhw r3, r31, r4")]
        public void TestInsMulhw(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C7D0016u, "mulhwu r3, r29, r0")]
        public void TestInsMulhwu(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x1C001880u, "mulli r0, r0, 0x1880")]
        [DataRow(0x1FBD0030u, "mulli r29, r29, 0x30")]
        public void TestInsMulli(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C7D01D6u, "mullw r3, r29, r0")]
        public void TestInsMullw(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C7D03B8u, "nand r29, r3, r0")]
        public void TestInsNand(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C0600D0u, "neg r0, r6")]
        [DataRow(0x7C4A00D1u, "neg. r2, r10")]
        [DataRow(0x7CC104D0u, "nego r6, r1")]
        [DataRow(0x7DF004D1u, "nego. r15, r16")]
        public void TestInsNeg(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C0500F8u, "nor r5, r0, r0")]
        public void TestInsNor(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C04DB78u, "or r4, r0, r27")]
        public void TestInsOr(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C042338u, "orc r4, r0, r4")]
        public void TestInsOrc(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x60002204u, "ori r0, r0, 0x2204")]
        public void TestInsOri(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x67A06800u, "oris r0, r29, 0x6800")]
        public void TestInsOris(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x4C000064u, "rfi")]
        public void TestInsRfi(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x500306FEu, "rlwimi r3, r0, 0, 27, 31")]
        [DataRow(0x50032D74u, "rlwimi r3, r0, 5, 21, 26")]
        [DataRow(0x5400003Fu, "clrrwi. r0, r0, 0")]
        public void TestInsRlwimi(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x54000423u, "rlwinm. r0, r0, 0, 16, 17")]
        [DataRow(0x54000432u, "rlwinm r0, r0, 0, 16, 25")]
        [DataRow(0x54096226u, "rlwinm r9, r0, 12, 8, 19", false)]
        [DataRow(0x57E5103Au, "slwi r5, r31, 2")]
        [DataRow(0x54832026u, "extlwi r3, r4, 20, 4")]
        [DataRow(0x5483AB3Eu, "extrwi r3, r4, 20, 1")]
        [DataRow(0x540027BEu, "extrwi r0, r0, 2, 2")]
        [DataRow(0x54839B3Eu, "rlwinm r3, r4, 19, 12, 31")]
        [DataRow(0x5483203Eu, "rotlwi r3, r4, 4")]
        [DataRow(0x5483E03Eu, "rotrwi r3, r4, 4")]
        [DataRow(0x5464043Eu, "clrlwi r4, r3, 16")]
        [DataRow(0x54830036u, "clrrwi r3, r4, 4")]
        [DataRow(0x54640FBCu, "clrlslwi r4, r3, 31, 1")]
        [DataRow(0x54092DB4u, "clrlslwi r9, r0, 27, 5")]
        [DataRow(0x54096226u, "clrlslwi r9, r0, 20, 12")]
        public void TestInsRlwinm(uint instrValue, string disasmString, bool useMnemonics = true) {
            DisasmTest(instrValue, disasmString, useMnemonics);
        }

        [TestMethod]
        [DataRow(0x5D6A67FEu, "rlwnm r10, r11, r12, 31, 31")]
        [DataRow(0x5FC52EFEu, "rlwnm r5, r30, r5, 27, 31")]
        [DataRow(0x5FC5283Fu, "rotlw. r5, r30, r5")]
        public void TestInsRlwnm(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x44000002u, "sc")]
        public void TestInsSc(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C042830u, "slw r4, r0, r5")]
        public void TestInsSlw(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C043E30u, "sraw r4, r0, r7")]
        public void TestInsSraw(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C000E70u, "srawi r0, r0, 1")]
        [DataRow(0x7C001670u, "srawi r0, r0, 2")]
        public void TestInsSrawi(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C001C30u, "srw r0, r0, r3")]
        [DataRow(0x7C600430u, "srw r0, r3, r0")]
        public void TestInsSrw(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x980105ECu, "stb r0, 0x5ec(r1)")]
        [DataRow(0x98030000u, "stb r0, 0x0(r3)")]
        public void TestInsStb(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x9D2A7428u, "stbu r9, 0x7428(r10)")]
        [DataRow(0x9D66FFFFu, "stbu r11, -0x1(r6)")]
        public void TestInsStbu(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C08F9EEu, "stbux r0, r8, r31")]
        public void TestInsStbux(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C03F9AEu, "stbx r0, r3, r31")]
        public void TestInsStbx(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xD80D97B0u, "stfd f0, -0x6850(r13)")]
        [DataRow(0xD8050090u, "stfd f0, 0x90(r5)")]
        public void TestInsStfd(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xDC24FFC0u, "stfdu f1, -0x40(r4)")]
        public void TestInsStfdu(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C4405AEu, "stfdx f2, r4, r0")]
        public void TestInsStfdx(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xD003086Cu, "stfs f0, 0x86c(r3)")]
        [DataRow(0xD0038000u, "stfs f0, -0x8000(r3)")]
        public void TestInsStfs(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C465D2Eu, "stfsx f2, r6, r11")]
        public void TestInsStfsx(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xB0038A7Cu, "sth r0, -0x7584(r3)")]
        [DataRow(0xB0035036u, "sth r0, 0x5036(r3)")]
        public void TestInsSth(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C60072Cu, "sthbrx r3, r0, r0")]
        public void TestInsSthbrx(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xB4055B88u, "sthu r0, 0x5b88(r5)")]
        public void TestInsSthu(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C03236Eu, "sthux r0, r3, r4")]
        public void TestInsSthux(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C1C2B2Eu, "sthx r0, r28, r5")]
        public void TestInsSthx(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0xBFA202A4u, "stmw r29, 0x2a4(r2)")]
        public void TestInsStmw(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x900140CCu, "stw r0, 0x40cc(r1)")]
        [DataRow(0x9003FFBCu, "stw r0, -0x44(r3)")]
        public void TestInsStw(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C00FD2Cu, "stwbrx r0, r0, r31")]
        public void TestInsStwbrx(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x9421EBC0u, "stwu r1, -0x1440(r1)")]
        public void TestInsStwu(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C01B96Eu, "stwux r0, r1, r23")]
        public void TestInsStwux(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C03212Eu, "stwx r0, r3, r4")]
        public void TestInsStwx(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C051850u, "subf r0, r5, r3")]
        [DataRow(0x7C051851u, "subf. r0, r5, r3")]
        public void TestInsSubf(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C040010u, "subfc r0, r4, r0")]
        public void TestInsSubfc(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C030110u, "subfe r0, r3, r0")]
        public void TestInsSubfe(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x200602FFu, "subfic r0, r6, 0x2ff")]
        public void TestInsSubfic(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C000190u, "subfze r0, r0")]
        public void TestInsSubfze(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7c0004acu, "sync")]
        public void TestInsSync(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C001A64u, "tlbie r3")]
        public void TestTlbie(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C00046Cu, "tlbsync")]
        public void TestTlbsync(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C063808u, "tw 0, r6, r7")]
        [DataRow(0x7C842808u, "tweq r4, r5")]
        [DataRow(0x7CA42808u, "twlge r4, r5")]
        [DataRow(0x7FE00008u, "trap")]
        public void TestTw(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x0C000000u, "twi 0, r0, 0x0")]
        [DataRow(0x0D07FFFFu, "twgti r7, -0x1")]
        [DataRow(0x0CC4FF01u, "twllei r4, -0xff")]
        [DataRow(0x0FE40003u, "twui r4, 0x3")]
        public void TestTwi(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x7C052A78u, "xor r5, r0, r5")]
        [DataRow(0x7D275279u, "xor. r7, r9, r10")]
        public void TestInsXor(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x68E71021u, "xori r7, r7, 0x1021")]
        public void TestInsXori(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }

        [TestMethod]
        [DataRow(0x6E3D8000u, "xoris r29, r17, 0x8000")]
        public void TestInsXoris(uint instrValue, string disasmString) {
            DisasmTest(instrValue, disasmString);
        }
    }
}
