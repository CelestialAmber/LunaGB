using System;

namespace Tsukimi.Core.LunaGB {
	public class Disassembler {
		Memory memory;

		//Taken from mgbdis
		string[] instructions = {
		"nop",
		"ld bc,d16",
		"ld [bc],a",
		"inc bc",
		"inc b",
		"dec b",
		"ld b,d8",
		"rlca",
		"ld [a16],sp",
		"add hl,bc",
		"ld a,[bc]",
		"dec bc",
		"inc c",
		"dec c",
		"ld c,d8",
		"rrca",
		"stop",
		"ld de,d16",
		"ld [de],a",
		"inc de",
		"inc d",
		"dec d",
		"ld d,d8",
		"rla",
		"jr pc+r8",
		"add hl,de",
		"ld a,[de]",
		"dec de",
		"inc e",
		"dec e",
		"ld e,d8",
		"rra",
		"jr nz,pc+r8",
		"ld hl,d16",
		"ld [hl+],a",
		"inc hl",
		"inc h",
		"dec h",
		"ld h,d8",
		"daa",
		"jr z,pc+r8",
		"add hl,hl",
		"ld a,[hl+]",
		"dec hl",
		"inc l",
		"dec l",
		"ld l,d8",
		"cpl",
		"jr nc,pc+r8",
		"ld sp,d16",
		"ld [hl-],a",
		"inc sp",
		"inc [hl]",
		"dec [hl]",
		"ld [hl],d8",
		"scf",
		"jr c,pc+r8",
		"add hl,sp",
		"ld a,[hl-]",
		"dec sp",
		"inc a",
		"dec a",
		"ld a,d8",
		"ccf",
		"ld b,b",
		"ld b,c",
		"ld b,d",
		"ld b,e",
		"ld b,h",
		"ld b,l",
		"ld b,[hl]",
		"ld b,a",
		"ld c,b",
		"ld c,c",
		"ld c,d",
		"ld c,e",
		"ld c,h",
		"ld c,l",
		"ld c,[hl]",
		"ld c,a",
		"ld d,b",
		"ld d,c",
		"ld d,d",
		"ld d,e",
		"ld d,h",
		"ld d,l",
		"ld d,[hl]",
		"ld d,a",
		"ld e,b",
		"ld e,c",
		"ld e,d",
		"ld e,e",
		"ld e,h",
		"ld e,l",
		"ld e,[hl]",
		"ld e,a",
		"ld h,b",
		"ld h,c",
		"ld h,d",
		"ld h,e",
		"ld h,h",
		"ld h,l",
		"ld h,[hl]",
		"ld h,a",
		"ld l,b",
		"ld l,c",
		"ld l,d",
		"ld l,e",
		"ld l,h",
		"ld l,l",
		"ld l,[hl]",
		"ld l,a",
		"ld [hl],b",
		"ld [hl],c",
		"ld [hl],d",
		"ld [hl],e",
		"ld [hl],h",
		"ld [hl],l",
		"halt",
		"ld [hl],a",
		"ld a,b",
		"ld a,c",
		"ld a,d",
		"ld a,e",
		"ld a,h",
		"ld a,l",
		"ld a,[hl]",
		"ld a,a",
		"add b",
		"add c",
		"add d",
		"add e",
		"add h",
		"add l",
		"add [hl]",
		"add a",
		"adc b",
		"adc c",
		"adc d",
		"adc e",
		"adc h",
		"adc l",
		"adc [hl]",
		"adc a",
		"sub b",
		"sub c",
		"sub d",
		"sub e",
		"sub h",
		"sub l",
		"sub [hl]",
		"sub a",
		"sbc b",
		"sbc c",
		"sbc d",
		"sbc e",
		"sbc h",
		"sbc l",
		"sbc [hl]",
		"sbc a",
		"and b",
		"and c",
		"and d",
		"and e",
		"and h",
		"and l",
		"and [hl]",
		"and a",
		"xor b",
		"xor c",
		"xor d",
		"xor e",
		"xor h",
		"xor l",
		"xor [hl]",
		"xor a",
		"or b",
		"or c",
		"or d",
		"or e",
		"or h",
		"or l",
		"or [hl]",
		"or a",
		"cp b",
		"cp c",
		"cp d",
		"cp e",
		"cp h",
		"cp l",
		"cp [hl]",
		"cp a",
		"ret nz",
		"pop bc",
		"jp nz,a16",
		"jp a16",
		"call nz,a16",
		"push bc",
		"add d8",
		"rst $00",
		"ret z",
		"ret",
		"jp z,a16",
		"CBPREFIX",
		"call z,a16",
		"call a16",
		"adc d8",
		"rst $08",
		"ret nc",
		"pop de",
		"jp nc,a16",
		"db $d3",
		"call nc,a16",
		"push de",
		"sub d8",
		"rst $10",
		"ret c",
		"reti",
		"jp c,a16",
		"db $db",
		"call c,a16",
		"db $dd",
		"sbc d8",
		"rst $18",
		"ldh [a8],a",
		"pop hl",
		"ld [c],a",
		"db $e3",
		"db $e4",
		"push hl",
		"and d8",
		"rst $20",
		"add sp,r8",
		"jp hl",
		"ld [a16],a",
		"db $eb",
		"db $ec",
		"db $ed",
		"xor d8",
		"rst $28",
		"ldh a,[a8]",
		"pop af",
		"ld a,[c]",
		"di",
		"db $f4",
		"push af",
		"or d8",
		"rst $30",
		"ld hl,sp+r8",
		"ld sp,hl",
		"ld a,[a16]",
		"ei",
		"db $fc",
		"db $fd",
		"cp d8",
		"rst $38",
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
			string newInstructionString = "";
			byte opcode = memory.GetByte(address++);
			string instructionString = instructions[opcode];

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

					newInstructionString = instructionName + " " + param;
				}
				else
				{
					//bit, res, set
					instructionName = opcode < 0x80 ? "bit" : opcode < 0xC0 ? "res" : "set";
					int bitNum = (opcode % 0x40)/8;

					newInstructionString = instructionName + " " + bitNum + "," + param;
				}

			}
			else
			{
				//Regular instructions
				newInstructionString = instructionString;

				//Jump offset parameter
				if (newInstructionString.Contains("pc+r8"))
				{
					sbyte jumpOffset = (sbyte)memory.GetByte(address++);
					ushort newAddress = (ushort)(address - Math.Abs(jumpOffset));
					newInstructionString = newInstructionString.Replace("pc+r8","$" + newAddress.ToString("X"));
				}

				//Signed 8bit parameter (add sp,r8)
				if (newInstructionString.Contains("r8"))
				{
					sbyte val = (sbyte)memory.GetByte(address++);
					newInstructionString = newInstructionString.Replace("r8", val.ToString());
				}

				//8bit address offset (ldh)
				if (newInstructionString.Contains("a8"))
				{
					ushort hramAddress = (ushort)(0xFF00 + memory.GetByte(address++));
					newInstructionString = newInstructionString.Replace("a8", "$" + hramAddress.ToString("X"));
				}

				//16bit address
				if (newInstructionString.Contains("a16"))
				{
					ushort val = memory.GetUInt16(address);
					address += 2;
					newInstructionString = newInstructionString.Replace("a16", "$" + val.ToString("X"));
				}

				//Unsigned 8bit parameter
				if (newInstructionString.Contains("d8"))
				{
					byte val = memory.GetByte(address++);
					newInstructionString = newInstructionString.Replace("d8", "$" + val.ToString("X2"));
				}

				//Unsigned 16bit parameter
				if (newInstructionString.Contains("d16"))
				{
					ushort val = memory.GetUInt16(address);
					address += 2;
					newInstructionString = newInstructionString.Replace("d16", "$" + val.ToString("X"));
				}
			}

			return newInstructionString;
		}
	}
}

