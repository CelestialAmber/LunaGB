using System;
using System.Collections.Generic;

namespace LunaGB.Tests{

	public class CPUState{
		public ushort pc;
		public ushort sp;
		public byte a;
		public byte b;
		public byte c;
		public byte d;
		public byte e;
		public byte f;
		public byte h;
		public byte l;
		public byte ime;
		public byte ie;
		public int[][] ram;
	}

	public class CPUTest{
		public string name = "";
		public CPUState? initial;
		public CPUState? final;
		//public (int,int,string)[] cycles;
	}
}
