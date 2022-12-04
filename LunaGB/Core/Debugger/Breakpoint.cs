using System;
namespace LunaGB.Core.Debug
{

	public class Breakpoint
	{
		//Breakpoint condition flags
		public bool read, write, execute;
		public int minAddress, maxAddress;
		public byte value; //used for breakpoints specifying a specific value that was read/written
		public bool specificValue; //does this breakpoint specify a specific value to break on?
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

