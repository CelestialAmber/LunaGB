using System;
using System.Diagnostics;
using Tsukimi.Core.LunaCube.Disassembler;

namespace LunaCubeTest
{
    internal class Program
    {
        static uint[] instructionValues = {
        0x9421FFF0,
        0x7C0802A6,
        0xC022ABC4,
        0x90010014,
        0x93E1000C,
        0x7C7F1B78,
        0x80630024,
        0x4BF014BD,
        0x2C030000,
        0x4182007C,
        0x807F0020,
        0x38A00000,
        0x809F002C,
        0x81830000,
        0x818C002C,
        0x7D8903A6,
        0x4E800421,
        0x807F0020,
        0x38A00000,
        0x809F0030,
        0x81830000,
        0x818C002C,
        0x7D8903A6,
        0x4E800421,
        0x807F0020,
        0x38A00000,
        0x809F0024,
        0x81830000,
        0x818C002C,
        0x7D8903A6,
        0x4E800421,
        0x807F0020,
        0x38A00001,
        0x809F0028,
        0x81830000,
        0x818C002C,
        0x7D8903A6,
        0x4E800421,
        0x38000002,
        0x901F0044,
        0x80010014,
        0x83E1000C,
        0x7C0803A6,
        0x38210010,
        0x4E800020
        };

        static void Main(string[] args)
        {
            foreach(uint instruction in instructionValues)
            {
                DisassembleInstruction(instruction);
            }
        }

        static void DisassembleInstruction(uint instrVal)
        {
            string disasmString = Disassembler.DisassembleInstruction(instrVal);
            Console.WriteLine("{0:X8} -> {1}", instrVal, disasmString);
        }
    }
}
