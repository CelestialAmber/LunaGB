using System;
using System.IO;
using System.Linq;
using LunaGB.Core.ROMMappers;

namespace LunaGB.Core
{
	public enum ROMMapper {
		Basic = 0x0,
		MBC1 = 0x1,
		MBC1Ram = 0x2,
		MBC1RamBattery = 0x3,
		MBC2 = 0x5,
		MBC2Battery = 0x6,
		RomRam = 0x8,
		RomRamBattery = 0x9,
		MMM01 = 0xB,
		MMM01Ram = 0xC,
		MMM01RamBattery = 0xD,
		MBC3TimerBattery = 0xF,
		MBC3TimerRamBattery = 0x10,
		MBC3 = 0x11,
		MBC3Ram = 0x12,
		MBC3RamBattery = 0x13,
		MBC5 = 0x19,
		MBC5Ram = 0x1A,
		MBC5RamBattery = 0x1B,
		MBC5Rumble = 0x1C,
		MBC5RumbleRam = 0x1D,
		MBC5RumbleRamBattery = 0x1E,
		MBC6 = 0x20,
		MBC7 = 0x22,
		PocketCamera = 0xFC,
		BandaiTama5 = 0xFD,
		HuC3 = 0xFE,
		HuC1 = 0xFF
	}

	//contains all the rom header data
	public struct ROMHeader {
		/*
		0x100-0x103: entry point
		0x104-0x133: nintendo logo
		/*
		In later cartridges, the bytes at 0x13F-0x142 are the manufacturer code,
		and 0x143 is the CGB flag
		*/
		public string title; //0x134-0x143
		//80: works on GBC/GB, C0: only works on GBC
		public byte cgbFlag; //0x143
		public string newLicenseeCode; //0x144-0x145
		//00: doesn't support sgb, 03: supports sgb
		public byte sgbFlag; //0x146
		/*
		Values:
		00h  ROM ONLY                 19h  MBC5
		01h  MBC1                     1Ah  MBC5+RAM
		02h  MBC1+RAM                 1Bh  MBC5+RAM+BATTERY
		03h  MBC1+RAM+BATTERY         1Ch  MBC5+RUMBLE
		05h  MBC2                     1Dh  MBC5+RUMBLE+RAM
		06h  MBC2+BATTERY             1Eh  MBC5+RUMBLE+RAM+BATTERY
		08h  ROM+RAM                  20h  MBC6
		09h  ROM+RAM+BATTERY          22h  MBC7+SENSOR+RUMBLE+RAM+BATTERY
		0Bh  MMM01
		0Ch  MMM01+RAM
		0Dh  MMM01+RAM+BATTERY
		0Fh  MBC3+TIMER+BATTERY
		10h  MBC3+TIMER+RAM+BATTERY   FCh  POCKET CAMERA
		11h  MBC3                     FDh  BANDAI TAMA5
		12h  MBC3+RAM                 FEh  HuC3
		13h  MBC3+RAM+BATTERY         FFh  HuC1+RAM+BATTERY
		*/
		public byte cartridgeType; //0x147
		/*
		Values:
		00h -  32KByte (no ROM banking)
		01h -  64KByte (4 banks)
		02h - 128KByte (8 banks)
		03h - 256KByte (16 banks)
		04h - 512KByte (32 banks)
		05h -   1MByte (64 banks)  - only 63 banks used by MBC1
		06h -   2MByte (128 banks) - only 125 banks used by MBC1
		07h -   4MByte (256 banks)
		08h -   8MByte (512 banks)
		52h - 1.1MByte (72 banks)
		53h - 1.2MByte (80 banks)
		54h - 1.5MByte (96 banks)
		*/
		public byte romSize; //0x148
		/*
		Values:
		00h - None
		01h - 2 KBytes
		02h - 8 Kbytes
		03h - 32 KBytes (4 banks of 8KBytes each)
		04h - 128 KBytes (16 banks of 8KBytes each)
		05h - 64 KBytes (8 banks of 8KBytes each)
		*/
		public byte ramSize; //0x149
		//0: jp, 1: non-jp
		public byte destinationCode; //0x14A
		public byte oldLicenseeCode; //0x14B
		//usually 0
		public byte maskRomVersionNumber; //0x14C
		/*
		If the header checksum isn't correct, the game won't work on real hardware
		Pseudocode for calculating the checksum:
		x = 0;
		for(int i = 0x134; i <= 0x14C; i++){
			x -= mem[i] - 1;
		}
		*/
		public byte headerChecksum; //0x14D
		/*
		Calculated by adding up all the bytes in the rom except the global checksum bytes
		The Game Boy doesn't check this checksum
		*/
		public ushort globalChecksum; //0x14E-0x14F
	}

