using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using LunaGB.Core;
using Newtonsoft.Json;
using System.Threading;
using System.Linq;

namespace LunaGB.Tests{

	public class Program{
		private static CPU cpu;

		private static string[] testFilesToRun = {
			"00.json"
		};

		static StringBuilder sb;
		static Disassembler disasm;


		public static void Main(string[] args){
			RunAllTests();
		}

		public static void RunAllTests(){
			cpu = new CPU();

			sb = new StringBuilder();
			disasm = new Disassembler(cpu.memory);

			string basePath = "Resources/Tests/sm83/v1/";

			
			//foreach(string file in testFilesToRun){
			//	RunInstructionTests(basePath + file);
			//}

			foreach(string file in Directory.EnumerateFiles(basePath).Order()){
				if(!file.Contains("10.json")) RunInstructionTests(file);
			}

		}

		private static void RunInstructionTests(string file){
			CPUTest[] tests = LoadSM83CPUTestFile(file);
			//Console.WriteLine("Tests loaded :3 Let's run them x3");

			int passedTests = 0;
			int totalTests = tests.Length;

			//Run each test, and count how many tests were passed
			foreach(CPUTest test in tests){
				RunTest(test);
				if(CheckIfPassedTest(test) == true){
					passedTests++;
				}else{
					//Log information about the failed test to the string builder to be outputted later
					LogFailedTestInfo(test);	
				}
			}

			File.WriteAllText("failed_tests.txt",sb.ToString());

			Console.WriteLine("Passed " + passedTests + "/" + totalTests + " tests for test file " + file);
		}

		static void LogFailedTestInfo(CPUTest test){
			CPUState initialRegs = test.initial;
			string instruction = disasm.Disassemble(initialRegs.pc);
			byte opcode = cpu.memory.GetByte(initialRegs.pc);
			CPUState finalRegs = test.final;
			sb.AppendLine("Failed test for opcode " + opcode.ToString("X2") + " (" + instruction + "), test name: " + test.name);
			if(finalRegs.a != cpu.A) sb.AppendLine("A = 0x" + cpu.A.ToString("X2") + ", should be 0x" + finalRegs.a.ToString("X2") + " (original value: 0x" + initialRegs.a.ToString("X2") + ")");
			if(finalRegs.b != cpu.B) sb.AppendLine("B = 0x" + cpu.B.ToString("X2") + ", should be 0x" + finalRegs.b.ToString("X2") + " (original value: 0x" + initialRegs.b.ToString("X2") + ")");
			if(finalRegs.c != cpu.C) sb.AppendLine("C = 0x" + cpu.C.ToString("X2") + ", should be 0x" + finalRegs.c.ToString("X2") + " (original value: 0x" + initialRegs.c.ToString("X2") + ")");
			if(finalRegs.d != cpu.D) sb.AppendLine("D = 0x" + cpu.D.ToString("X2") + ", should be 0x" + finalRegs.d.ToString("X2") + " (original value: 0x" + initialRegs.d.ToString("X2") + ")");
			if(finalRegs.e != cpu.E) sb.AppendLine("E = 0x" + cpu.E.ToString("X2") + ", should be 0x" + finalRegs.e.ToString("X2") + " (original value: 0x" + initialRegs.e.ToString("X2") + ")");
			if(finalRegs.f != cpu.F) sb.AppendLine("F = 0x" + cpu.F.ToString("X2") + ", should be 0x" + finalRegs.f.ToString("X2") + " (original value: 0x" + initialRegs.f.ToString("X2") + ")");
			if(finalRegs.h != cpu.H) sb.AppendLine("H = 0x" + cpu.H.ToString("X2") + ", should be 0x" + finalRegs.h.ToString("X2") + " (original value: 0x" + initialRegs.h.ToString("X2") + ")");
			if(finalRegs.l != cpu.L) sb.AppendLine("L = 0x" + cpu.L.ToString("X2") + ", should be 0x" + finalRegs.l.ToString("X2") + " (original value: 0x" + initialRegs.l.ToString("X2") + ")");
			if(finalRegs.pc != cpu.pc) sb.AppendLine("PC = 0x" + cpu.pc.ToString("X4") + ", should be 0x" + finalRegs.pc.ToString("X4") + " (original value: 0x" + initialRegs.pc.ToString("X4") + ")");
			if(finalRegs.sp != cpu.sp) sb.AppendLine("SP = 0x" + cpu.sp.ToString("X4") + ", should be 0x" + finalRegs.sp.ToString("X4") + " (original value: 0x" + initialRegs.sp.ToString("X4") + ")");
			if(finalRegs.ime != (cpu.ime ? 1 : 0)) sb.AppendLine("IME = " + (cpu.ime ? 1 : 0) + ", should be " + finalRegs.ime + " (original value: " + initialRegs.ime + ")");
			if(finalRegs.ie != cpu.IE) sb.AppendLine("IE = " + cpu.IE + ", should be " + finalRegs.ie + " (original value: " + initialRegs.ie + ")");

			foreach(int[] ramEntry in finalRegs.ram){
				int address = ramEntry[0];
				byte correctVal = (byte)ramEntry[1];
				byte memVal = cpu.memory.GetByte(address);
				if(memVal != correctVal){
					sb.AppendLine("Value at address 0x" + address.ToString("X4") + " is 0x" + memVal.ToString("X2") + ", should be 0x" + correctVal.ToString("X2"));
				}
			}
		}


		public static void RunTest(CPUTest test){
			//Initialize the cpu with the test data
			CPUState initialState = test.initial;
			int[][] entries = initialState.ram;

			cpu.Init();
			cpu.memory.Init();
			cpu.A = initialState.a;
			cpu.B = initialState.b;
			cpu.C = initialState.c;
			cpu.D = initialState.d;
			cpu.E = initialState.e;
			cpu.F = initialState.f;
			cpu.H = initialState.h;
			cpu.L = initialState.l;
			cpu.pc = initialState.pc;
			cpu.sp = initialState.sp;
			cpu.IE = initialState.ie;
			cpu.ime = initialState.ime == 1 ? true : false;

			foreach(int[] entry in entries){
				cpu.memory.WriteByteCPUTest(entry[0], (byte)entry[1]);
			}

			//Run the test
			cpu.ExecuteInstruction();
		}

		private static bool CheckIfPassedTest(CPUTest test){
			CPUState finalState = test.final;
			//Check if any register values are wrong
			if(cpu.A != finalState.a || cpu.B != finalState.b || cpu.C != finalState.c || cpu.D != finalState.d || cpu.E != finalState.e
			|| cpu.F != finalState.f || cpu.H != finalState.h || cpu.L != finalState.l || cpu.pc != finalState.pc || cpu.sp != finalState.sp
			|| cpu.ime != (finalState.ime == 1 ? true : false)){
				return false;
			}

			//Check if any bytes in ram are wrong
			foreach(int[] entry in finalState.ram){
				if(cpu.memory.GetByte(entry[0]) != (byte)entry[1]){
					return false;
				}
			}

			//If everything matches, we passed the test :3
			return true;

		}

		private static CPUTest[] LoadSM83CPUTestFile(string file){
			string text = File.ReadAllText(file);
			CPUTest[]? testData = JsonConvert.DeserializeObject<CPUTest[]>(text);
			
			if(testData == null) throw new NullReferenceException();

			return testData;
		}

	}

}