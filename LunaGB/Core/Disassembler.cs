using System;
using System.Security.Cryptography;

namespace LunaGB.Core {
	public class Disassembler {
		Memory memory;

		public enum InstructionType {
			Nop,
			Load,
			LoadH,
			Inc,
			Dec,
			Add,
			Adc,
			Sub,
			Sbc,
			And,
			Or,
			Xor,
			Rlca,
			Rrca,
			Rla,
			Rra,
			Daa,
			Cpl,
			Ccf,
			Scf,
			Compare,
			Jump,
			RelativeJump,
			Call,
			Rst,
			Ret,
			Reti,
			Push,
			Pop,
			Di,
			Ei,
			Stop,
			Halt
		}

		public enum OpcodeParam {
			Immediate8,
			ImmediateSigned8,
			Immediate16,
			A,
			B,
			C,
			D,
			E,
			F,
			H,
			L,
			AF,
			BC,
			DE,
			HL,
			SP,
			HLMinus,
			HLPlus,
			Rst00,
			Rst08,
			Rst10,
			Rst18,
			Rst20,
			Rst28,
			Rst30,
			Rst38,
			SPPlusImmediate
        }

		public enum Condition {
			None,
			Zero,
			Carry,
			NotZero,
			NoCarry
        }

		public class OpcodeParameter
		{
			public OpcodeParam type;
			public bool referencesMemory; //whether the parameter references memory

			public OpcodeParameter(OpcodeParam type, bool referencesMemory)
			{
				this.type = type;
				this.referencesMemory = referencesMemory;
			}

			public OpcodeParameter(OpcodeParam type)
			{
				this.type = type;
				referencesMemory = false;
			}
		}

		public class Opcode {
			public InstructionType instructionType;
			public OpcodeParameter[] parameters;
			public Condition condition;


			public Opcode() { }

			public Opcode(InstructionType instructionType) {
				this.instructionType = instructionType;
			}


			public Opcode(InstructionType instructionType, OpcodeParameter[] parameters) {
				this.instructionType = instructionType;
				this.parameters = parameters;
            }

			public Opcode(InstructionType instructionType, OpcodeParameter[] parameters, Condition condition) {
				this.instructionType = instructionType;
				this.parameters = parameters;
				this.condition = condition;
			}
		}

