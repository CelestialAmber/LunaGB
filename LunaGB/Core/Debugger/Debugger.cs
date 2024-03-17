using System;
using System.Collections.Generic;

namespace LunaGB.Core.Debug
{
	public class Debugger
	{
		public List<Breakpoint> breakpoints = new List<Breakpoint>();
		public bool breakpointsEnabled = false;
		public bool stepping = false;

		//Event for breakpoints
		public delegate void BreakpointEvent(Breakpoint breakpoint);
		public event BreakpointEvent OnHitBreakpoint;


		public Debugger()
		{

		}

		public void InitBreakpoints()
		{
			breakpoints.Clear();
			breakpoints.Add(new Breakpoint(0xC2B5, false, false, true));
			breakpoints[0].enabled = true;
		}

		//Checks whether one of the breakpoints was hit when the emulator reads a byte
		public void OnMemoryRead(int address)
		{
			foreach (Breakpoint breakpoint in breakpoints)
			{
				if (breakpoint.enabled && breakpoint.read && address >= breakpoint.minAddress && address <= breakpoint.maxAddress)
				{
					OnHitBreakpoint?.Invoke(breakpoint);
				}
			}
		}

		//Checks whether one of the breakpoints was hit when the emulator writes a byte
		public void OnMemoryWrite(int address)
		{
			foreach (Breakpoint breakpoint in breakpoints)
			{
				if (breakpoint.enabled && breakpoint.write && address >= breakpoint.minAddress && address <= breakpoint.maxAddress)
				{
					OnHitBreakpoint?.Invoke(breakpoint);
				}
			}
		}

		//Checks whether one of the breakpoints was hit when the emulator executes an instruction
		public void OnExecute(int address)
		{
			foreach (Breakpoint breakpoint in breakpoints)
			{
				if (breakpoint.enabled && breakpoint.execute && address >= breakpoint.minAddress && address <= breakpoint.maxAddress)
				{
					OnHitBreakpoint?.Invoke(breakpoint);
				}
			}
		}
	}
}

