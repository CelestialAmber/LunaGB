using System;
using System.Collections.Generic;

namespace LunaGB.Tests.SM83Tests{

	public static class StringExtensions{
		public static int HexStringToInt(this string str){
			return Convert.ToInt32(str, 16);
		}
	}

	public class SM83CPURegsJSONData {
		public string a = "";
		public string b = "";
		public string c = "";
		public string d = "";
		public string e = "";
		public string f = "";
		public string h = "";
		public string l = "";
		public string pc = "";
		public string sp = "";
	}

	
	public class SM83StateJSONData{
		public SM83CPURegsJSONData? cpu;
		public string[][]? ram;
	}

	public class SM83CPUTestJSONData{
		public string? name;
		public SM83StateJSONData? initial;
		public SM83StateJSONData? final;
		public string[][]? cycles;
	}


	public class RamEntry{
		public ushort address;
		public byte value;

		public RamEntry(ushort address, byte value){
			this.address = address;
			this.value = value;
		}
	}

	public class SM83CPURegs{
		public byte a;
		public byte b;
		public byte c;
		public byte d;
		public byte e;
		public byte f;
		public byte h;
		public byte l;
		public ushort pc;
		public ushort sp;

		public static SM83CPURegs CreateFromJSONData(SM83CPURegsJSONData data){
			SM83CPURegs regs = new SM83CPURegs();

			regs.a = (byte)data.a.HexStringToInt();
			regs.b = (byte)data.b.HexStringToInt();
			regs.c = (byte)data.c.HexStringToInt();
			regs.d = (byte)data.d.HexStringToInt();
			regs.e = (byte)data.e.HexStringToInt();
			regs.f = (byte)data.f.HexStringToInt();
			regs.h = (byte)data.h.HexStringToInt();
			regs.l = (byte)data.l.HexStringToInt();
			regs.pc = (ushort)data.pc.HexStringToInt();
			regs.sp = (ushort)data.sp.HexStringToInt();

			return regs;
		}
	}

	public class SM83State{
		public SM83CPURegs cpuRegs;
		public RamEntry[] ramEntries;

		public static SM83State CreateFromJSONData(SM83StateJSONData data){
			SM83State state = new SM83State();

			state.cpuRegs = SM83CPURegs.CreateFromJSONData(data.cpu);
			state.ramEntries = new RamEntry[data.ram.Length];
			for(int i = 0; i < data.ram.Length; i++){
				string[] ramEntryData = data.ram[i];
				ushort address = (ushort)ramEntryData[0].HexStringToInt();
				byte value = (byte)ramEntryData[1].HexStringToInt();
				RamEntry entry = new RamEntry(address, value);
				state.ramEntries[i] = entry;
			}

			return state;
		}

	}

	public class CycleState{
		public ushort address;
		public byte value;
		public string state;

		public CycleState(ushort address, byte value, string state){
			this.address = address;
			this.value = value;
			this.state = state;
		}
	}

	public class SM83CPUTestData{
		public string name = "";
		public SM83State? initial;
		public SM83State? final;

		public static SM83CPUTestData CreateFromJSONData(SM83CPUTestJSONData data){
			SM83CPUTestData testData = new SM83CPUTestData();

			testData.name = data.name;
			testData.initial = SM83State.CreateFromJSONData(data.initial);
			testData.final = SM83State.CreateFromJSONData(data.final);

			return testData;
		}
	}
}