	public class ROM
	{
		ROMHeader header;
		public int currentBank; //current switchable bank number
		public byte[] rom;
		public Cartridge romMapper; //rom mapper class storing the rom data and handling rom mapping
		ROMMapper mapper;
		public bool loadedRom;
		public bool mapperSupported;


		public void OpenROM(string path) {
			loadedRom = false;
			rom = File.ReadAllBytes(path);
			ReadHeader();
			//PrintHeaderInfo();
			DetermineROMMapper();
			if(!mapperSupported) return; //If the ROM uses an unsupported mapper, return
			romMapper.rom = rom;
			romMapper.currentBank = 1; //The default bank in bank slot 1 is bank 1
			loadedRom = true;
		}

		//Determines which ROM mapper to use based on the cartridge type in the header
		public void DetermineROMMapper() {
			mapperSupported = true;
			mapper = (ROMMapper)header.cartridgeType;
			switch (mapper) {
				case ROMMapper.Basic:
					//Basic rom (0)
					romMapper = new BasicCartridge();
					break;
				default:
					romMapper = new BasicCartridge();
					//Console.WriteLine("Error: Only the basic rom mapper (0) is implemented");
					//mapperSupported = false;
					break;
			}
		}

		public void ReadHeader() {
			//Read all the information in the header

			//The header data starts at 0x134
			int offset = 0x134;

			byte[] titleBytes = rom.Skip(offset).Take(16).ToArray();
			string title = "";

			//Convert the title bytes to a string
			for(int i = 0; i < 16; i++) {
				if (titleBytes[i] >= 0x20 && titleBytes[i] < 0x80) {
					title += (char)titleBytes[i];
				} else {
					//If the current byte isn't a character (mostly 0), we've probably reached the end of the string
					//This may need to be changed to account for exceptions :>
					break;
				}
			}

			//whether to read the new values that take up the title bytes (manufacturer code, cgb flag)
			bool readNewValues = true;

			byte cgbFlag = 0;

			if (readNewValues) {
				offset += 15;

				//read the manufacturer code

				cgbFlag = ReadByte(offset++);
			} else {
				offset += 16;
			}

			string newLicenseeCode = "";
			//Read the two bytes as ascii chars
			newLicenseeCode += (char)ReadByte(offset++);
			newLicenseeCode += (char)ReadByte(offset++);

			byte sgbFlag = ReadByte(offset++);
			byte cartridgeType = ReadByte(offset++);
			byte romSize = ReadByte(offset++);
			byte ramSize = ReadByte(offset++);
			byte destinationCode = ReadByte(offset++);
			byte oldLicenseeCode = ReadByte(offset++);
			byte maskRomVersionNumber = ReadByte(offset++);
			byte headerChecksum = ReadByte(offset++);
			ushort globalChecksum = ReadUInt16(offset);

			header = new ROMHeader();

			header.title = title;
			header.cgbFlag = cgbFlag;
			header.newLicenseeCode = newLicenseeCode;
			header.sgbFlag = sgbFlag;
			header.cartridgeType = cartridgeType;
			header.romSize = romSize;
			header.ramSize = ramSize;
			header.destinationCode = destinationCode;
			header.oldLicenseeCode = oldLicenseeCode;
			header.maskRomVersionNumber = maskRomVersionNumber;
			header.headerChecksum = headerChecksum;
			header.globalChecksum = globalChecksum;
		}

		public void PrintHeaderInfo() {
			Console.WriteLine("Header information:");
			Console.WriteLine("Name: " + header.title);
			Console.WriteLine("GBC: " + (header.cgbFlag == 0x80 ? "Supports GBC" : header.cgbFlag == 0xC0 ? "GBC only" : "No GBC support"));
			Console.WriteLine("New licensee code: " + header.newLicenseeCode);
			Console.WriteLine("Supports SGB: " + (header.sgbFlag == 0x03 ? "yes" : "no"));
			Console.WriteLine("Cartridge type byte: " + header.cartridgeType);
			Console.WriteLine("ROM size: " + header.romSize);
			Console.WriteLine("RAM size: " + header.ramSize);
			Console.WriteLine("Destination code: " + (header.destinationCode == 0 ? "JP" : "Non-JP"));
			Console.WriteLine("Old licensee code: " + header.oldLicenseeCode);
			Console.WriteLine("Header checksum: " + header.headerChecksum.ToString("X1"));
			Console.WriteLine("Global Checksum: " + header.globalChecksum.ToString("X2"));
		}


		//TODO: This needs to changed in the future to allow for other cartridge types
		public byte ReadByte(int index) {
			return rom[index];
		}

		public ushort ReadUInt16(int index) {
			//The Game Boy uses little endian
			byte lo = ReadByte(index);
			byte hi = ReadByte(index + 1);

			return (ushort)((hi << 8) + lo);
		}
	}
}

