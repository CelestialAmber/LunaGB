﻿using System;
namespace LunaGB.Core.Debug
{

	public class Breakpoint
	{
		//Breakpoint condition flags
		public bool read, write, execute;
		public int minAddress, maxAddress;
		public bool enabled;

		public Breakpoint(int address, bool read, bool write, bool execute)
		{
			this.read = read;
			this.write = write;
			this.execute = execute;
			minAddress = address;
			maxAddress = address;
		}

		public Breakpoint(int minAddress, int maxAddress, bool read, bool write, bool execute)
		{
			this.read = read;
			this.write = write;
			this.execute = execute;
			this.minAddress = minAddress;
			this.maxAddress = maxAddress;
		}
	}
}

