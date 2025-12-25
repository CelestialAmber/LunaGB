using System;
using System.Collections.Generic;
using LunaCube.Disassembler;
using Tsukimi.Graphics;

namespace LunaCubeTest
{
    internal class Program
    {
        static uint[] instructionValues = {
            0x9421FFF0,
            0x7C0802A6,
            0x90010014,
            0x93E1000C,
            0x7C7F1B78,
            0x48004EA5,
            0x2C030001,
            0x4182001C,
            0x3C608052,
            0x3863D1D0,
            0x38630084,
            0x4CC63182,
            0x48000C75,
            0x480000D8,
            0x3C608060,
            0x80632960,
            0x2C030000,
            0x41820024,
            0x3C808057,
            0x38849F24,
            0x93E4000C,
            0x38840004,
            0x80A30000,
            0x81850024,
            0x7D8903A6,
            0x4E800421,
            0x7FE3FB78,
            0x48004E4D,
            0x2C030001,
            0x4182001C,
            0x3C608052,
            0x3863D1D0,
            0x386300B4,
            0x4CC63182,
            0x48000C1D,
            0x4800000C,
            0x38000001,
            0x981F0080,
            0x809F04F4,
            0x7FE3FB78,
            0x480047BD,
            0x7FE3FB78,
            0x48004B89,
            0x807F0054,
            0x4BFF5305,
            0x807F04F0,
            0x2C030000,
            0x41820014,
            0x80830000,
            0x81840014,
            0x7D8903A6,
            0x4E800421,
            0x7FE3FB78,
            0x480020D9,
            0x38000000,
            0x3C608060,
            0x901F0628,
            0x80632960,
            0x2C030000,
            0x41820020,
            0x80830000,
            0x3CA08057,
            0x38A59F24,
            0x81840024,
            0x3885006C,
            0x7D8903A6,
            0x4E800421,
            0x80010014,
            0x83E1000C,
            0x7C0803A6,
            0x38210010,
            0x4E800020
        };
        static uint startAddress = 0x8039CF94;

        static void Main(string[] args)
        {
            DisassembleTest();
        }

        static void DisassembleTest()
        {
            Disassembler disasm = new Disassembler();
            disasm.Disassemble(instructionValues, startAddress);
            List<DisassembledInstruction> instructions = disasm.disassembledInstructions;
            List<uint> branchTargets = disasm.branchDestAddresses;

            uint endAddress = instructions[^1].address;

            for (int i = 0; i < instructions.Count; i++)
            {
                PrintInstruction(instructions[i], branchTargets, startAddress, endAddress);
            }
        }

        static void PrintInstruction(DisassembledInstruction instruction, List<uint> branchTargets, uint startAddress, uint endAddress)
        {

            //Print the address
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("{0:x8}: ", instruction.address);
            Console.ResetColor();

            //If this instruction is a branch target, print the preceding arrow
            if (branchTargets.Contains(instruction.address))
            {
                int index = branchTargets.IndexOf(instruction.address);
                Console.ForegroundColor = IntToConsoleColor(index);
                Console.Write("-> ");
                Console.ResetColor();
            }
            else
            {
                //Otherwise just print spaces
                Console.Write("   ");
            }

            //Print the disassembled instruction
            Console.Write("{0}", instruction.disasmString);

            uint destAddr = instruction.branchDestAddress;

            //If this instruction is a branch instruction, and the target is within the disassembled range, print an arrow after
            if (instruction.isBranchInstruction && destAddr >= startAddress && destAddr <= endAddress)
            {
                int index = branchTargets.IndexOf(destAddr);
                if (index == -1)
                {
                    throw new Exception("Invalid branch destination address somehow :<");
                }

                Console.ForegroundColor = IntToConsoleColor(index);
                Console.Write(" ->");
                Console.ResetColor();
            }

            Console.WriteLine();
        }

        //Converts the given value to a console color that is always non black/white.
        static ConsoleColor IntToConsoleColor(int index)
        {
            return (ConsoleColor)((index % (int)ConsoleColor.Yellow) + 1);
        }
    }
}