		public Opcode[] opcodes = {
			//00
			new Opcode(InstructionType.Nop),
			//01
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[]{
					new OpcodeParameter(OpcodeParam.BC),
					new OpcodeParameter(OpcodeParam.Immediate16)
				}
			),
			//02
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.BC,true),
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//03
			new Opcode(
				InstructionType.Inc,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.BC)
				}
			),
			//04
			new Opcode(
				InstructionType.Inc,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.B)
				}
			),
			//05
			new Opcode(
				InstructionType.Dec,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.B)
				}
			),
			//06
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.B),
				new OpcodeParameter(OpcodeParam.Immediate8)
				}
			),
			//07
			new Opcode(
				InstructionType.Rlca
			),
			//08
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.Immediate16,true),
				new OpcodeParameter(OpcodeParam.SP)
				}
			),
			//09
			new Opcode(
				InstructionType.Add,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.HL),
				new OpcodeParameter(OpcodeParam.BC)
				}
			),
			//0A
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.A),
				new OpcodeParameter(OpcodeParam.BC,true)
				}
			),
			//0B
			new Opcode(
				InstructionType.Dec,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.BC)
				}
			),
			//0C
			new Opcode(
				InstructionType.Inc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.C)
				}
			),
			//0D
			new Opcode(
				InstructionType.Dec,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.C)
				}
			),
			//0E
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.C),
				new OpcodeParameter(OpcodeParam.Immediate8)
				}
			),
			//0F
			new Opcode(
				InstructionType.Rrca
			),
			//00
			new Opcode(InstructionType.Stop),
			//01
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.DE),
				new OpcodeParameter(OpcodeParam.Immediate16)
				}
			),
			//02
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.DE,true),
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//03
			new Opcode(
				InstructionType.Inc,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.DE)
				}
			),
			//04
			new Opcode(
				InstructionType.Inc,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.D)
				}
			),
			//05
			new Opcode(
				InstructionType.Dec,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.D)
				}
			),
			//06
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.D),
				new OpcodeParameter(OpcodeParam.Immediate8)
				}
			),
			//07
			new Opcode(
				InstructionType.Rla
			),
			//08
			new Opcode(
				InstructionType.RelativeJump,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.ImmediateSigned8)
				}
			),
			//09
			new Opcode(
				InstructionType.Add,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.HL),
				new OpcodeParameter(OpcodeParam.DE)
				}
			),
			//0A
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.A),
				new OpcodeParameter(OpcodeParam.DE,true)
				}
			),
			//0B
			new Opcode(
				InstructionType.Dec,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.DE)
				}
			),
			//0C
			new Opcode(
				InstructionType.Inc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.E)
				}
			),
			//0D
			new Opcode(
				InstructionType.Dec,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.E)
				}
			),
			//0E
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.E),
				new OpcodeParameter(OpcodeParam.Immediate8)
				}
			),
			//0F
			new Opcode(
				InstructionType.Rra
			),
			//20
			new Opcode(
				InstructionType.RelativeJump,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.ImmediateSigned8)
				},
				Condition.NotZero
			),
			//01
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.HL),
				new OpcodeParameter(OpcodeParam.Immediate16)
				}
			),
			//02
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.HLPlus,true),
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//03
			new Opcode(
				InstructionType.Inc,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.HL)
				}
			),
			//04
			new Opcode(
				InstructionType.Inc,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.H)
				}
			),
			//05
			new Opcode(
				InstructionType.Dec,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.H)
				}
			),
			//06
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.H),
				new OpcodeParameter(OpcodeParam.Immediate8)
				}
			),
			//07
			new Opcode(
				InstructionType.Daa
			),
			//08
			new Opcode(
				InstructionType.RelativeJump,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.ImmediateSigned8),
				},
				Condition.Zero
			),
			//09
			new Opcode(
				InstructionType.Add,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.HL),
				new OpcodeParameter(OpcodeParam.HL)
				}
			),
			//0A
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.A),
				new OpcodeParameter(OpcodeParam.HLPlus,true)
				}
			),
			//0B
			new Opcode(
				InstructionType.Dec,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.HL)
				}
			),
			//0C
			new Opcode(
				InstructionType.Inc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.L)
				}
			),
			//0D
			new Opcode(
				InstructionType.Dec,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.L)
				}
			),
			//0E
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.L),
				new OpcodeParameter(OpcodeParam.Immediate8)
				}
			),
			//2F
			new Opcode(
				InstructionType.Cpl
			),
			//30
			new Opcode(
				InstructionType.RelativeJump,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.ImmediateSigned8)
				},
				Condition.NoCarry
			),
			//31
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.SP),
				new OpcodeParameter(OpcodeParam.Immediate16)
				}
			),
			//02
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.HLMinus,true),
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//03
			new Opcode(
				InstructionType.Inc,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.SP)
				}
			),
			//04
			new Opcode(
				InstructionType.Inc,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.HL,true)
				}
			),
			//05
			new Opcode(
				InstructionType.Dec,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.HL,true)
				}
			),
			//06
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.HL,true),
				new OpcodeParameter(OpcodeParam.Immediate8)
				}
			),
			//07
			new Opcode(
				InstructionType.Scf
			),
			//08
			new Opcode(
				InstructionType.RelativeJump,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.ImmediateSigned8),
				},
				Condition.Carry
			),
			//09
			new Opcode(
				InstructionType.Add,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.HL),
				new OpcodeParameter(OpcodeParam.SP)
				}
			),
			//0A
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.A),
				new OpcodeParameter(OpcodeParam.HLMinus,true)
				}
			),
			//0B
			new Opcode(
				InstructionType.Dec,
				new OpcodeParameter[]{
				new OpcodeParameter(OpcodeParam.SP)
				}
			),
			//3C
			new Opcode(
				InstructionType.Inc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//3D
			new Opcode(
				InstructionType.Dec,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//3E
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A),
				new OpcodeParameter(OpcodeParam.Immediate8)
				}
			),
			//3F
			new Opcode(
				InstructionType.Ccf
			),
			//40
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.B),
				new OpcodeParameter(OpcodeParam.B)
				}
			),
			//41
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.B),
				new OpcodeParameter(OpcodeParam.C)
				}
			),
			//42
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.B),
				new OpcodeParameter(OpcodeParam.D)
				}
			),
			//43
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.B),
				new OpcodeParameter(OpcodeParam.E)
				}
			),
			//44
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.B),
				new OpcodeParameter(OpcodeParam.H)
				}
			),
			//45
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.B),
				new OpcodeParameter(OpcodeParam.L)
				}
			),
			//46
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.B),
				new OpcodeParameter(OpcodeParam.HL,true)
				}
			),
			//47
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.B),
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//48
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.C),
				new OpcodeParameter(OpcodeParam.B)
				}
			),
			//49
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.C),
				new OpcodeParameter(OpcodeParam.C)
				}
			),
			//4A
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.C),
				new OpcodeParameter(OpcodeParam.D)
				}
			),
			//4B
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.C),
				new OpcodeParameter(OpcodeParam.E)
				}
			),
			//4C
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.C),
				new OpcodeParameter(OpcodeParam.H)
				}
			),
			//4D
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.C),
				new OpcodeParameter(OpcodeParam.L)
				}
			),
			//4E
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.C),
				new OpcodeParameter(OpcodeParam.HL,true)
				}
			),
			//4F
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.C),
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//40
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.D),
				new OpcodeParameter(OpcodeParam.B)
				}
			),
			//41
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.D),
				new OpcodeParameter(OpcodeParam.C)
				}
			),
			//42
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.D),
				new OpcodeParameter(OpcodeParam.D)
				}
			),
			//43
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.D),
				new OpcodeParameter(OpcodeParam.E)
				}
			),
			//44
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.D),
				new OpcodeParameter(OpcodeParam.H)
				}
			),
			//45
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.D),
				new OpcodeParameter(OpcodeParam.L)
				}
			),
			//46
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.D),
				new OpcodeParameter(OpcodeParam.HL,true)
				}
			),
			//47
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.D),
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//48
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.E),
				new OpcodeParameter(OpcodeParam.B)
				}
			),
			//49
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.E),
				new OpcodeParameter(OpcodeParam.C)
				}
			),
			//4A
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.E),
				new OpcodeParameter(OpcodeParam.D)
				}
			),
			//4B
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.E),
				new OpcodeParameter(OpcodeParam.E)
				}
			),
			//4C
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.E),
				new OpcodeParameter(OpcodeParam.H)
				}
			),
			//4D
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.E),
				new OpcodeParameter(OpcodeParam.L)
				}
			),
			//4E
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.E),
				new OpcodeParameter(OpcodeParam.HL,true)
				}
			),
			//4F
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.E),
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//40
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.H),
				new OpcodeParameter(OpcodeParam.B)
				}
			),
			//41
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.H),
				new OpcodeParameter(OpcodeParam.C)
				}
			),
			//42
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.H),
				new OpcodeParameter(OpcodeParam.D)
				}
			),
			//43
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.H),
				new OpcodeParameter(OpcodeParam.E)
				}
			),
			//44
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.H),
				new OpcodeParameter(OpcodeParam.H)
				}
			),
			//45
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.H),
				new OpcodeParameter(OpcodeParam.L)
				}
			),
			//46
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.H),
				new OpcodeParameter(OpcodeParam.HL,true)
				}
			),
			//47
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.H),
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//48
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.L),
				new OpcodeParameter(OpcodeParam.B)
				}
			),
			//49
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.L),
				new OpcodeParameter(OpcodeParam.C)
				}
			),
			//4A
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.L),
				new OpcodeParameter(OpcodeParam.D)
				}
			),
			//4B
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.L),
				new OpcodeParameter(OpcodeParam.E)
				}
			),
			//4C
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.L),
				new OpcodeParameter(OpcodeParam.H)
				}
			),
			//4D
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.L),
				new OpcodeParameter(OpcodeParam.L)
				}
			),
			//4E
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.L),
				new OpcodeParameter(OpcodeParam.HL,true)
				}
			),
			//4F
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.L),
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//40
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.HL,true),
				new OpcodeParameter(OpcodeParam.B)
				}
			),
			//41
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.HL,true),
				new OpcodeParameter(OpcodeParam.C)
				}
			),
			//42
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.HL,true),
				new OpcodeParameter(OpcodeParam.D)
				}
			),
			//43
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.HL,true),
				new OpcodeParameter(OpcodeParam.E)
				}
			),
			//44
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.HL,true),
				new OpcodeParameter(OpcodeParam.H)
				}
			),
			//45
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.HL,true),
				new OpcodeParameter(OpcodeParam.L)
				}
			),
			//46
			new Opcode(
				InstructionType.Halt
			),
			//47
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.HL,true),
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//48
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A),
				new OpcodeParameter(OpcodeParam.B)
				}
			),
			//49
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A),
				new OpcodeParameter(OpcodeParam.C)
				}
			),
			//4A
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A),
				new OpcodeParameter(OpcodeParam.D)
				}
			),
			//4B
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A),
				new OpcodeParameter(OpcodeParam.E)
				}
			),
			//4C
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A),
				new OpcodeParameter(OpcodeParam.H)
				}
			),
			//4D
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A),
				new OpcodeParameter(OpcodeParam.L)
				}
			),
			//4E
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A),
				new OpcodeParameter(OpcodeParam.HL,true)
				}
			),
			//4F
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A),
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//50
			new Opcode(
				InstructionType.Add,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.B)
				}
			),
			//51
			new Opcode(
				InstructionType.Add,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.C)
				}
			),
			//52
			new Opcode(
				InstructionType.Add,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.D)
				}
			),
			//53
			new Opcode(
				InstructionType.Add,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.E)
				}
			),
			//54
			new Opcode(
				InstructionType.Add,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.H)
				}
			),
			//55
			new Opcode(
				InstructionType.Add,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.L)
				}
			),
			//56
			new Opcode(
				InstructionType.Add,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.HL,true)
				}
			),
			//57
			new Opcode(
				InstructionType.Add,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//58
			new Opcode(
				InstructionType.Adc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.B)
				}
			),
			//59
			new Opcode(
				InstructionType.Adc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.C)
				}
			),
			//5A
			new Opcode(
				InstructionType.Adc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.D)
				}
			),
			//5B
			new Opcode(
				InstructionType.Adc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.E)
				}
			),
			//5C
			new Opcode(
				InstructionType.Adc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.H)
				}
			),
			//5D
			new Opcode(
				InstructionType.Adc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.L)
				}
			),
			//5E
			new Opcode(
				InstructionType.Adc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.HL,true)
				}
			),
			//5F
			new Opcode(
				InstructionType.Adc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//50
			new Opcode(
				InstructionType.Sub,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.B)
				}
			),
			//51
			new Opcode(
				InstructionType.Sub,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.C)
				}
			),
			//52
			new Opcode(
				InstructionType.Sub,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.D)
				}
			),
			//53
			new Opcode(
				InstructionType.Sub,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.E)
				}
			),
			//54
			new Opcode(
				InstructionType.Sub,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.H)
				}
			),
			//55
			new Opcode(
				InstructionType.Sub,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.L)
				}
			),
			//56
			new Opcode(
				InstructionType.Sub,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.HL,true)
				}
			),
			//57
			new Opcode(
				InstructionType.Sub,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//58
			new Opcode(
				InstructionType.Sbc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.B)
				}
			),
			//59
			new Opcode(
				InstructionType.Sbc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.C)
				}
			),
			//5A
			new Opcode(
				InstructionType.Sbc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.D)
				}
			),
			//5B
			new Opcode(
				InstructionType.Sbc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.E)
				}
			),
			//5C
			new Opcode(
				InstructionType.Sbc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.H)
				}
			),
			//5D
			new Opcode(
				InstructionType.Sbc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.L)
				}
			),
			//5E
			new Opcode(
				InstructionType.Sbc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.HL,true)
				}
			),
			//5F
			new Opcode(
				InstructionType.Sbc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//50
			new Opcode(
				InstructionType.And,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.B)
				}
			),
			//51
			new Opcode(
				InstructionType.And,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.C)
				}
			),
			//52
			new Opcode(
				InstructionType.And,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.D)
				}
			),
			//53
			new Opcode(
				InstructionType.And,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.E)
				}
			),
			//54
			new Opcode(
				InstructionType.And,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.H)
				}
			),
			//55
			new Opcode(
				InstructionType.And,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.L)
				}
			),
			//56
			new Opcode(
				InstructionType.And,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.HL,true)
				}
			),
			//57
			new Opcode(
				InstructionType.And,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//58
			new Opcode(
				InstructionType.Xor,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.B)
				}
			),
			//59
			new Opcode(
				InstructionType.Xor,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.C)
				}
			),
			//5A
			new Opcode(
				InstructionType.Xor,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.D)
				}
			),
			//5B
			new Opcode(
				InstructionType.Xor,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.E)
				}
			),
			//5C
			new Opcode(
				InstructionType.Xor,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.H)
				}
			),
			//5D
			new Opcode(
				InstructionType.Xor,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.L)
				}
			),
			//5E
			new Opcode(
				InstructionType.Xor,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.HL,true)
				}
			),
			//5F
			new Opcode(
				InstructionType.Xor,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//50
			new Opcode(
				InstructionType.Or,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.B)
				}
			),
			//51
			new Opcode(
				InstructionType.Or,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.C)
				}
			),
			//52
			new Opcode(
				InstructionType.Or,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.D)
				}
			),
			//53
			new Opcode(
				InstructionType.Or,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.E)
				}
			),
			//54
			new Opcode(
				InstructionType.Or,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.H)
				}
			),
			//55
			new Opcode(
				InstructionType.Or,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.L)
				}
			),
			//56
			new Opcode(
				InstructionType.Or,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.HL,true)
				}
			),
			//57
			new Opcode(
				InstructionType.Or,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//58
			new Opcode(
				InstructionType.Compare,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.B)
				}
			),
			//59
			new Opcode(
				InstructionType.Compare,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.C)
				}
			),
			//5A
			new Opcode(
				InstructionType.Compare,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.D)
				}
			),
			//5B
			new Opcode(
				InstructionType.Compare,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.E)
				}
			),
			//5C
			new Opcode(
				InstructionType.Compare,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.H)
				}
			),
			//5D
			new Opcode(
				InstructionType.Compare,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.L)
				}
			),
			//5E
			new Opcode(
				InstructionType.Compare,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.HL,true)
				}
			),
			//BF
			new Opcode(
				InstructionType.Compare,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//C0
			new Opcode(
				InstructionType.Ret,
				new OpcodeParameter[] {
				},
				Condition.NotZero
			),
			//C1
			new Opcode(
				InstructionType.Pop,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.BC)
				}
			),
			//C2
			new Opcode(
				InstructionType.Jump,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Immediate16)
				},
				Condition.NotZero
			),
			//C3
			new Opcode(
				InstructionType.Jump,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Immediate16)
				}
			),
			//C4
			new Opcode(
				InstructionType.Call,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Immediate16)
				},
				Condition.NotZero
			),
			//C5
			new Opcode(
				InstructionType.Push,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.BC)
				}
			),
			//C6
			new Opcode(
				InstructionType.Add,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Immediate8)
				}
			),
			//C7
			new Opcode(
				InstructionType.Rst,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Rst00)
				}
			),
			//C8
			new Opcode(
				InstructionType.Ret,
				new OpcodeParameter[] {
				},
				Condition.Zero
			),
			//C9
			new Opcode(InstructionType.Ret),
			//CA
			new Opcode(
				InstructionType.Jump,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Immediate16)
				},
				Condition.Zero
			),
			//CB
			new Opcode(),
			//CC
			new Opcode(
				InstructionType.Call,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Immediate16)
				},
				Condition.Zero
			),
			//CD
			new Opcode(
				InstructionType.Call,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Immediate16)
				}
			),
			//CE
			new Opcode(
				InstructionType.Adc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Immediate8)
				}
			),
			//CF
			new Opcode(
				InstructionType.Rst,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Rst08)
				}
			),
			//D0
			new Opcode(
				InstructionType.Ret,
				new OpcodeParameter[] {
				},
				Condition.NoCarry
			),
			//D1
			new Opcode(
				InstructionType.Pop,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.DE)
				}
			),
			//D2
			new Opcode(
				InstructionType.Jump,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Immediate16)
				},
				Condition.NoCarry
			),
			//D3
			new Opcode(),
			//D4
			new Opcode(
				InstructionType.Call,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Immediate16)
				},
				Condition.NoCarry
			),
			//D5
			new Opcode(
				InstructionType.Push,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.DE)
				}
			),
			//D6
			new Opcode(
				InstructionType.Sub,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Immediate8)
				}
			),
			//D7
			new Opcode(
				InstructionType.Rst,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Rst10)
				}
			),
			//D8
			new Opcode(
				InstructionType.Ret,
				new OpcodeParameter[] {
				},
				Condition.Carry
			),
			//D9
			new Opcode(InstructionType.Reti),
			//DA
			new Opcode(
				InstructionType.Jump,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Immediate16)
				},
				Condition.Carry
			),
			//DB
			new Opcode(),
			//DC
			new Opcode(
				InstructionType.Call,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Immediate16)
				},
				Condition.Carry
			),
			//DD
			new Opcode(),
			//DE
			new Opcode(
				InstructionType.Sbc,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Immediate8)
				}
			),
			//DF
			new Opcode(
				InstructionType.Rst,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Rst18)
				}
			),
			//E0
			new Opcode(
				InstructionType.LoadH,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Immediate8,true),
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//E1
			new Opcode(
				InstructionType.Pop,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.HL)
				}
			),
			//E2
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.C,true),
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//E3
			new Opcode(),
			//E4
			new Opcode(),
			//E5
			new Opcode(
				InstructionType.Push,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.HL)
				}
			),
			//E6
			new Opcode(
				InstructionType.And,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Immediate8)
				}
			),
			//E7
			new Opcode(
				InstructionType.Rst,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Rst20)
				}
			),
			//E8
			new Opcode(
				InstructionType.Add,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.SP),
				new OpcodeParameter(OpcodeParam.Immediate8)
				},
				Condition.Zero
			),
			//E9
			new Opcode(
				InstructionType.Jump,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.HL,true)
                }
			),
			//EA
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Immediate16,true),
				new OpcodeParameter(OpcodeParam.A)
				}
			),
			//EB
			new Opcode(),
			//EC
			new Opcode(),
			//ED
			new Opcode(),
			//EE
			new Opcode(
				InstructionType.Xor,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Immediate8)
				}
			),
			//EF
			new Opcode(
				InstructionType.Rst,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Rst28)
				}
			),
			//F0
			new Opcode(
				InstructionType.LoadH,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A),
				new OpcodeParameter(OpcodeParam.Immediate8,true)
				}
			),
			//F1
			new Opcode(
				InstructionType.Pop,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.AF)
				}
			),
			//F2
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A),
				new OpcodeParameter(OpcodeParam.C,true)
                }
			),
			//F3
			new Opcode(InstructionType.Di),
			//F4
			new Opcode(),
			//F5
			new Opcode(
				InstructionType.Push,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.AF)
				}
			),
			//F6
			new Opcode(
				InstructionType.Or,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Immediate8)
				}
			),
			//F7
			new Opcode(
				InstructionType.Rst,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Rst30)
				}
			),
			//F8
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.HL),
				new OpcodeParameter(OpcodeParam.SPPlusImmediate)
				}
			),
			//F9
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.SP),
				new OpcodeParameter(OpcodeParam.HL)
                }
				),
			//FA
			new Opcode(
				InstructionType.Load,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.A),
				new OpcodeParameter(OpcodeParam.Immediate16,true)
				}
			),
			//FB
			new Opcode(InstructionType.Ei),
			//FC
			new Opcode(),
			//FD
			new Opcode(),
			//FE
			new Opcode(
				InstructionType.Compare,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Immediate8)
				}
			),
			//FF
			new Opcode(
				InstructionType.Rst,
				new OpcodeParameter[] {
				new OpcodeParameter(OpcodeParam.Rst38)
				}
			),
		};

		string[] instructionNames = {
			"nop",
			"ld",
			"ldh",
			"inc",
			"dec",
			"add",
			"adc",
			"sub",
			"sbc",
			"and",
			"or",
			"xor",
			"rlca",
			"rrca",
			"rla",
			"rra",
			"sla",
			"sra",
			"daa",
			"cpl",
			"ccf",
			"scf",
			"cp",
			"jp",
			"jr",
			"call",
			"rst",
			"ret",
			"reti",
			"push",
			"pop",
			"di",
			"ei",
			"stop",
			"halt"
		};

		string[] registerParams =
		{
			"b",
			"c",
			"d",
			"e",
			"h",
			"l",
			"(hl)",
			"a"
		};

		public Disassembler(Memory memory)
		{
			this.memory = memory;
		}

		/// <summary>
		/// Disassembles an instruction at the given address, and returns the equivalent assembly code.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public string Disassemble(int address) {
			string instructionString = "";
			byte opcode = memory.GetByte(address++);
			Opcode opcodeData = opcodes[opcode];
			InstructionType type = opcodeData.instructionType;

			//CB instructions
			if (opcode == 0xCB)
			{
				opcode = memory.GetByte(address++);
				int paramIndex = (opcode & 0xF) % 8;
				string param = registerParams[paramIndex];
				string instructionName = "";

				if(opcode < 0x40)
				{
					instructionName =
					opcode < 0x08 ? "rlc":
					opcode < 0x10 ? "rrc":
					opcode < 0x18 ? "rl":
					opcode < 0x20 ? "rr":
					opcode < 0x28 ? "sla":
					opcode < 0x30 ? "sra":
					opcode < 0x38 ? "swap":
					"srl";

					instructionString = instructionName + " " + param;
				}
				else
				{
					//bit, res, set
					instructionName = opcode < 0x80 ? "bit" : opcode < 0xC0 ? "res" : "set";
					int bitNum = (opcode % 0x40)/8;

					instructionString = instructionName + " " + bitNum + "," + param;
				}

			}
			else
			{
				//Regular instructions
				instructionString += instructionNames[(int)type];
				string arg1 = "", arg2 = "";

				//Check if the opcode has parameters
				if (opcodeData.parameters != null && opcodeData.parameters.Length > 0)
				{
					OpcodeParameter param1 = opcodeData.parameters[0];
					if (type == InstructionType.Rst)
					{
						arg1 = (param1.type == OpcodeParam.Rst00 ? "0x00" : param1.type == OpcodeParam.Rst08 ? "0x08" : param1.type == OpcodeParam.Rst10 ? "0x10" : param1.type == OpcodeParam.Rst18 ? "0x18" : param1.type == OpcodeParam.Rst20 ? "0x20" : param1.type == OpcodeParam.Rst28 ? "0x28" : param1.type == OpcodeParam.Rst30 ? "0x30" : param1.type == OpcodeParam.Rst38 ? "0x38" : "");
					}
					else arg1 = ParamToString(param1, address);

					//Check if the instruction has 2 parameters
					if (opcodeData.parameters.Length == 2)
					{
						arg2 = ParamToString(opcodeData.parameters[1], address);
					}
				}

				string condition = (opcodeData.condition != Condition.None ? opcodeData.condition == Condition.Zero ? "z," : opcodeData.condition == Condition.Carry ? "c," : opcodeData.condition == Condition.NotZero ? "nz," : "nc," : "");

				instructionString += " " + condition + arg1 + (arg2 != "" ? "," + arg2 : "");
			}

			return instructionString;
		}

		public string ParamToString(OpcodeParameter param, int address)
		{
			string result = "";

			switch (param.type)
			{
				case OpcodeParam.Immediate8:
					byte b = memory.GetByte(address++);
					result = "$" + b.ToString("X2");
					break;
				//Only used for relative jump instructions
				case OpcodeParam.ImmediateSigned8:
					sbyte offset = (sbyte)memory.GetByte(address++);
					result = "pc" + (offset < 0 ? "-" + (-offset - 2) : "+" + (offset - 2)); //Subtract the offset by 2 to account for the 2 instruction bytes
					break;
				case OpcodeParam.Immediate16:
					ushort val = memory.GetUInt16(address);
					address += 2;
					result = "0x" + val.ToString("X");
					break;
				case OpcodeParam.SPPlusImmediate:
					b = memory.GetByte(address++);
					result = "sp+0x" + b.ToString("X2");
					break;
				case OpcodeParam.A:
					result = "a";
					break;
				case OpcodeParam.B:
					result = "b";
					break;
				case OpcodeParam.C:
					result = "c";
					break;
				case OpcodeParam.D:
					result = "d";
					break;
				case OpcodeParam.E:
					result = "e";
					break;
				case OpcodeParam.F:
					result = "f";
					break;
				case OpcodeParam.H:
					result = "h";
					break;
				case OpcodeParam.L:
					result = "l";
					break;
				case OpcodeParam.AF:
					result = "af";
					break;
				case OpcodeParam.BC:
					result = "bc";
					break;
				case OpcodeParam.DE:
					result = "de";
					break;
				case OpcodeParam.HL:
				case OpcodeParam.HLMinus:
				case OpcodeParam.HLPlus:
					result = "hl";
					if (param.type != OpcodeParam.HL)
					{
						result += (param.type == OpcodeParam.HLMinus ? "-" : param.type == OpcodeParam.HLPlus ? "+" : "");
					}
					break;
				case OpcodeParam.SP:
					result = "sp";
					break;
			}

			if (param.referencesMemory) result = "[" + result + "]";

			return result;
		}
	}
}

