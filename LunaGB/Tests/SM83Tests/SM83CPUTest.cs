using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using LunaGB.Core;
using Newtonsoft.Json;
using System.Threading;

namespace LunaGB.Tests.SM83Tests{

	public class SM83CPUTest{
		private static CPU cpu;

		private static string[] testFilesToRun = {
			"cb.json"
		};

		static StringBuilder sb;
		static Disassembler disasm;

		public static void RunAllTests(){
			cpu = new CPU();

			sb = new StringBuilder();
			disasm = new Disassembler(cpu.memory);

			string basePath = "Resources/Tests/sm83-test-data/cpu_tests/v1/";

			
			foreach(string file in testFilesToRun){
				RunInstructionTests(basePath + file);
			}

		}

		private static void RunInstructionTests(string file){
			List<SM83CPUTestData> tests = LoadSM83CPUTestFile(file);
			//Console.WriteLine("Tests loaded :3 Let's run them x3");

			int passedTests = 0;
			int totalTests = tests.Count;

			//Run each test, and count how many tests were passed
			foreach(SM83CPUTestData test in tests){
				RunTest(test);
				if(CheckIfPassedTest(test) == true){
					passedTests++;
				}else{
					//Log information about the failed test to the string builder to be outputted later
					SM83CPURegs initialRegs = test.initial.cpuRegs;
					string instruction = disasm.Disassemble(initialRegs.pc);
					byte opcode = cpu.memory.GetByte(initialRegs.pc + 1);
					SM83CPURegs finalRegs = test.final.cpuRegs;
					sb.AppendLine("Failed test for CB instruction " + opcode.ToString("X2") + " (" + instruction + ")");
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
					//Console.WriteLine(";< failed a test... it was " + test.name);
				}
			}

			File.WriteAllText("failed_tests.txt",sb.ToString());

			Console.WriteLine("Passed " + passedTests + "/" + totalTests + " tests for test file " + file);
		}


		public static void RunTest(SM83CPUTestData test){
			//Initialize the cpu with the test data
			SM83State initialState = test.initial;
			SM83CPURegs regs = initialState.cpuRegs;
			RamEntry[] entries = initialState.ramEntries;

			cpu.Init();
			cpu.memory.Init();
			cpu.A = regs.a;
			cpu.B = regs.b;
			cpu.C = regs.c;
			cpu.D = regs.d;
			cpu.E = regs.e;
			cpu.F = regs.f;
			cpu.H = regs.h;
			cpu.L = regs.l;
			cpu.pc = regs.pc;
			cpu.sp = regs.sp;

			foreach(RamEntry entry in entries){
				cpu.memory.WriteByteCPUTest(entry.address, entry.value);
			}

			//Run the test
			cpu.ExecuteInstruction();
		}

		private static bool CheckIfPassedTest(SM83CPUTestData test){
			SM83State finalState = test.final;
			SM83CPURegs regs = finalState.cpuRegs;
			//Check if any register values are wrong
			if(cpu.A != regs.a || cpu.B != regs.b || cpu.C != regs.c || cpu.D != regs.d || cpu.E != regs.e
			|| cpu.F != regs.f || cpu.H != regs.h || cpu.L != regs.l || cpu.pc != regs.pc || cpu.sp != regs.sp){
				return false;
			}

			//Check if any bytes in ram are wrong
			foreach(RamEntry entry in finalState.ramEntries){
				if(cpu.memory.GetByte(entry.address) != entry.value){
					return false;
				}
			}

			//If everything matches, we passed the test :3
			return true;

		}

		private static List<SM83CPUTestData> LoadSM83CPUTestFile(string file){
			List<SM83CPUTestData> tests = new List<SM83CPUTestData>();

			string text = File.ReadAllText(file);
			SM83CPUTestJSONData[]? testData = JsonConvert.DeserializeObject<SM83CPUTestJSONData[]>(text);
			
			if(testData == null) throw new NullReferenceException();

			for(int i = 0; i < testData.Length; i++){
				tests.Add(SM83CPUTestData.CreateFromJSONData(testData[i]));
			}

			return tests;
		}

	}

}